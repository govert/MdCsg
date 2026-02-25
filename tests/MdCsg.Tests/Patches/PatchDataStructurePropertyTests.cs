using MdCsg.Math;
using MdCsg.Patches;

namespace MdCsg.Tests.Patches;

/// <summary>Phase 6: Patch — data structure properties, default values, classification state</summary>
public class PatchDataStructurePropertyTests
{
    [Fact]
    public void NewPatch_HasId()
    {
        var patch = new Patch(42);
        Assert.Equal(42, patch.Id);
    }

    [Fact]
    public void NewPatch_EmptySubTriangles()
    {
        var patch = new Patch(0);
        Assert.Empty(patch.SubTriangleIndices);
    }

    [Fact]
    public void NewPatch_IsInsideNull()
    {
        var patch = new Patch(0);
        Assert.Null(patch.IsInside);
    }

    [Fact]
    public void NewPatch_CoplanarNull()
    {
        var patch = new Patch(0);
        Assert.Null(patch.CoplanarNormalsAgree);
    }

    [Fact]
    public void NewPatch_HasConfidentPointFalse()
    {
        var patch = new Patch(0);
        Assert.False(patch.HasConfidentPoint);
    }

    [Fact]
    public void AddSubTriangleIndices_Works()
    {
        var patch = new Patch(0);
        patch.SubTriangleIndices.Add(0);
        patch.SubTriangleIndices.Add(5);
        patch.SubTriangleIndices.Add(10);
        Assert.Equal(3, patch.SubTriangleIndices.Count);
        Assert.Contains(5, patch.SubTriangleIndices);
    }

    [Fact]
    public void SetIsInside_True()
    {
        var patch = new Patch(0);
        patch.IsInside = true;
        Assert.True(patch.IsInside.Value);
    }

    [Fact]
    public void SetIsInside_False()
    {
        var patch = new Patch(0);
        patch.IsInside = false;
        Assert.False(patch.IsInside.Value);
    }

    [Fact]
    public void SetCoplanarNormalsAgree_True()
    {
        var patch = new Patch(0);
        patch.CoplanarNormalsAgree = true;
        Assert.True(patch.CoplanarNormalsAgree.Value);
    }

    [Fact]
    public void SetCoplanarNormalsAgree_False()
    {
        var patch = new Patch(0);
        patch.CoplanarNormalsAgree = false;
        Assert.False(patch.CoplanarNormalsAgree.Value);
    }

    [Fact]
    public void SetConfidentPoint_Works()
    {
        var patch = new Patch(0);
        patch.ConfidentPoint = new Vec3(1, 2, 3);
        patch.HasConfidentPoint = true;
        Assert.Equal(new Vec3(1, 2, 3), patch.ConfidentPoint);
        Assert.True(patch.HasConfidentPoint);
    }

    [Fact]
    public void SetSourceMesh_Works()
    {
        var patch = new Patch(0);
        patch.SourceMesh = 1;
        Assert.Equal(1, patch.SourceMesh);
    }

    [Fact]
    public void MultiplePatchIds_AreUnique()
    {
        var patches = Enumerable.Range(0, 10).Select(i => new Patch(i)).ToList();
        var ids = patches.Select(p => p.Id).ToHashSet();
        Assert.Equal(10, ids.Count);
    }

    [Fact]
    public void PatchWithManySubTriangles_CountCorrect()
    {
        var patch = new Patch(0);
        for (int i = 0; i < 100; i++)
            patch.SubTriangleIndices.Add(i);
        Assert.Equal(100, patch.SubTriangleIndices.Count);
    }
}
