using MdCsg.Classification;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Api;

/// <summary>
/// An implicit finite cylinder defined by base center, axis direction, radius, and height.
/// Can be used as an analytic CSG operand without mesh tessellation.
/// Uses analytical edge-cylinder intersection for exact crossing detection.
/// </summary>
public class ImplicitCylinder : ImplicitSolid
{
    /// <summary>Center of the base cap.</summary>
    public Vec3 Base { get; }

    /// <summary>Unit axis direction (from base toward top).</summary>
    public Vec3 Axis { get; }

    /// <summary>Radius of the cylinder.</summary>
    public double Radius { get; }

    /// <summary>Height of the cylinder along the axis.</summary>
    public double Height { get; }

    /// <summary>
    /// Creates an implicit finite cylinder.
    /// </summary>
    /// <param name="baseCenter">Center of the base cap.</param>
    /// <param name="axis">Axis direction (will be normalized).</param>
    /// <param name="radius">Radius of the cylinder.</param>
    /// <param name="height">Height of the cylinder along the axis.</param>
    public ImplicitCylinder(Vec3 baseCenter, Vec3 axis, double radius, double height)
    {
        Base = baseCenter;
        Axis = axis.Normalized;
        Radius = radius;
        Height = height;
    }

    /// <inheritdoc />
    public override IPointClassifier CreateClassifier() =>
        new CylinderPointClassifier(Base, Axis, Radius, Height);

    /// <inheritdoc />
    public override Vec3 SurfaceNormal(Vec3 point)
    {
        var v = point - Base;
        double h = Vec3.Dot(v, Axis);

        if (h < 0) return -Axis;
        if (h > Height) return Axis;

        var perp = v - Axis * h;
        double perpLen = perp.Length;
        if (perpLen > 0)
            return perp / perpLen;

        return Vec3.Cross(Axis, System.Math.Abs(Vec3.Dot(Axis, new Vec3(1, 0, 0))) < 0.9
            ? new Vec3(1, 0, 0)
            : new Vec3(0, 1, 0)).Normalized;
    }

    /// <summary>
    /// Computes intersection segments using analytical edge-cylinder intersection.
    /// Handles both the lateral surface (quadratic) and cap planes (linear).
    /// </summary>
    public override Dictionary<int, List<IntersectionSegment>> ComputeIntersections(
        HalfEdgeMesh mesh, double gridSize = MathUtil.DefaultGridSize)
    {
        var faceSegments = new Dictionary<int, List<IntersectionSegment>>();

        for (int faceIdx = 0; faceIdx < mesh.Faces.Count; faceIdx++)
        {
            var face = mesh.Faces[faceIdx];
            face.GetTrianglePositions(out var va, out var vb, out var vc);

            var crossings = new List<Vec3>();
            FindEdgeCylinderIntersections(va, vb, crossings);
            FindEdgeCylinderIntersections(vb, vc, crossings);
            FindEdgeCylinderIntersections(vc, va, crossings);

            for (int i = 0; i + 1 < crossings.Count; i += 2)
            {
                var p0 = SnapRounding.Snap(crossings[i], gridSize);
                var p1 = SnapRounding.Snap(crossings[i + 1], gridSize);

                var seg = new IntersectionSegment(p0, p1, faceIdx, -1);
                if (seg.IsDegenerate) continue;

                if (!faceSegments.TryGetValue(faceIdx, out var list))
                {
                    list = new List<IntersectionSegment>();
                    faceSegments[faceIdx] = list;
                }
                list.Add(seg);
            }
        }

        return faceSegments;
    }

    /// <summary>
    /// Finds intersection points of a line segment with the finite cylinder.
    /// Checks both the lateral surface and the two cap planes.
    /// </summary>
    private void FindEdgeCylinderIntersections(Vec3 v0, Vec3 v1, List<Vec3> crossings)
    {
        var d = v1 - v0;
        var m = v0 - Base;
        double edgeLenSq = Vec3.Dot(d, d);
        if (edgeLenSq < 1e-30) return;

        // Collect all candidate t values with their points
        var candidates = new List<(double t, Vec3 point)>();

        // Lateral surface: solve |perp(P(t) - Base)|² = R²
        FindLateralIntersections(d, m, candidates);

        // Cap planes: solve dot(P(t) - Base, Axis) = 0 and = Height
        FindCapIntersection(d, m, 0, candidates);
        FindCapIntersection(d, m, Height, candidates);

        // Sort by parameter to maintain boundary traversal order
        candidates.Sort((a, b) => a.t.CompareTo(b.t));

        // Add valid crossings
        foreach (var (t, point) in candidates)
        {
            if (t >= 0 && t <= 1)
                crossings.Add(point);
        }
    }

    /// <summary>
    /// Finds intersections with the lateral (cylindrical) surface.
    /// </summary>
    private void FindLateralIntersections(Vec3 d, Vec3 m, List<(double t, Vec3 point)> candidates)
    {
        // Perpendicular components (remove axial component)
        var dPerp = d - Axis * Vec3.Dot(d, Axis);
        var mPerp = m - Axis * Vec3.Dot(m, Axis);

        double a = Vec3.Dot(dPerp, dPerp);
        if (a < 1e-30) return; // edge parallel to axis

        double b = 2 * Vec3.Dot(mPerp, dPerp);
        double c = Vec3.Dot(mPerp, mPerp) - Radius * Radius;

        double disc = b * b - 4 * a * c;
        if (disc < 0) return;

        double sqrtDisc = System.Math.Sqrt(disc);
        double t1 = (-b - sqrtDisc) / (2 * a);
        double t2 = (-b + sqrtDisc) / (2 * a);

        // Check t1: within edge range and within cylinder height
        if (t1 >= -1e-12 && t1 <= 1 + 1e-12)
        {
            double h = Vec3.Dot(m + d * t1, Axis);
            if (h >= 0 && h <= Height)
                candidates.Add((t1, Base + m + d * t1));
        }

        // Check t2: within edge range and within cylinder height
        if (t2 >= -1e-12 && t2 <= 1 + 1e-12 && System.Math.Abs(t2 - t1) > 1e-12)
        {
            double h = Vec3.Dot(m + d * t2, Axis);
            if (h >= 0 && h <= Height)
                candidates.Add((t2, Base + m + d * t2));
        }
    }

    /// <summary>
    /// Finds intersection with a cap plane at the given height along the axis.
    /// </summary>
    private void FindCapIntersection(Vec3 d, Vec3 m, double capHeight, List<(double t, Vec3 point)> candidates)
    {
        double dDotAxis = Vec3.Dot(d, Axis);
        if (System.Math.Abs(dDotAxis) < 1e-15) return; // parallel to cap

        double mDotAxis = Vec3.Dot(m, Axis);
        double t = (capHeight - mDotAxis) / dDotAxis;

        if (t >= -1e-12 && t <= 1 + 1e-12)
        {
            var point = Base + m + d * t;
            // Check radial distance at cap
            var v = m + d * t;
            var perp = v - Axis * Vec3.Dot(v, Axis);
            double perpDist = perp.Length;
            if (perpDist <= Radius)
                candidates.Add((t, point));
        }
    }
}
