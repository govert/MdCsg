using MdCsg.Fitting;

namespace MdCsg.Tests.Fitting;

public class NelderMeadTests
{
    [Fact]
    public void Minimize_SphereFunction_FindsOrigin()
    {
        // f(x,y) = x^2 + y^2, min at (0,0)
        var result = NelderMead.Minimize(p => p[0] * p[0] + p[1] * p[1], new[] { 1.0, 1.0 });
        Assert.True(System.Math.Abs(result.Solution[0]) < 1e-4, $"x should be near 0, got {result.Solution[0]}");
        Assert.True(System.Math.Abs(result.Solution[1]) < 1e-4, $"y should be near 0, got {result.Solution[1]}");
        Assert.True(result.Value < 1e-8);
    }

    [Fact]
    public void Minimize_Rosenbrock_NearMinimum()
    {
        // f(x,y) = (1-x)^2 + 100*(y-x^2)^2, min at (1,1)
        var result = NelderMead.Minimize(p =>
        {
            double x = p[0], y = p[1];
            return (1 - x) * (1 - x) + 100 * (y - x * x) * (y - x * x);
        }, new[] { -1.0, 1.0 }, 10000);

        Assert.True(System.Math.Abs(result.Solution[0] - 1) < 0.01,
            $"x should be near 1, got {result.Solution[0]}");
        Assert.True(System.Math.Abs(result.Solution[1] - 1) < 0.01,
            $"y should be near 1, got {result.Solution[1]}");
    }

    [Fact]
    public void Minimize_1D_FindsMinimum()
    {
        // f(x) = (x-3)^2
        var result = NelderMead.Minimize(p => (p[0] - 3) * (p[0] - 3), new[] { 0.0 }, initialStep: new[] { 1.0 });
        Assert.True(System.Math.Abs(result.Solution[0] - 3) < 1e-4, $"x should be near 3, got {result.Solution[0]}");
    }

    [Fact]
    public void Minimize_3D_SphereFunction()
    {
        var result = NelderMead.Minimize(
            p => p[0] * p[0] + p[1] * p[1] + p[2] * p[2],
            new[] { 5.0, -3.0, 2.0 });
        Assert.True(result.Value < 1e-10);
    }

    [Fact]
    public void Minimize_Convergence_ReturnsIterationCount()
    {
        var result = NelderMead.Minimize(p => p[0] * p[0], new[] { 10.0 }, 100);
        Assert.True(result.Iterations > 0);
        Assert.True(result.Iterations <= 100);
    }

    [Fact]
    public void Minimize_KnownMinima_Beale()
    {
        // Beale function: min at (3, 0.5)
        var result = NelderMead.Minimize(p =>
        {
            double x = p[0], y = p[1];
            double t1 = 1.5 - x + x * y;
            double t2 = 2.25 - x + x * y * y;
            double t3 = 2.625 - x + x * y * y * y;
            return t1 * t1 + t2 * t2 + t3 * t3;
        }, new[] { 1.0, 1.0 }, 5000);

        Assert.True(System.Math.Abs(result.Solution[0] - 3) < 0.1,
            $"x should be near 3, got {result.Solution[0]}");
        Assert.True(System.Math.Abs(result.Solution[1] - 0.5) < 0.1,
            $"y should be near 0.5, got {result.Solution[1]}");
    }
}
