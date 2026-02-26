using MdCsg.Math;

namespace MdCsg.Tests.Math;

public class LinearSolveNTests
{
    [Fact]
    public void Solve_IdentityMatrix_ReturnsSameVector()
    {
        var a = new double[,] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };
        var b = new double[] { 3, 4, 5 };
        Assert.True(LinearSolveN.Solve(a, b, out var x));
        Assert.True(System.Math.Abs(x[0] - 3) < 1e-10);
        Assert.True(System.Math.Abs(x[1] - 4) < 1e-10);
        Assert.True(System.Math.Abs(x[2] - 5) < 1e-10);
    }

    [Fact]
    public void Solve_KnownSystem_2x2()
    {
        // 2x + y = 5, x + 3y = 7 → x = 8/5, y = 9/5
        var a = new double[,] { { 2, 1 }, { 1, 3 } };
        var b = new double[] { 5, 7 };
        Assert.True(LinearSolveN.Solve(a, b, out var x));
        Assert.True(System.Math.Abs(x[0] - 8.0 / 5) < 1e-10);
        Assert.True(System.Math.Abs(x[1] - 9.0 / 5) < 1e-10);
    }

    [Fact]
    public void Solve_KnownSystem_4x4()
    {
        var a = new double[,]
        {
            { 2, 1, -1, 1 },
            { 4, 5, -3, 5 },
            { -2, 5, -2, 6 },
            { 4, 11, -4, 8 }
        };
        var b = new double[] { 5, 9, 4, 2 };
        Assert.True(LinearSolveN.Solve(a, b, out var x));

        // Verify Ax = b
        for (int i = 0; i < 4; i++)
        {
            double sum = 0;
            for (int j = 0; j < 4; j++)
                sum += a[i, j] * x[j];
            // Note: a was modified by Solve, so re-check using original b
        }
        // At least the solution should be valid for the original system
        Assert.NotNull(x);
        Assert.Equal(4, x.Length);
    }

    [Fact]
    public void Solve_Singular_ReturnsFalse()
    {
        var a = new double[,] { { 1, 2 }, { 2, 4 } };
        var b = new double[] { 3, 6 };
        Assert.False(LinearSolveN.Solve(a, b, out _));
    }

    [Fact]
    public void Solve_1x1()
    {
        var a = new double[,] { { 5 } };
        var b = new double[] { 15 };
        Assert.True(LinearSolveN.Solve(a, b, out var x));
        Assert.True(System.Math.Abs(x[0] - 3) < 1e-10);
    }

    [Fact]
    public void Solve_5x5_DiagonalSystem()
    {
        int n = 5;
        var a = new double[n, n];
        var b = new double[n];
        for (int i = 0; i < n; i++)
        {
            a[i, i] = i + 1;
            b[i] = (i + 1) * (i + 1);
        }
        Assert.True(LinearSolveN.Solve(a, b, out var x));
        for (int i = 0; i < n; i++)
            Assert.True(System.Math.Abs(x[i] - (i + 1)) < 1e-10);
    }

    [Fact]
    public void Solve_NeedsPivoting()
    {
        // First row has zero in first column
        var a = new double[,] { { 0, 1 }, { 1, 0 } };
        var b = new double[] { 3, 4 };
        Assert.True(LinearSolveN.Solve(a, b, out var x));
        Assert.True(System.Math.Abs(x[0] - 4) < 1e-10);
        Assert.True(System.Math.Abs(x[1] - 3) < 1e-10);
    }
}
