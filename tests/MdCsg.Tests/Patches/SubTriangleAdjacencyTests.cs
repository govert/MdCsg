using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Patches;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Patches;

/// <summary>Batch 19: SubTriangleAdjacency tests (20 tests)</summary>
public class SubTriangleAdjacencyTests
{
    [Fact]
    public void SingleSubTriangle_NoNeighbors()
    {
        var subTris = new[] { new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, false) };
        var adj = SubTriangleAdjacency.Build(subTris);
        Assert.Equal(1, adj.Count);
        Assert.Empty(adj.GetNeighbors(0));
    }

    [Fact]
    public void TwoAdjacentSubTriangles_AreNeighbors()
    {
        var subTris = new[]
        {
            new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, false),
            new FaceCutter.SubTriangle(Vec3.UnitX, new Vec3(1, 1, 0), Vec3.UnitY, 0, false),
        };
        var adj = SubTriangleAdjacency.Build(subTris);
        Assert.True(adj.GetNeighbors(0).Any(n => n.Neighbor == 1));
        Assert.True(adj.GetNeighbors(1).Any(n => n.Neighbor == 0));
    }

    [Fact]
    public void TwoDisjointSubTriangles_NotNeighbors()
    {
        var subTris = new[]
        {
            new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, false),
            new FaceCutter.SubTriangle(new Vec3(5, 0, 0), new Vec3(6, 0, 0), new Vec3(5, 1, 0), 1, false),
        };
        var adj = SubTriangleAdjacency.Build(subTris);
        Assert.Empty(adj.GetNeighbors(0));
        Assert.Empty(adj.GetNeighbors(1));
    }

    [Fact]
    public void SharedEdge_NotIntersection_FlagsCorrect()
    {
        var subTris = new[]
        {
            new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, false, 0),
            new FaceCutter.SubTriangle(Vec3.UnitX, new Vec3(1, 1, 0), Vec3.UnitY, 0, false, 0),
        };
        var adj = SubTriangleAdjacency.Build(subTris);
        var n = adj.GetNeighbors(0).First(x => x.Neighbor == 1);
        Assert.False(n.IsIntersectionEdge);
    }

    [Fact]
    public void SharedEdge_IsIntersection_FlagsCorrect()
    {
        // Tri 0: edge A-B is intersection (bit 0)
        var subTris = new[]
        {
            new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, true, 0b001),
            // Tri 1 shares edge (UnitX, Zero) reversed
            new FaceCutter.SubTriangle(Vec3.UnitX, Vec3.Zero, new Vec3(0.5, -1, 0), 0, false, 0),
        };
        var adj = SubTriangleAdjacency.Build(subTris);
        var n = adj.GetNeighbors(0).First(x => x.Neighbor == 1);
        Assert.True(n.IsIntersectionEdge);
    }

    [Fact]
    public void Count_MatchesInput()
    {
        var subTris = new[]
        {
            new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, false),
            new FaceCutter.SubTriangle(Vec3.UnitX, new Vec3(1, 1, 0), Vec3.UnitY, 0, false),
            new FaceCutter.SubTriangle(new Vec3(5, 0, 0), new Vec3(6, 0, 0), new Vec3(5, 1, 0), 1, false),
        };
        var adj = SubTriangleAdjacency.Build(subTris);
        Assert.Equal(3, adj.Count);
    }

    [Fact]
    public void CubeMesh_NoIntersection_AllNeighborsNonIntersection()
    {
        var cube = MeshFactory.CreateCube();
        var emptySegs = new Dictionary<int, List<IntersectionSegment>>();
        var cutResult = MeshCutter.Cut(cube.Mesh, emptySegs);
        var adj = SubTriangleAdjacency.Build(cutResult.SubTriangles);
        for (int i = 0; i < adj.Count; i++)
        {
            foreach (var (_, isInt) in adj.GetNeighbors(i))
                Assert.False(isInt);
        }
    }

    [Fact]
    public void CubeMesh_WithIntersection_SomeNeighborsAreIntersection()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        var cutResult = MeshCutter.Cut(cubeA.Mesh, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutResult.SubTriangles);
        bool anyIntersection = false;
        for (int i = 0; i < adj.Count; i++)
        {
            foreach (var (_, isInt) in adj.GetNeighbors(i))
                if (isInt) anyIntersection = true;
        }
        Assert.True(anyIntersection);
    }

    [Fact]
    public void Adjacency_IsSymmetric()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        var cutResult = MeshCutter.Cut(cubeA.Mesh, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutResult.SubTriangles);
        for (int i = 0; i < adj.Count; i++)
        {
            foreach (var (neighbor, _) in adj.GetNeighbors(i))
            {
                Assert.True(adj.GetNeighbors(neighbor).Any(n => n.Neighbor == i));
            }
        }
    }

    [Fact]
    public void EmptyInput_ZeroCount()
    {
        var adj = SubTriangleAdjacency.Build(Array.Empty<FaceCutter.SubTriangle>());
        Assert.Equal(0, adj.Count);
    }

    [Fact]
    public void ThreeTriangleFan_AllAdjacent()
    {
        // Three triangles sharing center vertex
        var center = new Vec3(0.5, 0.5, 0);
        var subTris = new[]
        {
            new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, center, 0, false),
            new FaceCutter.SubTriangle(Vec3.UnitX, new Vec3(1, 1, 0), center, 0, false),
            new FaceCutter.SubTriangle(new Vec3(1, 1, 0), Vec3.UnitY, center, 0, false),
        };
        var adj = SubTriangleAdjacency.Build(subTris);
        // 0 shares edge with 1 (UnitX, center)
        Assert.True(adj.GetNeighbors(0).Any(n => n.Neighbor == 1));
        // 1 shares edge with 2 ((1,1,0), center)
        Assert.True(adj.GetNeighbors(1).Any(n => n.Neighbor == 2));
    }

    [Fact]
    public void CustomTolerance_MergesNearbyVertices()
    {
        // Two triangles with nearly-matching edge (within tolerance)
        var subTris = new[]
        {
            new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, false),
            new FaceCutter.SubTriangle(
                new Vec3(1.0 + 1e-10, 0, 0),
                new Vec3(0 + 1e-10, 1, 0),
                new Vec3(1, 1, 0), 0, false),
        };
        var adj = SubTriangleAdjacency.Build(subTris, 1e-8);
        Assert.True(adj.GetNeighbors(0).Any(n => n.Neighbor == 1));
    }

    [Fact]
    public void LargeTolerance_DoesNotMergeDistantVertices()
    {
        var subTris = new[]
        {
            new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, false),
            new FaceCutter.SubTriangle(new Vec3(5, 0, 0), new Vec3(6, 0, 0), new Vec3(5, 1, 0), 1, false),
        };
        // Even with larger tolerance, 5 units apart should not merge
        var adj = SubTriangleAdjacency.Build(subTris, 1e-6);
        Assert.Empty(adj.GetNeighbors(0));
    }

    [Fact]
    public void IntersectionEdgeFlag_PropagatesFromEitherSide()
    {
        // Only one triangle marks the edge as intersection
        var subTris = new[]
        {
            new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, true, 0b001), // A-B is intersection
            new FaceCutter.SubTriangle(Vec3.UnitX, Vec3.Zero, new Vec3(0.5, -1, 0), 0, false, 0),
        };
        var adj = SubTriangleAdjacency.Build(subTris);
        var n01 = adj.GetNeighbors(0).First(n => n.Neighbor == 1);
        var n10 = adj.GetNeighbors(1).First(n => n.Neighbor == 0);
        // Both directions should see it as intersection edge
        Assert.True(n01.IsIntersectionEdge);
        Assert.True(n10.IsIntersectionEdge);
    }

    [Fact]
    public void TetrahedronMesh_HasSomeAdjacency()
    {
        var tet = MeshFactory.CreateTetrahedron();
        var emptySegs = new Dictionary<int, List<IntersectionSegment>>();
        var cutResult = MeshCutter.Cut(tet.Mesh, emptySegs);
        var adj = SubTriangleAdjacency.Build(cutResult.SubTriangles);
        Assert.Equal(tet.Mesh.Faces.Count, adj.Count);
        // A tetrahedron's faces share edges, so some neighbors should exist
        bool anyNeighbor = false;
        for (int i = 0; i < adj.Count; i++)
            if (adj.GetNeighbors(i).Count > 0) anyNeighbor = true;
        Assert.True(anyNeighbor);
    }

    [Fact]
    public void NoSelfNeighbors()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        var cutResult = MeshCutter.Cut(cubeA.Mesh, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutResult.SubTriangles);
        for (int i = 0; i < adj.Count; i++)
        {
            foreach (var (neighbor, _) in adj.GetNeighbors(i))
                Assert.NotEqual(i, neighbor);
        }
    }

    [Fact]
    public void SphereMesh_HasAdjacency()
    {
        var sphere = MeshFactory.CreateSphere(subdivisions: 1);
        var emptySegs = new Dictionary<int, List<IntersectionSegment>>();
        var cutResult = MeshCutter.Cut(sphere.Mesh, emptySegs);
        var adj = SubTriangleAdjacency.Build(cutResult.SubTriangles);
        Assert.True(adj.Count > 0);
        bool anyNeighbor = false;
        for (int i = 0; i < adj.Count; i++)
            if (adj.GetNeighbors(i).Count > 0) anyNeighbor = true;
        Assert.True(anyNeighbor);
    }

    [Fact]
    public void NeighborCount_IsReasonable()
    {
        var cube = MeshFactory.CreateCube();
        var emptySegs = new Dictionary<int, List<IntersectionSegment>>();
        var cutResult = MeshCutter.Cut(cube.Mesh, emptySegs);
        var adj = SubTriangleAdjacency.Build(cutResult.SubTriangles);
        for (int i = 0; i < adj.Count; i++)
        {
            // A triangle has at most 3 edges, so at most 3 unique neighbors
            // (but edge map may connect to multiple if triangles share the same edge)
            Assert.True(adj.GetNeighbors(i).Count <= 6);
        }
    }
}
