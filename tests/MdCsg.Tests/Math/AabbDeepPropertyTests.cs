using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Aabb — Overlaps, Contains, Union, Expand, SurfaceArea, FromTriangle, FromPoint</summary>
public class AabbDeepPropertyTests
{
    [Fact]
    public void Overlaps_SameBox_True()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        Assert.True(box.Overlaps(box));
    }

    [Fact]
    public void Overlaps_Adjacent_True()
    {
        var a = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(1, 0, 0), new Vec3(2, 1, 1));
        Assert.True(a.Overlaps(b));
        Assert.True(b.Overlaps(a));
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
    public void Contains_Interior_True()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        Assert.True(box.Contains(new Vec3(0.5, 0.5, 0.5)));
    }

    [Fact]
    public void Contains_Corner_True()
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
    public void Union_TwoBoxes_EnclosesBoth()
    {
        var a = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(2, 2, 2), new Vec3(3, 3, 3));
        var u = Aabb.Union(a, b);
        Assert.Equal(0, u.Min.X);
        Assert.Equal(3, u.Max.X);
    }

    [Fact]
    public void Union_IsCommutative()
    {
        var a = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(-1, 2, -1), new Vec3(3, 4, 2));
        var u1 = Aabb.Union(a, b);
        var u2 = Aabb.Union(b, a);
        Assert.Equal(u1, u2);
    }

    [Fact]
    public void ExpandToInclude_NewPoint_GrowsBox()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        var grown = box.ExpandToInclude(new Vec3(5, 5, 5));
        Assert.True(grown.Contains(new Vec3(5, 5, 5)));
        Assert.Equal(5, grown.Max.X);
    }

    [Fact]
    public void ExpandToInclude_InteriorPoint_NoChange()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        var same = box.ExpandToInclude(new Vec3(0.5, 0.5, 0.5));
        Assert.Equal(box, same);
    }

    [Fact]
    public void FromTriangle_EnclosesAllVertices()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var c = new Vec3(7, 2, 1);
        var box = Aabb.FromTriangle(a, b, c);
        Assert.True(box.Contains(a));
        Assert.True(box.Contains(b));
        Assert.True(box.Contains(c));
    }

    [Fact]
    public void FromPoint_CreatesDegenerateBox()
    {
        var p = new Vec3(1, 2, 3);
        var box = Aabb.FromPoint(p);
        Assert.Equal(p, box.Min);
        Assert.Equal(p, box.Max);
        Assert.True(box.Contains(p));
    }

    [Fact]
    public void Size_UnitCube()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        Assert.Equal(1, box.Size.X);
        Assert.Equal(1, box.Size.Y);
        Assert.Equal(1, box.Size.Z);
    }

    [Fact]
    public void Center_UnitCube()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(2, 4, 6));
        Assert.Equal(1, box.Center.X);
        Assert.Equal(2, box.Center.Y);
        Assert.Equal(3, box.Center.Z);
    }

    [Fact]
    public void SurfaceArea_UnitCube()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        Assert.True(System.Math.Abs(box.SurfaceArea - 6.0) < 1e-10);
    }

    [Fact]
    public void SurfaceArea_Rectangular()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(2, 3, 4));
        // 2*(2*3 + 3*4 + 4*2) = 2*(6+12+8) = 52
        Assert.True(System.Math.Abs(box.SurfaceArea - 52.0) < 1e-10);
    }

    [Fact]
    public void Expand_ByMargin_GrowsCorrectly()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        var expanded = box.Expand(0.5);
        Assert.True(System.Math.Abs(expanded.Min.X - (-0.5)) < 1e-10);
        Assert.True(System.Math.Abs(expanded.Max.X - 1.5) < 1e-10);
    }

    [Fact]
    public void Empty_HasInvalidBounds()
    {
        var empty = Aabb.Empty;
        Assert.True(empty.Min.X > empty.Max.X);
    }

    [Fact]
    public void Empty_ExpandToInclude_BecomesPoint()
    {
        var box = Aabb.Empty.ExpandToInclude(new Vec3(1, 2, 3));
        Assert.Equal(1, box.Min.X);
        Assert.Equal(1, box.Max.X);
    }

    [Fact]
    public void Overlaps_PartialOverlap_True()
    {
        var a = new Aabb(new Vec3(0, 0, 0), new Vec3(2, 2, 2));
        var b = new Aabb(new Vec3(1, 1, 1), new Vec3(3, 3, 3));
        Assert.True(a.Overlaps(b));
    }
}
