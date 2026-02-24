using MdCsg.Math;
using MdCsg.Predicates;

namespace MdCsg.Tests.Predicates;

/// <summary>Batch 7: Orient3D extended tests (20 tests)</summary>
public class Orient3DExtTests
{
    [Fact]
    public void PointAbovePlane_XY()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0, 0, 1);
        Assert.Equal(PredicateSign.Positive, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void PointBelowPlane_XY()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0, 0, -1);
        Assert.Equal(PredicateSign.Negative, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void CoplanarPoint_XY()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0.5, 0.5, 0);
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void PointAbovePlane_YZ()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(0, 1, 0);
        var c = new Vec3(0, 0, 1);
        var d = new Vec3(1, 0, 0);
        Assert.Equal(PredicateSign.Positive, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void PointAbovePlane_XZ()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(0, 0, 1);
        var c = new Vec3(1, 0, 0);
        var d = new Vec3(0, 1, 0);
        Assert.Equal(PredicateSign.Positive, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void AllSamePoint_IsZero()
    {
        var p = new Vec3(5, 5, 5);
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(p, p, p, p));
    }

    [Fact]
    public void ThreeOfFourSame_IsZero()
    {
        var p = new Vec3(1, 2, 3);
        var q = new Vec3(4, 5, 6);
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(p, p, q, q));
    }

    [Fact]
    public void CyclicPermutation_SameSign()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0, 0, 1);
        var s1 = Orient3D.Evaluate(a, b, c, d);
        var s2 = Orient3D.Evaluate(b, c, a, d);
        // cyclic permutation of first 3 preserves sign
        Assert.Equal(s1, s2);
    }

    [Fact]
    public void SwapTwo_FlipsSign()
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
    public void LargeCoordinates()
    {
        double big = 1e10;
        var a = new Vec3(big, big, big);
        var b = new Vec3(big + 1, big, big);
        var c = new Vec3(big, big + 1, big);
        var d = new Vec3(big, big, big + 1);
        Assert.Equal(PredicateSign.Positive, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void TinyTetrahedron()
    {
        double eps = 1e-12;
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(eps, 0, 0);
        var c = new Vec3(0, eps, 0);
        var d = new Vec3(0, 0, eps);
        Assert.Equal(PredicateSign.Positive, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void NegativeCoordinates()
    {
        var a = new Vec3(-1, -1, -1);
        var b = new Vec3(0, -1, -1);
        var c = new Vec3(-1, 0, -1);
        var d = new Vec3(-1, -1, 0);
        Assert.Equal(PredicateSign.Positive, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void OffsetPlane_PointAbove()
    {
        var a = new Vec3(0, 0, 5);
        var b = new Vec3(1, 0, 5);
        var c = new Vec3(0, 1, 5);
        var d = new Vec3(0, 0, 6);
        Assert.Equal(PredicateSign.Positive, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void OffsetPlane_PointBelow()
    {
        var a = new Vec3(0, 0, 5);
        var b = new Vec3(1, 0, 5);
        var c = new Vec3(0, 1, 5);
        var d = new Vec3(0, 0, 4);
        Assert.Equal(PredicateSign.Negative, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void FourCollinearPoints()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(2, 0, 0);
        var d = new Vec3(3, 0, 0);
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void FourCoplanarPoints_InXYPlane()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(1, 1, 0);
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void RegularTetrahedron()
    {
        var a = new Vec3(1, 1, 1);
        var b = new Vec3(1, -1, -1);
        var c = new Vec3(-1, 1, -1);
        var d = new Vec3(-1, -1, 1);
        var sign = Orient3D.Evaluate(a, b, c, d);
        Assert.NotEqual(PredicateSign.Zero, sign);
    }

    [Fact]
    public void NearCoplanar_Positive()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0.5, 0.5, 1e-14);
        Assert.Equal(PredicateSign.Positive, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void NearCoplanar_Negative()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0.5, 0.5, -1e-14);
        Assert.Equal(PredicateSign.Negative, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void MixedLargeAndTiny()
    {
        var a = new Vec3(1e8, 1e8, 1e8);
        var b = new Vec3(1e8 + 1, 1e8, 1e8);
        var c = new Vec3(1e8, 1e8 + 1, 1e8);
        var d = new Vec3(1e8, 1e8, 1e8 + 1e-6);
        Assert.Equal(PredicateSign.Positive, Orient3D.Evaluate(a, b, c, d));
    }
}
