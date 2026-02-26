using MdCsg.Math;

namespace MdCsg.Fitting;

/// <summary>
/// Fits a plane to a set of 3D points using principal component analysis (PCA).
/// The normal is the eigenvector corresponding to the smallest eigenvalue
/// of the covariance matrix.
/// </summary>
public static class PlaneFitter
{
    /// <summary>
    /// Fits a plane to the given points.
    /// </summary>
    /// <param name="points">At least 3 non-collinear points.</param>
    /// <returns>The fitted plane with centroid, normal, and RMS residual.</returns>
    public static FittedPlane Fit(Vec3[] points)
    {
        if (points.Length < 3)
            throw new ArgumentException("At least 3 points are required to fit a plane.", nameof(points));

        // Compute centroid
        double cx = 0, cy = 0, cz = 0;
        for (int i = 0; i < points.Length; i++)
        {
            cx += points[i].X;
            cy += points[i].Y;
            cz += points[i].Z;
        }
        double inv = 1.0 / points.Length;
        var centroid = new Vec3(cx * inv, cy * inv, cz * inv);

        // Build 3x3 covariance matrix
        double cov00 = 0, cov01 = 0, cov02 = 0;
        double cov11 = 0, cov12 = 0;
        double cov22 = 0;

        for (int i = 0; i < points.Length; i++)
        {
            double dx = points[i].X - centroid.X;
            double dy = points[i].Y - centroid.Y;
            double dz = points[i].Z - centroid.Z;

            cov00 += dx * dx;
            cov01 += dx * dy;
            cov02 += dx * dz;
            cov11 += dy * dy;
            cov12 += dy * dz;
            cov22 += dz * dz;
        }

        var cov = Mat3.Symmetric(cov00 * inv, cov01 * inv, cov02 * inv,
            cov11 * inv, cov12 * inv, cov22 * inv);

        // Eigendecompose: smallest eigenvalue's eigenvector = normal
        var (eigenvalues, eigenvectors) = Eigen3.Decompose(cov);

        // Eigenvalues are sorted ascending, so column 0 = smallest
        var normal = eigenvectors.Column(0).Normalized;
        double rms = System.Math.Sqrt(System.Math.Max(0, eigenvalues.X));

        return new FittedPlane(centroid, normal, rms);
    }
}
