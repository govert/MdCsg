using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Vec3 — Distance, DistanceSquared, Normalized, Min/Max, Dot, Cross</summary>
public class Vec3DistanceNormTests
{
    [Fact]
    public void Distance_SamePoint_Zero()
    {
        Assert.Equal(0, Vec3.Distance(new Vec3(1, 2, 3), new Vec3(1, 2, 3)), 10);
    }

    [Fact]
    public void Distance_UnitX()
    {
        Assert.Equal(1.0, Vec3.Distance(Vec3.Zero, new Vec3(1, 0, 0)), 10);
    }

    [Fact]
    public void Distance_Symmetric()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        Assert.Equal(Vec3.Distance(a, b), Vec3.Distance(b, a), 10);
    }

    [Fact]
    public void DistanceSquared_Agreement()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 6, 8);
        double d = Vec3.Distance(a, b);
        double dSq = Vec3.DistanceSquared(a, b);
        Assert.Equal(d * d, dSq, 8);
    }

    [Fact]
    public void Normalized_HasLengthOne()
    {
        var v = new Vec3(3, 4, 0);
        Assert.Equal(1.0, v.Normalized.Length, 10);
    }

    [Fact]
    public void Normalized_DirectionPreserved()
    {
        var v = new Vec3(3, 0, 0);
        var n = v.Normalized;
        Assert.Equal(1.0, n.X, 10);
        Assert.Equal(0, n.Y, 10);
        Assert.Equal(0, n.Z, 10);
    }

    [Fact]
    public void Min_ComponentWise()
    {
        var a = new Vec3(1, 5, 3);
        var b = new Vec3(4, 2, 6);
        var m = Vec3.Min(a, b);
        Assert.Equal(new Vec3(1, 2, 3), m);
    }

    [Fact]
    public void Max_ComponentWise()
    {
        var a = new Vec3(1, 5, 3);
        var b = new Vec3(4, 2, 6);
        var m = Vec3.Max(a, b);
        Assert.Equal(new Vec3(4, 5, 6), m);
    }

    [Fact]
    public void Dot_Orthogonal_Zero()
    {
        Assert.Equal(0, Vec3.Dot(new Vec3(1, 0, 0), new Vec3(0, 1, 0)), 10);
    }

    [Fact]
    public void Dot_Parallel_ProductOfLengths()
    {
        var a = new Vec3(3, 0, 0);
        var b = new Vec3(5, 0, 0);
        Assert.Equal(15, Vec3.Dot(a, b), 10);
    }

    [Fact]
    public void Dot_Antiparallel_Negative()
    {
        var a = new Vec3(1, 0, 0);
        var b = new Vec3(-1, 0, 0);
        Assert.Equal(-1, Vec3.Dot(a, b), 10);
    }

    [Fact]
    public void Cross_XY_IsZ()
    {
        var x = new Vec3(1, 0, 0);
        var y = new Vec3(0, 1, 0);
        Assert.Equal(new Vec3(0, 0, 1), Vec3.Cross(x, y));
    }

    [Fact]
    public void Cross_Anticommutative()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var ab = Vec3.Cross(a, b);
        var ba = Vec3.Cross(b, a);
        Assert.Equal(-ab.X, ba.X, 10);
        Assert.Equal(-ab.Y, ba.Y, 10);
        Assert.Equal(-ab.Z, ba.Z, 10);
    }

    [Fact]
    public void Cross_SameVector_Zero()
    {
        var v = new Vec3(1, 2, 3);
        var c = Vec3.Cross(v, v);
        Assert.Equal(0, c.X, 10);
        Assert.Equal(0, c.Y, 10);
        Assert.Equal(0, c.Z, 10);
    }

    [Fact]
    public void Length_345_Is5()
    {
        Assert.Equal(5, new Vec3(3, 4, 0).Length, 10);
    }

    [Fact]
    public void LengthSquared_Agreement()
    {
        var v = new Vec3(2, 3, 6);
        Assert.Equal(v.Length * v.Length, v.LengthSquared, 8);
    }

    [Fact]
    public void Zero_IsZeroVector()
    {
        Assert.Equal(0, Vec3.Zero.X);
        Assert.Equal(0, Vec3.Zero.Y);
        Assert.Equal(0, Vec3.Zero.Z);
    }

    [Fact]
    public void Arithmetic_Addition()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        Assert.Equal(new Vec3(5, 7, 9), a + b);
    }

    [Fact]
    public void Arithmetic_Subtraction()
    {
        var a = new Vec3(5, 7, 9);
        var b = new Vec3(4, 5, 6);
        Assert.Equal(new Vec3(1, 2, 3), a - b);
    }
}
