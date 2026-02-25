using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Aabb — SurfaceArea, FromTriangle, FromPoint, Union, Contains, ExpandToInclude</summary>
public class AabbSurfaceAreaTriangleTests
{
    [Fact]
    public void SurfaceArea_UnitCube()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        Assert.Equal(6.0, box.SurfaceArea, 1e-14);
    }

    [Fact]
    public void SurfaceArea_FlatBox()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(2, 3, 0));
        Assert.Equal(2 * (2 * 3 + 3 * 0 + 0 * 2), box.SurfaceArea, 1e-14);
    }

    [Fact]
    public void SurfaceArea_Point_IsZero()
    {
        var box = Aabb.FromPoint(new Vec3(1, 2, 3));
        Assert.Equal(0, box.SurfaceArea, 1e-14);
    }

    [Fact]
    public void SurfaceArea_Rectangular()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(2, 3, 5));
        Assert.Equal(2 * (2 * 3 + 3 * 5 + 5 * 2), box.SurfaceArea, 1e-14);
    }

    [Fact]
    public void FromTriangle_EnclosesTri()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(3, 0, 0);
        var c = new Vec3(0, 4, 0);
        var box = Aabb.FromTriangle(a, b, c);
        Assert.True(box.Contains(a));
        Assert.True(box.Contains(b));
        Assert.True(box.Contains(c));
    }

    [Fact]
    public void FromTriangle_Tight()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 2, 3);
        var c = new Vec3(1, 5, 3);
        var box = Aabb.FromTriangle(a, b, c);
        Assert.Equal(1, box.Min.X, 1e-14);
        Assert.Equal(2, box.Min.Y, 1e-14);
        Assert.Equal(3, box.Min.Z, 1e-14);
        Assert.Equal(4, box.Max.X, 1e-14);
        Assert.Equal(5, box.Max.Y, 1e-14);
        Assert.Equal(3, box.Max.Z, 1e-14);
    }

    [Fact]
    public void FromPoint_MinEqualsMax()
    {
        var p = new Vec3(1, 2, 3);
        var box = Aabb.FromPoint(p);
        Assert.Equal(p, box.Min);
        Assert.Equal(p, box.Max);
    }

    [Fact]
    public void Union_EnclosesAll()
    {
        var a = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(2, 2, 2), new Vec3(3, 3, 3));
        var u = Aabb.Union(a, b);
        Assert.Equal(0, u.Min.X, 1e-14);
        Assert.Equal(3, u.Max.X, 1e-14);
    }

    [Fact]
    public void Union_Symmetric()
    {
        var a = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(2, 2, 2), new Vec3(3, 3, 3));
        Assert.Equal(Aabb.Union(a, b), Aabb.Union(b, a));
    }

    [Fact]
    public void Contains_Interior_True()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        Assert.True(box.Contains(new Vec3(0.5, 0.5, 0.5)));
    }

    [Fact]
    public void Contains_OnBoundary_True()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        Assert.True(box.Contains(new Vec3(0, 0, 0)));
        Assert.True(box.Contains(new Vec3(1, 1, 1)));
    }

    [Fact]
    public void Contains_Outside_False()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        Assert.False(box.Contains(new Vec3(2, 0, 0)));
    }

    [Fact]
    public void ExpandToInclude_GrowsBox()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        var expanded = box.ExpandToInclude(new Vec3(5, 5, 5));
        Assert.Equal(0, expanded.Min.X, 1e-14);
        Assert.Equal(5, expanded.Max.X, 1e-14);
    }

    [Fact]
    public void ExpandToInclude_PointInside_NoChange()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(2, 2, 2));
        var expanded = box.ExpandToInclude(new Vec3(1, 1, 1));
        Assert.Equal(box, expanded);
    }

    [Fact]
    public void Empty_IsValid()
    {
        var empty = Aabb.Empty;
        // Empty box should have Min > Max in all components
        Assert.True(empty.Min.X > empty.Max.X);
    }

    [Fact]
    public void Overlaps_Adjacent_True()
    {
        var a = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(1, 0, 0), new Vec3(2, 1, 1));
        Assert.True(a.Overlaps(b));
    }

    [Fact]
    public void Overlaps_Separated_False()
    {
        var a = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(2, 2, 2), new Vec3(3, 3, 3));
        Assert.False(a.Overlaps(b));
    }

    [Fact]
    public void Overlaps_Symmetric()
    {
        var a = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(0.5, 0.5, 0.5), new Vec3(1.5, 1.5, 1.5));
        Assert.Equal(a.Overlaps(b), b.Overlaps(a));
    }
}
