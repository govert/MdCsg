using MdCsg.Arithmetic;

namespace MdCsg.Tests.Predicates;

/// <summary>Phase 6: AdaptivePrecision — Det2x2Sign, Det3x3Sign, Det4x4Sign with various inputs</summary>
public class AdaptivePrecisionPropertyTests
{
    [Fact]
    public void Det2x2Sign_Positive()
    {
        // 2*4 - 1*3 = 5 > 0
        Assert.Equal(1, AdaptivePrecision.Det2x2Sign(2, 1, 3, 4));
    }

    [Fact]
    public void Det2x2Sign_Negative()
    {
        // 1*3 - 2*4 = -5 < 0
        Assert.Equal(-1, AdaptivePrecision.Det2x2Sign(1, 2, 4, 3));
    }

    [Fact]
    public void Det2x2Sign_Zero()
    {
        Assert.Equal(0, AdaptivePrecision.Det2x2Sign(2, 3, 2, 3));
    }

    [Fact]
    public void Det2x2Sign_LargeValues_NearCancellation()
    {
        // a*d - b*c = 1e16 - (1e16 - 1) = 1
        double a = 1e8, b = 1e8 + 1, c = 1e8 - 1, d = 1e8;
        Assert.Equal(1, AdaptivePrecision.Det2x2Sign(a, b, c, d));
    }

    [Fact]
    public void Det3x3Sign_Identity_Positive()
    {
        Assert.Equal(1, AdaptivePrecision.Det3x3Sign(1, 0, 0, 0, 1, 0, 0, 0, 1));
    }

    [Fact]
    public void Det3x3Sign_AllZero_Zero()
    {
        Assert.Equal(0, AdaptivePrecision.Det3x3Sign(0, 0, 0, 0, 0, 0, 0, 0, 0));
    }

    [Fact]
    public void Det3x3Sign_SwapRows_FlipsSign()
    {
        int s1 = AdaptivePrecision.Det3x3Sign(1, 2, 3, 4, 5, 6, 7, 8, 10);
        int s2 = AdaptivePrecision.Det3x3Sign(4, 5, 6, 1, 2, 3, 7, 8, 10);
        Assert.Equal(-s1, s2);
    }

    [Fact]
    public void Det3x3Sign_SingularMatrix_Zero()
    {
        // Rows are linearly dependent: row3 = row1 + row2
        Assert.Equal(0, AdaptivePrecision.Det3x3Sign(1, 0, 0, 0, 1, 0, 1, 1, 0));
    }

    [Fact]
    public void Det4x4Sign_Identity_Positive()
    {
        Assert.Equal(1, AdaptivePrecision.Det4x4Sign(
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1));
    }

    [Fact]
    public void Det4x4Sign_AllZero_Zero()
    {
        Assert.Equal(0, AdaptivePrecision.Det4x4Sign(
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0));
    }

    [Fact]
    public void Det4x4Sign_Scaled_SameSign()
    {
        // Scaling by positive constant doesn't change sign
        int s1 = AdaptivePrecision.Det4x4Sign(
            1, 2, 3, 4,
            5, 6, 7, 8,
            9, 10, 12, 13,
            14, 15, 16, 18);
        int s2 = AdaptivePrecision.Det4x4Sign(
            2, 4, 6, 8,
            5, 6, 7, 8,
            9, 10, 12, 13,
            14, 15, 16, 18);
        // Row 1 doubled: det is doubled, sign preserved
        Assert.Equal(s1, s2);
    }

    [Fact]
    public void Det2x2Sign_Symmetric_Deterministic()
    {
        // Same inputs produce same result every time
        int s1 = AdaptivePrecision.Det2x2Sign(1.23456789, 9.87654321, 3.14159265, 2.71828182);
        int s2 = AdaptivePrecision.Det2x2Sign(1.23456789, 9.87654321, 3.14159265, 2.71828182);
        Assert.Equal(s1, s2);
    }

    [Fact]
    public void Det3x3Sign_Diagonal_Positive()
    {
        // Diagonal matrix with positive entries: det = product of diagonal
        Assert.Equal(1, AdaptivePrecision.Det3x3Sign(2, 0, 0, 0, 3, 0, 0, 0, 5));
    }

    [Fact]
    public void Det3x3Sign_Diagonal_Negative()
    {
        // One negative diagonal entry: det = -30
        Assert.Equal(-1, AdaptivePrecision.Det3x3Sign(-2, 0, 0, 0, 3, 0, 0, 0, 5));
    }
}
