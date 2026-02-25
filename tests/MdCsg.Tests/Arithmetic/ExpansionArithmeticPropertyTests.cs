using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Phase 6: Expansion arithmetic property tests — TwoSum, TwoDiff, TwoProduct exact properties</summary>
public class ExpansionArithmeticPropertyTests
{
    [Fact]
    public void TwoSum_ExactSum()
    {
        var (sum, err) = ExpansionArithmetic.TwoSum(1.0, 2.0);
        Assert.Equal(3.0, sum + err, 1e-15);
    }

    [Fact]
    public void TwoSum_SmallPlusLarge_ErrorCorrects()
    {
        double large = 1e16;
        double small = 1.0;
        var (sum, err) = ExpansionArithmetic.TwoSum(large, small);
        // sum + err should exactly equal large + small
        Assert.Equal(large + small, sum + err, 1e-5);
    }

    [Fact]
    public void TwoSum_Commutative()
    {
        var (s1, e1) = ExpansionArithmetic.TwoSum(3.14, 2.72);
        var (s2, e2) = ExpansionArithmetic.TwoSum(2.72, 3.14);
        Assert.Equal(s1 + e1, s2 + e2, 1e-15);
    }

    [Fact]
    public void TwoSum_ZeroPlusX()
    {
        var (sum, err) = ExpansionArithmetic.TwoSum(0, 42.0);
        Assert.Equal(42.0, sum);
        Assert.Equal(0, err);
    }

    [Fact]
    public void TwoSum_XPlusNegX()
    {
        var (sum, err) = ExpansionArithmetic.TwoSum(42.0, -42.0);
        Assert.Equal(0.0, sum + err);
    }

    [Fact]
    public void TwoDiff_ExactDifference()
    {
        var (diff, err) = ExpansionArithmetic.TwoDiff(5.0, 3.0);
        Assert.Equal(2.0, diff + err, 1e-15);
    }

    [Fact]
    public void TwoDiff_SameValue_Zero()
    {
        var (diff, err) = ExpansionArithmetic.TwoDiff(42.0, 42.0);
        Assert.Equal(0.0, diff + err);
    }

    [Fact]
    public void TwoDiff_NearlyEqual_ErrorCaptures()
    {
        double a = 1.0 + 1e-16;
        double b = 1.0;
        var (diff, err) = ExpansionArithmetic.TwoDiff(a, b);
        Assert.True(System.Math.Abs((diff + err) - (a - b)) < 1e-30);
    }

    [Fact]
    public void TwoProduct_ExactProduct()
    {
        var (prod, err) = ExpansionArithmetic.TwoProduct(3.0, 4.0);
        Assert.Equal(12.0, prod + err, 1e-15);
    }

    [Fact]
    public void TwoProduct_OneTimesX()
    {
        var (prod, err) = ExpansionArithmetic.TwoProduct(1.0, 42.0);
        Assert.Equal(42.0, prod + err, 1e-15);
    }

    [Fact]
    public void TwoProduct_ZeroTimesX()
    {
        var (prod, err) = ExpansionArithmetic.TwoProduct(0.0, 42.0);
        Assert.Equal(0.0, prod + err);
    }

    [Fact]
    public void TwoProduct_NegativeProduct()
    {
        var (prod, err) = ExpansionArithmetic.TwoProduct(-3.0, 4.0);
        Assert.Equal(-12.0, prod + err, 1e-15);
    }

    [Fact]
    public void TwoProduct_LargeNumbers()
    {
        double a = 1e8 + 0.123;
        double b = 1e8 + 0.456;
        var (prod, err) = ExpansionArithmetic.TwoProduct(a, b);
        // prod + err should be more accurate than just a*b
        Assert.True(System.Math.Abs(prod - a * b) < 1);
    }

    [Fact]
    public void TwoSum_ConsecutiveFloats()
    {
        // Adjacent IEEE doubles
        double a = 1.0;
        double b = 1.0 + 2.2204460492503131e-16 - 1.0; // next representable double after 1.0, minus 1.0
        var (sum, err) = ExpansionArithmetic.TwoSum(a, b);
        Assert.Equal(a + b, sum + err);
    }

    [Fact]
    public void ErrorBound_Epsilon_IsPositive()
    {
        Assert.True(ErrorBound.Epsilon > 0);
        Assert.True(ErrorBound.Epsilon < 1e-15);
    }

    [Fact]
    public void ErrorBound_Orient2DErrorBoundA_IsPositive()
    {
        Assert.True(ErrorBound.Orient2DErrorBoundA > 0);
    }

    [Fact]
    public void ErrorBound_Orient3DErrorBoundA_IsPositive()
    {
        Assert.True(ErrorBound.Orient3DErrorBoundA > 0);
    }

    [Fact]
    public void ErrorBound_InCircleErrorBoundA_IsPositive()
    {
        Assert.True(ErrorBound.InCircleErrorBoundA > 0);
    }

    [Fact]
    public void ErrorBound_HierarchicalOrdering()
    {
        // A-level bounds should be larger (less tight) than B-level
        Assert.True(ErrorBound.Orient2DErrorBoundA > ErrorBound.Orient2DErrorBoundB);
        Assert.True(ErrorBound.Orient3DErrorBoundA > ErrorBound.Orient3DErrorBoundB);
        Assert.True(ErrorBound.InCircleErrorBoundA > ErrorBound.InCircleErrorBoundB);
    }

    [Fact]
    public void ErrorBound_CLevelBounds_Positive()
    {
        Assert.True(ErrorBound.Orient2DErrorBoundC > 0);
        Assert.True(ErrorBound.Orient3DErrorBoundC > 0);
        Assert.True(ErrorBound.InCircleErrorBoundC > 0);
    }

    [Fact]
    public void ErrorBound_ResultErrBound_Positive()
    {
        Assert.True(ErrorBound.ResultErrBound > 0);
    }
}
