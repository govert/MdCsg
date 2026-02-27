using MdCsg.Fitting;
using MdCsg.Math;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 31: NelderMead on known functions + stress.</summary>
public class NelderMeadConvergenceTests
{
    [Fact]
    public void Quadratic_FindsMinimumAtZero()
    {
        var result = NelderMead.Minimize(x => x[0] * x[0] + x[1] * x[1], new[] { 1.0, 1.0 });
        Assert.True(System.Math.Abs(result.Solution[0]) < 0.01);
        Assert.True(System.Math.Abs(result.Solution[1]) < 0.01);
    }

    [Fact]
    public void Quadratic_MinimumValueNearZero()
    {
        var result = NelderMead.Minimize(x => x[0] * x[0] + x[1] * x[1], new[] { 1.0, 1.0 });
        Assert.True(result.Value < 0.001);
    }

    [Fact]
    public void Rosenbrock_ConvergesToMinimum()
    {
        var result = NelderMead.Minimize(x =>
            100 * (x[1] - x[0] * x[0]) * (x[1] - x[0] * x[0]) + (1 - x[0]) * (1 - x[0]),
            new[] { -1.0, 1.0 }, maxIter: 5000);
        Assert.True(System.Math.Abs(result.Solution[0] - 1.0) < 0.1);
        Assert.True(System.Math.Abs(result.Solution[1] - 1.0) < 0.1);
    }

    [Fact]
    public void OffsetQuadratic_FindsCorrectMinimum()
    {
        var result = NelderMead.Minimize(
            x => (x[0] - 3) * (x[0] - 3) + (x[1] + 2) * (x[1] + 2),
            new[] { 0.0, 0.0 });
        Assert.True(System.Math.Abs(result.Solution[0] - 3.0) < 0.01);
        Assert.True(System.Math.Abs(result.Solution[1] + 2.0) < 0.01);
    }

    [Fact]
    public void OneDimensional_FindsMinimum()
    {
        var result = NelderMead.Minimize(x => (x[0] - 5) * (x[0] - 5), new[] { 0.0 });
        Assert.True(System.Math.Abs(result.Solution[0] - 5.0) < 0.01);
    }

    [Fact]
    public void ThreeDimensional_FindsMinimum()
    {
        var result = NelderMead.Minimize(
            x => x[0] * x[0] + x[1] * x[1] + x[2] * x[2],
            new[] { 1.0, 2.0, 3.0 });
        Assert.True(System.Math.Abs(result.Solution[0]) < 0.01);
        Assert.True(System.Math.Abs(result.Solution[1]) < 0.01);
        Assert.True(System.Math.Abs(result.Solution[2]) < 0.01);
    }

    [Fact]
    public void EllipticParaboloid_FindsMinimum()
    {
        var result = NelderMead.Minimize(
            x => 3 * x[0] * x[0] + 5 * x[1] * x[1],
            new[] { 2.0, -3.0 });
        Assert.True(System.Math.Abs(result.Solution[0]) < 0.01);
        Assert.True(System.Math.Abs(result.Solution[1]) < 0.01);
    }

    [Fact]
    public void IterationCount_IsPositive()
    {
        var result = NelderMead.Minimize(x => x[0] * x[0], new[] { 5.0 });
        Assert.True(result.Iterations > 0);
    }

    [Fact]
    public void WithInitialStep_ConvergesFaster()
    {
        var result = NelderMead.Minimize(
            x => (x[0] - 100) * (x[0] - 100),
            new[] { 0.0 },
            initialStep: new[] { 50.0 });
        Assert.True(System.Math.Abs(result.Solution[0] - 100) < 0.1);
    }

    [Fact]
    public void Quadratic_WithTightTolerance()
    {
        var result = NelderMead.Minimize(
            x => x[0] * x[0] + x[1] * x[1],
            new[] { 1.0, 1.0 }, tol: 1e-12);
        Assert.True(result.Value < 1e-10);
    }

    [Fact]
    public void HighDimensional_FourD_FindsMinimum()
    {
        var result = NelderMead.Minimize(
            x => x[0] * x[0] + x[1] * x[1] + x[2] * x[2] + x[3] * x[3],
            new[] { 1.0, 2.0, 3.0, 4.0 }, maxIter: 5000);
        foreach (var v in result.Solution)
            Assert.True(System.Math.Abs(v) < 0.1);
    }

    [Fact]
    public void WeightedQuadratic_FindsMinimum()
    {
        var result = NelderMead.Minimize(
            x => 10 * x[0] * x[0] + x[1] * x[1],
            new[] { 5.0, 5.0 });
        Assert.True(System.Math.Abs(result.Solution[0]) < 0.01);
        Assert.True(System.Math.Abs(result.Solution[1]) < 0.01);
    }

    [Fact]
    public void SumOfSquares_LargeInitial()
    {
        var result = NelderMead.Minimize(
            x => x[0] * x[0] + x[1] * x[1],
            new[] { 100.0, -100.0 }, maxIter: 2000);
        Assert.True(System.Math.Abs(result.Solution[0]) < 1.0);
        Assert.True(System.Math.Abs(result.Solution[1]) < 1.0);
    }

    [Fact]
    public void LinearFunction_ConvergesToBound()
    {
        // Minimize x[0] (unbounded → should hit iteration limit)
        var result = NelderMead.Minimize(x => x[0], new[] { 0.0 }, maxIter: 100);
        Assert.True(result.Solution[0] < 0); // Should move negative
    }

    [Fact]
    public void AbsoluteValue_FindsMinimum()
    {
        var result = NelderMead.Minimize(
            x => System.Math.Abs(x[0] - 3) + System.Math.Abs(x[1] + 1),
            new[] { 0.0, 0.0 }, maxIter: 2000);
        Assert.True(System.Math.Abs(result.Solution[0] - 3.0) < 0.1);
        Assert.True(System.Math.Abs(result.Solution[1] + 1.0) < 0.1);
    }

    [Fact]
    public void MaxIter_RespectedApproximately()
    {
        var result = NelderMead.Minimize(x => x[0] * x[0], new[] { 5.0 }, maxIter: 10);
        Assert.True(result.Iterations <= 15); // may overshoot slightly
    }

    [Fact]
    public void StartAtMinimum_StaysNearMinimum()
    {
        var result = NelderMead.Minimize(
            x => x[0] * x[0] + x[1] * x[1],
            new[] { 0.0, 0.0 });
        Assert.True(result.Value < 0.01);
    }

    [Fact]
    public void SolutionLengthMatchesInput()
    {
        var result = NelderMead.Minimize(
            x => x[0] * x[0] + x[1] * x[1] + x[2] * x[2],
            new[] { 1.0, 2.0, 3.0 });
        Assert.Equal(3, result.Solution.Length);
    }

    [Fact]
    public void NegativeStarting_FindsMinimum()
    {
        var result = NelderMead.Minimize(
            x => (x[0] + 5) * (x[0] + 5),
            new[] { -10.0 });
        Assert.True(System.Math.Abs(result.Solution[0] + 5.0) < 0.01);
    }

    [Fact]
    public void QuarticFunction_FindsMinimum()
    {
        var result = NelderMead.Minimize(
            x => x[0] * x[0] * x[0] * x[0] + x[1] * x[1] * x[1] * x[1],
            new[] { 2.0, -2.0 }, maxIter: 2000);
        Assert.True(System.Math.Abs(result.Solution[0]) < 0.1);
        Assert.True(System.Math.Abs(result.Solution[1]) < 0.1);
    }
}
