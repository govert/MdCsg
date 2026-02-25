using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Ray struct properties — PointAt, record semantics, direction, origin</summary>
public class RayStructPropertyTests
{
    [Fact]
    public void PointAt_Zero_IsOrigin()
    {
        var ray = new Ray(new Vec3(1, 2, 3), new Vec3(1, 0, 0));
        Assert.Equal(ray.Origin, ray.PointAt(0));
    }

    [Fact]
    public void PointAt_One_IsOriginPlusDirection()
    {
        var origin = new Vec3(1, 2, 3);
        var dir = new Vec3(0, 1, 0);
        var ray = new Ray(origin, dir);
        var expected = origin + dir;
        Assert.Equal(expected, ray.PointAt(1));
    }

    [Fact]
    public void PointAt_Negative_GoesBackward()
    {
        var origin = new Vec3(1, 2, 3);
        var dir = new Vec3(1, 0, 0);
        var ray = new Ray(origin, dir);
        var p = ray.PointAt(-1);
        Assert.Equal(0, p.X, 1e-14);
        Assert.Equal(2, p.Y, 1e-14);
        Assert.Equal(3, p.Z, 1e-14);
    }

    [Fact]
    public void PointAt_LargeT()
    {
        var ray = new Ray(Vec3.Zero, new Vec3(1, 0, 0));
        var p = ray.PointAt(1000);
        Assert.Equal(1000, p.X, 1e-10);
    }

    [Fact]
    public void RecordEquality()
    {
        var a = new Ray(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        var b = new Ray(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        Assert.Equal(a, b);
    }

    [Fact]
    public void RecordInequality()
    {
        var a = new Ray(new Vec3(1, 2, 3), new Vec3(4, 5, 6));
        var b = new Ray(new Vec3(1, 2, 3), new Vec3(4, 5, 7));
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Origin_Accessible()
    {
        var ray = new Ray(new Vec3(10, 20, 30), Vec3.UnitX);
        Assert.Equal(10, ray.Origin.X);
        Assert.Equal(20, ray.Origin.Y);
        Assert.Equal(30, ray.Origin.Z);
    }

    [Fact]
    public void Direction_Accessible()
    {
        var ray = new Ray(Vec3.Zero, new Vec3(3, 4, 5));
        Assert.Equal(3, ray.Direction.X);
        Assert.Equal(4, ray.Direction.Y);
        Assert.Equal(5, ray.Direction.Z);
    }

    [Fact]
    public void PointAt_LinearInterpolation()
    {
        var ray = new Ray(new Vec3(0, 0, 0), new Vec3(1, 2, 3));
        var p1 = ray.PointAt(0.5);
        Assert.Equal(0.5, p1.X, 1e-14);
        Assert.Equal(1.0, p1.Y, 1e-14);
        Assert.Equal(1.5, p1.Z, 1e-14);
    }

    [Fact]
    public void ToString_NotNull()
    {
        var ray = new Ray(Vec3.Zero, Vec3.UnitX);
        Assert.NotNull(ray.ToString());
        Assert.True(ray.ToString().Length > 0);
    }

    [Fact]
    public void PointAt_MultipleSteps_Consistent()
    {
        var ray = new Ray(Vec3.Zero, new Vec3(1, 1, 1));
        var p2 = ray.PointAt(2);
        var p1 = ray.PointAt(1);
        // p2 = 2 * p1
        Assert.Equal(p1.X * 2, p2.X, 1e-14);
        Assert.Equal(p1.Y * 2, p2.Y, 1e-14);
        Assert.Equal(p1.Z * 2, p2.Z, 1e-14);
    }
}
