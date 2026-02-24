using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Batch 4: ErrorBound and ExpansionArithmetic extended tests (20 tests)</summary>
public class ErrorBoundExpansionExtTests
{
    // --- ErrorBound constants ---

    [Fact]
    public void Epsilon_IsIEEE754MachineEpsilon()
    {
        Assert.Equal(1.1102230246251565e-16, ErrorBound.Epsilon);
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
    public void Orient2D_BoundsDecrease_AtHigherLevels()
    {
        Assert.True(ErrorBound.Orient2DErrorBoundA > ErrorBound.Orient2DErrorBoundB);
        Assert.True(ErrorBound.Orient2DErrorBoundB > ErrorBound.Orient2DErrorBoundC);
    }

    [Fact]
    public void Orient3D_BoundsDecrease_AtHigherLevels()
    {
        Assert.True(ErrorBound.Orient3DErrorBoundA > ErrorBound.Orient3DErrorBoundB);
        Assert.True(ErrorBound.Orient3DErrorBoundB > ErrorBound.Orient3DErrorBoundC);
    }

    [Fact]
    public void InCircle_BoundsDecrease_AtHigherLevels()
    {
        Assert.True(ErrorBound.InCircleErrorBoundA > ErrorBound.InCircleErrorBoundB);
        Assert.True(ErrorBound.InCircleErrorBoundB > ErrorBound.InCircleErrorBoundC);
    }

    // --- ExpansionArithmetic extended ---

    [Fact]
    public void TwoSum_LargeAndSmall()
    {
        var (s, e) = ExpansionArithmetic.TwoSum(1e15, 1e-5);
        Assert.Equal(1e15 + 1e-5, s + e, 1e-10);
    }

    [Fact]
    public void TwoSum_EqualValues()
    {
        var (s, e) = ExpansionArithmetic.TwoSum(1.5, 1.5);
        Assert.Equal(3.0, s + e, 1e-15);
    }

    [Fact]
    public void TwoSum_NegativeValues()
    {
        var (s, e) = ExpansionArithmetic.TwoSum(-3.0, -4.0);
        Assert.Equal(-7.0, s + e, 1e-15);
    }

    [Fact]
    public void TwoDiff_EqualValues_IsZero()
    {
        var (s, e) = ExpansionArithmetic.TwoDiff(5.0, 5.0);
        Assert.Equal(0.0, s + e, 1e-15);
    }

    [Fact]
    public void TwoProduct_Zero()
    {
        var (h, l) = ExpansionArithmetic.TwoProduct(0.0, 12345.0);
        Assert.Equal(0, h);
        Assert.Equal(0, l);
    }

    [Fact]
    public void TwoProduct_One()
    {
        var (h, l) = ExpansionArithmetic.TwoProduct(1.0, 7.0);
        Assert.Equal(7.0, h + l, 1e-15);
    }

    [Fact]
    public void GrowExpansion_SingleElement()
    {
        Span<double> e = stackalloc double[] { 3.0 };
        Span<double> h = stackalloc double[4];
        int len = ExpansionArithmetic.GrowExpansion(e, 4.0, h);
        double sum = 0;
        for (int i = 0; i < len; i++) sum += h[i];
        Assert.Equal(7.0, sum, 1e-15);
    }

    [Fact]
    public void GrowExpansion_MultipleGrowths()
    {
        Span<double> e = stackalloc double[] { 1.0 };
        Span<double> h1 = stackalloc double[4];
        int len1 = ExpansionArithmetic.GrowExpansion(e, 2.0, h1);
        Span<double> h2 = stackalloc double[8];
        int len2 = ExpansionArithmetic.GrowExpansion(h1[..len1], 3.0, h2);
        double sum = 0;
        for (int i = 0; i < len2; i++) sum += h2[i];
        Assert.Equal(6.0, sum, 1e-15);
    }

    [Fact]
    public void ScaleExpansion_ByZero()
    {
        Span<double> e = stackalloc double[] { 5.0 };
        Span<double> h = stackalloc double[4];
        int len = ExpansionArithmetic.ScaleExpansion(e, 0.0, h);
        double sum = 0;
        for (int i = 0; i < len; i++) sum += h[i];
        Assert.Equal(0.0, sum, 1e-15);
    }

    [Fact]
    public void ScaleExpansion_ByOne()
    {
        Span<double> e = stackalloc double[] { 5.0 };
        Span<double> h = stackalloc double[4];
        int len = ExpansionArithmetic.ScaleExpansion(e, 1.0, h);
        double sum = 0;
        for (int i = 0; i < len; i++) sum += h[i];
        Assert.Equal(5.0, sum, 1e-15);
    }

    [Fact]
    public void ExpansionSum_WithZero()
    {
        Span<double> a = stackalloc double[] { 0.0 };
        Span<double> b = stackalloc double[] { 5.0 };
        Span<double> h = stackalloc double[8];
        int len = ExpansionArithmetic.ExpansionSum(a, b, h);
        double sum = 0;
        for (int i = 0; i < len; i++) sum += h[i];
        Assert.Equal(5.0, sum, 1e-15);
    }

    [Fact]
    public void Sign_LargePositive()
    {
        Span<double> e = stackalloc double[] { 1e-20, 1e10 };
        Assert.Equal(1, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Sign_LargeNegative()
    {
        Span<double> e = stackalloc double[] { 1e-20, -1e10 };
        Assert.Equal(-1, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Sign_AllZeros()
    {
        Span<double> e = stackalloc double[] { 0.0, 0.0, 0.0 };
        Assert.Equal(0, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void TwoDiff_LargeAndSmall()
    {
        var (s, e) = ExpansionArithmetic.TwoDiff(1e15, 1e-5);
        Assert.Equal(1e15 - 1e-5, s + e, 1e-5);
    }
}
