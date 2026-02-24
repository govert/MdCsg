using MdCsg.Math;

namespace MdCsg.Tests.Math;

public class Vec3Tests
{
    [Fact]
    public void Addition()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        Assert.Equal(new Vec3(5, 7, 9), a + b);
    }

    [Fact]
    public void Subtraction()
    {
        var a = new Vec3(4, 5, 6);
        var b = new Vec3(1, 2, 3);
        Assert.Equal(new Vec3(3, 3, 3), a - b);
    }

    [Fact]
    public void ScalarMultiplication()
    {
        var v = new Vec3(1, 2, 3);
        Assert.Equal(new Vec3(2, 4, 6), v * 2);
        Assert.Equal(new Vec3(2, 4, 6), 2 * v);
    }

    [Fact]
    public void DotProduct()
    {
        var a = new Vec3(1, 0, 0);
        var b = new Vec3(0, 1, 0);
        Assert.Equal(0, Vec3.Dot(a, b));
        Assert.Equal(1, Vec3.Dot(a, a));
    }

    [Fact]
    public void CrossProduct()
    {
        Assert.Equal(Vec3.UnitZ, Vec3.Cross(Vec3.UnitX, Vec3.UnitY));
        Assert.Equal(-Vec3.UnitZ, Vec3.Cross(Vec3.UnitY, Vec3.UnitX));
    }

    [Fact]
    public void Length()
    {
        var v = new Vec3(3, 4, 0);
        Assert.Equal(5, v.Length, 1e-15);
    }

    [Fact]
    public void Normalized()
    {
        var v = new Vec3(0, 0, 5);
        var n = v.Normalized;
        Assert.Equal(1.0, n.Length, 1e-15);
        Assert.Equal(Vec3.UnitZ, n);
    }

    [Fact]
    public void Negation()
    {
        var v = new Vec3(1, -2, 3);
        Assert.Equal(new Vec3(-1, 2, -3), -v);
    }

    [Fact]
    public void Indexer()
    {
        var v = new Vec3(10, 20, 30);
        Assert.Equal(10, v[0]);
        Assert.Equal(20, v[1]);
        Assert.Equal(30, v[2]);
    }

    [Fact]
    public void Distance()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(3, 4, 0);
        Assert.Equal(5, Vec3.Distance(a, b), 1e-15);
    }

    [Fact]
    public void SnapToGrid()
    {
        var v = new Vec3(1.000000001, 2.000000002, 3.000000003);
        var snapped = v.SnapToGrid(1e-8);
        Assert.Equal(1.0, snapped.X, 1e-15);
        Assert.Equal(2.0, snapped.Y, 1e-15);
        Assert.Equal(3.0, snapped.Z, 1e-15);
    }

    [Fact]
    public void MinMax()
    {
        var a = new Vec3(1, 5, 3);
        var b = new Vec3(4, 2, 6);
        Assert.Equal(new Vec3(1, 2, 3), Vec3.Min(a, b));
        Assert.Equal(new Vec3(4, 5, 6), Vec3.Max(a, b));
    }
}
