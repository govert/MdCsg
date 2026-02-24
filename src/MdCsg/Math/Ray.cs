namespace MdCsg.Math;

/// <summary>
/// A ray with an origin and direction.
/// </summary>
/// <param name="Origin">The origin point.</param>
/// <param name="Direction">The ray direction (should be normalized).</param>
public readonly record struct Ray(Vec3 Origin, Vec3 Direction)
{
    /// <summary>Evaluates the point at parameter t along the ray.</summary>
    public Vec3 PointAt(double t) => Origin + Direction * t;
}
