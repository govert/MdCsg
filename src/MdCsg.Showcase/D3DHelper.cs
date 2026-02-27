using System;
using System.Runtime.InteropServices;
using TerraFX.Interop.Windows;

namespace MdCsg.Showcase;

[StructLayout(LayoutKind.Sequential)]
internal struct GpuVertex
{
    public float PX, PY, PZ;
    public float NX, NY, NZ;
}

internal static class D3DHelper
{
    public static void ThrowIfFailed(HRESULT hr)
    {
        if (hr.Value < 0)
            throw new Exception($"D3D11 error: HRESULT 0x{(uint)hr.Value:X8}");
    }
}
