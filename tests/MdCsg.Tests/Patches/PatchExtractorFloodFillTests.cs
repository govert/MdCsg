using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Patches;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Patches;

/// <summary>Phase 6: PatchExtractor flood-fill — BFS correctness, intersection edge splitting, disconnected regions</summary>
public class PatchExtractorFloodFillTests
{
    // Helper to create sub-triangles
    private static FaceCutter.SubTriangle MakeSub(int i, bool hasIntersection = false, byte flags = 0)
    {
        return new FaceCutter.SubTriangle(
            new Vec3(i, 0, 0), new Vec3(i + 1, 0, 0), new Vec3(i, 1, 0),
            0, hasIntersection, flags);
    }

    // Helper to build a simple manual adjacency via Build
    private static SubTriangleAdjacency BuildAdjacencyFromSubs(IReadOnlyList<FaceCutter.SubTriangle> subs)
    {
        return SubTriangleAdjacency.Build(subs);
    }

    [Fact]
    public void SingleSubTriangle_SinglePatch()
    {
        var subs = new List<FaceCutter.SubTriangle> { MakeSub(0) };
        var adj = BuildAdjacencyFromSubs(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        Assert.Equal(1, patches.Count);
        Assert.Equal(1, patches[0].SubTriangleIndices.Count);
        Assert.Equal(0, patches[0].SubTriangleIndices[0]);
    }

    [Fact]
    public void PatchIds_Sequential()
    {
        // Each sub-triangle is isolated (no shared edges)
        var subs = new List<FaceCutter.SubTriangle>();
        for (int i = 0; i < 5; i++)
        {
            subs.Add(new FaceCutter.SubTriangle(
                new Vec3(i * 10, 0, 0), new Vec3(i * 10 + 1, 0, 0), new Vec3(i * 10, 1, 0),
                0, false, 0));
        }
        var adj = BuildAdjacencyFromSubs(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        for (int i = 0; i < patches.Count; i++)
            Assert.Equal(i, patches[i].Id);
    }

    [Fact]
    public void TwoConnected_NoIntersection_OnePatch()
    {
        // Two triangles sharing an edge, no intersection flags
        var a = Vec3.Zero;
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(1, 1, 0);
        var subs = new List<FaceCutter.SubTriangle>
        {
            new(a, b, c, 0, false, 0),
            new(b, d, c, 0, false, 0),
        };
        var adj = BuildAdjacencyFromSubs(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        // Both in same patch since shared edge BC is not intersection
        Assert.Equal(1, patches.Count);
        Assert.Equal(2, patches[0].SubTriangleIndices.Count);
    }

    [Fact]
    public void TwoConnected_WithIntersectionEdge_TwoPatches()
    {
        // Two triangles sharing edge BC, which is an intersection edge
        var a = Vec3.Zero;
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(1, 1, 0);
        var subs = new List<FaceCutter.SubTriangle>
        {
            new(a, b, c, 0, true, 0x02), // BC is intersection (bit 1)
            new(b, d, c, 0, true, 0x04), // CA is intersection (bit 2 = edge index 2 of second tri, sharing BC with first)
        };
        var adj = BuildAdjacencyFromSubs(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        // Should split into 2 patches because the shared edge is flagged as intersection
        Assert.Equal(2, patches.Count);
    }

    [Fact]
    public void AllSubTriangles_CoveredByPatches()
    {
        var subs = new List<FaceCutter.SubTriangle>();
        for (int i = 0; i < 10; i++)
            subs.Add(new FaceCutter.SubTriangle(
                new Vec3(i * 10, 0, 0), new Vec3(i * 10 + 1, 0, 0), new Vec3(i * 10, 1, 0),
                0, false, 0));
        var adj = BuildAdjacencyFromSubs(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        var covered = new HashSet<int>();
        foreach (var p in patches)
            foreach (int idx in p.SubTriangleIndices)
                covered.Add(idx);
        Assert.Equal(subs.Count, covered.Count);
    }

    [Fact]
    public void NoSubTriangle_InTwoPatches()
    {
        var subs = new List<FaceCutter.SubTriangle>();
        for (int i = 0; i < 8; i++)
        {
            subs.Add(new FaceCutter.SubTriangle(
                new Vec3(i * 10, 0, 0), new Vec3(i * 10 + 1, 0, 0), new Vec3(i * 10, 1, 0),
                0, i == 3, (byte)(i == 3 ? 0x01 : 0)));
        }
        var adj = BuildAdjacencyFromSubs(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        var allIndices = new List<int>();
        foreach (var p in patches)
            allIndices.AddRange(p.SubTriangleIndices);
        Assert.Equal(allIndices.Count, allIndices.Distinct().Count()); // No duplicates
    }

    [Fact]
    public void CubeMesh_NoCuts_SinglePatch()
    {
        // A cube mesh with no intersection creates a single patch per connected component
        var mesh = MeshFactory.CreateCube().Mesh;
        var cutResult = MeshCutter.Cut(mesh, new Dictionary<int, List<MdCsg.Intersection.IntersectionSegment>>());
        var adj = SubTriangleAdjacency.Build(cutResult.SubTriangles);
        var patches = PatchExtractor.Extract(cutResult.SubTriangles, adj);
        // All 12 triangles connected through shared edges = 1 patch
        Assert.Equal(1, patches.Count);
        Assert.Equal(12, patches[0].SubTriangleIndices.Count);
    }

    [Fact]
    public void Patch_SubTriangleIndices_InBounds()
    {
        var subs = new List<FaceCutter.SubTriangle>();
        for (int i = 0; i < 20; i++)
        {
            subs.Add(new FaceCutter.SubTriangle(
                new Vec3(i * 10, 0, 0), new Vec3(i * 10 + 1, 0, 0), new Vec3(i * 10, 1, 0),
                0, false, 0));
        }
        var adj = BuildAdjacencyFromSubs(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        foreach (var p in patches)
            foreach (int idx in p.SubTriangleIndices)
                Assert.True(idx >= 0 && idx < subs.Count);
    }

    [Fact]
    public void CutCubePair_MultiplePatches()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = MdCsg.Intersection.IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patches = PatchExtractor.Extract(cutA.SubTriangles, adj);
        // Should have multiple patches since intersection edges split them
        Assert.True(patches.Count > 1);
    }

    [Fact]
    public void CutCubePair_AllPatchesCoverAllSubTriangles()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = MdCsg.Intersection.IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patches = PatchExtractor.Extract(cutA.SubTriangles, adj);
        int total = 0;
        foreach (var p in patches) total += p.SubTriangleIndices.Count;
        Assert.Equal(cutA.SubTriangles.Count, total);
    }

    [Fact]
    public void TetrahedronMesh_NoCuts_SinglePatch()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        var cutResult = MeshCutter.Cut(mesh, new Dictionary<int, List<MdCsg.Intersection.IntersectionSegment>>());
        var adj = SubTriangleAdjacency.Build(cutResult.SubTriangles);
        var patches = PatchExtractor.Extract(cutResult.SubTriangles, adj);
        Assert.Equal(1, patches.Count);
        Assert.Equal(4, patches[0].SubTriangleIndices.Count);
    }

    [Fact]
    public void SphereMesh_NoCuts_SinglePatch()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh;
        var cutResult = MeshCutter.Cut(mesh, new Dictionary<int, List<MdCsg.Intersection.IntersectionSegment>>());
        var adj = SubTriangleAdjacency.Build(cutResult.SubTriangles);
        var patches = PatchExtractor.Extract(cutResult.SubTriangles, adj);
        Assert.Equal(1, patches.Count);
    }

    [Fact]
    public void PatchExtraction_Deterministic()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = MdCsg.Intersection.IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutA.SubTriangles);

        var patches1 = PatchExtractor.Extract(cutA.SubTriangles, adj);
        var patches2 = PatchExtractor.Extract(cutA.SubTriangles, adj);

        Assert.Equal(patches1.Count, patches2.Count);
        for (int i = 0; i < patches1.Count; i++)
        {
            Assert.Equal(patches1[i].SubTriangleIndices.Count, patches2[i].SubTriangleIndices.Count);
            for (int j = 0; j < patches1[i].SubTriangleIndices.Count; j++)
                Assert.Equal(patches1[i].SubTriangleIndices[j], patches2[i].SubTriangleIndices[j]);
        }
    }

    [Fact]
    public void Patch_FirstTriangle_IsFloodFillSeed()
    {
        // The first element in each patch's SubTriangleIndices should be the BFS seed
        var subs = new List<FaceCutter.SubTriangle>();
        for (int i = 0; i < 5; i++)
        {
            subs.Add(new FaceCutter.SubTriangle(
                new Vec3(i * 10, 0, 0), new Vec3(i * 10 + 1, 0, 0), new Vec3(i * 10, 1, 0),
                0, false, 0));
        }
        var adj = BuildAdjacencyFromSubs(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        // Each isolated sub-triangle forms its own patch, seed is the sub-triangle index
        foreach (var p in patches)
        {
            Assert.Equal(1, p.SubTriangleIndices.Count);
            Assert.Equal(p.Id, p.SubTriangleIndices[0]); // Seed = sequential scan order
        }
    }

    [Fact]
    public void ChainOfTriangles_NoIntersection_OnePatch()
    {
        // Create a chain of triangles sharing edges (fan from origin)
        var subs = new List<FaceCutter.SubTriangle>();
        var o = Vec3.Zero;
        for (int i = 0; i < 6; i++)
        {
            double angle1 = i * System.Math.PI / 3;
            double angle2 = (i + 1) * System.Math.PI / 3;
            var a = new Vec3(System.Math.Cos(angle1), System.Math.Sin(angle1), 0);
            var b = new Vec3(System.Math.Cos(angle2), System.Math.Sin(angle2), 0);
            subs.Add(new FaceCutter.SubTriangle(o, a, b, 0, false, 0));
        }
        var adj = BuildAdjacencyFromSubs(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        // All connected through shared edges at origin
        Assert.Equal(1, patches.Count);
        Assert.Equal(6, patches[0].SubTriangleIndices.Count);
    }

    [Fact]
    public void Patches_DefaultProperties()
    {
        var subs = new List<FaceCutter.SubTriangle> { MakeSub(0) };
        var adj = BuildAdjacencyFromSubs(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        var p = patches[0];
        Assert.Null(p.IsInside); // Not classified yet
        Assert.False(p.HasConfidentPoint); // Not computed yet
        Assert.Null(p.CoplanarNormalsAgree); // Not set yet
    }
}
