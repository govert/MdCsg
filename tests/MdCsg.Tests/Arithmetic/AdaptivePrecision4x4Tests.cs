using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Phase 6: AdaptivePrecision Det4x4Sign — 4×4 determinant sign, escalation to Rational</summary>
public class AdaptivePrecision4x4Tests
{
    [Fact]
    public void Identity_Det4x4_IsPositive()
    {
        int sign = AdaptivePrecision.Det4x4Sign(
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1);
        Assert.Equal(1, sign);
    }

    [Fact]
    public void ZeroMatrix_Det4x4_IsZero()
    {
        int sign = AdaptivePrecision.Det4x4Sign(
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0);
        Assert.Equal(0, sign);
    }

    [Fact]
    public void SwapRows_FlipsSign()
    {
        // Original: det = 1
        int sign1 = AdaptivePrecision.Det4x4Sign(
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1);
        // Swap row 0 and row 1
        int sign2 = AdaptivePrecision.Det4x4Sign(
            0, 1, 0, 0,
            1, 0, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1);
        Assert.Equal(1, sign1);
        Assert.Equal(-1, sign2);
    }

    [Fact]
    public void DuplicateRows_DetIsZero()
    {
        int sign = AdaptivePrecision.Det4x4Sign(
            1, 2, 3, 4,
            1, 2, 3, 4,
            5, 6, 7, 8,
            9, 10, 11, 12);
        Assert.Equal(0, sign);
    }

    [Fact]
    public void ScalarMultiple_PreservesSign()
    {
        int sign1 = AdaptivePrecision.Det4x4Sign(
            1, 0, 0, 0,
            0, 2, 0, 0,
            0, 0, 3, 0,
            0, 0, 0, 4);
        Assert.Equal(1, sign1); // det = 24 > 0
    }

    [Fact]
    public void NearlyDegenerate_FallsToRational()
    {
        // Create a nearly singular matrix that forces escalation
        double eps = 1e-15;
        int sign = AdaptivePrecision.Det4x4Sign(
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            eps, eps, eps, 1);
        Assert.Equal(1, sign); // det ≈ 1
    }

    [Fact]
    public void NegativeDiagonal_Sign()
    {
        int sign = AdaptivePrecision.Det4x4Sign(
            -1, 0, 0, 0,
            0, -1, 0, 0,
            0, 0, -1, 0,
            0, 0, 0, -1);
        Assert.Equal(1, sign); // det = 1 (even number of negatives)
    }

    [Fact]
    public void ThreeNegatives_OnePositive_NegativeDet()
    {
        int sign = AdaptivePrecision.Det4x4Sign(
            -1, 0, 0, 0,
            0, -1, 0, 0,
            0, 0, -1, 0,
            0, 0, 0, 1);
        Assert.Equal(-1, sign); // det = -1 (odd number of negatives)
    }

    [Fact]
    public void LargeValues_CorrectSign()
    {
        double L = 1e10;
        int sign = AdaptivePrecision.Det4x4Sign(
            L, 0, 0, 0,
            0, L, 0, 0,
            0, 0, L, 0,
            0, 0, 0, L);
        Assert.Equal(1, sign);
    }

    [Fact]
    public void Det3x3Sign_Identity_IsPositive()
    {
        int sign = AdaptivePrecision.Det3x3Sign(1, 0, 0, 0, 1, 0, 0, 0, 1);
        Assert.Equal(1, sign);
    }

    [Fact]
    public void Det3x3Sign_SwapRows_FlipsSign()
    {
        int sign = AdaptivePrecision.Det3x3Sign(0, 1, 0, 1, 0, 0, 0, 0, 1);
        Assert.Equal(-1, sign);
    }

    [Fact]
    public void Det2x2Sign_Simple()
    {
        // |3 2; 1 4| = 12 - 2 = 10 > 0
        Assert.Equal(1, AdaptivePrecision.Det2x2Sign(3, 2, 1, 4));
    }

    [Fact]
    public void Det2x2Sign_NegativeDet()
    {
        // |1 4; 3 2| = 2 - 12 = -10 < 0
        Assert.Equal(-1, AdaptivePrecision.Det2x2Sign(1, 4, 3, 2));
    }

    [Fact]
    public void Det2x2Sign_Zero()
    {
        // |2 4; 1 2| = 4 - 4 = 0
        Assert.Equal(0, AdaptivePrecision.Det2x2Sign(2, 4, 1, 2));
    }

    [Fact]
    public void Det2x2Sign_NearlyZero_ForcesEscalation()
    {
        // Nearly cancelling products, but perturbation larger than machine epsilon
        double a = 1.0 + 1e-14;
        double b = 1.0;
        double c = 1.0;
        double d = 1.0 + 1e-14;
        // det = (1+eps)(1+eps) - 1*1 = 2eps + eps^2 > 0
        Assert.Equal(1, AdaptivePrecision.Det2x2Sign(a, b, c, d));
    }
}
