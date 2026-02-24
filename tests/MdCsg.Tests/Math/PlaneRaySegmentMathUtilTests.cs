using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Batch 38: Plane, Ray, Segment, MathUtil tests (20 tests)</summary>
public class PlaneRaySegmentMathUtilTests
{
    // --- Plane ---
    [Fact]
    public void Plane_FromPoints_XYPlane()
    {
        var plane = Plane.FromPoints(
            new Vec3(0, 0, 0),
            new Vec3(1, 0, 0),
            new Vec3(0, 1, 0));
        // Normal should point in +Z (CCW winding in XY plane)
        Assert.True(System.Math.Abs(plane.Normal.Z - 1.0) < 1e-10);
        Assert.True(System.Math.Abs(plane.Normal.X) < 1e-10);
        Assert.True(System.Math.Abs(plane.Normal.Y) < 1e-10);
    }

    [Fact]
    public void Plane_FromPoints_OffsetPlane()
    {
        var plane = Plane.FromPoints(
            new Vec3(0, 0, 5),
            new Vec3(1, 0, 5),
            new Vec3(0, 1, 5));
        Assert.True(System.Math.Abs(plane.Distance - 5.0) < 1e-10);
    }

    [Fact]
    public void Plane_SignedDistanceTo_AbovePlane()
    {
        var plane = Plane.FromPoints(
            new Vec3(0, 0, 0),
            new Vec3(1, 0, 0),
            new Vec3(0, 1, 0));
        double dist = plane.SignedDistanceTo(new Vec3(0, 0, 3));
        Assert.True(dist > 0);
        Assert.True(System.Math.Abs(dist - 3.0) < 1e-10);
    }

    [Fact]
    public void Plane_SignedDistanceTo_BelowPlane()
    {
        var plane = Plane.FromPoints(
            new Vec3(0, 0, 0),
            new Vec3(1, 0, 0),
            new Vec3(0, 1, 0));
        double dist = plane.SignedDistanceTo(new Vec3(0, 0, -2));
        Assert.True(dist < 0);
    }

    [Fact]
    public void Plane_SignedDistanceTo_OnPlane()
    {
        var plane = Plane.FromPoints(
            new Vec3(0, 0, 0),
            new Vec3(1, 0, 0),
            new Vec3(0, 1, 0));
        double dist = plane.SignedDistanceTo(new Vec3(5, 7, 0));
        Assert.True(System.Math.Abs(dist) < 1e-10);
    }

    [Fact]
    public void Plane_Flipped_ReversesNormalAndDistance()
    {
        var plane = Plane.FromPoints(
            new Vec3(0, 0, 0),
            new Vec3(1, 0, 0),
            new Vec3(0, 1, 0));
        var flipped = plane.Flipped;
        Assert.True(System.Math.Abs(flipped.Normal.Z - (-plane.Normal.Z)) < 1e-10);
        Assert.True(System.Math.Abs(flipped.Distance - (-plane.Distance)) < 1e-10);
    }

    [Fact]
    public void Plane_ToString_ContainsPlane()
    {
        var plane = Plane.FromPoints(Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        Assert.Contains("Plane", plane.ToString());
    }

    // --- Ray ---
    [Fact]
    public void Ray_PointAt_Zero_IsOrigin()
    {
        var ray = new Ray(new Vec3(1, 2, 3), Vec3.UnitX);
        var p = ray.PointAt(0);
        Assert.True(Vec3.Distance(p, new Vec3(1, 2, 3)) < 1e-10);
    }

    [Fact]
    public void Ray_PointAt_One_IsOriginPlusDirection()
    {
        var ray = new Ray(new Vec3(1, 2, 3), new Vec3(0, 0, 5));
        var p = ray.PointAt(1);
        Assert.True(Vec3.Distance(p, new Vec3(1, 2, 8)) < 1e-10);
    }

    [Fact]
    public void Ray_PointAt_Negative_GoesBackward()
    {
        var ray = new Ray(Vec3.Zero, Vec3.UnitX);
        var p = ray.PointAt(-2);
        Assert.True(System.Math.Abs(p.X - (-2)) < 1e-10);
    }

    // --- Segment ---
    [Fact]
    public void Segment_Direction_IsEndMinusStart()
    {
        var seg = new Segment(new Vec3(1, 0, 0), new Vec3(4, 0, 0));
        Assert.True(System.Math.Abs(seg.Direction.X - 3) < 1e-10);
    }

    [Fact]
    public void Segment_Length_Correct()
    {
        var seg = new Segment(Vec3.Zero, new Vec3(3, 4, 0));
        Assert.True(System.Math.Abs(seg.Length - 5.0) < 1e-10);
    }

    [Fact]
    public void Segment_Midpoint_Correct()
    {
        var seg = new Segment(new Vec3(0, 0, 0), new Vec3(2, 4, 6));
        var mid = seg.Midpoint;
        Assert.True(Vec3.Distance(mid, new Vec3(1, 2, 3)) < 1e-10);
    }

    [Fact]
    public void Segment_PointAt_Zero_IsStart()
    {
        var seg = new Segment(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        Assert.True(Vec3.Distance(seg.PointAt(0), seg.Start) < 1e-10);
    }

    [Fact]
    public void Segment_PointAt_One_IsEnd()
    {
        var seg = new Segment(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        Assert.True(Vec3.Distance(seg.PointAt(1), seg.End) < 1e-10);
    }

    [Fact]
    public void Segment_PointAt_Half_IsMidpoint()
    {
        var seg = new Segment(new Vec3(0, 0, 0), new Vec3(10, 0, 0));
        Assert.True(Vec3.Distance(seg.PointAt(0.5), seg.Midpoint) < 1e-10);
    }

    // --- MathUtil ---
    [Fact]
    public void MathUtil_Epsilon_IsSmallPositive()
    {
        Assert.True(MathUtil.Epsilon > 0);
        Assert.True(MathUtil.Epsilon < 1e-5);
    }

    [Fact]
    public void MathUtil_SnapToGrid_RoundsCorrectly()
    {
        double val = 1.23456789;
        double snapped = MathUtil.SnapToGrid(val, 0.01);
        Assert.True(System.Math.Abs(snapped - 1.23) < 1e-12);
    }

    [Fact]
    public void MathUtil_SnapToGrid_DefaultGrid()
    {
        double val = 1.0;
        double snapped = MathUtil.SnapToGrid(val);
        Assert.True(System.Math.Abs(snapped - 1.0) < 1e-12);
    }

    [Fact]
    public void MathUtil_Fma_CorrectResult()
    {
        double result = MathUtil.Fma(3.0, 4.0, 5.0);
        Assert.True(System.Math.Abs(result - 17.0) < 1e-10);
    }
}
