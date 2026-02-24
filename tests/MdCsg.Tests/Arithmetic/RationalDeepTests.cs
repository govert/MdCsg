using System.Numerics;
using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Phase 6: Rational arithmetic deep tests — construction, operations, edge cases</summary>
public class RationalDeepTests
{
    [Fact]
    public void Zero_IsZero()
    {
        Assert.Equal(BigInteger.Zero, Rational.Zero.Numerator);
        Assert.Equal(BigInteger.One, Rational.Zero.Denominator);
    }

    [Fact]
    public void One_IsOne()
    {
        Assert.Equal(BigInteger.One, Rational.One.Numerator);
        Assert.Equal(BigInteger.One, Rational.One.Denominator);
    }

    [Fact]
    public void Constructor_Normalizes_GCD()
    {
        var r = new Rational(6, 4);
        Assert.Equal(new BigInteger(3), r.Numerator);
        Assert.Equal(new BigInteger(2), r.Denominator);
    }

    [Fact]
    public void Constructor_NegativeDenominator_Flipped()
    {
        var r = new Rational(3, -4);
        Assert.Equal(new BigInteger(-3), r.Numerator);
        Assert.Equal(new BigInteger(4), r.Denominator);
    }

    [Fact]
    public void Constructor_ZeroDenominator_Throws()
    {
        Assert.Throws<DivideByZeroException>(() => new Rational(1, 0));
    }

    [Fact]
    public void FromDouble_Zero()
    {
        var r = Rational.FromDouble(0.0);
        Assert.Equal(BigInteger.Zero, r.Numerator);
    }

    [Fact]
    public void FromDouble_One()
    {
        var r = Rational.FromDouble(1.0);
        Assert.Equal(BigInteger.One, r.Numerator);
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
        Assert.True(r.Numerator < 0);
        Assert.True(r.Denominator > 0);
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
    public void Addition_Basic()
    {
        var a = new Rational(1, 3);
        var b = new Rational(1, 6);
        var sum = a + b;
        Assert.Equal(new BigInteger(1), sum.Numerator);
        Assert.Equal(new BigInteger(2), sum.Denominator);
    }

    [Fact]
    public void Subtraction_Basic()
    {
        var a = new Rational(1, 2);
        var b = new Rational(1, 3);
        var diff = a - b;
        Assert.Equal(new BigInteger(1), diff.Numerator);
        Assert.Equal(new BigInteger(6), diff.Denominator);
    }

    [Fact]
    public void Multiplication_Basic()
    {
        var a = new Rational(2, 3);
        var b = new Rational(3, 4);
        var prod = a * b;
        Assert.Equal(new BigInteger(1), prod.Numerator);
        Assert.Equal(new BigInteger(2), prod.Denominator);
    }

    [Fact]
    public void Division_Basic()
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
    public void Comparison_LessThan()
    {
        var a = new Rational(1, 3);
        var b = new Rational(1, 2);
        Assert.True(a < b);
        Assert.True(a.CompareTo(b) < 0);
    }

    [Fact]
    public void Comparison_Equals()
    {
        var a = new Rational(2, 4);
        var b = new Rational(1, 2);
        Assert.True(a == b);
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void Comparison_GreaterThan()
    {
        var a = new Rational(3, 4);
        var b = new Rational(1, 4);
        Assert.True(a > b);
    }

    [Fact]
    public void Sign_Positive()
    {
        var r = new Rational(5, 7);
        Assert.True(r.Numerator > 0);
    }

    [Fact]
    public void Sign_Negative()
    {
        var r = new Rational(-5, 7);
        Assert.True(r.Numerator < 0);
    }

    [Fact]
    public void Sign_Zero()
    {
        var r = new Rational(0, 7);
        Assert.True(r.Numerator.IsZero);
    }

    [Fact]
    public void FromDouble_SmallNumber()
    {
        var r = Rational.FromDouble(1e-15);
        Assert.True(r.Numerator > 0);
        Assert.True(r.Denominator > 0);
    }

    [Fact]
    public void FromDouble_LargeNumber()
    {
        var r = Rational.FromDouble(1e15);
        Assert.True(r.Numerator > 0);
    }

    [Fact]
    public void AddZero_Identity()
    {
        var r = new Rational(3, 7);
        var sum = r + Rational.Zero;
        Assert.Equal(r, sum);
    }

    [Fact]
    public void MultiplyByOne_Identity()
    {
        var r = new Rational(3, 7);
        var prod = r * Rational.One;
        Assert.Equal(r, prod);
    }

    [Fact]
    public void MultiplyByZero_Zero()
    {
        var r = new Rational(3, 7);
        var prod = r * Rational.Zero;
        Assert.Equal(Rational.Zero, prod);
    }

    [Fact]
    public void SubtractSelf_Zero()
    {
        var r = new Rational(5, 13);
        var diff = r - r;
        Assert.Equal(Rational.Zero, diff);
    }
}
