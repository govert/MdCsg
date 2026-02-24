using MdCsg.Math;

namespace MdCsg.Mesh;

/// <summary>
/// A half-edge (DCEL) mesh data structure for topological queries.
/// </summary>
public class HalfEdgeMesh
{
    private readonly List<Vertex> _vertices = [];
    private readonly List<HalfEdge> _halfEdges = [];
    private readonly List<Face> _faces = [];

    /// <summary>All vertices in the mesh.</summary>
    public IReadOnlyList<Vertex> Vertices => _vertices;

    /// <summary>All half-edges in the mesh.</summary>
    public IReadOnlyList<HalfEdge> HalfEdges => _halfEdges;

    /// <summary>All faces in the mesh.</summary>
    public IReadOnlyList<Face> Faces => _faces;

    /// <summary>Adds a new vertex at the given position.</summary>
    public Vertex AddVertex(Vec3 position)
    {
        var v = new Vertex(_vertices.Count, position);
        _vertices.Add(v);
        return v;
    }

    /// <summary>Adds a new half-edge and returns it.</summary>
    public HalfEdge AddHalfEdge()
    {
        var he = new HalfEdge(_halfEdges.Count);
        _halfEdges.Add(he);
        return he;
    }

    /// <summary>
    /// Adds a triangular face from three vertices. Creates the three half-edges,
    /// links them in a cycle, and returns the face.
    /// </summary>
    public Face AddFace(Vertex v0, Vertex v1, Vertex v2)
    {
        var face = new Face(_faces.Count);
        _faces.Add(face);

        var he0 = AddHalfEdge();
        var he1 = AddHalfEdge();
        var he2 = AddHalfEdge();

        // Targets: each half-edge points to the next vertex in the triangle
        he0.Target = v1;
        he1.Target = v2;
        he2.Target = v0;

        // Face links
        he0.Face = face;
        he1.Face = face;
        he2.Face = face;

        // Next/Prev cycle
        he0.Next = he1; he1.Prev = he0;
        he1.Next = he2; he2.Prev = he1;
        he2.Next = he0; he0.Prev = he2;

        face.Edge = he0;

        // Set outgoing edges for vertices (if not already set)
        v0.OutgoingEdge ??= he0;
        v1.OutgoingEdge ??= he1;
        v2.OutgoingEdge ??= he2;

        return face;
    }

    /// <summary>
    /// Iterates all faces adjacent to a vertex (faces sharing this vertex).
    /// </summary>
    public IEnumerable<Face> FacesAroundVertex(Vertex v)
    {
        foreach (var face in _faces)
        {
            var start = face.Edge;
            var current = start;
            do
            {
                if (current.Origin == v || current.Target == v)
                {
                    yield return face;
                    break;
                }
                current = current.Next;
            } while (current != start);
        }
    }

    /// <summary>
    /// Returns the AABB of the entire mesh.
    /// </summary>
    public Aabb GetBounds()
    {
        if (_vertices.Count == 0)
            return Aabb.Empty;

        var bounds = Aabb.FromPoint(_vertices[0].Position);
        for (int i = 1; i < _vertices.Count; i++)
            bounds = bounds.ExpandToInclude(_vertices[i].Position);
        return bounds;
    }
}
