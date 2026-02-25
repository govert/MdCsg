using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Plane — FromPoints, SignedDistanceTo, Flipped, geometric properties</summary>
public class PlaneGeometryPropertyTests
{
    [Fact]
    public void FromPoints_XYPlane_NormalIsZ()
    {
        var plane = Plane.FromPoints(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.Equal(0.0, plane.Normal.X, 10);
        Assert.Equal(0.0, plane.Normal.Y, 10);
        Assert.Equal(1.0, System.Math.Abs(plane.Normal.Z), 10);
    }

    [Fact]
    public void FromPoints_XYPlane_DistanceZero()
    {
        var plane = Plane.FromPoints(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.Equal(0.0, plane.Distance, 10);
    }

    [Fact]
    public void FromPoints_OffsetPlane_DistanceNonZero()
    {
        // Plane at z = 5
        var plane = Plane.FromPoints(new Vec3(0, 0, 5), new Vec3(1, 0, 5), new Vec3(0, 1, 5));
        Assert.True(System.Math.Abs(plane.Distance) > 0.1);
    }

    [Fact]
    public void FromPoints_NormalIsUnit()
    {
        var plane = Plane.FromPoints(new Vec3(1, 2, 3), new Vec3(4, 5, 6), new Vec3(7, 8, 10));
        double len = plane.Normal.Length;
        Assert.Equal(1.0, len, 10);
    }

    [Fact]
    public void SignedDistanceTo_PointOnPlane_Zero()
    {
        var plane = Plane.FromPoints(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.Equal(0.0, plane.SignedDistanceTo(new Vec3(0.5, 0.5, 0)), 10);
    }

    [Fact]
    public void SignedDistanceTo_OriginalPoints_Zero()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var c = new Vec3(7, 8, 10);
        var plane = Plane.FromPoints(a, b, c);
        Assert.True(System.Math.Abs(plane.SignedDistanceTo(a)) < 1e-10);
        Assert.True(System.Math.Abs(plane.SignedDistanceTo(b)) < 1e-10);
        Assert.True(System.Math.Abs(plane.SignedDistanceTo(c)) < 1e-10);
    }

    [Fact]
    public void SignedDistanceTo_AboveAndBelow_OppositeSign()
    {
        var plane = Plane.FromPoints(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        double above = plane.SignedDistanceTo(new Vec3(0, 0, 1));
        double below = plane.SignedDistanceTo(new Vec3(0, 0, -1));
        Assert.True(above * below < 0, "Above and below should have opposite signs");
    }

    [Fact]
    public void SignedDistanceTo_XYPlane_EqualsZComponent()
    {
        var plane = Plane.FromPoints(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        double dist = plane.SignedDistanceTo(new Vec3(0, 0, 5));
        Assert.Equal(5.0, System.Math.Abs(dist), 10);
    }

    [Fact]
    public void Flipped_NormalReversed()
    {
        var plane = Plane.FromPoints(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var flipped = plane.Flipped;
        Assert.Equal(-plane.Normal.X, flipped.Normal.X, 15);
        Assert.Equal(-plane.Normal.Y, flipped.Normal.Y, 15);
        Assert.Equal(-plane.Normal.Z, flipped.Normal.Z, 15);
    }

    [Fact]
    public void Flipped_DistanceNegated()
    {
        var plane = Plane.FromPoints(new Vec3(0, 0, 5), new Vec3(1, 0, 5), new Vec3(0, 1, 5));
        var flipped = plane.Flipped;
        Assert.Equal(-plane.Distance, flipped.Distance, 15);
    }

    [Fact]
    public void Flipped_SignedDistance_Negated()
    {
        var plane = Plane.FromPoints(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var flipped = plane.Flipped;
        var point = new Vec3(1, 2, 3);
        Assert.Equal(-plane.SignedDistanceTo(point), flipped.SignedDistanceTo(point), 10);
    }

    [Fact]
    public void DoubleFlip_RestoresOriginal()
    {
        var plane = Plane.FromPoints(new Vec3(1, 2, 3), new Vec3(4, 5, 6), new Vec3(7, 8, 10));
        var doubleFlipped = plane.Flipped.Flipped;
        Assert.Equal(plane.Normal.X, doubleFlipped.Normal.X, 15);
        Assert.Equal(plane.Normal.Y, doubleFlipped.Normal.Y, 15);
        Assert.Equal(plane.Normal.Z, doubleFlipped.Normal.Z, 15);
        Assert.Equal(plane.Distance, doubleFlipped.Distance, 15);
    }

    [Fact]
    public void ToString_ContainsPlane()
    {
        var plane = Plane.FromPoints(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.Contains("Plane", plane.ToString());
    }

    [Fact]
    public void FromPoints_TiltedPlane_NormalOrthogonal()
    {
        var a = new Vec3(1, 0, 0);
        var b = new Vec3(0, 1, 0);
        var c = new Vec3(0, 0, 1);
        var plane = Plane.FromPoints(a, b, c);
        // Normal should be orthogonal to edges
        double dotAB = Vec3.Dot(plane.Normal, b - a);
        double dotAC = Vec3.Dot(plane.Normal, c - a);
        Assert.True(System.Math.Abs(dotAB) < 1e-10);
        Assert.True(System.Math.Abs(dotAC) < 1e-10);
    }

    [Fact]
    public void SignedDistanceTo_ParallelToPlane_SameDistance()
    {
        var plane = Plane.FromPoints(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        // All points at the same height should have the same signed distance
        double d1 = plane.SignedDistanceTo(new Vec3(0, 0, 3));
        double d2 = plane.SignedDistanceTo(new Vec3(100, 200, 3));
        Assert.Equal(d1, d2, 10);
    }

    [Fact]
    public void FromPoints_CCW_NormalDirection()
    {
        // CCW winding in XY plane: normal should point +Z
        var plane = Plane.FromPoints(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        // Cross(B-A, C-A) = Cross((1,0,0), (0,1,0)) = (0,0,1)
        Assert.True(plane.Normal.Z > 0);
    }
}
