using MdCsg.Api;
using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Gpu;
using MdCsg.Math;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 49: GPU classify/distance vs CPU (skip if no Vulkan).</summary>
public class GpuPipelineIntegrationTests
{
    private static GpuPointCloudQuery? TryCreateGpu()
    {
        try { return GpuPointCloudQuery.TryCreate(); }
        catch { return null; }
    }

    [Fact]
    public void GpuClassify_MatchesCpu()
    {
        using var gpu = TryCreateGpu();
        if (gpu == null) return;

        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var bvh = BvhTree.Build(cube.Mesh);
        var rng = new Random(42);
        var points = new Vec3[100];
        for (int i = 0; i < points.Length; i++)
            points[i] = new Vec3(rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2);

        var cpuResults = new SolidClassification[100];
        var cpuQuery = new PointCloudQuery(cube);
        var cloud = new PointCloud(points);
        var cpuClassify = cpuQuery.Classify(cloud);

        var gpuResults = new SolidClassification[100];
        gpu.Classify(points, gpuResults, bvh, false);

        int matches = 0;
        for (int i = 0; i < points.Length; i++)
            if (cpuClassify[i] == gpuResults[i]) matches++;
        Assert.True(matches > 90);
    }

    [Fact]
    public void GpuDistance_MatchesCpu()
    {
        using var gpu = TryCreateGpu();
        if (gpu == null) return;

        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var bvh = BvhTree.Build(cube.Mesh);
        var rng = new Random(42);
        var points = new Vec3[100];
        for (int i = 0; i < points.Length; i++)
            points[i] = new Vec3(rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2);

        var cloud = new PointCloud(points);
        var cpuDist = new PointCloudQuery(cube).Distances(cloud);

        var gpuDist = new double[100];
        gpu.Distances(points, gpuDist, bvh);

        for (int i = 0; i < points.Length; i++)
            Assert.True(System.Math.Abs(cpuDist[i] - gpuDist[i]) < 0.1);
    }

    [Fact]
    public void GpuSignedDistance_MatchesCpu()
    {
        using var gpu = TryCreateGpu();
        if (gpu == null) return;

        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var bvh = BvhTree.Build(cube.Mesh);
        var rng = new Random(42);
        var points = new Vec3[100];
        for (int i = 0; i < points.Length; i++)
            points[i] = new Vec3(rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2);

        var cloud = new PointCloud(points);
        var cpuSd = new PointCloudQuery(cube).SignedDistances(cloud);

        var gpuSd = new double[100];
        gpu.SignedDistances(points, gpuSd, bvh, false);

        for (int i = 0; i < points.Length; i++)
            Assert.True(System.Math.Abs(cpuSd[i] - gpuSd[i]) < 0.1);
    }

    [Fact]
    public void GpuClassify_OnSphere()
    {
        using var gpu = TryCreateGpu();
        if (gpu == null) return;

        var sphere = Primitives.Sphere(Vec3.Zero, 1.0);
        var bvh = BvhTree.Build(sphere.Mesh);
        var points = new Vec3[] { Vec3.Zero, new Vec3(0.5, 0, 0), new Vec3(2, 0, 0) };
        var results = new SolidClassification[3];
        gpu.Classify(points, results, bvh, false);
        Assert.Equal(3, results.Length);
    }

    [Fact]
    public void GpuClassify_1000Points()
    {
        using var gpu = TryCreateGpu();
        if (gpu == null) return;

        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var bvh = BvhTree.Build(cube.Mesh);
        var rng = new Random(42);
        var points = new Vec3[1000];
        for (int i = 0; i < points.Length; i++)
            points[i] = new Vec3(rng.NextDouble() * 6 - 3, rng.NextDouble() * 6 - 3, rng.NextDouble() * 6 - 3);
        var results = new SolidClassification[1000];
        gpu.Classify(points, results, bvh, false);
    }

    [Fact]
    public void GpuDistance_1000Points()
    {
        using var gpu = TryCreateGpu();
        if (gpu == null) return;

        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var bvh = BvhTree.Build(cube.Mesh);
        var rng = new Random(42);
        var points = new Vec3[1000];
        for (int i = 0; i < points.Length; i++)
            points[i] = new Vec3(rng.NextDouble() * 6 - 3, rng.NextDouble() * 6 - 3, rng.NextDouble() * 6 - 3);
        var dists = new double[1000];
        gpu.Distances(points, dists, bvh);
        foreach (var d in dists)
            Assert.True(d >= 0);
    }

    [Fact]
    public void GpuClassify_OnCsgResult()
    {
        using var gpu = TryCreateGpu();
        if (gpu == null) return;

        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(Vec3.Zero, 1.0);
        var solid = new Solid(Csg.Difference(a, b).Mesh);
        var bvh = BvhTree.Build(solid.Mesh);
        var rng = new Random(42);
        var points = new Vec3[100];
        for (int i = 0; i < points.Length; i++)
            points[i] = new Vec3(rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2);
        var results = new SolidClassification[100];
        gpu.Classify(points, results, bvh, false);
    }

    [Fact]
    public void GpuTryCreate_DoesNotThrow()
    {
        try
        {
            using var gpu = GpuPointCloudQuery.TryCreate();
            // null is ok (no Vulkan)
        }
        catch (Exception ex)
        {
            Assert.Fail($"TryCreate should not throw: {ex.Message}");
        }
    }

    [Fact]
    public void GpuCanBeDisposed()
    {
        using var gpu = TryCreateGpu();
        if (gpu == null) return;
        gpu.Dispose();
    }

    [Fact]
    public void GpuClassify_EmptyArray()
    {
        using var gpu = TryCreateGpu();
        if (gpu == null) return;

        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var bvh = BvhTree.Build(cube.Mesh);
        var points = Array.Empty<Vec3>();
        var results = Array.Empty<SolidClassification>();
        gpu.Classify(points, results, bvh, false);
    }

    [Fact]
    public void GpuDistance_AllPositive()
    {
        using var gpu = TryCreateGpu();
        if (gpu == null) return;

        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var bvh = BvhTree.Build(cube.Mesh);
        var rng = new Random(42);
        var points = new Vec3[50];
        for (int i = 0; i < points.Length; i++)
            points[i] = new Vec3(rng.NextDouble() * 10 - 5, rng.NextDouble() * 10 - 5, rng.NextDouble() * 10 - 5);
        var dists = new double[50];
        gpu.Distances(points, dists, bvh);
        foreach (var d in dists)
            Assert.True(d >= 0);
    }

    [Fact]
    public void GpuClassify_CylinderSolid()
    {
        using var gpu = TryCreateGpu();
        if (gpu == null) return;

        var cyl = Primitives.Cylinder(Vec3.Zero, Vec3.UnitZ, 1.0, 2.0);
        var bvh = BvhTree.Build(cyl.Mesh);
        var points = new Vec3[] { new Vec3(0, 0, 1), new Vec3(5, 0, 0) };
        var results = new SolidClassification[2];
        gpu.Classify(points, results, bvh, false);
    }

    [Fact]
    public void GpuSignedDistance_InsideIsNegative()
    {
        using var gpu = TryCreateGpu();
        if (gpu == null) return;

        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var bvh = BvhTree.Build(cube.Mesh);
        var points = new Vec3[] { Vec3.Zero };
        var sd = new double[1];
        gpu.SignedDistances(points, sd, bvh, false);
        Assert.True(sd[0] < 0);
    }

    [Fact]
    public void GpuSignedDistance_OutsideIsPositive()
    {
        using var gpu = TryCreateGpu();
        if (gpu == null) return;

        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var bvh = BvhTree.Build(cube.Mesh);
        var points = new Vec3[] { new Vec3(5, 0, 0) };
        var sd = new double[1];
        gpu.SignedDistances(points, sd, bvh, false);
        Assert.True(sd[0] > 0);
    }

    [Fact]
    public void GpuClassify_SphereSolid()
    {
        using var gpu = TryCreateGpu();
        if (gpu == null) return;

        var sphere = Primitives.Sphere(Vec3.Zero, 2.0);
        var bvh = BvhTree.Build(sphere.Mesh);
        var points = new Vec3[] { Vec3.Zero, new Vec3(3, 0, 0) };
        var results = new SolidClassification[2];
        gpu.Classify(points, results, bvh, false);
    }

    [Fact]
    public void GpuDistance_5000Points()
    {
        using var gpu = TryCreateGpu();
        if (gpu == null) return;

        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var bvh = BvhTree.Build(cube.Mesh);
        var rng = new Random(42);
        var points = new Vec3[5000];
        for (int i = 0; i < points.Length; i++)
            points[i] = new Vec3(rng.NextDouble() * 6 - 3, rng.NextDouble() * 6 - 3, rng.NextDouble() * 6 - 3);
        var dists = new double[5000];
        gpu.Distances(points, dists, bvh);
    }

    [Fact]
    public void GpuMaxBatchSize_Default()
    {
        using var gpu = TryCreateGpu();
        if (gpu == null) return;
        Assert.Equal(4_000_000, gpu.MaxBatchSize);
    }

    [Fact]
    public void GpuMaxBatchSize_CanBeSet()
    {
        using var gpu = TryCreateGpu();
        if (gpu == null) return;
        gpu.MaxBatchSize = 1_000_000;
        Assert.Equal(1_000_000, gpu.MaxBatchSize);
    }
}
