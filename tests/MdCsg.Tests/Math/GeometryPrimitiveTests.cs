using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Geometry primitive tests — Vec2, Triangle3, Segment, Plane edge cases</summary>
public class GeometryPrimitiveTests
{
    // --- Vec2 ---

    [Fact]
    public void Vec2_Constructor()
    {
        var v = new Vec2(3, 4);
        Assert.Equal(3, v.X, 10);
        Assert.Equal(4, v.Y, 10);
    }

    [Fact]
    public void Vec2_Dot_Orthogonal()
    {
        Assert.Equal(0, Vec2.Dot(new Vec2(1, 0), new Vec2(0, 1)), 10);
    }

    [Fact]
    public void Vec2_Dot_Parallel()
    {
        Assert.Equal(2, Vec2.Dot(new Vec2(1, 1), new Vec2(1, 1)), 10);
    }

    [Fact]
    public void Vec2_Add()
    {
        var r = new Vec2(1, 2) + new Vec2(3, 4);
        Assert.Equal(4, r.X, 10);
        Assert.Equal(6, r.Y, 10);
    }

    [Fact]
    public void Vec2_Subtract()
    {
        var r = new Vec2(5, 6) - new Vec2(1, 2);
        Assert.Equal(4, r.X, 10);
        Assert.Equal(4, r.Y, 10);
    }

    [Fact]
    public void Vec2_Negate()
    {
        var v = -new Vec2(3, -4);
        Assert.Equal(-3, v.X, 10);
        Assert.Equal(4, v.Y, 10);
    }

    [Fact]
    public void Vec2_ScalarMultiply()
    {
        var v = new Vec2(3, 4) * 2;
        Assert.Equal(6, v.X, 10);
        Assert.Equal(8, v.Y, 10);
    }

    // --- Triangle3 advanced ---

    [Fact]
    public void Triangle3_ZeroArea_DegenerateCollinear()
    {
        var t = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(2, 0, 0));
        Assert.True(t.Area < 1e-10);
    }

    [Fact]
    public void Triangle3_ZeroArea_DuplicateVertices()
    {
        var t = new Triangle3(new Vec3(1, 1, 1), new Vec3(1, 1, 1), new Vec3(2, 2, 2));
        Assert.True(t.Area < 1e-10);
    }

    [Fact]
    public void Triangle3_Centroid_ThirdOfSum()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(6, 0, 0);
        var c = new Vec3(0, 6, 0);
        var t = new Triangle3(a, b, c);
        Assert.Equal(2, t.Centroid.X, 10);
        Assert.Equal(2, t.Centroid.Y, 10);
        Assert.Equal(0, t.Centroid.Z, 10);
    }

    [Fact]
    public void Triangle3_Normal_ZPlane()
    {
        var t = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.True(System.Math.Abs(t.Normal.Z) > 0.9);
        Assert.True(t.Normal.Z > 0); // CCW in XY → +Z normal
    }

    [Fact]
    public void Triangle3_Area_ScalesWithSize()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(0, 2, 0));
        Assert.Equal(t1.Area * 4, t2.Area, 10); // Area scales as square of linear
    }

    [Fact]
    public void Triangle3_Indexer_Works()
    {
        var t = new Triangle3(new Vec3(1, 2, 3), new Vec3(4, 5, 6), new Vec3(7, 8, 9));
        Assert.Equal(1, t[0].X, 10);
        Assert.Equal(4, t[1].X, 10);
        Assert.Equal(7, t[2].X, 10);
    }

    // --- Segment advanced ---

    [Fact]
    public void Segment_ZeroLength()
    {
        var s = new Segment(new Vec3(1, 1, 1), new Vec3(1, 1, 1));
        Assert.Equal(0, s.Length, 10);
    }

    [Fact]
    public void Segment_Direction()
    {
        var s = new Segment(new Vec3(0, 0, 0), new Vec3(3, 0, 0));
        var dir = s.End - s.Start;
        Assert.Equal(3, dir.X, 10);
        Assert.Equal(0, dir.Y, 10);
    }

    // --- Aabb advanced ---

    [Fact]
    public void Aabb_FromPoint_MinMaxEqual()
    {
        var box = Aabb.FromPoint(new Vec3(1, 2, 3));
        Assert.Equal(1, box.Min.X, 10);
        Assert.Equal(1, box.Max.X, 10);
    }

    [Fact]
    public void Aabb_ExpandToInclude_Grows()
    {
        var box = Aabb.FromPoint(new Vec3(0, 0, 0));
        box = box.ExpandToInclude(new Vec3(1, 1, 1));
        Assert.Equal(0, box.Min.X, 10);
        Assert.Equal(1, box.Max.X, 10);
    }

    [Fact]
    public void Aabb_Union_EnclosesBoth()
    {
        var a = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(2, 2, 2), new Vec3(3, 3, 3));
        var u = Aabb.Union(a, b);
        Assert.Equal(0, u.Min.X, 10);
        Assert.Equal(3, u.Max.X, 10);
    }

    [Fact]
    public void Aabb_Overlaps_Touching_True()
    {
        var a = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(1, 0, 0), new Vec3(2, 1, 1));
        Assert.True(a.Overlaps(b));
    }

    // --- Plane advanced ---

    [Fact]
    public void Plane_FromPoints_PointOnPlane_ZeroDistance()
    {
        var p = Plane.FromPoints(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.True(System.Math.Abs(p.SignedDistanceTo(new Vec3(0.5, 0.5, 0))) < 1e-10);
    }

    [Fact]
    public void Plane_FromPoints_PointAbove_PositiveDistance()
    {
        var p = Plane.FromPoints(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.True(p.SignedDistanceTo(new Vec3(0, 0, 1)) > 0);
    }

    [Fact]
    public void Plane_FromPoints_PointBelow_NegativeDistance()
    {
        var p = Plane.FromPoints(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.True(p.SignedDistanceTo(new Vec3(0, 0, -1)) < 0);
    }

    // --- Ray advanced ---

    [Fact]
    public void Ray_PointAt_Negative_BehindOrigin()
    {
        var ray = new Ray(new Vec3(0, 0, 0), new Vec3(1, 0, 0));
        var p = ray.PointAt(-1);
        Assert.Equal(-1, p.X, 10);
    }

    [Fact]
    public void Ray_PointAt_DiagonalDirection()
    {
        var ray = new Ray(new Vec3(0, 0, 0), new Vec3(1, 1, 0).Normalized);
        var p = ray.PointAt(System.Math.Sqrt(2));
        Assert.True(System.Math.Abs(p.X - 1) < 1e-10);
        Assert.True(System.Math.Abs(p.Y - 1) < 1e-10);
    }
}
