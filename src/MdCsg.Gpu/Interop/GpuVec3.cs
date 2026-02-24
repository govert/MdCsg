using System.Runtime.InteropServices;

namespace MdCsg.Gpu.Interop;

/// <summary>
/// GPU-compatible 3D vector (float32). Padded to 16 bytes for GLSL vec4 alignment in buffers.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 16)]
public struct GpuVec3
{
    /// <summary>X component.</summary>
    public float X;

    /// <summary>Y component.</summary>
    public float Y;

    /// <summary>Z component.</summary>
    public float Z;

    /// <summary>Padding for 16-byte alignment.</summary>
    public float Pad;

    /// <summary>
    /// Creates a GpuVec3 from double-precision components (truncated to float32).
    /// </summary>
    public GpuVec3(double x, double y, double z)
    {
        X = (float)x;
        Y = (float)y;
        Z = (float)z;
        Pad = 0;
    }
}
