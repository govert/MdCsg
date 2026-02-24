using MdCsg.Math;

namespace MdCsg.Tests.Math;

public class AabbTests
{
    [Fact]
    public void Overlaps_WhenOverlapping_ReturnsTrue()
    {
        var a = new Aabb(new Vec3(0, 0, 0), new Vec3(2, 2, 2));
        var b = new Aabb(new Vec3(1, 1, 1), new Vec3(3, 3, 3));
        Assert.True(a.Overlaps(b));
        Assert.True(b.Overlaps(a));
    }

    [Fact]
    public void Overlaps_WhenDisjoint_ReturnsFalse()
    {
        var a = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(2, 2, 2), new Vec3(3, 3, 3));
        Assert.False(a.Overlaps(b));
    }

    [Fact]
    public void Overlaps_WhenTouching_ReturnsTrue()
    {
        var a = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(1, 1, 1), new Vec3(2, 2, 2));
        Assert.True(a.Overlaps(b));
    }

    [Fact]
    public void Union()
    {
        var a = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        var b = new Aabb(new Vec3(2, 2, 2), new Vec3(3, 3, 3));
        var u = Aabb.Union(a, b);
        Assert.Equal(new Vec3(0, 0, 0), u.Min);
        Assert.Equal(new Vec3(3, 3, 3), u.Max);
    }

    [Fact]
    public void SurfaceArea()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 2, 3));
        // 2*(1*2 + 2*3 + 3*1) = 2*(2+6+3) = 22
        Assert.Equal(22, box.SurfaceArea, 1e-15);
    }

    [Fact]
    public void FromTriangle()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 1);
        var box = Aabb.FromTriangle(a, b, c);
        Assert.Equal(Vec3.Zero, box.Min);
        Assert.Equal(new Vec3(1, 1, 1), box.Max);
    }

    [Fact]
    public void Contains()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        Assert.True(box.Contains(new Vec3(0.5, 0.5, 0.5)));
        Assert.False(box.Contains(new Vec3(2, 0.5, 0.5)));
    }
}
