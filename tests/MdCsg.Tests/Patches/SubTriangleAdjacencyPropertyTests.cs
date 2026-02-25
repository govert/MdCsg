using MdCsg.Cutting;
using MdCsg.Math;
using MdCsg.Patches;

namespace MdCsg.Tests.Patches;

/// <summary>Phase 6: SubTriangleAdjacency - Edge-based adjacency, intersection edge detection, tolerance</summary>
public class SubTriangleAdjacencyPropertyTests
{
    private static FaceCutter.SubTriangle MakeSub(Vec3 a, Vec3 b, Vec3 c, int faceIdx = 0, bool hasIntEdge = false, byte flags = 0)
        => new(a, b, c, faceIdx, hasIntEdge, flags);

    [Fact]
    public void SingleTriangle_NoNeighbors()
    {
        var subs = new[] { MakeSub(Vec3.Zero, Vec3.UnitX, Vec3.UnitY) };
        var adj = SubTriangleAdjacency.Build(subs);
        Assert.Equal(1, adj.Count);
        Assert.Empty(adj.GetNeighbors(0));
    }

    [Fact]
    public void TwoTriangles_SharedEdge_AreNeighbors()
    {
        var subs = new[]
        {
            MakeSub(Vec3.Zero, Vec3.UnitX, Vec3.UnitY),
            MakeSub(Vec3.UnitX, Vec3.Zero, Vec3.UnitZ),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        Assert.Equal(2, adj.Count);
        Assert.Contains(adj.GetNeighbors(0), n => n.Neighbor == 1);
        Assert.Contains(adj.GetNeighbors(1), n => n.Neighbor == 0);
    }

    [Fact]
    public void TwoTriangles_SharedEdge_NotIntersectionEdge()
    {
        var subs = new[]
        {
            MakeSub(Vec3.Zero, Vec3.UnitX, Vec3.UnitY),
            MakeSub(Vec3.UnitX, Vec3.Zero, Vec3.UnitZ),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var neighbors = adj.GetNeighbors(0);
        Assert.Contains(neighbors, n => n.Neighbor == 1 && !n.IsIntersectionEdge);
    }

    [Fact]
    public void TwoTriangles_SharedIntersectionEdge_MarkedAsIntersection()
    {
        var subs = new[]
        {
            MakeSub(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, true, 1),
            MakeSub(Vec3.UnitX, Vec3.Zero, Vec3.UnitZ, 0, false, 0),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var neighbors = adj.GetNeighbors(0);
        Assert.Contains(neighbors, n => n.Neighbor == 1 && n.IsIntersectionEdge);
    }

    [Fact]
    public void DisjointTriangles_NoNeighbors()
    {
        var subs = new[]
        {
            MakeSub(Vec3.Zero, Vec3.UnitX, Vec3.UnitY),
            MakeSub(new Vec3(100, 0, 0), new Vec3(101, 0, 0), new Vec3(100, 1, 0)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        Assert.Empty(adj.GetNeighbors(0));
        Assert.Empty(adj.GetNeighbors(1));
    }

    [Fact]
    public void ThreeTriangles_FanFromVertex_AllAdjacent()
    {
        var subs = new[]
        {
            MakeSub(Vec3.Zero, Vec3.UnitX, Vec3.UnitY),
            MakeSub(Vec3.Zero, Vec3.UnitY, Vec3.UnitZ),
            MakeSub(Vec3.Zero, Vec3.UnitZ, Vec3.UnitX),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        Assert.Equal(2, adj.GetNeighbors(0).Count);
        Assert.Equal(2, adj.GetNeighbors(1).Count);
        Assert.Equal(2, adj.GetNeighbors(2).Count);
    }

    [Fact]
    public void ThreeTriangles_Strip_MiddleHasTwoNeighbors()
    {
        var subs = new[]
        {
            MakeSub(Vec3.Zero, Vec3.UnitX, Vec3.UnitY),
            MakeSub(Vec3.UnitX, new Vec3(1, 1, 0), Vec3.UnitY),
            MakeSub(new Vec3(1, 1, 0), new Vec3(0, 2, 0), Vec3.UnitY),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        Assert.Equal(2, adj.GetNeighbors(1).Count);
        Assert.Contains(adj.GetNeighbors(1), n => n.Neighbor == 0);
        Assert.Contains(adj.GetNeighbors(1), n => n.Neighbor == 2);
    }

    [Fact]
    public void AdjacencyIsSymmetric()
    {
        var subs = new[]
        {
            MakeSub(Vec3.Zero, Vec3.UnitX, Vec3.UnitY),
            MakeSub(Vec3.UnitX, Vec3.Zero, Vec3.UnitZ),
            MakeSub(Vec3.Zero, Vec3.UnitY, Vec3.UnitZ),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        for (int i = 0; i < adj.Count; i++)
        {
            foreach (var (neighbor, _) in adj.GetNeighbors(i))
            {
                Assert.Contains(adj.GetNeighbors(neighbor), n => n.Neighbor == i);
            }
        }
    }

    [Fact]
    public void EmptyInput_EmptyAdjacency()
    {
        var adj = SubTriangleAdjacency.Build(Array.Empty<FaceCutter.SubTriangle>());
        Assert.Equal(0, adj.Count);
    }

    [Fact]
    public void Count_MatchesInput()
    {
        var subs = new[]
        {
            MakeSub(Vec3.Zero, Vec3.UnitX, Vec3.UnitY),
            MakeSub(Vec3.UnitX, Vec3.Zero, Vec3.UnitZ),
            MakeSub(new Vec3(5, 0, 0), new Vec3(6, 0, 0), new Vec3(5, 1, 0)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        Assert.Equal(3, adj.Count);
    }

    [Fact]
    public void SharedVertex_NotEnoughForAdjacency()
    {
        var subs = new[]
        {
            MakeSub(Vec3.Zero, Vec3.UnitX, Vec3.UnitY),
            MakeSub(Vec3.Zero, new Vec3(0, 0, 1), new Vec3(-1, 0, 0)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        Assert.Empty(adj.GetNeighbors(0));
        Assert.Empty(adj.GetNeighbors(1));
    }

    [Fact]
    public void IntersectionEdge_EitherSideSaysYes_IsTrue()
    {
        var subs = new[]
        {
            MakeSub(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, false, 0),
            MakeSub(Vec3.UnitX, Vec3.Zero, Vec3.UnitZ, 0, true, 1),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var n0 = adj.GetNeighbors(0);
        Assert.Contains(n0, n => n.IsIntersectionEdge);
    }

    [Fact]
    public void CustomTolerance_MergesNearVertices()
    {
        var subs = new[]
        {
            MakeSub(Vec3.Zero, Vec3.UnitX, Vec3.UnitY),
            MakeSub(new Vec3(1.0005, 0, 0), new Vec3(0.0005, 0, 0), Vec3.UnitZ),
        };
        var adjStrict = SubTriangleAdjacency.Build(subs, 1e-8);
        Assert.Empty(adjStrict.GetNeighbors(0));

        var adjLoose = SubTriangleAdjacency.Build(subs, 0.01);
        Assert.NotEmpty(adjLoose.GetNeighbors(0));
    }
}
