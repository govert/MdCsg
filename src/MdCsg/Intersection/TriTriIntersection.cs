using MdCsg.Math;
using MdCsg.Predicates;

namespace MdCsg.Intersection;

/// <summary>
/// Triangle-triangle intersection using Moller's algorithm with robust predicates
/// for classification and double-precision geometry for intersection points.
/// </summary>
public static class TriTriIntersection
{
    /// <summary>
    /// Tests whether two triangles intersect and returns the intersection segment if they do.
    /// </summary>
    public static bool Intersect(Triangle3 t1, Triangle3 t2, out IntersectionSegment segment)
    {
        segment = default;

        // Compute plane of t1
        var n1 = Vec3.Cross(t1.B - t1.A, t1.C - t1.A);
        var d1 = Vec3.Dot(n1, t1.A);

        // Signed distances of t2's vertices to t1's plane
        var dist20 = Vec3.Dot(n1, t2.A) - d1;
        var dist21 = Vec3.Dot(n1, t2.B) - d1;
        var dist22 = Vec3.Dot(n1, t2.C) - d1;

        // Use robust predicates for sign classification
        var sign20 = Orient3D.Evaluate(t1.A, t1.B, t1.C, t2.A);
        var sign21 = Orient3D.Evaluate(t1.A, t1.B, t1.C, t2.B);
        var sign22 = Orient3D.Evaluate(t1.A, t1.B, t1.C, t2.C);

        if (AllSameNonZeroSign(sign20, sign21, sign22))
            return false;

        // Compute plane of t2
        var n2 = Vec3.Cross(t2.B - t2.A, t2.C - t2.A);
        var d2 = Vec3.Dot(n2, t2.A);

        var dist10 = Vec3.Dot(n2, t1.A) - d2;
        var dist11 = Vec3.Dot(n2, t1.B) - d2;
        var dist12 = Vec3.Dot(n2, t1.C) - d2;

        var sign10 = Orient3D.Evaluate(t2.A, t2.B, t2.C, t1.A);
        var sign11 = Orient3D.Evaluate(t2.A, t2.B, t2.C, t1.B);
        var sign12 = Orient3D.Evaluate(t2.A, t2.B, t2.C, t1.C);

        if (AllSameNonZeroSign(sign10, sign11, sign12))
            return false;

        // Coplanar case — handled separately by IntersectCoplanar
        if (sign20 == PredicateSign.Zero && sign21 == PredicateSign.Zero && sign22 == PredicateSign.Zero)
            return false;

        // Intersection line direction
        var lineDir = Vec3.Cross(n1, n2);

        // Pick projection axis with largest component
        int maxAxis = 0;
        double maxVal = System.Math.Abs(lineDir.X);
        if (System.Math.Abs(lineDir.Y) > maxVal) { maxAxis = 1; maxVal = System.Math.Abs(lineDir.Y); }
        if (System.Math.Abs(lineDir.Z) > maxVal) { maxAxis = 2; }

        // Compute intersection intervals on the projection axis
        if (!ComputeEdgePlaneIntersections(t1, dist10, dist11, dist12, sign10, sign11, sign12, maxAxis,
                out var t1p0, out var t1p1, out var t1v0, out var t1v1))
            return false;

        if (!ComputeEdgePlaneIntersections(t2, dist20, dist21, dist22, sign20, sign21, sign22, maxAxis,
                out var t2p0, out var t2p1, out var t2v0, out var t2v1))
            return false;

        // Ensure intervals are ordered
        if (t1p0 > t1p1) { (t1p0, t1p1) = (t1p1, t1p0); (t1v0, t1v1) = (t1v1, t1v0); }
        if (t2p0 > t2p1) { (t2p0, t2p1) = (t2p1, t2p0); (t2v0, t2v1) = (t2v1, t2v0); }

        // Compute overlap
        double overlapMin = System.Math.Max(t1p0, t2p0);
        double overlapMax = System.Math.Min(t1p1, t2p1);

        if (overlapMin > overlapMax + MathUtil.Epsilon)
            return false;

        // Compute actual 3D points for the overlap endpoints
        Vec3 start = t1p0 >= t2p0 ? t1v0 : t2v0;
        Vec3 end = t1p1 <= t2p1 ? t1v1 : t2v1;

        segment = new IntersectionSegment(start, end, -1, -1);
        return !segment.IsDegenerate;
    }

    private static bool ComputeEdgePlaneIntersections(
        Triangle3 tri,
        double d0, double d1, double d2,
        PredicateSign s0, PredicateSign s1, PredicateSign s2,
        int projAxis,
        out double proj0, out double proj1,
        out Vec3 pt0, out Vec3 pt1)
    {
        proj0 = proj1 = 0;
        pt0 = pt1 = Vec3.Zero;

        // Find the "odd" vertex that's alone on one side of the plane
        // The other two vertices are on the same side (or on the plane)
        int oddIdx;
        if (IsOddVertex(s0, s1, s2))
            oddIdx = 0;
        else if (IsOddVertex(s1, s0, s2))
            oddIdx = 1;
        else
            oddIdx = 2;

        Vec3 vOdd = tri[oddIdx];
        Vec3 vA = tri[(oddIdx + 1) % 3];
        Vec3 vB = tri[(oddIdx + 2) % 3];
        double dOdd = oddIdx == 0 ? d0 : oddIdx == 1 ? d1 : d2;
        double dA = oddIdx == 0 ? d1 : oddIdx == 1 ? d0 : d0;
        double dB = oddIdx == 0 ? d2 : oddIdx == 1 ? d2 : d1;
        PredicateSign sA = oddIdx == 0 ? s1 : oddIdx == 1 ? s0 : s0;
        PredicateSign sB = oddIdx == 0 ? s2 : oddIdx == 1 ? s2 : s1;

        // Compute edge-plane intersection for edge (odd, A) and (odd, B)
        if (sA == PredicateSign.Zero)
        {
            pt0 = vA;
        }
        else
        {
            double t = dOdd / (dOdd - dA);
            pt0 = vOdd + (vA - vOdd) * t;
        }

        if (sB == PredicateSign.Zero)
        {
            pt1 = vB;
        }
        else
        {
            double t = dOdd / (dOdd - dB);
            pt1 = vOdd + (vB - vOdd) * t;
        }

        proj0 = pt0[projAxis];
        proj1 = pt1[projAxis];
        return true;
    }

    private static bool IsOddVertex(PredicateSign s, PredicateSign sOther1, PredicateSign sOther2)
    {
        // s is the odd one if both others agree and differ from s
        if (s == PredicateSign.Zero && sOther1 == PredicateSign.Zero && sOther2 == PredicateSign.Zero)
            return false;

        if (s == PredicateSign.Positive)
            return sOther1 != PredicateSign.Positive && sOther2 != PredicateSign.Positive;
        if (s == PredicateSign.Negative)
            return sOther1 != PredicateSign.Negative && sOther2 != PredicateSign.Negative;

        // s is Zero: it's odd if both others agree on a non-zero sign
        return sOther1 == sOther2 && sOther1 != PredicateSign.Zero;
    }

    private static bool AllSameNonZeroSign(PredicateSign a, PredicateSign b, PredicateSign c) =>
        (a == PredicateSign.Positive && b == PredicateSign.Positive && c == PredicateSign.Positive) ||
        (a == PredicateSign.Negative && b == PredicateSign.Negative && c == PredicateSign.Negative);

    /// <summary>
    /// Tests if two triangles are coplanar (all vertices of t2 are on t1's plane).
    /// </summary>
    public static bool AreCoplanar(Triangle3 t1, Triangle3 t2)
    {
        var s0 = Orient3D.Evaluate(t1.A, t1.B, t1.C, t2.A);
        var s1 = Orient3D.Evaluate(t1.A, t1.B, t1.C, t2.B);
        var s2 = Orient3D.Evaluate(t1.A, t1.B, t1.C, t2.C);
        return s0 == PredicateSign.Zero && s1 == PredicateSign.Zero && s2 == PredicateSign.Zero;
    }

    /// <summary>
    /// Computes intersection segments for two coplanar triangles.
    /// Returns clipping segments: edges of t2 clipped to t1's interior (cuts for t1),
    /// and edges of t1 clipped to t2's interior (cuts for t2).
    /// Also returns whether the triangles' normals agree in direction.
    /// </summary>
    public static bool IntersectCoplanar(
        Triangle3 t1, Triangle3 t2,
        out List<(Vec3 Start, Vec3 End)> segsForT1,
        out List<(Vec3 Start, Vec3 End)> segsForT2,
        out bool normalsAgree)
    {
        segsForT1 = [];
        segsForT2 = [];

        var n1 = Vec3.Cross(t1.B - t1.A, t1.C - t1.A);
        var n2 = Vec3.Cross(t2.B - t2.A, t2.C - t2.A);
        normalsAgree = Vec3.Dot(n1, n2) > 0;

        // Project to 2D
        int dropAxis = Cutting.ConstrainedTriangulator.GetDominantAxis(n1);
        var t1a = Cutting.ConstrainedTriangulator.ProjectTo2D(t1.A, dropAxis);
        var t1b = Cutting.ConstrainedTriangulator.ProjectTo2D(t1.B, dropAxis);
        var t1c = Cutting.ConstrainedTriangulator.ProjectTo2D(t1.C, dropAxis);

        var t2a = Cutting.ConstrainedTriangulator.ProjectTo2D(t2.A, dropAxis);
        var t2b = Cutting.ConstrainedTriangulator.ProjectTo2D(t2.B, dropAxis);
        var t2c = Cutting.ConstrainedTriangulator.ProjectTo2D(t2.C, dropAxis);

        // Clip edges of t2 against t1 → segments that cut t1
        ClipEdgeAgainstTriangle(t2.A, t2.B, t2a, t2b, t1a, t1b, t1c, segsForT1);
        ClipEdgeAgainstTriangle(t2.B, t2.C, t2b, t2c, t1a, t1b, t1c, segsForT1);
        ClipEdgeAgainstTriangle(t2.C, t2.A, t2c, t2a, t1a, t1b, t1c, segsForT1);

        // Clip edges of t1 against t2 → segments that cut t2
        ClipEdgeAgainstTriangle(t1.A, t1.B, t1a, t1b, t2a, t2b, t2c, segsForT2);
        ClipEdgeAgainstTriangle(t1.B, t1.C, t1b, t1c, t2a, t2b, t2c, segsForT2);
        ClipEdgeAgainstTriangle(t1.C, t1.A, t1c, t1a, t2a, t2b, t2c, segsForT2);

        return segsForT1.Count > 0 || segsForT2.Count > 0;
    }

    /// <summary>
    /// Clips a 3D edge (p0→p1) against a 2D triangle (ta, tb, tc) using parametric clipping.
    /// The 2D projections (p0_2d, p1_2d) are used for the clipping math.
    /// If the clipped portion is non-degenerate, adds it to the result list.
    /// </summary>
    private static void ClipEdgeAgainstTriangle(
        Vec3 p0_3d, Vec3 p1_3d,
        Vec2 p0, Vec2 p1,
        Vec2 ta, Vec2 tb, Vec2 tc,
        List<(Vec3 Start, Vec3 End)> result)
    {
        // Parametric t ∈ [0, 1] along p0→p1
        double tMin = 0, tMax = 1;

        // Clip against each edge of the triangle using half-plane test
        // Edge ta→tb: inside is the side where tc lies
        if (!ClipAgainstHalfPlane(p0, p1, ta, tb, tc, ref tMin, ref tMax)) return;
        if (!ClipAgainstHalfPlane(p0, p1, tb, tc, ta, ref tMin, ref tMax)) return;
        if (!ClipAgainstHalfPlane(p0, p1, tc, ta, tb, ref tMin, ref tMax)) return;

        if (tMax - tMin < 1e-10) return; // Degenerate

        // Don't produce segments that exactly match the triangle's edges
        // (they would be on the boundary, not cutting through the interior)
        if (tMin < 1e-10 && tMax > 1 - 1e-10) return;

        var start3d = p0_3d + (p1_3d - p0_3d) * tMin;
        var end3d = p0_3d + (p1_3d - p0_3d) * tMax;

        if (Vec3.DistanceSquared(start3d, end3d) > MathUtil.Epsilon * MathUtil.Epsilon)
            result.Add((start3d, end3d));
    }

    /// <summary>
    /// Clips the parametric interval [tMin, tMax] of segment p0→p1 against the half-plane
    /// defined by edge (ea→eb) where the interior side is where 'interior' lies.
    /// </summary>
    private static bool ClipAgainstHalfPlane(
        Vec2 p0, Vec2 p1,
        Vec2 ea, Vec2 eb, Vec2 interior,
        ref double tMin, ref double tMax)
    {
        // Edge normal (not normalized, pointing toward interior)
        var edgeDir = eb - ea;
        // Perpendicular direction: rotate 90° CCW
        var normal = new Vec2(-edgeDir.Y, edgeDir.X);

        // Ensure normal points toward interior
        if (Vec2.Dot(normal, interior - ea) < 0)
            normal = -normal;

        double d0 = Vec2.Dot(normal, p0 - ea);
        double d1 = Vec2.Dot(normal, p1 - ea);

        if (d0 < -1e-12 && d1 < -1e-12) return false; // Both outside
        if (d0 >= -1e-12 && d1 >= -1e-12) return true;  // Both inside

        // Compute intersection parameter
        double t = d0 / (d0 - d1);

        if (d0 < 0)
            tMin = System.Math.Max(tMin, t);
        else
            tMax = System.Math.Min(tMax, t);

        return tMin <= tMax + 1e-12;
    }
}
