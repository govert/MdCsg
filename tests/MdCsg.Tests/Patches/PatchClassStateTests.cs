using MdCsg.Cutting;
using MdCsg.Math;
using MdCsg.Patches;

namespace MdCsg.Tests.Patches;

/// <summary>Phase 6: Patch class state — initial state, classification state, PatchExtractor boundary conditions</summary>
public class PatchClassStateTests
{
    private static FaceCutter.SubTriangle MakeSub(Vec3 a, Vec3 b, Vec3 c, int origFace, bool hasIntersection, byte flags = 0)
        => new FaceCutter.SubTriangle(a, b, c, origFace, hasIntersection, flags);

    [Fact]
    public void Patch_InitialState_NoClassification()
    {
        var patch = new Patch(0);
        Assert.Null(patch.IsInside);
        Assert.False(patch.HasConfidentPoint);
        Assert.Empty(patch.SubTriangleIndices);
    }

    [Fact]
    public void Patch_Id_Preserved()
    {
        var patch = new Patch(42);
        Assert.Equal(42, patch.Id);
    }

    [Fact]
    public void Patch_SourceMesh_DefaultZero()
    {
        var patch = new Patch(0);
        Assert.Equal(0, patch.SourceMesh);
    }

    [Fact]
    public void Patch_SourceMesh_Settable()
    {
        var patch = new Patch(0);
        patch.SourceMesh = 1;
        Assert.Equal(1, patch.SourceMesh);
    }

    [Fact]
    public void Patch_CoplanarNormalsAgree_DefaultNull()
    {
        var patch = new Patch(0);
        Assert.Null(patch.CoplanarNormalsAgree);
    }

    [Fact]
    public void Patch_CoplanarNormalsAgree_Settable()
    {
        var patch = new Patch(0);
        patch.CoplanarNormalsAgree = true;
        Assert.True(patch.CoplanarNormalsAgree);
        patch.CoplanarNormalsAgree = false;
        Assert.False(patch.CoplanarNormalsAgree);
    }

    [Fact]
    public void Patch_ConfidentPoint_Settable()
    {
        var patch = new Patch(0);
        var point = new Vec3(1, 2, 3);
        patch.ConfidentPoint = point;
        Assert.Equal(point, patch.ConfidentPoint);
    }

    [Fact]
    public void Patch_IsInside_Settable()
    {
        var patch = new Patch(0);
        patch.IsInside = true;
        Assert.True(patch.IsInside);
        patch.IsInside = false;
        Assert.False(patch.IsInside);
    }

    [Fact]
    public void Patch_SubTriangleIndices_Mutable()
    {
        var patch = new Patch(0);
        patch.SubTriangleIndices.Add(5);
        patch.SubTriangleIndices.Add(10);
        Assert.Equal(2, patch.SubTriangleIndices.Count);
        Assert.Equal(5, patch.SubTriangleIndices[0]);
        Assert.Equal(10, patch.SubTriangleIndices[1]);
    }

    [Fact]
    public void Extract_EmptyInput_EmptyPatches()
    {
        var subs = new List<FaceCutter.SubTriangle>();
        var adj = SubTriangleAdjacency.Build(subs, 1e-8);
        var patches = PatchExtractor.Extract(subs, adj);
        Assert.Empty(patches);
    }

    [Fact]
    public void Extract_AllSubTriangles_Assigned()
    {
        // 3 isolated triangles = 3 patches, each with 1 sub-triangle
        var subs = new List<FaceCutter.SubTriangle>
        {
            MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false),
            MakeSub(new Vec3(10, 0, 0), new Vec3(11, 0, 0), new Vec3(10, 1, 0), 1, false),
            MakeSub(new Vec3(20, 0, 0), new Vec3(21, 0, 0), new Vec3(20, 1, 0), 2, false)
        };
        var adj = SubTriangleAdjacency.Build(subs, 1e-8);
        var patches = PatchExtractor.Extract(subs, adj);
        int total = patches.Sum(p => p.SubTriangleIndices.Count);
        Assert.Equal(3, total);
    }

    [Fact]
    public void Extract_PatchIds_StartAtZero()
    {
        var subs = new List<FaceCutter.SubTriangle>
        {
            MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false),
            MakeSub(new Vec3(10, 0, 0), new Vec3(11, 0, 0), new Vec3(10, 1, 0), 1, false)
        };
        var adj = SubTriangleAdjacency.Build(subs, 1e-8);
        var patches = PatchExtractor.Extract(subs, adj);
        Assert.Equal(0, patches[0].Id);
        Assert.Equal(1, patches[1].Id);
    }

    [Fact]
    public void Extract_ChainOfTriangles_OnePatch()
    {
        // Chain: T0 shares edge with T1, T1 shares edge with T2, no intersection edges
        var subs = new List<FaceCutter.SubTriangle>
        {
            MakeSub(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0.5, 1, 0), 0, false),
            MakeSub(new Vec3(1, 0, 0), new Vec3(0.5, 1, 0), new Vec3(1.5, 1, 0), 1, false),
            MakeSub(new Vec3(0.5, 1, 0), new Vec3(1.5, 1, 0), new Vec3(1, 2, 0), 2, false)
        };
        var adj = SubTriangleAdjacency.Build(subs, 1e-8);
        var patches = PatchExtractor.Extract(subs, adj);
        Assert.Single(patches);
        Assert.Equal(3, patches[0].SubTriangleIndices.Count);
    }
}
