using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Code coverage: AdaptivePrecision escalation paths and Rational edge cases</summary>
public class AdaptivePrecisionCoverageTests
{
    [Fact]
    public void Det2x2Sign_FastPath_ClearlyPositive()
    {
        // Large positive determinant: 10*10 - 1*1 = 99
        Assert.Equal(1, AdaptivePrecision.Det2x2Sign(10, 1, 1, 10));
    }

    [Fact]
    public void Det2x2Sign_FastPath_ClearlyNegative()
    {
        Assert.Equal(-1, AdaptivePrecision.Det2x2Sign(1, 10, 10, 1));
    }

    [Fact]
    public void Det2x2Sign_FastPath_Zero()
    {
        // 2*3 - 6*1 = 0
        Assert.Equal(0, AdaptivePrecision.Det2x2Sign(2, 6, 1, 3));
    }

    [Fact]
    public void Det2x2Sign_NearlyZero_TriggerExpansion()
    {
        // Values chosen so det is very small relative to operands,
        // pushing past the fast-path error bound into expansion arithmetic.
        // det = 1e8 * (1e8 + 1e-8) - (1e8 + 1e-8) * 1e8 = 0
        // But a slightly asymmetric version:
        // det = (1e8 + 1) * 1e8 - 1e8 * (1e8 + 1) = 0 (exact zero triggers rational)
        int sign = AdaptivePrecision.Det2x2Sign(1e8 + 1, 1e8, 1e8 + 1, 1e8);
        // det = (1e8+1)*1e8 - 1e8*(1e8+1) = 0
        Assert.Equal(0, sign);
    }

    [Fact]
    public void Det2x2Sign_ExactZero_TriggerRational()
    {
        // Exact zero should go through all escalation levels
        Assert.Equal(0, AdaptivePrecision.Det2x2Sign(3, 6, 1, 2));
    }

    [Fact]
    public void Det3x3Sign_ClearlyPositive()
    {
        // Identity-like determinant = 1
        int sign = AdaptivePrecision.Det3x3Sign(
            1, 0, 0,
            0, 1, 0,
            0, 0, 1);
        Assert.Equal(1, sign);
    }

    [Fact]
    public void Det3x3Sign_ClearlyNegative()
    {
        // Swap two rows to negate determinant
        int sign = AdaptivePrecision.Det3x3Sign(
            0, 1, 0,
            1, 0, 0,
            0, 0, 1);
        Assert.Equal(-1, sign);
    }

    [Fact]
    public void Det3x3Sign_Zero_SingularMatrix()
    {
        // Row 3 = Row 1 + Row 2
        int sign = AdaptivePrecision.Det3x3Sign(
            1, 0, 0,
            0, 1, 0,
            1, 1, 0);
        Assert.Equal(0, sign);
    }

    [Fact]
    public void Det3x3Sign_NearlyZero_TriggerRational()
    {
        // Values that look nearly singular but aren't
        double eps = 1e-15;
        int sign = AdaptivePrecision.Det3x3Sign(
            1, 0, 0,
            0, 1, 0,
            0, 0, eps);
        Assert.Equal(1, sign);
    }

    [Fact]
    public void Det4x4Sign_ClearlyPositive()
    {
        int sign = AdaptivePrecision.Det4x4Sign(
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1);
        Assert.Equal(1, sign);
    }

    [Fact]
    public void Det4x4Sign_Zero_SingularMatrix()
    {
        int sign = AdaptivePrecision.Det4x4Sign(
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            1, 1, 1, 0); // last row: (1,1,1,0) doesn't make it singular... let me fix
        // Actually this may not be singular. Use duplicate rows:
        // Row 4 = Row 1
        sign = AdaptivePrecision.Det4x4Sign(
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            1, 0, 0, 0);
        Assert.Equal(0, sign);
    }

    [Fact]
    public void Det4x4Sign_NegativeResult()
    {
        // Swap two rows of identity → negative determinant
        int sign = AdaptivePrecision.Det4x4Sign(
            0, 1, 0, 0,
            1, 0, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1);
        Assert.Equal(-1, sign);
    }

    // --- Rational coverage ---
    [Fact]
    public void Rational_FromDouble_NaN_Throws()
    {
        Assert.Throws<ArgumentException>(() => Rational.FromDouble(double.NaN));
    }

    [Fact]
    public void Rational_FromDouble_Infinity_Throws()
    {
        Assert.Throws<ArgumentException>(() => Rational.FromDouble(double.PositiveInfinity));
        Assert.Throws<ArgumentException>(() => Rational.FromDouble(double.NegativeInfinity));
    }

    [Fact]
    public void Rational_FromDouble_Zero()
    {
        var r = Rational.FromDouble(0.0);
        Assert.Equal(0, r.Sign);
    }

    [Fact]
    public void Rational_FromDouble_NegativeZero()
    {
        var r = Rational.FromDouble(-0.0);
        Assert.Equal(0, r.Sign);
    }

    [Fact]
    public void Rational_FromDouble_Subnormal()
    {
        // Smallest positive subnormal: 5e-324
        var r = Rational.FromDouble(double.Epsilon);
        Assert.Equal(1, r.Sign);
    }

    [Fact]
    public void Rational_FromDouble_NegativeSubnormal()
    {
        var r = Rational.FromDouble(-double.Epsilon);
        Assert.Equal(-1, r.Sign);
    }

    [Fact]
    public void Rational_FromDouble_ExactInteger()
    {
        var r = Rational.FromDouble(42.0);
        Assert.Equal(42, (int)r.Numerator);
        Assert.Equal(1, (int)r.Denominator);
    }

    [Fact]
    public void Rational_FromDouble_Half()
    {
        var r = Rational.FromDouble(0.5);
        Assert.Equal(1, (int)r.Numerator);
        Assert.Equal(2, (int)r.Denominator);
    }

    [Fact]
    public void Rational_DivideByZero_Throws()
    {
        Assert.Throws<DivideByZeroException>(() => new Rational(1, 0));
    }

    [Fact]
    public void Rational_NegativeDenominator_Normalized()
    {
        var r = new Rational(3, -4);
        Assert.True(r.Denominator > 0);
        Assert.Equal(-1, r.Sign);
    }

    [Fact]
    public void Rational_Operators()
    {
        var a = Rational.FromDouble(3);
        var b = Rational.FromDouble(4);
        Assert.Equal(7, (int)(a + b).Numerator);
        Assert.Equal(-1, (a - b).Sign);
        Assert.Equal(12, (int)(a * b).Numerator);
        Assert.True(a < b);
        Assert.True(b > a);
        Assert.True(a != b);
    }

    [Fact]
    public void Rational_Division()
    {
        var a = Rational.FromDouble(3);
        var b = Rational.FromDouble(4);
        var r = a / b;
        Assert.Equal(3, (int)r.Numerator);
        Assert.Equal(4, (int)r.Denominator);
    }

    [Fact]
    public void Rational_Negation()
    {
        var a = Rational.FromDouble(5);
        var neg = -a;
        Assert.Equal(-1, neg.Sign);
    }

    [Fact]
    public void Rational_Equality()
    {
        var a = new Rational(2, 4);
        var b = new Rational(1, 2);
        Assert.True(a == b);
        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Rational_ToString_Integer()
    {
        var r = Rational.FromDouble(5);
        Assert.Equal("5", r.ToString());
    }

    [Fact]
    public void Rational_ToString_Fraction()
    {
        var r = new Rational(1, 3);
        Assert.Equal("1/3", r.ToString());
    }

    [Fact]
    public void Rational_CompareTo()
    {
        var a = Rational.FromDouble(1);
        var b = Rational.FromDouble(2);
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(a));
    }

    [Fact]
    public void Rational_LeqGeq()
    {
        var a = Rational.FromDouble(3);
        var b = Rational.FromDouble(3);
        Assert.True(a <= b);
        Assert.True(a >= b);
    }
}
