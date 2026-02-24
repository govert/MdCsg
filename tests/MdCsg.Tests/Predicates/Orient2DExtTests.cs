using MdCsg.Math;
using MdCsg.Predicates;

namespace MdCsg.Tests.Predicates;

/// <summary>Batch 6: Orient2D extended tests (20 tests)</summary>
public class Orient2DExtTests
{
    [Fact]
    public void EquilateralTriangle_CCW()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0.5, System.Math.Sqrt(3) / 2);
        Assert.Equal(PredicateSign.Positive, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void EquilateralTriangle_CW()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(0.5, System.Math.Sqrt(3) / 2);
        var c = new Vec2(1, 0);
        Assert.Equal(PredicateSign.Negative, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void RightAngle_CCW()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);
        Assert.Equal(PredicateSign.Positive, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void RightAngle_CW()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(0, 1);
        var c = new Vec2(1, 0);
        Assert.Equal(PredicateSign.Negative, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void ThreeCollinear_OnXAxis()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(2, 0);
        Assert.Equal(PredicateSign.Zero, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void ThreeCollinear_OnDiagonal()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 1);
        var c = new Vec2(2, 2);
        Assert.Equal(PredicateSign.Zero, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void ThreeCollinear_Reversed()
    {
        var a = new Vec2(2, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 0);
        Assert.Equal(PredicateSign.Zero, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void AllSamePoint()
    {
        var p = new Vec2(5, 5);
        Assert.Equal(PredicateSign.Zero, Orient2D.Evaluate(p, p, p));
    }

    [Fact]
    public void LargeCoordinates_CCW()
    {
        var a = new Vec2(1e10, 1e10);
        var b = new Vec2(1e10 + 1, 1e10);
        var c = new Vec2(1e10, 1e10 + 1);
        Assert.Equal(PredicateSign.Positive, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void LargeCoordinates_CW()
    {
        var a = new Vec2(1e10, 1e10);
        var b = new Vec2(1e10, 1e10 + 1);
        var c = new Vec2(1e10 + 1, 1e10);
        Assert.Equal(PredicateSign.Negative, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void NegativeCoordinates()
    {
        var a = new Vec2(-1, -1);
        var b = new Vec2(0, -1);
        var c = new Vec2(-1, 0);
        Assert.Equal(PredicateSign.Positive, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void TinyTriangle_CCW()
    {
        double eps = 1e-12;
        var a = new Vec2(0, 0);
        var b = new Vec2(eps, 0);
        var c = new Vec2(0, eps);
        Assert.Equal(PredicateSign.Positive, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void TinyTriangle_CW()
    {
        double eps = 1e-12;
        var a = new Vec2(0, 0);
        var b = new Vec2(0, eps);
        var c = new Vec2(eps, 0);
        Assert.Equal(PredicateSign.Negative, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void CyclicPermutation_SameSign()
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
    public void SwapTwo_FlipsSign()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);
        var s1 = Orient2D.Evaluate(a, b, c);
        var s2 = Orient2D.Evaluate(a, c, b);
        Assert.Equal(PredicateSign.Positive, s1);
        Assert.Equal(PredicateSign.Negative, s2);
    }

    [Fact]
    public void PointOnSegment_Collinear()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(2, 0);
        var c = new Vec2(1, 0); // midpoint
        Assert.Equal(PredicateSign.Zero, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void PointJustLeft()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(2, 0);
        var c = new Vec2(1, 1e-14); // just above
        Assert.Equal(PredicateSign.Positive, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void PointJustRight()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(2, 0);
        var c = new Vec2(1, -1e-14); // just below
        Assert.Equal(PredicateSign.Negative, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void UnitSquareVertices_CCW()
    {
        // 3 of 4 corners of unit square
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(1, 1);
        Assert.Equal(PredicateSign.Positive, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void MixedLargeAndSmall()
    {
        var a = new Vec2(1e8, 0);
        var b = new Vec2(1e8 + 1e-6, 0);
        var c = new Vec2(1e8, 1e-6);
        Assert.Equal(PredicateSign.Positive, Orient2D.Evaluate(a, b, c));
    }
}
