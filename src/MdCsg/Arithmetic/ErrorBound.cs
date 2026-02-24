namespace MdCsg.Arithmetic;

/// <summary>
/// Precomputed error bound constants for adaptive geometric predicates.
/// Based on Shewchuk's error analysis.
/// </summary>
public static class ErrorBound
{
    /// <summary>Machine epsilon for IEEE 754 double: 2^-53.</summary>
    public const double Epsilon = 1.1102230246251565e-16; // 2^-53

    /// <summary>Error bound for the fast (double-precision) orient2d filter.</summary>
    public static readonly double Orient2DErrorBoundA = (3.0 + 16.0 * Epsilon) * Epsilon;

    /// <summary>Error bound for the B-level orient2d filter.</summary>
    public static readonly double Orient2DErrorBoundB = (2.0 + 12.0 * Epsilon) * Epsilon;

    /// <summary>Error bound for the C-level orient2d filter.</summary>
    public static readonly double Orient2DErrorBoundC = (9.0 + 64.0 * Epsilon) * Epsilon * Epsilon;

    /// <summary>Error bound for the fast orient3d filter.</summary>
    public static readonly double Orient3DErrorBoundA = (7.0 + 56.0 * Epsilon) * Epsilon;

    /// <summary>Error bound for the B-level orient3d filter.</summary>
    public static readonly double Orient3DErrorBoundB = (3.0 + 28.0 * Epsilon) * Epsilon;

    /// <summary>Error bound for the C-level orient3d filter.</summary>
    public static readonly double Orient3DErrorBoundC = (26.0 + 288.0 * Epsilon) * Epsilon * Epsilon;

    /// <summary>Error bound for the fast incircle filter.</summary>
    public static readonly double InCircleErrorBoundA = (10.0 + 96.0 * Epsilon) * Epsilon;

    /// <summary>Error bound for the B-level incircle filter.</summary>
    public static readonly double InCircleErrorBoundB = (4.0 + 48.0 * Epsilon) * Epsilon;

    /// <summary>Error bound for the C-level incircle filter.</summary>
    public static readonly double InCircleErrorBoundC = (44.0 + 576.0 * Epsilon) * Epsilon * Epsilon;

    /// <summary>General result bound coefficient.</summary>
    public static readonly double ResultErrBound = (3.0 + 8.0 * Epsilon) * Epsilon;
}
