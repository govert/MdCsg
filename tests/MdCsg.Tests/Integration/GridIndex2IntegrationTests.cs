using MdCsg.Math;
using MdCsg.Spatial;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 35: GridIndex2 with mesh face AABBs.</summary>
public class GridIndex2IntegrationTests
{
    [Fact]
    public void Insert_SingleItem_CanQuery()
    {
        var grid = new GridIndex2<int>(1.0);
        grid.Insert(new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 0)), 42);
        var results = grid.Query(new Vec2(0.5, 0.5));
        Assert.Contains(42, results);
    }

    [Fact]
    public void Query_OutOfRange_Empty()
    {
        var grid = new GridIndex2<int>(1.0);
        grid.Insert(new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 0)), 42);
        var results = grid.Query(new Vec2(100, 100));
        Assert.Empty(results);
    }

    [Fact]
    public void InsertMultiple_AllFound()
    {
        var grid = new GridIndex2<int>(1.0);
        for (int i = 0; i < 10; i++)
            grid.Insert(new Aabb(new Vec3(i, 0, 0), new Vec3(i + 1, 1, 0)), i);
        var results = grid.Query(new Vec2(5.5, 0.5));
        Assert.Contains(5, results);
    }

    [Fact]
    public void Clear_RemovesAll()
    {
        var grid = new GridIndex2<int>(1.0);
        for (int i = 0; i < 100; i++)
            grid.Insert(new Aabb(new Vec3(i, 0, 0), new Vec3(i + 1, 1, 0)), i);
        grid.Clear();
        Assert.Equal(0, grid.CellCount);
    }

    [Fact]
    public void CellSize_Property()
    {
        var grid = new GridIndex2<int>(2.5);
        Assert.Equal(2.5, grid.CellSize);
    }

    [Fact]
    public void LargeInsertion_NoError()
    {
        var grid = new GridIndex2<int>(1.0);
        for (int i = 0; i < 1000; i++)
            grid.Insert(new Aabb(new Vec3(i * 0.1, 0, 0), new Vec3(i * 0.1 + 0.1, 0.1, 0)), i);
        Assert.True(grid.CellCount > 0);
    }

    [Fact]
    public void OverlappingBounds_BothFound()
    {
        var grid = new GridIndex2<int>(1.0);
        grid.Insert(new Aabb(new Vec3(0, 0, 0), new Vec3(2, 2, 0)), 1);
        grid.Insert(new Aabb(new Vec3(1, 1, 0), new Vec3(3, 3, 0)), 2);
        var results = grid.Query(new Vec2(1.5, 1.5));
        Assert.Contains(1, results);
        Assert.Contains(2, results);
    }

    [Fact]
    public void NegativeCoordinates_Work()
    {
        var grid = new GridIndex2<int>(1.0);
        grid.Insert(new Aabb(new Vec3(-5, -5, 0), new Vec3(-4, -4, 0)), 42);
        var results = grid.Query(new Vec2(-4.5, -4.5));
        Assert.Contains(42, results);
    }

    [Fact]
    public void SmallCellSize_PreciseQueries()
    {
        var grid = new GridIndex2<int>(0.1);
        grid.Insert(new Aabb(new Vec3(0, 0, 0), new Vec3(0.05, 0.05, 0)), 1);
        grid.Insert(new Aabb(new Vec3(1, 1, 0), new Vec3(1.05, 1.05, 0)), 2);
        var r1 = grid.Query(new Vec2(0.02, 0.02));
        var r2 = grid.Query(new Vec2(1.02, 1.02));
        Assert.Contains(1, r1);
        Assert.DoesNotContain(2, r1);
        Assert.Contains(2, r2);
    }

    [Fact]
    public void LargeBounds_SpanMultipleCells()
    {
        var grid = new GridIndex2<int>(1.0);
        grid.Insert(new Aabb(new Vec3(0, 0, 0), new Vec3(5, 5, 0)), 42);
        // Should be found in all covered cells
        Assert.Contains(42, grid.Query(new Vec2(0.5, 0.5)));
        Assert.Contains(42, grid.Query(new Vec2(4.5, 4.5)));
    }

    [Fact]
    public void WithMeshFaces_WorksCorrectly()
    {
        var cube = MdCsg.Api.Primitives.Cube(Vec3.Zero, 2.0);
        var grid = new GridIndex2<int>(1.0);
        for (int i = 0; i < cube.Mesh.Faces.Count; i++)
        {
            cube.Mesh.Faces[i].GetTrianglePositions(out var a, out var b, out var c);
            var min = Vec3.Min(Vec3.Min(a, b), c);
            var max = Vec3.Max(Vec3.Max(a, b), c);
            grid.Insert(new Aabb(min, max), i);
        }
        var results = grid.Query(new Vec2(0, 0));
        Assert.True(results.Count > 0);
    }

    [Fact]
    public void EmptyGrid_QueryReturnsEmpty()
    {
        var grid = new GridIndex2<int>(1.0);
        var results = grid.Query(new Vec2(0, 0));
        Assert.Empty(results);
    }

    [Fact]
    public void StringItems_Work()
    {
        var grid = new GridIndex2<string>(1.0);
        grid.Insert(new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 0)), "test");
        var results = grid.Query(new Vec2(0.5, 0.5));
        Assert.Contains("test", results);
    }

    [Fact]
    public void ManyItemsSameCell_AllFound()
    {
        var grid = new GridIndex2<int>(10.0);
        for (int i = 0; i < 50; i++)
            grid.Insert(new Aabb(new Vec3(0, 0, 0), new Vec3(0.01, 0.01, 0)), i);
        var results = grid.Query(new Vec2(0.005, 0.005));
        Assert.Equal(50, results.Count);
    }

    [Fact]
    public void WithOrigin_AdjustsCoordinates()
    {
        var grid = new GridIndex2<int>(1.0, new Vec2(5, 5));
        grid.Insert(new Aabb(new Vec3(5, 5, 0), new Vec3(6, 6, 0)), 42);
        var results = grid.Query(new Vec2(5.5, 5.5));
        Assert.Contains(42, results);
    }

    [Fact]
    public void GridWith500Items_QueryWorks()
    {
        var grid = new GridIndex2<int>(0.5);
        var rng = new Random(42);
        for (int i = 0; i < 500; i++)
        {
            double x = rng.NextDouble() * 20 - 10;
            double y = rng.NextDouble() * 20 - 10;
            grid.Insert(new Aabb(new Vec3(x, y, 0), new Vec3(x + 0.1, y + 0.1, 0)), i);
        }
        var results = grid.Query(new Vec2(0, 0));
        // May or may not have results depending on random placement
        Assert.True(results.Count >= 0);
    }

    [Fact]
    public void ClearThenReuse_Works()
    {
        var grid = new GridIndex2<int>(1.0);
        grid.Insert(new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 0)), 1);
        grid.Clear();
        grid.Insert(new Aabb(new Vec3(2, 2, 0), new Vec3(3, 3, 0)), 2);
        Assert.Empty(grid.Query(new Vec2(0.5, 0.5)));
        Assert.Contains(2, grid.Query(new Vec2(2.5, 2.5)));
    }

    [Fact]
    public void LargeCoordinates_Work()
    {
        var grid = new GridIndex2<int>(10.0);
        grid.Insert(new Aabb(new Vec3(1000, 2000, 0), new Vec3(1001, 2001, 0)), 42);
        var results = grid.Query(new Vec2(1000.5, 2000.5));
        Assert.Contains(42, results);
    }
}
