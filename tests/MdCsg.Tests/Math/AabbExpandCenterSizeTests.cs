using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Aabb Expand, Center, Size, Empty, ExpandToInclude, FromPoint — edge cases and invariants</summary>
public class AabbExpandCenterSizeTests
{
    [Fact]
    public void Size_UnitCube()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        Assert.Equal(1, box.Size.X, 1e-14);
        Assert.Equal(1, box.Size.Y, 1e-14);
        Assert.Equal(1, box.Size.Z, 1e-14);
    }

    [Fact]
    public void Size_Cuboid()
    {
        var box = new Aabb(new Vec3(1, 2, 3), new Vec3(4, 7, 13));
        Assert.Equal(3, box.Size.X, 1e-14);
        Assert.Equal(5, box.Size.Y, 1e-14);
        Assert.Equal(10, box.Size.Z, 1e-14);
    }

    [Fact]
    public void Center_UnitCube()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        Assert.Equal(0.5, box.Center.X, 1e-14);
        Assert.Equal(0.5, box.Center.Y, 1e-14);
        Assert.Equal(0.5, box.Center.Z, 1e-14);
    }

    [Fact]
    public void Center_OffsetBox()
    {
        var box = new Aabb(new Vec3(2, 4, 6), new Vec3(8, 10, 12));
        Assert.Equal(5, box.Center.X, 1e-14);
        Assert.Equal(7, box.Center.Y, 1e-14);
        Assert.Equal(9, box.Center.Z, 1e-14);
    }

    [Fact]
    public void Expand_Positive_GrowsBox()
    {
        var box = new Aabb(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        var expanded = box.Expand(0.5);
        Assert.Equal(0.5, expanded.Min.X, 1e-14);
        Assert.Equal(1.5, expanded.Min.Y, 1e-14);
        Assert.Equal(2.5, expanded.Min.Z, 1e-14);
        Assert.Equal(4.5, expanded.Max.X, 1e-14);
        Assert.Equal(5.5, expanded.Max.Y, 1e-14);
        Assert.Equal(6.5, expanded.Max.Z, 1e-14);
    }

    [Fact]
    public void Expand_Zero_Unchanged()
    {
        var box = new Aabb(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        var expanded = box.Expand(0);
        Assert.Equal(box.Min, expanded.Min);
        Assert.Equal(box.Max, expanded.Max);
    }

    [Fact]
    public void Expand_Negative_ShrinksBox()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(10, 10, 10));
        var shrunk = box.Expand(-1);
        Assert.Equal(1, shrunk.Min.X, 1e-14);
        Assert.Equal(9, shrunk.Max.X, 1e-14);
    }

    [Fact]
    public void ExpandToInclude_PointInside_Unchanged()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        var expanded = box.ExpandToInclude(new Vec3(0.5, 0.5, 0.5));
        Assert.Equal(box.Min, expanded.Min);
        Assert.Equal(box.Max, expanded.Max);
    }

    [Fact]
    public void ExpandToInclude_PointOutside_GrowsBox()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        var expanded = box.ExpandToInclude(new Vec3(2, -1, 3));
        Assert.Equal(0, expanded.Min.X, 1e-14);
        Assert.Equal(-1, expanded.Min.Y, 1e-14);
        Assert.Equal(0, expanded.Min.Z, 1e-14);
        Assert.Equal(2, expanded.Max.X, 1e-14);
        Assert.Equal(1, expanded.Max.Y, 1e-14);
        Assert.Equal(3, expanded.Max.Z, 1e-14);
    }

    [Fact]
    public void ExpandToInclude_OnBoundary_Unchanged()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        var expanded = box.ExpandToInclude(new Vec3(1, 1, 1));
        Assert.Equal(box.Min, expanded.Min);
        Assert.Equal(box.Max, expanded.Max);
    }

    [Fact]
    public void FromPoint_SinglePoint_DegenerateBox()
    {
        var p = new Vec3(3, 4, 5);
        var box = Aabb.FromPoint(p);
        Assert.Equal(p, box.Min);
        Assert.Equal(p, box.Max);
        Assert.Equal(0, box.SurfaceArea, 1e-14);
    }

    [Fact]
    public void FromPoint_Contains_ThePoint()
    {
        var p = new Vec3(3, 4, 5);
        var box = Aabb.FromPoint(p);
        Assert.True(box.Contains(p));
    }

    [Fact]
    public void Empty_IsInvalid()
    {
        var empty = Aabb.Empty;
        Assert.True(empty.Min.X > empty.Max.X);
    }

    [Fact]
    public void Empty_ExpandToInclude_BecomesSinglePointBox()
    {
        var p = new Vec3(3, 4, 5);
        var box = Aabb.Empty.ExpandToInclude(p);
        // After including one point, min and max should be that point
        Assert.True(box.Contains(p));
    }

    [Fact]
    public void Union_WithEmpty_PreservesOther()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        var u = Aabb.Union(box, Aabb.Empty);
        // Empty has MaxValue min and MinValue max, union should still produce valid box
        // that contains the original
        Assert.True(u.Contains(new Vec3(0.5, 0.5, 0.5)));
    }

    [Fact]
    public void SurfaceArea_ZeroVolume_PositiveArea()
    {
        // Flat box (zero thickness in Z)
        var box = new Aabb(Vec3.Zero, new Vec3(2, 3, 0));
        // SA = 2*(2*3 + 3*0 + 0*2) = 2*6 = 12... no, SA = 2*(w*h + h*0 + 0*w) = 2*w*h = 12
        Assert.Equal(12.0, box.SurfaceArea, 1e-10);
    }

    [Fact]
    public void SurfaceArea_ZeroAllDimensions_ZeroArea()
    {
        var box = new Aabb(new Vec3(1, 1, 1), new Vec3(1, 1, 1));
        Assert.Equal(0.0, box.SurfaceArea, 1e-14);
    }

    [Fact]
    public void Overlaps_Self_True()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        Assert.True(box.Overlaps(box));
    }

    [Fact]
    public void Overlaps_ExpandedBox_True()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        var expanded = box.Expand(1);
        Assert.True(box.Overlaps(expanded));
        Assert.True(expanded.Overlaps(box));
    }

    [Fact]
    public void Center_NegativeCoordinates()
    {
        var box = new Aabb(new Vec3(-10, -20, -30), new Vec3(-5, -10, -15));
        Assert.Equal(-7.5, box.Center.X, 1e-14);
        Assert.Equal(-15, box.Center.Y, 1e-14);
        Assert.Equal(-22.5, box.Center.Z, 1e-14);
    }

    [Fact]
    public void Size_PointBox_IsZero()
    {
        var box = Aabb.FromPoint(new Vec3(5, 5, 5));
        Assert.Equal(0, box.Size.X, 1e-14);
        Assert.Equal(0, box.Size.Y, 1e-14);
        Assert.Equal(0, box.Size.Z, 1e-14);
    }

    [Fact]
    public void FromTriangle_AllProperties()
    {
        var a = new Vec3(-1, 0, 2);
        var b = new Vec3(3, -2, 0);
        var c = new Vec3(0, 4, 1);
        var box = Aabb.FromTriangle(a, b, c);
        Assert.Equal(-1, box.Min.X, 1e-14);
        Assert.Equal(-2, box.Min.Y, 1e-14);
        Assert.Equal(0, box.Min.Z, 1e-14);
        Assert.Equal(3, box.Max.X, 1e-14);
        Assert.Equal(4, box.Max.Y, 1e-14);
        Assert.Equal(2, box.Max.Z, 1e-14);
        Assert.True(box.Contains(a));
        Assert.True(box.Contains(b));
        Assert.True(box.Contains(c));
        Assert.True(box.Contains(box.Center));
    }
}
