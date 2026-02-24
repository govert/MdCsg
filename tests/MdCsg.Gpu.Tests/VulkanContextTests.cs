using MdCsg.Gpu;
using Xunit;

namespace MdCsg.Gpu.Tests;

public class VulkanContextTests
{
    [Fact]
    public void TryCreate_ReturnsContextOrNull()
    {
        // Should not throw — returns null gracefully if Vulkan unavailable
        using var ctx = VulkanContext.TryCreate();

        if (ctx != null)
        {
            Assert.True(ctx.IsAvailable);
            Assert.NotEmpty(ctx.DeviceName);
        }
        // If null, Vulkan is simply not available — that's OK
    }

    [Fact]
    public void Dispose_IsIdempotent()
    {
        var ctx = VulkanContext.TryCreate();
        if (ctx == null) return; // Skip if no Vulkan

        ctx.Dispose();
        ctx.Dispose(); // Should not throw
    }

    [Fact]
    public void DeviceName_IsPopulated()
    {
        using var ctx = VulkanContext.TryCreate();
        if (ctx == null) return;

        Assert.NotNull(ctx.DeviceName);
        Assert.NotEqual("Unknown", ctx.DeviceName);
    }
}
