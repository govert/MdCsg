using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Vec2 — arithmetic, Dot, Cross, Length, Normalized, operators</summary>
public class Vec2DeepPropertyTests
{
    [Fact]
    public void Addition_Components()
    {
        var a = new Vec2(1, 2);
        var b = new Vec2(3, 4);
        var c = a + b;
        Assert.Equal(4, c.X);
        Assert.Equal(6, c.Y);
    }

    [Fact]
    public void Subtraction_Components()
    {
        var a = new Vec2(5, 7);
        var b = new Vec2(2, 3);
        var c = a - b;
        Assert.Equal(3, c.X);
        Assert.Equal(4, c.Y);
    }

    [Fact]
    public void Negation()
    {
        var a = new Vec2(3, -4);
        var n = -a;
        Assert.Equal(-3, n.X);
        Assert.Equal(4, n.Y);
    }

    [Fact]
    public void ScalarMultiply_Right()
    {
        var a = new Vec2(2, 3);
        var c = a * 5.0;
        Assert.Equal(10, c.X);
        Assert.Equal(15, c.Y);
    }

    [Fact]
    public void ScalarMultiply_Left()
    {
        var a = new Vec2(2, 3);
        var c = 5.0 * a;
        Assert.Equal(10, c.X);
        Assert.Equal(15, c.Y);
    }

    [Fact]
    public void ScalarDivide()
    {
        var a = new Vec2(10, 6);
        var c = a / 2.0;
        Assert.Equal(5, c.X);
        Assert.Equal(3, c.Y);
    }

    [Fact]
    public void Dot_Perpendicular_Zero()
    {
        Assert.True(System.Math.Abs(Vec2.Dot(new Vec2(1, 0), new Vec2(0, 1))) < 1e-15);
    }

    [Fact]
    public void Dot_Parallel_Product()
    {
        Assert.True(System.Math.Abs(Vec2.Dot(new Vec2(3, 0), new Vec2(5, 0)) - 15.0) < 1e-15);
    }

    [Fact]
    public void Cross_Perpendicular_One()
    {
        Assert.True(System.Math.Abs(Vec2.Cross(new Vec2(1, 0), new Vec2(0, 1)) - 1.0) < 1e-15);
    }

    [Fact]
    public void Cross_AntiCommutative()
    {
        var a = new Vec2(3, 4);
        var b = new Vec2(5, 6);
        Assert.True(System.Math.Abs(Vec2.Cross(a, b) + Vec2.Cross(b, a)) < 1e-15);
    }

    [Fact]
    public void LengthSquared_345Triangle()
    {
        var v = new Vec2(3, 4);
        Assert.True(System.Math.Abs(v.LengthSquared - 25.0) < 1e-15);
    }

    [Fact]
    public void Length_345Triangle()
    {
        var v = new Vec2(3, 4);
        Assert.True(System.Math.Abs(v.Length - 5.0) < 1e-15);
    }

    [Fact]
    public void Normalized_HasUnitLength()
    {
        var v = new Vec2(7, 11);
        Assert.True(System.Math.Abs(v.Normalized.Length - 1.0) < 1e-10);
    }

    [Fact]
    public void Zero_HasZeroLength()
    {
        Assert.Equal(0, Vec2.Zero.LengthSquared);
    }

    [Fact]
    public void ToString_Format()
    {
        var v = new Vec2(1.5, 2.5);
        Assert.Contains("1.5", v.ToString());
        Assert.Contains("2.5", v.ToString());
    }

    [Fact]
    public void RecordEquality()
    {
        var a = new Vec2(1, 2);
        var b = new Vec2(1, 2);
        Assert.Equal(a, b);
        Assert.NotEqual(a, new Vec2(1, 3));
    }
}
