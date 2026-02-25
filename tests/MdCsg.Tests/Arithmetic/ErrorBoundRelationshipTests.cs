using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Phase 6: ErrorBound — constant relationships, Shewchuk bound ordering</summary>
public class ErrorBoundRelationshipTests
{
    [Fact]
    public void Epsilon_IsIeee754_2Pow53()
    {
        Assert.Equal(System.Math.Pow(2, -53), ErrorBound.Epsilon, 1e-30);
    }

    [Fact]
    public void Orient2D_BoundA_GreaterThanB()
    {
        Assert.True(ErrorBound.Orient2DErrorBoundA > ErrorBound.Orient2DErrorBoundB);
    }

    [Fact]
    public void Orient2D_BoundB_GreaterThanC()
    {
        Assert.True(ErrorBound.Orient2DErrorBoundB > ErrorBound.Orient2DErrorBoundC);
    }

    [Fact]
    public void Orient3D_BoundA_GreaterThanB()
    {
        Assert.True(ErrorBound.Orient3DErrorBoundA > ErrorBound.Orient3DErrorBoundB);
    }

    [Fact]
    public void Orient3D_BoundB_GreaterThanC()
    {
        Assert.True(ErrorBound.Orient3DErrorBoundB > ErrorBound.Orient3DErrorBoundC);
    }

    [Fact]
    public void InCircle_BoundA_GreaterThanB()
    {
        Assert.True(ErrorBound.InCircleErrorBoundA > ErrorBound.InCircleErrorBoundB);
    }

    [Fact]
    public void InCircle_BoundB_GreaterThanC()
    {
        Assert.True(ErrorBound.InCircleErrorBoundB > ErrorBound.InCircleErrorBoundC);
    }

    [Fact]
    public void AllBounds_Positive()
    {
        Assert.True(ErrorBound.Orient2DErrorBoundA > 0);
        Assert.True(ErrorBound.Orient2DErrorBoundB > 0);
        Assert.True(ErrorBound.Orient2DErrorBoundC > 0);
        Assert.True(ErrorBound.Orient3DErrorBoundA > 0);
        Assert.True(ErrorBound.Orient3DErrorBoundB > 0);
        Assert.True(ErrorBound.Orient3DErrorBoundC > 0);
        Assert.True(ErrorBound.InCircleErrorBoundA > 0);
        Assert.True(ErrorBound.InCircleErrorBoundB > 0);
        Assert.True(ErrorBound.InCircleErrorBoundC > 0);
        Assert.True(ErrorBound.ResultErrBound > 0);
    }

    [Fact]
    public void CLevelBounds_AreSquareEpsilon()
    {
        // C-level bounds are proportional to Epsilon^2
        double eps2 = ErrorBound.Epsilon * ErrorBound.Epsilon;
        Assert.True(ErrorBound.Orient2DErrorBoundC < 100 * eps2);
        Assert.True(ErrorBound.Orient3DErrorBoundC < 500 * eps2);
        Assert.True(ErrorBound.InCircleErrorBoundC < 700 * eps2);
    }

    [Fact]
    public void ABounds_AreLinearEpsilon()
    {
        // A-level bounds are proportional to Epsilon^1
        double eps = ErrorBound.Epsilon;
        Assert.True(ErrorBound.Orient2DErrorBoundA < 100 * eps);
        Assert.True(ErrorBound.Orient3DErrorBoundA < 100 * eps);
        Assert.True(ErrorBound.InCircleErrorBoundA < 200 * eps);
    }

    [Fact]
    public void Orient3D_BoundsLarger_ThanOrient2D()
    {
        // 3D bounds should be larger due to higher-dimensional computation
        Assert.True(ErrorBound.Orient3DErrorBoundA > ErrorBound.Orient2DErrorBoundA);
    }

    [Fact]
    public void InCircle_BoundsLarger_ThanOrient3D()
    {
        Assert.True(ErrorBound.InCircleErrorBoundA > ErrorBound.Orient3DErrorBoundA);
    }

    [Fact]
    public void ResultErrBound_IsPositive()
    {
        Assert.True(ErrorBound.ResultErrBound > 0);
        Assert.True(ErrorBound.ResultErrBound < ErrorBound.Orient2DErrorBoundA);
    }
}
