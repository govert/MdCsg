using MdCsg.Api;
using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Patches;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Patches;

/// <summary>Phase 6: PatchExtractor.Extract — flood fill, intersection edge boundaries, patch counts</summary>
public class PatchExtractorPropertyTests
{
    [Fact]
    public void Extract_SingleTriangle_OnePatch()
    {
        var subTris = new List<FaceCutter.SubTriangle>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false)
        };
        var adjacency = SubTriangleAdjacency.Build(subTris);
        var patches = PatchExtractor.Extract(subTris, adjacency);
        Assert.Single(patches);
        Assert.Single(patches[0].SubTriangleIndices);
        Assert.Equal(0, patches[0].SubTriangleIndices[0]);
    }

    [Fact]
    public void Extract_TwoDisjointTriangles_TwoPatches()
    {
        var subTris = new List<FaceCutter.SubTriangle>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false),
            new(new Vec3(10, 10, 0), new Vec3(11, 10, 0), new Vec3(10, 11, 0), 1, false)
        };
        var adjacency = SubTriangleAdjacency.Build(subTris);
        var patches = PatchExtractor.Extract(subTris, adjacency);
        Assert.Equal(2, patches.Count);
    }

    [Fact]
    public void Extract_TwoAdjacentTriangles_NoIntersectionEdge_OnePatch()
    {
        // Two triangles sharing edge (0,0,0)-(1,0,0)
        var subTris = new List<FaceCutter.SubTriangle>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0.5, 1, 0), 0, false),
            new(new Vec3(1, 0, 0), new Vec3(0, 0, 0), new Vec3(0.5, -1, 0), 0, false)
        };
        var adjacency = SubTriangleAdjacency.Build(subTris);
        var patches = PatchExtractor.Extract(subTris, adjacency);
        Assert.Single(patches);
        Assert.Equal(2, patches[0].SubTriangleIndices.Count);
    }

    [Fact]
    public void Extract_TwoAdjacentTriangles_WithIntersectionEdge_TwoPatches()
    {
        // Two triangles sharing an edge that IS an intersection edge
        var subTris = new List<FaceCutter.SubTriangle>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0.5, 1, 0), 0, true, 0x01), // edge A-B is intersection
            new(new Vec3(1, 0, 0), new Vec3(0, 0, 0), new Vec3(0.5, -1, 0), 0, true, 0x01)  // edge A-B is intersection
        };
        var adjacency = SubTriangleAdjacency.Build(subTris);
        var patches = PatchExtractor.Extract(subTris, adjacency);
        Assert.Equal(2, patches.Count);
    }

    [Fact]
    public void Extract_PatchIds_AreSequential()
    {
        var subTris = new List<FaceCutter.SubTriangle>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false),
            new(new Vec3(10, 10, 0), new Vec3(11, 10, 0), new Vec3(10, 11, 0), 1, false),
            new(new Vec3(20, 20, 0), new Vec3(21, 20, 0), new Vec3(20, 21, 0), 2, false)
        };
        var adjacency = SubTriangleAdjacency.Build(subTris);
        var patches = PatchExtractor.Extract(subTris, adjacency);
        for (int i = 0; i < patches.Count; i++)
        {
            Assert.Equal(i, patches[i].Id);
        }
    }

    [Fact]
    public void Extract_AllSubTrianglesAssigned()
    {
        var subTris = new List<FaceCutter.SubTriangle>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false),
            new(new Vec3(1, 0, 0), new Vec3(0, 0, 0), new Vec3(0.5, -1, 0), 0, false),
            new(new Vec3(10, 0, 0), new Vec3(11, 0, 0), new Vec3(10, 1, 0), 1, false)
        };
        var adjacency = SubTriangleAdjacency.Build(subTris);
        var patches = PatchExtractor.Extract(subTris, adjacency);
        var allIndices = new HashSet<int>();
        foreach (var patch in patches)
            foreach (var idx in patch.SubTriangleIndices)
                allIndices.Add(idx);
        Assert.Equal(subTris.Count, allIndices.Count);
    }

    [Fact]
    public void Extract_NoSubTriangleInMultiplePatches()
    {
        var subTris = new List<FaceCutter.SubTriangle>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false),
            new(new Vec3(1, 0, 0), new Vec3(0, 0, 0), new Vec3(0.5, -1, 0), 0, false),
            new(new Vec3(10, 0, 0), new Vec3(11, 0, 0), new Vec3(10, 1, 0), 1, false)
        };
        var adjacency = SubTriangleAdjacency.Build(subTris);
        var patches = PatchExtractor.Extract(subTris, adjacency);
        var seen = new HashSet<int>();
        foreach (var patch in patches)
            foreach (var idx in patch.SubTriangleIndices)
                Assert.True(seen.Add(idx), $"Sub-triangle {idx} in multiple patches");
    }

    [Fact]
    public void Extract_FromCsgResult_PatchesHaveSubTriangles()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Union(a, b);
        // PatchCountA and PatchCountB should reflect extracted patches
        Assert.True(result.PatchCountA > 0 || result.PatchCountB > 0);
    }

    [Fact]
    public void Extract_EmptyInput_NoPatches()
    {
        var subTris = new List<FaceCutter.SubTriangle>();
        var adjacency = SubTriangleAdjacency.Build(subTris);
        var patches = PatchExtractor.Extract(subTris, adjacency);
        Assert.Empty(patches);
    }
}
