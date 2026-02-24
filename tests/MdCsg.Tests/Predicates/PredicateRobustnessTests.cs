using MdCsg.Arithmetic;
using MdCsg.Math;
using MdCsg.Predicates;

namespace MdCsg.Tests.Predicates;

/// <summary>Phase 6: Predicate robustness with near-degenerate inputs</summary>
public class PredicateRobustnessTests
{
    [Fact]
    public void Orient3D_ExactlyCoplanar_ReturnsZero()
    {
        // Four coplanar points on XY plane
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(1, 1, 0);
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void Orient3D_SlightlyAbove_ReturnsPositive()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0.5, 0.5, 0.001);
        Assert.Equal(PredicateSign.Positive, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void Orient3D_SlightlyBelow_ReturnsNegative()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0.5, 0.5, -0.001);
        Assert.Equal(PredicateSign.Negative, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void Orient3D_VeryNearlyCoplanar_CorrectSign()
    {
        // Point barely above the plane → should detect positive
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0.5, 0.5, 1e-14);
        var result = Orient3D.Evaluate(a, b, c, d);
        Assert.Equal(PredicateSign.Positive, result);
    }

    [Fact]
    public void Orient3D_LargeCoordinates_StillExact()
    {
        // Large coordinates, exact coplanar
        var a = new Vec3(1e8, 1e8, 0);
        var b = new Vec3(1e8 + 1, 1e8, 0);
        var c = new Vec3(1e8, 1e8 + 1, 0);
        var d = new Vec3(1e8 + 0.5, 1e8 + 0.5, 0);
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void Orient2D_ExactlyCollinear_ReturnsZero()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 1);
        var c = new Vec2(2, 2);
        Assert.Equal(PredicateSign.Zero, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void Orient2D_CCW_ReturnsPositive()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);
        Assert.Equal(PredicateSign.Positive, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void Orient2D_CW_ReturnsNegative()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(0, 1);
        var c = new Vec2(1, 0);
        Assert.Equal(PredicateSign.Negative, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void Orient2D_NearlyCollinear_CorrectSign()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0.5, 1e-15);
        Assert.Equal(PredicateSign.Positive, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void Orient2D_LargeCoordinates_ExactCollinear()
    {
        var a = new Vec2(1e8, 1e8);
        var b = new Vec2(1e8 + 1, 1e8 + 1);
        var c = new Vec2(1e8 + 2, 1e8 + 2);
        Assert.Equal(PredicateSign.Zero, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void InCircle_OnCircle_ReturnsZero()
    {
        // Square's four corners lie on a circle
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(1, 1);
        var d = new Vec2(0, 1);
        Assert.Equal(PredicateSign.Zero, InCircle.Evaluate(a, b, c, d));
    }

    [Fact]
    public void InCircle_InsideCircle_ReturnsPositive()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);
        var d = new Vec2(0.2, 0.2); // inside circumcircle
        Assert.Equal(PredicateSign.Positive, InCircle.Evaluate(a, b, c, d));
    }

    [Fact]
    public void InCircle_OutsideCircle_ReturnsNegative()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);
        var d = new Vec2(5, 5); // far outside circumcircle
        Assert.Equal(PredicateSign.Negative, InCircle.Evaluate(a, b, c, d));
    }

    [Fact]
    public void PlaneClassification_Above_Positive()
    {
        var classification = PlaneClassification.Classify(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), new Vec3(0, 0, 1));
        Assert.Equal(PredicateSign.Positive, classification);
    }

    [Fact]
    public void PlaneClassification_Below_Negative()
    {
        var classification = PlaneClassification.Classify(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), new Vec3(0, 0, -1));
        Assert.Equal(PredicateSign.Negative, classification);
    }

    [Fact]
    public void PlaneClassification_OnPlane_Zero()
    {
        var classification = PlaneClassification.Classify(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), new Vec3(0.5, 0.5, 0));
        Assert.Equal(PredicateSign.Zero, classification);
    }

    [Fact]
    public void Det3x3Sign_ScaledIdentity_Positive()
    {
        // 2I has det = 8
        int sign = AdaptivePrecision.Det3x3Sign(
            2, 0, 0,
            0, 2, 0,
            0, 0, 2);
        Assert.Equal(1, sign);
    }

    [Fact]
    public void Det3x3Sign_PermutedRows_CyclesSign()
    {
        // Cyclic permutation preserves sign, transposition flips it
        int sign1 = AdaptivePrecision.Det3x3Sign(
            1, 2, 3,
            4, 5, 6,
            7, 8, 10);
        int sign2 = AdaptivePrecision.Det3x3Sign(
            4, 5, 6,
            7, 8, 10,
            1, 2, 3);
        Assert.Equal(sign1, sign2); // cyclic permutation preserves sign
    }
}
