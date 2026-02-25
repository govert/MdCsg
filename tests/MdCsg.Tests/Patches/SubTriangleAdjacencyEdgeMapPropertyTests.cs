using MdCsg.Cutting;
using MdCsg.Math;
using MdCsg.Patches;

namespace MdCsg.Tests.Patches;

/// <summary>Phase 6: SubTriangleAdjacency — edge map, neighbor discovery, intersection edge tagging, disconnected triangles</summary>
public class SubTriangleAdjacencyEdgeMapPropertyTests
{
    [Fact]
    public void Build_SingleTriangle_NoNeighbors()
    {
        var subTris = new[]
        {
            new FaceCutter.SubTriangle(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0), 0, false)
        };
        var adj = SubTriangleAdjacency.Build(subTris);
        Assert.Equal(1, adj.Count);
        Assert.Empty(adj.GetNeighbors(0));
    }

    [Fact]
    public void Build_TwoAdjacentTriangles_Bidirectional()
    {
        var subTris = new[]
        {
            new FaceCutter.SubTriangle(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0), 0, false),
            new FaceCutter.SubTriangle(new Vec3(1,0,0), new Vec3(1,1,0), new Vec3(0,1,0), 0, false)
        };
        var adj = SubTriangleAdjacency.Build(subTris);
        Assert.Equal(2, adj.Count);
        // Triangle 0 should neighbor triangle 1 (shared edge (1,0,0)-(0,1,0))
        var n0 = adj.GetNeighbors(0);
        Assert.True(n0.Count > 0, "Triangle 0 should have neighbors");
        bool found01 = false;
        foreach (var (neighbor, _) in n0) if (neighbor == 1) found01 = true;
        Assert.True(found01, "Triangle 0 should neighbor triangle 1");

        // And vice versa
        var n1 = adj.GetNeighbors(1);
        bool found10 = false;
        foreach (var (neighbor, _) in n1) if (neighbor == 0) found10 = true;
        Assert.True(found10, "Triangle 1 should neighbor triangle 0");
    }

    [Fact]
    public void Build_TwoDisjointTriangles_NoNeighbors()
    {
        var subTris = new[]
        {
            new FaceCutter.SubTriangle(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0), 0, false),
            new FaceCutter.SubTriangle(new Vec3(10,0,0), new Vec3(11,0,0), new Vec3(10,1,0), 1, false)
        };
        var adj = SubTriangleAdjacency.Build(subTris);
        Assert.Empty(adj.GetNeighbors(0));
        Assert.Empty(adj.GetNeighbors(1));
    }

    [Fact]
    public void Build_IntersectionEdge_Flagged()
    {
        // Triangle 0 has edge 0 (A-B) flagged as intersection
        var subTris = new[]
        {
            new FaceCutter.SubTriangle(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0), 0, true, 0b001),
            new FaceCutter.SubTriangle(new Vec3(1,0,0), new Vec3(0,0,0), new Vec3(0,-1,0), 0, false, 0)
        };
        var adj = SubTriangleAdjacency.Build(subTris);
        // Shared edge (0,0,0)-(1,0,0): tri 0 says it's intersection (bit 0), tri 1 doesn't
        // The edge should be flagged as intersection (OR of both sides)
        var neighbors0 = adj.GetNeighbors(0);
        bool foundIntersectionEdge = false;
        foreach (var (neighbor, isInt) in neighbors0)
        {
            if (neighbor == 1 && isInt) foundIntersectionEdge = true;
        }
        Assert.True(foundIntersectionEdge, "Shared edge should be flagged as intersection");
    }

    [Fact]
    public void Build_ThreeTriangleStrip_CorrectNeighborCounts()
    {
        // Three triangles in a strip: 0-1, 1-2 adjacent, 0-2 not adjacent
        var subTris = new[]
        {
            new FaceCutter.SubTriangle(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0), 0, false),
            new FaceCutter.SubTriangle(new Vec3(1,0,0), new Vec3(1,1,0), new Vec3(0,1,0), 0, false),
            new FaceCutter.SubTriangle(new Vec3(1,1,0), new Vec3(0,2,0), new Vec3(0,1,0), 0, false)
        };
        var adj = SubTriangleAdjacency.Build(subTris);
        // Triangle 0 neighbors: 1 (shared edge)
        // Triangle 1 neighbors: 0 and 2 (two shared edges)
        // Triangle 2 neighbors: 1 (shared edge)
        Assert.True(adj.GetNeighbors(1).Count >= 2, "Middle triangle should have at least 2 neighbors");
    }

    [Fact]
    public void Build_EmptyInput_EmptyResult()
    {
        var adj = SubTriangleAdjacency.Build(Array.Empty<FaceCutter.SubTriangle>());
        Assert.Equal(0, adj.Count);
    }

    [Fact]
    public void Build_SharedEdgeNoIntersection_FlaggedFalse()
    {
        var subTris = new[]
        {
            new FaceCutter.SubTriangle(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0), 0, false, 0),
            new FaceCutter.SubTriangle(new Vec3(1,0,0), new Vec3(1,1,0), new Vec3(0,1,0), 0, false, 0)
        };
        var adj = SubTriangleAdjacency.Build(subTris);
        foreach (var (neighbor, isInt) in adj.GetNeighbors(0))
        {
            Assert.False(isInt, "Non-intersection edges should not be flagged");
        }
    }

    [Fact]
    public void Build_Count_MatchesInput()
    {
        var subTris = new FaceCutter.SubTriangle[5];
        for (int i = 0; i < 5; i++)
        {
            subTris[i] = new FaceCutter.SubTriangle(
                new Vec3(i * 10, 0, 0), new Vec3(i * 10 + 1, 0, 0), new Vec3(i * 10, 1, 0), i, false);
        }
        var adj = SubTriangleAdjacency.Build(subTris);
        Assert.Equal(5, adj.Count);
    }

    [Fact]
    public void Build_NeighborIndicesInRange()
    {
        var subTris = new[]
        {
            new FaceCutter.SubTriangle(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0), 0, false),
            new FaceCutter.SubTriangle(new Vec3(1,0,0), new Vec3(1,1,0), new Vec3(0,1,0), 0, false),
            new FaceCutter.SubTriangle(new Vec3(1,1,0), new Vec3(0,2,0), new Vec3(0,1,0), 0, false)
        };
        var adj = SubTriangleAdjacency.Build(subTris);
        for (int i = 0; i < adj.Count; i++)
        {
            foreach (var (neighbor, _) in adj.GetNeighbors(i))
            {
                Assert.True(neighbor >= 0 && neighbor < adj.Count,
                    $"Neighbor {neighbor} out of range for triangle {i}");
            }
        }
    }

    [Fact]
    public void Build_NoSelfNeighbors()
    {
        var subTris = new[]
        {
            new FaceCutter.SubTriangle(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0), 0, false),
            new FaceCutter.SubTriangle(new Vec3(1,0,0), new Vec3(1,1,0), new Vec3(0,1,0), 0, false)
        };
        var adj = SubTriangleAdjacency.Build(subTris);
        for (int i = 0; i < adj.Count; i++)
        {
            foreach (var (neighbor, _) in adj.GetNeighbors(i))
            {
                Assert.NotEqual(i, neighbor);
            }
        }
    }
}
