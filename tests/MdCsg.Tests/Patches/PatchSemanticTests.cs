using MdCsg.Cutting;
using MdCsg.Math;
using MdCsg.Patches;

namespace MdCsg.Tests.Patches;

/// <summary>Semantic paths: Patch, PatchExtractor, SubTriangleAdjacency</summary>
public class PatchSemanticTests
{
    private static FaceCutter.SubTriangle MakeSub(double x, double y, bool hasIntersection = false, byte flags = 0) =>
        new(new Vec3(x, y, 0), new Vec3(x + 1, y, 0), new Vec3(x, y + 1, 0), 0, hasIntersection, flags);

    [Fact]
    public void Patch_Properties_DefaultValues()
    {
        var p = new Patch(42);
        Assert.Equal(42, p.Id);
        Assert.Empty(p.SubTriangleIndices);
        Assert.Null(p.IsInside);
        Assert.False(p.HasConfidentPoint);
        Assert.Equal(0, p.SourceMesh);
        Assert.Null(p.CoplanarNormalsAgree);
    }

    [Fact]
    public void Patch_Properties_Settable()
    {
        var p = new Patch(0)
        {
            IsInside = true,
            HasConfidentPoint = true,
            SourceMesh = 1,
            CoplanarNormalsAgree = false,
            ConfidentPoint = new Vec3(1, 2, 3)
        };
        Assert.True(p.IsInside);
        Assert.True(p.HasConfidentPoint);
        Assert.Equal(1, p.SourceMesh);
        Assert.False(p.CoplanarNormalsAgree);
        Assert.Equal(new Vec3(1, 2, 3), p.ConfidentPoint);
    }

    [Fact]
    public void SubTriangleAdjacency_TwoAdjacentTriangles()
    {
        // Two triangles sharing edge (1,0)-(1,1)
        var subs = new List<FaceCutter.SubTriangle>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(1, 1, 0), 0, false),
            new(new Vec3(1, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 1, 0), 0, false)
        };
        var adj = SubTriangleAdjacency.Build(subs);
        Assert.Equal(2, adj.Count);
        Assert.True(adj.GetNeighbors(0).Count > 0);
        Assert.True(adj.GetNeighbors(1).Count > 0);
    }

    [Fact]
    public void SubTriangleAdjacency_NoSharedEdge_NoNeighbors()
    {
        var subs = new List<FaceCutter.SubTriangle>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false),
            new(new Vec3(10, 10, 0), new Vec3(11, 10, 0), new Vec3(10, 11, 0), 0, false)
        };
        var adj = SubTriangleAdjacency.Build(subs);
        Assert.Empty(adj.GetNeighbors(0));
        Assert.Empty(adj.GetNeighbors(1));
    }

    [Fact]
    public void SubTriangleAdjacency_IntersectionEdge_Marked()
    {
        // Two triangles sharing an edge, one side flags it as intersection
        var subs = new List<FaceCutter.SubTriangle>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, true, 0b001), // edge 0 (A-B) is intersection
            new(new Vec3(1, 0, 0), new Vec3(0, 0, 0), new Vec3(1, 1, 0), 0, false)          // shared edge (1,0)-(0,0) is reversed
        };
        var adj = SubTriangleAdjacency.Build(subs);
        // The shared edge should be marked as intersection
        var neighbors0 = adj.GetNeighbors(0);
        if (neighbors0.Count > 0)
        {
            Assert.True(neighbors0.Any(n => n.IsIntersectionEdge),
                "Shared edge should be flagged as intersection");
        }
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
    public void PatchExtractor_TwoConnectedTriangles_OnePatch()
    {
        var subs = new List<FaceCutter.SubTriangle>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(1, 1, 0), 0, false),
            new(new Vec3(0, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0), 0, false)
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        Assert.Single(patches);
        Assert.Equal(2, patches[0].SubTriangleIndices.Count);
    }

    [Fact]
    public void PatchExtractor_TwoDisconnectedTriangles_TwoPatches()
    {
        var subs = new List<FaceCutter.SubTriangle>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false),
            new(new Vec3(10, 10, 0), new Vec3(11, 10, 0), new Vec3(10, 11, 0), 0, false)
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        Assert.Equal(2, patches.Count);
    }

    [Fact]
    public void PatchExtractor_IntersectionEdge_SplitsPatches()
    {
        // Two adjacent triangles separated by an intersection edge
        var subs = new List<FaceCutter.SubTriangle>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, true, 0b001), // A-B is intersection
            new(new Vec3(1, 0, 0), new Vec3(0, 0, 0), new Vec3(1, 1, 0), 0, true, 0b001)   // A-B is intersection (reversed)
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        // The shared intersection edge should prevent flood-fill
        Assert.Equal(2, patches.Count);
    }

    [Fact]
    public void PatchExtractor_PatchIds_Sequential()
    {
        var subs = new List<FaceCutter.SubTriangle>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false),
            new(new Vec3(10, 10, 0), new Vec3(11, 10, 0), new Vec3(10, 11, 0), 0, false),
            new(new Vec3(20, 20, 0), new Vec3(21, 20, 0), new Vec3(20, 21, 0), 0, false)
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        Assert.Equal(3, patches.Count);
        Assert.Equal(0, patches[0].Id);
        Assert.Equal(1, patches[1].Id);
        Assert.Equal(2, patches[2].Id);
    }
}
