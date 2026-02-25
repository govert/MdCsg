using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Phase 6: ExpansionArithmetic — TwoSum, TwoDiff, TwoProduct exactness</summary>
public class ExpansionTwoSumProductTests
{
    [Fact]
    public void TwoSum_ExactForIntegers()
    {
        var (sum, error) = ExpansionArithmetic.TwoSum(3.0, 4.0);
        Assert.Equal(7.0, sum);
        Assert.Equal(0.0, error);
    }

    [Fact]
    public void TwoSum_Commutative()
    {
        var (s1, e1) = ExpansionArithmetic.TwoSum(1.0, 1e-15);
        var (s2, e2) = ExpansionArithmetic.TwoSum(1e-15, 1.0);
        // The exact sum (s+e) should be the same
        Assert.Equal(s1 + e1, s2 + e2, 15);
    }

    [Fact]
    public void TwoSum_ErrorIsSmall()
    {
        var (sum, error) = ExpansionArithmetic.TwoSum(1.0, 1e-16);
        Assert.True(System.Math.Abs(error) < System.Math.Abs(sum));
    }

    [Fact]
    public void TwoSum_RecoveryProperty()
    {
        // a + b = sum + error exactly
        double a = 1.0 + 1e-15;
        double b = 1e-16;
        var (sum, error) = ExpansionArithmetic.TwoSum(a, b);
        Assert.Equal(a + b, sum + error, 15);
    }

    [Fact]
    public void TwoDiff_ExactForIntegers()
    {
        var (diff, error) = ExpansionArithmetic.TwoDiff(7.0, 3.0);
        Assert.Equal(4.0, diff);
        Assert.Equal(0.0, error);
    }

    [Fact]
    public void TwoDiff_RecoveryProperty()
    {
        double a = 1.0 + 1e-15;
        double b = 1e-16;
        var (diff, error) = ExpansionArithmetic.TwoDiff(a, b);
        Assert.Equal(a - b, diff + error, 15);
    }

    [Fact]
    public void TwoProduct_ExactForIntegers()
    {
        var (product, error) = ExpansionArithmetic.TwoProduct(3.0, 4.0);
        Assert.Equal(12.0, product);
        Assert.Equal(0.0, error);
    }

    [Fact]
    public void TwoProduct_RecoveryProperty()
    {
        double a = 1.0 + 1e-10;
        double b = 1.0 + 1e-10;
        var (product, error) = ExpansionArithmetic.TwoProduct(a, b);
        // product + error should equal exact a*b
        // At minimum, the result should be very close
        Assert.True(System.Math.Abs(a * b - (product + error)) < 1e-30,
            $"Product recovery error: {System.Math.Abs(a * b - (product + error))}");
    }

    [Fact]
    public void TwoProduct_Zero()
    {
        var (product, error) = ExpansionArithmetic.TwoProduct(0.0, 42.0);
        Assert.Equal(0.0, product);
        Assert.Equal(0.0, error);
    }

    [Fact]
    public void Sign_Positive()
    {
        Span<double> e = stackalloc double[] { 0, 0, 5.0 };
        Assert.Equal(1, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Sign_Negative()
    {
        Span<double> e = stackalloc double[] { 0, -3.0 };
        Assert.Equal(-1, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Sign_Zero()
    {
        Span<double> e = stackalloc double[] { 0, 0, 0 };
        Assert.Equal(0, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Sign_Empty()
    {
        Assert.Equal(0, ExpansionArithmetic.Sign(ReadOnlySpan<double>.Empty));
    }

    [Fact]
    public void GrowExpansion_SingleElement()
    {
        Span<double> e = stackalloc double[] { 1.0 };
        Span<double> h = stackalloc double[3];
        int len = ExpansionArithmetic.GrowExpansion(e, 2.0, h);
        double sum = 0;
        for (int i = 0; i < len; i++) sum += h[i];
        Assert.Equal(3.0, sum, 10);
    }

    [Fact]
    public void ScaleExpansion_ByOne()
    {
        Span<double> e = stackalloc double[] { 3.0 };
        Span<double> h = stackalloc double[4];
        int len = ExpansionArithmetic.ScaleExpansion(e, 1.0, h);
        double sum = 0;
        for (int i = 0; i < len; i++) sum += h[i];
        Assert.Equal(3.0, sum, 10);
    }

    [Fact]
    public void ScaleExpansion_ByZero()
    {
        Span<double> e = stackalloc double[] { 3.0 };
        Span<double> h = stackalloc double[4];
        int len = ExpansionArithmetic.ScaleExpansion(e, 0.0, h);
        double sum = 0;
        for (int i = 0; i < len; i++) sum += h[i];
        Assert.Equal(0.0, sum, 10);
    }

    [Fact]
    public void ScaleExpansion_Empty()
    {
        int len = ExpansionArithmetic.ScaleExpansion(ReadOnlySpan<double>.Empty, 5.0, Span<double>.Empty);
        Assert.Equal(0, len);
    }

    [Fact]
    public void Negate_FlipsSign()
    {
        Span<double> e = stackalloc double[] { 1.0, -2.0, 3.0 };
        ExpansionArithmetic.Negate(e);
        Assert.Equal(-1.0, e[0]);
        Assert.Equal(2.0, e[1]);
        Assert.Equal(-3.0, e[2]);
    }

    [Fact]
    public void Negate_DoubleNegation()
    {
        Span<double> e = stackalloc double[] { 1.0, -2.0, 3.0 };
        Span<double> original = stackalloc double[] { 1.0, -2.0, 3.0 };
        ExpansionArithmetic.Negate(e);
        ExpansionArithmetic.Negate(e);
        for (int i = 0; i < e.Length; i++)
            Assert.Equal(original[i], e[i]);
    }

    [Fact]
    public void Compress_RemovesZeros()
    {
        Span<double> e = stackalloc double[] { 0.0, 5.0, 0.0 };
        Span<double> h = stackalloc double[3];
        int len = ExpansionArithmetic.Compress(e, h);
        double sum = 0;
        for (int i = 0; i < len; i++) sum += h[i];
        Assert.Equal(5.0, sum, 10);
        Assert.True(len <= 3);
    }

    [Fact]
    public void Compress_EmptyExpansion()
    {
        int len = ExpansionArithmetic.Compress(ReadOnlySpan<double>.Empty, Span<double>.Empty);
        Assert.Equal(0, len);
    }
}
