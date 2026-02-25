using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Plane — Flipped, SignedDistance, normal direction</summary>
public class PlaneFlippedDistanceTests
{
    [Fact]
    public void SignedDistance_PositiveAbove()
    {
        var plane = new Plane(new Vec3(0, 0, 1), 0); // z = 0 plane
        var point = new Vec3(0, 0, 5);
        Assert.True(plane.SignedDistanceTo(point) > 0);
    }

    [Fact]
    public void SignedDistance_NegativeBelow()
    {
        var plane = new Plane(new Vec3(0, 0, 1), 0);
        var point = new Vec3(0, 0, -5);
        Assert.True(plane.SignedDistanceTo(point) < 0);
    }

    [Fact]
    public void SignedDistance_ZeroOnPlane()
    {
        var plane = new Plane(new Vec3(0, 0, 1), 0);
        var point = new Vec3(5, 5, 0);
        Assert.Equal(0.0, plane.SignedDistanceTo(point), 10);
    }

    [Fact]
    public void SignedDistance_OffsetPlane()
    {
        var plane = new Plane(new Vec3(0, 0, 1), 5); // z = 5 plane
        var point = new Vec3(0, 0, 7);
        Assert.Equal(2.0, plane.SignedDistanceTo(point), 10);
    }

    [Fact]
    public void Flipped_ReversesNormal()
    {
        var plane = new Plane(new Vec3(0, 0, 1), -3);
        var flipped = plane.Flipped;
        Assert.Equal(-plane.Normal.X, flipped.Normal.X, 10);
        Assert.Equal(-plane.Normal.Y, flipped.Normal.Y, 10);
        Assert.Equal(-plane.Normal.Z, flipped.Normal.Z, 10);
    }

    [Fact]
    public void Flipped_ReversesD()
    {
        var plane = new Plane(new Vec3(0, 0, 1), -3);
        var flipped = plane.Flipped;
        Assert.Equal(-plane.Distance, flipped.Distance, 10);
    }

    [Fact]
    public void Flipped_FlipsSignedDistance()
    {
        var plane = new Plane(new Vec3(0, 0, 1), 0);
        var flipped = plane.Flipped;
        var point = new Vec3(0, 0, 5);
        Assert.Equal(-plane.SignedDistanceTo(point), flipped.SignedDistanceTo(point), 10);
    }

    [Fact]
    public void DoubleFlip_RestoresPlane()
    {
        var plane = new Plane(new Vec3(0, 0, 1), -3);
        var doubleFlipped = plane.Flipped.Flipped;
        Assert.Equal(plane.Normal.X, doubleFlipped.Normal.X, 10);
        Assert.Equal(plane.Normal.Y, doubleFlipped.Normal.Y, 10);
        Assert.Equal(plane.Normal.Z, doubleFlipped.Normal.Z, 10);
        Assert.Equal(plane.Distance, doubleFlipped.Distance, 10);
    }

    [Fact]
    public void Triangle_Plane_NormalDirection()
    {
        var tri = new Triangle3(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var plane = tri.Plane;
        // Normal should point in +Z direction for CCW winding in XY plane
        Assert.True(plane.Normal.Z > 0, $"Expected +Z normal, got {plane.Normal}");
    }

    [Fact]
    public void Triangle_Plane_VerticesOnPlane()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 0, 1);
        var c = new Vec3(2, 5, 0);
        var tri = new Triangle3(a, b, c);
        var plane = tri.Plane;
        Assert.Equal(0.0, plane.SignedDistanceTo(a), 8);
        Assert.Equal(0.0, plane.SignedDistanceTo(b), 8);
        Assert.Equal(0.0, plane.SignedDistanceTo(c), 8);
    }

    [Fact]
    public void Plane_RecordEquality()
    {
        var p1 = new Plane(new Vec3(0, 0, 1), -5);
        var p2 = new Plane(new Vec3(0, 0, 1), -5);
        Assert.Equal(p1, p2);
    }

    [Fact]
    public void Plane_RecordInequality()
    {
        var p1 = new Plane(new Vec3(0, 0, 1), -5);
        var p2 = new Plane(new Vec3(0, 1, 0), -5);
        Assert.NotEqual(p1, p2);
    }
}
