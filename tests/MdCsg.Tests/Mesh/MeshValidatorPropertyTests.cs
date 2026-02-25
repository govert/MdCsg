using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: MeshValidator — AllEdgesHaveTwins, IsEdgeManifold, IsConsistentlyOriented, EulerCharacteristic, HasValidFaceCycles, Validate</summary>
public class MeshValidatorPropertyTests
{
    [Fact]
    public void Cube_AllEdgesHaveTwins()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        Assert.True(MeshValidator.AllEdgesHaveTwins(cube.Mesh));
    }

    [Fact]
    public void Cube_IsEdgeManifold()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        Assert.True(MeshValidator.IsEdgeManifold(cube.Mesh));
    }

    [Fact]
    public void Cube_IsConsistentlyOriented()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        Assert.True(MeshValidator.IsConsistentlyOriented(cube.Mesh));
    }

    [Fact]
    public void Cube_HasValidFaceCycles()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        Assert.True(MeshValidator.HasValidFaceCycles(cube.Mesh));
    }

    [Fact]
    public void Cube_EulerCharacteristic_IsTwo()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        Assert.Equal(2, MeshValidator.EulerCharacteristic(cube.Mesh));
    }

    [Fact]
    public void Sphere_AllEdgesHaveTwins()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        Assert.True(MeshValidator.AllEdgesHaveTwins(sphere.Mesh));
    }

    [Fact]
    public void Sphere_IsEdgeManifold()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        Assert.True(MeshValidator.IsEdgeManifold(sphere.Mesh));
    }

    [Fact]
    public void Sphere_EulerCharacteristic_IsTwo()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        Assert.Equal(2, MeshValidator.EulerCharacteristic(sphere.Mesh));
    }

    [Fact]
    public void Tetrahedron_AllValidationChecksPass()
    {
        var tet = MeshFactory.CreateTetrahedron();
        var result = MeshValidator.Validate(tet.Mesh);
        Assert.True(result.AllEdgesHaveTwins);
        Assert.True(result.IsEdgeManifold);
        Assert.True(result.IsConsistentlyOriented);
        Assert.True(result.HasValidFaceCycles);
        Assert.Equal(2, result.EulerCharacteristic);
        Assert.True(result.IsClosedManifold);
    }

    [Fact]
    public void Tetrahedron_Validate_CorrectCounts()
    {
        var tet = MeshFactory.CreateTetrahedron();
        var result = MeshValidator.Validate(tet.Mesh);
        Assert.Equal(4, result.VertexCount);
        Assert.Equal(6, result.EdgeCount);
        Assert.Equal(4, result.FaceCount);
    }

    [Fact]
    public void Cube_Validate_CorrectCounts()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var result = MeshValidator.Validate(cube.Mesh);
        Assert.Equal(8, result.VertexCount);
        Assert.Equal(12, result.FaceCount);
    }

    [Fact]
    public void Cube_Validate_IsClosedManifold()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var result = MeshValidator.Validate(cube.Mesh);
        Assert.True(result.IsClosedManifold);
    }

    [Fact]
    public void EmptyMesh_NotEdgeManifold_ButValid()
    {
        var mesh = new HalfEdgeMesh();
        // Empty mesh trivially has all properties true (vacuous truth)
        Assert.True(MeshValidator.AllEdgesHaveTwins(mesh));
        Assert.True(MeshValidator.IsEdgeManifold(mesh));
        Assert.True(MeshValidator.HasValidFaceCycles(mesh));
        Assert.Equal(0, MeshValidator.EulerCharacteristic(mesh));
    }

    [Fact]
    public void Sphere_Validate_IsClosedManifold()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var result = MeshValidator.Validate(sphere.Mesh);
        Assert.True(result.IsClosedManifold);
    }

    [Fact]
    public void Sphere_HasValidFaceCycles()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        Assert.True(MeshValidator.HasValidFaceCycles(sphere.Mesh));
    }

    [Fact]
    public void Sphere_IsConsistentlyOriented()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        Assert.True(MeshValidator.IsConsistentlyOriented(sphere.Mesh));
    }

    [Fact]
    public void Validate_Record_HasAllFields()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 1.0);
        var result = MeshValidator.Validate(cube.Mesh);
        // Verify all fields are accessible
        Assert.True(result.VertexCount > 0);
        Assert.True(result.EdgeCount > 0);
        Assert.True(result.FaceCount > 0);
        Assert.True(result.EulerCharacteristic == 2);
    }

    [Fact]
    public void Sphere_Sub0_Validate_ClosedManifold()
    {
        // Icosahedron (subdivision 0) — 12 vertices, 20 faces, 30 edges
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 0);
        var result = MeshValidator.Validate(sphere.Mesh);
        Assert.True(result.IsClosedManifold);
        Assert.Equal(12, result.VertexCount);
        Assert.Equal(20, result.FaceCount);
        Assert.Equal(30, result.EdgeCount);
        Assert.Equal(2, result.EulerCharacteristic);
    }
}
