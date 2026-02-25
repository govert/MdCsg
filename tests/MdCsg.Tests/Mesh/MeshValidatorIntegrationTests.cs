using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: MeshValidator integration — validate factory meshes and CSG output topological properties</summary>
public class MeshValidatorIntegrationTests
{
    [Fact]
    public void Cube_HasValidFaceCycles()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        Assert.True(MeshValidator.HasValidFaceCycles(mesh));
    }

    [Fact]
    public void Cube_IsConsistentlyOriented()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        Assert.True(MeshValidator.IsConsistentlyOriented(mesh));
    }

    [Fact]
    public void Cube_AllEdgesHaveTwins()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        Assert.True(MeshValidator.AllEdgesHaveTwins(mesh));
    }

    [Fact]
    public void Cube_IsEdgeManifold()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        Assert.True(MeshValidator.IsEdgeManifold(mesh));
    }

    [Fact]
    public void Cube_EulerCharacteristic_Is2()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        Assert.Equal(2, MeshValidator.EulerCharacteristic(mesh));
    }

    [Fact]
    public void Cube_Validate_IsClosedManifold()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var result = MeshValidator.Validate(mesh);
        Assert.True(result.IsClosedManifold);
    }

    [Fact]
    public void Sphere_HasValidFaceCycles()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        Assert.True(MeshValidator.HasValidFaceCycles(mesh));
    }

    [Fact]
    public void Sphere_IsConsistentlyOriented()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        Assert.True(MeshValidator.IsConsistentlyOriented(mesh));
    }

    [Fact]
    public void Sphere_EulerCharacteristic_Is2()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        Assert.Equal(2, MeshValidator.EulerCharacteristic(mesh));
    }

    [Fact]
    public void Tetrahedron_HasValidFaceCycles()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        Assert.True(MeshValidator.HasValidFaceCycles(mesh));
    }

    [Fact]
    public void Tetrahedron_IsConsistentlyOriented()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        Assert.True(MeshValidator.IsConsistentlyOriented(mesh));
    }

    [Fact]
    public void Tetrahedron_EulerCharacteristic_Is2()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        Assert.Equal(2, MeshValidator.EulerCharacteristic(mesh));
    }

    [Fact]
    public void Tetrahedron_IsClosedManifold()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        var result = MeshValidator.Validate(mesh);
        Assert.True(result.IsClosedManifold);
    }

    [Fact]
    public void Validate_Cube_VertexCount()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var result = MeshValidator.Validate(mesh);
        Assert.Equal(8, result.VertexCount);
    }

    [Fact]
    public void Validate_Cube_FaceCount()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var result = MeshValidator.Validate(mesh);
        Assert.Equal(12, result.FaceCount);
    }

    [Fact]
    public void Validate_Cube_EdgeCount()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var result = MeshValidator.Validate(mesh);
        Assert.Equal(18, result.EdgeCount); // 12 faces × 3 half-edges / 2
    }

    [Fact]
    public void Validate_Tetrahedron_VertexCount()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        var result = MeshValidator.Validate(mesh);
        Assert.Equal(4, result.VertexCount);
    }

    [Fact]
    public void Validate_Tetrahedron_FaceCount()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        var result = MeshValidator.Validate(mesh);
        Assert.Equal(4, result.FaceCount);
    }

    [Fact]
    public void Validate_Tetrahedron_EdgeCount()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        var result = MeshValidator.Validate(mesh);
        Assert.Equal(6, result.EdgeCount);
    }

    [Fact]
    public void CsgOutput_HasValidFaceCycles()
    {
        var a = new MdCsg.Api.Solid(MeshFactory.CreateCube().Mesh);
        var b = new MdCsg.Api.Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = MdCsg.Api.Csg.Union(a, b);
        Assert.True(MeshValidator.HasValidFaceCycles(result.Mesh));
    }

    [Fact]
    public void CsgOutput_IsConsistentlyOriented()
    {
        var a = new MdCsg.Api.Solid(MeshFactory.CreateCube().Mesh);
        var b = new MdCsg.Api.Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = MdCsg.Api.Csg.Union(a, b);
        Assert.True(MeshValidator.IsConsistentlyOriented(result.Mesh));
    }

    [Fact]
    public void SingleTriangle_NotClosedManifold()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(new Vec3(0, 0, 0));
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        mesh.AddFace(v0, v1, v2);
        var result = MeshValidator.Validate(mesh);
        Assert.False(result.IsClosedManifold);
        Assert.False(result.AllEdgesHaveTwins);
    }

    [Fact]
    public void SingleTriangle_HasValidFaceCycles()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(new Vec3(0, 0, 0));
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        mesh.AddFace(v0, v1, v2);
        Assert.True(MeshValidator.HasValidFaceCycles(mesh));
    }

    [Fact]
    public void OffsetCube_IsClosedManifold()
    {
        var mesh = MeshFactory.CreateCube(new Vec3(10, 20, 30), 5).Mesh;
        var result = MeshValidator.Validate(mesh);
        Assert.True(result.IsClosedManifold);
    }
}
