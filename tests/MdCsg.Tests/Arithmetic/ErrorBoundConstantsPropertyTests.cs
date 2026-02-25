using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Phase 6: ErrorBound — Shewchuk constants, Epsilon correctness, error bound hierarchy</summary>
public class ErrorBoundConstantsPropertyTests
{
    [Fact]
    public void Epsilon_Is2ToNeg53()
    {
        double expected = System.Math.Pow(2.0, -53);
        Assert.Equal(expected, ErrorBound.Epsilon, 15);
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
    public void Orient2D_ErrorBounds_AGreaterThanC()
    {
        // A is the coarsest filter, C the finest → A > C
        Assert.True(ErrorBound.Orient2DErrorBoundA > ErrorBound.Orient2DErrorBoundC);
    }

    [Fact]
    public void Orient3D_ErrorBounds_AGreaterThanC()
    {
        Assert.True(ErrorBound.Orient3DErrorBoundA > ErrorBound.Orient3DErrorBoundC);
    }

    [Fact]
    public void InCircle_ErrorBounds_AGreaterThanC()
    {
        Assert.True(ErrorBound.InCircleErrorBoundA > ErrorBound.InCircleErrorBoundC);
    }

    [Fact]
    public void AllBoundsAreLessThanOne()
    {
        Assert.True(ErrorBound.Orient2DErrorBoundA < 1);
        Assert.True(ErrorBound.Orient3DErrorBoundA < 1);
        Assert.True(ErrorBound.InCircleErrorBoundA < 1);
        Assert.True(ErrorBound.ResultErrBound < 1);
    }

    [Fact]
    public void Orient3DErrorBoundA_GreaterThanOrient2DErrorBoundA()
    {
        // 3D determinant has more terms → larger error bound
        Assert.True(ErrorBound.Orient3DErrorBoundA > ErrorBound.Orient2DErrorBoundA);
    }

    [Fact]
    public void InCircleErrorBoundA_GreaterThanOrient3DErrorBoundA()
    {
        // InCircle is a 4x4 determinant → largest error bound
        Assert.True(ErrorBound.InCircleErrorBoundA > ErrorBound.Orient3DErrorBoundA);
    }
}
