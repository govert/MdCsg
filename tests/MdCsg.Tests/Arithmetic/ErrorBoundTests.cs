using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Phase 6: ErrorBound constants verification</summary>
public class ErrorBoundTests
{
    [Fact]
    public void Epsilon_IsIeee754()
    {
        // 2^-53 ≈ 1.11e-16
        Assert.True(ErrorBound.Epsilon > 1e-17);
        Assert.True(ErrorBound.Epsilon < 2e-16);
    }

    [Fact]
    public void Orient2D_ErrorBounds_Positive()
    {
        Assert.True(ErrorBound.Orient2DErrorBoundA > 0);
        Assert.True(ErrorBound.Orient2DErrorBoundB > 0);
        Assert.True(ErrorBound.Orient2DErrorBoundC > 0);
    }

    [Fact]
    public void Orient3D_ErrorBounds_Positive()
    {
        Assert.True(ErrorBound.Orient3DErrorBoundA > 0);
        Assert.True(ErrorBound.Orient3DErrorBoundB > 0);
        Assert.True(ErrorBound.Orient3DErrorBoundC > 0);
    }

    [Fact]
    public void InCircle_ErrorBounds_Positive()
    {
        Assert.True(ErrorBound.InCircleErrorBoundA > 0);
        Assert.True(ErrorBound.InCircleErrorBoundB > 0);
        Assert.True(ErrorBound.InCircleErrorBoundC > 0);
    }

    [Fact]
    public void ResultErrBound_Positive()
    {
        Assert.True(ErrorBound.ResultErrBound > 0);
    }

    [Fact]
    public void ErrorBounds_Ordering()
    {
        // A bounds are for the fast filter (larger), B for intermediate, C for finest
        Assert.True(ErrorBound.Orient2DErrorBoundA > ErrorBound.Orient2DErrorBoundC);
        Assert.True(ErrorBound.Orient3DErrorBoundA > ErrorBound.Orient3DErrorBoundC);
        Assert.True(ErrorBound.InCircleErrorBoundA > ErrorBound.InCircleErrorBoundC);
    }

    [Fact]
    public void ErrorBounds_SmallValues()
    {
        // All bounds should be very small (close to machine epsilon)
        Assert.True(ErrorBound.Orient2DErrorBoundA < 1e-14);
        Assert.True(ErrorBound.Orient3DErrorBoundA < 1e-14);
        Assert.True(ErrorBound.InCircleErrorBoundA < 1e-14);
    }
}
