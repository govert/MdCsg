using MdCsg.Api;
using MdCsg.Math;

namespace MdCsg.Tests.Api;

public class HalfSpaceTests
{
    [Fact]
    public void Constructor_FromPlane_PreservesPlane()
    {
        var plane = new Plane(Vec3.UnitZ, 5.0);
        var hs = new HalfSpace(plane);
        Assert.Equal(plane, hs.Plane);
    }

    [Fact]
    public void Constructor_FromNormalAndDistance()
    {
        var hs = new HalfSpace(Vec3.UnitY, 3.0);
        Assert.Equal(Vec3.UnitY, hs.Plane.Normal);
        Assert.Equal(3.0, hs.Plane.Distance, 1e-12);
    }

    [Fact]
    public void Constructor_NormalizesNormal()
    {
        var hs = new HalfSpace(new Vec3(0, 0, 2), 3.0);
        Assert.Equal(1.0, hs.Plane.Normal.Length, 1e-12);
    }

    [Fact]
    public void FromPointAndNormal_PointOnPlane()
    {
        var hs = HalfSpace.FromPointAndNormal(new Vec3(0, 0, 5), Vec3.UnitZ);
        // The point should be exactly on the plane (distance = 0)
        Assert.Equal(0.0, hs.Plane.SignedDistanceTo(new Vec3(0, 0, 5)), 1e-12);
    }

    [Fact]
    public void FromPointAndNormal_ClassifiesCorrectly()
    {
        var hs = HalfSpace.FromPointAndNormal(new Vec3(0, 0, 5), Vec3.UnitZ);
        // Above the plane → positive signed distance (inside)
        Assert.True(hs.Plane.SignedDistanceTo(new Vec3(0, 0, 10)) > 0);
        // Below the plane → negative signed distance (outside)
        Assert.True(hs.Plane.SignedDistanceTo(new Vec3(0, 0, 0)) < 0);
    }

    [Fact]
    public void Flipped_ReversesClassification()
    {
        var hs = HalfSpace.FromPointAndNormal(Vec3.Zero, Vec3.UnitZ);
        var flipped = hs.Flipped;

        var testPoint = new Vec3(0, 0, 1);
        // Original: point above z=0 is on the normal side (positive)
        Assert.True(hs.Plane.SignedDistanceTo(testPoint) > 0);
        // Flipped: same point is now on the opposite side (negative)
        Assert.True(flipped.Plane.SignedDistanceTo(testPoint) < 0);
    }

    [Fact]
    public void Flipped_PlaneDistanceNegated()
    {
        var hs = new HalfSpace(Vec3.UnitX, 3.0);
        var flipped = hs.Flipped;
        Assert.Equal(-3.0, flipped.Plane.Distance, 1e-12);
    }
}
