using MdCsg.Classification;
using MdCsg.Math;

namespace MdCsg.Tests.Classification;

/// <summary>Phase 6: ConfidentPoint — PointTriangleDistanceSq for all Voronoi regions, symmetry, edge cases</summary>
public class ConfidentPointVoronoiRegionPropertyTests
{
    [Fact]
    public void PointTriangleDistSq_PointOnVertex_Zero()
    {
        var a = Vec3.Zero;
        var b = Vec3.UnitX;
        var c = Vec3.UnitY;
        double dist = ConfidentPoint.PointTriangleDistanceSq(a, a, b, c);
        Assert.True(dist < 1e-20, $"Distance to vertex A should be ~0, got {dist}");
    }

    [Fact]
    public void PointTriangleDistSq_PointOnVertexB_Zero()
    {
        var a = Vec3.Zero;
        var b = Vec3.UnitX;
        var c = Vec3.UnitY;
        double dist = ConfidentPoint.PointTriangleDistanceSq(b, a, b, c);
        Assert.True(dist < 1e-20);
    }

    [Fact]
    public void PointTriangleDistSq_PointOnVertexC_Zero()
    {
        var a = Vec3.Zero;
        var b = Vec3.UnitX;
        var c = Vec3.UnitY;
        double dist = ConfidentPoint.PointTriangleDistanceSq(c, a, b, c);
        Assert.True(dist < 1e-20);
    }

    [Fact]
    public void PointTriangleDistSq_PointOnCentroid_Zero()
    {
        var a = Vec3.Zero;
        var b = new Vec3(3, 0, 0);
        var c = new Vec3(0, 3, 0);
        var centroid = (a + b + c) / 3.0;
        double dist = ConfidentPoint.PointTriangleDistanceSq(centroid, a, b, c);
        Assert.True(dist < 1e-20, $"Distance to centroid should be ~0, got {dist}");
    }

    [Fact]
    public void PointTriangleDistSq_PointAboveTriangle_HeightSquared()
    {
        var a = Vec3.Zero;
        var b = new Vec3(3, 0, 0);
        var c = new Vec3(0, 3, 0);
        var centroid = (a + b + c) / 3.0;
        var above = centroid + new Vec3(0, 0, 5);
        double dist = ConfidentPoint.PointTriangleDistanceSq(above, a, b, c);
        Assert.True(System.Math.Abs(dist - 25.0) < 0.01, $"Should be 25, got {dist}");
    }

    [Fact]
    public void PointTriangleDistSq_PointOnEdge_Zero()
    {
        var a = Vec3.Zero;
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(0, 2, 0);
        var midAB = (a + b) * 0.5;
        double dist = ConfidentPoint.PointTriangleDistanceSq(midAB, a, b, c);
        Assert.True(dist < 1e-20, $"Point on edge should have distance ~0, got {dist}");
    }

    [Fact]
    public void PointTriangleDistSq_PointNearestToEdgeAB()
    {
        var a = Vec3.Zero;
        var b = new Vec3(4, 0, 0);
        var c = new Vec3(2, 4, 0);
        var point = new Vec3(2, -1, 0);
        double dist = ConfidentPoint.PointTriangleDistanceSq(point, a, b, c);
        Assert.True(System.Math.Abs(dist - 1.0) < 0.01, $"Expected ~1, got {dist}");
    }

    [Fact]
    public void PointTriangleDistSq_PointNearestToVertexA()
    {
        var a = Vec3.Zero;
        var b = new Vec3(4, 0, 0);
        var c = new Vec3(0, 4, 0);
        var point = new Vec3(-1, -1, 0);
        double dist = ConfidentPoint.PointTriangleDistanceSq(point, a, b, c);
        Assert.True(System.Math.Abs(dist - 2.0) < 0.01, $"Expected ~2, got {dist}");
    }

    [Fact]
    public void PointTriangleDistSq_SymmetricPoint_SameDistance()
    {
        var a = Vec3.Zero;
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(1, 2, 0);
        var p1 = new Vec3(1, 1, 3);
        var p2 = new Vec3(1, 1, -3);
        double d1 = ConfidentPoint.PointTriangleDistanceSq(p1, a, b, c);
        double d2 = ConfidentPoint.PointTriangleDistanceSq(p2, a, b, c);
        Assert.True(System.Math.Abs(d1 - d2) < 0.01,
            $"Symmetric points should have same distance: {d1} vs {d2}");
    }

    [Fact]
    public void PointTriangleDistSq_NonNegative()
    {
        var a = Vec3.Zero;
        var b = Vec3.UnitX;
        var c = Vec3.UnitY;
        var points = new[]
        {
            new Vec3(0.5, 0.5, 0.5), new Vec3(-1, -1, -1),
            new Vec3(10, 0, 0), new Vec3(0, 0, 100)
        };
        foreach (var p in points)
        {
            double dist = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
            Assert.True(dist >= 0, $"Distance squared should be non-negative for point {p}, got {dist}");
        }
    }

    [Fact]
    public void PointTriangleDistSq_FarPoint_LessThanOrEqualDistToAnyVertex()
    {
        var a = Vec3.Zero;
        var b = Vec3.UnitX;
        var c = Vec3.UnitY;
        var farPoint = new Vec3(1000, 1000, 1000);
        double dist = ConfidentPoint.PointTriangleDistanceSq(farPoint, a, b, c);
        double distToA = Vec3.DistanceSquared(farPoint, a);
        double distToB = Vec3.DistanceSquared(farPoint, b);
        double distToC = Vec3.DistanceSquared(farPoint, c);
        double minVertexDist = System.Math.Min(distToA, System.Math.Min(distToB, distToC));
        Assert.True(dist <= minVertexDist + 0.01,
            $"Closest distance {dist} should be <= min vertex distance {minVertexDist}");
    }

    [Fact]
    public void PointTriangleDistSq_PointOnEdgeBC_Zero()
    {
        var a = Vec3.Zero;
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(0, 2, 0);
        var midBC = (b + c) * 0.5;
        double dist = ConfidentPoint.PointTriangleDistanceSq(midBC, a, b, c);
        Assert.True(dist < 1e-20);
    }

    [Fact]
    public void PointTriangleDistSq_PointOnEdgeCA_Zero()
    {
        var a = Vec3.Zero;
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(0, 2, 0);
        var midCA = (c + a) * 0.5;
        double dist = ConfidentPoint.PointTriangleDistanceSq(midCA, a, b, c);
        Assert.True(dist < 1e-20);
    }

    [Fact]
    public void PointTriangleDistSq_NearestToEdgeBC()
    {
        var a = Vec3.Zero;
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(0, 2, 0);
        // Point beyond edge BC: nearest point is midpoint of BC at (1,1,0)
        var point = new Vec3(1.5, 1.5, 0);
        double dist = ConfidentPoint.PointTriangleDistanceSq(point, a, b, c);
        // Point is on the line BC extended, or just outside — should be small distance
        Assert.True(dist < 1.0, $"Point near edge BC should be close, got {dist}");
    }
}
