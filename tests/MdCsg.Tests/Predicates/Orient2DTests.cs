using MdCsg.Math;
using MdCsg.Predicates;

namespace MdCsg.Tests.Predicates;

public class Orient2DTests
{
    [Fact]
    public void CounterClockwise_IsPositive()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);
        Assert.Equal(PredicateSign.Positive, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void Clockwise_IsNegative()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(0, 1);
        var c = new Vec2(1, 0);
        Assert.Equal(PredicateSign.Negative, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void Collinear_IsZero()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 1);
        var c = new Vec2(2, 2);
        Assert.Equal(PredicateSign.Zero, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void NearlyCollinear_ResolvedCorrectly()
    {
        // Points that are nearly but not quite collinear
        var a = new Vec2(0, 0);
        var b = new Vec2(1e10, 1e10);
        var c = new Vec2(2e10, 2e10 + 1e-5);
        // c is slightly above the line ab, should be positive
        Assert.Equal(PredicateSign.Positive, Orient2D.Evaluate(a, b, c));
    }
}
