using MdCsg.Classification;
using MdCsg.Math;

namespace MdCsg.Tests.Classification;

/// <summary>Phase 6: Voronoi region tests for PointTriangleDistanceSq — all 7 regions</summary>
public class PointTriangleDistanceVoronoiTests
{
    // Canonical triangle: A=(0,0,0), B=(1,0,0), C=(0,1,0)
    private static readonly Vec3 A = new(0, 0, 0);
    private static readonly Vec3 B = new(1, 0, 0);
    private static readonly Vec3 C = new(0, 1, 0);

    [Fact]
    public void ClosestToVertexA()
    {
        var p = new Vec3(-1, -1, 0);
        double distSq = ConfidentPoint.PointTriangleDistanceSq(p, A, B, C);
        Assert.Equal(Vec3.DistanceSquared(p, A), distSq, 1e-10);
    }

    [Fact]
    public void ClosestToVertexB()
    {
        var p = new Vec3(2, -1, 0);
        double distSq = ConfidentPoint.PointTriangleDistanceSq(p, A, B, C);
        Assert.Equal(Vec3.DistanceSquared(p, B), distSq, 1e-10);
    }

    [Fact]
    public void ClosestToVertexC()
    {
        var p = new Vec3(-1, 2, 0);
        double distSq = ConfidentPoint.PointTriangleDistanceSq(p, A, B, C);
        Assert.Equal(Vec3.DistanceSquared(p, C), distSq, 1e-10);
    }

    [Fact]
    public void ClosestToEdgeAB()
    {
        var p = new Vec3(0.5, -1, 0);
        double distSq = ConfidentPoint.PointTriangleDistanceSq(p, A, B, C);
        // Closest point on AB is (0.5, 0, 0)
        Assert.Equal(Vec3.DistanceSquared(p, new Vec3(0.5, 0, 0)), distSq, 1e-10);
    }

    [Fact]
    public void ClosestToEdgeAC()
    {
        var p = new Vec3(-1, 0.5, 0);
        double distSq = ConfidentPoint.PointTriangleDistanceSq(p, A, B, C);
        // Closest point on AC is (0, 0.5, 0)
        Assert.Equal(Vec3.DistanceSquared(p, new Vec3(0, 0.5, 0)), distSq, 1e-10);
    }

    [Fact]
    public void ClosestToEdgeBC()
    {
        var p = new Vec3(1, 1, 0);
        double distSq = ConfidentPoint.PointTriangleDistanceSq(p, A, B, C);
        // Closest to edge BC: midpoint of B=(1,0) C=(0,1) is (0.5,0.5)
        var closest = new Vec3(0.5, 0.5, 0);
        Assert.Equal(Vec3.DistanceSquared(p, closest), distSq, 1e-10);
    }

    [Fact]
    public void ClosestToInterior()
    {
        var p = new Vec3(0.2, 0.2, 5.0);
        double distSq = ConfidentPoint.PointTriangleDistanceSq(p, A, B, C);
        // Point directly above (0.2, 0.2) which is inside the triangle
        // Closest point is (0.2, 0.2, 0) so distance = 5.0
        Assert.Equal(25.0, distSq, 1e-10);
    }

    [Fact]
    public void PointOnVertex_ZeroDistance()
    {
        double distSq = ConfidentPoint.PointTriangleDistanceSq(A, A, B, C);
        Assert.Equal(0, distSq, 1e-15);
    }

    [Fact]
    public void PointOnEdge_ZeroDistance()
    {
        var p = new Vec3(0.5, 0, 0); // midpoint of AB
        double distSq = ConfidentPoint.PointTriangleDistanceSq(p, A, B, C);
        Assert.Equal(0, distSq, 1e-15);
    }

    [Fact]
    public void PointOnInterior_ZeroDistance()
    {
        var p = new Vec3(0.25, 0.25, 0);
        double distSq = ConfidentPoint.PointTriangleDistanceSq(p, A, B, C);
        Assert.Equal(0, distSq, 1e-15);
    }

    [Fact]
    public void SymmetricAboveBelowPlane()
    {
        var above = new Vec3(0.25, 0.25, 1.0);
        var below = new Vec3(0.25, 0.25, -1.0);
        double distAbove = ConfidentPoint.PointTriangleDistanceSq(above, A, B, C);
        double distBelow = ConfidentPoint.PointTriangleDistanceSq(below, A, B, C);
        Assert.Equal(distAbove, distBelow, 1e-10);
    }

    [Fact]
    public void AlwaysNonNegative()
    {
        var points = new[]
        {
            new Vec3(0, 0, 0), new Vec3(10, 10, 10), new Vec3(-5, 3, 7),
            new Vec3(0.5, 0.5, 0), new Vec3(1e-10, 1e-10, 1e-10)
        };
        foreach (var p in points)
        {
            double distSq = ConfidentPoint.PointTriangleDistanceSq(p, A, B, C);
            Assert.True(distSq >= 0, $"distSq was {distSq} for point {p}");
        }
    }

    [Fact]
    public void LargeTriangle_PointFarAway()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1000, 0, 0);
        var c = new Vec3(0, 1000, 0);
        var p = new Vec3(500, 500, 100);
        double distSq = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.True(distSq > 0);
    }

    [Fact]
    public void DegenerateTriangle_CollinearVertices()
    {
        // Degenerate: all three vertices collinear (line segment A-B)
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(2, 0, 0);
        var p = new Vec3(1, 1, 0);
        double distSq = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        // Should return distance to closest point on the line, which is (1, 0, 0)
        Assert.True(distSq >= 0);
        Assert.True(distSq <= 1.0 + 1e-10);
    }

    [Fact]
    public void Tilted_Triangle_Interior()
    {
        // Triangle in 3D at angle
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 1);
        var c = new Vec3(0, 1, 1);
        var centroid = (a + b + c) / 3.0;
        // Point exactly at centroid should have zero distance
        double distSq = ConfidentPoint.PointTriangleDistanceSq(centroid, a, b, c);
        Assert.True(distSq < 1e-20);
    }

    [Fact]
    public void TriangleInYZ_Plane()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(0, 1, 0);
        var c = new Vec3(0, 0, 1);
        var p = new Vec3(3, 0.25, 0.25);
        double distSq = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        // (0.25, 0.25) is inside the triangle so distance = 3.0
        Assert.Equal(9.0, distSq, 1e-10);
    }
}
