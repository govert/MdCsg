using MdCsg.Math;
using MdCsg.Predicates;

namespace MdCsg.Tests.Predicates;

/// <summary>Phase 6: PredicateSign enum — values, casting, usage across predicates</summary>
public class PredicateSignEnumTests
{
    [Fact]
    public void Positive_ValueIs1()
    {
        Assert.Equal(1, (int)PredicateSign.Positive);
    }

    [Fact]
    public void Zero_ValueIs0()
    {
        Assert.Equal(0, (int)PredicateSign.Zero);
    }

    [Fact]
    public void Negative_ValueIsMinus1()
    {
        Assert.Equal(-1, (int)PredicateSign.Negative);
    }

    [Fact]
    public void AllValues_AreDefined()
    {
        var values = Enum.GetValues(typeof(PredicateSign));
        Assert.Equal(3, values.Length);
    }

    [Fact]
    public void Orient3D_ReturnsAllThreeSigns()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);

        Assert.Equal(PredicateSign.Positive, Orient3D.Evaluate(a, b, c, new Vec3(0, 0, 1)));
        Assert.Equal(PredicateSign.Negative, Orient3D.Evaluate(a, b, c, new Vec3(0, 0, -1)));
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(a, b, c, new Vec3(0.5, 0.5, 0)));
    }

    [Fact]
    public void Orient2D_ReturnsAllThreeSigns()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c_left = new Vec2(0, 1);
        var c_right = new Vec2(0, -1);
        var c_collinear = new Vec2(0.5, 0);

        Assert.Equal(PredicateSign.Positive, Orient2D.Evaluate(a, b, c_left));
        Assert.Equal(PredicateSign.Negative, Orient2D.Evaluate(a, b, c_right));
        Assert.Equal(PredicateSign.Zero, Orient2D.Evaluate(a, b, c_collinear));
    }

    [Fact]
    public void PlaneClassification_ReturnsAllThreeSigns()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);

        Assert.Equal(PredicateSign.Positive, PlaneClassification.Classify(a, b, c, new Vec3(0, 0, 1)));
        Assert.Equal(PredicateSign.Negative, PlaneClassification.Classify(a, b, c, new Vec3(0, 0, -1)));
        Assert.Equal(PredicateSign.Zero, PlaneClassification.Classify(a, b, c, new Vec3(1, 1, 0)));
    }

    [Fact]
    public void InCircle_ReturnsAllThreeSigns()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);

        // Inside the circumcircle
        var inside = new Vec2(0.3, 0.3);
        Assert.Equal(PredicateSign.Positive, InCircle.Evaluate(a, b, c, inside));

        // Outside the circumcircle
        var outside = new Vec2(5, 5);
        Assert.Equal(PredicateSign.Negative, InCircle.Evaluate(a, b, c, outside));
    }

    [Fact]
    public void Sign_CastableToInt_ForMultiplication()
    {
        // Common pattern: using sign value in calculations
        int posVal = (int)PredicateSign.Positive;
        int negVal = (int)PredicateSign.Negative;
        int zeroVal = (int)PredicateSign.Zero;

        Assert.Equal(-1, posVal * negVal);
        Assert.Equal(1, negVal * negVal);
        Assert.Equal(0, zeroVal * posVal);
    }

    [Fact]
    public void SwapArguments_FlipsOrient3DSign()
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
    public void SwapArguments_FlipsOrient2DSign()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);

        var s1 = Orient2D.Evaluate(a, b, c);
        var s2 = Orient2D.Evaluate(b, a, c);

        Assert.NotEqual(s1, s2);
    }
}
