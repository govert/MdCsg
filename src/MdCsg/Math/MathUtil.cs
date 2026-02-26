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
    /// Clamps a value to the range [min, max].
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <param name="min">The minimum bound.</param>
    /// <param name="max">The maximum bound.</param>
    /// <returns>The clamped value.</returns>
    public static double Clamp(double value, double min, double max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    /// <summary>
    /// Fused multiply-add: returns a*b + c with a single rounding.
    /// </summary>
    public static double Fma(double a, double b, double c)
    {
#if NET
        return System.Math.FusedMultiplyAdd(a, b, c);
#else
        return a * b + c;
#endif
    }
}
