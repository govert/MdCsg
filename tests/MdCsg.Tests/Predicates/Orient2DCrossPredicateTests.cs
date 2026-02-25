using MdCsg.Math;
using MdCsg.Predicates;

namespace MdCsg.Tests.Predicates;

/// <summary>Phase 6: Orient2D cross-predicate — symmetry, antisymmetry, consistency with cross product, degenerate</summary>
public class Orient2DCrossPredicateTests
{
    [Fact]
    public void CCW_Triangle_IsPositive()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);
        Assert.Equal(PredicateSign.Positive, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void CW_Triangle_IsNegative()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(0, 1);
        var c = new Vec2(1, 0);
        Assert.Equal(PredicateSign.Negative, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void Collinear_IsZero()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(2, 0);
        Assert.Equal(PredicateSign.Zero, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void CyclicPermutation_PreservesSign()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);
        var s1 = Orient2D.Evaluate(a, b, c);
        var s2 = Orient2D.Evaluate(b, c, a);
        var s3 = Orient2D.Evaluate(c, a, b);
        Assert.Equal(s1, s2);
        Assert.Equal(s1, s3);
    }

    [Fact]
    public void SwapTwoPoints_FlipsSign()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);
        var s1 = Orient2D.Evaluate(a, b, c);
        var s2 = Orient2D.Evaluate(b, a, c);
        Assert.NotEqual(s1, s2);
    }

    [Fact]
    public void IdenticalPoints_IsZero()
    {
        var p = new Vec2(5, 5);
        Assert.Equal(PredicateSign.Zero, Orient2D.Evaluate(p, p, p));
    }

    [Fact]
    public void TwoIdenticalPoints_IsZero()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 1);
        Assert.Equal(PredicateSign.Zero, Orient2D.Evaluate(a, a, b));
        Assert.Equal(PredicateSign.Zero, Orient2D.Evaluate(a, b, b));
        Assert.Equal(PredicateSign.Zero, Orient2D.Evaluate(a, b, a));
    }

    [Fact]
    public void LargeCoordinates_StillCorrect()
    {
        double L = 1e12;
        var a = new Vec2(L, 0);
        var b = new Vec2(L + 1, 0);
        var c = new Vec2(L, 1);
        Assert.Equal(PredicateSign.Positive, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void NearlyCollinear_SmallPerturbation()
    {
        // Nearly collinear but c is slightly above the line
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0.5, 1e-14);
        Assert.Equal(PredicateSign.Positive, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void NearlyCollinear_SmallPerturbationBelow()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0.5, -1e-14);
        Assert.Equal(PredicateSign.Negative, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void ConsistencyWithCrossProduct()
    {
        // Orient2D sign should agree with cross product sign (b-a)×(c-a)
        var rng = new System.Random(42);
        for (int i = 0; i < 50; i++)
        {
            var a = new Vec2(rng.NextDouble(), rng.NextDouble());
            var b = new Vec2(rng.NextDouble(), rng.NextDouble());
            var c = new Vec2(rng.NextDouble(), rng.NextDouble());
            var sign = Orient2D.Evaluate(a, b, c);
            double cross = (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
            if (sign == PredicateSign.Positive) Assert.True(cross > -1e-10);
            if (sign == PredicateSign.Negative) Assert.True(cross < 1e-10);
        }
    }

    [Fact]
    public void PlaneClassification_ConsistentWithOrient3D()
    {
        // PlaneClassification.Classify delegates to Orient3D
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var above = new Vec3(0, 0, 1);
        var below = new Vec3(0, 0, -1);
        var on = new Vec3(0.5, 0.5, 0);
        Assert.Equal(Orient3D.Evaluate(a, b, c, above), PlaneClassification.Classify(a, b, c, above));
        Assert.Equal(Orient3D.Evaluate(a, b, c, below), PlaneClassification.Classify(a, b, c, below));
        Assert.Equal(Orient3D.Evaluate(a, b, c, on), PlaneClassification.Classify(a, b, c, on));
    }
}
