using MdCsg.Math;
using MdCsg.Spatial;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 34: Octree 10k+ points, sphere queries.</summary>
public class OctreeStressTests
{
    [Fact]
    public void Insert_SingleItem_CanQuery()
    {
        var octree = new Octree<int>(new Aabb(new Vec3(-10, -10, -10), new Vec3(10, 10, 10)));
        octree.Insert(Vec3.Zero, 42);
        var results = octree.QuerySphere(Vec3.Zero, 1.0);
        Assert.Contains(42, results);
    }

    [Fact]
    public void Insert1000Points_AllQueryable()
    {
        var bounds = new Aabb(new Vec3(-50, -50, -50), new Vec3(50, 50, 50));
        var octree = new Octree<int>(bounds);
        var rng = new Random(42);
        for (int i = 0; i < 1000; i++)
            octree.Insert(new Vec3(rng.NextDouble() * 100 - 50, rng.NextDouble() * 100 - 50, rng.NextDouble() * 100 - 50), i);
        var results = octree.QuerySphere(Vec3.Zero, 10.0);
        Assert.True(results.Count > 0);
    }

    [Fact]
    public void Insert10000Points_NoError()
    {
        var bounds = new Aabb(new Vec3(-100, -100, -100), new Vec3(100, 100, 100));
        var octree = new Octree<int>(bounds);
        var rng = new Random(42);
        for (int i = 0; i < 10000; i++)
            octree.Insert(new Vec3(rng.NextDouble() * 200 - 100, rng.NextDouble() * 200 - 100, rng.NextDouble() * 200 - 100), i);
        var results = octree.QuerySphere(Vec3.Zero, 20.0);
        Assert.True(results.Count > 0);
    }

    [Fact]
    public void SphereQuery_NoResults_OutOfBounds()
    {
        var bounds = new Aabb(new Vec3(-10, -10, -10), new Vec3(10, 10, 10));
        var octree = new Octree<int>(bounds);
        octree.Insert(Vec3.Zero, 42);
        var results = octree.QuerySphere(new Vec3(100, 0, 0), 1.0);
        Assert.Empty(results);
    }

    [Fact]
    public void AabbQuery_FindsItems()
    {
        var bounds = new Aabb(new Vec3(-10, -10, -10), new Vec3(10, 10, 10));
        var octree = new Octree<int>(bounds);
        octree.Insert(Vec3.Zero, 42);
        var results = octree.Query(new Aabb(new Vec3(-1, -1, -1), new Vec3(1, 1, 1)));
        Assert.Contains(42, results);
    }

    [Fact]
    public void AabbQuery_ExcludesDistantItems()
    {
        var bounds = new Aabb(new Vec3(-10, -10, -10), new Vec3(10, 10, 10));
        var octree = new Octree<int>(bounds);
        octree.Insert(new Vec3(9, 9, 9), 42);
        var results = octree.Query(new Aabb(new Vec3(-1, -1, -1), new Vec3(1, 1, 1)));
        Assert.DoesNotContain(42, results);
    }

    [Fact]
    public void ClusteredPoints_CorrectlyFound()
    {
        var bounds = new Aabb(new Vec3(-50, -50, -50), new Vec3(50, 50, 50));
        var octree = new Octree<int>(bounds);
        // Insert cluster near origin
        var rng = new Random(42);
        for (int i = 0; i < 100; i++)
            octree.Insert(new Vec3(rng.NextDouble() * 2 - 1, rng.NextDouble() * 2 - 1, rng.NextDouble() * 2 - 1), i);
        // Insert far-away points
        for (int i = 100; i < 200; i++)
            octree.Insert(new Vec3(rng.NextDouble() * 2 + 40, rng.NextDouble() * 2 + 40, rng.NextDouble() * 2 + 40), i);

        var nearOrigin = octree.QuerySphere(Vec3.Zero, 3.0);
        Assert.True(nearOrigin.Count >= 50);
    }

    [Fact]
    public void MaxDepth_Respected()
    {
        var bounds = new Aabb(new Vec3(-10, -10, -10), new Vec3(10, 10, 10));
        var octree = new Octree<int>(bounds, maxDepth: 4);
        for (int i = 0; i < 1000; i++)
            octree.Insert(Vec3.Zero, i); // All at same location
        var results = octree.QuerySphere(Vec3.Zero, 0.1);
        Assert.Equal(1000, results.Count);
    }

    [Fact]
    public void SphereQuery_LargeRadius_FindsAll()
    {
        var bounds = new Aabb(new Vec3(-10, -10, -10), new Vec3(10, 10, 10));
        var octree = new Octree<int>(bounds);
        for (int i = 0; i < 50; i++)
            octree.Insert(new Vec3(i * 0.1 - 2.5, 0, 0), i);
        var results = octree.QuerySphere(Vec3.Zero, 100.0);
        Assert.Equal(50, results.Count);
    }

    [Fact]
    public void SphereQuery_SmallRadius_FindsFew()
    {
        var bounds = new Aabb(new Vec3(-10, -10, -10), new Vec3(10, 10, 10));
        var octree = new Octree<int>(bounds);
        var rng = new Random(42);
        for (int i = 0; i < 1000; i++)
            octree.Insert(new Vec3(rng.NextDouble() * 20 - 10, rng.NextDouble() * 20 - 10, rng.NextDouble() * 20 - 10), i);
        var results = octree.QuerySphere(Vec3.Zero, 0.5);
        Assert.True(results.Count < 1000);
    }

    [Fact]
    public void StringItems_Work()
    {
        var bounds = new Aabb(new Vec3(-10, -10, -10), new Vec3(10, 10, 10));
        var octree = new Octree<string>(bounds);
        octree.Insert(new Vec3(1, 2, 3), "hello");
        var results = octree.QuerySphere(new Vec3(1, 2, 3), 1.0);
        Assert.Contains("hello", results);
    }

    [Fact]
    public void Bounds_Property()
    {
        var bounds = new Aabb(new Vec3(-5, -5, -5), new Vec3(5, 5, 5));
        var octree = new Octree<int>(bounds);
        Assert.Equal(bounds, octree.Bounds);
    }

    [Fact]
    public void NegativeCoordinates_Work()
    {
        var bounds = new Aabb(new Vec3(-100, -100, -100), new Vec3(100, 100, 100));
        var octree = new Octree<int>(bounds);
        octree.Insert(new Vec3(-50, -50, -50), 99);
        var results = octree.QuerySphere(new Vec3(-50, -50, -50), 1.0);
        Assert.Contains(99, results);
    }

    [Fact]
    public void BoundaryPoints_Found()
    {
        var bounds = new Aabb(new Vec3(-10, -10, -10), new Vec3(10, 10, 10));
        var octree = new Octree<int>(bounds);
        octree.Insert(new Vec3(10, 10, 10), 42);
        var results = octree.QuerySphere(new Vec3(10, 10, 10), 0.5);
        Assert.Contains(42, results);
    }

    [Fact]
    public void EmptyOctree_QueryReturnsEmpty()
    {
        var bounds = new Aabb(new Vec3(-10, -10, -10), new Vec3(10, 10, 10));
        var octree = new Octree<int>(bounds);
        var results = octree.QuerySphere(Vec3.Zero, 100.0);
        Assert.Empty(results);
    }

    [Fact]
    public void ManyItemsAtSameLocation_AllFound()
    {
        var bounds = new Aabb(new Vec3(-10, -10, -10), new Vec3(10, 10, 10));
        var octree = new Octree<int>(bounds);
        for (int i = 0; i < 200; i++)
            octree.Insert(Vec3.Zero, i);
        var results = octree.QuerySphere(Vec3.Zero, 0.1);
        Assert.Equal(200, results.Count);
    }

    [Fact]
    public void Insert15000Points_SphereQueryWorks()
    {
        var bounds = new Aabb(new Vec3(-100, -100, -100), new Vec3(100, 100, 100));
        var octree = new Octree<int>(bounds);
        var rng = new Random(42);
        for (int i = 0; i < 15000; i++)
            octree.Insert(new Vec3(rng.NextDouble() * 200 - 100, rng.NextDouble() * 200 - 100, rng.NextDouble() * 200 - 100), i);
        // Radius 5 can be empty for this deterministic random set; use a robust query radius.
        var results = octree.QuerySphere(Vec3.Zero, 10.0);
        Assert.True(results.Count > 0);
    }

    [Fact]
    public void AabbQuery_CornerRegion()
    {
        var bounds = new Aabb(new Vec3(-10, -10, -10), new Vec3(10, 10, 10));
        var octree = new Octree<int>(bounds);
        octree.Insert(new Vec3(8, 8, 8), 42);
        octree.Insert(new Vec3(-8, -8, -8), 43);
        var results = octree.Query(new Aabb(new Vec3(7, 7, 7), new Vec3(10, 10, 10)));
        Assert.Contains(42, results);
        Assert.DoesNotContain(43, results);
    }
}
