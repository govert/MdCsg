using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Intersection;

/// <summary>
/// Computes intersection segments between a mesh and an infinite plane.
/// For each mesh face that straddles the plane, produces an intersection segment.
/// </summary>
public static class PlaneIntersector
{
    /// <summary>
    /// Computes all intersection segments between a mesh and a plane.
    /// </summary>
    /// <param name="mesh">The mesh to intersect.</param>
    /// <param name="plane">The cutting plane.</param>
    /// <param name="gridSize">Snap-rounding grid resolution.</param>
    /// <returns>Map from face index to intersection segments crossing that face.</returns>
    public static Dictionary<int, List<IntersectionSegment>> Compute(
        HalfEdgeMesh mesh, Plane plane, double gridSize = MathUtil.DefaultGridSize)
    {
        var faceSegments = new Dictionary<int, List<IntersectionSegment>>();

        for (int faceIdx = 0; faceIdx < mesh.Faces.Count; faceIdx++)
        {
            var face = mesh.Faces[faceIdx];
            face.GetTrianglePositions(out var va, out var vb, out var vc);

            double da = plane.SignedDistanceTo(va);
            double db = plane.SignedDistanceTo(vb);
            double dc = plane.SignedDistanceTo(vc);

            if (!IntersectPlaneTriangle(va, vb, vc, da, db, dc, out var p0, out var p1))
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
    /// Computes the intersection of a plane with a triangle given pre-computed signed distances.
    /// Returns two points where the plane crosses the triangle edges.
    /// </summary>
    private static bool IntersectPlaneTriangle(
        Vec3 va, Vec3 vb, Vec3 vc,
        double da, double db, double dc,
        out Vec3 p0, out Vec3 p1)
    {
        p0 = p1 = Vec3.Zero;

        // Classify vertices by sign
        int positiveCount = (da > 0 ? 1 : 0) + (db > 0 ? 1 : 0) + (dc > 0 ? 1 : 0);
        int negativeCount = (da < 0 ? 1 : 0) + (db < 0 ? 1 : 0) + (dc < 0 ? 1 : 0);

        // All on the same side or all on the plane: no proper intersection
        if (positiveCount == 0 && negativeCount == 0) return false; // all on plane (degenerate)
        if (negativeCount == 0) return false; // all positive or zero
        if (positiveCount == 0) return false; // all negative or zero

        // Find the two crossing edges and interpolate
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

        // Edge crosses the plane if signs differ (one strictly positive, one strictly negative),
        // or one is on the plane and the other is not (vertex-on-plane case produces a crossing point).
        bool crosses = (d0 > 0 && d1 < 0) || (d0 < 0 && d1 > 0);
        bool vertexOnPlane = (d0 == 0.0 && d1 != 0.0);

        if (crosses)
        {
            double t = d0 / (d0 - d1);
            crossings[count++] = v0 + (v1 - v0) * t;
        }
        else if (vertexOnPlane)
        {
            crossings[count++] = v0;
        }
    }
}
