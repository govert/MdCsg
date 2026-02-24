using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Phase 6: Arithmetic invariants for expansion and rational types</summary>
public class ArithmeticInvariantTests
{
    [Fact]
    public void TwoSum_ExactSum()
    {
        double a = 1.0, b = 1e-16;
        var (s, e) = ExpansionArithmetic.TwoSum(a, b);
        Assert.Equal(a + b, s + e, 10);
    }

    [Fact]
    public void TwoSum_Symmetric()
    {
        double a = 3.14, b = 2.72;
        var (s1, e1) = ExpansionArithmetic.TwoSum(a, b);
        var (s2, e2) = ExpansionArithmetic.TwoSum(b, a);
        Assert.Equal(s1, s2, 10);
        Assert.Equal(s1 + e1, s2 + e2, 10);
    }

    [Fact]
    public void TwoDiff_ExactDifference()
    {
        double a = 1.0, b = 1e-16;
        var (s, e) = ExpansionArithmetic.TwoDiff(a, b);
        Assert.Equal(a - b, s + e, 10);
    }

    [Fact]
    public void TwoProduct_ExactProduct()
    {
        double a = 3.0, b = 7.0;
        var (p, e) = ExpansionArithmetic.TwoProduct(a, b);
        Assert.Equal(21.0, p + e, 10);
    }

    [Fact]
    public void TwoProduct_SmallValues()
    {
        double a = 1e-100, b = 1e-100;
        var (p, e) = ExpansionArithmetic.TwoProduct(a, b);
        Assert.Equal(a * b, p + e, 10);
    }

    [Fact]
    public void Sign_Positive()
    {
        ReadOnlySpan<double> e = [1e-30, 1e-20, 0.001];
        Assert.Equal(1, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Sign_Negative()
    {
        ReadOnlySpan<double> e = [-1e-30, -1e-20, -0.001];
        Assert.Equal(-1, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Sign_Zero()
    {
        ReadOnlySpan<double> e = [0.0];
        Assert.Equal(0, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Negate_FlipsSign()
    {
        Span<double> e = [1.0, 0.001, 1e-20];
        int signBefore = ExpansionArithmetic.Sign(e);
        ExpansionArithmetic.Negate(e);
        int signAfter = ExpansionArithmetic.Sign(e);
        Assert.Equal(-signBefore, signAfter);
    }

    [Fact]
    public void Negate_DoubleNegate_Identity()
    {
        Span<double> e = [3.14, 0.001, 1e-20];
        double[] original = [3.14, 0.001, 1e-20];
        ExpansionArithmetic.Negate(e);
        ExpansionArithmetic.Negate(e);
        for (int i = 0; i < original.Length; i++)
            Assert.Equal(original[i], e[i], 10);
    }

    [Fact]
    public void Compress_ZeroExpansion()
    {
        ReadOnlySpan<double> e = [0.0, 0.0, 0.0];
        Span<double> h = stackalloc double[3];
        int len = ExpansionArithmetic.Compress(e, h);
        Assert.Equal(0, ExpansionArithmetic.Sign(h[..len]));
    }

    [Fact]
    public void GrowExpansion_AddsValue()
    {
        Span<double> e = [1.0];
        Span<double> h = stackalloc double[4];
        int len = ExpansionArithmetic.GrowExpansion(e, 2.0, h);
        double total = 0;
        for (int i = 0; i < len; i++) total += h[i];
        Assert.Equal(3.0, total, 10);
    }

    [Fact]
    public void ExpansionSum_AddsExpansions()
    {
        Span<double> a = [1.0];
        Span<double> b = [2.0];
        Span<double> h = stackalloc double[4];
        int len = ExpansionArithmetic.ExpansionSum(a, b, h);
        Assert.Equal(1, ExpansionArithmetic.Sign(h[..len]));
    }

    [Fact]
    public void ScaleExpansion_MultipliesByScalar()
    {
        Span<double> e = [1.0];
        Span<double> h = stackalloc double[4];
        int len = ExpansionArithmetic.ScaleExpansion(e, 5.0, h);
        double total = 0;
        for (int i = 0; i < len; i++) total += h[i];
        Assert.Equal(5.0, total, 10);
    }

    [Fact]
    public void ScaleExpansion_ByZero_IsZero()
    {
        Span<double> e = [1.0, 0.001];
        Span<double> h = stackalloc double[8];
        int len = ExpansionArithmetic.ScaleExpansion(e, 0.0, h);
        Assert.Equal(0, ExpansionArithmetic.Sign(h[..len]));
    }

    [Fact]
    public void ScaleExpansion_ByNegative_FlipsSign()
    {
        Span<double> e = [1.0];
        Span<double> h = stackalloc double[4];
        int len = ExpansionArithmetic.ScaleExpansion(e, -1.0, h);
        Assert.Equal(-1, ExpansionArithmetic.Sign(h[..len]));
    }

    [Fact]
    public void Rational_Addition_Commutative()
    {
        var a = Rational.FromDouble(3.0);
        var b = Rational.FromDouble(7.0);
        Assert.Equal(a + b, b + a);
    }

    [Fact]
    public void Rational_Multiplication()
    {
        var a = Rational.FromDouble(3.0);
        var b = Rational.FromDouble(7.0);
        var prod = a * b;
        Assert.Equal(Rational.FromDouble(21.0), prod);
    }

    [Fact]
    public void Rational_Division()
    {
        var a = Rational.FromDouble(10.0);
        var b = Rational.FromDouble(2.0);
        var div = a / b;
        Assert.Equal(Rational.FromDouble(5.0), div);
    }

    [Fact]
    public void Rational_Subtraction()
    {
        var a = Rational.FromDouble(5.0);
        var b = Rational.FromDouble(3.0);
        var diff = a - b;
        Assert.Equal(Rational.FromDouble(2.0), diff);
    }

    [Fact]
    public void Rational_Comparison()
    {
        var a = Rational.FromDouble(1.0);
        var b = Rational.FromDouble(2.0);
        Assert.True(a < b);
        Assert.True(b > a);
        Assert.False(a > b);
    }

    [Fact]
    public void Rational_Equality()
    {
        var a = Rational.FromDouble(42.0);
        var b = Rational.FromDouble(42.0);
        Assert.Equal(a, b);
    }

    [Fact]
    public void Rational_Zero_EqualsItself()
    {
        var zero = Rational.FromDouble(0.0);
        Assert.Equal(zero, zero);
    }

    [Fact]
    public void Rational_Negative()
    {
        var a = Rational.FromDouble(-5.0);
        var b = -Rational.FromDouble(5.0);
        Assert.Equal(a, b);
    }

    [Fact]
    public void Det3x3Sign_Identity_Positive()
    {
        Assert.Equal(1, AdaptivePrecision.Det3x3Sign(1, 0, 0, 0, 1, 0, 0, 0, 1));
    }

    [Fact]
    public void Det3x3Sign_RowSwap_FlipsSign()
    {
        int s1 = AdaptivePrecision.Det3x3Sign(1, 2, 3, 4, 5, 6, 7, 8, 10);
        int s2 = AdaptivePrecision.Det3x3Sign(4, 5, 6, 1, 2, 3, 7, 8, 10);
        Assert.Equal(-s1, s2);
    }

    [Fact]
    public void Det3x3Sign_SingularMatrix_Zero()
    {
        Assert.Equal(0, AdaptivePrecision.Det3x3Sign(1, 0, 0, 0, 1, 0, 1, 1, 0));
    }

    [Fact]
    public void Det3x3Sign_ScaledRows_SameSign()
    {
        int s1 = AdaptivePrecision.Det3x3Sign(1, 2, 3, 4, 5, 6, 7, 8, 10);
        int s2 = AdaptivePrecision.Det3x3Sign(2, 4, 6, 4, 5, 6, 7, 8, 10);
        Assert.Equal(s1, s2);
    }

    [Fact]
    public void Det2x2Sign_Negative()
    {
        // det = 1*4 - 2*3 = -2
        Assert.Equal(-1, AdaptivePrecision.Det2x2Sign(1, 2, 3, 4));
    }

    [Fact]
    public void Det2x2Sign_Zero()
    {
        Assert.Equal(0, AdaptivePrecision.Det2x2Sign(1, 2, 1, 2));
    }

    [Fact]
    public void Det4x4Sign_ScaledIdentity()
    {
        Assert.Equal(1, AdaptivePrecision.Det4x4Sign(
            2, 0, 0, 0,
            0, 2, 0, 0,
            0, 0, 2, 0,
            0, 0, 0, 2));
    }
}
