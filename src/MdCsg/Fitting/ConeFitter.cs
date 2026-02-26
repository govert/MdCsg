using MdCsg.Math;

namespace MdCsg.Fitting;

/// <summary>
/// Fits a cone to a 3D point cloud using PCA initial guess + Nelder-Mead optimization.
/// </summary>
public static class ConeFitter
{
    /// <summary>
    /// Fits a cone to a set of 3D points.
    /// </summary>
    /// <param name="points">The 3D points (at least 5).</param>
    /// <returns>The fitted cone parameters.</returns>
    public static FittedCone Fit(Vec3[] points)
    {
        if (points.Length < 5)
            throw new ArgumentException("At least 5 points required.", nameof(points));

        int n = points.Length;

        // Compute centroid
        var centroid = Vec3.Zero;
        for (int i = 0; i < n; i++)
            centroid = centroid + points[i];
        centroid = centroid / n;

        // PCA for initial axis candidates
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
        var (_, eigenvectors) = Eigen3.Decompose(cov);

        // Data spread for step sizing
        double spread = 0;
        for (int i = 0; i < n; i++)
            spread += (points[i] - centroid).LengthSquared;
        spread = System.Math.Sqrt(spread / n);
        double posStep = spread * 0.1;
        if (posStep < 0.01) posStep = 0.01;
        var steps = new double[] { posStep, posStep, posStep, 0.2, 0.2, 0.1 };

        // Try all 3 eigenvectors as initial axis guesses, keep best
        NelderMeadResult bestResult = default;
        double bestValue = double.MaxValue;

        for (int ei = 0; ei < 3; ei++)
        {
            var candidateAxis = eigenvectors.Column(ei).Normalized;

            double sumAngle = 0;
            int angleCount = 0;
            for (int i = 0; i < n; i++)
            {
                var d = points[i] - centroid;
                double along = Vec3.Dot(d, candidateAxis);
                double perp = (d - candidateAxis * along).Length;
                if (System.Math.Abs(along) > 1e-10)
                {
                    sumAngle += System.Math.Atan2(perp, System.Math.Abs(along));
                    angleCount++;
                }
            }
            double halfAngleGuess = angleCount > 0 ? sumAngle / angleCount : 0.3;
            if (halfAngleGuess < 0.01) halfAngleGuess = 0.3;

            double[] initial = {
                centroid.X, centroid.Y, centroid.Z,
                System.Math.Atan2(candidateAxis.Y, candidateAxis.X),
                System.Math.Acos(MathUtil.Clamp(candidateAxis.Z, -1, 1)),
                halfAngleGuess
            };

            var result = NelderMead.Minimize(p => ConeObjective(p, points), initial, 3000, 1e-12, steps);
            if (result.Value < bestValue)
            {
                bestValue = result.Value;
                bestResult = result;
            }
        }

        var sol = bestResult.Solution;
        var apex = new Vec3(sol[0], sol[1], sol[2]);
        double azimuth = sol[3], inclination = sol[4];
        var axis = SphericalCoordinates.FromSpherical(azimuth, inclination, 1.0).Normalized;
        double halfAngle = System.Math.Abs(sol[5]);

        // Normalize: if half-angle > π/2, flip axis and use supplementary angle
        if (halfAngle > System.Math.PI / 2)
        {
            axis = axis * -1;
            halfAngle = System.Math.PI - halfAngle;
        }

        // Compute RMS
        double rms = System.Math.Sqrt(bestValue / n);

        return new FittedCone(apex, axis, halfAngle, rms);
    }

    private static double ConeObjective(double[] p, Vec3[] points)
    {
        var apex = new Vec3(p[0], p[1], p[2]);
        var axis = SphericalCoordinates.FromSpherical(p[3], p[4], 1.0).Normalized;
        double halfAngle = System.Math.Abs(p[5]);
        double tanHA = System.Math.Tan(halfAngle);

        double sumSqDist = 0;
        for (int i = 0; i < points.Length; i++)
        {
            var d = points[i] - apex;
            double along = Vec3.Dot(d, axis);
            double perp = (d - axis * along).Length;
            // Distance to cone surface: |perp - along * tan(halfAngle)| / sqrt(1 + tan^2)
            double dist = System.Math.Abs(perp - along * tanHA) / System.Math.Sqrt(1 + tanHA * tanHA);
            sumSqDist += dist * dist;
        }
        return sumSqDist;
    }
}
