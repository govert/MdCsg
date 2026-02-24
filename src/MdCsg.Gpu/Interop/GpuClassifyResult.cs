using System.Runtime.InteropServices;

namespace MdCsg.Gpu.Interop;

/// <summary>
/// Per-patch classification result returned from the GPU.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct GpuClassifyResult
{
    /// <summary>1 = inside, 0 = outside.</summary>
    public int IsInside;

    /// <summary>The confident point margin as int-reinterpreted float bits (from GPU floatBitsToInt).</summary>
    public int MarginBits;
}
