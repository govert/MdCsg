using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Batch 5: Rational and AdaptivePrecision extended tests (20 tests)</summary>
public class RationalAdaptiveExtTests
{
    // --- Rational extended ---

    [Fact]
    public void Rational_Addition_DifferentDenominators()
    {
        var a = new Rational(1, 3);
        var b = new Rational(1, 6);
        var result = a + b;
        Assert.Equal(1, result.Sign);
        // 1/3 + 1/6 = 1/2
        Assert.Equal(0.5, (double)result.Numerator / (double)result.Denominator, 1e-15);
    }

    [Fact]
    public void Rational_Subtraction_SameValue()
    {
        var a = new Rational(3, 7);
        var result = a - a;
        Assert.Equal(0, result.Sign);
    }

    [Fact]
    public void Rational_Multiplication_Fractions()
    {
        var a = new Rational(2, 3);
        var b = new Rational(3, 4);
        var result = a * b;
        // 2/3 * 3/4 = 1/2
        Assert.Equal(0.5, (double)result.Numerator / (double)result.Denominator, 1e-15);
    }

    [Fact]
    public void Rational_Division_Fractions()
    {
        var a = new Rational(1, 2);
        var b = new Rational(1, 4);
        var result = a / b;
        // (1/2) / (1/4) = 2
        Assert.Equal(2.0, (double)result.Numerator / (double)result.Denominator, 1e-15);
    }

    [Fact]
    public void Rational_FromDouble_One()
    {
        var r = Rational.FromDouble(1.0);
        Assert.Equal(1, r.Sign);
    }

    [Fact]
    public void Rational_FromDouble_NegativeInteger()
    {
        var r = Rational.FromDouble(-5.0);
        Assert.Equal(-1, r.Sign);
    }

    [Fact]
    public void Rational_Comparison_LessThan()
    {
        var a = new Rational(1, 3);
        var b = new Rational(1, 2);
        Assert.True(a < b);
        Assert.False(b < a);
    }

    [Fact]
    public void Rational_Comparison_Equal()
    {
        var a = new Rational(2, 4);
        var b = new Rational(1, 2);
        Assert.True(a == b);
    }

    [Fact]
    public void Rational_Zero_HasZeroSign()
    {
        var r = new Rational(0, 1);
        Assert.Equal(0, r.Sign);
    }

    [Fact]
    public void Rational_MultiplyByZero()
    {
        var a = new Rational(5, 3);
        var zero = new Rational(0, 1);
        var result = a * zero;
        Assert.Equal(0, result.Sign);
    }

    // --- AdaptivePrecision extended ---

    [Fact]
    public void Det2x2Sign_Identity()
    {
        // det [[1,0],[0,1]] = 1 > 0
        Assert.Equal(1, AdaptivePrecision.Det2x2Sign(1, 0, 0, 1));
    }

    [Fact]
    public void Det2x2Sign_Scaled()
    {
        // det [[2,0],[0,3]] = 6 > 0
        Assert.Equal(1, AdaptivePrecision.Det2x2Sign(2, 0, 0, 3));
    }

    [Fact]
    public void Det2x2Sign_RowSwap_Negates()
    {
        int d1 = AdaptivePrecision.Det2x2Sign(1, 2, 3, 4);
        int d2 = AdaptivePrecision.Det2x2Sign(3, 4, 1, 2);
        Assert.Equal(-d1, d2);
    }

    [Fact]
    public void Det3x3Sign_Identity()
    {
        Assert.Equal(1, AdaptivePrecision.Det3x3Sign(
            1, 0, 0,
            0, 1, 0,
            0, 0, 1));
    }

    [Fact]
    public void Det3x3Sign_Diagonal()
    {
        Assert.Equal(1, AdaptivePrecision.Det3x3Sign(
            2, 0, 0,
            0, 3, 0,
            0, 0, 4));
    }

    [Fact]
    public void Det3x3Sign_RowSwap_Negates()
    {
        int d1 = AdaptivePrecision.Det3x3Sign(1, 2, 3, 4, 5, 6, 7, 8, 10);
        int d2 = AdaptivePrecision.Det3x3Sign(4, 5, 6, 1, 2, 3, 7, 8, 10);
        Assert.Equal(-d1, d2);
    }

    [Fact]
    public void Det4x4Sign_Identity()
    {
        Assert.Equal(1, AdaptivePrecision.Det4x4Sign(
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1));
    }

    [Fact]
    public void Det4x4Sign_Diagonal()
    {
        Assert.Equal(1, AdaptivePrecision.Det4x4Sign(
            2, 0, 0, 0,
            0, 3, 0, 0,
            0, 0, 4, 0,
            0, 0, 0, 5));
    }

    [Fact]
    public void Det3x3Sign_DuplicateRows_IsZero()
    {
        Assert.Equal(0, AdaptivePrecision.Det3x3Sign(
            1, 2, 3,
            1, 2, 3,
            4, 5, 6));
    }

    [Fact]
    public void Det2x2Sign_TinyPositive_ResolvedCorrectly()
    {
        // (1 + 1e-15) * 1 - 1 * 1 = 1e-15 > 0
        double eps = 1e-15;
        Assert.Equal(1, AdaptivePrecision.Det2x2Sign(1.0 + eps, 1.0, 1.0, 1.0));
    }
}
