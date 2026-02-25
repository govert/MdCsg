using MdCsg.Math;
using MdCsg.Predicates;

namespace MdCsg.Tests.Predicates;

/// <summary>Phase 6: PlaneClassification consistency — Orient3D agreement, signed distance, coplanar detection</summary>
public class PlaneClassificationConsistencyTests
{
    [Fact]
    public void Classify_MatchesOrient3D()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var above = new Vec3(0, 0, 1);
        var below = new Vec3(0, 0, -1);
        var on = new Vec3(0.5, 0.5, 0);

        Assert.Equal(Orient3D.Evaluate(a, b, c, above), PlaneClassification.Classify(a, b, c, above));
        Assert.Equal(Orient3D.Evaluate(a, b, c, below), PlaneClassification.Classify(a, b, c, below));
        Assert.Equal(Orient3D.Evaluate(a, b, c, on), PlaneClassification.Classify(a, b, c, on));
    }

    [Fact]
    public void Classify_PositiveSide_MatchesSignedDistance()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var plane = Plane.FromPoints(a, b, c);

        var point = new Vec3(0, 0, 5);
        var sign = PlaneClassification.Classify(a, b, c, point);
        double dist = plane.SignedDistanceTo(point);

        Assert.Equal(PredicateSign.Positive, sign);
        Assert.True(dist > 0);
    }

    [Fact]
    public void Classify_NegativeSide_MatchesSignedDistance()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var plane = Plane.FromPoints(a, b, c);

        var point = new Vec3(0, 0, -5);
        var sign = PlaneClassification.Classify(a, b, c, point);
        double dist = plane.SignedDistanceTo(point);

        Assert.Equal(PredicateSign.Negative, sign);
        Assert.True(dist < 0);
    }

    [Fact]
    public void Classify_OnPlane_Zero()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);

        // Point on plane: linear combination of a, b, c
        var point = new Vec3(0.3, 0.3, 0);
        Assert.Equal(PredicateSign.Zero, PlaneClassification.Classify(a, b, c, point));
    }

    [Fact]
    public void FlippedPlane_ReversesSide()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var plane = Plane.FromPoints(a, b, c);
        var flipped = plane.Flipped;

        var point = new Vec3(0, 0, 1);
        double d1 = plane.SignedDistanceTo(point);
        double d2 = flipped.SignedDistanceTo(point);

        Assert.True(System.Math.Abs(d1 + d2) < 1e-10, "Flipped plane should negate signed distance");
    }

    [Fact]
    public void PlaneFromPoints_VerticesOnPlane()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var c = new Vec3(7, 2, 1);
        var plane = Plane.FromPoints(a, b, c);

        Assert.True(System.Math.Abs(plane.SignedDistanceTo(a)) < 1e-10);
        Assert.True(System.Math.Abs(plane.SignedDistanceTo(b)) < 1e-10);
        Assert.True(System.Math.Abs(plane.SignedDistanceTo(c)) < 1e-10);
    }

    [Fact]
    public void SignedDistance_Monotonic_AlongNormal()
    {
        var plane = Plane.FromPoints(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));

        // Normal is (0,0,1), moving along Z
        double d0 = plane.SignedDistanceTo(new Vec3(0, 0, 0));
        double d1 = plane.SignedDistanceTo(new Vec3(0, 0, 1));
        double d2 = plane.SignedDistanceTo(new Vec3(0, 0, 2));

        Assert.True(d1 > d0);
        Assert.True(d2 > d1);
    }

    [Fact]
    public void RandomPoints_ClassifyAndDistanceAgree()
    {
        var rng = new Random(42);
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var plane = Plane.FromPoints(a, b, c);

        for (int i = 0; i < 100; i++)
        {
            var point = new Vec3(
                rng.NextDouble() * 2 - 1,
                rng.NextDouble() * 2 - 1,
                rng.NextDouble() * 2 - 1);

            var sign = PlaneClassification.Classify(a, b, c, point);
            double dist = plane.SignedDistanceTo(point);

            if (sign == PredicateSign.Positive)
                Assert.True(dist >= -1e-10, $"Positive classified but dist={dist}");
            else if (sign == PredicateSign.Negative)
                Assert.True(dist <= 1e-10, $"Negative classified but dist={dist}");
        }
    }

    [Fact]
    public void TiltedPlane_ClassifyCorrectly()
    {
        // Plane at 45 degrees: normal is (1,0,1)/sqrt(2)
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(0, 1, 0);
        var c = new Vec3(1, 0, -1); // creates a tilted plane

        var above = new Vec3(1, 0, 1); // on the normal side
        var below = new Vec3(-1, 0, -1); // opposite

        var signAbove = PlaneClassification.Classify(a, b, c, above);
        var signBelow = PlaneClassification.Classify(a, b, c, below);

        Assert.NotEqual(signAbove, signBelow);
        Assert.NotEqual(PredicateSign.Zero, signAbove);
    }

    [Fact]
    public void NearPlanePoint_StillClassified()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);

        // Very close to plane but not on it
        var near = new Vec3(0, 0, 1e-14);
        var sign = PlaneClassification.Classify(a, b, c, near);
        // The robust predicate should still classify this correctly
        Assert.Equal(PredicateSign.Positive, sign);
    }

    [Fact]
    public void LargeCoordinates_ClassifyCorrectly()
    {
        double L = 1e8;
        var a = new Vec3(L, 0, 0);
        var b = new Vec3(0, L, 0);
        var c = new Vec3(0, 0, 0);

        var above = new Vec3(0, 0, L);
        var sign = PlaneClassification.Classify(a, b, c, above);
        Assert.NotEqual(PredicateSign.Zero, sign);
    }
}
