using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Batch 1: Vec3 extended tests (20 tests)</summary>
public class Vec3ExtendedTests
{
    [Fact]
    public void Division()
    {
        var v = new Vec3(6, 9, 12);
        Assert.Equal(new Vec3(2, 3, 4), v / 3);
    }

    [Fact]
    public void LengthSquared()
    {
        var v = new Vec3(3, 4, 0);
        Assert.Equal(25, v.LengthSquared);
    }

    [Fact]
    public void DistanceSquared()
    {
        var a = new Vec3(1, 0, 0);
        var b = new Vec3(4, 4, 0);
        Assert.Equal(25, Vec3.DistanceSquared(a, b));
    }

    [Fact]
    public void ZeroVector_HasZeroLength()
    {
        Assert.Equal(0, Vec3.Zero.Length);
    }

    [Fact]
    public void UnitVectors_AreOrthogonal()
    {
        Assert.Equal(0, Vec3.Dot(Vec3.UnitX, Vec3.UnitY));
        Assert.Equal(0, Vec3.Dot(Vec3.UnitY, Vec3.UnitZ));
        Assert.Equal(0, Vec3.Dot(Vec3.UnitZ, Vec3.UnitX));
    }

    [Fact]
    public void UnitVectors_HaveUnitLength()
    {
        Assert.Equal(1, Vec3.UnitX.Length, 1e-15);
        Assert.Equal(1, Vec3.UnitY.Length, 1e-15);
        Assert.Equal(1, Vec3.UnitZ.Length, 1e-15);
    }

    [Fact]
    public void CrossProduct_SelfIsZero()
    {
        var v = new Vec3(1, 2, 3);
        Assert.Equal(Vec3.Zero, Vec3.Cross(v, v));
    }

    [Fact]
    public void CrossProduct_Anticommutative()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        Assert.Equal(-Vec3.Cross(a, b), Vec3.Cross(b, a));
    }

    [Fact]
    public void DotProduct_Commutative()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        Assert.Equal(Vec3.Dot(a, b), Vec3.Dot(b, a));
    }

    [Fact]
    public void DotProduct_Distributive()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var c = new Vec3(7, 8, 9);
        Assert.Equal(Vec3.Dot(a, b + c), Vec3.Dot(a, b) + Vec3.Dot(a, c), 1e-12);
    }

    [Fact]
    public void ScalarLeftMultiply()
    {
        var v = new Vec3(1, 2, 3);
        Assert.Equal(v * 5.0, 5.0 * v);
    }

    [Fact]
    public void Normalized_PreservesDirection()
    {
        var v = new Vec3(3, 4, 0);
        var n = v.Normalized;
        Assert.Equal(0.6, n.X, 1e-15);
        Assert.Equal(0.8, n.Y, 1e-15);
        Assert.Equal(0.0, n.Z, 1e-15);
    }

    [Fact]
    public void DoubleNegation_IsIdentity()
    {
        var v = new Vec3(1, -2, 3);
        Assert.Equal(v, -(-v));
    }

    [Fact]
    public void AdditionSubtraction_Inverse()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        Assert.Equal(a, a + b - b);
    }

    [Fact]
    public void Indexer_ThrowsOutOfRange()
    {
        var v = new Vec3(1, 2, 3);
        Assert.Throws<ArgumentOutOfRangeException>(() => v[3]);
        Assert.Throws<ArgumentOutOfRangeException>(() => v[-1]);
    }

    [Fact]
    public void SnapToGrid_SnapsSmallDeviations()
    {
        var v = new Vec3(1.00000000499, 2.0, 3.0);
        var snapped = v.SnapToGrid(1e-8);
        Assert.Equal(1.0, snapped.X, 1e-10);
    }

    [Fact]
    public void SnapToGrid_PreservesExactValues()
    {
        var v = new Vec3(1.5, 2.5, 3.5);
        var snapped = v.SnapToGrid(0.5);
        Assert.Equal(v, snapped);
    }

    [Fact]
    public void Min_ComponentWise()
    {
        var a = new Vec3(1, 5, 3);
        var b = new Vec3(4, 2, 6);
        var min = Vec3.Min(a, b);
        Assert.Equal(1, min.X);
        Assert.Equal(2, min.Y);
        Assert.Equal(3, min.Z);
    }

    [Fact]
    public void Max_ComponentWise()
    {
        var a = new Vec3(1, 5, 3);
        var b = new Vec3(4, 2, 6);
        var max = Vec3.Max(a, b);
        Assert.Equal(4, max.X);
        Assert.Equal(5, max.Y);
        Assert.Equal(6, max.Z);
    }

    [Fact]
    public void ToString_Format()
    {
        var v = new Vec3(1, 2, 3);
        Assert.Equal("(1, 2, 3)", v.ToString());
    }
}
