namespace MdCsg.Math;

/// <summary>
/// Common math constants and utility methods.
/// </summary>
public static class MathUtil
{
    /// <summary>
    /// Default geometric epsilon for approximate comparisons.
    /// </summary>
    public const double Epsilon = 1e-10;

    /// <summary>
    /// Default snap grid resolution for intersection point rounding.
    /// </summary>
    public const double DefaultGridSize = 1e-8;

    /// <summary>
    /// Snaps a value to the nearest multiple of <paramref name="gridSize"/>.
    /// </summary>
    public static double SnapToGrid(double value, double gridSize = DefaultGridSize)
    {
        return System.Math.Round(value / gridSize) * gridSize;
    }

    /// <summary>
    /// Fused multiply-add: returns a*b + c with a single rounding.
    /// </summary>
    public static double Fma(double a, double b, double c)
    {
        return System.Math.FusedMultiplyAdd(a, b, c);
    }
}
