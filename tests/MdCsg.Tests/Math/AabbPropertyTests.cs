using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: AABB property tests — construction, contains, overlaps, union, expand</summary>
public class AabbPropertyTests
{
    [Fact]
    public void FromTriangle_ContainsAllVertices()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 0, 6);
        var c = new Vec3(2, 5, 1);
        var aabb = Aabb.FromTriangle(a, b, c);
        Assert.True(aabb.Contains(a));
        Assert.True(aabb.Contains(b));
        Assert.True(aabb.Contains(c));
    }

    [Fact]
    public void FromPoint_MinEqualsMax()
    {
        var p = new Vec3(3, 4, 5);
        var aabb = Aabb.FromPoint(p);
        Assert.Equal(p, aabb.Min);
        Assert.Equal(p, aabb.Max);
    }

    [Fact]
    public void Size_Correct()
    {
        var aabb = new Aabb(new Vec3(0, 0, 0), new Vec3(3, 4, 5));
        Assert.Equal(new Vec3(3, 4, 5), aabb.Size);
    }

    [Fact]
    public void Center_Correct()
    {
        var aabb = new Aabb(new Vec3(0, 0, 0), new Vec3(10, 20, 30));
        Assert.Equal(new Vec3(5, 10, 15), aabb.Center);
    }

    [Fact]
    public void SurfaceArea_UnitCube()
    {
        var aabb = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        Assert.Equal(6.0, aabb.SurfaceArea, 12);
    }

    [Fact]
    public void SurfaceArea_Rectangle()
    {
        var aabb = new Aabb(Vec3.Zero, new Vec3(2, 3, 5));
        Assert.Equal(62.0, aabb.SurfaceArea, 12);
    }

    [Fact]
    public void Contains_InsidePoint()
    {
        var aabb = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        Assert.True(aabb.Contains(new Vec3(0.5, 0.5, 0.5)));
    }

    [Fact]
    public void Contains_OutsidePoint()
    {
        var aabb = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        Assert.False(aabb.Contains(new Vec3(2, 0.5, 0.5)));
    }

    [Fact]
    public void Contains_BoundaryPoint()
    {
        var aabb = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        Assert.True(aabb.Contains(new Vec3(1, 1, 1)));
        Assert.True(aabb.Contains(Vec3.Zero));
    }

    [Fact]
    public void Overlaps_Self()
    {
        var aabb = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        Assert.True(aabb.Overlaps(aabb));
    }

    [Fact]
    public void Overlaps_Disjoint()
    {
        var a = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(5, 5, 5), new Vec3(6, 6, 6));
        Assert.False(a.Overlaps(b));
        Assert.False(b.Overlaps(a));
    }

    [Fact]
    public void Overlaps_Touching()
    {
        var a = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(1, 0, 0), new Vec3(2, 1, 1));
        Assert.True(a.Overlaps(b));
    }

    [Fact]
    public void Union_ContainsBoth()
    {
        var a = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(2, 2, 2), new Vec3(3, 3, 3));
        var u = Aabb.Union(a, b);
        Assert.Equal(Vec3.Zero, u.Min);
        Assert.Equal(new Vec3(3, 3, 3), u.Max);
    }

    [Fact]
    public void ExpandToInclude_GrowsBox()
    {
        var aabb = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        var expanded = aabb.ExpandToInclude(new Vec3(5, 5, 5));
        Assert.True(expanded.Contains(new Vec3(5, 5, 5)));
        Assert.Equal(Vec3.Zero, expanded.Min);
    }

    [Fact]
    public void Expand_Margin()
    {
        var aabb = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        var expanded = aabb.Expand(0.5);
        Assert.Equal(new Vec3(-0.5, -0.5, -0.5), expanded.Min);
        Assert.Equal(new Vec3(1.5, 1.5, 1.5), expanded.Max);
    }

    [Fact]
    public void Empty_HasMaxMinValues()
    {
        Assert.Equal(double.MaxValue, Aabb.Empty.Min.X);
        Assert.Equal(double.MinValue, Aabb.Empty.Max.X);
    }

    [Fact]
    public void RecordEquality()
    {
        var a = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        var b = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        Assert.Equal(a, b);
    }

    [Fact]
    public void FromTriangle_TightBounds()
    {
        var a = new Vec3(-1, 0, 2);
        var b = new Vec3(3, -2, 0);
        var c = new Vec3(0, 4, -1);
        var aabb = Aabb.FromTriangle(a, b, c);
        Assert.Equal(-1, aabb.Min.X);
        Assert.Equal(-2, aabb.Min.Y);
        Assert.Equal(-1, aabb.Min.Z);
        Assert.Equal(3, aabb.Max.X);
        Assert.Equal(4, aabb.Max.Y);
        Assert.Equal(2, aabb.Max.Z);
    }
}
