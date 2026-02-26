using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>
/// Parameterized tests for Plane construction, distance, and properties.
/// </summary>
public class PlaneParameterizedTests
{
    // =========================================================================
    // FromPoints: axis-aligned planes
    // =========================================================================

    [Theory]
    [InlineData(0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1)]   // XY plane, normal +Z
    [InlineData(0, 0, 0, 1, 0, 0, 0, 0, 1, 0, -1, 0)]   // XZ plane, normal -Y
    [InlineData(0, 0, 0, 0, 1, 0, 0, 0, 1, 1, 0, 0)]    // YZ plane, normal +X
    public void FromPoints_AxisAligned_CorrectNormal(
        double ax, double ay, double az,
        double bx, double by, double bz,
        double cx, double cy, double cz,
        double enx, double eny, double enz)
    {
        var plane = Plane.FromPoints(new Vec3(ax, ay, az), new Vec3(bx, by, bz), new Vec3(cx, cy, cz));
        var expectedNormal = new Vec3(enx, eny, enz).Normalized;

        double alignment = System.Math.Abs(Vec3.Dot(plane.Normal, expectedNormal));
        Assert.True(alignment > 0.999,
            $"Normal alignment: {alignment}");
    }

    // =========================================================================
    // FromPointAndNormal: point lies on the plane
    // =========================================================================

    [Theory]
    [InlineData(0, 0, 0, 1, 0, 0)]
    [InlineData(0, 0, 0, 0, 1, 0)]
    [InlineData(0, 0, 0, 0, 0, 1)]
    [InlineData(5, 0, 0, 1, 0, 0)]
    [InlineData(0, 5, 0, 0, 1, 0)]
    [InlineData(0, 0, 5, 0, 0, 1)]
    [InlineData(1, 2, 3, 1, 1, 1)]
    [InlineData(-5, 10, -3, 2, -1, 3)]
    public void FromPointAndNormal_PointOnPlane_ZeroDistance(
        double px, double py, double pz, double nx, double ny, double nz)
    {
        var point = new Vec3(px, py, pz);
        var normal = new Vec3(nx, ny, nz);
        var plane = Plane.FromPointAndNormal(point, normal);

        double sd = plane.SignedDistanceTo(point);
        Assert.True(System.Math.Abs(sd) < 1e-10,
            $"Point ({px},{py},{pz}) should be on the plane, got sd={sd}");
    }

    // =========================================================================
    // SignedDistanceTo: known distances
    // =========================================================================

    [Theory]
    [InlineData(0, 0, 1, 0, 0, 0, 5, 5)]     // z-plane at 0, point at z=5 → sd=5
    [InlineData(0, 0, 1, 3, 0, 0, 5, 2)]      // z-plane at 3, point at z=5 → sd=2
    [InlineData(0, 0, 1, 0, 0, 0, -3, -3)]    // z-plane at 0, point at z=-3 → sd=-3
    [InlineData(1, 0, 0, 5, 10, 0, 0, 5)]     // x-plane at 5, point at x=10 → sd=5
    [InlineData(1, 0, 0, 5, 0, 0, 0, -5)]     // x-plane at 5, point at x=0 → sd=-5
    public void SignedDistance_KnownValues(
        double nx, double ny, double nz, double dist,
        double px, double py, double pz, double expectedSd)
    {
        var plane = new Plane(new Vec3(nx, ny, nz), dist);
        double sd = plane.SignedDistanceTo(new Vec3(px, py, pz));
        Assert.True(System.Math.Abs(sd - expectedSd) < 1e-10,
            $"Expected sd={expectedSd}, got {sd}");
    }

    // =========================================================================
    // Flipped: negates normal and distance
    // =========================================================================

    [Theory]
    [InlineData(1, 0, 0, 5)]
    [InlineData(0, 1, 0, -3)]
    [InlineData(0, 0, 1, 0)]
    [InlineData(0.577, 0.577, 0.577, 10)]
    public void Flipped_NegatesNormalAndDistance(double nx, double ny, double nz, double dist)
    {
        var plane = new Plane(new Vec3(nx, ny, nz), dist);
        var flipped = plane.Flipped;

        Assert.True(System.Math.Abs(flipped.Normal.X + plane.Normal.X) < 1e-10);
        Assert.True(System.Math.Abs(flipped.Normal.Y + plane.Normal.Y) < 1e-10);
        Assert.True(System.Math.Abs(flipped.Normal.Z + plane.Normal.Z) < 1e-10);
        Assert.True(System.Math.Abs(flipped.Distance + plane.Distance) < 1e-10);
    }

    [Theory]
    [InlineData(1, 0, 0, 0)]
    [InlineData(0, 1, 0, 5)]
    [InlineData(0, 0, 1, -3)]
    public void DoubleFlipped_IsOriginal(double nx, double ny, double nz, double dist)
    {
        var plane = new Plane(new Vec3(nx, ny, nz), dist);
        var doubleFlipped = plane.Flipped.Flipped;

        Assert.True(System.Math.Abs(doubleFlipped.Normal.X - plane.Normal.X) < 1e-10);
        Assert.True(System.Math.Abs(doubleFlipped.Distance - plane.Distance) < 1e-10);
    }

    // =========================================================================
    // SignedDistance: many random points, verify sign consistency
    // =========================================================================

    [Theory]
    [InlineData(1, 42)]
    [InlineData(2, 99)]
    [InlineData(3, 123)]
    [InlineData(4, 7)]
    [InlineData(5, 314)]
    public void RandomPoints_SignConsistency(int _, int seed)
    {
        var rng = new Random(seed);
        var normal = Vec3.UnitZ;
        double planeD = 0;
        var plane = new Plane(normal, planeD);

        for (int i = 0; i < 20; i++)
        {
            var p = new Vec3(rng.NextDouble() * 10 - 5, rng.NextDouble() * 10 - 5, rng.NextDouble() * 10 - 5);
            double sd = plane.SignedDistanceTo(p);

            if (p.Z > 0)
                Assert.True(sd > 0, $"Point z={p.Z} should be above z=0 plane, got sd={sd}");
            else if (p.Z < 0)
                Assert.True(sd < 0, $"Point z={p.Z} should be below z=0 plane, got sd={sd}");
        }
    }
}
