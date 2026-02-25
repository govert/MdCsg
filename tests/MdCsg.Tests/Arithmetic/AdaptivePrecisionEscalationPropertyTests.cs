using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Phase 6: AdaptivePrecision — Det2x2Sign, Det3x3Sign, Det4x4Sign escalation, exact results</summary>
public class AdaptivePrecisionEscalationPropertyTests
{
    [Fact]
    public void Det2x2Sign_SimplePositive()
    {
        // |2 1; 1 3| = 6 - 1 = 5 > 0
        Assert.Equal(1, AdaptivePrecision.Det2x2Sign(2, 1, 1, 3));
    }

    [Fact]
    public void Det2x2Sign_SimpleNegative()
    {
        // |1 3; 2 1| = 1 - 6 = -5 < 0
        Assert.Equal(-1, AdaptivePrecision.Det2x2Sign(1, 3, 2, 1));
    }

    [Fact]
    public void Det2x2Sign_Zero()
    {
        // |2 4; 1 2| = 4 - 4 = 0
        Assert.Equal(0, AdaptivePrecision.Det2x2Sign(2, 4, 1, 2));
    }

    [Fact]
    public void Det2x2Sign_NearlyCancelling_ExactResult()
    {
        // Values that nearly cancel in double but have a definite sign
        double a = 1e15 + 1;
        double b = 1e15;
        double c = 1e15;
        double d = 1e15 + 1;
        // ad - bc = (1e15+1)^2 - (1e15)^2 = 2e15 + 1 > 0
        Assert.Equal(1, AdaptivePrecision.Det2x2Sign(a, b, c, d));
    }

    [Fact]
    public void Det2x2Sign_Antisymmetric()
    {
        // Swapping rows negates the determinant
        double a = 3, b = 7, c = 11, d = 13;
        int sign1 = AdaptivePrecision.Det2x2Sign(a, b, c, d);
        int sign2 = AdaptivePrecision.Det2x2Sign(c, d, a, b);
        Assert.Equal(-sign1, sign2);
    }

    [Fact]
    public void Det2x2Sign_Identity_IsOne()
    {
        // |1 0; 0 1| = 1
        Assert.Equal(1, AdaptivePrecision.Det2x2Sign(1, 0, 0, 1));
    }

    [Fact]
    public void Det3x3Sign_SimplePositive()
    {
        // Identity matrix has determinant 1
        Assert.Equal(1, AdaptivePrecision.Det3x3Sign(1, 0, 0, 0, 1, 0, 0, 0, 1));
    }

    [Fact]
    public void Det3x3Sign_SimpleNegative()
    {
        // Swap two rows of identity: determinant = -1
        Assert.Equal(-1, AdaptivePrecision.Det3x3Sign(0, 1, 0, 1, 0, 0, 0, 0, 1));
    }

    [Fact]
    public void Det3x3Sign_Zero_SingularMatrix()
    {
        // Row 3 = Row 1 + Row 2 → singular
        Assert.Equal(0, AdaptivePrecision.Det3x3Sign(1, 0, 0, 0, 1, 0, 1, 1, 0));
    }

    [Fact]
    public void Det3x3Sign_RowSwap_FlipsSign()
    {
        double a = 2, b = 3, c = 5, d = 7, e = 11, f = 13, g = 17, h = 19, i = 23;
        int sign1 = AdaptivePrecision.Det3x3Sign(a, b, c, d, e, f, g, h, i);
        // Swap rows 1 and 2
        int sign2 = AdaptivePrecision.Det3x3Sign(d, e, f, a, b, c, g, h, i);
        Assert.Equal(-sign1, sign2);
    }

    [Fact]
    public void Det3x3Sign_ScaledRow_ScalesDet()
    {
        // Scaling a row by k multiplies determinant by k
        // So scaling by positive k preserves sign
        int sign1 = AdaptivePrecision.Det3x3Sign(1, 2, 3, 4, 5, 6, 7, 8, 10);
        int sign2 = AdaptivePrecision.Det3x3Sign(2, 4, 6, 4, 5, 6, 7, 8, 10);
        Assert.Equal(sign1, sign2);
    }

    [Fact]
    public void Det4x4Sign_Identity_IsOne()
    {
        Assert.Equal(1, AdaptivePrecision.Det4x4Sign(
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1));
    }

    [Fact]
    public void Det4x4Sign_Zero_SingularMatrix()
    {
        // Row 4 = Row 1: singular
        Assert.Equal(0, AdaptivePrecision.Det4x4Sign(
            1, 2, 3, 4,
            5, 6, 7, 8,
            9, 10, 11, 12,
            1, 2, 3, 4));
    }

    [Fact]
    public void Det4x4Sign_RowSwap_FlipsSign()
    {
        int sign1 = AdaptivePrecision.Det4x4Sign(
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1);
        // Swap rows 1 and 2
        int sign2 = AdaptivePrecision.Det4x4Sign(
            0, 1, 0, 0,
            1, 0, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1);
        Assert.Equal(-sign1, sign2);
    }

    [Fact]
    public void Det4x4Sign_KnownValue()
    {
        // 4x4 with known determinant = 1*1*1*1 = 1 (diagonal)
        int sign = AdaptivePrecision.Det4x4Sign(
            2, 0, 0, 0,
            0, 3, 0, 0,
            0, 0, 5, 0,
            0, 0, 0, 7);
        Assert.Equal(1, sign); // det = 2*3*5*7 = 210
    }

    [Fact]
    public void Det2x2Sign_AllZeros_IsZero()
    {
        Assert.Equal(0, AdaptivePrecision.Det2x2Sign(0, 0, 0, 0));
    }

    [Fact]
    public void Det3x3Sign_AllZeros_IsZero()
    {
        Assert.Equal(0, AdaptivePrecision.Det3x3Sign(0, 0, 0, 0, 0, 0, 0, 0, 0));
    }

    [Fact]
    public void Det4x4Sign_NegativeDiagonal()
    {
        int sign = AdaptivePrecision.Det4x4Sign(
            -1, 0, 0, 0,
            0, -1, 0, 0,
            0, 0, -1, 0,
            0, 0, 0, -1);
        Assert.Equal(1, sign); // (-1)^4 = 1
    }

    [Fact]
    public void Det4x4Sign_OddNegativeDiagonal()
    {
        int sign = AdaptivePrecision.Det4x4Sign(
            -1, 0, 0, 0,
            0, -1, 0, 0,
            0, 0, -1, 0,
            0, 0, 0, 1);
        Assert.Equal(-1, sign); // (-1)^3 * 1 = -1
    }
}
