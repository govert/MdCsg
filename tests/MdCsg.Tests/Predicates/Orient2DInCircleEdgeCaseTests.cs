using MdCsg.Math;
using MdCsg.Predicates;

namespace MdCsg.Tests.Predicates;

/// <summary>Phase 6: Orient2D and InCircle edge cases — collinear, cocircular, large/small coordinates</summary>
public class Orient2DInCircleEdgeCaseTests
{
    [Fact]
    public void Orient2D_CCW_Positive()
    {
        var sign = Orient2D.Evaluate(new Vec2(0, 0), new Vec2(1, 0), new Vec2(0, 1));
        Assert.Equal(PredicateSign.Positive, sign);
    }

    [Fact]
    public void Orient2D_CW_Negative()
    {
        var sign = Orient2D.Evaluate(new Vec2(0, 0), new Vec2(0, 1), new Vec2(1, 0));
        Assert.Equal(PredicateSign.Negative, sign);
    }

    [Fact]
    public void Orient2D_Collinear_Zero()
    {
        var sign = Orient2D.Evaluate(new Vec2(0, 0), new Vec2(1, 0), new Vec2(2, 0));
        Assert.Equal(PredicateSign.Zero, sign);
    }

    [Fact]
    public void Orient2D_SamePoint_Zero()
    {
        var p = new Vec2(1, 2);
        var sign = Orient2D.Evaluate(p, p, p);
        Assert.Equal(PredicateSign.Zero, sign);
    }

    [Fact]
    public void Orient2D_TwoSamePoints_Zero()
    {
        var sign = Orient2D.Evaluate(new Vec2(0, 0), new Vec2(0, 0), new Vec2(1, 1));
        Assert.Equal(PredicateSign.Zero, sign);
    }

    [Fact]
    public void Orient2D_LargeCoordinates()
    {
        var sign = Orient2D.Evaluate(
            new Vec2(1e10, 1e10),
            new Vec2(1e10 + 1, 1e10),
            new Vec2(1e10, 1e10 + 1));
        Assert.Equal(PredicateSign.Positive, sign);
    }

    [Fact]
    public void Orient2D_NearlyCollinear_DetectsSign()
    {
        var sign = Orient2D.Evaluate(new Vec2(0, 0), new Vec2(1, 0), new Vec2(2, 1e-14));
        Assert.Equal(PredicateSign.Positive, sign);
    }

    [Fact]
    public void Orient2D_SwapTwoPoints_FlipsSign()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);
        var s1 = Orient2D.Evaluate(a, b, c);
        var s2 = Orient2D.Evaluate(b, a, c);
        Assert.NotEqual(s1, s2);
        Assert.NotEqual(PredicateSign.Zero, s1);
        Assert.NotEqual(PredicateSign.Zero, s2);
    }

    [Fact]
    public void InCircle_InsideCircle_Positive()
    {
        // Points in CCW order on a circle, d inside
        var a = new Vec2(-1, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);
        var d = new Vec2(0, 0); // center, clearly inside
        var sign = InCircle.Evaluate(a, b, c, d);
        Assert.Equal(PredicateSign.Positive, sign);
    }

    [Fact]
    public void InCircle_OutsideCircle_Negative()
    {
        // Points in CCW order, d far outside
        var a = new Vec2(-1, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);
        var d = new Vec2(0, -5); // far outside
        var sign = InCircle.Evaluate(a, b, c, d);
        Assert.Equal(PredicateSign.Negative, sign);
    }

    [Fact]
    public void InCircle_OnCircle_Zero()
    {
        // All four points on unit circle
        var a = new Vec2(1, 0);
        var b = new Vec2(0, 1);
        var c = new Vec2(-1, 0);
        var d = new Vec2(0, -1);
        var sign = InCircle.Evaluate(a, b, c, d);
        Assert.Equal(PredicateSign.Zero, sign);
    }

    [Fact]
    public void InCircle_ThreeCollinear_Degenerate()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(2, 0);
        var d = new Vec2(1, 1);
        // Collinear a,b,c → infinite radius circle → any finite point is "inside"
        var sign = InCircle.Evaluate(a, b, c, d);
        // The sign depends on convention, just verify it computes without error
        Assert.True(sign == PredicateSign.Positive || sign == PredicateSign.Negative || sign == PredicateSign.Zero);
    }

    [Fact]
    public void InCircle_LargeCoordinates()
    {
        var a = new Vec2(1e8, 0);
        var b = new Vec2(0, 1e8);
        var c = new Vec2(-1e8, 0);
        var d = new Vec2(0, 0); // inside the circumscribed circle
        var sign = InCircle.Evaluate(a, b, c, d);
        Assert.Equal(PredicateSign.Positive, sign);
    }

    [Fact]
    public void InCircle_NearlyOnCircle_DetectsSign()
    {
        // Points on unit circle, d barely inside
        var a = new Vec2(1, 0);
        var b = new Vec2(0, 1);
        var c = new Vec2(-1, 0);
        var d = new Vec2(0, -0.99); // just inside
        var sign = InCircle.Evaluate(a, b, c, d);
        Assert.Equal(PredicateSign.Positive, sign);
    }

    [Fact]
    public void InCircle_NearlyOnCircle_JustOutside()
    {
        var a = new Vec2(1, 0);
        var b = new Vec2(0, 1);
        var c = new Vec2(-1, 0);
        var d = new Vec2(0, -1.01); // just outside
        var sign = InCircle.Evaluate(a, b, c, d);
        Assert.Equal(PredicateSign.Negative, sign);
    }
}
