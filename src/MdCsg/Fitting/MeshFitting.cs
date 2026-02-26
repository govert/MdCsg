using MdCsg.Api;
using MdCsg.Math;

namespace MdCsg.Fitting;

/// <summary>
/// Converts fitted primitives back to CSG-ready objects (solids and planes).
/// </summary>
public static class MeshFitting
{
    /// <summary>
    /// Converts a <see cref="FittedSphere"/> to a <see cref="Solid"/> icosphere mesh.
    /// </summary>
    /// <param name="sphere">The fitted sphere.</param>
    /// <param name="subdivisions">Icosphere subdivision level (0 = icosahedron, 2 = 320 faces).</param>
    /// <returns>A solid mesh approximating the fitted sphere.</returns>
    public static Solid ToSolid(FittedSphere sphere, int subdivisions = 2)
        => Primitives.Sphere(sphere.Center, sphere.Radius, subdivisions);

    /// <summary>
    /// Converts a <see cref="FittedCylinder"/> to a <see cref="Solid"/> cylinder mesh.
    /// </summary>
    /// <param name="cylinder">The fitted cylinder.</param>
    /// <param name="segments">Number of circumferential segments.</param>
    /// <returns>A solid mesh approximating the fitted cylinder.</returns>
    public static Solid ToSolid(FittedCylinder cylinder, int segments = 32)
        => Primitives.Cylinder(cylinder.BaseCenter, cylinder.Axis, cylinder.Radius, cylinder.Height, segments);

    /// <summary>
    /// Converts a <see cref="FittedPlane"/> to a <see cref="Plane"/> for half-space operations.
    /// </summary>
    /// <param name="fittedPlane">The fitted plane.</param>
    /// <returns>A math plane with the fitted normal passing through the fitted centroid.</returns>
    public static Plane ToPlane(FittedPlane fittedPlane)
        => Plane.FromPointAndNormal(fittedPlane.Centroid, fittedPlane.Normal);

    /// <summary>
    /// Fits a sphere to the given points and returns a solid mesh.
    /// </summary>
    /// <param name="points">At least 4 non-coplanar points.</param>
    /// <param name="subdivisions">Icosphere subdivision level.</param>
    /// <returns>A solid mesh approximating the fitted sphere.</returns>
    public static Solid FitSphere(Vec3[] points, int subdivisions = 2)
        => ToSolid(SphereFitter.Fit(points), subdivisions);

    /// <summary>
    /// Fits a cylinder to the given points and returns a solid mesh.
    /// </summary>
    /// <param name="points">At least 5 points distributed around and along the cylinder.</param>
    /// <param name="normals">Optional per-point surface normals for improved axis estimation.</param>
    /// <param name="axisHint">Optional axis direction hint.</param>
    /// <param name="segments">Number of circumferential segments.</param>
    /// <returns>A solid mesh approximating the fitted cylinder.</returns>
    public static Solid FitCylinder(Vec3[] points, Vec3[]? normals = null,
                                     Vec3? axisHint = null, int segments = 32)
        => ToSolid(CylinderFitter.Fit(points, normals, axisHint), segments);

    /// <summary>
    /// Fits a plane to the given points and returns a <see cref="Plane"/>.
    /// </summary>
    /// <param name="points">At least 3 non-collinear points.</param>
    /// <returns>A math plane from the fit.</returns>
    public static Plane FitPlane(Vec3[] points)
        => ToPlane(PlaneFitter.Fit(points));
}
