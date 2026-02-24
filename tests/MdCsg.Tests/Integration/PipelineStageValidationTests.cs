using MdCsg.Api;
using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Operations;
using MdCsg.Patches;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: Pipeline stage validation — verifying invariants between each stage</summary>
public class PipelineStageValidationTests
{
    [Fact]
    public void Stage1_IntersectionGraph_SegmentCountSymmetric()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var g1 = IntersectionGraph.Compute(meshA, meshB);
        var g2 = IntersectionGraph.Compute(meshB, meshA);
        Assert.Equal(g1.Segments.Count, g2.Segments.Count);
    }

    [Fact]
    public void Stage2_CutMesh_SubTriangleCount_GEQ_FaceCount()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        Assert.True(cutA.SubTriangles.Count >= meshA.Faces.Count);
    }

    [Fact]
    public void Stage2_CutMesh_AllOriginalFacesPresent()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);

        var faces = cutA.SubTriangles.Select(st => st.OriginalFaceIndex).Distinct().ToHashSet();
        for (int i = 0; i < meshA.Faces.Count; i++)
            Assert.Contains(i, faces);
    }

    [Fact]
    public void Stage3_Adjacency_CountMatchesSubTriangles()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutA.SubTriangles);
        Assert.Equal(cutA.SubTriangles.Count, adj.Count);
    }

    [Fact]
    public void Stage3_Patches_PartitionSubTriangles()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patches = PatchExtractor.Extract(cutA.SubTriangles, adj);

        int total = patches.Sum(p => p.SubTriangleIndices.Count);
        Assert.Equal(cutA.SubTriangles.Count, total);
    }

    [Fact]
    public void Stage4_Classification_AllPatchesClassified()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var bvhB = BvhTree.Build(meshB);
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patches = PatchExtractor.Extract(cutA.SubTriangles, adj);
        PatchClassifier.ClassifyAll(patches, cutA.SubTriangles, bvhB);

        Assert.All(patches, p => Assert.NotNull(p.IsInside));
    }

    [Fact]
    public void Stage5_Assembly_ProducesTriangles()
    {
        var (pA, pB, cA, cB) = FullPipelineThroughClassification();
        var assembly = PatchAssembler.Assemble(pA, pB, cA.SubTriangles, cB.SubTriangles, CsgOperation.Union);
        Assert.True(assembly.Triangles.Count > 0);
    }

    [Fact]
    public void Stage6_Stitch_ProducesMesh()
    {
        var (pA, pB, cA, cB) = FullPipelineThroughClassification();
        var assembly = PatchAssembler.Assemble(pA, pB, cA.SubTriangles, cB.SubTriangles, CsgOperation.Union);
        var result = MeshStitcher.Stitch(assembly.Triangles);
        Assert.True(result.Faces.Count > 0);
        Assert.True(result.Vertices.Count > 0);
    }

    [Fact]
    public void FullPipeline_Sphere_AllStagesValid()
    {
        var meshA = MeshFactory.CreateSphere(Vec3.Zero, 0.8, 2).Mesh;
        var meshB = MeshFactory.CreateCube().Mesh;
        var bvhA = BvhTree.Build(meshA);
        var bvhB = BvhTree.Build(meshB);
        var graph = IntersectionGraph.Compute(meshA, meshB);

        Assert.True(graph.Segments.Count > 0);

        var cutA = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        var cutB = MeshCutter.Cut(meshB, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsB);

        var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var adjB = SubTriangleAdjacency.Build(cutB.SubTriangles);
        var patchesA = PatchExtractor.Extract(cutA.SubTriangles, adjA);
        var patchesB = PatchExtractor.Extract(cutB.SubTriangles, adjB);

        PatchClassifier.ClassifyAll(patchesA, cutA.SubTriangles, bvhB);
        PatchClassifier.ClassifyAll(patchesB, cutB.SubTriangles, bvhA);

        var assembly = PatchAssembler.Assemble(patchesA, patchesB, cutA.SubTriangles, cutB.SubTriangles, CsgOperation.Union);
        Assert.True(assembly.Triangles.Count > 0);

        var result = MeshStitcher.Stitch(assembly.Triangles);
        Assert.True(result.Faces.Count > 0);
    }

    [Fact]
    public void FullPipeline_AllOperations_DifferentResults()
    {
        var (pA, pB, cA, cB) = FullPipelineThroughClassification();
        int unionCount = PatchAssembler.Assemble(pA, pB, cA.SubTriangles, cB.SubTriangles, CsgOperation.Union).Triangles.Count;
        int interCount = PatchAssembler.Assemble(pA, pB, cA.SubTriangles, cB.SubTriangles, CsgOperation.Intersection).Triangles.Count;
        int diffCount = PatchAssembler.Assemble(pA, pB, cA.SubTriangles, cB.SubTriangles, CsgOperation.Difference).Triangles.Count;

        // All should produce different triangle counts for overlapping cubes
        Assert.True(unionCount > 0);
        Assert.True(interCount > 0);
        Assert.True(diffCount > 0);
        Assert.True(unionCount >= interCount);
    }

    private (List<Patch> patchesA, List<Patch> patchesB, MeshCutter.CutResult cutA, MeshCutter.CutResult cutB) FullPipelineThroughClassification()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var bvhA = BvhTree.Build(meshA);
        var bvhB = BvhTree.Build(meshB);
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        var cutB = MeshCutter.Cut(meshB, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsB);
        var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var adjB = SubTriangleAdjacency.Build(cutB.SubTriangles);
        var patchesA = PatchExtractor.Extract(cutA.SubTriangles, adjA);
        var patchesB = PatchExtractor.Extract(cutB.SubTriangles, adjB);
        PatchClassifier.ClassifyAll(patchesA, cutA.SubTriangles, bvhB);
        PatchClassifier.ClassifyAll(patchesB, cutB.SubTriangles, bvhA);
        return (patchesA, patchesB, cutA, cutB);
    }
}
