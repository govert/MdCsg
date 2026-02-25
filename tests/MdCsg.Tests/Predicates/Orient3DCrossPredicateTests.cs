using MdCsg.Math;
using MdCsg.Predicates;

namespace MdCsg.Tests.Predicates;

/// <summary>Phase 6: Orient3D cross-predicate — symmetry, sign flips, large coordinates, consistency</summary>
public class Orient3DCrossPredicateTests
{
    [Fact]
    public void PointAbovePlane_IsPositive()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0, 0, 1);
        Assert.Equal(PredicateSign.Positive, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void PointBelowPlane_IsNegative()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0, 0, -1);
        Assert.Equal(PredicateSign.Negative, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void PointOnPlane_IsZero()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0.5, 0.5, 0);
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void EvenPermutation_PreservesSign()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0, 0, 1);
        var s1 = Orient3D.Evaluate(a, b, c, d);
        // Double cyclic shift is an even permutation (2 transpositions)
        var s2 = Orient3D.Evaluate(c, d, a, b);
        Assert.Equal(s1, s2);
        // Swap first pair AND last pair: also even
        var s3 = Orient3D.Evaluate(b, a, d, c);
        Assert.Equal(s1, s3);
    }

    [Fact]
    public void SwapTwoPoints_FlipsSign()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0, 0, 1);
        var s1 = Orient3D.Evaluate(a, b, c, d);
        var s2 = Orient3D.Evaluate(b, a, c, d);
        Assert.NotEqual(s1, s2);
    }

    [Fact]
    public void AllSamePoint_IsZero()
    {
        var p = new Vec3(5, 5, 5);
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(p, p, p, p));
    }

    [Fact]
    public void ThreeCoplanarPoints_IsZero()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(1, 1, 0); // on same plane
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void LargeCoordinates_StillCorrect()
    {
        double L = 1e10;
        var a = new Vec3(L, 0, 0);
        var b = new Vec3(0, L, 0);
        var c = new Vec3(0, 0, L);
        var d = Vec3.Zero;
        // det(b-a, c-a, d-a) — should be negative (d is "below")
        var sign = Orient3D.Evaluate(a, b, c, d);
        Assert.NotEqual(PredicateSign.Zero, sign);
    }

    [Fact]
    public void NearlyCoplanar_SmallPerturbation()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0.5, 0.5, 1e-14);
        Assert.Equal(PredicateSign.Positive, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void NearlyCoplanar_NegativePerturbation()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0.5, 0.5, -1e-14);
        Assert.Equal(PredicateSign.Negative, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void RandomPoints_NonZero()
    {
        var rng = new System.Random(42);
        int zeroCount = 0;
        for (int i = 0; i < 50; i++)
        {
            var a = new Vec3(rng.NextDouble(), rng.NextDouble(), rng.NextDouble());
            var b = new Vec3(rng.NextDouble(), rng.NextDouble(), rng.NextDouble());
            var c = new Vec3(rng.NextDouble(), rng.NextDouble(), rng.NextDouble());
            var d = new Vec3(rng.NextDouble(), rng.NextDouble(), rng.NextDouble());
            var sign = Orient3D.Evaluate(a, b, c, d);
            if (sign == PredicateSign.Zero) zeroCount++;
        }
        // Random points are almost never coplanar
        Assert.True(zeroCount <= 2);
    }
}
