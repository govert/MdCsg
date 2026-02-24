using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

public class RationalTests
{
    [Fact]
    public void FromDouble_ExactForIntegers()
    {
        var r = Rational.FromDouble(42.0);
        Assert.Equal(42, (int)r.Numerator);
        Assert.Equal(1, (int)r.Denominator);
    }

    [Fact]
    public void FromDouble_ExactForHalf()
    {
        var r = Rational.FromDouble(0.5);
        Assert.Equal(1, (int)r.Numerator);
        Assert.Equal(2, (int)r.Denominator);
    }

    [Fact]
    public void FromDouble_Zero()
    {
        var r = Rational.FromDouble(0.0);
        Assert.Equal(0, r.Sign);
    }

    [Fact]
    public void FromDouble_Negative()
    {
        var r = Rational.FromDouble(-3.0);
        Assert.Equal(-1, r.Sign);
        Assert.Equal(-3, (int)r.Numerator);
    }

    [Fact]
    public void Arithmetic_AddSubMulDiv()
    {
        var a = new Rational(1, 3);
        var b = new Rational(1, 6);

        Assert.Equal(new Rational(1, 2), a + b);
        Assert.Equal(new Rational(1, 6), a - b);
        Assert.Equal(new Rational(1, 18), a * b);
        Assert.Equal(new Rational(2, 1), a / b);
    }

    [Fact]
    public void Sign_Correct()
    {
        Assert.Equal(1, new Rational(3, 4).Sign);
        Assert.Equal(-1, new Rational(-3, 4).Sign);
        Assert.Equal(0, Rational.Zero.Sign);
    }

    [Fact]
    public void Comparison()
    {
        var a = new Rational(1, 3);
        var b = new Rational(1, 2);
        Assert.True(a < b);
        Assert.True(b > a);
        Assert.True(a != b);
    }

    [Fact]
    public void NormalizesSign()
    {
        // Negative denominator should be normalized
        var r = new Rational(1, -2);
        Assert.Equal(-1, r.Sign);
        Assert.True(r.Denominator > 0);
    }

    [Fact]
    public void Reduces()
    {
        var r = new Rational(6, 4);
        Assert.Equal(new Rational(3, 2), r);
    }

    [Fact]
    public void FromDouble_PreservesExactValue()
    {
        // Test with a value that has exact IEEE representation
        double val = 0.125; // 1/8 exactly
        var r = Rational.FromDouble(val);
        Assert.Equal(new Rational(1, 8), r);
    }
}
