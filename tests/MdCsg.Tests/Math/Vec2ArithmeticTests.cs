using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Vec2 arithmetic operator and property tests</summary>
public class Vec2ArithmeticTests
{
    [Fact]
    public void Add_TwoVectors()
    {
        var a = new Vec2(1, 2);
        var b = new Vec2(3, 4);
        var c = a + b;
        Assert.Equal(4, c.X);
        Assert.Equal(6, c.Y);
    }

    [Fact]
    public void Subtract_TwoVectors()
    {
        var a = new Vec2(5, 7);
        var b = new Vec2(2, 3);
        var c = a - b;
        Assert.Equal(3, c.X);
        Assert.Equal(4, c.Y);
    }

    [Fact]
    public void Negate()
    {
        var a = new Vec2(3, -4);
        var b = -a;
        Assert.Equal(-3, b.X);
        Assert.Equal(4, b.Y);
    }

    [Fact]
    public void MultiplyByScalar_Right()
    {
        var a = new Vec2(2, 3);
        var b = a * 5;
        Assert.Equal(10, b.X);
        Assert.Equal(15, b.Y);
    }

    [Fact]
    public void MultiplyByScalar_Left()
    {
        var a = new Vec2(2, 3);
        var b = 5 * a;
        Assert.Equal(10, b.X);
        Assert.Equal(15, b.Y);
    }

    [Fact]
    public void DivideByScalar()
    {
        var a = new Vec2(10, 20);
        var b = a / 5;
        Assert.Equal(2, b.X);
        Assert.Equal(4, b.Y);
    }

    [Fact]
    public void Dot_Perpendicular_Zero()
    {
        var a = new Vec2(1, 0);
        var b = new Vec2(0, 1);
        Assert.Equal(0, Vec2.Dot(a, b));
    }

    [Fact]
    public void Dot_Parallel_Product()
    {
        var a = new Vec2(3, 0);
        var b = new Vec2(4, 0);
        Assert.Equal(12, Vec2.Dot(a, b));
    }

    [Fact]
    public void Dot_General()
    {
        var a = new Vec2(1, 2);
        var b = new Vec2(3, 4);
        Assert.Equal(11, Vec2.Dot(a, b)); // 1*3 + 2*4 = 11
    }

    [Fact]
    public void Cross_ParallelVectors_Zero()
    {
        var a = new Vec2(1, 2);
        var b = new Vec2(2, 4);
        Assert.Equal(0, Vec2.Cross(a, b));
    }

    [Fact]
    public void Cross_Perpendicular()
    {
        var a = new Vec2(1, 0);
        var b = new Vec2(0, 1);
        Assert.Equal(1, Vec2.Cross(a, b));
    }

    [Fact]
    public void Cross_Anticommutative()
    {
        var a = new Vec2(3, 5);
        var b = new Vec2(7, 11);
        Assert.Equal(-Vec2.Cross(b, a), Vec2.Cross(a, b), 1e-15);
    }

    [Fact]
    public void Length_UnitVectors()
    {
        Assert.Equal(1, new Vec2(1, 0).Length, 1e-15);
        Assert.Equal(1, new Vec2(0, 1).Length, 1e-15);
    }

    [Fact]
    public void Length_345Triangle()
    {
        Assert.Equal(5, new Vec2(3, 4).Length, 1e-15);
    }

    [Fact]
    public void LengthSquared()
    {
        var v = new Vec2(3, 4);
        Assert.Equal(25, v.LengthSquared);
    }

    [Fact]
    public void Normalized_UnitLength()
    {
        var v = new Vec2(3, 4);
        var n = v.Normalized;
        Assert.Equal(1, n.Length, 1e-10);
    }

    [Fact]
    public void Normalized_SameDirection()
    {
        var v = new Vec2(3, 4);
        var n = v.Normalized;
        Assert.True(n.X > 0);
        Assert.True(n.Y > 0);
    }

    [Fact]
    public void Zero_IsZero()
    {
        Assert.Equal(0, Vec2.Zero.X);
        Assert.Equal(0, Vec2.Zero.Y);
    }

    [Fact]
    public void RecordEquality()
    {
        var a = new Vec2(1, 2);
        var b = new Vec2(1, 2);
        Assert.Equal(a, b);
    }

    [Fact]
    public void RecordInequality()
    {
        var a = new Vec2(1, 2);
        var b = new Vec2(1, 3);
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void ToString_Format()
    {
        var v = new Vec2(1, 2);
        Assert.Contains("1", v.ToString());
        Assert.Contains("2", v.ToString());
    }

    [Fact]
    public void AddZero_Identity()
    {
        var v = new Vec2(5, 7);
        Assert.Equal(v, v + Vec2.Zero);
    }

    [Fact]
    public void SubtractSelf_Zero()
    {
        var v = new Vec2(5, 7);
        Assert.Equal(Vec2.Zero, v - v);
    }

    [Fact]
    public void MultiplyByZero_Zero()
    {
        var v = new Vec2(5, 7);
        Assert.Equal(Vec2.Zero, v * 0);
    }

    [Fact]
    public void MultiplyByOne_Identity()
    {
        var v = new Vec2(5, 7);
        Assert.Equal(v, v * 1);
    }
}
