using System.Numerics;
using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Phase 6: Rational — BigInteger edge cases, reduction, sign handling, FromDouble IEEE decomposition</summary>
public class RationalBigIntegerEdgeCasePropertyTests
{
    [Fact]
    public void Constructor_ReducesToLowestTerms()
    {
        var r = new Rational(6, 4);
        Assert.Equal(new BigInteger(3), r.Numerator);
        Assert.Equal(new BigInteger(2), r.Denominator);
    }

    [Fact]
    public void Constructor_NegativeDenominator_Normalized()
    {
        var r = new Rational(3, -5);
        Assert.Equal(new BigInteger(-3), r.Numerator);
        Assert.Equal(new BigInteger(5), r.Denominator);
    }

    [Fact]
    public void Constructor_ZeroDenominator_Throws()
    {
        Assert.Throws<DivideByZeroException>(() => new Rational(1, 0));
    }

    [Fact]
    public void Constructor_ZeroNumerator_IsZero()
    {
        var r = new Rational(0, 7);
        Assert.Equal(BigInteger.Zero, r.Numerator);
    }

    [Fact]
    public void Sign_Positive()
    {
        var r = new Rational(3, 7);
        Assert.Equal(1, r.Sign);
    }

    [Fact]
    public void Sign_Negative()
    {
        var r = new Rational(-3, 7);
        Assert.Equal(-1, r.Sign);
    }

    [Fact]
    public void Sign_Zero()
    {
        Assert.Equal(0, Rational.Zero.Sign);
    }

    [Fact]
    public void Addition_WithDifferentDenominators()
    {
        // 1/3 + 1/6 = 1/2
        var a = new Rational(1, 3);
        var b = new Rational(1, 6);
        Assert.Equal(new Rational(1, 2), a + b);
    }

    [Fact]
    public void Subtraction_ResultingInZero()
    {
        var a = new Rational(5, 7);
        Assert.Equal(Rational.Zero, a - a);
    }

    [Fact]
    public void Multiplication_ReducesCorrectly()
    {
        // 2/3 * 3/4 = 1/2
        var a = new Rational(2, 3);
        var b = new Rational(3, 4);
        Assert.Equal(new Rational(1, 2), a * b);
    }

    [Fact]
    public void Division_ByReciprocal()
    {
        // (2/3) / (2/3) = 1
        var a = new Rational(2, 3);
        Assert.Equal(Rational.One, a / a);
    }

    [Fact]
    public void Division_ByZero_Throws()
    {
        Assert.Throws<DivideByZeroException>(() => Rational.One / Rational.Zero);
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
    }

    [Fact]
    public void FromDouble_SmallFraction_Correct()
    {
        var r = Rational.FromDouble(0.125); // 1/8
        Assert.Equal(new Rational(1, 8), r);
    }

    [Fact]
    public void FromDouble_LargeInteger_Correct()
    {
        var r = Rational.FromDouble(1024.0);
        Assert.Equal(new Rational(1024, 1), r);
    }

    [Fact]
    public void Comparison_LessThanOrEqual()
    {
        var a = new Rational(1, 3);
        var b = new Rational(1, 2);
        Assert.True(a <= b);
        Assert.True(a <= a);
    }

    [Fact]
    public void Comparison_GreaterThanOrEqual()
    {
        var a = new Rational(2, 3);
        var b = new Rational(1, 3);
        Assert.True(a >= b);
        Assert.True(a >= a);
    }

    [Fact]
    public void Equals_DifferentRepresentations_Equal()
    {
        var a = new Rational(2, 4);
        var b = new Rational(3, 6);
        Assert.Equal(a, b);
    }

    [Fact]
    public void Negate_FlipsSign()
    {
        var r = new Rational(3, 7);
        Assert.Equal(new Rational(-3, 7), -r);
    }

    [Fact]
    public void LargeNumbers_ArithmeticCorrect()
    {
        var big = new Rational(new BigInteger(long.MaxValue), BigInteger.One);
        var result = big - big;
        Assert.Equal(Rational.Zero, result);
    }

    [Fact]
    public void MultiplicativeInverse()
    {
        var r = new Rational(3, 7);
        var inv = Rational.One / r;
        Assert.Equal(Rational.One, r * inv);
    }
}
