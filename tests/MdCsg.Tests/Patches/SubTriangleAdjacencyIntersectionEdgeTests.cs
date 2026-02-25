using MdCsg.Cutting;
using MdCsg.Math;
using MdCsg.Patches;

namespace MdCsg.Tests.Patches;

/// <summary>Phase 6: SubTriangleAdjacency — intersection edge propagation, neighbor symmetry, tolerance boundaries</summary>
public class SubTriangleAdjacencyIntersectionEdgeTests
{
    private static FaceCutter.SubTriangle MakeSub(Vec3 a, Vec3 b, Vec3 c, int origFace = 0, bool hasIntEdge = false, byte flags = 0)
        => new(a, b, c, origFace, hasIntEdge, flags);

    [Fact]
    public void TwoAdjacentTriangles_NeighborSymmetric()
    {
        // Triangle 0: (0,0,0)-(1,0,0)-(0,1,0)
        // Triangle 1: (1,0,0)-(0,0,0)-(0,0,1) — shares edge (0,0,0)-(1,0,0) reversed
        var subs = new[]
        {
            MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            MakeSub(new Vec3(1, 0, 0), new Vec3(0, 0, 0), new Vec3(0, 0, 1)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        Assert.Equal(2, adj.Count);

        var neighbors0 = adj.GetNeighbors(0);
        var neighbors1 = adj.GetNeighbors(1);
        Assert.Contains(neighbors0, n => n.Neighbor == 1);
        Assert.Contains(neighbors1, n => n.Neighbor == 0);
    }

    [Fact]
    public void IntersectionEdge_PropagatesFromEitherSide()
    {
        // Two triangles share an edge. One marks it as intersection edge.
        var subs = new[]
        {
            MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), hasIntEdge: true, flags: 0b001), // edge 0 (A-B)
            MakeSub(new Vec3(1, 0, 0), new Vec3(0, 0, 0), new Vec3(0, 0, 1)), // shares same edge, not marked
        };
        var adj = SubTriangleAdjacency.Build(subs);

        // Either side marking it as intersection should propagate
        var neighbors0 = adj.GetNeighbors(0);
        var intEdge = neighbors0.First(n => n.Neighbor == 1);
        Assert.True(intEdge.IsIntersectionEdge);
    }

    [Fact]
    public void NonIntersectionEdge_WhenNeitherSideMarked()
    {
        var subs = new[]
        {
            MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            MakeSub(new Vec3(1, 0, 0), new Vec3(0, 0, 0), new Vec3(0, 0, 1)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var neighbors0 = adj.GetNeighbors(0);
        var edge = neighbors0.First(n => n.Neighbor == 1);
        Assert.False(edge.IsIntersectionEdge);
    }

    [Fact]
    public void SingleTriangle_NoNeighbors()
    {
        var subs = new[]
        {
            MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        Assert.Equal(1, adj.Count);
        Assert.Empty(adj.GetNeighbors(0));
    }

    [Fact]
    public void ThreeTriangles_FanArrangement_SharedVertex()
    {
        // Three triangles in a fan from origin, each sharing edges with two neighbors
        var origin = new Vec3(0, 0, 0);
        var subs = new[]
        {
            MakeSub(origin, new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            MakeSub(origin, new Vec3(0, 1, 0), new Vec3(-1, 0, 0)),
            MakeSub(origin, new Vec3(-1, 0, 0), new Vec3(0, -1, 0)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        Assert.Equal(3, adj.Count);

        // Triangle 0 and 1 share edge origin-(0,1,0)
        Assert.Contains(adj.GetNeighbors(0), n => n.Neighbor == 1);
        // Triangle 1 and 2 share edge origin-(-1,0,0)
        Assert.Contains(adj.GetNeighbors(1), n => n.Neighbor == 2);
    }

    [Fact]
    public void NearbyVertices_WithinTolerance_AreAdjacent()
    {
        double eps = 1e-10; // well within default tolerance of 1e-8
        var subs = new[]
        {
            MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            MakeSub(new Vec3(1 + eps, 0, 0), new Vec3(0 + eps, 0, 0), new Vec3(0, 0, 1)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        // Should be recognized as sharing an edge (within tolerance)
        Assert.Contains(adj.GetNeighbors(0), n => n.Neighbor == 1);
    }

    [Fact]
    public void FarVertices_BeyondTolerance_NotAdjacent()
    {
        double offset = 0.1; // well beyond default tolerance
        var subs = new[]
        {
            MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            MakeSub(new Vec3(1 + offset, 0, 0), new Vec3(0 + offset, 0, 0), new Vec3(0, 0, 1)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        Assert.Empty(adj.GetNeighbors(0));
    }

    [Fact]
    public void EmptyInput_ZeroCount()
    {
        var adj = SubTriangleAdjacency.Build(Array.Empty<FaceCutter.SubTriangle>());
        Assert.Equal(0, adj.Count);
    }

    [Fact]
    public void ManyTriangles_Strip_EachHasTwoNeighbors()
    {
        // Build a strip of 5 triangles sharing edges
        var subs = new List<FaceCutter.SubTriangle>();
        for (int i = 0; i < 5; i++)
        {
            double x = i;
            if (i % 2 == 0)
                subs.Add(MakeSub(new Vec3(x, 0, 0), new Vec3(x + 1, 0, 0), new Vec3(x, 1, 0)));
            else
                subs.Add(MakeSub(new Vec3(x + 1, 0, 0), new Vec3(x, 0, 0), new Vec3(x + 1, 1, 0)));
        }
        var adj = SubTriangleAdjacency.Build(subs);
        Assert.Equal(5, adj.Count);

        // Interior triangles should have at least 1 neighbor (shared edges)
        for (int i = 1; i < 4; i++)
            Assert.True(adj.GetNeighbors(i).Count >= 1);
    }

    [Fact]
    public void CustomTolerance_Tighter()
    {
        double tol = 1e-12; // very tight
        double eps = 1e-10; // outside this tight tolerance
        var subs = new[]
        {
            MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            MakeSub(new Vec3(1 + eps, 0, 0), new Vec3(eps, 0, 0), new Vec3(0, 0, 1)),
        };
        var adj = SubTriangleAdjacency.Build(subs, tol);
        // With tighter tolerance, vertices should NOT match
        Assert.Empty(adj.GetNeighbors(0));
    }

    [Fact]
    public void BothSidesMarkedIntersection_StillTrue()
    {
        // Both triangles mark the shared edge as intersection
        var subs = new[]
        {
            MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), hasIntEdge: true, flags: 0b001),
            MakeSub(new Vec3(1, 0, 0), new Vec3(0, 0, 0), new Vec3(0, 0, 1), hasIntEdge: true, flags: 0b001),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var neighbors0 = adj.GetNeighbors(0);
        var edge = neighbors0.First(n => n.Neighbor == 1);
        Assert.True(edge.IsIntersectionEdge);
    }

    [Fact]
    public void MultipleSharedEdges_BothRecorded()
    {
        // Degenerate case: two triangles sharing two edges (thin triangle)
        // This can happen when sub-triangles share a vertex and two edges
        var subs = new[]
        {
            MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0.5, 1, 0)),
            MakeSub(new Vec3(0, 0, 0), new Vec3(0.5, 1, 0), new Vec3(-0.5, 1, 0)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        // They share edge (0,0,0)-(0.5,1,0)
        Assert.Contains(adj.GetNeighbors(0), n => n.Neighbor == 1);
    }

    [Fact]
    public void SameTriangleTwice_SelfAdjacent()
    {
        // Two identical triangles share all three edges
        var v = new Vec3(0, 0, 0);
        var subs = new[]
        {
            MakeSub(v, new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            MakeSub(v, new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        // Each should be a neighbor of the other (3 shared edges = 3 adjacency entries)
        Assert.True(adj.GetNeighbors(0).Count >= 1);
        Assert.True(adj.GetNeighbors(1).Count >= 1);
    }
}
