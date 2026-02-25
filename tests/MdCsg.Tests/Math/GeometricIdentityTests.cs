using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Mathematical identity tests — Cauchy-Schwarz, triangle inequality, vector properties</summary>
public class GeometricIdentityTests
{
    [Theory]
    [InlineData(1, 0, 0, 0, 1, 0)]
    [InlineData(1, 2, 3, 4, 5, 6)]
    [InlineData(-1, 3, -5, 7, -2, 4)]
    public void CauchySchwarz_DotLessOrEqual_ProductOfLengths(
        double ax, double ay, double az, double bx, double by, double bz)
    {
        var a = new Vec3(ax, ay, az);
        var b = new Vec3(bx, by, bz);
        double dot = System.Math.Abs(Vec3.Dot(a, b));
        double product = a.Length * b.Length;
        Assert.True(dot <= product + 1e-10, $"|a·b| = {dot} > |a|·|b| = {product}");
    }

    [Theory]
    [InlineData(1, 0, 0, 0, 1, 0)]
    [InlineData(1, 2, 3, 4, 5, 6)]
    [InlineData(-1, 3, -5, 7, -2, 4)]
    public void TriangleInequality_LengthOfSum_LeOrEq_SumOfLengths(
        double ax, double ay, double az, double bx, double by, double bz)
    {
        var a = new Vec3(ax, ay, az);
        var b = new Vec3(bx, by, bz);
        double sumLen = (a + b).Length;
        double lenSum = a.Length + b.Length;
        Assert.True(sumLen <= lenSum + 1e-10, $"|a+b| = {sumLen} > |a|+|b| = {lenSum}");
    }

    [Theory]
    [InlineData(1, 0, 0, 0, 1, 0)]
    [InlineData(1, 2, 3, 4, 5, 6)]
    public void CrossProduct_Perpendicular_ToDotProducts(
        double ax, double ay, double az, double bx, double by, double bz)
    {
        var a = new Vec3(ax, ay, az);
        var b = new Vec3(bx, by, bz);
        var cross = Vec3.Cross(a, b);
        // cross product is perpendicular to both inputs
        Assert.Equal(0, Vec3.Dot(cross, a), 1e-10);
        Assert.Equal(0, Vec3.Dot(cross, b), 1e-10);
    }

    [Fact]
    public void CrossProduct_Anticommutative()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var ab = Vec3.Cross(a, b);
        var ba = Vec3.Cross(b, a);
        Assert.Equal(-ab.X, ba.X, 1e-10);
        Assert.Equal(-ab.Y, ba.Y, 1e-10);
        Assert.Equal(-ab.Z, ba.Z, 1e-10);
    }

    [Fact]
    public void DotProduct_Commutative()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        Assert.Equal(Vec3.Dot(a, b), Vec3.Dot(b, a));
    }

    [Fact]
    public void DotProduct_Distributive()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var c = new Vec3(7, 8, 9);
        Assert.Equal(Vec3.Dot(a, b + c), Vec3.Dot(a, b) + Vec3.Dot(a, c), 1e-10);
    }

    [Fact]
    public void Triangle3_Area_NonNegative()
    {
        var t = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.True(t.Area >= 0);
    }

    [Fact]
    public void Triangle3_Area_UnitRightTriangle()
    {
        var t = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.Equal(0.5, t.Area, 1e-10);
    }

    [Fact]
    public void Triangle3_Centroid_InsideTriangle()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var t = new Triangle3(a, b, c);
        var centroid = t.Centroid;
        Assert.True(centroid.X > 0 && centroid.X < 1);
        Assert.True(centroid.Y > 0 && centroid.Y < 1);
    }

    [Fact]
    public void Plane_SignedDistance_OppositeForFlipped()
    {
        var p = Plane.FromPoints(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var point = new Vec3(0, 0, 5);
        double d1 = p.SignedDistanceTo(point);
        double d2 = p.Flipped.SignedDistanceTo(point);
        Assert.Equal(-d1, d2, 1e-10);
    }

    [Fact]
    public void Segment_Length_EqualsDistance()
    {
        var start = new Vec3(0, 0, 0);
        var end = new Vec3(3, 4, 0);
        var seg = new Segment(start, end);
        Assert.Equal(Vec3.Distance(start, end), seg.Length, 1e-10);
    }

    [Fact]
    public void Segment_Midpoint_EquidistantFromEndpoints()
    {
        var start = new Vec3(0, 0, 0);
        var end = new Vec3(4, 0, 0);
        var seg = new Segment(start, end);
        Assert.Equal(Vec3.Distance(seg.Midpoint, start), Vec3.Distance(seg.Midpoint, end), 1e-10);
    }

    [Fact]
    public void Aabb_Overlaps_Symmetric()
    {
        var a = new Aabb(new Vec3(0, 0, 0), new Vec3(2, 2, 2));
        var b = new Aabb(new Vec3(1, 1, 1), new Vec3(3, 3, 3));
        Assert.Equal(a.Overlaps(b), b.Overlaps(a));
    }

    [Fact]
    public void Aabb_Union_ContainsBoth()
    {
        var a = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(2, 2, 2), new Vec3(3, 3, 3));
        var u = Aabb.Union(a, b);
        Assert.True(u.Contains(new Vec3(0.5, 0.5, 0.5)));
        Assert.True(u.Contains(new Vec3(2.5, 2.5, 2.5)));
    }
}
