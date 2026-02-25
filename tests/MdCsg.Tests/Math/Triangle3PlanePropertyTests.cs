using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Triangle3 and Plane — Normal, Area, Centroid, Bounds, SignedDistance, Plane.FromPoints</summary>
public class Triangle3PlanePropertyTests
{
    [Fact]
    public void Triangle3_Normal_CorrectDirection()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var normal = tri.Normal;
        Assert.True(System.Math.Abs(normal.Z - 1.0) < 1e-10);
        Assert.True(System.Math.Abs(normal.X) < 1e-10);
    }

    [Fact]
    public void Triangle3_UnitNormal_HasUnitLength()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(3, 0, 0), new Vec3(0, 4, 0));
        var n = tri.UnitNormal;
        Assert.True(System.Math.Abs(n.Length - 1.0) < 1e-10);
    }

    [Fact]
    public void Triangle3_Area_RightTriangle()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(3, 0, 0), new Vec3(0, 4, 0));
        Assert.True(System.Math.Abs(tri.Area - 6.0) < 1e-10);
    }

    [Fact]
    public void Triangle3_Centroid_Average()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(3, 0, 0), new Vec3(0, 3, 0));
        Assert.True(System.Math.Abs(tri.Centroid.X - 1.0) < 1e-10);
        Assert.True(System.Math.Abs(tri.Centroid.Y - 1.0) < 1e-10);
    }

    [Fact]
    public void Triangle3_Bounds_EnclosesVertices()
    {
        var tri = new Triangle3(new Vec3(1, 2, 3), new Vec3(4, 5, 6), new Vec3(7, 8, 9));
        var bounds = tri.Bounds;
        Assert.True(bounds.Contains(tri.A));
        Assert.True(bounds.Contains(tri.B));
        Assert.True(bounds.Contains(tri.C));
    }

    [Fact]
    public void Triangle3_Indexer_Works()
    {
        var tri = new Triangle3(new Vec3(1, 0, 0), new Vec3(0, 1, 0), new Vec3(0, 0, 1));
        Assert.Equal(new Vec3(1, 0, 0), tri[0]);
        Assert.Equal(new Vec3(0, 1, 0), tri[1]);
        Assert.Equal(new Vec3(0, 0, 1), tri[2]);
    }

    [Fact]
    public void Triangle3_Indexer_OutOfRange_Throws()
    {
        var tri = new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        Assert.Throws<ArgumentOutOfRangeException>(() => tri[3]);
        Assert.Throws<ArgumentOutOfRangeException>(() => tri[-1]);
    }

    [Fact]
    public void Triangle3_DoubleArea_TwiceArea()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(0, 3, 0));
        Assert.True(System.Math.Abs(tri.DoubleArea - tri.Area * 2.0) < 1e-10);
    }

    [Fact]
    public void Plane_FromPoints_NormalPointsUp()
    {
        var plane = Plane.FromPoints(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.True(System.Math.Abs(plane.Normal.Z - 1.0) < 1e-10);
    }

    [Fact]
    public void Plane_SignedDistanceTo_OnPlane_Zero()
    {
        var plane = Plane.FromPoints(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.True(System.Math.Abs(plane.SignedDistanceTo(new Vec3(0.5, 0.5, 0))) < 1e-10);
    }

    [Fact]
    public void Plane_SignedDistanceTo_Above_Positive()
    {
        var plane = Plane.FromPoints(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.True(plane.SignedDistanceTo(new Vec3(0, 0, 1)) > 0);
    }

    [Fact]
    public void Plane_SignedDistanceTo_Below_Negative()
    {
        var plane = Plane.FromPoints(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.True(plane.SignedDistanceTo(new Vec3(0, 0, -1)) < 0);
    }

    [Fact]
    public void Plane_Flipped_ReversesNormal()
    {
        var plane = Plane.FromPoints(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var flipped = plane.Flipped;
        Assert.True(System.Math.Abs(flipped.Normal.Z + plane.Normal.Z) < 1e-10);
    }

    [Fact]
    public void Plane_Flipped_ReversesSignedDistance()
    {
        var plane = Plane.FromPoints(
            new Vec3(0, 0, 5), new Vec3(1, 0, 5), new Vec3(0, 1, 5));
        var p = new Vec3(0, 0, 10);
        double d1 = plane.SignedDistanceTo(p);
        double d2 = plane.Flipped.SignedDistanceTo(p);
        Assert.True(System.Math.Abs(d1 + d2) < 1e-10);
    }

    [Fact]
    public void Triangle3_Plane_ContainsAllVertices()
    {
        var tri = new Triangle3(new Vec3(1, 2, 3), new Vec3(4, 5, 6), new Vec3(7, 8, 0));
        var plane = tri.Plane;
        Assert.True(System.Math.Abs(plane.SignedDistanceTo(tri.A)) < 1e-10);
        Assert.True(System.Math.Abs(plane.SignedDistanceTo(tri.B)) < 1e-10);
        Assert.True(System.Math.Abs(plane.SignedDistanceTo(tri.C)) < 1e-10);
    }

    [Fact]
    public void Plane_FromPoints_OffsetPlane_CorrectDistance()
    {
        var plane = Plane.FromPoints(
            new Vec3(0, 0, 5), new Vec3(1, 0, 5), new Vec3(0, 1, 5));
        Assert.True(System.Math.Abs(plane.Distance - 5.0) < 1e-10);
    }
}
