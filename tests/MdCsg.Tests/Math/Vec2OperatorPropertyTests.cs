using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Vec2 — operator properties, dot, cross, normalization</summary>
public class Vec2OperatorPropertyTests
{
    [Fact]
    public void Zero_IsZero()
    {
        Assert.Equal(0.0, Vec2.Zero.X);
        Assert.Equal(0.0, Vec2.Zero.Y);
    }

    [Fact]
    public void Addition_Commutative()
    {
        var a = new Vec2(1, 2);
        var b = new Vec2(3, 4);
        Assert.Equal(a + b, b + a);
    }

    [Fact]
    public void Addition_Associative()
    {
        var a = new Vec2(1, 2);
        var b = new Vec2(3, 4);
        var c = new Vec2(5, 6);
        var lhs = (a + b) + c;
        var rhs = a + (b + c);
        Assert.Equal(lhs.X, rhs.X, 10);
        Assert.Equal(lhs.Y, rhs.Y, 10);
    }

    [Fact]
    public void Addition_Zero_Identity()
    {
        var a = new Vec2(1, 2);
        Assert.Equal(a, a + Vec2.Zero);
    }

    [Fact]
    public void Subtraction_Self_Zero()
    {
        var a = new Vec2(1, 2);
        Assert.Equal(Vec2.Zero, a - a);
    }

    [Fact]
    public void Negation_DoubleNegate()
    {
        var a = new Vec2(1, 2);
        Assert.Equal(a, -(-a));
    }

    [Fact]
    public void ScalarMul_Identity()
    {
        var a = new Vec2(3, 4);
        Assert.Equal(a, a * 1.0);
    }

    [Fact]
    public void ScalarMul_Zero()
    {
        var a = new Vec2(3, 4);
        Assert.Equal(Vec2.Zero, a * 0.0);
    }

    [Fact]
    public void ScalarMul_Commutative()
    {
        var a = new Vec2(3, 4);
        Assert.Equal(a * 2.0, 2.0 * a);
    }

    [Fact]
    public void ScalarDiv_ByOne_Identity()
    {
        var a = new Vec2(3, 4);
        Assert.Equal(a, a / 1.0);
    }

    [Fact]
    public void ScalarDiv_ByTwo_Halves()
    {
        var a = new Vec2(6, 8);
        Assert.Equal(new Vec2(3, 4), a / 2.0);
    }

    [Fact]
    public void Dot_Commutative()
    {
        var a = new Vec2(1, 2);
        var b = new Vec2(3, 4);
        Assert.Equal(Vec2.Dot(a, b), Vec2.Dot(b, a), 15);
    }

    [Fact]
    public void Dot_Orthogonal_Zero()
    {
        var a = new Vec2(1, 0);
        var b = new Vec2(0, 1);
        Assert.Equal(0.0, Vec2.Dot(a, b), 15);
    }

    [Fact]
    public void Dot_KnownValue()
    {
        var a = new Vec2(1, 2);
        var b = new Vec2(3, 4);
        Assert.Equal(11.0, Vec2.Dot(a, b), 15); // 3 + 8
    }

    [Fact]
    public void Cross_AntiCommutative()
    {
        var a = new Vec2(1, 2);
        var b = new Vec2(3, 4);
        Assert.Equal(-Vec2.Cross(a, b), Vec2.Cross(b, a), 15);
    }

    [Fact]
    public void Cross_WithSelf_Zero()
    {
        var a = new Vec2(1, 2);
        Assert.Equal(0.0, Vec2.Cross(a, a), 15);
    }

    [Fact]
    public void Cross_KnownValue()
    {
        var a = new Vec2(1, 0);
        var b = new Vec2(0, 1);
        Assert.Equal(1.0, Vec2.Cross(a, b), 15);
    }

    [Fact]
    public void Cross_ParallelVectors_Zero()
    {
        var a = new Vec2(1, 2);
        var b = new Vec2(2, 4);
        Assert.Equal(0.0, Vec2.Cross(a, b), 15);
    }

    [Fact]
    public void Length_345()
    {
        var v = new Vec2(3, 4);
        Assert.Equal(5.0, v.Length, 15);
    }

    [Fact]
    public void LengthSquared_Consistent()
    {
        var v = new Vec2(3, 4);
        Assert.Equal(v.Length * v.Length, v.LengthSquared, 10);
    }

    [Fact]
    public void Normalized_HasUnitLength()
    {
        var v = new Vec2(3, 4);
        Assert.Equal(1.0, v.Normalized.Length, 10);
    }

    [Fact]
    public void Normalized_SameDirection()
    {
        var v = new Vec2(3, 4);
        var n = v.Normalized;
        Assert.Equal(0.6, n.X, 10);
        Assert.Equal(0.8, n.Y, 10);
    }

    [Fact]
    public void ToString_ContainsComponents()
    {
        var v = new Vec2(1.5, 2.5);
        var s = v.ToString();
        Assert.Contains("1.5", s);
        Assert.Contains("2.5", s);
    }
}
