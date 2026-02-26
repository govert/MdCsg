using MdCsg.Fitting;
using MdCsg.Math;

namespace MdCsg.Tests.Fitting;

public class ConeFitterTests
{
    [Fact]
    public void Fit_ZAxisCone_RecoverAxis()
    {
        var points = GenerateConePoints(Vec3.Zero, Vec3.UnitZ, System.Math.PI / 6, 100);
        var result = ConeFitter.Fit(points);

        double axisDot = System.Math.Abs(Vec3.Dot(result.Axis, Vec3.UnitZ));
        Assert.True(axisDot > 0.9, $"Axis should be along Z, dot={axisDot}");
    }

    [Fact]
    public void Fit_ZAxisCone_RecoverHalfAngle()
    {
        double halfAngle = System.Math.PI / 6; // 30 degrees
        var points = GenerateConePoints(Vec3.Zero, Vec3.UnitZ, halfAngle, 200);
        var result = ConeFitter.Fit(points);

        Assert.True(System.Math.Abs(result.HalfAngle - halfAngle) < 0.1,
            $"Half angle should be ~{halfAngle}, got {result.HalfAngle}");
    }

    [Theory]
    [InlineData(15)]
    [InlineData(30)]
    [InlineData(45)]
    public void Fit_VariousHalfAngles(int angleDeg)
    {
        double halfAngle = angleDeg * System.Math.PI / 180;
        var points = GenerateConePoints(Vec3.Zero, Vec3.UnitZ, halfAngle, 200);
        var result = ConeFitter.Fit(points);

        Assert.True(System.Math.Abs(result.HalfAngle - halfAngle) < 0.15,
            $"Expected half angle ~{halfAngle:F3}, got {result.HalfAngle:F3}");
    }

    [Fact]
    public void Fit_TiltedCone_RecoverAxis()
    {
        var axis = new Vec3(1, 1, 0).Normalized;
        var points = GenerateConePoints(Vec3.Zero, axis, System.Math.PI / 4, 200);
        var result = ConeFitter.Fit(points);

        double axisDot = System.Math.Abs(Vec3.Dot(result.Axis, axis));
        Assert.True(axisDot > 0.85, $"Tilted axis dot={axisDot}");
    }

    [Fact]
    public void Fit_LowRms()
    {
        var points = GenerateConePoints(Vec3.Zero, Vec3.UnitZ, System.Math.PI / 6, 200);
        var result = ConeFitter.Fit(points);
        Assert.True(result.RmsResidual < 0.1, $"RMS should be small, got {result.RmsResidual}");
    }

    [Fact]
    public void Fit_TooFewPoints_Throws()
    {
        Assert.Throws<ArgumentException>(() => ConeFitter.Fit(new Vec3[4]));
    }

    private static Vec3[] GenerateConePoints(Vec3 apex, Vec3 axis, double halfAngle, int count)
    {
        var (u, v, w) = BuildBasis(axis);
        var rng = new Random(42);
        var points = new Vec3[count];

        for (int i = 0; i < count; i++)
        {
            double t = rng.NextDouble() * 5 + 0.5; // distance along axis
            double angle = rng.NextDouble() * 2 * System.Math.PI;
            double r = t * System.Math.Tan(halfAngle);

            points[i] = apex + w * t + u * (r * System.Math.Cos(angle)) + v * (r * System.Math.Sin(angle));
        }

        return points;
    }

    private static (Vec3 U, Vec3 V, Vec3 W) BuildBasis(Vec3 axis)
    {
        var w = axis.Normalized;
        var helper = System.Math.Abs(w.X) < 0.9 ? Vec3.UnitX : Vec3.UnitY;
        var u = Vec3.Cross(w, helper).Normalized;
        var v = Vec3.Cross(w, u);
        return (u, v, w);
    }
}
