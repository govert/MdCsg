using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Phase 6: ExpansionArithmetic deep tests — TwoSum, TwoDiff, TwoProduct exactness</summary>
public class ExpansionArithmeticDeepTests
{
    [Fact]
    public void TwoSum_ExactInDoubles()
    {
        var (s, e) = ExpansionArithmetic.TwoSum(1.0, 2.0);
        Assert.Equal(3.0, s, 15);
        Assert.Equal(0.0, e, 15);
    }

    [Fact]
    public void TwoSum_WithRoundingError()
    {
        // 1.0 + 1e-16: the sum loses precision, error captures it
        var (s, e) = ExpansionArithmetic.TwoSum(1.0, 1e-16);
        Assert.True(System.Math.Abs((s + e) - (1.0 + 1e-16)) < 1e-30,
            $"Sum+Error should be exact: s={s}, e={e}");
    }

    [Fact]
    public void TwoSum_LargeAndSmall()
    {
        var (s, e) = ExpansionArithmetic.TwoSum(1e15, 1.0);
        Assert.True(System.Math.Abs((s + e) - (1e15 + 1.0)) < 1e-1);
    }

    [Fact]
    public void TwoSum_Symmetric()
    {
        var (s1, e1) = ExpansionArithmetic.TwoSum(3.14, 2.72);
        var (s2, e2) = ExpansionArithmetic.TwoSum(2.72, 3.14);
        Assert.Equal(s1, s2, 15);
    }

    [Fact]
    public void TwoSum_NearCancellation()
    {
        // Use values where the exact result is known
        double a = 1.0 + 1e-15;
        double b = -(1.0);
        var (s, e) = ExpansionArithmetic.TwoSum(a, b);
        // s + e should equal a + b exactly
        Assert.Equal(a + b, s + e, 15);
    }

    [Fact]
    public void TwoDiff_Basic()
    {
        var (d, e) = ExpansionArithmetic.TwoDiff(5.0, 3.0);
        Assert.Equal(2.0, d, 15);
        Assert.Equal(0.0, e, 15);
    }

    [Fact]
    public void TwoDiff_NearCancellation()
    {
        double a = 1.0 + 1e-15;
        double b = 1.0;
        var (d, e) = ExpansionArithmetic.TwoDiff(a, b);
        // d + e should exactly equal a - b
        Assert.Equal(a - b, d + e, 15);
    }

    [Fact]
    public void TwoDiff_SameValue_Zero()
    {
        var (d, e) = ExpansionArithmetic.TwoDiff(42.0, 42.0);
        Assert.Equal(0.0, d, 15);
        Assert.Equal(0.0, e, 15);
    }

    [Fact]
    public void TwoProduct_ExactInDoubles()
    {
        var (p, e) = ExpansionArithmetic.TwoProduct(3.0, 4.0);
        Assert.Equal(12.0, p, 15);
        Assert.Equal(0.0, e, 10);
    }

    [Fact]
    public void TwoProduct_WithRoundingError()
    {
        // 1.1 * 1.1 = 1.21 approximately, but has rounding error
        var (p, e) = ExpansionArithmetic.TwoProduct(1.1, 1.1);
        Assert.True(System.Math.Abs((p + e) - 1.1 * 1.1) < 1e-25);
    }

    [Fact]
    public void TwoProduct_LargeValues()
    {
        var (p, e) = ExpansionArithmetic.TwoProduct(1e10, 1e10);
        Assert.True(System.Math.Abs(p - 1e20) < 1e5);
    }

    [Fact]
    public void TwoProduct_SmallValues()
    {
        var (p, e) = ExpansionArithmetic.TwoProduct(1e-10, 1e-10);
        Assert.True(System.Math.Abs((p + e) - 1e-20) < 1e-35);
    }

    [Fact]
    public void TwoProduct_ByZero_IsZero()
    {
        var (p, e) = ExpansionArithmetic.TwoProduct(42.0, 0.0);
        Assert.Equal(0.0, p, 15);
    }

    [Fact]
    public void TwoProduct_ByOne_Identity()
    {
        var (p, e) = ExpansionArithmetic.TwoProduct(3.14, 1.0);
        Assert.Equal(3.14, p, 15);
        Assert.Equal(0.0, e, 15);
    }

    [Fact]
    public void TwoSum_TwoDiff_Inverse()
    {
        // (a + b) - b should give a back
        double a = 3.14;
        double b = 2.72;
        var (s, es) = ExpansionArithmetic.TwoSum(a, b);
        var (d, ed) = ExpansionArithmetic.TwoDiff(s, b);
        Assert.True(System.Math.Abs((d + ed) - a) < 1e-14);
    }

    [Fact]
    public void TwoProduct_Commutative()
    {
        var (p1, e1) = ExpansionArithmetic.TwoProduct(3.14, 2.72);
        var (p2, e2) = ExpansionArithmetic.TwoProduct(2.72, 3.14);
        Assert.Equal(p1, p2, 15);
    }

    [Fact]
    public void TwoSum_NegativeValues()
    {
        var (s, e) = ExpansionArithmetic.TwoSum(-5.0, -3.0);
        Assert.Equal(-8.0, s, 15);
    }

    [Fact]
    public void TwoProduct_NegativeValues()
    {
        var (p, e) = ExpansionArithmetic.TwoProduct(-3.0, 4.0);
        Assert.Equal(-12.0, p, 15);
    }
}
