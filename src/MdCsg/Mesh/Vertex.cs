using MdCsg.Math;

namespace MdCsg.Mesh;

/// <summary>
/// A vertex in the half-edge mesh.
/// </summary>
public class Vertex
{
    /// <summary>Unique vertex identifier.</summary>
    public int Id { get; }

    /// <summary>Position in 3D space.</summary>
    public Vec3 Position { get; set; }

    /// <summary>One outgoing half-edge from this vertex (arbitrary choice).</summary>
    public HalfEdge? OutgoingEdge { get; set; }

    /// <summary>
    /// Creates a new vertex.
    /// </summary>
    public Vertex(int id, Vec3 position)
    {
        Id = id;
        Position = position;
    }
}
