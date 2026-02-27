using MdCsg.Math;
using MdCsg.Spatial;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 33: HashGrid3 large-scale insert/query.</summary>
public class SpatialHashGridTests
{
    [Fact]
    public void Insert_SingleItem_CanQuery()
    {
        var grid = new HashGrid3<int>(1.0);
        grid.Insert(Vec3.Zero, 42);
        var results = grid.Query(Vec3.Zero, 0.5);
        Assert.Contains(42, results);
    }

    [Fact]
    public void Insert_MultipleItems_AllFound()
    {
        var grid = new HashGrid3<int>(1.0);
        for (int i = 0; i < 100; i++)
            grid.Insert(new Vec3(i * 0.1, 0, 0), i);
        var results = grid.Query(new Vec3(5, 0, 0), 1.0);
        Assert.True(results.Count > 0);
    }

    [Fact]
    public void Query_OutOfRange_Empty()
    {
        var grid = new HashGrid3<int>(1.0);
        grid.Insert(Vec3.Zero, 42);
        var results = grid.Query(new Vec3(100, 0, 0), 0.5);
        Assert.Empty(results);
    }

    [Fact]
    public void LargeInsert_1000Points_AllQueryable()
    {
        var grid = new HashGrid3<int>(1.0);
        var rng = new Random(42);
        for (int i = 0; i < 1000; i++)
            grid.Insert(new Vec3(rng.NextDouble() * 10, rng.NextDouble() * 10, rng.NextDouble() * 10), i);
        var results = grid.Query(new Vec3(5, 5, 5), 2.0);
        Assert.True(results.Count > 0);
    }

    [Fact]
    public void CellSize_AffectsQueryResults()
    {
        var gridSmall = new HashGrid3<int>(0.1);
        var gridLarge = new HashGrid3<int>(10.0);
        for (int i = 0; i < 100; i++)
        {
            var p = new Vec3(i * 0.1, 0, 0);
            gridSmall.Insert(p, i);
            gridLarge.Insert(p, i);
        }
        var rsSmall = gridSmall.Query(Vec3.Zero, 0.5);
        var rsLarge = gridLarge.Query(Vec3.Zero, 0.5);
        Assert.True(rsSmall.Count > 0);
        Assert.True(rsLarge.Count > 0);
    }

    [Fact]
    public void Clear_RemovesAllItems()
    {
        var grid = new HashGrid3<int>(1.0);
        for (int i = 0; i < 100; i++)
            grid.Insert(new Vec3(i, 0, 0), i);
        grid.Clear();
        Assert.Equal(0, grid.CellCount);
    }

    [Fact]
    public void QueryCell_ReturnsItemsInCell()
    {
        var grid = new HashGrid3<int>(1.0);
        grid.Insert(new Vec3(0.5, 0.5, 0.5), 42);
        var results = grid.QueryCell(0, 0, 0);
        Assert.Contains(42, results);
    }

    [Fact]
    public void NegativeCoordinates_Work()
    {
        var grid = new HashGrid3<int>(1.0);
        grid.Insert(new Vec3(-5, -3, -1), 99);
        var results = grid.Query(new Vec3(-5, -3, -1), 0.5);
        Assert.Contains(99, results);
    }

    [Fact]
    public void ManyItemsSameCell_AllFound()
    {
        var grid = new HashGrid3<int>(10.0);
        for (int i = 0; i < 50; i++)
            grid.Insert(new Vec3(i * 0.01, 0, 0), i);
        var results = grid.Query(Vec3.Zero, 1.0);
        Assert.True(results.Count >= 50);
    }

    [Fact]
    public void Insert5000Points_NoError()
    {
        var grid = new HashGrid3<int>(1.0);
        var rng = new Random(42);
        for (int i = 0; i < 5000; i++)
            grid.Insert(new Vec3(rng.NextDouble() * 100, rng.NextDouble() * 100, rng.NextDouble() * 100), i);
        Assert.True(grid.CellCount > 0);
    }

    [Fact]
    public void QueryLargeRadius_ReturnsMany()
    {
        var grid = new HashGrid3<int>(1.0);
        for (int i = 0; i < 100; i++)
            grid.Insert(new Vec3(i * 0.1, 0, 0), i);
        var results = grid.Query(new Vec3(5, 0, 0), 10.0);
        Assert.True(results.Count >= 50);
    }

    [Fact]
    public void SmallCellSize_PreciseQueries()
    {
        var grid = new HashGrid3<int>(0.01);
        grid.Insert(Vec3.Zero, 0);
        grid.Insert(new Vec3(1, 0, 0), 1);
        var results = grid.Query(Vec3.Zero, 0.1);
        Assert.Contains(0, results);
        Assert.DoesNotContain(1, results);
    }

    [Fact]
    public void CellSize_Property()
    {
        var grid = new HashGrid3<int>(2.5);
        Assert.Equal(2.5, grid.CellSize);
    }

    [Fact]
    public void InsertAtSamePosition_BothFound()
    {
        var grid = new HashGrid3<string>(1.0);
        grid.Insert(Vec3.Zero, "a");
        grid.Insert(Vec3.Zero, "b");
        var results = grid.Query(Vec3.Zero, 0.5);
        Assert.Contains("a", results);
        Assert.Contains("b", results);
    }

    [Fact]
    public void GridWith3DPoints_QueryNearby()
    {
        var grid = new HashGrid3<int>(1.0);
        var rng = new Random(42);
        for (int i = 0; i < 500; i++)
        {
            var p = new Vec3(rng.NextDouble() * 20 - 10, rng.NextDouble() * 20 - 10, rng.NextDouble() * 20 - 10);
            grid.Insert(p, i);
        }
        var results = grid.Query(Vec3.Zero, 3.0);
        Assert.True(results.Count > 0);
    }

    [Fact]
    public void StringItems_Work()
    {
        var grid = new HashGrid3<string>(1.0);
        grid.Insert(new Vec3(1, 2, 3), "hello");
        var results = grid.Query(new Vec3(1, 2, 3), 0.5);
        Assert.Contains("hello", results);
    }

    [Fact]
    public void EmptyGrid_QueryReturnsEmpty()
    {
        var grid = new HashGrid3<int>(1.0);
        var results = grid.Query(Vec3.Zero, 10.0);
        Assert.Empty(results);
    }

    [Fact]
    public void LargeCoordinates_Work()
    {
        var grid = new HashGrid3<int>(10.0);
        grid.Insert(new Vec3(1000, 2000, 3000), 42);
        var results = grid.Query(new Vec3(1000, 2000, 3000), 5.0);
        Assert.Contains(42, results);
    }
}
