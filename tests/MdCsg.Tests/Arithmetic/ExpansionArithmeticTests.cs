using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

public class ExpansionArithmeticTests
{
    [Fact]
    public void TwoSum_ExactForSmallValues()
    {
        var (sum, err) = ExpansionArithmetic.TwoSum(1.0, 2.0);
        Assert.Equal(3.0, sum);
        Assert.Equal(0.0, err);
    }

    [Fact]
    public void TwoSum_CapturesRoundingError()
    {
        // 1 + epsilon/2 should have rounding error
        double big = 1e16;
        double small = 1.0;
        var (sum, err) = ExpansionArithmetic.TwoSum(big, small);
        Assert.Equal(big + small, sum);
        // The exact sum is big + small, and err captures what was lost
        Assert.Equal(big + small, sum + err);
    }

    [Fact]
    public void TwoDiff_Exact()
    {
        var (diff, err) = ExpansionArithmetic.TwoDiff(5.0, 3.0);
        Assert.Equal(2.0, diff);
        Assert.Equal(0.0, err);
    }

    [Fact]
    public void TwoProduct_Exact()
    {
        var (prod, err) = ExpansionArithmetic.TwoProduct(3.0, 4.0);
        Assert.Equal(12.0, prod);
        Assert.Equal(0.0, err);
    }

    [Fact]
    public void TwoProduct_CapturesError()
    {
        // Large values whose product loses precision
        double a = 1e15 + 1;
        double b = 1e15 + 2;
        var (prod, err) = ExpansionArithmetic.TwoProduct(a, b);
        // The exact value should be recoverable
        Assert.Equal(a * b, prod + err);
    }

    [Fact]
    public void GrowExpansion_AddsToExpansion()
    {
        Span<double> e = stackalloc double[] { 1.0, 2.0 };
        Span<double> h = stackalloc double[4];
        int len = ExpansionArithmetic.GrowExpansion(e, 3.0, h);
        // Sum of expansion should be 1 + 2 + 3 = 6
        double sum = 0;
        for (int i = 0; i < len; i++) sum += h[i];
        Assert.Equal(6.0, sum, 1e-15);
    }

    [Fact]
    public void Sign_Positive()
    {
        Span<double> e = stackalloc double[] { 0.0, 1.0 };
        Assert.Equal(1, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Sign_Negative()
    {
        Span<double> e = stackalloc double[] { 0.0, -1.0 };
        Assert.Equal(-1, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Sign_Zero()
    {
        Span<double> e = stackalloc double[] { 0.0, 0.0 };
        Assert.Equal(0, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void ExpansionSum_TwoExpansions()
    {
        Span<double> a = stackalloc double[] { 1.0, 2.0 };
        Span<double> b = stackalloc double[] { 3.0, 4.0 };
        Span<double> h = stackalloc double[8];
        int len = ExpansionArithmetic.ExpansionSum(a, b, h);
        double sum = 0;
        for (int i = 0; i < len; i++) sum += h[i];
        Assert.Equal(10.0, sum, 1e-15);
    }

    [Fact]
    public void ScaleExpansion_ByScalar()
    {
        Span<double> e = stackalloc double[] { 1.0, 2.0 };
        Span<double> h = stackalloc double[8];
        int len = ExpansionArithmetic.ScaleExpansion(e, 3.0, h);
        double sum = 0;
        for (int i = 0; i < len; i++) sum += h[i];
        Assert.Equal(9.0, sum, 1e-15);
    }
}
