using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Phase 6: ErrorBound Shewchuk constants — verify computed values against known formulas</summary>
public class ErrorBoundShewchukVerificationTests
{
    private const double Eps = ErrorBound.Epsilon; // 2^-53

    [Fact]
    public void Epsilon_Is2PowMinus53()
    {
        double expected = System.Math.Pow(2, -53);
        Assert.Equal(expected, Eps, 10);
    }

    [Fact]
    public void Epsilon_IsPositive()
    {
        Assert.True(Eps > 0);
    }

    [Fact]
    public void Epsilon_IsSmall()
    {
        Assert.True(Eps < 1e-15);
    }

    [Fact]
    public void Orient2DErrorBoundA_Formula()
    {
        double expected = (3.0 + 16.0 * Eps) * Eps;
        Assert.Equal(expected, ErrorBound.Orient2DErrorBoundA);
    }

    [Fact]
    public void Orient2DErrorBoundB_Formula()
    {
        double expected = (2.0 + 12.0 * Eps) * Eps;
        Assert.Equal(expected, ErrorBound.Orient2DErrorBoundB);
    }

    [Fact]
    public void Orient2DErrorBoundC_Formula()
    {
        double expected = (9.0 + 64.0 * Eps) * Eps * Eps;
        Assert.Equal(expected, ErrorBound.Orient2DErrorBoundC);
    }

    [Fact]
    public void Orient3DErrorBoundA_Formula()
    {
        double expected = (7.0 + 56.0 * Eps) * Eps;
        Assert.Equal(expected, ErrorBound.Orient3DErrorBoundA);
    }

    [Fact]
    public void Orient3DErrorBoundB_Formula()
    {
        double expected = (3.0 + 28.0 * Eps) * Eps;
        Assert.Equal(expected, ErrorBound.Orient3DErrorBoundB);
    }

    [Fact]
    public void Orient3DErrorBoundC_Formula()
    {
        double expected = (26.0 + 288.0 * Eps) * Eps * Eps;
        Assert.Equal(expected, ErrorBound.Orient3DErrorBoundC);
    }

    [Fact]
    public void InCircleErrorBoundA_Formula()
    {
        double expected = (10.0 + 96.0 * Eps) * Eps;
        Assert.Equal(expected, ErrorBound.InCircleErrorBoundA);
    }

    [Fact]
    public void InCircleErrorBoundB_Formula()
    {
        double expected = (4.0 + 48.0 * Eps) * Eps;
        Assert.Equal(expected, ErrorBound.InCircleErrorBoundB);
    }

    [Fact]
    public void InCircleErrorBoundC_Formula()
    {
        double expected = (44.0 + 576.0 * Eps) * Eps * Eps;
        Assert.Equal(expected, ErrorBound.InCircleErrorBoundC);
    }

    [Fact]
    public void ResultErrBound_Formula()
    {
        double expected = (3.0 + 8.0 * Eps) * Eps;
        Assert.Equal(expected, ErrorBound.ResultErrBound);
    }

    [Fact]
    public void ErrorBounds_ArePositive()
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
    public void ErrorBounds_Ordering_AGreaterThanB()
    {
        // A-level bounds are looser (more permissive) than B-level bounds
        Assert.True(ErrorBound.Orient2DErrorBoundA > ErrorBound.Orient2DErrorBoundB);
        Assert.True(ErrorBound.Orient3DErrorBoundA > ErrorBound.Orient3DErrorBoundB);
        Assert.True(ErrorBound.InCircleErrorBoundA > ErrorBound.InCircleErrorBoundB);
    }

    [Fact]
    public void ErrorBounds_CSmallest()
    {
        // C-level bounds are the tightest (order eps^2)
        Assert.True(ErrorBound.Orient2DErrorBoundC < ErrorBound.Orient2DErrorBoundB);
        Assert.True(ErrorBound.Orient3DErrorBoundC < ErrorBound.Orient3DErrorBoundB);
        Assert.True(ErrorBound.InCircleErrorBoundC < ErrorBound.InCircleErrorBoundB);
    }

    [Fact]
    public void Orient3D_Bounds_LargerThan_Orient2D()
    {
        // 3D predicates need larger error bounds than 2D
        Assert.True(ErrorBound.Orient3DErrorBoundA > ErrorBound.Orient2DErrorBoundA);
    }

    [Fact]
    public void InCircle_Bounds_LargerThan_Orient2D()
    {
        // InCircle is a 4-point predicate, larger than 3-point Orient2D
        Assert.True(ErrorBound.InCircleErrorBoundA > ErrorBound.Orient2DErrorBoundA);
    }
}
