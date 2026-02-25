using MdCsg.Math;
using MdCsg.Predicates;

namespace MdCsg.Tests.Predicates;

/// <summary>Phase 6: Orient2D/Orient3D consistency — 2D is special case of 3D, sign properties</summary>
public class Orient2DOrient3DConsistencyTests
{
    [Fact]
    public void Orient2D_CCW_IsPositive()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);
        Assert.Equal(PredicateSign.Positive, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void Orient2D_CW_IsNegative()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(0, 1);
        var c = new Vec2(1, 0);
        Assert.Equal(PredicateSign.Negative, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void Orient2D_Collinear_IsZero()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(2, 0);
        Assert.Equal(PredicateSign.Zero, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void Orient3D_Positive_Tetrahedron()
    {
        // Standard right-handed tetrahedron
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0, 0, 1); // above ABC plane
        Assert.Equal(PredicateSign.Positive, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void Orient3D_Negative_BelowPlane()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0, 0, -1); // below ABC plane
        Assert.Equal(PredicateSign.Negative, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void Orient3D_Coplanar_IsZero()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(1, 1, 0); // on the same plane
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void Orient3D_TransposeFirstTwo_FlipsSign()
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
    public void Orient2D_TransposeFirstTwo_FlipsSign()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);

        var s1 = Orient2D.Evaluate(a, b, c);
        var s2 = Orient2D.Evaluate(b, a, c);
        Assert.NotEqual(s1, s2);
    }

    [Fact]
    public void Orient3D_LargeCoordinates_Correct()
    {
        double big = 1e12;
        var a = new Vec3(big, 0, 0);
        var b = new Vec3(0, big, 0);
        var c = new Vec3(0, 0, big);
        var d = new Vec3(big, big, big); // above the plane through a,b,c

        // Determinant = big^3 * det(I-permutation stuff)
        var sign = Orient3D.Evaluate(a, b, c, d);
        Assert.NotEqual(PredicateSign.Zero, sign);
    }

    [Fact]
    public void Orient2D_NearCollinear_Correct()
    {
        // Points very close to collinear — tests adaptive precision
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0.5, 1e-15); // slightly above the line

        var sign = Orient2D.Evaluate(a, b, c);
        Assert.Equal(PredicateSign.Positive, sign);
    }

    [Fact]
    public void Orient3D_NearCoplanar_Correct()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0.5, 0.5, 1e-15); // barely above the plane

        var sign = Orient3D.Evaluate(a, b, c, d);
        Assert.Equal(PredicateSign.Positive, sign);
    }

    [Fact]
    public void Orient2D_AllSamePoint_IsZero()
    {
        var p = new Vec2(3.14, 2.72);
        Assert.Equal(PredicateSign.Zero, Orient2D.Evaluate(p, p, p));
    }

    [Fact]
    public void Orient3D_AllSamePoint_IsZero()
    {
        var p = new Vec3(3.14, 2.72, 1.41);
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(p, p, p, p));
    }

    [Fact]
    public void Orient2D_DoubleSwap_PreservesSign()
    {
        // Swapping two pairs is an even permutation → sign preserved
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);

        var s1 = Orient2D.Evaluate(a, b, c);
        // Cyclic: (b,c,a) — one cyclic shift of 3 elements = 2 transpositions = even
        var s2 = Orient2D.Evaluate(b, c, a);
        Assert.Equal(s1, s2);
    }

    [Fact]
    public void Orient3D_DoubleTranspose_PreservesSign()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0, 0, 1);

        var s1 = Orient3D.Evaluate(a, b, c, d);
        // Double transposition: swap (a,b) and (c,d)
        var s2 = Orient3D.Evaluate(b, a, d, c);
        Assert.Equal(s1, s2);
    }
}
