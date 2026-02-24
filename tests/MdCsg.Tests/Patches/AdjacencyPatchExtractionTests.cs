using MdCsg.Cutting;
using MdCsg.Math;
using MdCsg.Patches;

namespace MdCsg.Tests.Patches;

/// <summary>Phase 6: SubTriangleAdjacency and PatchExtractor invariants</summary>
public class AdjacencyPatchExtractionTests
{
    private static FaceCutter.SubTriangle MakeSub(Vec3 a, Vec3 b, Vec3 c, int face = 0, byte edgeFlags = 0)
        => new(a, b, c, face, edgeFlags != 0, edgeFlags);

    [Fact]
    public void Adjacency_SingleTriangle_NoNeighbors()
    {
        var subs = new List<FaceCutter.SubTriangle>
        {
            MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0))
        };
        var adj = SubTriangleAdjacency.Build(subs);
        Assert.Equal(1, adj.Count);
        Assert.Empty(adj.GetNeighbors(0));
    }

    [Fact]
    public void Adjacency_TwoAdjacentTriangles_BothNeighbors()
    {
        var subs = new List<FaceCutter.SubTriangle>
        {
            MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            MakeSub(new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        Assert.Equal(2, adj.Count);
        Assert.Contains(adj.GetNeighbors(0), n => n.Neighbor == 1);
        Assert.Contains(adj.GetNeighbors(1), n => n.Neighbor == 0);
    }

    [Fact]
    public void Adjacency_SharedEdge_NotIntersectionEdge()
    {
        var subs = new List<FaceCutter.SubTriangle>
        {
            MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            MakeSub(new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var neighbors = adj.GetNeighbors(0);
        Assert.All(neighbors, n => Assert.False(n.IsIntersectionEdge));
    }

    [Fact]
    public void Adjacency_IntersectionEdge_Flagged()
    {
        // Triangle 0: edge B-C (bit 1) is intersection edge
        var subs = new List<FaceCutter.SubTriangle>
        {
            MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, 0b010),
            MakeSub(new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0), 0, 0b100), // C-A is intersection
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var neighbors0 = adj.GetNeighbors(0);
        // The shared edge should be flagged as intersection
        Assert.Contains(neighbors0, n => n.IsIntersectionEdge);
    }

    [Fact]
    public void Adjacency_DisjointTriangles_NoNeighbors()
    {
        var subs = new List<FaceCutter.SubTriangle>
        {
            MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            MakeSub(new Vec3(5, 5, 5), new Vec3(6, 5, 5), new Vec3(5, 6, 5)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        Assert.Empty(adj.GetNeighbors(0));
        Assert.Empty(adj.GetNeighbors(1));
    }

    [Fact]
    public void PatchExtractor_SingleTriangle_SinglePatch()
    {
        var subs = new List<FaceCutter.SubTriangle>
        {
            MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0))
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        Assert.Single(patches);
        Assert.Single(patches[0].SubTriangleIndices);
    }

    [Fact]
    public void PatchExtractor_TwoConnected_SinglePatch()
    {
        var subs = new List<FaceCutter.SubTriangle>
        {
            MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            MakeSub(new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        Assert.Single(patches);
        Assert.Equal(2, patches[0].SubTriangleIndices.Count);
    }

    [Fact]
    public void PatchExtractor_TwoDisjoint_TwoPatches()
    {
        var subs = new List<FaceCutter.SubTriangle>
        {
            MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            MakeSub(new Vec3(5, 5, 5), new Vec3(6, 5, 5), new Vec3(5, 6, 5)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        Assert.Equal(2, patches.Count);
    }

    [Fact]
    public void PatchExtractor_IntersectionEdge_SplitsPatches()
    {
        // Two triangles sharing edge, but edge is intersection edge
        var subs = new List<FaceCutter.SubTriangle>
        {
            MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, 0b010),
            MakeSub(new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0), 0, 0b100),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        Assert.Equal(2, patches.Count);
    }

    [Fact]
    public void PatchExtractor_AllTrianglesAssigned()
    {
        var subs = new List<FaceCutter.SubTriangle>
        {
            MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            MakeSub(new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0)),
            MakeSub(new Vec3(5, 5, 5), new Vec3(6, 5, 5), new Vec3(5, 6, 5)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        int totalAssigned = patches.Sum(p => p.SubTriangleIndices.Count);
        Assert.Equal(subs.Count, totalAssigned);
    }

    [Fact]
    public void PatchExtractor_NoOverlap_UniqueTriangles()
    {
        var subs = new List<FaceCutter.SubTriangle>
        {
            MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            MakeSub(new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0)),
            MakeSub(new Vec3(5, 5, 5), new Vec3(6, 5, 5), new Vec3(5, 6, 5)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        var allIndices = patches.SelectMany(p => p.SubTriangleIndices).ToList();
        Assert.Equal(allIndices.Count, allIndices.Distinct().Count());
    }

    [Fact]
    public void PatchExtractor_PatchIds_Sequential()
    {
        var subs = new List<FaceCutter.SubTriangle>
        {
            MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            MakeSub(new Vec3(5, 5, 5), new Vec3(6, 5, 5), new Vec3(5, 6, 5)),
            MakeSub(new Vec3(10, 10, 10), new Vec3(11, 10, 10), new Vec3(10, 11, 10)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        for (int i = 0; i < patches.Count; i++)
            Assert.Equal(i, patches[i].Id);
    }

    [Fact]
    public void Patch_InitialState()
    {
        var patch = new Patch(42);
        Assert.Equal(42, patch.Id);
        Assert.Empty(patch.SubTriangleIndices);
        Assert.Null(patch.IsInside);
        Assert.False(patch.HasConfidentPoint);
        Assert.Null(patch.CoplanarNormalsAgree);
    }

    [Fact]
    public void Patch_SetProperties()
    {
        var patch = new Patch(0);
        patch.IsInside = true;
        patch.ConfidentPoint = new Vec3(1, 2, 3);
        patch.HasConfidentPoint = true;
        patch.CoplanarNormalsAgree = false;
        Assert.True(patch.IsInside);
        Assert.True(patch.HasConfidentPoint);
        Assert.Equal(new Vec3(1, 2, 3), patch.ConfidentPoint);
        Assert.False(patch.CoplanarNormalsAgree);
    }

    [Fact]
    public void Adjacency_ThreeTriangleFan_AllConnected()
    {
        // Three triangles sharing a vertex at (0,0,0)
        var subs = new List<FaceCutter.SubTriangle>
        {
            MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0.5, 0.5, 0)),
            MakeSub(new Vec3(0, 0, 0), new Vec3(0.5, 0.5, 0), new Vec3(0, 1, 0)),
            MakeSub(new Vec3(0, 0, 0), new Vec3(0, 1, 0), new Vec3(-0.5, 0.5, 0)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        // T0 shares edge with T1 (vertex 0,0,0 to 0.5,0.5,0)
        Assert.Contains(adj.GetNeighbors(0), n => n.Neighbor == 1);
        Assert.Contains(adj.GetNeighbors(1), n => n.Neighbor == 2);
    }
}
