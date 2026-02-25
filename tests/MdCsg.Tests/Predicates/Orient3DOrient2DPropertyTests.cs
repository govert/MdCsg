using MdCsg.Math;
using MdCsg.Predicates;

namespace MdCsg.Tests.Predicates;

/// <summary>Phase 6: Orient3D and Orient2D — sign correctness, collinear/coplanar, symmetry, degenerate inputs</summary>
public class Orient3DOrient2DPropertyTests
{
    // --- Orient3D ---

    [Fact]
    public void Orient3D_PointAbovePlane_Positive()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0, 0, 1); // above XY plane
        Assert.Equal(PredicateSign.Positive, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void Orient3D_PointBelowPlane_Negative()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0, 0, -1); // below XY plane
        Assert.Equal(PredicateSign.Negative, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void Orient3D_PointOnPlane_Zero()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0.5, 0.5, 0); // on XY plane
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void Orient3D_SwapBC_FlipsSign()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0, 0, 1);
        var s1 = Orient3D.Evaluate(a, b, c, d);
        var s2 = Orient3D.Evaluate(a, c, b, d);
        Assert.NotEqual(s1, s2);
    }

    [Fact]
    public void Orient3D_IdenticalPoints_Zero()
    {
        var p = new Vec3(1, 2, 3);
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(p, p, p, p));
    }

    [Fact]
    public void Orient3D_CollinearABC_Zero()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(2, 0, 0);
        var d = new Vec3(0, 0, 1);
        // a,b,c collinear → degenerate plane → zero determinant for any d
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void Orient3D_LargeCoordinates_StillCorrect()
    {
        var a = new Vec3(1e10, 0, 0);
        var b = new Vec3(1e10 + 1, 0, 0);
        var c = new Vec3(1e10, 1, 0);
        var d = new Vec3(1e10, 0, 1);
        Assert.Equal(PredicateSign.Positive, Orient3D.Evaluate(a, b, c, d));
    }

    // --- Orient2D ---

    [Fact]
    public void Orient2D_LeftTurn_Positive()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1); // left of A→B
        Assert.Equal(PredicateSign.Positive, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void Orient2D_RightTurn_Negative()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, -1); // right of A→B
        Assert.Equal(PredicateSign.Negative, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void Orient2D_Collinear_Zero()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(2, 0); // on the line
        Assert.Equal(PredicateSign.Zero, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void Orient2D_SwapAB_FlipsSign()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);
        var s1 = Orient2D.Evaluate(a, b, c);
        var s2 = Orient2D.Evaluate(b, a, c);
        Assert.NotEqual(s1, s2);
    }

    [Fact]
    public void Orient2D_IdenticalPoints_Zero()
    {
        var p = new Vec2(5, 5);
        Assert.Equal(PredicateSign.Zero, Orient2D.Evaluate(p, p, p));
    }

    [Fact]
    public void Orient2D_LargeCoordinates_StillCorrect()
    {
        var a = new Vec2(1e15, 0);
        var b = new Vec2(1e15 + 1, 0);
        var c = new Vec2(1e15, 1);
        Assert.Equal(PredicateSign.Positive, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void Orient2D_NearCollinear_Robust()
    {
        // Points almost collinear — needs adaptive precision
        var a = new Vec2(0, 0);
        var b = new Vec2(1e-8, 1e-8);
        var c = new Vec2(2e-8, 2e-8);
        Assert.Equal(PredicateSign.Zero, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void PredicateSign_EnumValues()
    {
        Assert.Equal(-1, (int)PredicateSign.Negative);
        Assert.Equal(0, (int)PredicateSign.Zero);
        Assert.Equal(1, (int)PredicateSign.Positive);
    }
}
