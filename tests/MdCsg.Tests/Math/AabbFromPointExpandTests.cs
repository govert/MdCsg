using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Aabb — FromPoint, Expand, ExpandToInclude, Union edge cases</summary>
public class AabbFromPointExpandTests
{
    [Fact]
    public void FromPoint_MinEqualsMax()
    {
        var p = new Vec3(1, 2, 3);
        var box = Aabb.FromPoint(p);
        Assert.Equal(p, box.Min);
        Assert.Equal(p, box.Max);
    }

    [Fact]
    public void FromPoint_ZeroSize()
    {
        var box = Aabb.FromPoint(new Vec3(5, 5, 5));
        Assert.Equal(0.0, box.Size.X, 15);
        Assert.Equal(0.0, box.Size.Y, 15);
        Assert.Equal(0.0, box.Size.Z, 15);
    }

    [Fact]
    public void Expand_GrowsByMarginOnAllSides()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        var expanded = box.Expand(0.5);
        Assert.Equal(new Vec3(-0.5, -0.5, -0.5), expanded.Min);
        Assert.Equal(new Vec3(1.5, 1.5, 1.5), expanded.Max);
    }

    [Fact]
    public void Expand_ZeroMargin_NoChange()
    {
        var box = new Aabb(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        var expanded = box.Expand(0);
        Assert.Equal(box.Min, expanded.Min);
        Assert.Equal(box.Max, expanded.Max);
    }

    [Fact]
    public void ExpandToInclude_PointInside_NoChange()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(2, 2, 2));
        var expanded = box.ExpandToInclude(new Vec3(1, 1, 1));
        Assert.Equal(box.Min, expanded.Min);
        Assert.Equal(box.Max, expanded.Max);
    }

    [Fact]
    public void ExpandToInclude_PointOutside_Grows()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        var expanded = box.ExpandToInclude(new Vec3(3, 3, 3));
        Assert.Equal(new Vec3(0, 0, 0), expanded.Min);
        Assert.Equal(new Vec3(3, 3, 3), expanded.Max);
    }

    [Fact]
    public void ExpandToInclude_NegativePoint()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        var expanded = box.ExpandToInclude(new Vec3(-2, -3, -4));
        Assert.Equal(new Vec3(-2, -3, -4), expanded.Min);
        Assert.Equal(new Vec3(1, 1, 1), expanded.Max);
    }

    [Fact]
    public void Union_TwoBoxes_EnclosessBoth()
    {
        var a = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(2, 2, 2), new Vec3(3, 3, 3));
        var u = Aabb.Union(a, b);
        Assert.Equal(new Vec3(0, 0, 0), u.Min);
        Assert.Equal(new Vec3(3, 3, 3), u.Max);
    }

    [Fact]
    public void Union_Overlapping_EnclosessBoth()
    {
        var a = new Aabb(new Vec3(0, 0, 0), new Vec3(2, 2, 2));
        var b = new Aabb(new Vec3(1, 1, 1), new Vec3(3, 3, 3));
        var u = Aabb.Union(a, b);
        Assert.Equal(new Vec3(0, 0, 0), u.Min);
        Assert.Equal(new Vec3(3, 3, 3), u.Max);
    }

    [Fact]
    public void Union_WithSelf_NoChange()
    {
        var box = new Aabb(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        var u = Aabb.Union(box, box);
        Assert.Equal(box.Min, u.Min);
        Assert.Equal(box.Max, u.Max);
    }

    [Fact]
    public void FromTriangle_ContainsAllVertices()
    {
        var a = new Vec3(1, 5, 3);
        var b = new Vec3(4, 2, 6);
        var c = new Vec3(2, 3, 1);
        var box = Aabb.FromTriangle(a, b, c);
        Assert.True(box.Contains(a));
        Assert.True(box.Contains(b));
        Assert.True(box.Contains(c));
    }

    [Fact]
    public void Center_IsAverage()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(4, 6, 8));
        Assert.Equal(new Vec3(2, 3, 4), box.Center);
    }

    [Fact]
    public void SurfaceArea_UnitCube()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        Assert.Equal(6.0, box.SurfaceArea, 10);
    }

    [Fact]
    public void SurfaceArea_FlatBox_IsZero()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 0));
        // Degenerate in Z: 2*(1*1 + 1*0 + 0*1) = 2
        Assert.Equal(2.0, box.SurfaceArea, 10);
    }

    [Fact]
    public void SurfaceArea_Rectangular()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(2, 3, 4));
        // 2*(2*3 + 3*4 + 4*2) = 2*(6+12+8) = 52
        Assert.Equal(52.0, box.SurfaceArea, 10);
    }

    [Fact]
    public void Overlaps_IdenticalBoxes_True()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        Assert.True(box.Overlaps(box));
    }

    [Fact]
    public void Overlaps_DisjointBoxes_False()
    {
        var a = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(2, 2, 2), new Vec3(3, 3, 3));
        Assert.False(a.Overlaps(b));
    }

    [Fact]
    public void Overlaps_TouchingFaces_True()
    {
        var a = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(1, 0, 0), new Vec3(2, 1, 1));
        Assert.True(a.Overlaps(b));
    }

    [Fact]
    public void Contains_PointOnBoundary_True()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        Assert.True(box.Contains(new Vec3(0, 0, 0)));
        Assert.True(box.Contains(new Vec3(1, 1, 1)));
    }
}
