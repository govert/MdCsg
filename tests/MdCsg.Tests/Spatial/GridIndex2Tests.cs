using MdCsg.Math;
using MdCsg.Spatial;

namespace MdCsg.Tests.Spatial;

public class GridIndex2Tests
{
    // ── Insert & Query ──────────────────────────────────────────────

    [Fact]
    public void Insert_SingleItem_QueryFindsIt()
    {
        var grid = new GridIndex2<int>(1.0);
        var bounds = new Aabb(new Vec3(0, 0, 0), new Vec3(0.5, 0.5, 0.5));
        grid.Insert(bounds, 42);
        var result = grid.Query(new Vec2(0.25, 0.25));
        Assert.Contains(42, result);
    }

    [Fact]
    public void Query_EmptyGrid_ReturnsEmpty()
    {
        var grid = new GridIndex2<int>(1.0);
        var result = grid.Query(new Vec2(0, 0));
        Assert.Empty(result);
    }

    [Fact]
    public void Query_DifferentCell_NotReturned()
    {
        var grid = new GridIndex2<int>(1.0);
        var bounds = new Aabb(new Vec3(5, 5, 0), new Vec3(5.5, 5.5, 1));
        grid.Insert(bounds, 1);
        var result = grid.Query(new Vec2(0, 0));
        Assert.Empty(result);
    }

    // ── Multi-cell items ────────────────────────────────────────────

    [Fact]
    public void Insert_LargeItem_SpansMultipleCells()
    {
        var grid = new GridIndex2<int>(1.0);
        // Item from (0,0) to (2.5, 2.5) spans cells (0,0), (0,1), (0,2), (1,0), (1,1), (1,2), (2,0), (2,1), (2,2)
        var bounds = new Aabb(new Vec3(0, 0, 0), new Vec3(2.5, 2.5, 1));
        grid.Insert(bounds, 1);

        // Should find the item from any cell it spans
        Assert.Contains(1, grid.Query(new Vec2(0.5, 0.5)));
        Assert.Contains(1, grid.Query(new Vec2(1.5, 1.5)));
        Assert.Contains(1, grid.Query(new Vec2(2.2, 2.2)));
    }

    [Fact]
    public void Insert_LargeItem_CellCount()
    {
        var grid = new GridIndex2<int>(1.0);
        var bounds = new Aabb(new Vec3(0, 0, 0), new Vec3(2.5, 2.5, 1));
        grid.Insert(bounds, 1);
        // Spans 3x3 = 9 cells
        Assert.Equal(9, grid.CellCount);
    }

    // ── Multiple items in same cell ─────────────────────────────────

    [Fact]
    public void Insert_MultipleItemsSameCell_AllReturned()
    {
        var grid = new GridIndex2<string>(1.0);
        var bounds1 = new Aabb(new Vec3(0.1, 0.1, 0), new Vec3(0.2, 0.2, 1));
        var bounds2 = new Aabb(new Vec3(0.3, 0.3, 0), new Vec3(0.4, 0.4, 1));
        grid.Insert(bounds1, "a");
        grid.Insert(bounds2, "b");

        var result = grid.Query(new Vec2(0.5, 0.5));
        Assert.Equal(2, result.Count);
        Assert.Contains("a", result);
        Assert.Contains("b", result);
    }

    // ── Various cell sizes ──────────────────────────────────────────

    [Theory]
    [InlineData(0.1)]
    [InlineData(1.0)]
    [InlineData(10.0)]
    public void Insert_VariousCellSizes_WorksCorrectly(double cellSize)
    {
        var grid = new GridIndex2<int>(cellSize);
        var bounds = new Aabb(new Vec3(0.5, 0.5, 0), new Vec3(0.6, 0.6, 1));
        grid.Insert(bounds, 1);
        var result = grid.Query(new Vec2(0.55, 0.55));
        Assert.Single(result);
    }

    // ── Origin offset ───────────────────────────────────────────────

    [Fact]
    public void Constructor_WithOrigin_OffsetsCells()
    {
        var grid = new GridIndex2<int>(1.0, new Vec2(0.5, 0.5));
        // With origin (0.5, 0.5), position (0.25, 0.25) maps to cell (-1, -1)
        // Position (0.75, 0.75) maps to cell (0, 0)
        var bounds = new Aabb(new Vec3(0.6, 0.6, 0), new Vec3(0.7, 0.7, 1));
        grid.Insert(bounds, 1);

        Assert.Contains(1, grid.Query(new Vec2(0.75, 0.75)));
        Assert.Empty(grid.Query(new Vec2(0.25, 0.25)));
    }

    // ── Negative coordinates ────────────────────────────────────────

    [Fact]
    public void Insert_NegativeCoordinates_WorksCorrectly()
    {
        var grid = new GridIndex2<int>(1.0);
        var bounds = new Aabb(new Vec3(-5, -3, 0), new Vec3(-4.5, -2.5, 1));
        grid.Insert(bounds, 42);
        var result = grid.Query(new Vec2(-4.8, -2.8));
        Assert.Single(result);
        Assert.Equal(42, result[0]);
    }

    // ── Clear ───────────────────────────────────────────────────────

    [Fact]
    public void Clear_RemovesAllItems()
    {
        var grid = new GridIndex2<int>(1.0);
        var bounds = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        grid.Insert(bounds, 1);
        grid.Clear();
        Assert.Equal(0, grid.CellCount);
        Assert.Empty(grid.Query(new Vec2(0.5, 0.5)));
    }

    // ── Edge cases ──────────────────────────────────────────────────

    [Fact]
    public void Constructor_ZeroCellSize_Throws()
    {
        Assert.Throws<ArgumentException>(() => new GridIndex2<int>(0));
    }

    [Fact]
    public void Constructor_NegativeCellSize_Throws()
    {
        Assert.Throws<ArgumentException>(() => new GridIndex2<int>(-1));
    }

    [Fact]
    public void Insert_PointItem_SingleCell()
    {
        var grid = new GridIndex2<int>(1.0);
        var bounds = new Aabb(new Vec3(0.5, 0.5, 0), new Vec3(0.5, 0.5, 0));
        grid.Insert(bounds, 1);
        Assert.Equal(1, grid.CellCount);
    }

    // ── Many items ──────────────────────────────────────────────────

    [Fact]
    public void Insert_ManySmallItems_QueryReturnsCorrectSubset()
    {
        var grid = new GridIndex2<int>(1.0);
        var rng = new Random(42);
        int inCell = 0;

        for (int i = 0; i < 100; i++)
        {
            double x = rng.NextDouble() * 10;
            double y = rng.NextDouble() * 10;
            var bounds = new Aabb(new Vec3(x, y, 0), new Vec3(x + 0.01, y + 0.01, 1));
            grid.Insert(bounds, i);
            // Count how many land in cell containing (5.5, 5.5)
            if (x >= 5 && x < 6 && y >= 5 && y < 6)
                inCell++;
        }

        var result = grid.Query(new Vec2(5.5, 5.5));
        Assert.Equal(inCell, result.Count);
    }
}
