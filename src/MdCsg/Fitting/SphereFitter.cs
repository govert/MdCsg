using MdCsg.Math;

namespace MdCsg.Fitting;

/// <summary>
/// Fits a sphere to a set of 3D points using algebraic least squares.
/// Centers the data for numerical stability, solves a 3x3 linear system,
/// then computes the radius as the mean distance from the fitted center.
/// </summary>
public static class SphereFitter
{
    /// <summary>
    /// Fits a sphere to the given points.
    /// </summary>
    /// <param name="points">At least 4 non-coplanar points.</param>
    /// <returns>The fitted sphere with center, radius, and RMS residual.</returns>
    public static FittedSphere Fit(Vec3[] points)
    {
        if (points.Length < 4)
            throw new ArgumentException("At least 4 points are required to fit a sphere.", nameof(points));

        int n = points.Length;

        // Center the data for numerical stability
        double mx = 0, my = 0, mz = 0;
        for (int i = 0; i < n; i++)
        {
            mx += points[i].X;
            my += points[i].Y;
            mz += points[i].Z;
        }
        double inv = 1.0 / n;
        mx *= inv; my *= inv; mz *= inv;

        // In centered coordinates x' = x - mx, etc., the sphere equation becomes:
        //   x'^2 + y'^2 + z'^2 + A*x' + B*y' + C*z' + D = 0
        // Since sum(x') = 0, the 4x4 normal equations decouple into a 3x3 system:
        //   [Sxx Sxy Sxz] [A]   [-Sxr']
        //   [Sxy Syy Syz] [B] = [-Syr']
        //   [Sxz Syz Szz] [C]   [-Szr']
        // where Sxr' = sum(x' * r'), r' = x'^2 + y'^2 + z'^2

        double sxx = 0, syy = 0, szz = 0;
        double sxy = 0, sxz = 0, syz = 0;
        double sxr = 0, syr = 0, szr = 0;

        for (int i = 0; i < n; i++)
        {
            double x = points[i].X - mx;
            double y = points[i].Y - my;
            double z = points[i].Z - mz;
            double r2 = x * x + y * y + z * z;

            sxx += x * x; syy += y * y; szz += z * z;
            sxy += x * y; sxz += x * z; syz += y * z;
            sxr += x * r2; syr += y * r2; szr += z * r2;
        }

        var lhs = new Mat3(sxx, sxy, sxz, sxy, syy, syz, sxz, syz, szz);
        var rhs = new Vec3(-sxr, -syr, -szr);

        if (!LinearSolve3.Solve(lhs, rhs, out var abc))
        {
            return FitFromCentroid(points);
        }

        // Convert back to original coordinates: center = mean + (-A/2, -B/2, -C/2)
        var center = new Vec3(mx - abc.X * 0.5, my - abc.Y * 0.5, mz - abc.Z * 0.5);

        // Compute radius and RMS
        double sumDist = 0;
        for (int i = 0; i < n; i++)
            sumDist += Vec3.Distance(points[i], center);
        double radius = sumDist / n;
        double rmsResidual = ComputeRms(points, center, radius);

        return new FittedSphere(center, radius, rmsResidual);
    }

    private static FittedSphere FitFromCentroid(Vec3[] points)
    {
        double cx = 0, cy = 0, cz = 0;
        for (int i = 0; i < points.Length; i++)
        {
            cx += points[i].X;
            cy += points[i].Y;
            cz += points[i].Z;
        }
        double inv = 1.0 / points.Length;
        var center = new Vec3(cx * inv, cy * inv, cz * inv);

        double sumDist = 0;
        for (int i = 0; i < points.Length; i++)
            sumDist += Vec3.Distance(points[i], center);
        double radius = sumDist * inv;

        return new FittedSphere(center, radius, ComputeRms(points, center, radius));
    }

    private static double ComputeRms(Vec3[] points, Vec3 center, double radius)
    {
        double sumSq = 0;
        for (int i = 0; i < points.Length; i++)
        {
            double err = Vec3.Distance(points[i], center) - radius;
            sumSq += err * err;
        }
        return System.Math.Sqrt(sumSq / points.Length);
    }
}
