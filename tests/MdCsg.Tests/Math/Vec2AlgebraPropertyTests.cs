using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Vec2 — dot, cross, arithmetic operators, normalization, length</summary>
public class Vec2AlgebraPropertyTests
{
    [Fact]
    public void Dot_Perpendicular_Zero()
    {
        Assert.Equal(0.0, Vec2.Dot(new Vec2(1, 0), new Vec2(0, 1)));
    }

    [Fact]
    public void Dot_Parallel_LengthProduct()
    {
        var a = new Vec2(3, 0);
        var b = new Vec2(4, 0);
        Assert.Equal(12.0, Vec2.Dot(a, b));
    }

    [Fact]
    public void Dot_Commutative()
    {
        var a = new Vec2(3, 7);
        var b = new Vec2(11, 13);
        Assert.Equal(Vec2.Dot(a, b), Vec2.Dot(b, a));
    }

    [Fact]
    public void Cross_UnitVectors_One()
    {
        Assert.Equal(1.0, Vec2.Cross(new Vec2(1, 0), new Vec2(0, 1)));
    }

    [Fact]
    public void Cross_Parallel_Zero()
    {
        Assert.Equal(0.0, Vec2.Cross(new Vec2(3, 0), new Vec2(6, 0)));
    }

    [Fact]
    public void Cross_Antisymmetric()
    {
        var a = new Vec2(3, 7);
        var b = new Vec2(11, 13);
        Assert.Equal(Vec2.Cross(a, b), -Vec2.Cross(b, a));
    }

    [Fact]
    public void Add_Commutative()
    {
        var a = new Vec2(1, 2);
        var b = new Vec2(3, 4);
        Assert.Equal(a + b, b + a);
    }

    [Fact]
    public void Add_Zero_IsIdentity()
    {
        var a = new Vec2(5, 7);
        Assert.Equal(a, a + Vec2.Zero);
    }

    [Fact]
    public void Subtract_Self_IsZero()
    {
        var a = new Vec2(5, 7);
        Assert.Equal(Vec2.Zero, a - a);
    }

    [Fact]
    public void Negate_DoubleNegate_IsOriginal()
    {
        var a = new Vec2(3, -4);
        Assert.Equal(a, -(-a));
    }

    [Fact]
    public void ScalarMultiply_One_IsIdentity()
    {
        var a = new Vec2(3, 4);
        Assert.Equal(a, a * 1.0);
    }

    [Fact]
    public void ScalarMultiply_Zero_IsZero()
    {
        var a = new Vec2(3, 4);
        Assert.Equal(Vec2.Zero, a * 0.0);
    }

    [Fact]
    public void ScalarMultiply_LeftRight_Same()
    {
        var a = new Vec2(3, 4);
        Assert.Equal(a * 5.0, 5.0 * a);
    }

    [Fact]
    public void Divide_ByOne_IsIdentity()
    {
        var a = new Vec2(3, 4);
        Assert.Equal(a, a / 1.0);
    }

    [Fact]
    public void Length_UnitX_IsOne()
    {
        Assert.Equal(1.0, new Vec2(1, 0).Length);
    }

    [Fact]
    public void Length_3_4_IsFive()
    {
        Assert.True(System.Math.Abs(new Vec2(3, 4).Length - 5.0) < 1e-10);
    }

    [Fact]
    public void LengthSquared_3_4_Is25()
    {
        Assert.Equal(25.0, new Vec2(3, 4).LengthSquared);
    }

    [Fact]
    public void Normalized_UnitLength()
    {
        var v = new Vec2(3, 4);
        var n = v.Normalized;
        Assert.True(System.Math.Abs(n.Length - 1.0) < 1e-10);
    }

    [Fact]
    public void Normalized_SameDirection()
    {
        var v = new Vec2(3, 4);
        var n = v.Normalized;
        // Cross product should be zero (same direction)
        Assert.True(System.Math.Abs(Vec2.Cross(v, n)) < 1e-10);
        // Dot product should be positive
        Assert.True(Vec2.Dot(v, n) > 0);
    }

    [Fact]
    public void Zero_LengthSquared_IsZero()
    {
        Assert.Equal(0.0, Vec2.Zero.LengthSquared);
    }

    [Fact]
    public void Add_Components_Correct()
    {
        var a = new Vec2(1, 2);
        var b = new Vec2(3, 4);
        var c = a + b;
        Assert.Equal(4.0, c.X);
        Assert.Equal(6.0, c.Y);
    }

    [Fact]
    public void Subtract_Components_Correct()
    {
        var a = new Vec2(5, 7);
        var b = new Vec2(3, 2);
        var c = a - b;
        Assert.Equal(2.0, c.X);
        Assert.Equal(5.0, c.Y);
    }

    [Fact]
    public void ToString_Format()
    {
        var v = new Vec2(1.5, 2.5);
        var s = v.ToString();
        Assert.Contains("1.5", s);
        Assert.Contains("2.5", s);
    }
}
