using System.Runtime.InteropServices;
using MdCsg.Api;
using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Gpu.Interop;
using MdCsg.Math;
using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace MdCsg.Gpu;

/// <summary>
/// GPU-accelerated point cloud query engine implementing <see cref="IPointCloudQueryStrategy"/>.
/// Falls back to CPU on runtime errors.
/// </summary>
public sealed unsafe class GpuPointCloudQuery : IPointCloudQueryStrategy, IDisposable
{
    private readonly VulkanContext _ctx;
    private readonly ComputePipeline _distancePipeline;
    private readonly ComputePipeline _classifyPipeline;
    private readonly ComputePipeline _signedDistancePipeline;
    private bool _disposed;

    /// <summary>
    /// Maximum number of points per GPU batch to limit memory usage.
    /// </summary>
    public int MaxBatchSize { get; set; } = 4_000_000;

    /// <summary>
    /// Creates a GPU point cloud query engine.
    /// </summary>
    /// <param name="ctx">The Vulkan context to use.</param>
    public GpuPointCloudQuery(VulkanContext ctx)
    {
        _ctx = ctx;
        const uint pushSize = 12; // 3 x uint

        _distancePipeline = ComputePipeline.Create(ctx,
            "MdCsg.Gpu.Shaders.batch_distance.comp", 5, pushSize);
        _classifyPipeline = ComputePipeline.Create(ctx,
            "MdCsg.Gpu.Shaders.batch_classify.comp", 5, pushSize);
        _signedDistancePipeline = ComputePipeline.Create(ctx,
            "MdCsg.Gpu.Shaders.batch_signed_distance.comp", 5, pushSize);
    }

    /// <summary>
    /// Attempts to create a GPU point cloud query engine.
    /// Returns null if Vulkan is not available.
    /// </summary>
    public static GpuPointCloudQuery? TryCreate()
    {
        var ctx = VulkanContext.TryCreate();
        if (ctx == null) return null;

        try
        {
            return new GpuPointCloudQuery(ctx);
        }
        catch
        {
            ctx.Dispose();
            return null;
        }
    }

    /// <inheritdoc />
    public void Classify(Vec3[] points, SolidClassification[] results, BvhTree bvh, bool useWindingNumber)
    {
        try
        {
            ClassifyGpu(points, results, bvh);
        }
        catch
        {
            ClassifyCpuFallback(points, results, bvh, useWindingNumber);
        }
    }

    /// <inheritdoc />
    public void Distances(Vec3[] points, double[] results, BvhTree bvh)
    {
        try
        {
            DistancesGpu(points, results, bvh);
        }
        catch
        {
            DistancesCpuFallback(points, results, bvh);
        }
    }

    /// <inheritdoc />
    public void SignedDistances(Vec3[] points, double[] results, BvhTree bvh, bool useWindingNumber)
    {
        try
        {
            SignedDistancesGpu(points, results, bvh);
        }
        catch
        {
            SignedDistancesCpuFallback(points, results, bvh, useWindingNumber);
        }
    }

    private void ClassifyGpu(Vec3[] points, SolidClassification[] results, BvhTree bvh)
    {
        ProcessInBatches(points, bvh, (batchPoints, data, bufferPool, bvhNodesBuf, faceIndicesBuf, triVertsBuf) =>
        {
            int count = batchPoints.Length;
            var queryBuf = bufferPool.Upload<GpuVec3>(data.QueryPoints);
            ulong outputSize = (ulong)(count * sizeof(int));
            var outputBuf = bufferPool.CreateOutputBuffer(outputSize);

            var pushConstants = new PushConstants
            {
                PointCount = (uint)count,
                BvhNodeCount = (uint)data.BvhNodes.Length,
                FaceCount = (uint)(data.TriangleVertices.Length / 3)
            };

            DispatchSinglePass(_classifyPipeline, bufferPool, pushConstants,
                bvhNodesBuf, (ulong)(data.BvhNodes.Length * Marshal.SizeOf<GpuBvhNode>()),
                faceIndicesBuf, (ulong)(data.FaceIndices.Length * sizeof(int)),
                triVertsBuf, (ulong)(data.TriangleVertices.Length * Marshal.SizeOf<GpuVec3>()),
                queryBuf, (ulong)(count * Marshal.SizeOf<GpuVec3>()),
                outputBuf, outputSize,
                (uint)count);

            var gpuResults = bufferPool.Download<int>(outputBuf, count);
            return gpuResults;
        },
        (batchStart, gpuResults) =>
        {
            var intResults = (int[])gpuResults;
            for (int i = 0; i < intResults.Length; i++)
                results[batchStart + i] = intResults[i] != 0 ? SolidClassification.Inside : SolidClassification.Outside;
        });
    }

    private void DistancesGpu(Vec3[] points, double[] results, BvhTree bvh)
    {
        ProcessInBatches(points, bvh, (batchPoints, data, bufferPool, bvhNodesBuf, faceIndicesBuf, triVertsBuf) =>
        {
            int count = batchPoints.Length;
            var queryBuf = bufferPool.Upload<GpuVec3>(data.QueryPoints);
            ulong outputSize = (ulong)(count * sizeof(float));
            var outputBuf = bufferPool.CreateOutputBuffer(outputSize);

            var pushConstants = new PushConstants
            {
                PointCount = (uint)count,
                BvhNodeCount = (uint)data.BvhNodes.Length,
                FaceCount = (uint)(data.TriangleVertices.Length / 3)
            };

            DispatchSinglePass(_distancePipeline, bufferPool, pushConstants,
                bvhNodesBuf, (ulong)(data.BvhNodes.Length * Marshal.SizeOf<GpuBvhNode>()),
                faceIndicesBuf, (ulong)(data.FaceIndices.Length * sizeof(int)),
                triVertsBuf, (ulong)(data.TriangleVertices.Length * Marshal.SizeOf<GpuVec3>()),
                queryBuf, (ulong)(count * Marshal.SizeOf<GpuVec3>()),
                outputBuf, outputSize,
                (uint)count);

            var gpuResults = bufferPool.Download<float>(outputBuf, count);
            return gpuResults;
        },
        (batchStart, gpuResults) =>
        {
            var floatResults = (float[])gpuResults;
            for (int i = 0; i < floatResults.Length; i++)
                results[batchStart + i] = floatResults[i];
        });
    }

    private void SignedDistancesGpu(Vec3[] points, double[] results, BvhTree bvh)
    {
        ProcessInBatches(points, bvh, (batchPoints, data, bufferPool, bvhNodesBuf, faceIndicesBuf, triVertsBuf) =>
        {
            int count = batchPoints.Length;
            var queryBuf = bufferPool.Upload<GpuVec3>(data.QueryPoints);
            ulong outputSize = (ulong)(count * sizeof(float));
            var outputBuf = bufferPool.CreateOutputBuffer(outputSize);

            var pushConstants = new PushConstants
            {
                PointCount = (uint)count,
                BvhNodeCount = (uint)data.BvhNodes.Length,
                FaceCount = (uint)(data.TriangleVertices.Length / 3)
            };

            DispatchSinglePass(_signedDistancePipeline, bufferPool, pushConstants,
                bvhNodesBuf, (ulong)(data.BvhNodes.Length * Marshal.SizeOf<GpuBvhNode>()),
                faceIndicesBuf, (ulong)(data.FaceIndices.Length * sizeof(int)),
                triVertsBuf, (ulong)(data.TriangleVertices.Length * Marshal.SizeOf<GpuVec3>()),
                queryBuf, (ulong)(count * Marshal.SizeOf<GpuVec3>()),
                outputBuf, outputSize,
                (uint)count);

            var gpuResults = bufferPool.Download<float>(outputBuf, count);
            return gpuResults;
        },
        (batchStart, gpuResults) =>
        {
            var floatResults = (float[])gpuResults;
            for (int i = 0; i < floatResults.Length; i++)
                results[batchStart + i] = floatResults[i];
        });
    }

    private void ProcessInBatches(Vec3[] points, BvhTree bvh,
        Func<Vec3[], PointCloudFlattener.FlattenedPointCloudData, GpuBufferPool, Buffer, Buffer, Buffer, object> processFunc,
        Action<int, object> writeBack)
    {
        // Pre-flatten BVH data (shared across batches)
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

            var batchPoints = new Vec3[batchSize];
            Array.Copy(points, batchStart, batchPoints, 0, batchSize);

            var queryPts = new GpuVec3[batchSize];
            for (int i = 0; i < batchSize; i++)
                queryPts[i] = new GpuVec3(batchPoints[i].X, batchPoints[i].Y, batchPoints[i].Z);

            var data = new PointCloudFlattener.FlattenedPointCloudData
            {
                BvhNodes = gpuNodes,
                FaceIndices = faceIndices,
                TriangleVertices = triVerts,
                QueryPoints = queryPts
            };

            using var bufferPool = new GpuBufferPool(_ctx);
            var bvhNodesBuf = bufferPool.Upload<GpuBvhNode>(gpuNodes);
            var faceIndicesBuf = bufferPool.Upload<int>(faceIndices);
            var triVertsBuf = bufferPool.Upload<GpuVec3>(triVerts);

            var gpuResults = processFunc(batchPoints, data, bufferPool, bvhNodesBuf, faceIndicesBuf, triVertsBuf);
            writeBack(batchStart, gpuResults);
        }
    }

    private void DispatchSinglePass(ComputePipeline pipeline, GpuBufferPool bufferPool,
        PushConstants pushConstants,
        Buffer buf0, ulong size0,
        Buffer buf1, ulong size1,
        Buffer buf2, ulong size2,
        Buffer buf3, ulong size3,
        Buffer buf4, ulong size4,
        uint pointCount)
    {
        var descSet = pipeline.AllocateDescriptorSet(
            stackalloc (Buffer, ulong)[]
            {
                (buf0, size0),
                (buf1, size1),
                (buf2, size2),
                (buf3, size3),
                (buf4, size4)
            });

        var cmdBuffer = _ctx.AllocateCommandBuffer();
        var beginInfo = new CommandBufferBeginInfo
        {
            SType = StructureType.CommandBufferBeginInfo,
            Flags = CommandBufferUsageFlags.OneTimeSubmitBit
        };
        _ctx.Vk.BeginCommandBuffer(cmdBuffer, &beginInfo);

        _ctx.Vk.CmdBindPipeline(cmdBuffer, PipelineBindPoint.Compute, pipeline.Pipeline);
        _ctx.Vk.CmdBindDescriptorSets(cmdBuffer, PipelineBindPoint.Compute,
            pipeline.PipelineLayout, 0, 1, &descSet, 0, null);
        _ctx.Vk.CmdPushConstants(cmdBuffer, pipeline.PipelineLayout,
            ShaderStageFlags.ComputeBit, 0, (uint)sizeof(PushConstants), &pushConstants);

        uint groupCount = (pointCount + 255) / 256;
        _ctx.Vk.CmdDispatch(cmdBuffer, groupCount, 1, 1);

        _ctx.Vk.EndCommandBuffer(cmdBuffer);
        _ctx.SubmitAndWait(cmdBuffer);
        _ctx.FreeCommandBuffer(cmdBuffer);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PushConstants
    {
        public uint PointCount;
        public uint BvhNodeCount;
        public uint FaceCount;
    }

    private static void ClassifyCpuFallback(Vec3[] points, SolidClassification[] results,
        BvhTree bvh, bool useWindingNumber)
    {
        for (int i = 0; i < points.Length; i++)
        {
            results[i] = useWindingNumber
                ? WindingNumberClassifier.Classify(points[i], bvh)
                : RayCastClassifier.Classify(points[i], bvh);
        }
    }

    private static void DistancesCpuFallback(Vec3[] points, double[] results, BvhTree bvh)
    {
        var classifier = new BvhPointClassifier(bvh);
        for (int i = 0; i < points.Length; i++)
            results[i] = classifier.DistanceToSurface(points[i]);
    }

    private static void SignedDistancesCpuFallback(Vec3[] points, double[] results,
        BvhTree bvh, bool useWindingNumber)
    {
        var classifier = new BvhPointClassifier(bvh, useWindingNumber);
        for (int i = 0; i < points.Length; i++)
        {
            double dist = classifier.DistanceToSurface(points[i]);
            var cls = classifier.Classify(points[i]);
            results[i] = cls == SolidClassification.Inside ? -dist : dist;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _distancePipeline.Dispose();
        _classifyPipeline.Dispose();
        _signedDistancePipeline.Dispose();
    }
}
