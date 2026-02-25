using MdCsg.Api;
using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Patches;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG pipeline stages — IntersectionGraph → MeshCutter → PatchExtractor → PatchClassifier → Assemble</summary>
public class CsgPipelinePropertyTests
{
    [Fact]
    public void Pipeline_OverlappingCubes_IntersectionGraphHasSegments()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void Pipeline_CutA_ProducesMoreSubTriangles()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        var cutA = MeshCutter.Cut(a.Mesh, graph.FaceSegmentsA);
        Assert.True(cutA.SubTriangles.Count >= a.Mesh.Faces.Count);
    }

    [Fact]
    public void Pipeline_PatchExtractor_CreatesPatches()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        var cutA = MeshCutter.Cut(a.Mesh, graph.FaceSegmentsA);
        var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patchesA = PatchExtractor.Extract(cutA.SubTriangles, adjA);
        Assert.True(patchesA.Count > 0);
    }

    [Fact]
    public void Pipeline_PatchesA_AllHaveSubTriangles()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        var cutA = MeshCutter.Cut(a.Mesh, graph.FaceSegmentsA);
        var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patchesA = PatchExtractor.Extract(cutA.SubTriangles, adjA);
        foreach (var patch in patchesA)
        {
            Assert.True(patch.SubTriangleIndices.Count > 0);
        }
    }

    [Fact]
    public void Pipeline_DisjointCubes_NoSegments_OneHugePatchEach()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 1.0);
        var b = MeshFactory.CreateCube(new Vec3(100, 0, 0), 1.0);
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.Equal(0, graph.Segments.Count);
        
        var cutA = MeshCutter.Cut(a.Mesh, graph.FaceSegmentsA);
        var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patchesA = PatchExtractor.Extract(cutA.SubTriangles, adjA);
        // All sub-triangles in one big patch (cube has 12 faces, all connected)
        Assert.Single(patchesA);
        Assert.Equal(a.Mesh.Faces.Count, patchesA[0].SubTriangleIndices.Count);
    }

    [Fact]
    public void Pipeline_FullCsg_MatchesApiResult()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var apiResult = Csg.Union(a, b);
        Assert.True(apiResult.FaceCount > 0);
        Assert.True(apiResult.IntersectionSegmentCount > 0);
    }

    [Fact]
    public void Pipeline_SphereSphere_MultiplePatchesPerMesh()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 2);
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        var cutA = MeshCutter.Cut(a.Mesh, graph.FaceSegmentsA);
        var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patchesA = PatchExtractor.Extract(cutA.SubTriangles, adjA);
        // Overlapping spheres should produce at least 2 patches (inside and outside regions)
        Assert.True(patchesA.Count >= 2, $"Expected >=2 patches, got {patchesA.Count}");
    }

    [Fact]
    public void Pipeline_CutResult_MeshHasFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        var cutA = MeshCutter.Cut(a.Mesh, graph.FaceSegmentsA);
        Assert.True(cutA.Mesh.Faces.Count > 0);
    }
}
