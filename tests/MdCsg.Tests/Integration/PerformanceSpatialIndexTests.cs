using System.Diagnostics;
using MdCsg.Math;
using MdCsg.Spatial;
using Xunit.Abstractions;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 45: Timed: spatial index insert/query 50k+.</summary>
public class PerformanceSpatialIndexTests
{
    private readonly ITestOutputHelper _output;

    public PerformanceSpatialIndexTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void HashGrid3_Insert10000_Under2Seconds()
    {
        var grid = new HashGrid3<int>(1.0);
        var rng = new Random(42);
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 10000; i++)
            grid.Insert(new Vec3(rng.NextDouble() * 100, rng.NextDouble() * 100, rng.NextDouble() * 100), i);
        sw.Stop();
        _output.WriteLine($"HashGrid3 10k insert: {sw.ElapsedMilliseconds}ms, {grid.CellCount} cells");
        Assert.True(sw.ElapsedMilliseconds < 2000);
    }

    [Fact]
    public void HashGrid3_Insert50000_Under5Seconds()
    {
        var grid = new HashGrid3<int>(1.0);
        var rng = new Random(42);
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 50000; i++)
            grid.Insert(new Vec3(rng.NextDouble() * 100, rng.NextDouble() * 100, rng.NextDouble() * 100), i);
        sw.Stop();
        _output.WriteLine($"HashGrid3 50k insert: {sw.ElapsedMilliseconds}ms, {grid.CellCount} cells");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public void HashGrid3_Query10000_Under5Seconds()
    {
        var grid = new HashGrid3<int>(1.0);
        var rng = new Random(42);
        for (int i = 0; i < 10000; i++)
            grid.Insert(new Vec3(rng.NextDouble() * 100, rng.NextDouble() * 100, rng.NextDouble() * 100), i);
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 10000; i++)
            grid.Query(new Vec3(rng.NextDouble() * 100, rng.NextDouble() * 100, rng.NextDouble() * 100), 2.0);
        sw.Stop();
        _output.WriteLine($"HashGrid3 10k query: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public void Octree_Insert10000_Under2Seconds()
    {
        var bounds = new Aabb(new Vec3(-50, -50, -50), new Vec3(50, 50, 50));
        var octree = new Octree<int>(bounds);
        var rng = new Random(42);
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 10000; i++)
            octree.Insert(new Vec3(rng.NextDouble() * 100 - 50, rng.NextDouble() * 100 - 50, rng.NextDouble() * 100 - 50), i);
        sw.Stop();
        _output.WriteLine($"Octree 10k insert: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 2000);
    }

    [Fact]
    public void Octree_Insert50000_Under5Seconds()
    {
        var bounds = new Aabb(new Vec3(-100, -100, -100), new Vec3(100, 100, 100));
        var octree = new Octree<int>(bounds);
        var rng = new Random(42);
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 50000; i++)
            octree.Insert(new Vec3(rng.NextDouble() * 200 - 100, rng.NextDouble() * 200 - 100, rng.NextDouble() * 200 - 100), i);
        sw.Stop();
        _output.WriteLine($"Octree 50k insert: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public void Octree_SphereQuery10000_Under5Seconds()
    {
        var bounds = new Aabb(new Vec3(-50, -50, -50), new Vec3(50, 50, 50));
        var octree = new Octree<int>(bounds);
        var rng = new Random(42);
        for (int i = 0; i < 10000; i++)
            octree.Insert(new Vec3(rng.NextDouble() * 100 - 50, rng.NextDouble() * 100 - 50, rng.NextDouble() * 100 - 50), i);
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 10000; i++)
            octree.QuerySphere(new Vec3(rng.NextDouble() * 100 - 50, rng.NextDouble() * 100 - 50, rng.NextDouble() * 100 - 50), 5.0);
        sw.Stop();
        _output.WriteLine($"Octree 10k sphere query: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public void Octree_AabbQuery10000_Under5Seconds()
    {
        var bounds = new Aabb(new Vec3(-50, -50, -50), new Vec3(50, 50, 50));
        var octree = new Octree<int>(bounds);
        var rng = new Random(42);
        for (int i = 0; i < 10000; i++)
            octree.Insert(new Vec3(rng.NextDouble() * 100 - 50, rng.NextDouble() * 100 - 50, rng.NextDouble() * 100 - 50), i);
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 10000; i++)
        {
            double x = rng.NextDouble() * 80 - 40;
            double y = rng.NextDouble() * 80 - 40;
            double z = rng.NextDouble() * 80 - 40;
            octree.Query(new Aabb(new Vec3(x - 2, y - 2, z - 2), new Vec3(x + 2, y + 2, z + 2)));
        }
        sw.Stop();
        _output.WriteLine($"Octree 10k AABB query: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public void GridIndex2_Insert10000_Under2Seconds()
    {
        var grid = new GridIndex2<int>(1.0);
        var rng = new Random(42);
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 10000; i++)
        {
            double x = rng.NextDouble() * 100;
            double y = rng.NextDouble() * 100;
            grid.Insert(new Aabb(new Vec3(x, y, 0), new Vec3(x + 0.5, y + 0.5, 0)), i);
        }
        sw.Stop();
        _output.WriteLine($"GridIndex2 10k insert: {sw.ElapsedMilliseconds}ms, {grid.CellCount} cells");
        Assert.True(sw.ElapsedMilliseconds < 2000);
    }

    [Fact]
    public void GridIndex2_Insert50000_Under5Seconds()
    {
        var grid = new GridIndex2<int>(1.0);
        var rng = new Random(42);
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 50000; i++)
        {
            double x = rng.NextDouble() * 100;
            double y = rng.NextDouble() * 100;
            grid.Insert(new Aabb(new Vec3(x, y, 0), new Vec3(x + 0.5, y + 0.5, 0)), i);
        }
        sw.Stop();
        _output.WriteLine($"GridIndex2 50k insert: {sw.ElapsedMilliseconds}ms, {grid.CellCount} cells");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public void GridIndex2_Query10000_Under5Seconds()
    {
        var grid = new GridIndex2<int>(1.0);
        var rng = new Random(42);
        for (int i = 0; i < 10000; i++)
        {
            double x = rng.NextDouble() * 100;
            double y = rng.NextDouble() * 100;
            grid.Insert(new Aabb(new Vec3(x, y, 0), new Vec3(x + 0.5, y + 0.5, 0)), i);
        }
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 10000; i++)
            grid.Query(new Vec2(rng.NextDouble() * 100, rng.NextDouble() * 100));
        sw.Stop();
        _output.WriteLine($"GridIndex2 10k query: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public void HashGrid3_ClearAndRefill_Under2Seconds()
    {
        var grid = new HashGrid3<int>(1.0);
        var rng = new Random(42);
        var sw = Stopwatch.StartNew();
        for (int round = 0; round < 5; round++)
        {
            for (int i = 0; i < 5000; i++)
                grid.Insert(new Vec3(rng.NextDouble() * 100, rng.NextDouble() * 100, rng.NextDouble() * 100), i);
            grid.Clear();
        }
        sw.Stop();
        _output.WriteLine($"HashGrid3 5 rounds of 5k: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 2000);
    }

    [Fact]
    public void HashGrid3_SmallCellSize_Insert10000_Under5Seconds()
    {
        var grid = new HashGrid3<int>(0.1);
        var rng = new Random(42);
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 10000; i++)
            grid.Insert(new Vec3(rng.NextDouble() * 10, rng.NextDouble() * 10, rng.NextDouble() * 10), i);
        sw.Stop();
        _output.WriteLine($"HashGrid3(0.1) 10k: {sw.ElapsedMilliseconds}ms, {grid.CellCount} cells");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public void Octree_HighDensity_SameLocation_Under2Seconds()
    {
        var bounds = new Aabb(new Vec3(-10, -10, -10), new Vec3(10, 10, 10));
        var octree = new Octree<int>(bounds);
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 10000; i++)
            octree.Insert(Vec3.Zero, i);
        sw.Stop();
        _output.WriteLine($"Octree 10k same location: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 2000);
    }

    [Fact]
    public void Octree_LargeRadius_Query_Under2Seconds()
    {
        var bounds = new Aabb(new Vec3(-100, -100, -100), new Vec3(100, 100, 100));
        var octree = new Octree<int>(bounds);
        var rng = new Random(42);
        for (int i = 0; i < 10000; i++)
            octree.Insert(new Vec3(rng.NextDouble() * 200 - 100, rng.NextDouble() * 200 - 100, rng.NextDouble() * 200 - 100), i);
        var sw = Stopwatch.StartNew();
        var results = octree.QuerySphere(Vec3.Zero, 200.0);
        sw.Stop();
        _output.WriteLine($"Octree large radius query: {sw.ElapsedMilliseconds}ms, {results.Count} results");
        Assert.True(sw.ElapsedMilliseconds < 2000);
    }

    [Fact]
    public void GridIndex2_LargeBounds_Insert_Under2Seconds()
    {
        var grid = new GridIndex2<int>(1.0);
        var rng = new Random(42);
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 5000; i++)
        {
            double x = rng.NextDouble() * 100;
            double y = rng.NextDouble() * 100;
            grid.Insert(new Aabb(new Vec3(x, y, 0), new Vec3(x + 5, y + 5, 0)), i);
        }
        sw.Stop();
        _output.WriteLine($"GridIndex2 5k large bounds: {sw.ElapsedMilliseconds}ms, {grid.CellCount} cells");
        Assert.True(sw.ElapsedMilliseconds < 2000);
    }

    [Fact]
    public void HashGrid3_QueryCellDirect_Under2Seconds()
    {
        var grid = new HashGrid3<int>(1.0);
        var rng = new Random(42);
        for (int i = 0; i < 10000; i++)
            grid.Insert(new Vec3(rng.NextDouble() * 50, rng.NextDouble() * 50, rng.NextDouble() * 50), i);
        var sw = Stopwatch.StartNew();
        for (int x = 0; x < 50; x++)
            for (int y = 0; y < 50; y++)
                for (int z = 0; z < 5; z++)
                    grid.QueryCell(x, y, z);
        sw.Stop();
        _output.WriteLine($"HashGrid3 12500 cell queries: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 2000);
    }

    [Fact]
    public void AllSpatialIndices_Insert5000Each_Under5Seconds()
    {
        var rng = new Random(42);
        var sw = Stopwatch.StartNew();

        var hg = new HashGrid3<int>(1.0);
        for (int i = 0; i < 5000; i++)
            hg.Insert(new Vec3(rng.NextDouble() * 100, rng.NextDouble() * 100, rng.NextDouble() * 100), i);

        var bounds = new Aabb(new Vec3(-50, -50, -50), new Vec3(50, 50, 50));
        var ot = new Octree<int>(bounds);
        for (int i = 0; i < 5000; i++)
            ot.Insert(new Vec3(rng.NextDouble() * 100 - 50, rng.NextDouble() * 100 - 50, rng.NextDouble() * 100 - 50), i);

        var gi = new GridIndex2<int>(1.0);
        for (int i = 0; i < 5000; i++)
        {
            double x = rng.NextDouble() * 100;
            double y = rng.NextDouble() * 100;
            gi.Insert(new Aabb(new Vec3(x, y, 0), new Vec3(x + 0.5, y + 0.5, 0)), i);
        }

        sw.Stop();
        _output.WriteLine($"All indices 5k each: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }
}
