using MdCsg.Math;
using MdCsg.Predicates;

namespace MdCsg.Tests.Predicates;

/// <summary>Phase 6: PlaneClassification wrapper tests — point vs plane classification</summary>
public class PlaneClassificationTests
{
    [Fact]
    public void PointAbove_IsPositive()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        Assert.Equal(PredicateSign.Positive, PlaneClassification.Classify(a, b, c, new Vec3(0, 0, 1)));
    }

    [Fact]
    public void PointBelow_IsNegative()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        Assert.Equal(PredicateSign.Negative, PlaneClassification.Classify(a, b, c, new Vec3(0, 0, -1)));
    }

    [Fact]
    public void PointOnPlane_IsZero()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        Assert.Equal(PredicateSign.Zero, PlaneClassification.Classify(a, b, c, new Vec3(0.5, 0.5, 0)));
    }

    [Fact]
    public void PlanePointIsOnPlane_AllThree()
    {
        var a = Vec3.Zero;
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        Assert.Equal(PredicateSign.Zero, PlaneClassification.Classify(a, b, c, a));
        Assert.Equal(PredicateSign.Zero, PlaneClassification.Classify(a, b, c, b));
        Assert.Equal(PredicateSign.Zero, PlaneClassification.Classify(a, b, c, c));
    }

    [Fact]
    public void ReversedWinding_FlipsSign()
    {
        var a = Vec3.Zero;
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var testPoint = new Vec3(0, 0, 1);
        var sign1 = PlaneClassification.Classify(a, b, c, testPoint);
        var sign2 = PlaneClassification.Classify(a, c, b, testPoint);
        Assert.NotEqual(sign1, sign2);
    }

    [Fact]
    public void OffsetPlane_CorrectSides()
    {
        var a = new Vec3(0, 0, 5);
        var b = new Vec3(1, 0, 5);
        var c = new Vec3(0, 1, 5);
        var above = PlaneClassification.Classify(a, b, c, new Vec3(0, 0, 10));
        var below = PlaneClassification.Classify(a, b, c, new Vec3(0, 0, 0));
        Assert.NotEqual(above, below);
    }

    [Fact]
    public void SmallPerturbation_Detected()
    {
        var a = Vec3.Zero;
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var result = PlaneClassification.Classify(a, b, c, new Vec3(0, 0, 1e-10));
        Assert.Equal(PredicateSign.Positive, result);
    }

    [Fact]
    public void LargeCoordinates_CorrectClassification()
    {
        var a = new Vec3(1000, 2000, 3000);
        var b = new Vec3(1001, 2000, 3000);
        var c = new Vec3(1000, 2001, 3000);
        Assert.Equal(PredicateSign.Positive, PlaneClassification.Classify(a, b, c, new Vec3(1000, 2000, 3001)));
    }

    [Fact]
    public void SymmetricAboveBelow()
    {
        var a = Vec3.Zero;
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var above = PlaneClassification.Classify(a, b, c, new Vec3(0, 0, 5));
        var below = PlaneClassification.Classify(a, b, c, new Vec3(0, 0, -5));
        Assert.True(
            (above == PredicateSign.Positive && below == PredicateSign.Negative) ||
            (above == PredicateSign.Negative && below == PredicateSign.Positive));
    }

    [Fact]
    public void DiagonalPlane_NonZeroClassification()
    {
        var a = Vec3.Zero;
        var b = new Vec3(1, 1, 0);
        var c = new Vec3(0, 0, 1);
        var result = PlaneClassification.Classify(a, b, c, new Vec3(10, -10, 0));
        Assert.NotEqual(PredicateSign.Zero, result);
    }
}
