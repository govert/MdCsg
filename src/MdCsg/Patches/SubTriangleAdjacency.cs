using MdCsg.Cutting;
using MdCsg.Math;

namespace MdCsg.Patches;

/// <summary>
/// Builds an adjacency graph for sub-triangles, tracking which edges are intersection edges.
/// </summary>
public class SubTriangleAdjacency
{
    private readonly List<List<(int Neighbor, bool IsIntersectionEdge)>> _adjacency;

    /// <summary>Number of sub-triangles.</summary>
    public int Count => _adjacency.Count;

    /// <summary>
    /// Gets the neighbors of a sub-triangle.
    /// </summary>
    public IReadOnlyList<(int Neighbor, bool IsIntersectionEdge)> GetNeighbors(int index) => _adjacency[index];

    private SubTriangleAdjacency(List<List<(int, bool)>> adjacency)
    {
        _adjacency = adjacency;
    }

    /// <summary>
    /// Builds the adjacency graph from a list of sub-triangles.
    /// Two sub-triangles are adjacent if they share an edge.
    /// The shared edge may or may not be an intersection edge.
    /// </summary>
    public static SubTriangleAdjacency Build(IReadOnlyList<FaceCutter.SubTriangle> subTriangles, double tolerance = 1e-8)
    {
        int n = subTriangles.Count;
        var adjacency = new List<List<(int, bool)>>(n);
        for (int i = 0; i < n; i++)
            adjacency.Add([]);

        // Build edge map: (snapped v1, snapped v2) → list of sub-triangle indices
        var edgeMap = new Dictionary<(long, long), List<(int TriIndex, bool IsIntersectionEdge)>>();
        var toleranceInv = 1.0 / tolerance;

        for (int i = 0; i < n; i++)
        {
            var tri = subTriangles[i];
            var verts = new[] { tri.A, tri.B, tri.C };

            for (int e = 0; e < 3; e++)
            {
                var v1 = verts[e];
                var v2 = verts[(e + 1) % 3];

                long h1 = HashVec3(v1, toleranceInv);
                long h2 = HashVec3(v2, toleranceInv);

                // Canonical edge ordering
                var edgeKey = h1 < h2 ? (h1, h2) : (h2, h1);

                if (!edgeMap.TryGetValue(edgeKey, out var list))
                {
                    list = [];
                    edgeMap[edgeKey] = list;
                }
                list.Add((i, tri.HasIntersectionEdge));
            }
        }

        // Connect triangles that share edges
        foreach (var (_, tris) in edgeMap)
        {
            for (int i = 0; i < tris.Count; i++)
            {
                for (int j = i + 1; j < tris.Count; j++)
                {
                    int a = tris[i].TriIndex;
                    int b = tris[j].TriIndex;
                    bool isIntEdge = tris[i].IsIntersectionEdge || tris[j].IsIntersectionEdge;

                    adjacency[a].Add((b, isIntEdge));
                    adjacency[b].Add((a, isIntEdge));
                }
            }
        }

        return new SubTriangleAdjacency(adjacency);
    }

    private static long HashVec3(Vec3 v, double invTol)
    {
        long x = (long)System.Math.Round(v.X * invTol);
        long y = (long)System.Math.Round(v.Y * invTol);
        long z = (long)System.Math.Round(v.Z * invTol);
        return HashCode.Combine(x, y, z);
    }
}
