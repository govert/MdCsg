using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Segment struct — Direction, Length, Midpoint, PointAt, record equality</summary>
public class SegmentStructTests
{
    [Fact]
    public void Direction_IsEndMinusStart()
    {
        var s = new Segment(new Vec3(1, 2, 3), new Vec3(4, 6, 8));
        Assert.Equal(new Vec3(3, 4, 5), s.Direction);
    }

    [Fact]
    public void Length_UnitSegment()
    {
        var s = new Segment(Vec3.Zero, new Vec3(1, 0, 0));
        Assert.Equal(1.0, s.Length, 10);
    }

    [Fact]
    public void Length_Diagonal()
    {
        var s = new Segment(Vec3.Zero, new Vec3(3, 4, 0));
        Assert.Equal(5.0, s.Length, 10);
    }

    [Fact]
    public void Length_ZeroLength()
    {
        var s = new Segment(new Vec3(1, 2, 3), new Vec3(1, 2, 3));
        Assert.Equal(0.0, s.Length);
    }

    [Fact]
    public void Midpoint_IsAverage()
    {
        var s = new Segment(new Vec3(0, 0, 0), new Vec3(2, 4, 6));
        Assert.Equal(new Vec3(1, 2, 3), s.Midpoint);
    }

    [Fact]
    public void Midpoint_NegativeCoords()
    {
        var s = new Segment(new Vec3(-2, -4, -6), new Vec3(2, 4, 6));
        Assert.Equal(Vec3.Zero, s.Midpoint);
    }

    [Fact]
    public void PointAt_Zero_ReturnsStart()
    {
        var s = new Segment(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        Assert.Equal(s.Start, s.PointAt(0));
    }

    [Fact]
    public void PointAt_One_ReturnsEnd()
    {
        var s = new Segment(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        Assert.Equal(s.End, s.PointAt(1));
    }

    [Fact]
    public void PointAt_Half_ReturnsMidpoint()
    {
        var s = new Segment(new Vec3(0, 0, 0), new Vec3(4, 6, 8));
        Assert.Equal(new Vec3(2, 3, 4), s.PointAt(0.5));
    }

    [Fact]
    public void PointAt_Extrapolation()
    {
        var s = new Segment(Vec3.Zero, new Vec3(1, 0, 0));
        Assert.Equal(new Vec3(2, 0, 0), s.PointAt(2.0));
    }

    [Fact]
    public void PointAt_NegativeT()
    {
        var s = new Segment(new Vec3(1, 0, 0), new Vec3(3, 0, 0));
        Assert.Equal(new Vec3(-1, 0, 0), s.PointAt(-1.0));
    }

    [Fact]
    public void RecordEquality_SameValues()
    {
        var s1 = new Segment(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        var s2 = new Segment(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        Assert.Equal(s1, s2);
    }

    [Fact]
    public void RecordEquality_DifferentValues()
    {
        var s1 = new Segment(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        var s2 = new Segment(new Vec3(0, 0, 0), new Vec3(4, 5, 6));
        Assert.NotEqual(s1, s2);
    }

    [Fact]
    public void Start_Property()
    {
        var s = new Segment(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        Assert.Equal(new Vec3(1, 2, 3), s.Start);
    }

    [Fact]
    public void End_Property()
    {
        var s = new Segment(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        Assert.Equal(new Vec3(4, 5, 6), s.End);
    }

    [Fact]
    public void Direction_ZeroSegment_IsZero()
    {
        var s = new Segment(new Vec3(5, 5, 5), new Vec3(5, 5, 5));
        Assert.Equal(Vec3.Zero, s.Direction);
    }

    [Fact]
    public void PointAt_QuarterAndThreeQuarter()
    {
        var s = new Segment(Vec3.Zero, new Vec3(4, 0, 0));
        Assert.Equal(new Vec3(1, 0, 0), s.PointAt(0.25));
        Assert.Equal(new Vec3(3, 0, 0), s.PointAt(0.75));
    }

    [Fact]
    public void Midpoint_PointAt_Agreement()
    {
        var s = new Segment(new Vec3(1, 3, 5), new Vec3(7, 11, 13));
        var mid = s.Midpoint;
        var ptHalf = s.PointAt(0.5);
        Assert.Equal(mid, ptHalf);
    }

    [Fact]
    public void Length_3D_Diagonal()
    {
        var s = new Segment(Vec3.Zero, new Vec3(1, 1, 1));
        Assert.Equal(System.Math.Sqrt(3), s.Length, 10);
    }

    [Fact]
    public void Direction_Length_Agreement()
    {
        var s = new Segment(new Vec3(1, 2, 3), new Vec3(5, 6, 7));
        Assert.Equal(s.Length, s.Direction.Length, 10);
    }
}
