using System.Diagnostics;
using MdCsg.Api;
using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Gpu;
using MdCsg.Math;
using Xunit.Abstractions;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 50: GPU 1M+ point batches, SDF grid (skip if no Vulkan).</summary>
public class GpuLargeBatchTests
{
    private readonly ITestOutputHelper _output;

    public GpuLargeBatchTests(ITestOutputHelper output) => _output = output;

    private static GpuPointCloudQuery? TryCreateGpu()
    {
        try { return GpuPointCloudQuery.TryCreate(); }
        catch { return null; }
    }

    [Fact]
    public void GpuClassify_100000Points()
    {
        using var gpu = TryCreateGpu();
        if (gpu == null) return;

        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var bvh = BvhTree.Build(cube.Mesh);
        var rng = new Random(42);
        var points = new Vec3[100000];
        for (int i = 0; i < points.Length; i++)
            points[i] = new Vec3(rng.NextDouble() * 6 - 3, rng.NextDouble() * 6 - 3, rng.NextDouble() * 6 - 3);
        var results = new SolidClassification[100000];
        var sw = Stopwatch.StartNew();
        gpu.Classify(points, results, bvh, false);
        sw.Stop();
        _output.WriteLine($"GPU classify 100k: {sw.ElapsedMilliseconds}ms");
    }

    [Fact]
    public void GpuDistance_100000Points()
    {
        using var gpu = TryCreateGpu();
        if (gpu == null) return;

        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var bvh = BvhTree.Build(cube.Mesh);
        var rng = new Random(42);
        var points = new Vec3[100000];
        for (int i = 0; i < points.Length; i++)
            points[i] = new Vec3(rng.NextDouble() * 6 - 3, rng.NextDouble() * 6 - 3, rng.NextDouble() * 6 - 3);
        var dists = new double[100000];
        var sw = Stopwatch.StartNew();
        gpu.Distances(points, dists, bvh);
        sw.Stop();
        _output.WriteLine($"GPU distance 100k: {sw.ElapsedMilliseconds}ms");
    }

    [Fact]
    public void GpuClassify_500000Points()
    {
        using var gpu = TryCreateGpu();
        if (gpu == null) return;

        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var bvh = BvhTree.Build(cube.Mesh);
        var rng = new Random(42);
        var points = new Vec3[500000];
        for (int i = 0; i < points.Length; i++)
            points[i] = new Vec3(rng.NextDouble() * 6 - 3, rng.NextDouble() * 6 - 3, rng.NextDouble() * 6 - 3);
        var results = new SolidClassification[500000];
        var sw = Stopwatch.StartNew();
        gpu.Classify(points, results, bvh, false);
        sw.Stop();
        _output.WriteLine($"GPU classify 500k: {sw.ElapsedMilliseconds}ms");
    }

    [Fact]
    public void GpuClassify_1000000Points()
    {
        using var gpu = TryCreateGpu();
        if (gpu == null) return;

        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var bvh = BvhTree.Build(cube.Mesh);
        var rng = new Random(42);
        var points = new Vec3[1000000];
        for (int i = 0; i < points.Length; i++)
            points[i] = new Vec3(rng.NextDouble() * 6 - 3, rng.NextDouble() * 6 - 3, rng.NextDouble() * 6 - 3);
        var results = new SolidClassification[1000000];
        var sw = Stopwatch.StartNew();
        gpu.Classify(points, results, bvh, false);
        sw.Stop();
        _output.WriteLine($"GPU classify 1M: {sw.ElapsedMilliseconds}ms");
    }

    [Fact]
    public void GpuDistance_1000000Points()
    {
        using var gpu = TryCreateGpu();
        if (gpu == null) return;

        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var bvh = BvhTree.Build(cube.Mesh);
        var rng = new Random(42);
        var points = new Vec3[1000000];
        for (int i = 0; i < points.Length; i++)
            points[i] = new Vec3(rng.NextDouble() * 6 - 3, rng.NextDouble() * 6 - 3, rng.NextDouble() * 6 - 3);
        var dists = new double[1000000];
        var sw = Stopwatch.StartNew();
        gpu.Distances(points, dists, bvh);
        sw.Stop();
        _output.WriteLine($"GPU distance 1M: {sw.ElapsedMilliseconds}ms");
    }

    [Fact]
    public void GpuSignedDistance_1000000Points()
    {
        using var gpu = TryCreateGpu();
        if (gpu == null) return;

        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var bvh = BvhTree.Build(cube.Mesh);
        var rng = new Random(42);
        var points = new Vec3[1000000];
        for (int i = 0; i < points.Length; i++)
            points[i] = new Vec3(rng.NextDouble() * 6 - 3, rng.NextDouble() * 6 - 3, rng.NextDouble() * 6 - 3);
        var sd = new double[1000000];
        var sw = Stopwatch.StartNew();
        gpu.SignedDistances(points, sd, bvh, false);
        sw.Stop();
        _output.WriteLine($"GPU signed distance 1M: {sw.ElapsedMilliseconds}ms");
    }

    [Fact]
    public void GpuClassify_OnHighPolySolid()
    {
        using var gpu = TryCreateGpu();
        if (gpu == null) return;

        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 4);
        var bvh = BvhTree.Build(sphere.Mesh);
        var rng = new Random(42);
        var points = new Vec3[10000];
        for (int i = 0; i < points.Length; i++)
            points[i] = new Vec3(rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2);
        var results = new SolidClassification[10000];
        var sw = Stopwatch.StartNew();
        gpu.Classify(points, results, bvh, false);
        sw.Stop();
        _output.WriteLine($"GPU classify 10k on highPoly: {sw.ElapsedMilliseconds}ms");
    }

    [Fact]
    public void GpuClassify_OnCsgResult_100000Points()
    {
        using var gpu = TryCreateGpu();
        if (gpu == null) return;

        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(Vec3.Zero, 1.0);
        var solid = new Solid(Csg.Difference(a, b).Mesh);
        var bvh = BvhTree.Build(solid.Mesh);
        var rng = new Random(42);
        var points = new Vec3[100000];
        for (int i = 0; i < points.Length; i++)
            points[i] = new Vec3(rng.NextDouble() * 6 - 3, rng.NextDouble() * 6 - 3, rng.NextDouble() * 6 - 3);
        var results = new SolidClassification[100000];
        var sw = Stopwatch.StartNew();
        gpu.Classify(points, results, bvh, false);
        sw.Stop();
        _output.WriteLine($"GPU classify 100k on CSG: {sw.ElapsedMilliseconds}ms");
    }

    [Fact]
    public void GpuVsCpu_LargeBatch_MatchesApproximately()
    {
        using var gpu = TryCreateGpu();
        if (gpu == null) return;

        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var bvh = BvhTree.Build(cube.Mesh);
        var rng = new Random(42);
        var points = new Vec3[1000];
        for (int i = 0; i < points.Length; i++)
            points[i] = new Vec3(rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2);

        var cloud = new PointCloud(points);
        var cpuResult = new PointCloudQuery(cube).Classify(cloud);

        var gpuResult = new SolidClassification[1000];
        gpu.Classify(points, gpuResult, bvh, false);

        int matches = 0;
        for (int i = 0; i < points.Length; i++)
            if (cpuResult[i] == gpuResult[i]) matches++;
        _output.WriteLine($"CPU/GPU match: {matches}/{points.Length}");
        Assert.True(matches > 950);
    }

    [Fact]
    public void GpuClassify_MultipleSolids_Sequential()
    {
        using var gpu = TryCreateGpu();
        if (gpu == null) return;

        var rng = new Random(42);
        var points = new Vec3[1000];
        for (int i = 0; i < points.Length; i++)
            points[i] = new Vec3(rng.NextDouble() * 6 - 3, rng.NextDouble() * 6 - 3, rng.NextDouble() * 6 - 3);

        var solids = new[] {
            Primitives.Cube(Vec3.Zero, 2.0),
            Primitives.Sphere(Vec3.Zero, 1.5),
            Primitives.Cylinder(Vec3.Zero, Vec3.UnitZ, 1.0, 3.0)
        };

        foreach (var solid in solids)
        {
            var bvh = BvhTree.Build(solid.Mesh);
            var results = new SolidClassification[1000];
            gpu.Classify(points, results, bvh, false);
        }
    }

    [Fact]
    public void GpuDistance_OnSphere_100000Points()
    {
        using var gpu = TryCreateGpu();
        if (gpu == null) return;

        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        var bvh = BvhTree.Build(sphere.Mesh);
        var rng = new Random(42);
        var points = new Vec3[100000];
        for (int i = 0; i < points.Length; i++)
            points[i] = new Vec3(rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2);
        var dists = new double[100000];
        var sw = Stopwatch.StartNew();
        gpu.Distances(points, dists, bvh);
        sw.Stop();
        _output.WriteLine($"GPU distance 100k on sphere: {sw.ElapsedMilliseconds}ms");
    }

    [Fact]
    public void GpuClassify_2000000Points()
    {
        using var gpu = TryCreateGpu();
        if (gpu == null) return;

        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var bvh = BvhTree.Build(cube.Mesh);
        var rng = new Random(42);
        var points = new Vec3[2000000];
        for (int i = 0; i < points.Length; i++)
            points[i] = new Vec3(rng.NextDouble() * 6 - 3, rng.NextDouble() * 6 - 3, rng.NextDouble() * 6 - 3);
        var results = new SolidClassification[2000000];
        var sw = Stopwatch.StartNew();
        gpu.Classify(points, results, bvh, false);
        sw.Stop();
        _output.WriteLine($"GPU classify 2M: {sw.ElapsedMilliseconds}ms");
    }

    [Fact]
    public void GpuClassify_ThenCpuVerify()
    {
        using var gpu = TryCreateGpu();
        if (gpu == null) return;

        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var bvh = BvhTree.Build(cube.Mesh);
        var inside = new Vec3[] { Vec3.Zero, new Vec3(0.5, 0.5, 0.5), new Vec3(-0.5, -0.5, -0.5) };
        var outside = new Vec3[] { new Vec3(5, 0, 0), new Vec3(0, 5, 0), new Vec3(0, 0, 5) };
        var all = inside.Concat(outside).ToArray();
        var results = new SolidClassification[all.Length];
        gpu.Classify(all, results, bvh, false);

        for (int i = 0; i < inside.Length; i++)
            Assert.Equal(SolidClassification.Inside, results[i]);
        for (int i = 0; i < outside.Length; i++)
            Assert.Equal(SolidClassification.Outside, results[inside.Length + i]);
    }

    [Fact]
    public void GpuDistance_NearSurface_SmallValues()
    {
        using var gpu = TryCreateGpu();
        if (gpu == null) return;

        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var bvh = BvhTree.Build(cube.Mesh);
        var points = new Vec3[] {
            new Vec3(0.99, 0, 0), new Vec3(1.01, 0, 0),
            new Vec3(0, 0.99, 0), new Vec3(0, 1.01, 0)
        };
        var dists = new double[4];
        gpu.Distances(points, dists, bvh);
        foreach (var d in dists)
            Assert.True(d < 0.1);
    }

    [Fact]
    public void GpuClassify_Torus()
    {
        using var gpu = TryCreateGpu();
        if (gpu == null) return;

        var torus = Primitives.Torus(Vec3.Zero, Vec3.UnitZ, 3.0, 1.0);
        var bvh = BvhTree.Build(torus.Mesh);
        var rng = new Random(42);
        var points = new Vec3[10000];
        for (int i = 0; i < points.Length; i++)
            points[i] = new Vec3(rng.NextDouble() * 10 - 5, rng.NextDouble() * 10 - 5, rng.NextDouble() * 4 - 2);
        var results = new SolidClassification[10000];
        gpu.Classify(points, results, bvh, false);
    }

    [Fact]
    public void GpuSignedDistance_100000Points()
    {
        using var gpu = TryCreateGpu();
        if (gpu == null) return;

        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var bvh = BvhTree.Build(cube.Mesh);
        var rng = new Random(42);
        var points = new Vec3[100000];
        for (int i = 0; i < points.Length; i++)
            points[i] = new Vec3(rng.NextDouble() * 6 - 3, rng.NextDouble() * 6 - 3, rng.NextDouble() * 6 - 3);
        var sd = new double[100000];
        var sw = Stopwatch.StartNew();
        gpu.SignedDistances(points, sd, bvh, false);
        sw.Stop();
        _output.WriteLine($"GPU signed distance 100k: {sw.ElapsedMilliseconds}ms");
    }

    [Fact]
    public void GpuClassify_WithWindingNumber()
    {
        using var gpu = TryCreateGpu();
        if (gpu == null) return;

        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var bvh = BvhTree.Build(cube.Mesh);
        var points = new Vec3[] { Vec3.Zero, new Vec3(5, 0, 0) };
        var results = new SolidClassification[2];
        gpu.Classify(points, results, bvh, true);
        Assert.Equal(SolidClassification.Inside, results[0]);
        Assert.Equal(SolidClassification.Outside, results[1]);
    }
}
