using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Phase 6: AdaptivePrecision determinant evaluation deep tests</summary>
public class AdaptivePrecisionDeepTests
{
    // --- Det2x2Sign ---

    [Fact]
    public void Det2x2_Positive()
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
        // |1 2; 2 4| = 0 (rows proportional)
        Assert.Equal(0, AdaptivePrecision.Det2x2Sign(1, 2, 2, 4));
    }

    [Fact]
    public void Det2x2_LargePositive()
    {
        // |1e15 0; 0 1| = 1e15
        Assert.Equal(1, AdaptivePrecision.Det2x2Sign(1e15, 0, 0, 1));
    }

    [Fact]
    public void Det2x2_NearCancellation()
    {
        // |1+e e; 1 1| = (1+e)*1 - e*1 = 1 (near cancellation)
        double e = 1e-14;
        Assert.Equal(1, AdaptivePrecision.Det2x2Sign(1 + e, e, 1, 1));
    }

    [Fact]
    public void Det2x2_ExactCancellation()
    {
        // |a a; b b| = 0 for any a, b
        Assert.Equal(0, AdaptivePrecision.Det2x2Sign(3.14, 3.14, 2.71, 2.71));
    }

    [Fact]
    public void Det2x2_NegativeValues()
    {
        // |-1 0; 0 -1| = 1
        Assert.Equal(1, AdaptivePrecision.Det2x2Sign(-1, 0, 0, -1));
    }

    [Fact]
    public void Det2x2_TriggersExpansion()
    {
        // Nearly cancelling products that need expansion arithmetic
        // |1+eps -1; 1 -(1+eps)| = -(1+eps)^2 - (-1) = -(1+eps)^2 + 1 = -2eps-eps^2 < 0
        double eps = 1e-10;
        int sign = AdaptivePrecision.Det2x2Sign(1 + eps, -1, 1, -(1 + eps));
        Assert.Equal(-1, sign);
    }

    // --- Det3x3Sign ---

    [Fact]
    public void Det3x3_Identity_Positive()
    {
        Assert.Equal(1, AdaptivePrecision.Det3x3Sign(
            1, 0, 0,
            0, 1, 0,
            0, 0, 1));
    }

    [Fact]
    public void Det3x3_SwapRows_FlipsSign()
    {
        int s1 = AdaptivePrecision.Det3x3Sign(
            1, 2, 3,
            4, 5, 6,
            7, 8, 10);
        int s2 = AdaptivePrecision.Det3x3Sign(
            4, 5, 6,
            1, 2, 3,
            7, 8, 10);
        Assert.Equal(-s1, s2);
    }

    [Fact]
    public void Det3x3_Singular_Zero()
    {
        // Third row = first + second
        Assert.Equal(0, AdaptivePrecision.Det3x3Sign(
            1, 0, 0,
            0, 1, 0,
            1, 1, 0));
    }

    [Fact]
    public void Det3x3_KnownValue()
    {
        // |1 2 3; 0 1 4; 5 6 0| = 1(0-24) - 2(0-20) + 3(0-5) = -24+40-15 = 1
        Assert.Equal(1, AdaptivePrecision.Det3x3Sign(
            1, 2, 3,
            0, 1, 4,
            5, 6, 0));
    }

    [Fact]
    public void Det3x3_AllNegative()
    {
        // |-1 -2 -3; -4 -5 -6; -7 -8 -10| = -(det of positive version)
        int sPos = AdaptivePrecision.Det3x3Sign(1, 2, 3, 4, 5, 6, 7, 8, 10);
        int sNeg = AdaptivePrecision.Det3x3Sign(-1, -2, -3, -4, -5, -6, -7, -8, -10);
        Assert.Equal(-sPos, sNeg);
    }

    [Fact]
    public void Det3x3_LargeCoordinates()
    {
        // Scaled identity should still be positive
        double s = 1e12;
        Assert.Equal(1, AdaptivePrecision.Det3x3Sign(s, 0, 0, 0, s, 0, 0, 0, s));
    }

    // --- Det4x4Sign ---

    [Fact]
    public void Det4x4_Identity_Positive()
    {
        Assert.Equal(1, AdaptivePrecision.Det4x4Sign(
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1));
    }

    [Fact]
    public void Det4x4_Zero_SingularMatrix()
    {
        // All zeros
        Assert.Equal(0, AdaptivePrecision.Det4x4Sign(
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0));
    }

    [Fact]
    public void Det4x4_ProportionalRows_Zero()
    {
        // Row 4 = Row 1
        Assert.Equal(0, AdaptivePrecision.Det4x4Sign(
            1, 2, 3, 4,
            0, 1, 0, 0,
            0, 0, 1, 0,
            1, 2, 3, 4));
    }

    [Fact]
    public void Det4x4_SwapRows_FlipsSign()
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
    public void Det4x4_ScaledIdentity()
    {
        double s = 2.0;
        // det(s*I) = s^4 = 16 > 0
        Assert.Equal(1, AdaptivePrecision.Det4x4Sign(
            s, 0, 0, 0,
            0, s, 0, 0,
            0, 0, s, 0,
            0, 0, 0, s));
    }

    // --- Rational ---

    [Fact]
    public void Rational_FromDouble_Zero()
    {
        var r = Rational.FromDouble(0);
        Assert.Equal(0, r.Sign);
    }

    [Fact]
    public void Rational_FromDouble_Positive()
    {
        var r = Rational.FromDouble(3.14);
        Assert.Equal(1, r.Sign);
    }

    [Fact]
    public void Rational_FromDouble_Negative()
    {
        var r = Rational.FromDouble(-2.5);
        Assert.Equal(-1, r.Sign);
    }

    [Fact]
    public void Rational_Add_Commutative()
    {
        var a = Rational.FromDouble(1.5);
        var b = Rational.FromDouble(2.7);
        var sum1 = a + b;
        var sum2 = b + a;
        Assert.Equal(sum1.Sign, sum2.Sign);
    }

    [Fact]
    public void Rational_Multiply_Signs()
    {
        var pos = Rational.FromDouble(3);
        var neg = Rational.FromDouble(-2);
        Assert.Equal(-1, (pos * neg).Sign);
        Assert.Equal(1, (neg * neg).Sign);
        Assert.Equal(1, (pos * pos).Sign);
    }

    [Fact]
    public void Rational_Subtract_ToZero()
    {
        var a = Rational.FromDouble(7.0);
        var b = Rational.FromDouble(7.0);
        Assert.Equal(0, (a - b).Sign);
    }

    [Fact]
    public void Rational_ExactArithmetic_NoCancellationLoss()
    {
        // 1e15 + 1 - 1e15 should be exactly 1
        var large = Rational.FromDouble(1e15);
        var one = Rational.FromDouble(1);
        var result = large + one - large;
        Assert.Equal(1, result.Sign);
    }
}
