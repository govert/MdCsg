using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Segment struct — Direction, Length, Midpoint, PointAt, record equality</summary>
public class SegmentPropertyTests
{
    [Fact]
    public void Direction_IsEndMinusStart()
    {
        var seg = new Segment(new Vec3(1, 2, 3), new Vec3(4, 6, 8));
        var dir = seg.Direction;
        Assert.Equal(3, dir.X, 1e-14);
        Assert.Equal(4, dir.Y, 1e-14);
        Assert.Equal(5, dir.Z, 1e-14);
    }

    [Fact]
    public void Length_UnitX()
    {
        var seg = new Segment(Vec3.Zero, new Vec3(1, 0, 0));
        Assert.Equal(1.0, seg.Length, 1e-14);
    }

    [Fact]
    public void Length_Diagonal()
    {
        var seg = new Segment(Vec3.Zero, new Vec3(3, 4, 0));
        Assert.Equal(5.0, seg.Length, 1e-14);
    }

    [Fact]
    public void Length_Degenerate_IsZero()
    {
        var p = new Vec3(5, 5, 5);
        var seg = new Segment(p, p);
        Assert.Equal(0, seg.Length, 1e-14);
    }

    [Fact]
    public void Midpoint_IsAverage()
    {
        var seg = new Segment(new Vec3(0, 0, 0), new Vec3(4, 6, 8));
        var mid = seg.Midpoint;
        Assert.Equal(2, mid.X, 1e-14);
        Assert.Equal(3, mid.Y, 1e-14);
        Assert.Equal(4, mid.Z, 1e-14);
    }

    [Fact]
    public void Midpoint_Symmetric()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(7, 8, 9);
        var mid1 = new Segment(a, b).Midpoint;
        var mid2 = new Segment(b, a).Midpoint;
        Assert.Equal(mid1.X, mid2.X, 1e-14);
        Assert.Equal(mid1.Y, mid2.Y, 1e-14);
        Assert.Equal(mid1.Z, mid2.Z, 1e-14);
    }

    [Fact]
    public void PointAt_Zero_IsStart()
    {
        var seg = new Segment(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        var p = seg.PointAt(0);
        Assert.Equal(1, p.X, 1e-14);
        Assert.Equal(2, p.Y, 1e-14);
        Assert.Equal(3, p.Z, 1e-14);
    }

    [Fact]
    public void PointAt_One_IsEnd()
    {
        var seg = new Segment(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        var p = seg.PointAt(1);
        Assert.Equal(4, p.X, 1e-14);
        Assert.Equal(5, p.Y, 1e-14);
        Assert.Equal(6, p.Z, 1e-14);
    }

    [Fact]
    public void PointAt_Half_IsMidpoint()
    {
        var seg = new Segment(new Vec3(0, 0, 0), new Vec3(10, 0, 0));
        var p = seg.PointAt(0.5);
        Assert.Equal(5, p.X, 1e-14);
    }

    [Fact]
    public void PointAt_Extrapolation_BeyondEnd()
    {
        var seg = new Segment(Vec3.Zero, new Vec3(1, 0, 0));
        var p = seg.PointAt(2.0);
        Assert.Equal(2, p.X, 1e-14);
    }

    [Fact]
    public void PointAt_Negative_BeforeStart()
    {
        var seg = new Segment(Vec3.Zero, new Vec3(1, 0, 0));
        var p = seg.PointAt(-1.0);
        Assert.Equal(-1, p.X, 1e-14);
    }

    [Fact]
    public void RecordEquality_SameEndpoints()
    {
        var a = new Segment(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        var b = new Segment(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        Assert.Equal(a, b);
    }

    [Fact]
    public void RecordInequality_DifferentEndpoints()
    {
        var a = new Segment(Vec3.Zero, new Vec3(1, 0, 0));
        var b = new Segment(Vec3.Zero, new Vec3(0, 1, 0));
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void RecordInequality_ReversedEndpoints()
    {
        var a = new Segment(new Vec3(1, 0, 0), Vec3.Zero);
        var b = new Segment(Vec3.Zero, new Vec3(1, 0, 0));
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Direction_Reversed_IsNegated()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var d1 = new Segment(a, b).Direction;
        var d2 = new Segment(b, a).Direction;
        Assert.Equal(d1.X, -d2.X, 1e-14);
        Assert.Equal(d1.Y, -d2.Y, 1e-14);
        Assert.Equal(d1.Z, -d2.Z, 1e-14);
    }

    [Fact]
    public void Length_Reversed_IsSame()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        Assert.Equal(new Segment(a, b).Length, new Segment(b, a).Length, 1e-14);
    }

    [Fact]
    public void PointAt_Quarter_Increments()
    {
        var seg = new Segment(Vec3.Zero, new Vec3(8, 0, 0));
        Assert.Equal(2, seg.PointAt(0.25).X, 1e-14);
        Assert.Equal(4, seg.PointAt(0.50).X, 1e-14);
        Assert.Equal(6, seg.PointAt(0.75).X, 1e-14);
    }

    [Fact]
    public void Start_End_Preserved()
    {
        var start = new Vec3(1, 2, 3);
        var end = new Vec3(4, 5, 6);
        var seg = new Segment(start, end);
        Assert.Equal(start, seg.Start);
        Assert.Equal(end, seg.End);
    }

    [Fact]
    public void Length_3D_Diagonal()
    {
        var seg = new Segment(Vec3.Zero, new Vec3(1, 1, 1));
        Assert.Equal(System.Math.Sqrt(3), seg.Length, 1e-14);
    }
}
