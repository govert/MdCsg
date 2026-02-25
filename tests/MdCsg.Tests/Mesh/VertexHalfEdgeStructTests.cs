using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: Vertex, HalfEdge — struct properties, origin/target, face links</summary>
public class VertexHalfEdgeStructTests
{
    [Fact]
    public void Vertex_IdIsSequential()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        Assert.Equal(0, v0.Id);
        Assert.Equal(1, v1.Id);
        Assert.Equal(2, v2.Id);
    }

    [Fact]
    public void Vertex_PositionStored()
    {
        var mesh = new HalfEdgeMesh();
        var pos = new Vec3(3.14, 2.71, 1.41);
        var v = mesh.AddVertex(pos);
        Assert.Equal(pos, v.Position);
    }

    [Fact]
    public void HalfEdge_IdIsSequential()
    {
        var mesh = new HalfEdgeMesh();
        var he0 = mesh.AddHalfEdge();
        var he1 = mesh.AddHalfEdge();
        Assert.Equal(0, he0.Id);
        Assert.Equal(1, he1.Id);
    }

    [Fact]
    public void HalfEdge_Target_SetByAddFace()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        var face = mesh.AddFace(v0, v1, v2);

        var he = face.Edge;
        Assert.Equal(v1, he.Target);
        Assert.Equal(v2, he.Next.Target);
        Assert.Equal(v0, he.Next.Next.Target);
    }

    [Fact]
    public void HalfEdge_Origin_IsDerived()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        mesh.AddFace(v0, v1, v2);

        // Origin of a half-edge is Prev.Target
        foreach (var he in mesh.HalfEdges)
        {
            Assert.Equal(he.Prev.Target, he.Origin);
        }
    }

    [Fact]
    public void HalfEdge_Face_MatchesAddedFace()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        var face = mesh.AddFace(v0, v1, v2);

        Assert.Equal(face, face.Edge.Face);
        Assert.Equal(face, face.Edge.Next.Face);
        Assert.Equal(face, face.Edge.Next.Next.Face);
    }

    [Fact]
    public void HalfEdge_Twin_InitiallyNull()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        mesh.AddFace(v0, v1, v2);

        // Single face → no twins linked yet (MeshBuilder links twins)
        foreach (var he in mesh.HalfEdges)
            Assert.Null(he.Twin);
    }

    [Fact]
    public void HalfEdge_IsIntersectionEdge_DefaultFalse()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        mesh.AddFace(v0, v1, v2);

        foreach (var he in mesh.HalfEdges)
            Assert.False(he.IsIntersectionEdge);
    }

    [Fact]
    public void Vertex_OutgoingEdge_SetByFirstFace()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        mesh.AddFace(v0, v1, v2);

        Assert.NotNull(v0.OutgoingEdge);
        Assert.NotNull(v1.OutgoingEdge);
        Assert.NotNull(v2.OutgoingEdge);
    }

    [Fact]
    public void AddFace_Creates3HalfEdges()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        mesh.AddFace(v0, v1, v2);
        Assert.Equal(3, mesh.HalfEdges.Count);
    }

    [Fact]
    public void FaceCycle_Length3()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        var face = mesh.AddFace(v0, v1, v2);

        var start = face.Edge;
        var current = start;
        int count = 0;
        do { count++; current = current.Next; } while (current != start);
        Assert.Equal(3, count);
    }

    [Fact]
    public void Face_Id_Sequential()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        var v3 = mesh.AddVertex(new Vec3(1, 1, 0));
        var f0 = mesh.AddFace(v0, v1, v2);
        var f1 = mesh.AddFace(v1, v3, v2);
        Assert.Equal(0, f0.Id);
        Assert.Equal(1, f1.Id);
    }
}
