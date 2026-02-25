using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: HalfEdgeMesh direct API — AddVertex, AddFace, AddHalfEdge, FacesAroundVertex</summary>
public class HalfEdgeMeshDirectApiTests
{
    [Fact]
    public void AddVertex_ReturnsVertexWithCorrectId()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(new Vec3(1, 2, 3));
        Assert.Equal(0, v0.Id);
        Assert.Equal(new Vec3(1, 2, 3), v0.Position);
    }

    [Fact]
    public void AddVertex_SequentialIds()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        Assert.Equal(0, v0.Id);
        Assert.Equal(1, v1.Id);
        Assert.Equal(2, v2.Id);
        Assert.Equal(3, mesh.Vertices.Count);
    }

    [Fact]
    public void AddHalfEdge_ReturnsWithSequentialIds()
    {
        var mesh = new HalfEdgeMesh();
        var he0 = mesh.AddHalfEdge();
        var he1 = mesh.AddHalfEdge();
        Assert.Equal(0, he0.Id);
        Assert.Equal(1, he1.Id);
    }

    [Fact]
    public void AddFace_CreatesThreeHalfEdges()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));

        var face = mesh.AddFace(v0, v1, v2);
        Assert.Equal(3, mesh.HalfEdges.Count);
        Assert.Equal(1, mesh.Faces.Count);
        Assert.Equal(0, face.Id);
    }

    [Fact]
    public void AddFace_HalfEdges_FormCycle()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));

        var face = mesh.AddFace(v0, v1, v2);
        var he = face.Edge;
        Assert.Same(he, he.Next.Next.Next);
        Assert.Same(he, he.Prev.Prev.Prev);
    }

    [Fact]
    public void AddFace_HalfEdges_PointToFace()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));

        var face = mesh.AddFace(v0, v1, v2);
        var he = face.Edge;
        Assert.Same(face, he.Face);
        Assert.Same(face, he.Next.Face);
        Assert.Same(face, he.Next.Next.Face);
    }

    [Fact]
    public void AddFace_SetsOutgoingEdgesOnVertices()
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
    public void AddFace_Targets_CorrectVertices()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));

        var face = mesh.AddFace(v0, v1, v2);
        var he0 = face.Edge;
        // AddFace(v0, v1, v2): he0→v1, he1→v2, he2→v0
        Assert.Same(v1, he0.Target);
        Assert.Same(v2, he0.Next.Target);
        Assert.Same(v0, he0.Next.Next.Target);
    }

    [Fact]
    public void TwoFaces_SameVertices_NoTwinsYet()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        var v3 = mesh.AddVertex(new Vec3(0, 0, 1));

        mesh.AddFace(v0, v1, v2);
        mesh.AddFace(v1, v0, v3);

        // Direct AddFace does not link twins
        foreach (var he in mesh.HalfEdges)
            Assert.Null(he.Twin);
    }

    [Fact]
    public void FacesAroundVertex_SingleTriangle()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));

        var face = mesh.AddFace(v0, v1, v2);
        var faces = mesh.FacesAroundVertex(v0).ToList();
        Assert.Single(faces);
        Assert.Same(face, faces[0]);
    }

    [Fact]
    public void FacesAroundVertex_TwoTriangles_SharingVertex()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        var v3 = mesh.AddVertex(new Vec3(-1, 0, 0));

        var f1 = mesh.AddFace(v0, v1, v2);
        var f2 = mesh.AddFace(v0, v2, v3);

        var faces = mesh.FacesAroundVertex(v0).ToList();
        Assert.Equal(2, faces.Count);
        Assert.Contains(f1, faces);
        Assert.Contains(f2, faces);
    }

    [Fact]
    public void GetBounds_EmptyMesh_IsEmpty()
    {
        var mesh = new HalfEdgeMesh();
        var bounds = mesh.GetBounds();
        Assert.Equal(Aabb.Empty, bounds);
    }

    [Fact]
    public void GetBounds_SingleVertex()
    {
        var mesh = new HalfEdgeMesh();
        mesh.AddVertex(new Vec3(5, 10, 15));
        var bounds = mesh.GetBounds();
        Assert.Equal(new Vec3(5, 10, 15), bounds.Min);
        Assert.Equal(new Vec3(5, 10, 15), bounds.Max);
    }

    [Fact]
    public void GetBounds_MultipleFaces()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(new Vec3(-1, -2, -3));
        var v1 = mesh.AddVertex(new Vec3(4, 5, 6));
        var v2 = mesh.AddVertex(new Vec3(0, 0, 0));
        mesh.AddFace(v0, v1, v2);

        var bounds = mesh.GetBounds();
        Assert.Equal(new Vec3(-1, -2, -3), bounds.Min);
        Assert.Equal(new Vec3(4, 5, 6), bounds.Max);
    }
}
