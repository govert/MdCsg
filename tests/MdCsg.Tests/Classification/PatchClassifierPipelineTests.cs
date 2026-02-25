using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Patches;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Classification;

/// <summary>Phase 6: PatchClassifier pipeline — confident point selection, margin thresholds, degenerate detection, CPU strategy</summary>
public class PatchClassifierPipelineTests
{
    [Fact]
    public void ClassifyAll_CubePair_NoDegenerates()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patches = PatchExtractor.Extract(cutA.SubTriangles, adj);
        var bvhB = BvhTree.Build(meshB);
        int degenerates = PatchClassifier.ClassifyAll(patches, cutA.SubTriangles, bvhB);
        Assert.Equal(0, degenerates);
    }

    [Fact]
    public void ClassifyAll_AllPatchesClassified()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patches = PatchExtractor.Extract(cutA.SubTriangles, adj);
        var bvhB = BvhTree.Build(meshB);
        PatchClassifier.ClassifyAll(patches, cutA.SubTriangles, bvhB);
        foreach (var p in patches)
        {
            Assert.NotNull(p.IsInside);
        }
    }

    [Fact]
    public void ClassifyAll_HasInsideAndOutsidePatches()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patches = PatchExtractor.Extract(cutA.SubTriangles, adj);
        var bvhB = BvhTree.Build(meshB);
        PatchClassifier.ClassifyAll(patches, cutA.SubTriangles, bvhB);
        bool anyInside = false, anyOutside = false;
        foreach (var p in patches)
        {
            if (p.IsInside == true) anyInside = true;
            if (p.IsInside == false) anyOutside = true;
        }
        Assert.True(anyInside, "Should have inside patches");
        Assert.True(anyOutside, "Should have outside patches");
    }

    [Fact]
    public void ClassifyAll_WindingNumber_SameAsRayCast()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutA.SubTriangles);

        var patchesRay = PatchExtractor.Extract(cutA.SubTriangles, adj);
        var patchesWind = PatchExtractor.Extract(cutA.SubTriangles, adj);
        var bvhB = BvhTree.Build(meshB);

        PatchClassifier.ClassifyAll(patchesRay, cutA.SubTriangles, bvhB, false);
        PatchClassifier.ClassifyAll(patchesWind, cutA.SubTriangles, bvhB, true);

        Assert.Equal(patchesRay.Count, patchesWind.Count);
        for (int i = 0; i < patchesRay.Count; i++)
        {
            Assert.Equal(patchesRay[i].IsInside, patchesWind[i].IsInside);
        }
    }

    [Fact]
    public void ClassifyAll_ConfidentPoint_HasPositiveMargin()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patches = PatchExtractor.Extract(cutA.SubTriangles, adj);
        var bvhB = BvhTree.Build(meshB);
        PatchClassifier.ClassifyAll(patches, cutA.SubTriangles, bvhB);
        foreach (var p in patches)
        {
            Assert.True(p.HasConfidentPoint);
        }
    }

    [Fact]
    public void DegenerateMarginThreshold_IsSmall()
    {
        Assert.True(PatchClassifier.DegenerateMarginThreshold > 0);
        Assert.True(PatchClassifier.DegenerateMarginThreshold < 1e-6);
    }

    [Fact]
    public void CpuStrategy_MatchesDirectCall()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutA.SubTriangles);

        var patches1 = PatchExtractor.Extract(cutA.SubTriangles, adj);
        var patches2 = PatchExtractor.Extract(cutA.SubTriangles, adj);
        var bvhB = BvhTree.Build(meshB);

        int deg1 = PatchClassifier.ClassifyAll(patches1, cutA.SubTriangles, bvhB);
        var strategy = new CpuPatchClassificationStrategy();
        int deg2 = strategy.ClassifyAll(patches2, cutA.SubTriangles, bvhB, false);

        Assert.Equal(deg1, deg2);
        for (int i = 0; i < patches1.Count; i++)
            Assert.Equal(patches1[i].IsInside, patches2[i].IsInside);
    }

    [Fact]
    public void ConfidentPoint_CentroidBased()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patches = PatchExtractor.Extract(cutA.SubTriangles, adj);
        var bvhB = BvhTree.Build(meshB);
        PatchClassifier.ClassifyAll(patches, cutA.SubTriangles, bvhB);
        foreach (var p in patches)
        {
            // Confident point should not be at origin (unless patch happens to be there)
            var cp = p.ConfidentPoint;
            Assert.False(double.IsNaN(cp.X));
            Assert.False(double.IsInfinity(cp.X));
        }
    }

    [Fact]
    public void ClassifyAll_SpherePair_SomeInsideSomeOutside()
    {
        var meshA = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh;
        var meshB = MeshFactory.CreateSphere(new Vec3(0.5, 0, 0), 1.0, 2).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patches = PatchExtractor.Extract(cutA.SubTriangles, adj);
        var bvhB = BvhTree.Build(meshB);
        PatchClassifier.ClassifyAll(patches, cutA.SubTriangles, bvhB);
        bool anyInside = false, anyOutside = false;
        foreach (var p in patches)
        {
            if (p.IsInside == true) anyInside = true;
            if (p.IsInside == false) anyOutside = true;
        }
        Assert.True(anyInside);
        Assert.True(anyOutside);
    }

    [Fact]
    public void ConfidentPoint_FindConfidentPoint_ReturnsPositiveMargin()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patches = PatchExtractor.Extract(cutA.SubTriangles, adj);
        var bvhB = BvhTree.Build(meshB);
        foreach (var p in patches)
        {
            var (point, margin) = ConfidentPoint.FindConfidentPoint(p, cutA.SubTriangles, bvhB);
            Assert.True(margin > 0, $"Patch {p.Id} has non-positive margin: {margin}");
            Assert.False(double.IsNaN(point.X));
        }
    }

    [Fact]
    public void NoCutMesh_SinglePatch_ClassifiesCorrectly()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var empty = new Dictionary<int, List<IntersectionSegment>>();
        var cutA = MeshCutter.Cut(meshA, empty);
        var adj = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patches = PatchExtractor.Extract(cutA.SubTriangles, adj);
        Assert.Equal(1, patches.Count);
        // Classify against a distant cube
        var meshB = MeshFactory.CreateCube(new Vec3(10, 10, 10)).Mesh;
        var bvhB = BvhTree.Build(meshB);
        PatchClassifier.ClassifyAll(patches, cutA.SubTriangles, bvhB);
        Assert.Equal(false, patches[0].IsInside); // A is outside B
    }
}
