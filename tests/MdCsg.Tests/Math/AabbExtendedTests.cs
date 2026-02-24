using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Batch 41: Extended AABB tests (20 tests)</summary>
public class AabbExtendedTests
{
    [Fact]
    public void Empty_HasInvertedBounds()
    {
        var empty = Aabb.Empty;
        Assert.True(empty.Min.X > empty.Max.X);
    }

    [Fact]
    public void Size_CorrectForUnitBox()
    {
        var box = new Aabb(Vec3.Zero, Vec3.UnitX + Vec3.UnitY + Vec3.UnitZ);
        var size = box.Size;
        Assert.True(System.Math.Abs(size.X - 1) < 1e-10);
        Assert.True(System.Math.Abs(size.Y - 1) < 1e-10);
        Assert.True(System.Math.Abs(size.Z - 1) < 1e-10);
    }

    [Fact]
    public void Center_CorrectForUnitBox()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(2, 4, 6));
        var center = box.Center;
        Assert.True(System.Math.Abs(center.X - 1) < 1e-10);
        Assert.True(System.Math.Abs(center.Y - 2) < 1e-10);
        Assert.True(System.Math.Abs(center.Z - 3) < 1e-10);
    }

    [Fact]
    public void SurfaceArea_UnitCube()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        Assert.True(System.Math.Abs(box.SurfaceArea - 6.0) < 1e-10);
    }

    [Fact]
    public void SurfaceArea_Rectangular()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(2, 3, 5));
        // 2*(2*3 + 3*5 + 5*2) = 2*(6+15+10) = 62
        Assert.True(System.Math.Abs(box.SurfaceArea - 62.0) < 1e-10);
    }

    [Fact]
    public void Overlaps_DisjointOnXAxis()
    {
        var a = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(2, 0, 0), new Vec3(3, 1, 1));
        Assert.False(a.Overlaps(b));
    }

    [Fact]
    public void Overlaps_DisjointOnYAxis()
    {
        var a = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(0, 2, 0), new Vec3(1, 3, 1));
        Assert.False(a.Overlaps(b));
    }

    [Fact]
    public void Overlaps_DisjointOnZAxis()
    {
        var a = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(0, 0, 2), new Vec3(1, 1, 3));
        Assert.False(a.Overlaps(b));
    }

    [Fact]
    public void Contains_BoundaryPoints()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        Assert.True(box.Contains(Vec3.Zero));         // min corner
        Assert.True(box.Contains(new Vec3(1, 1, 1))); // max corner
        Assert.True(box.Contains(new Vec3(1, 0, 0))); // edge
    }

    [Fact]
    public void Contains_Outside_ReturnsFalse()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        Assert.False(box.Contains(new Vec3(-0.001, 0.5, 0.5)));
        Assert.False(box.Contains(new Vec3(0.5, 1.001, 0.5)));
    }

    [Fact]
    public void Union_SameBox_ReturnsSame()
    {
        var box = new Aabb(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        var u = Aabb.Union(box, box);
        Assert.Equal(box.Min, u.Min);
        Assert.Equal(box.Max, u.Max);
    }

    [Fact]
    public void Union_WithEmpty_ExpandsCorrectly()
    {
        var box = new Aabb(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        // Union with empty should keep the box (since empty has inverted bounds)
        var u = Aabb.Union(Aabb.Empty, box);
        Assert.True(u.Min.X <= box.Min.X);
        Assert.True(u.Max.X >= box.Max.X);
    }

    [Fact]
    public void ExpandToInclude_ExtendsMin()
    {
        var box = new Aabb(new Vec3(1, 1, 1), new Vec3(2, 2, 2));
        var expanded = box.ExpandToInclude(new Vec3(-1, 0, 0));
        Assert.True(System.Math.Abs(expanded.Min.X - (-1)) < 1e-10);
        Assert.True(System.Math.Abs(expanded.Min.Y - 0) < 1e-10);
    }

    [Fact]
    public void ExpandToInclude_ExtendsMax()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        var expanded = box.ExpandToInclude(new Vec3(5, 3, 2));
        Assert.True(System.Math.Abs(expanded.Max.X - 5) < 1e-10);
        Assert.True(System.Math.Abs(expanded.Max.Y - 3) < 1e-10);
    }

    [Fact]
    public void ExpandToInclude_InsidePoint_Unchanged()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(10, 10, 10));
        var expanded = box.ExpandToInclude(new Vec3(5, 5, 5));
        Assert.Equal(box.Min, expanded.Min);
        Assert.Equal(box.Max, expanded.Max);
    }

    [Fact]
    public void FromTriangle_DegenerateOnAxis()
    {
        var a = new Vec3(1, 0, 0);
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(3, 0, 0);
        var box = Aabb.FromTriangle(a, b, c);
        Assert.True(System.Math.Abs(box.Min.X - 1) < 1e-10);
        Assert.True(System.Math.Abs(box.Max.X - 3) < 1e-10);
        Assert.True(System.Math.Abs(box.Size.Y) < 1e-10);
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
    public void Expand_IncreasesSize()
    {
        var box = new Aabb(new Vec3(1, 1, 1), new Vec3(2, 2, 2));
        var expanded = box.Expand(0.5);
        Assert.True(System.Math.Abs(expanded.Min.X - 0.5) < 1e-10);
        Assert.True(System.Math.Abs(expanded.Max.X - 2.5) < 1e-10);
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
    public void Overlaps_PartialOverlap_OnlyXY()
    {
        var a = new Aabb(Vec3.Zero, new Vec3(2, 2, 1));
        var b = new Aabb(new Vec3(1, 1, 0), new Vec3(3, 3, 1));
        Assert.True(a.Overlaps(b));
    }
}
