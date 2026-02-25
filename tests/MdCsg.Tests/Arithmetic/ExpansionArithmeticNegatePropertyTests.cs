using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Phase 6: ExpansionArithmetic — Negate, Sign reversal, Compress consistency, TwoProduct edge cases</summary>
public class ExpansionArithmeticNegatePropertyTests
{
    [Fact]
    public void Negate_PositiveExpansion_FlipsSign()
    {
        Span<double> e = stackalloc double[4];
        var (s, err) = ExpansionArithmetic.TwoSum(1.0, 1e-20);
        e[0] = err;
        e[1] = s;
        int eLen = 2;
        Assert.Equal(1, ExpansionArithmetic.Sign(e[..eLen]));
        ExpansionArithmetic.Negate(e[..eLen]);
        Assert.Equal(-1, ExpansionArithmetic.Sign(e[..eLen]));
    }

    [Fact]
    public void Negate_NegativeExpansion_BecomesPositive()
    {
        Span<double> e = stackalloc double[4];
        var (s, err) = ExpansionArithmetic.TwoSum(-1.0, -1e-20);
        e[0] = err;
        e[1] = s;
        int eLen = 2;
        Assert.Equal(-1, ExpansionArithmetic.Sign(e[..eLen]));
        ExpansionArithmetic.Negate(e[..eLen]);
        Assert.Equal(1, ExpansionArithmetic.Sign(e[..eLen]));
    }

    [Fact]
    public void Negate_DoubleNegate_SameSign()
    {
        Span<double> e = stackalloc double[4];
        var (s, err) = ExpansionArithmetic.TwoSum(3.0, 1e-18);
        e[0] = err;
        e[1] = s;
        int eLen = 2;
        int original = ExpansionArithmetic.Sign(e[..eLen]);
        ExpansionArithmetic.Negate(e[..eLen]);
        ExpansionArithmetic.Negate(e[..eLen]);
        Assert.Equal(original, ExpansionArithmetic.Sign(e[..eLen]));
    }

    [Fact]
    public void TwoSum_ExactForIntegers()
    {
        var (s, err) = ExpansionArithmetic.TwoSum(3.0, 5.0);
        Assert.Equal(8.0, s);
        Assert.Equal(0.0, err);
    }

    [Fact]
    public void TwoSum_CapturesRoundingError()
    {
        var (s, err) = ExpansionArithmetic.TwoSum(1.0, 1e-16);
        Assert.Equal(1.0 + 1e-16, s);
        Assert.True(s + err == 1.0 + 1e-16 || System.Math.Abs(s + err - 1.0 - 1e-16) < 1e-30);
    }

    [Fact]
    public void TwoDiff_ExactForIntegers()
    {
        var (d, err) = ExpansionArithmetic.TwoDiff(5.0, 3.0);
        Assert.Equal(2.0, d);
        Assert.Equal(0.0, err);
    }

    [Fact]
    public void TwoProduct_ExactForSmallIntegers()
    {
        var (p, err) = ExpansionArithmetic.TwoProduct(3.0, 5.0);
        Assert.Equal(15.0, p);
        Assert.Equal(0.0, err);
    }

    [Fact]
    public void TwoProduct_ZeroTimes_Any_IsZero()
    {
        var (p, err) = ExpansionArithmetic.TwoProduct(0.0, 12345.0);
        Assert.Equal(0.0, p);
        Assert.Equal(0.0, err);
    }

    [Fact]
    public void Sign_ZeroExpansion_IsZero()
    {
        Span<double> e = stackalloc double[2];
        e[0] = 0.0;
        e[1] = 0.0;
        Assert.Equal(0, ExpansionArithmetic.Sign(e[..2]));
    }

    [Fact]
    public void Sign_SinglePositive_IsPositive()
    {
        Span<double> e = stackalloc double[1];
        e[0] = 1.0;
        Assert.Equal(1, ExpansionArithmetic.Sign(e[..1]));
    }

    [Fact]
    public void Sign_SingleNegative_IsNegative()
    {
        Span<double> e = stackalloc double[1];
        e[0] = -1.0;
        Assert.Equal(-1, ExpansionArithmetic.Sign(e[..1]));
    }

    [Fact]
    public void Compress_SmallExpansion_PreservesSign()
    {
        Span<double> e = stackalloc double[4];
        Span<double> h = stackalloc double[4];
        var (s, err) = ExpansionArithmetic.TwoSum(1.0, 1e-20);
        e[0] = err;
        e[1] = s;
        int origSign = ExpansionArithmetic.Sign(e[..2]);
        int hLen = ExpansionArithmetic.Compress(e[..2], h);
        Assert.Equal(origSign, ExpansionArithmetic.Sign(h[..hLen]));
    }

    [Fact]
    public void GrowExpansion_AddZero_Unchanged()
    {
        Span<double> e = stackalloc double[2];
        Span<double> h = stackalloc double[3];
        e[0] = 1e-20;
        e[1] = 5.0;
        int hLen = ExpansionArithmetic.GrowExpansion(e[..2], 0.0, h);
        int signE = ExpansionArithmetic.Sign(e[..2]);
        int signH = ExpansionArithmetic.Sign(h[..hLen]);
        Assert.Equal(signE, signH);
    }

    [Fact]
    public void ScaleExpansion_ByOne_PreservesSign()
    {
        Span<double> e = stackalloc double[2];
        Span<double> h = stackalloc double[4];
        var (s, err) = ExpansionArithmetic.TwoSum(3.0, 1e-18);
        e[0] = err;
        e[1] = s;
        int origSign = ExpansionArithmetic.Sign(e[..2]);
        int hLen = ExpansionArithmetic.ScaleExpansion(e[..2], 1.0, h);
        Assert.Equal(origSign, ExpansionArithmetic.Sign(h[..hLen]));
    }

    [Fact]
    public void ScaleExpansion_ByNegativeOne_FlipsSign()
    {
        Span<double> e = stackalloc double[2];
        Span<double> h = stackalloc double[4];
        var (s, err) = ExpansionArithmetic.TwoSum(3.0, 1e-18);
        e[0] = err;
        e[1] = s;
        int origSign = ExpansionArithmetic.Sign(e[..2]);
        int hLen = ExpansionArithmetic.ScaleExpansion(e[..2], -1.0, h);
        Assert.Equal(-origSign, ExpansionArithmetic.Sign(h[..hLen]));
    }

    [Fact]
    public void ScaleExpansion_ByZero_SignIsZero()
    {
        Span<double> e = stackalloc double[2];
        Span<double> h = stackalloc double[4];
        var (s, err) = ExpansionArithmetic.TwoSum(3.0, 1e-18);
        e[0] = err;
        e[1] = s;
        int hLen = ExpansionArithmetic.ScaleExpansion(e[..2], 0.0, h);
        Assert.Equal(0, ExpansionArithmetic.Sign(h[..hLen]));
    }

    [Fact]
    public void ExpansionSum_TwoPositive_Positive()
    {
        Span<double> e = stackalloc double[2];
        Span<double> f = stackalloc double[2];
        Span<double> h = stackalloc double[4];
        var (s1, err1) = ExpansionArithmetic.TwoSum(1.0, 1e-20);
        e[0] = err1; e[1] = s1;
        var (s2, err2) = ExpansionArithmetic.TwoSum(2.0, 1e-20);
        f[0] = err2; f[1] = s2;
        int hLen = ExpansionArithmetic.ExpansionSum(e[..2], f[..2], h);
        Assert.Equal(1, ExpansionArithmetic.Sign(h[..hLen]));
    }
}
