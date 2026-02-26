using MdCsg.Math;

namespace MdCsg.Fitting;

/// <summary>
/// Fits a cylinder to a set of 3D points using a two-stage approach:
/// (1) estimate the axis via PCA (largest eigenvector, or smallest eigenvector
/// of normal covariance if normals are available), then
/// (2) project points perpendicular to the axis and fit a 2D circle for radius and center.
/// </summary>
public static class CylinderFitter
{
    /// <summary>
    /// Fits a cylinder to the given points. Uses point PCA to estimate the axis direction.
    /// </summary>
    /// <param name="points">At least 5 points distributed around and along the cylinder.</param>
    /// <param name="normals">Optional per-point surface normals. If provided, the axis is estimated
    /// from the normal covariance (smallest eigenvector) which is typically more robust.</param>
    /// <param name="axisHint">Optional axis direction hint. If provided, the PCA result is only used
    /// if it roughly agrees with the hint; otherwise the hint is used directly.</param>
    /// <returns>The fitted cylinder with base center, axis, radius, height, and RMS residual.</returns>
    public static FittedCylinder Fit(Vec3[] points, Vec3[]? normals = null, Vec3? axisHint = null)
    {
        if (points.Length < 5)
            throw new ArgumentException("At least 5 points are required to fit a cylinder.", nameof(points));

        // Stage 1: Estimate axis direction
        Vec3 axis = EstimateAxis(points, normals);

        // If an axis hint is provided and PCA disagrees significantly, use the hint
        if (axisHint.HasValue)
        {
            var hint = axisHint.Value.Normalized;
            double agreement = System.Math.Abs(Vec3.Dot(axis, hint));
            if (agreement < 0.7) // More than ~45 degrees off
                axis = hint;
        }

        // Ensure consistent axis direction (prefer positive Z, then Y, then X)
        if (axis.Z < -1e-6 || (System.Math.Abs(axis.Z) < 1e-6 && axis.Y < -1e-6) ||
            (System.Math.Abs(axis.Z) < 1e-6 && System.Math.Abs(axis.Y) < 1e-6 && axis.X < 0))
            axis = -axis;

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

        // Stage 2: Project to 2D perpendicular to axis, fit circle
        // Build orthonormal basis (u, v) perpendicular to axis
        var helper = System.Math.Abs(axis.X) < 0.9 ? Vec3.UnitX : Vec3.UnitY;
        var u = Vec3.Cross(axis, helper).Normalized;
        var v = Vec3.Cross(axis, u);

        // Project points to 2D (u,v) plane through centroid
        var pts2d = new Vec2[points.Length];
        var axialPositions = new double[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            var d = points[i] - centroid;
            pts2d[i] = new Vec2(Vec3.Dot(d, u), Vec3.Dot(d, v));
            axialPositions[i] = Vec3.Dot(d, axis);
        }

        var (cx2d, cy2d, radius) = CircleFitter2D.Fit(pts2d);

        // Reconstruct 3D center from 2D circle center
        var center3d = centroid + u * cx2d + v * cy2d;

        // Compute axial extent (height)
        double minAxial = double.MaxValue;
        double maxAxial = double.MinValue;
        for (int i = 0; i < axialPositions.Length; i++)
        {
            if (axialPositions[i] < minAxial) minAxial = axialPositions[i];
            if (axialPositions[i] > maxAxial) maxAxial = axialPositions[i];
        }
        double height = maxAxial - minAxial;

        // Base center is at the bottom of the axial extent
        var baseCenter = center3d + axis * minAxial;

        // Compute RMS residual (radial distance error)
        double sumSq = 0;
        for (int i = 0; i < points.Length; i++)
        {
            var d = points[i] - baseCenter;
            double axialDist = Vec3.Dot(d, axis);
            var radialVec = d - axis * axialDist;
            double radialDist = radialVec.Length;
            double err = radialDist - radius;
            sumSq += err * err;
        }
        double rms = System.Math.Sqrt(sumSq / points.Length);

        return new FittedCylinder(baseCenter, axis, radius, height, rms);
    }

    private static Vec3 EstimateAxis(Vec3[] points, Vec3[]? normals)
    {
        if (normals != null && normals.Length == points.Length)
        {
            // Normal covariance: smallest eigenvector = axis direction
            // (normals of a cylinder are all perpendicular to the axis)
            return EstimateAxisFromNormals(normals);
        }

        // Point covariance: largest eigenvector = axis direction
        // (points on a cylinder are most spread along the axis)
        return EstimateAxisFromPoints(points);
    }

    private static Vec3 EstimateAxisFromPoints(Vec3[] points)
    {
        double cx = 0, cy = 0, cz = 0;
        for (int i = 0; i < points.Length; i++)
        {
            cx += points[i].X;
            cy += points[i].Y;
            cz += points[i].Z;
        }
        double inv = 1.0 / points.Length;
        cx *= inv; cy *= inv; cz *= inv;

        double c00 = 0, c01 = 0, c02 = 0;
        double c11 = 0, c12 = 0;
        double c22 = 0;

        for (int i = 0; i < points.Length; i++)
        {
            double dx = points[i].X - cx;
            double dy = points[i].Y - cy;
            double dz = points[i].Z - cz;
            c00 += dx * dx; c01 += dx * dy; c02 += dx * dz;
            c11 += dy * dy; c12 += dy * dz;
            c22 += dz * dz;
        }

        var cov = Mat3.Symmetric(c00 * inv, c01 * inv, c02 * inv, c11 * inv, c12 * inv, c22 * inv);
        var (eigenvalues, eigenvectors) = Eigen3.Decompose(cov);

        // Largest eigenvalue's eigenvector (column 2, since sorted ascending)
        return eigenvectors.Column(2).Normalized;
    }

    private static Vec3 EstimateAxisFromNormals(Vec3[] normals)
    {
        double cx = 0, cy = 0, cz = 0;
        for (int i = 0; i < normals.Length; i++)
        {
            cx += normals[i].X;
            cy += normals[i].Y;
            cz += normals[i].Z;
        }
        double inv = 1.0 / normals.Length;
        cx *= inv; cy *= inv; cz *= inv;

        double c00 = 0, c01 = 0, c02 = 0;
        double c11 = 0, c12 = 0;
        double c22 = 0;

        for (int i = 0; i < normals.Length; i++)
        {
            double dx = normals[i].X - cx;
            double dy = normals[i].Y - cy;
            double dz = normals[i].Z - cz;
            c00 += dx * dx; c01 += dx * dy; c02 += dx * dz;
            c11 += dy * dy; c12 += dy * dz;
            c22 += dz * dz;
        }

        var cov = Mat3.Symmetric(c00 * inv, c01 * inv, c02 * inv, c11 * inv, c12 * inv, c22 * inv);
        var (eigenvalues, eigenvectors) = Eigen3.Decompose(cov);

        // Smallest eigenvalue's eigenvector (column 0)
        return eigenvectors.Column(0).Normalized;
    }
}
