using MdCsg.Arithmetic;
using MdCsg.Math;

namespace MdCsg.Predicates;

/// <summary>
/// Robust 2D orientation predicate using adaptive precision arithmetic.
/// </summary>
public static class Orient2D
{
    /// <summary>
    /// Returns the orientation of point C with respect to the directed line from A to B.
    /// Positive means C is to the left (counter-clockwise), negative to the right (clockwise),
    /// zero means collinear.
    /// </summary>
    /// <remarks>
    /// Computes the sign of the determinant:
    /// | ax-cx  ay-cy |
    /// | bx-cx  by-cy |
    /// </remarks>
    public static PredicateSign Evaluate(Vec2 a, Vec2 b, Vec2 c)
    {
        var sign = AdaptivePrecision.Det2x2Sign(
            a.X - c.X, a.Y - c.Y,
            b.X - c.X, b.Y - c.Y);
        return (PredicateSign)sign;
    }
}
