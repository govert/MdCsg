using MdCsg.Math;

namespace MdCsg.Fitting;

/// <summary>
/// Fits a paraboloid z = ax^2 + bxy + cy^2 + dx + ey + f to 3D point data.
/// </summary>
public static class ParaboloidFitter
{
    /// <summary>
    /// Fits a paraboloid to a set of 3D points using 6x6 normal equations
    /// solved via <see cref="LinearSolveN"/>.
    /// </summary>
    /// <param name="points">The 3D points (at least 6). Z is the dependent variable.</param>
    /// <returns>The fitted paraboloid coefficients.</returns>
    public static FittedParaboloid Fit(Vec3[] points)
    {
        if (points.Length < 6)
            throw new ArgumentException("At least 6 points required.", nameof(points));

        int n = points.Length;

        // Center x and y for numerical stability
        double xMean = 0, yMean = 0;
        for (int i = 0; i < n; i++)
        {
            xMean += points[i].X;
            yMean += points[i].Y;
        }
        xMean /= n;
        yMean /= n;

        // Build 6x6 normal equations: columns are [x^2, xy, y^2, x, y, 1]
        var ata = new double[6, 6];
        var atb = new double[6];

        for (int i = 0; i < n; i++)
        {
            double x = points[i].X - xMean;
            double y = points[i].Y - yMean;
            double z = points[i].Z;

            double[] row = { x * x, x * y, y * y, x, y, 1.0 };

            for (int r = 0; r < 6; r++)
            {
                atb[r] += row[r] * z;
                for (int k = 0; k < 6; k++)
                    ata[r, k] += row[r] * row[k];
            }
        }

        if (!LinearSolveN.Solve(ata, atb, out var sol))
        {
            return new FittedParaboloid(0, 0, 0, 0, 0, 0, 0);
        }

        double a = sol[0], b = sol[1], c = sol[2];
        double d = sol[3], e = sol[4], f = sol[5];

        // Convert back from centered to original coordinates
        // z = a*(x-mx)^2 + b*(x-mx)*(y-my) + c*(y-my)^2 + d*(x-mx) + e*(y-my) + f
        double aO = a;
        double bO = b;
        double cO = c;
        double dO = d - 2 * a * xMean - b * yMean;
        double eO = e - 2 * c * yMean - b * xMean;
        double fO = a * xMean * xMean + b * xMean * yMean + c * yMean * yMean
                   - d * xMean - e * yMean + f;

        // Compute RMS
        double rss = 0;
        for (int i = 0; i < n; i++)
        {
            double x = points[i].X, y2 = points[i].Y, z = points[i].Z;
            double predicted = aO * x * x + bO * x * y2 + cO * y2 * y2 + dO * x + eO * y2 + fO;
            double residual = z - predicted;
            rss += residual * residual;
        }
        double rms = System.Math.Sqrt(rss / n);

        return new FittedParaboloid(aO, bO, cO, dO, eO, fO, rms);
    }
}
