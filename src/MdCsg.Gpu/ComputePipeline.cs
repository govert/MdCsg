using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace MdCsg.Gpu;

/// <summary>
/// Wraps a Vulkan compute pipeline created from GLSL source compiled to SPIR-V at runtime.
/// </summary>
public sealed unsafe class ComputePipeline : IDisposable
{
    private readonly VulkanContext _ctx;
    private readonly ShaderModule _shaderModule;
    private readonly DescriptorSetLayout _descriptorSetLayout;
    private readonly PipelineLayout _pipelineLayout;
    private readonly Pipeline _pipeline;
    private readonly int _bindingCount;
    private bool _disposed;

    /// <summary>The descriptor set layout for this pipeline.</summary>
    public DescriptorSetLayout DescriptorSetLayout => _descriptorSetLayout;

    /// <summary>The pipeline layout.</summary>
    public PipelineLayout PipelineLayout => _pipelineLayout;

    /// <summary>The Vulkan pipeline handle.</summary>
    public Pipeline Pipeline => _pipeline;

    private ComputePipeline(
        VulkanContext ctx, ShaderModule shaderModule,
        DescriptorSetLayout descriptorSetLayout, PipelineLayout pipelineLayout,
        Pipeline pipeline, int bindingCount)
    {
        _ctx = ctx;
        _shaderModule = shaderModule;
        _descriptorSetLayout = descriptorSetLayout;
        _pipelineLayout = pipelineLayout;
        _pipeline = pipeline;
        _bindingCount = bindingCount;
    }

    /// <summary>
    /// Creates a compute pipeline from an embedded GLSL resource (compiled to SPIR-V at runtime).
    /// </summary>
    /// <param name="ctx">The Vulkan context.</param>
    /// <param name="resourceName">The embedded GLSL resource name (e.g., "MdCsg.Gpu.Shaders.confident_point.comp").</param>
    /// <param name="bindingCount">Number of storage buffer bindings.</param>
    /// <param name="pushConstantSize">Size of push constants in bytes (0 for none).</param>
    public static ComputePipeline Create(
        VulkanContext ctx, string resourceName, int bindingCount, uint pushConstantSize = 0)
    {
        var spirv = ShaderCompiler.CompileCompute(resourceName);

        // Create shader module
        fixed (byte* pCode = spirv)
        {
            var moduleInfo = new ShaderModuleCreateInfo
            {
                SType = StructureType.ShaderModuleCreateInfo,
                CodeSize = (nuint)spirv.Length,
                PCode = (uint*)pCode
            };

            ctx.Vk.CreateShaderModule(ctx.Device, &moduleInfo, null, out var shaderModule);

            // Create descriptor set layout with N storage buffer bindings
            var bindings = new DescriptorSetLayoutBinding[bindingCount];
            for (int i = 0; i < bindingCount; i++)
            {
                bindings[i] = new DescriptorSetLayoutBinding
                {
                    Binding = (uint)i,
                    DescriptorType = DescriptorType.StorageBuffer,
                    DescriptorCount = 1,
                    StageFlags = ShaderStageFlags.ComputeBit
                };
            }

            DescriptorSetLayout descriptorSetLayout;
            fixed (DescriptorSetLayoutBinding* pBindings = bindings)
            {
                var layoutInfo = new DescriptorSetLayoutCreateInfo
                {
                    SType = StructureType.DescriptorSetLayoutCreateInfo,
                    BindingCount = (uint)bindingCount,
                    PBindings = pBindings
                };

                ctx.Vk.CreateDescriptorSetLayout(ctx.Device, &layoutInfo, null, out descriptorSetLayout);
            }

            // Create pipeline layout
            PipelineLayout pipelineLayout;
            var pushConstantRange = new PushConstantRange
            {
                StageFlags = ShaderStageFlags.ComputeBit,
                Offset = 0,
                Size = pushConstantSize
            };

            var pLayout = descriptorSetLayout;
            var pipelineLayoutInfo = new PipelineLayoutCreateInfo
            {
                SType = StructureType.PipelineLayoutCreateInfo,
                SetLayoutCount = 1,
                PSetLayouts = &pLayout,
                PushConstantRangeCount = pushConstantSize > 0 ? 1u : 0u,
                PPushConstantRanges = pushConstantSize > 0 ? &pushConstantRange : null
            };

            ctx.Vk.CreatePipelineLayout(ctx.Device, &pipelineLayoutInfo, null, out pipelineLayout);

            // Create compute pipeline
            var entryPoint = "main"u8;
            fixed (byte* pEntryPoint = entryPoint)
            {
                var stageInfo = new PipelineShaderStageCreateInfo
                {
                    SType = StructureType.PipelineShaderStageCreateInfo,
                    Stage = ShaderStageFlags.ComputeBit,
                    Module = shaderModule,
                    PName = pEntryPoint
                };

                var pipelineInfo = new ComputePipelineCreateInfo
                {
                    SType = StructureType.ComputePipelineCreateInfo,
                    Stage = stageInfo,
                    Layout = pipelineLayout
                };

                ctx.Vk.CreateComputePipelines(ctx.Device, default, 1, &pipelineInfo, null, out var pipeline);

                return new ComputePipeline(ctx, shaderModule, descriptorSetLayout, pipelineLayout, pipeline, bindingCount);
            }
        }
    }

    /// <summary>
    /// Allocates a descriptor set and binds the given storage buffers.
    /// </summary>
    public DescriptorSet AllocateDescriptorSet(ReadOnlySpan<(Buffer Buffer, ulong Size)> buffers)
    {
        if (buffers.Length != _bindingCount)
            throw new ArgumentException($"Expected {_bindingCount} buffers, got {buffers.Length}.");

        var layout = _descriptorSetLayout;
        var allocInfo = new DescriptorSetAllocateInfo
        {
            SType = StructureType.DescriptorSetAllocateInfo,
            DescriptorPool = _ctx.DescriptorPool,
            DescriptorSetCount = 1,
            PSetLayouts = &layout
        };

        _ctx.Vk.AllocateDescriptorSets(_ctx.Device, &allocInfo, out var descriptorSet);

        var bufferInfos = new DescriptorBufferInfo[_bindingCount];
        var writes = new WriteDescriptorSet[_bindingCount];

        for (int i = 0; i < _bindingCount; i++)
        {
            bufferInfos[i] = new DescriptorBufferInfo
            {
                Buffer = buffers[i].Buffer,
                Offset = 0,
                Range = buffers[i].Size
            };
        }

        fixed (DescriptorBufferInfo* pBufferInfos = bufferInfos)
        {
            for (int i = 0; i < _bindingCount; i++)
            {
                writes[i] = new WriteDescriptorSet
                {
                    SType = StructureType.WriteDescriptorSet,
                    DstSet = descriptorSet,
                    DstBinding = (uint)i,
                    DescriptorCount = 1,
                    DescriptorType = DescriptorType.StorageBuffer,
                    PBufferInfo = pBufferInfos + i
                };
            }

            fixed (WriteDescriptorSet* pWrites = writes)
                _ctx.Vk.UpdateDescriptorSets(_ctx.Device, (uint)_bindingCount, pWrites, 0, null);
        }

        return descriptorSet;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _ctx.Vk.DestroyPipeline(_ctx.Device, _pipeline, null);
        _ctx.Vk.DestroyPipelineLayout(_ctx.Device, _pipelineLayout, null);
        _ctx.Vk.DestroyDescriptorSetLayout(_ctx.Device, _descriptorSetLayout, null);
        _ctx.Vk.DestroyShaderModule(_ctx.Device, _shaderModule, null);
    }
}
