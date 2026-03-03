using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Predicates;

namespace MdCsg.Cutting;

/// <summary>
/// Cuts a single triangular face along intersection segments, producing sub-triangles.
/// </summary>
public static class FaceCutter
{
    /// <summary>
    /// A sub-triangle produced by cutting a face.
    /// </summary>
    /// <param name="A">First vertex position.</param>
    /// <param name="B">Second vertex position.</param>
    /// <param name="C">Third vertex position.</param>
    /// <param name="OriginalFaceIndex">Index of the original face that was cut.</param>
    /// <param name="HasIntersectionEdge">Whether any edge of this sub-triangle is an intersection edge.</param>
    /// <param name="IntersectionEdgeFlags">Per-edge flags: bit 0 = A-B, bit 1 = B-C, bit 2 = C-A.</param>
    public readonly record struct SubTriangle(Vec3 A, Vec3 B, Vec3 C, int OriginalFaceIndex, bool HasIntersectionEdge, byte IntersectionEdgeFlags = 0)
    {
        /// <summary>Returns true if the edge from vertex at index e to vertex at (e+1)%3 is an intersection edge.</summary>
        public bool IsEdgeIntersection(int e) => (IntersectionEdgeFlags & (1 << e)) != 0;
    }

    /// <summary>
    /// Cuts a triangle along the given intersection segments.
    /// </summary>
    /// <param name="triangle">The original triangle.</param>
    /// <param name="faceIndex">The original face index.</param>
    /// <param name="segments">Intersection segments that cross this triangle.</param>
    /// <returns>Sub-triangles after cutting.</returns>
    public static List<SubTriangle> CutFace(Triangle3 triangle, int faceIndex, IReadOnlyList<IntersectionSegment> segments)
    {
        return CutFace(triangle, faceIndex, segments, null, null);
    }

    /// <summary>
    /// Cuts a triangle along the given intersection segments, with optional edge split constraints.
    /// Edge split constraints ensure that split points on shared edges produce matching
    /// edge subdivisions in adjacent faces, maintaining mesh conformality.
    /// </summary>
    /// <param name="triangle">The original triangle.</param>
    /// <param name="faceIndex">The original face index.</param>
    /// <param name="segments">Intersection segments that cross this triangle.</param>
    /// <param name="edgeSplitPoints">Optional split points per edge (index 0=A-B, 1=B-C, 2=C-A). Null entries mean no splits.</param>
    /// <returns>Sub-triangles after cutting.</returns>
    public static List<SubTriangle> CutFace(Triangle3 triangle, int faceIndex,
        IReadOnlyList<IntersectionSegment> segments, List<Vec3>[]? edgeSplitPoints)
    {
        return CutFace(triangle, faceIndex, segments, edgeSplitPoints, null);
    }

    /// <summary>
    /// Cuts a triangle along the given intersection segments with optional edge split constraints and
    /// an optional triangulation kernel override.
    /// </summary>
    /// <param name="triangle">The original triangle.</param>
    /// <param name="faceIndex">The original face index.</param>
    /// <param name="segments">Intersection segments that cross this triangle.</param>
    /// <param name="edgeSplitPoints">Optional split points per edge (index 0=A-B, 1=B-C, 2=C-A).</param>
    /// <param name="triangulationKernel">Optional constrained triangulation kernel override.</param>
    /// <returns>Sub-triangles after cutting.</returns>
    public static List<SubTriangle> CutFace(
        Triangle3 triangle,
        int faceIndex,
        IReadOnlyList<IntersectionSegment> segments,
        List<Vec3>[]? edgeSplitPoints,
        ConstrainedTriangulationKernel? triangulationKernel)
    {
        if (!IsFinite(triangle.A) || !IsFinite(triangle.B) || !IsFinite(triangle.C))
            return [];

        if (segments.Count == 0 && edgeSplitPoints == null)
        {
            return [new SubTriangle(triangle.A, triangle.B, triangle.C, faceIndex, false)];
        }

        var planeNormal = triangle.Normal;
        double planeNormalLenSq = planeNormal.LengthSquared;
        bool hasValidPlane = planeNormalLenSq > 1e-30;

        Vec3 ProjectToFacePlane(Vec3 point)
        {
            if (!hasValidPlane) return point;
            double t = Vec3.Dot(point - triangle.A, planeNormal) / planeNormalLenSq;
            return point - planeNormal * t;
        }

        Vec3 ProjectAndClampToFace(Vec3 point)
        {
            var projected = ProjectToFacePlane(point);
            return GeometryUtil.NearestPointOnTriangle(projected, triangle.A, triangle.B, triangle.C);
        }

        // Collect all unique vertices: original triangle corners + intersection endpoints
        var vertices = new List<Vec3> { triangle.A, triangle.B, triangle.C };
        var constraintPairs = new List<(int Start, int End)>();
        // Track which constraints are intersection edges (vs edge-split constraints)
        var isIntersectionConstraint = new List<bool>();

        foreach (var seg in segments)
        {
            if (!IsFinite(seg.Start) || !IsFinite(seg.End))
                continue;

            int startIdx = AddOrFindVertex(vertices, ProjectAndClampToFace(seg.Start));
            int endIdx = AddOrFindVertex(vertices, ProjectAndClampToFace(seg.End));
            if (startIdx < 0 || endIdx < 0)
                continue;

            if (startIdx != endIdx)
            {
                constraintPairs.Add((startIdx, endIdx));
                isIntersectionConstraint.Add(true);
            }
        }

        // Add edge-split constraints: for each original edge with split points,
        // constrain the chain corner→split1→split2→...→nextCorner
        if (edgeSplitPoints != null)
        {
            // Edge indices: 0=A-B (vertex 0→1), 1=B-C (vertex 1→2), 2=C-A (vertex 2→0)
            int[] edgeStartVtx = [0, 1, 2];
            int[] edgeEndVtx = [1, 2, 0];

            for (int e = 0; e < 3; e++)
            {
                if (edgeSplitPoints.Length <= e || edgeSplitPoints[e] == null || edgeSplitPoints[e].Count == 0)
                    continue;

                var splits = edgeSplitPoints[e];
                // Add split point vertices, snapping to exact edge positions.
                // This ensures the CDT sees them exactly on the edge in 2D.
                var eA = vertices[edgeStartVtx[e]];
                var eB = vertices[edgeEndVtx[e]];
                var eDirSnap = eB - eA;
                double eLenSq = eDirSnap.LengthSquared;

                var splitIndices = new List<int>(splits.Count);
                foreach (var sp in splits)
                {
                    if (!IsFinite(sp))
                        continue;

                    int idx = AddOrFindVertex(vertices, ProjectAndClampToFace(sp));
                    if (idx < 0)
                        continue;

                    if (idx >= 3) // Only if not an original vertex
                    {
                        // Snap vertex to exact position on edge: P = A + t*(B-A)
                        if (eLenSq > 1e-30)
                        {
                            double t = Vec3.Dot(vertices[idx] - eA, eDirSnap) / eLenSq;
                            if (double.IsNaN(t) || double.IsInfinity(t))
                                continue;
                            t = System.Math.Max(0.0, System.Math.Min(1.0, t));
                            vertices[idx] = eA + eDirSnap * t;
                        }
                        splitIndices.Add(idx);
                    }
                }

                if (splitIndices.Count == 0) continue;

                // Sort by parametric t along the edge
                var eStart = vertices[edgeStartVtx[e]];
                var eDir = vertices[edgeEndVtx[e]] - eStart;
                splitIndices.Sort((a, b) =>
                {
                    double ta = Vec3.Dot(vertices[a] - eStart, eDir);
                    double tb = Vec3.Dot(vertices[b] - eStart, eDir);
                    return ta.CompareTo(tb);
                });

                // Chain: corner → split1 → split2 → ... → nextCorner
                int prev = edgeStartVtx[e];
                foreach (int si in splitIndices)
                {
                    constraintPairs.Add((prev, si));
                    isIntersectionConstraint.Add(false);
                    prev = si;
                }
                constraintPairs.Add((prev, edgeEndVtx[e]));
                isIntersectionConstraint.Add(false);
            }
        }

        // Triangulate with constraints
        var faceNormal = triangle.Normal;
        var kernel = triangulationKernel ?? DefaultTriangulationKernel;
        var triIndices = kernel(vertices, constraintPairs, faceNormal);

        var result = new List<SubTriangle>();
        var intersectionConstraints = new List<(Vec3 Start, Vec3 End)>();
        for (int ci = 0; ci < constraintPairs.Count; ci++)
        {
            if (!isIntersectionConstraint[ci]) continue;
            var (cs, ce) = constraintPairs[ci];
            intersectionConstraints.Add((vertices[cs], vertices[ce]));
        }

        foreach (var (a, b, c) in triIndices)
        {
            // Filter out degenerate sub-triangles using 3D area.
            // The CDT filters by 2D orientation, but face-plane projection can introduce
            // near-degenerate triangles that pass the 2D check. The 3D check is authoritative.
            var pa = vertices[a];
            var pb = vertices[b];
            var pc = vertices[c];
            double areaSq = Vec3.Cross(pb - pa, pc - pa).LengthSquared;
            if (areaSq < 1e-30) continue; // Fast path: definitely degenerate

            // For near-degenerate triangles, use certified predicate (same as output validator)
            if (areaSq < 1e-16)
            {
                if (IsProjectedAreaZero(pa, pb, pc)) continue;
            }

            // Check each edge individually: A-B (bit 0), B-C (bit 1), C-A (bit 2)
            byte edgeFlags = 0;
            if (IsIntersectionOutputEdge(pa, pb, intersectionConstraints)) edgeFlags |= 1;       // A-B
            if (IsIntersectionOutputEdge(pb, pc, intersectionConstraints)) edgeFlags |= 1 << 1;  // B-C
            if (IsIntersectionOutputEdge(pc, pa, intersectionConstraints)) edgeFlags |= 1 << 2;  // C-A

            result.Add(new SubTriangle(pa, pb, pc, faceIndex, edgeFlags != 0, edgeFlags));
        }

        return result;
    }

    private static IReadOnlyList<(int A, int B, int C)> DefaultTriangulationKernel(
        IReadOnlyList<Vec3> vertices3D,
        IReadOnlyList<(int Start, int End)> constraints,
        Vec3 faceNormal)
        => ConstrainedTriangulator.Triangulate(vertices3D, constraints, faceNormal);

    private static int AddOrFindVertex(List<Vec3> vertices, Vec3 point, double tolerance = 1e-8)
    {
        if (!IsFinite(point))
            return -1;

        for (int i = 0; i < vertices.Count; i++)
        {
            if (Vec3.DistanceSquared(vertices[i], point) < tolerance * tolerance)
                return i;
        }
        vertices.Add(point);
        return vertices.Count - 1;
    }

    private static bool IsIntersectionOutputEdge(
        Vec3 edgeStart,
        Vec3 edgeEnd,
        IReadOnlyList<(Vec3 Start, Vec3 End)> intersectionConstraints)
    {
        if (intersectionConstraints.Count == 0)
            return false;

        foreach (var (cs, ce) in intersectionConstraints)
        {
            if (EdgeLiesOnConstraint(edgeStart, edgeEnd, cs, ce))
                return true;
        }
        return false;
    }

    private static bool EdgeLiesOnConstraint(Vec3 e0, Vec3 e1, Vec3 c0, Vec3 c1)
    {
        var c = c1 - c0;
        double cLenSq = c.LengthSquared;
        if (cLenSq < 1e-24)
            return false;

        // Output edge must be collinear with the constraint segment.
        double col0 = Vec3.Cross(e0 - c0, c).LengthSquared;
        double col1 = Vec3.Cross(e1 - c0, c).LengthSquared;
        if (col0 > 1e-14 || col1 > 1e-14)
            return false;

        double t0 = Vec3.Dot(e0 - c0, c) / cLenSq;
        double t1 = Vec3.Dot(e1 - c0, c) / cLenSq;
        const double eps = 1e-8;
        return t0 >= -eps && t0 <= 1.0 + eps &&
               t1 >= -eps && t1 <= 1.0 + eps;
    }

    private static bool IsProjectedAreaZero(Vec3 a, Vec3 b, Vec3 c)
    {
        Vec3 n = Vec3.Cross(b - a, c - a);
        double ax = System.Math.Abs(n.X), ay = System.Math.Abs(n.Y), az = System.Math.Abs(n.Z);
        Vec2 pa, pb, pc;
        if (ax >= ay && ax >= az)
        { pa = new Vec2(a.Y, a.Z); pb = new Vec2(b.Y, b.Z); pc = new Vec2(c.Y, c.Z); }
        else if (ay >= az)
        { pa = new Vec2(a.X, a.Z); pb = new Vec2(b.X, b.Z); pc = new Vec2(c.X, c.Z); }
        else
        { pa = new Vec2(a.X, a.Y); pb = new Vec2(b.X, b.Y); pc = new Vec2(c.X, c.Y); }
        return Orient2D.Evaluate(pa, pb, pc) == PredicateSign.Zero;
    }

    private static bool IsFinite(Vec3 v) =>
        !(double.IsNaN(v.X) || double.IsInfinity(v.X) ||
          double.IsNaN(v.Y) || double.IsInfinity(v.Y) ||
          double.IsNaN(v.Z) || double.IsInfinity(v.Z));

    /// <summary>
    /// Snaps non-corner 2D vertices that lie near an original triangle edge to the exact
    /// parametric position on that edge. This prevents the CDT from seeing near-collinear
    /// points as off-edge, which would produce degenerate triangles.
    /// </summary>
    private static void SnapVertices2DToEdges(Vec2[] pts, int count)
    {
        // Original triangle corners are indices 0, 1, 2
        // Edges: 0→1, 1→2, 2→0
        ReadOnlySpan<int> edgeStart = [0, 1, 2];
        ReadOnlySpan<int> edgeEnd = [1, 2, 0];

        for (int i = 3; i < count; i++)
        {
            var p = pts[i];

            for (int e = 0; e < 3; e++)
            {
                var a = pts[edgeStart[e]];
                var b = pts[edgeEnd[e]];
                double dx = b.X - a.X;
                double dy = b.Y - a.Y;
                double lenSq = dx * dx + dy * dy;
                if (lenSq < 1e-30) continue;

                // Parametric position along edge
                double t = ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / lenSq;
                if (t <= 0.0 || t >= 1.0) continue; // Not interior to edge

                // Distance from point to edge line
                double projX = a.X + t * dx;
                double projY = a.Y + t * dy;
                double distSq = (p.X - projX) * (p.X - projX) + (p.Y - projY) * (p.Y - projY);

                // Snap if close to edge (within ~1e-6 in 2D)
                if (distSq < 1e-12)
                {
                    pts[i] = new Vec2(projX, projY);
                    break; // Each vertex can be on at most one edge interior
                }
            }
        }
    }
}
