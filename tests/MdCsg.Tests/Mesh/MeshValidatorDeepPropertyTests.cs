using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: MeshValidator — edge manifoldness, consistent orientation, Euler characteristic, deep checks</summary>
public class MeshValidatorDeepPropertyTests
{
    [Fact]
    public void Cube_AllEdgesHaveTwins()
    {
        var cube = MeshFactory.CreateCube();
        Assert.True(MeshValidator.AllEdgesHaveTwins(cube.Mesh));
    }

    [Fact]
    public void Cube_IsEdgeManifold()
    {
        var cube = MeshFactory.CreateCube();
        Assert.True(MeshValidator.IsEdgeManifold(cube.Mesh));
    }

    [Fact]
    public void Cube_IsConsistentlyOriented()
    {
        var cube = MeshFactory.CreateCube();
        Assert.True(MeshValidator.IsConsistentlyOriented(cube.Mesh));
    }

    [Fact]
    public void Cube_EulerCharacteristic_IsTwo()
    {
        var cube = MeshFactory.CreateCube();
        Assert.Equal(2, MeshValidator.EulerCharacteristic(cube.Mesh));
    }

    [Fact]
    public void Cube_HasValidFaceCycles()
    {
        var cube = MeshFactory.CreateCube();
        Assert.True(MeshValidator.HasValidFaceCycles(cube.Mesh));
    }

    [Fact]
    public void Tetrahedron_AllEdgesHaveTwins()
    {
        var tet = MeshFactory.CreateTetrahedron();
        Assert.True(MeshValidator.AllEdgesHaveTwins(tet.Mesh));
    }

    [Fact]
    public void Tetrahedron_EulerCharacteristic_IsTwo()
    {
        var tet = MeshFactory.CreateTetrahedron();
        Assert.Equal(2, MeshValidator.EulerCharacteristic(tet.Mesh));
    }

    [Fact]
    public void Sphere_AllEdgesHaveTwins()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        Assert.True(MeshValidator.AllEdgesHaveTwins(sphere.Mesh));
    }

    [Fact]
    public void Sphere_EulerCharacteristic_IsTwo()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        Assert.Equal(2, MeshValidator.EulerCharacteristic(sphere.Mesh));
    }

    [Fact]
    public void Sphere_IsConsistentlyOriented()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        Assert.True(MeshValidator.IsConsistentlyOriented(sphere.Mesh));
    }

    [Fact]
    public void SingleTriangle_NotAllEdgesHaveTwins()
    {
        var builder = new MeshBuilder();
        var mesh = builder.Build(new[] { new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY) });
        Assert.False(MeshValidator.AllEdgesHaveTwins(mesh));
    }

    [Fact]
    public void EmptyMesh_IsValid()
    {
        var mesh = new HalfEdgeMesh();
        Assert.True(MeshValidator.AllEdgesHaveTwins(mesh));
        Assert.True(MeshValidator.IsEdgeManifold(mesh));
        Assert.True(MeshValidator.IsConsistentlyOriented(mesh));
    }

    [Fact]
    public void Cube_Validate_IsValid()
    {
        var cube = MeshFactory.CreateCube();
        var result = MeshValidator.Validate(cube.Mesh);
        Assert.True(result.IsClosedManifold, $"Cube should be valid: {result}");
    }

    [Fact]
    public void Tetrahedron_Validate_IsValid()
    {
        var tet = MeshFactory.CreateTetrahedron();
        var result = MeshValidator.Validate(tet.Mesh);
        Assert.True(result.IsClosedManifold, $"Tetrahedron should be valid: {result}");
    }

    [Fact]
    public void Sphere_Validate_IsValid()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var result = MeshValidator.Validate(sphere.Mesh);
        Assert.True(result.IsClosedManifold, $"Sphere should be valid: {result}");
    }

    [Fact]
    public void Sphere_IsEdgeManifold()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        Assert.True(MeshValidator.IsEdgeManifold(sphere.Mesh));
    }

    [Fact]
    public void Sphere_HasValidFaceCycles()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        Assert.True(MeshValidator.HasValidFaceCycles(sphere.Mesh));
    }
}
