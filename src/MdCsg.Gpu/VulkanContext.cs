using System.Runtime.InteropServices;
using Silk.NET.Core;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace MdCsg.Gpu;

/// <summary>
/// Manages a Vulkan instance, physical device, logical device, and compute queue.
/// </summary>
public sealed unsafe class VulkanContext : IDisposable
{
    private readonly Vk _vk;
    private readonly Instance _instance;
    private readonly PhysicalDevice _physicalDevice;
    private readonly Device _device;
    private readonly Queue _computeQueue;
    private readonly CommandPool _commandPool;
    private readonly DescriptorPool _descriptorPool;
    private readonly uint _computeQueueFamily;
    private bool _disposed;

    /// <summary>The Vulkan API entry point.</summary>
    public Vk Vk => _vk;

    /// <summary>The Vulkan instance.</summary>
    public Instance Instance => _instance;

    /// <summary>The selected physical device.</summary>
    public PhysicalDevice PhysicalDevice => _physicalDevice;

    /// <summary>The logical device.</summary>
    public Device Device => _device;

    /// <summary>The compute queue.</summary>
    public Queue ComputeQueue => _computeQueue;

    /// <summary>The command pool for compute operations.</summary>
    public CommandPool CommandPool => _commandPool;

    /// <summary>The descriptor pool for binding resources.</summary>
    public DescriptorPool DescriptorPool => _descriptorPool;

    /// <summary>The compute queue family index.</summary>
    public uint ComputeQueueFamily => _computeQueueFamily;

    /// <summary>The name of the selected physical device.</summary>
    public string DeviceName { get; }

    /// <summary>Whether this context is valid and ready for compute work.</summary>
    public bool IsAvailable => !_disposed;

    private VulkanContext(
        Vk vk, Instance instance, PhysicalDevice physicalDevice,
        Device device, Queue computeQueue, CommandPool commandPool,
        DescriptorPool descriptorPool, uint computeQueueFamily, string deviceName)
    {
        _vk = vk;
        _instance = instance;
        _physicalDevice = physicalDevice;
        _device = device;
        _computeQueue = computeQueue;
        _commandPool = commandPool;
        _descriptorPool = descriptorPool;
        _computeQueueFamily = computeQueueFamily;
        DeviceName = deviceName;
    }

    /// <summary>
    /// Attempts to create a Vulkan context. Returns null if Vulkan is not available.
    /// </summary>
    public static VulkanContext? TryCreate()
    {
        Vk? vk = null;
        Instance instance = default;
        Device device = default;
        CommandPool commandPool = default;
        DescriptorPool descriptorPool = default;

        try
        {
            vk = Vk.GetApi();

            // Create instance
            var appInfo = new ApplicationInfo
            {
                SType = StructureType.ApplicationInfo,
                ApiVersion = Vk.Version10
            };

            var instanceInfo = new InstanceCreateInfo
            {
                SType = StructureType.InstanceCreateInfo,
                PApplicationInfo = &appInfo
            };

            if (vk.CreateInstance(&instanceInfo, null, out instance) != Result.Success)
                return null;

            // Enumerate physical devices
            uint deviceCount = 0;
            vk.EnumeratePhysicalDevices(instance, &deviceCount, null);
            if (deviceCount == 0)
            {
                vk.DestroyInstance(instance, null);
                return null;
            }

            var physicalDevices = new PhysicalDevice[deviceCount];
            fixed (PhysicalDevice* pDevices = physicalDevices)
                vk.EnumeratePhysicalDevices(instance, &deviceCount, pDevices);

            // Select device: prefer discrete GPU, accept integrated
            PhysicalDevice selectedDevice = default;
            uint computeFamily = uint.MaxValue;
            string deviceName = "Unknown";
            bool foundDiscrete = false;

            foreach (var pd in physicalDevices)
            {
                vk.GetPhysicalDeviceProperties(pd, out var props);

                // Find compute queue family
                uint queueFamilyCount = 0;
                vk.GetPhysicalDeviceQueueFamilyProperties(pd, &queueFamilyCount, null);
                var queueFamilies = new QueueFamilyProperties[queueFamilyCount];
                fixed (QueueFamilyProperties* pFamilies = queueFamilies)
                    vk.GetPhysicalDeviceQueueFamilyProperties(pd, &queueFamilyCount, pFamilies);

                uint? computeIdx = null;
                for (uint i = 0; i < queueFamilyCount; i++)
                {
                    if ((queueFamilies[i].QueueFlags & QueueFlags.ComputeBit) != 0)
                    {
                        computeIdx = i;
                        break;
                    }
                }

                if (computeIdx == null) continue;

                bool isDiscrete = props.DeviceType == PhysicalDeviceType.DiscreteGpu;
                if (isDiscrete || !foundDiscrete)
                {
                    selectedDevice = pd;
                    computeFamily = computeIdx.Value;
                    deviceName = Marshal.PtrToStringAnsi((nint)props.DeviceName)!;
                    foundDiscrete = isDiscrete;
                }
            }

            if (computeFamily == uint.MaxValue)
            {
                vk.DestroyInstance(instance, null);
                return null;
            }

            // Create logical device with one compute queue
            float queuePriority = 1.0f;
            var queueCreateInfo = new DeviceQueueCreateInfo
            {
                SType = StructureType.DeviceQueueCreateInfo,
                QueueFamilyIndex = computeFamily,
                QueueCount = 1,
                PQueuePriorities = &queuePriority
            };

            var deviceCreateInfo = new DeviceCreateInfo
            {
                SType = StructureType.DeviceCreateInfo,
                QueueCreateInfoCount = 1,
                PQueueCreateInfos = &queueCreateInfo
            };

            if (vk.CreateDevice(selectedDevice, &deviceCreateInfo, null, out device) != Result.Success)
            {
                vk.DestroyInstance(instance, null);
                return null;
            }

            vk.GetDeviceQueue(device, computeFamily, 0, out var computeQueue);

            // Create command pool
            var poolInfo = new CommandPoolCreateInfo
            {
                SType = StructureType.CommandPoolCreateInfo,
                QueueFamilyIndex = computeFamily,
                Flags = CommandPoolCreateFlags.ResetCommandBufferBit
            };

            if (vk.CreateCommandPool(device, &poolInfo, null, out commandPool) != Result.Success)
            {
                vk.DestroyDevice(device, null);
                vk.DestroyInstance(instance, null);
                return null;
            }

            // Create descriptor pool (generous sizes for our use case)
            var poolSizes = new DescriptorPoolSize[]
            {
                new() { Type = DescriptorType.StorageBuffer, DescriptorCount = 64 }
            };

            fixed (DescriptorPoolSize* pPoolSizes = poolSizes)
            {
                var descPoolInfo = new DescriptorPoolCreateInfo
                {
                    SType = StructureType.DescriptorPoolCreateInfo,
                    MaxSets = 16,
                    PoolSizeCount = (uint)poolSizes.Length,
                    PPoolSizes = pPoolSizes,
                    Flags = DescriptorPoolCreateFlags.FreeDescriptorSetBit
                };

                if (vk.CreateDescriptorPool(device, &descPoolInfo, null, out descriptorPool) != Result.Success)
                {
                    vk.DestroyCommandPool(device, commandPool, null);
                    vk.DestroyDevice(device, null);
                    vk.DestroyInstance(instance, null);
                    return null;
                }
            }

            return new VulkanContext(
                vk, instance, selectedDevice, device, computeQueue,
                commandPool, descriptorPool, computeFamily, deviceName);
        }
        catch
        {
            // Vulkan not available at all (missing driver, etc.)
            if (vk != null)
            {
                if (descriptorPool.Handle != 0 && device.Handle != 0)
                    vk.DestroyDescriptorPool(device, descriptorPool, null);
                if (commandPool.Handle != 0 && device.Handle != 0)
                    vk.DestroyCommandPool(device, commandPool, null);
                if (device.Handle != 0)
                    vk.DestroyDevice(device, null);
                if (instance.Handle != 0)
                    vk.DestroyInstance(instance, null);
            }
            return null;
        }
    }

    /// <summary>
    /// Allocates a single command buffer from the pool.
    /// </summary>
    public CommandBuffer AllocateCommandBuffer()
    {
        var allocInfo = new CommandBufferAllocateInfo
        {
            SType = StructureType.CommandBufferAllocateInfo,
            CommandPool = _commandPool,
            Level = CommandBufferLevel.Primary,
            CommandBufferCount = 1
        };

        _vk.AllocateCommandBuffers(_device, &allocInfo, out var cmdBuffer);
        return cmdBuffer;
    }

    /// <summary>
    /// Submits a command buffer and waits for completion.
    /// </summary>
    public void SubmitAndWait(CommandBuffer cmdBuffer)
    {
        var submitInfo = new SubmitInfo
        {
            SType = StructureType.SubmitInfo,
            CommandBufferCount = 1,
            PCommandBuffers = &cmdBuffer
        };

        _vk.QueueSubmit(_computeQueue, 1, &submitInfo, default);
        _vk.QueueWaitIdle(_computeQueue);
    }

    /// <summary>
    /// Frees a command buffer back to the pool.
    /// </summary>
    public void FreeCommandBuffer(CommandBuffer cmdBuffer)
    {
        _vk.FreeCommandBuffers(_device, _commandPool, 1, &cmdBuffer);
    }

    /// <summary>
    /// Gets the memory type index matching the given requirements.
    /// </summary>
    public uint FindMemoryType(uint typeFilter, MemoryPropertyFlags properties)
    {
        _vk.GetPhysicalDeviceMemoryProperties(_physicalDevice, out var memProperties);

        for (uint i = 0; i < memProperties.MemoryTypeCount; i++)
        {
            if ((typeFilter & (1u << (int)i)) != 0 &&
                (memProperties.MemoryTypes[(int)i].PropertyFlags & properties) == properties)
            {
                return i;
            }
        }

        throw new InvalidOperationException("Failed to find suitable memory type.");
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _vk.DeviceWaitIdle(_device);
        _vk.DestroyDescriptorPool(_device, _descriptorPool, null);
        _vk.DestroyCommandPool(_device, _commandPool, null);
        _vk.DestroyDevice(_device, null);
        _vk.DestroyInstance(_instance, null);
        _vk.Dispose();
    }
}
