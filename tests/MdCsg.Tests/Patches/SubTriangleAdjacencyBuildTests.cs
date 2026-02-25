using MdCsg.Cutting;
using MdCsg.Math;
using MdCsg.Patches;

namespace MdCsg.Tests.Patches;

/// <summary>Phase 6: SubTriangleAdjacency — build, neighbor relations, intersection edge tracking</summary>
public class SubTriangleAdjacencyBuildTests
{
    private static FaceCutter.SubTriangle MakeSub(Vec3 a, Vec3 b, Vec3 c, int face = 0, bool hasInt = false, byte flags = 0)
        => new(a, b, c, face, hasInt, flags);

    [Fact]
    public void SingleTriangle_NoNeighbors()
    {
        var subs = new[] { MakeSub(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0)) };
        var adj = SubTriangleAdjacency.Build(subs);
        Assert.Equal(1, adj.Count);
        Assert.Empty(adj.GetNeighbors(0));
    }

    [Fact]
    public void TwoTriangles_SharedEdge_AreNeighbors()
    {
        var subs = new[]
        {
            MakeSub(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            MakeSub(new Vec3(1, 0, 0), Vec3.Zero, new Vec3(1, 1, 0)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        Assert.Equal(2, adj.Count);
        Assert.True(adj.GetNeighbors(0).Any(n => n.Neighbor == 1));
        Assert.True(adj.GetNeighbors(1).Any(n => n.Neighbor == 0));
    }

    [Fact]
    public void TwoTriangles_NoSharedEdge_NotNeighbors()
    {
        var subs = new[]
        {
            MakeSub(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            MakeSub(new Vec3(5, 5, 5), new Vec3(6, 5, 5), new Vec3(5, 6, 5)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        Assert.Empty(adj.GetNeighbors(0));
        Assert.Empty(adj.GetNeighbors(1));
    }

    [Fact]
    public void AdjacencyIsSymmetric()
    {
        var subs = new[]
        {
            MakeSub(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0.5, 1, 0)),
            MakeSub(new Vec3(1, 0, 0), Vec3.Zero, new Vec3(0.5, -1, 0)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        foreach (var (neighbor, _) in adj.GetNeighbors(0))
        {
            Assert.True(adj.GetNeighbors(neighbor).Any(n => n.Neighbor == 0));
        }
    }

    [Fact]
    public void IntersectionEdge_FlaggedCorrectly()
    {
        // Triangle 0 has edge AB as intersection edge (bit 0)
        var subs = new[]
        {
            MakeSub(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0.5, 1, 0), 0, true, 0b001),
            MakeSub(new Vec3(1, 0, 0), Vec3.Zero, new Vec3(0.5, -1, 0), 0, false, 0),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        // The shared edge should be flagged as intersection
        var neighbors0 = adj.GetNeighbors(0);
        Assert.True(neighbors0.Any(n => n.Neighbor == 1 && n.IsIntersectionEdge));
    }

    [Fact]
    public void NonIntersectionEdge_NotFlagged()
    {
        var subs = new[]
        {
            MakeSub(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0.5, 1, 0), 0, false, 0),
            MakeSub(new Vec3(1, 0, 0), Vec3.Zero, new Vec3(0.5, -1, 0), 0, false, 0),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var neighbors0 = adj.GetNeighbors(0);
        Assert.True(neighbors0.All(n => !n.IsIntersectionEdge));
    }

    [Fact]
    public void ThreeTriangles_FanConfiguration()
    {
        // Three triangles sharing a common vertex at origin
        var a = Vec3.Zero;
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(-1, 0, 0);
        var subs = new[]
        {
            MakeSub(a, b, c),   // shares edge a-c with [2], edge a-b with [?]
            MakeSub(a, c, d),   // shares edge a-c with [0]
            MakeSub(a, d, new Vec3(0, -1, 0)), // shares edge a-d with [1]
        };
        var adj = SubTriangleAdjacency.Build(subs);
        Assert.Equal(3, adj.Count);
        // Tri 0 and Tri 1 share edge a-c
        Assert.True(adj.GetNeighbors(0).Any(n => n.Neighbor == 1));
        // Tri 1 and Tri 2 share edge a-d
        Assert.True(adj.GetNeighbors(1).Any(n => n.Neighbor == 2));
    }

    [Fact]
    public void Count_MatchesInput()
    {
        var subs = new[]
        {
            MakeSub(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            MakeSub(new Vec3(1, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 1, 0)),
            MakeSub(new Vec3(2, 0, 0), new Vec3(3, 0, 0), new Vec3(2, 1, 0)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        Assert.Equal(3, adj.Count);
    }

    [Fact]
    public void EitherSideIntersection_PropagatesFlag()
    {
        // If either side says the shared edge is intersection, it should be flagged
        var subs = new[]
        {
            MakeSub(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0.5, 1, 0), 0, false, 0),
            MakeSub(new Vec3(1, 0, 0), Vec3.Zero, new Vec3(0.5, -1, 0), 0, true, 0b001),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        // Tri 1 edge 0 (1→0) is flagged, so the shared edge should be intersection
        var neighbors0 = adj.GetNeighbors(0);
        Assert.True(neighbors0.Any(n => n.Neighbor == 1 && n.IsIntersectionEdge));
    }

    [Fact]
    public void Tolerance_MergesNearbyVertices()
    {
        // Two triangles with vertices that differ by less than tolerance
        var subs = new[]
        {
            MakeSub(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0.5, 1, 0)),
            MakeSub(new Vec3(1, 1e-10, 0), new Vec3(0, 1e-10, 0), new Vec3(0.5, -1, 0)),
        };
        // With default tolerance 1e-8, these should hash to the same bucket
        var adj = SubTriangleAdjacency.Build(subs);
        Assert.True(adj.GetNeighbors(0).Any(n => n.Neighbor == 1),
            "Near-coincident vertices should be treated as shared edge");
    }

    [Fact]
    public void EmptyInput_ReturnsEmptyAdjacency()
    {
        var adj = SubTriangleAdjacency.Build(Array.Empty<FaceCutter.SubTriangle>());
        Assert.Equal(0, adj.Count);
    }
}
