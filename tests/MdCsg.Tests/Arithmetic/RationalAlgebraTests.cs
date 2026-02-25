using System.Numerics;
using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Phase 6: Rational algebraic property tests — operators, FromDouble, ordering</summary>
public class RationalAlgebraTests
{
    [Fact]
    public void Add_OneHalf_Plus_OneThird()
    {
        var a = new Rational(1, 2);
        var b = new Rational(1, 3);
        var c = a + b;
        Assert.Equal(new Rational(5, 6), c);
    }

    [Fact]
    public void Add_Commutative()
    {
        var a = new Rational(3, 7);
        var b = new Rational(5, 11);
        Assert.Equal(a + b, b + a);
    }

    [Fact]
    public void Add_Zero_Identity()
    {
        var a = new Rational(3, 7);
        Assert.Equal(a, a + Rational.Zero);
    }

    [Fact]
    public void Subtract_SameValue_Zero()
    {
        var a = new Rational(5, 13);
        Assert.Equal(Rational.Zero, a - a);
    }

    [Fact]
    public void Multiply_Commutative()
    {
        var a = new Rational(3, 7);
        var b = new Rational(5, 11);
        Assert.Equal(a * b, b * a);
    }

    [Fact]
    public void Multiply_One_Identity()
    {
        var a = new Rational(5, 13);
        Assert.Equal(a, a * Rational.One);
    }

    [Fact]
    public void Multiply_Zero_IsZero()
    {
        var a = new Rational(5, 13);
        Assert.Equal(Rational.Zero, a * Rational.Zero);
    }

    [Fact]
    public void Divide_ByOne_Identity()
    {
        var a = new Rational(5, 13);
        Assert.Equal(a, a / Rational.One);
    }

    [Fact]
    public void Divide_BySelf_One()
    {
        var a = new Rational(5, 13);
        Assert.Equal(Rational.One, a / a);
    }

    [Fact]
    public void Divide_ByZero_Throws()
    {
        var a = new Rational(5, 13);
        Assert.Throws<DivideByZeroException>(() => a / Rational.Zero);
    }

    [Fact]
    public void Constructor_ZeroDenominator_Throws()
    {
        Assert.Throws<DivideByZeroException>(() => new Rational(1, 0));
    }

    [Fact]
    public void Constructor_NegativeDenominator_Normalized()
    {
        var r = new Rational(1, -3);
        Assert.True(r.Denominator > 0);
        Assert.Equal(-1, r.Sign);
    }

    [Fact]
    public void Constructor_Reduces()
    {
        var r = new Rational(6, 4);
        Assert.Equal(new BigInteger(3), r.Numerator);
        Assert.Equal(new BigInteger(2), r.Denominator);
    }

    [Fact]
    public void Negate()
    {
        var a = new Rational(3, 7);
        var b = -a;
        Assert.Equal(new Rational(-3, 7), b);
    }

    [Fact]
    public void Sign_Positive()
    {
        Assert.Equal(1, new Rational(3, 7).Sign);
    }

    [Fact]
    public void Sign_Negative()
    {
        Assert.Equal(-1, new Rational(-3, 7).Sign);
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
    public void Comparison_LessOrEqual_Equal()
    {
        Assert.True(new Rational(1, 2) <= new Rational(1, 2));
    }

    [Fact]
    public void Comparison_GreaterOrEqual_Equal()
    {
        Assert.True(new Rational(1, 2) >= new Rational(1, 2));
    }

    [Fact]
    public void Equality()
    {
        Assert.True(new Rational(2, 4) == new Rational(1, 2));
    }

    [Fact]
    public void Inequality()
    {
        Assert.True(new Rational(1, 3) != new Rational(1, 2));
    }

    [Fact]
    public void FromDouble_One()
    {
        var r = Rational.FromDouble(1.0);
        Assert.Equal(Rational.One, r);
    }

    [Fact]
    public void FromDouble_Zero()
    {
        var r = Rational.FromDouble(0.0);
        Assert.Equal(Rational.Zero, r);
    }

    [Fact]
    public void FromDouble_Half()
    {
        var r = Rational.FromDouble(0.5);
        Assert.Equal(new Rational(1, 2), r);
    }

    [Fact]
    public void FromDouble_Negative()
    {
        var r = Rational.FromDouble(-1.5);
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
    public void CompareTo_SameValue_Zero()
    {
        Assert.Equal(0, new Rational(1, 2).CompareTo(new Rational(1, 2)));
    }

    [Fact]
    public void ToString_WholeNumber()
    {
        Assert.Equal("3", new Rational(3, 1).ToString());
    }

    [Fact]
    public void ToString_Fraction()
    {
        Assert.Equal("1/3", new Rational(1, 3).ToString());
    }

    [Fact]
    public void GetHashCode_Equal_SameHash()
    {
        var a = new Rational(2, 4);
        var b = new Rational(1, 2);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Distributive_Property()
    {
        var a = new Rational(2, 3);
        var b = new Rational(3, 5);
        var c = new Rational(7, 11);
        // a * (b + c) == a*b + a*c
        Assert.Equal(a * (b + c), a * b + a * c);
    }
}
