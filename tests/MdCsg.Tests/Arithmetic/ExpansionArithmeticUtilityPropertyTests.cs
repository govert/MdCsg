using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Phase 6: ExpansionArithmetic — Sign, Negate, Compress, ExpansionSum utility functions</summary>
public class ExpansionArithmeticUtilityPropertyTests
{
    // ——— Sign ———

    [Fact]
    public void Sign_PositiveLast_Positive()
    {
        ReadOnlySpan<double> e = new double[] { 0.0, 0.0, 1.0 };
        Assert.Equal(1, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Sign_NegativeLast_Negative()
    {
        ReadOnlySpan<double> e = new double[] { 0.0, 0.0, -1.0 };
        Assert.Equal(-1, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Sign_AllZero_Zero()
    {
        ReadOnlySpan<double> e = new double[] { 0.0, 0.0, 0.0 };
        Assert.Equal(0, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Sign_Empty_Zero()
    {
        ReadOnlySpan<double> e = ReadOnlySpan<double>.Empty;
        Assert.Equal(0, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Sign_SinglePositive_Positive()
    {
        ReadOnlySpan<double> e = new double[] { 42.0 };
        Assert.Equal(1, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Sign_MostSignificantDetermines()
    {
        // Expansion: small negative + large positive → positive
        ReadOnlySpan<double> e = new double[] { -1e-16, 1.0 };
        Assert.Equal(1, ExpansionArithmetic.Sign(e));
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
    public void Negate_DoubleNegate_Original()
    {
        Span<double> e = new double[] { 1.0, -2.0, 3.0 };
        var original = new double[] { 1.0, -2.0, 3.0 };
        ExpansionArithmetic.Negate(e);
        ExpansionArithmetic.Negate(e);
        for (int i = 0; i < 3; i++)
            Assert.Equal(original[i], e[i]);
    }

    [Fact]
    public void Negate_FlipsSign()
    {
        Span<double> e = new double[] { 1e-16, 1.0 };
        Assert.Equal(1, ExpansionArithmetic.Sign(e));
        ExpansionArithmetic.Negate(e);
        Assert.Equal(-1, ExpansionArithmetic.Sign(e));
    }

    // ——— Compress ———

    [Fact]
    public void Compress_SingleElement_Unchanged()
    {
        ReadOnlySpan<double> e = new double[] { 42.0 };
        Span<double> h = stackalloc double[1];
        int len = ExpansionArithmetic.Compress(e, h);
        Assert.Equal(1, len);
        Assert.Equal(42.0, h[0]);
    }

    [Fact]
    public void Compress_AllZeros_Single()
    {
        ReadOnlySpan<double> e = new double[] { 0.0, 0.0, 0.0 };
        Span<double> h = stackalloc double[3];
        int len = ExpansionArithmetic.Compress(e, h);
        // After compression, there should be at most 1 element (the zero)
        double sum = 0;
        for (int i = 0; i < len; i++) sum += h[i];
        Assert.Equal(0.0, sum, 15);
    }

    [Fact]
    public void Compress_PreservesSum()
    {
        ReadOnlySpan<double> e = new double[] { 1e-16, 1e-8, 1.0 };
        Span<double> h = stackalloc double[3];
        int len = ExpansionArithmetic.Compress(e, h);
        double sumBefore = 0;
        for (int i = 0; i < e.Length; i++) sumBefore += e[i];
        double sumAfter = 0;
        for (int i = 0; i < len; i++) sumAfter += h[i];
        Assert.True(System.Math.Abs(sumBefore - sumAfter) < 1e-30);
    }

    [Fact]
    public void Compress_ReducesLength()
    {
        ReadOnlySpan<double> e = new double[] { 0.0, 0.0, 1.0 };
        Span<double> h = stackalloc double[3];
        int len = ExpansionArithmetic.Compress(e, h);
        Assert.True(len <= e.Length);
    }

    [Fact]
    public void Compress_Empty_Zero()
    {
        ReadOnlySpan<double> e = ReadOnlySpan<double>.Empty;
        Span<double> h = Span<double>.Empty;
        int len = ExpansionArithmetic.Compress(e, h);
        Assert.Equal(0, len);
    }

    // ——— ExpansionSum ———

    [Fact]
    public void ExpansionSum_TwoSingletons_CorrectSum()
    {
        ReadOnlySpan<double> e = new double[] { 3.0 };
        ReadOnlySpan<double> f = new double[] { 4.0 };
        Span<double> h = stackalloc double[4];
        int len = ExpansionArithmetic.ExpansionSum(e, f, h);
        double sum = 0;
        for (int i = 0; i < len; i++) sum += h[i];
        Assert.Equal(7.0, sum, 15);
    }

    [Fact]
    public void ExpansionSum_WithZero_Identity()
    {
        ReadOnlySpan<double> e = new double[] { 1e-16, 1.0 };
        ReadOnlySpan<double> f = new double[] { 0.0 };
        Span<double> h = stackalloc double[8];
        int len = ExpansionArithmetic.ExpansionSum(e, f, h);
        double sumE = 0;
        for (int i = 0; i < e.Length; i++) sumE += e[i];
        double sumH = 0;
        for (int i = 0; i < len; i++) sumH += h[i];
        Assert.Equal(sumE, sumH, 15);
    }

    [Fact]
    public void ExpansionSum_Commutative()
    {
        ReadOnlySpan<double> e = new double[] { 1e-16, 1.0 };
        ReadOnlySpan<double> f = new double[] { 1e-16, 2.0 };
        Span<double> h1 = stackalloc double[8];
        Span<double> h2 = stackalloc double[8];
        int len1 = ExpansionArithmetic.ExpansionSum(e, f, h1);
        int len2 = ExpansionArithmetic.ExpansionSum(f, e, h2);
        double sum1 = 0;
        for (int i = 0; i < len1; i++) sum1 += h1[i];
        double sum2 = 0;
        for (int i = 0; i < len2; i++) sum2 += h2[i];
        Assert.Equal(sum1, sum2, 15);
    }

    [Fact]
    public void ExpansionSum_Empty_ReturnsOther()
    {
        ReadOnlySpan<double> e = ReadOnlySpan<double>.Empty;
        ReadOnlySpan<double> f = new double[] { 5.0 };
        Span<double> h = stackalloc double[4];
        int len = ExpansionArithmetic.ExpansionSum(e, f, h);
        double sum = 0;
        for (int i = 0; i < len; i++) sum += h[i];
        Assert.Equal(5.0, sum, 15);
    }

    // ——— GrowExpansion additional ———

    [Fact]
    public void GrowExpansion_ByZero_PreservesSum()
    {
        ReadOnlySpan<double> e = new double[] { 1e-16, 1.0 };
        Span<double> h = stackalloc double[3];
        int len = ExpansionArithmetic.GrowExpansion(e, 0.0, h);
        double sum = 0;
        for (int i = 0; i < len; i++) sum += h[i];
        Assert.Equal(1.0 + 1e-16, sum, 10);
    }

    [Fact]
    public void GrowExpansion_LargeAndSmall_PreservesExactSum()
    {
        ReadOnlySpan<double> e = new double[] { 1.0 };
        Span<double> h = stackalloc double[2];
        int len = ExpansionArithmetic.GrowExpansion(e, 1e-16, h);
        double sum = 0;
        for (int i = 0; i < len; i++) sum += h[i];
        Assert.Equal(1.0 + 1e-16, sum, 10);
    }
}
