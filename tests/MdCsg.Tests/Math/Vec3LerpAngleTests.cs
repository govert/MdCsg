using MdCsg.Math;

namespace MdCsg.Tests.Math;

public class Vec3LerpAngleTests
{
    // ── Lerp ────────────────────────────────────────────────────────

    [Fact]
    public void Lerp_T0_ReturnsA()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        Assert.Equal(a, Vec3.Lerp(a, b, 0));
    }

    [Fact]
    public void Lerp_T1_ReturnsB()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        Assert.Equal(b, Vec3.Lerp(a, b, 1));
    }

    [Fact]
    public void Lerp_THalf_ReturnsMidpoint()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(4, 6, 8);
        var mid = Vec3.Lerp(a, b, 0.5);
        Assert.True(System.Math.Abs(mid.X - 2) < 1e-15);
        Assert.True(System.Math.Abs(mid.Y - 3) < 1e-15);
        Assert.True(System.Math.Abs(mid.Z - 4) < 1e-15);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.25)]
    [InlineData(0.5)]
    [InlineData(0.75)]
    [InlineData(1.0)]
    public void Lerp_ParametricValues_LieOnLine(double t)
    {
        var a = new Vec3(1, 0, 0);
        var b = new Vec3(0, 1, 0);
        var result = Vec3.Lerp(a, b, t);
        Assert.True(System.Math.Abs(result.X - (1 - t)) < 1e-14, $"X mismatch at t={t}");
        Assert.True(System.Math.Abs(result.Y - t) < 1e-14, $"Y mismatch at t={t}");
    }

    [Theory]
    [InlineData(-0.5)]
    [InlineData(1.5)]
    [InlineData(2.0)]
    public void Lerp_Extrapolation_Works(double t)
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(2, 0, 0);
        var result = Vec3.Lerp(a, b, t);
        Assert.True(System.Math.Abs(result.X - 2 * t) < 1e-14);
    }

    [Fact]
    public void Lerp_SamePoint_ReturnsSame()
    {
        var p = new Vec3(3, 4, 5);
        var result = Vec3.Lerp(p, p, 0.7);
        Assert.True(Vec3.DistanceSquared(result, p) < 1e-28);
    }

    public static IEnumerable<object[]> LerpPairs()
    {
        var vectors = new[]
        {
            new Vec3(1, 0, 0), new Vec3(0, 1, 0), new Vec3(0, 0, 1),
            new Vec3(-1, 2, 3), new Vec3(10, 20, 30)
        };
        var tValues = new[] { 0.0, 0.1, 0.5, 0.9, 1.0 };
        foreach (var a in vectors)
        foreach (var b in vectors)
        foreach (var t in tValues)
            yield return new object[] { a.X, a.Y, a.Z, b.X, b.Y, b.Z, t };
    }

    [Theory]
    [MemberData(nameof(LerpPairs))]
    public void Lerp_ResultOnLine(double ax, double ay, double az, double bx, double by, double bz, double t)
    {
        var a = new Vec3(ax, ay, az);
        var b = new Vec3(bx, by, bz);
        var result = Vec3.Lerp(a, b, t);
        var expected = a + (b - a) * t;
        Assert.True(Vec3.DistanceSquared(result, expected) < 1e-24,
            $"Lerp({a}, {b}, {t}) = {result}, expected {expected}");
    }

    // ── AngleBetween ────────────────────────────────────────────────

    [Fact]
    public void AngleBetween_ParallelVectors_ReturnsZero()
    {
        var a = new Vec3(1, 0, 0);
        var b = new Vec3(2, 0, 0);
        Assert.True(System.Math.Abs(Vec3.AngleBetween(a, b)) < 1e-12);
    }

    [Fact]
    public void AngleBetween_Antiparallel_ReturnsPi()
    {
        var a = new Vec3(1, 0, 0);
        var b = new Vec3(-3, 0, 0);
        Assert.True(System.Math.Abs(Vec3.AngleBetween(a, b) - System.Math.PI) < 1e-12);
    }

    [Fact]
    public void AngleBetween_Orthogonal_ReturnsPiOver2()
    {
        Assert.True(System.Math.Abs(Vec3.AngleBetween(Vec3.UnitX, Vec3.UnitY) - System.Math.PI / 2) < 1e-12);
        Assert.True(System.Math.Abs(Vec3.AngleBetween(Vec3.UnitY, Vec3.UnitZ) - System.Math.PI / 2) < 1e-12);
        Assert.True(System.Math.Abs(Vec3.AngleBetween(Vec3.UnitX, Vec3.UnitZ) - System.Math.PI / 2) < 1e-12);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(30.0)]
    [InlineData(45.0)]
    [InlineData(60.0)]
    [InlineData(90.0)]
    [InlineData(120.0)]
    [InlineData(150.0)]
    [InlineData(180.0)]
    public void AngleBetween_KnownAngleDegrees(double deg)
    {
        double angle = deg * System.Math.PI / 180.0;
        var a = Vec3.UnitX;
        double c = System.Math.Cos(angle);
        double s = System.Math.Sin(angle);
        var b = new Vec3(c, s, 0);
        double computed = Vec3.AngleBetween(a, b);
        Assert.True(System.Math.Abs(computed - angle) < 1e-10, $"Expected {angle}, got {computed}");
    }

    [Fact]
    public void AngleBetween_NonUnitVectors_StillCorrect()
    {
        var a = new Vec3(3, 0, 0);
        var b = new Vec3(5, 5, 0);
        double expected = System.Math.PI / 4;
        Assert.True(System.Math.Abs(Vec3.AngleBetween(a, b) - expected) < 1e-12);
    }

    [Fact]
    public void AngleBetween_Symmetric()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(-1, 4, 2);
        Assert.True(System.Math.Abs(Vec3.AngleBetween(a, b) - Vec3.AngleBetween(b, a)) < 1e-15);
    }

    [Fact]
    public void AngleBetween_3DOrthogonal()
    {
        var a = new Vec3(1, 1, 0);
        var b = new Vec3(0, 0, 1);
        Assert.True(System.Math.Abs(Vec3.AngleBetween(a, b) - System.Math.PI / 2) < 1e-12);
    }

    public static IEnumerable<object[]> AnglePairs()
    {
        var axes = new[]
        {
            Vec3.UnitX, Vec3.UnitY, Vec3.UnitZ,
            new Vec3(1, 1, 0).Normalized, new Vec3(1, 1, 1).Normalized
        };
        foreach (var a in axes)
        foreach (var b in axes)
            yield return new object[] { a.X, a.Y, a.Z, b.X, b.Y, b.Z };
    }

    [Theory]
    [MemberData(nameof(AnglePairs))]
    public void AngleBetween_InRange(double ax, double ay, double az, double bx, double by, double bz)
    {
        var a = new Vec3(ax, ay, az);
        var b = new Vec3(bx, by, bz);
        double angle = Vec3.AngleBetween(a, b);
        Assert.True(angle >= -1e-15 && angle <= System.Math.PI + 1e-15,
            $"Angle {angle} out of [0, pi] range for ({a}), ({b})");
    }
}
