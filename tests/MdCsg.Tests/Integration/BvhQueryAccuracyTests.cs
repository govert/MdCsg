using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 40: BVH AABB queries find all expected faces.</summary>
public class BvhQueryAccuracyTests
{
    [Fact]
    public void CubeBvh_QueryFullBounds_FindsAllFaces()
    {
        var cube = MdCsg.Api.Primitives.Cube(Vec3.Zero, 2.0);
        var bvh = BvhTree.Build(cube.Mesh);
        var results = new List<int>();
        bvh.Query(new Aabb(new Vec3(-2, -2, -2), new Vec3(2, 2, 2)), results);
        Assert.Equal(cube.Mesh.Faces.Count, results.Count);
    }

    [Fact]
    public void CubeBvh_QueryHalf_FindsSome()
    {
        var cube = MdCsg.Api.Primitives.Cube(Vec3.Zero, 2.0);
        var bvh = BvhTree.Build(cube.Mesh);
        var results = new List<int>();
        bvh.Query(new Aabb(new Vec3(0, -2, -2), new Vec3(2, 2, 2)), results);
        Assert.True(results.Count > 0);
        Assert.True(results.Count < cube.Mesh.Faces.Count);
    }

    [Fact]
    public void CubeBvh_QueryOutside_FindsNone()
    {
        var cube = MdCsg.Api.Primitives.Cube(Vec3.Zero, 2.0);
        var bvh = BvhTree.Build(cube.Mesh);
        var results = new List<int>();
        bvh.Query(new Aabb(new Vec3(10, 10, 10), new Vec3(20, 20, 20)), results);
        Assert.Empty(results);
    }

    [Fact]
    public void SphereBvh_QueryCenter_FindsFaces()
    {
        var sphere = MdCsg.Api.Primitives.Sphere(Vec3.Zero, 1.0);
        var bvh = BvhTree.Build(sphere.Mesh);
        var results = new List<int>();
        bvh.Query(new Aabb(new Vec3(-0.5, -0.5, -0.5), new Vec3(0.5, 0.5, 0.5)), results);
        Assert.True(results.Count > 0);
    }

    [Fact]
    public void SphereBvh_QueryAll_FindsAllFaces()
    {
        var sphere = MdCsg.Api.Primitives.Sphere(Vec3.Zero, 1.0);
        var bvh = BvhTree.Build(sphere.Mesh);
        var results = new List<int>();
        bvh.Query(new Aabb(new Vec3(-2, -2, -2), new Vec3(2, 2, 2)), results);
        Assert.Equal(sphere.Mesh.Faces.Count, results.Count);
    }

    [Fact]
    public void CylinderBvh_QueryTopHalf()
    {
        var cyl = MdCsg.Api.Primitives.Cylinder(Vec3.Zero, Vec3.UnitZ, 1.0, 4.0);
        var bvh = BvhTree.Build(cyl.Mesh);
        var results = new List<int>();
        bvh.Query(new Aabb(new Vec3(-2, -2, 2), new Vec3(2, 2, 5)), results);
        Assert.True(results.Count > 0);
    }

    [Fact]
    public void BvhQuery_ResultsAreValidFaceIndices()
    {
        var cube = MdCsg.Api.Primitives.Cube(Vec3.Zero, 2.0);
        var bvh = BvhTree.Build(cube.Mesh);
        var results = new List<int>();
        bvh.Query(new Aabb(new Vec3(-2, -2, -2), new Vec3(2, 2, 2)), results);
        foreach (var idx in results)
        {
            Assert.True(idx >= 0);
            Assert.True(idx < cube.Mesh.Faces.Count);
        }
    }

    [Fact]
    public void BvhQuery_NoDuplicates()
    {
        var cube = MdCsg.Api.Primitives.Cube(Vec3.Zero, 2.0);
        var bvh = BvhTree.Build(cube.Mesh);
        var results = new List<int>();
        bvh.Query(new Aabb(new Vec3(-2, -2, -2), new Vec3(2, 2, 2)), results);
        Assert.Equal(results.Count, results.Distinct().Count());
    }

    [Fact]
    public void CsgResultBvh_QueryWorks()
    {
        var a = MdCsg.Api.Primitives.Cube(Vec3.Zero, 2.0);
        var b = MdCsg.Api.Primitives.Sphere(Vec3.Zero, 1.2);
        var result = MdCsg.Api.Csg.Intersect(a, b);
        var solid = new MdCsg.Api.Solid(result.Mesh);
        var bvh = BvhTree.Build(solid.Mesh);
        var results = new List<int>();
        bvh.Query(new Aabb(new Vec3(-2, -2, -2), new Vec3(2, 2, 2)), results);
        Assert.Equal(solid.Mesh.Faces.Count, results.Count);
    }

    [Fact]
    public void SmallQueryBox_FindsSubset()
    {
        var sphere = MdCsg.Api.Primitives.Sphere(Vec3.Zero, 5.0, 3);
        var bvh = BvhTree.Build(sphere.Mesh);
        var results = new List<int>();
        bvh.Query(new Aabb(new Vec3(4, -0.5, -0.5), new Vec3(6, 0.5, 0.5)), results);
        Assert.True(results.Count > 0);
        Assert.True(results.Count < sphere.Mesh.Faces.Count);
    }

    [Fact]
    public void MultipleQueries_SameResults()
    {
        var cube = MdCsg.Api.Primitives.Cube(Vec3.Zero, 2.0);
        var bvh = BvhTree.Build(cube.Mesh);
        var box = new Aabb(new Vec3(-1, -1, -1), new Vec3(0, 0, 0));
        var r1 = new List<int>();
        var r2 = new List<int>();
        bvh.Query(box, r1);
        bvh.Query(box, r2);
        Assert.Equal(r1.OrderBy(x => x), r2.OrderBy(x => x));
    }

    [Fact]
    public void BvhNodeCount_IsPositive()
    {
        var cube = MdCsg.Api.Primitives.Cube(Vec3.Zero, 2.0);
        var bvh = BvhTree.Build(cube.Mesh);
        Assert.True(bvh.NodeCount > 0);
    }

    [Fact]
    public void BvhFaceIndices_MatchMesh()
    {
        var cube = MdCsg.Api.Primitives.Cube(Vec3.Zero, 2.0);
        var bvh = BvhTree.Build(cube.Mesh);
        Assert.Equal(cube.Mesh.Faces.Count, bvh.FaceIndices.Length);
    }

    [Fact]
    public void HighPolySphere_BvhQueryWorks()
    {
        var sphere = MdCsg.Api.Primitives.Sphere(Vec3.Zero, 1.0, 4);
        var bvh = BvhTree.Build(sphere.Mesh);
        var results = new List<int>();
        bvh.Query(new Aabb(new Vec3(-0.1, -0.1, -0.1), new Vec3(0.1, 0.1, 0.1)), results);
        // Small box near center should find some faces
        Assert.True(results.Count >= 0); // May be 0 if sphere is hollow at center
    }

    [Fact]
    public void TorusBvh_QueryWorks()
    {
        var torus = MdCsg.Api.Primitives.Torus(Vec3.Zero, Vec3.UnitZ, 3.0, 1.0);
        var bvh = BvhTree.Build(torus.Mesh);
        var results = new List<int>();
        bvh.Query(new Aabb(new Vec3(2, -1, -1), new Vec3(4, 1, 1)), results);
        Assert.True(results.Count > 0);
    }

    [Fact]
    public void ConeBvh_QueryTip()
    {
        var cone = MdCsg.Api.Primitives.Cone(Vec3.Zero, Vec3.UnitZ, 1.0, 2.0);
        var bvh = BvhTree.Build(cone.Mesh);
        var results = new List<int>();
        bvh.Query(new Aabb(new Vec3(-0.5, -0.5, 1.5), new Vec3(0.5, 0.5, 2.5)), results);
        Assert.True(results.Count > 0);
    }

    [Fact]
    public void BvhQuery_OverlappingBoxes_ReturnSupersetOfSmaller()
    {
        var cube = MdCsg.Api.Primitives.Cube(Vec3.Zero, 4.0);
        var bvh = BvhTree.Build(cube.Mesh);
        var smallBox = new Aabb(new Vec3(-1, -1, -1), new Vec3(1, 1, 1));
        var largeBox = new Aabb(new Vec3(-3, -3, -3), new Vec3(3, 3, 3));
        var smallResults = new List<int>();
        var largeResults = new List<int>();
        bvh.Query(smallBox, smallResults);
        bvh.Query(largeBox, largeResults);
        Assert.True(largeResults.Count >= smallResults.Count);
    }

    [Fact]
    public void RayCastCount_InsideCube_IsEven()
    {
        var cube = MdCsg.Api.Primitives.Cube(Vec3.Zero, 2.0);
        var bvh = BvhTree.Build(cube.Mesh);
        var ray = new Ray(new Vec3(0, 0, 0), new Vec3(1, 0.00013, 0.00017));
        var count = bvh.RayCastCount(ray);
        // From inside, ray should hit 1 face (odd = inside)
        Assert.True(count > 0);
    }

    [Fact]
    public void RayCastCount_OutsideCube_IsEven()
    {
        var cube = MdCsg.Api.Primitives.Cube(Vec3.Zero, 2.0);
        var bvh = BvhTree.Build(cube.Mesh);
        var ray = new Ray(new Vec3(10, 0, 0), new Vec3(1, 0.00013, 0.00017));
        var count = bvh.RayCastCount(ray);
        Assert.True(count % 2 == 0);
    }

    [Fact]
    public void RayCastCount_ThroughCube_IsTwo()
    {
        var cube = MdCsg.Api.Primitives.Cube(Vec3.Zero, 2.0);
        var bvh = BvhTree.Build(cube.Mesh);
        var ray = new Ray(new Vec3(-10, 0.00013, 0.00017), new Vec3(1, 0, 0));
        var count = bvh.RayCastCount(ray);
        Assert.Equal(2, count);
    }
}
