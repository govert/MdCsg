using MdCsg.Api;
using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Operations;
using MdCsg.Patches;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: Full CSG pipeline step-by-step tests — verifying each stage independently</summary>
public class CsgPipelineTests
{
    [Fact]
    public void Step1_IntersectionGraph_ProducesSegments()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void Step2_MeshCutter_ProducesMoreSubTriangles()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        Assert.True(cutA.SubTriangles.Count > meshA.Faces.Count);
    }

    [Fact]
    public void Step3_PatchExtraction_MultiplePatches()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patches = PatchExtractor.Extract(cutA.SubTriangles, adj);
        Assert.True(patches.Count >= 2);
    }

    [Fact]
    public void Step4_Classification_AllPatchesClassified()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var bvhB = BvhTree.Build(meshB);
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patches = PatchExtractor.Extract(cutA.SubTriangles, adj);
        PatchClassifier.ClassifyAll(patches, cutA.SubTriangles, bvhB);
        Assert.All(patches, p => Assert.NotNull(p.IsInside));
    }

    [Fact]
    public void Step5_Assembly_ProducesTriangles()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var bvhA = BvhTree.Build(meshA);
        var bvhB = BvhTree.Build(meshB);
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        var cutB = MeshCutter.Cut(meshB, graph.FaceSegmentsB);
        var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var adjB = SubTriangleAdjacency.Build(cutB.SubTriangles);
        var patchesA = PatchExtractor.Extract(cutA.SubTriangles, adjA);
        var patchesB = PatchExtractor.Extract(cutB.SubTriangles, adjB);
        PatchClassifier.ClassifyAll(patchesA, cutA.SubTriangles, bvhB);
        PatchClassifier.ClassifyAll(patchesB, cutB.SubTriangles, bvhA);
        var assembly = PatchAssembler.Assemble(patchesA, patchesB, cutA.SubTriangles, cutB.SubTriangles, CsgOperation.Union);
        Assert.True(assembly.Triangles.Count > 0);
    }

    [Fact]
    public void Step6_Stitching_ProducesMesh()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.Mesh.Faces.Count > 0);
        Assert.True(result.Mesh.Vertices.Count > 0);
    }

    [Fact]
    public void FullPipeline_DifferentGridSizes_ProduceResults()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result1 = Csg.Union(a, b, new CsgOptions { GridSize = 1e-8 });
        var result2 = Csg.Union(a, b, new CsgOptions { GridSize = 1e-6 });
        Assert.True(result1.FaceCount > 0);
        Assert.True(result2.FaceCount > 0);
    }

    [Fact]
    public void FullPipeline_WithWindingNumber_ProducesResults()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Union(a, b, new CsgOptions { UseWindingNumber = true });
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void FullPipeline_Tetrahedron_Cube()
    {
        var a = new Solid(MeshFactory.CreateTetrahedron(new Vec3(0.2, 0.2, 0.2), 0.5).Mesh);
        var b = new Solid(MeshFactory.CreateCube().Mesh);
        var union = Csg.Union(a, b);
        var inter = Csg.Intersect(a, b);
        var diff = Csg.Difference(a, b);
        Assert.True(union.FaceCount > 0);
        Assert.True(inter.FaceCount > 0);
        Assert.True(diff.FaceCount > 0);
    }

    [Fact]
    public void FullPipeline_CsgResult_HasMetadata()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.PatchCountA > 0);
        Assert.True(result.PatchCountB > 0);
        Assert.True(result.IntersectionSegmentCount > 0);
    }

    [Fact]
    public void FullPipeline_CpuStrategy_ExplicitlySet()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var options = new CsgOptions { ClassificationStrategy = new CpuPatchClassificationStrategy() };
        var result = Csg.Union(a, b, options);
        Assert.True(result.FaceCount > 0);
    }
}
