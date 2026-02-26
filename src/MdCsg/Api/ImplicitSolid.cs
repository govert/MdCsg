using MdCsg.Classification;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Api;

/// <summary>
/// An analytic (implicit) solid that can be used as an operand in CSG operations
/// without requiring mesh tessellation. The implicit surface is defined by a
/// point classifier and an intersection routine.
/// </summary>
public abstract class ImplicitSolid
{
    /// <summary>
    /// Creates a point classifier for this implicit solid.
    /// </summary>
    public abstract IPointClassifier CreateClassifier();

    /// <summary>
    /// Computes intersection segments between a mesh and this implicit surface.
    /// Default implementation uses signed-distance vertex classification with edge interpolation.
    /// </summary>
    /// <param name="mesh">The mesh to intersect.</param>
    /// <param name="gridSize">Snap-rounding grid resolution.</param>
    /// <returns>Map from mesh face index to intersection segments.</returns>
    public virtual Dictionary<int, List<IntersectionSegment>> ComputeIntersections(
        HalfEdgeMesh mesh, double gridSize = MathUtil.DefaultGridSize)
    {
        return ImplicitIntersector.Compute(mesh, CreateClassifier(), gridSize);
    }

    /// <summary>
    /// Returns the outward surface normal at the given point (pointing away from the solid's interior).
    /// Used for cap orientation during CSG assembly.
    /// </summary>
    /// <param name="point">A point on or near the implicit surface.</param>
    public abstract Vec3 SurfaceNormal(Vec3 point);
}
