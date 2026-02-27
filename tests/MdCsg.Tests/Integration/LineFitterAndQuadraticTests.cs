using MdCsg.Fitting;
using MdCsg.Math;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 32: LineFitter 2D/3D, QuadraticFitter stress.</summary>
public class LineFitterAndQuadraticTests
{
    [Fact]
    public void LineFitter2D_HorizontalLine()
    {
        var pts = new[] { new Vec2(0, 3), new Vec2(1, 3), new Vec2(2, 3), new Vec2(3, 3) };
        var fit = LineFitter.Fit(pts);
        Assert.True(System.Math.Abs(fit.Slope) < 0.01);
        Assert.True(System.Math.Abs(fit.Intercept - 3.0) < 0.01);
    }

    [Fact]
    public void LineFitter2D_PositiveSlope()
    {
        var pts = Enumerable.Range(0, 20).Select(i => new Vec2(i, 2 * i + 1)).ToArray();
        var fit = LineFitter.Fit(pts);
        Assert.True(System.Math.Abs(fit.Slope - 2.0) < 0.01);
        Assert.True(System.Math.Abs(fit.Intercept - 1.0) < 0.01);
    }

    [Fact]
    public void LineFitter2D_NegativeSlope()
    {
        var pts = Enumerable.Range(0, 20).Select(i => new Vec2(i, -0.5 * i + 10)).ToArray();
        var fit = LineFitter.Fit(pts);
        Assert.True(System.Math.Abs(fit.Slope + 0.5) < 0.01);
    }

    [Fact]
    public void LineFitter2D_RmsNearZero_PerfectLine()
    {
        var pts = Enumerable.Range(0, 10).Select(i => new Vec2(i, 3 * i + 2)).ToArray();
        var fit = LineFitter.Fit(pts);
        Assert.True(fit.RmsResidual < 0.001);
    }

    [Fact]
    public void LineFitter3D_AlongXAxis()
    {
        var pts = Enumerable.Range(0, 20).Select(i => new Vec3(i, 0, 0)).ToArray();
        var fit = LineFitter.Fit(pts);
        Assert.True(System.Math.Abs(System.Math.Abs(fit.Direction.X) - 1.0) < 0.01);
        Assert.True(fit.RmsResidual < 0.001);
    }

    [Fact]
    public void LineFitter3D_AlongDiagonal()
    {
        var dir = new Vec3(1, 1, 1).Normalized;
        var pts = Enumerable.Range(0, 20).Select(i => dir * i).ToArray();
        var fit = LineFitter.Fit(pts);
        Assert.True(System.Math.Abs(System.Math.Abs(Vec3.Dot(fit.Direction, dir)) - 1.0) < 0.01);
    }

    [Fact]
    public void LineFitter3D_WithNoise_LowRms()
    {
        var rng = new Random(42);
        var pts = Enumerable.Range(0, 50).Select(i =>
            new Vec3(i * 0.1, 0, 0) + new Vec3(0, rng.NextDouble() * 0.01, rng.NextDouble() * 0.01)
        ).ToArray();
        var fit = LineFitter.Fit(pts);
        Assert.True(fit.RmsResidual < 0.05);
    }

    [Fact]
    public void QuadraticFitter_Parabola_RecoverCoefficients()
    {
        var x = Enumerable.Range(0, 50).Select(i => i * 0.1).ToArray();
        var y = x.Select(xi => 1.5 * xi * xi - 2.0 * xi + 3.0).ToArray();
        var fit = QuadraticFitter.Fit1D(x, y);
        Assert.True(System.Math.Abs(fit.A - 1.5) < 0.01);
        Assert.True(System.Math.Abs(fit.B + 2.0) < 0.01);
        Assert.True(System.Math.Abs(fit.C - 3.0) < 0.01);
    }

    [Fact]
    public void QuadraticFitter_LinearData_AIsNearZero()
    {
        var x = Enumerable.Range(0, 50).Select(i => i * 0.1).ToArray();
        var y = x.Select(xi => 3.0 * xi + 1.0).ToArray();
        var fit = QuadraticFitter.Fit1D(x, y);
        Assert.True(System.Math.Abs(fit.A) < 0.01);
    }

    [Fact]
    public void QuadraticFitter_NoisyData_StillReasonable()
    {
        var rng = new Random(42);
        var x = Enumerable.Range(0, 100).Select(i => i * 0.1).ToArray();
        var y = x.Select(xi => 2.0 * xi * xi + xi + 1 + rng.NextDouble() * 0.5).ToArray();
        var fit = QuadraticFitter.Fit1D(x, y);
        Assert.True(System.Math.Abs(fit.A - 2.0) < 0.3);
    }

    [Fact]
    public void QuadraticFitter_NegativeA_InvertedParabola()
    {
        var x = Enumerable.Range(0, 50).Select(i => i * 0.1 - 2.5).ToArray();
        var y = x.Select(xi => -xi * xi + 5).ToArray();
        var fit = QuadraticFitter.Fit1D(x, y);
        Assert.True(fit.A < 0);
    }

    [Fact]
    public void QuadraticFitter_RmsIsZero_PerfectData()
    {
        var x = Enumerable.Range(0, 20).Select(i => i * 0.5).ToArray();
        var y = x.Select(xi => xi * xi).ToArray();
        var fit = QuadraticFitter.Fit1D(x, y);
        Assert.True(fit.RmsResidual < 0.001);
    }

    [Fact]
    public void LineFitter2D_TwoPoints_PerfectFit()
    {
        var pts = new[] { new Vec2(0, 0), new Vec2(1, 1) };
        var fit = LineFitter.Fit(pts);
        Assert.True(System.Math.Abs(fit.Slope - 1.0) < 0.01);
        Assert.True(fit.RmsResidual < 0.01);
    }

    [Fact]
    public void LineFitter3D_TwoPoints_PerfectFit()
    {
        var pts = new[] { new Vec3(0, 0, 0), new Vec3(1, 2, 3) };
        var fit = LineFitter.Fit(pts);
        Assert.True(fit.RmsResidual < 0.01);
    }

    [Fact]
    public void QuadraticFitter_ThreePoints_PerfectFit()
    {
        var x = new[] { 0.0, 1.0, 2.0 };
        var y = new[] { 1.0, 4.0, 9.0 }; // y = x² + 2x + 1
        var fit = QuadraticFitter.Fit1D(x, y);
        Assert.True(fit.RmsResidual < 0.01);
    }

    [Fact]
    public void LineFitter2D_LargeDataset()
    {
        var pts = Enumerable.Range(0, 1000).Select(i => new Vec2(i * 0.01, 2 * i * 0.01 + 3)).ToArray();
        var fit = LineFitter.Fit(pts);
        Assert.True(System.Math.Abs(fit.Slope - 2.0) < 0.01);
        Assert.True(System.Math.Abs(fit.Intercept - 3.0) < 0.01);
    }

    [Fact]
    public void LineFitter3D_LargeDataset()
    {
        var pts = Enumerable.Range(0, 1000).Select(i => new Vec3(i * 0.01, 0, 0)).ToArray();
        var fit = LineFitter.Fit(pts);
        Assert.True(fit.RmsResidual < 0.001);
    }

    [Fact]
    public void QuadraticFitter_LargeDataset()
    {
        var x = Enumerable.Range(0, 500).Select(i => i * 0.01).ToArray();
        var y = x.Select(xi => 3.0 * xi * xi + 2.0 * xi + 1.0).ToArray();
        var fit = QuadraticFitter.Fit1D(x, y);
        Assert.True(System.Math.Abs(fit.A - 3.0) < 0.01);
    }
}
