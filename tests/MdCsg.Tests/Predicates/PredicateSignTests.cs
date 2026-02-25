using MdCsg.Math;
using MdCsg.Predicates;

namespace MdCsg.Tests.Predicates;

/// <summary>Phase 6: PredicateSign enum, Orient2D/3D extended property tests, InCircle boundary tests</summary>
public class PredicateSignTests
{
    [Fact]
    public void PredicateSign_Values()
    {
        Assert.Equal(-1, (int)PredicateSign.Negative);
        Assert.Equal(0, (int)PredicateSign.Zero);
        Assert.Equal(1, (int)PredicateSign.Positive);
    }

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
    public void Orient2D_SwapBC_FlipsSign()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);
        var s1 = Orient2D.Evaluate(a, b, c);
        var s2 = Orient2D.Evaluate(a, c, b);
        Assert.NotEqual(s1, s2);
    }

    [Fact]
    public void Orient3D_AbovePlane_Positive()
    {
        // D above the plane of A,B,C (XY plane with normal +Z)
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0, 0, 1);
        // Orient3D = det(B-A, C-A, D-A)
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
    public void Orient3D_SwapCD_FlipsSign()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0, 0, 1);
        var s1 = Orient3D.Evaluate(a, b, c, d);
        var s2 = Orient3D.Evaluate(a, b, d, c);
        Assert.NotEqual(s1, s2);
    }

    [Fact]
    public void InCircle_Inside_Positive()
    {
        // Point inside the circumcircle of a CCW triangle
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);
        var d = new Vec2(0.25, 0.25);
        Assert.Equal(PredicateSign.Positive, InCircle.Evaluate(a, b, c, d));
    }

    [Fact]
    public void InCircle_Outside_Negative()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);
        var d = new Vec2(10, 10); // far outside
        Assert.Equal(PredicateSign.Negative, InCircle.Evaluate(a, b, c, d));
    }

    [Fact]
    public void InCircle_OnCircle_Zero()
    {
        // Circumcircle of right triangle at origin has center (0.5, 0.5) radius sqrt(0.5)
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);
        var d = new Vec2(1, 1); // on the circumcircle
        Assert.Equal(PredicateSign.Zero, InCircle.Evaluate(a, b, c, d));
    }

    [Fact]
    public void PlaneClassification_AbovePlane()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var point = new Vec3(0, 0, 1);
        var result = PlaneClassification.Classify(a, b, c, point);
        Assert.Equal(PredicateSign.Positive, result);
    }

    [Fact]
    public void PlaneClassification_BelowPlane()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var point = new Vec3(0, 0, -1);
        Assert.Equal(PredicateSign.Negative, PlaneClassification.Classify(a, b, c, point));
    }

    [Fact]
    public void PlaneClassification_OnPlane()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var point = new Vec3(0.5, 0.5, 0);
        Assert.Equal(PredicateSign.Zero, PlaneClassification.Classify(a, b, c, point));
    }

    [Fact]
    public void Orient2D_NearlyCollinear_UsesAdaptive()
    {
        // Points very nearly collinear — tests the adaptive precision path
        double eps = 1e-14;
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0.5, eps);
        // Should still correctly report positive (above)
        Assert.Equal(PredicateSign.Positive, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void Orient3D_NearlyCoplanar_UsesAdaptive()
    {
        double eps = 1e-14;
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0.5, 0.5, eps);
        Assert.Equal(PredicateSign.Positive, Orient3D.Evaluate(a, b, c, d));
    }
}
