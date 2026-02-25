using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Vec2 geometric properties — cross product, dot, normalize, arithmetic, record equality</summary>
public class Vec2GeometricPropertyTests
{
    [Fact]
    public void Cross_PerpendicularUnit_IsOne()
    {
        var a = new Vec2(1, 0);
        var b = new Vec2(0, 1);
        Assert.Equal(1.0, Vec2.Cross(a, b), 1e-14);
    }

    [Fact]
    public void Cross_AntiCommutative()
    {
        var a = new Vec2(3, 4);
        var b = new Vec2(5, 6);
        Assert.Equal(-Vec2.Cross(a, b), Vec2.Cross(b, a), 1e-14);
    }

    [Fact]
    public void Cross_Parallel_IsZero()
    {
        var a = new Vec2(1, 2);
        var b = new Vec2(2, 4);
        Assert.Equal(0, Vec2.Cross(a, b), 1e-14);
    }

    [Fact]
    public void Dot_SelfIs_LengthSquared()
    {
        var a = new Vec2(3, 4);
        Assert.Equal(a.LengthSquared, Vec2.Dot(a, a), 1e-14);
    }

    [Fact]
    public void Dot_Perpendicular_IsZero()
    {
        var a = new Vec2(1, 0);
        var b = new Vec2(0, 1);
        Assert.Equal(0, Vec2.Dot(a, b), 1e-14);
    }

    [Fact]
    public void Dot_Commutative()
    {
        var a = new Vec2(3, 4);
        var b = new Vec2(5, 6);
        Assert.Equal(Vec2.Dot(a, b), Vec2.Dot(b, a), 1e-14);
    }

    [Fact]
    public void Normalized_HasUnitLength()
    {
        var v = new Vec2(3, 4);
        Assert.Equal(1.0, v.Normalized.Length, 1e-14);
    }

    [Fact]
    public void Normalized_PreservesDirection()
    {
        var v = new Vec2(3, 4);
        var n = v.Normalized;
        Assert.True(Vec2.Dot(v, n) > 0);
    }

    [Fact]
    public void Length_345_Is5()
    {
        var v = new Vec2(3, 4);
        Assert.Equal(5.0, v.Length, 1e-14);
    }

    [Fact]
    public void LengthSquared_345_Is25()
    {
        var v = new Vec2(3, 4);
        Assert.Equal(25.0, v.LengthSquared, 1e-14);
    }

    [Fact]
    public void Addition()
    {
        var a = new Vec2(1, 2);
        var b = new Vec2(3, 4);
        var sum = a + b;
        Assert.Equal(4, sum.X);
        Assert.Equal(6, sum.Y);
    }

    [Fact]
    public void Subtraction()
    {
        var a = new Vec2(5, 6);
        var b = new Vec2(3, 4);
        var diff = a - b;
        Assert.Equal(2, diff.X);
        Assert.Equal(2, diff.Y);
    }

    [Fact]
    public void Negation()
    {
        var v = new Vec2(3, -4);
        var neg = -v;
        Assert.Equal(-3, neg.X);
        Assert.Equal(4, neg.Y);
    }

    [Fact]
    public void ScalarMultiply_Right()
    {
        var v = new Vec2(2, 3);
        var result = v * 5.0;
        Assert.Equal(10, result.X);
        Assert.Equal(15, result.Y);
    }

    [Fact]
    public void ScalarMultiply_Left()
    {
        var v = new Vec2(2, 3);
        var result = 5.0 * v;
        Assert.Equal(10, result.X);
        Assert.Equal(15, result.Y);
    }

    [Fact]
    public void Division()
    {
        var v = new Vec2(10, 20);
        var result = v / 5.0;
        Assert.Equal(2, result.X);
        Assert.Equal(4, result.Y);
    }

    [Fact]
    public void Zero_IsZero()
    {
        Assert.Equal(0, Vec2.Zero.X);
        Assert.Equal(0, Vec2.Zero.Y);
        Assert.Equal(0, Vec2.Zero.Length);
    }

    [Fact]
    public void RecordEquality()
    {
        var a = new Vec2(1, 2);
        var b = new Vec2(1, 2);
        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void RecordInequality()
    {
        var a = new Vec2(1, 2);
        var b = new Vec2(1, 3);
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void ToString_ContainsComponents()
    {
        var v = new Vec2(1.5, 2.5);
        var s = v.ToString();
        Assert.Contains("1.5", s);
        Assert.Contains("2.5", s);
    }

    [Fact]
    public void Cross_IsAreaOfParallelogram()
    {
        // Area of parallelogram formed by (1,0) and (1,1) = 1
        var a = new Vec2(1, 0);
        var b = new Vec2(1, 1);
        Assert.Equal(1.0, Vec2.Cross(a, b), 1e-14);
    }

    [Fact]
    public void SubtractSelf_IsZero()
    {
        var v = new Vec2(7, 8);
        Assert.Equal(Vec2.Zero, v - v);
    }
}
