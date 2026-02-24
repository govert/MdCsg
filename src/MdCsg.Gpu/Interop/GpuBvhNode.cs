using System.Runtime.InteropServices;

namespace MdCsg.Gpu.Interop;

/// <summary>
/// GPU-compatible BVH node. 48 bytes = 3 x vec4, matching GLSL std430 layout.
/// Layout:
///   vec4(boundsMinX, boundsMinY, boundsMinZ, boundsMaxX)   [0..15]
///   vec4(boundsMaxY, boundsMaxZ, leftOrStart, right)        [16..31]  (last two as floatBitsToInt)
///   vec4(primitiveCount, pad, pad, pad)                     [32..47]  (first as floatBitsToInt)
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 48)]
public struct GpuBvhNode
{
    // First vec4
    /// <summary>AABB min X.</summary>
    public float BoundsMinX;
    /// <summary>AABB min Y.</summary>
    public float BoundsMinY;
    /// <summary>AABB min Z.</summary>
    public float BoundsMinZ;
    /// <summary>AABB max X.</summary>
    public float BoundsMaxX;

    // Second vec4
    /// <summary>AABB max Y.</summary>
    public float BoundsMaxY;
    /// <summary>AABB max Z.</summary>
    public float BoundsMaxZ;
    /// <summary>Left child index (interior) or start index (leaf). Stored as int bits in float.</summary>
    public int LeftOrStart;
    /// <summary>Right child index (interior only). Stored as int bits in float.</summary>
    public int Right;

    // Third vec4
    /// <summary>Primitive count (0 = interior, >0 = leaf).</summary>
    public int PrimitiveCount;
    /// <summary>Padding.</summary>
    public int Pad0;
    /// <summary>Padding.</summary>
    public int Pad1;
    /// <summary>Padding.</summary>
    public int Pad2;
}
