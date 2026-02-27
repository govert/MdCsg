using MdCsg.Cutting;

namespace MdCsg.Patches;

/// <summary>
/// Extracts patches by flood-filling within each original face independently.
/// This prevents cross-face leakage through uncut faces that bridge the inside/outside
/// regions of the intersection curve. Within a single face, all sub-triangle vertices
/// come from the same FaceCutter vertex list, so adjacency uses exact bit-pattern matching.
/// </summary>
public static class IntraFacePatchExtractor
{
    /// <summary>
    /// Extracts patches from sub-triangles using intra-face flood fill.
    /// Each uncut face becomes one patch. Each cut face produces typically 2 patches
    /// (inside/outside of the intersection segment), separated by intersection edges.
    /// </summary>
    public static List<Patch> Extract(IReadOnlyList<FaceCutter.SubTriangle> subTriangles)
    {
        int n = subTriangles.Count;
        var patchOf = new int[n];
        for (int k = 0; k < n; k++) patchOf[k] = -1;

        // Group sub-triangles by original face
        var faceGroups = new Dictionary<int, List<int>>();
        for (int i = 0; i < n; i++)
        {
            int faceIdx = subTriangles[i].OriginalFaceIndex;
            if (!faceGroups.TryGetValue(faceIdx, out var list))
            {
                list = [];
                faceGroups[faceIdx] = list;
            }
            list.Add(i);
        }

        var patches = new List<Patch>();

        foreach (var kvp in faceGroups)
        {
            var indices = kvp.Value;

            if (indices.Count == 1)
            {
                // Single sub-triangle (uncut face) → one patch
                var patch = new Patch(patches.Count);
                patches.Add(patch);
                patch.SubTriangleIndices.Add(indices[0]);
                patchOf[indices[0]] = patch.Id;
                continue;
            }

            // Build local adjacency within this face's sub-triangles.
            // Vertices are bitwise-identical doubles from the same FaceCutter, so
            // we use exact long-bit keys for edge matching — no tolerance needed.
            var localAdj = BuildLocalAdjacency(subTriangles, indices);

            // Flood fill within this face, stopping at intersection edges
            for (int li = 0; li < indices.Count; li++)
            {
                int globalIdx = indices[li];
                if (patchOf[globalIdx] >= 0) continue;

                var patch = new Patch(patches.Count);
                patches.Add(patch);

                var queue = new Queue<int>();
                queue.Enqueue(li);
                patchOf[globalIdx] = patch.Id;
                patch.SubTriangleIndices.Add(globalIdx);

                while (queue.Count > 0)
                {
                    int current = queue.Dequeue();

                    foreach (var (neighbor, isIntersectionEdge) in localAdj[current])
                    {
                        int neighborGlobal = indices[neighbor];
                        if (patchOf[neighborGlobal] >= 0) continue;
                        if (isIntersectionEdge) continue;

                        patchOf[neighborGlobal] = patch.Id;
                        patch.SubTriangleIndices.Add(neighborGlobal);
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        return patches;
    }

    /// <summary>
    /// Builds a local adjacency graph for sub-triangles within a single original face.
    /// Uses exact bit-pattern matching on Vec3 components (no tolerance) since all
    /// vertices come from the same FaceCutter vertex list.
    /// </summary>
    private static List<(int Neighbor, bool IsIntersectionEdge)>[] BuildLocalAdjacency(
        IReadOnlyList<FaceCutter.SubTriangle> subTriangles,
        List<int> localIndices)
    {
        int count = localIndices.Count;
        var adj = new List<(int, bool)>[count];
        for (int i = 0; i < count; i++)
            adj[i] = [];

        // Edge map: (bit-pattern key) → list of (local index, edge index, is intersection edge)
        var edgeMap = new Dictionary<(long, long, long, long, long, long), List<(int LocalIdx, bool IsIntEdge)>>();

        for (int li = 0; li < count; li++)
        {
            var tri = subTriangles[localIndices[li]];

            for (int e = 0; e < 3; e++)
            {
                var v0 = e == 0 ? tri.A : e == 1 ? tri.B : tri.C;
                var v1 = e == 0 ? tri.B : e == 1 ? tri.C : tri.A;

                // Canonical edge key: use exact bit patterns, ordered
                var key0 = MakeKey(v0);
                var key1 = MakeKey(v1);
                var edgeKey = key0.CompareTo(key1) <= 0
                    ? (key0.Item1, key0.Item2, key0.Item3, key1.Item1, key1.Item2, key1.Item3)
                    : (key1.Item1, key1.Item2, key1.Item3, key0.Item1, key0.Item2, key0.Item3);

                if (!edgeMap.TryGetValue(edgeKey, out var list))
                {
                    list = [];
                    edgeMap[edgeKey] = list;
                }
                list.Add((li, tri.IsEdgeIntersection(e)));
            }
        }

        // Connect triangles sharing edges
        foreach (var kvp in edgeMap)
        {
            var tris = kvp.Value;
            for (int i = 0; i < tris.Count; i++)
            {
                for (int j = i + 1; j < tris.Count; j++)
                {
                    int a = tris[i].LocalIdx;
                    int b = tris[j].LocalIdx;
                    bool isIntEdge = tris[i].IsIntEdge || tris[j].IsIntEdge;

                    adj[a].Add((b, isIntEdge));
                    adj[b].Add((a, isIntEdge));
                }
            }
        }

        return adj;
    }

    private static (long, long, long) MakeKey(Math.Vec3 v)
    {
        return (
            BitConverter.DoubleToInt64Bits(v.X),
            BitConverter.DoubleToInt64Bits(v.Y),
            BitConverter.DoubleToInt64Bits(v.Z));
    }
}
