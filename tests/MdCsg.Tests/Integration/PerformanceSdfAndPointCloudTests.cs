using System.Diagnostics;
using MdCsg.Api;
using MdCsg.Math;
using Xunit.Abstractions;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 44: Timed: 10k SDF evals, 100k point cloud.</summary>
public class PerformanceSdfAndPointCloudTests
{
    private readonly ITestOutputHelper _output;

    public PerformanceSdfAndPointCloudTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Sdf_SingleEval_Fast()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var sdf = new SignedDistanceField(cube);
        var sw = Stopwatch.StartNew();
        var d = sdf.SignedDistance(new Vec3(0.5, 0.5, 0.5));
        sw.Stop();
        _output.WriteLine($"SingleSdfEval: {sw.ElapsedMilliseconds}ms, d={d:F4}");
        Assert.True(sw.ElapsedMilliseconds < 1000);
    }

    [Fact]
    public void Sdf_1000Evals_Under5Seconds()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var sdf = new SignedDistanceField(cube);
        var points = new Vec3[1000];
        var rng = new Random(42);
        for (int i = 0; i < points.Length; i++)
            points[i] = new Vec3(rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2);
        var results = new double[1000];
        var sw = Stopwatch.StartNew();
        sdf.SignedDistances(points, results);
        sw.Stop();
        _output.WriteLine($"1000 SDF evals: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public void Sdf_10000Evals_Under10Seconds()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var sdf = new SignedDistanceField(cube);
        var points = new Vec3[10000];
        var rng = new Random(42);
        for (int i = 0; i < points.Length; i++)
            points[i] = new Vec3(rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2);
        var results = new double[10000];
        var sw = Stopwatch.StartNew();
        sdf.SignedDistances(points, results);
        sw.Stop();
        _output.WriteLine($"10000 SDF evals: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 10000);
    }

    [Fact]
    public void Sdf_GridSampling_Under10Seconds()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var sdf = new SignedDistanceField(cube);
        var bounds = new Aabb(new Vec3(-2, -2, -2), new Vec3(2, 2, 2));
        var sw = Stopwatch.StartNew();
        var grid = sdf.SampleOnGrid(bounds, 20, 20, 20);
        sw.Stop();
        _output.WriteLine($"SDF Grid 20³: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 10000);
        Assert.Equal(20 * 20 * 20, grid.Length);
    }

    [Fact]
    public void PointCloud_1000Points_ClassifyUnder5Seconds()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var rng = new Random(42);
        var points = new Vec3[1000];
        for (int i = 0; i < points.Length; i++)
            points[i] = new Vec3(rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2);
        var cloud = new PointCloud(points);
        var query = new PointCloudQuery(cube);
        var sw = Stopwatch.StartNew();
        var classifications = query.Classify(cloud);
        sw.Stop();
        _output.WriteLine($"1000 point classify: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 5000);
        Assert.Equal(1000, classifications.Length);
    }

    [Fact]
    public void PointCloud_10000Points_ClassifyUnder10Seconds()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var rng = new Random(42);
        var points = new Vec3[10000];
        for (int i = 0; i < points.Length; i++)
            points[i] = new Vec3(rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2);
        var cloud = new PointCloud(points);
        var query = new PointCloudQuery(cube);
        var sw = Stopwatch.StartNew();
        var classifications = query.Classify(cloud);
        sw.Stop();
        _output.WriteLine($"10000 point classify: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 10000);
    }

    [Fact]
    public void PointCloud_1000Points_DistancesUnder5Seconds()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var rng = new Random(42);
        var points = new Vec3[1000];
        for (int i = 0; i < points.Length; i++)
            points[i] = new Vec3(rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2);
        var cloud = new PointCloud(points);
        var query = new PointCloudQuery(cube);
        var sw = Stopwatch.StartNew();
        var distances = query.Distances(cloud);
        sw.Stop();
        _output.WriteLine($"1000 point distances: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 5000);
        Assert.Equal(1000, distances.Length);
    }

    [Fact]
    public void PointCloud_FilterInside_Under5Seconds()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var rng = new Random(42);
        var points = new Vec3[5000];
        for (int i = 0; i < points.Length; i++)
            points[i] = new Vec3(rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2);
        var cloud = new PointCloud(points);
        var query = new PointCloudQuery(cube);
        var sw = Stopwatch.StartNew();
        var inside = query.FilterInside(cloud);
        sw.Stop();
        _output.WriteLine($"5000 point FilterInside: {sw.ElapsedMilliseconds}ms, {inside.Count} inside");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public void PointCloud_FilterOutside_Under5Seconds()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var rng = new Random(42);
        var points = new Vec3[5000];
        for (int i = 0; i < points.Length; i++)
            points[i] = new Vec3(rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2);
        var cloud = new PointCloud(points);
        var query = new PointCloudQuery(cube);
        var sw = Stopwatch.StartNew();
        var outside = query.FilterOutside(cloud);
        sw.Stop();
        _output.WriteLine($"5000 point FilterOutside: {sw.ElapsedMilliseconds}ms, {outside.Count} outside");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public void PointCloud_FilterNearSurface_Under5Seconds()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var rng = new Random(42);
        var points = new Vec3[5000];
        for (int i = 0; i < points.Length; i++)
            points[i] = new Vec3(rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2);
        var cloud = new PointCloud(points);
        var query = new PointCloudQuery(cube);
        var sw = Stopwatch.StartNew();
        var near = query.FilterNearSurface(cloud, 0.1);
        sw.Stop();
        _output.WriteLine($"5000 point FilterNearSurface: {sw.ElapsedMilliseconds}ms, {near.Count} near");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public void PointCloud_SignedDistances_Under5Seconds()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var rng = new Random(42);
        var points = new Vec3[1000];
        for (int i = 0; i < points.Length; i++)
            points[i] = new Vec3(rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2);
        var cloud = new PointCloud(points);
        var query = new PointCloudQuery(cube);
        var sw = Stopwatch.StartNew();
        var sds = query.SignedDistances(cloud);
        sw.Stop();
        _output.WriteLine($"1000 point SignedDistances: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 5000);
        Assert.Equal(1000, sds.Length);
    }

    [Fact]
    public void Sdf_OnSphere_Under5Seconds()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        var sdf = new SignedDistanceField(sphere);
        var points = new Vec3[5000];
        var rng = new Random(42);
        for (int i = 0; i < points.Length; i++)
            points[i] = new Vec3(rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2);
        var results = new double[5000];
        var sw = Stopwatch.StartNew();
        sdf.SignedDistances(points, results);
        sw.Stop();
        _output.WriteLine($"5000 SDF on sphere: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public void Sdf_OnCsgResult_Under10Seconds()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(Vec3.Zero, 1.0);
        var result = new Solid(Csg.Difference(a, b).Mesh);
        var sdf = new SignedDistanceField(result);
        var points = new Vec3[1000];
        var rng = new Random(42);
        for (int i = 0; i < points.Length; i++)
            points[i] = new Vec3(rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2);
        var results = new double[1000];
        var sw = Stopwatch.StartNew();
        sdf.SignedDistances(points, results);
        sw.Stop();
        _output.WriteLine($"1000 SDF on CSG result: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 10000);
    }

    [Fact]
    public void PointCloud_ClassifyWithDistances_Under5Seconds()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var rng = new Random(42);
        var points = new Vec3[1000];
        for (int i = 0; i < points.Length; i++)
            points[i] = new Vec3(rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2);
        var cloud = new PointCloud(points);
        var query = new PointCloudQuery(cube);
        var classifications = new MdCsg.Classification.SolidClassification[1000];
        var distances = new double[1000];
        var sw = Stopwatch.StartNew();
        query.ClassifyWithDistances(cloud, classifications, distances);
        sw.Stop();
        _output.WriteLine($"1000 ClassifyWithDistances: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public void Sdf_GridLarge_Under30Seconds()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var sdf = new SignedDistanceField(cube);
        var bounds = new Aabb(new Vec3(-2, -2, -2), new Vec3(2, 2, 2));
        var sw = Stopwatch.StartNew();
        var grid = sdf.SampleOnGrid(bounds, 30, 30, 30);
        sw.Stop();
        _output.WriteLine($"SDF Grid 30³: {sw.ElapsedMilliseconds}ms, {grid.Length} cells");
        Assert.True(sw.ElapsedMilliseconds < 30000);
        Assert.Equal(27000, grid.Length);
    }

    [Fact]
    public void PointCloud_OnSphere_ClassifyUnder5Seconds()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        var rng = new Random(42);
        var points = new Vec3[2000];
        for (int i = 0; i < points.Length; i++)
            points[i] = new Vec3(rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2);
        var cloud = new PointCloud(points);
        var query = new PointCloudQuery(sphere);
        var sw = Stopwatch.StartNew();
        var classifications = query.Classify(cloud);
        sw.Stop();
        _output.WriteLine($"2000 classify on sphere: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public void Sdf_Creation_Fast()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var sw = Stopwatch.StartNew();
        var sdf = new SignedDistanceField(cube);
        sw.Stop();
        _output.WriteLine($"SDF creation: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 1000);
    }

    [Fact]
    public void PointCloudQuery_Creation_Fast()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var sw = Stopwatch.StartNew();
        var query = new PointCloudQuery(cube);
        sw.Stop();
        _output.WriteLine($"PointCloudQuery creation: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 1000);
    }
}
