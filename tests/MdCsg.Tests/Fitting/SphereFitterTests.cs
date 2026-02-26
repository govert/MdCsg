using MdCsg.Fitting;
using MdCsg.Math;

namespace MdCsg.Tests.Fitting;

public class SphereFitterTests
{
    // =========================================================================
    // Unit sphere at origin
    // =========================================================================

    [Fact]
    public void UnitSphere_RecoverCenterAndRadius()
    {
        var center = Vec3.Zero;
        double radius = 1.0;
        var points = GenerateSpherePoints(center, radius, 50, seed: 1);

        var result = SphereFitter.Fit(points);

        Assert.True(Vec3.Distance(result.Center, center) < 0.05,
            $"Center should be near origin, got {result.Center}");
        Assert.True(System.Math.Abs(result.Radius - radius) < 0.05,
            $"Radius should be ~1.0, got {result.Radius}");
        Assert.True(result.RmsResidual < 0.05,
            $"RMS should be small, got {result.RmsResidual}");
    }

    // =========================================================================
    // Offset spheres (various centers)
    // =========================================================================

    [Theory]
    [InlineData(3, -2, 7, 2.5)]
    [InlineData(0, 0, 100, 1.0)]
    [InlineData(-10, -10, -10, 5.0)]
    [InlineData(0.5, 0.5, 0.5, 0.3)]
    [InlineData(100, 200, 300, 10.0)]
    public void OffCenterSphere_RecoverCenter(double cx, double cy, double cz, double radius)
    {
        var center = new Vec3(cx, cy, cz);
        var points = GenerateSpherePoints(center, radius, 100, seed: 2);

        var result = SphereFitter.Fit(points);

        double tol = radius * 0.05 + 0.05;
        Assert.True(Vec3.Distance(result.Center, center) < tol,
            $"Center error: {Vec3.Distance(result.Center, center)}, expected < {tol}");
        Assert.True(System.Math.Abs(result.Radius - radius) < tol,
            $"Radius error: {System.Math.Abs(result.Radius - radius)}");
    }

    // =========================================================================
    // Large / small radii
    // =========================================================================

    [Theory]
    [InlineData(0.01)]
    [InlineData(0.1)]
    [InlineData(1.0)]
    [InlineData(10.0)]
    [InlineData(100.0)]
    [InlineData(1000.0)]
    public void VariousRadii_RecoverRadius(double radius)
    {
        var center = Vec3.Zero;
        var points = GenerateSpherePoints(center, radius, 100, seed: 3);

        var result = SphereFitter.Fit(points);

        Assert.True(System.Math.Abs(result.Radius - radius) < radius * 0.05,
            $"Radius should be ~{radius}, got {result.Radius}");
    }

    // =========================================================================
    // Varying noise levels
    // =========================================================================

    [Theory]
    [InlineData(0.001)]
    [InlineData(0.01)]
    [InlineData(0.1)]
    [InlineData(0.5)]
    public void NoisySphere_RmsScalesWithNoise(double noiseAmplitude)
    {
        var center = new Vec3(1, 1, 1);
        double radius = 5.0;
        var rng = new Random(42);
        int n = 200;
        var points = new Vec3[n];
        for (int i = 0; i < n; i++)
        {
            var dir = RandomDirection(rng);
            double noise = (rng.NextDouble() - 0.5) * 2 * noiseAmplitude;
            points[i] = center + dir * (radius + noise);
        }

        var result = SphereFitter.Fit(points);

        Assert.True(Vec3.Distance(result.Center, center) < noiseAmplitude * 2 + 0.1,
            $"noise={noiseAmplitude}: center error={Vec3.Distance(result.Center, center)}");
        Assert.True(System.Math.Abs(result.Radius - radius) < noiseAmplitude * 2 + 0.1,
            $"noise={noiseAmplitude}: radius error={System.Math.Abs(result.Radius - radius)}");
        Assert.True(result.RmsResidual < noiseAmplitude * 2 + 0.01,
            $"noise={noiseAmplitude}: RMS={result.RmsResidual}");
    }

    // =========================================================================
    // Different point counts
    // =========================================================================

    [Theory]
    [InlineData(4)]
    [InlineData(6)]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(500)]
    [InlineData(1000)]
    public void VariousPointCounts_RecoverSphere(int count)
    {
        var center = Vec3.Zero;
        double radius = 2.0;
        var points = GenerateSpherePoints(center, radius, count, seed: 42);

        var result = SphereFitter.Fit(points);

        double tol = count < 10 ? 0.5 : 0.1;
        Assert.True(Vec3.Distance(result.Center, center) < tol,
            $"n={count}: center error={Vec3.Distance(result.Center, center)}");
        Assert.True(System.Math.Abs(result.Radius - radius) < tol,
            $"n={count}: radius error={System.Math.Abs(result.Radius - radius)}");
    }

    // =========================================================================
    // Hemisphere subsets
    // =========================================================================

    [Fact]
    public void UpperHemisphere_StillRecoversSphere()
    {
        var center = Vec3.Zero;
        double radius = 2.0;
        var rng = new Random(42);
        var pts = new List<Vec3>();
        while (pts.Count < 100)
        {
            var dir = RandomDirection(rng);
            if (dir.Z > 0) // upper hemisphere only
                pts.Add(center + dir * radius);
        }

        var result = SphereFitter.Fit(pts.ToArray());

        Assert.True(Vec3.Distance(result.Center, center) < 0.3,
            $"Center: {result.Center}");
        Assert.True(System.Math.Abs(result.Radius - radius) < 0.3,
            $"Radius: {result.Radius}");
    }

    [Fact]
    public void SphereCap_TopOnly_ReasonableFit()
    {
        var center = Vec3.Zero;
        double radius = 3.0;
        var rng = new Random(42);
        var pts = new List<Vec3>();
        // Only top cap (z > 0.5 * radius)
        while (pts.Count < 100)
        {
            var dir = RandomDirection(rng);
            if (dir.Z > 0.5)
                pts.Add(center + dir * radius);
        }

        var result = SphereFitter.Fit(pts.ToArray());

        // With only a cap, the fit is less precise but should still be reasonable
        Assert.True(Vec3.Distance(result.Center, center) < 1.0,
            $"Center: {result.Center}");
    }

    // =========================================================================
    // Consistent center recovery across seeds
    // =========================================================================

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(42)]
    [InlineData(99)]
    [InlineData(123)]
    public void DifferentSeeds_ConsistentRecovery(int seed)
    {
        var center = new Vec3(5, -3, 7);
        double radius = 4.0;
        var points = GenerateSpherePoints(center, radius, 200, seed: seed);

        var result = SphereFitter.Fit(points);

        Assert.True(Vec3.Distance(result.Center, center) < 0.2,
            $"seed={seed}: center error={Vec3.Distance(result.Center, center)}");
        Assert.True(System.Math.Abs(result.Radius - radius) < 0.2,
            $"seed={seed}: radius error={System.Math.Abs(result.Radius - radius)}");
    }

    // =========================================================================
    // Exact points (axis-aligned)
    // =========================================================================

    [Fact]
    public void ExactlySixPoints_OnSphere()
    {
        var center = Vec3.Zero;
        double radius = 1.0;
        var points = new Vec3[]
        {
            center + Vec3.UnitX * radius,
            center - Vec3.UnitX * radius,
            center + Vec3.UnitY * radius,
            center - Vec3.UnitY * radius,
            center + Vec3.UnitZ * radius,
            center - Vec3.UnitZ * radius,
        };

        var result = SphereFitter.Fit(points);

        Assert.True(Vec3.Distance(result.Center, center) < 1e-8,
            $"Center should be at origin, got {result.Center}");
        Assert.True(System.Math.Abs(result.Radius - radius) < 1e-8,
            $"Radius should be 1.0, got {result.Radius}");
    }

    [Fact]
    public void SixAxisPoints_OffCenter()
    {
        var center = new Vec3(5, 10, -3);
        double radius = 2.0;
        var points = new Vec3[]
        {
            center + Vec3.UnitX * radius,
            center - Vec3.UnitX * radius,
            center + Vec3.UnitY * radius,
            center - Vec3.UnitY * radius,
            center + Vec3.UnitZ * radius,
            center - Vec3.UnitZ * radius,
        };

        var result = SphereFitter.Fit(points);

        Assert.True(Vec3.Distance(result.Center, center) < 1e-6,
            $"Center should be at {center}, got {result.Center}");
        Assert.True(System.Math.Abs(result.Radius - radius) < 1e-6,
            $"Radius should be {radius}, got {result.Radius}");
    }

    [Fact]
    public void EightCornerPoints_OnSphere()
    {
        // Vertices of a cube inscribed in a sphere
        double r = System.Math.Sqrt(3); // radius = sqrt(3) for unit cube corners
        var points = new Vec3[]
        {
            new(1, 1, 1), new(1, 1, -1), new(1, -1, 1), new(1, -1, -1),
            new(-1, 1, 1), new(-1, 1, -1), new(-1, -1, 1), new(-1, -1, -1),
        };

        var result = SphereFitter.Fit(points);

        Assert.True(Vec3.Distance(result.Center, Vec3.Zero) < 1e-6,
            $"Center: {result.Center}");
        Assert.True(System.Math.Abs(result.Radius - r) < 1e-6,
            $"Radius should be sqrt(3)={r}, got {result.Radius}");
    }

    // =========================================================================
    // Edge cases
    // =========================================================================

    [Fact]
    public void TooFewPoints_Throws()
    {
        var points = new Vec3[] { Vec3.Zero, Vec3.UnitX, Vec3.UnitY };
        Assert.Throws<ArgumentException>(() => SphereFitter.Fit(points));
    }

    [Fact]
    public void ExactlyFour_MinimumPoints()
    {
        var points = new Vec3[]
        {
            new(1, 0, 0), new(-1, 0, 0), new(0, 1, 0), new(0, -1, 0),
        };
        var result = SphereFitter.Fit(points);

        Assert.True(Vec3.Distance(result.Center, Vec3.Zero) < 0.5,
            $"Center: {result.Center}");
    }

    // =========================================================================
    // Helpers
    // =========================================================================

    private static Vec3[] GenerateSpherePoints(Vec3 center, double radius, int count, int seed)
    {
        var rng = new Random(seed);
        var points = new Vec3[count];
        for (int i = 0; i < count; i++)
            points[i] = center + RandomDirection(rng) * radius;
        return points;
    }

    private static Vec3 RandomDirection(Random rng)
    {
        double u, v, s;
        do
        {
            u = rng.NextDouble() * 2 - 1;
            v = rng.NextDouble() * 2 - 1;
            s = u * u + v * v;
        } while (s >= 1.0 || s < 1e-10);

        double factor = 2.0 * System.Math.Sqrt(1 - s);
        return new Vec3(u * factor, v * factor, 1 - 2 * s);
    }
}
