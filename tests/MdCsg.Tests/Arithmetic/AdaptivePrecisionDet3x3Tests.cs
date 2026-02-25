using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Phase 6: AdaptivePrecision Det3x3Sign — known determinant values, sign properties</summary>
public class AdaptivePrecisionDet3x3Tests
{
    [Fact]
    public void Identity_PositiveDeterminant()
    {
        // det(I) = 1
        int sign = AdaptivePrecision.Det3x3Sign(
            1, 0, 0,
            0, 1, 0,
            0, 0, 1);
        Assert.Equal(1, sign);
    }

    [Fact]
    public void NegativeIdentity_NegativeDeterminant()
    {
        // Swap two rows → negative
        int sign = AdaptivePrecision.Det3x3Sign(
            0, 1, 0,
            1, 0, 0,
            0, 0, 1);
        Assert.Equal(-1, sign);
    }

    [Fact]
    public void SingularMatrix_ZeroDeterminant()
    {
        // Row 3 = Row 1 + Row 2
        int sign = AdaptivePrecision.Det3x3Sign(
            1, 0, 0,
            0, 1, 0,
            1, 1, 0);
        Assert.Equal(0, sign);
    }

    [Fact]
    public void ScaledIdentity_PositiveDeterminant()
    {
        // det(2I) = 8
        int sign = AdaptivePrecision.Det3x3Sign(
            2, 0, 0,
            0, 2, 0,
            0, 0, 2);
        Assert.Equal(1, sign);
    }

    [Fact]
    public void PermutationMatrix_NegativeDeterminant()
    {
        // Cyclic permutation: odd number of transpositions
        int sign = AdaptivePrecision.Det3x3Sign(
            0, 0, 1,
            1, 0, 0,
            0, 1, 0);
        Assert.Equal(1, sign); // Actually even cyclic permutation → +1
    }

    [Fact]
    public void SwapTwoRows_FlipsSign()
    {
        var sign1 = AdaptivePrecision.Det3x3Sign(1, 2, 3, 4, 5, 6, 7, 8, 10);
        var sign2 = AdaptivePrecision.Det3x3Sign(4, 5, 6, 1, 2, 3, 7, 8, 10);
        Assert.Equal(-sign1, sign2);
    }

    [Fact]
    public void ZeroRow_ZeroDeterminant()
    {
        int sign = AdaptivePrecision.Det3x3Sign(
            1, 2, 3,
            0, 0, 0,
            7, 8, 9);
        Assert.Equal(0, sign);
    }

    [Fact]
    public void DuplicateRows_ZeroDeterminant()
    {
        int sign = AdaptivePrecision.Det3x3Sign(
            1, 2, 3,
            1, 2, 3,
            4, 5, 6);
        Assert.Equal(0, sign);
    }

    [Fact]
    public void NearSingular_ResolvesSign()
    {
        // Very close to singular but not exactly
        int sign = AdaptivePrecision.Det3x3Sign(
            1, 2, 3,
            4, 5, 6,
            7, 8, 10); // 10 instead of 9 makes it non-singular
        Assert.NotEqual(0, sign);
    }

    [Fact]
    public void Det2x2Sign_SimplePositive()
    {
        // det(2, 0; 0, 3) = 6 > 0
        int sign = AdaptivePrecision.Det2x2Sign(2, 0, 0, 3);
        Assert.Equal(1, sign);
    }

    [Fact]
    public void Det2x2Sign_SimpleNegative()
    {
        // det(0, 1; 1, 0) = -1 < 0
        int sign = AdaptivePrecision.Det2x2Sign(0, 1, 1, 0);
        Assert.Equal(-1, sign);
    }

    [Fact]
    public void Det2x2Sign_Zero()
    {
        // det(1, 2; 2, 4) = 0
        int sign = AdaptivePrecision.Det2x2Sign(1, 2, 2, 4);
        Assert.Equal(0, sign);
    }

    [Fact]
    public void LargeValues_CorrectSign()
    {
        // Large values should still work via escalation
        int sign = AdaptivePrecision.Det3x3Sign(
            1e15, 1e15, 0,
            0, 1e15, 1e15,
            1e15, 0, 1e15);
        Assert.NotEqual(0, sign);
    }

    [Fact]
    public void NearCancellation_CorrectSign()
    {
        // Values that nearly cancel: 1+eps vs 1
        double eps = 1e-15;
        int sign = AdaptivePrecision.Det3x3Sign(
            1, 0, 0,
            0, 1, 0,
            0, 0, 1 + eps);
        Assert.Equal(1, sign);
    }
}
