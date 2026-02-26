using MdCsg.Math;

namespace MdCsg.Tests.Math;

public class SphericalCoordinatesTests
{
    private const double Tol = 1e-10;

    // ── Roundtrip ────────────────────────────────────────────────────

    [Theory]
    [InlineData(1, 0, 0)]
    [InlineData(0, 1, 0)]
    [InlineData(0, 0, 1)]
    [InlineData(-1, 0, 0)]
    [InlineData(0, -1, 0)]
    [InlineData(0, 0, -1)]
    [InlineData(1, 1, 1)]
    [InlineData(3, 4, 5)]
    [InlineData(-2, 3, -1)]
    public void Roundtrip_CartesianToSphericalAndBack(double x, double y, double z)
    {
        var v = new Vec3(x, y, z);
        var (az, inc, dist) = SphericalCoordinates.ToSpherical(v);
        var back = SphericalCoordinates.FromSpherical(az, inc, dist);
        Assert.True(Vec3.DistanceSquared(v, back) < Tol * Tol,
            $"Roundtrip failed for ({x},{y},{z}): got {back}");
    }

    // ── Axis-aligned vectors ────────────────────────────────────────

    [Fact]
    public void ToSpherical_PositiveZ_InclinationZero()
    {
        var (az, inc, dist) = SphericalCoordinates.ToSpherical(new Vec3(0, 0, 5));
        Assert.True(System.Math.Abs(inc) < Tol, $"Inclination should be 0, got {inc}");
        Assert.True(System.Math.Abs(dist - 5) < Tol);
    }

    [Fact]
    public void ToSpherical_NegativeZ_InclinationPi()
    {
        var (az, inc, dist) = SphericalCoordinates.ToSpherical(new Vec3(0, 0, -3));
        Assert.True(System.Math.Abs(inc - System.Math.PI) < Tol, $"Inclination should be pi, got {inc}");
        Assert.True(System.Math.Abs(dist - 3) < Tol);
    }

    [Fact]
    public void ToSpherical_PositiveX_AzimuthZero_InclinationPiOver2()
    {
        var (az, inc, dist) = SphericalCoordinates.ToSpherical(new Vec3(2, 0, 0));
        Assert.True(System.Math.Abs(az) < Tol, $"Azimuth should be 0, got {az}");
        Assert.True(System.Math.Abs(inc - System.Math.PI / 2) < Tol, $"Inclination should be pi/2, got {inc}");
        Assert.True(System.Math.Abs(dist - 2) < Tol);
    }

    [Fact]
    public void ToSpherical_PositiveY_AzimuthPiOver2()
    {
        var (az, inc, dist) = SphericalCoordinates.ToSpherical(new Vec3(0, 4, 0));
        Assert.True(System.Math.Abs(az - System.Math.PI / 2) < Tol, $"Azimuth should be pi/2, got {az}");
    }

    [Fact]
    public void ToSpherical_ZeroVector_ZeroEverything()
    {
        var (az, inc, dist) = SphericalCoordinates.ToSpherical(Vec3.Zero);
        Assert.Equal(0.0, az);
        Assert.Equal(0.0, inc);
        Assert.Equal(0.0, dist);
    }

    // ── FromSpherical ───────────────────────────────────────────────

    [Fact]
    public void FromSpherical_Equator_CorrectXY()
    {
        // azimuth=45°, inclination=90° (equator), distance=1
        double az = System.Math.PI / 4;
        double inc = System.Math.PI / 2;
        var v = SphericalCoordinates.FromSpherical(az, inc, 1);
        Assert.True(System.Math.Abs(v.X - System.Math.Cos(az)) < Tol);
        Assert.True(System.Math.Abs(v.Y - System.Math.Sin(az)) < Tol);
        Assert.True(System.Math.Abs(v.Z) < Tol);
    }

    [Fact]
    public void FromSpherical_NorthPole()
    {
        var v = SphericalCoordinates.FromSpherical(0, 0, 5);
        Assert.True(System.Math.Abs(v.X) < Tol);
        Assert.True(System.Math.Abs(v.Y) < Tol);
        Assert.True(System.Math.Abs(v.Z - 5) < Tol);
    }

    [Fact]
    public void FromSpherical_SouthPole()
    {
        var v = SphericalCoordinates.FromSpherical(0, System.Math.PI, 3);
        Assert.True(System.Math.Abs(v.X) < Tol);
        Assert.True(System.Math.Abs(v.Y) < Tol);
        Assert.True(System.Math.Abs(v.Z + 3) < Tol);
    }

    // ── Distance preservation ───────────────────────────────────────

    [Theory]
    [InlineData(0.5, 0.3, 2.0)]
    [InlineData(1.5, 1.0, 5.0)]
    [InlineData(-1.0, 2.5, 0.1)]
    [InlineData(3.14, 1.57, 10.0)]
    public void FromSpherical_DistancePreserved(double az, double inc, double dist)
    {
        var v = SphericalCoordinates.FromSpherical(az, inc, dist);
        Assert.True(System.Math.Abs(v.Length - dist) < Tol,
            $"Expected length {dist}, got {v.Length}");
    }

    // ── Quadrant coverage ───────────────────────────────────────────

    public static IEnumerable<object[]> QuadrantVectors()
    {
        yield return new object[] { 1.0, 1.0, 1.0 };
        yield return new object[] { -1.0, 1.0, 1.0 };
        yield return new object[] { 1.0, -1.0, 1.0 };
        yield return new object[] { 1.0, 1.0, -1.0 };
        yield return new object[] { -1.0, -1.0, 1.0 };
        yield return new object[] { -1.0, 1.0, -1.0 };
        yield return new object[] { 1.0, -1.0, -1.0 };
        yield return new object[] { -1.0, -1.0, -1.0 };
    }

    [Theory]
    [MemberData(nameof(QuadrantVectors))]
    public void Roundtrip_AllQuadrants(double x, double y, double z)
    {
        var v = new Vec3(x, y, z);
        var (az, inc, dist) = SphericalCoordinates.ToSpherical(v);
        var back = SphericalCoordinates.FromSpherical(az, inc, dist);
        Assert.True(Vec3.DistanceSquared(v, back) < Tol * Tol,
            $"Roundtrip failed for ({x},{y},{z}): got {back}");
    }
}
