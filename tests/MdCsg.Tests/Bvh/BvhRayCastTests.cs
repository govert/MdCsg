using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Bvh;

/// <summary>Phase 6: BvhTree.RayCastCount — ray-mesh intersection counting for inside/outside</summary>
public class BvhRayCastTests
{
    [Fact]
    public void RayFromOutside_ThroughCube_EvenHits()
    {
        var solid = MeshFactory.CreateCube();
        // Perturbed to avoid edge hits; each face has 2 triangles, so may hit 2 per face
        var ray = new Ray(new Vec3(-5, 0.5, 0.5), new Vec3(1, 0, 0));
        int count = solid.Bvh.RayCastCount(ray);
        Assert.True(count > 0 && count % 2 == 0, $"Outside-through hits should be even, got {count}");
    }

    [Fact]
    public void RayFromInside_Cube_OneHit()
    {
        var solid = MeshFactory.CreateCube();
        // Origin inside the unit cube [0,1]^3
        var ray = new Ray(new Vec3(0.5, 0.5, 0.5), new Vec3(1, 0.00013, 0.00017));
        int count = solid.Bvh.RayCastCount(ray);
        Assert.Equal(1, count); // only exit
    }

    [Fact]
    public void RayMissing_Cube_ZeroHits()
    {
        var solid = MeshFactory.CreateCube();
        var ray = new Ray(new Vec3(-5, 5, 5), new Vec3(1, 0, 0));
        int count = solid.Bvh.RayCastCount(ray);
        Assert.Equal(0, count);
    }

    [Fact]
    public void RayFromOutside_ThroughSphere_TwoHits()
    {
        var solid = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var ray = new Ray(new Vec3(-5, 0, 0), new Vec3(1, 0.00013, 0.00017));
        int count = solid.Bvh.RayCastCount(ray);
        Assert.Equal(2, count);
    }

    [Fact]
    public void RayFromInside_Sphere_OddHits()
    {
        var solid = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var ray = new Ray(Vec3.Zero, new Vec3(1, 0.00013, 0.00017));
        int count = solid.Bvh.RayCastCount(ray);
        Assert.True(count % 2 == 1, $"Inside sphere should have odd hit count, got {count}");
    }

    [Fact]
    public void RayFromOutside_Sphere_EvenHits()
    {
        var solid = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var ray = new Ray(new Vec3(-5, 0, 0), new Vec3(1, 0.00013, 0.00017));
        int count = solid.Bvh.RayCastCount(ray);
        Assert.True(count % 2 == 0, $"Outside sphere should have even hit count, got {count}");
    }

    [Fact]
    public void EmptyBvh_ZeroHits()
    {
        var builder = new MdCsg.Mesh.MeshBuilder();
        var mesh = builder.Build(System.Array.Empty<Triangle3>());
        var bvh = BvhTree.Build(mesh);
        var ray = new Ray(Vec3.Zero, new Vec3(1, 0, 0));
        Assert.Equal(0, bvh.RayCastCount(ray));
    }

    [Fact]
    public void Tetrahedron_RayThroughCenter_TwoHits()
    {
        var solid = MeshFactory.CreateTetrahedron();
        var centroid = new Vec3(0.25, 0.25, 0.25); // rough center
        var ray = new Ray(new Vec3(-5, 0.25, 0.25), new Vec3(1, 0.00013, 0.00017));
        int count = solid.Bvh.RayCastCount(ray);
        Assert.Equal(2, count);
    }

    [Fact]
    public void RayAlongAxis_X()
    {
        var solid = MeshFactory.CreateCube();
        var ray = new Ray(new Vec3(-5, 0.5, 0.5), new Vec3(1, 0, 0));
        Assert.True(solid.Bvh.RayCastCount(ray) >= 2);
    }

    [Fact]
    public void RayAlongAxis_Y()
    {
        var solid = MeshFactory.CreateCube();
        var ray = new Ray(new Vec3(0.5, -5, 0.5), new Vec3(0, 1, 0));
        Assert.True(solid.Bvh.RayCastCount(ray) >= 2);
    }

    [Fact]
    public void RayAlongAxis_Z()
    {
        var solid = MeshFactory.CreateCube();
        var ray = new Ray(new Vec3(0.5, 0.5, -5), new Vec3(0, 0, 1));
        Assert.True(solid.Bvh.RayCastCount(ray) >= 2);
    }

    [Fact]
    public void Query_CubeIntersectsOwnBounds()
    {
        var solid = MeshFactory.CreateCube();
        var results = new List<int>();
        solid.Bvh.Query(solid.Bounds, results);
        Assert.Equal(12, results.Count); // cube has 12 faces
    }

    [Fact]
    public void Query_EmptyBox_NoResults()
    {
        var solid = MeshFactory.CreateCube();
        var results = new List<int>();
        var farBox = new Aabb(new Vec3(100, 100, 100), new Vec3(101, 101, 101));
        solid.Bvh.Query(farBox, results);
        Assert.Empty(results);
    }
}
