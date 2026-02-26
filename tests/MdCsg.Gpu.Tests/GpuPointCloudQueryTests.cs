using MdCsg.Api;
using MdCsg.Classification;
using MdCsg.Math;
using Xunit;

namespace MdCsg.Gpu.Tests;

/// <summary>
/// Verifies that GPU point cloud queries produce results matching CPU.
/// Tests are skipped if no Vulkan device is available.
/// </summary>
public class GpuPointCloudQueryTests : IDisposable
{
    private readonly GpuPointCloudQuery? _gpuQuery;

    public GpuPointCloudQueryTests()
    {
        _gpuQuery = GpuPointCloudQuery.TryCreate();
    }

    public void Dispose()
    {
        _gpuQuery?.Dispose();
    }

    // =========================================================================
    // Classify
    // =========================================================================

    [Fact]
    public void Classify_GpuMatchesCpu()
    {
        if (_gpuQuery == null) return;

        var cube = Primitives.Cube(new Vec3(0.5, 0.5, 0.5), 1.0);
        var pts = new Vec3[]
        {
            new(0.5, 0.5, 0.5),   // inside
            new(2.0, 2.0, 2.0),   // outside
            new(0.2, 0.8, 0.3),   // inside
            new(-1.0, -1.0, -1.0) // outside
        };

        var cpuQuery = new PointCloudQuery(cube);
        var cpuResults = cpuQuery.Classify(new PointCloud(pts), parallel: false);

        var gpuQuery = new PointCloudQuery(cube, strategy: _gpuQuery);
        var gpuResults = gpuQuery.Classify(new PointCloud(pts));

        for (int i = 0; i < pts.Length; i++)
            Assert.Equal(cpuResults[i], gpuResults[i]);
    }

    // =========================================================================
    // Distances
    // =========================================================================

    [Fact]
    public void Distances_GpuMatchesCpu_WithinFloat32Precision()
    {
        if (_gpuQuery == null) return;

        var cube = Primitives.Cube(new Vec3(0.5, 0.5, 0.5), 1.0);
        var pts = new Vec3[]
        {
            new(0.5, 0.5, 0.5),   // center, dist=0.5
            new(2.0, 0.5, 0.5),   // 1 unit from face
            new(0.2, 0.8, 0.3),   // inside
        };

        var cpuQuery = new PointCloudQuery(cube);
        var cpuResults = cpuQuery.Distances(new PointCloud(pts), parallel: false);

        var gpuQuery = new PointCloudQuery(cube, strategy: _gpuQuery);
        var gpuResults = gpuQuery.Distances(new PointCloud(pts));

        for (int i = 0; i < pts.Length; i++)
        {
            // Allow float32 precision tolerance
            Assert.True(System.Math.Abs(cpuResults[i] - gpuResults[i]) < 0.01,
                $"Point {i}: CPU={cpuResults[i]}, GPU={gpuResults[i]}");
        }
    }

    // =========================================================================
    // Signed distances
    // =========================================================================

    [Fact]
    public void SignedDistances_GpuMatchesCpu_WithinFloat32Precision()
    {
        if (_gpuQuery == null) return;

        var cube = Primitives.Cube(new Vec3(0.5, 0.5, 0.5), 1.0);
        var pts = new Vec3[]
        {
            new(0.5, 0.5, 0.5),   // inside
            new(2.0, 0.5, 0.5),   // outside
        };

        var cpuQuery = new PointCloudQuery(cube);
        var cpuResults = cpuQuery.SignedDistances(new PointCloud(pts), parallel: false);

        var gpuQuery = new PointCloudQuery(cube, strategy: _gpuQuery);
        var gpuResults = gpuQuery.SignedDistances(new PointCloud(pts));

        for (int i = 0; i < pts.Length; i++)
        {
            // Signs must match
            Assert.True(System.Math.Sign(cpuResults[i]) == System.Math.Sign(gpuResults[i]),
                $"Point {i}: sign mismatch CPU={cpuResults[i]}, GPU={gpuResults[i]}");

            // Magnitudes within float32 tolerance
            Assert.True(System.Math.Abs(System.Math.Abs(cpuResults[i]) - System.Math.Abs(gpuResults[i])) < 0.01,
                $"Point {i}: magnitude mismatch CPU={cpuResults[i]}, GPU={gpuResults[i]}");
        }
    }

    // =========================================================================
    // Large batch
    // =========================================================================

    [Fact]
    public void LargeBatch_1000Points_ClassifyCorrectly()
    {
        if (_gpuQuery == null) return;

        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var rng = new Random(42);
        var pts = new Vec3[1000];
        for (int i = 0; i < pts.Length; i++)
            pts[i] = new Vec3(rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2);

        var cpuQuery = new PointCloudQuery(sphere);
        var cpuResults = cpuQuery.Classify(new PointCloud(pts), parallel: false);

        var gpuQuery = new PointCloudQuery(sphere, strategy: _gpuQuery);
        var gpuResults = gpuQuery.Classify(new PointCloud(pts));

        int mismatches = 0;
        for (int i = 0; i < pts.Length; i++)
            if (cpuResults[i] != gpuResults[i]) mismatches++;

        // Allow small number of mismatches due to float32 precision near surfaces
        Assert.True(mismatches < pts.Length * 0.02,
            $"Too many classification mismatches: {mismatches}/{pts.Length}");
    }
}
