using MdCsg.Fitting;
using MdCsg.Math;

namespace MdCsg.Tests.Fitting;

public class PlaneFitterTests
{
    // =========================================================================
    // Axis-aligned planes
    // =========================================================================

    [Fact]
    public void XYPlane_RecoverNormalZ()
    {
        var points = new Vec3[]
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0),
            new(1, 1, 0), new(0.5, 0.5, 0),
        };
        var result = PlaneFitter.Fit(points);

        Assert.True(System.Math.Abs(System.Math.Abs(result.Normal.Z) - 1.0) < 1e-8,
            $"Expected normal along Z, got {result.Normal}");
        Assert.True(result.RmsResidual < 1e-10,
            $"Expected near-zero residual, got {result.RmsResidual}");
    }

    [Fact]
    public void XZPlane_RecoverNormalY()
    {
        var points = new Vec3[]
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 0, 1),
            new(1, 0, 1), new(0.5, 0, 0.5),
        };
        var result = PlaneFitter.Fit(points);

        Assert.True(System.Math.Abs(System.Math.Abs(result.Normal.Y) - 1.0) < 1e-8,
            $"Expected normal along Y, got {result.Normal}");
    }

    [Fact]
    public void YZPlane_RecoverNormalX()
    {
        var points = new Vec3[]
        {
            new(0, 0, 0), new(0, 1, 0), new(0, 0, 1),
            new(0, 1, 1), new(0, 0.5, 0.5),
        };
        var result = PlaneFitter.Fit(points);

        Assert.True(System.Math.Abs(System.Math.Abs(result.Normal.X) - 1.0) < 1e-8,
            $"Expected normal along X, got {result.Normal}");
    }

    // =========================================================================
    // Offset planes
    // =========================================================================

    [Theory]
    [InlineData(0, 0, 1, 5)]    // z = 5
    [InlineData(0, 0, 1, -3)]   // z = -3
    [InlineData(1, 0, 0, 10)]   // x = 10
    [InlineData(0, 1, 0, -7)]   // y = -7
    [InlineData(0, 0, 1, 0.01)] // z = 0.01
    [InlineData(0, 0, 1, 100)]  // z = 100
    public void OffsetPlane_CentroidAtCorrectOffset(double nx, double ny, double nz, double offset)
    {
        var normal = new Vec3(nx, ny, nz);
        var rng = new Random(42);
        int n = 20;
        var points = new Vec3[n];

        // Generate points on the plane normal.dot(p) = offset
        var helper = System.Math.Abs(normal.X) < 0.9 ? Vec3.UnitX : Vec3.UnitY;
        var u = Vec3.Cross(normal, helper).Normalized;
        var v = Vec3.Cross(normal, u);

        for (int i = 0; i < n; i++)
        {
            double s = (rng.NextDouble() - 0.5) * 10;
            double t = (rng.NextDouble() - 0.5) * 10;
            points[i] = normal * offset + u * s + v * t;
        }

        var result = PlaneFitter.Fit(points);

        double alignment = System.Math.Abs(Vec3.Dot(result.Normal, normal));
        Assert.True(alignment > 0.999, $"Normal alignment: {alignment}");
        Assert.True(result.RmsResidual < 1e-8, $"RMS: {result.RmsResidual}");
    }

    [Fact]
    public void OffsetPlane_CentroidOnPlane()
    {
        var points = new Vec3[]
        {
            new(0, 0, 5), new(1, 0, 5), new(0, 1, 5),
            new(1, 1, 5), new(2, 3, 5),
        };
        var result = PlaneFitter.Fit(points);

        Assert.True(System.Math.Abs(result.Centroid.Z - 5.0) < 1e-10);
        Assert.True(System.Math.Abs(System.Math.Abs(result.Normal.Z) - 1.0) < 1e-8);
    }

    // =========================================================================
    // Tilted planes (multiple orientations)
    // =========================================================================

    [Fact]
    public void TiltedPlane_111_RecoverNormal()
    {
        var normal = new Vec3(1, 1, 1).Normalized;
        var points = new Vec3[]
        {
            new(1, -1, 0), new(0, 1, -1), new(-1, 0, 1),
            new(2, -1, -1), new(-1, 2, -1),
        };
        var result = PlaneFitter.Fit(points);

        double alignment = System.Math.Abs(Vec3.Dot(result.Normal, normal));
        Assert.True(alignment > 0.999,
            $"Normal should align with (1,1,1)/sqrt(3), got alignment {alignment}");
    }

    [Theory]
    [InlineData(1, 1, 0)]
    [InlineData(1, 0, 1)]
    [InlineData(0, 1, 1)]
    [InlineData(1, 2, 3)]
    [InlineData(3, -1, 2)]
    [InlineData(-2, 5, 1)]
    public void TiltedPlane_VariousNormals_RecoverDirection(double nx, double ny, double nz)
    {
        var normal = new Vec3(nx, ny, nz).Normalized;
        var rng = new Random(42);

        var helper = System.Math.Abs(normal.X) < 0.9 ? Vec3.UnitX : Vec3.UnitY;
        var u = Vec3.Cross(normal, helper).Normalized;
        var v = Vec3.Cross(normal, u);

        int n = 20;
        var points = new Vec3[n];
        for (int i = 0; i < n; i++)
        {
            double s = (rng.NextDouble() - 0.5) * 10;
            double t = (rng.NextDouble() - 0.5) * 10;
            points[i] = u * s + v * t;
        }

        var result = PlaneFitter.Fit(points);
        double alignment = System.Math.Abs(Vec3.Dot(result.Normal, normal));
        Assert.True(alignment > 0.999, $"Normal ({nx},{ny},{nz}): alignment {alignment}");
    }

    // =========================================================================
    // Varying noise levels
    // =========================================================================

    [Theory]
    [InlineData(0.001, 0.005)]
    [InlineData(0.01, 0.02)]
    [InlineData(0.1, 0.15)]
    [InlineData(0.5, 0.6)]
    public void NoisyPlane_RmsScalesWithNoise(double noiseAmplitude, double maxExpectedRms)
    {
        var rng = new Random(42);
        int n = 200;
        var points = new Vec3[n];
        for (int i = 0; i < n; i++)
        {
            double x = rng.NextDouble() * 10;
            double y = rng.NextDouble() * 10;
            double noise = (rng.NextDouble() - 0.5) * 2 * noiseAmplitude;
            points[i] = new Vec3(x, y, noise);
        }

        var result = PlaneFitter.Fit(points);

        Assert.True(System.Math.Abs(System.Math.Abs(result.Normal.Z) - 1.0) < 0.1,
            $"Normal should be ~Z, got {result.Normal}");
        Assert.True(result.RmsResidual < maxExpectedRms,
            $"RMS {result.RmsResidual} should be < {maxExpectedRms}");
    }

    [Fact]
    public void NoisyPlane_SmallResidual()
    {
        var rng = new Random(42);
        int n = 100;
        var points = new Vec3[n];
        for (int i = 0; i < n; i++)
        {
            double x = rng.NextDouble() * 10;
            double y = rng.NextDouble() * 10;
            double noise = (rng.NextDouble() - 0.5) * 0.01;
            points[i] = new Vec3(x, y, noise);
        }

        var result = PlaneFitter.Fit(points);

        Assert.True(System.Math.Abs(System.Math.Abs(result.Normal.Z) - 1.0) < 0.01,
            $"Normal should be ~Z, got {result.Normal}");
        Assert.True(result.RmsResidual < 0.01,
            $"RMS residual should be small, got {result.RmsResidual}");
    }

    // =========================================================================
    // Different point counts
    // =========================================================================

    [Theory]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(500)]
    [InlineData(1000)]
    public void VariousPointCounts_RecoverPlane(int count)
    {
        var rng = new Random(42);
        var points = new Vec3[count];
        for (int i = 0; i < count; i++)
        {
            double x = rng.NextDouble() * 10;
            double y = rng.NextDouble() * 10;
            points[i] = new Vec3(x, y, 0);
        }

        var result = PlaneFitter.Fit(points);

        Assert.True(System.Math.Abs(System.Math.Abs(result.Normal.Z) - 1.0) < 1e-6,
            $"n={count}: Normal.Z alignment off, got {result.Normal}");
        Assert.True(result.RmsResidual < 1e-8,
            $"n={count}: RMS should be near zero, got {result.RmsResidual}");
    }

    // =========================================================================
    // Points on large/small planes
    // =========================================================================

    [Theory]
    [InlineData(0.001)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(10000)]
    public void PointSpread_VariousScales(double scale)
    {
        var rng = new Random(42);
        int n = 30;
        var points = new Vec3[n];
        for (int i = 0; i < n; i++)
        {
            double x = (rng.NextDouble() - 0.5) * scale;
            double y = (rng.NextDouble() - 0.5) * scale;
            points[i] = new Vec3(x, y, 0);
        }

        var result = PlaneFitter.Fit(points);

        Assert.True(System.Math.Abs(System.Math.Abs(result.Normal.Z) - 1.0) < 1e-6,
            $"scale={scale}: Normal.Z alignment off");
    }

    // =========================================================================
    // Normal orientation consistency
    // =========================================================================

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(42)]
    [InlineData(99)]
    public void DifferentSeeds_NormalConsistent(int seed)
    {
        var rng = new Random(seed);
        int n = 50;
        var points = new Vec3[n];
        for (int i = 0; i < n; i++)
        {
            double x = rng.NextDouble() * 10;
            double y = rng.NextDouble() * 10;
            points[i] = new Vec3(x, y, 0);
        }

        var result = PlaneFitter.Fit(points);

        Assert.True(System.Math.Abs(System.Math.Abs(result.Normal.Z) - 1.0) < 1e-6,
            $"seed={seed}: Normal not aligned with Z");
    }

    // =========================================================================
    // Minimum points (3) works correctly
    // =========================================================================

    [Fact]
    public void ExactlyThreePoints_Fits()
    {
        var points = new Vec3[]
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0),
        };
        var result = PlaneFitter.Fit(points);

        Assert.True(System.Math.Abs(System.Math.Abs(result.Normal.Z) - 1.0) < 1e-8);
        Assert.True(result.RmsResidual < 1e-8);
    }

    [Fact]
    public void ExactlyThreePoints_Tilted()
    {
        var points = new Vec3[]
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 0, 1),
        };
        var result = PlaneFitter.Fit(points);

        Assert.True(System.Math.Abs(System.Math.Abs(result.Normal.Y) - 1.0) < 1e-8);
    }

    // =========================================================================
    // Noisy tilted plane
    // =========================================================================

    [Theory]
    [InlineData(1, 1, 1, 0.01)]
    [InlineData(1, 0, 0, 0.1)]
    [InlineData(0, 1, 0, 0.05)]
    [InlineData(1, 2, 3, 0.01)]
    public void NoisyTiltedPlane_RecoverNormal(double nx, double ny, double nz, double noiseAmp)
    {
        var normal = new Vec3(nx, ny, nz).Normalized;
        var rng = new Random(42);

        var helper = System.Math.Abs(normal.X) < 0.9 ? Vec3.UnitX : Vec3.UnitY;
        var u = Vec3.Cross(normal, helper).Normalized;
        var v = Vec3.Cross(normal, u);

        int n = 200;
        var points = new Vec3[n];
        for (int i = 0; i < n; i++)
        {
            double s = (rng.NextDouble() - 0.5) * 10;
            double t = (rng.NextDouble() - 0.5) * 10;
            double noise = (rng.NextDouble() - 0.5) * 2 * noiseAmp;
            points[i] = u * s + v * t + normal * noise;
        }

        var result = PlaneFitter.Fit(points);

        double alignment = System.Math.Abs(Vec3.Dot(result.Normal, normal));
        Assert.True(alignment > 0.99,
            $"Normal ({nx},{ny},{nz}): alignment {alignment}");
    }

    // =========================================================================
    // Edge cases
    // =========================================================================

    [Fact]
    public void TooFewPoints_Throws()
    {
        var points = new Vec3[] { Vec3.Zero, Vec3.UnitX };
        Assert.Throws<ArgumentException>(() => PlaneFitter.Fit(points));
    }

    [Fact]
    public void SinglePoint_Throws()
    {
        Assert.Throws<ArgumentException>(() => PlaneFitter.Fit(new[] { Vec3.Zero }));
    }

    [Fact]
    public void EmptyArray_Throws()
    {
        Assert.Throws<ArgumentException>(() => PlaneFitter.Fit(Array.Empty<Vec3>()));
    }
}
