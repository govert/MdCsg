using MdCsg.Math;

namespace MdCsg.Predicates;

/// <summary>
/// Classifies a point relative to a plane using robust Orient3D.
/// </summary>
public static class PlaneClassification
{
    /// <summary>
    /// Classifies a point relative to a plane defined by three points.
    /// </summary>
    /// <param name="planeA">First point on the plane.</param>
    /// <param name="planeB">Second point on the plane.</param>
    /// <param name="planeC">Third point on the plane.</param>
    /// <param name="point">The point to classify.</param>
    /// <returns>Positive if above (on normal side), Negative if below, Zero if on the plane.</returns>
    public static PredicateSign Classify(Vec3 planeA, Vec3 planeB, Vec3 planeC, Vec3 point) =>
        Orient3D.Evaluate(planeA, planeB, planeC, point);
}
