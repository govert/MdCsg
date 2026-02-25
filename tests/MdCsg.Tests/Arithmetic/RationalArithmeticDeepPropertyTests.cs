using System.Numerics;
using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Phase 6: Rational — arithmetic operators, comparison, FromDouble edge cases, normalization, division</summary>
public class RationalArithmeticDeepPropertyTests
{
    [Fact]
    public void Zero_HasZeroNumerator()
    {
        Assert.Equal(BigInteger.Zero, Rational.Zero.Numerator);
        Assert.Equal(BigInteger.One, Rational.Zero.Denominator);
    }

    [Fact]
    public void One_HasUnitNumeratorDenominator()
    {
        Assert.Equal(BigInteger.One, Rational.One.Numerator);
        Assert.Equal(BigInteger.One, Rational.One.Denominator);
    }

    [Fact]
    public void Constructor_NegativeDenominator_Normalizes()
    {
        var r = new Rational(3, -4);
        Assert.Equal(-3, (int)r.Numerator);
        Assert.Equal(4, (int)r.Denominator);
    }

    [Fact]
    public void Constructor_ReducesToLowestTerms()
    {
        var r = new Rational(6, 4);
        Assert.Equal(3, (int)r.Numerator);
        Assert.Equal(2, (int)r.Denominator);
    }

    [Fact]
    public void Constructor_ZeroDenominator_Throws()
    {
        Assert.Throws<DivideByZeroException>(() => new Rational(1, 0));
    }

    [Fact]
    public void Addition_Correct()
    {
        var a = new Rational(1, 3);
        var b = new Rational(1, 6);
        var sum = a + b;
        Assert.Equal(new Rational(1, 2), sum);
    }

    [Fact]
    public void Addition_Identity()
    {
        var a = new Rational(5, 7);
        Assert.Equal(a, a + Rational.Zero);
    }

    [Fact]
    public void Subtraction_Correct()
    {
        var a = new Rational(3, 4);
        var b = new Rational(1, 4);
        Assert.Equal(new Rational(1, 2), a - b);
    }

    [Fact]
    public void Subtraction_Self_Zero()
    {
        var a = new Rational(7, 11);
        Assert.Equal(Rational.Zero, a - a);
    }

    [Fact]
    public void Multiplication_Correct()
    {
        var a = new Rational(2, 3);
        var b = new Rational(3, 5);
        Assert.Equal(new Rational(2, 5), a * b);
    }

    [Fact]
    public void Multiplication_ByZero_IsZero()
    {
        var a = new Rational(7, 11);
        Assert.Equal(Rational.Zero, a * Rational.Zero);
    }

    [Fact]
    public void Multiplication_ByOne_Identity()
    {
        var a = new Rational(7, 11);
        Assert.Equal(a, a * Rational.One);
    }

    [Fact]
    public void Division_Correct()
    {
        var a = new Rational(2, 3);
        var b = new Rational(4, 5);
        // (2/3) / (4/5) = (2*5)/(3*4) = 10/12 = 5/6
        Assert.Equal(new Rational(5, 6), a / b);
    }

    [Fact]
    public void Division_BySelf_IsOne()
    {
        var a = new Rational(7, 11);
        Assert.Equal(Rational.One, a / a);
    }

    [Fact]
    public void Negation_FlipsSign()
    {
        var a = new Rational(3, 5);
        var neg = -a;
        Assert.Equal(-3, (int)neg.Numerator);
        Assert.Equal(5, (int)neg.Denominator);
    }

    [Fact]
    public void Negation_DoubleNegate_Identity()
    {
        var a = new Rational(7, 11);
        Assert.Equal(a, -(-a));
    }

    [Fact]
    public void Sign_Positive()
    {
        Assert.Equal(1, new Rational(3, 5).Sign);
    }

    [Fact]
    public void Sign_Negative()
    {
        Assert.Equal(-1, new Rational(-3, 5).Sign);
    }

    [Fact]
    public void Sign_Zero()
    {
        Assert.Equal(0, Rational.Zero.Sign);
    }

    [Fact]
    public void CompareTo_LessThan()
    {
        var a = new Rational(1, 3);
        var b = new Rational(1, 2);
        Assert.True(a < b);
    }

    [Fact]
    public void CompareTo_GreaterThan()
    {
        var a = new Rational(3, 4);
        var b = new Rational(1, 4);
        Assert.True(a > b);
    }

    [Fact]
    public void CompareTo_Equal()
    {
        var a = new Rational(2, 4);
        var b = new Rational(1, 2);
        Assert.True(a == b);
        Assert.True(a <= b);
        Assert.True(a >= b);
    }

    [Fact]
    public void FromDouble_Integer_ExactRoundtrip()
    {
        var r = Rational.FromDouble(42.0);
        Assert.Equal(42, (int)r.Numerator);
        Assert.Equal(1, (int)r.Denominator);
    }

    [Fact]
    public void FromDouble_Half_ExactRoundtrip()
    {
        var r = Rational.FromDouble(0.5);
        Assert.Equal(1, (int)r.Numerator);
        Assert.Equal(2, (int)r.Denominator);
    }

    [Fact]
    public void FromDouble_Negative_ExactRoundtrip()
    {
        var r = Rational.FromDouble(-3.0);
        Assert.Equal(-3, (int)r.Numerator);
        Assert.Equal(1, (int)r.Denominator);
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
    public void FromDouble_Zero_IsZero()
    {
        Assert.Equal(Rational.Zero, Rational.FromDouble(0.0));
    }

    [Fact]
    public void ToString_Integer_NoSlash()
    {
        Assert.Equal("42", new Rational(42, 1).ToString());
    }

    [Fact]
    public void ToString_Fraction_HasSlash()
    {
        string s = new Rational(1, 3).ToString();
        Assert.Contains("/", s);
    }

    [Fact]
    public void Equals_DifferentReductions_Equal()
    {
        var a = new Rational(2, 4);
        var b = new Rational(3, 6);
        Assert.Equal(a, b);
    }

    [Fact]
    public void GetHashCode_EqualValues_SameHash()
    {
        var a = new Rational(1, 2);
        var b = new Rational(2, 4);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }
}
