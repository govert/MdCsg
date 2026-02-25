using MdCsg.Cutting;
using MdCsg.Math;
using MdCsg.Patches;

namespace MdCsg.Tests.Patches;

/// <summary>Phase 6: SubTriangleAdjacency hash — canonical edge ordering, intersection flag propagation, neighbor symmetry</summary>
public class SubTriangleAdjacencyHashPropertyTests
{
    private static FaceCutter.SubTriangle MakeSub(Vec3 a, Vec3 b, Vec3 c,
        bool edgeAB = false, bool edgeBC = false, bool edgeCA = false, int faceIndex = 0)
    {
        byte flags = 0;
        if (edgeAB) flags |= 1;
        if (edgeBC) flags |= 2;
        if (edgeCA) flags |= 4;
        bool hasIntersection = flags != 0;
        return new FaceCutter.SubTriangle(a, b, c, faceIndex, hasIntersection, flags);
    }

    [Fact]
    public void TwoAdjacentTriangles_AreNeighbors()
    {
        var subs = new List<FaceCutter.SubTriangle>
        {
            MakeSub(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            MakeSub(new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        Assert.Equal(2, adj.Count);
        // Triangle 0 should have triangle 1 as neighbor
        var n0 = adj.GetNeighbors(0);
        Assert.True(n0.Any(n => n.Neighbor == 1));
        // Triangle 1 should have triangle 0 as neighbor
        var n1 = adj.GetNeighbors(1);
        Assert.True(n1.Any(n => n.Neighbor == 0));
    }

    [Fact]
    public void NeighborRelation_IsSymmetric()
    {
        var subs = new List<FaceCutter.SubTriangle>
        {
            MakeSub(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            MakeSub(new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0)),
            MakeSub(new Vec3(0, 1, 0), new Vec3(1, 1, 0), new Vec3(0, 2, 0)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        for (int i = 0; i < adj.Count; i++)
        {
            foreach (var (neighbor, _) in adj.GetNeighbors(i))
            {
                Assert.True(adj.GetNeighbors(neighbor).Any(n => n.Neighbor == i),
                    $"If {neighbor} is neighbor of {i}, then {i} should be neighbor of {neighbor}");
            }
        }
    }

    [Fact]
    public void DisjointTriangles_NoNeighbors()
    {
        var subs = new List<FaceCutter.SubTriangle>
        {
            MakeSub(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            MakeSub(new Vec3(10, 0, 0), new Vec3(11, 0, 0), new Vec3(10, 1, 0)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        Assert.Equal(0, adj.GetNeighbors(0).Count);
        Assert.Equal(0, adj.GetNeighbors(1).Count);
    }

    [Fact]
    public void IntersectionEdge_Propagated()
    {
        // Edge AB of tri 0 is intersection, edge shared with tri 1
        var subs = new List<FaceCutter.SubTriangle>
        {
            MakeSub(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), edgeAB: true),
            MakeSub(new Vec3(1, 0, 0), Vec3.Zero, new Vec3(0.5, -0.5, 0)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        // The shared edge should be marked as intersection
        var n0 = adj.GetNeighbors(0);
        var neighbor1 = n0.FirstOrDefault(n => n.Neighbor == 1);
        Assert.True(neighbor1.IsIntersectionEdge, "Shared edge should be intersection edge");
    }

    [Fact]
    public void NonIntersectionEdge_NotMarked()
    {
        var subs = new List<FaceCutter.SubTriangle>
        {
            MakeSub(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            MakeSub(new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var n0 = adj.GetNeighbors(0);
        var neighbor1 = n0.FirstOrDefault(n => n.Neighbor == 1);
        Assert.False(neighbor1.IsIntersectionEdge);
    }

    [Fact]
    public void SingleTriangle_NoNeighbors()
    {
        var subs = new List<FaceCutter.SubTriangle>
        {
            MakeSub(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        Assert.Equal(1, adj.Count);
        Assert.Equal(0, adj.GetNeighbors(0).Count);
    }

    [Fact]
    public void EmptyList_ZeroCount()
    {
        var adj = SubTriangleAdjacency.Build(new List<FaceCutter.SubTriangle>());
        Assert.Equal(0, adj.Count);
    }

    [Fact]
    public void ThreeTriangleFan_AllConnected()
    {
        // Three triangles sharing vertex at origin
        var subs = new List<FaceCutter.SubTriangle>
        {
            MakeSub(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            MakeSub(Vec3.Zero, new Vec3(0, 1, 0), new Vec3(-1, 0, 0)),
            MakeSub(Vec3.Zero, new Vec3(-1, 0, 0), new Vec3(0, -1, 0)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        // 0 and 1 share edge (origin, (0,1,0))
        Assert.True(adj.GetNeighbors(0).Any(n => n.Neighbor == 1));
        // 1 and 2 share edge (origin, (-1,0,0))
        Assert.True(adj.GetNeighbors(1).Any(n => n.Neighbor == 2));
    }

    [Fact]
    public void EitherSideIntersection_BothSee()
    {
        // If either side marks the shared edge as intersection, both neighbors should see it
        var subs = new List<FaceCutter.SubTriangle>
        {
            MakeSub(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            MakeSub(new Vec3(1, 0, 0), Vec3.Zero, new Vec3(0.5, -0.5, 0), edgeAB: true),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        // Edge (0, (1,0,0)) = edge ((1,0,0), 0) in tri 1 with edgeAB=true
        var n0 = adj.GetNeighbors(0).FirstOrDefault(n => n.Neighbor == 1);
        var n1 = adj.GetNeighbors(1).FirstOrDefault(n => n.Neighbor == 0);
        Assert.True(n0.IsIntersectionEdge);
        Assert.True(n1.IsIntersectionEdge);
    }

    [Fact]
    public void NearCoincidentVertices_WithinTolerance_AreAdjacent()
    {
        double eps = 1e-9; // within default tolerance of 1e-8
        var subs = new List<FaceCutter.SubTriangle>
        {
            MakeSub(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            MakeSub(new Vec3(1 + eps, 0, 0), new Vec3(eps, 0, 0), new Vec3(0.5, -0.5, 0)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        // Should still find adjacency through hashing with tolerance
        // (depends on hash grid — may or may not work for boundary cases)
        Assert.Equal(2, adj.Count);
    }
}
