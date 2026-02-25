using System.Numerics;
using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Phase 6: Rational — arithmetic operators, comparisons, FromDouble edge cases</summary>
public class RationalArithmeticPropertyTests
{
    [Fact]
    public void Addition_Commutative()
    {
        var a = new Rational(1, 3);
        var b = new Rational(2, 5);
        Assert.Equal(a + b, b + a);
    }

    [Fact]
    public void Addition_Associative()
    {
        var a = new Rational(1, 3);
        var b = new Rational(2, 5);
        var c = new Rational(3, 7);
        Assert.Equal((a + b) + c, a + (b + c));
    }

    [Fact]
    public void Addition_Identity()
    {
        var a = new Rational(5, 7);
        Assert.Equal(a, a + Rational.Zero);
    }

    [Fact]
    public void Multiplication_Commutative()
    {
        var a = new Rational(3, 4);
        var b = new Rational(5, 7);
        Assert.Equal(a * b, b * a);
    }

    [Fact]
    public void Multiplication_Identity()
    {
        var a = new Rational(5, 7);
        Assert.Equal(a, a * Rational.One);
    }

    [Fact]
    public void Multiplication_Zero()
    {
        var a = new Rational(5, 7);
        Assert.Equal(Rational.Zero, a * Rational.Zero);
    }

    [Fact]
    public void Subtraction_SelfIsZero()
    {
        var a = new Rational(5, 7);
        Assert.Equal(Rational.Zero, a - a);
    }

    [Fact]
    public void Division_SelfIsOne()
    {
        var a = new Rational(5, 7);
        Assert.Equal(Rational.One, a / a);
    }

    [Fact]
    public void Division_ByZero_Throws()
    {
        var a = new Rational(1, 1);
        Assert.Throws<DivideByZeroException>(() => a / Rational.Zero);
    }

    [Fact]
    public void Negation_DoubleNegation()
    {
        var a = new Rational(5, 7);
        Assert.Equal(a, -(-a));
    }

    [Fact]
    public void Comparison_LessThan()
    {
        var a = new Rational(1, 3);
        var b = new Rational(1, 2);
        Assert.True(a < b);
        Assert.False(b < a);
    }

    [Fact]
    public void Comparison_GreaterThan()
    {
        var a = new Rational(3, 4);
        var b = new Rational(1, 4);
        Assert.True(a > b);
    }

    [Fact]
    public void Comparison_LessEqual()
    {
        var a = new Rational(1, 2);
        Assert.True(a <= a);
        Assert.True(a <= new Rational(3, 4));
    }

    [Fact]
    public void Comparison_GreaterEqual()
    {
        var a = new Rational(1, 2);
        Assert.True(a >= a);
        Assert.True(a >= new Rational(1, 4));
    }

    [Fact]
    public void Sign_Positive()
    {
        Assert.Equal(1, new Rational(5, 7).Sign);
    }

    [Fact]
    public void Sign_Negative()
    {
        Assert.Equal(-1, new Rational(-5, 7).Sign);
    }

    [Fact]
    public void Sign_Zero()
    {
        Assert.Equal(0, Rational.Zero.Sign);
    }

    [Fact]
    public void NegativeDenominator_Normalized()
    {
        var r = new Rational(3, -5);
        Assert.True(r.Denominator > 0);
        Assert.Equal(-1, r.Sign);
    }

    [Fact]
    public void Reduced_Form()
    {
        var r = new Rational(6, 10);
        Assert.Equal(new BigInteger(3), r.Numerator);
        Assert.Equal(new BigInteger(5), r.Denominator);
    }

    [Fact]
    public void FromDouble_Integer()
    {
        var r = Rational.FromDouble(5.0);
        Assert.Equal(new BigInteger(5), r.Numerator);
        Assert.Equal(BigInteger.One, r.Denominator);
    }

    [Fact]
    public void FromDouble_Half()
    {
        var r = Rational.FromDouble(0.5);
        Assert.Equal(BigInteger.One, r.Numerator);
        Assert.Equal(new BigInteger(2), r.Denominator);
    }

    [Fact]
    public void FromDouble_Negative()
    {
        var r = Rational.FromDouble(-3.0);
        Assert.Equal(new BigInteger(-3), r.Numerator);
        Assert.Equal(BigInteger.One, r.Denominator);
    }

    [Fact]
    public void FromDouble_NaN_Throws()
    {
        Assert.Throws<ArgumentException>(() => Rational.FromDouble(double.NaN));
    }

    [Fact]
    public void FromDouble_Infinity_Throws()
    {
        Assert.Throws<ArgumentException>(() => Rational.FromDouble(double.PositiveInfinity));
        Assert.Throws<ArgumentException>(() => Rational.FromDouble(double.NegativeInfinity));
    }

    [Fact]
    public void FromDouble_Zero()
    {
        Assert.Equal(Rational.Zero, Rational.FromDouble(0.0));
    }

    [Fact]
    public void ToString_WholeNumber()
    {
        Assert.Equal("5", new Rational(5, 1).ToString());
    }

    [Fact]
    public void ToString_Fraction()
    {
        Assert.Equal("3/5", new Rational(3, 5).ToString());
    }

    [Fact]
    public void Equality_Operators()
    {
        var a = new Rational(1, 2);
        var b = new Rational(2, 4);
        Assert.True(a == b);
        Assert.False(a != b);
    }

    [Fact]
    public void GetHashCode_EqualValues_SameHash()
    {
        var a = new Rational(1, 2);
        var b = new Rational(2, 4);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Distributive_Law()
    {
        var a = new Rational(2, 3);
        var b = new Rational(1, 4);
        var c = new Rational(3, 5);
        Assert.Equal(a * (b + c), a * b + a * c);
    }
}
