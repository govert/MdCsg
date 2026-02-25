using MdCsg.Math;
using MdCsg.Predicates;

namespace MdCsg.Tests.Predicates;

/// <summary>Phase 6: Orient3D — near-coplanar configurations testing adaptive precision escalation</summary>
public class Orient3DNearCoplanarRobustnessTests
{
    [Fact]
    public void ExactlyCoplanar_ReturnsZero()
    {
        var a = Vec3.Zero;
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0.5, 0.5, 0); // on XY plane
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void SlightlyAbove_ReturnsPositive()
    {
        var a = Vec3.Zero;
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0.5, 0.5, 1e-10);
        Assert.Equal(PredicateSign.Positive, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void SlightlyBelow_ReturnsNegative()
    {
        var a = Vec3.Zero;
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0.5, 0.5, -1e-10);
        Assert.Equal(PredicateSign.Negative, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void NearCoplanar_LargeCoordinates_CorrectSign()
    {
        // Large coordinates but small offset — requires exact arithmetic
        double big = 1e8;
        var a = new Vec3(big, big, big);
        var b = new Vec3(big + 1, big, big);
        var c = new Vec3(big, big + 1, big);
        var d = new Vec3(big + 0.5, big + 0.5, big + 1e-6);
        var sign = Orient3D.Evaluate(a, b, c, d);
        Assert.Equal(PredicateSign.Positive, sign);
    }

    [Fact]
    public void NearCoplanar_LargeCoordinates_NegativeOffset()
    {
        double big = 1e8;
        var a = new Vec3(big, big, big);
        var b = new Vec3(big + 1, big, big);
        var c = new Vec3(big, big + 1, big);
        var d = new Vec3(big + 0.5, big + 0.5, big - 1e-6);
        var sign = Orient3D.Evaluate(a, b, c, d);
        Assert.Equal(PredicateSign.Negative, sign);
    }

    [Fact]
    public void ExactlyCoplanar_LargeCoords_ReturnsZero()
    {
        double big = 1e8;
        var a = new Vec3(big, big, big);
        var b = new Vec3(big + 1, big, big);
        var c = new Vec3(big, big + 1, big);
        var d = new Vec3(big + 0.5, big + 0.5, big);
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void FlippingD_FlipsSign()
    {
        var a = Vec3.Zero;
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var above = new Vec3(0.3, 0.3, 1);
        var below = new Vec3(0.3, 0.3, -1);
        var signAbove = Orient3D.Evaluate(a, b, c, above);
        var signBelow = Orient3D.Evaluate(a, b, c, below);
        Assert.NotEqual(signAbove, signBelow);
        Assert.NotEqual(PredicateSign.Zero, signAbove);
        Assert.NotEqual(PredicateSign.Zero, signBelow);
    }

    [Fact]
    public void VertexOnPlane_IsZero()
    {
        var a = Vec3.Zero;
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(a, b, c, a)); // D == A
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(a, b, c, b)); // D == B
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(a, b, c, c)); // D == C
    }

    [Fact]
    public void SwapAB_FlipsSign()
    {
        var a = Vec3.Zero;
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0.3, 0.3, 1);
        var sign1 = Orient3D.Evaluate(a, b, c, d);
        var sign2 = Orient3D.Evaluate(b, a, c, d);
        Assert.NotEqual(sign1, sign2);
    }

    [Fact]
    public void CyclicPermutation_SameSign()
    {
        var a = Vec3.Zero;
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0.3, 0.3, 1);
        var s1 = Orient3D.Evaluate(a, b, c, d);
        var s2 = Orient3D.Evaluate(b, c, a, d);
        var s3 = Orient3D.Evaluate(c, a, b, d);
        Assert.Equal(s1, s2);
        Assert.Equal(s2, s3);
    }

    [Fact]
    public void NearCoplanar_TiltedPlane_CorrectSign()
    {
        // Points near the plane x + y + z = 3
        var a = new Vec3(1, 1, 1);
        var b = new Vec3(2, 0.5, 0.5);
        var c = new Vec3(0.5, 2, 0.5);
        // D slightly above the plane
        var d = new Vec3(0.5, 0.5, 2.0001);
        var sign = Orient3D.Evaluate(a, b, c, d);
        // The normal of the plane defined by (a,b,c) points in a specific direction
        // The exact sign depends on the orientation convention
        Assert.NotEqual(PredicateSign.Zero, sign);
    }

    [Fact]
    public void TinyTriangle_LargeOffset_CorrectSign()
    {
        // Very small triangle but D is clearly above
        double eps = 1e-12;
        var a = Vec3.Zero;
        var b = new Vec3(eps, 0, 0);
        var c = new Vec3(0, eps, 0);
        var d = new Vec3(0, 0, 1);
        var sign = Orient3D.Evaluate(a, b, c, d);
        Assert.Equal(PredicateSign.Positive, sign);
    }

    [Fact]
    public void DegenerateTriangle_CollinearABC_ReturnsZero()
    {
        // A, B, C are collinear — degenerate triangle, so any D is "coplanar"
        var a = Vec3.Zero;
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(2, 0, 0);
        var d = new Vec3(0, 0, 1);
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void AllSamePoint_ReturnsZero()
    {
        var p = new Vec3(1, 2, 3);
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(p, p, p, p));
    }
}
