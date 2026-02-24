using MdCsg.Math;
using MdCsg.Predicates;

namespace MdCsg.Tests.Predicates;

public class Orient3DTests
{
    [Fact]
    public void PointAbovePlane_IsPositive()
    {
        // Triangle in XY plane, normal points up
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0, 0, 1); // above
        Assert.Equal(PredicateSign.Positive, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void PointBelowPlane_IsNegative()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0, 0, -1); // below
        Assert.Equal(PredicateSign.Negative, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void CoplanarPoint_IsZero()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(1, 1, 0); // on the XY plane
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void NearlyCoplanar_ResolvedCorrectly()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var d = new Vec3(0.5, 0.5, 1e-15); // barely above
        Assert.Equal(PredicateSign.Positive, Orient3D.Evaluate(a, b, c, d));
    }

    [Fact]
    public void AllSamePoint_IsZero()
    {
        var p = new Vec3(1, 2, 3);
        Assert.Equal(PredicateSign.Zero, Orient3D.Evaluate(p, p, p, p));
    }
}
