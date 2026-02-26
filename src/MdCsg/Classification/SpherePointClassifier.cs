using MdCsg.Math;

namespace MdCsg.Classification;

/// <summary>
/// Classifies points against a sphere. Points inside the sphere (distance to center &lt; radius)
/// are classified as Inside; points outside are classified as Outside.
/// </summary>
public class SpherePointClassifier : IPointClassifier
{
    private readonly Vec3 _center;
    private readonly double _radius;

    /// <summary>
    /// Creates a classifier for the given sphere.
    /// </summary>
    /// <param name="center">Center of the sphere.</param>
    /// <param name="radius">Radius of the sphere.</param>
    public SpherePointClassifier(Vec3 center, double radius)
    {
        _center = center;
        _radius = radius;
    }

    /// <inheritdoc />
    public SolidClassification Classify(Vec3 point) =>
        Vec3.Distance(point, _center) < _radius
            ? SolidClassification.Inside
            : SolidClassification.Outside;

    /// <inheritdoc />
    public double DistanceToSurface(Vec3 point) =>
        System.Math.Abs(Vec3.Distance(point, _center) - _radius);
}
