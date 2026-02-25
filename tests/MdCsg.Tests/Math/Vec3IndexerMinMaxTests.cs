using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: Vec3 — indexer, Min/Max, Distance, SnapToGrid edge cases</summary>
public class Vec3IndexerMinMaxTests
{
    [Fact]
    public void Indexer_0_ReturnsX()
    {
        var v = new Vec3(1, 2, 3);
        Assert.Equal(1, v[0]);
    }

    [Fact]
    public void Indexer_1_ReturnsY()
    {
        var v = new Vec3(1, 2, 3);
        Assert.Equal(2, v[1]);
    }

    [Fact]
    public void Indexer_2_ReturnsZ()
    {
        var v = new Vec3(1, 2, 3);
        Assert.Equal(3, v[2]);
    }

    [Fact]
    public void Indexer_OutOfRange_Throws()
    {
        var v = new Vec3(1, 2, 3);
        Assert.Throws<ArgumentOutOfRangeException>(() => v[3]);
        Assert.Throws<ArgumentOutOfRangeException>(() => v[-1]);
    }

    [Fact]
    public void Min_ReturnsComponentWiseMinimum()
    {
        var a = new Vec3(1, 5, 3);
        var b = new Vec3(4, 2, 6);
        var result = Vec3.Min(a, b);
        Assert.Equal(new Vec3(1, 2, 3), result);
    }

    [Fact]
    public void Max_ReturnsComponentWiseMaximum()
    {
        var a = new Vec3(1, 5, 3);
        var b = new Vec3(4, 2, 6);
        var result = Vec3.Max(a, b);
        Assert.Equal(new Vec3(4, 5, 6), result);
    }

    [Fact]
    public void Min_WithSelf_ReturnsSelf()
    {
        var v = new Vec3(1, 2, 3);
        Assert.Equal(v, Vec3.Min(v, v));
    }

    [Fact]
    public void Max_WithSelf_ReturnsSelf()
    {
        var v = new Vec3(1, 2, 3);
        Assert.Equal(v, Vec3.Max(v, v));
    }

    [Fact]
    public void Distance_SamePoint_IsZero()
    {
        var v = new Vec3(3, 4, 5);
        Assert.Equal(0.0, Vec3.Distance(v, v), 15);
    }

    [Fact]
    public void Distance_UnitAxes()
    {
        Assert.Equal(1.0, Vec3.Distance(Vec3.Zero, Vec3.UnitX), 15);
        Assert.Equal(1.0, Vec3.Distance(Vec3.Zero, Vec3.UnitY), 15);
        Assert.Equal(1.0, Vec3.Distance(Vec3.Zero, Vec3.UnitZ), 15);
    }

    [Fact]
    public void DistanceSquared_IsSquareOfDistance()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var dist = Vec3.Distance(a, b);
        var distSq = Vec3.DistanceSquared(a, b);
        Assert.Equal(dist * dist, distSq, 10);
    }

    [Fact]
    public void Distance_IsCommutative()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        Assert.Equal(Vec3.Distance(a, b), Vec3.Distance(b, a), 15);
    }

    [Fact]
    public void SnapToGrid_RoundsToNearestGrid()
    {
        var v = new Vec3(0.100000001, 0.200000001, 0.299999999);
        var snapped = v.SnapToGrid(0.1);
        Assert.Equal(0.1, snapped.X, 10);
        Assert.Equal(0.2, snapped.Y, 10);
        Assert.Equal(0.3, snapped.Z, 10);
    }

    [Fact]
    public void SnapToGrid_ExactValues_Unchanged()
    {
        var v = new Vec3(0.1, 0.2, 0.3);
        var snapped = v.SnapToGrid(0.1);
        Assert.Equal(0.1, snapped.X, 10);
        Assert.Equal(0.2, snapped.Y, 10);
        Assert.Equal(0.3, snapped.Z, 10);
    }

    [Fact]
    public void UnitVectors_AreOrthogonal()
    {
        Assert.Equal(0.0, Vec3.Dot(Vec3.UnitX, Vec3.UnitY), 15);
        Assert.Equal(0.0, Vec3.Dot(Vec3.UnitY, Vec3.UnitZ), 15);
        Assert.Equal(0.0, Vec3.Dot(Vec3.UnitZ, Vec3.UnitX), 15);
    }

    [Fact]
    public void UnitVectors_AreUnitLength()
    {
        Assert.Equal(1.0, Vec3.UnitX.Length, 15);
        Assert.Equal(1.0, Vec3.UnitY.Length, 15);
        Assert.Equal(1.0, Vec3.UnitZ.Length, 15);
    }

    [Fact]
    public void Zero_HasZeroLength()
    {
        Assert.Equal(0.0, Vec3.Zero.Length, 15);
    }

    [Fact]
    public void ScalarMultiplication_LeftAndRight_Equivalent()
    {
        var v = new Vec3(1, 2, 3);
        Assert.Equal(v * 5.0, 5.0 * v);
    }

    [Fact]
    public void Division_IsInverseOfMultiplication()
    {
        var v = new Vec3(2, 4, 6);
        var result = v / 2.0;
        Assert.Equal(new Vec3(1, 2, 3), result);
    }

    [Fact]
    public void Negation_FlipsSign()
    {
        var v = new Vec3(1, -2, 3);
        var neg = -v;
        Assert.Equal(new Vec3(-1, 2, -3), neg);
    }

    [Fact]
    public void DoubleNegation_ReturnsSelf()
    {
        var v = new Vec3(1, 2, 3);
        Assert.Equal(v, -(-v));
    }
}
