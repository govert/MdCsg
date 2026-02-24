using MdCsg.Mesh;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Batch 36: MeshValidator unit tests (20 tests)</summary>
public class MeshValidatorTests
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
    public void Cube_HasValidFaceCycles()
    {
        var cube = MeshFactory.CreateCube();
        Assert.True(MeshValidator.HasValidFaceCycles(cube.Mesh));
    }

    [Fact]
    public void Cube_EulerCharacteristic_Is2()
    {
        var cube = MeshFactory.CreateCube();
        Assert.Equal(2, MeshValidator.EulerCharacteristic(cube.Mesh));
    }

    [Fact]
    public void Cube_Validate_AllPassForClosedManifold()
    {
        var cube = MeshFactory.CreateCube();
        var result = MeshValidator.Validate(cube.Mesh);
        Assert.True(result.IsClosedManifold);
    }

    [Fact]
    public void Cube_Validate_CorrectCounts()
    {
        var cube = MeshFactory.CreateCube();
        var result = MeshValidator.Validate(cube.Mesh);
        Assert.Equal(8, result.VertexCount);
        Assert.Equal(18, result.EdgeCount); // 36 half-edges / 2
        Assert.Equal(12, result.FaceCount);
    }

    [Fact]
    public void Tetrahedron_AllEdgesHaveTwins()
    {
        var tet = MeshFactory.CreateTetrahedron();
        Assert.True(MeshValidator.AllEdgesHaveTwins(tet.Mesh));
    }

    [Fact]
    public void Tetrahedron_IsEdgeManifold()
    {
        var tet = MeshFactory.CreateTetrahedron();
        Assert.True(MeshValidator.IsEdgeManifold(tet.Mesh));
    }

    [Fact]
    public void Tetrahedron_EulerCharacteristic_Is2()
    {
        var tet = MeshFactory.CreateTetrahedron();
        Assert.Equal(2, MeshValidator.EulerCharacteristic(tet.Mesh));
    }

    [Fact]
    public void Tetrahedron_Validate_IsClosedManifold()
    {
        var tet = MeshFactory.CreateTetrahedron();
        var result = MeshValidator.Validate(tet.Mesh);
        Assert.True(result.IsClosedManifold);
    }

    [Fact]
    public void Tetrahedron_Validate_Counts()
    {
        var tet = MeshFactory.CreateTetrahedron();
        var result = MeshValidator.Validate(tet.Mesh);
        Assert.Equal(4, result.VertexCount);
        Assert.Equal(4, result.FaceCount);
    }

    [Fact]
    public void Sphere_Sub1_IsClosedManifold()
    {
        var sphere = MeshFactory.CreateSphere(subdivisions: 1);
        var result = MeshValidator.Validate(sphere.Mesh);
        Assert.True(result.IsClosedManifold);
    }

    [Fact]
    public void Sphere_Sub2_EulerCharacteristic_Is2()
    {
        var sphere = MeshFactory.CreateSphere(subdivisions: 2);
        Assert.Equal(2, MeshValidator.EulerCharacteristic(sphere.Mesh));
    }

    [Fact]
    public void Sphere_Sub1_HasValidFaceCycles()
    {
        var sphere = MeshFactory.CreateSphere(subdivisions: 1);
        Assert.True(MeshValidator.HasValidFaceCycles(sphere.Mesh));
    }

    [Fact]
    public void Sphere_Sub2_IsConsistentlyOriented()
    {
        var sphere = MeshFactory.CreateSphere(subdivisions: 2);
        Assert.True(MeshValidator.IsConsistentlyOriented(sphere.Mesh));
    }

    [Fact]
    public void Cube_Size2_IsClosedManifold()
    {
        var cube = MeshFactory.CreateCube(size: 2);
        var result = MeshValidator.Validate(cube.Mesh);
        Assert.True(result.IsClosedManifold);
    }

    [Fact]
    public void OffsetCube_IsClosedManifold()
    {
        var cube = MeshFactory.CreateCube(new Vec3(10, 20, 30));
        var result = MeshValidator.Validate(cube.Mesh);
        Assert.True(result.IsClosedManifold);
    }

    [Fact]
    public void Cube_TwinSymmetry()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var he in cube.Mesh.HalfEdges)
        {
            Assert.NotNull(he.Twin);
            Assert.Equal(he, he.Twin.Twin);
        }
    }

    [Fact]
    public void Cube_NextPrevConsistency()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var he in cube.Mesh.HalfEdges)
        {
            Assert.Equal(he, he.Next.Prev);
            Assert.Equal(he, he.Prev.Next);
        }
    }
}
