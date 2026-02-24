using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Patches;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Classification;

/// <summary>Phase 6: ConfidentPoint deep integration tests — margin values, classification accuracy</summary>
public class ConfidentPointDeepTests
{
    [Fact]
    public void FindConfidentPoint_CubeCube_HasPositiveMargin()
    {
        var (patches, subTris, bvh) = SetupCubeCube();
        foreach (var patch in patches)
        {
            var cp = ConfidentPoint.FindConfidentPoint(patch, subTris, bvh);
            // Most patches should have a positive margin
            if (cp.Margin > 0)
            {
                Assert.False(double.IsNaN(cp.Point.X));
                Assert.False(double.IsNaN(cp.Point.Y));
                Assert.False(double.IsNaN(cp.Point.Z));
            }
        }
    }

    [Fact]
    public void FindConfidentPoint_AllPatchesClassified()
    {
        var (patches, subTris, bvh) = SetupCubeCube();
        int classified = 0;
        foreach (var patch in patches)
        {
            var cp = ConfidentPoint.FindConfidentPoint(patch, subTris, bvh);
            if (cp.Margin > PatchClassifier.DegenerateMarginThreshold)
                classified++;
        }
        Assert.True(classified > 0, "At least some patches should have confident points");
    }

    [Fact]
    public void PointTriangleDistanceSq_PointOnVertex_Zero()
    {
        double dist = ConfidentPoint.PointTriangleDistanceSq(
            new Vec3(0, 0, 0),
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.True(dist < 1e-20);
    }

    [Fact]
    public void PointTriangleDistanceSq_PointOnEdge_Zero()
    {
        double dist = ConfidentPoint.PointTriangleDistanceSq(
            new Vec3(0.5, 0, 0),
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.True(dist < 1e-20);
    }

    [Fact]
    public void PointTriangleDistanceSq_PointAbove_CorrectDistance()
    {
        double dist = ConfidentPoint.PointTriangleDistanceSq(
            new Vec3(0.25, 0.25, 1),
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.True(System.Math.Abs(dist - 1.0) < 1e-10, $"Expected ~1.0, got {dist}");
    }

    [Fact]
    public void PointTriangleDistanceSq_PointFarAway_LargeDistance()
    {
        double dist = ConfidentPoint.PointTriangleDistanceSq(
            new Vec3(100, 0, 0),
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.True(dist > 9000); // ~99^2
    }

    [Fact]
    public void PointTriangleDistanceSq_Symmetric_InTriVertices()
    {
        var p = new Vec3(0.5, 0.5, 1);
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0.5, 1, 0);

        double d1 = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        double d2 = ConfidentPoint.PointTriangleDistanceSq(p, b, c, a);
        double d3 = ConfidentPoint.PointTriangleDistanceSq(p, c, a, b);

        // Distance should be the same regardless of vertex ordering
        Assert.True(System.Math.Abs(d1 - d2) < 1e-10);
        Assert.True(System.Math.Abs(d1 - d3) < 1e-10);
    }

    [Fact]
    public void PointTriangleDistanceSq_PointOnCentroid_Zero()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var centroid = (a + b + c) / 3;
        double dist = ConfidentPoint.PointTriangleDistanceSq(centroid, a, b, c);
        Assert.True(dist < 1e-20);
    }

    [Fact]
    public void ClassifyAll_CubeCube_PatchesHaveConfidentPoints()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var bvhB = BvhTree.Build(meshB);
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patches = PatchExtractor.Extract(cutA.SubTriangles, adjA);

        PatchClassifier.ClassifyAll(patches, cutA.SubTriangles, bvhB);

        int withConfident = patches.Count(p => p.HasConfidentPoint);
        Assert.True(withConfident > 0);
    }

    [Fact]
    public void ClassifyAll_DisjointCubes_AllPatchesOutside()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(10, 0, 0)).Mesh;
        var bvhB = BvhTree.Build(meshB);
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patches = PatchExtractor.Extract(cutA.SubTriangles, adjA);

        PatchClassifier.ClassifyAll(patches, cutA.SubTriangles, bvhB);

        Assert.All(patches, p => Assert.False(p.IsInside));
    }

    private (List<Patch> patches, IReadOnlyList<FaceCutter.SubTriangle> subTris, BvhTree bvh) SetupCubeCube()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var bvhB = BvhTree.Build(meshB);
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patches = PatchExtractor.Extract(cutA.SubTriangles, adjA);
        return (patches, cutA.SubTriangles, bvhB);
    }
}
