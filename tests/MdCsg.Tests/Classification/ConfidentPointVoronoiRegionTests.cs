using MdCsg.Classification;
using MdCsg.Math;

namespace MdCsg.Tests.Classification;

/// <summary>Phase 6: ConfidentPoint.PointTriangleDistanceSq — all 7 Voronoi regions, degenerate triangles, symmetry</summary>
public class ConfidentPointVoronoiRegionTests
{
    // The 7 Voronoi regions of a triangle:
    // Vertex A, Vertex B, Vertex C, Edge AB, Edge AC, Edge BC, Interior

    [Fact]
    public void VertexA_Region_ClosestToA()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        // Point behind vertex A (d1 <= 0 && d2 <= 0)
        var p = new Vec3(-1, -1, 0);
        double dist = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.Equal(Vec3.DistanceSquared(p, a), dist, 1e-12);
    }

    [Fact]
    public void VertexB_Region_ClosestToB()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        // Point beyond vertex B (d3 >= 0 && d4 <= d3)
        var p = new Vec3(2, -1, 0);
        double dist = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.Equal(Vec3.DistanceSquared(p, b), dist, 1e-12);
    }

    [Fact]
    public void VertexC_Region_ClosestToC()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        // Point beyond vertex C (d6 >= 0 && d5 <= d6)
        var p = new Vec3(-1, 2, 0);
        double dist = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.Equal(Vec3.DistanceSquared(p, c), dist, 1e-12);
    }

    [Fact]
    public void EdgeAB_Region_ClosestToEdge()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        // Point directly below midpoint of AB
        var p = new Vec3(0.5, -1, 0);
        double dist = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        // Closest point on AB is (0.5, 0, 0)
        Assert.Equal(Vec3.DistanceSquared(p, new Vec3(0.5, 0, 0)), dist, 1e-12);
    }

    [Fact]
    public void EdgeAC_Region_ClosestToEdge()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        // Point to the left of midpoint of AC
        var p = new Vec3(-1, 0.5, 0);
        double dist = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        // Closest point on AC is (0, 0.5, 0)
        Assert.Equal(Vec3.DistanceSquared(p, new Vec3(0, 0.5, 0)), dist, 1e-12);
    }

    [Fact]
    public void EdgeBC_Region_ClosestToEdge()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        // Point beyond the hypotenuse BC
        var p = new Vec3(1, 1, 0);
        double dist = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        // Closest point on BC is midpoint (0.5, 0.5, 0)
        Assert.Equal(Vec3.DistanceSquared(p, new Vec3(0.5, 0.5, 0)), dist, 1e-12);
    }

    [Fact]
    public void Interior_Region_ZeroDistance()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        // Point at centroid, on the plane
        var p = new Vec3(0.2, 0.2, 0);
        double dist = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.Equal(0, dist, 1e-12);
    }

    [Fact]
    public void Interior_AbovePlane_PerpendicularDistance()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        // Point directly above centroid
        var p = new Vec3(0.2, 0.2, 3.0);
        double dist = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.Equal(9.0, dist, 1e-10); // z^2 = 9
    }

    [Fact]
    public void PointOnVertex_ZeroDistance()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var c = new Vec3(7, 8, 0);
        Assert.Equal(0, ConfidentPoint.PointTriangleDistanceSq(a, a, b, c), 1e-14);
        Assert.Equal(0, ConfidentPoint.PointTriangleDistanceSq(b, a, b, c), 1e-14);
        Assert.Equal(0, ConfidentPoint.PointTriangleDistanceSq(c, a, b, c), 1e-14);
    }

    [Fact]
    public void PointOnEdgeMidpoint_ZeroDistance()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(0, 2, 0);
        var mid = (a + b) / 2;
        Assert.Equal(0, ConfidentPoint.PointTriangleDistanceSq(mid, a, b, c), 1e-14);
    }

    [Fact]
    public void AlwaysNonNegative()
    {
        var rng = new Random(42);
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        for (int i = 0; i < 100; i++)
        {
            var p = new Vec3(rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2, rng.NextDouble() * 4 - 2);
            double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
            Assert.True(d >= 0, $"Negative distance squared: {d}");
        }
    }

    [Fact]
    public void Symmetric_SamePointSameTriangle_SameResult()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(3, 0, 0);
        var c = new Vec3(0, 4, 0);
        var p = new Vec3(5, 5, 5);
        double d1 = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        double d2 = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.Equal(d1, d2, 1e-15);
    }

    [Fact]
    public void DegenerateTriangle_CollinearVertices()
    {
        // All vertices on a line
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(2, 0, 0);
        var p = new Vec3(1, 1, 0);
        double dist = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        // Should return distance to the nearest point on the line segment, which is (1,0,0)
        Assert.True(dist >= 0);
        Assert.True(dist <= 1.01); // Distance to (1,0,0) is 1
    }

    [Fact]
    public void DegenerateTriangle_TwoCoincidentVertices()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(0, 0, 0);
        var c = new Vec3(1, 0, 0);
        var p = new Vec3(0, 1, 0);
        double dist = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.True(dist >= 0);
    }

    [Fact]
    public void LargeTriangle_CorrectDistance()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1000, 0, 0);
        var c = new Vec3(0, 1000, 0);
        var p = new Vec3(100, 100, 10);
        double dist = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.Equal(100.0, dist, 1e-6); // 10^2 = 100
    }

    [Fact]
    public void NegativeCoordTriangle_CorrectDistance()
    {
        var a = new Vec3(-1, -1, -1);
        var b = new Vec3(1, -1, -1);
        var c = new Vec3(-1, 1, -1);
        var p = new Vec3(0, 0, 0);
        double dist = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.Equal(1.0, dist, 1e-10); // z-distance is 1
    }

    [Fact]
    public void PointFarAway_LargeDistance()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(1000, 1000, 1000);
        double dist = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.True(dist > 1e6);
    }

    [Fact]
    public void PointBelowPlane_SameAsAbove()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var above = new Vec3(0.2, 0.2, 5);
        var below = new Vec3(0.2, 0.2, -5);
        double da = ConfidentPoint.PointTriangleDistanceSq(above, a, b, c);
        double db = ConfidentPoint.PointTriangleDistanceSq(below, a, b, c);
        Assert.Equal(da, db, 1e-12);
    }

    [Fact]
    public void TriangleInArbitraryOrientation_CorrectDistance()
    {
        // Triangle in XZ plane
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 0, 1);
        var p = new Vec3(0.2, 7, 0.2);
        double dist = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.Equal(49.0, dist, 1e-10); // y-distance is 7, 7^2 = 49
    }
}
