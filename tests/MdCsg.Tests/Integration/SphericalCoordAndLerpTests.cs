using MdCsg.Math;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 39: SphericalCoordinates roundtrip, Vec3.Lerp/AngleBetween.</summary>
public class SphericalCoordAndLerpTests
{
    [Fact]
    public void SphericalRoundtrip_UnitX()
    {
        var (az, inc, dist) = SphericalCoordinates.ToSpherical(Vec3.UnitX);
        var back = SphericalCoordinates.FromSpherical(az, inc, dist);
        Assert.True((back - Vec3.UnitX).Length < 1e-10);
    }

    [Fact]
    public void SphericalRoundtrip_UnitY()
    {
        var (az, inc, dist) = SphericalCoordinates.ToSpherical(Vec3.UnitY);
        var back = SphericalCoordinates.FromSpherical(az, inc, dist);
        Assert.True((back - Vec3.UnitY).Length < 1e-10);
    }

    [Fact]
    public void SphericalRoundtrip_UnitZ()
    {
        var (az, inc, dist) = SphericalCoordinates.ToSpherical(Vec3.UnitZ);
        var back = SphericalCoordinates.FromSpherical(az, inc, dist);
        Assert.True((back - Vec3.UnitZ).Length < 1e-10);
    }

    [Fact]
    public void SphericalRoundtrip_ArbitraryPoint()
    {
        var p = new Vec3(3, 4, 5);
        var (az, inc, dist) = SphericalCoordinates.ToSpherical(p);
        var back = SphericalCoordinates.FromSpherical(az, inc, dist);
        Assert.True((back - p).Length < 1e-10);
    }

    [Fact]
    public void SphericalDistance_IsLength()
    {
        var p = new Vec3(3, 4, 5);
        var (_, _, dist) = SphericalCoordinates.ToSpherical(p);
        Assert.True(System.Math.Abs(dist - p.Length) < 1e-10);
    }

    [Fact]
    public void SphericalRoundtrip_NegativeCoordinates()
    {
        var p = new Vec3(-2, -3, -4);
        var (az, inc, dist) = SphericalCoordinates.ToSpherical(p);
        var back = SphericalCoordinates.FromSpherical(az, inc, dist);
        Assert.True((back - p).Length < 1e-10);
    }

    [Fact]
    public void SphericalRoundtrip_ManyRandomPoints()
    {
        var rng = new Random(42);
        for (int i = 0; i < 100; i++)
        {
            var p = new Vec3(rng.NextDouble() * 20 - 10, rng.NextDouble() * 20 - 10, rng.NextDouble() * 20 - 10);
            if (p.Length < 1e-6) continue;
            var (az, inc, dist) = SphericalCoordinates.ToSpherical(p);
            var back = SphericalCoordinates.FromSpherical(az, inc, dist);
            Assert.True((back - p).Length < 1e-8);
        }
    }

    [Fact]
    public void Lerp_AtZero_ReturnsA()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var result = Vec3.Lerp(a, b, 0);
        Assert.True((result - a).Length < 1e-10);
    }

    [Fact]
    public void Lerp_AtOne_ReturnsB()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var result = Vec3.Lerp(a, b, 1);
        Assert.True((result - b).Length < 1e-10);
    }

    [Fact]
    public void Lerp_AtHalf_ReturnsMidpoint()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(10, 10, 10);
        var result = Vec3.Lerp(a, b, 0.5);
        Assert.True((result - new Vec3(5, 5, 5)).Length < 1e-10);
    }

    [Fact]
    public void Lerp_Linearity()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(7, 8, 9);
        var t = 0.3;
        var expected = a + (b - a) * t;
        var result = Vec3.Lerp(a, b, t);
        Assert.True((result - expected).Length < 1e-10);
    }

    [Fact]
    public void AngleBetween_Perpendicular_Is90Degrees()
    {
        var angle = Vec3.AngleBetween(Vec3.UnitX, Vec3.UnitY);
        Assert.True(System.Math.Abs(angle - System.Math.PI / 2) < 1e-10);
    }

    [Fact]
    public void AngleBetween_Parallel_IsZero()
    {
        var angle = Vec3.AngleBetween(Vec3.UnitX, Vec3.UnitX);
        Assert.True(System.Math.Abs(angle) < 1e-10);
    }

    [Fact]
    public void AngleBetween_Opposite_IsPi()
    {
        var angle = Vec3.AngleBetween(Vec3.UnitX, new Vec3(-1, 0, 0));
        Assert.True(System.Math.Abs(angle - System.Math.PI) < 1e-10);
    }

    [Fact]
    public void AngleBetween_45Degrees()
    {
        var a = Vec3.UnitX;
        var b = new Vec3(1, 1, 0).Normalized;
        var angle = Vec3.AngleBetween(a, b);
        Assert.True(System.Math.Abs(angle - System.Math.PI / 4) < 1e-10);
    }

    [Fact]
    public void AngleBetween_IsSymmetric()
    {
        var a = new Vec3(1, 2, 3).Normalized;
        var b = new Vec3(4, -1, 2).Normalized;
        var angle1 = Vec3.AngleBetween(a, b);
        var angle2 = Vec3.AngleBetween(b, a);
        Assert.True(System.Math.Abs(angle1 - angle2) < 1e-10);
    }

    [Fact]
    public void AngleBetween_AllUnitAxisPairs_Are90()
    {
        Assert.True(System.Math.Abs(Vec3.AngleBetween(Vec3.UnitX, Vec3.UnitY) - System.Math.PI / 2) < 1e-10);
        Assert.True(System.Math.Abs(Vec3.AngleBetween(Vec3.UnitY, Vec3.UnitZ) - System.Math.PI / 2) < 1e-10);
        Assert.True(System.Math.Abs(Vec3.AngleBetween(Vec3.UnitX, Vec3.UnitZ) - System.Math.PI / 2) < 1e-10);
    }

    [Fact]
    public void Lerp_MultipleSteps_AreMonotonic()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(10, 0, 0);
        double prev = -1;
        for (int i = 0; i <= 10; i++)
        {
            var result = Vec3.Lerp(a, b, i / 10.0);
            Assert.True(result.X > prev);
            prev = result.X;
        }
    }

    [Fact]
    public void SphericalFromSpherical_KnownAngles()
    {
        // Azimuth=0, inclination=pi/2 → UnitX direction at distance 1
        var result = SphericalCoordinates.FromSpherical(0, System.Math.PI / 2, 1.0);
        Assert.True(result.Length > 0.99 && result.Length < 1.01);
    }

    [Fact]
    public void SphericalToSpherical_DistanceAlwaysPositive()
    {
        var rng = new Random(42);
        for (int i = 0; i < 50; i++)
        {
            var p = new Vec3(rng.NextDouble() * 10 - 5, rng.NextDouble() * 10 - 5, rng.NextDouble() * 10 - 5);
            if (p.Length < 1e-6) continue;
            var (_, _, dist) = SphericalCoordinates.ToSpherical(p);
            Assert.True(dist > 0);
        }
    }
}
