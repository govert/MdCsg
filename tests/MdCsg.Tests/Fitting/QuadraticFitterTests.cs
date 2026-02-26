using MdCsg.Fitting;

namespace MdCsg.Tests.Fitting;

public class QuadraticFitterTests
{
    [Fact]
    public void Fit1D_PerfectParabola_RecoverCoefficients()
    {
        // y = 2x^2 + 3x + 1
        var x = new double[] { -2, -1, 0, 1, 2, 3 };
        var y = new double[6];
        for (int i = 0; i < 6; i++)
            y[i] = 2 * x[i] * x[i] + 3 * x[i] + 1;

        var result = QuadraticFitter.Fit1D(x, y);
        Assert.True(System.Math.Abs(result.A - 2) < 1e-8, $"A should be 2, got {result.A}");
        Assert.True(System.Math.Abs(result.B - 3) < 1e-8, $"B should be 3, got {result.B}");
        Assert.True(System.Math.Abs(result.C - 1) < 1e-8, $"C should be 1, got {result.C}");
        Assert.True(result.RmsResidual < 1e-8);
    }

    [Fact]
    public void Fit1D_LinearData_ZeroA()
    {
        // y = 5x + 2 (no quadratic term)
        var x = new double[] { 0, 1, 2, 3, 4, 5 };
        var y = new double[6];
        for (int i = 0; i < 6; i++) y[i] = 5 * x[i] + 2;

        var result = QuadraticFitter.Fit1D(x, y);
        Assert.True(System.Math.Abs(result.A) < 1e-8, $"A should be 0, got {result.A}");
        Assert.True(System.Math.Abs(result.B - 5) < 1e-8, $"B should be 5, got {result.B}");
    }

    [Fact]
    public void Fit1D_ConstantData_ZeroAB()
    {
        var x = new double[] { 0, 1, 2, 3 };
        var y = new double[] { 7, 7, 7, 7 };

        var result = QuadraticFitter.Fit1D(x, y);
        Assert.True(System.Math.Abs(result.A) < 1e-8);
        Assert.True(System.Math.Abs(result.B) < 1e-8);
        Assert.True(System.Math.Abs(result.C - 7) < 1e-8);
    }

    [Fact]
    public void Fit1D_NoisyData_ReasonableCoefficients()
    {
        var rng = new Random(42);
        int n = 100;
        var x = new double[n];
        var y = new double[n];
        for (int i = 0; i < n; i++)
        {
            x[i] = (i - 50) * 0.1;
            y[i] = 3 * x[i] * x[i] - 2 * x[i] + 1 + (rng.NextDouble() - 0.5) * 0.5;
        }

        var result = QuadraticFitter.Fit1D(x, y);
        Assert.True(System.Math.Abs(result.A - 3) < 0.2, $"A should be ~3, got {result.A}");
        Assert.True(System.Math.Abs(result.B + 2) < 0.2, $"B should be ~-2, got {result.B}");
    }

    [Fact]
    public void Fit1D_OffsetParabola_CorrectVertex()
    {
        // y = (x - 5)^2 = x^2 - 10x + 25
        var x = new double[] { 3, 4, 5, 6, 7 };
        var y = new double[5];
        for (int i = 0; i < 5; i++)
            y[i] = (x[i] - 5) * (x[i] - 5);

        var result = QuadraticFitter.Fit1D(x, y);
        Assert.True(System.Math.Abs(result.A - 1) < 1e-8);
        Assert.True(System.Math.Abs(result.B + 10) < 1e-8);
        Assert.True(System.Math.Abs(result.C - 25) < 1e-8);
    }

    [Fact]
    public void Fit1D_TooFewPoints_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            QuadraticFitter.Fit1D(new double[] { 0, 1 }, new double[] { 0, 1 }));
    }
}
