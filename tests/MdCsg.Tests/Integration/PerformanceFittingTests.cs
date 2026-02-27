using System.Diagnostics;
using MdCsg.Fitting;
using MdCsg.Math;
using Xunit.Abstractions;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 46: Timed: fitting 10k+ points.</summary>
public class PerformanceFittingTests
{
    private readonly ITestOutputHelper _output;

    public PerformanceFittingTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void PlaneFit_1000Points_Under2Seconds()
    {
        var rng = new Random(42);
        var pts = Enumerable.Range(0, 1000).Select(_ =>
            new Vec3(rng.NextDouble() * 10, rng.NextDouble() * 10, rng.NextDouble() * 0.01)).ToArray();
        var sw = Stopwatch.StartNew();
        var fit = PlaneFitter.Fit(pts);
        sw.Stop();
        _output.WriteLine($"PlaneFit 1k: {sw.ElapsedMilliseconds}ms, rms={fit.RmsResidual:F6}");
        Assert.True(sw.ElapsedMilliseconds < 2000);
    }

    [Fact]
    public void PlaneFit_10000Points_Under5Seconds()
    {
        var rng = new Random(42);
        var pts = Enumerable.Range(0, 10000).Select(_ =>
            new Vec3(rng.NextDouble() * 10, rng.NextDouble() * 10, rng.NextDouble() * 0.01)).ToArray();
        var sw = Stopwatch.StartNew();
        var fit = PlaneFitter.Fit(pts);
        sw.Stop();
        _output.WriteLine($"PlaneFit 10k: {sw.ElapsedMilliseconds}ms, rms={fit.RmsResidual:F6}");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public void SphereFit_1000Points_Under2Seconds()
    {
        var rng = new Random(42);
        var pts = Enumerable.Range(0, 1000).Select(_ =>
        {
            var dir = new Vec3(rng.NextDouble() * 2 - 1, rng.NextDouble() * 2 - 1, rng.NextDouble() * 2 - 1).Normalized;
            return dir * 5.0 + new Vec3(rng.NextDouble() * 0.01, rng.NextDouble() * 0.01, rng.NextDouble() * 0.01);
        }).ToArray();
        var sw = Stopwatch.StartNew();
        var fit = SphereFitter.Fit(pts);
        sw.Stop();
        _output.WriteLine($"SphereFit 1k: {sw.ElapsedMilliseconds}ms, r={fit.Radius:F4}, rms={fit.RmsResidual:F6}");
        Assert.True(sw.ElapsedMilliseconds < 2000);
    }

    [Fact]
    public void SphereFit_10000Points_Under5Seconds()
    {
        var rng = new Random(42);
        var pts = Enumerable.Range(0, 10000).Select(_ =>
        {
            var dir = new Vec3(rng.NextDouble() * 2 - 1, rng.NextDouble() * 2 - 1, rng.NextDouble() * 2 - 1).Normalized;
            return dir * 5.0 + new Vec3(rng.NextDouble() * 0.01, rng.NextDouble() * 0.01, rng.NextDouble() * 0.01);
        }).ToArray();
        var sw = Stopwatch.StartNew();
        var fit = SphereFitter.Fit(pts);
        sw.Stop();
        _output.WriteLine($"SphereFit 10k: {sw.ElapsedMilliseconds}ms, r={fit.Radius:F4}");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public void CylinderFit_1000Points_Under5Seconds()
    {
        var rng = new Random(42);
        var axis = Vec3.UnitZ;
        var pts = Enumerable.Range(0, 1000).Select(_ =>
        {
            double angle = rng.NextDouble() * 2 * System.Math.PI;
            double h = rng.NextDouble() * 10;
            return new Vec3(3.0 * System.Math.Cos(angle), 3.0 * System.Math.Sin(angle), h)
                + new Vec3(rng.NextDouble() * 0.01, rng.NextDouble() * 0.01, 0);
        }).ToArray();
        var sw = Stopwatch.StartNew();
        var fit = CylinderFitter.Fit(pts, axisHint: axis);
        sw.Stop();
        _output.WriteLine($"CylinderFit 1k: {sw.ElapsedMilliseconds}ms, r={fit.Radius:F4}, rms={fit.RmsResidual:F6}");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public void CylinderFit_10000Points_Under10Seconds()
    {
        var rng = new Random(42);
        var axis = Vec3.UnitZ;
        var pts = Enumerable.Range(0, 10000).Select(_ =>
        {
            double angle = rng.NextDouble() * 2 * System.Math.PI;
            double h = rng.NextDouble() * 10;
            return new Vec3(3.0 * System.Math.Cos(angle), 3.0 * System.Math.Sin(angle), h)
                + new Vec3(rng.NextDouble() * 0.01, rng.NextDouble() * 0.01, 0);
        }).ToArray();
        var sw = Stopwatch.StartNew();
        var fit = CylinderFitter.Fit(pts, axisHint: axis);
        sw.Stop();
        _output.WriteLine($"CylinderFit 10k: {sw.ElapsedMilliseconds}ms, r={fit.Radius:F4}");
        Assert.True(sw.ElapsedMilliseconds < 10000);
    }

    [Fact]
    public void LineFit2D_10000Points_Under2Seconds()
    {
        var pts = Enumerable.Range(0, 10000).Select(i => new Vec2(i * 0.001, 2.0 * i * 0.001 + 1.0)).ToArray();
        var sw = Stopwatch.StartNew();
        var fit = LineFitter.Fit(pts);
        sw.Stop();
        _output.WriteLine($"LineFit2D 10k: {sw.ElapsedMilliseconds}ms, slope={fit.Slope:F4}");
        Assert.True(sw.ElapsedMilliseconds < 2000);
    }

    [Fact]
    public void LineFit3D_10000Points_Under2Seconds()
    {
        var pts = Enumerable.Range(0, 10000).Select(i => new Vec3(i * 0.001, 0, 0)).ToArray();
        var sw = Stopwatch.StartNew();
        var fit = LineFitter.Fit(pts);
        sw.Stop();
        _output.WriteLine($"LineFit3D 10k: {sw.ElapsedMilliseconds}ms, rms={fit.RmsResidual:F6}");
        Assert.True(sw.ElapsedMilliseconds < 2000);
    }

    [Fact]
    public void QuadraticFit_10000Points_Under2Seconds()
    {
        var x = Enumerable.Range(0, 10000).Select(i => i * 0.001).ToArray();
        var y = x.Select(xi => 2.0 * xi * xi + xi + 1.0).ToArray();
        var sw = Stopwatch.StartNew();
        var fit = QuadraticFitter.Fit1D(x, y);
        sw.Stop();
        _output.WriteLine($"QuadraticFit 10k: {sw.ElapsedMilliseconds}ms, A={fit.A:F4}");
        Assert.True(sw.ElapsedMilliseconds < 2000);
    }

    [Fact]
    public void NelderMead_HighDimensional_Under5Seconds()
    {
        var sw = Stopwatch.StartNew();
        var result = NelderMead.Minimize(
            x => x.Sum(v => v * v),
            Enumerable.Range(0, 10).Select(i => (double)(i + 1)).ToArray(),
            maxIter: 5000);
        sw.Stop();
        _output.WriteLine($"NelderMead 10D: {sw.ElapsedMilliseconds}ms, val={result.Value:F6}");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public void NelderMead_ManyIterations_Under5Seconds()
    {
        var sw = Stopwatch.StartNew();
        var result = NelderMead.Minimize(
            x => x[0] * x[0] + x[1] * x[1],
            new[] { 10.0, 10.0 },
            maxIter: 10000);
        sw.Stop();
        _output.WriteLine($"NelderMead 10k iter: {sw.ElapsedMilliseconds}ms, iters={result.Iterations}");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public void PlaneFit_50000Points_Under10Seconds()
    {
        var rng = new Random(42);
        var pts = Enumerable.Range(0, 50000).Select(_ =>
            new Vec3(rng.NextDouble() * 10, rng.NextDouble() * 10, rng.NextDouble() * 0.01)).ToArray();
        var sw = Stopwatch.StartNew();
        var fit = PlaneFitter.Fit(pts);
        sw.Stop();
        _output.WriteLine($"PlaneFit 50k: {sw.ElapsedMilliseconds}ms, rms={fit.RmsResidual:F6}");
        Assert.True(sw.ElapsedMilliseconds < 10000);
    }

    [Fact]
    public void SphereFit_50000Points_Under10Seconds()
    {
        var rng = new Random(42);
        var pts = Enumerable.Range(0, 50000).Select(_ =>
        {
            var dir = new Vec3(rng.NextDouble() * 2 - 1, rng.NextDouble() * 2 - 1, rng.NextDouble() * 2 - 1).Normalized;
            return dir * 5.0;
        }).ToArray();
        var sw = Stopwatch.StartNew();
        var fit = SphereFitter.Fit(pts);
        sw.Stop();
        _output.WriteLine($"SphereFit 50k: {sw.ElapsedMilliseconds}ms, r={fit.Radius:F4}");
        Assert.True(sw.ElapsedMilliseconds < 10000);
    }

    [Fact]
    public void MultipleFitsSequential_Under10Seconds()
    {
        var rng = new Random(42);
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 10; i++)
        {
            var pts = Enumerable.Range(0, 1000).Select(_ =>
                new Vec3(rng.NextDouble() * 10, rng.NextDouble() * 10, rng.NextDouble() * 0.01)).ToArray();
            PlaneFitter.Fit(pts);
        }
        sw.Stop();
        _output.WriteLine($"10 plane fits of 1k: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 10000);
    }

    [Fact]
    public void ConeFit_1000Points_Under10Seconds()
    {
        var rng = new Random(42);
        double halfAngle = System.Math.PI / 6;
        var pts = Enumerable.Range(0, 1000).Select(_ =>
        {
            double h = rng.NextDouble() * 5 + 1;
            double r = h * System.Math.Tan(halfAngle);
            double angle = rng.NextDouble() * 2 * System.Math.PI;
            return new Vec3(r * System.Math.Cos(angle), r * System.Math.Sin(angle), h);
        }).ToArray();
        var sw = Stopwatch.StartNew();
        var fit = ConeFitter.Fit(pts);
        sw.Stop();
        _output.WriteLine($"ConeFit 1k: {sw.ElapsedMilliseconds}ms, halfAngle={fit.HalfAngle:F4}");
        Assert.True(sw.ElapsedMilliseconds < 10000);
    }

    [Fact]
    public void MeshFitting_FitSphere_Under5Seconds()
    {
        var sphere = MdCsg.Api.Primitives.Sphere(Vec3.Zero, 3.0, 3);
        var verts = sphere.Mesh.Vertices.Select(v => v.Position).ToArray();
        var sw = Stopwatch.StartNew();
        var solid = MeshFitting.FitSphere(verts);
        sw.Stop();
        _output.WriteLine($"MeshFitting sphere: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public void LineFit2D_WithNoise_Under2Seconds()
    {
        var rng = new Random(42);
        var pts = Enumerable.Range(0, 10000).Select(i =>
            new Vec2(i * 0.001, 2.0 * i * 0.001 + 1.0 + rng.NextDouble() * 0.1)).ToArray();
        var sw = Stopwatch.StartNew();
        var fit = LineFitter.Fit(pts);
        sw.Stop();
        _output.WriteLine($"LineFit2D noisy 10k: {sw.ElapsedMilliseconds}ms, slope={fit.Slope:F4}, rms={fit.RmsResidual:F6}");
        Assert.True(sw.ElapsedMilliseconds < 2000);
    }

    [Fact]
    public void QuadraticFit_WithNoise_Under2Seconds()
    {
        var rng = new Random(42);
        var x = Enumerable.Range(0, 10000).Select(i => i * 0.001).ToArray();
        var y = x.Select(xi => 2.0 * xi * xi + xi + 1.0 + rng.NextDouble() * 0.5).ToArray();
        var sw = Stopwatch.StartNew();
        var fit = QuadraticFitter.Fit1D(x, y);
        sw.Stop();
        _output.WriteLine($"QuadraticFit noisy 10k: {sw.ElapsedMilliseconds}ms, A={fit.A:F4}");
        Assert.True(sw.ElapsedMilliseconds < 2000);
    }
}
