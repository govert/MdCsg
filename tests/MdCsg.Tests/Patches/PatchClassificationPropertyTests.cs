using MdCsg.Math;
using MdCsg.Patches;

namespace MdCsg.Tests.Patches;

/// <summary>Phase 6: Patch class property and state management tests</summary>
public class PatchClassificationPropertyTests
{
    [Fact]
    public void Constructor_SetsId()
    {
        var patch = new Patch(42);
        Assert.Equal(42, patch.Id);
    }

    [Fact]
    public void SubTriangleIndices_InitiallyEmpty()
    {
        var patch = new Patch(0);
        Assert.Empty(patch.SubTriangleIndices);
    }

    [Fact]
    public void SubTriangleIndices_Mutable()
    {
        var patch = new Patch(0);
        patch.SubTriangleIndices.Add(1);
        patch.SubTriangleIndices.Add(2);
        patch.SubTriangleIndices.Add(3);
        Assert.Equal(3, patch.SubTriangleIndices.Count);
    }

    [Fact]
    public void ConfidentPoint_DefaultZero()
    {
        var patch = new Patch(0);
        Assert.Equal(Vec3.Zero, patch.ConfidentPoint);
    }

    [Fact]
    public void ConfidentPoint_Settable()
    {
        var patch = new Patch(0);
        patch.ConfidentPoint = new Vec3(1, 2, 3);
        Assert.Equal(new Vec3(1, 2, 3), patch.ConfidentPoint);
    }

    [Fact]
    public void IsInside_DefaultNull()
    {
        var patch = new Patch(0);
        Assert.Null(patch.IsInside);
    }

    [Fact]
    public void IsInside_Settable()
    {
        var patch = new Patch(0);
        patch.IsInside = true;
        Assert.True(patch.IsInside);
        patch.IsInside = false;
        Assert.False(patch.IsInside);
    }

    [Fact]
    public void HasConfidentPoint_DefaultFalse()
    {
        var patch = new Patch(0);
        Assert.False(patch.HasConfidentPoint);
    }

    [Fact]
    public void HasConfidentPoint_Settable()
    {
        var patch = new Patch(0);
        patch.HasConfidentPoint = true;
        Assert.True(patch.HasConfidentPoint);
    }

    [Fact]
    public void SourceMesh_DefaultZero()
    {
        var patch = new Patch(0);
        Assert.Equal(0, patch.SourceMesh);
    }

    [Fact]
    public void SourceMesh_Settable()
    {
        var patch = new Patch(0);
        patch.SourceMesh = 1;
        Assert.Equal(1, patch.SourceMesh);
    }

    [Fact]
    public void CoplanarNormalsAgree_DefaultNull()
    {
        var patch = new Patch(0);
        Assert.Null(patch.CoplanarNormalsAgree);
    }

    [Fact]
    public void CoplanarNormalsAgree_Settable()
    {
        var patch = new Patch(0);
        patch.CoplanarNormalsAgree = true;
        Assert.True(patch.CoplanarNormalsAgree);
        patch.CoplanarNormalsAgree = false;
        Assert.False(patch.CoplanarNormalsAgree);
    }

    [Fact]
    public void MultiplePatchIds_Unique()
    {
        var patches = Enumerable.Range(0, 10).Select(i => new Patch(i)).ToList();
        var ids = patches.Select(p => p.Id).ToHashSet();
        Assert.Equal(10, ids.Count);
    }

    [Fact]
    public void FullLifecycle_ClassifyPatch()
    {
        var patch = new Patch(0);
        patch.SubTriangleIndices.Add(0);
        patch.SubTriangleIndices.Add(1);
        patch.SourceMesh = 0;
        patch.ConfidentPoint = new Vec3(0.5, 0.5, 0.5);
        patch.HasConfidentPoint = true;
        patch.IsInside = true;
        Assert.Equal(2, patch.SubTriangleIndices.Count);
        Assert.True(patch.HasConfidentPoint);
        Assert.True(patch.IsInside);
    }
}
