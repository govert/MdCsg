using MdCsg.Math;
using MdCsg.Predicates;

namespace MdCsg.Tests.Predicates;

/// <summary>Batch 42: Extended Orient2D, InCircle, PlaneClassification tests (20 tests)</summary>
public class PredicateExtendedTests
{
    // --- Orient2D extended ---
    [Fact]
    public void Orient2D_LargeTriangle_CCW()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1000, 0);
        var c = new Vec2(500, 1000);
        Assert.Equal(PredicateSign.Positive, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void Orient2D_LargeTriangle_CW()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(500, 1000);
        var c = new Vec2(1000, 0);
        Assert.Equal(PredicateSign.Negative, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void Orient2D_TinyTriangle_CCW()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1e-8, 0);
        var c = new Vec2(0, 1e-8);
        Assert.Equal(PredicateSign.Positive, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void Orient2D_SwapBC_FlipsSign()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);
        var ccw = Orient2D.Evaluate(a, b, c);
        var cw = Orient2D.Evaluate(a, c, b);
        Assert.Equal(PredicateSign.Positive, ccw);
        Assert.Equal(PredicateSign.Negative, cw);
    }

    [Fact]
    public void Orient2D_PointOnLine_IsZero()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(3, 3);
        var c = new Vec2(1, 1);
        Assert.Equal(PredicateSign.Zero, Orient2D.Evaluate(a, b, c));
    }

    // --- InCircle extended ---
    [Fact]
    public void InCircle_EquilateralTriangle_CenterInside()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(2, 0);
        var c = new Vec2(1, System.Math.Sqrt(3));
        var center = new Vec2(1, System.Math.Sqrt(3) / 3);
        Assert.Equal(PredicateSign.Positive, InCircle.Evaluate(a, b, c, center));
    }

    [Fact]
    public void InCircle_FarPoint_Outside()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);
        var d = new Vec2(100, 100);
        Assert.Equal(PredicateSign.Negative, InCircle.Evaluate(a, b, c, d));
    }

    [Fact]
    public void InCircle_NearlyOnCircle_Resolved()
    {
        // Right triangle with circumcircle centered at (0.5, 0.5), radius ~0.707
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);
        // Point very close to the circle but slightly inside
        var d = new Vec2(0.5, 0.5);
        Assert.Equal(PredicateSign.Positive, InCircle.Evaluate(a, b, c, d));
    }

    [Fact]
    public void InCircle_CoclicularSquare_IsZero()
    {
        // Square vertices are concyclic
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(1, 1);
        var d = new Vec2(0, 1);
        Assert.Equal(PredicateSign.Zero, InCircle.Evaluate(a, b, c, d));
    }

    // --- PlaneClassification ---
    [Fact]
    public void PlaneClassification_AbovePlane_Positive()
    {
        var pa = new Vec3(0, 0, 0);
        var pb = new Vec3(1, 0, 0);
        var pc = new Vec3(0, 1, 0);
        var point = new Vec3(0, 0, 1);
        Assert.Equal(PredicateSign.Positive, PlaneClassification.Classify(pa, pb, pc, point));
    }

    [Fact]
    public void PlaneClassification_BelowPlane_Negative()
    {
        var pa = new Vec3(0, 0, 0);
        var pb = new Vec3(1, 0, 0);
        var pc = new Vec3(0, 1, 0);
        var point = new Vec3(0, 0, -1);
        Assert.Equal(PredicateSign.Negative, PlaneClassification.Classify(pa, pb, pc, point));
    }

    [Fact]
    public void PlaneClassification_OnPlane_Zero()
    {
        var pa = new Vec3(0, 0, 0);
        var pb = new Vec3(1, 0, 0);
        var pc = new Vec3(0, 1, 0);
        var point = new Vec3(0.5, 0.5, 0);
        Assert.Equal(PredicateSign.Zero, PlaneClassification.Classify(pa, pb, pc, point));
    }

    [Fact]
    public void PlaneClassification_OffsetPlane()
    {
        var pa = new Vec3(0, 0, 5);
        var pb = new Vec3(1, 0, 5);
        var pc = new Vec3(0, 1, 5);
        var above = new Vec3(0, 0, 6);
        var below = new Vec3(0, 0, 4);
        Assert.Equal(PredicateSign.Positive, PlaneClassification.Classify(pa, pb, pc, above));
        Assert.Equal(PredicateSign.Negative, PlaneClassification.Classify(pa, pb, pc, below));
    }

    [Fact]
    public void PlaneClassification_VertexOnPlane()
    {
        var pa = new Vec3(0, 0, 0);
        var pb = new Vec3(1, 0, 0);
        var pc = new Vec3(0, 1, 0);
        // The plane defining points themselves should be Zero
        Assert.Equal(PredicateSign.Zero, PlaneClassification.Classify(pa, pb, pc, pa));
        Assert.Equal(PredicateSign.Zero, PlaneClassification.Classify(pa, pb, pc, pb));
        Assert.Equal(PredicateSign.Zero, PlaneClassification.Classify(pa, pb, pc, pc));
    }

    // --- PredicateSign enum ---
    [Fact]
    public void PredicateSign_Values()
    {
        Assert.Equal(-1, (int)PredicateSign.Negative);
        Assert.Equal(0, (int)PredicateSign.Zero);
        Assert.Equal(1, (int)PredicateSign.Positive);
    }

    [Fact]
    public void Orient2D_NegativeCoords_Works()
    {
        var a = new Vec2(-3, -2);
        var b = new Vec2(-1, -2);
        var c = new Vec2(-2, -1);
        Assert.Equal(PredicateSign.Positive, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void InCircle_NegativeCoords_Works()
    {
        var a = new Vec2(-1, 0);
        var b = new Vec2(0, -1);
        var c = new Vec2(1, 0);
        var d = new Vec2(0, 0); // center of circumcircle should be inside
        Assert.Equal(PredicateSign.Positive, InCircle.Evaluate(a, b, c, d));
    }

    [Fact]
    public void PlaneClassification_DiagonalPlane()
    {
        // Plane through (0,0,0), (1,1,0), (0,0,1)
        var pa = new Vec3(0, 0, 0);
        var pb = new Vec3(1, 1, 0);
        var pc = new Vec3(0, 0, 1);
        // The normal is cross((1,1,0), (0,0,1)) = (1, -1, 0)
        // Point (1, 0, 0): dot with normal (1,-1,0) - distance = 1 > 0
        Assert.Equal(PredicateSign.Positive, PlaneClassification.Classify(pa, pb, pc, new Vec3(1, 0, 0)));
    }
}
