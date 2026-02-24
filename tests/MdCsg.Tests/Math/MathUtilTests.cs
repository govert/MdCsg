using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: MathUtil tests — snap, epsilon, Fma</summary>
public class MathUtilTests
{
    [Fact]
    public void Epsilon_IsSmallPositive()
    {
        Assert.True(MathUtil.Epsilon > 0);
        Assert.True(MathUtil.Epsilon < 1e-6);
    }

    [Fact]
    public void DefaultGridSize_IsSmallPositive()
    {
        Assert.True(MathUtil.DefaultGridSize > 0);
        Assert.True(MathUtil.DefaultGridSize < 1e-4);
    }

    [Fact]
    public void SnapToGrid_Zero()
    {
        Assert.Equal(0, MathUtil.SnapToGrid(0));
    }

    [Fact]
    public void SnapToGrid_ExactMultiple()
    {
        double grid = 0.1;
        Assert.Equal(0.5, MathUtil.SnapToGrid(0.5, grid), 10);
    }

    [Fact]
    public void SnapToGrid_RoundsDown()
    {
        double grid = 0.1;
        Assert.Equal(0.3, MathUtil.SnapToGrid(0.34, grid), 10);
    }

    [Fact]
    public void SnapToGrid_RoundsUp()
    {
        double grid = 0.1;
        Assert.Equal(0.4, MathUtil.SnapToGrid(0.36, grid), 10);
    }

    [Fact]
    public void SnapToGrid_NegativeValue()
    {
        double grid = 0.1;
        Assert.Equal(-0.3, MathUtil.SnapToGrid(-0.34, grid), 10);
    }

    [Fact]
    public void SnapToGrid_DefaultGrid()
    {
        double val = 1.23456789012345e-8;
        double snapped = MathUtil.SnapToGrid(val);
        // Should snap to nearest 1e-8 multiple
        Assert.True(System.Math.Abs(snapped - 1e-8) < 1e-10 || System.Math.Abs(snapped - 2e-8) < 1e-10);
    }

    [Fact]
    public void SnapToGrid_LargeValue()
    {
        double grid = 1.0;
        Assert.Equal(100.0, MathUtil.SnapToGrid(99.7, grid), 10);
    }

    [Fact]
    public void Fma_BasicAdd()
    {
        Assert.Equal(7.0, MathUtil.Fma(2.0, 3.0, 1.0), 10);
    }

    [Fact]
    public void Fma_ZeroMultiply()
    {
        Assert.Equal(5.0, MathUtil.Fma(0, 100, 5.0), 10);
    }

    [Fact]
    public void Fma_NegativeResult()
    {
        Assert.Equal(-7.0, MathUtil.Fma(-2.0, 3.0, -1.0), 10);
    }

    [Fact]
    public void Vec3_SnapToGrid()
    {
        var v = new Vec3(0.123, 0.456, 0.789);
        var snapped = v.SnapToGrid(0.1);
        Assert.Equal(0.1, snapped.X, 5);
        Assert.Equal(0.5, snapped.Y, 5);
        Assert.Equal(0.8, snapped.Z, 5);
    }
}
