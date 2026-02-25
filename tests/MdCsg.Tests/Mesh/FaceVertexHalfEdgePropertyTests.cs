using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: Face, Vertex, HalfEdge — construction, geometric properties, DCEL invariants</summary>
public class FaceVertexHalfEdgePropertyTests
{
    [Fact]
    public void Vertex_Constructor_SetsIdAndPosition()
    {
        var v = new Vertex(5, new Vec3(1, 2, 3));
        Assert.Equal(5, v.Id);
        Assert.Equal(1.0, v.Position.X);
        Assert.Equal(2.0, v.Position.Y);
        Assert.Equal(3.0, v.Position.Z);
    }

    [Fact]
    public void Vertex_OutgoingEdge_DefaultNull()
    {
        var v = new Vertex(0, Vec3.Zero);
        Assert.Null(v.OutgoingEdge);
    }

    [Fact]
    public void HalfEdge_Constructor_SetsId()
    {
        var he = new HalfEdge(7);
        Assert.Equal(7, he.Id);
    }

    [Fact]
    public void HalfEdge_Twin_DefaultNull()
    {
        var he = new HalfEdge(0);
        Assert.Null(he.Twin);
    }

    [Fact]
    public void HalfEdge_IsIntersectionEdge_DefaultFalse()
    {
        var he = new HalfEdge(0);
        Assert.False(he.IsIntersectionEdge);
    }

    [Fact]
    public void HalfEdge_Origin_FromPrev()
    {
        var v0 = new Vertex(0, new Vec3(0, 0, 0));
        var v1 = new Vertex(1, new Vec3(1, 0, 0));
        var he0 = new HalfEdge(0) { Target = v1 };
        var he1 = new HalfEdge(1) { Target = v0, Next = he0 };
        he0.Prev = he1;
        // Origin of he0 = Prev.Target = he1.Target = v0
        Assert.Equal(v0, he0.Origin);
    }

    [Fact]
    public void HalfEdge_Origin_FromTwin_WhenNoPrev()
    {
        var v0 = new Vertex(0, new Vec3(0, 0, 0));
        var v1 = new Vertex(1, new Vec3(1, 0, 0));
        var he0 = new HalfEdge(0) { Target = v1 };
        var heTwin = new HalfEdge(1) { Target = v0 };
        he0.Twin = heTwin;
        // Origin of he0 = Twin.Target = v0
        Assert.Equal(v0, he0.Origin);
    }

    [Fact]
    public void Face_Constructor_SetsIdAndOriginalFaceId()
    {
        var face = new Face(3);
        Assert.Equal(3, face.Id);
        Assert.Equal(3, face.OriginalFaceId);
    }

    [Fact]
    public void Face_PatchId_DefaultNegativeOne()
    {
        var face = new Face(0);
        Assert.Equal(-1, face.PatchId);
    }

    [Fact]
    public void Face_GetVertices_ReturnsThreeForTriangle()
    {
        var mesh = BuildTriangleMesh(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var verts = mesh.Faces[0].GetVertices();
        Assert.Equal(3, verts.Count);
    }

    [Fact]
    public void Face_Centroid_IsAverageOfVertices()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(3, 0, 0);
        var c = new Vec3(0, 3, 0);
        var mesh = BuildTriangleMesh(a, b, c);
        var centroid = mesh.Faces[0].Centroid;
        Assert.True(System.Math.Abs(centroid.X - 1.0) < 1e-10);
        Assert.True(System.Math.Abs(centroid.Y - 1.0) < 1e-10);
        Assert.True(System.Math.Abs(centroid.Z) < 1e-10);
    }

    [Fact]
    public void Face_Normal_NonZero()
    {
        var mesh = BuildTriangleMesh(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var normal = mesh.Faces[0].Normal;
        Assert.True(normal.LengthSquared > 1e-20);
    }

    [Fact]
    public void Face_Normal_PointsInZDirection_ForXYTriangle()
    {
        var mesh = BuildTriangleMesh(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var normal = mesh.Faces[0].Normal;
        // Cross(b-a, c-a) = Cross((1,0,0), (0,1,0)) = (0,0,1)
        Assert.True(System.Math.Abs(normal.X) < 1e-10);
        Assert.True(System.Math.Abs(normal.Y) < 1e-10);
        Assert.True(normal.Z > 0);
    }

    [Fact]
    public void Face_UnitNormal_HasLength1()
    {
        var mesh = BuildTriangleMesh(new Vec3(0, 0, 0), new Vec3(5, 0, 0), new Vec3(0, 5, 0));
        var unitN = mesh.Faces[0].UnitNormal;
        Assert.True(System.Math.Abs(unitN.Length - 1.0) < 1e-10);
    }

    [Fact]
    public void Face_GetTrianglePositions_MatchesVertices()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var c = new Vec3(7, 8, 9);
        var mesh = BuildTriangleMesh(a, b, c);
        mesh.Faces[0].GetTrianglePositions(out var pa, out var pb, out var pc);
        var verts = mesh.Faces[0].GetVertices();
        Assert.Equal(verts[0].Position, pa);
        Assert.Equal(verts[1].Position, pb);
        Assert.Equal(verts[2].Position, pc);
    }

    [Fact]
    public void Face_OriginalFaceId_CanBeChanged()
    {
        var face = new Face(0);
        face.OriginalFaceId = 99;
        Assert.Equal(99, face.OriginalFaceId);
    }

    [Fact]
    public void Face_PatchId_CanBeSet()
    {
        var face = new Face(0);
        face.PatchId = 5;
        Assert.Equal(5, face.PatchId);
    }

    [Fact]
    public void HalfEdge_IsIntersectionEdge_CanBeSet()
    {
        var he = new HalfEdge(0);
        he.IsIntersectionEdge = true;
        Assert.True(he.IsIntersectionEdge);
    }

    [Fact]
    public void Vertex_Position_CanBeUpdated()
    {
        var v = new Vertex(0, Vec3.Zero);
        v.Position = new Vec3(10, 20, 30);
        Assert.Equal(10.0, v.Position.X);
        Assert.Equal(20.0, v.Position.Y);
        Assert.Equal(30.0, v.Position.Z);
    }

    [Fact]
    public void Face_Centroid_EquilateralTriangle()
    {
        double h = System.Math.Sqrt(3.0) / 2.0;
        var mesh = BuildTriangleMesh(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0.5, h, 0));
        var centroid = mesh.Faces[0].Centroid;
        Assert.True(System.Math.Abs(centroid.X - 0.5) < 1e-10);
        Assert.True(System.Math.Abs(centroid.Y - h / 3.0) < 1e-10);
    }

    [Fact]
    public void HalfEdge_Next_FormsCycle()
    {
        var mesh = BuildTriangleMesh(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var start = mesh.Faces[0].Edge;
        var current = start;
        int count = 0;
        do
        {
            current = current.Next;
            count++;
        } while (current != start && count < 100);
        Assert.Equal(3, count);
    }

    [Fact]
    public void HalfEdge_Prev_FormsCycle()
    {
        var mesh = BuildTriangleMesh(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var start = mesh.Faces[0].Edge;
        var current = start;
        int count = 0;
        do
        {
            current = current.Prev;
            count++;
        } while (current != start && count < 100);
        Assert.Equal(3, count);
    }

    private static HalfEdgeMesh BuildTriangleMesh(Vec3 a, Vec3 b, Vec3 c)
    {
        var builder = new MeshBuilder();
        return builder.Build(new[] { new Triangle3(a, b, c) });
    }
}
