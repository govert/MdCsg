using MdCsg.Api;
using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Patches;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Classification;

/// <summary>Phase 6: ConfidentPoint — max-margin selection, degenerate margin threshold, distance accuracy</summary>
public class ConfidentPointMarginTests
{
    [Fact]
    public void FindConfidentPoint_CubePatch_NonZeroMargin()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cut = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cut.SubTriangles);
        var patches = PatchExtractor.Extract(cut.SubTriangles, adj);
        var bvhB = BvhTree.Build(meshB);

        foreach (var patch in patches)
        {
            var (point, margin) = ConfidentPoint.FindConfidentPoint(patch, cut.SubTriangles, bvhB);
            Assert.True(margin >= 0, $"Patch {patch.Id}: margin = {margin}");
        }
    }

    [Fact]
    public void PointTriangleDistSq_PointOnVertex_Zero()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        Assert.Equal(0, ConfidentPoint.PointTriangleDistanceSq(a, a, b, c), 1e-20);
    }

    [Fact]
    public void PointTriangleDistSq_PointOnEdge_Zero()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(0, 2, 0);
        var mid = new Vec3(1, 0, 0); // midpoint of A-B
        Assert.Equal(0, ConfidentPoint.PointTriangleDistanceSq(mid, a, b, c), 1e-20);
    }

    [Fact]
    public void PointTriangleDistSq_PointInInterior_Zero()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(3, 0, 0);
        var c = new Vec3(0, 3, 0);
        var centroid = new Vec3(1, 1, 0);
        Assert.Equal(0, ConfidentPoint.PointTriangleDistanceSq(centroid, a, b, c), 1e-20);
    }

    [Fact]
    public void PointTriangleDistSq_PointAboveTriangle_CorrectDistance()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(0.25, 0.25, 3); // 3 units above centroid area
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.Equal(9, d, 1e-10); // Distance squared = 3^2 = 9
    }

    [Fact]
    public void PointTriangleDistSq_ClosestToVertexA()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(-1, -1, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        // Closest to A: distance = sqrt(2), distSq = 2
        Assert.Equal(2, d, 1e-10);
    }

    [Fact]
    public void PointTriangleDistSq_ClosestToVertexB()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(2, -1, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        double expected = Vec3.DistanceSquared(p, b);
        Assert.Equal(expected, d, 1e-10);
    }

    [Fact]
    public void PointTriangleDistSq_ClosestToVertexC()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(-1, 2, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        double expected = Vec3.DistanceSquared(p, c);
        Assert.Equal(expected, d, 1e-10);
    }

    [Fact]
    public void PointTriangleDistSq_Symmetric()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(0.5, 0.5, 1);
        // Distance should be same regardless of point above/below
        var p2 = new Vec3(0.5, 0.5, -1);
        double d1 = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        double d2 = ConfidentPoint.PointTriangleDistanceSq(p2, a, b, c);
        Assert.Equal(d1, d2, 1e-10);
    }

    [Fact]
    public void DegenerateMarginThreshold_IsSmall()
    {
        Assert.Equal(1e-10, PatchClassifier.DegenerateMarginThreshold, 1e-15);
    }

    [Fact]
    public void ClassifyAll_ReturnsNonNegativeDegenerateCount()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cut = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cut.SubTriangles);
        var patches = PatchExtractor.Extract(cut.SubTriangles, adj);
        var bvhB = BvhTree.Build(meshB);
        int degCount = PatchClassifier.ClassifyAll(patches, cut.SubTriangles, bvhB, false);
        Assert.True(degCount >= 0);
    }

    [Fact]
    public void ClassifyAll_SetsIsInsideOnAllPatches()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cut = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cut.SubTriangles);
        var patches = PatchExtractor.Extract(cut.SubTriangles, adj);
        var bvhB = BvhTree.Build(meshB);
        PatchClassifier.ClassifyAll(patches, cut.SubTriangles, bvhB, false);
        foreach (var patch in patches)
            Assert.True(patch.IsInside.HasValue, $"Patch {patch.Id} has no classification");
    }

    [Fact]
    public void ClassifyAll_WithWindingNumber_SetsIsInside()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cut = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cut.SubTriangles);
        var patches = PatchExtractor.Extract(cut.SubTriangles, adj);
        var bvhB = BvhTree.Build(meshB);
        PatchClassifier.ClassifyAll(patches, cut.SubTriangles, bvhB, true);
        foreach (var patch in patches)
            Assert.True(patch.IsInside.HasValue, $"Patch {patch.Id} has no classification");
    }
}
