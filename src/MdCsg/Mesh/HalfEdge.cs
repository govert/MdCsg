namespace MdCsg.Mesh;

/// <summary>
/// A half-edge in the DCEL (doubly-connected edge list) mesh.
/// </summary>
public class HalfEdge
{
    /// <summary>Unique half-edge identifier.</summary>
    public int Id { get; }

    /// <summary>The vertex this half-edge points to (target).</summary>
    public Vertex Target { get; set; } = null!;

    /// <summary>The twin (opposite) half-edge.</summary>
    public HalfEdge? Twin { get; set; }

    /// <summary>The next half-edge around the face (CCW).</summary>
    public HalfEdge Next { get; set; } = null!;

    /// <summary>The previous half-edge around the face (CCW).</summary>
    public HalfEdge Prev { get; set; } = null!;

    /// <summary>The face to the left of this half-edge.</summary>
    public Face? Face { get; set; }

    /// <summary>Whether this edge lies on an intersection curve.</summary>
    public bool IsIntersectionEdge { get; set; }

    /// <summary>
    /// Creates a new half-edge.
    /// </summary>
    public HalfEdge(int id)
    {
        Id = id;
    }

    /// <summary>The origin vertex (derived from Prev.Target or Twin.Target).</summary>
    public Vertex Origin => Prev?.Target ?? Twin?.Target ?? throw new InvalidOperationException("Cannot determine origin: neither Prev nor Twin is set.");
}
