using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Patches;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Patches;

/// <summary>Batch 20: PatchExtractor + Patch tests (20 tests)</summary>
public class PatchExtractorPatchTests
{
    [Fact]
    public void Patch_Id_IsSetByConstructor()
    {
        var p = new Patch(7);
        Assert.Equal(7, p.Id);
    }

    [Fact]
    public void Patch_SubTriangleIndices_InitiallyEmpty()
    {
        var p = new Patch(0);
        Assert.Empty(p.SubTriangleIndices);
    }

    [Fact]
    public void Patch_IsInside_DefaultNull()
    {
        var p = new Patch(0);
        Assert.Null(p.IsInside);
    }

    [Fact]
    public void Patch_HasConfidentPoint_DefaultFalse()
    {
        var p = new Patch(0);
        Assert.False(p.HasConfidentPoint);
    }

    [Fact]
    public void Patch_CoplanarNormalsAgree_DefaultNull()
    {
        var p = new Patch(0);
        Assert.Null(p.CoplanarNormalsAgree);
    }

    [Fact]
    public void Patch_Properties_CanBeSet()
    {
        var p = new Patch(0);
        p.IsInside = true;
        p.HasConfidentPoint = true;
        p.ConfidentPoint = Vec3.UnitX;
        p.SourceMesh = 1;
        p.CoplanarNormalsAgree = false;
        Assert.True(p.IsInside);
        Assert.True(p.HasConfidentPoint);
        Assert.Equal(Vec3.UnitX, p.ConfidentPoint);
        Assert.Equal(1, p.SourceMesh);
        Assert.False(p.CoplanarNormalsAgree);
    }

    [Fact]
    public void Extract_NoCuts_SinglePatch()
    {
        var cube = MeshFactory.CreateCube();
        var emptySegs = new Dictionary<int, List<IntersectionSegment>>();
        var cutResult = MeshCutter.Cut(cube.Mesh, emptySegs);
        var adj = SubTriangleAdjacency.Build(cutResult.SubTriangles);
        var patches = PatchExtractor.Extract(cutResult.SubTriangles, adj);
        // With no intersection edges, all sub-triangles should be in one patch
        Assert.Single(patches);
    }

    [Fact]
    public void Extract_NoCuts_PatchContainsAllSubTriangles()
    {
        var cube = MeshFactory.CreateCube();
        var emptySegs = new Dictionary<int, List<IntersectionSegment>>();
        var cutResult = MeshCutter.Cut(cube.Mesh, emptySegs);
        var adj = SubTriangleAdjacency.Build(cutResult.SubTriangles);
        var patches = PatchExtractor.Extract(cutResult.SubTriangles, adj);
        Assert.Equal(cutResult.SubTriangles.Count, patches[0].SubTriangleIndices.Count);
    }

    [Fact]
    public void Extract_WithCuts_MultiplePatches()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        var cutResult = MeshCutter.Cut(cubeA.Mesh, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutResult.SubTriangles);
        var patches = PatchExtractor.Extract(cutResult.SubTriangles, adj);
        Assert.True(patches.Count >= 2);
    }

    [Fact]
    public void Extract_PatchIds_AreSequential()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        var cutResult = MeshCutter.Cut(cubeA.Mesh, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutResult.SubTriangles);
        var patches = PatchExtractor.Extract(cutResult.SubTriangles, adj);
        for (int i = 0; i < patches.Count; i++)
            Assert.Equal(i, patches[i].Id);
    }

    [Fact]
    public void Extract_AllSubTriangles_AssignedToExactlyOnePatch()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        var cutResult = MeshCutter.Cut(cubeA.Mesh, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutResult.SubTriangles);
        var patches = PatchExtractor.Extract(cutResult.SubTriangles, adj);
        int total = patches.Sum(p => p.SubTriangleIndices.Count);
        Assert.Equal(cutResult.SubTriangles.Count, total);
    }

    [Fact]
    public void Extract_NoDuplicateSubTriangleAssignments()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        var cutResult = MeshCutter.Cut(cubeA.Mesh, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutResult.SubTriangles);
        var patches = PatchExtractor.Extract(cutResult.SubTriangles, adj);
        var allIndices = patches.SelectMany(p => p.SubTriangleIndices).ToList();
        Assert.Equal(allIndices.Count, allIndices.Distinct().Count());
    }

    [Fact]
    public void Extract_PatchesHaveNonZeroSubTriangles()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        var cutResult = MeshCutter.Cut(cubeA.Mesh, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutResult.SubTriangles);
        var patches = PatchExtractor.Extract(cutResult.SubTriangles, adj);
        foreach (var patch in patches)
            Assert.True(patch.SubTriangleIndices.Count > 0);
    }

    [Fact]
    public void Extract_Tetrahedron_NoCuts_SinglePatch()
    {
        var tet = MeshFactory.CreateTetrahedron();
        var cutResult = MeshCutter.Cut(tet.Mesh, new Dictionary<int, List<IntersectionSegment>>());
        var adj = SubTriangleAdjacency.Build(cutResult.SubTriangles);
        var patches = PatchExtractor.Extract(cutResult.SubTriangles, adj);
        Assert.Single(patches);
    }

    [Fact]
    public void Extract_Sphere_NoCuts_SinglePatch()
    {
        var sphere = MeshFactory.CreateSphere(subdivisions: 1);
        var cutResult = MeshCutter.Cut(sphere.Mesh, new Dictionary<int, List<IntersectionSegment>>());
        var adj = SubTriangleAdjacency.Build(cutResult.SubTriangles);
        var patches = PatchExtractor.Extract(cutResult.SubTriangles, adj);
        Assert.Single(patches);
    }

    [Fact]
    public void Extract_SubTriangleIndices_AreValidIndices()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        var cutResult = MeshCutter.Cut(cubeA.Mesh, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutResult.SubTriangles);
        var patches = PatchExtractor.Extract(cutResult.SubTriangles, adj);
        foreach (var patch in patches)
            foreach (var idx in patch.SubTriangleIndices)
                Assert.True(idx >= 0 && idx < cutResult.SubTriangles.Count);
    }

    [Fact]
    public void Extract_DisjointCubes_SinglePatch()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(5, 0, 0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        var cutResult = MeshCutter.Cut(cubeA.Mesh, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutResult.SubTriangles);
        var patches = PatchExtractor.Extract(cutResult.SubTriangles, adj);
        Assert.Single(patches);
    }

    [Fact]
    public void Extract_CubeSphere_ProducesMultiplePatches()
    {
        var cube = MeshFactory.CreateCube();
        var sphere = MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 1);
        var graph = IntersectionGraph.Compute(cube.Mesh, sphere.Mesh);
        if (graph.Segments.Count > 0)
        {
            var cutResult = MeshCutter.Cut(cube.Mesh, graph.FaceSegmentsA);
            var adj = SubTriangleAdjacency.Build(cutResult.SubTriangles);
            var patches = PatchExtractor.Extract(cutResult.SubTriangles, adj);
            Assert.True(patches.Count >= 2);
        }
    }

    [Fact]
    public void Patch_SubTriangleIndices_CanBeAdded()
    {
        var p = new Patch(0);
        p.SubTriangleIndices.Add(1);
        p.SubTriangleIndices.Add(5);
        p.SubTriangleIndices.Add(10);
        Assert.Equal(3, p.SubTriangleIndices.Count);
        Assert.Contains(5, p.SubTriangleIndices);
    }
}
