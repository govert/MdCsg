using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Aabb boolean algebra — union, overlap, separation, degenerate boxes</summary>
public class AabbBooleanTests
{
    [Fact]
    public void Overlaps_FullyContained_True()
    {
        var outer = new Aabb(new Vec3(0, 0, 0), new Vec3(10, 10, 10));
        var inner = new Aabb(new Vec3(2, 2, 2), new Vec3(8, 8, 8));
        Assert.True(outer.Overlaps(inner));
    }

    [Fact]
    public void Overlaps_Identical_True()
    {
        var a = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        Assert.True(a.Overlaps(a));
    }

    [Fact]
    public void Overlaps_TouchingFace_True()
    {
        var a = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(1, 0, 0), new Vec3(2, 1, 1));
        Assert.True(a.Overlaps(b));
    }

    [Fact]
    public void Overlaps_TouchingEdge_True()
    {
        var a = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(1, 1, 0), new Vec3(2, 2, 1));
        Assert.True(a.Overlaps(b));
    }

    [Fact]
    public void Overlaps_TouchingCorner_True()
    {
        var a = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(1, 1, 1), new Vec3(2, 2, 2));
        Assert.True(a.Overlaps(b));
    }

    [Fact]
    public void Overlaps_SeparatedX_False()
    {
        var a = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(2, 0, 0), new Vec3(3, 1, 1));
        Assert.False(a.Overlaps(b));
    }

    [Fact]
    public void Overlaps_SeparatedY_False()
    {
        var a = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(0, 2, 0), new Vec3(1, 3, 1));
        Assert.False(a.Overlaps(b));
    }

    [Fact]
    public void Overlaps_SeparatedZ_False()
    {
        var a = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(0, 0, 2), new Vec3(1, 1, 3));
        Assert.False(a.Overlaps(b));
    }

    [Fact]
    public void Overlaps_Symmetric()
    {
        var a = new Aabb(new Vec3(0, 0, 0), new Vec3(2, 2, 2));
        var b = new Aabb(new Vec3(1, 1, 1), new Vec3(3, 3, 3));
        Assert.Equal(a.Overlaps(b), b.Overlaps(a));
    }

    [Fact]
    public void Contains_Inside_True()
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
        Assert.False(box.Contains(new Vec3(0, -1, 0)));
    }

    [Fact]
    public void Union_TwoBoxes_ContainsBoth()
    {
        var a = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(2, 2, 2), new Vec3(3, 3, 3));
        var u = Aabb.Union(a, b);
        Assert.True(u.Contains(new Vec3(0.5, 0.5, 0.5)));
        Assert.True(u.Contains(new Vec3(2.5, 2.5, 2.5)));
    }

    [Fact]
    public void Union_Symmetric()
    {
        var a = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(2, 2, 2), new Vec3(3, 3, 3));
        var u1 = Aabb.Union(a, b);
        var u2 = Aabb.Union(b, a);
        Assert.Equal(u1.Min, u2.Min);
        Assert.Equal(u1.Max, u2.Max);
    }

    [Fact]
    public void Union_WithSelf_Unchanged()
    {
        var a = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        var u = Aabb.Union(a, a);
        Assert.Equal(a.Min, u.Min);
        Assert.Equal(a.Max, u.Max);
    }

    [Fact]
    public void SurfaceArea_UnitCube_Is6()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        Assert.Equal(6.0, box.SurfaceArea, 1e-10);
    }

    [Fact]
    public void SurfaceArea_Cuboid()
    {
        // 2×3×4 cuboid: SA = 2*(2*3 + 3*4 + 4*2) = 2*(6+12+8) = 52
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(2, 3, 4));
        Assert.Equal(52.0, box.SurfaceArea, 1e-10);
    }

    [Fact]
    public void SurfaceArea_FlatBox_IsZero()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 0));
        Assert.Equal(2.0, box.SurfaceArea, 1e-10); // 2*(1*1 + 1*0 + 0*1) = 2
    }

    [Fact]
    public void FromTriangle_ContainsAllVertices()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 0, 1);
        var c = new Vec3(2, 5, 0);
        var box = Aabb.FromTriangle(a, b, c);
        Assert.True(box.Contains(a));
        Assert.True(box.Contains(b));
        Assert.True(box.Contains(c));
    }

    [Fact]
    public void FromTriangle_TightBounds()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 0, 1);
        var c = new Vec3(2, 5, 0);
        var box = Aabb.FromTriangle(a, b, c);
        Assert.Equal(1, box.Min.X);
        Assert.Equal(0, box.Min.Y);
        Assert.Equal(0, box.Min.Z);
        Assert.Equal(4, box.Max.X);
        Assert.Equal(5, box.Max.Y);
        Assert.Equal(3, box.Max.Z);
    }

    [Fact]
    public void NegativeCoordinates_CorrectBounds()
    {
        var box = new Aabb(new Vec3(-3, -2, -1), new Vec3(1, 2, 3));
        Assert.True(box.Contains(Vec3.Zero));
        Assert.True(box.Contains(new Vec3(-3, -2, -1)));
        Assert.False(box.Contains(new Vec3(-4, 0, 0)));
    }
}
