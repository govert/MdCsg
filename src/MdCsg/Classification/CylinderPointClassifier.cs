using MdCsg.Math;

namespace MdCsg.Classification;

/// <summary>
/// Classifies points against a finite cylinder defined by base center, axis direction,
/// radius, and height. Points inside the cylinder are classified as Inside.
/// </summary>
public class CylinderPointClassifier : IPointClassifier
{
    private readonly Vec3 _base;
    private readonly Vec3 _axis;
    private readonly double _radius;
    private readonly double _height;

    /// <summary>
    /// Creates a classifier for the given finite cylinder.
    /// </summary>
    /// <param name="baseCenter">Center of the base cap.</param>
    /// <param name="axis">Unit axis direction (from base toward top).</param>
    /// <param name="radius">Radius of the cylinder.</param>
    /// <param name="height">Height of the cylinder along the axis.</param>
    public CylinderPointClassifier(Vec3 baseCenter, Vec3 axis, double radius, double height)
    {
        _base = baseCenter;
        _axis = axis.Normalized;
        _radius = radius;
        _height = height;
    }

    /// <inheritdoc />
    public SolidClassification Classify(Vec3 point)
    {
        var v = point - _base;
        double h = Vec3.Dot(v, _axis);
        if (h < 0 || h > _height) return SolidClassification.Outside;

        var perp = v - _axis * h;
        double perpDist = perp.Length;
        return perpDist < _radius ? SolidClassification.Inside : SolidClassification.Outside;
    }

    /// <inheritdoc />
    public double DistanceToSurface(Vec3 point)
    {
        var v = point - _base;
        double h = Vec3.Dot(v, _axis);
        var perp = v - _axis * h;
        double perpDist = perp.Length;

        // Signed distances to each bounding surface (negative = inside that surface)
        double dLateral = perpDist - _radius;
        double dBottom = -h;
        double dTop = h - _height;

        bool insideLateral = dLateral < 0;
        bool insideAxial = h >= 0 && h <= _height;

        if (insideLateral && insideAxial)
        {
            // Inside the cylinder: return distance to nearest surface
            return System.Math.Min(System.Math.Min(-dLateral, h), _height - h);
        }

        // Outside the cylinder: return distance to nearest surface point
        if (!insideAxial && !insideLateral)
        {
            // Outside both lateral surface and axial slab (near corner/rim)
            double axialDist = h < 0 ? -h : h - _height;
            return System.Math.Sqrt(dLateral * dLateral + axialDist * axialDist);
        }

        if (!insideLateral)
            return dLateral; // outside lateral surface only

        // Inside lateral but outside axial slab
        return h < 0 ? -h : h - _height;
    }
}
