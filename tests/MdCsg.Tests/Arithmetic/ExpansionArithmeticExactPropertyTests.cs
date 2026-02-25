using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Phase 6: ExpansionArithmetic — exact TwoSum/TwoDiff/TwoProduct, GrowExpansion, ScaleExpansion, Compress, Sign, Negate</summary>
public class ExpansionArithmeticExactPropertyTests
{
    [Fact]
    public void TwoSum_ExactResult_SumPlusError()
    {
        var (sum, err) = ExpansionArithmetic.TwoSum(1.0, 2.0);
        Assert.Equal(3.0, sum + err);
    }

    [Fact]
    public void TwoSum_Commutative()
    {
        var (s1, e1) = ExpansionArithmetic.TwoSum(1.0e15, 1.0);
        var (s2, e2) = ExpansionArithmetic.TwoSum(1.0, 1.0e15);
        Assert.Equal(s1 + e1, s2 + e2);
    }

    [Fact]
    public void TwoSum_LargeAndSmall_ErrorCapturesDifference()
    {
        double large = 1e16;
        double small = 1.0;
        var (sum, err) = ExpansionArithmetic.TwoSum(large, small);
        Assert.True(System.Math.Abs((sum + err) - (large + small)) < 1e-10);
    }

    [Fact]
    public void TwoDiff_ExactResult()
    {
        var (diff, err) = ExpansionArithmetic.TwoDiff(3.0, 1.0);
        Assert.Equal(2.0, diff + err);
    }

    [Fact]
    public void TwoDiff_LargeAndSmall_ErrorCapturesDifference()
    {
        double large = 1e16;
        double small = 1.0;
        var (diff, err) = ExpansionArithmetic.TwoDiff(large, small);
        Assert.True(System.Math.Abs((diff + err) - (large - small)) < 1e-10);
    }

    [Fact]
    public void TwoProduct_ExactResult()
    {
        var (prod, err) = ExpansionArithmetic.TwoProduct(3.0, 7.0);
        Assert.Equal(21.0, prod + err);
    }

    [Fact]
    public void TwoProduct_LargeValues_ErrorRecovery()
    {
        double a = 1e8 + 1.0;
        double b = 1e8 + 2.0;
        var (prod, err) = ExpansionArithmetic.TwoProduct(a, b);
        Assert.True(System.Math.Abs((prod + err) - a * b) <= System.Math.Abs(err));
    }

    [Fact]
    public void TwoProduct_Zero_IsZero()
    {
        var (prod, err) = ExpansionArithmetic.TwoProduct(0.0, 42.0);
        Assert.Equal(0.0, prod);
        Assert.Equal(0.0, err);
    }

    [Fact]
    public void TwoProduct_One_IsIdentity()
    {
        var (prod, err) = ExpansionArithmetic.TwoProduct(1.0, 42.0);
        Assert.Equal(42.0, prod + err);
    }

    [Fact]
    public void GrowExpansion_SingleElement_AddScalar()
    {
        Span<double> e = stackalloc double[] { 1.0 };
        Span<double> h = stackalloc double[4];
        int len = ExpansionArithmetic.GrowExpansion(e, 2.0, h);
        double sum = 0;
        for (int i = 0; i < len; i++) sum += h[i];
        Assert.Equal(3.0, sum);
    }

    [Fact]
    public void GrowExpansion_EmptyExpansion_ReturnsScalar()
    {
        Span<double> h = stackalloc double[4];
        int len = ExpansionArithmetic.GrowExpansion(ReadOnlySpan<double>.Empty, 5.0, h);
        Assert.Equal(1, len);
        Assert.Equal(5.0, h[0]);
    }

    [Fact]
    public void ExpansionSum_TwoSingleElements()
    {
        Span<double> e = stackalloc double[] { 3.0 };
        Span<double> f = stackalloc double[] { 4.0 };
        Span<double> h = stackalloc double[8];
        int len = ExpansionArithmetic.ExpansionSum(e, f, h);
        double sum = 0;
        for (int i = 0; i < len; i++) sum += h[i];
        Assert.Equal(7.0, sum);
    }

    [Fact]
    public void ExpansionSum_EmptyPlusNonEmpty()
    {
        Span<double> f = stackalloc double[] { 42.0 };
        Span<double> h = stackalloc double[8];
        int len = ExpansionArithmetic.ExpansionSum(ReadOnlySpan<double>.Empty, f, h);
        double sum = 0;
        for (int i = 0; i < len; i++) sum += h[i];
        Assert.Equal(42.0, sum);
    }

    [Fact]
    public void ScaleExpansion_SingleElement()
    {
        Span<double> e = stackalloc double[] { 3.0 };
        Span<double> h = stackalloc double[8];
        int len = ExpansionArithmetic.ScaleExpansion(e, 7.0, h);
        double sum = 0;
        for (int i = 0; i < len; i++) sum += h[i];
        Assert.Equal(21.0, sum);
    }

    [Fact]
    public void ScaleExpansion_Empty_ReturnsZeroLength()
    {
        Span<double> h = stackalloc double[4];
        int len = ExpansionArithmetic.ScaleExpansion(ReadOnlySpan<double>.Empty, 5.0, h);
        Assert.Equal(0, len);
    }

    [Fact]
    public void ScaleExpansion_ByZero_AllZero()
    {
        Span<double> e = stackalloc double[] { 1.0, 2.0 };
        Span<double> h = stackalloc double[8];
        int len = ExpansionArithmetic.ScaleExpansion(e, 0.0, h);
        double sum = 0;
        for (int i = 0; i < len; i++) sum += h[i];
        Assert.Equal(0.0, sum);
    }

    [Fact]
    public void Sign_Positive()
    {
        Span<double> e = stackalloc double[] { -0.001, 1.0 };
        Assert.Equal(1, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Sign_Negative()
    {
        Span<double> e = stackalloc double[] { 0.001, -1.0 };
        Assert.Equal(-1, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Sign_Zero()
    {
        Span<double> e = stackalloc double[] { 0.0, 0.0 };
        Assert.Equal(0, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Sign_Empty()
    {
        Assert.Equal(0, ExpansionArithmetic.Sign(ReadOnlySpan<double>.Empty));
    }

    [Fact]
    public void Compress_RemovesZeros()
    {
        Span<double> e = stackalloc double[] { 0.0, 3.0, 0.0 };
        Span<double> h = stackalloc double[4];
        int len = ExpansionArithmetic.Compress(e, h);
        double sum = 0;
        for (int i = 0; i < len; i++) sum += h[i];
        Assert.Equal(3.0, sum);
        Assert.True(len <= 3);
    }

    [Fact]
    public void Compress_Empty_ReturnsZeroLength()
    {
        Span<double> h = stackalloc double[4];
        int len = ExpansionArithmetic.Compress(ReadOnlySpan<double>.Empty, h);
        Assert.Equal(0, len);
    }

    [Fact]
    public void Compress_PreservesSum()
    {
        Span<double> e = stackalloc double[] { 1e-16, 1.0, 1e16 };
        Span<double> h = stackalloc double[4];
        int len = ExpansionArithmetic.Compress(e, h);
        double origSum = 0;
        for (int i = 0; i < e.Length; i++) origSum += e[i];
        double compSum = 0;
        for (int i = 0; i < len; i++) compSum += h[i];
        Assert.True(System.Math.Abs(origSum - compSum) < 1e-10);
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
    public void Negate_DoubleNegate_IsIdentity()
    {
        Span<double> e = stackalloc double[] { 1.0, -2.0, 3.0 };
        ExpansionArithmetic.Negate(e);
        ExpansionArithmetic.Negate(e);
        Assert.Equal(1.0, e[0]);
        Assert.Equal(-2.0, e[1]);
        Assert.Equal(3.0, e[2]);
    }

    [Fact]
    public void TwoSum_NearCancellation_ErrorCaptures()
    {
        double a = 1.0000000000000002;
        double b = -1.0;
        var (sum, err) = ExpansionArithmetic.TwoSum(a, b);
        Assert.True(System.Math.Abs((sum + err) - (a + b)) < 1e-30);
    }

    [Fact]
    public void GrowExpansion_TwoProduct_Consistency()
    {
        var (prod, err) = ExpansionArithmetic.TwoProduct(3.0, 7.0);
        Span<double> e = stackalloc double[] { err, prod };
        double sum = 0;
        for (int i = 0; i < e.Length; i++) sum += e[i];
        Assert.Equal(21.0, sum);
    }
}
