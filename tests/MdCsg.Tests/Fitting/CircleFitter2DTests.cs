using MdCsg.Fitting;
using MdCsg.Math;

namespace MdCsg.Tests.Fitting;

public class CircleFitter2DTests
{
    // =========================================================================
    // Points on known circle (uniform angular spacing — best case)
    // =========================================================================

    [Fact]
    public void UnitCircleAtOrigin_UniformPoints_RecoversCenterAndRadius()
    {
        var points = GenerateUniformCirclePoints(0, 0, 1.0, 50);
        var (cx, cy, r) = CircleFitter2D.Fit(points);

        Assert.True(System.Math.Abs(cx) < 1e-8, $"Center X should be ~0, got {cx}");
        Assert.True(System.Math.Abs(cy) < 1e-8, $"Center Y should be ~0, got {cy}");
        Assert.True(System.Math.Abs(r - 1.0) < 1e-8, $"Radius should be ~1, got {r}");
    }

    [Fact]
    public void UnitCircleAtOrigin_RandomPoints_RecoversCenterAndRadius()
    {
        var points = GenerateCirclePoints(0, 0, 1.0, 200, seed: 1);
        var (cx, cy, r) = CircleFitter2D.Fit(points);

        Assert.True(System.Math.Abs(cx) < 0.05, $"Center X should be ~0, got {cx}");
        Assert.True(System.Math.Abs(cy) < 0.05, $"Center Y should be ~0, got {cy}");
        Assert.True(System.Math.Abs(r - 1.0) < 0.05, $"Radius should be ~1, got {r}");
    }

    // =========================================================================
    // Off-center circles (uniform spacing for precision)
    // =========================================================================

    [Theory]
    [InlineData(3, 4, 2)]
    [InlineData(-5, 2, 1)]
    [InlineData(0, 10, 0.5)]
    [InlineData(100, -50, 10)]
    public void OffCenter_UniformPoints_RecoversCenterAndRadius(double ecx, double ecy, double er)
    {
        var points = GenerateUniformCirclePoints(ecx, ecy, er, 100);
        var (cx, cy, r) = CircleFitter2D.Fit(points);

        double tol = er * 0.01 + 0.01;
        Assert.True(System.Math.Abs(cx - ecx) < tol,
            $"Center X: expected {ecx}, got {cx}");
        Assert.True(System.Math.Abs(cy - ecy) < tol,
            $"Center Y: expected {ecy}, got {cy}");
        Assert.True(System.Math.Abs(r - er) < tol,
            $"Radius: expected {er}, got {r}");
    }

    [Theory]
    [InlineData(3, 4, 2)]
    [InlineData(-5, 2, 1)]
    [InlineData(100, -50, 10)]
    public void OffCenter_RandomPoints_RecoversCenterAndRadius(double ecx, double ecy, double er)
    {
        var points = GenerateCirclePoints(ecx, ecy, er, 200, seed: 42);
        var (cx, cy, r) = CircleFitter2D.Fit(points);

        double tol = er * 0.1 + 0.1;
        Assert.True(System.Math.Abs(cx - ecx) < tol,
            $"Center X: expected {ecx}, got {cx}");
        Assert.True(System.Math.Abs(cy - ecy) < tol,
            $"Center Y: expected {ecy}, got {cy}");
        Assert.True(System.Math.Abs(r - er) < tol,
            $"Radius: expected {er}, got {r}");
    }

    // =========================================================================
    // Various radii (uniform spacing)
    // =========================================================================

    [Theory]
    [InlineData(0.01)]
    [InlineData(0.1)]
    [InlineData(1.0)]
    [InlineData(10.0)]
    [InlineData(100.0)]
    [InlineData(1000.0)]
    public void VariousRadii_UniformPoints_RecoversRadius(double expectedRadius)
    {
        var points = GenerateUniformCirclePoints(0, 0, expectedRadius, 100);
        var (cx, cy, r) = CircleFitter2D.Fit(points);

        Assert.True(System.Math.Abs(cx) < expectedRadius * 1e-8 + 1e-8,
            $"Center X should be ~0, got {cx}");
        Assert.True(System.Math.Abs(cy) < expectedRadius * 1e-8 + 1e-8,
            $"Center Y should be ~0, got {cy}");
        Assert.True(System.Math.Abs(r - expectedRadius) < expectedRadius * 1e-8 + 1e-8,
            $"Expected radius {expectedRadius}, got {r}");
    }

    [Theory]
    [InlineData(0.1)]
    [InlineData(1.0)]
    [InlineData(10.0)]
    [InlineData(100.0)]
    public void VariousRadii_RandomPoints_RecoversRadius(double expectedRadius)
    {
        var points = GenerateCirclePoints(0, 0, expectedRadius, 500, seed: 7);
        var (_, _, r) = CircleFitter2D.Fit(points);

        Assert.True(System.Math.Abs(r - expectedRadius) < expectedRadius * 0.05,
            $"Expected radius {expectedRadius}, got {r}");
    }

    // =========================================================================
    // Minimum point count (3)
    // =========================================================================

    [Fact]
    public void ThreePoints_EquilateralTriangle_FitsCircle()
    {
        var points = new Vec2[]
        {
            new(1, 0),
            new(-0.5, System.Math.Sqrt(3) / 2),
            new(-0.5, -System.Math.Sqrt(3) / 2),
        };
        var (cx, cy, r) = CircleFitter2D.Fit(points);

        Assert.True(System.Math.Abs(cx) < 0.01, $"Center X should be ~0, got {cx}");
        Assert.True(System.Math.Abs(cy) < 0.01, $"Center Y should be ~0, got {cy}");
        Assert.True(System.Math.Abs(r - 1.0) < 0.01, $"Radius should be ~1, got {r}");
    }

    [Fact]
    public void ThreePoints_RightTriangleOnCircle()
    {
        // (1,1), (-1,1), (-1,-1) are on a circle centered ~(-0.33, 0.33) radius ~1.49
        // Actually the circumscribed circle: use the known formula
        var points = new Vec2[] { new(1, 1), new(-1, 1), new(-1, -1) };
        var (cx, cy, r) = CircleFitter2D.Fit(points);

        // Verify mean distance from center equals r
        double sumDist = 0;
        for (int i = 0; i < 3; i++)
        {
            double dx = points[i].X - cx;
            double dy = points[i].Y - cy;
            sumDist += System.Math.Sqrt(dx * dx + dy * dy);
        }
        double meanDist = sumDist / 3;
        Assert.True(System.Math.Abs(r - meanDist) < 0.01,
            $"Radius {r} should match mean distance {meanDist}");
    }

    [Fact]
    public void FourCardinalPoints_ExactCircle()
    {
        var points = new Vec2[] { new(1, 0), new(0, 1), new(-1, 0), new(0, -1) };
        var (cx, cy, r) = CircleFitter2D.Fit(points);

        Assert.True(System.Math.Abs(cx) < 1e-8, $"CX: {cx}");
        Assert.True(System.Math.Abs(cy) < 1e-8, $"CY: {cy}");
        Assert.True(System.Math.Abs(r - 1.0) < 1e-8, $"R: {r}");
    }

    // =========================================================================
    // Many points (uniform spacing gives best precision)
    // =========================================================================

    [Fact]
    public void ManyPoints_1000_Uniform_PreciseRecovery()
    {
        var points = GenerateUniformCirclePoints(2.5, -3.7, 5.0, 1000);
        var (cx, cy, r) = CircleFitter2D.Fit(points);

        Assert.True(System.Math.Abs(cx - 2.5) < 1e-8, $"Center X: {cx}");
        Assert.True(System.Math.Abs(cy - (-3.7)) < 1e-8, $"Center Y: {cy}");
        Assert.True(System.Math.Abs(r - 5.0) < 1e-8, $"Radius: {r}");
    }

    [Fact]
    public void ManyPoints_5000_Uniform_HighPrecision()
    {
        var points = GenerateUniformCirclePoints(0, 0, 1.0, 5000);
        var (cx, cy, r) = CircleFitter2D.Fit(points);

        Assert.True(System.Math.Abs(cx) < 1e-10);
        Assert.True(System.Math.Abs(cy) < 1e-10);
        Assert.True(System.Math.Abs(r - 1.0) < 1e-10);
    }

    [Fact]
    public void ManyPoints_1000_Random_ReasonablePrecision()
    {
        var points = GenerateCirclePoints(2.5, -3.7, 5.0, 1000, seed: 99);
        var (cx, cy, r) = CircleFitter2D.Fit(points);

        Assert.True(System.Math.Abs(cx - 2.5) < 0.1, $"Center X: {cx}");
        Assert.True(System.Math.Abs(cy - (-3.7)) < 0.1, $"Center Y: {cy}");
        Assert.True(System.Math.Abs(r - 5.0) < 0.1, $"Radius: {r}");
    }

    // =========================================================================
    // Half-circle arcs
    // =========================================================================

    [Fact]
    public void HalfCircle_UniformPoints_RecoversCenter()
    {
        var points = GenerateUniformArcPoints(0, 0, 2.0, 0, System.Math.PI, 50);
        var (cx, cy, r) = CircleFitter2D.Fit(points);

        Assert.True(System.Math.Abs(cx) < 0.05, $"Center X: {cx}");
        Assert.True(System.Math.Abs(cy) < 0.05, $"Center Y: {cy}");
        Assert.True(System.Math.Abs(r - 2.0) < 0.05, $"Radius: {r}");
    }

    [Fact]
    public void HalfCircle_RandomPoints_RecoversCenter()
    {
        var points = GenerateArcPoints(0, 0, 2.0, 0, System.Math.PI, 100, seed: 10);
        var (cx, cy, r) = CircleFitter2D.Fit(points);

        Assert.True(System.Math.Abs(cx) < 0.2, $"Center X: {cx}");
        Assert.True(System.Math.Abs(cy) < 0.2, $"Center Y: {cy}");
        Assert.True(System.Math.Abs(r - 2.0) < 0.2, $"Radius: {r}");
    }

    // =========================================================================
    // Quarter-circle arcs
    // =========================================================================

    [Fact]
    public void QuarterCircle_RecoversCircle()
    {
        var points = GenerateUniformArcPoints(0, 0, 3.0, 0, System.Math.PI / 2, 40);
        var (cx, cy, r) = CircleFitter2D.Fit(points);

        Assert.True(System.Math.Abs(cx) < 0.3, $"Center X: {cx}");
        Assert.True(System.Math.Abs(cy) < 0.3, $"Center Y: {cy}");
        Assert.True(System.Math.Abs(r - 3.0) < 0.3, $"Radius: {r}");
    }

    // =========================================================================
    // Noisy points
    // =========================================================================

    [Theory]
    [InlineData(0.001, 0.01)]
    [InlineData(0.01, 0.05)]
    [InlineData(0.1, 0.2)]
    public void NoisyPoints_RecoveryWithinTolerance(double noiseLevel, double tolerance)
    {
        var rng = new Random(42);
        int n = 200;
        double cx0 = 1.0, cy0 = 2.0, r0 = 3.0;
        var points = new Vec2[n];
        for (int i = 0; i < n; i++)
        {
            double angle = 2.0 * System.Math.PI * i / n;
            double noise = (rng.NextDouble() - 0.5) * 2 * noiseLevel;
            double rr = r0 + noise;
            points[i] = new Vec2(cx0 + rr * System.Math.Cos(angle),
                                  cy0 + rr * System.Math.Sin(angle));
        }
        var (cx, cy, r) = CircleFitter2D.Fit(points);

        Assert.True(System.Math.Abs(cx - cx0) < tolerance, $"CX: {cx}");
        Assert.True(System.Math.Abs(cy - cy0) < tolerance, $"CY: {cy}");
        Assert.True(System.Math.Abs(r - r0) < tolerance, $"R: {r}");
    }

    // =========================================================================
    // Evenly spaced point sets
    // =========================================================================

    [Fact]
    public void SixEvenlySpacedPoints_FitsCircle()
    {
        var points = new Vec2[6];
        for (int i = 0; i < 6; i++)
        {
            double angle = 2 * System.Math.PI * i / 6;
            points[i] = new Vec2(System.Math.Cos(angle), System.Math.Sin(angle));
        }
        var (cx, cy, r) = CircleFitter2D.Fit(points);

        Assert.True(System.Math.Abs(cx) < 1e-8);
        Assert.True(System.Math.Abs(cy) < 1e-8);
        Assert.True(System.Math.Abs(r - 1.0) < 1e-8);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(8)]
    [InlineData(12)]
    [InlineData(24)]
    [InlineData(100)]
    public void EvenlySpacedPoints_VaryingCount_FitsCircle(int count)
    {
        var points = GenerateUniformCirclePoints(0, 0, 1.0, count);
        var (cx, cy, r) = CircleFitter2D.Fit(points);

        Assert.True(System.Math.Abs(cx) < 1e-8, $"n={count}: CX={cx}");
        Assert.True(System.Math.Abs(cy) < 1e-8, $"n={count}: CY={cy}");
        Assert.True(System.Math.Abs(r - 1.0) < 1e-8, $"n={count}: R={r}");
    }

    // =========================================================================
    // Degenerate: fewer than 3 points
    // =========================================================================

    [Fact]
    public void TwoPoints_FallsBackToCentroid()
    {
        var points = new Vec2[] { new(0, 0), new(2, 0) };
        var (cx, cy, r) = CircleFitter2D.Fit(points);
        Assert.True(System.Math.Abs(cx - 1.0) < 1e-8);
        Assert.True(System.Math.Abs(cy) < 1e-8);
        Assert.True(System.Math.Abs(r - 1.0) < 1e-8);
    }

    [Fact]
    public void SinglePoint_FallsBackToCentroid()
    {
        var points = new Vec2[] { new(5, 3) };
        var (cx, cy, r) = CircleFitter2D.Fit(points);
        Assert.True(System.Math.Abs(cx - 5.0) < 1e-8);
        Assert.True(System.Math.Abs(cy - 3.0) < 1e-8);
        Assert.True(System.Math.Abs(r) < 1e-8);
    }

    // =========================================================================
    // Consistency: fitting same circle with different seeds
    // =========================================================================

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(42)]
    [InlineData(99)]
    public void DifferentSeeds_SimilarResults(int seed)
    {
        var points = GenerateCirclePoints(1.0, 2.0, 3.0, 500, seed: seed);
        var (cx, cy, r) = CircleFitter2D.Fit(points);

        Assert.True(System.Math.Abs(cx - 1.0) < 0.15, $"seed={seed}: CX={cx}");
        Assert.True(System.Math.Abs(cy - 2.0) < 0.15, $"seed={seed}: CY={cy}");
        Assert.True(System.Math.Abs(r - 3.0) < 0.15, $"seed={seed}: R={r}");
    }

    // =========================================================================
    // Helpers
    // =========================================================================

    private static Vec2[] GenerateCirclePoints(double cx, double cy, double radius, int count, int seed)
    {
        var rng = new Random(seed);
        var points = new Vec2[count];
        for (int i = 0; i < count; i++)
        {
            double angle = rng.NextDouble() * 2 * System.Math.PI;
            points[i] = new Vec2(cx + radius * System.Math.Cos(angle),
                                  cy + radius * System.Math.Sin(angle));
        }
        return points;
    }

    private static Vec2[] GenerateUniformCirclePoints(double cx, double cy, double radius, int count)
    {
        var points = new Vec2[count];
        for (int i = 0; i < count; i++)
        {
            double angle = 2 * System.Math.PI * i / count;
            points[i] = new Vec2(cx + radius * System.Math.Cos(angle),
                                  cy + radius * System.Math.Sin(angle));
        }
        return points;
    }

    private static Vec2[] GenerateArcPoints(double cx, double cy, double radius,
        double startAngle, double endAngle, int count, int seed)
    {
        var rng = new Random(seed);
        var points = new Vec2[count];
        for (int i = 0; i < count; i++)
        {
            double angle = startAngle + rng.NextDouble() * (endAngle - startAngle);
            points[i] = new Vec2(cx + radius * System.Math.Cos(angle),
                                  cy + radius * System.Math.Sin(angle));
        }
        return points;
    }

    private static Vec2[] GenerateUniformArcPoints(double cx, double cy, double radius,
        double startAngle, double endAngle, int count)
    {
        var points = new Vec2[count];
        for (int i = 0; i < count; i++)
        {
            double angle = startAngle + (endAngle - startAngle) * i / count;
            points[i] = new Vec2(cx + radius * System.Math.Cos(angle),
                                  cy + radius * System.Math.Sin(angle));
        }
        return points;
    }
}
