using MdCsg.Math;
using MdCsg.Predicates;

namespace MdCsg.Tests.Predicates;

/// <summary>Batch 8: InCircle and PlaneClassification extended tests (20 tests)</summary>
public class InCirclePlaneClassExtTests
{
    // --- InCircle ---

    [Fact]
    public void InCircle_CenterPoint_IsInside()
    {
        // Unit circle: vertices at (1,0), (0,1), (-1,0), test center (0,0)
        var a = new Vec2(1, 0);
        var b = new Vec2(0, 1);
        var c = new Vec2(-1, 0);
        Assert.Equal(PredicateSign.Positive, InCircle.Evaluate(a, b, c, new Vec2(0, 0)));
    }

    [Fact]
    public void InCircle_FarPoint_IsOutside()
    {
        var a = new Vec2(1, 0);
        var b = new Vec2(0, 1);
        var c = new Vec2(-1, 0);
        Assert.Equal(PredicateSign.Negative, InCircle.Evaluate(a, b, c, new Vec2(0, -5)));
    }

    [Fact]
    public void InCircle_PointOnCircle_IsZero()
    {
        // All 4 points on unit circle
        var a = new Vec2(1, 0);
        var b = new Vec2(0, 1);
        var c = new Vec2(-1, 0);
        var d = new Vec2(0, -1);
        Assert.Equal(PredicateSign.Zero, InCircle.Evaluate(a, b, c, d));
    }

    [Fact]
    public void InCircle_PointJustInside()
    {
        var a = new Vec2(1, 0);
        var b = new Vec2(0, 1);
        var c = new Vec2(-1, 0);
        // Just barely inside the circumcircle
        var d = new Vec2(0, -0.99);
        Assert.Equal(PredicateSign.Positive, InCircle.Evaluate(a, b, c, d));
    }

    [Fact]
    public void InCircle_PointJustOutside()
    {
        var a = new Vec2(1, 0);
        var b = new Vec2(0, 1);
        var c = new Vec2(-1, 0);
        // Just barely outside
        var d = new Vec2(0, -1.01);
        Assert.Equal(PredicateSign.Negative, InCircle.Evaluate(a, b, c, d));
    }

    [Fact]
    public void InCircle_RightTriangle()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);
        // Circumcenter of right triangle is at midpoint of hypotenuse (0.5, 0.5)
        // Circumradius = sqrt(0.5)
        Assert.Equal(PredicateSign.Positive, InCircle.Evaluate(a, b, c, new Vec2(0.5, 0.5)));
    }

    [Fact]
    public void InCircle_LargeTriangle()
    {
        double s = 1e6;
        var a = new Vec2(s, 0);
        var b = new Vec2(0, s);
        var c = new Vec2(-s, 0);
        Assert.Equal(PredicateSign.Positive, InCircle.Evaluate(a, b, c, new Vec2(0, 0)));
    }

    [Fact]
    public void InCircle_TinyTriangle()
    {
        double eps = 1e-10;
        var a = new Vec2(eps, 0);
        var b = new Vec2(0, eps);
        var c = new Vec2(-eps, 0);
        Assert.Equal(PredicateSign.Positive, InCircle.Evaluate(a, b, c, new Vec2(0, 0)));
    }

    [Fact]
    public void InCircle_EquilateralTriangle_Center()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0.5, System.Math.Sqrt(3) / 2);
        var center = new Vec2(0.5, System.Math.Sqrt(3) / 6);
        Assert.Equal(PredicateSign.Positive, InCircle.Evaluate(a, b, c, center));
    }

    [Fact]
    public void InCircle_Vertex_IsOnCircle()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);
        // Any vertex is on its own circumcircle
        Assert.Equal(PredicateSign.Zero, InCircle.Evaluate(a, b, c, a));
    }

    // --- PlaneClassification ---

    [Fact]
    public void PlaneClass_Above_XYPlane()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        Assert.Equal(PredicateSign.Positive, PlaneClassification.Classify(a, b, c, new Vec3(0, 0, 1)));
    }

    [Fact]
    public void PlaneClass_Below_XYPlane()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        Assert.Equal(PredicateSign.Negative, PlaneClassification.Classify(a, b, c, new Vec3(0, 0, -1)));
    }

    [Fact]
    public void PlaneClass_On_XYPlane()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        Assert.Equal(PredicateSign.Zero, PlaneClassification.Classify(a, b, c, new Vec3(5, 5, 0)));
    }

    [Fact]
    public void PlaneClass_OffsetPlane()
    {
        var a = new Vec3(0, 0, 3);
        var b = new Vec3(1, 0, 3);
        var c = new Vec3(0, 1, 3);
        Assert.Equal(PredicateSign.Positive, PlaneClassification.Classify(a, b, c, new Vec3(0, 0, 4)));
        Assert.Equal(PredicateSign.Negative, PlaneClassification.Classify(a, b, c, new Vec3(0, 0, 2)));
    }

    [Fact]
    public void PlaneClass_TiltedPlane()
    {
        // Plane through (0,0,0), (1,1,0), (0,0,1) — normal in -Y,+Z direction
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 1, 0);
        var c = new Vec3(0, 0, 1);
        var sign = PlaneClassification.Classify(a, b, c, new Vec3(0, -1, 0));
        Assert.NotEqual(PredicateSign.Zero, sign);
    }

    [Fact]
    public void PlaneClass_VertexOnPlane()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        Assert.Equal(PredicateSign.Zero, PlaneClassification.Classify(a, b, c, a));
        Assert.Equal(PredicateSign.Zero, PlaneClassification.Classify(a, b, c, b));
        Assert.Equal(PredicateSign.Zero, PlaneClassification.Classify(a, b, c, c));
    }

    [Fact]
    public void PlaneClass_Symmetric()
    {
        // Above from one side = Below from flipped
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var pt = new Vec3(0, 0, 1);
        var s1 = PlaneClassification.Classify(a, b, c, pt);
        var s2 = PlaneClassification.Classify(a, c, b, pt);
        Assert.NotEqual(s1, s2);
    }

    [Fact]
    public void PlaneClass_YZPlane()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(0, 1, 0);
        var c = new Vec3(0, 0, 1);
        Assert.Equal(PredicateSign.Positive, PlaneClassification.Classify(a, b, c, new Vec3(1, 0, 0)));
        Assert.Equal(PredicateSign.Negative, PlaneClassification.Classify(a, b, c, new Vec3(-1, 0, 0)));
    }

    [Fact]
    public void PlaneClass_XZPlane()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(0, 0, 1);
        var c = new Vec3(1, 0, 0);
        Assert.Equal(PredicateSign.Positive, PlaneClassification.Classify(a, b, c, new Vec3(0, 1, 0)));
        Assert.Equal(PredicateSign.Negative, PlaneClassification.Classify(a, b, c, new Vec3(0, -1, 0)));
    }

    [Fact]
    public void PlaneClass_NearlyCoplanar()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var pt = new Vec3(0.5, 0.5, 1e-14);
        Assert.Equal(PredicateSign.Positive, PlaneClassification.Classify(a, b, c, pt));
    }
}
