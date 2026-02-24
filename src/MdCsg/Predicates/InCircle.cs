using MdCsg.Arithmetic;
using MdCsg.Math;

namespace MdCsg.Predicates;

/// <summary>
/// Robust in-circle predicate using adaptive precision arithmetic.
/// Used for constrained Delaunay triangulation.
/// </summary>
public static class InCircle
{
    /// <summary>
    /// Tests whether point D lies inside the circumcircle of triangle ABC.
    /// Returns Positive if inside, Negative if outside, Zero if exactly on the circle.
    /// Points A, B, C must be in counter-clockwise order.
    /// </summary>
    /// <remarks>
    /// Computes the sign of the determinant:
    /// | ax-dx  ay-dy  (ax-dx)²+(ay-dy)² |
    /// | bx-dx  by-dy  (bx-dx)²+(by-dy)² |
    /// | cx-dx  cy-dy  (cx-dx)²+(cy-dy)² |
    /// </remarks>
    public static PredicateSign Evaluate(Vec2 a, Vec2 b, Vec2 c, Vec2 d)
    {
        var adx = a.X - d.X;
        var ady = a.Y - d.Y;
        var bdx = b.X - d.X;
        var bdy = b.Y - d.Y;
        var cdx = c.X - d.X;
        var cdy = c.Y - d.Y;

        var sign = AdaptivePrecision.Det3x3Sign(
            adx, ady, adx * adx + ady * ady,
            bdx, bdy, bdx * bdx + bdy * bdy,
            cdx, cdy, cdx * cdx + cdy * cdy);
        return (PredicateSign)sign;
    }
}
