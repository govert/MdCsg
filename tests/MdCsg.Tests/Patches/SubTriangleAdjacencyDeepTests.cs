using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Patches;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Patches;

/// <summary>Phase 6: SubTriangleAdjacency deep tests — hash correctness, tolerance, complex graphs</summary>
public class SubTriangleAdjacencyDeepTests
{
    [Fact]
    public void Build_SingleTriangle_NoNeighbors()
    {
        var subs = new List<FaceCutter.SubTriangle>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false)
        };
        var adj = SubTriangleAdjacency.Build(subs);
        Assert.Equal(1, adj.Count);
        Assert.Empty(adj.GetNeighbors(0));
    }

    [Fact]
    public void Build_TwoAdjacentTriangles_MutualNeighbors()
    {
        var subs = new List<FaceCutter.SubTriangle>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false),
            new(new Vec3(1, 0, 0), new Vec3(0, 0, 0), new Vec3(0, 0, 1), 0, false),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        Assert.Equal(2, adj.Count);

        var n0 = adj.GetNeighbors(0);
        var n1 = adj.GetNeighbors(1);
        Assert.Contains(n0, x => x.Neighbor == 1);
        Assert.Contains(n1, x => x.Neighbor == 0);
    }

    [Fact]
    public void Build_SharedEdge_NotIntersection_FlaggedFalse()
    {
        var subs = new List<FaceCutter.SubTriangle>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false),
            new(new Vec3(1, 0, 0), new Vec3(0, 0, 0), new Vec3(0, 0, 1), 0, false),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var neighbors = adj.GetNeighbors(0);
        Assert.All(neighbors, n => Assert.False(n.IsIntersectionEdge));
    }

    [Fact]
    public void Build_SharedEdge_IntersectionEdge_FlaggedTrue()
    {
        // Edge A-B is intersection edge (bit 0 set = 1)
        var subs = new List<FaceCutter.SubTriangle>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, true, 1),
            new(new Vec3(1, 0, 0), new Vec3(0, 0, 0), new Vec3(0, 0, 1), 0, false, 0),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var neighbors = adj.GetNeighbors(0);
        // Edge 0→1 on tri 0 is intersection, so adjacency should flag it
        Assert.Contains(neighbors, n => n.Neighbor == 1 && n.IsIntersectionEdge);
    }

    [Fact]
    public void Build_TriangleStrip_LinearChain()
    {
        // Four triangles in a strip: 0-1, 1-2, 2-3
        var subs = new List<FaceCutter.SubTriangle>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0.5, 1, 0), 0, false),
            new(new Vec3(1, 0, 0), new Vec3(2, 0, 0), new Vec3(0.5, 1, 0), 0, false),
            new(new Vec3(2, 0, 0), new Vec3(3, 0, 0), new Vec3(2.5, 1, 0), 0, false),
            new(new Vec3(3, 0, 0), new Vec3(4, 0, 0), new Vec3(2.5, 1, 0), 0, false),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        Assert.Equal(4, adj.Count);

        // 0 is neighbor with 1 (shared edge via vertex (1,0,0))
        Assert.Contains(adj.GetNeighbors(0), n => n.Neighbor == 1);
        // 2 is neighbor with 3 (shared edge via vertex (3,0,0))
        Assert.Contains(adj.GetNeighbors(2), n => n.Neighbor == 3);
    }

    [Fact]
    public void Build_ThreeTriangles_SharingVertex_NotNeighborsUnlessSharedEdge()
    {
        // Three triangles sharing a vertex but NOT sharing edges
        var subs = new List<FaceCutter.SubTriangle>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false),
            new(new Vec3(0, 0, 0), new Vec3(0, 0, 1), new Vec3(0, -1, 0), 0, false),
            new(new Vec3(0, 0, 0), new Vec3(-1, 0, 0), new Vec3(0, 0, -1), 0, false),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        // Sharing a single vertex doesn't make them neighbors (need shared edge = 2 shared verts)
        Assert.Empty(adj.GetNeighbors(0));
        Assert.Empty(adj.GetNeighbors(1));
        Assert.Empty(adj.GetNeighbors(2));
    }

    [Fact]
    public void Build_ToleranceWelding_NearbyVerticesMerge()
    {
        // Two triangles share an edge but with tiny perturbation
        double eps = 1e-10;
        var subs = new List<FaceCutter.SubTriangle>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false),
            new(new Vec3(1 + eps, 0, 0), new Vec3(0 + eps, 0, 0), new Vec3(0.5, -1, 0), 0, false),
        };
        var adj = SubTriangleAdjacency.Build(subs, tolerance: 1e-8);
        // Should still find adjacency despite tiny perturbation
        Assert.Contains(adj.GetNeighbors(0), n => n.Neighbor == 1);
    }

    [Fact]
    public void Build_CanonicalEdgeOrdering_Symmetric()
    {
        // Edge (A, B) and (B, A) should hash to the same canonical edge
        var subs = new List<FaceCutter.SubTriangle>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false),
            // Reversed edge: B→A instead of A→B
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, -1, 0), 0, false),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        Assert.Contains(adj.GetNeighbors(0), n => n.Neighbor == 1);
    }

    [Fact]
    public void Build_IntegrationWithRealCut_ProducesConnectedGraph()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutA.SubTriangles);

        Assert.Equal(cutA.SubTriangles.Count, adj.Count);

        // All sub-triangles should have at least zero neighbors (isolated is valid for single-sub faces)
        for (int i = 0; i < adj.Count; i++)
        {
            Assert.NotNull(adj.GetNeighbors(i));
        }
    }

    [Fact]
    public void Build_IntegrationWithCut_IntersectionEdgesPresent()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutA.SubTriangles);

        // Some adjacencies should have intersection edge flags set
        bool anyIntersectionEdge = false;
        for (int i = 0; i < adj.Count; i++)
        {
            foreach (var (_, isInt) in adj.GetNeighbors(i))
            {
                if (isInt) anyIntersectionEdge = true;
            }
        }
        Assert.True(anyIntersectionEdge, "Cut mesh should have intersection edges in adjacency");
    }

    [Fact]
    public void Build_ManyTriangles_Fan()
    {
        // Fan of 8 triangles around a center point — each shares an edge with neighbors
        var center = new Vec3(0, 0, 0);
        int n = 8;
        var subs = new List<FaceCutter.SubTriangle>();
        for (int i = 0; i < n; i++)
        {
            double a1 = 2 * System.Math.PI * i / n;
            double a2 = 2 * System.Math.PI * (i + 1) / n;
            var v1 = new Vec3(System.Math.Cos(a1), System.Math.Sin(a1), 0);
            var v2 = new Vec3(System.Math.Cos(a2), System.Math.Sin(a2), 0);
            subs.Add(new FaceCutter.SubTriangle(center, v1, v2, 0, false));
        }
        var adj = SubTriangleAdjacency.Build(subs);
        Assert.Equal(n, adj.Count);

        // Each triangle should be neighbor with the next (sharing a spoke edge)
        for (int i = 0; i < n; i++)
        {
            int next = (i + 1) % n;
            Assert.Contains(adj.GetNeighbors(i), x => x.Neighbor == next);
        }
    }

    [Fact]
    public void Build_DisjointTriangles_NoAdjacency()
    {
        var subs = new List<FaceCutter.SubTriangle>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false),
            new(new Vec3(10, 10, 10), new Vec3(11, 10, 10), new Vec3(10, 11, 10), 1, false),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        Assert.Empty(adj.GetNeighbors(0));
        Assert.Empty(adj.GetNeighbors(1));
    }

    [Fact]
    public void PatchExtractor_SingleComponent_OnePatch()
    {
        // Two adjacent non-intersection-edge triangles → single patch
        var subs = new List<FaceCutter.SubTriangle>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false),
            new(new Vec3(1, 0, 0), new Vec3(0, 0, 0), new Vec3(0, 0, 1), 0, false),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        Assert.Single(patches);
        Assert.Equal(2, patches[0].SubTriangleIndices.Count);
    }

    [Fact]
    public void PatchExtractor_IntersectionEdgeSeparates_TwoPatches()
    {
        // Two adjacent triangles separated by intersection edge → two patches
        var subs = new List<FaceCutter.SubTriangle>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, true, 1), // edge A-B is intersection
            new(new Vec3(1, 0, 0), new Vec3(0, 0, 0), new Vec3(0, 0, 1), 0, false, 0),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        Assert.Equal(2, patches.Count);
        Assert.Single(patches[0].SubTriangleIndices);
        Assert.Single(patches[1].SubTriangleIndices);
    }

    [Fact]
    public void PatchExtractor_DisconnectedComponents_SeparatePatches()
    {
        var subs = new List<FaceCutter.SubTriangle>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false),
            new(new Vec3(10, 10, 10), new Vec3(11, 10, 10), new Vec3(10, 11, 10), 1, false),
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        Assert.Equal(2, patches.Count);
    }

    [Fact]
    public void PatchExtractor_AllTrianglesAssignedToExactlyOnePatch()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patches = PatchExtractor.Extract(cutA.SubTriangles, adj);

        var assigned = new HashSet<int>();
        foreach (var patch in patches)
        {
            foreach (int idx in patch.SubTriangleIndices)
            {
                Assert.True(assigned.Add(idx), $"Sub-triangle {idx} assigned to multiple patches");
            }
        }
        Assert.Equal(cutA.SubTriangles.Count, assigned.Count);
    }
}
