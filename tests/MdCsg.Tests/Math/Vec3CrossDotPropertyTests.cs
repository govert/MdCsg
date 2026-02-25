using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Vec3 — cross product properties, dot product, Min/Max, Distance, SnapToGrid, indexer</summary>
public class Vec3CrossDotPropertyTests
{
    [Fact]
    public void Cross_UnitXY_IsUnitZ()
    {
        var c = Vec3.Cross(Vec3.UnitX, Vec3.UnitY);
        Assert.True(System.Math.Abs(c.X) < 1e-10);
        Assert.True(System.Math.Abs(c.Y) < 1e-10);
        Assert.True(System.Math.Abs(c.Z - 1.0) < 1e-10);
    }

    [Fact]
    public void Cross_Anticommutative()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var ab = Vec3.Cross(a, b);
        var ba = Vec3.Cross(b, a);
        Assert.True(System.Math.Abs(ab.X + ba.X) < 1e-10);
        Assert.True(System.Math.Abs(ab.Y + ba.Y) < 1e-10);
        Assert.True(System.Math.Abs(ab.Z + ba.Z) < 1e-10);
    }

    [Fact]
    public void Cross_SelfIsZero()
    {
        var a = new Vec3(3, 4, 5);
        var c = Vec3.Cross(a, a);
        Assert.True(c.LengthSquared < 1e-20);
    }

    [Fact]
    public void Cross_PerpendicularToInputs()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var c = Vec3.Cross(a, b);
        Assert.True(System.Math.Abs(Vec3.Dot(c, a)) < 1e-10);
        Assert.True(System.Math.Abs(Vec3.Dot(c, b)) < 1e-10);
    }

    [Fact]
    public void Dot_Commutative()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        Assert.Equal(Vec3.Dot(a, b), Vec3.Dot(b, a));
    }

    [Fact]
    public void Dot_Perpendicular_Zero()
    {
        Assert.True(System.Math.Abs(Vec3.Dot(Vec3.UnitX, Vec3.UnitY)) < 1e-10);
        Assert.True(System.Math.Abs(Vec3.Dot(Vec3.UnitY, Vec3.UnitZ)) < 1e-10);
        Assert.True(System.Math.Abs(Vec3.Dot(Vec3.UnitZ, Vec3.UnitX)) < 1e-10);
    }

    [Fact]
    public void Dot_UnitSelf_IsOne()
    {
        Assert.Equal(1.0, Vec3.Dot(Vec3.UnitX, Vec3.UnitX));
        Assert.Equal(1.0, Vec3.Dot(Vec3.UnitY, Vec3.UnitY));
        Assert.Equal(1.0, Vec3.Dot(Vec3.UnitZ, Vec3.UnitZ));
    }

    [Fact]
    public void Min_ComponentWise()
    {
        var a = new Vec3(1, 5, 3);
        var b = new Vec3(4, 2, 6);
        var m = Vec3.Min(a, b);
        Assert.Equal(1.0, m.X);
        Assert.Equal(2.0, m.Y);
        Assert.Equal(3.0, m.Z);
    }

    [Fact]
    public void Max_ComponentWise()
    {
        var a = new Vec3(1, 5, 3);
        var b = new Vec3(4, 2, 6);
        var m = Vec3.Max(a, b);
        Assert.Equal(4.0, m.X);
        Assert.Equal(5.0, m.Y);
        Assert.Equal(6.0, m.Z);
    }

    [Fact]
    public void Distance_SamePoint_Zero()
    {
        var p = new Vec3(1, 2, 3);
        Assert.Equal(0.0, Vec3.Distance(p, p));
    }

    [Fact]
    public void Distance_UnitX_IsOne()
    {
        Assert.Equal(1.0, Vec3.Distance(Vec3.Zero, Vec3.UnitX));
    }

    [Fact]
    public void DistanceSquared_3_4_0_Is25()
    {
        Assert.Equal(25.0, Vec3.DistanceSquared(Vec3.Zero, new Vec3(3, 4, 0)));
    }

    [Fact]
    public void SnapToGrid_RoundsCorrectly()
    {
        var v = new Vec3(0.123, 0.456, 0.789);
        var snapped = v.SnapToGrid(0.5);
        Assert.True(System.Math.Abs(snapped.X - 0.0) < 1e-10);
        Assert.True(System.Math.Abs(snapped.Y - 0.5) < 1e-10);
        Assert.True(System.Math.Abs(snapped.Z - 1.0) < 1e-10);
    }

    [Fact]
    public void Indexer_ReturnsComponents()
    {
        var v = new Vec3(1, 2, 3);
        Assert.Equal(1.0, v[0]);
        Assert.Equal(2.0, v[1]);
        Assert.Equal(3.0, v[2]);
    }

    [Fact]
    public void Indexer_OutOfRange_Throws()
    {
        var v = new Vec3(1, 2, 3);
        Assert.Throws<ArgumentOutOfRangeException>(() => v[3]);
    }

    [Fact]
    public void Negate_DoubleNegate_IsOriginal()
    {
        var v = new Vec3(1, -2, 3);
        Assert.Equal(v, -(-v));
    }

    [Fact]
    public void ScalarMultiply_LeftRight_Same()
    {
        var v = new Vec3(1, 2, 3);
        Assert.Equal(v * 5.0, 5.0 * v);
    }

    [Fact]
    public void Normalized_IsUnitLength()
    {
        var v = new Vec3(3, 4, 0);
        Assert.True(System.Math.Abs(v.Normalized.Length - 1.0) < 1e-10);
    }

    [Fact]
    public void Length_3_4_12_Is13()
    {
        Assert.True(System.Math.Abs(new Vec3(3, 4, 12).Length - 13.0) < 1e-10);
    }
}
