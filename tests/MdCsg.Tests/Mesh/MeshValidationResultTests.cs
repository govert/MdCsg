using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: MeshValidationResult and Validate() comprehensive tests</summary>
public class MeshValidationResultTests
{
    [Fact]
    public void Validate_Cube_AllChecksPass()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var result = MeshValidator.Validate(mesh);
        Assert.True(result.AllEdgesHaveTwins);
        Assert.True(result.IsEdgeManifold);
        Assert.True(result.IsConsistentlyOriented);
        Assert.True(result.HasValidFaceCycles);
    }

    [Fact]
    public void Validate_Cube_IsClosedManifold()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var result = MeshValidator.Validate(mesh);
        Assert.True(result.IsClosedManifold);
    }

    [Fact]
    public void Validate_Cube_EulerCharacteristic2()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var result = MeshValidator.Validate(mesh);
        Assert.Equal(2, result.EulerCharacteristic);
    }

    [Fact]
    public void Validate_Cube_Counts()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var result = MeshValidator.Validate(mesh);
        Assert.Equal(8, result.VertexCount);
        Assert.Equal(18, result.EdgeCount); // 12 edges on cube * 3 = 36 half-edges → 18 edges... actually 12 faces * 3 half-edges / 2 = 18
        Assert.Equal(12, result.FaceCount);
    }

    [Fact]
    public void Validate_Tetrahedron_EulerCharacteristic2()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        var result = MeshValidator.Validate(mesh);
        Assert.Equal(2, result.EulerCharacteristic);
    }

    [Fact]
    public void Validate_Tetrahedron_Counts()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        var result = MeshValidator.Validate(mesh);
        Assert.Equal(4, result.VertexCount);
        Assert.Equal(6, result.EdgeCount); // 4 faces * 3 / 2 = 6
        Assert.Equal(4, result.FaceCount);
    }

    [Fact]
    public void Validate_Sphere_IsClosedManifold()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var result = MeshValidator.Validate(mesh);
        Assert.True(result.IsClosedManifold);
    }

    [Fact]
    public void Validate_Sphere_EulerCharacteristic2()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var result = MeshValidator.Validate(mesh);
        Assert.Equal(2, result.EulerCharacteristic);
    }

    [Fact]
    public void Validate_SingleTriangle_NotClosed()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(new Vec3(0, 0, 0));
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        mesh.AddFace(v0, v1, v2);
        var result = MeshValidator.Validate(mesh);
        Assert.False(result.AllEdgesHaveTwins);
        Assert.False(result.IsClosedManifold);
    }

    [Fact]
    public void Validate_SingleTriangle_HasValidFaceCycles()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(new Vec3(0, 0, 0));
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        mesh.AddFace(v0, v1, v2);
        var result = MeshValidator.Validate(mesh);
        Assert.True(result.HasValidFaceCycles);
    }

    [Fact]
    public void EulerCharacteristic_DirectCall()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        Assert.Equal(2, MeshValidator.EulerCharacteristic(mesh));
    }

    [Fact]
    public void AllEdgesHaveTwins_DirectCall()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        Assert.True(MeshValidator.AllEdgesHaveTwins(mesh));
    }

    [Fact]
    public void IsEdgeManifold_DirectCall()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        Assert.True(MeshValidator.IsEdgeManifold(mesh));
    }

    [Fact]
    public void IsConsistentlyOriented_DirectCall()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        Assert.True(MeshValidator.IsConsistentlyOriented(mesh));
    }

    [Fact]
    public void HasValidFaceCycles_DirectCall()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        Assert.True(MeshValidator.HasValidFaceCycles(mesh));
    }

    [Fact]
    public void Validate_HighSubdivSphere_StillValid()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 4).Mesh;
        var result = MeshValidator.Validate(mesh);
        Assert.True(result.IsClosedManifold);
        Assert.Equal(2, result.EulerCharacteristic);
    }

    [Fact]
    public void Validate_OffsetCube_StillValid()
    {
        var mesh = MeshFactory.CreateCube(new Vec3(100, 200, 300)).Mesh;
        var result = MeshValidator.Validate(mesh);
        Assert.True(result.IsClosedManifold);
    }
}
