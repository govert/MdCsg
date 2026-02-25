using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Ray and Segment — construction, direction, length, midpoint, parametric evaluation</summary>
public class RaySegmentIntersectionTests
{
    [Fact]
    public void Ray_Origin_Stored()
    {
        var ray = new Ray(new Vec3(1, 2, 3), new Vec3(0, 0, 1));
        Assert.Equal(new Vec3(1, 2, 3), ray.Origin);
    }

    [Fact]
    public void Ray_Direction_Stored()
    {
        var ray = new Ray(new Vec3(0, 0, 0), new Vec3(1, 0, 0));
        Assert.Equal(new Vec3(1, 0, 0), ray.Direction);
    }

    [Fact]
    public void Ray_At_Parametric()
    {
        var ray = new Ray(new Vec3(0, 0, 0), new Vec3(1, 0, 0));
        var point = ray.PointAt(3);
        Assert.Equal(new Vec3(3, 0, 0), point);
    }

    [Fact]
    public void Ray_At_Zero_IsOrigin()
    {
        var ray = new Ray(new Vec3(1, 2, 3), new Vec3(0, 1, 0));
        Assert.Equal(ray.Origin, ray.PointAt(0));
    }

    [Fact]
    public void Ray_At_Negative_Behind()
    {
        var ray = new Ray(new Vec3(0, 0, 0), new Vec3(1, 0, 0));
        var point = ray.PointAt(-2);
        Assert.Equal(new Vec3(-2, 0, 0), point);
    }

    [Fact]
    public void Segment_Length_Correct()
    {
        var seg = new Segment(new Vec3(0, 0, 0), new Vec3(3, 4, 0));
        Assert.Equal(5, seg.Length, 1e-10);
    }

    [Fact]
    public void Segment_Midpoint_HalfWay()
    {
        var seg = new Segment(new Vec3(0, 0, 0), new Vec3(4, 0, 0));
        Assert.Equal(new Vec3(2, 0, 0), seg.Midpoint);
    }

    [Fact]
    public void Segment_Midpoint_Equidistant()
    {
        var seg = new Segment(new Vec3(1, 2, 3), new Vec3(5, 6, 7));
        var d1 = Vec3.Distance(seg.Midpoint, seg.Start);
        var d2 = Vec3.Distance(seg.Midpoint, seg.End);
        Assert.Equal(d1, d2, 1e-10);
    }

    [Fact]
    public void Segment_ZeroLength()
    {
        var seg = new Segment(new Vec3(1, 1, 1), new Vec3(1, 1, 1));
        Assert.Equal(0, seg.Length, 1e-10);
    }

    [Fact]
    public void Segment_Direction_CorrectVector()
    {
        var seg = new Segment(new Vec3(0, 0, 0), new Vec3(3, 0, 0));
        var dir = seg.End - seg.Start;
        Assert.Equal(new Vec3(3, 0, 0), dir);
    }

    [Fact]
    public void Segment_Midpoint_3D()
    {
        var seg = new Segment(new Vec3(1, 2, 3), new Vec3(3, 4, 5));
        Assert.Equal(new Vec3(2, 3, 4), seg.Midpoint);
    }

    [Fact]
    public void Ray_DiagonalDirection()
    {
        var ray = new Ray(Vec3.Zero, new Vec3(1, 1, 1));
        var p = ray.PointAt(2);
        Assert.Equal(new Vec3(2, 2, 2), p);
    }

    [Fact]
    public void Segment_NegativeCoords()
    {
        var seg = new Segment(new Vec3(-1, -2, -3), new Vec3(1, 2, 3));
        Assert.Equal(new Vec3(0, 0, 0), seg.Midpoint);
    }
}
