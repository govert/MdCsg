using MdCsg.Arithmetic;

namespace MdCsg.Tests.Arithmetic;

/// <summary>Phase 6: ExpansionArithmetic — extreme values, overflow, precision properties</summary>
public class ExpansionArithmeticOverflowPropertyTests
{
    [Fact]
    public void TwoSum_SmallValues_ExactSum()
    {
        var (sum, err) = ExpansionArithmetic.TwoSum(1.0, 1e-16);
        Assert.Equal(1.0 + 1e-16, sum + err, 10);
    }

    [Fact]
    public void TwoSum_OppositeValues_ExactCancel()
    {
        var (sum, err) = ExpansionArithmetic.TwoSum(1.0, -1.0);
        Assert.Equal(0.0, sum + err, 15);
    }

    [Fact]
    public void TwoDiff_SameValue_Zero()
    {
        var (diff, err) = ExpansionArithmetic.TwoDiff(42.0, 42.0);
        Assert.Equal(0.0, diff + err, 15);
    }

    [Fact]
    public void TwoProduct_SmallValues_ExactProduct()
    {
        var (prod, err) = ExpansionArithmetic.TwoProduct(3.0, 7.0);
        Assert.Equal(21.0, prod + err, 15);
    }

    [Fact]
    public void TwoProduct_NearOneValues_ExactProduct()
    {
        var (prod, err) = ExpansionArithmetic.TwoProduct(1.0 + 1e-15, 1.0 - 1e-15);
        // Should be very close to 1 - 1e-30
        Assert.True(System.Math.Abs(prod + err - (1.0 - 1e-30)) < 1e-28);
    }

    [Fact]
    public void TwoSum_LargeAndSmall_PreservesSmall()
    {
        double large = 1e15;
        double small = 1.0;
        var (sum, err) = ExpansionArithmetic.TwoSum(large, small);
        Assert.Equal(large + small, sum + err, 10);
    }

    [Fact]
    public void TwoProduct_ZeroTimes_Zero()
    {
        var (prod, err) = ExpansionArithmetic.TwoProduct(0.0, 42.0);
        Assert.Equal(0.0, prod + err, 15);
    }

    [Fact]
    public void TwoProduct_OneTimes_Identity()
    {
        var (prod, err) = ExpansionArithmetic.TwoProduct(1.0, 42.5);
        Assert.Equal(42.5, prod + err, 15);
    }

    [Fact]
    public void TwoDiff_NearbyValues_SmallResult()
    {
        double a = 1.0000000000001;
        double b = 1.0;
        var (diff, err) = ExpansionArithmetic.TwoDiff(a, b);
        Assert.True(System.Math.Abs(diff + err - (a - b)) < 1e-28);
    }

    [Fact]
    public void GrowExpansion_SingleElement_CorrectSum()
    {
        ReadOnlySpan<double> e = new double[] { 3.0 };
        Span<double> h = stackalloc double[2];
        int len = ExpansionArithmetic.GrowExpansion(e, 4.0, h);
        double sum = 0;
        for (int i = 0; i < len; i++) sum += h[i];
        Assert.Equal(7.0, sum, 15);
    }

    [Fact]
    public void GrowExpansion_MultipleElements_CorrectSum()
    {
        ReadOnlySpan<double> e = new double[] { 1e-16, 1.0 };
        Span<double> h = stackalloc double[3];
        int len = ExpansionArithmetic.GrowExpansion(e, 2.0, h);
        double sum = 0;
        for (int i = 0; i < len; i++) sum += h[i];
        Assert.Equal(3.0 + 1e-16, sum, 10);
    }

    [Fact]
    public void ScaleExpansion_ByOne_SameValue()
    {
        ReadOnlySpan<double> e = new double[] { 3.0 };
        Span<double> h = stackalloc double[4];
        int len = ExpansionArithmetic.ScaleExpansion(e, 1.0, h);
        double sum = 0;
        for (int i = 0; i < len; i++) sum += h[i];
        Assert.Equal(3.0, sum, 15);
    }

    [Fact]
    public void ScaleExpansion_ByZero_Zero()
    {
        ReadOnlySpan<double> e = new double[] { 42.0 };
        Span<double> h = stackalloc double[4];
        int len = ExpansionArithmetic.ScaleExpansion(e, 0.0, h);
        double sum = 0;
        for (int i = 0; i < len; i++) sum += h[i];
        Assert.Equal(0.0, sum, 15);
    }

    [Fact]
    public void ScaleExpansion_ByTwo_DoubleValue()
    {
        ReadOnlySpan<double> e = new double[] { 7.5 };
        Span<double> h = stackalloc double[4];
        int len = ExpansionArithmetic.ScaleExpansion(e, 2.0, h);
        double sum = 0;
        for (int i = 0; i < len; i++) sum += h[i];
        Assert.Equal(15.0, sum, 15);
    }

    [Fact]
    public void TwoSum_Commutative()
    {
        var (s1, e1) = ExpansionArithmetic.TwoSum(1.23456789, 9.87654321);
        var (s2, e2) = ExpansionArithmetic.TwoSum(9.87654321, 1.23456789);
        Assert.Equal(s1 + e1, s2 + e2, 15);
    }

    [Fact]
    public void TwoProduct_Commutative()
    {
        var (p1, e1) = ExpansionArithmetic.TwoProduct(1.23456789, 9.87654321);
        var (p2, e2) = ExpansionArithmetic.TwoProduct(9.87654321, 1.23456789);
        Assert.Equal(p1 + e1, p2 + e2, 15);
    }

    [Fact]
    public void Det3x3Sign_ExactlyZero_Singular()
    {
        // Singular matrix: third row is sum of first two
        int sign = AdaptivePrecision.Det3x3Sign(
            1, 0, 0,
            0, 1, 0,
            1, 1, 0); // row3 = row1 + row2 → det should be 0
        Assert.Equal(0, sign);
    }

    [Fact]
    public void Det3x3Sign_Identity_Positive()
    {
        int sign = AdaptivePrecision.Det3x3Sign(
            1, 0, 0,
            0, 1, 0,
            0, 0, 1);
        Assert.Equal(1, sign);
    }

    [Fact]
    public void Det3x3Sign_SwapRows_FlipsSign()
    {
        int s1 = AdaptivePrecision.Det3x3Sign(
            1, 2, 3,
            4, 5, 6,
            7, 8, 10);
        int s2 = AdaptivePrecision.Det3x3Sign(
            4, 5, 6,
            1, 2, 3,
            7, 8, 10);
        Assert.Equal(-s1, s2);
    }
}
