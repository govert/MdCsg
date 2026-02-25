using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Plane algebraic — FromPoints, SignedDistanceTo, Flipped, normal consistency</summary>
public class PlaneAlgebraicPropertyTests
{
    [Fact]
    public void FromPoints_XYPlane_NormalIsZ()
    {
        var plane = Plane.FromPoints(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.Equal(0, plane.Normal.X, 1e-14);
        Assert.Equal(0, plane.Normal.Y, 1e-14);
        Assert.Equal(1, plane.Normal.Z, 1e-14);
    }

    [Fact]
    public void FromPoints_XZPlane()
    {
        var plane = Plane.FromPoints(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 0, 1));
        // Normal should be -Y (by right-hand rule for XZ)
        Assert.Equal(0, plane.Normal.X, 1e-14);
        Assert.True(System.Math.Abs(plane.Normal.Y) > 0.99);
        Assert.Equal(0, plane.Normal.Z, 1e-14);
    }

    [Fact]
    public void SignedDistanceTo_OnPlane_IsZero()
    {
        var plane = Plane.FromPoints(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.Equal(0, plane.SignedDistanceTo(Vec3.Zero), 1e-14);
        Assert.Equal(0, plane.SignedDistanceTo(new Vec3(0.5, 0.5, 0)), 1e-14);
    }

    [Fact]
    public void SignedDistanceTo_AbovePlane_Positive()
    {
        var plane = Plane.FromPoints(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.True(plane.SignedDistanceTo(new Vec3(0, 0, 1)) > 0);
    }

    [Fact]
    public void SignedDistanceTo_BelowPlane_Negative()
    {
        var plane = Plane.FromPoints(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.True(plane.SignedDistanceTo(new Vec3(0, 0, -1)) < 0);
    }

    [Fact]
    public void SignedDistanceTo_UnitDistance()
    {
        var plane = Plane.FromPoints(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.Equal(5.0, plane.SignedDistanceTo(new Vec3(0, 0, 5)), 1e-14);
    }

    [Fact]
    public void Flipped_ReversesNormal()
    {
        var plane = Plane.FromPoints(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var flipped = plane.Flipped;
        Assert.Equal(-plane.Normal.X, flipped.Normal.X, 1e-14);
        Assert.Equal(-plane.Normal.Y, flipped.Normal.Y, 1e-14);
        Assert.Equal(-plane.Normal.Z, flipped.Normal.Z, 1e-14);
    }

    [Fact]
    public void Flipped_NegatesDistance()
    {
        var plane = Plane.FromPoints(new Vec3(0, 0, 1), new Vec3(1, 0, 1), new Vec3(0, 1, 1));
        var flipped = plane.Flipped;
        Assert.Equal(-plane.Distance, flipped.Distance, 1e-14);
    }

    [Fact]
    public void Flipped_SignedDistance_NegatesSign()
    {
        var plane = Plane.FromPoints(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var flipped = plane.Flipped;
        var p = new Vec3(0, 0, 3);
        Assert.Equal(-plane.SignedDistanceTo(p), flipped.SignedDistanceTo(p), 1e-14);
    }

    [Fact]
    public void DoubleFlip_IsIdentity()
    {
        var plane = Plane.FromPoints(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var doubleFlipped = plane.Flipped.Flipped;
        Assert.Equal(plane.Normal.X, doubleFlipped.Normal.X, 1e-14);
        Assert.Equal(plane.Normal.Y, doubleFlipped.Normal.Y, 1e-14);
        Assert.Equal(plane.Normal.Z, doubleFlipped.Normal.Z, 1e-14);
        Assert.Equal(plane.Distance, doubleFlipped.Distance, 1e-14);
    }

    [Fact]
    public void Normal_IsUnitLength()
    {
        var plane = Plane.FromPoints(new Vec3(1, 2, 3), new Vec3(4, 5, 6), new Vec3(7, 2, 3));
        Assert.Equal(1.0, plane.Normal.Length, 1e-14);
    }

    [Fact]
    public void OffsetPlane_CorrectDistance()
    {
        // Plane at z=5
        var plane = Plane.FromPoints(new Vec3(0, 0, 5), new Vec3(1, 0, 5), new Vec3(0, 1, 5));
        Assert.Equal(5.0, plane.Distance, 1e-14);
    }

    [Fact]
    public void NegativeOffsetPlane()
    {
        var plane = Plane.FromPoints(new Vec3(0, 0, -3), new Vec3(1, 0, -3), new Vec3(0, 1, -3));
        Assert.Equal(-3.0, plane.Distance, 1e-14);
    }

    [Fact]
    public void ToString_NotEmpty()
    {
        var plane = Plane.FromPoints(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.True(plane.ToString().Length > 0);
        Assert.Contains("Plane", plane.ToString());
    }

    [Fact]
    public void RecordEquality()
    {
        var a = Plane.FromPoints(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var b = Plane.FromPoints(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.Equal(a, b);
    }

    [Fact]
    public void DifferentPlanes_NotEqual()
    {
        var a = Plane.FromPoints(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var b = Plane.FromPoints(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 0, 1));
        Assert.NotEqual(a, b);
    }
}
