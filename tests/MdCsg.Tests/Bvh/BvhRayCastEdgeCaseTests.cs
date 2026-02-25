using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Bvh;

/// <summary>Phase 6: BVH ray cast edge case tests — parallel rays, degenerate cases, large offsets</summary>
public class BvhRayCastEdgeCaseTests
{
    [Fact]
    public void RayCastCount_ThroughCube_EvenHits()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        // Ray from outside through center — hits triangulated faces (may hit 2+ per face)
        var ray = new Ray(new Vec3(-1, 0.5, 0.5), new Vec3(1, 0, 0));
        int count = bvh.RayCastCount(ray);
        Assert.True(count % 2 == 0 && count >= 2, $"Expected even hits >= 2, got {count}");
    }

    [Fact]
    public void RayCastCount_MissCube_ZeroHits()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        // Ray parallel to cube, missing it
        var ray = new Ray(new Vec3(-1, 2, 2), new Vec3(1, 0, 0));
        int count = bvh.RayCastCount(ray);
        Assert.Equal(0, count);
    }

    [Fact]
    public void RayCastCount_FromInside_OddHits()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        // Ray from inside cube
        var ray = new Ray(new Vec3(0.5, 0.5, 0.5), new Vec3(1, 0.00013, 0.00017).Normalized);
        int count = bvh.RayCastCount(ray);
        Assert.True(count % 2 == 1, $"Expected odd hits from inside, got {count}");
    }

    [Fact]
    public void RayCastCount_AwayFromCube_ZeroHits()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        // Ray going away from cube
        var ray = new Ray(new Vec3(-1, 0.5, 0.5), new Vec3(-1, 0, 0));
        int count = bvh.RayCastCount(ray);
        Assert.Equal(0, count);
    }

    [Theory]
    [InlineData(0, 0.5, 0.5)]  // X-axis
    [InlineData(0.5, 0, 0.5)]  // Y-axis
    [InlineData(0.5, 0.5, 0)]  // Z-axis
    public void RayCastCount_AlongAxes_TwoHits(double ox, double oy, double oz)
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var dir = new Vec3(
            ox == 0 ? 1 : 0.00013,
            oy == 0 ? 1 : 0.00017,
            oz == 0 ? 1 : 0.00019).Normalized;
        var origin = new Vec3(ox == 0 ? -1 : ox, oy == 0 ? -1 : oy, oz == 0 ? -1 : oz);
        var ray = new Ray(origin, dir);
        int count = bvh.RayCastCount(ray);
        Assert.True(count >= 2, $"Expected at least 2 hits for axis-aligned ray, got {count}");
    }

    [Fact]
    public void RayCastCount_Sphere_TwoHits()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        var ray = new Ray(new Vec3(-2, 0, 0), new Vec3(1, 0.001, 0.001).Normalized);
        int count = bvh.RayCastCount(ray);
        Assert.Equal(2, count);
    }

    [Fact]
    public void RayCastCount_Sphere_MissAbove()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        var ray = new Ray(new Vec3(-2, 5, 0), new Vec3(1, 0, 0));
        int count = bvh.RayCastCount(ray);
        Assert.Equal(0, count);
    }

    [Fact]
    public void RayCastCount_OffsetCube_StillWorks()
    {
        var mesh = MeshFactory.CreateCube(new Vec3(100, 200, 300)).Mesh;
        var bvh = BvhTree.Build(mesh);
        var ray = new Ray(new Vec3(99, 200.5, 300.5), new Vec3(1, 0.00013, 0.00017).Normalized);
        int count = bvh.RayCastCount(ray);
        Assert.Equal(2, count);
    }

    [Fact]
    public void RayCastCount_DiagonalRay_EvenHits()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        // Diagonal ray through cube — hits triangulated faces
        var ray = new Ray(new Vec3(-1, -1, -1), new Vec3(1, 1, 1).Normalized);
        int count = bvh.RayCastCount(ray);
        Assert.True(count % 2 == 0 && count >= 2, $"Expected even hits >= 2, got {count}");
    }

    [Fact]
    public void RayCastCount_PerturbedDirections_SameResult()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var origin = new Vec3(-1, 0.5, 0.5);
        // Two slightly different directions
        var ray1 = new Ray(origin, new Vec3(1, 0.001, 0.002).Normalized);
        var ray2 = new Ray(origin, new Vec3(1, -0.001, 0.001).Normalized);
        Assert.Equal(bvh.RayCastCount(ray1), bvh.RayCastCount(ray2));
    }

    [Fact]
    public void RayCastCount_FromInsideSphere_OddHits()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        var ray = new Ray(Vec3.Zero, new Vec3(1, 0.001, 0.002).Normalized);
        int count = bvh.RayCastCount(ray);
        Assert.True(count % 2 == 1, $"Expected odd from inside sphere, got {count}");
    }

    [Fact]
    public void Query_EmptyResult_ForNonOverlappingBox()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var queryBox = new Aabb(new Vec3(5, 5, 5), new Vec3(6, 6, 6));
        var results = new List<int>();
        bvh.Query(queryBox, results);
        Assert.Empty(results);
    }

    [Fact]
    public void Query_AllFaces_ForEncompassingBox()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var queryBox = new Aabb(new Vec3(-1, -1, -1), new Vec3(2, 2, 2));
        var results = new List<int>();
        bvh.Query(queryBox, results);
        Assert.Equal(mesh.Faces.Count, results.Count);
    }

    [Fact]
    public void Query_PartialOverlap_SomeFaces()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        // Small box that overlaps only part of the cube
        var queryBox = new Aabb(new Vec3(-0.5, -0.5, -0.5), new Vec3(0.1, 0.1, 0.1));
        var results = new List<int>();
        bvh.Query(queryBox, results);
        Assert.True(results.Count > 0 && results.Count < mesh.Faces.Count);
    }
}
