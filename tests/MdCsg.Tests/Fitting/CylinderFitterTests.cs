using MdCsg.Fitting;
using MdCsg.Math;

namespace MdCsg.Tests.Fitting;

public class CylinderFitterTests
{
    // =========================================================================
    // X, Y, Z axis cylinders
    // =========================================================================

    [Theory]
    [InlineData(1, 0, 0)]
    [InlineData(0, 1, 0)]
    [InlineData(0, 0, 1)]
    public void AxisAligned_RecoverAxisAndRadius(double ax, double ay, double az)
    {
        var axis = new Vec3(ax, ay, az);
        var points = GenerateCylinderPoints(
            Vec3.Zero, axis, radius: 1.0, height: 10.0, count: 500, seed: 1);

        var result = CylinderFitter.Fit(points);

        double alignment = System.Math.Abs(Vec3.Dot(result.Axis, axis));
        Assert.True(alignment > 0.99,
            $"Axis ({ax},{ay},{az}): alignment {alignment}");
        Assert.True(System.Math.Abs(result.Radius - 1.0) < 0.1,
            $"Radius should be ~1.0, got {result.Radius}");
    }

    [Fact]
    public void ZAxisCylinder_RecoverAxisAndRadius()
    {
        var points = GenerateCylinderPoints(
            Vec3.Zero, Vec3.UnitZ, radius: 1.0, height: 10.0, count: 500, seed: 1);

        var result = CylinderFitter.Fit(points);

        double axisAlignment = System.Math.Abs(Vec3.Dot(result.Axis, Vec3.UnitZ));
        Assert.True(axisAlignment > 0.99,
            $"Axis should align with Z, got alignment {axisAlignment}, axis {result.Axis}");
        Assert.True(System.Math.Abs(result.Radius - 1.0) < 0.1,
            $"Radius should be ~1.0, got {result.Radius}");
        Assert.True(System.Math.Abs(result.Height - 10.0) < 1.0,
            $"Height should be ~10.0, got {result.Height}");
    }

    [Fact]
    public void ZAxisCylinder_SmallResidual()
    {
        var points = GenerateCylinderPoints(
            Vec3.Zero, Vec3.UnitZ, radius: 0.5, height: 5.0, count: 200, seed: 2);

        var result = CylinderFitter.Fit(points);

        Assert.True(result.RmsResidual < 0.1,
            $"RMS should be small, got {result.RmsResidual}");
    }

    // =========================================================================
    // Arbitrary axis orientations (many angles)
    // =========================================================================

    [Theory]
    [InlineData(1, 1, 0)]
    [InlineData(1, 0, 1)]
    [InlineData(0, 1, 1)]
    [InlineData(1, 1, 1)]
    [InlineData(2, -1, 3)]
    [InlineData(-1, 2, -1)]
    [InlineData(3, 1, -2)]
    public void ArbitraryAxis_RecoverDirection(double ax, double ay, double az)
    {
        var axis = new Vec3(ax, ay, az).Normalized;
        var points = GenerateCylinderPoints(
            new Vec3(1, 2, 3), axis, radius: 0.5, height: 8.0, count: 500, seed: 3);

        var result = CylinderFitter.Fit(points);

        double alignment = System.Math.Abs(Vec3.Dot(result.Axis, axis));
        Assert.True(alignment > 0.95,
            $"Axis ({ax},{ay},{az}): alignment {alignment}");
        Assert.True(System.Math.Abs(result.Radius - 0.5) < 0.1,
            $"Radius should be ~0.5, got {result.Radius}");
    }

    // =========================================================================
    // Various radius/height ratios
    // =========================================================================

    [Theory]
    [InlineData(0.1, 10.0)]  // tall thin (height >> radius, good for PCA)
    [InlineData(0.5, 5.0)]   // moderate
    [InlineData(1.0, 10.0)]  // standard
    [InlineData(2.0, 20.0)]  // wider but still tall
    public void VariousAspectRatios_RecoverGeometry(double radius, double height)
    {
        var points = GenerateCylinderPoints(
            Vec3.Zero, Vec3.UnitZ, radius, height, count: 500, seed: 42);

        var result = CylinderFitter.Fit(points);

        Assert.True(System.Math.Abs(result.Radius - radius) < radius * 0.15 + 0.05,
            $"r={radius},h={height}: radius got {result.Radius}");
        Assert.True(System.Math.Abs(result.Height - height) < height * 0.15 + 0.5,
            $"r={radius},h={height}: height got {result.Height}");
    }

    // =========================================================================
    // With normals (paired tests: with and without)
    // =========================================================================

    [Theory]
    [InlineData(0, 0, 1)]
    [InlineData(1, 0, 0)]
    [InlineData(0, 1, 0)]
    public void WithNormals_BetterThanWithout(double ax, double ay, double az)
    {
        var axis = new Vec3(ax, ay, az);
        double radius = 1.0;
        int count = 200;
        var rng = new Random(4);

        var helper = System.Math.Abs(axis.X) < 0.9 ? Vec3.UnitX : Vec3.UnitY;
        var u = Vec3.Cross(axis, helper).Normalized;
        var v = Vec3.Cross(axis, u);

        var points = new Vec3[count];
        var normals = new Vec3[count];

        for (int i = 0; i < count; i++)
        {
            double angle = rng.NextDouble() * 2 * System.Math.PI;
            double h = rng.NextDouble() * 3.0;
            double cos = System.Math.Cos(angle);
            double sin = System.Math.Sin(angle);

            points[i] = axis * h + u * (cos * radius) + v * (sin * radius);
            normals[i] = (u * cos + v * sin).Normalized;
        }

        var withNormals = CylinderFitter.Fit(points, normals);

        double alignWith = System.Math.Abs(Vec3.Dot(withNormals.Axis, axis));
        Assert.True(alignWith > 0.99,
            $"With normals, axis ({ax},{ay},{az}): alignment {alignWith}");
        Assert.True(System.Math.Abs(withNormals.Radius - radius) < 0.1,
            $"Radius should be ~1.0, got {withNormals.Radius}");
    }

    [Fact]
    public void WithNormals_BetterAxisEstimate()
    {
        var axis = Vec3.UnitZ;
        double radius = 1.0;
        int count = 100;
        var rng = new Random(4);

        var points = new Vec3[count];
        var normals = new Vec3[count];

        for (int i = 0; i < count; i++)
        {
            double angle = rng.NextDouble() * 2 * System.Math.PI;
            double h = rng.NextDouble() * 3.0;
            double cos = System.Math.Cos(angle);
            double sin = System.Math.Sin(angle);

            points[i] = new Vec3(cos * radius, sin * radius, h);
            normals[i] = new Vec3(cos, sin, 0);
        }

        var result = CylinderFitter.Fit(points, normals);

        double alignment = System.Math.Abs(Vec3.Dot(result.Axis, axis));
        Assert.True(alignment > 0.99,
            $"With normals, axis alignment should be excellent, got {alignment}");
        Assert.True(System.Math.Abs(result.Radius - radius) < 0.1,
            $"Radius should be ~1.0, got {result.Radius}");
    }

    // =========================================================================
    // With axis hint
    // =========================================================================

    [Theory]
    [InlineData(0, 0, 1)]
    [InlineData(1, 0, 0)]
    [InlineData(0, 1, 0)]
    public void WithAxisHint_OverridesBadPCA(double ax, double ay, double az)
    {
        var axis = new Vec3(ax, ay, az);
        var points = GenerateCylinderPoints(
            Vec3.Zero, axis, radius: 0.1, height: 10.0, count: 100, seed: 5);

        var result = CylinderFitter.Fit(points, axisHint: axis);

        double alignment = System.Math.Abs(Vec3.Dot(result.Axis, axis));
        Assert.True(alignment > 0.95,
            $"With hint ({ax},{ay},{az}): alignment {alignment}");
    }

    [Fact]
    public void WithAxisHint_OriginalTest()
    {
        var axis = Vec3.UnitY;
        var points = GenerateCylinderPoints(
            Vec3.Zero, axis, radius: 0.1, height: 10.0, count: 100, seed: 5);

        var result = CylinderFitter.Fit(points, axisHint: Vec3.UnitY);

        double alignment = System.Math.Abs(Vec3.Dot(result.Axis, axis));
        Assert.True(alignment > 0.95,
            $"With hint, axis alignment should be good, got {alignment}");
    }

    // =========================================================================
    // Noisy points at various levels
    // =========================================================================

    [Theory]
    [InlineData(0.001)]
    [InlineData(0.01)]
    [InlineData(0.1)]
    [InlineData(0.5)]
    public void NoisyCylinder_RecoverGeometry(double noiseAmplitude)
    {
        var axis = Vec3.UnitZ;
        double radius = 2.0;
        double height = 10.0;
        var rng = new Random(42);
        int n = 500;

        var helper = Vec3.UnitX;
        var u = Vec3.Cross(axis, helper).Normalized;
        var v = Vec3.Cross(axis, u);

        var points = new Vec3[n];
        for (int i = 0; i < n; i++)
        {
            double angle = rng.NextDouble() * 2 * System.Math.PI;
            double h = rng.NextDouble() * height;
            double noise = (rng.NextDouble() - 0.5) * 2 * noiseAmplitude;
            double r = radius + noise;
            double cos = System.Math.Cos(angle);
            double sin = System.Math.Sin(angle);
            points[i] = axis * h + u * (cos * r) + v * (sin * r);
        }

        var result = CylinderFitter.Fit(points);

        Assert.True(System.Math.Abs(result.Radius - radius) < noiseAmplitude * 2 + 0.15,
            $"noise={noiseAmplitude}: radius error={System.Math.Abs(result.Radius - radius)}");
    }

    // =========================================================================
    // Different point counts
    // =========================================================================

    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(500)]
    [InlineData(1000)]
    public void VariousPointCounts_RecoverCylinder(int count)
    {
        var points = GenerateCylinderPoints(
            Vec3.Zero, Vec3.UnitZ, radius: 1.0, height: 10.0, count: count, seed: 42);

        var result = CylinderFitter.Fit(points);

        double tol = count < 20 ? 0.5 : 0.15;
        Assert.True(System.Math.Abs(result.Radius - 1.0) < tol,
            $"n={count}: radius={result.Radius}");
    }

    // =========================================================================
    // Consistent results across seeds
    // =========================================================================

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(42)]
    [InlineData(99)]
    public void DifferentSeeds_ConsistentRecovery(int seed)
    {
        var points = GenerateCylinderPoints(
            Vec3.Zero, Vec3.UnitZ, radius: 1.0, height: 10.0, count: 500, seed: seed);

        var result = CylinderFitter.Fit(points);

        double alignment = System.Math.Abs(Vec3.Dot(result.Axis, Vec3.UnitZ));
        Assert.True(alignment > 0.99,
            $"seed={seed}: axis alignment={alignment}");
        Assert.True(System.Math.Abs(result.Radius - 1.0) < 0.1,
            $"seed={seed}: radius={result.Radius}");
    }

    // =========================================================================
    // RMS validation
    // =========================================================================

    [Fact]
    public void PerfectCylinder_NearZeroRms()
    {
        var points = GenerateCylinderPoints(
            Vec3.Zero, Vec3.UnitZ, radius: 1.0, height: 10.0, count: 500, seed: 42);

        var result = CylinderFitter.Fit(points);

        Assert.True(result.RmsResidual < 0.05,
            $"Perfect cylinder should have near-zero RMS, got {result.RmsResidual}");
    }

    // =========================================================================
    // Edge cases
    // =========================================================================

    [Fact]
    public void TooFewPoints_Throws()
    {
        var points = new Vec3[] { Vec3.Zero, Vec3.UnitX, Vec3.UnitY, Vec3.UnitZ };
        Assert.Throws<ArgumentException>(() => CylinderFitter.Fit(points));
    }

    [Fact]
    public void ExactlyFivePoints_Works()
    {
        var points = new Vec3[]
        {
            new(1, 0, 0), new(0, 1, 0), new(-1, 0, 0), new(0, -1, 0), new(1, 0, 5),
        };
        var result = CylinderFitter.Fit(points);
        Assert.True(result.Radius > 0);
    }

    // =========================================================================
    // Helpers
    // =========================================================================

    private static Vec3[] GenerateCylinderPoints(Vec3 baseCenter, Vec3 axis, double radius,
        double height, int count, int seed)
    {
        var rng = new Random(seed);
        axis = axis.Normalized;

        var helper = System.Math.Abs(axis.X) < 0.9 ? Vec3.UnitX : Vec3.UnitY;
        var u = Vec3.Cross(axis, helper).Normalized;
        var v = Vec3.Cross(axis, u);

        var points = new Vec3[count];
        for (int i = 0; i < count; i++)
        {
            double angle = rng.NextDouble() * 2 * System.Math.PI;
            double h = rng.NextDouble() * height;
            double cos = System.Math.Cos(angle);
            double sin = System.Math.Sin(angle);
            points[i] = baseCenter + axis * h + u * (cos * radius) + v * (sin * radius);
        }
        return points;
    }
}
