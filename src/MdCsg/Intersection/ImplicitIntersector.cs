using MdCsg.Classification;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Intersection;

/// <summary>
/// Computes intersection segments between a mesh and an implicit surface.
/// Uses signed-distance vertex classification with linear edge interpolation,
/// analogous to <see cref="PlaneIntersector"/> but generalized for any
/// <see cref="IPointClassifier"/>-backed surface.
/// </summary>
/// <remarks>
/// The linear interpolation assumes the implicit surface is approximately planar
/// across each triangle. For best results, the mesh should be well-tessellated
/// relative to the curvature of the implicit surface.
/// </remarks>
public static class ImplicitIntersector
{
    /// <summary>
    /// Computes all intersection segments between a mesh and an implicit surface.
    /// </summary>
    /// <param name="mesh">The mesh to intersect.</param>
    /// <param name="classifier">Point classifier for the implicit solid.</param>
    /// <param name="gridSize">Snap-rounding grid resolution.</param>
    /// <returns>Map from face index to intersection segments crossing that face.</returns>
    public static Dictionary<int, List<IntersectionSegment>> Compute(
        HalfEdgeMesh mesh, IPointClassifier classifier, double gridSize = MathUtil.DefaultGridSize)
    {
        var faceSegments = new Dictionary<int, List<IntersectionSegment>>();

        for (int faceIdx = 0; faceIdx < mesh.Faces.Count; faceIdx++)
        {
            var face = mesh.Faces[faceIdx];
            face.GetTrianglePositions(out var va, out var vb, out var vc);

            double da = SignedDistance(classifier, va);
            double db = SignedDistance(classifier, vb);
            double dc = SignedDistance(classifier, vc);

            if (!IntersectImplicitTriangle(va, vb, vc, da, db, dc, out var p0, out var p1))
                continue;

            p0 = SnapRounding.Snap(p0, gridSize);
            p1 = SnapRounding.Snap(p1, gridSize);

            var seg = new IntersectionSegment(p0, p1, faceIdx, -1);
            if (seg.IsDegenerate)
                continue;

            if (!faceSegments.TryGetValue(faceIdx, out var list))
            {
                list = new List<IntersectionSegment>();
                faceSegments[faceIdx] = list;
            }
            list.Add(seg);
        }

        return faceSegments;
    }

    /// <summary>
    /// Computes the signed distance from a point to the implicit surface.
    /// Negative inside, positive outside.
    /// </summary>
    private static double SignedDistance(IPointClassifier classifier, Vec3 point)
    {
        double dist = classifier.DistanceToSurface(point);
        return classifier.Classify(point) == SolidClassification.Inside ? -dist : dist;
    }

    /// <summary>
    /// Finds the intersection of an implicit surface with a triangle given pre-computed signed distances.
    /// Uses the same vertex-classification approach as <see cref="PlaneIntersector"/>.
    /// </summary>
    private static bool IntersectImplicitTriangle(
        Vec3 va, Vec3 vb, Vec3 vc,
        double da, double db, double dc,
        out Vec3 p0, out Vec3 p1)
    {
        p0 = p1 = Vec3.Zero;

        int positiveCount = (da > 0 ? 1 : 0) + (db > 0 ? 1 : 0) + (dc > 0 ? 1 : 0);
        int negativeCount = (da < 0 ? 1 : 0) + (db < 0 ? 1 : 0) + (dc < 0 ? 1 : 0);

        if (positiveCount == 0 && negativeCount == 0) return false;
        if (negativeCount == 0) return false;
        if (positiveCount == 0) return false;

        var crossings = new Vec3[2];
        int crossCount = 0;

        TryAddCrossing(va, vb, da, db, crossings, ref crossCount);
        TryAddCrossing(vb, vc, db, dc, crossings, ref crossCount);
        TryAddCrossing(vc, va, dc, da, crossings, ref crossCount);

        if (crossCount < 2)
            return false;

        p0 = crossings[0];
        p1 = crossings[1];
        return true;
    }

    private static void TryAddCrossing(Vec3 v0, Vec3 v1, double d0, double d1, Vec3[] crossings, ref int count)
    {
        if (count >= 2) return;

        bool crosses = (d0 > 0 && d1 < 0) || (d0 < 0 && d1 > 0);
        bool vertexOnSurface = (d0 == 0.0 && d1 != 0.0);

        if (crosses)
        {
            double t = d0 / (d0 - d1);
            crossings[count++] = v0 + (v1 - v0) * t;
        }
        else if (vertexOnSurface)
        {
            crossings[count++] = v0;
        }
    }
}
