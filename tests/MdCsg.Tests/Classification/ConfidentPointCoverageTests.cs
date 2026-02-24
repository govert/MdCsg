using MdCsg.Classification;
using MdCsg.Math;

namespace MdCsg.Tests.Classification;

/// <summary>Code coverage: ConfidentPoint Voronoi distance edge cases</summary>
public class ConfidentPointCoverageTests
{
    [Fact]
    public void PointTriangleDistanceSq_PointAtVertexA()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(a, a, b, c);
        Assert.True(d < 1e-20, $"Distance to vertex A should be ~0, got {d}");
    }

    [Fact]
    public void PointTriangleDistanceSq_PointAtVertexB()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(b, a, b, c);
        Assert.True(d < 1e-20);
    }

    [Fact]
    public void PointTriangleDistanceSq_PointAtVertexC()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(c, a, b, c);
        Assert.True(d < 1e-20);
    }

    [Fact]
    public void PointTriangleDistanceSq_PointOnEdgeAB()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(0, 2, 0);
        var p = new Vec3(1, 0, 0); // midpoint of AB
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.True(d < 1e-20);
    }

    [Fact]
    public void PointTriangleDistanceSq_PointOnEdgeAC()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(0, 2, 0);
        var p = new Vec3(0, 1, 0); // midpoint of AC
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.True(d < 1e-20);
    }

    [Fact]
    public void PointTriangleDistanceSq_PointOnEdgeBC()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(0, 2, 0);
        var p = new Vec3(1, 1, 0); // midpoint of BC
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.True(d < 1e-20);
    }

    [Fact]
    public void PointTriangleDistanceSq_PointInInterior()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(3, 0, 0);
        var c = new Vec3(0, 3, 0);
        var p = new Vec3(1, 1, 0); // centroid region
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.True(d < 1e-20);
    }

    [Fact]
    public void PointTriangleDistanceSq_PointAboveCentroid()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(3, 0, 0);
        var c = new Vec3(0, 3, 0);
        var p = new Vec3(1, 1, 2);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.True(System.Math.Abs(d - 4) < 0.01, $"Expected 4, got {d}");
    }

    [Fact]
    public void PointTriangleDistanceSq_PointBeyondVertexA()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(-1, -1, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        // Closest to vertex A: distance^2 = 1 + 1 = 2
        Assert.True(System.Math.Abs(d - 2) < 0.01, $"Expected 2, got {d}");
    }

    [Fact]
    public void PointTriangleDistanceSq_PointBeyondVertexB()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(2, -1, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        // Closest to vertex B: distance^2 = 1 + 1 = 2
        Assert.True(System.Math.Abs(d - 2) < 0.01, $"Expected 2, got {d}");
    }

    [Fact]
    public void PointTriangleDistanceSq_PointBeyondVertexC()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(-1, 2, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        // Closest to vertex C: distance^2 = 1 + 1 = 2
        Assert.True(System.Math.Abs(d - 2) < 0.01, $"Expected 2, got {d}");
    }

    [Fact]
    public void PointTriangleDistanceSq_ProjectsToEdgeAB()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(0, 2, 0);
        var p = new Vec3(1, -1, 0); // below edge AB
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.True(System.Math.Abs(d - 1) < 0.01, $"Expected 1, got {d}");
    }

    [Fact]
    public void PointTriangleDistanceSq_ProjectsToEdgeBC()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(0, 2, 0);
        var p = new Vec3(1.5, 1.5, 0); // beyond edge BC
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        // Closest point on BC is (1, 1) → distance^2 = (0.5^2 + 0.5^2) = 0.5
        Assert.True(System.Math.Abs(d - 0.5) < 0.1, $"Expected ~0.5, got {d}");
    }

    [Fact]
    public void PointTriangleDistanceSq_ProjectsToEdgeAC()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(0, 2, 0);
        var p = new Vec3(-1, 1, 0); // left of edge AC
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.True(System.Math.Abs(d - 1) < 0.01, $"Expected 1, got {d}");
    }

    [Fact]
    public void PointTriangleDistanceSq_PointFarAbove()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(0.25, 0.25, 100);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.True(System.Math.Abs(d - 10000) < 1, $"Expected ~10000, got {d}");
    }

    [Fact]
    public void PointTriangleDistanceSq_DegenerateTriangle_ThinLine()
    {
        // Degenerate triangle (collinear points)
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(2, 0, 0);
        var p = new Vec3(1, 1, 0);
        // Should not throw, returns some distance
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.True(d >= 0);
    }

    [Fact]
    public void PointTriangleDistanceSq_LargeTriangle()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1000, 0, 0);
        var c = new Vec3(0, 1000, 0);
        var p = new Vec3(500, 500, 0); // on the hypotenuse
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.True(d < 1, $"Point on hypotenuse should be close, got {d}");
    }

    [Fact]
    public void PointTriangleDistanceSq_SmallTriangle()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(0.001, 0, 0);
        var c = new Vec3(0, 0.001, 0);
        var p = new Vec3(0.0005, 0.0005, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.True(d < 1e-5);
    }
}
