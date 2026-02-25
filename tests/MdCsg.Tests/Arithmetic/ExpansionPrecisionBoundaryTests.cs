using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Phase 6: Expansion arithmetic precision boundaries — exact sums, catastrophic cancellation, known identities</summary>
public class ExpansionPrecisionBoundaryTests
{
    [Fact]
    public void TwoSum_ExactErrorCapture()
    {
        // TwoSum(a, b) = (s, e) where s+e = a+b exactly
        double a = 1e16;
        double b = 1.5;
        var (s, e) = ExpansionArithmetic.TwoSum(a, b);
        // s is the floating-point result, e captures the rounding error
        // s + e must equal a + b to machine precision
        Assert.True(System.Math.Abs((s + e) - (a + b)) < 1e-5);
    }

    [Fact]
    public void TwoDiff_ExactErrorCapture()
    {
        double a = 1.0;
        double b = 1e-16;
        var (s, e) = ExpansionArithmetic.TwoDiff(a, b);
        Assert.True(System.Math.Abs((s + e) - (a - b)) < 1e-30);
    }

    [Fact]
    public void TwoProduct_ExactErrorCapture()
    {
        double a = 1.0 + 1e-10;
        double b = 1.0 - 1e-10;
        var (p, e) = ExpansionArithmetic.TwoProduct(a, b);
        Assert.True(System.Math.Abs((p + e) - a * b) < 1e-25);
    }

    [Fact]
    public void GrowExpansion_MaintainsPrecision()
    {
        Span<double> e = stackalloc double[] { 1e-20 };
        Span<double> h = stackalloc double[4];
        int len = ExpansionArithmetic.GrowExpansion(e, 1e20, h);
        Assert.Equal(1, ExpansionArithmetic.Sign(h[..len]));
    }

    [Fact]
    public void ExpansionSum_CatastrophicCancellation()
    {
        // e1 represents 1.0 + eps, e2 represents -1.0 + eps
        // Sum should be 2*eps > 0
        double eps = 1e-16;
        ReadOnlySpan<double> e1 = new double[] { eps, 1.0 };
        ReadOnlySpan<double> e2 = new double[] { eps, -1.0 };
        Span<double> h = stackalloc double[8];
        int len = ExpansionArithmetic.ExpansionSum(e1, e2, h);
        Assert.Equal(1, ExpansionArithmetic.Sign(h[..len]));
    }

    [Fact]
    public void Sign_PositiveExpansion()
    {
        ReadOnlySpan<double> e = new double[] { 1e-20, 1.0 };
        Assert.Equal(1, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Sign_NegativeExpansion()
    {
        ReadOnlySpan<double> e = new double[] { 1e-20, -1.0 };
        Assert.Equal(-1, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Sign_ZeroExpansion()
    {
        ReadOnlySpan<double> e = new double[] { 0, 0, 0 };
        Assert.Equal(0, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Compress_ReducesOrPreservesLength()
    {
        ReadOnlySpan<double> e = new double[] { 0, 0, 1e-20, 0, 5.0 };
        Span<double> h = stackalloc double[5];
        int len = ExpansionArithmetic.Compress(e, h);
        Assert.True(len <= e.Length);
        Assert.Equal(ExpansionArithmetic.Sign(e), ExpansionArithmetic.Sign(h[..len]));
    }

    [Fact]
    public void Negate_FlipsSign()
    {
        Span<double> e = stackalloc double[] { 1e-20, 3.0 };
        int origSign = ExpansionArithmetic.Sign(e);
        ExpansionArithmetic.Negate(e);
        Assert.Equal(-origSign, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Negate_Twice_RestoresSign()
    {
        Span<double> e = stackalloc double[] { 1e-15, -2.5 };
        int origSign = ExpansionArithmetic.Sign(e);
        ExpansionArithmetic.Negate(e);
        ExpansionArithmetic.Negate(e);
        Assert.Equal(origSign, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void ScaleExpansion_ByOne_PreservesSign()
    {
        ReadOnlySpan<double> e = new double[] { 1e-20, 7.0 };
        Span<double> h = stackalloc double[8];
        int len = ExpansionArithmetic.ScaleExpansion(e, 1.0, h);
        Assert.Equal(ExpansionArithmetic.Sign(e), ExpansionArithmetic.Sign(h[..len]));
    }

    [Fact]
    public void ScaleExpansion_ByNegOne_FlipsSign()
    {
        ReadOnlySpan<double> e = new double[] { 1e-20, 7.0 };
        Span<double> h = stackalloc double[8];
        int len = ExpansionArithmetic.ScaleExpansion(e, -1.0, h);
        Assert.Equal(-1, ExpansionArithmetic.Sign(h[..len]));
    }

    [Fact]
    public void ScaleExpansion_ByZero_IsZero()
    {
        ReadOnlySpan<double> e = new double[] { 1e-20, 7.0 };
        Span<double> h = stackalloc double[8];
        int len = ExpansionArithmetic.ScaleExpansion(e, 0.0, h);
        Assert.Equal(0, ExpansionArithmetic.Sign(h[..len]));
    }

    [Fact]
    public void Det2x2Sign_ExactForNearCancellation()
    {
        // ad - bc where both products are close
        // a=1+eps, d=1-eps, b=1, c=1 → det = (1+eps)(1-eps) - 1 = -eps^2
        double eps = 1e-8;
        var sign = AdaptivePrecision.Det2x2Sign(1 + eps, 1, 1, 1 - eps);
        Assert.Equal(-1, sign);
    }

    [Fact]
    public void Det2x2Sign_ExactIdentity_Zero()
    {
        Assert.Equal(0, AdaptivePrecision.Det2x2Sign(3, 3, 3, 3));
    }

    [Fact]
    public void Det3x3Sign_AntiSymmetry_RowSwap()
    {
        double a11 = 1, a12 = 2, a13 = 3;
        double a21 = 4, a22 = 5, a23 = 6;
        double a31 = 7, a32 = 8, a33 = 10;

        int s1 = AdaptivePrecision.Det3x3Sign(a11, a12, a13, a21, a22, a23, a31, a32, a33);
        int s2 = AdaptivePrecision.Det3x3Sign(a21, a22, a23, a11, a12, a13, a31, a32, a33);
        Assert.Equal(-s1, s2);
    }

    [Fact]
    public void GrowExpansion_LargeSeries_StaysPositive()
    {
        // Sum 1/1 + 1/2 + ... + 1/100 via expansion arithmetic
        Span<double> e = stackalloc double[200];
        e[0] = 1.0;
        int eLen = 1;
        Span<double> temp = stackalloc double[200];

        for (int i = 2; i <= 100; i++)
        {
            int newLen = ExpansionArithmetic.GrowExpansion(e[..eLen], 1.0 / i, temp);
            temp[..newLen].CopyTo(e);
            eLen = newLen;
        }
        Assert.Equal(1, ExpansionArithmetic.Sign(e[..eLen]));
    }

    [Fact]
    public void TwoSum_Commutative()
    {
        double a = 3.14159;
        double b = 2.71828;
        var (s1, e1) = ExpansionArithmetic.TwoSum(a, b);
        var (s2, e2) = ExpansionArithmetic.TwoSum(b, a);
        Assert.Equal(s1, s2);
    }

    [Fact]
    public void TwoProduct_SmallTimesLarge()
    {
        double a = 1e-15;
        double b = 1e15;
        var (p, e) = ExpansionArithmetic.TwoProduct(a, b);
        // a*b = 1.0 exactly (in theory), but floating-point may have small error
        Assert.True(System.Math.Abs(p - 1.0) < 1e-10);
    }
}
