using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Segment — PointAt, Midpoint, Length, Direction properties</summary>
public class SegmentPointAtMidpointTests
{
    [Fact]
    public void PointAt_0_ReturnsStart()
    {
        var seg = new Segment(new Vec3(1, 0, 0), new Vec3(3, 0, 0));
        Assert.Equal(seg.Start, seg.PointAt(0));
    }

    [Fact]
    public void PointAt_1_ReturnsEnd()
    {
        var seg = new Segment(new Vec3(1, 0, 0), new Vec3(3, 0, 0));
        Assert.Equal(seg.End, seg.PointAt(1));
    }

    [Fact]
    public void PointAt_Half_ReturnsMidpoint()
    {
        var seg = new Segment(new Vec3(0, 0, 0), new Vec3(4, 0, 0));
        var mid = seg.PointAt(0.5);
        Assert.Equal(2.0, mid.X, 15);
        Assert.Equal(0.0, mid.Y, 15);
        Assert.Equal(0.0, mid.Z, 15);
    }

    [Fact]
    public void Midpoint_IsPointAtHalf()
    {
        var seg = new Segment(new Vec3(1, 2, 3), new Vec3(5, 6, 7));
        var mid = seg.Midpoint;
        var pointAtHalf = seg.PointAt(0.5);
        Assert.Equal(pointAtHalf.X, mid.X, 15);
        Assert.Equal(pointAtHalf.Y, mid.Y, 15);
        Assert.Equal(pointAtHalf.Z, mid.Z, 15);
    }

    [Fact]
    public void Midpoint_IsAverageOfEndpoints()
    {
        var seg = new Segment(new Vec3(1, 2, 3), new Vec3(5, 6, 7));
        var expected = (seg.Start + seg.End) * 0.5;
        Assert.Equal(expected, seg.Midpoint);
    }

    [Fact]
    public void Length_UnitSegmentAlongX()
    {
        var seg = new Segment(Vec3.Zero, new Vec3(1, 0, 0));
        Assert.Equal(1.0, seg.Length, 15);
    }

    [Fact]
    public void Length_DiagonalSegment()
    {
        var seg = new Segment(Vec3.Zero, new Vec3(1, 1, 1));
        Assert.Equal(System.Math.Sqrt(3), seg.Length, 10);
    }

    [Fact]
    public void Length_ZeroLengthSegment()
    {
        var seg = new Segment(new Vec3(1, 2, 3), new Vec3(1, 2, 3));
        Assert.Equal(0.0, seg.Length, 15);
    }

    [Fact]
    public void Direction_IsEndMinusStart()
    {
        var start = new Vec3(1, 2, 3);
        var end = new Vec3(4, 5, 6);
        var seg = new Segment(start, end);
        Assert.Equal(end - start, seg.Direction);
    }

    [Fact]
    public void Direction_UnitX()
    {
        var seg = new Segment(Vec3.Zero, new Vec3(1, 0, 0));
        Assert.Equal(Vec3.UnitX, seg.Direction);
    }

    [Fact]
    public void RecordEquality()
    {
        var a = new Segment(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        var b = new Segment(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        Assert.Equal(a, b);
    }

    [Fact]
    public void RecordInequality()
    {
        var a = new Segment(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        var b = new Segment(new Vec3(1, 2, 3), new Vec3(4, 5, 7));
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void PointAt_ExtrapolatesBeyond1()
    {
        var seg = new Segment(Vec3.Zero, new Vec3(1, 0, 0));
        var pt = seg.PointAt(2.0);
        Assert.Equal(2.0, pt.X, 15);
    }

    [Fact]
    public void PointAt_ExtrapolatesBefore0()
    {
        var seg = new Segment(Vec3.Zero, new Vec3(1, 0, 0));
        var pt = seg.PointAt(-1.0);
        Assert.Equal(-1.0, pt.X, 15);
    }
}
