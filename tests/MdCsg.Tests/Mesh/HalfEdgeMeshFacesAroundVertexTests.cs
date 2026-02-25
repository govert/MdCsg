using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: HalfEdgeMesh.FacesAroundVertex — correctness, boundary, isolated vertices</summary>
public class HalfEdgeMeshFacesAroundVertexTests
{
    [Fact]
    public void SingleFace_AllVertices_ReturnThatFace()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        var face = mesh.AddFace(v0, v1, v2);

        Assert.Contains(face, mesh.FacesAroundVertex(v0));
        Assert.Contains(face, mesh.FacesAroundVertex(v1));
        Assert.Contains(face, mesh.FacesAroundVertex(v2));
    }

    [Fact]
    public void IsolatedVertex_NoFaces()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        mesh.AddFace(v0, v1, v2);

        var isolated = mesh.AddVertex(new Vec3(5, 5, 5));
        Assert.Empty(mesh.FacesAroundVertex(isolated));
    }

    [Fact]
    public void TwoFaces_SharedEdge_CommonVertex()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        var v3 = mesh.AddVertex(new Vec3(1, 1, 0));
        var f1 = mesh.AddFace(v0, v1, v2);
        var f2 = mesh.AddFace(v1, v3, v2);

        // v1 and v2 are shared
        var facesAroundV1 = mesh.FacesAroundVertex(v1).ToList();
        Assert.Equal(2, facesAroundV1.Count);
        Assert.Contains(f1, facesAroundV1);
        Assert.Contains(f2, facesAroundV1);
    }

    [Fact]
    public void FanOfTriangles_CenterVertex_AllFaces()
    {
        var mesh = new HalfEdgeMesh();
        var center = mesh.AddVertex(Vec3.Zero);
        var outerVerts = new List<Vertex>();
        for (int i = 0; i < 6; i++)
        {
            double angle = i * System.Math.PI / 3;
            outerVerts.Add(mesh.AddVertex(new Vec3(System.Math.Cos(angle), System.Math.Sin(angle), 0)));
        }
        var faces = new List<Face>();
        for (int i = 0; i < 6; i++)
            faces.Add(mesh.AddFace(center, outerVerts[i], outerVerts[(i + 1) % 6]));

        var facesAroundCenter = mesh.FacesAroundVertex(center).ToList();
        Assert.Equal(6, facesAroundCenter.Count);
        foreach (var f in faces)
            Assert.Contains(f, facesAroundCenter);
    }

    [Fact]
    public void CubeMesh_CornerVertex_HasAdjacentFaces()
    {
        var cubeMesh = MeshFactory.CreateCube().Mesh;
        // Each vertex of a cube is shared by multiple faces
        var v = cubeMesh.Vertices[0];
        var faces = cubeMesh.FacesAroundVertex(v).ToList();
        Assert.True(faces.Count > 0, "Corner vertex should be part of at least one face");
    }

    [Fact]
    public void FacesAroundVertex_NoDuplicates()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        var v3 = mesh.AddVertex(new Vec3(-1, 0, 0));
        mesh.AddFace(v0, v1, v2);
        mesh.AddFace(v0, v2, v3);

        var faces = mesh.FacesAroundVertex(v0).ToList();
        Assert.Equal(faces.Count, faces.Distinct().Count());
    }

    [Fact]
    public void EdgeVertex_HasTwoFaces()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        var v3 = mesh.AddVertex(new Vec3(1, 1, 0));
        mesh.AddFace(v0, v1, v2);
        mesh.AddFace(v1, v3, v2);

        // v3 is only in the second face
        var facesAroundV3 = mesh.FacesAroundVertex(v3).ToList();
        Assert.Single(facesAroundV3);
    }

    [Fact]
    public void GetBounds_EmptyMesh_ReturnsEmpty()
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
    public void GetBounds_CubeMesh_CorrectExtent()
    {
        var cubeMesh = MeshFactory.CreateCube().Mesh;
        var bounds = cubeMesh.GetBounds();
        // Unit cube [0,1]^3
        Assert.True(bounds.Min.X <= 0.01);
        Assert.True(bounds.Min.Y <= 0.01);
        Assert.True(bounds.Min.Z <= 0.01);
        Assert.True(bounds.Max.X >= 0.99);
        Assert.True(bounds.Max.Y >= 0.99);
        Assert.True(bounds.Max.Z >= 0.99);
    }
}
