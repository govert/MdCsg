using MdCsg.Math;

namespace MdCsg.Classification;

/// <summary>
/// Classifies points against a half-space defined by a plane.
/// Points on the positive side (normal side) of the plane are Inside;
/// points on the negative side are Outside.
/// </summary>
public class PlanePointClassifier : IPointClassifier
{
    private readonly Plane _plane;

    /// <summary>
    /// Creates a classifier for the given plane.
    /// </summary>
    /// <param name="plane">The half-space boundary plane.</param>
    public PlanePointClassifier(Plane plane)
    {
        _plane = plane;
    }

    /// <inheritdoc />
    public SolidClassification Classify(Vec3 point) =>
        _plane.SignedDistanceTo(point) > 0
            ? SolidClassification.Inside
            : SolidClassification.Outside;

    /// <inheritdoc />
    public double DistanceToSurface(Vec3 point) =>
        System.Math.Abs(_plane.SignedDistanceTo(point));
}
