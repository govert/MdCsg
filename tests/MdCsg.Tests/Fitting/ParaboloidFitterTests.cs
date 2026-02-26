using MdCsg.Fitting;
using MdCsg.Math;

namespace MdCsg.Tests.Fitting;

public class ParaboloidFitterTests
{
    [Fact]
    public void Fit_FlatPlane_AllCoeffsNearZero()
    {
        // z = 5 (constant)
        var points = new Vec3[9];
        int k = 0;
        for (int i = -1; i <= 1; i++)
        for (int j = -1; j <= 1; j++)
            points[k++] = new Vec3(i, j, 5);

        var result = ParaboloidFitter.Fit(points);
        Assert.True(System.Math.Abs(result.A) < 1e-8);
        Assert.True(System.Math.Abs(result.B) < 1e-8);
        Assert.True(System.Math.Abs(result.C) < 1e-8);
        Assert.True(System.Math.Abs(result.F - 5) < 1e-8);
        Assert.True(result.RmsResidual < 1e-8);
    }

    [Fact]
    public void Fit_Bowl_RecoverCoefficients()
    {
        // z = x^2 + y^2
        var points = new Vec3[25];
        int k = 0;
        for (int i = -2; i <= 2; i++)
        for (int j = -2; j <= 2; j++)
            points[k++] = new Vec3(i, j, i * i + j * j);

        var result = ParaboloidFitter.Fit(points);
        Assert.True(System.Math.Abs(result.A - 1) < 1e-8, $"A should be 1, got {result.A}");
        Assert.True(System.Math.Abs(result.C - 1) < 1e-8, $"C should be 1, got {result.C}");
        Assert.True(System.Math.Abs(result.B) < 1e-8, $"B should be 0, got {result.B}");
    }

    [Fact]
    public void Fit_Saddle_RecoverCoefficients()
    {
        // z = x^2 - y^2
        var points = new Vec3[25];
        int k = 0;
        for (int i = -2; i <= 2; i++)
        for (int j = -2; j <= 2; j++)
            points[k++] = new Vec3(i, j, i * i - j * j);

        var result = ParaboloidFitter.Fit(points);
        Assert.True(System.Math.Abs(result.A - 1) < 1e-8, $"A should be 1, got {result.A}");
        Assert.True(System.Math.Abs(result.C + 1) < 1e-8, $"C should be -1, got {result.C}");
    }

    [Fact]
    public void Fit_WithLinearTerms_RecoverAll()
    {
        // z = x^2 + 2*x + 3*y + 4
        var points = new Vec3[25];
        int k = 0;
        for (int i = -2; i <= 2; i++)
        for (int j = -2; j <= 2; j++)
            points[k++] = new Vec3(i, j, i * i + 2 * i + 3 * j + 4);

        var result = ParaboloidFitter.Fit(points);
        Assert.True(System.Math.Abs(result.A - 1) < 1e-8);
        Assert.True(System.Math.Abs(result.D - 2) < 1e-8, $"D should be 2, got {result.D}");
        Assert.True(System.Math.Abs(result.E - 3) < 1e-8, $"E should be 3, got {result.E}");
        Assert.True(System.Math.Abs(result.F - 4) < 1e-8, $"F should be 4, got {result.F}");
    }

    [Fact]
    public void Fit_TooFewPoints_Throws()
    {
        var points = new Vec3[5];
        Assert.Throws<ArgumentException>(() => ParaboloidFitter.Fit(points));
    }
}
