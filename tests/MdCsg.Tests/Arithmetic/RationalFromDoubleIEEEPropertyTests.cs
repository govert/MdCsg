using System.Numerics;
using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Phase 6: Rational — FromDouble IEEE754 decomposition, power of two, negative, subnormal edge cases</summary>
public class RationalFromDoubleIEEEPropertyTests
{
    [Fact]
    public void FromDouble_Zero_IsRationalZero()
    {
        var r = Rational.FromDouble(0.0);
        Assert.Equal(Rational.Zero, r);
    }

    [Fact]
    public void FromDouble_One_IsRationalOne()
    {
        var r = Rational.FromDouble(1.0);
        Assert.Equal(Rational.One, r);
    }

    [Fact]
    public void FromDouble_Half_IsOneOverTwo()
    {
        var r = Rational.FromDouble(0.5);
        Assert.Equal(new Rational(1, 2), r);
    }

    [Fact]
    public void FromDouble_Quarter_IsOneOverFour()
    {
        var r = Rational.FromDouble(0.25);
        Assert.Equal(new Rational(1, 4), r);
    }

    [Fact]
    public void FromDouble_NegativeOne_Correct()
    {
        var r = Rational.FromDouble(-1.0);
        Assert.Equal(new Rational(-1, 1), r);
    }

    [Fact]
    public void FromDouble_NegativeHalf_Correct()
    {
        var r = Rational.FromDouble(-0.5);
        Assert.Equal(new Rational(-1, 2), r);
    }

    [Fact]
    public void FromDouble_PowerOfTwo_ExactDenominator()
    {
        for (int exp = 0; exp <= 10; exp++)
        {
            double val = System.Math.Pow(2, exp);
            var r = Rational.FromDouble(val);
            Assert.Equal(new Rational((long)val, 1), r);
        }
    }

    [Fact]
    public void FromDouble_InversePowerOfTwo_ExactFraction()
    {
        for (int exp = 1; exp <= 10; exp++)
        {
            double val = 1.0 / System.Math.Pow(2, exp);
            var r = Rational.FromDouble(val);
            Assert.Equal(BigInteger.One, r.Numerator);
            Assert.Equal(new BigInteger(1L << exp), r.Denominator);
        }
    }

    [Fact]
    public void FromDouble_ThreeQuarters_IsThreeOverFour()
    {
        var r = Rational.FromDouble(0.75);
        Assert.Equal(new Rational(3, 4), r);
    }

    [Fact]
    public void FromDouble_ToDouble_Roundtrip()
    {
        var testValues = new[] { 0.1, 0.2, 0.3, 1.5, 2.5, 3.14159, -7.7, 100.001 };
        foreach (double val in testValues)
        {
            var r = Rational.FromDouble(val);
            double back = (double)r.Numerator / (double)r.Denominator;
            Assert.Equal(val, back);
        }
    }

    [Fact]
    public void FromDouble_VerySmall_DoesNotThrow()
    {
        var r = Rational.FromDouble(double.Epsilon); // smallest positive subnormal
        Assert.True(r.Numerator > BigInteger.Zero);
        Assert.True(r.Denominator > BigInteger.Zero);
    }

    [Fact]
    public void FromDouble_MaxValue_DoesNotThrow()
    {
        var r = Rational.FromDouble(double.MaxValue);
        Assert.True(r.Numerator > BigInteger.Zero);
    }

    [Fact]
    public void FromDouble_MinValue_Negative()
    {
        var r = Rational.FromDouble(double.MinValue);
        Assert.True(r.Numerator < BigInteger.Zero);
    }

    [Fact]
    public void Rational_Abs_Positive()
    {
        var r = new Rational(-5, 3);
        var abs = r.Sign < 0 ? -r : r;
        Assert.True(abs.Numerator > BigInteger.Zero);
    }

    [Fact]
    public void Rational_AdditionCommutative()
    {
        var a = new Rational(3, 7);
        var b = new Rational(5, 11);
        Assert.Equal(a + b, b + a);
    }

    [Fact]
    public void Rational_MultiplicationCommutative()
    {
        var a = new Rational(3, 7);
        var b = new Rational(5, 11);
        Assert.Equal(a * b, b * a);
    }

    [Fact]
    public void Rational_SubtractionAnticommutative()
    {
        var a = new Rational(3, 7);
        var b = new Rational(5, 11);
        Assert.Equal(a - b, -(b - a));
    }

    [Fact]
    public void Rational_Distributive()
    {
        var a = new Rational(2, 3);
        var b = new Rational(3, 5);
        var c = new Rational(7, 11);
        Assert.Equal(a * (b + c), a * b + a * c);
    }

    [Fact]
    public void Rational_CompareWithSelf_Equal()
    {
        var a = new Rational(3, 7);
        Assert.True(a <= a);
        Assert.True(a >= a);
        Assert.Equal(a, a);
    }

    [Fact]
    public void Rational_CompareOrdering()
    {
        var a = new Rational(1, 3);
        var b = new Rational(1, 2);
        var c = new Rational(2, 3);
        Assert.True(a < b);
        Assert.True(b < c);
        Assert.True(a < c);
    }
}
