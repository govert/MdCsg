using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Phase 6: ExpansionArithmetic — ExpansionSum, Sign, Compress, Negate additional tests</summary>
public class ExpansionSumSignCompressPropertyTests
{
    // ——— Sign ———

    [Fact]
    public void Sign_Empty_Zero()
    {
        Assert.Equal(0, ExpansionArithmetic.Sign(ReadOnlySpan<double>.Empty));
    }

    [Fact]
    public void Sign_SinglePositive_Positive()
    {
        ReadOnlySpan<double> e = new double[] { 3.0 };
        Assert.Equal(1, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Sign_SingleNegative_Negative()
    {
        ReadOnlySpan<double> e = new double[] { -3.0 };
        Assert.Equal(-1, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Sign_SingleZero_Zero()
    {
        ReadOnlySpan<double> e = new double[] { 0.0 };
        Assert.Equal(0, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Sign_MultiElement_UsesLargest()
    {
        // Most significant (last) non-zero component determines sign
        ReadOnlySpan<double> e = new double[] { -1e-16, 1.0 };
        Assert.Equal(1, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Sign_MultiElement_NegativeLargest()
    {
        ReadOnlySpan<double> e = new double[] { 1e-16, -1.0 };
        Assert.Equal(-1, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Sign_AllZeros_Zero()
    {
        ReadOnlySpan<double> e = new double[] { 0.0, 0.0, 0.0 };
        Assert.Equal(0, ExpansionArithmetic.Sign(e));
    }

    // ——— Negate ———

    [Fact]
    public void Negate_FlipsAllComponents()
    {
        Span<double> e = new double[] { 1.0, -2.0, 3.0 };
        ExpansionArithmetic.Negate(e);
        Assert.Equal(-1.0, e[0]);
        Assert.Equal(2.0, e[1]);
        Assert.Equal(-3.0, e[2]);
    }

    [Fact]
    public void Negate_Zeros_StayZero()
    {
        Span<double> e = new double[] { 0.0, 0.0 };
        ExpansionArithmetic.Negate(e);
        Assert.Equal(0.0, e[0]);
        Assert.Equal(0.0, e[1]);
    }

    [Fact]
    public void Negate_DoubleNegate_Identity()
    {
        Span<double> e = new double[] { 1.5, -2.5, 3.5 };
        double[] original = { 1.5, -2.5, 3.5 };
        ExpansionArithmetic.Negate(e);
        ExpansionArithmetic.Negate(e);
        for (int i = 0; i < 3; i++)
            Assert.Equal(original[i], e[i], 15);
    }

    // ——— ExpansionSum ———

    [Fact]
    public void ExpansionSum_TwoSingletons_CorrectSum()
    {
        ReadOnlySpan<double> a = new double[] { 3.0 };
        ReadOnlySpan<double> b = new double[] { 4.0 };
        Span<double> h = stackalloc double[4];
        int len = ExpansionArithmetic.ExpansionSum(a, b, h);
        double sum = 0;
        for (int i = 0; i < len; i++) sum += h[i];
        Assert.Equal(7.0, sum, 15);
    }

    [Fact]
    public void ExpansionSum_WithEmpty_SameAsInput()
    {
        ReadOnlySpan<double> a = new double[] { 3.0, 4.0 };
        ReadOnlySpan<double> b = ReadOnlySpan<double>.Empty;
        Span<double> h = stackalloc double[4];
        int len = ExpansionArithmetic.ExpansionSum(a, b, h);
        double sum = 0;
        for (int i = 0; i < len; i++) sum += h[i];
        Assert.Equal(7.0, sum, 15);
    }

    [Fact]
    public void ExpansionSum_OppositeValues_NearZero()
    {
        ReadOnlySpan<double> a = new double[] { 5.0 };
        ReadOnlySpan<double> b = new double[] { -5.0 };
        Span<double> h = stackalloc double[4];
        int len = ExpansionArithmetic.ExpansionSum(a, b, h);
        double sum = 0;
        for (int i = 0; i < len; i++) sum += h[i];
        Assert.Equal(0.0, sum, 15);
    }

    [Fact]
    public void ExpansionSum_MultiElement_CorrectSum()
    {
        ReadOnlySpan<double> a = new double[] { 1e-16, 1.0 };
        ReadOnlySpan<double> b = new double[] { 1e-16, 2.0 };
        Span<double> h = stackalloc double[8];
        int len = ExpansionArithmetic.ExpansionSum(a, b, h);
        double sum = 0;
        for (int i = 0; i < len; i++) sum += h[i];
        Assert.Equal(3.0 + 2e-16, sum, 10);
    }

    // ——— Compress ———

    [Fact]
    public void Compress_SingleElement_Unchanged()
    {
        ReadOnlySpan<double> e = new double[] { 42.0 };
        Span<double> h = stackalloc double[2];
        int len = ExpansionArithmetic.Compress(e, h);
        Assert.Equal(1, len);
        Assert.Equal(42.0, h[0], 15);
    }

    [Fact]
    public void Compress_WithZeros_RemovesThem()
    {
        ReadOnlySpan<double> e = new double[] { 0.0, 0.0, 5.0 };
        Span<double> h = stackalloc double[4];
        int len = ExpansionArithmetic.Compress(e, h);
        double sum = 0;
        for (int i = 0; i < len; i++) sum += h[i];
        Assert.Equal(5.0, sum, 15);
        Assert.True(len <= 3);
    }

    [Fact]
    public void Compress_PreservesSum()
    {
        ReadOnlySpan<double> e = new double[] { 1e-16, 2e-8, 3.0 };
        Span<double> h = stackalloc double[4];
        int len = ExpansionArithmetic.Compress(e, h);
        double origSum = 1e-16 + 2e-8 + 3.0;
        double compSum = 0;
        for (int i = 0; i < len; i++) compSum += h[i];
        Assert.Equal(origSum, compSum, 10);
    }

    [Fact]
    public void Compress_Empty_ReturnsZero()
    {
        Span<double> h = stackalloc double[2];
        int len = ExpansionArithmetic.Compress(ReadOnlySpan<double>.Empty, h);
        Assert.Equal(0, len);
    }

    [Fact]
    public void Compress_IdempotentLength()
    {
        ReadOnlySpan<double> e = new double[] { 1e-16, 1e-8, 1.0 };
        Span<double> h1 = stackalloc double[4];
        int len1 = ExpansionArithmetic.Compress(e, h1);
        Span<double> h2 = stackalloc double[4];
        int len2 = ExpansionArithmetic.Compress(h1[..len1], h2);
        Assert.Equal(len1, len2);
    }

    // ——— TwoSum/TwoDiff/TwoProduct algebraic properties ———

    [Fact]
    public void TwoSum_ErrorIsZero_ForExactResult()
    {
        var (sum, err) = ExpansionArithmetic.TwoSum(1.0, 2.0);
        Assert.Equal(3.0, sum, 15);
        Assert.Equal(0.0, err, 15);
    }

    [Fact]
    public void TwoDiff_IdenticalValues_ZeroResult()
    {
        var (diff, err) = ExpansionArithmetic.TwoDiff(7.0, 7.0);
        Assert.Equal(0.0, diff + err, 15);
    }

    [Fact]
    public void TwoProduct_PowersOfTwo_ExactResult()
    {
        // Powers of two multiply exactly in FP
        var (prod, err) = ExpansionArithmetic.TwoProduct(4.0, 8.0);
        Assert.Equal(32.0, prod, 15);
        Assert.Equal(0.0, err, 15);
    }
}
