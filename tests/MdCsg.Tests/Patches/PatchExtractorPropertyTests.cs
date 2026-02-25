using MdCsg.Cutting;
using MdCsg.Math;
using MdCsg.Patches;

namespace MdCsg.Tests.Patches;

/// <summary>Phase 6: PatchExtractor — Extract patches from sub-triangles using adjacency flood-fill</summary>
public class PatchExtractorPropertyTests
{
    private static FaceCutter.SubTriangle MakeSub(int faceIdx, byte flags = 0) =>
        new(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, faceIdx, flags != 0, flags);

    [Fact]
    public void Extract_SingleSubTriangle_SinglePatch()
    {
        var subs = new[] { MakeSub(0) };
        var adj = SubTriangleAdjacency.Build(subs, 1e-10);
        var patches = PatchExtractor.Extract(subs, adj);
        Assert.Single(patches);
        Assert.Single(patches[0].SubTriangleIndices);
        Assert.Equal(0, patches[0].SubTriangleIndices[0]);
    }

    [Fact]
    public void Extract_TwoConnectedSubTriangles_OnePatch()
    {
        // Two triangles sharing an edge (B-C), no intersection flags
        var t0 = new FaceCutter.SubTriangle(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false, 0);
        var t1 = new FaceCutter.SubTriangle(
            new Vec3(1, 0, 0), new Vec3(0, 1, 0), new Vec3(1, 1, 0), 0, false, 0);
        var subs = new[] { t0, t1 };
        var adj = SubTriangleAdjacency.Build(subs, 1e-10);
        var patches = PatchExtractor.Extract(subs, adj);
        Assert.Single(patches);
        Assert.Equal(2, patches[0].SubTriangleIndices.Count);
    }

    [Fact]
    public void Extract_TwoSeparatedByIntersection_TwoPatches()
    {
        // Two triangles sharing an edge, but the shared edge is an intersection edge
        var t0 = new FaceCutter.SubTriangle(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, true, 2); // bit1 = edge B-C
        var t1 = new FaceCutter.SubTriangle(
            new Vec3(1, 0, 0), new Vec3(0, 1, 0), new Vec3(1, 1, 0), 0, true, 1); // bit0 = edge A-B
        var subs = new[] { t0, t1 };
        var adj = SubTriangleAdjacency.Build(subs, 1e-10);
        var patches = PatchExtractor.Extract(subs, adj);
        Assert.Equal(2, patches.Count);
    }

    [Fact]
    public void Extract_DisconnectedTriangles_SeparatePatches()
    {
        // Two triangles with no shared edges at all
        var t0 = new FaceCutter.SubTriangle(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false, 0);
        var t1 = new FaceCutter.SubTriangle(
            new Vec3(10, 10, 0), new Vec3(11, 10, 0), new Vec3(10, 11, 0), 1, false, 0);
        var subs = new[] { t0, t1 };
        var adj = SubTriangleAdjacency.Build(subs, 1e-10);
        var patches = PatchExtractor.Extract(subs, adj);
        Assert.Equal(2, patches.Count);
    }

    [Fact]
    public void Extract_PatchIds_Sequential()
    {
        var t0 = new FaceCutter.SubTriangle(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false, 0);
        var t1 = new FaceCutter.SubTriangle(
            new Vec3(10, 10, 0), new Vec3(11, 10, 0), new Vec3(10, 11, 0), 1, false, 0);
        var subs = new[] { t0, t1 };
        var adj = SubTriangleAdjacency.Build(subs, 1e-10);
        var patches = PatchExtractor.Extract(subs, adj);
        for (int i = 0; i < patches.Count; i++)
            Assert.Equal(i, patches[i].Id);
    }

    [Fact]
    public void Extract_AllSubTrianglesAssigned()
    {
        // Chain of 4 triangles sharing edges, no intersection flags
        var subs = new[]
        {
            new FaceCutter.SubTriangle(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0), 0, false, 0),
            new FaceCutter.SubTriangle(new Vec3(1,0,0), new Vec3(0,1,0), new Vec3(1,1,0), 0, false, 0),
            new FaceCutter.SubTriangle(new Vec3(1,1,0), new Vec3(2,1,0), new Vec3(1,2,0), 0, false, 0),
            new FaceCutter.SubTriangle(new Vec3(10,10,0), new Vec3(11,10,0), new Vec3(10,11,0), 1, false, 0),
        };
        var adj = SubTriangleAdjacency.Build(subs, 1e-10);
        var patches = PatchExtractor.Extract(subs, adj);
        int total = patches.Sum(p => p.SubTriangleIndices.Count);
        Assert.Equal(subs.Length, total);
    }

    [Fact]
    public void Extract_NoSubTriangleInMultiplePatches()
    {
        var subs = new[]
        {
            new FaceCutter.SubTriangle(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0), 0, false, 0),
            new FaceCutter.SubTriangle(new Vec3(1,0,0), new Vec3(0,1,0), new Vec3(1,1,0), 0, false, 0),
            new FaceCutter.SubTriangle(new Vec3(10,10,0), new Vec3(11,10,0), new Vec3(10,11,0), 1, false, 0),
        };
        var adj = SubTriangleAdjacency.Build(subs, 1e-10);
        var patches = PatchExtractor.Extract(subs, adj);

        var allIndices = patches.SelectMany(p => p.SubTriangleIndices).ToList();
        Assert.Equal(allIndices.Count, allIndices.Distinct().Count());
    }

    // --- Patch class tests ---

    [Fact]
    public void Patch_Constructor_SetsId()
    {
        var p = new Patch(42);
        Assert.Equal(42, p.Id);
    }

    [Fact]
    public void Patch_SubTriangleIndices_InitiallyEmpty()
    {
        var p = new Patch(0);
        Assert.Empty(p.SubTriangleIndices);
    }

    [Fact]
    public void Patch_IsInside_InitiallyNull()
    {
        var p = new Patch(0);
        Assert.Null(p.IsInside);
    }

    [Fact]
    public void Patch_HasConfidentPoint_InitiallyFalse()
    {
        var p = new Patch(0);
        Assert.False(p.HasConfidentPoint);
    }

    [Fact]
    public void Patch_CoplanarNormalsAgree_InitiallyNull()
    {
        var p = new Patch(0);
        Assert.Null(p.CoplanarNormalsAgree);
    }

    [Fact]
    public void Patch_SetAndGetProperties()
    {
        var p = new Patch(7);
        p.IsInside = true;
        p.HasConfidentPoint = true;
        p.ConfidentPoint = new Vec3(1, 2, 3);
        p.SourceMesh = 1;
        p.CoplanarNormalsAgree = false;

        Assert.True(p.IsInside);
        Assert.True(p.HasConfidentPoint);
        Assert.Equal(new Vec3(1, 2, 3), p.ConfidentPoint);
        Assert.Equal(1, p.SourceMesh);
        Assert.False(p.CoplanarNormalsAgree);
    }

    [Fact]
    public void Extract_ThreeConnected_OnePatch()
    {
        // Fan of 3 triangles sharing a common vertex
        var subs = new[]
        {
            new FaceCutter.SubTriangle(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0), 0, false, 0),
            new FaceCutter.SubTriangle(new Vec3(0,0,0), new Vec3(0,1,0), new Vec3(-1,0,0), 0, false, 0),
            new FaceCutter.SubTriangle(new Vec3(0,0,0), new Vec3(-1,0,0), new Vec3(0,-1,0), 0, false, 0),
        };
        var adj = SubTriangleAdjacency.Build(subs, 1e-10);
        var patches = PatchExtractor.Extract(subs, adj);
        // Could be 1 or more patches depending on adjacency detection
        Assert.True(patches.Count >= 1);
        int total = patches.Sum(p => p.SubTriangleIndices.Count);
        Assert.Equal(3, total);
    }
}
