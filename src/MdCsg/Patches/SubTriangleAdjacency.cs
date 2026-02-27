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

        // Phase 1: Assign canonical vertex IDs using spatial hashing with neighbor-cell
        // lookup. This ensures two positions within tolerance always get the same ID,
        // even when they straddle a hash-grid cell boundary.
        var vertexMap = new Dictionary<long, List<(int Id, Vec3 Pos)>>();
        var toleranceInv = 1.0 / tolerance;
        var toleranceSq = tolerance * tolerance;
        int nextId = 0;

        int GetCanonicalId(Vec3 pos)
        {
            long gx = (long)System.Math.Round(pos.X * toleranceInv);
            long gy = (long)System.Math.Round(pos.Y * toleranceInv);
            long gz = (long)System.Math.Round(pos.Z * toleranceInv);

            // Check current cell and all 26 neighbors
            for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            for (int dz = -1; dz <= 1; dz++)
            {
                var key = HashGrid(gx + dx, gy + dy, gz + dz);
                if (vertexMap.TryGetValue(key, out var bucket))
                {
                    for (int k = 0; k < bucket.Count; k++)
                    {
                        if (Vec3.DistanceSquared(bucket[k].Pos, pos) < toleranceSq)
                            return bucket[k].Id;
                    }
                }
            }

            // Not found — assign new ID and store in primary cell
            var primaryKey = HashGrid(gx, gy, gz);
            if (!vertexMap.TryGetValue(primaryKey, out var primaryBucket))
            {
                primaryBucket = [];
                vertexMap[primaryKey] = primaryBucket;
            }
            int id = nextId++;
            primaryBucket.Add((id, pos));
            return id;
        }

        // Phase 2: Build edge map using canonical vertex IDs (not position hashes).
        // This guarantees that geometrically shared edges always produce the same key.
        var edgeMap = new Dictionary<(int, int), List<(int TriIndex, bool IsThisEdgeIntersection)>>();

        for (int i = 0; i < n; i++)
        {
            var tri = subTriangles[i];
            var va = tri.A;
            var vb = tri.B;
            var vc = tri.C;
            int idA = GetCanonicalId(va);
            int idB = GetCanonicalId(vb);
            int idC = GetCanonicalId(vc);
            var ids = new[] { idA, idB, idC };

            for (int e = 0; e < 3; e++)
            {
                int id1 = ids[e];
                int id2 = ids[(e + 1) % 3];

                // Canonical edge ordering
                var edgeKey = id1 < id2 ? (id1, id2) : (id2, id1);

                if (!edgeMap.TryGetValue(edgeKey, out var list))
                {
                    list = [];
                    edgeMap[edgeKey] = list;
                }
                list.Add((i, tri.IsEdgeIntersection(e)));
            }
        }

        // Phase 3: Connect triangles that share edges
        foreach (var kvp in edgeMap)
        {
            var tris = kvp.Value;
            for (int i = 0; i < tris.Count; i++)
            {
                for (int j = i + 1; j < tris.Count; j++)
                {
                    int a = tris[i].TriIndex;
                    int b = tris[j].TriIndex;
                    // The shared edge is an intersection edge if EITHER side says it is
                    bool isIntEdge = tris[i].IsThisEdgeIntersection || tris[j].IsThisEdgeIntersection;

                    adjacency[a].Add((b, isIntEdge));
                    adjacency[b].Add((a, isIntEdge));
                }
            }
        }

        return new SubTriangleAdjacency(adjacency);
    }

    private static long HashGrid(long gx, long gy, long gz)
    {
#if NET
        return HashCode.Combine(gx, gy, gz);
#else
        unchecked { return (gx * 397L ^ gy) * 397L ^ gz; }
#endif
    }
}
