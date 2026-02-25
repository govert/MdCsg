using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Plane — FromPoints, SignedDistanceTo, Flipped, normal orientation</summary>
public class PlaneSignedDistancePropertyTests
{
    [Fact]
    public void FromPoints_XYPlane_NormalIsZ()
    {
        var plane = Plane.FromPoints(Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        Assert.True(System.Math.Abs(plane.Normal.Z - 1.0) < 1e-10);
        Assert.True(System.Math.Abs(plane.Normal.X) < 1e-10);
        Assert.True(System.Math.Abs(plane.Normal.Y) < 1e-10);
    }

    [Fact]
    public void FromPoints_OriginPlane_DistanceZero()
    {
        var plane = Plane.FromPoints(Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        Assert.True(System.Math.Abs(plane.Distance) < 1e-10);
    }

    [Fact]
    public void FromPoints_OffsetPlane_CorrectDistance()
    {
        // Plane at z=5
        var plane = Plane.FromPoints(new Vec3(0, 0, 5), new Vec3(1, 0, 5), new Vec3(0, 1, 5));
        Assert.True(System.Math.Abs(plane.Distance - 5.0) < 1e-10);
    }

    [Fact]
    public void SignedDistanceTo_PointAbovePlane_Positive()
    {
        var plane = Plane.FromPoints(Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        Assert.True(plane.SignedDistanceTo(Vec3.UnitZ) > 0);
    }

    [Fact]
    public void SignedDistanceTo_PointBelowPlane_Negative()
    {
        var plane = Plane.FromPoints(Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        Assert.True(plane.SignedDistanceTo(-Vec3.UnitZ) < 0);
    }

    [Fact]
    public void SignedDistanceTo_PointOnPlane_Zero()
    {
        var plane = Plane.FromPoints(Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        Assert.True(System.Math.Abs(plane.SignedDistanceTo(new Vec3(0.5, 0.5, 0))) < 1e-10);
    }

    [Fact]
    public void SignedDistanceTo_KnownDistance()
    {
        var plane = Plane.FromPoints(Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        double dist = plane.SignedDistanceTo(new Vec3(0, 0, 7));
        Assert.True(System.Math.Abs(dist - 7.0) < 1e-10);
    }

    [Fact]
    public void Flipped_ReversesNormal()
    {
        var plane = Plane.FromPoints(Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        var flipped = plane.Flipped;
        Assert.True(System.Math.Abs(flipped.Normal.X + plane.Normal.X) < 1e-10);
        Assert.True(System.Math.Abs(flipped.Normal.Y + plane.Normal.Y) < 1e-10);
        Assert.True(System.Math.Abs(flipped.Normal.Z + plane.Normal.Z) < 1e-10);
    }

    [Fact]
    public void Flipped_NegatesDistance()
    {
        var plane = Plane.FromPoints(new Vec3(0, 0, 5), new Vec3(1, 0, 5), new Vec3(0, 1, 5));
        var flipped = plane.Flipped;
        Assert.True(System.Math.Abs(flipped.Distance + plane.Distance) < 1e-10);
    }

    [Fact]
    public void Flipped_SignedDistance_Negated()
    {
        var plane = Plane.FromPoints(Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        var flipped = plane.Flipped;
        var point = new Vec3(1, 2, 3);
        Assert.True(System.Math.Abs(plane.SignedDistanceTo(point) + flipped.SignedDistanceTo(point)) < 1e-10);
    }

    [Fact]
    public void Flipped_DoubleFlip_IsOriginal()
    {
        var plane = Plane.FromPoints(Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        var doubleFlipped = plane.Flipped.Flipped;
        Assert.True(System.Math.Abs(doubleFlipped.Normal.X - plane.Normal.X) < 1e-10);
        Assert.True(System.Math.Abs(doubleFlipped.Normal.Y - plane.Normal.Y) < 1e-10);
        Assert.True(System.Math.Abs(doubleFlipped.Normal.Z - plane.Normal.Z) < 1e-10);
        Assert.True(System.Math.Abs(doubleFlipped.Distance - plane.Distance) < 1e-10);
    }

    [Fact]
    public void Normal_IsUnitLength()
    {
        var plane = Plane.FromPoints(new Vec3(1, 2, 3), new Vec3(4, 5, 6), new Vec3(7, 0, 1));
        Assert.True(System.Math.Abs(plane.Normal.Length - 1.0) < 1e-10);
    }

    [Fact]
    public void AllThreePoints_OnPlane()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var c = new Vec3(7, 0, 1);
        var plane = Plane.FromPoints(a, b, c);
        Assert.True(System.Math.Abs(plane.SignedDistanceTo(a)) < 1e-10);
        Assert.True(System.Math.Abs(plane.SignedDistanceTo(b)) < 1e-10);
        Assert.True(System.Math.Abs(plane.SignedDistanceTo(c)) < 1e-10);
    }

    [Fact]
    public void ToString_ContainsComponents()
    {
        var plane = Plane.FromPoints(Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        var s = plane.ToString();
        Assert.Contains("Plane", s);
    }
}
