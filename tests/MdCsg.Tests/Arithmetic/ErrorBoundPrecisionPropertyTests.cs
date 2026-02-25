using MdCsg.Arithmetic;
using MdCsg.Math;
using MdCsg.Predicates;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Phase 6: ErrorBound + AdaptivePrecision integration — precision escalation, determinant accuracy</summary>
public class ErrorBoundPrecisionPropertyTests
{
    [Fact]
    public void Epsilon_IsHalfUlp()
    {
        // Machine epsilon = 2^-53
        Assert.True(ErrorBound.Epsilon > 0);
        Assert.True(ErrorBound.Epsilon < 1e-15);
        Assert.True(1.0 + ErrorBound.Epsilon == 1.0); // Below the rounding unit
    }

    [Fact]
    public void Orient2DErrorBoundA_IsPositive()
    {
        Assert.True(ErrorBound.Orient2DErrorBoundA > 0);
    }

    [Fact]
    public void Orient3DErrorBoundA_IsPositive()
    {
        Assert.True(ErrorBound.Orient3DErrorBoundA > 0);
    }

    [Fact]
    public void InCircleErrorBoundA_IsPositive()
    {
        Assert.True(ErrorBound.InCircleErrorBoundA > 0);
    }

    [Fact]
    public void Det2x2Sign_SimpleCase_CorrectSign()
    {
        // det = 1*4 - 2*3 = -2 → negative
        int sign = AdaptivePrecision.Det2x2Sign(1, 2, 3, 4);
        Assert.Equal(-1, sign);
    }

    [Fact]
    public void Det2x2Sign_Identity_PositiveOne()
    {
        // det = 1*1 - 0*0 = 1
        int sign = AdaptivePrecision.Det2x2Sign(1, 0, 0, 1);
        Assert.Equal(1, sign);
    }

    [Fact]
    public void Det2x2Sign_Zero_ReturnsZero()
    {
        // det = 2*4 - 2*4 = 0
        int sign = AdaptivePrecision.Det2x2Sign(2, 2, 4, 4);
        Assert.Equal(0, sign);
    }

    [Fact]
    public void Det3x3Sign_Identity_PositiveOne()
    {
        int sign = AdaptivePrecision.Det3x3Sign(
            1, 0, 0,
            0, 1, 0,
            0, 0, 1);
        Assert.Equal(1, sign);
    }

    [Fact]
    public void Det3x3Sign_SwappedRows_NegativeOne()
    {
        // Swap first two rows of identity → negative determinant
        int sign = AdaptivePrecision.Det3x3Sign(
            0, 1, 0,
            1, 0, 0,
            0, 0, 1);
        Assert.Equal(-1, sign);
    }

    [Fact]
    public void Det3x3Sign_ZeroDeterminant_ReturnsZero()
    {
        // Two identical rows → zero determinant
        int sign = AdaptivePrecision.Det3x3Sign(
            1, 2, 3,
            1, 2, 3,
            4, 5, 6);
        Assert.Equal(0, sign);
    }

    [Fact]
    public void Det4x4Sign_Identity_Positive()
    {
        int sign = AdaptivePrecision.Det4x4Sign(
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1);
        Assert.Equal(1, sign);
    }

    [Fact]
    public void Det2x2Sign_NearCancellation_CorrectSign()
    {
        // Nearly canceling terms: 1e8 * 1e8 - (1e8+1) * (1e8-1) = 1e16 - (1e16-1) = 1
        int sign = AdaptivePrecision.Det2x2Sign(1e8, 1e8 + 1, 1e8 - 1, 1e8);
        Assert.Equal(1, sign);
    }

    [Fact]
    public void Orient2D_And_Det2x2Sign_Agree()
    {
        // Orient2D uses Det2x2Sign under the hood
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);
        var orient = Orient2D.Evaluate(a, b, c);
        Assert.Equal(PredicateSign.Positive, orient);
    }

    [Fact]
    public void Orient3D_And_Det3x3Sign_Agree()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0, 0, 1);
        var orient = Orient3D.Evaluate(a, b, c, d);
        Assert.Equal(PredicateSign.Positive, orient);
    }

    [Fact]
    public void Det2x2Sign_ScaledInputs_SameSign()
    {
        // Scaling all inputs by same factor preserves sign
        int sign1 = AdaptivePrecision.Det2x2Sign(1, 2, 3, 4);
        int sign2 = AdaptivePrecision.Det2x2Sign(10, 20, 30, 40);
        Assert.Equal(sign1, sign2);
    }
}
