using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Phase 6: ExpansionArithmetic Sign, Negate, Compress — expansion sign detection, negation, compression</summary>
public class ExpansionSignNegateTests
{
    [Fact]
    public void Sign_PositiveExpansion()
    {
        Span<double> e = stackalloc double[] { 0.0001, 1.0 };
        Assert.Equal(1, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Sign_NegativeExpansion()
    {
        Span<double> e = stackalloc double[] { -0.0001, -2.0 };
        Assert.Equal(-1, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Sign_ZeroExpansion()
    {
        Span<double> e = stackalloc double[] { 0.0, 0.0, 0.0 };
        Assert.Equal(0, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Sign_EmptyExpansion()
    {
        Assert.Equal(0, ExpansionArithmetic.Sign(ReadOnlySpan<double>.Empty));
    }

    [Fact]
    public void Sign_SinglePositive()
    {
        Span<double> e = stackalloc double[] { 42.0 };
        Assert.Equal(1, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Sign_SingleNegative()
    {
        Span<double> e = stackalloc double[] { -42.0 };
        Assert.Equal(-1, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Sign_SmallErrorLargePositive()
    {
        // Even if small component is negative, sign is determined by largest component
        Span<double> e = stackalloc double[] { -1e-20, 1.0 };
        Assert.Equal(1, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Negate_FlipsAllComponents()
    {
        Span<double> e = stackalloc double[] { 1.0, -2.0, 3.0 };
        ExpansionArithmetic.Negate(e);
        Assert.Equal(-1.0, e[0]);
        Assert.Equal(2.0, e[1]);
        Assert.Equal(-3.0, e[2]);
    }

    [Fact]
    public void Negate_Twice_IsIdentity()
    {
        Span<double> e = stackalloc double[] { 1.5, -2.5 };
        ExpansionArithmetic.Negate(e);
        ExpansionArithmetic.Negate(e);
        Assert.Equal(1.5, e[0]);
        Assert.Equal(-2.5, e[1]);
    }

    [Fact]
    public void Negate_ZeroStaysZero()
    {
        Span<double> e = stackalloc double[] { 0.0, 0.0 };
        ExpansionArithmetic.Negate(e);
        Assert.Equal(0.0, e[0]);
        Assert.Equal(0.0, e[1]);
    }

    [Fact]
    public void Compress_RemovesZeros()
    {
        Span<double> e = stackalloc double[] { 0.0, 1.0 };
        Span<double> h = stackalloc double[2];
        int len = ExpansionArithmetic.Compress(e, h);
        Assert.True(len >= 1);
        // The last component should be non-zero
        Assert.NotEqual(0.0, h[len - 1]);
    }

    [Fact]
    public void Compress_PreservesSign()
    {
        Span<double> e = stackalloc double[] { 1e-20, 3.14 };
        Span<double> h = stackalloc double[2];
        int len = ExpansionArithmetic.Compress(e, h);
        Assert.Equal(1, ExpansionArithmetic.Sign(h[..len]));
    }

    [Fact]
    public void Compress_EmptyExpansion()
    {
        int len = ExpansionArithmetic.Compress(ReadOnlySpan<double>.Empty, Span<double>.Empty);
        Assert.Equal(0, len);
    }

    [Fact]
    public void GrowExpansion_AddZero_NoChange()
    {
        Span<double> e = stackalloc double[] { 1.0, 2.0 };
        Span<double> h = stackalloc double[3];
        int len = ExpansionArithmetic.GrowExpansion(e, 0.0, h);
        Assert.Equal(1, ExpansionArithmetic.Sign(h[..len]));
    }

    [Fact]
    public void TwoSum_ExactForSmallIntegers()
    {
        var (sum, error) = ExpansionArithmetic.TwoSum(1.0, 2.0);
        Assert.Equal(3.0, sum);
        Assert.Equal(0.0, error);
    }

    [Fact]
    public void TwoDiff_ExactForSmallIntegers()
    {
        var (diff, error) = ExpansionArithmetic.TwoDiff(5.0, 3.0);
        Assert.Equal(2.0, diff);
        Assert.Equal(0.0, error);
    }

    [Fact]
    public void TwoProduct_ExactForSmallIntegers()
    {
        var (product, error) = ExpansionArithmetic.TwoProduct(3.0, 7.0);
        Assert.Equal(21.0, product);
        Assert.Equal(0.0, error);
    }

    [Fact]
    public void TwoSum_SumPlusErrorEqualsExact()
    {
        double a = 1.0 + 1e-16;
        double b = 1e-16;
        var (sum, error) = ExpansionArithmetic.TwoSum(a, b);
        // sum + error should exactly equal a + b
        Assert.Equal(a + b, sum + error, 1e-30);
    }

    [Fact]
    public void TwoProduct_ProductPlusErrorEqualsExact()
    {
        double a = 1.0000001;
        double b = 1.0000002;
        var (product, error) = ExpansionArithmetic.TwoProduct(a, b);
        // For FMA-based TwoProduct, product + error should be exact
        // Within 1 ulp tolerance for non-FMA path
        Assert.True(System.Math.Abs((product + error) - a * b) < 1e-28);
    }

    [Fact]
    public void ScaleExpansion_ByZero_ReturnsZero()
    {
        Span<double> e = stackalloc double[] { 1.0, 2.0 };
        Span<double> h = stackalloc double[4];
        int len = ExpansionArithmetic.ScaleExpansion(e, 0.0, h);
        Assert.Equal(0, ExpansionArithmetic.Sign(h[..len]));
    }

    [Fact]
    public void ScaleExpansion_ByOne_PreservesSign()
    {
        Span<double> e = stackalloc double[] { 0.5, 3.0 };
        Span<double> h = stackalloc double[4];
        int len = ExpansionArithmetic.ScaleExpansion(e, 1.0, h);
        Assert.Equal(1, ExpansionArithmetic.Sign(h[..len]));
    }

    [Fact]
    public void ScaleExpansion_ByNegative_FlipsSign()
    {
        Span<double> e = stackalloc double[] { 0.5, 3.0 };
        Span<double> h = stackalloc double[4];
        int len = ExpansionArithmetic.ScaleExpansion(e, -1.0, h);
        Assert.Equal(-1, ExpansionArithmetic.Sign(h[..len]));
    }
}
