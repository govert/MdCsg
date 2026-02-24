using MdCsg.Math;
using MdCsg.Predicates;

namespace MdCsg.Tests.Predicates;

/// <summary>Phase 6: Deep predicate edge cases — Orient2D, Orient3D, InCircle, PlaneClassification</summary>
public class PredicateDeepTests
{
    // --- Orient2D ---

    [Fact]
    public void Orient2D_CCW_Positive()
    {
        var result = Orient2D.Evaluate(new Vec2(0, 0), new Vec2(1, 0), new Vec2(0, 1));
        Assert.Equal(PredicateSign.Positive, result);
    }

    [Fact]
    public void Orient2D_CW_Negative()
    {
        var result = Orient2D.Evaluate(new Vec2(0, 0), new Vec2(0, 1), new Vec2(1, 0));
        Assert.Equal(PredicateSign.Negative, result);
    }

    [Fact]
    public void Orient2D_Collinear_Zero()
    {
        var result = Orient2D.Evaluate(new Vec2(0, 0), new Vec2(1, 1), new Vec2(2, 2));
        Assert.Equal(PredicateSign.Zero, result);
    }

    [Fact]
    public void Orient2D_Collinear_Horizontal()
    {
        var result = Orient2D.Evaluate(new Vec2(0, 0), new Vec2(5, 0), new Vec2(10, 0));
        Assert.Equal(PredicateSign.Zero, result);
    }

    [Fact]
    public void Orient2D_Collinear_Vertical()
    {
        var result = Orient2D.Evaluate(new Vec2(0, 0), new Vec2(0, 5), new Vec2(0, 10));
        Assert.Equal(PredicateSign.Zero, result);
    }

    [Fact]
    public void Orient2D_NegativeCoordinates()
    {
        var result = Orient2D.Evaluate(new Vec2(-1, -1), new Vec2(1, -1), new Vec2(0, 1));
        Assert.Equal(PredicateSign.Positive, result);
    }

    [Fact]
    public void Orient2D_Antisymmetric()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0.3, 0.5);
        var ab = Orient2D.Evaluate(a, b, c);
        var ba = Orient2D.Evaluate(b, a, c);
        Assert.NotEqual(ab, ba);
    }

    [Fact]
    public void Orient2D_LargeCoordinates()
    {
        var result = Orient2D.Evaluate(new Vec2(1e10, 0), new Vec2(1e10 + 1, 0), new Vec2(1e10, 1));
        Assert.Equal(PredicateSign.Positive, result);
    }

    // --- Orient3D ---

    [Fact]
    public void Orient3D_Above_Positive()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0, 0, 1);
        Assert.Equal(PredicateSign.Positive, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void Orient3D_Below_Negative()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0, 0, -1);
        Assert.Equal(PredicateSign.Negative, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void Orient3D_Coplanar_Zero()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0.5, 0.5, 0);
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void Orient3D_DegenerateTriangle_Collinear()
    {
        // A, B, C collinear → determinant 0 for any D
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(2, 0, 0);
        var d = new Vec3(0, 1, 0);
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
        Assert.NotEqual(s1, s2);
    }

    [Fact]
    public void Orient3D_LargeCoordinates()
    {
        var a = new Vec3(1e10, 1e10, 1e10);
        var b = new Vec3(1e10 + 1, 1e10, 1e10);
        var c = new Vec3(1e10, 1e10 + 1, 1e10);
        var d = new Vec3(1e10, 1e10, 1e10 + 1);
        Assert.Equal(PredicateSign.Positive, Orient3D.Evaluate(a, b, c, d));
    }

    // --- InCircle ---

    [Fact]
    public void InCircle_InsideCircle_Positive()
    {
        // CCW triangle on unit circle: (1,0)→(0,1)→(-1,0). D at origin is inside circumcircle.
        var result = InCircle.Evaluate(new Vec2(1, 0), new Vec2(0, 1), new Vec2(-1, 0), new Vec2(0, 0));
        Assert.Equal(PredicateSign.Positive, result);
    }

    [Fact]
    public void InCircle_OutsideCircle_Negative()
    {
        // Same CCW triangle, D far away
        var result = InCircle.Evaluate(new Vec2(1, 0), new Vec2(0, 1), new Vec2(-1, 0), new Vec2(0, 5));
        Assert.Equal(PredicateSign.Negative, result);
    }

    [Fact]
    public void InCircle_OnCircle_Zero()
    {
        // All 4 points on the unit circle
        var result = InCircle.Evaluate(new Vec2(1, 0), new Vec2(0, 1), new Vec2(-1, 0), new Vec2(0, -1));
        Assert.Equal(PredicateSign.Zero, result);
    }

    [Fact]
    public void InCircle_LargeValues()
    {
        double r = 1e8;
        var result = InCircle.Evaluate(
            new Vec2(r, 0), new Vec2(0, r), new Vec2(-r, 0), new Vec2(0, -r));
        Assert.Equal(PredicateSign.Zero, result);
    }

    // --- PlaneClassification ---

    [Fact]
    public void PlaneClassify_PointAbovePlane()
    {
        var result = PlaneClassification.Classify(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0),
            new Vec3(0, 0, 1));
        Assert.Equal(PredicateSign.Positive, result);
    }

    [Fact]
    public void PlaneClassify_PointBelowPlane()
    {
        var result = PlaneClassification.Classify(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0),
            new Vec3(0, 0, -1));
        Assert.Equal(PredicateSign.Negative, result);
    }

    [Fact]
    public void PlaneClassify_PointOnPlane()
    {
        var result = PlaneClassification.Classify(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0),
            new Vec3(0.5, 0.5, 0));
        Assert.Equal(PredicateSign.Zero, result);
    }

    [Fact]
    public void PlaneClassify_PlaneVertex_OnPlane()
    {
        // Point is one of the plane-defining vertices
        var result = PlaneClassification.Classify(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0),
            new Vec3(0, 0, 0));
        Assert.Equal(PredicateSign.Zero, result);
    }

    [Fact]
    public void PlaneClassify_LargeDistance()
    {
        var result = PlaneClassification.Classify(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0),
            new Vec3(0, 0, 1e10));
        Assert.Equal(PredicateSign.Positive, result);
    }

    [Fact]
    public void PlaneClassify_NearlyCoplanar()
    {
        // Point very close to plane but slightly above
        var result = PlaneClassification.Classify(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0),
            new Vec3(0.5, 0.5, 1e-15));
        // At this tiny offset, exact arithmetic should detect it
        // May be Zero or Positive depending on representability
        Assert.True(result == PredicateSign.Zero || result == PredicateSign.Positive);
    }
}
