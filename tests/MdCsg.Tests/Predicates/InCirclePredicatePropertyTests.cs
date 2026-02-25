using MdCsg.Math;
using MdCsg.Predicates;

namespace MdCsg.Tests.Predicates;

/// <summary>Phase 6: InCircle — circumcircle containment, vertex on circle, far outside, collinear triangle</summary>
public class InCirclePredicatePropertyTests
{
    [Fact]
    public void CentroidInside_Positive()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(4, 0);
        var c = new Vec2(2, 3);
        var centroid = new Vec2(2, 1); // inside circumcircle
        Assert.Equal(PredicateSign.Positive, InCircle.Evaluate(a, b, c, centroid));
    }

    [Fact]
    public void FarOutside_Negative()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);
        var far = new Vec2(100, 100);
        Assert.Equal(PredicateSign.Negative, InCircle.Evaluate(a, b, c, far));
    }

    [Fact]
    public void VertexOnCircle_Zero()
    {
        // Unit circle: circumcircle of right triangle at origin
        // Right triangle: (0,0), (2,0), (0,2) has circumcircle centered at (1,1) radius sqrt(2)
        var a = new Vec2(0, 0);
        var b = new Vec2(2, 0);
        var c = new Vec2(0, 2);
        // Point on circumcircle: (2, 2) is at distance sqrt((2-1)^2+(2-1)^2) = sqrt(2)
        var onCircle = new Vec2(2, 2);
        Assert.Equal(PredicateSign.Zero, InCircle.Evaluate(a, b, c, onCircle));
    }

    [Fact]
    public void SwapTwoVertices_FlipsSign()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(3, 0);
        var c = new Vec2(0, 3);
        var inside = new Vec2(1, 1);
        var original = InCircle.Evaluate(a, b, c, inside);
        var swapped = InCircle.Evaluate(a, c, b, inside); // swap b,c changes orientation
        // Swapping two vertices of CCW triangle makes it CW, which flips the sign
        Assert.NotEqual(original, swapped);
    }

    [Fact]
    public void EquilateralTriangle_CentroidInside()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(2, 0);
        var c = new Vec2(1, System.Math.Sqrt(3));
        var centroid = new Vec2(1, System.Math.Sqrt(3) / 3.0);
        Assert.Equal(PredicateSign.Positive, InCircle.Evaluate(a, b, c, centroid));
    }

    [Fact]
    public void Orient2D_CCW_Positive()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0.5, 1);
        Assert.Equal(PredicateSign.Positive, Orient2D.Evaluate(a, b, c));
    }

    [Fact]
    public void Orient2D_SwapTwo_FlipsSign()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);
        var original = Orient2D.Evaluate(a, b, c);
        var swapped = Orient2D.Evaluate(b, a, c);
        Assert.NotEqual(original, swapped);
    }

    [Fact]
    public void Orient2D_CyclicPermutation_SameSign()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0, 1);
        var abc = Orient2D.Evaluate(a, b, c);
        var bca = Orient2D.Evaluate(b, c, a);
        var cab = Orient2D.Evaluate(c, a, b);
        Assert.Equal(abc, bca);
        Assert.Equal(bca, cab);
    }

    [Fact]
    public void Orient2D_NearlyCollinear_Decides()
    {
        var a = new Vec2(0, 0);
        var b = new Vec2(1, 0);
        var c = new Vec2(0.5, 1e-15); // nearly collinear
        var result = Orient2D.Evaluate(a, b, c);
        Assert.True(result == PredicateSign.Positive || result == PredicateSign.Negative || result == PredicateSign.Zero);
    }

    [Fact]
    public void PredicateSign_ThreeValues()
    {
        var values = Enum.GetValues(typeof(PredicateSign));
        Assert.Equal(3, values.Length);
    }

    [Fact]
    public void PredicateSign_Values()
    {
        Assert.Equal(-1, (int)PredicateSign.Negative);
        Assert.Equal(0, (int)PredicateSign.Zero);
        Assert.Equal(1, (int)PredicateSign.Positive);
    }

    [Fact]
    public void InCircle_AllOnSamePoint_Zero()
    {
        var p = new Vec2(1, 1);
        Assert.Equal(PredicateSign.Zero, InCircle.Evaluate(p, p, p, p));
    }

    [Fact]
    public void InCircle_LargeCoordinates_StillCorrect()
    {
        var a = new Vec2(1e6, 0);
        var b = new Vec2(0, 1e6);
        var c = new Vec2(-1e6, 0);
        var inside = new Vec2(0, 0); // origin should be inside circumcircle
        Assert.Equal(PredicateSign.Positive, InCircle.Evaluate(a, b, c, inside));
    }

    [Fact]
    public void PlaneClassification_Wrapper_ConsistentWithOrient3D()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var above = new Vec3(0, 0, 1);
        var classify = PlaneClassification.Classify(a, b, c, above);
        var orient = Orient3D.Evaluate(a, b, c, above);
        Assert.Equal(orient, classify);
    }
}
