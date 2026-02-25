using MdCsg.Math;

namespace MdCsg.Classification;

/// <summary>
/// Classifies a point as inside or outside a solid region, and computes
/// distance to the region's surface. Abstracts the "other solid" in
/// patch classification so that both mesh-backed solids and analytic
/// primitives (half-spaces, etc.) can be used interchangeably.
/// </summary>
public interface IPointClassifier
{
    /// <summary>
    /// Classifies a point as inside or outside the solid region.
    /// </summary>
    SolidClassification Classify(Vec3 point);

    /// <summary>
    /// Returns the (unsigned) distance from the point to the nearest surface of the solid region.
    /// Used as the confidence margin for patch classification.
    /// </summary>
    double DistanceToSurface(Vec3 point);
}
