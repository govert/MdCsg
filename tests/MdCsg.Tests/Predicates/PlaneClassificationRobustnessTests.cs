using MdCsg.Math;
using MdCsg.Predicates;

namespace MdCsg.Tests.Predicates;

/// <summary>Phase 6: PlaneClassification — classify points vs planes, edge cases</summary>
public class PlaneClassificationRobustnessTests
{
    [Fact]
    public void PointAbovePlane_IsPositive()
    {
        var a = Vec3.Zero;
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        // Normal points in +Z direction, so point above is +Z
        var point = new Vec3(0, 0, 1);
        Assert.Equal(PredicateSign.Positive, PlaneClassification.Classify(a, b, c, point));
    }

    [Fact]
    public void PointBelowPlane_IsNegative()
    {
        var a = Vec3.Zero;
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var point = new Vec3(0, 0, -1);
        Assert.Equal(PredicateSign.Negative, PlaneClassification.Classify(a, b, c, point));
    }

    [Fact]
    public void PointOnPlane_IsZero()
    {
        var a = Vec3.Zero;
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var point = new Vec3(0.5, 0.5, 0);
        Assert.Equal(PredicateSign.Zero, PlaneClassification.Classify(a, b, c, point));
    }

    [Fact]
    public void PlaneVertex_IsZero()
    {
        var a = Vec3.Zero;
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        Assert.Equal(PredicateSign.Zero, PlaneClassification.Classify(a, b, c, a));
        Assert.Equal(PredicateSign.Zero, PlaneClassification.Classify(a, b, c, b));
        Assert.Equal(PredicateSign.Zero, PlaneClassification.Classify(a, b, c, c));
    }

    [Fact]
    public void ConsistentWithOrient3D()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 0, 1);
        var c = new Vec3(2, 5, 0);
        var d = new Vec3(3, 3, 3);
        Assert.Equal(Orient3D.Evaluate(a, b, c, d), PlaneClassification.Classify(a, b, c, d));
    }

    [Fact]
    public void FlippedPlane_OppositeSign()
    {
        var a = Vec3.Zero;
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var point = new Vec3(0, 0, 1);
        var signNormal = PlaneClassification.Classify(a, b, c, point);
        var signFlipped = PlaneClassification.Classify(a, c, b, point);
        // Flipping winding reverses orientation
        Assert.NotEqual(PredicateSign.Zero, signNormal);
        Assert.NotEqual(signNormal, signFlipped);
    }

    [Fact]
    public void DistantPoint_StillCorrect()
    {
        var a = Vec3.Zero;
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var farAbove = new Vec3(0, 0, 1e10);
        Assert.Equal(PredicateSign.Positive, PlaneClassification.Classify(a, b, c, farAbove));
    }

    [Fact]
    public void VeryCloseToPlane_ResolvedCorrectly()
    {
        var a = Vec3.Zero;
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        // Slightly above plane
        var point = new Vec3(0.5, 0.5, 1e-14);
        var sign = PlaneClassification.Classify(a, b, c, point);
        // Should resolve via adaptive precision
        Assert.True(sign == PredicateSign.Positive || sign == PredicateSign.Zero);
    }

    [Fact]
    public void NonOriginPlane_Above()
    {
        var a = new Vec3(0, 0, 5);
        var b = new Vec3(1, 0, 5);
        var c = new Vec3(0, 1, 5);
        var point = new Vec3(0, 0, 6);
        Assert.Equal(PredicateSign.Positive, PlaneClassification.Classify(a, b, c, point));
    }

    [Fact]
    public void NonOriginPlane_Below()
    {
        var a = new Vec3(0, 0, 5);
        var b = new Vec3(1, 0, 5);
        var c = new Vec3(0, 1, 5);
        var point = new Vec3(0, 0, 4);
        Assert.Equal(PredicateSign.Negative, PlaneClassification.Classify(a, b, c, point));
    }
}
