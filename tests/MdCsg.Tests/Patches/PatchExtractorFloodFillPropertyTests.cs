using MdCsg.Cutting;
using MdCsg.Math;
using MdCsg.Patches;

namespace MdCsg.Tests.Patches;

/// <summary>Phase 6: PatchExtractor — flood-fill grouping, intersection edge boundaries, patch ID assignment</summary>
public class PatchExtractorFloodFillPropertyTests
{
    [Fact]
    public void SingleTriangle_OneSubTriangle_OnePatch()
    {
        var subs = new List<FaceCutter.SubTriangle>
        {
            new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, false)
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        Assert.Equal(1, patches.Count);
        Assert.Equal(1, patches[0].SubTriangleIndices.Count);
    }

    [Fact]
    public void TwoAdjacentSubTriangles_NoIntersection_OnePatch()
    {
        var subs = new List<FaceCutter.SubTriangle>
        {
            new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, false),
            new FaceCutter.SubTriangle(Vec3.UnitX, new Vec3(1, 1, 0), Vec3.UnitY, 0, false)
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        Assert.Equal(1, patches.Count);
        Assert.Equal(2, patches[0].SubTriangleIndices.Count);
    }

    [Fact]
    public void TwoDisjointSubTriangles_TwoPatches()
    {
        var subs = new List<FaceCutter.SubTriangle>
        {
            new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, false),
            new FaceCutter.SubTriangle(new Vec3(10, 0, 0), new Vec3(11, 0, 0), new Vec3(10, 1, 0), 1, false)
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        Assert.Equal(2, patches.Count);
    }

    [Fact]
    public void TwoAdjacentWithIntersectionEdge_TwoPatches()
    {
        // Two triangles sharing an edge, but the shared edge is an intersection edge
        var subs = new List<FaceCutter.SubTriangle>
        {
            new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, true, 0b001), // edge 0 (A-B) is intersection
            new FaceCutter.SubTriangle(Vec3.UnitX, new Vec3(1, 1, 0), Vec3.UnitY, 0, true, 0b100)  // edge 2 (C-A) is intersection
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        // With intersection edge between them, they should be in separate patches
        Assert.Equal(2, patches.Count);
    }

    [Fact]
    public void PatchIds_Sequential()
    {
        var subs = new List<FaceCutter.SubTriangle>
        {
            new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, false),
            new FaceCutter.SubTriangle(new Vec3(10, 0, 0), new Vec3(11, 0, 0), new Vec3(10, 1, 0), 1, false)
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        for (int i = 0; i < patches.Count; i++)
        {
            Assert.Equal(i, patches[i].Id);
        }
    }

    [Fact]
    public void AllSubTriangles_AssignedToSomePatch()
    {
        var subs = new List<FaceCutter.SubTriangle>
        {
            new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, false),
            new FaceCutter.SubTriangle(Vec3.UnitX, new Vec3(1, 1, 0), Vec3.UnitY, 0, false),
            new FaceCutter.SubTriangle(new Vec3(10, 0, 0), new Vec3(11, 0, 0), new Vec3(10, 1, 0), 1, false)
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var patches = PatchExtractor.Extract(subs, adj);

        var assigned = new HashSet<int>();
        foreach (var patch in patches)
            foreach (int idx in patch.SubTriangleIndices)
                assigned.Add(idx);

        Assert.Equal(subs.Count, assigned.Count);
    }

    [Fact]
    public void NoOverlappingAssignment()
    {
        var subs = new List<FaceCutter.SubTriangle>
        {
            new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, false),
            new FaceCutter.SubTriangle(Vec3.UnitX, new Vec3(1, 1, 0), Vec3.UnitY, 0, false),
            new FaceCutter.SubTriangle(new Vec3(10, 0, 0), new Vec3(11, 0, 0), new Vec3(10, 1, 0), 1, false)
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var patches = PatchExtractor.Extract(subs, adj);

        var allIndices = new List<int>();
        foreach (var patch in patches)
            allIndices.AddRange(patch.SubTriangleIndices);

        Assert.Equal(allIndices.Count, new HashSet<int>(allIndices).Count);
    }

    [Fact]
    public void EmptySubTriangles_NoPatches()
    {
        var subs = new List<FaceCutter.SubTriangle>();
        var adj = SubTriangleAdjacency.Build(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        Assert.Equal(0, patches.Count);
    }

    [Fact]
    public void ThreeGroups_ThreePatches()
    {
        var subs = new List<FaceCutter.SubTriangle>
        {
            new FaceCutter.SubTriangle(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false),
            new FaceCutter.SubTriangle(new Vec3(10, 0, 0), new Vec3(11, 0, 0), new Vec3(10, 1, 0), 1, false),
            new FaceCutter.SubTriangle(new Vec3(20, 0, 0), new Vec3(21, 0, 0), new Vec3(20, 1, 0), 2, false)
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        Assert.Equal(3, patches.Count);
    }

    [Fact]
    public void Patch_SubTriangleIndices_NonEmpty()
    {
        var subs = new List<FaceCutter.SubTriangle>
        {
            new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, false)
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        foreach (var patch in patches)
        {
            Assert.True(patch.SubTriangleIndices.Count > 0);
        }
    }

    [Fact]
    public void FourAdjacentTriangles_OnePatch()
    {
        // Four triangles forming a strip with shared edges
        var subs = new List<FaceCutter.SubTriangle>
        {
            new FaceCutter.SubTriangle(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false),
            new FaceCutter.SubTriangle(new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0), 0, false),
            new FaceCutter.SubTriangle(new Vec3(1, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 1, 0), 1, false),
            new FaceCutter.SubTriangle(new Vec3(2, 0, 0), new Vec3(2, 1, 0), new Vec3(1, 1, 0), 1, false)
        };
        var adj = SubTriangleAdjacency.Build(subs);
        var patches = PatchExtractor.Extract(subs, adj);
        Assert.Equal(1, patches.Count);
        Assert.Equal(4, patches[0].SubTriangleIndices.Count);
    }

    [Fact]
    public void Patch_DefaultProperties()
    {
        var patch = new Patch(42);
        Assert.Equal(42, patch.Id);
        Assert.Null(patch.IsInside);
        Assert.False(patch.HasConfidentPoint);
        Assert.Equal(Vec3.Zero, patch.ConfidentPoint);
        Assert.Null(patch.CoplanarNormalsAgree);
    }
}
