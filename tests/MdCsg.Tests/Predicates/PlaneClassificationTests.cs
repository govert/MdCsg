using MdCsg.Math;
using MdCsg.Predicates;

namespace MdCsg.Tests.Predicates;

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
}
