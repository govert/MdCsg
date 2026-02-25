using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Bvh;

/// <summary>Phase 6: BVH ray-cast — RayCastCount for rays going through and missing meshes</summary>
public class BvhRayCastPropertyTests
{
    [Fact]
    public void RayCastCount_ThroughCube_Even()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var ray = new Ray(new Vec3(-5, 0, 0), new Vec3(1, 0, 0));
        int count = cube.Bvh.RayCastCount(ray);
        Assert.True(count % 2 == 0);
        Assert.True(count >= 2); // enters and exits
    }

    [Fact]
    public void RayCastCount_MissingCube_Zero()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var ray = new Ray(new Vec3(-5, 10, 0), new Vec3(1, 0, 0));
        int count = cube.Bvh.RayCastCount(ray);
        Assert.Equal(0, count);
    }

    [Fact]
    public void RayCastCount_ThroughSphere_Even()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var ray = new Ray(new Vec3(-5, 0, 0), new Vec3(1, 0, 0));
        int count = sphere.Bvh.RayCastCount(ray);
        Assert.True(count % 2 == 0);
        Assert.True(count >= 2);
    }

    [Fact]
    public void RayCastCount_MissingSphere_Zero()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var ray = new Ray(new Vec3(-5, 5, 0), new Vec3(1, 0, 0));
        Assert.Equal(0, sphere.Bvh.RayCastCount(ray));
    }

    [Fact]
    public void RayCastCount_InsideCube_Odd()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var ray = new Ray(Vec3.Zero, new Vec3(1, 0.00013, 0.00017));
        int count = cube.Bvh.RayCastCount(ray);
        Assert.True(count % 2 == 1); // inside → odd crossings
    }

    [Fact]
    public void RayCastCount_OutsideCube_Even()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var ray = new Ray(new Vec3(5, 0, 0), new Vec3(1, 0.00013, 0.00017));
        int count = cube.Bvh.RayCastCount(ray);
        Assert.True(count % 2 == 0);
    }

    [Fact]
    public void RayCastCount_AlongY_ThroughCube()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var ray = new Ray(new Vec3(0, -5, 0), new Vec3(0, 1, 0));
        int count = cube.Bvh.RayCastCount(ray);
        Assert.True(count >= 2);
    }

    [Fact]
    public void RayCastCount_AlongZ_ThroughCube()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var ray = new Ray(new Vec3(0, 0, -5), new Vec3(0, 0, 1));
        int count = cube.Bvh.RayCastCount(ray);
        Assert.True(count >= 2);
    }

    [Fact]
    public void RayCastCount_DiagonalRay_ThroughCube()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var dir = new Vec3(1, 1, 1).Normalized;
        var ray = new Ray(new Vec3(-5, -5, -5), dir);
        int count = cube.Bvh.RayCastCount(ray);
        Assert.True(count >= 2);
    }

    [Fact]
    public void RayCastCount_Tetrahedron_FromOutside_Even()
    {
        var tet = MeshFactory.CreateTetrahedron();
        var ray = new Ray(new Vec3(-5, 0.2, 0.1), new Vec3(1, 0.00013, 0.00017));
        int count = tet.Bvh.RayCastCount(ray);
        // From outside: expect 0 (miss) or 2 (through), both even
        Assert.True(count % 2 == 0);
    }

    [Fact]
    public void RayCastCount_EmptyBvh_Zero()
    {
        var empty = MdCsg.Api.Solid.FromTriangles(Array.Empty<Triangle3>());
        var ray = new Ray(Vec3.Zero, Vec3.UnitX);
        Assert.Equal(0, empty.Bvh.RayCastCount(ray));
    }
}
