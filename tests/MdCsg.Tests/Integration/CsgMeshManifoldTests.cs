using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG result mesh manifold properties — closed, oriented, Euler characteristic</summary>
public class CsgMeshManifoldTests
{
    [Fact]
    public void Union_HasValidFaceCycles()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(MeshValidator.HasValidFaceCycles(result.Mesh));
    }

    [Fact]
    public void Intersection_HasValidFaceCycles()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.True(MeshValidator.HasValidFaceCycles(result.Mesh));
    }

    [Fact]
    public void Difference_HasValidFaceCycles()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var result = Csg.Difference(a, b);
        Assert.True(MeshValidator.HasValidFaceCycles(result.Mesh));
    }

    [Fact]
    public void Union_EulerCharacteristic_Is2()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        int euler = MeshValidator.EulerCharacteristic(result.Mesh);
        Assert.Equal(2, euler);
    }

    [Fact]
    public void Intersection_EulerCharacteristic_Is2()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var result = Csg.Intersect(a, b);
        int euler = MeshValidator.EulerCharacteristic(result.Mesh);
        Assert.Equal(2, euler);
    }

    [Fact]
    public void Difference_EulerCharacteristic_Is2()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var result = Csg.Difference(a, b);
        int euler = MeshValidator.EulerCharacteristic(result.Mesh);
        Assert.Equal(2, euler);
    }

    [Fact]
    public void Union_SphereSphere_HasFaces()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 2).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
        Assert.True(MeshValidator.HasValidFaceCycles(result.Mesh));
    }

    [Fact]
    public void Union_AllEdgesHaveTwins()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(MeshValidator.AllEdgesHaveTwins(result.Mesh));
    }

    [Fact]
    public void Intersection_AllEdgesHaveTwins()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.True(MeshValidator.AllEdgesHaveTwins(result.Mesh));
    }

    [Fact]
    public void Difference_AllEdgesHaveTwins()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var result = Csg.Difference(a, b);
        Assert.True(MeshValidator.AllEdgesHaveTwins(result.Mesh));
    }

    [Fact]
    public void Union_IsEdgeManifold()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(MeshValidator.IsEdgeManifold(result.Mesh));
    }

    [Fact]
    public void Union_IsConsistentlyOriented()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(MeshValidator.IsConsistentlyOriented(result.Mesh));
    }

    [Fact]
    public void Cube_IsClosedManifold()
    {
        var cube = MeshFactory.CreateCube();
        var validation = MeshValidator.Validate(cube.Mesh);
        Assert.True(validation.IsClosedManifold);
    }

    [Fact]
    public void Sphere_IsClosedManifold()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var validation = MeshValidator.Validate(sphere.Mesh);
        Assert.True(validation.IsClosedManifold);
    }

    [Fact]
    public void CsgUnion_SphereCube_HasValidFaceCycles()
    {
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 2).Mesh);
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var result = Csg.Union(sphere, cube);
        Assert.True(MeshValidator.HasValidFaceCycles(result.Mesh));
    }
}
