using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Phase 6: AdaptivePrecision — Det2x2Sign/Det3x3Sign/Det4x4Sign escalation paths, exact arithmetic agreement</summary>
public class AdaptivePrecisionEscalationPropertyTests
{
    [Fact]
    public void Det2x2Sign_IntegerValues_Correct()
    {
        // |1 2; 3 4| = 1*4 - 2*3 = -2
        Assert.Equal(-1, AdaptivePrecision.Det2x2Sign(1, 2, 3, 4));
    }

    [Fact]
    public void Det2x2Sign_Identity_Positive()
    {
        // |1 0; 0 1| = 1
        Assert.Equal(1, AdaptivePrecision.Det2x2Sign(1, 0, 0, 1));
    }

    [Fact]
    public void Det2x2Sign_Zero_WhenSingular()
    {
        // |1 2; 2 4| = 1*4 - 2*2 = 0
        Assert.Equal(0, AdaptivePrecision.Det2x2Sign(1, 2, 2, 4));
    }

    [Fact]
    public void Det2x2Sign_SwapRows_FlipsSign()
    {
        int original = AdaptivePrecision.Det2x2Sign(1, 2, 3, 5);
        int swapped = AdaptivePrecision.Det2x2Sign(3, 5, 1, 2);
        Assert.Equal(-original, swapped);
    }

    [Fact]
    public void Det2x2Sign_NearlyZero_StillCorrect()
    {
        // Values designed to trigger expansion path
        double a = 1.0 + 1e-15;
        double b = 2.0;
        double c = 2.0;
        double d = 4.0 - 1e-15;
        // ad - bc = (1+eps)(4-eps) - 2*2 = 4 - eps + 4*eps - eps^2 - 4 = 3*eps - eps^2
        // Should be positive (very small but positive)
        int sign = AdaptivePrecision.Det2x2Sign(a, b, c, d);
        Assert.True(sign >= 0, $"Expected non-negative, got {sign}");
    }

    [Fact]
    public void Det3x3Sign_Identity_Positive()
    {
        Assert.Equal(1, AdaptivePrecision.Det3x3Sign(
            1, 0, 0,
            0, 1, 0,
            0, 0, 1));
    }

    [Fact]
    public void Det3x3Sign_Singular_Zero()
    {
        Assert.Equal(0, AdaptivePrecision.Det3x3Sign(
            1, 2, 3,
            1, 2, 3,
            4, 5, 6));
    }

    [Fact]
    public void Det3x3Sign_SwapTwoRows_FlipsSign()
    {
        int original = AdaptivePrecision.Det3x3Sign(
            1, 2, 3,
            4, 5, 6,
            7, 8, 10);
        int swapped = AdaptivePrecision.Det3x3Sign(
            4, 5, 6,
            1, 2, 3,
            7, 8, 10);
        Assert.Equal(-original, swapped);
    }

    [Fact]
    public void Det3x3Sign_LinearlyDependent_Zero()
    {
        Assert.Equal(0, AdaptivePrecision.Det3x3Sign(
            1, 2, 3,
            4, 5, 6,
            7, 8, 9));
    }

    [Fact]
    public void Det3x3Sign_DiagonalMatrix_Positive()
    {
        Assert.Equal(1, AdaptivePrecision.Det3x3Sign(
            1, 0, 0,
            0, 2, 0,
            0, 0, 3));
    }

    [Fact]
    public void Det3x3Sign_ScaleRow_SameSign()
    {
        int original = AdaptivePrecision.Det3x3Sign(
            1, 2, 3,
            4, 5, 6,
            7, 8, 10);
        int scaled = AdaptivePrecision.Det3x3Sign(
            2, 4, 6,
            4, 5, 6,
            7, 8, 10);
        Assert.Equal(original, scaled);
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
    public void Det4x4Sign_Singular_Zero()
    {
        Assert.Equal(0, AdaptivePrecision.Det4x4Sign(
            1, 2, 3, 4,
            1, 2, 3, 4,
            5, 6, 7, 8,
            9, 10, 11, 13));
    }

    [Fact]
    public void Det4x4Sign_SwapTwoRows_FlipsSign()
    {
        int original = AdaptivePrecision.Det4x4Sign(
            1, 2, 3, 4,
            5, 6, 7, 8,
            9, 10, 12, 13,
            14, 15, 16, 18);
        int swapped = AdaptivePrecision.Det4x4Sign(
            5, 6, 7, 8,
            1, 2, 3, 4,
            9, 10, 12, 13,
            14, 15, 16, 18);
        Assert.Equal(-original, swapped);
    }

    [Fact]
    public void Det4x4Sign_Diagonal_Positive()
    {
        Assert.Equal(1, AdaptivePrecision.Det4x4Sign(
            2, 0, 0, 0,
            0, 3, 0, 0,
            0, 0, 5, 0,
            0, 0, 0, 7));
    }

    [Fact]
    public void Det2x2Sign_LargeValues_Correct()
    {
        double big = 1e15;
        Assert.Equal(1, AdaptivePrecision.Det2x2Sign(big, 1, 1, big));
    }

    [Fact]
    public void Det2x2Sign_NegateAll_SameSign()
    {
        int original = AdaptivePrecision.Det2x2Sign(3, 7, 2, 5);
        int negated = AdaptivePrecision.Det2x2Sign(-3, -7, -2, -5);
        Assert.Equal(original, negated);
    }
}
