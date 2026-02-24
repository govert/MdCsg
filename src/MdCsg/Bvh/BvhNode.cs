using MdCsg.Math;

namespace MdCsg.Bvh;

/// <summary>
/// A node in a flattened BVH tree.
/// Interior nodes store left and right child indices.
/// Leaf nodes store a start index and count into the primitives array.
/// </summary>
public struct BvhNode
{
    /// <summary>Bounding box of this node.</summary>
    public Aabb Bounds;

    /// <summary>
    /// For interior nodes: index of the left child.
    /// For leaf nodes: start index in the primitives array.
    /// </summary>
    public int LeftOrStart;

    /// <summary>
    /// For interior nodes: index of the right child.
    /// For leaf nodes: unused.
    /// </summary>
    public int Right;

    /// <summary>Number of primitives (0 = interior, >0 = leaf).</summary>
    public int PrimitiveCount;

    /// <summary>Whether this is a leaf node.</summary>
    public readonly bool IsLeaf => PrimitiveCount > 0;
}
