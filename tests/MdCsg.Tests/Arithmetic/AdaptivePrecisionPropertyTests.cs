using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Phase 6: AdaptivePrecision determinant sign tests — 2x2, 3x3, 4x4 sign computation</summary>
public class AdaptivePrecisionPropertyTests
{
    [Fact]
    public void Det2x2Sign_Identity()
    {
        // det [[1,0],[0,1]] = 1
        int sign = AdaptivePrecision.Det2x2Sign(1, 0, 0, 1);
        Assert.Equal(1, sign);
    }

    [Fact]
    public void Det2x2Sign_Zero()
    {
        // det [[1,2],[2,4]] = 0
        int sign = AdaptivePrecision.Det2x2Sign(1, 2, 2, 4);
        Assert.Equal(0, sign);
    }

    [Fact]
    public void Det2x2Sign_Negative()
    {
        // det [[0,1],[1,0]] = -1
        int sign = AdaptivePrecision.Det2x2Sign(0, 1, 1, 0);
        Assert.Equal(-1, sign);
    }

    [Fact]
    public void Det3x3Sign_Identity()
    {
        // 3x3 identity matrix, det = 1
        int sign = AdaptivePrecision.Det3x3Sign(
            1, 0, 0,
            0, 1, 0,
            0, 0, 1);
        Assert.Equal(1, sign);
    }

    [Fact]
    public void Det3x3Sign_Zero()
    {
        // Singular matrix (rows linearly dependent)
        int sign = AdaptivePrecision.Det3x3Sign(
            1, 2, 3,
            4, 5, 6,
            7, 8, 9);
        Assert.Equal(0, sign);
    }

    [Fact]
    public void Det3x3Sign_Negative()
    {
        // Swap two rows of identity
        int sign = AdaptivePrecision.Det3x3Sign(
            0, 1, 0,
            1, 0, 0,
            0, 0, 1);
        Assert.Equal(-1, sign);
    }

    [Fact]
    public void Det4x4Sign_Identity()
    {
        int sign = AdaptivePrecision.Det4x4Sign(
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1);
        Assert.Equal(1, sign);
    }

    [Fact]
    public void Det4x4Sign_Zero()
    {
        // Singular 4x4 (duplicate rows)
        int sign = AdaptivePrecision.Det4x4Sign(
            1, 2, 3, 4,
            5, 6, 7, 8,
            1, 2, 3, 4,
            9, 10, 11, 12);
        Assert.Equal(0, sign);
    }

    [Fact]
    public void Det4x4Sign_Negative()
    {
        // Swap first two rows of identity
        int sign = AdaptivePrecision.Det4x4Sign(
            0, 1, 0, 0,
            1, 0, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1);
        Assert.Equal(-1, sign);
    }

    [Fact]
    public void Det2x2Sign_NearlyZero_CorrectSign()
    {
        // Very nearly zero determinant
        double eps = 1e-15;
        int sign = AdaptivePrecision.Det2x2Sign(1, 1 + eps, 1, 1 + 2 * eps);
        // det = 1*(1+2eps) - (1+eps)*1 = eps > 0
        Assert.Equal(1, sign);
    }

    [Fact]
    public void Det3x3Sign_NearlyZero_CorrectSign()
    {
        double eps = 1e-15;
        int sign = AdaptivePrecision.Det3x3Sign(
            1, 0, 0,
            0, 1, 0,
            0, 0, eps);
        // det = eps > 0
        Assert.Equal(1, sign);
    }

    [Fact]
    public void Det2x2Sign_LargeValues()
    {
        int sign = AdaptivePrecision.Det2x2Sign(1e15, 2e15, 3e15, 7e15);
        // det = 1e15*7e15 - 2e15*3e15 = 7e30 - 6e30 = 1e30 > 0
        Assert.Equal(1, sign);
    }

    [Fact]
    public void Det3x3Sign_SwapTwoRows_FlipsSign()
    {
        // Standard matrix
        int s1 = AdaptivePrecision.Det3x3Sign(
            1, 2, 3,
            4, 5, 6,
            7, 8, 10);
        // Swap rows 1 and 2
        int s2 = AdaptivePrecision.Det3x3Sign(
            4, 5, 6,
            1, 2, 3,
            7, 8, 10);
        Assert.Equal(-s1, s2);
    }
}
