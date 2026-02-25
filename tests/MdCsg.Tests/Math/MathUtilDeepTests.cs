using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: MathUtil deep tests — Epsilon, SnapToGrid, Fma</summary>
public class MathUtilDeepTests
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
    public void SnapToGrid_Zero_ReturnsZero()
    {
        Assert.Equal(0.0, MathUtil.SnapToGrid(0));
    }

    [Fact]
    public void SnapToGrid_Exact_Preserved()
    {
        Assert.Equal(0.5, MathUtil.SnapToGrid(0.5, 0.5), 12);
        Assert.Equal(1.0, MathUtil.SnapToGrid(1.0, 0.5), 12);
    }

    [Fact]
    public void SnapToGrid_RoundsCorrectly()
    {
        double val = 0.123456789;
        double grid = 1e-8;
        double snapped = MathUtil.SnapToGrid(val, grid);
        // Snapped value should be within grid/2 of original
        Assert.True(System.Math.Abs(snapped - val) <= grid / 2 + 1e-15);
    }

    [Fact]
    public void SnapToGrid_NegativeValue()
    {
        double snapped = MathUtil.SnapToGrid(-0.5, 0.5);
        Assert.Equal(-0.5, snapped, 12);
    }

    [Fact]
    public void SnapToGrid_LargeValue()
    {
        double snapped = MathUtil.SnapToGrid(1000.0, 1e-8);
        Assert.True(System.Math.Abs(snapped - 1000.0) < 1e-7);
    }

    [Fact]
    public void SnapToGrid_Idempotent()
    {
        double val = 0.123456789;
        double grid = 1e-8;
        double s1 = MathUtil.SnapToGrid(val, grid);
        double s2 = MathUtil.SnapToGrid(s1, grid);
        Assert.Equal(s1, s2, 15);
    }

    [Fact]
    public void Fma_BasicComputation()
    {
        double result = MathUtil.Fma(2.0, 3.0, 4.0);
        Assert.Equal(10.0, result, 12);
    }

    [Fact]
    public void Fma_ZeroAdd()
    {
        Assert.Equal(6.0, MathUtil.Fma(2.0, 3.0, 0.0), 12);
    }

    [Fact]
    public void Fma_ZeroMultiply()
    {
        Assert.Equal(5.0, MathUtil.Fma(0.0, 100.0, 5.0), 12);
    }

    [Fact]
    public void Fma_NegativeValues()
    {
        Assert.Equal(-2.0, MathUtil.Fma(-1.0, 3.0, 1.0), 12);
    }

    [Fact]
    public void Fma_LargeValues()
    {
        double result = MathUtil.Fma(1e15, 1e15, 1.0);
        Assert.True(result > 0);
    }

    [Fact]
    public void Fma_SmallValues()
    {
        double result = MathUtil.Fma(1e-15, 1e-15, 1e-30);
        Assert.True(result >= 0);
    }
}
