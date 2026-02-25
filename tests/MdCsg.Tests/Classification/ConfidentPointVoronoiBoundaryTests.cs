using MdCsg.Classification;
using MdCsg.Math;

namespace MdCsg.Tests.Classification;

/// <summary>Phase 6: ConfidentPoint.PointTriangleDistanceSq — all 7 Voronoi regions + boundaries</summary>
public class ConfidentPointVoronoiBoundaryTests
{
    private static readonly Vec3 A = new(0, 0, 0);
    private static readonly Vec3 B = new(1, 0, 0);
    private static readonly Vec3 C = new(0, 1, 0);

    [Fact]
    public void PointOnVertexA_DistanceZero()
    {
        double d = ConfidentPoint.PointTriangleDistanceSq(A, A, B, C);
        Assert.Equal(0, d, 10);
    }

    [Fact]
    public void PointOnVertexB_DistanceZero()
    {
        double d = ConfidentPoint.PointTriangleDistanceSq(B, A, B, C);
        Assert.Equal(0, d, 10);
    }

    [Fact]
    public void PointOnVertexC_DistanceZero()
    {
        double d = ConfidentPoint.PointTriangleDistanceSq(C, A, B, C);
        Assert.Equal(0, d, 10);
    }

    [Fact]
    public void PointOnEdgeAB_DistanceZero()
    {
        var p = new Vec3(0.5, 0, 0); // midpoint of AB
        double d = ConfidentPoint.PointTriangleDistanceSq(p, A, B, C);
        Assert.Equal(0, d, 10);
    }

    [Fact]
    public void PointOnEdgeAC_DistanceZero()
    {
        var p = new Vec3(0, 0.5, 0); // midpoint of AC
        double d = ConfidentPoint.PointTriangleDistanceSq(p, A, B, C);
        Assert.Equal(0, d, 10);
    }

    [Fact]
    public void PointOnEdgeBC_DistanceZero()
    {
        var p = new Vec3(0.5, 0.5, 0); // midpoint of BC
        double d = ConfidentPoint.PointTriangleDistanceSq(p, A, B, C);
        Assert.Equal(0, d, 10);
    }

    [Fact]
    public void PointInsideTriangle_DistanceZero()
    {
        var p = new Vec3(0.2, 0.2, 0); // inside triangle
        double d = ConfidentPoint.PointTriangleDistanceSq(p, A, B, C);
        Assert.Equal(0, d, 10);
    }

    [Fact]
    public void PointAboveTriangle_DistanceIsHeight()
    {
        var p = new Vec3(0.25, 0.25, 3);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, A, B, C);
        Assert.Equal(9, d, 5); // 3^2 = 9
    }

    [Fact]
    public void PointBelowTriangle_DistanceIsHeight()
    {
        var p = new Vec3(0.25, 0.25, -2);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, A, B, C);
        Assert.Equal(4, d, 5); // 2^2 = 4
    }

    [Fact]
    public void PointNearVertexA_Voronoi_ClosestToA()
    {
        // Point in vertex A's Voronoi region: beyond the edge normals at A
        var p = new Vec3(-1, -1, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, A, B, C);
        double expected = Vec3.DistanceSquared(p, A);
        Assert.Equal(expected, d, 8);
    }

    [Fact]
    public void PointNearVertexB_Voronoi_ClosestToB()
    {
        var p = new Vec3(2, -1, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, A, B, C);
        double expected = Vec3.DistanceSquared(p, B);
        Assert.Equal(expected, d, 8);
    }

    [Fact]
    public void PointNearVertexC_Voronoi_ClosestToC()
    {
        var p = new Vec3(-1, 2, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, A, B, C);
        double expected = Vec3.DistanceSquared(p, C);
        Assert.Equal(expected, d, 8);
    }

    [Fact]
    public void PointNearEdgeAB_Voronoi_ClosestToAB()
    {
        // Below edge AB (y < 0, x in (0,1))
        var p = new Vec3(0.5, -1, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, A, B, C);
        // Closest point on AB is (0.5, 0, 0)
        Assert.Equal(1.0, d, 8);
    }

    [Fact]
    public void PointNearEdgeAC_Voronoi_ClosestToAC()
    {
        // Left of edge AC (x < 0, y in (0,1))
        var p = new Vec3(-1, 0.5, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, A, B, C);
        // Closest point on AC is (0, 0.5, 0)
        Assert.Equal(1.0, d, 8);
    }

    [Fact]
    public void PointNearEdgeBC_Voronoi_ClosestToBC()
    {
        // Beyond edge BC (x+y > 1 region near midpoint of BC)
        var p = new Vec3(1, 1, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, A, B, C);
        // Closest point on BC: midpoint is (0.5, 0.5, 0). Distance from (1,1,0) to (0.5,0.5,0) = sqrt(0.5)
        Assert.Equal(0.5, d, 5);
    }

    [Fact]
    public void DistanceSq_IsNonNegative()
    {
        var random = new Random(42);
        for (int i = 0; i < 100; i++)
        {
            var p = new Vec3(random.NextDouble() * 10 - 5,
                random.NextDouble() * 10 - 5,
                random.NextDouble() * 10 - 5);
            double d = ConfidentPoint.PointTriangleDistanceSq(p, A, B, C);
            Assert.True(d >= 0, $"Distance squared should be non-negative: {d}");
        }
    }

    [Fact]
    public void DistanceSq_Symmetry_TriangleWinding()
    {
        // Distance shouldn't depend on triangle winding
        var p = new Vec3(0.3, 0.3, 1);
        double d1 = ConfidentPoint.PointTriangleDistanceSq(p, A, B, C);
        double d2 = ConfidentPoint.PointTriangleDistanceSq(p, A, C, B);
        Assert.Equal(d1, d2, 8);
    }

    [Fact]
    public void LargeTriangle_PointFarAway()
    {
        var la = new Vec3(0, 0, 0);
        var lb = new Vec3(1000, 0, 0);
        var lc = new Vec3(0, 1000, 0);
        var p = new Vec3(500, 500, 100);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, la, lb, lc);
        // Point is above the hypotenuse region, should be close to 10000 (height 100)
        Assert.True(d >= 10000 - 1, $"Expected ~10000, got {d}");
    }

    [Fact]
    public void DegenerateTriangle_Line_DistanceToLine()
    {
        // Degenerate triangle collinear along X axis
        var da = new Vec3(0, 0, 0);
        var db = new Vec3(1, 0, 0);
        var dc = new Vec3(2, 0, 0);
        var p = new Vec3(1, 1, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, da, db, dc);
        // Should return distance to nearest point on the line
        Assert.True(d >= 0);
        Assert.True(d <= 2.0); // can't be farther than distance to nearest vertex
    }
}
