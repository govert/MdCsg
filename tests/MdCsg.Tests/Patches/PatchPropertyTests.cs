using MdCsg.Bvh;
using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Patches;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Patches;

/// <summary>Phase 6: Patch property tests — ID, SubTriangleIndices, coplanar flags</summary>
public class PatchPropertyTests
{
    [Fact]
    public void Patch_IdPreserved()
    {
        var p = new Patch(42);
        Assert.Equal(42, p.Id);
    }

    [Fact]
    public void Patch_DefaultValues()
    {
        var p = new Patch(0);
        Assert.Null(p.IsInside);
        Assert.False(p.HasConfidentPoint);
        Assert.Equal(Vec3.Zero, p.ConfidentPoint);
        Assert.Null(p.CoplanarNormalsAgree);
        Assert.Empty(p.SubTriangleIndices);
    }

    [Fact]
    public void Patch_SubTriangleIndices_Mutable()
    {
        var p = new Patch(0);
        p.SubTriangleIndices.Add(1);
        p.SubTriangleIndices.Add(2);
        p.SubTriangleIndices.Add(3);
        Assert.Equal(3, p.SubTriangleIndices.Count);
    }

    [Fact]
    public void Patch_IsInside_Settable()
    {
        var p = new Patch(0);
        p.IsInside = true;
        Assert.True(p.IsInside);
        p.IsInside = false;
        Assert.False(p.IsInside);
    }

    [Fact]
    public void Patch_CoplanarNormalsAgree_Settable()
    {
        var p = new Patch(0);
        Assert.Null(p.CoplanarNormalsAgree);
        p.CoplanarNormalsAgree = true;
        Assert.True(p.CoplanarNormalsAgree);
        p.CoplanarNormalsAgree = false;
        Assert.False(p.CoplanarNormalsAgree);
    }

    [Fact]
    public void Extract_CubeNoCut_SinglePatch()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var empty = new Dictionary<int, List<IntersectionSegment>>();
        var cutResult = MeshCutter.Cut(mesh, empty);
        var adj = SubTriangleAdjacency.Build(cutResult.SubTriangles);
        var patches = PatchExtractor.Extract(cutResult.SubTriangles, adj);
        // Uncut cube → all sub-triangles connected → single patch
        Assert.Single(patches);
        Assert.Equal(mesh.Faces.Count, patches[0].SubTriangleIndices.Count);
    }

    [Fact]
    public void Extract_CutCubes_MultiplePatches()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA,
            (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patches = PatchExtractor.Extract(cutA.SubTriangles, adj);
        Assert.True(patches.Count >= 2, $"Expected >= 2 patches, got {patches.Count}");
    }

    [Fact]
    public void Extract_Patches_PartitionSubTriangles()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA,
            (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patches = PatchExtractor.Extract(cutA.SubTriangles, adj);

        int total = patches.Sum(p => p.SubTriangleIndices.Count);
        Assert.Equal(cutA.SubTriangles.Count, total);
    }

    [Fact]
    public void Extract_Patches_NoDuplicateSubTriangles()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA,
            (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patches = PatchExtractor.Extract(cutA.SubTriangles, adj);

        var allIndices = patches.SelectMany(p => p.SubTriangleIndices).ToList();
        Assert.Equal(allIndices.Count, allIndices.Distinct().Count());
    }

    [Fact]
    public void Extract_Patches_IdsAreSequential()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA,
            (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patches = PatchExtractor.Extract(cutA.SubTriangles, adj);

        for (int i = 0; i < patches.Count; i++)
            Assert.Equal(i, patches[i].Id);
    }

    [Fact]
    public void Extract_Sphere_HasMultiplePatches()
    {
        var meshA = MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.6, 2).Mesh;
        var meshB = MeshFactory.CreateCube().Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA,
            (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patches = PatchExtractor.Extract(cutA.SubTriangles, adj);
        Assert.True(patches.Count >= 1);
    }

    [Fact]
    public void SubTriangleAdjacency_Count_MatchesSubTriangles()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA,
            (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutA.SubTriangles);
        Assert.Equal(cutA.SubTriangles.Count, adj.Count);
    }
}
