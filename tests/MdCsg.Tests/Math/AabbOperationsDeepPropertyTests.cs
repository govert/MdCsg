using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Aabb — Overlaps, Contains, Union, Expand, FromTriangle, SurfaceArea, Center, Size deep coverage</summary>
public class AabbOperationsDeepPropertyTests
{
    [Fact]
    public void Overlaps_Identical_True()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        Assert.True(box.Overlaps(box));
    }

    [Fact]
    public void Overlaps_Containing_True()
    {
        var outer = new Aabb(new Vec3(-1, -1, -1), new Vec3(2, 2, 2));
        var inner = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        Assert.True(outer.Overlaps(inner));
        Assert.True(inner.Overlaps(outer));
    }

    [Fact]
    public void Overlaps_Disjoint_False()
    {
        var a = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(2, 2, 2), new Vec3(3, 3, 3));
        Assert.False(a.Overlaps(b));
        Assert.False(b.Overlaps(a));
    }

    [Fact]
    public void Overlaps_TouchingEdge_True()
    {
        var a = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(1, 0, 0), new Vec3(2, 1, 1));
        Assert.True(a.Overlaps(b));
    }

    [Fact]
    public void Contains_Interior_True()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        Assert.True(box.Contains(new Vec3(0.5, 0.5, 0.5)));
    }

    [Fact]
    public void Contains_OnBoundary_True()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        Assert.True(box.Contains(new Vec3(0, 0, 0)));
        Assert.True(box.Contains(new Vec3(1, 1, 1)));
    }

    [Fact]
    public void Contains_Outside_False()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        Assert.False(box.Contains(new Vec3(2, 0, 0)));
        Assert.False(box.Contains(new Vec3(-1, 0, 0)));
    }

    [Fact]
    public void Union_ExpandsToContainBoth()
    {
        var a = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(2, 2, 2), new Vec3(3, 3, 3));
        var u = Aabb.Union(a, b);
        Assert.Equal(0.0, u.Min.X);
        Assert.Equal(0.0, u.Min.Y);
        Assert.Equal(0.0, u.Min.Z);
        Assert.Equal(3.0, u.Max.X);
        Assert.Equal(3.0, u.Max.Y);
        Assert.Equal(3.0, u.Max.Z);
    }

    [Fact]
    public void Union_WithSelf_Unchanged()
    {
        var box = new Aabb(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        var u = Aabb.Union(box, box);
        Assert.Equal(box, u);
    }

    [Fact]
    public void ExpandToInclude_ExtendsMinMax()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        var expanded = box.ExpandToInclude(new Vec3(5, -3, 2));
        Assert.Equal(0.0, expanded.Min.X);
        Assert.Equal(-3.0, expanded.Min.Y);
        Assert.Equal(0.0, expanded.Min.Z);
        Assert.Equal(5.0, expanded.Max.X);
        Assert.Equal(1.0, expanded.Max.Y);
        Assert.Equal(2.0, expanded.Max.Z);
    }

    [Fact]
    public void ExpandToInclude_InsidePoint_Unchanged()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(10, 10, 10));
        var expanded = box.ExpandToInclude(new Vec3(5, 5, 5));
        Assert.Equal(box, expanded);
    }

    [Fact]
    public void FromTriangle_EnclosesVertices()
    {
        var a = new Vec3(1, 5, 3);
        var b = new Vec3(4, 2, 7);
        var c = new Vec3(6, 8, 1);
        var box = Aabb.FromTriangle(a, b, c);
        Assert.True(box.Contains(a));
        Assert.True(box.Contains(b));
        Assert.True(box.Contains(c));
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
    public void SurfaceArea_UnitCube_IsSix()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        Assert.Equal(6.0, box.SurfaceArea, 10);
    }

    [Fact]
    public void SurfaceArea_Rectangle_Correct()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(2, 3, 4));
        // 2(2*3 + 3*4 + 4*2) = 2(6+12+8) = 52
        Assert.Equal(52.0, box.SurfaceArea, 10);
    }

    [Fact]
    public void Center_UnitCube_IsHalf()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        Assert.Equal(new Vec3(0.5, 0.5, 0.5), box.Center);
    }

    [Fact]
    public void Size_Correct()
    {
        var box = new Aabb(new Vec3(1, 2, 3), new Vec3(4, 6, 9));
        Assert.Equal(new Vec3(3, 4, 6), box.Size);
    }

    [Fact]
    public void Expand_GrowsByMarginBothSides()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        var expanded = box.Expand(0.5);
        Assert.Equal(-0.5, expanded.Min.X);
        Assert.Equal(-0.5, expanded.Min.Y);
        Assert.Equal(-0.5, expanded.Min.Z);
        Assert.Equal(1.5, expanded.Max.X);
        Assert.Equal(1.5, expanded.Max.Y);
        Assert.Equal(1.5, expanded.Max.Z);
    }

    [Fact]
    public void Empty_Size_IsNegative()
    {
        // Empty AABB has MaxValue/MinValue → negative size dimensions
        var size = Aabb.Empty.Size;
        Assert.True(size.X < 0);
        Assert.True(size.Y < 0);
        Assert.True(size.Z < 0);
    }
}
