using MdCsg.Classification;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Classification;

/// <summary>Phase 6: ConfidentPoint — PointTriangleDistanceSq Voronoi regions, margin maximization, BVH distance</summary>
public class ConfidentPointMarginPropertyTests
{
    [Fact]
    public void PointTriangleDistanceSq_PointOnVertex_Zero()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        Assert.Equal(0.0, ConfidentPoint.PointTriangleDistanceSq(a, a, b, c), 10);
    }

    [Fact]
    public void PointTriangleDistanceSq_PointOnVertexB_Zero()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        Assert.Equal(0.0, ConfidentPoint.PointTriangleDistanceSq(b, a, b, c), 10);
    }

    [Fact]
    public void PointTriangleDistanceSq_PointOnVertexC_Zero()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        Assert.Equal(0.0, ConfidentPoint.PointTriangleDistanceSq(c, a, b, c), 10);
    }

    [Fact]
    public void PointTriangleDistanceSq_PointOnEdgeAB_Zero()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(0, 2, 0);
        var midAB = new Vec3(1, 0, 0);
        Assert.True(ConfidentPoint.PointTriangleDistanceSq(midAB, a, b, c) < 1e-20);
    }

    [Fact]
    public void PointTriangleDistanceSq_PointOnEdgeBC_Zero()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(0, 2, 0);
        var midBC = new Vec3(1, 1, 0);
        Assert.True(ConfidentPoint.PointTriangleDistanceSq(midBC, a, b, c) < 1e-20);
    }

    [Fact]
    public void PointTriangleDistanceSq_PointAboveCentroid_EqualsHeightSquared()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(3, 0, 0);
        var c = new Vec3(0, 3, 0);
        var above = new Vec3(1, 1, 5);
        Assert.Equal(25.0, ConfidentPoint.PointTriangleDistanceSq(above, a, b, c), 8);
    }

    [Fact]
    public void PointTriangleDistanceSq_PointBelowCentroid_EqualsHeightSquared()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(3, 0, 0);
        var c = new Vec3(0, 3, 0);
        var below = new Vec3(1, 1, -3);
        Assert.Equal(9.0, ConfidentPoint.PointTriangleDistanceSq(below, a, b, c), 8);
    }

    [Fact]
    public void PointTriangleDistanceSq_PointNearestEdgeAB_Region()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(4, 0, 0);
        var c = new Vec3(2, 4, 0);
        var p = new Vec3(2, -2, 0);
        Assert.Equal(4.0, ConfidentPoint.PointTriangleDistanceSq(p, a, b, c), 8);
    }

    [Fact]
    public void PointTriangleDistanceSq_PointNearestVertexA_Region()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(0, 2, 0);
        var p = new Vec3(-1, -1, 0);
        Assert.Equal(2.0, ConfidentPoint.PointTriangleDistanceSq(p, a, b, c), 8);
    }

    [Fact]
    public void PointTriangleDistanceSq_Symmetry_SameDistForMirroredPoints()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(1, 2, 0);
        var above = new Vec3(1, 0.5, 3);
        var below = new Vec3(1, 0.5, -3);
        double dAbove = ConfidentPoint.PointTriangleDistanceSq(above, a, b, c);
        double dBelow = ConfidentPoint.PointTriangleDistanceSq(below, a, b, c);
        Assert.Equal(dAbove, dBelow, 10);
    }

    [Fact]
    public void PointTriangleDistanceSq_AlwaysNonNegative()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var points = new[]
        {
            new Vec3(100, 200, 300), new Vec3(-5, -5, -5),
            new Vec3(0.5, 0.5, 0), new Vec3(0, 0, 0)
        };
        foreach (var p in points)
        {
            Assert.True(ConfidentPoint.PointTriangleDistanceSq(p, a, b, c) >= 0);
        }
    }

    [Fact]
    public void FindConfidentPoint_SingleSubTriangle_ReturnsCentroid()
    {
        var sphere = MeshFactory.CreateSphere(new Vec3(10, 0, 0), 1.0, 1);
        var patch = new MdCsg.Patches.Patch(0);
        var subTri = new MdCsg.Cutting.FaceCutter.SubTriangle(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false);
        patch.SubTriangleIndices.Add(0);
        var subTriangles = new[] { subTri };
        var (point, margin) = ConfidentPoint.FindConfidentPoint(patch, subTriangles, sphere.Bvh);
        var expectedCentroid = new Vec3(1.0 / 3.0, 1.0 / 3.0, 0);
        Assert.True(Vec3.DistanceSquared(point, expectedCentroid) < 1e-20);
        Assert.True(margin > 0, "Margin should be positive for disjoint meshes");
    }

    [Fact]
    public void FindConfidentPoint_MultipleSubTriangles_SelectsMaxMargin()
    {
        var sphere = MeshFactory.CreateSphere(new Vec3(5, 0, 0), 1.0, 1);
        var patch = new MdCsg.Patches.Patch(0);
        var near = new MdCsg.Cutting.FaceCutter.SubTriangle(
            new Vec3(3, 0, 0), new Vec3(4, 0, 0), new Vec3(3, 1, 0), 0, false);
        var far = new MdCsg.Cutting.FaceCutter.SubTriangle(
            new Vec3(-10, 0, 0), new Vec3(-9, 0, 0), new Vec3(-10, 1, 0), 1, false);
        patch.SubTriangleIndices.Add(0);
        patch.SubTriangleIndices.Add(1);
        var subTriangles = new[] { near, far };
        var (point, margin) = ConfidentPoint.FindConfidentPoint(patch, subTriangles, sphere.Bvh);
        var farCentroid = new Vec3(-29.0 / 3.0, 1.0 / 3.0, 0);
        Assert.True(Vec3.DistanceSquared(point, farCentroid) < 1e-10,
            $"Expected far centroid, got ({point.X}, {point.Y}, {point.Z})");
    }

    [Fact]
    public void FindConfidentPoint_MarginIsPositive_ForDisjointMeshes()
    {
        var sphere = MeshFactory.CreateSphere(new Vec3(20, 0, 0), 1.0, 1);
        var patch = new MdCsg.Patches.Patch(0);
        var subTri = new MdCsg.Cutting.FaceCutter.SubTriangle(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false);
        patch.SubTriangleIndices.Add(0);
        var (_, margin) = ConfidentPoint.FindConfidentPoint(patch, new[] { subTri }, sphere.Bvh);
        Assert.True(margin > 10, $"Margin should be large for far disjoint mesh, got {margin}");
    }

    [Fact]
    public void PointTriangleDistanceSq_DegenerateTriangle_StillWorks()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(2, 0, 0);
        var p = new Vec3(1, 1, 0);
        double dist = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.True(dist >= 0);
        Assert.True(dist <= 2.0);
    }

    [Fact]
    public void PointTriangleDistanceSq_LargeTriangle_CorrectForInterior()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1000, 0, 0);
        var c = new Vec3(0, 1000, 0);
        var p = new Vec3(100, 100, 7);
        Assert.Equal(49.0, ConfidentPoint.PointTriangleDistanceSq(p, a, b, c), 6);
    }

    [Fact]
    public void PointTriangleDistanceSq_NearestEdgeBC_Region()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(4, 0, 0);
        var c = new Vec3(0, 4, 0);
        var p = new Vec3(3, 3, 0);
        double dist = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.Equal(2.0, dist, 8);
    }
}
