namespace MdCsg.Math;

/// <summary>
/// Solves NxN linear systems using Gaussian elimination with partial pivoting.
/// </summary>
internal static class LinearSolveN
{
    /// <summary>
    /// Solves Ax = b for an NxN system. Returns false if the system is singular.
    /// </summary>
    /// <param name="a">The NxN coefficient matrix (will be modified in place).</param>
    /// <param name="b">The N-element right-hand side (will be modified in place).</param>
    /// <param name="x">The solution vector.</param>
    /// <returns>True if the system was solved, false if singular.</returns>
    internal static bool Solve(double[,] a, double[] b, out double[] x)
    {
        int n = b.Length;
        x = new double[n];

        // Forward elimination with partial pivoting
        for (int col = 0; col < n; col++)
        {
            // Find pivot
            int maxRow = col;
            double maxVal = System.Math.Abs(a[col, col]);
            for (int row = col + 1; row < n; row++)
            {
                double val = System.Math.Abs(a[row, col]);
                if (val > maxVal)
                {
                    maxVal = val;
                    maxRow = row;
                }
            }

            if (maxVal < 1e-30) return false;

            // Swap rows
            if (maxRow != col)
            {
                for (int j = col; j < n; j++)
                {
                    double tmp = a[col, j];
                    a[col, j] = a[maxRow, j];
                    a[maxRow, j] = tmp;
                }
                double bt = b[col];
                b[col] = b[maxRow];
                b[maxRow] = bt;
            }

            // Eliminate below
            double pivot = a[col, col];
            for (int row = col + 1; row < n; row++)
            {
                double factor = a[row, col] / pivot;
                a[row, col] = 0;
                for (int j = col + 1; j < n; j++)
                    a[row, j] -= factor * a[col, j];
                b[row] -= factor * b[col];
            }
        }

        // Back-substitution
        for (int row = n - 1; row >= 0; row--)
        {
            if (System.Math.Abs(a[row, row]) < 1e-30) return false;
            double sum = b[row];
            for (int j = row + 1; j < n; j++)
                sum -= a[row, j] * x[j];
            x[row] = sum / a[row, row];
        }

        return true;
    }
}
