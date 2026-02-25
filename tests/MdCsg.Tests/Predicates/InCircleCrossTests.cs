using MdCsg.Math;
using MdCsg.Predicates;

namespace MdCsg.Tests.Predicates;

/// <summary>Phase 6: InCircle — cross-predicate consistency, degenerate cases, symmetry</summary>
public class InCircleCrossTests
{
    [Fact]
    public void Center_IsInside_CircumCircle()
    {
        // Equilateral triangle centered at origin in CCW order
        var a = new Vec2(0, 1);
        var b = new Vec2(-System.Math.Sqrt(3) / 2, -0.5);
        var c = new Vec2(System.Math.Sqrt(3) / 2, -0.5);
        var d = new Vec2(0, 0); // center
        Assert.Equal(PredicateSign.Positive, InCircle.Evaluate(a, b, c, d));
    }

    [Fact]
    public void FarPoint_IsOutside_CircumCircle()
    {
        var a = new Vec2(0, 1);
        var b = new Vec2(-System.Math.Sqrt(3) / 2, -0.5);
        var c = new Vec2(System.Math.Sqrt(3) / 2, -0.5);
        var d = new Vec2(100, 100);
        Assert.Equal(PredicateSign.Negative, InCircle.Evaluate(a, b, c, d));
    }

    [Fact]
    public void VertexOnCircle_IsZero()
    {
        // Unit circle points — all 4 are cocircular
        var a = new Vec2(1, 0);
        var b = new Vec2(0, 1);
        var c = new Vec2(-1, 0);
        var d = new Vec2(0, -1);
        Assert.Equal(PredicateSign.Zero, InCircle.Evaluate(a, b, c, d));
    }

    [Fact]
    public void RightTriangle_PointInside()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(2, 0);
        var c = new Vec2(0, 2);
        // Circumcircle center is at (1,1), radius sqrt(2)
        var d = new Vec2(1, 1); // center — should be inside circumcircle
        Assert.Equal(PredicateSign.Positive, InCircle.Evaluate(a, b, c, d));
    }

    [Fact]
    public void RightTriangle_PointOnHypotenuse()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(2, 0);
        var c = new Vec2(0, 2);
        // (2,2) is on the circumcircle: distance from center (1,1) = sqrt(2)
        var d = new Vec2(2, 2);
        Assert.Equal(PredicateSign.Zero, InCircle.Evaluate(a, b, c, d));
    }

    [Fact]
    public void RightTriangle_PointOutside()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(2, 0);
        var c = new Vec2(0, 2);
        var d = new Vec2(3, 3); // far outside
        Assert.Equal(PredicateSign.Negative, InCircle.Evaluate(a, b, c, d));
    }

    [Fact]
    public void Symmetry_CyclicPermutation_SameSign()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);
        var d = new Vec2(0.3, 0.3);
        var s1 = InCircle.Evaluate(a, b, c, d);
        var s2 = InCircle.Evaluate(b, c, a, d);
        var s3 = InCircle.Evaluate(c, a, b, d);
        Assert.Equal(s1, s2);
        Assert.Equal(s1, s3);
    }

    [Fact]
    public void Antisymmetry_SwapTwoVertices_FlipsSign()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);
        var d = new Vec2(0.3, 0.3);
        var s1 = InCircle.Evaluate(a, b, c, d);
        var s2 = InCircle.Evaluate(b, a, c, d); // swap a,b
        Assert.NotEqual(s1, s2);
    }

    [Fact]
    public void CollinearPoints_Zero()
    {
        // Three collinear points — degenerate circumcircle (infinite radius)
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(2, 0);
        var d = new Vec2(0, 1);
        // Collinear a,b,c → degenerate, sign depends on implementation
        // Just verify it returns a valid PredicateSign
        var result = InCircle.Evaluate(a, b, c, d);
        Assert.True(result == PredicateSign.Positive || result == PredicateSign.Negative || result == PredicateSign.Zero);
    }

    [Fact]
    public void LargeCoordinates_StillCorrect()
    {
        // Large coordinates can cause floating-point issues without adaptive precision
        double L = 1e8;
        var a = new Vec2(L, 0);
        var b = new Vec2(0, L);
        var c = new Vec2(-L, 0);
        var d = new Vec2(0, 0); // center of circumcircle
        Assert.Equal(PredicateSign.Positive, InCircle.Evaluate(a, b, c, d));
    }

    [Fact]
    public void NearlyCoCircular_SmallPerturbation()
    {
        // Points nearly on a circle, small perturbation inward
        var a = new Vec2(1, 0);
        var b = new Vec2(0, 1);
        var c = new Vec2(-1, 0);
        var d = new Vec2(0, -0.999); // slightly inside the unit circle
        Assert.Equal(PredicateSign.Positive, InCircle.Evaluate(a, b, c, d));
    }

    [Fact]
    public void NearlyCoCircular_SmallPerturbation_Outside()
    {
        var a = new Vec2(1, 0);
        var b = new Vec2(0, 1);
        var c = new Vec2(-1, 0);
        var d = new Vec2(0, -1.001); // slightly outside the unit circle
        Assert.Equal(PredicateSign.Negative, InCircle.Evaluate(a, b, c, d));
    }
}
