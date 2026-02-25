using MdCsg.Classification;
using MdCsg.Math;

namespace MdCsg.Tests.Classification;

/// <summary>Phase 6: PointTriangleDistanceSq deep tests — all Voronoi regions</summary>
public class PointTriangleDistanceTests
{
    private static readonly Vec3 A = new(0, 0, 0);
    private static readonly Vec3 B = new(1, 0, 0);
    private static readonly Vec3 C = new(0, 1, 0);

    [Fact]
    public void PointOnTriangle_ZeroDistance()
    {
        // Centroid of the triangle
        var p = (A + B + C) / 3.0;
        double d = ConfidentPoint.PointTriangleDistanceSq(p, A, B, C);
        Assert.True(d < 1e-20, $"Expected ~0, got {d}");
    }

    [Fact]
    public void PointOnVertex_A_ZeroDistance()
    {
        double d = ConfidentPoint.PointTriangleDistanceSq(A, A, B, C);
        Assert.True(d < 1e-20, $"Expected ~0, got {d}");
    }

    [Fact]
    public void PointOnVertex_B_ZeroDistance()
    {
        double d = ConfidentPoint.PointTriangleDistanceSq(B, A, B, C);
        Assert.True(d < 1e-20, $"Expected ~0, got {d}");
    }

    [Fact]
    public void PointOnVertex_C_ZeroDistance()
    {
        double d = ConfidentPoint.PointTriangleDistanceSq(C, A, B, C);
        Assert.True(d < 1e-20, $"Expected ~0, got {d}");
    }

    [Fact]
    public void PointOnEdgeAB_ZeroDistance()
    {
        var p = (A + B) / 2.0;
        double d = ConfidentPoint.PointTriangleDistanceSq(p, A, B, C);
        Assert.True(d < 1e-20, $"Expected ~0, got {d}");
    }

    [Fact]
    public void PointOnEdgeAC_ZeroDistance()
    {
        var p = (A + C) / 2.0;
        double d = ConfidentPoint.PointTriangleDistanceSq(p, A, B, C);
        Assert.True(d < 1e-20, $"Expected ~0, got {d}");
    }

    [Fact]
    public void PointOnEdgeBC_ZeroDistance()
    {
        var p = (B + C) / 2.0;
        double d = ConfidentPoint.PointTriangleDistanceSq(p, A, B, C);
        Assert.True(d < 1e-20, $"Expected ~0, got {d}");
    }

    [Fact]
    public void PointAboveTriangle_DistanceIsHeight()
    {
        // Point directly above centroid at height 1
        var centroid = (A + B + C) / 3.0;
        var p = centroid + new Vec3(0, 0, 1);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, A, B, C);
        Assert.True(System.Math.Abs(d - 1.0) < 1e-10, $"Expected 1.0, got {d}");
    }

    [Fact]
    public void PointBelowTriangle_DistanceIsHeight()
    {
        var centroid = (A + B + C) / 3.0;
        var p = centroid + new Vec3(0, 0, -3);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, A, B, C);
        Assert.True(System.Math.Abs(d - 9.0) < 1e-10, $"Expected 9.0, got {d}");
    }

    [Fact]
    public void PointNearVertexA_VoronoiRegionA()
    {
        // Point in the Voronoi region of vertex A (beyond A away from triangle)
        var p = new Vec3(-1, -1, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, A, B, C);
        double expected = Vec3.DistanceSquared(p, A);
        Assert.True(System.Math.Abs(d - expected) < 1e-10, $"Expected {expected}, got {d}");
    }

    [Fact]
    public void PointNearVertexB_VoronoiRegionB()
    {
        var p = new Vec3(2, -1, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, A, B, C);
        double expected = Vec3.DistanceSquared(p, B);
        Assert.True(System.Math.Abs(d - expected) < 1e-10, $"Expected {expected}, got {d}");
    }

    [Fact]
    public void PointNearVertexC_VoronoiRegionC()
    {
        var p = new Vec3(-1, 2, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, A, B, C);
        double expected = Vec3.DistanceSquared(p, C);
        Assert.True(System.Math.Abs(d - expected) < 1e-10, $"Expected {expected}, got {d}");
    }

    [Fact]
    public void PointNearEdgeAB_VoronoiRegionAB()
    {
        // Point below edge AB (y negative, x in [0,1])
        var p = new Vec3(0.5, -1, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, A, B, C);
        // Closest point on AB is (0.5, 0, 0)
        double expected = Vec3.DistanceSquared(p, new Vec3(0.5, 0, 0));
        Assert.True(System.Math.Abs(d - expected) < 1e-10, $"Expected {expected}, got {d}");
    }

    [Fact]
    public void PointNearEdgeAC_VoronoiRegionAC()
    {
        // Point left of edge AC (x negative, y in [0,1])
        var p = new Vec3(-1, 0.5, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, A, B, C);
        // Closest point on AC is (0, 0.5, 0)
        double expected = Vec3.DistanceSquared(p, new Vec3(0, 0.5, 0));
        Assert.True(System.Math.Abs(d - expected) < 1e-10, $"Expected {expected}, got {d}");
    }

    [Fact]
    public void PointNearEdgeBC_VoronoiRegionBC()
    {
        // Point beyond hypotenuse BC
        var p = new Vec3(1, 1, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, A, B, C);
        // Closest point on BC: B + t*(C-B) where t projects p onto BC
        var bc = C - B;
        double t = Vec3.Dot(p - B, bc) / Vec3.Dot(bc, bc);
        t = System.Math.Max(0, System.Math.Min(1, t));
        var closest = B + bc * t;
        double expected = Vec3.DistanceSquared(p, closest);
        Assert.True(System.Math.Abs(d - expected) < 1e-10, $"Expected {expected}, got {d}");
    }

    [Fact]
    public void PointAboveVertex_DistanceCombinesXYAndZ()
    {
        var p = new Vec3(-1, -1, 1);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, A, B, C);
        double expected = Vec3.DistanceSquared(p, A);  // Closest to A
        Assert.True(System.Math.Abs(d - expected) < 1e-10);
    }

    [Fact]
    public void Distance_Symmetric_SameTriangle()
    {
        // Distance should be the same regardless of how we label the triangle vertices
        var p = new Vec3(0.3, 0.3, 1);
        double d1 = ConfidentPoint.PointTriangleDistanceSq(p, A, B, C);
        double d2 = ConfidentPoint.PointTriangleDistanceSq(p, B, C, A);
        double d3 = ConfidentPoint.PointTriangleDistanceSq(p, C, A, B);
        Assert.True(System.Math.Abs(d1 - d2) < 1e-10, $"d1={d1}, d2={d2}");
        Assert.True(System.Math.Abs(d1 - d3) < 1e-10, $"d1={d1}, d3={d3}");
    }

    [Fact]
    public void Distance_LargeTriangle_Works()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1000, 0, 0);
        var c = new Vec3(0, 1000, 0);
        var p = new Vec3(500, 500, 10);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        // Point is above the hypotenuse region. Distance to triangle plane is 10 (z component)
        // but closest point might be on the edge BC
        Assert.True(d > 0);
        Assert.True(d <= 100 + 1e-6); // at most 10^2 = 100 if above triangle
    }

    [Fact]
    public void Distance_DegenerateTriangle_CollinearVertices()
    {
        // Degenerate triangle (all points on X axis)
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(2, 0, 0);
        var p = new Vec3(1, 1, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        // Should still return a reasonable distance (closest to edge/vertex)
        Assert.True(d > 0);
        Assert.True(d <= 2); // closest to (1,0,0) → distance = 1
    }

    [Fact]
    public void Distance_AlwaysNonNegative()
    {
        var rng = new Random(42);
        for (int i = 0; i < 100; i++)
        {
            var a = new Vec3(rng.NextDouble(), rng.NextDouble(), rng.NextDouble());
            var b = new Vec3(rng.NextDouble(), rng.NextDouble(), rng.NextDouble());
            var c = new Vec3(rng.NextDouble(), rng.NextDouble(), rng.NextDouble());
            var p = new Vec3(rng.NextDouble() * 2 - 0.5, rng.NextDouble() * 2 - 0.5, rng.NextDouble() * 2 - 0.5);
            double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
            Assert.True(d >= 0, $"Negative distance: {d} for iter {i}");
            Assert.False(double.IsNaN(d), $"NaN distance for iter {i}");
        }
    }

    [Fact]
    public void Distance_PointOnPlaneOutsideTriangle()
    {
        // Point on the same plane as triangle but outside it
        var p = new Vec3(2, 2, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, A, B, C);
        // Closest is vertex C=(0,1,0) or B=(1,0,0)?
        // Actually closest might be the hypotenuse midpoint
        Assert.True(d > 0);
    }

    [Fact]
    public void Distance_IdenticalTriangleVertices_HandlesGracefully()
    {
        var p = new Vec3(1, 1, 1);
        var a = new Vec3(0, 0, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, a, a);
        // All vertices same → distance to that point
        double expected = Vec3.DistanceSquared(p, a);
        Assert.True(System.Math.Abs(d - expected) < 1e-10);
    }
}
