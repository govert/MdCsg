using MdCsg.Math;

namespace MdCsg.Bvh;

/// <summary>
/// Dual-tree BVH traversal for finding all overlapping face pairs between two meshes.
/// </summary>
public static class BvhTraversal
{
    /// <summary>
    /// Finds all pairs of overlapping faces between two BVH trees.
    /// Returns a list of (faceIndexA, faceIndexB) pairs.
    /// </summary>
    public static List<(int FaceA, int FaceB)> FindOverlappingPairs(BvhTree treeA, BvhTree treeB)
    {
        var pairs = new List<(int, int)>();
        if (treeA.NodeCount == 0 || treeB.NodeCount == 0)
            return pairs;

        TraverseRecursive(treeA, treeB, 0, 0, pairs);
        return pairs;
    }

    private static void TraverseRecursive(
        BvhTree treeA, BvhTree treeB,
        int nodeA, int nodeB,
        List<(int, int)> pairs)
    {
        var nodesA = treeA.Nodes;
        var nodesB = treeB.Nodes;

        if (!nodesA[nodeA].Bounds.Overlaps(nodesB[nodeB].Bounds))
            return;

        bool leafA = nodesA[nodeA].IsLeaf;
        bool leafB = nodesB[nodeB].IsLeaf;

        if (leafA && leafB)
        {
            // Both leaves: emit all pairs
            for (int i = 0; i < nodesA[nodeA].PrimitiveCount; i++)
            {
                int faceA = treeA.GetFaceIndex(nodesA[nodeA].LeftOrStart + i);
                for (int j = 0; j < nodesB[nodeB].PrimitiveCount; j++)
                {
                    int faceB = treeB.GetFaceIndex(nodesB[nodeB].LeftOrStart + j);
                    // Check face-level AABB overlap
                    var boundsA = GetFaceBounds(treeA, faceA);
                    var boundsB = GetFaceBounds(treeB, faceB);
                    if (boundsA.Overlaps(boundsB))
                        pairs.Add((faceA, faceB));
                }
            }
        }
        else if (leafA || (!leafB && nodesB[nodeB].Bounds.SurfaceArea > nodesA[nodeA].Bounds.SurfaceArea))
        {
            // Descend into B
            TraverseRecursive(treeA, treeB, nodeA, nodesB[nodeB].LeftOrStart, pairs);
            TraverseRecursive(treeA, treeB, nodeA, nodesB[nodeB].Right, pairs);
        }
        else
        {
            // Descend into A
            TraverseRecursive(treeA, treeB, nodesA[nodeA].LeftOrStart, nodeB, pairs);
            TraverseRecursive(treeA, treeB, nodesA[nodeA].Right, nodeB, pairs);
        }
    }

    private static Aabb GetFaceBounds(BvhTree tree, int faceIndex)
    {
        var face = tree.Mesh.Faces[faceIndex];
        face.GetTrianglePositions(out var a, out var b, out var c);
        return Aabb.FromTriangle(a, b, c);
    }
}
