using MdCsg.Math;
using MdCsg.Patches;

namespace MdCsg.Tests.Patches;

/// <summary>Phase 6: Patch — constructor, properties, SubTriangleIndices, classification state</summary>
public class PatchConstructionPropertyTests
{
    [Fact]
    public void Patch_Constructor_SetsId()
    {
        var patch = new Patch(42);
        Assert.Equal(42, patch.Id);
    }

    [Fact]
    public void Patch_Constructor_SubTriangleIndicesEmpty()
    {
        var patch = new Patch(0);
        Assert.Empty(patch.SubTriangleIndices);
    }

    [Fact]
    public void Patch_Constructor_IsInsideNull()
    {
        var patch = new Patch(0);
        Assert.Null(patch.IsInside);
    }

    [Fact]
    public void Patch_Constructor_HasConfidentPointFalse()
    {
        var patch = new Patch(0);
        Assert.False(patch.HasConfidentPoint);
    }

    [Fact]
    public void Patch_Constructor_SourceMeshZero()
    {
        var patch = new Patch(0);
        Assert.Equal(0, patch.SourceMesh);
    }

    [Fact]
    public void Patch_Constructor_CoplanarNormalsAgreeNull()
    {
        var patch = new Patch(0);
        Assert.Null(patch.CoplanarNormalsAgree);
    }

    [Fact]
    public void Patch_SubTriangleIndices_CanAdd()
    {
        var patch = new Patch(0);
        patch.SubTriangleIndices.Add(5);
        patch.SubTriangleIndices.Add(10);
        Assert.Equal(2, patch.SubTriangleIndices.Count);
        Assert.Equal(5, patch.SubTriangleIndices[0]);
        Assert.Equal(10, patch.SubTriangleIndices[1]);
    }

    [Fact]
    public void Patch_IsInside_SetGet()
    {
        var patch = new Patch(0);
        patch.IsInside = true;
        Assert.True(patch.IsInside);
        patch.IsInside = false;
        Assert.False(patch.IsInside);
    }

    [Fact]
    public void Patch_HasConfidentPoint_SetGet()
    {
        var patch = new Patch(0);
        patch.HasConfidentPoint = true;
        Assert.True(patch.HasConfidentPoint);
    }

    [Fact]
    public void Patch_SourceMesh_SetGet()
    {
        var patch = new Patch(0);
        patch.SourceMesh = 1;
        Assert.Equal(1, patch.SourceMesh);
    }

    [Fact]
    public void Patch_CoplanarNormalsAgree_True()
    {
        var patch = new Patch(0);
        patch.CoplanarNormalsAgree = true;
        Assert.True(patch.CoplanarNormalsAgree);
    }

    [Fact]
    public void Patch_CoplanarNormalsAgree_False()
    {
        var patch = new Patch(0);
        patch.CoplanarNormalsAgree = false;
        Assert.False(patch.CoplanarNormalsAgree);
    }

    [Fact]
    public void Patch_ConfidentPoint_SetGet()
    {
        var patch = new Patch(0);
        var pt = new Vec3(1, 2, 3);
        patch.ConfidentPoint = pt;
        Assert.Equal(1.0, patch.ConfidentPoint.X);
        Assert.Equal(2.0, patch.ConfidentPoint.Y);
        Assert.Equal(3.0, patch.ConfidentPoint.Z);
    }

    [Fact]
    public void Patch_MultipleIds_AreDistinct()
    {
        var p0 = new Patch(0);
        var p1 = new Patch(1);
        var p2 = new Patch(2);
        Assert.NotEqual(p0.Id, p1.Id);
        Assert.NotEqual(p1.Id, p2.Id);
    }

    [Fact]
    public void Patch_SubTriangleIndices_CanClear()
    {
        var patch = new Patch(0);
        patch.SubTriangleIndices.Add(1);
        patch.SubTriangleIndices.Add(2);
        patch.SubTriangleIndices.Clear();
        Assert.Empty(patch.SubTriangleIndices);
    }

    [Fact]
    public void Patch_SubTriangleIndices_ContainsCheck()
    {
        var patch = new Patch(0);
        patch.SubTriangleIndices.Add(7);
        patch.SubTriangleIndices.Add(13);
        Assert.Contains(7, patch.SubTriangleIndices);
        Assert.Contains(13, patch.SubTriangleIndices);
        Assert.DoesNotContain(99, patch.SubTriangleIndices);
    }

    [Fact]
    public void Patch_LargeId_Works()
    {
        var patch = new Patch(999999);
        Assert.Equal(999999, patch.Id);
    }

    [Fact]
    public void Patch_IsInside_SetToNullAfterTrue()
    {
        var patch = new Patch(0);
        patch.IsInside = true;
        patch.IsInside = null;
        Assert.Null(patch.IsInside);
    }
}
