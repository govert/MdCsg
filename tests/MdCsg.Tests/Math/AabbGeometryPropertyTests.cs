using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Aabb — overlap, containment, union, surface area, expand, factory methods</summary>
public class AabbGeometryPropertyTests
{
    [Fact]
    public void UnitCube_Size()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        Assert.Equal(new Vec3(1, 1, 1), box.Size);
    }

    [Fact]
    public void UnitCube_Center()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        Assert.Equal(new Vec3(0.5, 0.5, 0.5), box.Center);
    }

    [Fact]
    public void UnitCube_SurfaceArea()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        Assert.Equal(6.0, box.SurfaceArea, 15);
    }

    [Fact]
    public void Rectangle_SurfaceArea()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(2, 3, 4));
        // 2*(2*3 + 3*4 + 4*2) = 2*(6+12+8) = 52
        Assert.Equal(52.0, box.SurfaceArea, 15);
    }

    [Fact]
    public void PointBox_SurfaceAreaZero()
    {
        var box = Aabb.FromPoint(new Vec3(1, 2, 3));
        Assert.Equal(0.0, box.SurfaceArea, 15);
    }

    [Fact]
    public void Overlaps_Identical_True()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        Assert.True(box.Overlaps(box));
    }

    [Fact]
    public void Overlaps_PartialOverlap_True()
    {
        var a = new Aabb(Vec3.Zero, new Vec3(2, 2, 2));
        var b = new Aabb(new Vec3(1, 1, 1), new Vec3(3, 3, 3));
        Assert.True(a.Overlaps(b));
        Assert.True(b.Overlaps(a));
    }

    [Fact]
    public void Overlaps_Disjoint_False()
    {
        var a = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(2, 2, 2), new Vec3(3, 3, 3));
        Assert.False(a.Overlaps(b));
        Assert.False(b.Overlaps(a));
    }

    [Fact]
    public void Overlaps_TouchingEdge_True()
    {
        var a = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(1, 0, 0), new Vec3(2, 1, 1));
        Assert.True(a.Overlaps(b));
    }

    [Fact]
    public void Overlaps_DisjointOnOneAxis_False()
    {
        var a = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(0, 0, 2), new Vec3(1, 1, 3));
        Assert.False(a.Overlaps(b));
    }

    [Fact]
    public void Contains_InteriorPoint_True()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        Assert.True(box.Contains(new Vec3(0.5, 0.5, 0.5)));
    }

    [Fact]
    public void Contains_CornerPoint_True()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        Assert.True(box.Contains(Vec3.Zero));
        Assert.True(box.Contains(new Vec3(1, 1, 1)));
    }

    [Fact]
    public void Contains_ExteriorPoint_False()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        Assert.False(box.Contains(new Vec3(2, 0, 0)));
    }

    [Fact]
    public void Union_TwoBoxes_EnclosessBoth()
    {
        var a = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(2, 2, 2), new Vec3(3, 3, 3));
        var u = Aabb.Union(a, b);
        Assert.Equal(Vec3.Zero, u.Min);
        Assert.Equal(new Vec3(3, 3, 3), u.Max);
    }

    [Fact]
    public void Union_Commutative()
    {
        var a = new Aabb(Vec3.Zero, new Vec3(1, 2, 3));
        var b = new Aabb(new Vec3(-1, -2, -3), new Vec3(0.5, 0.5, 0.5));
        Assert.Equal(Aabb.Union(a, b), Aabb.Union(b, a));
    }

    [Fact]
    public void Union_WithItself_Same()
    {
        var box = new Aabb(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        var u = Aabb.Union(box, box);
        Assert.Equal(box, u);
    }

    [Fact]
    public void ExpandToInclude_NewPoint_GrowsBox()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        var expanded = box.ExpandToInclude(new Vec3(2, 2, 2));
        Assert.Equal(Vec3.Zero, expanded.Min);
        Assert.Equal(new Vec3(2, 2, 2), expanded.Max);
    }

    [Fact]
    public void ExpandToInclude_InteriorPoint_NoChange()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        var expanded = box.ExpandToInclude(new Vec3(0.5, 0.5, 0.5));
        Assert.Equal(box, expanded);
    }

    [Fact]
    public void FromTriangle_EnclosesAllVertices()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, -1, 0);
        var c = new Vec3(-2, 5, 7);
        var box = Aabb.FromTriangle(a, b, c);
        Assert.True(box.Contains(a));
        Assert.True(box.Contains(b));
        Assert.True(box.Contains(c));
    }

    [Fact]
    public void FromTriangle_TightBounds()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, -1, 0);
        var c = new Vec3(-2, 5, 7);
        var box = Aabb.FromTriangle(a, b, c);
        Assert.Equal(-2.0, box.Min.X);
        Assert.Equal(-1.0, box.Min.Y);
        Assert.Equal(0.0, box.Min.Z);
        Assert.Equal(4.0, box.Max.X);
        Assert.Equal(5.0, box.Max.Y);
        Assert.Equal(7.0, box.Max.Z);
    }

    [Fact]
    public void FromPoint_MinEqualsMax()
    {
        var p = new Vec3(3, 4, 5);
        var box = Aabb.FromPoint(p);
        Assert.Equal(p, box.Min);
        Assert.Equal(p, box.Max);
    }

    [Fact]
    public void Expand_Margin_GrowsEvenly()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        var expanded = box.Expand(0.5);
        Assert.Equal(new Vec3(-0.5, -0.5, -0.5), expanded.Min);
        Assert.Equal(new Vec3(1.5, 1.5, 1.5), expanded.Max);
    }

    [Fact]
    public void Expand_ZeroMargin_NoChange()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        Assert.Equal(box, box.Expand(0));
    }

    [Fact]
    public void Empty_HasInvertedBounds()
    {
        var empty = Aabb.Empty;
        // Min > Max on all axes (sentinel value)
        Assert.True(empty.Min.X > empty.Max.X);
        Assert.True(empty.Min.Y > empty.Max.Y);
        Assert.True(empty.Min.Z > empty.Max.Z);
    }

    [Fact]
    public void Overlaps_Symmetric()
    {
        var a = new Aabb(Vec3.Zero, new Vec3(2, 3, 4));
        var b = new Aabb(new Vec3(1, 1, 1), new Vec3(5, 5, 5));
        Assert.Equal(a.Overlaps(b), b.Overlaps(a));
    }
}
