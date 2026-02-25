using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Phase 6: AdaptivePrecision Det2x2/Det3x3 — sign correctness, zero cases, near-zero, large values</summary>
public class AdaptiveDetSignTests
{
    [Fact]
    public void Det2x2_Identity_Positive()
    {
        // |1 0; 0 1| = 1
        Assert.Equal(1, AdaptivePrecision.Det2x2Sign(1, 0, 0, 1));
    }

    [Fact]
    public void Det2x2_Negative()
    {
        // |0 1; 1 0| = -1
        Assert.Equal(-1, AdaptivePrecision.Det2x2Sign(0, 1, 1, 0));
    }

    [Fact]
    public void Det2x2_Zero()
    {
        // |1 2; 2 4| = 4-4 = 0
        Assert.Equal(0, AdaptivePrecision.Det2x2Sign(1, 2, 2, 4));
    }

    [Fact]
    public void Det2x2_NearZeroPositive()
    {
        // Carefully constructed to be positive but small
        double a = 1.0, b = 1.0 - 1e-14, c = 1.0, d = 1.0;
        // ad - bc = 1 - (1 - 1e-14) = 1e-14 > 0
        Assert.Equal(1, AdaptivePrecision.Det2x2Sign(a, b, c, d));
    }

    [Fact]
    public void Det2x2_NearZeroNegative()
    {
        double a = 1.0, b = 1.0 + 1e-14, c = 1.0, d = 1.0;
        // ad - bc = 1 - (1 + 1e-14) = -1e-14 < 0
        Assert.Equal(-1, AdaptivePrecision.Det2x2Sign(a, b, c, d));
    }

    [Fact]
    public void Det2x2_LargeValues_CorrectSign()
    {
        // Large values that cancel: (1e8 + 1)(1e8) - (1e8)(1e8 + 1) = 0
        // But: (1e8 + 1)(1e8 + 2) - (1e8)(1e8 + 3)
        // = 1e16 + 3e8 + 2 - 1e16 - 3e8 = 2 > 0
        Assert.Equal(1, AdaptivePrecision.Det2x2Sign(1e8 + 1, 1e8, 1e8 + 3, 1e8 + 2));
    }

    [Fact]
    public void Det2x2_AllZeros()
    {
        Assert.Equal(0, AdaptivePrecision.Det2x2Sign(0, 0, 0, 0));
    }

    [Fact]
    public void Det3x3_Identity_Positive()
    {
        // Identity matrix det = 1
        Assert.Equal(1, AdaptivePrecision.Det3x3Sign(
            1, 0, 0,
            0, 1, 0,
            0, 0, 1));
    }

    [Fact]
    public void Det3x3_SwapTwoRows_Negative()
    {
        // Swap rows 1 and 2 of identity → det = -1
        Assert.Equal(-1, AdaptivePrecision.Det3x3Sign(
            0, 1, 0,
            1, 0, 0,
            0, 0, 1));
    }

    [Fact]
    public void Det3x3_Zero_DuplicateRows()
    {
        // Two identical rows → det = 0
        Assert.Equal(0, AdaptivePrecision.Det3x3Sign(
            1, 2, 3,
            1, 2, 3,
            4, 5, 6));
    }

    [Fact]
    public void Det3x3_Zero_ZeroRow()
    {
        Assert.Equal(0, AdaptivePrecision.Det3x3Sign(
            0, 0, 0,
            1, 2, 3,
            4, 5, 6));
    }

    [Fact]
    public void Det3x3_Known_Positive()
    {
        // |1 0 0; 0 1 0; 0 0 2| = 2
        Assert.Equal(1, AdaptivePrecision.Det3x3Sign(
            1, 0, 0,
            0, 1, 0,
            0, 0, 2));
    }

    [Fact]
    public void Det3x3_Known_Negative()
    {
        // |1 0 0; 0 1 0; 0 0 -1| = -1
        Assert.Equal(-1, AdaptivePrecision.Det3x3Sign(
            1, 0, 0,
            0, 1, 0,
            0, 0, -1));
    }

    [Fact]
    public void Det3x3_CofactorExpansion()
    {
        // |1 2 3; 4 5 6; 7 8 9| = 0 (linearly dependent rows)
        Assert.Equal(0, AdaptivePrecision.Det3x3Sign(
            1, 2, 3,
            4, 5, 6,
            7, 8, 9));
    }

    [Fact]
    public void Det3x3_NearZero_AdaptivePrecision()
    {
        // Construct nearly-singular matrix
        double e = 1e-15;
        // |1 0 0; 0 1 0; 0 0 e| = e > 0
        Assert.Equal(1, AdaptivePrecision.Det3x3Sign(
            1, 0, 0,
            0, 1, 0,
            0, 0, e));
    }

    [Fact]
    public void Det3x3_NearZero_Negative()
    {
        double e = 1e-15;
        Assert.Equal(-1, AdaptivePrecision.Det3x3Sign(
            1, 0, 0,
            0, 1, 0,
            0, 0, -e));
    }

    [Fact]
    public void Det3x3_AllZeros()
    {
        Assert.Equal(0, AdaptivePrecision.Det3x3Sign(
            0, 0, 0,
            0, 0, 0,
            0, 0, 0));
    }

    [Fact]
    public void Det2x2_NegativeValues()
    {
        // |-1 0; 0 -1| = 1
        Assert.Equal(1, AdaptivePrecision.Det2x2Sign(-1, 0, 0, -1));
    }

    [Fact]
    public void Det2x2_MixedSigns()
    {
        // |3 -2; 1 4| = 12 + 2 = 14 > 0
        Assert.Equal(1, AdaptivePrecision.Det2x2Sign(3, -2, 1, 4));
    }

    [Fact]
    public void Det3x3_PermutationMatrix_Negative()
    {
        // Cyclic permutation (even): det = 1
        Assert.Equal(1, AdaptivePrecision.Det3x3Sign(
            0, 0, 1,
            1, 0, 0,
            0, 1, 0));
    }

    [Fact]
    public void Det3x3_LargeValues_CorrectSign()
    {
        // Diagonal matrix with large values
        Assert.Equal(1, AdaptivePrecision.Det3x3Sign(
            1e10, 0, 0,
            0, 1e10, 0,
            0, 0, 1e10));
    }
}
