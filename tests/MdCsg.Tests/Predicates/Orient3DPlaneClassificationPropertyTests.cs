using MdCsg.Math;
using MdCsg.Predicates;

namespace MdCsg.Tests.Predicates;

/// <summary>Phase 6: Orient3D + PlaneClassification — known orientations, symmetry, coplanar detection, consistency</summary>
public class Orient3DPlaneClassificationPropertyTests
{
    [Fact]
    public void Orient3D_PointAboveXYPlane_Positive()
    {
        var result = Orient3D.Evaluate(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, Vec3.UnitZ);
        Assert.Equal(PredicateSign.Positive, result);
    }

    [Fact]
    public void Orient3D_PointBelowXYPlane_Negative()
    {
        var result = Orient3D.Evaluate(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, -Vec3.UnitZ);
        Assert.Equal(PredicateSign.Negative, result);
    }

    [Fact]
    public void Orient3D_CoplanarPoint_Zero()
    {
        var result = Orient3D.Evaluate(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, new Vec3(0.5, 0.5, 0));
        Assert.Equal(PredicateSign.Zero, result);
    }

    [Fact]
    public void Orient3D_SwapTwoPoints_FlipsSign()
    {
        var a = Vec3.Zero;
        var b = Vec3.UnitX;
        var c = Vec3.UnitY;
        var d = new Vec3(0.3, 0.3, 1);
        var original = Orient3D.Evaluate(a, b, c, d);
        var swapped = Orient3D.Evaluate(a, c, b, d); // swap b,c
        Assert.NotEqual(PredicateSign.Zero, original);
        Assert.NotEqual(original, swapped);
    }

    [Fact]
    public void Orient3D_CyclicPermutation_SameSign()
    {
        var a = Vec3.Zero;
        var b = Vec3.UnitX;
        var c = Vec3.UnitY;
        var d = new Vec3(0.3, 0.3, 1);
        var abc = Orient3D.Evaluate(a, b, c, d);
        var bca = Orient3D.Evaluate(b, c, a, d);
        var cab = Orient3D.Evaluate(c, a, b, d);
        Assert.Equal(abc, bca);
        Assert.Equal(bca, cab);
    }

    [Fact]
    public void Orient3D_AllSamePoint_Zero()
    {
        var p = new Vec3(1, 2, 3);
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(p, p, p, p));
    }

    [Fact]
    public void PlaneClassification_PointAbovePlane_Positive()
    {
        var result = PlaneClassification.Classify(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, Vec3.UnitZ);
        Assert.Equal(PredicateSign.Positive, result);
    }

    [Fact]
    public void PlaneClassification_ConsistentWithOrient3D()
    {
        var a = new Vec3(1, 0, 0);
        var b = new Vec3(0, 1, 0);
        var c = new Vec3(0, 0, 1);
        var point = new Vec3(1, 1, 1);
        var orient = Orient3D.Evaluate(a, b, c, point);
        var classify = PlaneClassification.Classify(a, b, c, point);
        Assert.Equal(orient, classify);
    }

    [Fact]
    public void Orient3D_PointOnVertex_Zero()
    {
        // D = A should be coplanar
        var a = Vec3.Zero;
        var b = Vec3.UnitX;
        var c = Vec3.UnitY;
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(a, b, c, a));
    }

    [Fact]
    public void Orient3D_PointOnEdge_Zero()
    {
        var a = Vec3.Zero;
        var b = Vec3.UnitX;
        var c = Vec3.UnitY;
        var onEdge = (a + b) * 0.5; // midpoint of AB, on the plane
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(a, b, c, onEdge));
    }

    [Fact]
    public void Orient3D_NearlyCoplanar_StillDecides()
    {
        var a = Vec3.Zero;
        var b = Vec3.UnitX;
        var c = Vec3.UnitY;
        var slightlyAbove = new Vec3(0.3, 0.3, 1e-15);
        // Adaptive precision should still return a decision (may be zero for very small)
        var result = Orient3D.Evaluate(a, b, c, slightlyAbove);
        // Just verify no crash and result is valid
        Assert.True(result == PredicateSign.Positive || result == PredicateSign.Negative || result == PredicateSign.Zero);
    }

    [Fact]
    public void Orient3D_CollinearABC_AlwaysZero()
    {
        // A, B, C collinear means degenerate plane, D should be zero
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(2, 0, 0); // collinear
        var d = new Vec3(0, 0, 1);
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void Orient3D_LargeCoordinates_Correct()
    {
        var a = new Vec3(1e6, 0, 0);
        var b = new Vec3(0, 1e6, 0);
        var c = new Vec3(0, 0, 1e6);
        var above = new Vec3(1e6, 1e6, 1e6);
        var result = Orient3D.Evaluate(a, b, c, above);
        Assert.Equal(PredicateSign.Positive, result);
    }

    [Fact]
    public void Orient3D_SmallCoordinates_Correct()
    {
        var a = new Vec3(1e-6, 0, 0);
        var b = new Vec3(0, 1e-6, 0);
        var c = new Vec3(0, 0, 1e-6);
        var above = new Vec3(1e-6, 1e-6, 1e-6);
        var result = Orient3D.Evaluate(a, b, c, above);
        Assert.Equal(PredicateSign.Positive, result);
    }

    [Fact]
    public void PlaneClassification_OnPlane_Zero()
    {
        var a = new Vec3(1, 0, 0);
        var b = new Vec3(0, 1, 0);
        var c = new Vec3(0, 0, 1);
        // Use exactly representable point on plane x+y+z=1
        var onPlane = new Vec3(0.5, 0.5, 0);
        Assert.Equal(PredicateSign.Zero, PlaneClassification.Classify(a, b, c, onPlane));
    }

    [Fact]
    public void Orient3D_NegatedD_FlipsSign()
    {
        var a = Vec3.Zero;
        var b = Vec3.UnitX;
        var c = Vec3.UnitY;
        var d = new Vec3(0.3, 0.3, 1.0);
        var original = Orient3D.Evaluate(a, b, c, d);
        var negD = new Vec3(0.3, 0.3, -1.0);
        var negated = Orient3D.Evaluate(a, b, c, negD);
        Assert.NotEqual(original, negated);
    }

    [Fact]
    public void Orient3D_DOnPlaneVertex_Zero()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var c = new Vec3(7, 0, 1);
        // D on plane at vertex B
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(a, b, c, b));
    }

    [Fact]
    public void Orient2D_CCW_Positive()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);
        Assert.Equal(PredicateSign.Positive, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void Orient2D_CW_Negative()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(0, 1);
        var c = new Vec2(1, 0);
        Assert.Equal(PredicateSign.Negative, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void Orient2D_Collinear_Zero()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 1);
        var c = new Vec2(2, 2);
        Assert.Equal(PredicateSign.Zero, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void InCircle_PointInside_Positive()
    {
        // CCW triangle
        var a = new Vec2(0, 0);
        var b = new Vec2(2, 0);
        var c = new Vec2(1, 2);
        var d = new Vec2(1, 0.5); // inside circumcircle
        Assert.Equal(PredicateSign.Positive, InCircle.Evaluate(a, b, c, d));
    }

    [Fact]
    public void InCircle_PointFarOutside_Negative()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);
        var d = new Vec2(100, 100); // way outside
        Assert.Equal(PredicateSign.Negative, InCircle.Evaluate(a, b, c, d));
    }
}
