using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Segment, Ray, and Plane type tests — properties, parametric evaluation, edge cases</summary>
public class SegmentRayPlaneTests
{
    // ── Segment ──

    [Fact]
    public void Segment_Direction_IsEndMinusStart()
    {
        var s = new Segment(new Vec3(1, 2, 3), new Vec3(4, 6, 8));
        Assert.Equal(new Vec3(3, 4, 5), s.Direction);
    }

    [Fact]
    public void Segment_Length_CorrectForUnitAxis()
    {
        var s = new Segment(Vec3.Zero, new Vec3(1, 0, 0));
        Assert.Equal(1.0, s.Length, 12);
    }

    [Fact]
    public void Segment_Length_Diagonal()
    {
        var s = new Segment(Vec3.Zero, new Vec3(3, 4, 0));
        Assert.Equal(5.0, s.Length, 12);
    }

    [Fact]
    public void Segment_ZeroLength()
    {
        var s = new Segment(new Vec3(1, 2, 3), new Vec3(1, 2, 3));
        Assert.Equal(0.0, s.Length, 12);
        Assert.Equal(new Vec3(1, 2, 3), s.Midpoint);
    }

    [Fact]
    public void Segment_Midpoint_IsAverage()
    {
        var s = new Segment(new Vec3(0, 0, 0), new Vec3(10, 20, 30));
        Assert.Equal(new Vec3(5, 10, 15), s.Midpoint);
    }

    [Fact]
    public void Segment_PointAt_Zero_ReturnsStart()
    {
        var s = new Segment(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        var p = s.PointAt(0);
        Assert.Equal(s.Start, p);
    }

    [Fact]
    public void Segment_PointAt_One_ReturnsEnd()
    {
        var s = new Segment(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        var p = s.PointAt(1);
        Assert.Equal(s.End, p);
    }

    [Fact]
    public void Segment_PointAt_Half_ReturnsMidpoint()
    {
        var s = new Segment(new Vec3(0, 0, 0), new Vec3(10, 20, 30));
        var p = s.PointAt(0.5);
        Assert.Equal(s.Midpoint, p);
    }

    [Fact]
    public void Segment_PointAt_Extrapolates_BeyondOne()
    {
        var s = new Segment(Vec3.Zero, new Vec3(1, 0, 0));
        var p = s.PointAt(2);
        Assert.Equal(new Vec3(2, 0, 0), p);
    }

    [Fact]
    public void Segment_PointAt_Extrapolates_Negative()
    {
        var s = new Segment(new Vec3(1, 0, 0), new Vec3(2, 0, 0));
        var p = s.PointAt(-1);
        Assert.Equal(new Vec3(0, 0, 0), p);
    }

    [Fact]
    public void Segment_RecordEquality()
    {
        var a = new Segment(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        var b = new Segment(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        Assert.Equal(a, b);
    }

    [Fact]
    public void Segment_RecordInequality()
    {
        var a = new Segment(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        var b = new Segment(new Vec3(1, 2, 3), new Vec3(4, 5, 7));
        Assert.NotEqual(a, b);
    }

    // ── Ray ──

    [Fact]
    public void Ray_PointAt_Zero_ReturnsOrigin()
    {
        var r = new Ray(new Vec3(1, 2, 3), new Vec3(1, 0, 0));
        Assert.Equal(r.Origin, r.PointAt(0));
    }

    [Fact]
    public void Ray_PointAt_Positive()
    {
        var r = new Ray(Vec3.Zero, new Vec3(0, 1, 0));
        Assert.Equal(new Vec3(0, 5, 0), r.PointAt(5));
    }

    [Fact]
    public void Ray_PointAt_Negative()
    {
        var r = new Ray(new Vec3(10, 0, 0), new Vec3(1, 0, 0));
        Assert.Equal(new Vec3(7, 0, 0), r.PointAt(-3));
    }

    [Fact]
    public void Ray_RecordEquality()
    {
        var a = new Ray(Vec3.Zero, new Vec3(1, 0, 0));
        var b = new Ray(Vec3.Zero, new Vec3(1, 0, 0));
        Assert.Equal(a, b);
    }

    [Fact]
    public void Ray_DiagonalDirection_PointAt()
    {
        var dir = new Vec3(1, 1, 1).Normalized;
        var r = new Ray(Vec3.Zero, dir);
        var p = r.PointAt(System.Math.Sqrt(3));
        Assert.True(Vec3.Distance(p, new Vec3(1, 1, 1)) < 1e-12);
    }

    // ── Plane ──

    [Fact]
    public void Plane_FromPoints_XYPlane()
    {
        var p = Plane.FromPoints(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.True(System.Math.Abs(p.Normal.Z - 1) < 1e-12 || System.Math.Abs(p.Normal.Z + 1) < 1e-12);
        Assert.True(System.Math.Abs(p.Distance) < 1e-12);
    }

    [Fact]
    public void Plane_FromPoints_OffsetPlane()
    {
        var p = Plane.FromPoints(new Vec3(0, 0, 5), new Vec3(1, 0, 5), new Vec3(0, 1, 5));
        Assert.True(System.Math.Abs(p.Normal.Z) > 0.99);
        Assert.True(System.Math.Abs(System.Math.Abs(p.Distance) - 5) < 1e-12);
    }

    [Fact]
    public void Plane_SignedDistanceTo_PointOnPlane_Zero()
    {
        var p = Plane.FromPoints(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.True(System.Math.Abs(p.SignedDistanceTo(new Vec3(0.5, 0.3, 0))) < 1e-12);
    }

    [Fact]
    public void Plane_SignedDistanceTo_PointAbove_Positive()
    {
        var p = Plane.FromPoints(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        double d = p.SignedDistanceTo(new Vec3(0, 0, 10));
        Assert.True(System.Math.Abs(d) > 9);
    }

    [Fact]
    public void Plane_SignedDistanceTo_PointBelow_Negative()
    {
        var p = Plane.FromPoints(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        double above = p.SignedDistanceTo(new Vec3(0, 0, 1));
        double below = p.SignedDistanceTo(new Vec3(0, 0, -1));
        Assert.True(above * below < 0); // opposite signs
    }

    [Fact]
    public void Plane_Flipped_ReversesNormalAndDistance()
    {
        var p = Plane.FromPoints(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var f = p.Flipped;
        Assert.True(Vec3.Distance(p.Normal, -f.Normal) < 1e-12);
        Assert.True(System.Math.Abs(p.Distance + f.Distance) < 1e-12);
    }

    [Fact]
    public void Plane_Flipped_ReversesSigns()
    {
        var p = Plane.FromPoints(new Vec3(0, 0, 5), new Vec3(1, 0, 5), new Vec3(0, 1, 5));
        double d1 = p.SignedDistanceTo(new Vec3(0, 0, 10));
        double d2 = p.Flipped.SignedDistanceTo(new Vec3(0, 0, 10));
        Assert.True(d1 * d2 < 0); // opposite signs
    }

    [Fact]
    public void Plane_ToString_ContainsNormal()
    {
        var p = Plane.FromPoints(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.Contains("Plane(", p.ToString());
    }

    [Fact]
    public void Plane_RecordEquality()
    {
        var a = new Plane(new Vec3(0, 0, 1), 5);
        var b = new Plane(new Vec3(0, 0, 1), 5);
        Assert.Equal(a, b);
    }

    [Fact]
    public void Plane_FromPoints_Winding_Matters()
    {
        var p1 = Plane.FromPoints(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var p2 = Plane.FromPoints(Vec3.Zero, new Vec3(0, 1, 0), new Vec3(1, 0, 0));
        // Reversed winding → opposite normal
        Assert.True(Vec3.Distance(p1.Normal, -p2.Normal) < 1e-12);
    }
}
