using MdCsg.Math;

namespace MdCsg.Api;

/// <summary>
/// An infinite half-space: everything on the positive (normal) side of a plane.
/// Can be used as an operand in CSG operations to clip or slice meshes.
/// </summary>
public class HalfSpace
{
    /// <summary>The plane defining the boundary of this half-space.</summary>
    public Plane Plane { get; }

    /// <summary>
    /// Creates a half-space from a plane. Points on the positive side
    /// (where <see cref="Plane.SignedDistanceTo"/> &gt; 0) are inside.
    /// </summary>
    public HalfSpace(Plane plane)
    {
        Plane = plane;
    }

    /// <summary>
    /// Creates a half-space from a unit normal and signed distance from origin.
    /// </summary>
    public HalfSpace(Vec3 normal, double distance)
        : this(new Plane(normal.Normalized, distance))
    {
    }

    /// <summary>
    /// Creates a half-space from a point on the boundary plane and its outward normal.
    /// </summary>
    public static HalfSpace FromPointAndNormal(Vec3 point, Vec3 normal) =>
        new(Plane.FromPointAndNormal(point, normal));

    /// <summary>
    /// Returns the complementary half-space (everything on the other side of the plane).
    /// </summary>
    public HalfSpace Flipped => new(Plane.Flipped);
}
