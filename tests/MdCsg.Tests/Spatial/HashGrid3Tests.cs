using MdCsg.Math;
using MdCsg.Spatial;

namespace MdCsg.Tests.Spatial;

public class HashGrid3Tests
{
    // ── Insert & Query ──────────────────────────────────────────────

    [Fact]
    public void Insert_SingleItem_QueryFindsIt()
    {
        var grid = new HashGrid3<int>(1.0);
        grid.Insert(new Vec3(0.5, 0.5, 0.5), 42);
        var result = grid.Query(new Vec3(0.5, 0.5, 0.5), 0.1);
        Assert.Contains(42, result);
    }

    [Fact]
    public void Query_EmptyGrid_ReturnsEmpty()
    {
        var grid = new HashGrid3<int>(1.0);
        var result = grid.Query(Vec3.Zero, 10);
        Assert.Empty(result);
    }

    [Fact]
    public void Query_ItemOutsideRadius_NotReturned()
    {
        var grid = new HashGrid3<int>(1.0);
        grid.Insert(new Vec3(5, 0, 0), 1);
        var result = grid.Query(Vec3.Zero, 1.0);
        Assert.Empty(result);
    }

    [Fact]
    public void Query_ItemAtBoundary_IncludedIfWithinRadius()
    {
        var grid = new HashGrid3<int>(1.0);
        grid.Insert(new Vec3(1, 0, 0), 1);
        var result = grid.Query(Vec3.Zero, 1.0);
        Assert.Contains(1, result);
    }

    [Fact]
    public void Query_MultipleItems_ReturnsAllWithinRadius()
    {
        var grid = new HashGrid3<string>(1.0);
        grid.Insert(new Vec3(0.1, 0, 0), "a");
        grid.Insert(new Vec3(0.2, 0, 0), "b");
        grid.Insert(new Vec3(10, 0, 0), "c");

        var result = grid.Query(Vec3.Zero, 1.0);
        Assert.Equal(2, result.Count);
        Assert.Contains("a", result);
        Assert.Contains("b", result);
    }

    // ── Cell queries ────────────────────────────────────────────────

    [Fact]
    public void QueryCell_ReturnsItemsInSpecificCell()
    {
        var grid = new HashGrid3<int>(1.0);
        grid.Insert(new Vec3(0.5, 0.5, 0.5), 1); // cell (0,0,0)
        grid.Insert(new Vec3(1.5, 0.5, 0.5), 2); // cell (1,0,0)

        var cell0 = grid.QueryCell(0, 0, 0);
        var cell1 = grid.QueryCell(1, 0, 0);

        Assert.Single(cell0);
        Assert.Contains(1, cell0);
        Assert.Single(cell1);
        Assert.Contains(2, cell1);
    }

    [Fact]
    public void QueryCell_EmptyCell_ReturnsEmpty()
    {
        var grid = new HashGrid3<int>(1.0);
        var result = grid.QueryCell(99, 99, 99);
        Assert.Empty(result);
    }

    // ── Many items ──────────────────────────────────────────────────

    [Fact]
    public void Insert_ManyItems_AllRetrievable()
    {
        var grid = new HashGrid3<int>(1.0);
        var rng = new Random(42);

        for (int i = 0; i < 1000; i++)
        {
            var pos = new Vec3(rng.NextDouble() * 10, rng.NextDouble() * 10, rng.NextDouble() * 10);
            grid.Insert(pos, i);
        }

        // Query the entire space
        var result = grid.Query(new Vec3(5, 5, 5), 20);
        Assert.Equal(1000, result.Count);
    }

    [Fact]
    public void Query_SmallRadius_ReturnsSubset()
    {
        var grid = new HashGrid3<int>(0.5);
        var rng = new Random(42);

        for (int i = 0; i < 100; i++)
        {
            var pos = new Vec3(rng.NextDouble() * 10, rng.NextDouble() * 10, rng.NextDouble() * 10);
            grid.Insert(pos, i);
        }

        var result = grid.Query(new Vec3(5, 5, 5), 0.5);
        Assert.True(result.Count < 100); // should be a small subset
    }

    // ── Various cell sizes ──────────────────────────────────────────

    [Theory]
    [InlineData(0.1)]
    [InlineData(1.0)]
    [InlineData(10.0)]
    public void Insert_VariousCellSizes_WorksCorrectly(double cellSize)
    {
        var grid = new HashGrid3<int>(cellSize);
        grid.Insert(new Vec3(0.5, 0.5, 0.5), 1);
        var result = grid.Query(new Vec3(0.5, 0.5, 0.5), 0.01);
        Assert.Single(result);
    }

    // ── Negative coordinates ────────────────────────────────────────

    [Fact]
    public void Insert_NegativeCoordinates_WorksCorrectly()
    {
        var grid = new HashGrid3<int>(1.0);
        grid.Insert(new Vec3(-5, -3, -1), 42);
        var result = grid.Query(new Vec3(-5, -3, -1), 0.1);
        Assert.Single(result);
        Assert.Equal(42, result[0]);
    }

    // ── Clear ───────────────────────────────────────────────────────

    [Fact]
    public void Clear_RemovesAllItems()
    {
        var grid = new HashGrid3<int>(1.0);
        grid.Insert(Vec3.Zero, 1);
        grid.Insert(new Vec3(1, 1, 1), 2);
        grid.Clear();
        Assert.Equal(0, grid.CellCount);
        Assert.Empty(grid.Query(Vec3.Zero, 100));
    }

    // ── CellCount ───────────────────────────────────────────────────

    [Fact]
    public void CellCount_ReflectsInsertedCells()
    {
        var grid = new HashGrid3<int>(1.0);
        Assert.Equal(0, grid.CellCount);
        grid.Insert(new Vec3(0.5, 0.5, 0.5), 1);
        Assert.Equal(1, grid.CellCount);
        grid.Insert(new Vec3(1.5, 0.5, 0.5), 2); // different cell
        Assert.Equal(2, grid.CellCount);
        grid.Insert(new Vec3(0.3, 0.3, 0.3), 3); // same cell as first
        Assert.Equal(2, grid.CellCount);
    }

    // ── Edge: zero cell size ────────────────────────────────────────

    [Fact]
    public void Constructor_ZeroCellSize_Throws()
    {
        Assert.Throws<ArgumentException>(() => new HashGrid3<int>(0));
    }

    [Fact]
    public void Constructor_NegativeCellSize_Throws()
    {
        Assert.Throws<ArgumentException>(() => new HashGrid3<int>(-1));
    }

    // ── 3D radius query precision ───────────────────────────────────

    [Fact]
    public void Query_DiagonalDistance_CorrectFiltering()
    {
        var grid = new HashGrid3<int>(1.0);
        // Point at (1,1,1): distance from origin = sqrt(3) ≈ 1.73
        grid.Insert(new Vec3(1, 1, 1), 1);

        var within2 = grid.Query(Vec3.Zero, 2.0);
        var within1 = grid.Query(Vec3.Zero, 1.0);

        Assert.Single(within2);
        Assert.Empty(within1);
    }
}
