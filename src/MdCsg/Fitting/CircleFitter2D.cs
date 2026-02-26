using MdCsg.Math;

namespace MdCsg.Fitting;

/// <summary>
/// Fits a 2D circle to a set of 2D points using algebraic least squares.
/// </summary>
internal static class CircleFitter2D
{
    /// <summary>
    /// Fits a 2D circle to the given points.
    /// Returns the center (cx, cy) and radius.
    /// </summary>
    internal static (double Cx, double Cy, double Radius) Fit(Vec2[] points)
    {
        int n = points.Length;
        if (n < 3)
            return FitFromCentroid(points);

        // Center the data for numerical stability (same approach as SphereFitter).
        // In centered coordinates x' = x - mx, the circle equation becomes:
        //   x'^2 + y'^2 + A*x' + B*y' + D = 0
        // Since sum(x') = sum(y') = 0, the normal equations decouple into a 2x2 system.
        double mx = 0, my = 0;
        for (int i = 0; i < n; i++)
        {
            mx += points[i].X;
            my += points[i].Y;
        }
        double inv = 1.0 / n;
        mx *= inv;
        my *= inv;

        double sxx = 0, syy = 0, sxy = 0;
        double sxr = 0, syr = 0;

        for (int i = 0; i < n; i++)
        {
            double x = points[i].X - mx;
            double y = points[i].Y - my;
            double r2 = x * x + y * y;
            sxx += x * x;
            syy += y * y;
            sxy += x * y;
            sxr += x * r2;
            syr += y * r2;
        }

        double det = sxx * syy - sxy * sxy;
        if (System.Math.Abs(det) < 1e-30)
            return FitFromCentroid(points);

        double a = (-sxr * syy + syr * sxy) / det;
        double b = (-syr * sxx + sxr * sxy) / det;

        // Convert back to original coordinates
        double cx = mx - a * 0.5;
        double cy = my - b * 0.5;

        double sumDist = 0;
        for (int i = 0; i < n; i++)
        {
            double dx = points[i].X - cx;
            double dy = points[i].Y - cy;
            sumDist += System.Math.Sqrt(dx * dx + dy * dy);
        }
        double radius = sumDist / n;

        return (cx, cy, radius);
    }

    private static (double Cx, double Cy, double Radius) FitFromCentroid(Vec2[] points)
    {
        double cx = 0, cy = 0;
        for (int i = 0; i < points.Length; i++)
        {
            cx += points[i].X;
            cy += points[i].Y;
        }
        double inv = 1.0 / System.Math.Max(1, points.Length);
        cx *= inv;
        cy *= inv;

        double sumDist = 0;
        for (int i = 0; i < points.Length; i++)
        {
            double dx = points[i].X - cx;
            double dy = points[i].Y - cy;
            sumDist += System.Math.Sqrt(dx * dx + dy * dy);
        }
        return (cx, cy, sumDist * inv);
    }
}
