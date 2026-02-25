using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Segment — Direction, Length, Midpoint, PointAt</summary>
public class SegmentPropertyTests
{
    [Fact]
    public void Direction_IsEndMinusStart()
    {
        var seg = new Segment(new Vec3(1, 2, 3), new Vec3(4, 6, 8));
        Assert.Equal(new Vec3(3, 4, 5), seg.Direction);
    }

    [Fact]
    public void Length_UnitSegment()
    {
        var seg = new Segment(Vec3.Zero, Vec3.UnitX);
        Assert.True(System.Math.Abs(seg.Length - 1.0) < 1e-15);
    }

    [Fact]
    public void Length_ZeroSegment()
    {
        var seg = new Segment(Vec3.Zero, Vec3.Zero);
        Assert.True(seg.Length < 1e-15);
    }

    [Fact]
    public void Length_Diagonal()
    {
        var seg = new Segment(Vec3.Zero, new Vec3(3, 4, 0));
        Assert.True(System.Math.Abs(seg.Length - 5.0) < 1e-15);
    }

    [Fact]
    public void Midpoint_Center()
    {
        var seg = new Segment(new Vec3(0, 0, 0), new Vec3(2, 4, 6));
        Assert.Equal(new Vec3(1, 2, 3), seg.Midpoint);
    }

    [Fact]
    public void Midpoint_Symmetric()
    {
        var seg = new Segment(new Vec3(-1, -1, -1), new Vec3(1, 1, 1));
        Assert.True(Vec3.Distance(seg.Midpoint, Vec3.Zero) < 1e-15);
    }

    [Fact]
    public void PointAt_Zero_IsStart()
    {
        var seg = new Segment(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        Assert.Equal(seg.Start, seg.PointAt(0));
    }

    [Fact]
    public void PointAt_One_IsEnd()
    {
        var seg = new Segment(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        Assert.Equal(seg.End, seg.PointAt(1));
    }

    [Fact]
    public void PointAt_Half_IsMidpoint()
    {
        var seg = new Segment(new Vec3(0, 0, 0), new Vec3(4, 6, 8));
        var mid = seg.PointAt(0.5);
        Assert.True(Vec3.Distance(mid, seg.Midpoint) < 1e-15);
    }

    [Fact]
    public void PointAt_BeyondOne_Extrapolates()
    {
        var seg = new Segment(Vec3.Zero, Vec3.UnitX);
        var p = seg.PointAt(2.0);
        Assert.True(System.Math.Abs(p.X - 2.0) < 1e-15);
    }

    [Fact]
    public void PointAt_Negative_Extrapolates()
    {
        var seg = new Segment(Vec3.Zero, Vec3.UnitX);
        var p = seg.PointAt(-1.0);
        Assert.True(System.Math.Abs(p.X - (-1.0)) < 1e-15);
    }

    [Fact]
    public void RecordEquality()
    {
        var a = new Segment(Vec3.Zero, Vec3.UnitX);
        var b = new Segment(Vec3.Zero, Vec3.UnitX);
        Assert.Equal(a, b);
    }

    [Fact]
    public void RecordInequality()
    {
        var a = new Segment(Vec3.Zero, Vec3.UnitX);
        var b = new Segment(Vec3.Zero, Vec3.UnitY);
        Assert.NotEqual(a, b);
    }
}
