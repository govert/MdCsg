using System.Numerics;
using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Phase 6: Rational — construction, normalization, arithmetic, comparison, FromDouble, Sign</summary>
public class RationalDeepPropertyTests
{
    [Fact]
    public void Constructor_Normalizes()
    {
        var r = new Rational(4, 6);
        Assert.Equal(new BigInteger(2), r.Numerator);
        Assert.Equal(new BigInteger(3), r.Denominator);
    }

    [Fact]
    public void Constructor_NegativeDenominator_Flips()
    {
        var r = new Rational(3, -5);
        Assert.True(r.Numerator < 0);
        Assert.True(r.Denominator > 0);
    }

    [Fact]
    public void Constructor_ZeroDenominator_Throws()
    {
        Assert.Throws<DivideByZeroException>(() => new Rational(1, 0));
    }

    [Fact]
    public void Zero_HasZeroNumerator()
    {
        Assert.Equal(BigInteger.Zero, Rational.Zero.Numerator);
        Assert.Equal(BigInteger.One, Rational.Zero.Denominator);
    }

    [Fact]
    public void One_HasUnitValues()
    {
        Assert.Equal(BigInteger.One, Rational.One.Numerator);
        Assert.Equal(BigInteger.One, Rational.One.Denominator);
    }

    [Fact]
    public void Addition_Simple()
    {
        var a = new Rational(1, 3);
        var b = new Rational(1, 6);
        var sum = a + b;
        Assert.Equal(new Rational(1, 2), sum);
    }

    [Fact]
    public void Subtraction_Simple()
    {
        var a = new Rational(3, 4);
        var b = new Rational(1, 4);
        Assert.Equal(new Rational(1, 2), a - b);
    }

    [Fact]
    public void Multiplication_Simple()
    {
        var a = new Rational(2, 3);
        var b = new Rational(3, 5);
        Assert.Equal(new Rational(2, 5), a * b);
    }

    [Fact]
    public void Division_Simple()
    {
        var a = new Rational(2, 3);
        var b = new Rational(4, 5);
        // (2/3) / (4/5) = (2/3) * (5/4) = 10/12 = 5/6
        Assert.Equal(new Rational(5, 6), a / b);
    }

    [Fact]
    public void Negation()
    {
        var a = new Rational(3, 7);
        var neg = -a;
        Assert.Equal(new Rational(-3, 7), neg);
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
    public void Comparison_LessThan()
    {
        Assert.True(new Rational(1, 3) < new Rational(1, 2));
    }

    [Fact]
    public void Comparison_GreaterThan()
    {
        Assert.True(new Rational(2, 3) > new Rational(1, 2));
    }

    [Fact]
    public void Equality_SameValue()
    {
        Assert.Equal(new Rational(2, 4), new Rational(1, 2));
    }

    [Fact]
    public void Equality_DifferentValues()
    {
        Assert.NotEqual(new Rational(1, 3), new Rational(1, 4));
    }

    [Fact]
    public void FromDouble_Zero()
    {
        Assert.Equal(Rational.Zero, Rational.FromDouble(0.0));
    }

    [Fact]
    public void FromDouble_One()
    {
        Assert.Equal(Rational.One, Rational.FromDouble(1.0));
    }

    [Fact]
    public void FromDouble_Half()
    {
        Assert.Equal(new Rational(1, 2), Rational.FromDouble(0.5));
    }

    [Fact]
    public void FromDouble_Negative()
    {
        var r = Rational.FromDouble(-1.5);
        Assert.True(r < Rational.Zero);
        Assert.Equal(new Rational(-3, 2), r);
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
    public void ThirdTimesThree_IsOne()
    {
        var third = new Rational(1, 3);
        var result = third * new Rational(3, 1);
        Assert.Equal(Rational.One, result);
    }

    [Fact]
    public void ToString_WholeNumber()
    {
        Assert.Equal("5", new Rational(5, 1).ToString());
    }

    [Fact]
    public void ToString_Fraction()
    {
        Assert.Equal("2/3", new Rational(2, 3).ToString());
    }

    [Fact]
    public void CompareTo_Equal_ReturnsZero()
    {
        Assert.Equal(0, new Rational(1, 2).CompareTo(new Rational(2, 4)));
    }

    [Fact]
    public void GetHashCode_Equal_SameHash()
    {
        var a = new Rational(2, 4);
        var b = new Rational(1, 2);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }
}
