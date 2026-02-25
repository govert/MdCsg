using MdCsg.Math;
using MdCsg.Predicates;

namespace MdCsg.Tests.Predicates;

/// <summary>Phase 6: Orient3D robustness — near-coplanar, large coordinates, adaptive precision paths</summary>
public class Orient3DRobustnessTests
{
    [Fact]
    public void NearCoplanar_SmallPerturbation_DetectsSign()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        // d is barely above the plane
        var d = new Vec3(0.5, 0.5, 1e-14);
        var sign = Orient3D.Evaluate(a, b, c, d);
        // Should detect positive (above plane)
        Assert.Equal(PredicateSign.Positive, sign);
    }

    [Fact]
    public void NearCoplanar_NegativePerturbation_DetectsSign()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0.5, 0.5, -1e-14);
        var sign = Orient3D.Evaluate(a, b, c, d);
        Assert.Equal(PredicateSign.Negative, sign);
    }

    [Fact]
    public void ExactlyCoplanar_Zero()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0.5, 0.5, 0);
        var sign = Orient3D.Evaluate(a, b, c, d);
        Assert.Equal(PredicateSign.Zero, sign);
    }

    [Fact]
    public void LargeCoordinates_StillCorrect()
    {
        var a = new Vec3(1e8, 1e8, 1e8);
        var b = new Vec3(1e8 + 1, 1e8, 1e8);
        var c = new Vec3(1e8, 1e8 + 1, 1e8);
        var d = new Vec3(1e8, 1e8, 1e8 + 1);
        var sign = Orient3D.Evaluate(a, b, c, d);
        Assert.Equal(PredicateSign.Positive, sign);
    }

    [Fact]
    public void SwapTwoPoints_FlipsSign()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0, 0, 1);
        var sign1 = Orient3D.Evaluate(a, b, c, d);
        var sign2 = Orient3D.Evaluate(b, a, c, d);
        Assert.Equal(PredicateSign.Positive, sign1);
        Assert.Equal(PredicateSign.Negative, sign2);
    }

    [Fact]
    public void DegenerateTriangle_PointOnLine_Zero()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(2, 0, 0); // Collinear
        var d = new Vec3(0, 1, 0);
        var sign = Orient3D.Evaluate(a, b, c, d);
        Assert.Equal(PredicateSign.Zero, sign);
    }

    [Fact]
    public void NegativeCoordinates_CorrectSign()
    {
        var a = new Vec3(-1, -1, -1);
        var b = new Vec3(0, -1, -1);
        var c = new Vec3(-1, 0, -1);
        var d = new Vec3(-1, -1, 0);
        var sign = Orient3D.Evaluate(a, b, c, d);
        Assert.Equal(PredicateSign.Positive, sign);
    }

    [Theory]
    [InlineData(1e-15)]
    [InlineData(1e-13)]
    [InlineData(1e-10)]
    public void VerySmallPerturbation_StillDetected(double eps)
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0, 0, eps);
        var sign = Orient3D.Evaluate(a, b, c, d);
        Assert.Equal(PredicateSign.Positive, sign);
    }

    [Fact]
    public void EvenPermutation_SameSign()
    {
        // Even permutation (swap two pairs) preserves sign: Orient3D(a,b,c,d) = Orient3D(c,d,a,b)
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0, 0, 1);
        var s1 = Orient3D.Evaluate(a, b, c, d);
        // Swap (a,b) and (c,d) — this is two transpositions = even permutation
        var s2 = Orient3D.Evaluate(c, d, a, b);
        Assert.Equal(s1, s2);
    }

    [Fact]
    public void AllSamePoint_Zero()
    {
        var p = new Vec3(1, 2, 3);
        var sign = Orient3D.Evaluate(p, p, p, p);
        Assert.Equal(PredicateSign.Zero, sign);
    }

    [Fact]
    public void TwoSamePoints_Zero()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var sign = Orient3D.Evaluate(a, a, b, new Vec3(0, 1, 0));
        Assert.Equal(PredicateSign.Zero, sign);
    }
}
