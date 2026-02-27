using System.Runtime.InteropServices;
using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Gpu.Interop;
using MdCsg.Math;
using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace MdCsg.Gpu;

/// <summary>
/// GPU-accelerated signed distance field sampling using BVH traversal.
/// Computes signed distance and classification in a single pass.
/// Falls back to CPU on runtime errors.
/// </summary>
public sealed unsafe class GpuSignedDistanceField : IDisposable
{
    private readonly VulkanContext _ctx;
    private readonly ComputePipeline _sdfGridPipeline;
    private bool _disposed;

    /// <summary>
    /// Maximum number of points per GPU batch to limit memory usage.
    /// </summary>
    public int MaxBatchSize { get; set; } = 4_000_000;

    /// <summary>
    /// Creates a GPU signed distance field engine.
    /// </summary>
    /// <param name="ctx">The Vulkan context to use.</param>
    public GpuSignedDistanceField(VulkanContext ctx)
    {
        _ctx = ctx;
        const uint pushSize = 12; // 3 x uint

        // batch_sdf_grid.comp: 6 bindings (BvhNodes, FaceIndices, TriVerts, QueryPoints, Distances, Classifications)
        _sdfGridPipeline = ComputePipeline.Create(ctx,
            "MdCsg.Gpu.Shaders.batch_sdf_grid.comp", 6, pushSize);
    }

    /// <summary>
    /// Attempts to create a GPU signed distance field engine.
    /// Returns null if Vulkan is not available.
    /// </summary>
    public static GpuSignedDistanceField? TryCreate()
    {
        var ctx = VulkanContext.TryCreate();
        if (ctx == null) return null;

        try
        {
            return new GpuSignedDistanceField(ctx);
        }
        catch
        {
            ctx.Dispose();
            return null;
        }
    }

    /// <summary>
    /// Samples the signed distance field on a regular 3D grid.
    /// </summary>
    /// <param name="bvh">The BVH tree for the solid mesh.</param>
    /// <param name="bounds">The AABB defining the grid bounds.</param>
    /// <param name="nx">Grid resolution in X.</param>
    /// <param name="ny">Grid resolution in Y.</param>
    /// <param name="nz">Grid resolution in Z.</param>
    /// <returns>An array of signed distances (negative inside, positive outside) in x-major order.</returns>
    public double[] SampleOnGrid(BvhTree bvh, Aabb bounds, int nx, int ny, int nz)
    {
        int total = nx * ny * nz;
        var points = new Vec3[total];
        double dx = (bounds.Max.X - bounds.Min.X) / System.Math.Max(nx - 1, 1);
        double dy = (bounds.Max.Y - bounds.Min.Y) / System.Math.Max(ny - 1, 1);
        double dz = (bounds.Max.Z - bounds.Min.Z) / System.Math.Max(nz - 1, 1);

        int idx = 0;
        for (int iz = 0; iz < nz; iz++)
            for (int iy = 0; iy < ny; iy++)
                for (int ix = 0; ix < nx; ix++)
                    points[idx++] = new Vec3(
                        bounds.Min.X + ix * dx,
                        bounds.Min.Y + iy * dy,
                        bounds.Min.Z + iz * dz);

        var results = new double[total];
        SignedDistances(points, results, bvh);
        return results;
    }

    /// <summary>
    /// Computes signed distances for a batch of query points.
    /// Negative values indicate the point is inside the solid.
    /// </summary>
    /// <param name="points">Query points.</param>
    /// <param name="results">Output signed distances.</param>
    /// <param name="bvh">The BVH tree for the solid mesh.</param>
    public void SignedDistances(Vec3[] points, double[] results, BvhTree bvh)
    {
        try
        {
            SignedDistancesGpu(points, results, bvh);
        }
        catch
        {
            SignedDistancesCpuFallback(points, results, bvh);
        }
    }

    private void SignedDistancesGpu(Vec3[] points, double[] results, BvhTree bvh)
    {
        // Pre-flatten BVH data
        var nodes = bvh.Nodes;
        var gpuNodes = new GpuBvhNode[nodes.Length];
        for (int i = 0; i < nodes.Length; i++)
        {
            ref readonly var node = ref nodes[i];
            gpuNodes[i] = new GpuBvhNode
            {
                BoundsMinX = (float)node.Bounds.Min.X,
                BoundsMinY = (float)node.Bounds.Min.Y,
                BoundsMinZ = (float)node.Bounds.Min.Z,
                BoundsMaxX = (float)node.Bounds.Max.X,
                BoundsMaxY = (float)node.Bounds.Max.Y,
                BoundsMaxZ = (float)node.Bounds.Max.Z,
                LeftOrStart = node.LeftOrStart,
                Right = node.Right,
                PrimitiveCount = node.PrimitiveCount
            };
        }

        var faceIndicesSpan = bvh.FaceIndices;
        var faceIndices = new int[faceIndicesSpan.Length];
        faceIndicesSpan.CopyTo(faceIndices);

        var mesh = bvh.Mesh;
        var triVerts = new GpuVec3[mesh.Faces.Count * 3];
        for (int i = 0; i < mesh.Faces.Count; i++)
        {
            var face = mesh.Faces[i];
            var verts = face.GetVertices();
            triVerts[i * 3 + 0] = new GpuVec3(verts[0].Position.X, verts[0].Position.Y, verts[0].Position.Z);
            triVerts[i * 3 + 1] = new GpuVec3(verts[1].Position.X, verts[1].Position.Y, verts[1].Position.Z);
            triVerts[i * 3 + 2] = new GpuVec3(verts[2].Position.X, verts[2].Position.Y, verts[2].Position.Z);
        }

        for (int batchStart = 0; batchStart < points.Length; batchStart += MaxBatchSize)
        {
            int batchEnd = System.Math.Min(batchStart + MaxBatchSize, points.Length);
            int batchSize = batchEnd - batchStart;

            var queryPts = new GpuVec3[batchSize];
            for (int i = 0; i < batchSize; i++)
            {
                var p = points[batchStart + i];
                queryPts[i] = new GpuVec3(p.X, p.Y, p.Z);
            }

            using var bufferPool = new GpuBufferPool(_ctx);
            var bvhNodesBuf = bufferPool.Upload<GpuBvhNode>(gpuNodes);
            var faceIndicesBuf = bufferPool.Upload<int>(faceIndices);
            var triVertsBuf = bufferPool.Upload<GpuVec3>(triVerts);
            var queryBuf = bufferPool.Upload<GpuVec3>(queryPts);
            ulong distSize = (ulong)(batchSize * sizeof(float));
            ulong classSize = (ulong)(batchSize * sizeof(int));
            var distBuf = bufferPool.CreateOutputBuffer(distSize);
            var classBuf = bufferPool.CreateOutputBuffer(classSize);

            DispatchBatch(bufferPool, gpuNodes, faceIndices, triVerts,
                bvhNodesBuf, faceIndicesBuf, triVertsBuf, queryBuf,
                distBuf, distSize, classBuf, classSize, (uint)batchSize);

            var gpuDistances = bufferPool.Download<float>(distBuf, batchSize);
            for (int i = 0; i < batchSize; i++)
                results[batchStart + i] = gpuDistances[i];
        }
    }

    private void DispatchBatch(GpuBufferPool bufferPool,
        GpuBvhNode[] gpuNodes, int[] faceIndices, GpuVec3[] triVerts,
        Buffer bvhNodesBuf, Buffer faceIndicesBuf, Buffer triVertsBuf,
        Buffer queryBuf, Buffer distBuf, ulong distSize,
        Buffer classBuf, ulong classSize, uint batchSize)
    {
        var pushConstants = new PushConstants
        {
            PointCount = batchSize,
            BvhNodeCount = (uint)gpuNodes.Length,
            FaceCount = (uint)(triVerts.Length / 3)
        };

        Span<(Buffer Buffer, ulong Size)> bindings = stackalloc (Buffer, ulong)[]
        {
            (bvhNodesBuf, (ulong)(gpuNodes.Length * Marshal.SizeOf<GpuBvhNode>())),
            (faceIndicesBuf, (ulong)(faceIndices.Length * sizeof(int))),
            (triVertsBuf, (ulong)(triVerts.Length * Marshal.SizeOf<GpuVec3>())),
            (queryBuf, (ulong)(batchSize * (uint)Marshal.SizeOf<GpuVec3>())),
            (distBuf, distSize),
            (classBuf, classSize)
        };

        var descSet = _sdfGridPipeline.AllocateDescriptorSet(bindings);

        var cmdBuffer = _ctx.AllocateCommandBuffer();
        var beginInfo = new CommandBufferBeginInfo
        {
            SType = StructureType.CommandBufferBeginInfo,
            Flags = CommandBufferUsageFlags.OneTimeSubmitBit
        };
        _ctx.Vk.BeginCommandBuffer(cmdBuffer, &beginInfo);

        _ctx.Vk.CmdBindPipeline(cmdBuffer, PipelineBindPoint.Compute, _sdfGridPipeline.Pipeline);
        _ctx.Vk.CmdBindDescriptorSets(cmdBuffer, PipelineBindPoint.Compute,
            _sdfGridPipeline.PipelineLayout, 0, 1, &descSet, 0, null);
        _ctx.Vk.CmdPushConstants(cmdBuffer, _sdfGridPipeline.PipelineLayout,
            ShaderStageFlags.ComputeBit, 0, (uint)sizeof(PushConstants), &pushConstants);

        uint groupCount = (batchSize + 255) / 256;
        _ctx.Vk.CmdDispatch(cmdBuffer, groupCount, 1, 1);

        _ctx.Vk.EndCommandBuffer(cmdBuffer);
        _ctx.SubmitAndWait(cmdBuffer);
        _ctx.FreeCommandBuffer(cmdBuffer);
    }

    private static void SignedDistancesCpuFallback(Vec3[] points, double[] results, BvhTree bvh)
    {
        var classifier = new BvhPointClassifier(bvh);
        for (int i = 0; i < points.Length; i++)
        {
            double dist = classifier.DistanceToSurface(points[i]);
            var cls = classifier.Classify(points[i]);
            results[i] = cls == SolidClassification.Inside ? -dist : dist;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PushConstants
    {
        public uint PointCount;
        public uint BvhNodeCount;
        public uint FaceCount;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _sdfGridPipeline.Dispose();
    }
}
