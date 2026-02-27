using MdCsg.Math;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 37: Circumcircle/sphere, NearestPointOnTriangle.</summary>
public class GeometryUtilIntegrationTests
{
    [Fact]
    public void NearestPointOnTriangle_InsideTriangle()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(10, 0, 0);
        var c = new Vec3(0, 10, 0);
        var p = new Vec3(2, 2, 0);
        var nearest = GeometryUtil.NearestPointOnTriangle(p, a, b, c);
        Assert.True((nearest - p).Length < 1e-10);
    }

    [Fact]
    public void NearestPointOnTriangle_AboveTriangle()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(10, 0, 0);
        var c = new Vec3(0, 10, 0);
        var p = new Vec3(2, 2, 5);
        var nearest = GeometryUtil.NearestPointOnTriangle(p, a, b, c);
        Assert.True(System.Math.Abs(nearest.Z) < 1e-10);
        Assert.True(System.Math.Abs(nearest.X - 2) < 1e-10);
        Assert.True(System.Math.Abs(nearest.Y - 2) < 1e-10);
    }

    [Fact]
    public void NearestPointOnTriangle_NearVertex()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(10, 0, 0);
        var c = new Vec3(0, 10, 0);
        var p = new Vec3(-1, -1, 0);
        var nearest = GeometryUtil.NearestPointOnTriangle(p, a, b, c);
        Assert.True((nearest - a).Length < 1e-10);
    }

    [Fact]
    public void NearestPointOnTriangle_NearEdge()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(10, 0, 0);
        var c = new Vec3(5, 10, 0);
        var p = new Vec3(5, -1, 0);
        var nearest = GeometryUtil.NearestPointOnTriangle(p, a, b, c);
        Assert.True(System.Math.Abs(nearest.Y) < 1e-10);
        Assert.True(nearest.X >= 0 && nearest.X <= 10);
    }

    [Fact]
    public void NearestPointOnTriangle_DistantPoint()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(100, 100, 100);
        var nearest = GeometryUtil.NearestPointOnTriangle(p, a, b, c);
        // Should be on the triangle
        Assert.True(nearest.Z < 1e-10);
    }

    [Fact]
    public void Circumcircle_EquilateralTriangle()
    {
        double r = 1.0;
        var a = new Vec3(r, 0, 0);
        var b = new Vec3(-r / 2, r * System.Math.Sqrt(3) / 2, 0);
        var c = new Vec3(-r / 2, -r * System.Math.Sqrt(3) / 2, 0);
        var (center, radius) = GeometryUtil.Circumcircle(a, b, c);
        Assert.True(center.Length < 1e-8);
        Assert.True(System.Math.Abs(radius - r) < 1e-8);
    }

    [Fact]
    public void Circumcircle_RightTriangle()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(4, 0, 0);
        var c = new Vec3(0, 3, 0);
        var (center, radius) = GeometryUtil.Circumcircle(a, b, c);
        // Hypotenuse = 5, circumradius = 2.5
        Assert.True(System.Math.Abs(radius - 2.5) < 1e-8);
    }

    [Fact]
    public void Circumcircle_AllVerticesOnCircle()
    {
        var a = new Vec3(1, 0, 0);
        var b = new Vec3(0, 1, 0);
        var c = new Vec3(-1, 0, 0);
        var (center, radius) = GeometryUtil.Circumcircle(a, b, c);
        Assert.True(System.Math.Abs((a - center).Length - radius) < 1e-8);
        Assert.True(System.Math.Abs((b - center).Length - radius) < 1e-8);
        Assert.True(System.Math.Abs((c - center).Length - radius) < 1e-8);
    }

    [Fact]
    public void Circumsphere_RegularTetrahedron()
    {
        // Regular tetrahedron vertices
        var a = new Vec3(1, 1, 1);
        var b = new Vec3(1, -1, -1);
        var c = new Vec3(-1, 1, -1);
        var d = new Vec3(-1, -1, 1);
        var (center, radius) = GeometryUtil.Circumsphere(a, b, c, d);
        Assert.True(System.Math.Abs((a - center).Length - radius) < 1e-8);
        Assert.True(System.Math.Abs((b - center).Length - radius) < 1e-8);
        Assert.True(System.Math.Abs((c - center).Length - radius) < 1e-8);
        Assert.True(System.Math.Abs((d - center).Length - radius) < 1e-8);
    }

    [Fact]
    public void Circumsphere_AxisAlignedTetrahedron()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(1, 2, 0);
        var d = new Vec3(1, 1, 2);
        var (center, radius) = GeometryUtil.Circumsphere(a, b, c, d);
        Assert.True(System.Math.Abs((a - center).Length - radius) < 1e-8);
        Assert.True(System.Math.Abs((d - center).Length - radius) < 1e-8);
    }

    [Fact]
    public void Circumsphere_CenterAtOrigin()
    {
        var a = new Vec3(1, 0, 0);
        var b = new Vec3(-1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0, 0, 1);
        var (center, radius) = GeometryUtil.Circumsphere(a, b, c, d);
        Assert.True(System.Math.Abs(radius - 1.0) < 1e-8);
    }

    [Fact]
    public void NearestPointOnTriangle_OnEdgeMidpoint()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(1, 2, 0);
        var p = new Vec3(1, -1, 0);
        var nearest = GeometryUtil.NearestPointOnTriangle(p, a, b, c);
        Assert.True(System.Math.Abs(nearest.Y) < 1e-10);
        Assert.True(nearest.X >= 0 && nearest.X <= 2);
    }

    [Fact]
    public void NearestPointOnTriangle_3DTriangle()
    {
        var a = new Vec3(0, 0, 1);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(0, 0, 0);
        var nearest = GeometryUtil.NearestPointOnTriangle(p, a, b, c);
        double dist = (nearest - p).Length;
        Assert.True(dist > 0);
        Assert.True(dist < 2);
    }

    [Fact]
    public void Circumcircle_IsoscelesTriangle()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(4, 0, 0);
        var c = new Vec3(2, 3, 0);
        var (center, radius) = GeometryUtil.Circumcircle(a, b, c);
        Assert.True(System.Math.Abs((a - center).Length - radius) < 1e-8);
        Assert.True(System.Math.Abs((c - center).Length - radius) < 1e-8);
    }

    [Fact]
    public void Circumcircle_3DTriangle()
    {
        var a = new Vec3(1, 0, 0);
        var b = new Vec3(0, 1, 0);
        var c = new Vec3(0, 0, 1);
        var (center, radius) = GeometryUtil.Circumcircle(a, b, c);
        Assert.True(System.Math.Abs((a - center).Length - radius) < 1e-8);
        Assert.True(System.Math.Abs((b - center).Length - radius) < 1e-8);
        Assert.True(System.Math.Abs((c - center).Length - radius) < 1e-8);
    }

    [Fact]
    public void NearestPointOnTriangle_AtVertex()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var nearest = GeometryUtil.NearestPointOnTriangle(a, a, b, c);
        Assert.True((nearest - a).Length < 1e-10);
    }

    [Fact]
    public void NearestPointOnTriangle_SymmetricAboveCenter()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(6, 0, 0);
        var c = new Vec3(3, 6, 0);
        var center = (a + b + c) * (1.0 / 3.0);
        var p = center + new Vec3(0, 0, 10);
        var nearest = GeometryUtil.NearestPointOnTriangle(p, a, b, c);
        Assert.True((nearest - center).Length < 1e-8);
    }

    [Fact]
    public void Circumsphere_AllVerticesEquidistant()
    {
        var a = new Vec3(2, 0, 0);
        var b = new Vec3(0, 2, 0);
        var c = new Vec3(0, 0, 2);
        var d = new Vec3(2, 2, 2);
        var (center, radius) = GeometryUtil.Circumsphere(a, b, c, d);
        Assert.True(System.Math.Abs((a - center).Length - radius) < 1e-6);
        Assert.True(System.Math.Abs((b - center).Length - radius) < 1e-6);
        Assert.True(System.Math.Abs((c - center).Length - radius) < 1e-6);
        Assert.True(System.Math.Abs((d - center).Length - radius) < 1e-6);
    }
}
