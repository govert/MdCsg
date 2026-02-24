using MdCsg.Cutting;

namespace MdCsg.Patches;

/// <summary>
/// Extracts patches from sub-triangles by flood-filling, stopping at intersection edges.
/// </summary>
public static class PatchExtractor
{
    /// <summary>
    /// Extracts patches from a list of sub-triangles.
    /// Each patch is a maximal connected set of sub-triangles reachable without
    /// crossing an intersection edge.
    /// </summary>
    public static List<Patch> Extract(
        IReadOnlyList<FaceCutter.SubTriangle> subTriangles,
        SubTriangleAdjacency adjacency)
    {
        int n = subTriangles.Count;
        var patchOf = new int[n];
        Array.Fill(patchOf, -1);

        var patches = new List<Patch>();

        for (int i = 0; i < n; i++)
        {
            if (patchOf[i] >= 0) continue;

            var patch = new Patch(patches.Count);
            patches.Add(patch);

            // BFS flood-fill
            var queue = new Queue<int>();
            queue.Enqueue(i);
            patchOf[i] = patch.Id;
            patch.SubTriangleIndices.Add(i);

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();

                foreach (var (neighbor, isIntersectionEdge) in adjacency.GetNeighbors(current))
                {
                    if (patchOf[neighbor] >= 0) continue;

                    // Stop at intersection edges
                    if (isIntersectionEdge) continue;

                    patchOf[neighbor] = patch.Id;
                    patch.SubTriangleIndices.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        return patches;
    }
}
