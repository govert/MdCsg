using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: HalfEdgeMesh topology - AddVertex, AddFace, FacesAroundVertex, GetBounds, face properties</summary>
public class HalfEdgeMeshTopologyPropertyTests
{
    [Fact]
    public void AddVertex_IncrementsCount()
    {
        var mesh = new HalfEdgeMesh();
        mesh.AddVertex(Vec3.Zero);
        mesh.AddVertex(Vec3.UnitX);
        Assert.Equal(2, mesh.Vertices.Count);
    }

    [Fact]
    public void AddVertex_AssignsSequentialIds()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(Vec3.UnitX);
        Assert.Equal(0, v0.Id);
        Assert.Equal(1, v1.Id);
    }

    [Fact]
    public void AddVertex_StoresPosition()
    {
        var mesh = new HalfEdgeMesh();
        var v = mesh.AddVertex(new Vec3(1, 2, 3));
        Assert.Equal(new Vec3(1, 2, 3), v.Position);
    }

    [Fact]
    public void AddFace_CreatesThreeHalfEdges()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(Vec3.UnitX);
        var v2 = mesh.AddVertex(Vec3.UnitY);
        mesh.AddFace(v0, v1, v2);
        Assert.Equal(3, mesh.HalfEdges.Count);
        Assert.Single(mesh.Faces);
    }

    [Fact]
    public void AddFace_EdgesCycleProperly()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(Vec3.UnitX);
        var v2 = mesh.AddVertex(Vec3.UnitY);
        var face = mesh.AddFace(v0, v1, v2);

        var start = face.Edge;
        Assert.Same(start, start.Next.Next.Next);
    }

    [Fact]
    public void AddFace_PrevPointsBack()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(Vec3.UnitX);
        var v2 = mesh.AddVertex(Vec3.UnitY);
        mesh.AddFace(v0, v1, v2);

        foreach (var he in mesh.HalfEdges)
            Assert.Same(he, he.Next.Prev);
    }

    [Fact]
    public void AddFace_AllEdgesPointToFace()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(Vec3.UnitX);
        var v2 = mesh.AddVertex(Vec3.UnitY);
        var face = mesh.AddFace(v0, v1, v2);

        foreach (var he in mesh.HalfEdges)
            Assert.Same(face, he.Face);
    }

    [Fact]
    public void AddFace_VerticesHaveOutgoingEdges()
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
    public void FacesAroundVertex_Center_AllFaces()
    {
        // Build a fan of 3 triangles around center vertex
        var mesh = new HalfEdgeMesh();
        var center = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(Vec3.UnitX);
        var v2 = mesh.AddVertex(Vec3.UnitY);
        var v3 = mesh.AddVertex(Vec3.UnitZ);
        mesh.AddFace(center, v1, v2);
        mesh.AddFace(center, v2, v3);
        mesh.AddFace(center, v3, v1);

        var faces = mesh.FacesAroundVertex(center).ToList();
        Assert.Equal(3, faces.Count);
    }

    [Fact]
    public void FacesAroundVertex_Corner_OneFace()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(Vec3.UnitX);
        var v2 = mesh.AddVertex(Vec3.UnitY);
        var v3 = mesh.AddVertex(new Vec3(10, 10, 10));
        mesh.AddFace(v0, v1, v2);
        mesh.AddFace(v1, v3, v2); // shares v1 and v2 with first, not v0

        var faces = mesh.FacesAroundVertex(v0).ToList();
        Assert.Single(faces);
    }

    [Fact]
    public void GetBounds_SingleTriangle()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(new Vec3(-1, -2, -3));
        var v1 = mesh.AddVertex(new Vec3(4, 5, 6));
        var v2 = mesh.AddVertex(new Vec3(0, 0, 0));
        mesh.AddFace(v0, v1, v2);

        var bounds = mesh.GetBounds();
        Assert.Equal(-1, bounds.Min.X);
        Assert.Equal(-2, bounds.Min.Y);
        Assert.Equal(-3, bounds.Min.Z);
        Assert.Equal(4, bounds.Max.X);
        Assert.Equal(5, bounds.Max.Y);
        Assert.Equal(6, bounds.Max.Z);
    }

    [Fact]
    public void GetBounds_EmptyMesh_IsEmpty()
    {
        var mesh = new HalfEdgeMesh();
        var bounds = mesh.GetBounds();
        // Aabb.Empty should have max < min
        Assert.True(bounds.Min.X > bounds.Max.X);
    }

    [Fact]
    public void GetBounds_Cube_MatchesExtents()
    {
        // CreateCube scales unit cube [0,1]^3 by size and adds offset
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var bounds = cube.Mesh.GetBounds();
        Assert.True(System.Math.Abs(bounds.Min.X - 0.0) < 1e-10);
        Assert.True(System.Math.Abs(bounds.Max.X - 2.0) < 1e-10);
        Assert.True(System.Math.Abs(bounds.Min.Y - 0.0) < 1e-10);
        Assert.True(System.Math.Abs(bounds.Max.Y - 2.0) < 1e-10);
    }

    // --- Face properties ---

    [Fact]
    public void Face_GetVertices_ReturnsThree()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(Vec3.UnitX);
        var v2 = mesh.AddVertex(Vec3.UnitY);
        var face = mesh.AddFace(v0, v1, v2);

        var verts = face.GetVertices();
        Assert.Equal(3, verts.Count);
    }

    [Fact]
    public void Face_Centroid_IsAverage()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(new Vec3(0, 0, 0));
        var v1 = mesh.AddVertex(new Vec3(3, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 3, 0));
        var face = mesh.AddFace(v0, v1, v2);

        var centroid = face.Centroid;
        Assert.True(Vec3.Distance(centroid, new Vec3(1, 1, 0)) < 1e-10);
    }

    [Fact]
    public void Face_Normal_IsNonZero()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(Vec3.UnitX);
        var v2 = mesh.AddVertex(Vec3.UnitY);
        var face = mesh.AddFace(v0, v1, v2);

        Assert.True(face.Normal.Length > 1e-10);
    }

    [Fact]
    public void Face_UnitNormal_IsUnitLength()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(Vec3.UnitX);
        var v2 = mesh.AddVertex(Vec3.UnitY);
        var face = mesh.AddFace(v0, v1, v2);

        Assert.True(System.Math.Abs(face.UnitNormal.Length - 1.0) < 1e-10);
    }

    [Fact]
    public void Face_OriginalFaceId_DefaultsToId()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(Vec3.UnitX);
        var v2 = mesh.AddVertex(Vec3.UnitY);
        var face = mesh.AddFace(v0, v1, v2);

        Assert.Equal(face.Id, face.OriginalFaceId);
    }

    [Fact]
    public void Face_PatchId_DefaultsToMinus1()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(Vec3.UnitX);
        var v2 = mesh.AddVertex(Vec3.UnitY);
        var face = mesh.AddFace(v0, v1, v2);

        Assert.Equal(-1, face.PatchId);
    }

    [Fact]
    public void Face_GetTrianglePositions_Matches()
    {
        var mesh = new HalfEdgeMesh();
        var p0 = new Vec3(1, 2, 3);
        var p1 = new Vec3(4, 5, 6);
        var p2 = new Vec3(7, 8, 9);
        var v0 = mesh.AddVertex(p0);
        var v1 = mesh.AddVertex(p1);
        var v2 = mesh.AddVertex(p2);
        var face = mesh.AddFace(v0, v1, v2);

        face.GetTrianglePositions(out var a, out var b, out var c);
        // Positions come from half-edge targets, which are v1, v2, v0
        var positions = new HashSet<Vec3> { a, b, c };
        Assert.Contains(p0, positions);
        Assert.Contains(p1, positions);
        Assert.Contains(p2, positions);
    }

    // --- HalfEdge properties ---

    [Fact]
    public void HalfEdge_Origin_IsPrevTarget()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(Vec3.UnitX);
        var v2 = mesh.AddVertex(Vec3.UnitY);
        mesh.AddFace(v0, v1, v2);

        foreach (var he in mesh.HalfEdges)
            Assert.Same(he.Origin, he.Prev.Target);
    }

    [Fact]
    public void HalfEdge_SequentialIds()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(Vec3.UnitX);
        var v2 = mesh.AddVertex(Vec3.UnitY);
        mesh.AddFace(v0, v1, v2);

        for (int i = 0; i < mesh.HalfEdges.Count; i++)
            Assert.Equal(i, mesh.HalfEdges[i].Id);
    }
}
