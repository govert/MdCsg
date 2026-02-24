using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

public class AdaptivePrecisionTests
{
    [Fact]
    public void Det2x2_Positive()
    {
        // | 2 1 |
        // | 1 3 | = 6 - 1 = 5
        Assert.Equal(1, AdaptivePrecision.Det2x2Sign(2, 1, 1, 3));
    }

    [Fact]
    public void Det2x2_Negative()
    {
        // | 1 3 |
        // | 2 1 | = 1 - 6 = -5
        Assert.Equal(-1, AdaptivePrecision.Det2x2Sign(1, 3, 2, 1));
    }

    [Fact]
    public void Det2x2_Zero()
    {
        // | 2 4 |
        // | 1 2 | = 4 - 4 = 0
        Assert.Equal(0, AdaptivePrecision.Det2x2Sign(2, 4, 1, 2));
    }

    [Fact]
    public void Det2x2_NearZero_Resolved()
    {
        // Create values where ad - bc is very close to zero but not zero
        // (1 + eps) * 1 - 1 * 1 = eps
        double eps = 1e-15;
        double a = 1.0 + eps;
        double b = 1.0;
        double c = 1.0;
        double d = 1.0;
        // ad - bc = (1+eps)*1 - 1*1 = eps > 0
        Assert.Equal(1, AdaptivePrecision.Det2x2Sign(a, b, c, d));
    }

    [Fact]
    public void Det3x3_Positive()
    {
        // Identity-like matrix
        // | 1 0 0 |
        // | 0 1 0 |
        // | 0 0 1 | = 1
        Assert.Equal(1, AdaptivePrecision.Det3x3Sign(1, 0, 0, 0, 1, 0, 0, 0, 1));
    }

    [Fact]
    public void Det3x3_Negative()
    {
        // Swapping two rows negates determinant
        // | 0 1 0 |
        // | 1 0 0 |
        // | 0 0 1 | = -1
        Assert.Equal(-1, AdaptivePrecision.Det3x3Sign(0, 1, 0, 1, 0, 0, 0, 0, 1));
    }

    [Fact]
    public void Det3x3_Zero_SingularMatrix()
    {
        // Linearly dependent rows
        // | 1 2 3 |
        // | 4 5 6 |
        // | 7 8 9 | = 0
        Assert.Equal(0, AdaptivePrecision.Det3x3Sign(1, 2, 3, 4, 5, 6, 7, 8, 9));
    }

    [Fact]
    public void Det4x4_Positive()
    {
        // 4x4 identity = 1
        Assert.Equal(1, AdaptivePrecision.Det4x4Sign(
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1));
    }

    [Fact]
    public void Det4x4_Zero_SingularMatrix()
    {
        // Row of zeros
        Assert.Equal(0, AdaptivePrecision.Det4x4Sign(
            0, 0, 0, 0,
            1, 2, 3, 4,
            5, 6, 7, 8,
            9, 10, 11, 12));
    }

    [Fact]
    public void Det2x2_LargeMagnitude()
    {
        // Large values: should still determine sign correctly
        double big = 1e15;
        // (big+1)*big - big*big = big
        Assert.Equal(1, AdaptivePrecision.Det2x2Sign(big + 1, big, big, big));
    }
}
