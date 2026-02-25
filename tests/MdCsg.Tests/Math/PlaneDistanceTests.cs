using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Plane — SignedDistanceTo, FromPoints, Flipped, Normal direction</summary>
public class PlaneDistanceTests
{
    [Fact]
    public void XYPlane_PointAbove_PositiveDistance()
    {
        var plane = Plane.FromPoints(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var dist = plane.SignedDistanceTo(new Vec3(0, 0, 5));
        Assert.True(dist > 0, $"Distance should be positive, got {dist}");
    }

    [Fact]
    public void XYPlane_PointBelow_NegativeDistance()
    {
        var plane = Plane.FromPoints(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var dist = plane.SignedDistanceTo(new Vec3(0, 0, -5));
        Assert.True(dist < 0, $"Distance should be negative, got {dist}");
    }

    [Fact]
    public void XYPlane_PointOnPlane_ZeroDistance()
    {
        var plane = Plane.FromPoints(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var dist = plane.SignedDistanceTo(new Vec3(5, 5, 0));
        Assert.Equal(0, dist, 1e-10);
    }

    [Fact]
    public void Flipped_OppositeDistance()
    {
        var plane = Plane.FromPoints(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var point = new Vec3(1, 1, 3);
        var d1 = plane.SignedDistanceTo(point);
        var d2 = plane.Flipped.SignedDistanceTo(point);
        Assert.Equal(-d1, d2, 1e-10);
    }

    [Fact]
    public void Flipped_OppositeNormal()
    {
        var plane = Plane.FromPoints(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var flipped = plane.Flipped;
        Assert.Equal(-plane.Normal.X, flipped.Normal.X, 1e-10);
        Assert.Equal(-plane.Normal.Y, flipped.Normal.Y, 1e-10);
        Assert.Equal(-plane.Normal.Z, flipped.Normal.Z, 1e-10);
    }

    [Fact]
    public void FromPoints_NormalIsUnit()
    {
        var plane = Plane.FromPoints(new Vec3(0, 0, 0), new Vec3(3, 0, 0), new Vec3(0, 4, 0));
        Assert.Equal(1.0, plane.Normal.Length, 1e-10);
    }

    [Fact]
    public void FromPoints_XYPlane_NormalIsZ()
    {
        var plane = Plane.FromPoints(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.Equal(0, plane.Normal.X, 1e-10);
        Assert.Equal(0, plane.Normal.Y, 1e-10);
        Assert.True(System.Math.Abs(plane.Normal.Z) > 0.99);
    }

    [Fact]
    public void FromPoints_XZPlane_NormalIsY()
    {
        var plane = Plane.FromPoints(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 0, 1));
        Assert.True(System.Math.Abs(plane.Normal.Y) > 0.99);
    }

    [Fact]
    public void FromPoints_YZPlane_NormalIsX()
    {
        var plane = Plane.FromPoints(new Vec3(0, 0, 0), new Vec3(0, 1, 0), new Vec3(0, 0, 1));
        Assert.True(System.Math.Abs(plane.Normal.X) > 0.99);
    }

    [Fact]
    public void SignedDistance_Magnitude_EqualsGeometricDistance()
    {
        var plane = Plane.FromPoints(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var point = new Vec3(0, 0, 7);
        Assert.Equal(7.0, System.Math.Abs(plane.SignedDistanceTo(point)), 1e-10);
    }

    [Fact]
    public void OffsetPlane_CorrectDistance()
    {
        // Plane at z=5
        var plane = Plane.FromPoints(new Vec3(0, 0, 5), new Vec3(1, 0, 5), new Vec3(0, 1, 5));
        Assert.Equal(0, plane.SignedDistanceTo(new Vec3(0, 0, 5)), 1e-10);
        Assert.True(plane.SignedDistanceTo(new Vec3(0, 0, 10)) > 0 || plane.SignedDistanceTo(new Vec3(0, 0, 10)) < 0);
        // The distance should have magnitude 5
        Assert.Equal(5.0, System.Math.Abs(plane.SignedDistanceTo(new Vec3(0, 0, 10))), 1e-10);
    }

    [Fact]
    public void TiltedPlane_DistanceConsistent()
    {
        var plane = Plane.FromPoints(new Vec3(0, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 0, 1));
        var point = new Vec3(5, 0, 0);
        var d1 = plane.SignedDistanceTo(point);
        var d2 = plane.Flipped.SignedDistanceTo(point);
        Assert.Equal(-d1, d2, 1e-10);
    }

    [Fact]
    public void DoubleFlip_SameAsOriginal()
    {
        var plane = Plane.FromPoints(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var doubleFlipped = plane.Flipped.Flipped;
        Assert.Equal(plane.Normal.X, doubleFlipped.Normal.X, 1e-10);
        Assert.Equal(plane.Normal.Y, doubleFlipped.Normal.Y, 1e-10);
        Assert.Equal(plane.Normal.Z, doubleFlipped.Normal.Z, 1e-10);
    }
}
