using System.Runtime.InteropServices;
using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Cutting;
using MdCsg.Gpu.Interop;
using MdCsg.Patches;
using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace MdCsg.Gpu;

/// <summary>
/// GPU-accelerated patch classification using Vulkan compute shaders.
/// Implements <see cref="IPatchClassificationStrategy"/> and falls back to CPU on runtime errors.
/// </summary>
public sealed unsafe class GpuPatchClassifier : IPatchClassificationStrategy, IDisposable
{
    private readonly VulkanContext _ctx;
    private readonly ComputePipeline _confidentPointPipeline;
    private readonly ComputePipeline _raycastPipeline;
    private readonly ComputePipeline _windingPipeline;
    private bool _disposed;

    /// <summary>
    /// Creates a new GPU patch classifier. Compiles shaders on first creation.
    /// </summary>
    public GpuPatchClassifier(VulkanContext ctx)
    {
        _ctx = ctx;

        // Push constants: 3 x uint (patchCount, bvhNodeCount, faceCount) = 12 bytes
        const uint pushSize = 12;

        _confidentPointPipeline = ComputePipeline.Create(ctx,
            "MdCsg.Gpu.Shaders.confident_point.comp", 7, pushSize);
        _raycastPipeline = ComputePipeline.Create(ctx,
            "MdCsg.Gpu.Shaders.raycast_classify.comp", 5, pushSize);
        _windingPipeline = ComputePipeline.Create(ctx,
            "MdCsg.Gpu.Shaders.winding_number.comp", 3, pushSize);
    }

    /// <inheritdoc />
    public int ClassifyAll(
        IReadOnlyList<Patch> patches,
        IReadOnlyList<FaceCutter.SubTriangle> subTriangles,
        BvhTree otherBvh,
        bool useWindingNumber)
    {
        if (patches.Count == 0) return 0;

        try
        {
            return ClassifyAllGpu(patches, subTriangles, otherBvh, useWindingNumber);
        }
        catch
        {
            // Fall back to CPU on any GPU error
            return PatchClassifier.ClassifyAll(patches, subTriangles, otherBvh, useWindingNumber);
        }
    }

    private int ClassifyAllGpu(
        IReadOnlyList<Patch> patches,
        IReadOnlyList<FaceCutter.SubTriangle> subTriangles,
        BvhTree otherBvh,
        bool useWindingNumber)
    {
        var data = BvhFlattener.Flatten(otherBvh, patches, subTriangles);

        using var bufferPool = new GpuBufferPool(_ctx);

        // Upload all data to GPU
        var bvhNodesBuf = bufferPool.Upload<GpuBvhNode>(data.BvhNodes);
        var faceIndicesBuf = bufferPool.Upload<int>(data.FaceIndices);
        var triVertsBuf = bufferPool.Upload<GpuVec3>(data.TriangleVertices);
        var subTriVertsBuf = bufferPool.Upload<GpuVec3>(data.SubTriangleVertices);
        var patchOffsetsBuf = bufferPool.Upload<int>(data.PatchOffsets);
        var patchSubTriIndicesBuf = bufferPool.Upload<int>(data.PatchSubTriangleIndices);

        // Output buffer for confident points (vec4 per patch: xyz = point, w = margin)
        ulong confidentPointsSize = (ulong)(patches.Count * 16); // sizeof(vec4) = 16
        var confidentPointsBuf = bufferPool.CreateOutputBuffer(confidentPointsSize);

        // Output buffer for classification results
        ulong classifyResultsSize = (ulong)(patches.Count * Marshal.SizeOf<GpuClassifyResult>());
        var classifyResultsBuf = bufferPool.CreateOutputBuffer(classifyResultsSize);

        // Push constants
        var pushConstants = new PushConstants
        {
            PatchCount = (uint)patches.Count,
            BvhNodeCount = (uint)data.BvhNodes.Length,
            FaceCount = (uint)(data.TriangleVertices.Length / 3)
        };

        // --- Phase 1: Confident point finding ---
        var confidentPointDescSet = _confidentPointPipeline.AllocateDescriptorSet(
            stackalloc (Buffer, ulong)[]
            {
                (bvhNodesBuf, (ulong)(data.BvhNodes.Length * Marshal.SizeOf<GpuBvhNode>())),
                (faceIndicesBuf, (ulong)(data.FaceIndices.Length * sizeof(int))),
                (triVertsBuf, (ulong)(data.TriangleVertices.Length * Marshal.SizeOf<GpuVec3>())),
                (subTriVertsBuf, (ulong)(data.SubTriangleVertices.Length * Marshal.SizeOf<GpuVec3>())),
                (patchOffsetsBuf, (ulong)(data.PatchOffsets.Length * sizeof(int))),
                (patchSubTriIndicesBuf, (ulong)(data.PatchSubTriangleIndices.Length * sizeof(int))),
                (confidentPointsBuf, confidentPointsSize)
            });

        var cmdBuffer = _ctx.AllocateCommandBuffer();
        var beginInfo = new CommandBufferBeginInfo
        {
            SType = StructureType.CommandBufferBeginInfo,
            Flags = CommandBufferUsageFlags.OneTimeSubmitBit
        };
        _ctx.Vk.BeginCommandBuffer(cmdBuffer, &beginInfo);

        // Dispatch confident point kernel (1 workgroup per patch, 256 threads each)
        _ctx.Vk.CmdBindPipeline(cmdBuffer, PipelineBindPoint.Compute, _confidentPointPipeline.Pipeline);
        _ctx.Vk.CmdBindDescriptorSets(cmdBuffer, PipelineBindPoint.Compute,
            _confidentPointPipeline.PipelineLayout, 0, 1, &confidentPointDescSet, 0, null);
        _ctx.Vk.CmdPushConstants(cmdBuffer, _confidentPointPipeline.PipelineLayout,
            ShaderStageFlags.ComputeBit, 0, (uint)sizeof(PushConstants), &pushConstants);
        _ctx.Vk.CmdDispatch(cmdBuffer, (uint)patches.Count, 1, 1);

        // Memory barrier between confident point and classification
        var memoryBarrier = new MemoryBarrier
        {
            SType = StructureType.MemoryBarrier,
            SrcAccessMask = AccessFlags.ShaderWriteBit,
            DstAccessMask = AccessFlags.ShaderReadBit
        };
        _ctx.Vk.CmdPipelineBarrier(cmdBuffer,
            PipelineStageFlags.ComputeShaderBit, PipelineStageFlags.ComputeShaderBit,
            0, 1, &memoryBarrier, 0, null, 0, null);

        // --- Phase 2: Classification (raycast or winding number) ---
        if (useWindingNumber)
        {
            var windingDescSet = _windingPipeline.AllocateDescriptorSet(
                stackalloc (Buffer, ulong)[]
                {
                    (triVertsBuf, (ulong)(data.TriangleVertices.Length * Marshal.SizeOf<GpuVec3>())),
                    (confidentPointsBuf, confidentPointsSize),
                    (classifyResultsBuf, classifyResultsSize)
                });

            _ctx.Vk.CmdBindPipeline(cmdBuffer, PipelineBindPoint.Compute, _windingPipeline.Pipeline);
            _ctx.Vk.CmdBindDescriptorSets(cmdBuffer, PipelineBindPoint.Compute,
                _windingPipeline.PipelineLayout, 0, 1, &windingDescSet, 0, null);
            _ctx.Vk.CmdPushConstants(cmdBuffer, _windingPipeline.PipelineLayout,
                ShaderStageFlags.ComputeBit, 0, (uint)sizeof(PushConstants), &pushConstants);
            // One workgroup (256 threads) per patch
            _ctx.Vk.CmdDispatch(cmdBuffer, (uint)patches.Count, 1, 1);
        }
        else
        {
            var raycastDescSet = _raycastPipeline.AllocateDescriptorSet(
                stackalloc (Buffer, ulong)[]
                {
                    (bvhNodesBuf, (ulong)(data.BvhNodes.Length * Marshal.SizeOf<GpuBvhNode>())),
                    (faceIndicesBuf, (ulong)(data.FaceIndices.Length * sizeof(int))),
                    (triVertsBuf, (ulong)(data.TriangleVertices.Length * Marshal.SizeOf<GpuVec3>())),
                    (confidentPointsBuf, confidentPointsSize),
                    (classifyResultsBuf, classifyResultsSize)
                });

            _ctx.Vk.CmdBindPipeline(cmdBuffer, PipelineBindPoint.Compute, _raycastPipeline.Pipeline);
            _ctx.Vk.CmdBindDescriptorSets(cmdBuffer, PipelineBindPoint.Compute,
                _raycastPipeline.PipelineLayout, 0, 1, &raycastDescSet, 0, null);
            _ctx.Vk.CmdPushConstants(cmdBuffer, _raycastPipeline.PipelineLayout,
                ShaderStageFlags.ComputeBit, 0, (uint)sizeof(PushConstants), &pushConstants);
            // One thread per patch, workgroup size 64
            uint groupCount = ((uint)patches.Count + 63) / 64;
            _ctx.Vk.CmdDispatch(cmdBuffer, groupCount, 1, 1);
        }

        _ctx.Vk.EndCommandBuffer(cmdBuffer);
        _ctx.SubmitAndWait(cmdBuffer);
        _ctx.FreeCommandBuffer(cmdBuffer);

        // Download results
        var results = bufferPool.Download<GpuClassifyResult>(classifyResultsBuf, patches.Count);

        // Download confident points for writing back
        var confidentPointsRaw = bufferPool.Download<ConfidentPointResult>(confidentPointsBuf, patches.Count);

        // Write results back to patch objects
        int degenerateCount = 0;
        for (int i = 0; i < patches.Count; i++)
        {
            var patch = patches[i];
            var cp = confidentPointsRaw[i];
            var result = results[i];

            patch.ConfidentPoint = new MdCsg.Math.Vec3(cp.X, cp.Y, cp.Z);
            float margin = BitConverter.Int32BitsToSingle(result.MarginBits);
            patch.HasConfidentPoint = margin > PatchClassifier.DegenerateMarginThreshold;
            patch.IsInside = result.IsInside != 0;

            if (!patch.HasConfidentPoint)
                degenerateCount++;
        }

        return degenerateCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PushConstants
    {
        public uint PatchCount;
        public uint BvhNodeCount;
        public uint FaceCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ConfidentPointResult
    {
        public float X, Y, Z, Margin;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _confidentPointPipeline.Dispose();
        _raycastPipeline.Dispose();
        _windingPipeline.Dispose();
    }
}
