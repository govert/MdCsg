using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: MathUtil — Epsilon, DefaultGridSize, SnapToGrid, Fma</summary>
public class MathUtilPropertyTests
{
    [Fact]
    public void Epsilon_IsPositive()
    {
        Assert.True(MathUtil.Epsilon > 0);
    }

    [Fact]
    public void Epsilon_IsSmall()
    {
        Assert.True(MathUtil.Epsilon < 1e-5);
    }

    [Fact]
    public void DefaultGridSize_IsPositive()
    {
        Assert.True(MathUtil.DefaultGridSize > 0);
    }

    [Fact]
    public void DefaultGridSize_IsSmall()
    {
        Assert.True(MathUtil.DefaultGridSize < 1e-3);
    }

    [Fact]
    public void SnapToGrid_ExactMultiple_Unchanged()
    {
        double val = 3.0 * MathUtil.DefaultGridSize;
        double snapped = MathUtil.SnapToGrid(val);
        Assert.True(System.Math.Abs(snapped - val) < 1e-20);
    }

    [Fact]
    public void SnapToGrid_Zero_ReturnsZero()
    {
        Assert.Equal(0.0, MathUtil.SnapToGrid(0.0));
    }

    [Fact]
    public void SnapToGrid_NearGrid_SnapsToNearest()
    {
        double grid = 0.01;
        double val = 0.024; // nearest to 0.02
        double snapped = MathUtil.SnapToGrid(val, grid);
        Assert.True(System.Math.Abs(snapped - 0.02) < 1e-10);
    }

    [Fact]
    public void SnapToGrid_NegativeValue_Snaps()
    {
        double grid = 0.1;
        double val = -0.34;
        double snapped = MathUtil.SnapToGrid(val, grid);
        Assert.True(System.Math.Abs(snapped - (-0.3)) < 1e-10);
    }

    [Fact]
    public void SnapToGrid_LargeValue_Snaps()
    {
        double grid = 1.0;
        double snapped = MathUtil.SnapToGrid(7.6, grid);
        Assert.True(System.Math.Abs(snapped - 8.0) < 1e-10);
    }

    [Fact]
    public void SnapToGrid_VerySmallGrid_Snaps()
    {
        double grid = 1e-12;
        double val = 1.0000000000005;
        double snapped = MathUtil.SnapToGrid(val, grid);
        Assert.True(System.Math.Abs(snapped - val) < grid);
    }

    [Fact]
    public void Fma_SimpleCase()
    {
        double result = MathUtil.Fma(2.0, 3.0, 4.0);
        Assert.True(System.Math.Abs(result - 10.0) < 1e-15);
    }

    [Fact]
    public void Fma_ZeroAdd()
    {
        double result = MathUtil.Fma(5.0, 7.0, 0.0);
        Assert.True(System.Math.Abs(result - 35.0) < 1e-15);
    }

    [Fact]
    public void Fma_ZeroMultiply()
    {
        double result = MathUtil.Fma(0.0, 100.0, 42.0);
        Assert.True(System.Math.Abs(result - 42.0) < 1e-15);
    }

    [Fact]
    public void Fma_NegativeValues()
    {
        double result = MathUtil.Fma(-3.0, 4.0, 2.0);
        Assert.True(System.Math.Abs(result - (-10.0)) < 1e-15);
    }

    [Fact]
    public void SnapToGrid_Idempotent()
    {
        double val = 3.14159;
        double grid = 0.01;
        double s1 = MathUtil.SnapToGrid(val, grid);
        double s2 = MathUtil.SnapToGrid(s1, grid);
        Assert.Equal(s1, s2);
    }

    [Fact]
    public void SnapToGrid_DifferentGridSizes()
    {
        double val = 0.567;
        double snap1 = MathUtil.SnapToGrid(val, 0.1);
        double snap2 = MathUtil.SnapToGrid(val, 0.01);
        // Finer grid should produce result closer to original
        Assert.True(System.Math.Abs(snap2 - val) <= System.Math.Abs(snap1 - val) + 1e-10);
    }
}
