namespace MdCsg.Math;

/// <summary>
/// Conversion between Cartesian and spherical coordinates (physics convention).
/// Azimuth is the angle in the XY plane from the X axis, inclination is the angle from the Z axis.
/// </summary>
public static class SphericalCoordinates
{
    /// <summary>
    /// Converts a Cartesian vector to spherical coordinates.
    /// </summary>
    /// <param name="v">The Cartesian vector.</param>
    /// <returns>
    /// Azimuth (radians, in [-pi, pi] from +X toward +Y),
    /// inclination (radians, in [0, pi] from +Z),
    /// and distance (non-negative).
    /// </returns>
    public static (double Azimuth, double Inclination, double Distance) ToSpherical(Vec3 v)
    {
        double r = v.Length;
        if (r < 1e-30)
            return (0, 0, 0);

        double azimuth = System.Math.Atan2(v.Y, v.X);
        double inclination = System.Math.Acos(MathUtil.Clamp(v.Z / r, -1, 1));
        return (azimuth, inclination, r);
    }

    /// <summary>
    /// Converts spherical coordinates to a Cartesian vector.
    /// </summary>
    /// <param name="azimuth">Angle in the XY plane from the X axis (radians).</param>
    /// <param name="inclination">Angle from the Z axis (radians).</param>
    /// <param name="distance">Radial distance (non-negative).</param>
    /// <returns>The Cartesian vector.</returns>
    public static Vec3 FromSpherical(double azimuth, double inclination, double distance)
    {
        double sinInc = System.Math.Sin(inclination);
        return new Vec3(
            distance * sinInc * System.Math.Cos(azimuth),
            distance * sinInc * System.Math.Sin(azimuth),
            distance * System.Math.Cos(inclination));
    }
}
