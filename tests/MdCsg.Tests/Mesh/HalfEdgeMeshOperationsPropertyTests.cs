using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: HalfEdgeMesh — AddVertex/AddHalfEdge/AddFace, GetBounds, FacesAroundVertex, manual DCEL construction</summary>
public class HalfEdgeMeshOperationsPropertyTests
{
    [Fact]
    public void AddVertex_IncreasesCount()
    {
        var mesh = new HalfEdgeMesh();
        Assert.Equal(0, mesh.Vertices.Count);
        mesh.AddVertex(Vec3.Zero);
        Assert.Equal(1, mesh.Vertices.Count);
        mesh.AddVertex(Vec3.UnitX);
        Assert.Equal(2, mesh.Vertices.Count);
    }

    [Fact]
    public void AddVertex_AssignsSequentialIds()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(Vec3.UnitX);
        var v2 = mesh.AddVertex(Vec3.UnitY);
        Assert.Equal(0, v0.Id);
        Assert.Equal(1, v1.Id);
        Assert.Equal(2, v2.Id);
    }

    [Fact]
    public void AddVertex_StoresPosition()
    {
        var mesh = new HalfEdgeMesh();
        var pos = new Vec3(1.5, 2.5, 3.5);
        var v = mesh.AddVertex(pos);
        Assert.Equal(pos, v.Position);
    }

    [Fact]
    public void AddHalfEdge_IncreasesCount()
    {
        var mesh = new HalfEdgeMesh();
        Assert.Equal(0, mesh.HalfEdges.Count);
        mesh.AddHalfEdge();
        Assert.Equal(1, mesh.HalfEdges.Count);
    }

    [Fact]
    public void AddFace_CreatesThreeHalfEdges()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(Vec3.UnitX);
        var v2 = mesh.AddVertex(Vec3.UnitY);
        int heBefore = mesh.HalfEdges.Count;
        mesh.AddFace(v0, v1, v2);
        Assert.Equal(heBefore + 3, mesh.HalfEdges.Count);
    }

    [Fact]
    public void AddFace_SetsCorrectTargets()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(Vec3.UnitX);
        var v2 = mesh.AddVertex(Vec3.UnitY);
        var face = mesh.AddFace(v0, v1, v2);
        Assert.Same(v1, face.Edge.Target);
        Assert.Same(v2, face.Edge.Next.Target);
        Assert.Same(v0, face.Edge.Next.Next.Target);
    }

    [Fact]
    public void AddFace_FormsValidCycle()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(Vec3.UnitX);
        var v2 = mesh.AddVertex(Vec3.UnitY);
        var face = mesh.AddFace(v0, v1, v2);
        Assert.Same(face.Edge, face.Edge.Next.Next.Next);
        Assert.Same(face.Edge, face.Edge.Prev.Prev.Prev);
    }

    [Fact]
    public void AddFace_SetsOutgoingEdges()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(Vec3.UnitX);
        var v2 = mesh.AddVertex(Vec3.UnitY);
        mesh.AddFace(v0, v1, v2);
        Assert.NotNull(v0.OutgoingEdge);
        Assert.NotNull(v1.OutgoingEdge);
        Assert.NotNull(v2.OutgoingEdge);
    }

    [Fact]
    public void GetBounds_SingleVertex_PointBounds()
    {
        var mesh = new HalfEdgeMesh();
        mesh.AddVertex(new Vec3(1, 2, 3));
        var bounds = mesh.GetBounds();
        Assert.Equal(1.0, bounds.Min.X);
        Assert.Equal(2.0, bounds.Min.Y);
        Assert.Equal(3.0, bounds.Min.Z);
        Assert.Equal(1.0, bounds.Max.X);
    }

    [Fact]
    public void GetBounds_MultipleVertices_EnclosesAll()
    {
        var mesh = new HalfEdgeMesh();
        mesh.AddVertex(new Vec3(-1, -2, -3));
        mesh.AddVertex(new Vec3(4, 5, 6));
        mesh.AddVertex(new Vec3(0, 0, 0));
        var bounds = mesh.GetBounds();
        Assert.Equal(-1.0, bounds.Min.X);
        Assert.Equal(-2.0, bounds.Min.Y);
        Assert.Equal(-3.0, bounds.Min.Z);
        Assert.Equal(4.0, bounds.Max.X);
        Assert.Equal(5.0, bounds.Max.Y);
        Assert.Equal(6.0, bounds.Max.Z);
    }

    [Fact]
    public void GetBounds_EmptyMesh_ReturnsEmpty()
    {
        var mesh = new HalfEdgeMesh();
        var bounds = mesh.GetBounds();
        Assert.Equal(Aabb.Empty, bounds);
    }

    [Fact]
    public void FacesAroundVertex_SingleFace_ReturnsThatFace()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(Vec3.UnitX);
        var v2 = mesh.AddVertex(Vec3.UnitY);
        var face = mesh.AddFace(v0, v1, v2);
        var faces = mesh.FacesAroundVertex(v0).ToList();
        Assert.Single(faces);
        Assert.Same(face, faces[0]);
    }

    [Fact]
    public void FacesAroundVertex_TwoFacesSharedVertex_ReturnsBoth()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(Vec3.UnitX);
        var v2 = mesh.AddVertex(Vec3.UnitY);
        var v3 = mesh.AddVertex(Vec3.UnitZ);
        mesh.AddFace(v0, v1, v2);
        mesh.AddFace(v0, v2, v3);
        var faces = mesh.FacesAroundVertex(v0).ToList();
        Assert.Equal(2, faces.Count);
    }

    [Fact]
    public void FacesAroundVertex_UnrelatedVertex_ReturnsNone()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(Vec3.UnitX);
        var v2 = mesh.AddVertex(Vec3.UnitY);
        var v3 = mesh.AddVertex(new Vec3(10, 10, 10));
        mesh.AddFace(v0, v1, v2);
        var faces = mesh.FacesAroundVertex(v3).ToList();
        Assert.Empty(faces);
    }

    [Fact]
    public void CubeGetBounds_CorrectDimensions()
    {
        var cube = MeshFactory.CreateCube(new Vec3(1, 2, 3), 2.0);
        var bounds = cube.Mesh.GetBounds();
        Assert.True(System.Math.Abs(bounds.Size.X - 2.0) < 0.01);
        Assert.True(System.Math.Abs(bounds.Size.Y - 2.0) < 0.01);
        Assert.True(System.Math.Abs(bounds.Size.Z - 2.0) < 0.01);
    }

    [Fact]
    public void CubeFacesAroundVertex_EachVertexHasThreeFaces()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var v in cube.Mesh.Vertices)
        {
            var faces = cube.Mesh.FacesAroundVertex(v).ToList();
            Assert.True(faces.Count >= 3,
                $"Vertex {v.Id} has {faces.Count} adjacent faces, expected >= 3");
        }
    }

    [Fact]
    public void AddFace_AllHalfEdgesReferFace()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(Vec3.UnitX);
        var v2 = mesh.AddVertex(Vec3.UnitY);
        var face = mesh.AddFace(v0, v1, v2);
        var he = face.Edge;
        Assert.Same(face, he.Face);
        Assert.Same(face, he.Next.Face);
        Assert.Same(face, he.Next.Next.Face);
    }
}
