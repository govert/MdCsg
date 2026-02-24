using MdCsg.Math;

namespace MdCsg.Tests.Math;

public class TriangleTests
{
    [Fact]
    public void Normal_PointsUp_ForCCWTriangleInXYPlane()
    {
        var tri = new Triangle3(
            new Vec3(0, 0, 0),
            new Vec3(1, 0, 0),
            new Vec3(0, 1, 0));
        var n = tri.UnitNormal;
        Assert.Equal(0, n.X, 1e-15);
        Assert.Equal(0, n.Y, 1e-15);
        Assert.Equal(1, n.Z, 1e-15);
    }

    [Fact]
    public void Area_UnitRightTriangle()
    {
        var tri = new Triangle3(
            new Vec3(0, 0, 0),
            new Vec3(1, 0, 0),
            new Vec3(0, 1, 0));
        Assert.Equal(0.5, tri.Area, 1e-15);
    }

    [Fact]
    public void Centroid()
    {
        var tri = new Triangle3(
            new Vec3(0, 0, 0),
            new Vec3(3, 0, 0),
            new Vec3(0, 3, 0));
        var c = tri.Centroid;
        Assert.Equal(1, c.X, 1e-15);
        Assert.Equal(1, c.Y, 1e-15);
        Assert.Equal(0, c.Z, 1e-15);
    }

    [Fact]
    public void Bounds()
    {
        var tri = new Triangle3(
            new Vec3(1, 2, 3),
            new Vec3(4, 5, 6),
            new Vec3(7, 0, 1));
        var b = tri.Bounds;
        Assert.Equal(new Vec3(1, 0, 1), b.Min);
        Assert.Equal(new Vec3(7, 5, 6), b.Max);
    }

    [Fact]
    public void Plane_ContainsVertices()
    {
        var tri = new Triangle3(
            new Vec3(0, 0, 0),
            new Vec3(1, 0, 0),
            new Vec3(0, 1, 0));
        var plane = tri.Plane;
        Assert.Equal(0, plane.SignedDistanceTo(tri.A), 1e-15);
        Assert.Equal(0, plane.SignedDistanceTo(tri.B), 1e-15);
        Assert.Equal(0, plane.SignedDistanceTo(tri.C), 1e-15);
    }

    [Fact]
    public void Indexer()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var c = new Vec3(7, 8, 9);
        var tri = new Triangle3(a, b, c);
        Assert.Equal(a, tri[0]);
        Assert.Equal(b, tri[1]);
        Assert.Equal(c, tri[2]);
    }
}
