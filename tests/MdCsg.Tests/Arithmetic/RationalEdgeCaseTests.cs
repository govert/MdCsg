using System.Numerics;
using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Phase 6: Rational edge cases — FromDouble, division, zero, sign, comparison, normalization</summary>
public class RationalEdgeCaseTests
{
    [Fact]
    public void Zero_IsZero()
    {
        Assert.Equal(0, Rational.Zero.Sign);
        Assert.Equal(BigInteger.Zero, Rational.Zero.Numerator);
        Assert.Equal(BigInteger.One, Rational.Zero.Denominator);
    }

    [Fact]
    public void One_IsOne()
    {
        Assert.Equal(1, Rational.One.Sign);
        Assert.Equal(BigInteger.One, Rational.One.Numerator);
        Assert.Equal(BigInteger.One, Rational.One.Denominator);
    }

    [Fact]
    public void FromDouble_Zero()
    {
        var r = Rational.FromDouble(0.0);
        Assert.Equal(Rational.Zero, r);
    }

    [Fact]
    public void FromDouble_One()
    {
        var r = Rational.FromDouble(1.0);
        Assert.Equal(Rational.One, r);
    }

    [Fact]
    public void FromDouble_NegativeOne()
    {
        var r = Rational.FromDouble(-1.0);
        Assert.Equal(-1, r.Sign);
        Assert.True(r.Numerator < 0);
    }

    [Fact]
    public void FromDouble_Half()
    {
        var r = Rational.FromDouble(0.5);
        Assert.Equal(BigInteger.One, r.Numerator);
        Assert.Equal(new BigInteger(2), r.Denominator);
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
    public void DivideByZero_Throws()
    {
        Assert.Throws<DivideByZeroException>(() => new Rational(1, 0));
    }

    [Fact]
    public void NegativeDenominator_Normalized()
    {
        var r = new Rational(3, -4);
        Assert.True(r.Denominator > 0);
        Assert.True(r.Numerator < 0);
    }

    [Fact]
    public void Reduced_Form()
    {
        var r = new Rational(6, 4);
        Assert.Equal(new BigInteger(3), r.Numerator);
        Assert.Equal(new BigInteger(2), r.Denominator);
    }

    [Fact]
    public void Addition_BasicArithmetic()
    {
        var a = new Rational(1, 2);
        var b = new Rational(1, 3);
        var sum = a + b;
        Assert.Equal(new BigInteger(5), sum.Numerator);
        Assert.Equal(new BigInteger(6), sum.Denominator);
    }

    [Fact]
    public void Subtraction_BasicArithmetic()
    {
        var a = new Rational(3, 4);
        var b = new Rational(1, 4);
        var diff = a - b;
        Assert.Equal(new BigInteger(1), diff.Numerator);
        Assert.Equal(new BigInteger(2), diff.Denominator);
    }

    [Fact]
    public void Multiplication_BasicArithmetic()
    {
        var a = new Rational(2, 3);
        var b = new Rational(3, 5);
        var prod = a * b;
        Assert.Equal(new BigInteger(2), prod.Numerator);
        Assert.Equal(new BigInteger(5), prod.Denominator);
    }

    [Fact]
    public void Division_BasicArithmetic()
    {
        var a = new Rational(2, 3);
        var b = new Rational(4, 5);
        var quot = a / b;
        Assert.Equal(new BigInteger(5), quot.Numerator);
        Assert.Equal(new BigInteger(6), quot.Denominator);
    }

    [Fact]
    public void Negation()
    {
        var r = new Rational(3, 4);
        var neg = -r;
        Assert.Equal(new BigInteger(-3), neg.Numerator);
        Assert.Equal(new BigInteger(4), neg.Denominator);
    }

    [Fact]
    public void DoubleNegation_Identity()
    {
        var r = new Rational(3, 4);
        Assert.Equal(r, -(-r));
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
    public void Comparison_Equal()
    {
        var a = new Rational(2, 4);
        var b = new Rational(1, 2);
        Assert.True(a == b);
        Assert.False(a != b);
    }

    [Fact]
    public void ToString_WholeNumber()
    {
        var r = new Rational(5, 1);
        Assert.Equal("5", r.ToString());
    }

    [Fact]
    public void ToString_Fraction()
    {
        var r = new Rational(3, 7);
        Assert.Equal("3/7", r.ToString());
    }

    [Fact]
    public void Sign_Positive()
    {
        Assert.Equal(1, new Rational(3, 4).Sign);
    }

    [Fact]
    public void Sign_Negative()
    {
        Assert.Equal(-1, new Rational(-3, 4).Sign);
    }

    [Fact]
    public void Sign_Zero()
    {
        Assert.Equal(0, new Rational(0, 5).Sign);
    }

    [Fact]
    public void AddZero_Identity()
    {
        var r = new Rational(3, 4);
        Assert.Equal(r, r + Rational.Zero);
    }

    [Fact]
    public void MultiplyOne_Identity()
    {
        var r = new Rational(3, 4);
        Assert.Equal(r, r * Rational.One);
    }

    [Fact]
    public void MultiplyZero_IsZero()
    {
        var r = new Rational(3, 4);
        Assert.Equal(Rational.Zero, r * Rational.Zero);
    }

    [Fact]
    public void SubtractSelf_IsZero()
    {
        var r = new Rational(7, 11);
        Assert.Equal(Rational.Zero, r - r);
    }

    [Fact]
    public void GetHashCode_Equal_SameHash()
    {
        var a = new Rational(2, 4); // normalized to 1/2
        var b = new Rational(3, 6); // normalized to 1/2
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }
}
