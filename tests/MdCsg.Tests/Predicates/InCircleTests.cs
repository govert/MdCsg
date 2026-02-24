using MdCsg.Math;
using MdCsg.Predicates;

namespace MdCsg.Tests.Predicates;

public class InCircleTests
{
    [Fact]
    public void PointInsideCircumcircle_IsPositive()
    {
        // Unit triangle CCW, circumcircle centered at (0.5, 0.5) with radius ~0.707
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);
        var d = new Vec2(0.25, 0.25); // inside
        Assert.Equal(PredicateSign.Positive, InCircle.Evaluate(a, b, c, d));
    }

    [Fact]
    public void PointOutsideCircumcircle_IsNegative()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);
        var d = new Vec2(5, 5); // far outside
        Assert.Equal(PredicateSign.Negative, InCircle.Evaluate(a, b, c, d));
    }

    [Fact]
    public void PointOnCircumcircle_IsZero()
    {
        // Equilateral-ish: all four points on the same circle
        // Square vertices are concyclic
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(1, 1);
        var d = new Vec2(0, 1);
        Assert.Equal(PredicateSign.Zero, InCircle.Evaluate(a, b, c, d));
    }
}
