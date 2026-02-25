using MdCsg.Api;
using MdCsg.Classification;
using MdCsg.Cutting;
using MdCsg.Math;
using MdCsg.Patches;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Classification;

/// <summary>Phase 6: ConfidentPoint - Max-margin centroid selection, PointTriangleDistanceSq Voronoi regions</summary>
public class ConfidentPointPropertyTests
{
    [Fact]
    public void PointTriDistSq_VertexA_Zero()
    {
        double d = ConfidentPoint.PointTriangleDistanceSq(Vec3.Zero, Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        Assert.True(d < 1e-20);
    }

    [Fact]
    public void PointTriDistSq_VertexB_Zero()
    {
        double d = ConfidentPoint.PointTriangleDistanceSq(Vec3.UnitX, Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        Assert.True(d < 1e-20);
    }

    [Fact]
    public void PointTriDistSq_VertexC_Zero()
    {
        double d = ConfidentPoint.PointTriangleDistanceSq(Vec3.UnitY, Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        Assert.True(d < 1e-20);
    }

    [Fact]
    public void PointTriDistSq_OnEdgeAB_Zero()
    {
        var mid = new Vec3(0.5, 0, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(mid, Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        Assert.True(d < 1e-20);
    }

    [Fact]
    public void PointTriDistSq_OnEdgeAC_Zero()
    {
        var mid = new Vec3(0, 0.5, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(mid, Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        Assert.True(d < 1e-20);
    }

    [Fact]
    public void PointTriDistSq_OnEdgeBC_Zero()
    {
        var mid = new Vec3(0.5, 0.5, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(mid, Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        Assert.True(d < 1e-20);
    }

    [Fact]
    public void PointTriDistSq_CentroidOnTriangle_Zero()
    {
        var centroid = new Vec3(1.0 / 3, 1.0 / 3, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(centroid, Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        Assert.True(d < 1e-20);
    }

    [Fact]
    public void PointTriDistSq_AboveCenter_IsHeight()
    {
        var point = new Vec3(1.0 / 3, 1.0 / 3, 1.0);
        double d = ConfidentPoint.PointTriangleDistanceSq(point, Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        Assert.True(System.Math.Abs(d - 1.0) < 1e-10);
    }

    [Fact]
    public void PointTriDistSq_BelowCenter_IsHeight()
    {
        var point = new Vec3(1.0 / 3, 1.0 / 3, -2.0);
        double d = ConfidentPoint.PointTriangleDistanceSq(point, Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        Assert.True(System.Math.Abs(d - 4.0) < 1e-10);
    }

    [Fact]
    public void PointTriDistSq_NearestVertexA_Region()
    {
        var point = new Vec3(-1, -1, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(point, Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        double expected = Vec3.DistanceSquared(point, Vec3.Zero);
        Assert.True(System.Math.Abs(d - expected) < 1e-10);
    }

    [Fact]
    public void PointTriDistSq_NearestVertexB_Region()
    {
        var point = new Vec3(2, -1, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(point, Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        double expected = Vec3.DistanceSquared(point, Vec3.UnitX);
        Assert.True(System.Math.Abs(d - expected) < 1e-10);
    }

    [Fact]
    public void PointTriDistSq_NearestVertexC_Region()
    {
        var point = new Vec3(-1, 2, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(point, Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        double expected = Vec3.DistanceSquared(point, Vec3.UnitY);
        Assert.True(System.Math.Abs(d - expected) < 1e-10);
    }

    [Fact]
    public void PointTriDistSq_NearestEdgeAB_Region()
    {
        var point = new Vec3(0.5, -1, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(point, Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        Assert.True(System.Math.Abs(d - 1.0) < 1e-10);
    }

    [Fact]
    public void PointTriDistSq_NearestEdgeBC_Region()
    {
        var point = new Vec3(1, 1, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(point, Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        Assert.True(System.Math.Abs(d - 0.5) < 1e-10);
    }

    [Fact]
    public void PointTriDistSq_IsNonNegative()
    {
        var rng = new Random(42);
        for (int i = 0; i < 100; i++)
        {
            var p = new Vec3(rng.NextDouble() * 10 - 5, rng.NextDouble() * 10 - 5, rng.NextDouble() * 10 - 5);
            double d = ConfidentPoint.PointTriangleDistanceSq(p, Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
            Assert.True(d >= 0);
        }
    }

    [Fact]
    public void PointTriDistSq_SymmetricInZ()
    {
        var p1 = new Vec3(0.3, 0.3, 1.0);
        var p2 = new Vec3(0.3, 0.3, -1.0);
        double d1 = ConfidentPoint.PointTriangleDistanceSq(p1, Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        double d2 = ConfidentPoint.PointTriangleDistanceSq(p2, Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        Assert.True(System.Math.Abs(d1 - d2) < 1e-10);
    }

    [Fact]
    public void PointTriDistSq_LargeTriangle()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1000, 0, 0);
        var c = new Vec3(0, 1000, 0);
        var p = new Vec3(500, 500, 3);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.True(d >= 9.0 - 1e-5);
    }

    [Fact]
    public void FindConfidentPoint_SingleSubTriangle_ReturnsCentroid()
    {
        var otherCube = MeshFactory.CreateCube(new Vec3(100, 0, 0), 2.0);
        var sub = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, false, 0);
        var subs = new[] { sub };
        var patch = new Patch(0);
        patch.SubTriangleIndices.Add(0);

        var (point, margin) = ConfidentPoint.FindConfidentPoint(patch, subs, otherCube.Bvh);

        var expectedCentroid = new Vec3(1.0 / 3, 1.0 / 3, 0);
        Assert.True(Vec3.Distance(point, expectedCentroid) < 1e-10);
        Assert.True(margin > 0);
    }

    [Fact]
    public void FindConfidentPoint_MultipleSubTriangles_PicksMaxMargin()
    {
        var otherCube = MeshFactory.CreateCube(new Vec3(10, 0, 0), 2.0);
        var sub0 = new FaceCutter.SubTriangle(new Vec3(9, 0, 0), new Vec3(10, 0, 0), new Vec3(9, 1, 0), 0, false, 0);
        var sub1 = new FaceCutter.SubTriangle(new Vec3(-100, 0, 0), new Vec3(-99, 0, 0), new Vec3(-100, 1, 0), 1, false, 0);
        var subs = new[] { sub0, sub1 };
        var patch = new Patch(0);
        patch.SubTriangleIndices.Add(0);
        patch.SubTriangleIndices.Add(1);

        var (point, margin) = ConfidentPoint.FindConfidentPoint(patch, subs, otherCube.Bvh);

        var centroid1 = (new Vec3(-100, 0, 0) + new Vec3(-99, 0, 0) + new Vec3(-100, 1, 0)) / 3.0;
        Assert.True(Vec3.Distance(point, centroid1) < 1e-10);
    }

    [Fact]
    public void FindConfidentPoint_MarginIsPositive()
    {
        var otherCube = MeshFactory.CreateCube(new Vec3(10, 0, 0), 2.0);
        var sub = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, false, 0);
        var subs = new[] { sub };
        var patch = new Patch(0);
        patch.SubTriangleIndices.Add(0);

        var (_, margin) = ConfidentPoint.FindConfidentPoint(patch, subs, otherCube.Bvh);
        Assert.True(margin > 0);
    }
}
