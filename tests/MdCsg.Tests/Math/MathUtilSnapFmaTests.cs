using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Phase 6: MathUtil — SnapToGrid, Fma, constants</summary>
public class MathUtilSnapFmaTests
{
    [Fact]
    public void Epsilon_IsSmall()
    {
        Assert.True(MathUtil.Epsilon > 0);
        Assert.True(MathUtil.Epsilon < 1e-8);
    }

    [Fact]
    public void DefaultGridSize_IsSmall()
    {
        Assert.True(MathUtil.DefaultGridSize > 0);
        Assert.True(MathUtil.DefaultGridSize < 1e-6);
    }

    [Fact]
    public void SnapToGrid_Zero_StaysZero()
    {
        Assert.Equal(0, MathUtil.SnapToGrid(0));
    }

    [Fact]
    public void SnapToGrid_ExactMultiple_Unchanged()
    {
        double gridSize = 0.1;
        Assert.Equal(0.5, MathUtil.SnapToGrid(0.5, gridSize), 1e-15);
    }

    [Fact]
    public void SnapToGrid_BetweenMultiples_RoundsToNearest()
    {
        double gridSize = 0.1;
        Assert.Equal(0.5, MathUtil.SnapToGrid(0.52, gridSize), 1e-15);
        Assert.Equal(0.6, MathUtil.SnapToGrid(0.57, gridSize), 1e-15);
    }

    [Fact]
    public void SnapToGrid_Negative_SnapsCorrectly()
    {
        double gridSize = 0.1;
        Assert.Equal(-0.5, MathUtil.SnapToGrid(-0.52, gridSize), 1e-15);
    }

    [Fact]
    public void SnapToGrid_DefaultGrid_Idempotent()
    {
        double val = 0.12345;
        double snapped = MathUtil.SnapToGrid(val);
        double snappedAgain = MathUtil.SnapToGrid(snapped);
        Assert.Equal(snapped, snappedAgain);
    }

    [Fact]
    public void SnapToGrid_VerySmallGrid_HighPrecision()
    {
        double gridSize = 1e-12;
        double val = 1.0000000000015;
        double snapped = MathUtil.SnapToGrid(val, gridSize);
        Assert.True(System.Math.Abs(snapped - val) <= gridSize);
    }

    [Fact]
    public void SnapToGrid_LargeGrid_RoundsCoarsely()
    {
        double gridSize = 10.0;
        Assert.Equal(50, MathUtil.SnapToGrid(47.5, gridSize), 1e-10);
    }

    [Fact]
    public void Fma_BasicCase()
    {
        // FMA(2, 3, 4) = 2*3 + 4 = 10
        Assert.Equal(10, MathUtil.Fma(2, 3, 4));
    }

    [Fact]
    public void Fma_Identity()
    {
        // FMA(a, 1, 0) = a
        Assert.Equal(5.5, MathUtil.Fma(5.5, 1, 0));
    }

    [Fact]
    public void Fma_ZeroMultiplier()
    {
        // FMA(0, b, c) = c
        Assert.Equal(7.0, MathUtil.Fma(0, 3, 7));
    }

    [Fact]
    public void Fma_NegativeValues()
    {
        Assert.Equal(-2, MathUtil.Fma(-1, 3, 1));
    }

    [Fact]
    public void Fma_LargeValues_NoOverflow()
    {
        double a = 1e150;
        double b = 1e150;
        double c = -1e300;
        double result = MathUtil.Fma(a, b, c);
        // a*b + c = 1e300 - 1e300 = 0
        Assert.Equal(0, result, 1e290);
    }

    [Theory]
    [InlineData(0.1)]
    [InlineData(0.01)]
    [InlineData(1)]
    [InlineData(0.001)]
    public void SnapToGrid_AlwaysMultipleOfGrid(double gridSize)
    {
        double val = 3.14159;
        double snapped = MathUtil.SnapToGrid(val, gridSize);
        double remainder = snapped / gridSize - System.Math.Round(snapped / gridSize);
        Assert.True(System.Math.Abs(remainder) < 1e-10);
    }

    [Fact]
    public void SnapToGrid_OneGridSize_RoundsToIntegers()
    {
        Assert.Equal(3, MathUtil.SnapToGrid(3.4, 1.0), 1e-15);
        Assert.Equal(4, MathUtil.SnapToGrid(3.6, 1.0), 1e-15);
    }
}
