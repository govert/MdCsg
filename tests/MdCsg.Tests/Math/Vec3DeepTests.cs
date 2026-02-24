using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Deep Vec3, Vec2, Aabb, Ray tests</summary>
public class Vec3DeepTests
{
    // --- Vec3 ---

    [Fact]
    public void Vec3_Zero()
    {
        Assert.Equal(0, Vec3.Zero.X);
        Assert.Equal(0, Vec3.Zero.Y);
        Assert.Equal(0, Vec3.Zero.Z);
    }

    [Fact]
    public void Vec3_Length_UnitVectors()
    {
        Assert.Equal(1, new Vec3(1, 0, 0).Length, 10);
        Assert.Equal(1, new Vec3(0, 1, 0).Length, 10);
        Assert.Equal(1, new Vec3(0, 0, 1).Length, 10);
    }

    [Fact]
    public void Vec3_Length_345()
    {
        Assert.Equal(5, new Vec3(3, 4, 0).Length, 10);
    }

    [Fact]
    public void Vec3_Normalize_Unit()
    {
        var n = new Vec3(3, 0, 0).Normalized;
        Assert.Equal(1, n.X, 10);
        Assert.Equal(0, n.Y, 10);
    }

    [Fact]
    public void Vec3_Cross_Orthogonal()
    {
        var c = Vec3.Cross(new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.Equal(0, c.X, 10);
        Assert.Equal(0, c.Y, 10);
        Assert.Equal(1, c.Z, 10);
    }

    [Fact]
    public void Vec3_Cross_Anticommutative()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var ab = Vec3.Cross(a, b);
        var ba = Vec3.Cross(b, a);
        Assert.Equal(-ab.X, ba.X, 10);
        Assert.Equal(-ab.Y, ba.Y, 10);
        Assert.Equal(-ab.Z, ba.Z, 10);
    }

    [Fact]
    public void Vec3_Dot_Orthogonal_Zero()
    {
        Assert.Equal(0, Vec3.Dot(new Vec3(1, 0, 0), new Vec3(0, 1, 0)), 10);
    }

    [Fact]
    public void Vec3_Dot_Parallel()
    {
        Assert.Equal(6, Vec3.Dot(new Vec3(1, 2, 3), new Vec3(1, 1, 1)), 10);
    }

    [Fact]
    public void Vec3_Add()
    {
        var r = new Vec3(1, 2, 3) + new Vec3(4, 5, 6);
        Assert.Equal(5, r.X, 10);
        Assert.Equal(7, r.Y, 10);
        Assert.Equal(9, r.Z, 10);
    }

    [Fact]
    public void Vec3_Subtract()
    {
        var r = new Vec3(4, 5, 6) - new Vec3(1, 2, 3);
        Assert.Equal(3, r.X, 10);
        Assert.Equal(3, r.Y, 10);
        Assert.Equal(3, r.Z, 10);
    }

    [Fact]
    public void Vec3_ScalarMultiply()
    {
        var r = new Vec3(1, 2, 3) * 2;
        Assert.Equal(2, r.X, 10);
        Assert.Equal(4, r.Y, 10);
        Assert.Equal(6, r.Z, 10);
    }

    [Fact]
    public void Vec3_ScalarDivide()
    {
        var r = new Vec3(4, 6, 8) / 2;
        Assert.Equal(2, r.X, 10);
        Assert.Equal(3, r.Y, 10);
        Assert.Equal(4, r.Z, 10);
    }

    [Fact]
    public void Vec3_Distance_Same_Zero()
    {
        var a = new Vec3(1, 2, 3);
        Assert.Equal(0, Vec3.DistanceSquared(a, a), 10);
    }

    [Fact]
    public void Vec3_Distance_UnitApart()
    {
        Assert.Equal(1, Vec3.DistanceSquared(new Vec3(0, 0, 0), new Vec3(1, 0, 0)), 10);
    }

    [Fact]
    public void Vec3_Negate()
    {
        var v = new Vec3(1, -2, 3);
        var n = -v;
        Assert.Equal(-1, n.X, 10);
        Assert.Equal(2, n.Y, 10);
        Assert.Equal(-3, n.Z, 10);
    }

    // --- Aabb ---

    [Fact]
    public void Aabb_Contains_Inside()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        Assert.True(box.Contains(new Vec3(0.5, 0.5, 0.5)));
    }

    [Fact]
    public void Aabb_Contains_Outside()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        Assert.False(box.Contains(new Vec3(2, 2, 2)));
    }

    [Fact]
    public void Aabb_Overlaps_True()
    {
        var a = new Aabb(new Vec3(0, 0, 0), new Vec3(2, 2, 2));
        var b = new Aabb(new Vec3(1, 1, 1), new Vec3(3, 3, 3));
        Assert.True(a.Overlaps(b));
    }

    [Fact]
    public void Aabb_Overlaps_False()
    {
        var a = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(2, 2, 2), new Vec3(3, 3, 3));
        Assert.False(a.Overlaps(b));
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

    // --- Ray ---

    [Fact]
    public void Ray_PointAt_Zero_IsOrigin()
    {
        var ray = new Ray(new Vec3(1, 2, 3), new Vec3(1, 0, 0));
        var p = ray.PointAt(0);
        Assert.Equal(1, p.X, 10);
        Assert.Equal(2, p.Y, 10);
    }

    [Fact]
    public void Ray_PointAt_One()
    {
        var ray = new Ray(new Vec3(0, 0, 0), new Vec3(1, 0, 0));
        var p = ray.PointAt(1);
        Assert.Equal(1, p.X, 10);
    }

    // --- Segment ---

    [Fact]
    public void Segment_Length()
    {
        var seg = new Segment(new Vec3(0, 0, 0), new Vec3(3, 4, 0));
        Assert.Equal(5, seg.Length, 10);
    }

    [Fact]
    public void Segment_Midpoint()
    {
        var seg = new Segment(new Vec3(0, 0, 0), new Vec3(2, 2, 2));
        var mid = seg.Midpoint;
        Assert.Equal(1, mid.X, 10);
        Assert.Equal(1, mid.Y, 10);
        Assert.Equal(1, mid.Z, 10);
    }

    // --- Triangle3 ---

    [Fact]
    public void Triangle3_Area_UnitRightTriangle()
    {
        var t = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.Equal(0.5, t.Area, 5);
    }

    [Fact]
    public void Triangle3_Centroid()
    {
        var t = new Triangle3(new Vec3(0, 0, 0), new Vec3(3, 0, 0), new Vec3(0, 3, 0));
        Assert.Equal(1, t.Centroid.X, 10);
        Assert.Equal(1, t.Centroid.Y, 10);
        Assert.Equal(0, t.Centroid.Z, 10);
    }

    [Fact]
    public void Triangle3_Normal_UnitLength()
    {
        var t = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.Equal(1, t.Normal.Length, 5);
    }

    [Fact]
    public void Triangle3_Normal_PointsZ()
    {
        var t = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.True(t.Normal.Z > 0.9);
    }

    // --- Plane ---

    [Fact]
    public void Plane_FromPoints_Normal()
    {
        var p = Plane.FromPoints(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.True(System.Math.Abs(p.Normal.Z) > 0.9);
    }

    [Fact]
    public void Plane_DistanceTo_OnPlane()
    {
        var p = Plane.FromPoints(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.True(System.Math.Abs(p.SignedDistanceTo(new Vec3(0.5, 0.5, 0))) < 1e-10);
    }

    [Fact]
    public void Plane_DistanceTo_Above()
    {
        var p = Plane.FromPoints(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.True(p.SignedDistanceTo(new Vec3(0, 0, 5)) > 0);
    }
}
