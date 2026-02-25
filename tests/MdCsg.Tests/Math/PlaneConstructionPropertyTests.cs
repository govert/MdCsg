using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Plane — construction, SignedDistance, Flipped, normal consistency</summary>
public class PlaneConstructionPropertyTests
{
    [Fact]
    public void FromPoints_XYPlane_NormalIsZ()
    {
        var p = Plane.FromPoints(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.True(System.Math.Abs(p.Normal.X) < 1e-10);
        Assert.True(System.Math.Abs(p.Normal.Y) < 1e-10);
        Assert.True(p.Normal.Z > 0);
    }

    [Fact]
    public void FromPoints_XZPlane_NormalIsY()
    {
        var p = Plane.FromPoints(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 0, 1));
        Assert.True(System.Math.Abs(p.Normal.X) < 1e-10);
        Assert.True(System.Math.Abs(p.Normal.Z) < 1e-10);
        // Normal is -Y because cross(X,Z) = -Y
        Assert.True(System.Math.Abs(p.Normal.Y) > 0.9);
    }

    [Fact]
    public void SignedDistance_PointOnPlane_IsZero()
    {
        var p = Plane.FromPoints(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.True(System.Math.Abs(p.SignedDistanceTo(new Vec3(0.5, 0.3, 0))) < 1e-10);
    }

    [Fact]
    public void SignedDistance_PointAbove_IsPositive()
    {
        var p = Plane.FromPoints(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.True(p.SignedDistanceTo(new Vec3(0, 0, 1)) > 0);
    }

    [Fact]
    public void SignedDistance_PointBelow_IsNegative()
    {
        var p = Plane.FromPoints(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.True(p.SignedDistanceTo(new Vec3(0, 0, -1)) < 0);
    }

    [Fact]
    public void Flipped_ReversesNormal()
    {
        var p = Plane.FromPoints(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var f = p.Flipped;
        Assert.Equal(-p.Normal.X, f.Normal.X, 10);
        Assert.Equal(-p.Normal.Y, f.Normal.Y, 10);
        Assert.Equal(-p.Normal.Z, f.Normal.Z, 10);
    }

    [Fact]
    public void Flipped_ReversesDistance()
    {
        var p = Plane.FromPoints(new Vec3(0, 0, 1), new Vec3(1, 0, 1), new Vec3(0, 1, 1));
        var f = p.Flipped;
        Assert.Equal(-p.Distance, f.Distance, 10);
    }

    [Fact]
    public void Flipped_ReversesSignedDistance()
    {
        var p = Plane.FromPoints(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var f = p.Flipped;
        var pt = new Vec3(0, 0, 5);
        Assert.Equal(-p.SignedDistanceTo(pt), f.SignedDistanceTo(pt), 10);
    }

    [Fact]
    public void DoubleFlip_IsOriginal()
    {
        var p = Plane.FromPoints(new Vec3(1, 2, 3), new Vec3(4, 5, 6), new Vec3(7, 8, 10));
        var ff = p.Flipped.Flipped;
        Assert.Equal(p.Normal.X, ff.Normal.X, 10);
        Assert.Equal(p.Normal.Y, ff.Normal.Y, 10);
        Assert.Equal(p.Normal.Z, ff.Normal.Z, 10);
        Assert.Equal(p.Distance, ff.Distance, 10);
    }

    [Fact]
    public void FromPoints_AllVerticesOnPlane()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var c = new Vec3(7, 8, 10);
        var p = Plane.FromPoints(a, b, c);
        Assert.True(System.Math.Abs(p.SignedDistanceTo(a)) < 1e-10);
        Assert.True(System.Math.Abs(p.SignedDistanceTo(b)) < 1e-10);
        Assert.True(System.Math.Abs(p.SignedDistanceTo(c)) < 1e-10);
    }

    [Fact]
    public void RecordEquality()
    {
        var p1 = new Plane(new Vec3(0, 0, 1), 5.0);
        var p2 = new Plane(new Vec3(0, 0, 1), 5.0);
        Assert.Equal(p1, p2);
    }

    [Fact]
    public void SignedDistance_ProportionalToHeight()
    {
        var p = Plane.FromPoints(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        double d1 = p.SignedDistanceTo(new Vec3(0, 0, 1));
        double d2 = p.SignedDistanceTo(new Vec3(0, 0, 2));
        Assert.Equal(d1 * 2, d2, 8);
    }
}
