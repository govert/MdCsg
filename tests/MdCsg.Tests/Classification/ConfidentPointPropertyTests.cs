using MdCsg.Api;
using MdCsg.Classification;
using MdCsg.Cutting;
using MdCsg.Math;
using MdCsg.Patches;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Classification;

/// <summary>Phase 6: ConfidentPoint — FindConfidentPoint margin, PointTriangleDistanceSq Voronoi regions</summary>
public class ConfidentPointPropertyTests
{
    [Fact]
    public void PointTriangleDistanceSq_PointOnVertex_Zero()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        double dist = ConfidentPoint.PointTriangleDistanceSq(a, a, b, c);
        Assert.True(dist < 1e-20);
    }

    [Fact]
    public void PointTriangleDistanceSq_PointOnEdge_Zero()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var mid = (a + b) / 2.0;
        double dist = ConfidentPoint.PointTriangleDistanceSq(mid, a, b, c);
        Assert.True(dist < 1e-20);
    }

    [Fact]
    public void PointTriangleDistanceSq_PointOnInterior_Zero()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var centroid = (a + b + c) / 3.0;
        double dist = ConfidentPoint.PointTriangleDistanceSq(centroid, a, b, c);
        Assert.True(dist < 1e-20);
    }

    [Fact]
    public void PointTriangleDistanceSq_PointAbove_CorrectDistance()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var centroid = (a + b + c) / 3.0;
        var above = new Vec3(centroid.X, centroid.Y, 1.0);
        double dist = ConfidentPoint.PointTriangleDistanceSq(above, a, b, c);
        Assert.True(System.Math.Abs(dist - 1.0) < 1e-10);
    }

    [Fact]
    public void PointTriangleDistanceSq_PointFarFromVertex_ClosestToVertex()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(-1, -1, 0);
        double dist = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        double expectedSq = Vec3.DistanceSquared(p, a);
        Assert.True(System.Math.Abs(dist - expectedSq) < 1e-10);
    }

    [Fact]
    public void PointTriangleDistanceSq_PointFarFromVertexB_ClosestToB()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(2, -1, 0);
        double dist = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        double expectedSq = Vec3.DistanceSquared(p, b);
        Assert.True(System.Math.Abs(dist - expectedSq) < 1e-10);
    }

    [Fact]
    public void PointTriangleDistanceSq_PointFarFromVertexC_ClosestToC()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(-1, 2, 0);
        double dist = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        double expectedSq = Vec3.DistanceSquared(p, c);
        Assert.True(System.Math.Abs(dist - expectedSq) < 1e-10);
    }

    [Fact]
    public void PointTriangleDistanceSq_PointNearEdgeAB_ClosestOnEdge()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(0, 2, 0);
        var p = new Vec3(1, -1, 0);
        double dist = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.True(System.Math.Abs(dist - 1.0) < 1e-10);
    }

    [Fact]
    public void PointTriangleDistanceSq_PointNearEdgeBC_ClosestOnEdge()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(0, 2, 0);
        var p = new Vec3(2, 2, 0);
        double dist = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        double expectedSq = Vec3.DistanceSquared(p, new Vec3(1, 1, 0));
        Assert.True(System.Math.Abs(dist - expectedSq) < 1e-10);
    }

    [Fact]
    public void PointTriangleDistanceSq_Symmetric()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(0.5, 0.5, 1);
        double d1 = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        double d2 = ConfidentPoint.PointTriangleDistanceSq(p, b, c, a);
        double d3 = ConfidentPoint.PointTriangleDistanceSq(p, c, a, b);
        Assert.True(System.Math.Abs(d1 - d2) < 1e-10);
        Assert.True(System.Math.Abs(d1 - d3) < 1e-10);
    }

    [Fact]
    public void FindConfidentPoint_SingleSubTriangle_ReturnsCentroid()
    {
        var sphere = MeshFactory.CreateSphere(new Vec3(10, 0, 0), 1.0, 1);

        var st = new FaceCutter.SubTriangle(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false);
        var patch = new Patch(0);
        patch.SubTriangleIndices.Add(0);

        var (point, margin) = ConfidentPoint.FindConfidentPoint(patch, new[] { st }, sphere.Bvh);
        var expectedCentroid = (st.A + st.B + st.C) / 3.0;
        Assert.True(Vec3.DistanceSquared(point, expectedCentroid) < 1e-20);
        Assert.True(margin >= 0);
    }

    [Fact]
    public void FindConfidentPoint_MultipleSubTriangles_PicksLargestMargin()
    {
        var sphere = MeshFactory.CreateSphere(new Vec3(10, 10, 10), 1.0, 1);

        var near = new FaceCutter.SubTriangle(
            new Vec3(9, 10, 10), new Vec3(9.1, 10, 10), new Vec3(9, 10.1, 10), 0, false);
        var far = new FaceCutter.SubTriangle(
            new Vec3(0, 0, 0), new Vec3(0.1, 0, 0), new Vec3(0, 0.1, 0), 1, false);

        var patch = new Patch(0);
        patch.SubTriangleIndices.Add(0);
        patch.SubTriangleIndices.Add(1);

        var (point, margin) = ConfidentPoint.FindConfidentPoint(patch, new[] { near, far }, sphere.Bvh);
        var farCentroid = (far.A + far.B + far.C) / 3.0;
        Assert.True(Vec3.DistanceSquared(point, farCentroid) < 1e-10);
    }

    [Fact]
    public void FindConfidentPoint_Margin_IsPositive()
    {
        var sphere = MeshFactory.CreateSphere(new Vec3(5, 0, 0), 1.0, 1);

        var st = new FaceCutter.SubTriangle(
            new Vec3(-1, -1, -1), new Vec3(-0.9, -1, -1), new Vec3(-1, -0.9, -1), 0, false);
        var patch = new Patch(0);
        patch.SubTriangleIndices.Add(0);

        var (_, margin) = ConfidentPoint.FindConfidentPoint(patch, new[] { st }, sphere.Bvh);
        Assert.True(margin > 0);
    }

    [Fact]
    public void PointTriangleDistanceSq_AlwaysNonNegative()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var c = new Vec3(7, 2, 1);
        var points = new[] {
            Vec3.Zero, new Vec3(10, 10, 10), new Vec3(-5, -5, -5),
            new Vec3(1, 2, 3), new Vec3(2.5, 3.5, 4.5)
        };
        foreach (var p in points)
        {
            double dist = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
            Assert.True(dist >= 0, $"Distance squared should be non-negative for point {p}");
        }
    }
}
