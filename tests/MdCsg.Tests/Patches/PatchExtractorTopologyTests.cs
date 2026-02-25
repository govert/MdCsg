using MdCsg.Api;
using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Patches;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Patches;

/// <summary>Phase 6: PatchExtractor topology — patch count for various geometries, patch sub-triangle coverage</summary>
public class PatchExtractorTopologyTests
{
    [Fact]
    public void OverlappingCubes_MultiplePatchesPerMesh()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);

        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        var cutA = MeshCutter.Cut(a.Mesh, graph.FaceSegmentsA);
        var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patchesA = PatchExtractor.Extract(cutA.SubTriangles, adjA);

        // Overlapping cubes should have multiple patches (inside and outside)
        Assert.True(patchesA.Count >= 2,
            $"Expected >=2 patches for mesh A, got {patchesA.Count}");
    }

    [Fact]
    public void DisjointCubes_SinglePatchPerMesh()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);

        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        // No intersection segments → no cuts → entire mesh is one patch
        var cutA = MeshCutter.Cut(a.Mesh, graph.FaceSegmentsA);
        var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patchesA = PatchExtractor.Extract(cutA.SubTriangles, adjA);

        Assert.Equal(1, patchesA.Count);
    }

    [Fact]
    public void AllSubTriangles_BelongToExactlyOnePatch()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0)).Mesh);

        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        var cutA = MeshCutter.Cut(a.Mesh, graph.FaceSegmentsA);
        var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patchesA = PatchExtractor.Extract(cutA.SubTriangles, adjA);

        // Collect all sub-triangle indices from all patches
        var covered = new HashSet<int>();
        foreach (var patch in patchesA)
        {
            foreach (var idx in patch.SubTriangleIndices)
            {
                Assert.True(covered.Add(idx),
                    $"Sub-triangle {idx} belongs to multiple patches");
            }
        }
        Assert.Equal(cutA.SubTriangles.Count, covered.Count);
    }

    [Fact]
    public void PatchIndices_AreValid()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);

        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        var cutA = MeshCutter.Cut(a.Mesh, graph.FaceSegmentsA);
        var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patchesA = PatchExtractor.Extract(cutA.SubTriangles, adjA);

        for (int i = 0; i < patchesA.Count; i++)
        {
            Assert.Equal(i, patchesA[i].Id);
            foreach (var idx in patchesA[i].SubTriangleIndices)
            {
                Assert.True(idx >= 0 && idx < cutA.SubTriangles.Count,
                    $"Patch {i} has invalid sub-triangle index {idx}");
            }
        }
    }

    [Fact]
    public void EachPatch_HasAtLeastOneSubTriangle()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);

        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        var cutA = MeshCutter.Cut(a.Mesh, graph.FaceSegmentsA);
        var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patchesA = PatchExtractor.Extract(cutA.SubTriangles, adjA);

        foreach (var patch in patchesA)
            Assert.True(patch.SubTriangleIndices.Count > 0,
                $"Patch {patch.Id} has no sub-triangles");
    }

    [Fact]
    public void SphereCube_HasMultiplePatches()
    {
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 2).Mesh);
        var cube = new Solid(MeshFactory.CreateCube().Mesh);

        var graph = IntersectionGraph.Compute(sphere.Mesh, cube.Mesh);
        var cutSphere = MeshCutter.Cut(sphere.Mesh, graph.FaceSegmentsA);
        var adjSphere = SubTriangleAdjacency.Build(cutSphere.SubTriangles);
        var patches = PatchExtractor.Extract(cutSphere.SubTriangles, adjSphere);

        Assert.True(patches.Count >= 2,
            $"Sphere-cube should have >=2 patches, got {patches.Count}");
    }

    [Fact]
    public void EmptyInput_NoPatches()
    {
        var subs = Array.Empty<FaceCutter.SubTriangle>();
        var adj = SubTriangleAdjacency.Build(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        Assert.Empty(patches);
    }

    [Fact]
    public void SingleSubTriangle_OnePatch()
    {
        var subs = new List<FaceCutter.SubTriangle>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false)
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        Assert.Single(patches);
        Assert.Single(patches[0].SubTriangleIndices);
    }
}
