using MdCsg.Arithmetic;
using MdCsg.Math;

namespace MdCsg.Predicates;

/// <summary>
/// Robust 3D orientation predicate using adaptive precision arithmetic.
/// </summary>
public static class Orient3D
{
    /// <summary>
    /// Returns the orientation of point D with respect to the plane defined by A, B, C.
    /// Positive means D is above the plane (on the side the normal (B-A)×(C-A) points to),
    /// negative means below, zero means coplanar.
    /// </summary>
    /// <remarks>
    /// Computes the sign of det(B-A, C-A, D-A) = (B-A)·((C-A)×(D-A)).
    /// </remarks>
    public static PredicateSign Evaluate(Vec3 a, Vec3 b, Vec3 c, Vec3 d)
    {
        var sign = AdaptivePrecision.Det3x3Sign(
            b.X - a.X, b.Y - a.Y, b.Z - a.Z,
            c.X - a.X, c.Y - a.Y, c.Z - a.Z,
            d.X - a.X, d.Y - a.Y, d.Z - a.Z);
        return (PredicateSign)sign;
    }
}
