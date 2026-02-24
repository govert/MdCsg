using MdCsg.Bvh;
using MdCsg.Math;

namespace MdCsg.Classification;

/// <summary>
/// Classifies a point as inside or outside a solid using the generalized winding number.
/// The winding number sums solid angles subtended by each face as seen from the query point.
/// A winding number near 1 means inside; near 0 means outside.
/// </summary>
public static class WindingNumberClassifier
{
    /// <summary>
    /// Classifies a point relative to a solid mesh using the generalized winding number.
    /// </summary>
    public static SolidClassification Classify(Vec3 point, BvhTree bvh)
    {
        double windingNumber = ComputeWindingNumber(point, bvh);
        return windingNumber > 0.5 ? SolidClassification.Inside : SolidClassification.Outside;
    }

    /// <summary>
    /// Computes the generalized winding number of a point relative to a mesh.
    /// </summary>
    public static double ComputeWindingNumber(Vec3 point, BvhTree bvh)
    {
        if (bvh.NodeCount == 0) return 0;

        double totalSolidAngle = 0;
        for (int i = 0; i < bvh.Mesh.Faces.Count; i++)
        {
            var face = bvh.Mesh.Faces[i];
            face.GetTrianglePositions(out var a, out var b, out var c);
            totalSolidAngle += TriangleSolidAngle(point, a, b, c);
        }

        return totalSolidAngle / (4.0 * System.Math.PI);
    }

    /// <summary>
    /// Computes the solid angle subtended by a triangle as seen from a point.
    /// Uses the Van Oosterom-Strackee formula.
    /// </summary>
    private static double TriangleSolidAngle(Vec3 p, Vec3 a, Vec3 b, Vec3 c)
    {
        var pa = a - p;
        var pb = b - p;
        var pc = c - p;

        double la = pa.Length;
        double lb = pb.Length;
        double lc = pc.Length;

        if (la < 1e-15 || lb < 1e-15 || lc < 1e-15)
            return 0; // point on vertex

        double numerator = Vec3.Dot(pa, Vec3.Cross(pb, pc));
        double denominator = la * lb * lc
                           + Vec3.Dot(pa, pb) * lc
                           + Vec3.Dot(pb, pc) * la
                           + Vec3.Dot(pc, pa) * lb;

        return 2.0 * System.Math.Atan2(numerator, denominator);
    }
}
