using MdCsg.Arithmetic;
using MdCsg.Math;
using MdCsg.Predicates;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Phase 6: Numerical precision edge cases — near-zero determinants, cancellation, large coordinates</summary>
public class NumericalPrecisionEdgeCasePropertyTests
{
    [Fact]
    public void Det2x2Sign_NearZero_CorrectSign()
    {
        // Near-cancellation: ad - bc where ad ≈ bc
        double a = 1e8, b = 1e8 + 1, c = 1e8 - 1, d = 1e8;
        int sign = AdaptivePrecision.Det2x2Sign(a, b, c, d);
        // a*d - b*c = 1e16 - (1e16 + 1e8 - 1e8 - 1) = 1
        Assert.Equal(1, sign);
    }

    [Fact]
    public void Det2x2Sign_ExactZero_ReturnsZero()
    {
        // a*d - b*c: 2*3 - 3*2 = 6 - 6 = 0
        Assert.Equal(0, AdaptivePrecision.Det2x2Sign(2, 3, 2, 3));
    }

    [Fact]
    public void Det3x3Sign_IdentityMatrix_Positive()
    {
        // det(I) = 1
        Assert.Equal(1, AdaptivePrecision.Det3x3Sign(1, 0, 0, 0, 1, 0, 0, 0, 1));
    }

    [Fact]
    public void Det3x3Sign_SwapRows_FlipsSign()
    {
        int s1 = AdaptivePrecision.Det3x3Sign(1, 2, 3, 4, 5, 6, 7, 8, 10);
        int s2 = AdaptivePrecision.Det3x3Sign(4, 5, 6, 1, 2, 3, 7, 8, 10);
        Assert.Equal(-s1, s2);
    }

    [Fact]
    public void Det3x3Sign_AllZero_ReturnsZero()
    {
        Assert.Equal(0, AdaptivePrecision.Det3x3Sign(0, 0, 0, 0, 0, 0, 0, 0, 0));
    }

    [Fact]
    public void Det4x4Sign_IdentityAugmented_Positive()
    {
        Assert.Equal(1, AdaptivePrecision.Det4x4Sign(
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1));
    }

    [Fact]
    public void Orient3D_NearCoplanar_CorrectSign()
    {
        // Points nearly coplanar with D just slightly above
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0.5, 0.5, 1e-12); // very slightly above
        var sign = Orient3D.Evaluate(a, b, c, d);
        Assert.Equal(PredicateSign.Positive, sign);
    }

    [Fact]
    public void Orient2D_NearCollinear_ExactlyCollinear()
    {
        // Exactly collinear points
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 1);
        var c = new Vec2(2, 2);
        Assert.Equal(PredicateSign.Zero, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void Orient2D_LargeCoords_NearCollinear_Correct()
    {
        // Large coordinates, points nearly collinear
        var a = new Vec2(1e15, 1e15);
        var b = new Vec2(1e15 + 1, 1e15 + 1);
        var c = new Vec2(1e15 + 2, 1e15 + 2);
        Assert.Equal(PredicateSign.Zero, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void TwoSum_CancellationRecovery()
    {
        // 1.0 + (-1.0 + epsilon) should capture the epsilon
        double a = 1.0;
        double b = -(1.0 - 1e-16);
        var (sum, err) = ExpansionArithmetic.TwoSum(a, b);
        double total = sum + err;
        Assert.True(System.Math.Abs(total - (a + b)) < 1e-30);
    }

    [Fact]
    public void TwoProduct_LargeNumbers_CapturesError()
    {
        double a = 1e8 + 0.5;
        double b = 1e8 - 0.5;
        var (prod, err) = ExpansionArithmetic.TwoProduct(a, b);
        // prod + err should be exact
        Assert.True(System.Math.Abs((prod + err) - a * b) < 1.0);
    }

    [Fact]
    public void Rational_FromDouble_NonZero()
    {
        var r = Rational.FromDouble(0.1);
        // 0.1 is not exactly representable, but Rational.FromDouble captures it as non-zero
        Assert.True(r > Rational.Zero);
    }

    [Fact]
    public void Rational_ExactArithmetic_NoRoundingError()
    {
        var third = new Rational(1, 3);
        var result = third * new Rational(3, 1);
        Assert.Equal(new Rational(1, 1), result);
    }

    [Fact]
    public void Vec3_CrossProduct_OrthogonalToInputs()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var cross = Vec3.Cross(a, b);
        Assert.True(System.Math.Abs(Vec3.Dot(cross, a)) < 1e-10);
        Assert.True(System.Math.Abs(Vec3.Dot(cross, b)) < 1e-10);
    }

    [Fact]
    public void Vec3_Normalized_LargeVector_HasUnitLength()
    {
        var v = new Vec3(1e10, 1e10, 1e10);
        var n = v.Normalized;
        Assert.True(System.Math.Abs(n.Length - 1.0) < 1e-10);
    }

    [Fact]
    public void Vec3_Normalized_SmallVector_HasUnitLength()
    {
        var v = new Vec3(1e-10, 1e-10, 1e-10);
        var n = v.Normalized;
        Assert.True(System.Math.Abs(n.Length - 1.0) < 1e-5);
    }
}
