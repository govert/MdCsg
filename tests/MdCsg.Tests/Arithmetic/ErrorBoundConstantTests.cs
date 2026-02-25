using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Phase 6: ErrorBound constants — machine epsilon, error bound hierarchy, positivity</summary>
public class ErrorBoundConstantTests
{
    [Fact]
    public void MachineEpsilon_IsPositive()
    {
        Assert.True(ErrorBound.Epsilon > 0);
    }

    [Fact]
    public void MachineEpsilon_IsSmall()
    {
        Assert.True(ErrorBound.Epsilon < 1e-15);
    }

    [Fact]
    public void MachineEpsilon_Is2Pow53()
    {
        Assert.Equal(1.1102230246251565e-16, ErrorBound.Epsilon, 1e-30);
    }

    [Fact]
    public void Orient2D_BoundsPositive()
    {
        Assert.True(ErrorBound.Orient2DErrorBoundA > 0);
        Assert.True(ErrorBound.Orient2DErrorBoundB > 0);
        Assert.True(ErrorBound.Orient2DErrorBoundC > 0);
    }

    [Fact]
    public void Orient3D_BoundsPositive()
    {
        Assert.True(ErrorBound.Orient3DErrorBoundA > 0);
        Assert.True(ErrorBound.Orient3DErrorBoundB > 0);
        Assert.True(ErrorBound.Orient3DErrorBoundC > 0);
    }

    [Fact]
    public void InCircle_BoundsPositive()
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
    public void Orient2D_A_GreaterThan_B()
    {
        // A-level bound is looser than B-level
        Assert.True(ErrorBound.Orient2DErrorBoundA > ErrorBound.Orient2DErrorBoundB);
    }

    [Fact]
    public void Orient2D_B_GreaterThan_C()
    {
        // B-level bound is looser than C-level
        Assert.True(ErrorBound.Orient2DErrorBoundB > ErrorBound.Orient2DErrorBoundC);
    }

    [Fact]
    public void Orient3D_A_GreaterThan_B()
    {
        Assert.True(ErrorBound.Orient3DErrorBoundA > ErrorBound.Orient3DErrorBoundB);
    }

    [Fact]
    public void Orient3D_B_GreaterThan_C()
    {
        Assert.True(ErrorBound.Orient3DErrorBoundB > ErrorBound.Orient3DErrorBoundC);
    }

    [Fact]
    public void InCircle_A_GreaterThan_B()
    {
        Assert.True(ErrorBound.InCircleErrorBoundA > ErrorBound.InCircleErrorBoundB);
    }

    [Fact]
    public void InCircle_B_GreaterThan_C()
    {
        Assert.True(ErrorBound.InCircleErrorBoundB > ErrorBound.InCircleErrorBoundC);
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
    public void Orient3D_A_LargerThan_Orient2D_A()
    {
        // 3D bounds are larger than 2D bounds (more terms in the determinant)
        Assert.True(ErrorBound.Orient3DErrorBoundA > ErrorBound.Orient2DErrorBoundA);
    }

    [Fact]
    public void InCircle_A_LargerThan_Orient2D_A()
    {
        Assert.True(ErrorBound.InCircleErrorBoundA > ErrorBound.Orient2DErrorBoundA);
    }
}
