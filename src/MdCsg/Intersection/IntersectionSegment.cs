using MdCsg.Math;

namespace MdCsg.Intersection;

/// <summary>
/// Result of a triangle-triangle intersection: a line segment
/// lying on both triangles.
/// </summary>
/// <param name="Start">Start point of the intersection segment.</param>
/// <param name="End">End point of the intersection segment.</param>
/// <param name="FaceIndexA">Index of the first triangle.</param>
/// <param name="FaceIndexB">Index of the second triangle.</param>
public readonly record struct IntersectionSegment(
    Vec3 Start,
    Vec3 End,
    int FaceIndexA,
    int FaceIndexB)
{
    /// <summary>Returns the length of the segment.</summary>
    public double Length => Vec3.Distance(Start, End);

    /// <summary>Returns whether this is a degenerate (zero-length) segment.</summary>
    public bool IsDegenerate => Length < MathUtil.Epsilon;
}
