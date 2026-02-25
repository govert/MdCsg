using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Phase 6: ErrorBound — constant value verification, ordering, positivity</summary>
public class ErrorBoundConstantPropertyTests
{
    [Fact]
    public void Epsilon_Is2PowMinus53()
    {
        double expected = System.Math.Pow(2, -53);
        Assert.Equal(expected, ErrorBound.Epsilon, 10);
    }

    [Fact]
    public void Epsilon_IsPositive()
    {
        Assert.True(ErrorBound.Epsilon > 0);
    }

    [Fact]
    public void Epsilon_IsSmall()
    {
        Assert.True(ErrorBound.Epsilon < 1e-15);
    }

    [Fact]
    public void Orient2DErrorBoundA_IsPositive()
    {
        Assert.True(ErrorBound.Orient2DErrorBoundA > 0);
    }

    [Fact]
    public void Orient2DErrorBoundB_IsPositive()
    {
        Assert.True(ErrorBound.Orient2DErrorBoundB > 0);
    }

    [Fact]
    public void Orient2DErrorBoundC_IsPositive()
    {
        Assert.True(ErrorBound.Orient2DErrorBoundC > 0);
    }

    [Fact]
    public void Orient3DErrorBoundA_IsPositive()
    {
        Assert.True(ErrorBound.Orient3DErrorBoundA > 0);
    }

    [Fact]
    public void Orient3DErrorBoundB_IsPositive()
    {
        Assert.True(ErrorBound.Orient3DErrorBoundB > 0);
    }

    [Fact]
    public void Orient3DErrorBoundC_IsPositive()
    {
        Assert.True(ErrorBound.Orient3DErrorBoundC > 0);
    }

    [Fact]
    public void InCircleErrorBoundA_IsPositive()
    {
        Assert.True(ErrorBound.InCircleErrorBoundA > 0);
    }

    [Fact]
    public void InCircleErrorBoundB_IsPositive()
    {
        Assert.True(ErrorBound.InCircleErrorBoundB > 0);
    }

    [Fact]
    public void InCircleErrorBoundC_IsPositive()
    {
        Assert.True(ErrorBound.InCircleErrorBoundC > 0);
    }

    [Fact]
    public void ResultErrBound_IsPositive()
    {
        Assert.True(ErrorBound.ResultErrBound > 0);
    }

    [Fact]
    public void Orient2D_BoundsOrder_C_LessThan_B_LessThan_A()
    {
        // C-level is tighter (smaller) than B, which is tighter than A
        Assert.True(ErrorBound.Orient2DErrorBoundC < ErrorBound.Orient2DErrorBoundB);
        Assert.True(ErrorBound.Orient2DErrorBoundB < ErrorBound.Orient2DErrorBoundA);
    }

    [Fact]
    public void Orient3D_BoundsOrder_C_LessThan_B_LessThan_A()
    {
        Assert.True(ErrorBound.Orient3DErrorBoundC < ErrorBound.Orient3DErrorBoundB);
        Assert.True(ErrorBound.Orient3DErrorBoundB < ErrorBound.Orient3DErrorBoundA);
    }

    [Fact]
    public void InCircle_BoundsOrder_C_LessThan_B_LessThan_A()
    {
        Assert.True(ErrorBound.InCircleErrorBoundC < ErrorBound.InCircleErrorBoundB);
        Assert.True(ErrorBound.InCircleErrorBoundB < ErrorBound.InCircleErrorBoundA);
    }

    [Fact]
    public void AllBounds_LessThanOne()
    {
        Assert.True(ErrorBound.Orient2DErrorBoundA < 1);
        Assert.True(ErrorBound.Orient3DErrorBoundA < 1);
        Assert.True(ErrorBound.InCircleErrorBoundA < 1);
        Assert.True(ErrorBound.ResultErrBound < 1);
    }

    [Fact]
    public void Orient2DErrorBoundA_MatchesFormula()
    {
        double eps = ErrorBound.Epsilon;
        double expected = (3.0 + 16.0 * eps) * eps;
        Assert.Equal(expected, ErrorBound.Orient2DErrorBoundA, 15);
    }

    [Fact]
    public void Orient3DErrorBoundA_MatchesFormula()
    {
        double eps = ErrorBound.Epsilon;
        double expected = (7.0 + 56.0 * eps) * eps;
        Assert.Equal(expected, ErrorBound.Orient3DErrorBoundA, 15);
    }

    [Fact]
    public void InCircleErrorBoundA_MatchesFormula()
    {
        double eps = ErrorBound.Epsilon;
        double expected = (10.0 + 96.0 * eps) * eps;
        Assert.Equal(expected, ErrorBound.InCircleErrorBoundA, 15);
    }
}
