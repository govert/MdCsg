using MdCsg.Classification;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Api;

/// <summary>
/// An implicit sphere defined by center and radius.
/// Can be used as an analytic CSG operand without mesh tessellation.
/// Uses analytical edge-sphere intersection (quadratic formula) for exact
/// crossing detection, avoiding the limitations of vertex classification
/// on coarse meshes.
/// </summary>
public class ImplicitSphere : ImplicitSolid
{
    /// <summary>Center of the sphere.</summary>
    public Vec3 Center { get; }

    /// <summary>Radius of the sphere.</summary>
    public double Radius { get; }

    /// <summary>
    /// Creates an implicit sphere.
    /// </summary>
    /// <param name="center">Center of the sphere.</param>
    /// <param name="radius">Radius of the sphere.</param>
    public ImplicitSphere(Vec3 center, double radius)
    {
        Center = center;
        Radius = radius;
    }

    /// <inheritdoc />
    public override IPointClassifier CreateClassifier() =>
        new SpherePointClassifier(Center, Radius);

    /// <inheritdoc />
    public override Vec3 SurfaceNormal(Vec3 point)
    {
        var dir = point - Center;
        double len = dir.Length;
        return len > 0 ? dir / len : new Vec3(0, 1, 0);
    }

    /// <summary>
    /// Computes intersection segments using analytical edge-sphere intersection.
    /// Solves the quadratic equation for each triangle edge to find exact crossing points,
    /// handling cases where the sphere passes through an edge without either vertex being inside.
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
            FindEdgeSphereIntersections(va, vb, crossings);
            FindEdgeSphereIntersections(vb, vc, crossings);
            FindEdgeSphereIntersections(vc, va, crossings);

            // Pair crossings into segments (consecutive pairs in boundary traversal order)
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
    /// Finds intersection points of a line segment with the sphere using the quadratic formula.
    /// </summary>
    private void FindEdgeSphereIntersections(Vec3 v0, Vec3 v1, List<Vec3> crossings)
    {
        var d = v1 - v0;
        var m = v0 - Center;

        double a = Vec3.Dot(d, d);
        if (a < 1e-30) return; // degenerate edge

        double b = 2 * Vec3.Dot(m, d);
        double c = Vec3.Dot(m, m) - Radius * Radius;

        double disc = b * b - 4 * a * c;
        if (disc < 0) return;

        double sqrtDisc = System.Math.Sqrt(disc);
        double t1 = (-b - sqrtDisc) / (2 * a);
        double t2 = (-b + sqrtDisc) / (2 * a);

        // Add crossings that fall within the edge parameter range [0, 1]
        if (t1 >= 0 && t1 <= 1)
            crossings.Add(v0 + d * t1);
        if (t2 >= 0 && t2 <= 1 && System.Math.Abs(t2 - t1) > 1e-12)
            crossings.Add(v0 + d * t2);
    }
}
