using MdCsg.Math;
using MdCsg.Predicates;

namespace MdCsg.Tests.Predicates;

/// <summary>Phase 6: PlaneClassification, InCircle, PredicateSign — classification, in-circle, enum values</summary>
public class PlaneClassificationInCirclePropertyTests
{
    // --- PredicateSign ---

    [Fact]
    public void PredicateSign_Values()
    {
        Assert.Equal(-1, (int)PredicateSign.Negative);
        Assert.Equal(0, (int)PredicateSign.Zero);
        Assert.Equal(1, (int)PredicateSign.Positive);
    }

    // --- PlaneClassification ---

    [Fact]
    public void PlaneClassification_AbovePlane_Positive()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var point = new Vec3(0, 0, 1);
        var sign = PlaneClassification.Classify(a, b, c, point);
        Assert.Equal(PredicateSign.Positive, sign);
    }

    [Fact]
    public void PlaneClassification_BelowPlane_Negative()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var point = new Vec3(0, 0, -1);
        var sign = PlaneClassification.Classify(a, b, c, point);
        Assert.Equal(PredicateSign.Negative, sign);
    }

    [Fact]
    public void PlaneClassification_OnPlane_Zero()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var point = new Vec3(0.5, 0.5, 0); // On the XY plane
        var sign = PlaneClassification.Classify(a, b, c, point);
        Assert.Equal(PredicateSign.Zero, sign);
    }

    [Fact]
    public void PlaneClassification_VertexOnPlane_Zero()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var sign = PlaneClassification.Classify(a, b, c, a);
        Assert.Equal(PredicateSign.Zero, sign);
    }

    [Fact]
    public void PlaneClassification_TiltedPlane_AboveIsPositive()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 1); // tilted plane
        var normalSide = Vec3.Cross(b - a, c - a).Normalized;
        var point = a + normalSide * 5;
        var sign = PlaneClassification.Classify(a, b, c, point);
        Assert.Equal(PredicateSign.Positive, sign);
    }

    [Fact]
    public void PlaneClassification_SymmetricWithOrient3D()
    {
        // PlaneClassification.Classify is just Orient3D.Evaluate
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var c = new Vec3(7, 8, 10);
        var d = new Vec3(1, 1, 1);
        var planeResult = PlaneClassification.Classify(a, b, c, d);
        var orient3dResult = Orient3D.Evaluate(a, b, c, d);
        Assert.Equal(orient3dResult, planeResult);
    }

    // --- InCircle ---

    [Fact]
    public void InCircle_InsideCircumcircle_Positive()
    {
        // Unit triangle centered at origin, test point at origin (inside circumcircle)
        var a = new Vec2(0, 0);
        var b = new Vec2(2, 0);
        var c = new Vec2(1, 2);
        var d = new Vec2(1, 0.5); // Clearly inside
        var result = InCircle.Evaluate(a, b, c, d);
        Assert.Equal(PredicateSign.Positive, result);
    }

    [Fact]
    public void InCircle_OutsideCircumcircle_Negative()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);
        var d = new Vec2(10, 10); // Far outside
        var result = InCircle.Evaluate(a, b, c, d);
        Assert.Equal(PredicateSign.Negative, result);
    }

    [Fact]
    public void InCircle_OnCircumcircle_Zero()
    {
        // Four points on the unit circle — d should be on the circumcircle
        double r = 1.0;
        var a = new Vec2(r, 0);
        var b = new Vec2(0, r);
        var c = new Vec2(-r, 0);
        var d = new Vec2(0, -r);
        var result = InCircle.Evaluate(a, b, c, d);
        Assert.Equal(PredicateSign.Zero, result);
    }

    [Fact]
    public void InCircle_RightTriangle_CenterInside()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(2, 0);
        var c = new Vec2(0, 2);
        var d = new Vec2(0.5, 0.5); // Inside triangle, hence inside circumcircle
        var result = InCircle.Evaluate(a, b, c, d);
        Assert.Equal(PredicateSign.Positive, result);
    }

    [Fact]
    public void InCircle_VertexIsOnCircle()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);
        // Testing one of the triangle's own vertices — should be Zero
        var result = InCircle.Evaluate(a, b, c, a);
        Assert.Equal(PredicateSign.Zero, result);
    }

    [Fact]
    public void InCircle_LargeCoordinates_StillCorrect()
    {
        double offset = 1e6;
        var a = new Vec2(offset, offset);
        var b = new Vec2(offset + 2, offset);
        var c = new Vec2(offset, offset + 2);
        var inside = new Vec2(offset + 0.5, offset + 0.5);
        var result = InCircle.Evaluate(a, b, c, inside);
        Assert.Equal(PredicateSign.Positive, result);
    }
}
