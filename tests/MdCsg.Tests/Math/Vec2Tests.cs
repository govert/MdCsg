using MdCsg.Math;

namespace MdCsg.Tests.Math;

public class Vec2Tests
{
    [Fact]
    public void Addition()
    {
        var a = new Vec2(1, 2);
        var b = new Vec2(3, 4);
        Assert.Equal(new Vec2(4, 6), a + b);
    }

    [Fact]
    public void DotProduct()
    {
        var a = new Vec2(1, 0);
        var b = new Vec2(0, 1);
        Assert.Equal(0, Vec2.Dot(a, b));
    }

    [Fact]
    public void CrossProduct()
    {
        var a = new Vec2(1, 0);
        var b = new Vec2(0, 1);
        Assert.Equal(1, Vec2.Cross(a, b));
    }

    [Fact]
    public void Length()
    {
        var v = new Vec2(3, 4);
        Assert.Equal(5, v.Length, 1e-15);
    }

    [Fact]
    public void Normalized()
    {
        var v = new Vec2(0, 5);
        var n = v.Normalized;
        Assert.Equal(1.0, n.Length, 1e-15);
    }
}
