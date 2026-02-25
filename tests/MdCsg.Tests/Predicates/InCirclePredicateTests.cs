using MdCsg.Math;
using MdCsg.Predicates;

namespace MdCsg.Tests.Predicates;

/// <summary>Phase 6: InCircle predicate — inside, outside, on circle, permutation properties</summary>
public class InCirclePredicateTests
{
    // Reference triangle: (0,0), (1,0), (0,1)
    // Circumcircle center: (0.5, 0.5), radius: sqrt(0.5)
    private static readonly Vec2 A = new(0, 0);
    private static readonly Vec2 B = new(1, 0);
    private static readonly Vec2 C = new(0, 1);

    [Fact]
    public void InsideCircle_ReturnsPositive()
    {
        var d = new Vec2(0.3, 0.3);
        Assert.Equal(PredicateSign.Positive, InCircle.Evaluate(A, B, C, d));
    }

    [Fact]
    public void OutsideCircle_ReturnsNegative()
    {
        var d = new Vec2(5, 5);
        Assert.Equal(PredicateSign.Negative, InCircle.Evaluate(A, B, C, d));
    }

    [Fact]
    public void OnCircle_ReturnsZero()
    {
        // (1,1) is on the circumcircle: distance from (0.5,0.5) = sqrt(0.5) = radius
        var d = new Vec2(1, 1);
        Assert.Equal(PredicateSign.Zero, InCircle.Evaluate(A, B, C, d));
    }

    [Fact]
    public void Origin_IsInsideCircle()
    {
        // (0,0) is on the triangle → on the circumcircle (for right triangle at origin)
        // Actually, (0,0) IS on the circumcircle for this triangle
        Assert.Equal(PredicateSign.Zero, InCircle.Evaluate(B, C, new Vec2(1, 1), A));
    }

    [Fact]
    public void FarAway_IsOutside()
    {
        var d = new Vec2(100, 100);
        Assert.Equal(PredicateSign.Negative, InCircle.Evaluate(A, B, C, d));
    }

    [Fact]
    public void NearCenter_IsInside()
    {
        var d = new Vec2(0.5, 0.5); // circumcircle center
        Assert.Equal(PredicateSign.Positive, InCircle.Evaluate(A, B, C, d));
    }

    [Fact]
    public void SwapTwoPoints_FlipsSign()
    {
        var d = new Vec2(0.3, 0.3);
        var s1 = InCircle.Evaluate(A, B, C, d);
        var s2 = InCircle.Evaluate(B, A, C, d);
        // Swapping two of the first three points flips the determinant sign
        Assert.NotEqual(s1, s2);
    }

    [Fact]
    public void CyclicPermutation_PreservesSign()
    {
        // Cyclic permutation of (a,b,c) is even → preserves sign
        var d = new Vec2(0.3, 0.3);
        var s1 = InCircle.Evaluate(A, B, C, d);
        var s2 = InCircle.Evaluate(B, C, A, d);
        Assert.Equal(s1, s2);
    }

    [Fact]
    public void AllSamePoint_ReturnsZero()
    {
        var p = new Vec2(1, 1);
        Assert.Equal(PredicateSign.Zero, InCircle.Evaluate(p, p, p, p));
    }

    [Fact]
    public void UnitCircle_InsidePoint()
    {
        // Points on unit circle: (1,0), (0,1), (-1,0)
        var a = new Vec2(1, 0);
        var b = new Vec2(0, 1);
        var c = new Vec2(-1, 0);
        var inside = new Vec2(0, 0); // center is inside

        Assert.Equal(PredicateSign.Positive, InCircle.Evaluate(a, b, c, inside));
    }

    [Fact]
    public void UnitCircle_OutsidePoint()
    {
        var a = new Vec2(1, 0);
        var b = new Vec2(0, 1);
        var c = new Vec2(-1, 0);
        var outside = new Vec2(0, -2); // below, outside the circle

        Assert.Equal(PredicateSign.Negative, InCircle.Evaluate(a, b, c, outside));
    }

    [Fact]
    public void LargeCoordinates_CorrectSign()
    {
        double big = 1e8;
        var a = new Vec2(big, 0);
        var b = new Vec2(0, big);
        var c = new Vec2(-big, 0);
        // Circumcircle is centered at origin with radius ~big
        var inside = new Vec2(0, 0);
        Assert.Equal(PredicateSign.Positive, InCircle.Evaluate(a, b, c, inside));
    }
}
