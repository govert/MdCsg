using MdCsg.Math;
using MdCsg.Spatial;

namespace MdCsg.Tests.Spatial;

public class OctreeTests
{
    private static Aabb UnitBounds => new(new Vec3(-10, -10, -10), new Vec3(10, 10, 10));

    // ── Insert & AABB Query ─────────────────────────────────────────

    [Fact]
    public void Insert_SingleItem_QueryFindsIt()
    {
        var tree = new Octree<int>(UnitBounds);
        tree.Insert(Vec3.Zero, 42);
        var result = tree.Query(new Aabb(new Vec3(-1, -1, -1), new Vec3(1, 1, 1)));
        Assert.Contains(42, result);
    }

    [Fact]
    public void Query_EmptyTree_ReturnsEmpty()
    {
        var tree = new Octree<int>(UnitBounds);
        var result = tree.Query(new Aabb(new Vec3(-1, -1, -1), new Vec3(1, 1, 1)));
        Assert.Empty(result);
    }

    [Fact]
    public void Query_ItemOutsideBounds_NotReturned()
    {
        var tree = new Octree<int>(UnitBounds);
        tree.Insert(new Vec3(5, 5, 5), 1);
        var result = tree.Query(new Aabb(new Vec3(-1, -1, -1), new Vec3(1, 1, 1)));
        Assert.Empty(result);
    }

    [Fact]
    public void Query_MultipleItems_ReturnsCorrectSubset()
    {
        var tree = new Octree<int>(UnitBounds);
        tree.Insert(new Vec3(0, 0, 0), 1);
        tree.Insert(new Vec3(0.5, 0.5, 0.5), 2);
        tree.Insert(new Vec3(5, 5, 5), 3);

        var result = tree.Query(new Aabb(new Vec3(-1, -1, -1), new Vec3(1, 1, 1)));
        Assert.Equal(2, result.Count);
        Assert.Contains(1, result);
        Assert.Contains(2, result);
    }

    // ── Sphere Query ────────────────────────────────────────────────

    [Fact]
    public void QuerySphere_FindsItemsWithinRadius()
    {
        var tree = new Octree<int>(UnitBounds);
        tree.Insert(new Vec3(0.5, 0, 0), 1);
        tree.Insert(new Vec3(5, 0, 0), 2);

        var result = tree.QuerySphere(Vec3.Zero, 1.0);
        Assert.Single(result);
        Assert.Contains(1, result);
    }

    [Fact]
    public void QuerySphere_EmptyTree_ReturnsEmpty()
    {
        var tree = new Octree<int>(UnitBounds);
        var result = tree.QuerySphere(Vec3.Zero, 10);
        Assert.Empty(result);
    }

    [Fact]
    public void QuerySphere_DiagonalDistance()
    {
        var tree = new Octree<int>(UnitBounds);
        tree.Insert(new Vec3(1, 1, 1), 1); // distance = sqrt(3) ≈ 1.73

        var within2 = tree.QuerySphere(Vec3.Zero, 2.0);
        var within1 = tree.QuerySphere(Vec3.Zero, 1.0);

        Assert.Single(within2);
        Assert.Empty(within1);
    }

    // ── Subdivision ─────────────────────────────────────────────────

    [Fact]
    public void Insert_ManyItems_SubdividesCorrectly()
    {
        var tree = new Octree<int>(UnitBounds, maxDepth: 4, maxItems: 4);
        var rng = new Random(42);

        for (int i = 0; i < 100; i++)
        {
            var pos = new Vec3(
                rng.NextDouble() * 20 - 10,
                rng.NextDouble() * 20 - 10,
                rng.NextDouble() * 20 - 10);
            tree.Insert(pos, i);
        }

        // Query entire space — should find all items
        var result = tree.Query(UnitBounds);
        Assert.Equal(100, result.Count);
    }

    [Fact]
    public void Insert_MaxDepth_StopsSubdividing()
    {
        var tree = new Octree<int>(UnitBounds, maxDepth: 1, maxItems: 2);

        // Insert many items at the same position
        for (int i = 0; i < 100; i++)
            tree.Insert(Vec3.Zero, i);

        var result = tree.Query(new Aabb(new Vec3(-1, -1, -1), new Vec3(1, 1, 1)));
        Assert.Equal(100, result.Count);
    }

    // ── Boundary items ──────────────────────────────────────────────

    [Fact]
    public void Insert_AtBounds_IsRetrievable()
    {
        var bounds = new Aabb(Vec3.Zero, new Vec3(10, 10, 10));
        var tree = new Octree<int>(bounds);

        tree.Insert(Vec3.Zero, 1);
        tree.Insert(new Vec3(10, 10, 10), 2);

        var result = tree.Query(bounds);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Insert_OutsideBounds_Ignored()
    {
        var bounds = new Aabb(Vec3.Zero, new Vec3(10, 10, 10));
        var tree = new Octree<int>(bounds);

        tree.Insert(new Vec3(-1, -1, -1), 1);
        var result = tree.Query(bounds);
        Assert.Empty(result);
    }

    // ── Many items with small queries ───────────────────────────────

    [Fact]
    public void Query_SmallRegion_ReturnsSubset()
    {
        var tree = new Octree<int>(UnitBounds, maxItems: 8);
        var rng = new Random(42);

        for (int i = 0; i < 500; i++)
        {
            var pos = new Vec3(
                rng.NextDouble() * 20 - 10,
                rng.NextDouble() * 20 - 10,
                rng.NextDouble() * 20 - 10);
            tree.Insert(pos, i);
        }

        var smallQuery = new Aabb(new Vec3(-0.5, -0.5, -0.5), new Vec3(0.5, 0.5, 0.5));
        var result = tree.Query(smallQuery);
        Assert.True(result.Count < 500);
    }

    // ── Negative coordinates ────────────────────────────────────────

    [Fact]
    public void Insert_NegativeCoordinates_WorksCorrectly()
    {
        var tree = new Octree<int>(UnitBounds);
        tree.Insert(new Vec3(-5, -3, -1), 42);
        var result = tree.QuerySphere(new Vec3(-5, -3, -1), 0.1);
        Assert.Single(result);
        Assert.Equal(42, result[0]);
    }

    // ── Clustered data ──────────────────────────────────────────────

    [Fact]
    public void Insert_ClusteredData_HandlesCorrectly()
    {
        var tree = new Octree<int>(UnitBounds, maxItems: 4);

        // All items at the same position
        for (int i = 0; i < 50; i++)
            tree.Insert(new Vec3(1, 2, 3), i);

        var result = tree.QuerySphere(new Vec3(1, 2, 3), 0.1);
        Assert.Equal(50, result.Count);
    }

    // ── Bounds property ─────────────────────────────────────────────

    [Fact]
    public void Bounds_ReturnsConstructorBounds()
    {
        var bounds = new Aabb(new Vec3(-5, -5, -5), new Vec3(5, 5, 5));
        var tree = new Octree<int>(bounds);
        Assert.Equal(bounds.Min, tree.Bounds.Min);
        Assert.Equal(bounds.Max, tree.Bounds.Max);
    }
}
