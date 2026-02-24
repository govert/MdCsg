using MdCsg.Classification;

namespace MdCsg.Gpu;

/// <summary>
/// Static factory for creating GPU-accelerated or fallback classification strategies.
/// </summary>
public static class GpuAccelerator
{
    /// <summary>
    /// Attempts to create a GPU-accelerated classifier.
    /// Returns null if Vulkan is not available.
    /// The caller owns the returned object and must dispose it when done.
    /// </summary>
    public static GpuPatchClassifier? TryCreate()
    {
        var ctx = VulkanContext.TryCreate();
        if (ctx == null) return null;

        try
        {
            return new GpuPatchClassifier(ctx);
        }
        catch
        {
            ctx.Dispose();
            return null;
        }
    }

    /// <summary>
    /// Creates a GPU classifier if available, otherwise returns the CPU fallback.
    /// </summary>
    public static IPatchClassificationStrategy CreateWithFallback()
    {
        return TryCreate() as IPatchClassificationStrategy
            ?? new CpuPatchClassificationStrategy();
    }
}
