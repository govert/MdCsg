namespace MdCsg.Math;

/// <summary>
/// A plane defined by a normal vector and signed distance from origin.
/// The plane equation is: Normal.Dot(P) - Distance = 0.
/// </summary>
/// <param name="Normal">The unit normal of the plane.</param>
/// <param name="Distance">The signed distance from the origin along the normal.</param>
public readonly record struct Plane(Vec3 Normal, double Distance)
{
    /// <summary>
    /// Creates a plane from three non-collinear points (counter-clockwise winding).
    /// </summary>
    public static Plane FromPoints(Vec3 a, Vec3 b, Vec3 c)
    {
        var normal = Vec3.Cross(b - a, c - a).Normalized;
        var distance = Vec3.Dot(normal, a);
        return new Plane(normal, distance);
    }

    /// <summary>
    /// Returns the signed distance from a point to this plane.
    /// Positive means the point is on the side the normal points to.
    /// </summary>
    public double SignedDistanceTo(Vec3 point) =>
        Vec3.Dot(Normal, point) - Distance;

    /// <summary>
    /// Returns the flipped plane (reversed normal and negated distance).
    /// </summary>
    public Plane Flipped => new(-Normal, -Distance);

    /// <inheritdoc />
    public override string ToString() => $"Plane({Normal}, d={Distance})";
}
