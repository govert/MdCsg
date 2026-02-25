using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Vec2 — basic operations, properties, record equality</summary>
public class Vec2PropertyTests
{
    [Fact]
    public void Constructor_SetsComponents()
    {
        var v = new Vec2(3, 4);
        Assert.Equal(3, v.X);
        Assert.Equal(4, v.Y);
    }

    [Fact]
    public void Addition()
    {
        var a = new Vec2(1, 2);
        var b = new Vec2(3, 4);
        Assert.Equal(new Vec2(4, 6), a + b);
    }

    [Fact]
    public void Subtraction()
    {
        var a = new Vec2(5, 7);
        var b = new Vec2(3, 4);
        Assert.Equal(new Vec2(2, 3), a - b);
    }

    [Fact]
    public void ScalarMultiplication()
    {
        var v = new Vec2(3, 4);
        Assert.Equal(new Vec2(6, 8), v * 2);
    }

    [Fact]
    public void Length_345()
    {
        var v = new Vec2(3, 4);
        Assert.Equal(5.0, v.Length, 10);
    }

    [Fact]
    public void LengthSquared()
    {
        var v = new Vec2(3, 4);
        Assert.Equal(25.0, v.LengthSquared, 10);
    }

    [Fact]
    public void RecordEquality()
    {
        Assert.Equal(new Vec2(1, 2), new Vec2(1, 2));
    }

    [Fact]
    public void RecordInequality()
    {
        Assert.NotEqual(new Vec2(1, 2), new Vec2(1, 3));
    }

    [Fact]
    public void Zero_IsDefaultLike()
    {
        var v = new Vec2(0, 0);
        Assert.Equal(0, v.Length);
    }

    [Fact]
    public void Dot_Product()
    {
        var a = new Vec2(1, 0);
        var b = new Vec2(0, 1);
        Assert.Equal(0, Vec2.Dot(a, b), 10);
    }

    [Fact]
    public void Dot_Parallel()
    {
        var a = new Vec2(2, 0);
        var b = new Vec2(3, 0);
        Assert.Equal(6, Vec2.Dot(a, b), 10);
    }

    [Fact]
    public void Cross2D_Value()
    {
        // Cross product in 2D gives scalar: ax*by - ay*bx
        var a = new Vec2(1, 0);
        var b = new Vec2(0, 1);
        double cross = a.X * b.Y - a.Y * b.X;
        Assert.Equal(1, cross, 10);
    }

    [Fact]
    public void Negation()
    {
        var v = new Vec2(3, -4);
        var neg = new Vec2(-v.X, -v.Y);
        Assert.Equal(new Vec2(-3, 4), neg);
    }

    [Fact]
    public void Distance_Symmetric()
    {
        var a = new Vec2(1, 2);
        var b = new Vec2(4, 6);
        double d1 = (a - b).Length;
        double d2 = (b - a).Length;
        Assert.Equal(d1, d2, 10);
    }
}
