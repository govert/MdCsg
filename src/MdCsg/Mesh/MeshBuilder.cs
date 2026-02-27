using MdCsg.Math;

namespace MdCsg.Mesh;

/// <summary>
/// Builds a <see cref="HalfEdgeMesh"/> from triangle soup (vertex arrays + index arrays).
/// Handles vertex welding and twin linking.
/// </summary>
public class MeshBuilder
{
    private readonly double _weldTolerance;

    /// <summary>
    /// Creates a mesh builder with the given vertex welding tolerance.
    /// </summary>
    /// <param name="weldTolerance">Distance below which vertices are merged.</param>
    public MeshBuilder(double weldTolerance = 1e-10)
    {
        _weldTolerance = weldTolerance;
    }

    /// <summary>
    /// Builds a half-edge mesh from a list of triangles (three Vec3 vertices each).
    /// </summary>
    public HalfEdgeMesh Build(IReadOnlyList<Triangle3> triangles)
    {
        var mesh = new HalfEdgeMesh();
        var vertexMap = new Dictionary<long, List<Vertex>>();
        var weldTolSq = _weldTolerance * _weldTolerance;
        var invTol = 1.0 / _weldTolerance;

        (long gx, long gy, long gz) GridCoords(Vec3 pos)
        {
            return (
                (long)System.Math.Round(pos.X * invTol),
                (long)System.Math.Round(pos.Y * invTol),
                (long)System.Math.Round(pos.Z * invTol));
        }

        long HashGrid(long gx, long gy, long gz)
        {
#if NET
            return HashCode.Combine(gx, gy, gz);
#else
            unchecked { return (gx * 397L ^ gy) * 397L ^ gz; }
#endif
        }

        Vertex GetOrAddVertex(Vec3 pos)
        {
            var (gx, gy, gz) = GridCoords(pos);

            // Check current cell and all 26 neighbors to handle cell-boundary cases.
            // Two points within weldTolerance can map to adjacent grid cells when they
            // straddle a cell boundary, so we must check all 3^3 = 27 neighboring cells.
            for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            for (int dz = -1; dz <= 1; dz++)
            {
                var key = HashGrid(gx + dx, gy + dy, gz + dz);
                if (vertexMap.TryGetValue(key, out var bucket))
                {
                    for (int i = 0; i < bucket.Count; i++)
                    {
                        if (Vec3.DistanceSquared(bucket[i].Position, pos) < weldTolSq)
                            return bucket[i];
                    }
                }
            }

            // Not found — add to primary cell
            var primaryKey = HashGrid(gx, gy, gz);
            if (!vertexMap.TryGetValue(primaryKey, out var primaryBucket))
            {
                primaryBucket = [];
                vertexMap[primaryKey] = primaryBucket;
            }
            var v = mesh.AddVertex(pos);
            primaryBucket.Add(v);
            return v;
        }

        // Create all faces
        foreach (var tri in triangles)
        {
            var v0 = GetOrAddVertex(tri.A);
            var v1 = GetOrAddVertex(tri.B);
            var v2 = GetOrAddVertex(tri.C);
            mesh.AddFace(v0, v1, v2);
        }

        // Link twins: build edge map (origin→target) → half-edge
        LinkTwins(mesh);

        return mesh;
    }

    /// <summary>
    /// Builds a half-edge mesh from vertex positions and triangle indices.
    /// </summary>
    public HalfEdgeMesh Build(IReadOnlyList<Vec3> positions, IReadOnlyList<(int I0, int I1, int I2)> triangleIndices)
    {
        var mesh = new HalfEdgeMesh();

        // Add all vertices
        var verts = new Vertex[positions.Count];
        for (int i = 0; i < positions.Count; i++)
            verts[i] = mesh.AddVertex(positions[i]);

        // Add all faces
        foreach (var (i0, i1, i2) in triangleIndices)
            mesh.AddFace(verts[i0], verts[i1], verts[i2]);

        LinkTwins(mesh);
        return mesh;
    }

    private static void LinkTwins(HalfEdgeMesh mesh)
    {
        // Map from (originId, targetId) → list of half-edges.
        // Using a list handles non-manifold edges where multiple half-edges share
        // the same directed edge key. A single-valued dictionary would overwrite
        // earlier entries, leaving them permanently orphaned (unable to find twins).
        var edgeMap = new Dictionary<(int, int), List<HalfEdge>>();

        foreach (var he in mesh.HalfEdges)
        {
            var key = (he.Origin.Id, he.Target.Id);
            if (!edgeMap.TryGetValue(key, out var list))
            {
                list = new List<HalfEdge>(1);
                edgeMap[key] = list;
            }
            list.Add(he);
        }

        foreach (var he in mesh.HalfEdges)
        {
            if (he.Twin != null) continue;
            var twinKey = (he.Target.Id, he.Origin.Id);
            if (edgeMap.TryGetValue(twinKey, out var candidates))
            {
                for (int i = 0; i < candidates.Count; i++)
                {
                    var twin = candidates[i];
                    if (twin.Twin == null && twin != he)
                    {
                        he.Twin = twin;
                        twin.Twin = he;
                        break;
                    }
                }
            }
        }
    }

}
