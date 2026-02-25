using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: MathUtil — SnapToGrid, Fma, constants</summary>
public class MathUtilSnapToGridPropertyTests
{
    [Fact]
    public void Epsilon_Is1eMinus10()
    {
        Assert.Equal(1e-10, MathUtil.Epsilon, 15);
    }

    [Fact]
    public void DefaultGridSize_Is1eMinus8()
    {
        Assert.Equal(1e-8, MathUtil.DefaultGridSize, 15);
    }

    [Fact]
    public void SnapToGrid_IntegerValue_Unchanged()
    {
        Assert.Equal(5.0, MathUtil.SnapToGrid(5.0), 10);
    }

    [Fact]
    public void SnapToGrid_ZeroValue_Zero()
    {
        Assert.Equal(0.0, MathUtil.SnapToGrid(0.0), 15);
    }

    [Fact]
    public void SnapToGrid_TinyOffset_RoundsToNearest()
    {
        double val = 1.0 + 3e-9; // less than half a grid unit from 1.0
        double snapped = MathUtil.SnapToGrid(val, 1e-8);
        Assert.True(System.Math.Abs(snapped - 1.0) < 1e-7);
    }

    [Fact]
    public void SnapToGrid_Idempotent()
    {
        double val = 1.23456789;
        double snap1 = MathUtil.SnapToGrid(val);
        double snap2 = MathUtil.SnapToGrid(snap1);
        Assert.Equal(snap1, snap2, 15);
    }

    [Fact]
    public void SnapToGrid_NegativeValue_Works()
    {
        double val = -1.0 + 3e-9;
        double snapped = MathUtil.SnapToGrid(val, 1e-8);
        Assert.True(System.Math.Abs(snapped - (-1.0)) < 1e-7);
    }

    [Fact]
    public void SnapToGrid_LargeValue_Works()
    {
        double val = 1e10;
        double snapped = MathUtil.SnapToGrid(val, 1e-8);
        Assert.True(System.Math.Abs(snapped - val) < 1e-7);
    }

    [Fact]
    public void SnapToGrid_HalfGridStep_RoundsConsistently()
    {
        double halfStep = 0.5e-8;
        double snapped = MathUtil.SnapToGrid(halfStep, 1e-8);
        // Should round to either 0 or 1e-8, depending on banker's rounding
        Assert.True(snapped == 0.0 || System.Math.Abs(snapped - 1e-8) < 1e-16);
    }

    [Fact]
    public void SnapToGrid_CustomGridSize()
    {
        double val = 0.17;
        double gridSize = 0.1;
        double snapped = MathUtil.SnapToGrid(val, gridSize);
        Assert.True(System.Math.Abs(snapped - 0.2) < 1e-10);
    }

    [Fact]
    public void Fma_BasicCase()
    {
        double result = MathUtil.Fma(2.0, 3.0, 4.0);
        Assert.Equal(10.0, result, 15);
    }

    [Fact]
    public void Fma_ZeroC()
    {
        double result = MathUtil.Fma(2.0, 3.0, 0.0);
        Assert.Equal(6.0, result, 15);
    }

    [Fact]
    public void Fma_ZeroA()
    {
        double result = MathUtil.Fma(0.0, 3.0, 4.0);
        Assert.Equal(4.0, result, 15);
    }

    [Fact]
    public void Fma_IdentityMultiplication()
    {
        double result = MathUtil.Fma(1.0, 7.0, 0.0);
        Assert.Equal(7.0, result, 15);
    }

    [Fact]
    public void Fma_NegativeValues()
    {
        double result = MathUtil.Fma(-2.0, 3.0, 1.0);
        Assert.Equal(-5.0, result, 15);
    }
}
