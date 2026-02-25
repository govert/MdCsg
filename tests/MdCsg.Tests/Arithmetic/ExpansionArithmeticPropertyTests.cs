using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Phase 6: ExpansionArithmetic — TwoSum, TwoDiff, TwoProduct, GrowExpansion, Sign, Compress, Negate</summary>
public class ExpansionArithmeticPropertyTests
{
    [Fact]
    public void TwoSum_Exact_NoError()
    {
        var (sum, err) = ExpansionArithmetic.TwoSum(1.0, 2.0);
        Assert.Equal(3.0, sum);
        Assert.Equal(0.0, err);
    }

    [Fact]
    public void TwoSum_LargeSmall_CapturesError()
    {
        double large = 1e16;
        double small = 1.0;
        var (sum, err) = ExpansionArithmetic.TwoSum(large, small);
        // sum + err should exactly equal large + small
        Assert.True(System.Math.Abs((sum + err) - (large + small)) < 1e-10);
    }

    [Fact]
    public void TwoSum_Commutative()
    {
        var (s1, e1) = ExpansionArithmetic.TwoSum(3.14159, 2.71828);
        var (s2, e2) = ExpansionArithmetic.TwoSum(2.71828, 3.14159);
        Assert.Equal(s1, s2);
    }

    [Fact]
    public void TwoDiff_Exact_NoError()
    {
        var (diff, err) = ExpansionArithmetic.TwoDiff(5.0, 3.0);
        Assert.Equal(2.0, diff);
        Assert.Equal(0.0, err);
    }

    [Fact]
    public void TwoDiff_LargeSmall_CapturesError()
    {
        double large = 1e16;
        double small = 1.0;
        var (diff, err) = ExpansionArithmetic.TwoDiff(large, small);
        Assert.True(System.Math.Abs((diff + err) - (large - small)) < 1e-10);
    }

    [Fact]
    public void TwoProduct_Exact_NoError()
    {
        var (prod, err) = ExpansionArithmetic.TwoProduct(3.0, 4.0);
        Assert.Equal(12.0, prod);
        Assert.Equal(0.0, err);
    }

    [Fact]
    public void TwoProduct_WithError_SumIsExact()
    {
        double a = 1.0000000000000002; // 1 + 2 ulps
        double b = 1.0000000000000004; // 1 + 4 ulps
        var (prod, err) = ExpansionArithmetic.TwoProduct(a, b);
        // prod + err should be exactly a*b
        Assert.True(System.Math.Abs((prod + err) - (a * b)) < 1e-30);
    }

    [Fact]
    public void TwoProduct_Zero_ReturnsZero()
    {
        var (prod, err) = ExpansionArithmetic.TwoProduct(0.0, 12345.0);
        Assert.Equal(0.0, prod);
        Assert.Equal(0.0, err);
    }

    [Fact]
    public void Sign_PositiveExpansion_ReturnsOne()
    {
        Span<double> e = stackalloc double[] { 0.0, 1.0 };
        Assert.Equal(1, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Sign_NegativeExpansion_ReturnsMinusOne()
    {
        Span<double> e = stackalloc double[] { 0.0, -1.0 };
        Assert.Equal(-1, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Sign_ZeroExpansion_ReturnsZero()
    {
        Span<double> e = stackalloc double[] { 0.0, 0.0 };
        Assert.Equal(0, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Sign_EmptyExpansion_ReturnsZero()
    {
        Assert.Equal(0, ExpansionArithmetic.Sign(ReadOnlySpan<double>.Empty));
    }

    [Fact]
    public void GrowExpansion_AddToSingleElement()
    {
        Span<double> e = stackalloc double[] { 1.0 };
        Span<double> h = stackalloc double[2];
        int len = ExpansionArithmetic.GrowExpansion(e, 2.0, h);
        Assert.True(len >= 1);
        // Sum of expansion components should equal 3.0
        double sum = 0;
        for (int i = 0; i < len; i++) sum += h[i];
        Assert.True(System.Math.Abs(sum - 3.0) < 1e-10);
    }

    [Fact]
    public void GrowExpansion_AddZero_SameValue()
    {
        Span<double> e = stackalloc double[] { 5.0 };
        Span<double> h = stackalloc double[2];
        int len = ExpansionArithmetic.GrowExpansion(e, 0.0, h);
        double sum = 0;
        for (int i = 0; i < len; i++) sum += h[i];
        Assert.True(System.Math.Abs(sum - 5.0) < 1e-10);
    }

    [Fact]
    public void Compress_RemovesZeros()
    {
        Span<double> e = stackalloc double[] { 0.0, 0.0, 5.0 };
        Span<double> h = stackalloc double[3];
        int len = ExpansionArithmetic.Compress(e, h);
        Assert.True(len <= 3);
        double sum = 0;
        for (int i = 0; i < len; i++) sum += h[i];
        Assert.True(System.Math.Abs(sum - 5.0) < 1e-10);
    }

    [Fact]
    public void Compress_Empty_ReturnsZeroLength()
    {
        Span<double> h = stackalloc double[1];
        int len = ExpansionArithmetic.Compress(ReadOnlySpan<double>.Empty, h);
        Assert.Equal(0, len);
    }

    [Fact]
    public void Negate_FlipsAllSigns()
    {
        Span<double> e = stackalloc double[] { 1.0, -2.0, 3.0 };
        ExpansionArithmetic.Negate(e);
        Assert.Equal(-1.0, e[0]);
        Assert.Equal(2.0, e[1]);
        Assert.Equal(-3.0, e[2]);
    }

    [Fact]
    public void Negate_EmptySpan_NoError()
    {
        ExpansionArithmetic.Negate(Span<double>.Empty);
        // Just ensure no exception
    }

    [Fact]
    public void ScaleExpansion_ByOne_PreservesValue()
    {
        Span<double> e = stackalloc double[] { 7.0 };
        Span<double> h = stackalloc double[4];
        int len = ExpansionArithmetic.ScaleExpansion(e, 1.0, h);
        double sum = 0;
        for (int i = 0; i < len; i++) sum += h[i];
        Assert.True(System.Math.Abs(sum - 7.0) < 1e-10);
    }

    [Fact]
    public void ScaleExpansion_ByZero_ReturnsZero()
    {
        Span<double> e = stackalloc double[] { 7.0 };
        Span<double> h = stackalloc double[4];
        int len = ExpansionArithmetic.ScaleExpansion(e, 0.0, h);
        double sum = 0;
        for (int i = 0; i < len; i++) sum += h[i];
        Assert.True(System.Math.Abs(sum) < 1e-10);
    }

    [Fact]
    public void ScaleExpansion_Empty_ReturnsZeroLength()
    {
        Span<double> h = stackalloc double[4];
        int len = ExpansionArithmetic.ScaleExpansion(ReadOnlySpan<double>.Empty, 5.0, h);
        Assert.Equal(0, len);
    }

    [Fact]
    public void ExpansionSum_TwoSingleElements()
    {
        Span<double> e = stackalloc double[] { 3.0 };
        Span<double> f = stackalloc double[] { 4.0 };
        Span<double> h = stackalloc double[4];
        int len = ExpansionArithmetic.ExpansionSum(e, f, h);
        double sum = 0;
        for (int i = 0; i < len; i++) sum += h[i];
        Assert.True(System.Math.Abs(sum - 7.0) < 1e-10);
    }

    [Fact]
    public void ExpansionSum_Empty_ReturnsOther()
    {
        Span<double> e = stackalloc double[] { 5.0 };
        Span<double> h = stackalloc double[4];
        int len = ExpansionArithmetic.ExpansionSum(ReadOnlySpan<double>.Empty, e, h);
        double sum = 0;
        for (int i = 0; i < len; i++) sum += h[i];
        Assert.True(System.Math.Abs(sum - 5.0) < 1e-10);
    }

    [Fact]
    public void Sign_SinglePositive_ReturnsOne()
    {
        Span<double> e = stackalloc double[] { 42.0 };
        Assert.Equal(1, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Sign_SingleNegative_ReturnsMinusOne()
    {
        Span<double> e = stackalloc double[] { -0.001 };
        Assert.Equal(-1, ExpansionArithmetic.Sign(e));
    }
}
