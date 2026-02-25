using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Patches;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Patches;

/// <summary>Phase 6: SubTriangleAdjacency edge case tests — fan topology, large offset, intersection edge propagation</summary>
public class SubTriangleAdjacencyEdgeCaseTests
{
    [Fact]
    public void Build_ThreeTriangles_FanTopology_AllAdjacent()
    {
        // Three triangles sharing edge from (0,0,0) to (1,0,0)
        var subs = new List<FaceCutter.SubTriangle>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false),
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 0, 1), 0, false),
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, -1, 0), 0, false)
        };
        var adj = SubTriangleAdjacency.Build(subs);
        for (int i = 0; i < 3; i++)
            Assert.True(adj.GetNeighbors(i).Count >= 2);
    }

    [Fact]
    public void Build_IntersectionEdge_BothSidesFlagged()
    {
        // Both sides flag the shared edge as intersection
        var subs = new List<FaceCutter.SubTriangle>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, true, 0b001),
            new(new Vec3(1, 0, 0), new Vec3(0, 0, 0), new Vec3(0, 0, 1), 0, true, 0b001)
        };
        var adj = SubTriangleAdjacency.Build(subs);
        Assert.Contains(adj.GetNeighbors(0), n => n.Neighbor == 1 && n.IsIntersectionEdge);
    }

    [Fact]
    public void Build_LargeOffset_HashStillWorks()
    {
        var subs = new List<FaceCutter.SubTriangle>
        {
            new(new Vec3(1000, 2000, 3000), new Vec3(1001, 2000, 3000), new Vec3(1000, 2001, 3000), 0, false),
            new(new Vec3(1001, 2000, 3000), new Vec3(1000, 2000, 3000), new Vec3(1000, 2000, 3001), 0, false)
        };
        var adj = SubTriangleAdjacency.Build(subs);
        Assert.Contains(adj.GetNeighbors(0), n => n.Neighbor == 1);
    }

    [Fact]
    public void Build_ManyTriangles_OctagonFan()
    {
        var center = new Vec3(0, 0, 0);
        int n = 8;
        var subs = new List<FaceCutter.SubTriangle>();
        for (int i = 0; i < n; i++)
        {
            double a1 = 2 * System.Math.PI * i / n;
            double a2 = 2 * System.Math.PI * (i + 1) / n;
            var v1 = new Vec3(System.Math.Cos(a1), System.Math.Sin(a1), 0);
            var v2 = new Vec3(System.Math.Cos(a2), System.Math.Sin(a2), 0);
            subs.Add(new FaceCutter.SubTriangle(center, v1, v2, 0, false));
        }
        var adj = SubTriangleAdjacency.Build(subs);
        Assert.Equal(n, adj.Count);
        for (int i = 0; i < n; i++)
        {
            int next = (i + 1) % n;
            Assert.Contains(adj.GetNeighbors(i), x => x.Neighbor == next);
        }
    }

    [Fact]
    public void Build_CubeNoCut_NoIntersectionEdges()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var empty = new Dictionary<int, List<IntersectionSegment>>();
        var cutResult = MeshCutter.Cut(mesh, empty);
        var adj = SubTriangleAdjacency.Build(cutResult.SubTriangles);
        for (int i = 0; i < adj.Count; i++)
            foreach (var (_, isIntEdge) in adj.GetNeighbors(i))
                Assert.False(isIntEdge);
    }

    [Fact]
    public void Build_CutCubes_HasIntersectionEdges()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA,
            (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutA.SubTriangles);

        bool found = false;
        for (int i = 0; i < adj.Count && !found; i++)
            foreach (var (_, isInt) in adj.GetNeighbors(i))
                if (isInt) { found = true; break; }
        Assert.True(found);
    }

    [Fact]
    public void Build_NeighborSymmetry_Always()
    {
        var subs = new List<FaceCutter.SubTriangle>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false),
            new(new Vec3(1, 0, 0), new Vec3(0, 0, 0), new Vec3(1, 1, 0), 0, false),
            new(new Vec3(0, 0, 0), new Vec3(0, 1, 0), new Vec3(-1, 0, 0), 0, false)
        };
        var adj = SubTriangleAdjacency.Build(subs);
        for (int i = 0; i < 3; i++)
            foreach (var (neighbor, _) in adj.GetNeighbors(i))
                Assert.Contains(adj.GetNeighbors(neighbor), n => n.Neighbor == i);
    }

    [Fact]
    public void PatchExtractor_SingleTriangle_SinglePatch()
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

    [Fact]
    public void PatchExtractor_IntersectionEdge_SplitsPatches()
    {
        var subs = new List<FaceCutter.SubTriangle>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, true, 0b001),
            new(new Vec3(1, 0, 0), new Vec3(0, 0, 0), new Vec3(0, 0, 1), 0, false, 0)
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        Assert.Equal(2, patches.Count);
    }

    [Fact]
    public void PatchExtractor_DisjointTriangles_SeparatePatches()
    {
        var subs = new List<FaceCutter.SubTriangle>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false),
            new(new Vec3(10, 10, 10), new Vec3(11, 10, 10), new Vec3(10, 11, 10), 1, false)
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        Assert.Equal(2, patches.Count);
    }

    [Fact]
    public void PatchExtractor_ChainOfThree_OneOrTwoPatches()
    {
        // Three triangles in a chain: 0-1-2, no intersection edges → 1 patch
        var subs = new List<FaceCutter.SubTriangle>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false),
            new(new Vec3(1, 0, 0), new Vec3(2, 0, 0), new Vec3(0, 1, 0), 0, false),
            new(new Vec3(2, 0, 0), new Vec3(3, 0, 0), new Vec3(0, 1, 0), 0, false)
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        Assert.Single(patches);
        Assert.Equal(3, patches[0].SubTriangleIndices.Count);
    }
}
