using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Phase 6: AdaptivePrecision — Det2x2Sign, Det3x3Sign, Det4x4Sign correctness and escalation</summary>
public class AdaptivePrecisionDeterminantPropertyTests
{
    // ——— Det2x2Sign ———

    [Fact]
    public void Det2x2Sign_Identity_Positive()
    {
        Assert.Equal(1, AdaptivePrecision.Det2x2Sign(1, 0, 0, 1));
    }

    [Fact]
    public void Det2x2Sign_Zero_Singular()
    {
        Assert.Equal(0, AdaptivePrecision.Det2x2Sign(1, 2, 2, 4));
    }

    [Fact]
    public void Det2x2Sign_Negative()
    {
        // |0 1; 1 0| = -1
        Assert.Equal(-1, AdaptivePrecision.Det2x2Sign(0, 1, 1, 0));
    }

    [Fact]
    public void Det2x2Sign_SwapColumns_FlipsSign()
    {
        int s1 = AdaptivePrecision.Det2x2Sign(3, 5, 7, 11);
        int s2 = AdaptivePrecision.Det2x2Sign(5, 3, 11, 7);
        Assert.Equal(-s1, s2);
    }

    [Fact]
    public void Det2x2Sign_SwapRows_FlipsSign()
    {
        int s1 = AdaptivePrecision.Det2x2Sign(3, 5, 7, 11);
        int s2 = AdaptivePrecision.Det2x2Sign(7, 11, 3, 5);
        Assert.Equal(-s1, s2);
    }

    [Fact]
    public void Det2x2Sign_ScaleRow_PreservesSign()
    {
        int s1 = AdaptivePrecision.Det2x2Sign(3, 5, 7, 11);
        int s2 = AdaptivePrecision.Det2x2Sign(6, 10, 7, 11); // doubled first row
        Assert.Equal(s1, s2);
    }

    [Fact]
    public void Det2x2Sign_NearZero_CorrectSign()
    {
        // ad - bc where ad ≈ bc but not quite
        double a = 1.0 + 1e-15;
        double d = 1.0;
        double b = 1.0;
        double c = 1.0;
        // det = (1+1e-15)*1 - 1*1 = 1e-15 > 0
        Assert.Equal(1, AdaptivePrecision.Det2x2Sign(a, b, c, d));
    }

    [Fact]
    public void Det2x2Sign_LargeValues_CorrectSign()
    {
        double big = 1e10;
        // det = big*(big+1) - big*big = big
        Assert.Equal(1, AdaptivePrecision.Det2x2Sign(big, big, big, big + 1));
    }

    // ——— Det3x3Sign ———

    [Fact]
    public void Det3x3Sign_Identity_Positive()
    {
        Assert.Equal(1, AdaptivePrecision.Det3x3Sign(1, 0, 0, 0, 1, 0, 0, 0, 1));
    }

    [Fact]
    public void Det3x3Sign_ZeroMatrix_Zero()
    {
        Assert.Equal(0, AdaptivePrecision.Det3x3Sign(0, 0, 0, 0, 0, 0, 0, 0, 0));
    }

    [Fact]
    public void Det3x3Sign_DuplicateRows_Zero()
    {
        Assert.Equal(0, AdaptivePrecision.Det3x3Sign(1, 2, 3, 1, 2, 3, 4, 5, 6));
    }

    [Fact]
    public void Det3x3Sign_SwapRows_FlipsSign()
    {
        int s1 = AdaptivePrecision.Det3x3Sign(1, 2, 3, 4, 5, 6, 7, 8, 10);
        int s2 = AdaptivePrecision.Det3x3Sign(4, 5, 6, 1, 2, 3, 7, 8, 10);
        Assert.Equal(-s1, s2);
    }

    [Fact]
    public void Det3x3Sign_CyclicRows_SameSign()
    {
        int s1 = AdaptivePrecision.Det3x3Sign(1, 2, 3, 4, 5, 6, 7, 8, 10);
        int s2 = AdaptivePrecision.Det3x3Sign(4, 5, 6, 7, 8, 10, 1, 2, 3);
        Assert.Equal(s1, s2);
    }

    [Fact]
    public void Det3x3Sign_Diagonal_Positive()
    {
        // diag(2,3,5) = 30 > 0
        Assert.Equal(1, AdaptivePrecision.Det3x3Sign(2, 0, 0, 0, 3, 0, 0, 0, 5));
    }

    [Fact]
    public void Det3x3Sign_Diagonal_Negative()
    {
        // diag(-1,1,1) = -1
        Assert.Equal(-1, AdaptivePrecision.Det3x3Sign(-1, 0, 0, 0, 1, 0, 0, 0, 1));
    }

    [Fact]
    public void Det3x3Sign_KnownValue()
    {
        // |1 2 3; 4 5 6; 7 8 10| = 1*(50-48) - 2*(40-42) + 3*(32-35) = 2 + 4 - 9 = -3
        Assert.Equal(-1, AdaptivePrecision.Det3x3Sign(1, 2, 3, 4, 5, 6, 7, 8, 10));
    }

    // ——— Det4x4Sign ———

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
    public void Det4x4Sign_ZeroMatrix_Zero()
    {
        Assert.Equal(0, AdaptivePrecision.Det4x4Sign(
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0));
    }

    [Fact]
    public void Det4x4Sign_DuplicateRows_Zero()
    {
        Assert.Equal(0, AdaptivePrecision.Det4x4Sign(
            1, 2, 3, 4,
            1, 2, 3, 4,
            5, 6, 7, 8,
            9, 10, 11, 13));
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
    public void Det4x4Sign_SwapRows_FlipsSign()
    {
        int s1 = AdaptivePrecision.Det4x4Sign(
            1, 2, 3, 4,
            5, 6, 7, 8,
            9, 10, 12, 13,
            14, 15, 16, 18);
        int s2 = AdaptivePrecision.Det4x4Sign(
            5, 6, 7, 8,
            1, 2, 3, 4,
            9, 10, 12, 13,
            14, 15, 16, 18);
        Assert.Equal(-s1, s2);
    }

    [Fact]
    public void Det4x4Sign_NearSingular_Escalates()
    {
        // Construct a nearly-singular 4x4 matrix
        double eps = 1e-14;
        int sign = AdaptivePrecision.Det4x4Sign(
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, eps);
        Assert.Equal(1, sign); // det = eps > 0
    }

    [Fact]
    public void Det2x2Sign_AllZeros_Zero()
    {
        Assert.Equal(0, AdaptivePrecision.Det2x2Sign(0, 0, 0, 0));
    }

    [Fact]
    public void Det3x3Sign_AntiDiagonal_Sign()
    {
        // Anti-diagonal: |0 0 1; 0 1 0; 1 0 0| = -1
        Assert.Equal(-1, AdaptivePrecision.Det3x3Sign(0, 0, 1, 0, 1, 0, 1, 0, 0));
    }
}
