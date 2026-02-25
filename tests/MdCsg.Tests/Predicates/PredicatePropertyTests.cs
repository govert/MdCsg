using MdCsg.Math;
using MdCsg.Predicates;

namespace MdCsg.Tests.Predicates;

/// <summary>Phase 6: Orient2D, Orient3D, InCircle, PlaneClassification predicates</summary>
public class PredicatePropertyTests
{
    // --- Orient2D ---

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
        var b = new Vec2(1, 0);
        var c = new Vec2(2, 0);
        Assert.Equal(PredicateSign.Zero, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void Orient2D_SwapAB_FlipsSign()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0.5, 1);
        var s1 = Orient2D.Evaluate(a, b, c);
        var s2 = Orient2D.Evaluate(b, a, c);
        Assert.Equal(-(int)s1, (int)s2);
    }

    [Fact]
    public void Orient2D_IdenticalPoints_Zero()
    {
        var p = new Vec2(5, 3);
        Assert.Equal(PredicateSign.Zero, Orient2D.Evaluate(p, p, p));
    }

    [Fact]
    public void Orient2D_TwoIdentical_Zero()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 1);
        Assert.Equal(PredicateSign.Zero, Orient2D.Evaluate(a, b, a));
    }

    [Fact]
    public void Orient2D_LargeCoordinates_Correct()
    {
        var a = new Vec2(1e8, 1e8);
        var b = new Vec2(1e8 + 1, 1e8);
        var c = new Vec2(1e8, 1e8 + 1);
        Assert.Equal(PredicateSign.Positive, Orient2D.Evaluate(a, b, c));
    }

    // --- Orient3D ---

    [Fact]
    public void Orient3D_AbovePlane_Positive()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0, 0, 1);
        Assert.Equal(PredicateSign.Positive, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void Orient3D_BelowPlane_Negative()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0, 0, -1);
        Assert.Equal(PredicateSign.Negative, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void Orient3D_OnPlane_Zero()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0.5, 0.5, 0);
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void Orient3D_SwapBC_FlipsSign()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0, 0, 1);
        var s1 = Orient3D.Evaluate(a, b, c, d);
        var s2 = Orient3D.Evaluate(a, c, b, d);
        Assert.Equal(-(int)s1, (int)s2);
    }

    [Fact]
    public void Orient3D_AllCoplanar_Zero()
    {
        // All four points on z=0 plane
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(1, 1, 0);
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void Orient3D_IdenticalPoints_Zero()
    {
        var p = new Vec3(3, 4, 5);
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(p, p, p, p));
    }

    [Fact]
    public void Orient3D_LargeCoordinates_Correct()
    {
        var a = new Vec3(1e8, 0, 0);
        var b = new Vec3(0, 1e8, 0);
        var c = new Vec3(0, 0, 1e8);
        var d = new Vec3(1e8, 1e8, 1e8);
        // d is above the plane ABC
        var sign = Orient3D.Evaluate(a, b, c, d);
        Assert.NotEqual(PredicateSign.Zero, sign);
    }

    // --- InCircle ---

    [Fact]
    public void InCircle_InsideCircle_Positive()
    {
        // CCW triangle forming a circle, d at center
        var a = new Vec2(1, 0);
        var b = new Vec2(0, 1);
        var c = new Vec2(-1, 0);
        var d = new Vec2(0, 0);
        Assert.Equal(PredicateSign.Positive, InCircle.Evaluate(a, b, c, d));
    }

    [Fact]
    public void InCircle_OutsideCircle_Negative()
    {
        var a = new Vec2(1, 0);
        var b = new Vec2(0, 1);
        var c = new Vec2(-1, 0);
        var d = new Vec2(0, -5);
        Assert.Equal(PredicateSign.Negative, InCircle.Evaluate(a, b, c, d));
    }

    [Fact]
    public void InCircle_OnCircle_Zero()
    {
        // Unit circle points: all on the circle
        var a = new Vec2(1, 0);
        var b = new Vec2(0, 1);
        var c = new Vec2(-1, 0);
        var d = new Vec2(0, -1);
        Assert.Equal(PredicateSign.Zero, InCircle.Evaluate(a, b, c, d));
    }

    [Fact]
    public void InCircle_SwapAB_FlipsSign()
    {
        var a = new Vec2(1, 0);
        var b = new Vec2(0, 1);
        var c = new Vec2(-1, 0);
        var d = new Vec2(0, 0);
        var s1 = InCircle.Evaluate(a, b, c, d);
        var s2 = InCircle.Evaluate(b, a, c, d);
        Assert.Equal(-(int)s1, (int)s2);
    }

    [Fact]
    public void InCircle_FarInsideCircle_Positive()
    {
        var a = new Vec2(10, 0);
        var b = new Vec2(0, 10);
        var c = new Vec2(-10, 0);
        var d = new Vec2(0, 0);
        Assert.Equal(PredicateSign.Positive, InCircle.Evaluate(a, b, c, d));
    }

    // --- PlaneClassification ---

    [Fact]
    public void PlaneClassification_AbovePlane_Positive()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        Assert.Equal(PredicateSign.Positive, PlaneClassification.Classify(a, b, c, new Vec3(0, 0, 1)));
    }

    [Fact]
    public void PlaneClassification_BelowPlane_Negative()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        Assert.Equal(PredicateSign.Negative, PlaneClassification.Classify(a, b, c, new Vec3(0, 0, -1)));
    }

    [Fact]
    public void PlaneClassification_OnPlane_Zero()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        Assert.Equal(PredicateSign.Zero, PlaneClassification.Classify(a, b, c, new Vec3(3, 4, 0)));
    }

    [Fact]
    public void PlaneClassification_MatchesOrient3D()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var c = new Vec3(7, 8, 10);
        var d = new Vec3(1, 1, 1);
        Assert.Equal(Orient3D.Evaluate(a, b, c, d), PlaneClassification.Classify(a, b, c, d));
    }

    // --- PredicateSign enum ---

    [Fact]
    public void PredicateSign_Values_MatchIntegers()
    {
        Assert.Equal(-1, (int)PredicateSign.Negative);
        Assert.Equal(0, (int)PredicateSign.Zero);
        Assert.Equal(1, (int)PredicateSign.Positive);
    }
}
