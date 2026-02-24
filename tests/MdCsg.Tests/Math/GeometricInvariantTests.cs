using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Geometric invariants for Vec3, Vec2, Triangle3, Plane</summary>
public class GeometricInvariantTests
{
    [Fact]
    public void Vec3_Cross_AntiCommutative()
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
    public void Vec3_Cross_OrthogonalToInputs()
    {
        var a = new Vec3(1, 0, 0);
        var b = new Vec3(0, 1, 0);
        var c = Vec3.Cross(a, b);
        Assert.True(System.Math.Abs(Vec3.Dot(c, a)) < 1e-10);
        Assert.True(System.Math.Abs(Vec3.Dot(c, b)) < 1e-10);
    }

    [Fact]
    public void Vec3_Cross_ParallelVectors_Zero()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(2, 4, 6);
        var c = Vec3.Cross(a, b);
        Assert.True(c.Length < 1e-10);
    }

    [Fact]
    public void Vec3_Dot_Commutative()
    {
        var a = new Vec3(1.5, 2.7, 3.3);
        var b = new Vec3(4.1, 5.9, 6.2);
        Assert.Equal(Vec3.Dot(a, b), Vec3.Dot(b, a), 10);
    }

    [Fact]
    public void Vec3_Dot_SelfEquals_LengthSquared()
    {
        var a = new Vec3(3, 4, 5);
        Assert.Equal(a.LengthSquared, Vec3.Dot(a, a), 10);
    }

    [Fact]
    public void Vec3_Normalized_HasUnitLength()
    {
        var a = new Vec3(3, 4, 5);
        Assert.Equal(1.0, a.Normalized.Length, 10);
    }

    [Fact]
    public void Vec3_Distance_Symmetric()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        Assert.Equal(Vec3.Distance(a, b), Vec3.Distance(b, a), 10);
    }

    [Fact]
    public void Vec3_Distance_TriangleInequality()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        double ab = Vec3.Distance(a, b);
        double bc = Vec3.Distance(b, c);
        double ac = Vec3.Distance(a, c);
        Assert.True(ac <= ab + bc + 1e-10);
        Assert.True(ab <= ac + bc + 1e-10);
        Assert.True(bc <= ab + ac + 1e-10);
    }

    [Fact]
    public void Vec3_Min_Max_ComponentWise()
    {
        var a = new Vec3(1, 5, 3);
        var b = new Vec3(4, 2, 6);
        var min = Vec3.Min(a, b);
        var max = Vec3.Max(a, b);
        Assert.Equal(1, min.X);
        Assert.Equal(2, min.Y);
        Assert.Equal(3, min.Z);
        Assert.Equal(4, max.X);
        Assert.Equal(5, max.Y);
        Assert.Equal(6, max.Z);
    }

    [Fact]
    public void Vec2_Cross_AntiCommutative()
    {
        var a = new Vec2(1, 2);
        var b = new Vec2(3, 4);
        Assert.Equal(-Vec2.Cross(a, b), Vec2.Cross(b, a), 10);
    }

    [Fact]
    public void Vec2_Dot_Commutative()
    {
        var a = new Vec2(1.5, 2.7);
        var b = new Vec2(3.3, 4.1);
        Assert.Equal(Vec2.Dot(a, b), Vec2.Dot(b, a), 10);
    }

    [Fact]
    public void Triangle3_Area_Nonnegative()
    {
        var t = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.True(t.Area >= 0);
    }

    [Fact]
    public void Triangle3_RightTriangle_AreaHalfBase()
    {
        var t = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(0, 3, 0));
        Assert.Equal(3.0, t.Area, 10); // 1/2 * 2 * 3
    }

    [Fact]
    public void Triangle3_Centroid_InsideTriangle()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(3, 0, 0);
        var c = new Vec3(0, 3, 0);
        var t = new Triangle3(a, b, c);
        var centroid = t.Centroid;
        Assert.Equal(1, centroid.X, 10);
        Assert.Equal(1, centroid.Y, 10);
        Assert.Equal(0, centroid.Z, 10);
    }

    [Fact]
    public void Triangle3_Normal_PerpendicularToEdges()
    {
        var t = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var n = t.Normal;
        var e1 = t.B - t.A;
        var e2 = t.C - t.A;
        Assert.True(System.Math.Abs(Vec3.Dot(n, e1)) < 1e-10);
        Assert.True(System.Math.Abs(Vec3.Dot(n, e2)) < 1e-10);
    }

    [Fact]
    public void Triangle3_UnitNormal_HasUnitLength()
    {
        var t = new Triangle3(new Vec3(0, 0, 0), new Vec3(5, 0, 0), new Vec3(0, 7, 0));
        Assert.Equal(1.0, t.UnitNormal.Length, 10);
    }

    [Fact]
    public void Triangle3_DegenerateTriangle_ZeroArea()
    {
        var t = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(2, 0, 0));
        Assert.True(t.Area < 1e-10);
    }

    [Fact]
    public void Triangle3_Bounds_ContainsAllVertices()
    {
        var t = new Triangle3(new Vec3(1, 2, 3), new Vec3(4, 5, 6), new Vec3(7, 8, 9));
        var b = t.Bounds;
        Assert.True(b.Contains(t.A));
        Assert.True(b.Contains(t.B));
        Assert.True(b.Contains(t.C));
    }

    [Fact]
    public void Plane_FromPoints_NormalLength()
    {
        var p = Plane.FromPoints(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.Equal(1.0, p.Normal.Length, 10);
    }

    [Fact]
    public void Plane_FromPoints_AllPointsOnPlane()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 3);
        var c = new Vec3(7, 2, 3);
        var p = Plane.FromPoints(a, b, c);
        Assert.True(System.Math.Abs(p.SignedDistanceTo(a)) < 1e-10);
        Assert.True(System.Math.Abs(p.SignedDistanceTo(b)) < 1e-10);
        Assert.True(System.Math.Abs(p.SignedDistanceTo(c)) < 1e-10);
    }

    [Fact]
    public void Plane_Flipped_NegatesDistance()
    {
        var p = Plane.FromPoints(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var point = new Vec3(0, 0, 5);
        double d1 = p.SignedDistanceTo(point);
        double d2 = p.Flipped.SignedDistanceTo(point);
        Assert.Equal(-d1, d2, 10);
    }

    [Fact]
    public void Plane_SignedDistance_LinearScaling()
    {
        var p = Plane.FromPoints(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        double d1 = p.SignedDistanceTo(new Vec3(0, 0, 1));
        double d2 = p.SignedDistanceTo(new Vec3(0, 0, 3));
        Assert.Equal(d1 * 3, d2, 10);
    }

    [Fact]
    public void Segment_Length_Nonnegative()
    {
        var s = new Segment(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        Assert.True(s.Length >= 0);
    }

    [Fact]
    public void Segment_ZeroLength_SameEndpoints()
    {
        var p = new Vec3(1, 2, 3);
        var s = new Segment(p, p);
        Assert.True(s.Length < 1e-10);
    }

    [Fact]
    public void Segment_Midpoint_EquidistantFromEndpoints()
    {
        var s = new Segment(new Vec3(0, 0, 0), new Vec3(4, 0, 0));
        Assert.Equal(Vec3.Distance(s.Start, s.Midpoint), Vec3.Distance(s.End, s.Midpoint), 10);
    }

    [Fact]
    public void Segment_PointAt_Boundaries()
    {
        var s = new Segment(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        Assert.Equal(s.Start, s.PointAt(0));
        Assert.Equal(s.End, s.PointAt(1));
    }

    [Fact]
    public void Ray_PointAt_MovesAlongDirection()
    {
        var r = new Ray(new Vec3(0, 0, 0), new Vec3(0, 0, 1));
        var p = r.PointAt(10);
        Assert.Equal(0, p.X, 10);
        Assert.Equal(0, p.Y, 10);
        Assert.Equal(10, p.Z, 10);
    }

    [Fact]
    public void Ray_PointAt_Negative()
    {
        var r = new Ray(new Vec3(0, 0, 0), new Vec3(1, 0, 0));
        var p = r.PointAt(-5);
        Assert.Equal(-5, p.X, 10);
    }
}
