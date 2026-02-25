using MdCsg.Math;
using MdCsg.Predicates;

namespace MdCsg.Tests.Predicates;

/// <summary>Phase 6: Orient2D — robustness, symmetry, near-collinear configurations</summary>
public class Orient2DRobustnessPropertyTests
{
    [Fact]
    public void CCW_TriangleVertices_Positive()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);
        Assert.Equal(PredicateSign.Positive, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void CW_TriangleVertices_Negative()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(0, 1);
        var c = new Vec2(1, 0);
        Assert.Equal(PredicateSign.Negative, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void Collinear_Points_Zero()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(2, 0);
        Assert.Equal(PredicateSign.Zero, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void SwapAB_FlipsSign()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0.5, 1);
        var s1 = Orient2D.Evaluate(a, b, c);
        var s2 = Orient2D.Evaluate(b, a, c);
        Assert.NotEqual(s1, s2);
    }

    [Fact]
    public void CyclicPermutation_SameSign()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0.5, 1);
        var s1 = Orient2D.Evaluate(a, b, c);
        var s2 = Orient2D.Evaluate(b, c, a);
        var s3 = Orient2D.Evaluate(c, a, b);
        Assert.Equal(s1, s2);
        Assert.Equal(s2, s3);
    }

    [Fact]
    public void IdenticalPoints_Zero()
    {
        var p = new Vec2(1, 2);
        Assert.Equal(PredicateSign.Zero, Orient2D.Evaluate(p, p, p));
    }

    [Fact]
    public void TwoIdenticalPoints_Zero()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        Assert.Equal(PredicateSign.Zero, Orient2D.Evaluate(a, b, a));
        Assert.Equal(PredicateSign.Zero, Orient2D.Evaluate(a, a, b));
        Assert.Equal(PredicateSign.Zero, Orient2D.Evaluate(a, b, b));
    }

    [Fact]
    public void NearCollinear_SlightlyLeft_Positive()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0.5, 1e-10);
        Assert.Equal(PredicateSign.Positive, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void NearCollinear_SlightlyRight_Negative()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0.5, -1e-10);
        Assert.Equal(PredicateSign.Negative, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void LargeCoordinates_CorrectSign()
    {
        double big = 1e8;
        var a = new Vec2(big, big);
        var b = new Vec2(big + 1, big);
        var c = new Vec2(big + 0.5, big + 1);
        Assert.Equal(PredicateSign.Positive, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void LargeCoordinates_Collinear_Zero()
    {
        double big = 1e8;
        var a = new Vec2(big, big);
        var b = new Vec2(big + 1, big + 1);
        var c = new Vec2(big + 2, big + 2);
        Assert.Equal(PredicateSign.Zero, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void UnitSquareCorners_CCW()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(1, 1);
        Assert.Equal(PredicateSign.Positive, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void DiagonalLine_Collinear()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(3, 3);
        var c = new Vec2(1, 1);
        Assert.Equal(PredicateSign.Zero, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void NegativeCoordinates_Works()
    {
        var a = new Vec2(-1, -1);
        var b = new Vec2(1, -1);
        var c = new Vec2(0, 1);
        Assert.Equal(PredicateSign.Positive, Orient2D.Evaluate(a, b, c));
    }
}
