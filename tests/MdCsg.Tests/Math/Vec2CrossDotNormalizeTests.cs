using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Vec2 — Cross, Dot, Normalized, Length, operators</summary>
public class Vec2CrossDotNormalizeTests
{
    [Fact]
    public void Cross_Perpendicular_NonZero()
    {
        var a = new Vec2(1, 0);
        var b = new Vec2(0, 1);
        Assert.Equal(1.0, Vec2.Cross(a, b), 15);
    }

    [Fact]
    public void Cross_Parallel_Zero()
    {
        var a = new Vec2(1, 0);
        var b = new Vec2(2, 0);
        Assert.Equal(0.0, Vec2.Cross(a, b), 15);
    }

    [Fact]
    public void Cross_AntiCommutative()
    {
        var a = new Vec2(3, 4);
        var b = new Vec2(1, 2);
        Assert.Equal(-Vec2.Cross(a, b), Vec2.Cross(b, a), 15);
    }

    [Fact]
    public void Dot_Perpendicular_Zero()
    {
        var a = new Vec2(1, 0);
        var b = new Vec2(0, 1);
        Assert.Equal(0.0, Vec2.Dot(a, b), 15);
    }

    [Fact]
    public void Dot_SameVector_IsLengthSquared()
    {
        var v = new Vec2(3, 4);
        Assert.Equal(v.LengthSquared, Vec2.Dot(v, v), 15);
    }

    [Fact]
    public void Dot_Commutative()
    {
        var a = new Vec2(1, 2);
        var b = new Vec2(3, 4);
        Assert.Equal(Vec2.Dot(a, b), Vec2.Dot(b, a), 15);
    }

    [Fact]
    public void Length_345Triangle()
    {
        var v = new Vec2(3, 4);
        Assert.Equal(5.0, v.Length, 15);
    }

    [Fact]
    public void LengthSquared_345Triangle()
    {
        var v = new Vec2(3, 4);
        Assert.Equal(25.0, v.LengthSquared, 15);
    }

    [Fact]
    public void Normalized_IsUnitLength()
    {
        var v = new Vec2(3, 4);
        var n = v.Normalized;
        Assert.Equal(1.0, n.Length, 10);
    }

    [Fact]
    public void Normalized_SameDirection()
    {
        var v = new Vec2(3, 4);
        var n = v.Normalized;
        Assert.True(Vec2.Dot(v, n) > 0, "Normalized should point same direction");
    }

    [Fact]
    public void Zero_HasZeroLength()
    {
        Assert.Equal(0.0, Vec2.Zero.Length, 15);
    }

    [Fact]
    public void Addition_ComponentWise()
    {
        var a = new Vec2(1, 2);
        var b = new Vec2(3, 4);
        Assert.Equal(new Vec2(4, 6), a + b);
    }

    [Fact]
    public void Subtraction_ComponentWise()
    {
        var a = new Vec2(4, 6);
        var b = new Vec2(1, 2);
        Assert.Equal(new Vec2(3, 4), a - b);
    }

    [Fact]
    public void Negation_FlipsSign()
    {
        var v = new Vec2(1, -2);
        Assert.Equal(new Vec2(-1, 2), -v);
    }

    [Fact]
    public void ScalarMultiply_Right()
    {
        var v = new Vec2(1, 2);
        Assert.Equal(new Vec2(3, 6), v * 3);
    }

    [Fact]
    public void ScalarMultiply_Left()
    {
        var v = new Vec2(1, 2);
        Assert.Equal(new Vec2(3, 6), 3 * v);
    }

    [Fact]
    public void Division_ByScalar()
    {
        var v = new Vec2(6, 8);
        Assert.Equal(new Vec2(3, 4), v / 2);
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
        var v = new Vec2(1.5, 2.5);
        Assert.Contains("1.5", v.ToString());
        Assert.Contains("2.5", v.ToString());
    }
}
