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
        var vertexMap = new Dictionary<long, Vertex>();

        Vertex GetOrAddVertex(Vec3 pos)
        {
            var key = HashPosition(pos);
            if (vertexMap.TryGetValue(key, out var existing) &&
                Vec3.DistanceSquared(existing.Position, pos) < _weldTolerance * _weldTolerance)
            {
                return existing;
            }
            var v = mesh.AddVertex(pos);
            vertexMap[key] = v;
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
        // Map from (originId, targetId) → half-edge
        var edgeMap = new Dictionary<(int, int), HalfEdge>();

        foreach (var he in mesh.HalfEdges)
        {
            var originId = he.Origin.Id;
            var targetId = he.Target.Id;
            var key = (originId, targetId);
            edgeMap[key] = he;
        }

        foreach (var he in mesh.HalfEdges)
        {
            if (he.Twin != null) continue;
            var twinKey = (he.Target.Id, he.Origin.Id);
            if (edgeMap.TryGetValue(twinKey, out var twin))
            {
                he.Twin = twin;
                twin.Twin = he;
            }
        }
    }

    private long HashPosition(Vec3 pos)
    {
        // Snap to grid and hash
        var invTol = 1.0 / _weldTolerance;
        long x = (long)System.Math.Round(pos.X * invTol);
        long y = (long)System.Math.Round(pos.Y * invTol);
        long z = (long)System.Math.Round(pos.Z * invTol);
        return HashCode.Combine(x, y, z);
    }
}
