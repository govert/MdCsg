using System.Numerics;
using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Phase 6: Rational — exact arithmetic properties, FromDouble fidelity, operator correctness</summary>
public class RationalExactArithmeticPropertyTests
{
    [Fact]
    public void Zero_HasNumeratorZero()
    {
        Assert.Equal(BigInteger.Zero, Rational.Zero.Numerator);
        Assert.Equal(BigInteger.One, Rational.Zero.Denominator);
    }

    [Fact]
    public void One_HasNumeratorOne()
    {
        Assert.Equal(BigInteger.One, Rational.One.Numerator);
        Assert.Equal(BigInteger.One, Rational.One.Denominator);
    }

    [Fact]
    public void Construction_ReducesToLowestTerms()
    {
        var r = new Rational(6, 4);
        Assert.Equal(new BigInteger(3), r.Numerator);
        Assert.Equal(new BigInteger(2), r.Denominator);
    }

    [Fact]
    public void Construction_NegativeDenominator_Normalized()
    {
        var r = new Rational(3, -5);
        Assert.Equal(new BigInteger(-3), r.Numerator);
        Assert.Equal(new BigInteger(5), r.Denominator);
    }

    [Fact]
    public void Construction_BothNegative_PositiveResult()
    {
        var r = new Rational(-3, -5);
        Assert.Equal(new BigInteger(3), r.Numerator);
        Assert.Equal(new BigInteger(5), r.Denominator);
    }

    [Fact]
    public void Construction_ZeroDenominator_Throws()
    {
        Assert.Throws<DivideByZeroException>(() => new Rational(1, 0));
    }

    [Fact]
    public void FromDouble_IntegerValues_ExactConversion()
    {
        for (int i = -10; i <= 10; i++)
        {
            var r = Rational.FromDouble(i);
            Assert.Equal(new BigInteger(i), r.Numerator);
            Assert.Equal(BigInteger.One, r.Denominator);
        }
    }

    [Fact]
    public void FromDouble_Half_Exact()
    {
        var r = Rational.FromDouble(0.5);
        Assert.Equal(BigInteger.One, r.Numerator);
        Assert.Equal(new BigInteger(2), r.Denominator);
    }

    [Fact]
    public void FromDouble_Quarter_Exact()
    {
        var r = Rational.FromDouble(0.25);
        Assert.Equal(BigInteger.One, r.Numerator);
        Assert.Equal(new BigInteger(4), r.Denominator);
    }

    [Fact]
    public void FromDouble_NegativeValue_CorrectSign()
    {
        var r = Rational.FromDouble(-3.0);
        Assert.Equal(-1, r.Sign);
        Assert.Equal(new BigInteger(-3), r.Numerator);
    }

    [Fact]
    public void FromDouble_Zero_IsZero()
    {
        var r = Rational.FromDouble(0.0);
        Assert.Equal(0, r.Sign);
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
    public void Addition_Commutative()
    {
        var a = new Rational(1, 3);
        var b = new Rational(2, 5);
        Assert.Equal(a + b, b + a);
    }

    [Fact]
    public void Addition_WithZero_Identity()
    {
        var a = new Rational(7, 11);
        Assert.Equal(a, a + Rational.Zero);
    }

    [Fact]
    public void Addition_Correct()
    {
        var a = new Rational(1, 3);
        var b = new Rational(1, 6);
        var sum = a + b;
        Assert.Equal(BigInteger.One, sum.Numerator);
        Assert.Equal(new BigInteger(2), sum.Denominator);
    }

    [Fact]
    public void Subtraction_SameValue_Zero()
    {
        var a = new Rational(7, 11);
        var diff = a - a;
        Assert.Equal(0, diff.Sign);
    }

    [Fact]
    public void Multiplication_Commutative()
    {
        var a = new Rational(2, 3);
        var b = new Rational(5, 7);
        Assert.Equal(a * b, b * a);
    }

    [Fact]
    public void Multiplication_ByOne_Identity()
    {
        var a = new Rational(13, 17);
        Assert.Equal(a, a * Rational.One);
    }

    [Fact]
    public void Multiplication_ByZero_Zero()
    {
        var a = new Rational(13, 17);
        Assert.Equal(0, (a * Rational.Zero).Sign);
    }

    [Fact]
    public void Multiplication_Correct()
    {
        var a = new Rational(2, 3);
        var b = new Rational(3, 4);
        var product = a * b;
        Assert.Equal(BigInteger.One, product.Numerator);
        Assert.Equal(new BigInteger(2), product.Denominator);
    }

    [Fact]
    public void Division_Correct()
    {
        var a = new Rational(1, 2);
        var b = new Rational(1, 3);
        var result = a / b;
        Assert.Equal(new BigInteger(3), result.Numerator);
        Assert.Equal(new BigInteger(2), result.Denominator);
    }

    [Fact]
    public void Division_ByItself_One()
    {
        var a = new Rational(7, 11);
        var result = a / a;
        Assert.Equal(Rational.One, result);
    }

    [Fact]
    public void Negation_FlipsSign()
    {
        var a = new Rational(3, 5);
        var neg = -a;
        Assert.Equal(-1, neg.Sign);
        Assert.Equal(new BigInteger(-3), neg.Numerator);
        Assert.Equal(new BigInteger(5), neg.Denominator);
    }

    [Fact]
    public void Negation_DoubleNegation_Identity()
    {
        var a = new Rational(3, 5);
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
    public void Comparison_Equality()
    {
        var a = new Rational(2, 4);
        var b = new Rational(1, 2);
        Assert.True(a == b);
        Assert.False(a != b);
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
    public void ToString_WholeNumber_NoSlash()
    {
        var r = new Rational(5, 1);
        Assert.Equal("5", r.ToString());
    }

    [Fact]
    public void ToString_Fraction_ShowsSlash()
    {
        var r = new Rational(1, 3);
        Assert.Equal("1/3", r.ToString());
    }

    [Fact]
    public void FromDouble_PowerOfTwo_ExactDenominator()
    {
        var r = Rational.FromDouble(0.125); // 1/8
        Assert.Equal(BigInteger.One, r.Numerator);
        Assert.Equal(new BigInteger(8), r.Denominator);
    }

    [Fact]
    public void Distributive_Property()
    {
        var a = new Rational(2, 3);
        var b = new Rational(3, 7);
        var c = new Rational(5, 11);
        // a * (b + c) == a*b + a*c
        Assert.Equal(a * (b + c), a * b + a * c);
    }

    [Fact]
    public void Associative_Addition()
    {
        var a = new Rational(1, 2);
        var b = new Rational(1, 3);
        var c = new Rational(1, 5);
        Assert.Equal((a + b) + c, a + (b + c));
    }

    [Fact]
    public void Associative_Multiplication()
    {
        var a = new Rational(2, 3);
        var b = new Rational(3, 5);
        var c = new Rational(5, 7);
        Assert.Equal((a * b) * c, a * (b * c));
    }

    [Fact]
    public void GetHashCode_EqualValues_SameHash()
    {
        var a = new Rational(2, 4);
        var b = new Rational(1, 2);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void CompareTo_NegativeVsPositive()
    {
        var neg = new Rational(-1, 2);
        var pos = new Rational(1, 2);
        Assert.True(neg.CompareTo(pos) < 0);
        Assert.True(pos.CompareTo(neg) > 0);
    }

    [Fact]
    public void LargeValues_StillExact()
    {
        var big = new Rational(new BigInteger(long.MaxValue), new BigInteger(long.MaxValue - 1));
        var prod = big * big;
        Assert.True(prod > Rational.One);
    }
}
