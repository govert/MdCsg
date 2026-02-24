using MdCsg.Math;

namespace MdCsg.Mesh;

/// <summary>
/// A face in the half-edge mesh (triangle).
/// </summary>
public class Face
{
    /// <summary>Unique face identifier.</summary>
    public int Id { get; }

    /// <summary>One half-edge on the face boundary.</summary>
    public HalfEdge Edge { get; set; } = null!;

    /// <summary>The original face ID before cutting (for tracking provenance).</summary>
    public int OriginalFaceId { get; set; } = -1;

    /// <summary>The patch this face belongs to (-1 = unassigned).</summary>
    public int PatchId { get; set; } = -1;

    /// <summary>
    /// Creates a new face.
    /// </summary>
    public Face(int id)
    {
        Id = id;
        OriginalFaceId = id;
    }

    /// <summary>Returns the vertices of this face in order.</summary>
    public IReadOnlyList<Vertex> GetVertices()
    {
        var result = new List<Vertex>();
        var start = Edge;
        var current = start;
        do
        {
            result.Add(current.Target);
            current = current.Next;
        } while (current != start);
        return result;
    }

    /// <summary>Gets the three triangle vertex positions without allocating.</summary>
    public void GetTrianglePositions(out Vec3 a, out Vec3 b, out Vec3 c)
    {
        var e0 = Edge;
        a = e0.Target.Position;
        var e1 = e0.Next;
        b = e1.Target.Position;
        c = e1.Next.Target.Position;
    }

    /// <summary>Returns the centroid of this face.</summary>
    public Vec3 Centroid
    {
        get
        {
            GetTrianglePositions(out var a, out var b, out var c);
            return (a + b + c) / 3.0;
        }
    }

    /// <summary>Returns the normal of this face (assumes triangle).</summary>
    public Vec3 Normal
    {
        get
        {
            GetTrianglePositions(out var a, out var b, out var c);
            return Vec3.Cross(b - a, c - a);
        }
    }

    /// <summary>Returns the unit normal of this face.</summary>
    public Vec3 UnitNormal
    {
        get
        {
            var n = Normal;
            var len = n.Length;
            return len > 0 ? n / len : Vec3.Zero;
        }
    }
}
