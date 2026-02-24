namespace MdCsg.Math;

/// <summary>
/// A line segment between two 3D points.
/// </summary>
/// <param name="Start">The start point.</param>
/// <param name="End">The end point.</param>
public readonly record struct Segment(Vec3 Start, Vec3 End)
{
    /// <summary>Returns the direction vector (not normalized).</summary>
    public Vec3 Direction => End - Start;

    /// <summary>Returns the length of the segment.</summary>
    public double Length => Direction.Length;

    /// <summary>Returns the midpoint.</summary>
    public Vec3 Midpoint => (Start + End) * 0.5;

    /// <summary>Evaluates the point at parameter t (0=Start, 1=End).</summary>
    public Vec3 PointAt(double t) => Start + Direction * t;
}
