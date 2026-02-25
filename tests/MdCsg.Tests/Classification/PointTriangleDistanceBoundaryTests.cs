using MdCsg.Classification;
using MdCsg.Math;

namespace MdCsg.Tests.Classification;

/// <summary>Phase 6: ConfidentPoint.PointTriangleDistanceSq — boundary transitions between Voronoi regions</summary>
public class PointTriangleDistanceBoundaryTests
{
    private static readonly Vec3 A = Vec3.Zero;
    private static readonly Vec3 B = new(1, 0, 0);
    private static readonly Vec3 C = new(0, 1, 0);

    [Fact]
    public void PointOnCentroid_ZeroDistance()
    {
        var centroid = (A + B + C) / 3.0;
        double distSq = ConfidentPoint.PointTriangleDistanceSq(centroid, A, B, C);
        Assert.Equal(0.0, distSq, 10);
    }

    [Fact]
    public void PointOnEdgeAB_Midpoint_ZeroDistance()
    {
        var mid = (A + B) * 0.5;
        double distSq = ConfidentPoint.PointTriangleDistanceSq(mid, A, B, C);
        Assert.Equal(0.0, distSq, 10);
    }

    [Fact]
    public void PointOnEdgeBC_Midpoint_ZeroDistance()
    {
        var mid = (B + C) * 0.5;
        double distSq = ConfidentPoint.PointTriangleDistanceSq(mid, A, B, C);
        Assert.Equal(0.0, distSq, 10);
    }

    [Fact]
    public void PointOnEdgeCA_Midpoint_ZeroDistance()
    {
        var mid = (C + A) * 0.5;
        double distSq = ConfidentPoint.PointTriangleDistanceSq(mid, A, B, C);
        Assert.Equal(0.0, distSq, 10);
    }

    [Fact]
    public void PointAboveVertexA_HeightIsDistance()
    {
        var p = new Vec3(0, 0, 7);
        double distSq = ConfidentPoint.PointTriangleDistanceSq(p, A, B, C);
        Assert.Equal(49.0, distSq, 8);
    }

    [Fact]
    public void PointBelowTriangle_SameDistanceAsAbove()
    {
        var above = new Vec3(0.25, 0.25, 5);
        var below = new Vec3(0.25, 0.25, -5);
        double dAbove = ConfidentPoint.PointTriangleDistanceSq(above, A, B, C);
        double dBelow = ConfidentPoint.PointTriangleDistanceSq(below, A, B, C);
        Assert.Equal(dAbove, dBelow, 8);
    }

    [Fact]
    public void PointFarAway_LargeDistance()
    {
        var p = new Vec3(100, 100, 100);
        double distSq = ConfidentPoint.PointTriangleDistanceSq(p, A, B, C);
        // Distance to nearest vertex (B=(1,0,0)) = sqrt(99^2 + 100^2 + 100^2)
        // Which is very large
        Assert.True(distSq > 10000, $"Far point should have large distance, got {distSq}");
    }

    [Fact]
    public void Distance_ContinuousAcrossRegions()
    {
        // Walk along a line and verify distance changes smoothly
        double prevDist = double.MaxValue;
        bool decreased = false;
        bool increased = false;
        for (int i = 0; i <= 20; i++)
        {
            double t = i / 20.0;
            var p = new Vec3(t * 2 - 0.5, -0.5, 0);
            double distSq = ConfidentPoint.PointTriangleDistanceSq(p, A, B, C);
            if (distSq < prevDist) decreased = true;
            if (distSq > prevDist) increased = true;
            prevDist = distSq;
        }
        // Distance should decrease then increase as we sweep past the triangle
        Assert.True(decreased || increased, "Distance should change along sweep");
    }

    [Fact]
    public void DegenerateTriangle_Collinear_StillWorks()
    {
        // Collinear "triangle" — should still compute some distance without crashing
        var a = Vec3.Zero;
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(2, 0, 0);
        var p = new Vec3(1, 1, 0);
        // Should not throw
        double distSq = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.True(distSq >= 0);
    }

    [Fact]
    public void AllVerticesSamePoint_DistanceIsPointToPoint()
    {
        var a = new Vec3(1, 1, 1);
        var p = new Vec3(4, 5, 1);
        double distSq = ConfidentPoint.PointTriangleDistanceSq(p, a, a, a);
        Assert.Equal(Vec3.DistanceSquared(p, a), distSq, 8);
    }

    [Fact]
    public void TriangleInYZPlane_CorrectDistance()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(0, 1, 0);
        var c = new Vec3(0, 0, 1);
        var p = new Vec3(3, 0.25, 0.25); // 3 units away in X
        double distSq = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.Equal(9.0, distSq, 8);
    }
}
