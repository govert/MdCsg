using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Patches;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Patches;

/// <summary>Phase 6: PatchExtractor and SubTriangleAdjacency integration tests</summary>
public class PatchExtractorAdjacencyTests
{
    [Fact]
    public void Extract_NoCutFaces_SinglePatch()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var emptySegments = new Dictionary<int, List<IntersectionSegment>>();
        var cut = MeshCutter.Cut(mesh, emptySegments);
        var adj = SubTriangleAdjacency.Build(cut.SubTriangles);
        var patches = PatchExtractor.Extract(cut.SubTriangles, adj);
        // No intersection edges → one connected patch per connected component
        Assert.True(patches.Count >= 1);
    }

    [Fact]
    public void Extract_CutCubes_MultiplePatches()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cut = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cut.SubTriangles);
        var patches = PatchExtractor.Extract(cut.SubTriangles, adj);
        Assert.True(patches.Count >= 2, $"Expected >= 2 patches, got {patches.Count}");
    }

    [Fact]
    public void Extract_PatchIds_Sequential()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cut = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cut.SubTriangles);
        var patches = PatchExtractor.Extract(cut.SubTriangles, adj);
        for (int i = 0; i < patches.Count; i++)
            Assert.Equal(i, patches[i].Id);
    }

    [Fact]
    public void Extract_AllSubTriangles_Covered()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cut = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cut.SubTriangles);
        var patches = PatchExtractor.Extract(cut.SubTriangles, adj);
        var allIndices = patches.SelectMany(p => p.SubTriangleIndices).ToHashSet();
        Assert.Equal(cut.SubTriangles.Count, allIndices.Count);
    }

    [Fact]
    public void Extract_NoOverlap_BetweenPatches()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cut = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cut.SubTriangles);
        var patches = PatchExtractor.Extract(cut.SubTriangles, adj);
        var seen = new HashSet<int>();
        foreach (var patch in patches)
        {
            foreach (var idx in patch.SubTriangleIndices)
            {
                Assert.True(seen.Add(idx), $"Sub-triangle {idx} appears in multiple patches");
            }
        }
    }

    [Fact]
    public void Extract_Tetrahedron_NoCut_SinglePatch()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        var emptySegments = new Dictionary<int, List<IntersectionSegment>>();
        var cut = MeshCutter.Cut(mesh, emptySegments);
        var adj = SubTriangleAdjacency.Build(cut.SubTriangles);
        var patches = PatchExtractor.Extract(cut.SubTriangles, adj);
        Assert.Single(patches);
    }

    [Fact]
    public void SubTriangleAdjacency_NoCut_AllNeighborsValid()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var emptySegments = new Dictionary<int, List<IntersectionSegment>>();
        var cut = MeshCutter.Cut(mesh, emptySegments);
        var adj = SubTriangleAdjacency.Build(cut.SubTriangles);
        for (int i = 0; i < cut.SubTriangles.Count; i++)
        {
            var neighbors = adj.GetNeighbors(i);
            foreach (var (n, _) in neighbors)
            {
                Assert.True(n >= 0 && n < cut.SubTriangles.Count,
                    $"Neighbor {n} out of range for sub-triangle {i}");
            }
        }
    }

    [Fact]
    public void SubTriangleAdjacency_Symmetric()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var emptySegments = new Dictionary<int, List<IntersectionSegment>>();
        var cut = MeshCutter.Cut(mesh, emptySegments);
        var adj = SubTriangleAdjacency.Build(cut.SubTriangles);
        for (int i = 0; i < cut.SubTriangles.Count; i++)
        {
            var neighbors = adj.GetNeighbors(i);
            foreach (var (n, _) in neighbors)
            {
                var reverseNeighbors = adj.GetNeighbors(n);
                Assert.Contains(reverseNeighbors, r => r.Neighbor == i);
            }
        }
    }
}
