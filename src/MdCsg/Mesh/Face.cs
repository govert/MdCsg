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

    /// <summary>Returns the centroid of this face.</summary>
    public Vec3 Centroid
    {
        get
        {
            var vertices = GetVertices();
            var sum = Vec3.Zero;
            foreach (var v in vertices)
                sum = sum + v.Position;
            return sum / vertices.Count;
        }
    }

    /// <summary>Returns the normal of this face (assumes triangle).</summary>
    public Vec3 Normal
    {
        get
        {
            var verts = GetVertices();
            if (verts.Count < 3) return Vec3.Zero;
            return Vec3.Cross(verts[1].Position - verts[0].Position, verts[2].Position - verts[0].Position);
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
