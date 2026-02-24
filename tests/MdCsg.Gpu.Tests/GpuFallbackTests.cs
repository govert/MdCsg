using MdCsg.Classification;
using MdCsg.Gpu;
using Xunit;

namespace MdCsg.Gpu.Tests;

public class GpuFallbackTests
{
    [Fact]
    public void TryCreate_DoesNotThrow()
    {
        // Should never throw, even if Vulkan is not available
        var classifier = GpuAccelerator.TryCreate();
        classifier?.Dispose();
    }

    [Fact]
    public void CreateWithFallback_AlwaysReturnsStrategy()
    {
        var strategy = GpuAccelerator.CreateWithFallback();
        Assert.NotNull(strategy);

        // Should be either GPU or CPU strategy
        Assert.True(
            strategy is GpuPatchClassifier || strategy is CpuPatchClassificationStrategy);

        (strategy as IDisposable)?.Dispose();
    }

    [Fact]
    public void CpuFallback_WorksWhenGpuUnavailable()
    {
        // CreateWithFallback should always work — even without a GPU
        var strategy = GpuAccelerator.CreateWithFallback();
        Assert.NotNull(strategy);
        (strategy as IDisposable)?.Dispose();
    }
}
