using MdCsg.Cutting;
using MdCsg.Math;
using MdCsg.Patches;

namespace MdCsg.Tests.Patches;

/// <summary>Phase 6: SubTriangleAdjacency — Build from sub-triangles, neighbor queries, intersection edge flags</summary>
public class SubTriangleAdjacencyPropertyTests
{
    [Fact]
    public void Build_TwoAdjacentTriangles_AreNeighbors()
    {
        var t0 = new FaceCutter.SubTriangle(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false);
        var t1 = new FaceCutter.SubTriangle(
            new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0), 0, false);
        var adj = SubTriangleAdjacency.Build(new[] { t0, t1 });

        Assert.Equal(2, adj.Count);
        var n0 = adj.GetNeighbors(0);
        Assert.True(n0.Any(n => n.Neighbor == 1));
        var n1 = adj.GetNeighbors(1);
        Assert.True(n1.Any(n => n.Neighbor == 0));
    }

    [Fact]
    public void Build_TwoDisjointTriangles_NotNeighbors()
    {
        var t0 = new FaceCutter.SubTriangle(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false);
        var t1 = new FaceCutter.SubTriangle(
            new Vec3(10, 10, 10), new Vec3(11, 10, 10), new Vec3(10, 11, 10), 1, false);
        var adj = SubTriangleAdjacency.Build(new[] { t0, t1 });

        Assert.Empty(adj.GetNeighbors(0));
        Assert.Empty(adj.GetNeighbors(1));
    }

    [Fact]
    public void Build_IntersectionEdge_FlaggedCorrectly()
    {
        var t0 = new FaceCutter.SubTriangle(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, true, 1);
        var t1 = new FaceCutter.SubTriangle(
            new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 0, 0), 0, true, 4);
        var adj = SubTriangleAdjacency.Build(new[] { t0, t1 });

        var n0 = adj.GetNeighbors(0);
        var neighborEntry = n0.FirstOrDefault(n => n.Neighbor == 1);
        Assert.True(neighborEntry.IsIntersectionEdge);
    }

    [Fact]
    public void Build_NonIntersectionSharedEdge_NotFlagged()
    {
        var t0 = new FaceCutter.SubTriangle(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false, 0);
        var t1 = new FaceCutter.SubTriangle(
            new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0), 0, false, 0);
        var adj = SubTriangleAdjacency.Build(new[] { t0, t1 });

        var n0 = adj.GetNeighbors(0);
        var neighborEntry = n0.FirstOrDefault(n => n.Neighbor == 1);
        Assert.False(neighborEntry.IsIntersectionEdge);
    }

    [Fact]
    public void Build_SingleTriangle_NoNeighbors()
    {
        var t0 = new FaceCutter.SubTriangle(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false);
        var adj = SubTriangleAdjacency.Build(new[] { t0 });

        Assert.Equal(1, adj.Count);
        Assert.Empty(adj.GetNeighbors(0));
    }

    [Fact]
    public void Build_Empty_ZeroCount()
    {
        var adj = SubTriangleAdjacency.Build(Array.Empty<FaceCutter.SubTriangle>());
        Assert.Equal(0, adj.Count);
    }

    [Fact]
    public void Build_ThreeTriangleFan_CorrectNeighborCounts()
    {
        var t0 = new FaceCutter.SubTriangle(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0.5, 1, 0), 0, false);
        var t1 = new FaceCutter.SubTriangle(
            new Vec3(0, 0, 0), new Vec3(0.5, 1, 0), new Vec3(-0.5, 1, 0), 0, false);
        var t2 = new FaceCutter.SubTriangle(
            new Vec3(0, 0, 0), new Vec3(-0.5, 1, 0), new Vec3(-1, 0, 0), 0, false);
        var adj = SubTriangleAdjacency.Build(new[] { t0, t1, t2 });

        Assert.Equal(3, adj.Count);
        Assert.True(adj.GetNeighbors(0).Any(n => n.Neighbor == 1));
        Assert.True(adj.GetNeighbors(1).Any(n => n.Neighbor == 2));
        Assert.False(adj.GetNeighbors(0).Any(n => n.Neighbor == 2));
    }

    [Fact]
    public void Build_Adjacency_IsSymmetric()
    {
        var t0 = new FaceCutter.SubTriangle(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false);
        var t1 = new FaceCutter.SubTriangle(
            new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0), 0, false);
        var t2 = new FaceCutter.SubTriangle(
            new Vec3(0, 0, 0), new Vec3(0, 1, 0), new Vec3(-1, 0, 0), 0, false);
        var adj = SubTriangleAdjacency.Build(new[] { t0, t1, t2 });

        for (int i = 0; i < adj.Count; i++)
        {
            foreach (var (neighbor, isIntEdge) in adj.GetNeighbors(i))
            {
                Assert.True(adj.GetNeighbors(neighbor).Any(n => n.Neighbor == i),
                    $"Adjacency should be symmetric: {i} -> {neighbor} but not reverse");
            }
        }
    }

    [Fact]
    public void Build_WithCustomTolerance_MergesNearbyVertices()
    {
        var t0 = new FaceCutter.SubTriangle(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false);
        var t1 = new FaceCutter.SubTriangle(
            new Vec3(1.0 + 1e-10, 0, 0), new Vec3(1, 1, 0), new Vec3(0 + 1e-10, 1, 0), 0, false);
        var adj = SubTriangleAdjacency.Build(new[] { t0, t1 }, 1e-6);

        Assert.True(adj.GetNeighbors(0).Any(n => n.Neighbor == 1));
    }

    [Fact]
    public void Build_EitherSideFlags_BothSeeIntersection()
    {
        // t0's shared edge with t1 is B-C (edge index 1), so flag = 1<<1 = 2
        var t0 = new FaceCutter.SubTriangle(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, true, 2);
        var t1 = new FaceCutter.SubTriangle(
            new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0), 0, false, 0);
        var adj = SubTriangleAdjacency.Build(new[] { t0, t1 });

        var n0 = adj.GetNeighbors(0).FirstOrDefault(n => n.Neighbor == 1);
        var n1 = adj.GetNeighbors(1).FirstOrDefault(n => n.Neighbor == 0);
        Assert.True(n0.IsIntersectionEdge);
        Assert.True(n1.IsIntersectionEdge);
    }

    [Fact]
    public void Build_FourTriangleQuad_HasCorrectTopology()
    {
        var t0 = new FaceCutter.SubTriangle(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0.5, 0.5, 0), 0, false);
        var t1 = new FaceCutter.SubTriangle(
            new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0.5, 0.5, 0), 0, false);
        var t2 = new FaceCutter.SubTriangle(
            new Vec3(1, 1, 0), new Vec3(0, 1, 0), new Vec3(0.5, 0.5, 0), 0, false);
        var t3 = new FaceCutter.SubTriangle(
            new Vec3(0, 1, 0), new Vec3(0, 0, 0), new Vec3(0.5, 0.5, 0), 0, false);
        var adj = SubTriangleAdjacency.Build(new[] { t0, t1, t2, t3 });

        Assert.Equal(4, adj.Count);
        for (int i = 0; i < 4; i++)
        {
            Assert.Equal(2, adj.GetNeighbors(i).Count);
        }
    }
}
