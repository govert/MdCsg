using MdCsg.Fitting;
using MdCsg.Math;

namespace MdCsg.Tests.Fitting;

public class LineFitterTests
{
    // ── 2D Line Fit ─────────────────────────────────────────────────

    [Fact]
    public void Fit2D_PerfectLine_ZeroResidual()
    {
        // y = 2x + 1
        var points = new Vec2[] { new(0, 1), new(1, 3), new(2, 5), new(3, 7) };
        var result = LineFitter.Fit(points);
        Assert.True(System.Math.Abs(result.Slope - 2) < 1e-10);
        Assert.True(System.Math.Abs(result.Intercept - 1) < 1e-10);
        Assert.True(result.RmsResidual < 1e-10);
    }

    [Fact]
    public void Fit2D_HorizontalLine_ZeroSlope()
    {
        var points = new Vec2[] { new(0, 5), new(1, 5), new(2, 5) };
        var result = LineFitter.Fit(points);
        Assert.True(System.Math.Abs(result.Slope) < 1e-10);
        Assert.True(System.Math.Abs(result.Intercept - 5) < 1e-10);
    }

    [Fact]
    public void Fit2D_NoisyData_ReasonableSlope()
    {
        // y = x + noise
        var rng = new Random(42);
        var points = new Vec2[50];
        for (int i = 0; i < 50; i++)
            points[i] = new Vec2(i, i + (rng.NextDouble() - 0.5) * 0.2);

        var result = LineFitter.Fit(points);
        Assert.True(System.Math.Abs(result.Slope - 1) < 0.05);
        Assert.True(result.RmsResidual < 0.2);
    }

    [Fact]
    public void Fit2D_TwoPoints_PerfectFit()
    {
        var points = new Vec2[] { new(0, 0), new(1, 1) };
        var result = LineFitter.Fit(points);
        Assert.True(System.Math.Abs(result.Slope - 1) < 1e-10);
        Assert.True(System.Math.Abs(result.Intercept) < 1e-10);
    }

    [Fact]
    public void Fit2D_TooFewPoints_Throws()
    {
        Assert.Throws<ArgumentException>(() => LineFitter.Fit(new Vec2[] { new(0, 0) }));
    }

    // ── 3D Line Fit ─────────────────────────────────────────────────

    [Fact]
    public void Fit3D_PointsAlongXAxis_RecoverXDirection()
    {
        var points = new Vec3[10];
        for (int i = 0; i < 10; i++)
            points[i] = new Vec3(i, 0, 0);

        var result = LineFitter.Fit(points);
        // Direction should be along X axis
        double dot = System.Math.Abs(Vec3.Dot(result.Direction, Vec3.UnitX));
        Assert.True(dot > 0.99, $"Direction {result.Direction} should be along X, dot={dot}");
        Assert.True(result.RmsResidual < 1e-10);
    }

    [Fact]
    public void Fit3D_DiagonalLine()
    {
        var dir = new Vec3(1, 1, 1).Normalized;
        var points = new Vec3[20];
        for (int i = 0; i < 20; i++)
            points[i] = dir * i;

        var result = LineFitter.Fit(points);
        double dot = System.Math.Abs(Vec3.Dot(result.Direction, dir));
        Assert.True(dot > 0.99, $"Direction should be along (1,1,1), dot={dot}");
    }

    [Fact]
    public void Fit3D_NoisyData_ReasonableDirection()
    {
        var rng = new Random(42);
        var dir = Vec3.UnitZ;
        var points = new Vec3[50];
        for (int i = 0; i < 50; i++)
        {
            points[i] = dir * i + new Vec3(
                (rng.NextDouble() - 0.5) * 0.1,
                (rng.NextDouble() - 0.5) * 0.1,
                0);
        }

        var result = LineFitter.Fit(points);
        double dot = System.Math.Abs(Vec3.Dot(result.Direction, dir));
        Assert.True(dot > 0.99, $"Noisy Z-line: direction={result.Direction}, dot={dot}");
    }

    [Fact]
    public void Fit3D_TooFewPoints_Throws()
    {
        Assert.Throws<ArgumentException>(() => LineFitter.Fit(new Vec3[] { Vec3.Zero }));
    }

    [Fact]
    public void Fit3D_CentroidIsOnLine()
    {
        var points = new Vec3[] { new(0, 0, 0), new(2, 0, 0), new(4, 0, 0) };
        var result = LineFitter.Fit(points);
        // Centroid should be (2,0,0)
        Assert.True(System.Math.Abs(result.Point.X - 2) < 1e-10);
    }
}
