using MdCsg.Math;

namespace MdCsg.Fitting;

/// <summary>
/// Line fitting for 2D and 3D point sets.
/// </summary>
public static class LineFitter
{
    /// <summary>
    /// Fits a 2D line y = slope*x + intercept using ordinary least squares (OLS).
    /// </summary>
    /// <param name="points">The 2D points (at least 2).</param>
    /// <returns>The fitted line parameters.</returns>
    public static FittedLine2D Fit(Vec2[] points)
    {
        if (points.Length < 2)
            throw new ArgumentException("At least 2 points required.", nameof(points));

        int n = points.Length;
        double sumX = 0, sumY = 0, sumXX = 0, sumXY = 0;
        for (int i = 0; i < n; i++)
        {
            sumX += points[i].X;
            sumY += points[i].Y;
            sumXX += points[i].X * points[i].X;
            sumXY += points[i].X * points[i].Y;
        }

        double det = n * sumXX - sumX * sumX;
        if (System.Math.Abs(det) < 1e-30)
        {
            // Vertical line or single point — return zero slope at mean
            return new FittedLine2D(0, sumY / n, 0);
        }

        double slope = (n * sumXY - sumX * sumY) / det;
        double intercept = (sumY - slope * sumX) / n;

        // Compute RMS residual
        double rss = 0;
        for (int i = 0; i < n; i++)
        {
            double residual = points[i].Y - (slope * points[i].X + intercept);
            rss += residual * residual;
        }
        double rms = System.Math.Sqrt(rss / n);

        return new FittedLine2D(slope, intercept, rms);
    }

    /// <summary>
    /// Fits a 3D line through a point cloud using PCA (largest eigenvector = line direction).
    /// </summary>
    /// <param name="points">The 3D points (at least 2).</param>
    /// <returns>The fitted line passing through the centroid.</returns>
    public static FittedLine3D Fit(Vec3[] points)
    {
        if (points.Length < 2)
            throw new ArgumentException("At least 2 points required.", nameof(points));

        int n = points.Length;

        // Compute centroid
        var centroid = Vec3.Zero;
        for (int i = 0; i < n; i++)
            centroid = centroid + points[i];
        centroid = centroid / n;

        // Build covariance matrix
        double c00 = 0, c01 = 0, c02 = 0, c11 = 0, c12 = 0, c22 = 0;
        for (int i = 0; i < n; i++)
        {
            var d = points[i] - centroid;
            c00 += d.X * d.X;
            c01 += d.X * d.Y;
            c02 += d.X * d.Z;
            c11 += d.Y * d.Y;
            c12 += d.Y * d.Z;
            c22 += d.Z * d.Z;
        }
        double inv = 1.0 / n;
        var cov = Mat3.Symmetric(c00 * inv, c01 * inv, c02 * inv, c11 * inv, c12 * inv, c22 * inv);

        // Eigendecomposition — largest eigenvector is the line direction
        var (eigenvalues, eigenvectors) = Eigen3.Decompose(cov);
        var direction = eigenvectors.Column(2).Normalized; // ascending order → largest is index 2

        // RMS: sqrt of sum of the two smaller eigenvalues
        double rms = System.Math.Sqrt(eigenvalues.X + eigenvalues.Y);

        return new FittedLine3D(centroid, direction, rms);
    }
}
