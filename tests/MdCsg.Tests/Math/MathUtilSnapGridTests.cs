using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: MathUtil — SnapToGrid, Fma, constants</summary>
public class MathUtilSnapGridTests
{
    [Fact]
    public void Epsilon_IsPositive()
    {
        Assert.True(MathUtil.Epsilon > 0);
    }

    [Fact]
    public void Epsilon_Is1e10()
    {
        Assert.Equal(1e-10, MathUtil.Epsilon);
    }

    [Fact]
    public void DefaultGridSize_Is1e8()
    {
        Assert.Equal(1e-8, MathUtil.DefaultGridSize);
    }

    [Fact]
    public void SnapToGrid_ExactMultiple_Unchanged()
    {
        double result = MathUtil.SnapToGrid(0.5, 0.1);
        Assert.Equal(0.5, result, 1e-14);
    }

    [Fact]
    public void SnapToGrid_RoundsToNearest()
    {
        double result = MathUtil.SnapToGrid(0.54, 0.1);
        Assert.Equal(0.5, result, 1e-14);
    }

    [Fact]
    public void SnapToGrid_RoundsUp()
    {
        double result = MathUtil.SnapToGrid(0.56, 0.1);
        Assert.Equal(0.6, result, 1e-14);
    }

    [Fact]
    public void SnapToGrid_Zero_StaysZero()
    {
        double result = MathUtil.SnapToGrid(0, 0.1);
        Assert.Equal(0, result, 1e-14);
    }

    [Fact]
    public void SnapToGrid_Negative()
    {
        double result = MathUtil.SnapToGrid(-0.34, 0.1);
        Assert.Equal(-0.3, result, 1e-14);
    }

    [Fact]
    public void SnapToGrid_DefaultGridSize()
    {
        double result = MathUtil.SnapToGrid(1.0000000049);
        Assert.Equal(1.0, result, 1e-14);
    }

    [Fact]
    public void SnapToGrid_LargeValue()
    {
        double result = MathUtil.SnapToGrid(1000.123, 1.0);
        Assert.Equal(1000.0, result, 1e-14);
    }

    [Fact]
    public void SnapToGrid_VerySmallGrid()
    {
        double result = MathUtil.SnapToGrid(0.5, 1e-15);
        Assert.Equal(0.5, result, 1e-20);
    }

    [Fact]
    public void Fma_BasicComputation()
    {
        double result = MathUtil.Fma(2, 3, 4);
        Assert.Equal(10, result, 1e-14);
    }

    [Fact]
    public void Fma_Zero()
    {
        double result = MathUtil.Fma(0, 5, 3);
        Assert.Equal(3, result, 1e-14);
    }

    [Fact]
    public void Fma_Negative()
    {
        double result = MathUtil.Fma(-2, 3, 10);
        Assert.Equal(4, result, 1e-14);
    }

    [Fact]
    public void Fma_LargeValues()
    {
        double result = MathUtil.Fma(1e10, 1e10, -1e20);
        Assert.Equal(0, result, 1e5);
    }
}
