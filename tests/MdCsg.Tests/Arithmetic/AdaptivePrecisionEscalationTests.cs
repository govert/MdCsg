using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Phase 6: AdaptivePrecision escalation — near-zero determinants forcing expansion/Rational paths</summary>
public class AdaptivePrecisionEscalationTests
{
    [Fact]
    public void Det2x2Sign_ExactZero()
    {
        // ad - bc = 1*4 - 2*2 = 0
        int sign = AdaptivePrecision.Det2x2Sign(1, 2, 2, 4);
        Assert.Equal(0, sign);
    }

    [Fact]
    public void Det2x2Sign_ClearlyPositive()
    {
        // ad - bc = 3*5 - 1*2 = 13
        int sign = AdaptivePrecision.Det2x2Sign(3, 1, 2, 5);
        Assert.Equal(1, sign);
    }

    [Fact]
    public void Det2x2Sign_ClearlyNegative()
    {
        // ad - bc = 1*2 - 3*5 = -13
        int sign = AdaptivePrecision.Det2x2Sign(1, 3, 5, 2);
        Assert.Equal(-1, sign);
    }

    [Fact]
    public void Det2x2Sign_NearZero_Positive()
    {
        // This is designed to be close to zero but positive
        // ad - bc = (1+eps)*1 - 1*1 = eps > 0
        double eps = 1e-15;
        int sign = AdaptivePrecision.Det2x2Sign(1 + eps, 1, 1, 1);
        Assert.Equal(1, sign);
    }

    [Fact]
    public void Det2x2Sign_NearZero_Negative()
    {
        double eps = 1e-15;
        int sign = AdaptivePrecision.Det2x2Sign(1 - eps, 1, 1, 1);
        Assert.Equal(-1, sign);
    }

    [Fact]
    public void Det2x2Sign_LargeValues_ExactZero()
    {
        // 2^50 * 2^50 - 2^50 * 2^50 = 0
        double big = System.Math.Pow(2, 50);
        int sign = AdaptivePrecision.Det2x2Sign(big, big, big, big);
        Assert.Equal(0, sign);
    }

    [Fact]
    public void Det2x2Sign_LargeValues_TinyDifference()
    {
        // Force escalation: values so large that double can't distinguish
        double big = System.Math.Pow(2, 40);
        double a = big + 1;
        double d = big;
        // (big+1)*big - big*big = big > 0
        int sign = AdaptivePrecision.Det2x2Sign(a, big, big, d);
        Assert.Equal(1, sign);
    }

    [Fact]
    public void Det3x3Sign_IdentityMatrix()
    {
        // |I| = 1
        int sign = AdaptivePrecision.Det3x3Sign(
            1, 0, 0,
            0, 1, 0,
            0, 0, 1);
        Assert.Equal(1, sign);
    }

    [Fact]
    public void Det3x3Sign_SingularMatrix()
    {
        // Row 3 = Row 1 → determinant = 0
        int sign = AdaptivePrecision.Det3x3Sign(
            1, 2, 3,
            4, 5, 6,
            1, 2, 3);
        Assert.Equal(0, sign);
    }

    [Fact]
    public void Det3x3Sign_NegativeDeterminant()
    {
        // Swap two rows of identity → det = -1
        int sign = AdaptivePrecision.Det3x3Sign(
            0, 1, 0,
            1, 0, 0,
            0, 0, 1);
        Assert.Equal(-1, sign);
    }

    [Fact]
    public void Det3x3Sign_NearSingular_Positive()
    {
        double eps = 1e-14;
        // Third row nearly linear combo of first two
        int sign = AdaptivePrecision.Det3x3Sign(
            1, 0, 0,
            0, 1, 0,
            0, 0, eps);
        Assert.Equal(1, sign);
    }

    [Fact]
    public void Det3x3Sign_NearSingular_Negative()
    {
        double eps = 1e-14;
        int sign = AdaptivePrecision.Det3x3Sign(
            1, 0, 0,
            0, 1, 0,
            0, 0, -eps);
        Assert.Equal(-1, sign);
    }

    [Fact]
    public void Det4x4Sign_IdentityMatrix()
    {
        int sign = AdaptivePrecision.Det4x4Sign(
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1);
        Assert.Equal(1, sign);
    }

    [Fact]
    public void Det4x4Sign_SingularMatrix()
    {
        // Row 4 = Row 1 → det = 0
        int sign = AdaptivePrecision.Det4x4Sign(
            1, 2, 3, 4,
            5, 6, 7, 8,
            9, 10, 11, 12,
            1, 2, 3, 4);
        Assert.Equal(0, sign);
    }

    [Fact]
    public void Det4x4Sign_SwapRows_FlipsSign()
    {
        // Permutation matrix → det = -1
        int sign = AdaptivePrecision.Det4x4Sign(
            0, 1, 0, 0,
            1, 0, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1);
        Assert.Equal(-1, sign);
    }

    [Fact]
    public void Det2x2Sign_AllZero()
    {
        Assert.Equal(0, AdaptivePrecision.Det2x2Sign(0, 0, 0, 0));
    }

    [Fact]
    public void Det3x3Sign_AllZero()
    {
        Assert.Equal(0, AdaptivePrecision.Det3x3Sign(0, 0, 0, 0, 0, 0, 0, 0, 0));
    }

    [Fact]
    public void Det4x4Sign_AllZero()
    {
        Assert.Equal(0, AdaptivePrecision.Det4x4Sign(
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0));
    }

    [Fact]
    public void Det2x2Sign_ScaleInvariance()
    {
        // Scaling a row multiplies determinant by that scalar
        // Sign should match
        int sign1 = AdaptivePrecision.Det2x2Sign(3, 1, 2, 5);
        int sign2 = AdaptivePrecision.Det2x2Sign(3 * 1000, 1 * 1000, 2, 5);
        Assert.Equal(sign1, sign2);
    }

    [Fact]
    public void Det3x3Sign_RowSwap_FlipsSign()
    {
        int sign1 = AdaptivePrecision.Det3x3Sign(
            1, 2, 3,
            4, 5, 6,
            7, 8, 10);
        int sign2 = AdaptivePrecision.Det3x3Sign(
            4, 5, 6,
            1, 2, 3,
            7, 8, 10);
        Assert.Equal(-sign1, sign2);
    }
}
