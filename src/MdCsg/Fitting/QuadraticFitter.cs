using MdCsg.Math;

namespace MdCsg.Fitting;

/// <summary>
/// Fits a 1D quadratic y = ax^2 + bx + c to (x, y) data.
/// </summary>
public static class QuadraticFitter
{
    /// <summary>
    /// Fits a quadratic to (x, y) data points using normal equations via <see cref="LinearSolve3"/>.
    /// </summary>
    /// <param name="x">The x values (at least 3).</param>
    /// <param name="y">The y values (same length as x).</param>
    /// <returns>The fitted quadratic coefficients.</returns>
    public static FittedQuadratic1D Fit1D(double[] x, double[] y)
    {
        if (x.Length < 3 || x.Length != y.Length)
            throw new ArgumentException("At least 3 matching x/y pairs required.");

        int n = x.Length;

        // Center the x data for numerical stability
        double xMean = 0;
        for (int i = 0; i < n; i++) xMean += x[i];
        xMean /= n;

        // Normal equations for y = a*xc^2 + b*xc + c  where xc = x - xMean
        double s0 = n, s1 = 0, s2 = 0, s3 = 0, s4 = 0;
        double r0 = 0, r1 = 0, r2 = 0;
        for (int i = 0; i < n; i++)
        {
            double xc = x[i] - xMean;
            double x2 = xc * xc;
            s1 += xc;
            s2 += x2;
            s3 += x2 * xc;
            s4 += x2 * x2;
            r0 += y[i];
            r1 += y[i] * xc;
            r2 += y[i] * x2;
        }

        // Solve [s4 s3 s2] [a]   [r2]
        //       [s3 s2 s1] [b] = [r1]
        //       [s2 s1 s0] [c]   [r0]
        var mat = new Mat3(s4, s3, s2, s3, s2, s1, s2, s1, s0);
        var rhs = new Vec3(r2, r1, r0);

        if (!LinearSolve3.Solve(mat, rhs, out var sol))
        {
            return new FittedQuadratic1D(0, 0, r0 / n, 0);
        }

        double a = sol.X;
        double b = sol.Y;
        double c = sol.Z;

        // Convert back from centered coordinates: y = a*(x-m)^2 + b*(x-m) + c
        // = a*x^2 - 2am*x + am^2 + bx - bm + c
        // = a*x^2 + (b - 2am)*x + (am^2 - bm + c)
        double aOrig = a;
        double bOrig = b - 2 * a * xMean;
        double cOrig = a * xMean * xMean - b * xMean + c;

        // Compute RMS residual
        double rss = 0;
        for (int i = 0; i < n; i++)
        {
            double predicted = aOrig * x[i] * x[i] + bOrig * x[i] + cOrig;
            double residual = y[i] - predicted;
            rss += residual * residual;
        }
        double rms = System.Math.Sqrt(rss / n);

        return new FittedQuadratic1D(aOrig, bOrig, cOrig, rms);
    }
}
