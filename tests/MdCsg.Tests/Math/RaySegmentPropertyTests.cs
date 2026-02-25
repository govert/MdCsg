using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Ray and Segment — PointAt, Direction, Length, Midpoint, record equality</summary>
public class RaySegmentPropertyTests
{
    // --- Ray tests ---

    [Fact]
    public void Ray_PointAt_Zero_ReturnsOrigin()
    {
        var ray = new Ray(new Vec3(1, 2, 3), new Vec3(1, 0, 0));
        var p = ray.PointAt(0);
        Assert.Equal(1.0, p.X);
        Assert.Equal(2.0, p.Y);
        Assert.Equal(3.0, p.Z);
    }

    [Fact]
    public void Ray_PointAt_One_ReturnsOriginPlusDirection()
    {
        var ray = new Ray(new Vec3(0, 0, 0), new Vec3(3, 4, 0));
        var p = ray.PointAt(1);
        Assert.Equal(3.0, p.X);
        Assert.Equal(4.0, p.Y);
        Assert.Equal(0.0, p.Z);
    }

    [Fact]
    public void Ray_PointAt_Negative_GoesBackwards()
    {
        var ray = new Ray(new Vec3(5, 5, 5), new Vec3(1, 0, 0));
        var p = ray.PointAt(-2);
        Assert.Equal(3.0, p.X);
        Assert.Equal(5.0, p.Y);
        Assert.Equal(5.0, p.Z);
    }

    [Fact]
    public void Ray_PointAt_LargeT_Scales()
    {
        var ray = new Ray(Vec3.Zero, new Vec3(0, 0, 1));
        var p = ray.PointAt(100);
        Assert.Equal(100.0, p.Z);
    }

    [Fact]
    public void Ray_RecordEquality_SameValues()
    {
        var r1 = new Ray(new Vec3(1, 2, 3), new Vec3(0, 0, 1));
        var r2 = new Ray(new Vec3(1, 2, 3), new Vec3(0, 0, 1));
        Assert.Equal(r1, r2);
    }

    [Fact]
    public void Ray_RecordEquality_DifferentOrigin()
    {
        var r1 = new Ray(new Vec3(1, 2, 3), new Vec3(0, 0, 1));
        var r2 = new Ray(new Vec3(4, 5, 6), new Vec3(0, 0, 1));
        Assert.NotEqual(r1, r2);
    }

    [Fact]
    public void Ray_RecordEquality_DifferentDirection()
    {
        var r1 = new Ray(Vec3.Zero, Vec3.UnitX);
        var r2 = new Ray(Vec3.Zero, Vec3.UnitY);
        Assert.NotEqual(r1, r2);
    }

    // --- Segment tests ---

    [Fact]
    public void Segment_Direction_IsEndMinusStart()
    {
        var seg = new Segment(new Vec3(1, 0, 0), new Vec3(4, 0, 0));
        Assert.Equal(3.0, seg.Direction.X);
        Assert.Equal(0.0, seg.Direction.Y);
        Assert.Equal(0.0, seg.Direction.Z);
    }

    [Fact]
    public void Segment_Length_IsCorrect()
    {
        var seg = new Segment(new Vec3(0, 0, 0), new Vec3(3, 4, 0));
        Assert.True(System.Math.Abs(seg.Length - 5.0) < 1e-10);
    }

    [Fact]
    public void Segment_Length_ZeroLength()
    {
        var seg = new Segment(new Vec3(1, 1, 1), new Vec3(1, 1, 1));
        Assert.Equal(0.0, seg.Length);
    }

    [Fact]
    public void Segment_Midpoint_IsAverage()
    {
        var seg = new Segment(new Vec3(0, 0, 0), new Vec3(10, 20, 30));
        Assert.Equal(5.0, seg.Midpoint.X);
        Assert.Equal(10.0, seg.Midpoint.Y);
        Assert.Equal(15.0, seg.Midpoint.Z);
    }

    [Fact]
    public void Segment_PointAt_Zero_ReturnsStart()
    {
        var seg = new Segment(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        var p = seg.PointAt(0);
        Assert.Equal(1.0, p.X);
        Assert.Equal(2.0, p.Y);
        Assert.Equal(3.0, p.Z);
    }

    [Fact]
    public void Segment_PointAt_One_ReturnsEnd()
    {
        var seg = new Segment(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        var p = seg.PointAt(1);
        Assert.Equal(4.0, p.X);
        Assert.Equal(5.0, p.Y);
        Assert.Equal(6.0, p.Z);
    }

    [Fact]
    public void Segment_PointAt_Half_ReturnsMidpoint()
    {
        var seg = new Segment(new Vec3(0, 0, 0), new Vec3(10, 0, 0));
        var p = seg.PointAt(0.5);
        Assert.True(System.Math.Abs(p.X - 5.0) < 1e-10);
    }

    [Fact]
    public void Segment_PointAt_BeyondOne_Extrapolates()
    {
        var seg = new Segment(new Vec3(0, 0, 0), new Vec3(1, 0, 0));
        var p = seg.PointAt(2);
        Assert.Equal(2.0, p.X);
    }

    [Fact]
    public void Segment_RecordEquality_SameValues()
    {
        var s1 = new Segment(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        var s2 = new Segment(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        Assert.Equal(s1, s2);
    }

    [Fact]
    public void Segment_RecordEquality_Different()
    {
        var s1 = new Segment(new Vec3(0, 0, 0), new Vec3(1, 0, 0));
        var s2 = new Segment(new Vec3(0, 0, 0), new Vec3(0, 1, 0));
        Assert.NotEqual(s1, s2);
    }

    [Fact]
    public void Segment_Direction_Diagonal()
    {
        var seg = new Segment(new Vec3(1, 1, 1), new Vec3(4, 5, 7));
        Assert.Equal(3.0, seg.Direction.X);
        Assert.Equal(4.0, seg.Direction.Y);
        Assert.Equal(6.0, seg.Direction.Z);
    }

    [Fact]
    public void Segment_Midpoint_Symmetric()
    {
        var a = new Vec3(3, 7, 11);
        var b = new Vec3(13, 17, 19);
        var seg = new Segment(a, b);
        Assert.True(System.Math.Abs(seg.Midpoint.X - 8.0) < 1e-10);
        Assert.True(System.Math.Abs(seg.Midpoint.Y - 12.0) < 1e-10);
        Assert.True(System.Math.Abs(seg.Midpoint.Z - 15.0) < 1e-10);
    }

    [Fact]
    public void Segment_Length_3D_Pythagorean()
    {
        // 1,2,2 -> length = 3
        var seg = new Segment(Vec3.Zero, new Vec3(1, 2, 2));
        Assert.True(System.Math.Abs(seg.Length - 3.0) < 1e-10);
    }
}
