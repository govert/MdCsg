using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Patches;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Classification;

/// <summary>Phase 6: PatchClassifier integration tests with real meshes</summary>
public class PatchClassifierIntegrationTests
{
    [Fact]
    public void ClassifyAll_CubeCube_AllPatchesClassified()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh;
        var bvhB = BvhTree.Build(meshB);

        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patches = PatchExtractor.Extract(cutA.SubTriangles, adjA);

        int degCount = PatchClassifier.ClassifyAll(patches, cutA.SubTriangles, bvhB);

        foreach (var patch in patches)
        {
            Assert.NotNull(patch.IsInside);
        }
    }

    [Fact]
    public void ClassifyAll_HasConfidentPoints()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh;
        var bvhB = BvhTree.Build(meshB);

        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patches = PatchExtractor.Extract(cutA.SubTriangles, adjA);

        PatchClassifier.ClassifyAll(patches, cutA.SubTriangles, bvhB);

        // Most patches should have confident points
        int withConfident = patches.Count(p => p.HasConfidentPoint);
        Assert.True(withConfident > 0, "Should have patches with confident points");
    }

    [Fact]
    public void ClassifyAll_HasClassifiedPatches()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh;
        var bvhB = BvhTree.Build(meshB);

        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patches = PatchExtractor.Extract(cutA.SubTriangles, adjA);

        PatchClassifier.ClassifyAll(patches, cutA.SubTriangles, bvhB);

        bool anyInside = patches.Any(p => p.IsInside == true);
        bool anyOutside = patches.Any(p => p.IsInside == false);
        // At least one of inside or outside should exist
        Assert.True(anyInside || anyOutside, "Should have classified patches");
        Assert.True(patches.Count >= 1);
    }

    [Fact]
    public void ClassifyAll_WindingNumber_SameResult()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh;
        var bvhB = BvhTree.Build(meshB);

        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);

        // RayCast classification
        var patchesRC = PatchExtractor.Extract(cutA.SubTriangles, adjA);
        PatchClassifier.ClassifyAll(patchesRC, cutA.SubTriangles, bvhB, useWindingNumber: false);

        // WindingNumber classification
        var patchesWN = PatchExtractor.Extract(cutA.SubTriangles, adjA);
        PatchClassifier.ClassifyAll(patchesWN, cutA.SubTriangles, bvhB, useWindingNumber: true);

        // Should agree
        for (int i = 0; i < patchesRC.Count; i++)
        {
            Assert.Equal(patchesRC[i].IsInside, patchesWN[i].IsInside);
        }
    }

    [Fact]
    public void ClassifyAll_DisjointCubes_AllOutside()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(10, 0, 0)).Mesh;
        var bvhB = BvhTree.Build(meshB);

        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patches = PatchExtractor.Extract(cutA.SubTriangles, adjA);

        PatchClassifier.ClassifyAll(patches, cutA.SubTriangles, bvhB);

        // All patches of A should be outside B
        Assert.All(patches, p => Assert.False(p.IsInside));
    }

    [Fact]
    public void ClassifyAll_ContainedCube_AllInsideOrOutside()
    {
        var meshA = MeshFactory.CreateCube(size: 0.3).Mesh; // small cube
        var meshB = MeshFactory.CreateCube(new Vec3(-1, -1, -1), size: 5).Mesh; // large cube
        var bvhB = BvhTree.Build(meshB);

        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patches = PatchExtractor.Extract(cutA.SubTriangles, adjA);

        PatchClassifier.ClassifyAll(patches, cutA.SubTriangles, bvhB);

        // Small cube entirely inside large cube
        Assert.All(patches, p => Assert.True(p.IsInside));
    }

    [Fact]
    public void DegenerateMarginThreshold_IsSmall()
    {
        Assert.True(PatchClassifier.DegenerateMarginThreshold > 0);
        Assert.True(PatchClassifier.DegenerateMarginThreshold < 1e-5);
    }

    [Fact]
    public void CpuPatchClassificationStrategy_Works()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh;
        var bvhB = BvhTree.Build(meshB);

        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patches = PatchExtractor.Extract(cutA.SubTriangles, adjA);

        var strategy = new CpuPatchClassificationStrategy();
        int deg = strategy.ClassifyAll(patches, cutA.SubTriangles, bvhB, false);

        Assert.True(patches.All(p => p.IsInside.HasValue));
    }
}
