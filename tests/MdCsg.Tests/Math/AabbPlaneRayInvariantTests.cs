using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: AABB, Plane, Ray mathematical invariants</summary>
public class AabbPlaneRayInvariantTests
{
    [Fact]
    public void Aabb_Union_Contains_Both()
    {
        var a = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(2, 2, 2), new Vec3(3, 3, 3));
        var u = Aabb.Union(a, b);
        Assert.True(u.Contains(a.Min));
        Assert.True(u.Contains(a.Max));
        Assert.True(u.Contains(b.Min));
        Assert.True(u.Contains(b.Max));
    }

    [Fact]
    public void Aabb_Overlaps_Symmetric()
    {
        var a = new Aabb(new Vec3(0, 0, 0), new Vec3(2, 2, 2));
        var b = new Aabb(new Vec3(1, 1, 1), new Vec3(3, 3, 3));
        Assert.Equal(a.Overlaps(b), b.Overlaps(a));
    }

    [Fact]
    public void Aabb_NoOverlap_Disjoint()
    {
        var a = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(2, 2, 2), new Vec3(3, 3, 3));
        Assert.False(a.Overlaps(b));
    }

    [Fact]
    public void Aabb_Contains_Point_Inside()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        Assert.True(box.Contains(new Vec3(0.5, 0.5, 0.5)));
    }

    [Fact]
    public void Aabb_Contains_Point_Outside()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        Assert.False(box.Contains(new Vec3(2, 0, 0)));
    }

    [Fact]
    public void Aabb_ExpandToInclude_GrowsBounds()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        var expanded = box.ExpandToInclude(new Vec3(2, 2, 2));
        Assert.True(expanded.Contains(new Vec3(2, 2, 2)));
        Assert.True(expanded.Contains(new Vec3(0, 0, 0)));
    }

    [Fact]
    public void Aabb_Expand_SymmetricGrowth()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        var expanded = box.Expand(0.5);
        Assert.Equal(-0.5, expanded.Min.X, 10);
        Assert.Equal(1.5, expanded.Max.X, 10);
    }

    [Fact]
    public void Aabb_Size()
    {
        var box = new Aabb(new Vec3(1, 2, 3), new Vec3(4, 6, 9));
        var size = box.Size;
        Assert.Equal(3, size.X, 10);
        Assert.Equal(4, size.Y, 10);
        Assert.Equal(6, size.Z, 10);
    }

    [Fact]
    public void Aabb_Center()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(2, 4, 6));
        var center = box.Center;
        Assert.Equal(1, center.X, 10);
        Assert.Equal(2, center.Y, 10);
        Assert.Equal(3, center.Z, 10);
    }

    [Fact]
    public void Aabb_SurfaceArea()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 2, 3));
        // SA = 2*(1*2 + 2*3 + 1*3) = 2*(2+6+3) = 22
        Assert.Equal(22, box.SurfaceArea, 10);
    }

    [Fact]
    public void Aabb_FromTriangle()
    {
        var box = Aabb.FromTriangle(new Vec3(1, 2, 3), new Vec3(4, 5, 6), new Vec3(7, 8, 9));
        Assert.Equal(1, box.Min.X);
        Assert.Equal(7, box.Max.X);
    }

    [Fact]
    public void Aabb_Empty_OverlapsNothing()
    {
        var empty = Aabb.Empty;
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        Assert.False(empty.Overlaps(box));
    }

    [Fact]
    public void Plane_FromPoints_Normal()
    {
        var plane = Plane.FromPoints(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.True(System.Math.Abs(plane.Normal.Z) > 0);
        Assert.True(System.Math.Abs(plane.Normal.X) < 1e-10);
        Assert.True(System.Math.Abs(plane.Normal.Y) < 1e-10);
    }

    [Fact]
    public void Plane_SignedDistance_PositiveAbove()
    {
        var plane = Plane.FromPoints(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        double d = plane.SignedDistanceTo(new Vec3(0, 0, 1));
        Assert.True(d > 0);
    }

    [Fact]
    public void Plane_SignedDistance_NegativeBelow()
    {
        var plane = Plane.FromPoints(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        double d = plane.SignedDistanceTo(new Vec3(0, 0, -1));
        Assert.True(d < 0);
    }

    [Fact]
    public void Plane_SignedDistance_ZeroOnPlane()
    {
        var plane = Plane.FromPoints(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        double d = plane.SignedDistanceTo(new Vec3(0.5, 0.5, 0));
        Assert.True(System.Math.Abs(d) < 1e-10);
    }

    [Fact]
    public void Plane_Flipped_ReversesNormal()
    {
        var plane = Plane.FromPoints(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var flipped = plane.Flipped;
        Assert.True(Vec3.Dot(plane.Normal, flipped.Normal) < 0);
    }

    [Fact]
    public void Ray_PointAt_Origin()
    {
        var ray = new Ray(new Vec3(1, 2, 3), new Vec3(1, 0, 0));
        Assert.Equal(new Vec3(1, 2, 3), ray.PointAt(0));
    }

    [Fact]
    public void Ray_PointAt_Positive()
    {
        var ray = new Ray(new Vec3(0, 0, 0), new Vec3(1, 0, 0));
        Assert.Equal(new Vec3(5, 0, 0), ray.PointAt(5));
    }

    [Fact]
    public void Segment_Properties()
    {
        var seg = new Segment(new Vec3(0, 0, 0), new Vec3(3, 4, 0));
        Assert.Equal(5, seg.Length, 10);
        Assert.Equal(new Vec3(1.5, 2, 0), seg.Midpoint);
    }

    [Fact]
    public void Segment_PointAt()
    {
        var seg = new Segment(new Vec3(0, 0, 0), new Vec3(2, 0, 0));
        Assert.Equal(new Vec3(1, 0, 0), seg.PointAt(0.5));
    }
}
