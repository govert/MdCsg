using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Segment and Ray — constructors, properties, PointAt, parameter interpolation</summary>
public class SegmentRayMathPropertyTests
{
    // --- Segment tests ---

    [Fact]
    public void Segment_Direction_IsEndMinusStart()
    {
        var seg = new Segment(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        Assert.Equal(new Vec3(3, 3, 3), seg.Direction);
    }

    [Fact]
    public void Segment_Length_CorrectValue()
    {
        var seg = new Segment(Vec3.Zero, new Vec3(3, 4, 0));
        Assert.Equal(5.0, seg.Length, 10);
    }

    [Fact]
    public void Segment_Length_Zero()
    {
        var seg = new Segment(Vec3.Zero, Vec3.Zero);
        Assert.Equal(0.0, seg.Length);
    }

    [Fact]
    public void Segment_Midpoint_IsAverage()
    {
        var seg = new Segment(new Vec3(0, 0, 0), new Vec3(2, 4, 6));
        Assert.Equal(new Vec3(1, 2, 3), seg.Midpoint);
    }

    [Fact]
    public void Segment_PointAt_Zero_IsStart()
    {
        var seg = new Segment(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        Assert.Equal(seg.Start, seg.PointAt(0));
    }

    [Fact]
    public void Segment_PointAt_One_IsEnd()
    {
        var seg = new Segment(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        Assert.Equal(seg.End, seg.PointAt(1));
    }

    [Fact]
    public void Segment_PointAt_Half_IsMidpoint()
    {
        var seg = new Segment(new Vec3(0, 0, 0), new Vec3(4, 4, 4));
        var mid = seg.PointAt(0.5);
        Assert.Equal(2.0, mid.X, 10);
        Assert.Equal(2.0, mid.Y, 10);
        Assert.Equal(2.0, mid.Z, 10);
    }

    [Fact]
    public void Segment_PointAt_Extrapolation()
    {
        var seg = new Segment(Vec3.Zero, new Vec3(1, 0, 0));
        var p = seg.PointAt(2.0);
        Assert.Equal(2.0, p.X, 10);
    }

    [Fact]
    public void Segment_RecordEquality()
    {
        var s1 = new Segment(Vec3.Zero, Vec3.UnitX);
        var s2 = new Segment(Vec3.Zero, Vec3.UnitX);
        Assert.Equal(s1, s2);
    }

    [Fact]
    public void Segment_RecordInequality()
    {
        var s1 = new Segment(Vec3.Zero, Vec3.UnitX);
        var s2 = new Segment(Vec3.Zero, Vec3.UnitY);
        Assert.NotEqual(s1, s2);
    }

    // --- Ray tests ---

    [Fact]
    public void Ray_PointAt_Zero_IsOrigin()
    {
        var ray = new Ray(new Vec3(1, 2, 3), Vec3.UnitX);
        Assert.Equal(ray.Origin, ray.PointAt(0));
    }

    [Fact]
    public void Ray_PointAt_PositiveT()
    {
        var ray = new Ray(Vec3.Zero, new Vec3(1, 0, 0));
        var p = ray.PointAt(5.0);
        Assert.Equal(5.0, p.X, 10);
        Assert.Equal(0.0, p.Y, 10);
    }

    [Fact]
    public void Ray_PointAt_NegativeT()
    {
        var ray = new Ray(Vec3.Zero, new Vec3(1, 0, 0));
        var p = ray.PointAt(-3.0);
        Assert.Equal(-3.0, p.X, 10);
    }

    [Fact]
    public void Ray_RecordEquality()
    {
        var r1 = new Ray(Vec3.Zero, Vec3.UnitX);
        var r2 = new Ray(Vec3.Zero, Vec3.UnitX);
        Assert.Equal(r1, r2);
    }

    [Fact]
    public void Ray_RecordInequality_DifferentOrigin()
    {
        var r1 = new Ray(Vec3.Zero, Vec3.UnitX);
        var r2 = new Ray(Vec3.UnitY, Vec3.UnitX);
        Assert.NotEqual(r1, r2);
    }

    [Fact]
    public void Ray_RecordInequality_DifferentDirection()
    {
        var r1 = new Ray(Vec3.Zero, Vec3.UnitX);
        var r2 = new Ray(Vec3.Zero, Vec3.UnitY);
        Assert.NotEqual(r1, r2);
    }

    [Fact]
    public void Ray_PointAt_DiagonalDirection()
    {
        var dir = new Vec3(1, 1, 1).Normalized;
        var ray = new Ray(Vec3.Zero, dir);
        var p = ray.PointAt(System.Math.Sqrt(3));
        Assert.True(System.Math.Abs(p.X - 1.0) < 1e-10);
        Assert.True(System.Math.Abs(p.Y - 1.0) < 1e-10);
        Assert.True(System.Math.Abs(p.Z - 1.0) < 1e-10);
    }

    [Fact]
    public void Segment_UnitX_Length_IsOne()
    {
        var seg = new Segment(Vec3.Zero, Vec3.UnitX);
        Assert.Equal(1.0, seg.Length, 10);
    }

    [Fact]
    public void Segment_UnitY_Direction()
    {
        var seg = new Segment(Vec3.Zero, Vec3.UnitY);
        Assert.Equal(Vec3.UnitY, seg.Direction);
    }
}
