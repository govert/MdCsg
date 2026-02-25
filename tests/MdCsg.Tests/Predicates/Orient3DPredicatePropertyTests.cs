using MdCsg.Math;
using MdCsg.Predicates;

namespace MdCsg.Tests.Predicates;

/// <summary>Phase 6: Orient3D predicate — sign determination, anti-symmetry, coplanarity, precision</summary>
public class Orient3DPredicatePropertyTests
{
    [Fact]
    public void Evaluate_AbovePlane_Positive()
    {
        var sign = Orient3D.Evaluate(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0),
            new Vec3(0, 0, 1));
        Assert.Equal(PredicateSign.Positive, sign);
    }

    [Fact]
    public void Evaluate_BelowPlane_Negative()
    {
        var sign = Orient3D.Evaluate(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0),
            new Vec3(0, 0, -1));
        Assert.Equal(PredicateSign.Negative, sign);
    }

    [Fact]
    public void Evaluate_OnPlane_Zero()
    {
        var sign = Orient3D.Evaluate(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0),
            new Vec3(0.5, 0.5, 0));
        Assert.Equal(PredicateSign.Zero, sign);
    }

    [Fact]
    public void Evaluate_VertexA_IsZero()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var c = new Vec3(7, 8, 10);
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(a, b, c, a));
    }

    [Fact]
    public void Evaluate_VertexB_IsZero()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var c = new Vec3(7, 8, 10);
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(a, b, c, b));
    }

    [Fact]
    public void Evaluate_VertexC_IsZero()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var c = new Vec3(7, 8, 10);
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(a, b, c, c));
    }

    [Fact]
    public void Evaluate_SwappingBC_FlipsSign()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0, 0, 1);
        var sign1 = Orient3D.Evaluate(a, b, c, d);
        var sign2 = Orient3D.Evaluate(a, c, b, d);
        Assert.True((int)sign1 == -(int)sign2,
            $"Swapping B and C should flip sign: {sign1} vs {sign2}");
    }

    [Fact]
    public void Evaluate_SwappingAB_FlipsSign()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0, 0, 1);
        var sign1 = Orient3D.Evaluate(a, b, c, d);
        var sign2 = Orient3D.Evaluate(b, a, c, d);
        Assert.True((int)sign1 == -(int)sign2);
    }

    [Fact]
    public void Evaluate_EvenPermutation_PreservesSign()
    {
        // Orient3D(a,b,c,d) = det(B-A, C-A, D-A)
        // Swapping two pairs (even permutation) should preserve sign
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0, 0, 1);
        var sign1 = Orient3D.Evaluate(a, b, c, d);
        // Swap (a,b) and (c,d) — double swap is even permutation
        var sign2 = Orient3D.Evaluate(b, a, d, c);
        Assert.Equal(sign1, sign2);
    }

    [Fact]
    public void Evaluate_LargeCoordinates_CorrectSign()
    {
        var offset = new Vec3(1e8, 1e8, 1e8);
        var a = offset + new Vec3(0, 0, 0);
        var b = offset + new Vec3(1, 0, 0);
        var c = offset + new Vec3(0, 1, 0);
        var above = offset + new Vec3(0, 0, 1e-6);
        var sign = Orient3D.Evaluate(a, b, c, above);
        Assert.Equal(PredicateSign.Positive, sign);
    }

    [Fact]
    public void Evaluate_NearlyCoplanar_CorrectSign()
    {
        // Points nearly coplanar but not quite
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0.5, 0.5, 1e-12);
        var sign = Orient3D.Evaluate(a, b, c, d);
        Assert.Equal(PredicateSign.Positive, sign);
    }

    [Fact]
    public void Evaluate_ExactlyCoplanar_LargeCoords_IsZero()
    {
        var a = new Vec3(1e6, 0, 0);
        var b = new Vec3(0, 1e6, 0);
        var c = new Vec3(0, 0, 0);
        var d = new Vec3(5e5, 5e5, 0); // On XY plane
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void Evaluate_AllSamePoint_Zero()
    {
        var p = new Vec3(1, 2, 3);
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(p, p, p, p));
    }

    [Fact]
    public void Evaluate_ThreeCollinear_Zero()
    {
        // Three collinear points form a degenerate plane
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(2, 0, 0); // collinear with a and b
        var d = new Vec3(0, 0, 1);
        // det(B-A, C-A, D-A) = det((1,0,0), (2,0,0), (0,0,1)) = 0 (rows 1,2 proportional)
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(a, b, c, d));
    }
}
