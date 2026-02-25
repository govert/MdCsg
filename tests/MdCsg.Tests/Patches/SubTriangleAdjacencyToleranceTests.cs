using MdCsg.Cutting;
using MdCsg.Math;
using MdCsg.Patches;

namespace MdCsg.Tests.Patches;

/// <summary>Phase 6: SubTriangleAdjacency tolerance and flag propagation edge cases</summary>
public class SubTriangleAdjacencyToleranceTests
{
    private static FaceCutter.SubTriangle MakeSub(Vec3 a, Vec3 b, Vec3 c, int origFace = 0, byte edgeFlags = 0)
    {
        return new FaceCutter.SubTriangle(a, b, c, origFace, edgeFlags != 0, edgeFlags);
    }

    [Fact]
    public void SingleTriangle_NoNeighbors()
    {
        var subs = new[]
        {
            MakeSub(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0))
        };
        var adj = SubTriangleAdjacency.Build(subs);
        Assert.Equal(1, adj.Count);
        Assert.Equal(0, adj.GetNeighbors(0).Count);
    }

    [Fact]
    public void TwoTriangles_SharedEdge_AreNeighbors()
    {
        var subs = new[]
        {
            MakeSub(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0)),
            MakeSub(new Vec3(1,0,0), new Vec3(0,0,0), new Vec3(0,0,1)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        Assert.True(adj.GetNeighbors(0).Count > 0);
        Assert.True(adj.GetNeighbors(1).Count > 0);
    }

    [Fact]
    public void SharedEdge_IntersectionFlagPropagates_FromOneSide()
    {
        // Only triangle 0 has its AB edge flagged as intersection
        var subs = new[]
        {
            MakeSub(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0), 0, 0x01),
            MakeSub(new Vec3(1,0,0), new Vec3(0,0,0), new Vec3(0,0,1), 0, 0),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var neighbors0 = adj.GetNeighbors(0);
        Assert.True(neighbors0.Count > 0);
        Assert.True(neighbors0[0].IsIntersectionEdge,
            "Intersection flag should propagate when either side is flagged");
    }

    [Fact]
    public void SharedEdge_NotIntersection_WhenNeitherFlagged()
    {
        var subs = new[]
        {
            MakeSub(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0), 0, 0),
            MakeSub(new Vec3(1,0,0), new Vec3(0,0,0), new Vec3(0,0,1), 0, 0),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        Assert.False(adj.GetNeighbors(0)[0].IsIntersectionEdge);
    }

    [Fact]
    public void CustomTolerance_MergesNearbyVertices()
    {
        var subs = new[]
        {
            MakeSub(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0)),
            MakeSub(new Vec3(1.0005,0,0), new Vec3(0.0005,0,0), new Vec3(0,0,1)),
        };
        var adj = SubTriangleAdjacency.Build(subs, tolerance: 0.01);
        Assert.True(adj.GetNeighbors(0).Count > 0,
            "With tolerance 0.01, offset 0.0005 should merge");
    }

    [Fact]
    public void SmallTolerance_DoesNotMerge()
    {
        var subs = new[]
        {
            MakeSub(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0)),
            MakeSub(new Vec3(1.01,0,0), new Vec3(0.01,0,0), new Vec3(0,0,1)),
        };
        var adj = SubTriangleAdjacency.Build(subs, tolerance: 0.001);
        Assert.Equal(0, adj.GetNeighbors(0).Count);
    }

    [Fact]
    public void AdjacencyIsSymmetric()
    {
        var subs = new[]
        {
            MakeSub(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0)),
            MakeSub(new Vec3(1,0,0), new Vec3(0,0,0), new Vec3(0,0,1)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        bool has01 = false, has10 = false;
        foreach (var n in adj.GetNeighbors(0))
            if (n.Neighbor == 1) has01 = true;
        foreach (var n in adj.GetNeighbors(1))
            if (n.Neighbor == 0) has10 = true;
        Assert.True(has01 && has10);
    }

    [Fact]
    public void ThreeTriangles_Fan_EachHas2Neighbors()
    {
        var subs = new[]
        {
            MakeSub(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0)),
            MakeSub(new Vec3(0,0,0), new Vec3(0,1,0), new Vec3(0,0,1)),
            MakeSub(new Vec3(0,0,0), new Vec3(0,0,1), new Vec3(1,0,0)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        for (int i = 0; i < 3; i++)
            Assert.Equal(2, adj.GetNeighbors(i).Count);
    }

    [Fact]
    public void EmptyInput_CountZero()
    {
        var adj = SubTriangleAdjacency.Build(Array.Empty<FaceCutter.SubTriangle>());
        Assert.Equal(0, adj.Count);
    }

    [Fact]
    public void Count_MatchesInput()
    {
        var subs = new FaceCutter.SubTriangle[7];
        for (int i = 0; i < 7; i++)
        {
            var offset = new Vec3(i * 10, 0, 0);
            subs[i] = MakeSub(offset, offset + new Vec3(1, 0, 0), offset + new Vec3(0, 1, 0));
        }
        var adj = SubTriangleAdjacency.Build(subs);
        Assert.Equal(7, adj.Count);
    }

    [Fact]
    public void NeighborIndices_AreInRange()
    {
        var subs = new[]
        {
            MakeSub(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0)),
            MakeSub(new Vec3(1,0,0), new Vec3(0,0,0), new Vec3(0,0,1)),
            MakeSub(new Vec3(0,0,0), new Vec3(0,0,1), new Vec3(0,1,0)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        for (int i = 0; i < adj.Count; i++)
            foreach (var n in adj.GetNeighbors(i))
            {
                Assert.True(n.Neighbor >= 0 && n.Neighbor < adj.Count);
                Assert.NotEqual(i, n.Neighbor);
            }
    }

    [Fact]
    public void DisjointTriangles_NoNeighbors()
    {
        var subs = new[]
        {
            MakeSub(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0)),
            MakeSub(new Vec3(100,100,100), new Vec3(101,100,100), new Vec3(100,101,100)),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        Assert.Equal(0, adj.GetNeighbors(0).Count);
        Assert.Equal(0, adj.GetNeighbors(1).Count);
    }
}
