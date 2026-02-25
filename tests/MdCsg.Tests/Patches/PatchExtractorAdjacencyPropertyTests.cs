using MdCsg.Cutting;
using MdCsg.Math;
using MdCsg.Patches;

namespace MdCsg.Tests.Patches;

/// <summary>Phase 6: PatchExtractor + SubTriangleAdjacency — extraction, flood-fill, adjacency building</summary>
public class PatchExtractorAdjacencyPropertyTests
{
    private static FaceCutter.SubTriangle MakeST(Vec3 a, Vec3 b, Vec3 c, int faceIdx, bool hasInt, byte flags = 0)
        => new(a, b, c, faceIdx, hasInt, flags);

    [Fact]
    public void Adjacency_TwoAdjacentTriangles_AreNeighbors()
    {
        var v0 = new Vec3(0, 0, 0);
        var v1 = new Vec3(1, 0, 0);
        var v2 = new Vec3(0, 1, 0);
        var v3 = new Vec3(1, 1, 0);
        var subTris = new[]
        {
            MakeST(v0, v1, v2, 0, false),
            MakeST(v1, v3, v2, 1, false),
        };
        var adj = SubTriangleAdjacency.Build(subTris);
        Assert.Equal(2, adj.Count);
        Assert.True(adj.GetNeighbors(0).Any(n => n.Neighbor == 1));
        Assert.True(adj.GetNeighbors(1).Any(n => n.Neighbor == 0));
    }

    [Fact]
    public void Adjacency_TwoDisjointTriangles_NoNeighbors()
    {
        var subTris = new[]
        {
            MakeST(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false),
            MakeST(new Vec3(10, 10, 10), new Vec3(11, 10, 10), new Vec3(10, 11, 10), 1, false),
        };
        var adj = SubTriangleAdjacency.Build(subTris);
        Assert.Empty(adj.GetNeighbors(0));
        Assert.Empty(adj.GetNeighbors(1));
    }

    [Fact]
    public void Adjacency_SharedIntersectionEdge_FlaggedAsIntersection()
    {
        var v0 = new Vec3(0, 0, 0);
        var v1 = new Vec3(1, 0, 0);
        var v2 = new Vec3(0, 1, 0);
        var v3 = new Vec3(1, 1, 0);
        // Tri0 edge A-B (index 0) is intersection, tri1 has no intersection edge
        var subTris = new[]
        {
            MakeST(v0, v1, v2, 0, true, 0b001), // edge 0 (A-B = v0-v1) is intersection
            MakeST(v1, v0, v3, 1, false),        // shares v1-v0 edge
        };
        var adj = SubTriangleAdjacency.Build(subTris);
        var neighbors0 = adj.GetNeighbors(0);
        Assert.True(neighbors0.Any(n => n.Neighbor == 1 && n.IsIntersectionEdge),
            "Shared edge should be flagged as intersection");
    }

    [Fact]
    public void Adjacency_SharedNonIntersectionEdge_NotFlagged()
    {
        var v0 = new Vec3(0, 0, 0);
        var v1 = new Vec3(1, 0, 0);
        var v2 = new Vec3(0, 1, 0);
        var v3 = new Vec3(1, 1, 0);
        var subTris = new[]
        {
            MakeST(v0, v1, v2, 0, false),
            MakeST(v1, v3, v2, 1, false),
        };
        var adj = SubTriangleAdjacency.Build(subTris);
        var neighbors0 = adj.GetNeighbors(0);
        Assert.True(neighbors0.Any(n => n.Neighbor == 1 && !n.IsIntersectionEdge));
    }

    [Fact]
    public void Adjacency_Count_MatchesSubTriangles()
    {
        var subTris = new[]
        {
            MakeST(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, false),
            MakeST(Vec3.UnitX, new Vec3(1, 1, 0), Vec3.UnitY, 1, false),
            MakeST(new Vec3(10, 0, 0), new Vec3(11, 0, 0), new Vec3(10, 1, 0), 2, false),
        };
        var adj = SubTriangleAdjacency.Build(subTris);
        Assert.Equal(3, adj.Count);
    }

    [Fact]
    public void Extract_SingleTriangle_OnePatch()
    {
        var subTris = new[]
        {
            MakeST(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, false),
        };
        var adj = SubTriangleAdjacency.Build(subTris);
        var patches = PatchExtractor.Extract(subTris, adj);
        Assert.Single(patches);
        Assert.Single(patches[0].SubTriangleIndices);
        Assert.Equal(0, patches[0].SubTriangleIndices[0]);
    }

    [Fact]
    public void Extract_TwoConnectedNoIntersection_OnePatch()
    {
        var v0 = new Vec3(0, 0, 0);
        var v1 = new Vec3(1, 0, 0);
        var v2 = new Vec3(0, 1, 0);
        var v3 = new Vec3(1, 1, 0);
        var subTris = new[]
        {
            MakeST(v0, v1, v2, 0, false),
            MakeST(v1, v3, v2, 1, false),
        };
        var adj = SubTriangleAdjacency.Build(subTris);
        var patches = PatchExtractor.Extract(subTris, adj);
        Assert.Single(patches);
        Assert.Equal(2, patches[0].SubTriangleIndices.Count);
    }

    [Fact]
    public void Extract_TwoDisconnected_TwoPatches()
    {
        var subTris = new[]
        {
            MakeST(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false),
            MakeST(new Vec3(10, 10, 10), new Vec3(11, 10, 10), new Vec3(10, 11, 10), 1, false),
        };
        var adj = SubTriangleAdjacency.Build(subTris);
        var patches = PatchExtractor.Extract(subTris, adj);
        Assert.Equal(2, patches.Count);
        Assert.Single(patches[0].SubTriangleIndices);
        Assert.Single(patches[1].SubTriangleIndices);
    }

    [Fact]
    public void Extract_SeparatedByIntersectionEdge_TwoPatches()
    {
        var v0 = new Vec3(0, 0, 0);
        var v1 = new Vec3(1, 0, 0);
        var v2 = new Vec3(0, 1, 0);
        var v3 = new Vec3(1, 1, 0);
        // Edge v0-v1 is intersection on tri 0
        var subTris = new[]
        {
            MakeST(v0, v1, v2, 0, true, 0b001), // edge 0 (A-B) = v0-v1 is intersection
            MakeST(v1, v0, v3, 1, true, 0b001),  // edge 0 (A-B) = v1-v0 is intersection
        };
        var adj = SubTriangleAdjacency.Build(subTris);
        var patches = PatchExtractor.Extract(subTris, adj);
        Assert.Equal(2, patches.Count);
    }

    [Fact]
    public void Extract_PatchIds_AreSequential()
    {
        var subTris = new[]
        {
            MakeST(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false),
            MakeST(new Vec3(10, 0, 0), new Vec3(11, 0, 0), new Vec3(10, 1, 0), 1, false),
            MakeST(new Vec3(20, 0, 0), new Vec3(21, 0, 0), new Vec3(20, 1, 0), 2, false),
        };
        var adj = SubTriangleAdjacency.Build(subTris);
        var patches = PatchExtractor.Extract(subTris, adj);
        for (int i = 0; i < patches.Count; i++)
            Assert.Equal(i, patches[i].Id);
    }

    [Fact]
    public void Extract_AllSubTrianglesAssigned()
    {
        var v0 = new Vec3(0, 0, 0);
        var v1 = new Vec3(1, 0, 0);
        var v2 = new Vec3(0, 1, 0);
        var v3 = new Vec3(1, 1, 0);
        var v4 = new Vec3(10, 0, 0);
        var v5 = new Vec3(11, 0, 0);
        var v6 = new Vec3(10, 1, 0);
        var subTris = new[]
        {
            MakeST(v0, v1, v2, 0, false),
            MakeST(v1, v3, v2, 1, false),
            MakeST(v4, v5, v6, 2, false),
        };
        var adj = SubTriangleAdjacency.Build(subTris);
        var patches = PatchExtractor.Extract(subTris, adj);
        int totalAssigned = patches.Sum(p => p.SubTriangleIndices.Count);
        Assert.Equal(subTris.Length, totalAssigned);
    }

    [Fact]
    public void Extract_ThreeConnectedChain_OnePatch()
    {
        // Three triangles forming a strip
        var v0 = new Vec3(0, 0, 0);
        var v1 = new Vec3(1, 0, 0);
        var v2 = new Vec3(0, 1, 0);
        var v3 = new Vec3(1, 1, 0);
        var v4 = new Vec3(2, 0, 0);
        var subTris = new[]
        {
            MakeST(v0, v1, v2, 0, false),
            MakeST(v1, v3, v2, 1, false),
            MakeST(v1, v4, v3, 2, false),
        };
        var adj = SubTriangleAdjacency.Build(subTris);
        var patches = PatchExtractor.Extract(subTris, adj);
        Assert.Single(patches);
        Assert.Equal(3, patches[0].SubTriangleIndices.Count);
    }

    [Fact]
    public void Extract_IntersectionEdgeInMiddle_SplitsChain()
    {
        // Three triangles forming a strip, intersection edge between t0 and t1
        var v0 = new Vec3(0, 0, 0);
        var v1 = new Vec3(1, 0, 0);
        var v2 = new Vec3(0, 1, 0);
        var v3 = new Vec3(1, 1, 0);
        var v4 = new Vec3(2, 0, 0);
        var subTris = new[]
        {
            // t0: edges are v0-v1 (e0), v1-v2 (e1), v2-v0 (e2). Mark edge 1 (v1-v2) as intersection
            MakeST(v0, v1, v2, 0, true, 0b010),
            // t1: edges are v1-v3 (e0), v3-v2 (e1), v2-v1 (e2). Mark edge 2 (v2-v1) as intersection
            MakeST(v1, v3, v2, 1, true, 0b100),
            // t2: shares v1-v3 with t1 normally (no intersection edge)
            MakeST(v1, v4, v3, 2, false),
        };
        var adj = SubTriangleAdjacency.Build(subTris);
        var patches = PatchExtractor.Extract(subTris, adj);
        // t0 is separated from t1/t2 by the intersection edge v1-v2
        Assert.True(patches.Count >= 2, $"Should have at least 2 patches, got {patches.Count}");
    }

    [Fact]
    public void Adjacency_FourTriangleFan_CorrectNeighborCount()
    {
        // Four triangles sharing a common vertex at center
        var center = new Vec3(0, 0, 0);
        var v1 = new Vec3(1, 0, 0);
        var v2 = new Vec3(0, 1, 0);
        var v3 = new Vec3(-1, 0, 0);
        var v4 = new Vec3(0, -1, 0);
        var subTris = new[]
        {
            MakeST(center, v1, v2, 0, false),
            MakeST(center, v2, v3, 1, false),
            MakeST(center, v3, v4, 2, false),
            MakeST(center, v4, v1, 3, false),
        };
        var adj = SubTriangleAdjacency.Build(subTris);
        // Each triangle shares edges with its two neighbors in the fan
        for (int i = 0; i < 4; i++)
        {
            Assert.True(adj.GetNeighbors(i).Count >= 1,
                $"Fan triangle {i} should have at least 1 neighbor");
        }
    }

    [Fact]
    public void Adjacency_Symmetric()
    {
        var v0 = new Vec3(0, 0, 0);
        var v1 = new Vec3(1, 0, 0);
        var v2 = new Vec3(0, 1, 0);
        var v3 = new Vec3(1, 1, 0);
        var subTris = new[]
        {
            MakeST(v0, v1, v2, 0, false),
            MakeST(v1, v3, v2, 1, false),
        };
        var adj = SubTriangleAdjacency.Build(subTris);
        // If 0 has neighbor 1, then 1 must have neighbor 0
        foreach (var (neighbor, isInt) in adj.GetNeighbors(0))
            Assert.True(adj.GetNeighbors(neighbor).Any(n => n.Neighbor == 0));
    }
}
