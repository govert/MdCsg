using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

public class ParallelismTests
{
    private static readonly CsgOptions SequentialOptions = new() { Parallel = false };
    private static readonly CsgOptions ParallelOptions = new() { Parallel = true };

    // =========================================================================
    // Sequential and parallel produce identical results
    // =========================================================================

    [Fact]
    public void Union_OverlappingCubes_ParallelMatchesSequential()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5));

        var seqResult = Csg.Union(a, b, SequentialOptions);
        var parResult = Csg.Union(a, b, ParallelOptions);

        double seqVol = VolumeCalculator.ComputeAbsoluteVolume(seqResult.Mesh);
        double parVol = VolumeCalculator.ComputeAbsoluteVolume(parResult.Mesh);

        Assert.True(System.Math.Abs(seqVol - parVol) < 0.01,
            $"Sequential vol={seqVol:F6}, Parallel vol={parVol:F6} should match");
        Assert.Equal(seqResult.FaceCount, parResult.FaceCount);
    }

    [Fact]
    public void Intersection_OverlappingCubes_ParallelMatchesSequential()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5));

        var seqResult = Csg.Intersect(a, b, SequentialOptions);
        var parResult = Csg.Intersect(a, b, ParallelOptions);

        double seqVol = VolumeCalculator.ComputeAbsoluteVolume(seqResult.Mesh);
        double parVol = VolumeCalculator.ComputeAbsoluteVolume(parResult.Mesh);

        Assert.True(System.Math.Abs(seqVol - parVol) < 0.01,
            $"Sequential vol={seqVol:F6}, Parallel vol={parVol:F6} should match");
        Assert.Equal(seqResult.FaceCount, parResult.FaceCount);
    }

    [Fact]
    public void Difference_OverlappingCubes_ParallelMatchesSequential()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5));

        var seqResult = Csg.Difference(a, b, SequentialOptions);
        var parResult = Csg.Difference(a, b, ParallelOptions);

        double seqVol = VolumeCalculator.ComputeAbsoluteVolume(seqResult.Mesh);
        double parVol = VolumeCalculator.ComputeAbsoluteVolume(parResult.Mesh);

        Assert.True(System.Math.Abs(seqVol - parVol) < 0.01,
            $"Sequential vol={seqVol:F6}, Parallel vol={parVol:F6} should match");
        Assert.Equal(seqResult.FaceCount, parResult.FaceCount);
    }

    // =========================================================================
    // Parallel with spheres (higher face count)
    // =========================================================================

    [Fact]
    public void Union_OverlappingSpheres_ParallelMatchesSequential()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var b = MeshFactory.CreateSphere(new Vec3(0.5, 0, 0), 1.0, 2);

        var seqResult = Csg.Union(a, b, SequentialOptions);
        var parResult = Csg.Union(a, b, ParallelOptions);

        double seqVol = VolumeCalculator.ComputeAbsoluteVolume(seqResult.Mesh);
        double parVol = VolumeCalculator.ComputeAbsoluteVolume(parResult.Mesh);

        Assert.True(System.Math.Abs(seqVol - parVol) < 0.1,
            $"Sequential vol={seqVol:F6}, Parallel vol={parVol:F6} should match");
    }

    [Fact]
    public void Difference_OverlappingSpheres_ParallelMatchesSequential()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var b = MeshFactory.CreateSphere(new Vec3(0.5, 0, 0), 1.0, 2);

        var seqResult = Csg.Difference(a, b, SequentialOptions);
        var parResult = Csg.Difference(a, b, ParallelOptions);

        double seqVol = VolumeCalculator.ComputeAbsoluteVolume(seqResult.Mesh);
        double parVol = VolumeCalculator.ComputeAbsoluteVolume(parResult.Mesh);

        Assert.True(System.Math.Abs(seqVol - parVol) < 0.1,
            $"Sequential vol={seqVol:F6}, Parallel vol={parVol:F6} should match");
    }

    // =========================================================================
    // HalfSpace operations with parallel
    // =========================================================================

    [Fact]
    public void Intersect_CubeHalfSpace_ParallelMatchesSequential()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2);
        var hs = HalfSpace.FromPointAndNormal(new Vec3(1, 0, 0), new Vec3(1, 0, 0));

        var seqResult = Csg.Intersect(cube, hs, SequentialOptions);
        var parResult = Csg.Intersect(cube, hs, ParallelOptions);

        double seqVol = VolumeCalculator.ComputeAbsoluteVolume(seqResult.Mesh);
        double parVol = VolumeCalculator.ComputeAbsoluteVolume(parResult.Mesh);

        Assert.True(System.Math.Abs(seqVol - parVol) < 0.01,
            $"Sequential vol={seqVol:F6}, Parallel vol={parVol:F6} should match");
    }

    // =========================================================================
    // Thread safety: repeated concurrent operations produce consistent results
    // =========================================================================

    [Fact]
    public void ParallelCsg_RepeatedOperations_ProduceConsistentResults()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5));

        var volumes = new double[20];
        var faceCounts = new int[20];

        System.Threading.Tasks.Parallel.For(0, 20, i =>
        {
            var result = Csg.Union(a, b, ParallelOptions);
            volumes[i] = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
            faceCounts[i] = result.FaceCount;
        });

        // All runs should produce the same results
        for (int i = 1; i < 20; i++)
        {
            Assert.True(System.Math.Abs(volumes[i] - volumes[0]) < 0.01,
                $"Run {i} vol={volumes[i]:F6} should match run 0 vol={volumes[0]:F6}");
            Assert.Equal(faceCounts[0], faceCounts[i]);
        }
    }

    // =========================================================================
    // Parallel produces valid meshes
    // =========================================================================

    [Fact]
    public void Union_ParallelEnabled_ProducesValidMesh()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5));

        var result = Csg.Union(a, b, ParallelOptions);

        Assert.True(result.FaceCount > 0);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        // Union of two overlapping unit cubes: volume should be ~1.875
        Assert.True(vol > 1.5 && vol < 2.2, $"Union volume should be ~1.875, got {vol}");
    }

    [Fact]
    public void Intersection_ParallelEnabled_ProducesValidMesh()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5));

        var result = Csg.Intersect(a, b, ParallelOptions);

        Assert.True(result.FaceCount > 0);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        // Intersection of two overlapping unit cubes: volume should be ~0.125
        Assert.True(vol > 0.05 && vol < 0.25, $"Intersection volume should be ~0.125, got {vol}");
    }

    [Fact]
    public void Difference_ParallelEnabled_ProducesValidMesh()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5));

        var result = Csg.Difference(a, b, ParallelOptions);

        Assert.True(result.FaceCount > 0);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        // A \ B: unit cube minus 0.5^3 overlap = 0.875
        Assert.True(vol > 0.6 && vol < 1.1, $"Difference volume should be ~0.875, got {vol}");
    }
}
