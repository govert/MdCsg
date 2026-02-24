using MdCsg.Classification;
using MdCsg.Math;

namespace MdCsg.Tests.Classification;

/// <summary>Phase 6: ConfidentPoint.PointTriangleDistanceSq Voronoi region edge cases</summary>
public class ConfidentPointEdgeCaseTests
{
    [Fact]
    public void DistSq_PointAtVertexA()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(a, a, b, c);
        Assert.True(d < 1e-20);
    }

    [Fact]
    public void DistSq_PointAtVertexB()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(b, a, b, c);
        Assert.True(d < 1e-20);
    }

    [Fact]
    public void DistSq_PointAtVertexC()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(c, a, b, c);
        Assert.True(d < 1e-20);
    }

    [Fact]
    public void DistSq_PointOnEdgeAB_Midpoint()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(0, 2, 0);
        var p = new Vec3(1, 0, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.True(d < 1e-20);
    }

    [Fact]
    public void DistSq_PointOnEdgeAC_Midpoint()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(0, 2, 0);
        var p = new Vec3(0, 1, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.True(d < 1e-20);
    }

    [Fact]
    public void DistSq_PointOnEdgeBC_Midpoint()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(0, 2, 0);
        var p = new Vec3(1, 1, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.True(d < 1e-20);
    }

    [Fact]
    public void DistSq_PointInInterior()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(3, 0, 0);
        var c = new Vec3(0, 3, 0);
        var p = new Vec3(0.5, 0.5, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.True(d < 1e-20);
    }

    [Fact]
    public void DistSq_PointAboveInterior()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(3, 0, 0);
        var c = new Vec3(0, 3, 0);
        var p = new Vec3(0.5, 0.5, 5);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.Equal(25.0, d, 5);
    }

    [Fact]
    public void DistSq_PointBelowInterior()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(3, 0, 0);
        var c = new Vec3(0, 3, 0);
        var p = new Vec3(0.5, 0.5, -2);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.Equal(4.0, d, 5);
    }

    [Fact]
    public void DistSq_PointInVoronoiRegionA()
    {
        // Point outside triangle, closest to vertex A
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(-1, -1, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.Equal(2.0, d, 5); // dist to origin = sqrt(2), sq = 2
    }

    [Fact]
    public void DistSq_PointInVoronoiRegionB()
    {
        // Point outside, closest to vertex B
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(2, -1, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        double expected = Vec3.DistanceSquared(p, b);
        Assert.Equal(expected, d, 5);
    }

    [Fact]
    public void DistSq_PointInVoronoiRegionC()
    {
        // Point outside, closest to vertex C
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(-1, 2, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        double expected = Vec3.DistanceSquared(p, c);
        Assert.Equal(expected, d, 5);
    }

    [Fact]
    public void DistSq_PointInVoronoiEdgeAB()
    {
        // Point closest to edge AB (not vertices)
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(0, 2, 0);
        var p = new Vec3(1, -1, 0); // below midpoint of AB
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.Equal(1.0, d, 5);
    }

    [Fact]
    public void DistSq_PointInVoronoiEdgeAC()
    {
        // Point closest to edge AC (not vertices)
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(0, 2, 0);
        var p = new Vec3(-1, 1, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.Equal(1.0, d, 5);
    }

    [Fact]
    public void DistSq_PointInVoronoiEdgeBC()
    {
        // Point closest to edge BC
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(0, 2, 0);
        var p = new Vec3(2, 2, 0); // diagonal corner
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        // Closest point on BC: parametric point at (1,1), dist = sqrt(2), sq = 2
        Assert.Equal(2.0, d, 5);
    }

    [Fact]
    public void DistSq_3D_PointAboveEdge()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(0, 2, 0);
        var p = new Vec3(1, -1, 3); // above edge AB, 1 away in Y, 3 in Z
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.Equal(10.0, d, 5); // 1^2 + 3^2 = 10
    }

    [Fact]
    public void DistSq_LargeTriangle()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1000, 0, 0);
        var c = new Vec3(0, 1000, 0);
        var p = new Vec3(100, 100, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.True(d < 1e-10);
    }

    [Fact]
    public void DistSq_TinyTriangle()
    {
        double s = 1e-6;
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(s, 0, 0);
        var c = new Vec3(0, s, 0);
        var p = new Vec3(s / 3, s / 3, s); // above centroid
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.Equal(s * s, d, 15);
    }

    [Fact]
    public void DistSq_SymmetricPoint_SameDistance()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(1, 2, 0);
        // Two points symmetric about the triangle plane
        var p1 = new Vec3(1, 0.5, 1);
        var p2 = new Vec3(1, 0.5, -1);
        double d1 = ConfidentPoint.PointTriangleDistanceSq(p1, a, b, c);
        double d2 = ConfidentPoint.PointTriangleDistanceSq(p2, a, b, c);
        Assert.Equal(d1, d2, 10);
    }

    [Fact]
    public void DistSq_Centroid_IsZero()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var centroid = (a + b + c) / 3.0;
        double d = ConfidentPoint.PointTriangleDistanceSq(centroid, a, b, c);
        Assert.True(d < 1e-20);
    }

    [Fact]
    public void DistSq_FarPoint()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(1000, 1000, 1000);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        // Closest point is C=(0,1,0), dist^2 = 1000000 + 999^2 + 1000000
        Assert.True(d > 1e6);
    }
}
