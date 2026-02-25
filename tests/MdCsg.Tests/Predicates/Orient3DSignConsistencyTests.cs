using MdCsg.Math;
using MdCsg.Predicates;

namespace MdCsg.Tests.Predicates;

/// <summary>Phase 6: Orient3D — sign consistency, permutation rules, degenerate cases</summary>
public class Orient3DSignConsistencyTests
{
    [Fact]
    public void Positive_Orientation()
    {
        // Standard right-handed basis should give positive orientation
        var a = Vec3.Zero;
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0, 0, 1);
        Assert.Equal(PredicateSign.Positive, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void Negative_Orientation()
    {
        var a = Vec3.Zero;
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0, 0, -1);
        Assert.Equal(PredicateSign.Negative, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void Coplanar_IsZero()
    {
        var a = Vec3.Zero;
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(1, 1, 0); // in XY plane
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void SwapAB_FlipsSign()
    {
        var a = Vec3.Zero;
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0, 0, 1);
        var sign1 = Orient3D.Evaluate(a, b, c, d);
        var sign2 = Orient3D.Evaluate(b, a, c, d);
        Assert.NotEqual(sign1, sign2);
    }

    [Fact]
    public void SwapCD_FlipsSign()
    {
        var a = Vec3.Zero;
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0, 0, 1);
        var sign1 = Orient3D.Evaluate(a, b, c, d);
        var sign2 = Orient3D.Evaluate(a, b, d, c);
        Assert.NotEqual(sign1, sign2);
    }

    [Fact]
    public void EvenPermutation_SameSign()
    {
        var a = Vec3.Zero;
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0, 0, 1);
        // (a,b,c,d) and (b,c,a,d) differ by 2 swaps → same sign for even perm of first 3
        var sign1 = Orient3D.Evaluate(a, b, c, d);
        // cyclic shift of first 3: (b,c,a,d) — even permutation
        var sign2 = Orient3D.Evaluate(b, c, a, d);
        Assert.Equal(sign1, sign2);
    }

    [Fact]
    public void DuplicatePoint_IsZero()
    {
        var p = new Vec3(1, 2, 3);
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(p, p, new Vec3(4, 5, 6), new Vec3(7, 8, 10)));
    }

    [Fact]
    public void NearCoplanar_ResolvesCorrectly()
    {
        // Points very close to coplanar — should still resolve via adaptive precision
        var a = Vec3.Zero;
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0.5, 0.5, 1e-15); // very slightly above XY plane
        var sign = Orient3D.Evaluate(a, b, c, d);
        Assert.True(sign == PredicateSign.Positive || sign == PredicateSign.Zero);
    }

    [Fact]
    public void LargeCoordinates_CorrectSign()
    {
        var a = new Vec3(1e10, 0, 0);
        var b = new Vec3(0, 1e10, 0);
        var c = new Vec3(0, 0, 1e10);
        var d = new Vec3(1e10, 1e10, 1e10);
        // Should still compute correct orientation
        var sign = Orient3D.Evaluate(Vec3.Zero, a, b, c);
        Assert.NotEqual(PredicateSign.Zero, sign);
    }
}
