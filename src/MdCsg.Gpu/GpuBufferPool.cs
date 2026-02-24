using System.Runtime.InteropServices;
using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace MdCsg.Gpu;

/// <summary>
/// Manages Vulkan buffer creation, upload, and download via staging buffers.
/// </summary>
public sealed unsafe class GpuBufferPool : IDisposable
{
    private readonly VulkanContext _ctx;
    private readonly List<(Buffer Buffer, DeviceMemory Memory)> _allocations = [];
    private bool _disposed;

    /// <summary>
    /// Creates a new buffer pool bound to the given Vulkan context.
    /// </summary>
    public GpuBufferPool(VulkanContext ctx)
    {
        _ctx = ctx;
    }

    /// <summary>
    /// Creates a device-local storage buffer and uploads data to it via a staging buffer.
    /// </summary>
    public Buffer Upload<T>(ReadOnlySpan<T> data) where T : unmanaged
    {
        ulong size = (ulong)(data.Length * sizeof(T));
        if (size == 0) size = 4; // Vulkan requires non-zero buffer size

        // Create staging buffer (host-visible)
        var staging = CreateBuffer(size,
            BufferUsageFlags.TransferSrcBit,
            MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit);

        // Copy data to staging
        void* mapped;
        _ctx.Vk.MapMemory(_ctx.Device, staging.Memory, 0, size, 0, &mapped);
        if (data.Length > 0)
            data.CopyTo(new Span<T>(mapped, data.Length));
        _ctx.Vk.UnmapMemory(_ctx.Device, staging.Memory);

        // Create device-local buffer
        var deviceBuffer = CreateBuffer(size,
            BufferUsageFlags.StorageBufferBit | BufferUsageFlags.TransferDstBit,
            MemoryPropertyFlags.DeviceLocalBit);

        // Copy staging -> device
        CopyBuffer(staging.Buffer, deviceBuffer.Buffer, size);

        // Free staging immediately
        _ctx.Vk.DestroyBuffer(_ctx.Device, staging.Buffer, null);
        _ctx.Vk.FreeMemory(_ctx.Device, staging.Memory, null);
        _allocations.Remove(staging);

        return deviceBuffer.Buffer;
    }

    /// <summary>
    /// Creates a device-local storage buffer suitable for GPU output, initialized to zero.
    /// </summary>
    public Buffer CreateOutputBuffer(ulong size)
    {
        if (size == 0) size = 4;

        var buffer = CreateBuffer(size,
            BufferUsageFlags.StorageBufferBit | BufferUsageFlags.TransferSrcBit,
            MemoryPropertyFlags.DeviceLocalBit);

        return buffer.Buffer;
    }

    /// <summary>
    /// Downloads data from a device-local buffer via a staging buffer.
    /// </summary>
    public T[] Download<T>(Buffer deviceBuffer, int count) where T : unmanaged
    {
        ulong size = (ulong)(count * sizeof(T));
        if (size == 0) return [];

        // Create staging buffer (host-visible)
        var staging = CreateBuffer(size,
            BufferUsageFlags.TransferDstBit,
            MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit);

        // Copy device -> staging
        CopyBuffer(deviceBuffer, staging.Buffer, size);

        // Read from staging
        void* mapped;
        _ctx.Vk.MapMemory(_ctx.Device, staging.Memory, 0, size, 0, &mapped);
        var result = new T[count];
        new ReadOnlySpan<T>(mapped, count).CopyTo(result);
        _ctx.Vk.UnmapMemory(_ctx.Device, staging.Memory);

        // Free staging
        _ctx.Vk.DestroyBuffer(_ctx.Device, staging.Buffer, null);
        _ctx.Vk.FreeMemory(_ctx.Device, staging.Memory, null);
        _allocations.Remove(staging);

        return result;
    }

    private (Buffer Buffer, DeviceMemory Memory) CreateBuffer(
        ulong size, BufferUsageFlags usage, MemoryPropertyFlags memoryFlags)
    {
        var bufferInfo = new BufferCreateInfo
        {
            SType = StructureType.BufferCreateInfo,
            Size = size,
            Usage = usage,
            SharingMode = SharingMode.Exclusive
        };

        _ctx.Vk.CreateBuffer(_ctx.Device, &bufferInfo, null, out var buffer);

        _ctx.Vk.GetBufferMemoryRequirements(_ctx.Device, buffer, out var memReqs);

        var allocInfo = new MemoryAllocateInfo
        {
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = memReqs.Size,
            MemoryTypeIndex = _ctx.FindMemoryType(memReqs.MemoryTypeBits, memoryFlags)
        };

        _ctx.Vk.AllocateMemory(_ctx.Device, &allocInfo, null, out var memory);
        _ctx.Vk.BindBufferMemory(_ctx.Device, buffer, memory, 0);

        var alloc = (buffer, memory);
        _allocations.Add(alloc);
        return alloc;
    }

    private void CopyBuffer(Buffer src, Buffer dst, ulong size)
    {
        var cmdBuffer = _ctx.AllocateCommandBuffer();

        var beginInfo = new CommandBufferBeginInfo
        {
            SType = StructureType.CommandBufferBeginInfo,
            Flags = CommandBufferUsageFlags.OneTimeSubmitBit
        };

        _ctx.Vk.BeginCommandBuffer(cmdBuffer, &beginInfo);

        var copyRegion = new BufferCopy { Size = size };
        _ctx.Vk.CmdCopyBuffer(cmdBuffer, src, dst, 1, &copyRegion);

        _ctx.Vk.EndCommandBuffer(cmdBuffer);
        _ctx.SubmitAndWait(cmdBuffer);
        _ctx.FreeCommandBuffer(cmdBuffer);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var (buffer, memory) in _allocations)
        {
            _ctx.Vk.DestroyBuffer(_ctx.Device, buffer, null);
            _ctx.Vk.FreeMemory(_ctx.Device, memory, null);
        }
        _allocations.Clear();
    }
}
