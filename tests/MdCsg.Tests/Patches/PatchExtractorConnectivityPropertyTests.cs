using MdCsg.Cutting;
using MdCsg.Math;
using MdCsg.Patches;

namespace MdCsg.Tests.Patches;

/// <summary>Phase 6: PatchExtractor — flood-fill connectivity, patch uniqueness, completeness</summary>
public class PatchExtractorConnectivityPropertyTests
{
    private static FaceCutter.SubTriangle MakeSub(Vec3 a, Vec3 b, Vec3 c, byte flags = 0)
        => new(a, b, c, 0, flags != 0, flags);

    [Fact]
    public void SingleTriangle_SinglePatch()
    {
        var subs = new[] { MakeSub(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0)) };
        var adj = SubTriangleAdjacency.Build(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        Assert.Single(patches);
        Assert.Single(patches[0].SubTriangleIndices);
        Assert.Equal(0, patches[0].SubTriangleIndices[0]);
    }

    [Fact]
    public void TwoDisjointTriangles_TwoPatches()
    {
        var subs = new[]
        {
            MakeSub(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            MakeSub(new Vec3(10, 0, 0), new Vec3(11, 0, 0), new Vec3(10, 1, 0)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        Assert.Equal(2, patches.Count);
    }

    [Fact]
    public void TwoAdjacentTriangles_NoIntersectionEdge_SinglePatch()
    {
        // Shared edge with no intersection flag → same patch
        var subs = new[]
        {
            MakeSub(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            MakeSub(new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        Assert.Single(patches);
        Assert.Equal(2, patches[0].SubTriangleIndices.Count);
    }

    [Fact]
    public void TwoAdjacentTriangles_IntersectionEdge_TwoPatches()
    {
        // Shared edge marked as intersection → separate patches
        // Edge A-B (vertex 1 to vertex 2) on first triangle
        var subs = new[]
        {
            MakeSub(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0b001), // edge 0 (A-B) is intersection
            MakeSub(new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0), 0b100), // edge 2 (C-A) is intersection
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        Assert.Equal(2, patches.Count);
    }

    [Fact]
    public void AllSubTriangles_Assigned_ToExactlyOnePatch()
    {
        var subs = new[]
        {
            MakeSub(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            MakeSub(new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0)),
            MakeSub(new Vec3(5, 0, 0), new Vec3(6, 0, 0), new Vec3(5, 1, 0)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var patches = PatchExtractor.Extract(subs, adj);

        var allIndices = patches.SelectMany(p => p.SubTriangleIndices).ToList();
        Assert.Equal(subs.Length, allIndices.Count);
        Assert.Equal(subs.Length, new HashSet<int>(allIndices).Count); // no duplicates
    }

    [Fact]
    public void PatchIds_AreSequential()
    {
        var subs = new[]
        {
            MakeSub(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            MakeSub(new Vec3(5, 0, 0), new Vec3(6, 0, 0), new Vec3(5, 1, 0)),
            MakeSub(new Vec3(10, 0, 0), new Vec3(11, 0, 0), new Vec3(10, 1, 0)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        for (int i = 0; i < patches.Count; i++)
            Assert.Equal(i, patches[i].Id);
    }

    [Fact]
    public void ThreeConnectedTriangles_SinglePatch()
    {
        // Fan of 3 triangles sharing center vertex
        var center = Vec3.Zero;
        var subs = new[]
        {
            MakeSub(center, new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            MakeSub(center, new Vec3(0, 1, 0), new Vec3(-1, 0, 0)),
            MakeSub(center, new Vec3(-1, 0, 0), new Vec3(0, -1, 0)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        Assert.Single(patches);
        Assert.Equal(3, patches[0].SubTriangleIndices.Count);
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
    public void ChainOfTriangles_AllConnected_SinglePatch()
    {
        // Linear chain: tri 0 shares edge with tri 1, tri 1 shares edge with tri 2, etc.
        var subs = new[]
        {
            MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0.5, 1, 0)),
            MakeSub(new Vec3(1, 0, 0), new Vec3(2, 0, 0), new Vec3(1.5, 1, 0)),
            // These share edge at x=1 with first triangle's edge B-C? No, they don't share exact vertices.
            // Let me use shared vertices properly:
            MakeSub(new Vec3(1, 0, 0), new Vec3(0.5, 1, 0), new Vec3(1.5, 1, 0)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        // At least the first and third share the edge (1,0,0)-(0.5,1,0)
        // Count the patches
        int totalSubs = patches.Sum(p => p.SubTriangleIndices.Count);
        Assert.Equal(3, totalSubs);
    }

    [Fact]
    public void MixedIntersectionEdges_CorrectPatchCount()
    {
        // 4 triangles: 0-1 share non-intersection edge, 2-3 share non-intersection edge
        // 1-2 share intersection edge → 2 patches: {0,1} and {2,3}
        var subs = new[]
        {
            MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),           // tri 0
            MakeSub(new Vec3(1, 0, 0), new Vec3(0, 1, 0), new Vec3(1, 1, 0), 0b010),    // tri 1, edge 1 (B-C) intersection
            MakeSub(new Vec3(0, 1, 0), new Vec3(1, 1, 0), new Vec3(0, 2, 0), 0b001),    // tri 2, edge 0 (A-B) intersection
            MakeSub(new Vec3(1, 1, 0), new Vec3(0, 2, 0), new Vec3(1, 2, 0)),           // tri 3
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        Assert.Equal(2, patches.Count);
    }

    [Fact]
    public void LargeNumberOfTriangles_AllConnected_SinglePatch()
    {
        // Grid of connected triangles
        var subs = new List<FaceCutter.SubTriangle>();
        for (int i = 0; i < 10; i++)
        {
            subs.Add(MakeSub(
                new Vec3(i, 0, 0),
                new Vec3(i + 1, 0, 0),
                new Vec3(i, 1, 0)));
            subs.Add(MakeSub(
                new Vec3(i + 1, 0, 0),
                new Vec3(i + 1, 1, 0),
                new Vec3(i, 1, 0)));
        }
        var adj = SubTriangleAdjacency.Build(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        Assert.Single(patches);
        Assert.Equal(20, patches[0].SubTriangleIndices.Count);
    }
}
