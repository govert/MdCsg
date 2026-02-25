using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Aabb — Union containment, Expand margin, Overlaps symmetry, SurfaceArea, FromTriangle vertices</summary>
public class AabbUnionContainmentPropertyTests
{
    [Fact]
    public void UnitCube_Size_IsOne()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        Assert.Equal(1.0, box.Size.X);
        Assert.Equal(1.0, box.Size.Y);
        Assert.Equal(1.0, box.Size.Z);
    }

    [Fact]
    public void UnitCube_Center_IsHalf()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        Assert.Equal(0.5, box.Center.X);
        Assert.Equal(0.5, box.Center.Y);
        Assert.Equal(0.5, box.Center.Z);
    }

    [Fact]
    public void UnitCube_SurfaceArea_IsSix()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        Assert.True(System.Math.Abs(box.SurfaceArea - 6.0) < 1e-10);
    }

    [Fact]
    public void SurfaceArea_2x3x4_Is52()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(2, 3, 4));
        Assert.True(System.Math.Abs(box.SurfaceArea - 52.0) < 1e-10);
    }

    [Fact]
    public void Contains_PointInside_True()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        Assert.True(box.Contains(new Vec3(0.5, 0.5, 0.5)));
    }

    [Fact]
    public void Contains_PointOutside_False()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        Assert.False(box.Contains(new Vec3(2, 0.5, 0.5)));
    }

    [Fact]
    public void Contains_PointOnBoundary_True()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        Assert.True(box.Contains(new Vec3(1, 1, 1)));
        Assert.True(box.Contains(Vec3.Zero));
    }

    [Fact]
    public void Overlaps_OverlappingBoxes_True()
    {
        var a = new Aabb(Vec3.Zero, new Vec3(2, 2, 2));
        var b = new Aabb(new Vec3(1, 1, 1), new Vec3(3, 3, 3));
        Assert.True(a.Overlaps(b));
        Assert.True(b.Overlaps(a));
    }

    [Fact]
    public void Overlaps_DisjointBoxes_False()
    {
        var a = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(5, 5, 5), new Vec3(6, 6, 6));
        Assert.False(a.Overlaps(b));
        Assert.False(b.Overlaps(a));
    }

    [Fact]
    public void Overlaps_TouchingFaces_True()
    {
        var a = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(1, 0, 0), new Vec3(2, 1, 1));
        Assert.True(a.Overlaps(b));
    }

    [Fact]
    public void Union_TwoBoxes_ContainsBoth()
    {
        var a = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(2, 2, 2), new Vec3(3, 3, 3));
        var u = Aabb.Union(a, b);
        Assert.True(u.Contains(new Vec3(0.5, 0.5, 0.5)));
        Assert.True(u.Contains(new Vec3(2.5, 2.5, 2.5)));
    }

    [Fact]
    public void Union_Commutative()
    {
        var a = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(2, 3, 4), new Vec3(5, 6, 7));
        Assert.Equal(Aabb.Union(a, b), Aabb.Union(b, a));
    }

    [Fact]
    public void ExpandToInclude_PointOutside_BoxGrows()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        var expanded = box.ExpandToInclude(new Vec3(5, 5, 5));
        Assert.True(expanded.Contains(new Vec3(5, 5, 5)));
        Assert.True(expanded.Contains(Vec3.Zero));
    }

    [Fact]
    public void ExpandToInclude_PointInside_BoxUnchanged()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(2, 2, 2));
        var expanded = box.ExpandToInclude(new Vec3(1, 1, 1));
        Assert.Equal(box, expanded);
    }

    [Fact]
    public void FromTriangle_ContainsAllVertices()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var c = new Vec3(7, 0, 1);
        var box = Aabb.FromTriangle(a, b, c);
        Assert.True(box.Contains(a));
        Assert.True(box.Contains(b));
        Assert.True(box.Contains(c));
    }

    [Fact]
    public void FromPoint_ContainsPoint()
    {
        var p = new Vec3(3, 4, 5);
        var box = Aabb.FromPoint(p);
        Assert.True(box.Contains(p));
    }

    [Fact]
    public void Expand_IncreasesSize()
    {
        var box = new Aabb(Vec3.Zero, new Vec3(1, 1, 1));
        var expanded = box.Expand(0.5);
        Assert.True(System.Math.Abs(expanded.Min.X - (-0.5)) < 1e-10);
        Assert.True(System.Math.Abs(expanded.Max.X - 1.5) < 1e-10);
    }

    [Fact]
    public void Overlaps_Symmetric()
    {
        var a = new Aabb(new Vec3(0, 0, 0), new Vec3(3, 3, 3));
        var b = new Aabb(new Vec3(1, 1, 1), new Vec3(5, 5, 5));
        Assert.Equal(a.Overlaps(b), b.Overlaps(a));
    }

    [Fact]
    public void Union_WithSelf_IsSelf()
    {
        var box = new Aabb(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        Assert.Equal(box, Aabb.Union(box, box));
    }
}
