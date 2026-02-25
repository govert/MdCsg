using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Phase 6: Rational — comparison, Sign property, special cases, FromDouble edge cases</summary>
public class RationalComparisonTests
{
    [Fact]
    public void Sign_Positive()
    {
        var r = Rational.FromDouble(3.14);
        Assert.Equal(1, r.Sign);
    }

    [Fact]
    public void Sign_Negative()
    {
        var r = Rational.FromDouble(-2.72);
        Assert.Equal(-1, r.Sign);
    }

    [Fact]
    public void Sign_Zero()
    {
        var r = Rational.FromDouble(0.0);
        Assert.Equal(0, r.Sign);
    }

    [Fact]
    public void FromDouble_One()
    {
        var r = Rational.FromDouble(1.0);
        Assert.Equal(1, r.Sign);
    }

    [Fact]
    public void FromDouble_NegativeOne()
    {
        var r = Rational.FromDouble(-1.0);
        Assert.Equal(-1, r.Sign);
    }

    [Fact]
    public void FromDouble_NaN_Throws()
    {
        Assert.Throws<ArgumentException>(() => Rational.FromDouble(double.NaN));
    }

    [Fact]
    public void FromDouble_PositiveInfinity_Throws()
    {
        Assert.Throws<ArgumentException>(() => Rational.FromDouble(double.PositiveInfinity));
    }

    [Fact]
    public void FromDouble_NegativeInfinity_Throws()
    {
        Assert.Throws<ArgumentException>(() => Rational.FromDouble(double.NegativeInfinity));
    }

    [Fact]
    public void Multiplication_SignPreservation()
    {
        var pos = Rational.FromDouble(2);
        var neg = Rational.FromDouble(-3);
        Assert.Equal(-1, (pos * neg).Sign); // positive * negative = negative
    }

    [Fact]
    public void Multiplication_NegNeg_Positive()
    {
        var a = Rational.FromDouble(-2);
        var b = Rational.FromDouble(-3);
        Assert.Equal(1, (a * b).Sign);
    }

    [Fact]
    public void Addition_Cancellation()
    {
        var a = Rational.FromDouble(5);
        var b = Rational.FromDouble(-5);
        Assert.Equal(0, (a + b).Sign);
    }

    [Fact]
    public void Subtraction_SameValue()
    {
        var a = Rational.FromDouble(42);
        Assert.Equal(0, (a - a).Sign);
    }

    [Fact]
    public void FromDouble_SmallPositive()
    {
        var r = Rational.FromDouble(1e-300);
        Assert.Equal(1, r.Sign);
    }

    [Fact]
    public void FromDouble_SmallNegative()
    {
        var r = Rational.FromDouble(-1e-300);
        Assert.Equal(-1, r.Sign);
    }

    [Fact]
    public void FromDouble_LargePositive()
    {
        var r = Rational.FromDouble(1e200);
        Assert.Equal(1, r.Sign);
    }

    [Fact]
    public void Det2x2_Via_Rational_ExactForIntegers()
    {
        // ad - bc = 7*11 - 3*5 = 77 - 15 = 62 > 0
        var a = Rational.FromDouble(7);
        var b = Rational.FromDouble(3);
        var c = Rational.FromDouble(5);
        var d = Rational.FromDouble(11);
        Assert.Equal(1, (a * d - b * c).Sign);
    }

    [Fact]
    public void Multiplication_ByZero_IsZero()
    {
        var a = Rational.FromDouble(42);
        var z = Rational.FromDouble(0);
        Assert.Equal(0, (a * z).Sign);
    }
}
