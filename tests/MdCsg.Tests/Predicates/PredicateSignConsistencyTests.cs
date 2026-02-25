using MdCsg.Math;
using MdCsg.Predicates;

namespace MdCsg.Tests.Predicates;

/// <summary>Phase 6: Predicate sign consistency — Orient2D/3D, InCircle sign convention coherence, boundary behavior</summary>
public class PredicateSignConsistencyTests
{
    [Fact]
    public void Orient2D_CCW_Positive()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);
        Assert.Equal(PredicateSign.Positive, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void Orient2D_CW_Negative()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(0, 1);
        var c = new Vec2(1, 0);
        Assert.Equal(PredicateSign.Negative, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void Orient2D_Collinear_Zero()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 1);
        var c = new Vec2(2, 2);
        Assert.Equal(PredicateSign.Zero, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void Orient2D_SwapPair_FlipsSign()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);
        var s1 = Orient2D.Evaluate(a, b, c);
        var s2 = Orient2D.Evaluate(b, a, c);
        Assert.NotEqual(PredicateSign.Zero, s1);
        Assert.NotEqual(s1, s2);
    }

    [Fact]
    public void Orient2D_CyclicPermutation_SameSign()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);
        var s1 = Orient2D.Evaluate(a, b, c);
        var s2 = Orient2D.Evaluate(b, c, a);
        var s3 = Orient2D.Evaluate(c, a, b);
        Assert.Equal(s1, s2);
        Assert.Equal(s2, s3);
    }

    [Fact]
    public void Orient3D_Above_Positive()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0, 0, 1); // Above the ABC plane
        Assert.Equal(PredicateSign.Positive, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void Orient3D_Below_Negative()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0, 0, -1); // Below the ABC plane
        Assert.Equal(PredicateSign.Negative, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void Orient3D_Coplanar_Zero()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(1, 1, 0); // On the same plane
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void Orient3D_SwapFirstTwoVertices_FlipsSign()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0, 0, 1);
        var s1 = Orient3D.Evaluate(a, b, c, d);
        var s2 = Orient3D.Evaluate(b, a, c, d);
        Assert.NotEqual(PredicateSign.Zero, s1);
        Assert.NotEqual(s1, s2);
    }

    [Fact]
    public void Orient3D_EvenPermutation_SameSign()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0, 0, 1);
        var s1 = Orient3D.Evaluate(a, b, c, d);
        var s2 = Orient3D.Evaluate(c, d, a, b); // Even permutation (swap two pairs)
        Assert.Equal(s1, s2);
    }

    [Fact]
    public void Orient3D_DuplicateVertex_Zero()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(a, b, c, a));
    }

    [Fact]
    public void InCircle_CCW_InsidePoint_Positive()
    {
        // CCW triangle
        var a = new Vec2(-1, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);
        var inside = new Vec2(0, 0); // Inside circumcircle
        Assert.Equal(PredicateSign.Positive, InCircle.Evaluate(a, b, c, inside));
    }

    [Fact]
    public void InCircle_CCW_OutsidePoint_Negative()
    {
        var a = new Vec2(-1, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);
        var outside = new Vec2(0, -5); // Far below, outside circumcircle
        Assert.Equal(PredicateSign.Negative, InCircle.Evaluate(a, b, c, outside));
    }

    [Fact]
    public void InCircle_OnCircle_Zero()
    {
        // Unit circle: all 4 points on circle
        double r = 1.0;
        var a = new Vec2(-r, 0);
        var b = new Vec2(r, 0);
        var c = new Vec2(0, r);
        var d = new Vec2(0, -r);
        Assert.Equal(PredicateSign.Zero, InCircle.Evaluate(a, b, c, d));
    }

    [Fact]
    public void Orient2D_LargeCoordinates_StillCorrect()
    {
        double s = 1e8;
        var a = new Vec2(s, s);
        var b = new Vec2(s + 1, s);
        var c = new Vec2(s, s + 1);
        Assert.Equal(PredicateSign.Positive, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void Orient3D_LargeCoordinates_StillCorrect()
    {
        double s = 1e8;
        var a = new Vec3(s, s, s);
        var b = new Vec3(s + 1, s, s);
        var c = new Vec3(s, s + 1, s);
        var d = new Vec3(s, s, s + 1);
        Assert.Equal(PredicateSign.Positive, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void Orient2D_NearlyCollinear_Detects()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0.5, 1e-15);
        // Nearly collinear — predicate should still give a sign
        var sign = Orient2D.Evaluate(a, b, c);
        // The exact sign depends on adaptive precision, but should be deterministic
        Assert.True(sign == PredicateSign.Positive || sign == PredicateSign.Zero);
    }

    [Fact]
    public void Orient3D_NearlyCoplanar_Detects()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0, 0, 1e-15);
        var sign = Orient3D.Evaluate(a, b, c, d);
        Assert.True(sign == PredicateSign.Positive || sign == PredicateSign.Zero);
    }
}
