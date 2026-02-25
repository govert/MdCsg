using MdCsg.Math;
using MdCsg.Patches;

namespace MdCsg.Tests.Patches;

/// <summary>Phase 6: Patch — coplanar classification state, confident point, IsInside</summary>
public class PatchCoplanarClassificationTests
{
    [Fact]
    public void NewPatch_DefaultState()
    {
        var patch = new Patch(0);
        Assert.Equal(0, patch.Id);
        Assert.Empty(patch.SubTriangleIndices);
        Assert.Null(patch.IsInside);
        Assert.False(patch.HasConfidentPoint);
        Assert.Null(patch.CoplanarNormalsAgree);
        Assert.Equal(Vec3.Zero, patch.ConfidentPoint);
    }

    [Fact]
    public void Patch_IdIsAssigned()
    {
        var patch = new Patch(42);
        Assert.Equal(42, patch.Id);
    }

    [Fact]
    public void Patch_SubTriangles_CanBeAdded()
    {
        var patch = new Patch(0);
        patch.SubTriangleIndices.Add(0);
        patch.SubTriangleIndices.Add(5);
        patch.SubTriangleIndices.Add(10);
        Assert.Equal(3, patch.SubTriangleIndices.Count);
        Assert.Contains(5, patch.SubTriangleIndices);
    }

    [Fact]
    public void Patch_SetIsInside_True()
    {
        var patch = new Patch(0);
        patch.IsInside = true;
        Assert.True(patch.IsInside.Value);
    }

    [Fact]
    public void Patch_SetIsInside_False()
    {
        var patch = new Patch(0);
        patch.IsInside = false;
        Assert.False(patch.IsInside.Value);
    }

    [Fact]
    public void Patch_SetConfidentPoint()
    {
        var patch = new Patch(0);
        patch.ConfidentPoint = new Vec3(1, 2, 3);
        patch.HasConfidentPoint = true;
        Assert.Equal(new Vec3(1, 2, 3), patch.ConfidentPoint);
        Assert.True(patch.HasConfidentPoint);
    }

    [Fact]
    public void Patch_CoplanarNormalsAgree_True()
    {
        var patch = new Patch(0);
        patch.CoplanarNormalsAgree = true;
        Assert.True(patch.CoplanarNormalsAgree.Value);
    }

    [Fact]
    public void Patch_CoplanarNormalsAgree_False()
    {
        var patch = new Patch(0);
        patch.CoplanarNormalsAgree = false;
        Assert.False(patch.CoplanarNormalsAgree.Value);
    }

    [Fact]
    public void Patch_SourceMesh_Default()
    {
        var patch = new Patch(0);
        Assert.Equal(0, patch.SourceMesh);
    }

    [Fact]
    public void Patch_SourceMesh_CanBeSet()
    {
        var patch = new Patch(0);
        patch.SourceMesh = 1;
        Assert.Equal(1, patch.SourceMesh);
    }

    [Fact]
    public void Patch_CoplanarWithConfidentPoint_HasBothSet()
    {
        var patch = new Patch(0);
        patch.CoplanarNormalsAgree = true;
        patch.HasConfidentPoint = true;
        patch.ConfidentPoint = new Vec3(1, 1, 1);
        Assert.True(patch.CoplanarNormalsAgree.HasValue);
        Assert.True(patch.HasConfidentPoint);
    }

    [Fact]
    public void Patch_CoplanarWithoutConfidentPoint()
    {
        var patch = new Patch(0);
        patch.CoplanarNormalsAgree = true;
        Assert.True(patch.CoplanarNormalsAgree.HasValue);
        Assert.False(patch.HasConfidentPoint);
    }
}
