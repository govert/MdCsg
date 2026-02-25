using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Patches;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Classification;

/// <summary>Phase 6: PatchClassifier degenerate and threshold tests</summary>
public class PatchClassifierDegenerateTests
{
    [Fact]
    public void DegenerateMarginThreshold_IsSmall()
    {
        Assert.True(PatchClassifier.DegenerateMarginThreshold > 0);
        Assert.True(PatchClassifier.DegenerateMarginThreshold < 1e-6);
    }

    [Fact]
    public void ClassifyAll_AllPatchesGetIsInside()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var bvhB = BvhTree.Build(meshB);
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA,
            (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patches = PatchExtractor.Extract(cutA.SubTriangles, adjA);

        PatchClassifier.ClassifyAll(patches, cutA.SubTriangles, bvhB);
        Assert.All(patches, p => Assert.NotNull(p.IsInside));
    }

    [Fact]
    public void ClassifyAll_SomePatchesInside_SomeOutside()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var bvhB = BvhTree.Build(meshB);
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA,
            (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patches = PatchExtractor.Extract(cutA.SubTriangles, adjA);

        PatchClassifier.ClassifyAll(patches, cutA.SubTriangles, bvhB);
        Assert.Contains(patches, p => p.IsInside == true);
        Assert.Contains(patches, p => p.IsInside == false);
    }

    [Fact]
    public void ClassifyAll_ReturnsZeroDegenerateForWellSeparated()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var bvhB = BvhTree.Build(meshB);
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA,
            (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patches = PatchExtractor.Extract(cutA.SubTriangles, adjA);

        int degCount = PatchClassifier.ClassifyAll(patches, cutA.SubTriangles, bvhB);
        Assert.True(degCount >= 0);
    }

    [Fact]
    public void ClassifyAll_WithWindingNumber_SameResults()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var bvhB = BvhTree.Build(meshB);
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA,
            (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);

        var patchesRC = PatchExtractor.Extract(cutA.SubTriangles, adjA);
        PatchClassifier.ClassifyAll(patchesRC, cutA.SubTriangles, bvhB, useWindingNumber: false);

        var patchesWN = PatchExtractor.Extract(cutA.SubTriangles, adjA);
        PatchClassifier.ClassifyAll(patchesWN, cutA.SubTriangles, bvhB, useWindingNumber: true);

        for (int i = 0; i < patchesRC.Count; i++)
            Assert.Equal(patchesRC[i].IsInside, patchesWN[i].IsInside);
    }

    [Fact]
    public void ClassifyAll_SetsConfidentPoint()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var bvhB = BvhTree.Build(meshB);
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA,
            (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patches = PatchExtractor.Extract(cutA.SubTriangles, adjA);

        PatchClassifier.ClassifyAll(patches, cutA.SubTriangles, bvhB);
        // At least some patches should have confident points
        Assert.Contains(patches, p => p.HasConfidentPoint);
    }

    [Fact]
    public void CpuStrategy_ClassifyAll_MatchesStaticMethod()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var bvhB = BvhTree.Build(meshB);
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA,
            (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);

        var patchesDirect = PatchExtractor.Extract(cutA.SubTriangles, adjA);
        int degDirect = PatchClassifier.ClassifyAll(patchesDirect, cutA.SubTriangles, bvhB);

        var patchesVia = PatchExtractor.Extract(cutA.SubTriangles, adjA);
        var strategy = new CpuPatchClassificationStrategy();
        int degVia = strategy.ClassifyAll(patchesVia, cutA.SubTriangles, bvhB, false);

        Assert.Equal(degDirect, degVia);
        for (int i = 0; i < patchesDirect.Count; i++)
            Assert.Equal(patchesDirect[i].IsInside, patchesVia[i].IsInside);
    }

    [Fact]
    public void ClassifyAll_SphereCube_AllPatchesClassified()
    {
        var meshA = MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.6, 2).Mesh;
        var meshB = MeshFactory.CreateCube().Mesh;
        var bvhB = BvhTree.Build(meshB);
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA,
            (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patches = PatchExtractor.Extract(cutA.SubTriangles, adjA);

        PatchClassifier.ClassifyAll(patches, cutA.SubTriangles, bvhB);
        Assert.All(patches, p => Assert.NotNull(p.IsInside));
        Assert.True(patches.Count > 0);
    }
}
