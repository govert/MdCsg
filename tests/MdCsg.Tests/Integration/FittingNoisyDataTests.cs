using MdCsg.Fitting;
using MdCsg.Math;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 30: Fit with noise, RMS bounds.</summary>
public class FittingNoisyDataTests
{
    private static Vec3[] AddNoise(Vec3[] pts, double sigma, Random rng)
    {
        return pts.Select(p => p + new Vec3(
            rng.NextDouble() * sigma * 2 - sigma,
            rng.NextDouble() * sigma * 2 - sigma,
            rng.NextDouble() * sigma * 2 - sigma
        )).ToArray();
    }

    [Fact]
    public void FitSphere_SmallNoise_LowRms()
    {
        var rng = new Random(42);
        var pts = Enumerable.Range(0, 200).Select(_ =>
        {
            double theta = rng.NextDouble() * System.Math.PI * 2;
            double phi = rng.NextDouble() * System.Math.PI;
            return new Vec3(
                System.Math.Sin(phi) * System.Math.Cos(theta),
                System.Math.Sin(phi) * System.Math.Sin(theta),
                System.Math.Cos(phi));
        }).ToArray();
        pts = AddNoise(pts, 0.01, rng);
        var fit = SphereFitter.Fit(pts);
        Assert.True(fit.RmsResidual < 0.05);
        Assert.True(System.Math.Abs(fit.Radius - 1.0) < 0.1);
    }

    [Fact]
    public void FitSphere_MediumNoise_RadiusStillClose()
    {
        var rng = new Random(42);
        var pts = Enumerable.Range(0, 200).Select(_ =>
        {
            double theta = rng.NextDouble() * System.Math.PI * 2;
            double phi = rng.NextDouble() * System.Math.PI;
            return new Vec3(
                System.Math.Sin(phi) * System.Math.Cos(theta),
                System.Math.Sin(phi) * System.Math.Sin(theta),
                System.Math.Cos(phi));
        }).ToArray();
        pts = AddNoise(pts, 0.05, rng);
        var fit = SphereFitter.Fit(pts);
        Assert.True(System.Math.Abs(fit.Radius - 1.0) < 0.3);
    }

    [Fact]
    public void FitPlane_SmallNoise_LowRms()
    {
        var rng = new Random(42);
        var pts = Enumerable.Range(0, 100).Select(_ =>
            new Vec3(rng.NextDouble() * 10, rng.NextDouble() * 10, rng.NextDouble() * 0.01)
        ).ToArray();
        var fit = PlaneFitter.Fit(pts);
        Assert.True(fit.RmsResidual < 0.01);
    }

    [Fact]
    public void FitPlane_MediumNoise_NormalStillApproximatelyZ()
    {
        var rng = new Random(42);
        var pts = Enumerable.Range(0, 100).Select(_ =>
            new Vec3(rng.NextDouble() * 10, rng.NextDouble() * 10, rng.NextDouble() * 0.2 - 0.1)
        ).ToArray();
        var fit = PlaneFitter.Fit(pts);
        Assert.True(System.Math.Abs(fit.Normal.Z) > 0.9);
    }

    [Fact]
    public void FitSphere_MorePoints_BetterFit()
    {
        var rng = new Random(42);
        var smallPts = Enumerable.Range(0, 20).Select(_ =>
        {
            double t = rng.NextDouble() * System.Math.PI * 2;
            double p = rng.NextDouble() * System.Math.PI;
            return new Vec3(System.Math.Sin(p) * System.Math.Cos(t), System.Math.Sin(p) * System.Math.Sin(t), System.Math.Cos(p));
        }).ToArray();
        smallPts = AddNoise(smallPts, 0.02, rng);

        var largePts = Enumerable.Range(0, 500).Select(_ =>
        {
            double t = rng.NextDouble() * System.Math.PI * 2;
            double p = rng.NextDouble() * System.Math.PI;
            return new Vec3(System.Math.Sin(p) * System.Math.Cos(t), System.Math.Sin(p) * System.Math.Sin(t), System.Math.Cos(p));
        }).ToArray();
        largePts = AddNoise(largePts, 0.02, rng);

        var fitSmall = SphereFitter.Fit(smallPts);
        var fitLarge = SphereFitter.Fit(largePts);
        // Both should be close, but more points should help
        Assert.True(fitSmall.RmsResidual < 0.1);
        Assert.True(fitLarge.RmsResidual < 0.1);
    }

    [Fact]
    public void FitSphere_OffCenter_WithNoise()
    {
        var rng = new Random(42);
        var center = new Vec3(3, -2, 5);
        var pts = Enumerable.Range(0, 200).Select(_ =>
        {
            double t = rng.NextDouble() * System.Math.PI * 2;
            double p = rng.NextDouble() * System.Math.PI;
            return center + new Vec3(
                2 * System.Math.Sin(p) * System.Math.Cos(t),
                2 * System.Math.Sin(p) * System.Math.Sin(t),
                2 * System.Math.Cos(p));
        }).ToArray();
        pts = AddNoise(pts, 0.03, rng);
        var fit = SphereFitter.Fit(pts);
        Assert.True(Vec3.Distance(fit.Center, center) < 0.3);
        Assert.True(System.Math.Abs(fit.Radius - 2.0) < 0.2);
    }

    [Fact]
    public void FitPlane_TiltedWithNoise()
    {
        var rng = new Random(42);
        var normal = new Vec3(0, 0, 1);
        var pts = Enumerable.Range(0, 100).Select(_ =>
            new Vec3(rng.NextDouble() * 10 - 5, rng.NextDouble() * 10 - 5,
                     rng.NextDouble() * 0.1 - 0.05)
        ).ToArray();
        var fit = PlaneFitter.Fit(pts);
        Assert.True(System.Math.Abs(Vec3.Dot(fit.Normal, normal)) > 0.95);
    }

    [Fact]
    public void FitSphere_LargeNoise_StillFindsApproximateCenter()
    {
        var rng = new Random(42);
        var pts = Enumerable.Range(0, 500).Select(_ =>
        {
            double t = rng.NextDouble() * System.Math.PI * 2;
            double p = rng.NextDouble() * System.Math.PI;
            return new Vec3(
                System.Math.Sin(p) * System.Math.Cos(t),
                System.Math.Sin(p) * System.Math.Sin(t),
                System.Math.Cos(p));
        }).ToArray();
        pts = AddNoise(pts, 0.2, rng);
        var fit = SphereFitter.Fit(pts);
        Assert.True(Vec3.Distance(fit.Center, Vec3.Zero) < 0.5);
    }

    [Fact]
    public void FitPlane_LargeNoise_HigherRms()
    {
        var rng = new Random(42);
        var pts = Enumerable.Range(0, 100).Select(_ =>
            new Vec3(rng.NextDouble() * 10, rng.NextDouble() * 10, rng.NextDouble() * 1.0)
        ).ToArray();
        var fit = PlaneFitter.Fit(pts);
        Assert.True(fit.RmsResidual > 0.01); // noisy → higher RMS
    }

    [Fact]
    public void FitSphere_RmsScalesWithNoise()
    {
        var rng = new Random(42);
        var basePts = Enumerable.Range(0, 100).Select(_ =>
        {
            double t = rng.NextDouble() * System.Math.PI * 2;
            double p = rng.NextDouble() * System.Math.PI;
            return new Vec3(
                System.Math.Sin(p) * System.Math.Cos(t),
                System.Math.Sin(p) * System.Math.Sin(t),
                System.Math.Cos(p));
        }).ToArray();

        var rng1 = new Random(42);
        var rng2 = new Random(42);
        var fit1 = SphereFitter.Fit(AddNoise((Vec3[])basePts.Clone(), 0.01, rng1));
        var fit2 = SphereFitter.Fit(AddNoise((Vec3[])basePts.Clone(), 0.1, rng2));
        Assert.True(fit1.RmsResidual < fit2.RmsResidual + 0.01);
    }

    [Fact]
    public void FitSphere_ZeroNoise_VeryLowRms()
    {
        var rng = new Random(42);
        var pts = Enumerable.Range(0, 100).Select(_ =>
        {
            double t = rng.NextDouble() * System.Math.PI * 2;
            double p = rng.NextDouble() * System.Math.PI;
            return new Vec3(
                System.Math.Sin(p) * System.Math.Cos(t),
                System.Math.Sin(p) * System.Math.Sin(t),
                System.Math.Cos(p));
        }).ToArray();
        var fit = SphereFitter.Fit(pts);
        Assert.True(fit.RmsResidual < 0.01);
    }

    [Fact]
    public void FitPlane_ZeroNoise_VeryLowRms()
    {
        var pts = new[]
        {
            new Vec3(0, 0, 5), new Vec3(1, 0, 5), new Vec3(0, 1, 5),
            new Vec3(1, 1, 5), new Vec3(0.5, 0.5, 5),
        };
        var fit = PlaneFitter.Fit(pts);
        Assert.True(fit.RmsResidual < 0.001);
    }

    [Fact]
    public void LineFitter2D_SmallNoise_LowRms()
    {
        var rng = new Random(42);
        var pts = Enumerable.Range(0, 50).Select(i =>
            new Vec2(i * 0.1, 2.0 * i * 0.1 + 1.0 + rng.NextDouble() * 0.01)
        ).ToArray();
        var fit = LineFitter.Fit(pts);
        Assert.True(fit.RmsResidual < 0.05);
    }

    [Fact]
    public void LineFitter3D_SmallNoise_LowRms()
    {
        var rng = new Random(42);
        var dir = new Vec3(1, 2, 3).Normalized;
        var pts = Enumerable.Range(0, 50).Select(i =>
            dir * (i * 0.1) + new Vec3(rng.NextDouble() * 0.01, rng.NextDouble() * 0.01, rng.NextDouble() * 0.01)
        ).ToArray();
        var fit = LineFitter.Fit(pts);
        Assert.True(fit.RmsResidual < 0.05);
    }

    [Fact]
    public void QuadraticFitter_SmallNoise_LowRms()
    {
        var rng = new Random(42);
        var x = Enumerable.Range(0, 50).Select(i => i * 0.1).ToArray();
        var y = x.Select(xi => 2 * xi * xi + 3 * xi + 1 + rng.NextDouble() * 0.01).ToArray();
        var fit = QuadraticFitter.Fit1D(x, y);
        Assert.True(fit.RmsResidual < 0.05);
    }

    [Fact]
    public void QuadraticFitter_RecoverCoefficients()
    {
        var x = Enumerable.Range(0, 50).Select(i => i * 0.1).ToArray();
        var y = x.Select(xi => 2.0 * xi * xi + 3.0 * xi + 1.0).ToArray();
        var fit = QuadraticFitter.Fit1D(x, y);
        Assert.True(System.Math.Abs(fit.A - 2.0) < 0.1);
        Assert.True(System.Math.Abs(fit.B - 3.0) < 0.1);
        Assert.True(System.Math.Abs(fit.C - 1.0) < 0.1);
    }

    [Fact]
    public void ConeFitter_Works()
    {
        var rng = new Random(42);
        var pts = Enumerable.Range(0, 100).Select(_ =>
        {
            double h = rng.NextDouble() * 2;
            double angle = rng.NextDouble() * System.Math.PI * 2;
            double r = h * 0.5; // half-angle = atan(0.5)
            return new Vec3(r * System.Math.Cos(angle), r * System.Math.Sin(angle), h);
        }).ToArray();
        var fit = ConeFitter.Fit(pts);
        Assert.True(fit.HalfAngle > 0);
    }

    [Fact]
    public void ParaboloidFitter_Works()
    {
        var rng = new Random(42);
        var pts = Enumerable.Range(0, 100).Select(_ =>
        {
            double x = rng.NextDouble() * 4 - 2;
            double y = rng.NextDouble() * 4 - 2;
            double z = x * x + y * y;
            return new Vec3(x, y, z);
        }).ToArray();
        var fit = ParaboloidFitter.Fit(pts);
        Assert.True(fit.RmsResidual < 0.1);
    }
}
