using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Code coverage: ExpansionArithmetic internal operations</summary>
public class ExpansionArithmeticCoverageTests
{
    [Fact]
    public void TwoSum_ExactForSmallValues()
    {
        var (sum, err) = ExpansionArithmetic.TwoSum(1.0, 2.0);
        Assert.Equal(3.0, sum);
        Assert.Equal(0.0, err);
    }

    [Fact]
    public void TwoSum_CapturesRoundoffError()
    {
        // 1.0 + 1e-16: the sum is 1.0 (rounded), error captures 1e-16
        var (sum, err) = ExpansionArithmetic.TwoSum(1.0, 1e-16);
        Assert.Equal(1.0, sum + err, 5);
        Assert.True(System.Math.Abs(1.0 + 1e-16 - (sum + err)) < 1e-30);
    }

    [Fact]
    public void TwoDiff_ExactForSmallValues()
    {
        var (diff, err) = ExpansionArithmetic.TwoDiff(5.0, 3.0);
        Assert.Equal(2.0, diff);
        Assert.Equal(0.0, err);
    }

    [Fact]
    public void TwoDiff_CapturesRoundoffError()
    {
        var (diff, err) = ExpansionArithmetic.TwoDiff(1.0, 1e-16);
        Assert.True(System.Math.Abs(1.0 - 1e-16 - (diff + err)) < 1e-30);
    }

    [Fact]
    public void TwoProduct_ExactForSmallIntegers()
    {
        var (prod, err) = ExpansionArithmetic.TwoProduct(3.0, 7.0);
        Assert.Equal(21.0, prod);
        Assert.Equal(0.0, err);
    }

    [Fact]
    public void TwoProduct_CapturesError()
    {
        // Product of values that can't be represented exactly
        var (prod, err) = ExpansionArithmetic.TwoProduct(1.0 + 1e-15, 1.0 - 1e-15);
        // The exact product is 1 - 1e-30, but double can't represent this
        Assert.True(System.Math.Abs(prod + err - (1.0 - 1e-30)) < 1e-28);
    }

    [Fact]
    public void GrowExpansion_SingleElement()
    {
        Span<double> e = stackalloc double[] { 3.0 };
        Span<double> h = stackalloc double[4];
        int len = ExpansionArithmetic.GrowExpansion(e, 4.0, h);
        // Sum should be 7.0
        double total = 0;
        for (int i = 0; i < len; i++) total += h[i];
        Assert.Equal(7.0, total);
    }

    [Fact]
    public void GrowExpansion_EmptyExpansion()
    {
        Span<double> e = stackalloc double[0];
        Span<double> h = stackalloc double[4];
        int len = ExpansionArithmetic.GrowExpansion(e, 5.0, h);
        Assert.Equal(1, len);
        Assert.Equal(5.0, h[0]);
    }

    [Fact]
    public void ExpansionSum_TwoExpansions()
    {
        Span<double> e = stackalloc double[] { 1.0, 2.0 };
        Span<double> f = stackalloc double[] { 3.0, 4.0 };
        Span<double> h = stackalloc double[16];
        int len = ExpansionArithmetic.ExpansionSum(e, f, h);
        double total = 0;
        for (int i = 0; i < len; i++) total += h[i];
        Assert.Equal(10.0, total, 10);
    }

    [Fact]
    public void ExpansionSum_EmptyPlusNonEmpty()
    {
        Span<double> e = stackalloc double[0];
        Span<double> f = stackalloc double[] { 3.0 };
        Span<double> h = stackalloc double[8];
        int len = ExpansionArithmetic.ExpansionSum(e, f, h);
        double total = 0;
        for (int i = 0; i < len; i++) total += h[i];
        Assert.Equal(3.0, total);
    }

    [Fact]
    public void ScaleExpansion_SingleElement()
    {
        Span<double> e = stackalloc double[] { 5.0 };
        Span<double> h = stackalloc double[8];
        int len = ExpansionArithmetic.ScaleExpansion(e, 3.0, h);
        double total = 0;
        for (int i = 0; i < len; i++) total += h[i];
        Assert.Equal(15.0, total, 10);
    }

    [Fact]
    public void ScaleExpansion_EmptyExpansion()
    {
        Span<double> e = stackalloc double[0];
        Span<double> h = stackalloc double[8];
        int len = ExpansionArithmetic.ScaleExpansion(e, 3.0, h);
        Assert.Equal(0, len);
    }

    [Fact]
    public void ScaleExpansion_MultiElement()
    {
        Span<double> e = stackalloc double[] { 1.0, 2.0 };
        Span<double> h = stackalloc double[16];
        int len = ExpansionArithmetic.ScaleExpansion(e, 4.0, h);
        double total = 0;
        for (int i = 0; i < len; i++) total += h[i];
        Assert.Equal(12.0, total, 10);
    }

    [Fact]
    public void Sign_PositiveExpansion()
    {
        Span<double> e = stackalloc double[] { 0.0, 3.0 };
        Assert.Equal(1, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Sign_NegativeExpansion()
    {
        Span<double> e = stackalloc double[] { 0.0, -2.0 };
        Assert.Equal(-1, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Sign_ZeroExpansion()
    {
        Span<double> e = stackalloc double[] { 0.0, 0.0 };
        Assert.Equal(0, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Sign_EmptyExpansion()
    {
        Span<double> e = stackalloc double[0];
        Assert.Equal(0, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Compress_RemovesZeros()
    {
        Span<double> e = stackalloc double[] { 0.0, 0.0, 5.0 };
        Span<double> h = stackalloc double[4];
        int len = ExpansionArithmetic.Compress(e, h);
        Assert.True(len >= 1);
        double total = 0;
        for (int i = 0; i < len; i++) total += h[i];
        Assert.Equal(5.0, total, 10);
    }

    [Fact]
    public void Compress_EmptyExpansion()
    {
        Span<double> e = stackalloc double[0];
        Span<double> h = stackalloc double[4];
        int len = ExpansionArithmetic.Compress(e, h);
        Assert.Equal(0, len);
    }

    [Fact]
    public void Compress_SingleElement()
    {
        Span<double> e = stackalloc double[] { 42.0 };
        Span<double> h = stackalloc double[4];
        int len = ExpansionArithmetic.Compress(e, h);
        Assert.Equal(1, len);
        Assert.Equal(42.0, h[0]);
    }

    [Fact]
    public void Negate_AllElements()
    {
        Span<double> e = stackalloc double[] { 1.0, -2.0, 3.0 };
        ExpansionArithmetic.Negate(e);
        Assert.Equal(-1.0, e[0]);
        Assert.Equal(2.0, e[1]);
        Assert.Equal(-3.0, e[2]);
    }

    [Fact]
    public void Negate_Empty()
    {
        Span<double> e = stackalloc double[0];
        ExpansionArithmetic.Negate(e); // Should not throw
    }

    [Fact]
    public void ErrorBound_Constants_Positive()
    {
        Assert.True(ErrorBound.Epsilon > 0);
        Assert.True(ErrorBound.Orient2DErrorBoundA > 0);
        Assert.True(ErrorBound.Orient2DErrorBoundB > 0);
        Assert.True(ErrorBound.Orient2DErrorBoundC > 0);
        Assert.True(ErrorBound.Orient3DErrorBoundA > 0);
        Assert.True(ErrorBound.Orient3DErrorBoundB > 0);
        Assert.True(ErrorBound.Orient3DErrorBoundC > 0);
        Assert.True(ErrorBound.InCircleErrorBoundA > 0);
        Assert.True(ErrorBound.InCircleErrorBoundB > 0);
        Assert.True(ErrorBound.InCircleErrorBoundC > 0);
        Assert.True(ErrorBound.ResultErrBound > 0);
    }

    [Fact]
    public void ErrorBound_Ordering_A_GT_B_GT_C()
    {
        // The A bounds should be >= B bounds >= C bounds
        Assert.True(ErrorBound.Orient2DErrorBoundA >= ErrorBound.Orient2DErrorBoundB);
        Assert.True(ErrorBound.Orient3DErrorBoundA >= ErrorBound.Orient3DErrorBoundB);
        Assert.True(ErrorBound.InCircleErrorBoundA >= ErrorBound.InCircleErrorBoundB);
    }
}
