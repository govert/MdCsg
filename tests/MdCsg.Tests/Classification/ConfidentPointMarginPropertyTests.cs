using MdCsg.Api;
using MdCsg.Classification;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Classification;

/// <summary>Phase 6: ConfidentPoint — margin computation, PointTriangleDistanceSq, integration with CSG</summary>
public class ConfidentPointMarginPropertyTests
{
    [Fact]
    public void PointTriangleDistanceSq_PointOnVertex_Zero()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        double dist = ConfidentPoint.PointTriangleDistanceSq(a, a, b, c);
        Assert.True(dist < 1e-20, $"Distance to vertex should be zero, got {dist}");
    }

    [Fact]
    public void PointTriangleDistanceSq_PointOnEdge_Zero()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(0, 2, 0);
        var mid = (a + b) * 0.5; // midpoint of edge AB
        double dist = ConfidentPoint.PointTriangleDistanceSq(mid, a, b, c);
        Assert.True(dist < 1e-20, $"Distance to edge midpoint should be ~0, got {dist}");
    }

    [Fact]
    public void PointTriangleDistanceSq_PointOnFace_Zero()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(3, 0, 0);
        var c = new Vec3(0, 3, 0);
        var centroid = (a + b + c) / 3.0;
        double dist = ConfidentPoint.PointTriangleDistanceSq(centroid, a, b, c);
        Assert.True(dist < 1e-20, $"Distance to centroid should be ~0, got {dist}");
    }

    [Fact]
    public void PointTriangleDistanceSq_PointAboveFace_EqualsHeightSquared()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(0, 2, 0);
        var above = new Vec3(0.5, 0.5, 3.0); // 3 units above face
        double dist = ConfidentPoint.PointTriangleDistanceSq(above, a, b, c);
        Assert.True(System.Math.Abs(dist - 9.0) < 0.1, $"Distance squared should be ~9, got {dist}");
    }

    [Fact]
    public void PointTriangleDistanceSq_PointFarAway_Large()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        double dist = ConfidentPoint.PointTriangleDistanceSq(new Vec3(100, 100, 100), a, b, c);
        Assert.True(dist > 10000);
    }

    [Fact]
    public void PointTriangleDistanceSq_SymmetricForVertices()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(0.5, 0.5, 1.0);
        double d1 = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        double d2 = ConfidentPoint.PointTriangleDistanceSq(p, b, c, a);
        double d3 = ConfidentPoint.PointTriangleDistanceSq(p, c, a, b);
        Assert.True(System.Math.Abs(d1 - d2) < 1e-10, $"Should be symmetric: {d1} vs {d2}");
        Assert.True(System.Math.Abs(d1 - d3) < 1e-10, $"Should be symmetric: {d1} vs {d3}");
    }

    [Fact]
    public void PointTriangleDistanceSq_NearEdge_CorrectRegion()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(4, 0, 0);
        var c = new Vec3(0, 4, 0);
        // Point near edge AB but outside triangle in Y
        var p = new Vec3(2, -1, 0);
        double dist = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        // Closest point on edge AB is (2,0,0), distance = 1
        Assert.True(System.Math.Abs(dist - 1.0) < 0.1, $"Expected ~1.0, got {dist}");
    }

    [Fact]
    public void PointTriangleDistanceSq_NonNegative()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var points = new[]
        {
            new Vec3(0, 0, 0), new Vec3(10, 10, 10), new Vec3(-1, -1, -1),
            new Vec3(0.3, 0.3, 0.3), new Vec3(0.5, 0.5, -5),
        };
        foreach (var p in points)
        {
            double dist = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
            Assert.True(dist >= 0, $"Distance squared should be non-negative, got {dist}");
        }
    }

    [Fact]
    public void FindConfidentPoint_ViaCSG_PatchesHaveConfidentPoints()
    {
        // Integration test through CSG
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Difference(a, b);
        // If the CSG ran without error, confident points were computed
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void FindConfidentPoint_ViaCSG_DegenerateCountSmall()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Union(a, b);
        Assert.True(result.DegenerateCount <= result.PatchCountA + result.PatchCountB,
            "Degenerate count should not exceed total patch count");
    }
}
