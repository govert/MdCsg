using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Phase 6: ExpansionArithmetic — TwoSum, TwoDiff, TwoProduct, GrowExpansion, Sign, Compress, ErrorBound</summary>
public class ExpansionArithmeticPropertyTests
{
    // --- TwoSum ---

    [Fact]
    public void TwoSum_ExactForSmallIntegers()
    {
        var (sum, err) = ExpansionArithmetic.TwoSum(3.0, 5.0);
        Assert.Equal(8.0, sum);
        Assert.Equal(0.0, err);
    }

    [Fact]
    public void TwoSum_PreservesExactSum()
    {
        double a = 1.0, b = 1e-16;
        var (sum, err) = ExpansionArithmetic.TwoSum(a, b);
        // a + b = sum + err exactly
        Assert.True(System.Math.Abs((sum + err) - (a + b)) < 1e-30);
    }

    [Fact]
    public void TwoSum_Commutative()
    {
        double a = 1.5, b = 2.5;
        var (s1, e1) = ExpansionArithmetic.TwoSum(a, b);
        var (s2, e2) = ExpansionArithmetic.TwoSum(b, a);
        Assert.Equal(s1, s2);
    }

    [Fact]
    public void TwoSum_ZeroPlusX_IsX()
    {
        var (sum, err) = ExpansionArithmetic.TwoSum(0.0, 42.0);
        Assert.Equal(42.0, sum);
        Assert.Equal(0.0, err);
    }

    // --- TwoDiff ---

    [Fact]
    public void TwoDiff_ExactForSmallIntegers()
    {
        var (diff, err) = ExpansionArithmetic.TwoDiff(8.0, 3.0);
        Assert.Equal(5.0, diff);
        Assert.Equal(0.0, err);
    }

    [Fact]
    public void TwoDiff_SameValue_Zero()
    {
        var (diff, err) = ExpansionArithmetic.TwoDiff(42.0, 42.0);
        Assert.Equal(0.0, diff);
        Assert.Equal(0.0, err);
    }

    [Fact]
    public void TwoDiff_PreservesExactDifference()
    {
        double a = 1.0, b = 1e-16;
        var (diff, err) = ExpansionArithmetic.TwoDiff(a, b);
        Assert.True(System.Math.Abs((diff + err) - (a - b)) < 1e-30);
    }

    // --- TwoProduct ---

    [Fact]
    public void TwoProduct_ExactForSmallIntegers()
    {
        var (prod, err) = ExpansionArithmetic.TwoProduct(3.0, 5.0);
        Assert.Equal(15.0, prod);
        Assert.Equal(0.0, err);
    }

    [Fact]
    public void TwoProduct_ZeroTimesX_Zero()
    {
        var (prod, err) = ExpansionArithmetic.TwoProduct(0.0, 42.0);
        Assert.Equal(0.0, prod);
        Assert.Equal(0.0, err);
    }

    [Fact]
    public void TwoProduct_PreservesExactProduct()
    {
        double a = 1.0 + 1e-15, b = 1.0 + 1e-15;
        var (prod, err) = ExpansionArithmetic.TwoProduct(a, b);
        // prod + err should be exact
        Assert.True(System.Math.Abs(prod - a * b) < 1e-25);
    }

    // --- Sign ---

    [Fact]
    public void Sign_PositiveExpansion()
    {
        Span<double> e = stackalloc double[] { 0.0, 1.0 };
        Assert.Equal(1, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Sign_NegativeExpansion()
    {
        Span<double> e = stackalloc double[] { 0.0, -1.0 };
        Assert.Equal(-1, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Sign_ZeroExpansion()
    {
        Span<double> e = stackalloc double[] { 0.0, 0.0 };
        Assert.Equal(0, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Sign_SinglePositive()
    {
        Span<double> e = stackalloc double[] { 5.0 };
        Assert.Equal(1, ExpansionArithmetic.Sign(e));
    }

    [Fact]
    public void Sign_EmptyExpansion()
    {
        Assert.Equal(0, ExpansionArithmetic.Sign(ReadOnlySpan<double>.Empty));
    }

    // --- GrowExpansion ---

    [Fact]
    public void GrowExpansion_AddToEmpty_SingleElement()
    {
        Span<double> h = stackalloc double[2];
        int len = ExpansionArithmetic.GrowExpansion(ReadOnlySpan<double>.Empty, 5.0, h);
        Assert.Equal(1, len);
        Assert.Equal(5.0, h[0]);
    }

    [Fact]
    public void GrowExpansion_ExactIntegers_NoError()
    {
        Span<double> e = stackalloc double[] { 3.0 };
        Span<double> h = stackalloc double[3];
        int len = ExpansionArithmetic.GrowExpansion(e, 5.0, h);
        // Sum of expansion should be 8.0
        double total = 0;
        for (int i = 0; i < len; i++) total += h[i];
        Assert.True(System.Math.Abs(total - 8.0) < 1e-15);
    }

    // --- Compress ---

    [Fact]
    public void Compress_AlreadyCompressed_SameLength()
    {
        Span<double> e = stackalloc double[] { 1.0 };
        Span<double> h = stackalloc double[2];
        int len = ExpansionArithmetic.Compress(e, h);
        Assert.Equal(1, len);
        Assert.Equal(1.0, h[0]);
    }

    [Fact]
    public void Compress_EmptyExpansion_Zero()
    {
        int len = ExpansionArithmetic.Compress(ReadOnlySpan<double>.Empty, Span<double>.Empty);
        Assert.Equal(0, len);
    }

    [Fact]
    public void Compress_WithZeros_Reduced()
    {
        Span<double> e = stackalloc double[] { 0.0, 0.0, 5.0 };
        Span<double> h = stackalloc double[4];
        int len = ExpansionArithmetic.Compress(e, h);
        // Should compress away zeros
        double total = 0;
        for (int i = 0; i < len; i++) total += h[i];
        Assert.True(System.Math.Abs(total - 5.0) < 1e-15);
    }

    // --- Negate ---

    [Fact]
    public void Negate_FlipsSign()
    {
        Span<double> e = stackalloc double[] { 1.0, -2.0, 3.0 };
        ExpansionArithmetic.Negate(e);
        Assert.Equal(-1.0, e[0]);
        Assert.Equal(2.0, e[1]);
        Assert.Equal(-3.0, e[2]);
    }

    // --- ErrorBound ---

    [Fact]
    public void ErrorBound_Epsilon_IsIEEE754()
    {
        Assert.True(System.Math.Abs(ErrorBound.Epsilon - System.Math.Pow(2, -53)) < 1e-30);
    }

    [Fact]
    public void ErrorBound_Orient2D_BoundsArePositive()
    {
        Assert.True(ErrorBound.Orient2DErrorBoundA > 0);
        Assert.True(ErrorBound.Orient2DErrorBoundB > 0);
        Assert.True(ErrorBound.Orient2DErrorBoundC > 0);
    }

    [Fact]
    public void ErrorBound_Orient3D_BoundsArePositive()
    {
        Assert.True(ErrorBound.Orient3DErrorBoundA > 0);
        Assert.True(ErrorBound.Orient3DErrorBoundB > 0);
        Assert.True(ErrorBound.Orient3DErrorBoundC > 0);
    }

    [Fact]
    public void ErrorBound_InCircle_BoundsArePositive()
    {
        Assert.True(ErrorBound.InCircleErrorBoundA > 0);
        Assert.True(ErrorBound.InCircleErrorBoundB > 0);
        Assert.True(ErrorBound.InCircleErrorBoundC > 0);
    }

    [Fact]
    public void ErrorBound_ResultErrBound_IsPositive()
    {
        Assert.True(ErrorBound.ResultErrBound > 0);
    }

    [Fact]
    public void ErrorBound_BoundsAreTiny()
    {
        // All bounds should be much smaller than 1
        Assert.True(ErrorBound.Orient2DErrorBoundA < 1e-10);
        Assert.True(ErrorBound.Orient3DErrorBoundA < 1e-10);
        Assert.True(ErrorBound.InCircleErrorBoundA < 1e-10);
    }

    [Fact]
    public void ErrorBound_CLevels_SmallerThanALevels()
    {
        // C-level bounds use Epsilon^2, so they should be smaller than A-level bounds
        Assert.True(ErrorBound.Orient2DErrorBoundC < ErrorBound.Orient2DErrorBoundA);
        Assert.True(ErrorBound.Orient3DErrorBoundC < ErrorBound.Orient3DErrorBoundA);
        Assert.True(ErrorBound.InCircleErrorBoundC < ErrorBound.InCircleErrorBoundA);
    }
}
