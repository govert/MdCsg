using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: MeshValidator - AllEdgesHaveTwins, IsEdgeManifold, ConsistentOrientation, EulerCharacteristic, ValidateFaceCycles</summary>
public class MeshValidatorPropertyTests
{
    // --- Cube validation ---

    [Fact]
    public void Cube_HasValidFaceCycles()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        Assert.True(MeshValidator.HasValidFaceCycles(cube.Mesh));
    }

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
    public void Cube_EulerCharacteristic_Is2()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        Assert.Equal(2, MeshValidator.EulerCharacteristic(cube.Mesh));
    }

    [Fact]
    public void Cube_Validate_IsClosedManifold()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var result = MeshValidator.Validate(cube.Mesh);
        Assert.True(result.IsClosedManifold);
    }

    [Fact]
    public void Cube_Validate_CorrectCounts()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var result = MeshValidator.Validate(cube.Mesh);
        Assert.Equal(8, result.VertexCount);
        Assert.Equal(12, result.FaceCount);
        Assert.Equal(18, result.EdgeCount); // 36 half-edges / 2
    }

    // --- Sphere validation ---

    [Fact]
    public void Sphere_HasValidFaceCycles()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 3);
        Assert.True(MeshValidator.HasValidFaceCycles(sphere.Mesh));
    }

    [Fact]
    public void Sphere_AllEdgesHaveTwins()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 3);
        Assert.True(MeshValidator.AllEdgesHaveTwins(sphere.Mesh));
    }

    [Fact]
    public void Sphere_IsEdgeManifold()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 3);
        Assert.True(MeshValidator.IsEdgeManifold(sphere.Mesh));
    }

    [Fact]
    public void Sphere_EulerCharacteristic_Is2()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 3);
        Assert.Equal(2, MeshValidator.EulerCharacteristic(sphere.Mesh));
    }

    [Fact]
    public void Sphere_IsClosedManifold()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 3);
        var result = MeshValidator.Validate(sphere.Mesh);
        Assert.True(result.IsClosedManifold);
    }

    // --- Tetrahedron validation ---

    [Fact]
    public void Tetrahedron_IsClosedManifold()
    {
        var tet = MeshFactory.CreateTetrahedron();
        var result = MeshValidator.Validate(tet.Mesh);
        Assert.True(result.IsClosedManifold);
    }

    [Fact]
    public void Tetrahedron_EulerCharacteristic_Is2()
    {
        var tet = MeshFactory.CreateTetrahedron();
        Assert.Equal(2, MeshValidator.EulerCharacteristic(tet.Mesh));
    }

    [Fact]
    public void Tetrahedron_CorrectCounts()
    {
        var tet = MeshFactory.CreateTetrahedron();
        var result = MeshValidator.Validate(tet.Mesh);
        Assert.Equal(4, result.VertexCount);
        Assert.Equal(4, result.FaceCount);
        Assert.Equal(6, result.EdgeCount);
    }

    // --- Single triangle (not closed) ---

    [Fact]
    public void SingleTriangle_NotAllEdgesHaveTwins()
    {
        var builder = new MeshBuilder();
        var tris = new[] { new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY) };
        var mesh = builder.Build(tris);
        Assert.False(MeshValidator.AllEdgesHaveTwins(mesh));
    }

    [Fact]
    public void SingleTriangle_NotClosedManifold()
    {
        var builder = new MeshBuilder();
        var tris = new[] { new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY) };
        var mesh = builder.Build(tris);
        var result = MeshValidator.Validate(mesh);
        Assert.False(result.IsClosedManifold);
    }

    [Fact]
    public void SingleTriangle_HasValidFaceCycles()
    {
        var builder = new MeshBuilder();
        var tris = new[] { new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY) };
        var mesh = builder.Build(tris);
        Assert.True(MeshValidator.HasValidFaceCycles(mesh));
    }

    // --- Empty mesh ---

    [Fact]
    public void EmptyMesh_AllChecksPass()
    {
        var builder = new MeshBuilder();
        var mesh = builder.Build(Array.Empty<Triangle3>());
        Assert.True(MeshValidator.AllEdgesHaveTwins(mesh));
        Assert.True(MeshValidator.IsEdgeManifold(mesh));
        Assert.True(MeshValidator.IsConsistentlyOriented(mesh));
        Assert.True(MeshValidator.HasValidFaceCycles(mesh));
    }

    [Fact]
    public void EmptyMesh_EulerCharacteristic_IsZero()
    {
        var builder = new MeshBuilder();
        var mesh = builder.Build(Array.Empty<Triangle3>());
        Assert.Equal(0, MeshValidator.EulerCharacteristic(mesh));
    }

    // --- MeshValidationResult ---

    [Fact]
    public void ValidationResult_HasAllFields()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var result = MeshValidator.Validate(cube.Mesh);
        Assert.True(result.AllEdgesHaveTwins);
        Assert.True(result.IsEdgeManifold);
        Assert.True(result.IsConsistentlyOriented);
        Assert.True(result.HasValidFaceCycles);
        Assert.Equal(2, result.EulerCharacteristic);
        Assert.True(result.VertexCount > 0);
        Assert.True(result.EdgeCount > 0);
        Assert.True(result.FaceCount > 0);
    }
}
