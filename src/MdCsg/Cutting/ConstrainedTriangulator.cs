using MdCsg.Math;
using MdCsg.Predicates;

namespace MdCsg.Cutting;

/// <summary>
/// Constrained Delaunay triangulation for re-triangulating a face after it has been cut
/// by intersection segments. Uses robust predicates and Lawson edge flipping to maximize
/// minimum triangle angles.
///
/// Algorithm:
/// 1. Start with the original triangle (vertices 0,1,2)
/// 2. Insert each additional vertex by splitting the containing triangle
/// 3. Enforce constraint edges by removing crossing edges and re-triangulating cavities
/// 4. Apply Lawson edge flipping on non-constraint edges to achieve Delaunay quality
/// </summary>
public static class ConstrainedTriangulator
{
    /// <summary>
    /// Triangulates a polygon with constrained edges (intersection segments).
    /// All points are projected to 2D along the dominant axis of the face normal.
    /// </summary>
    /// <param name="vertices3D">Polygon vertices in 3D (first 3 = original triangle).</param>
    /// <param name="constraints">Constrained edges as index pairs.</param>
    /// <param name="faceNormal">The normal of the original face (for projection).</param>
    /// <returns>List of triangles as index triples.</returns>
    public static List<(int A, int B, int C)> Triangulate(
        IReadOnlyList<Vec3> vertices3D,
        IReadOnlyList<(int Start, int End)> constraints,
        Vec3 faceNormal)
    {
        if (vertices3D.Count < 3)
            return [];

        // Project to 2D using the dominant axis
        int dropAxis = GetDominantAxis(faceNormal);
        var pts = new Vec2[vertices3D.Count];
        for (int i = 0; i < vertices3D.Count; i++)
            pts[i] = ProjectTo2D(vertices3D[i], dropAxis);

        if (vertices3D.Count == 3)
            return [(0, 1, 2)];

        if (constraints.Count == 0)
            return EarClipTriangulate(pts);

        // Constrained Delaunay triangulation
        return ConstrainedDelaunay(pts, constraints);
    }

    // =========================================================================
    // Constrained Delaunay triangulation
    // =========================================================================

    private static List<(int A, int B, int C)> ConstrainedDelaunay(
        Vec2[] pts, IReadOnlyList<(int Start, int End)> constraints)
    {
        int n = pts.Length;

        // Start with initial triangle (0,1,2) in CCW order
        bool flipped = false;
        var tris = new List<(int A, int B, int C)>();
        if (Orient2D.Evaluate(pts[0], pts[1], pts[2]) == PredicateSign.Positive)
            tris.Add((0, 1, 2));
        else
        {
            tris.Add((0, 2, 1));
            flipped = true;
        }

        // Phase 1: Insert each additional vertex
        for (int v = 3; v < n; v++)
            InsertVertex(tris, pts, v);

        // Phase 2: Enforce constraint edges
        foreach (var (s, e) in constraints)
            EnforceConstraint(tris, pts, s, e);

        // Phase 3: Lawson flips to achieve Delaunay quality on non-constraint edges
        var constraintSet = new HashSet<long>();
        foreach (var (cs, ce) in constraints)
            constraintSet.Add(EdgeKey(cs, ce));

        LawsonFlipPass(tris, pts, constraintSet);

        // Remove degenerate triangles
        tris.RemoveAll(t =>
            Orient2D.Evaluate(pts[t.A], pts[t.B], pts[t.C]) != PredicateSign.Positive);

        // Restore original winding order if we flipped
        if (flipped)
        {
            for (int i = 0; i < tris.Count; i++)
                tris[i] = (tris[i].A, tris[i].C, tris[i].B);
        }

        return tris;
    }

    // =========================================================================
    // Lawson edge flipping (post-processing)
    // =========================================================================

    /// <summary>
    /// Iteratively flips non-Delaunay, non-constraint edges to improve triangle quality.
    /// </summary>
    private static void LawsonFlipPass(List<(int A, int B, int C)> tris, Vec2[] pts, HashSet<long> constraintSet)
    {
        bool changed = true;
        int maxPasses = tris.Count * 3 + 10;

        while (changed && maxPasses-- > 0)
        {
            changed = false;

            for (int i = 0; i < tris.Count; i++)
            {
                var (a1, b1, c1) = tris[i];

                // Check each of the 3 edges
                if (TryFlipSharedEdge(tris, pts, constraintSet, i, a1, b1, c1))
                { changed = true; break; }
                if (TryFlipSharedEdge(tris, pts, constraintSet, i, b1, c1, a1))
                { changed = true; break; }
                if (TryFlipSharedEdge(tris, pts, constraintSet, i, c1, a1, b1))
                { changed = true; break; }
            }
        }
    }

    /// <summary>
    /// Checks if edge (e0,e1) shared between triangle i and some other triangle j
    /// should be flipped to satisfy the Delaunay criterion.
    /// The vertex 'opp' is opposite the edge in triangle i.
    /// </summary>
    private static bool TryFlipSharedEdge(
        List<(int A, int B, int C)> tris, Vec2[] pts,
        HashSet<long> constraintSet,
        int i, int e0, int e1, int opp)
    {
        // Skip constraint edges
        if (constraintSet.Contains(EdgeKey(e0, e1)))
            return false;

        // Find the neighbor triangle sharing edge (e0, e1)
        for (int j = 0; j < tris.Count; j++)
        {
            if (j == i) continue;
            var (a2, b2, c2) = tris[j];

            int other = FindOppositeVertex(a2, b2, c2, e0, e1);
            if (other < 0) continue;

            // We have quad: opp-e0-other-e1
            // Check InCircle: is 'other' inside circumcircle of (opp, e0, e1)?
            var orient = Orient2D.Evaluate(pts[opp], pts[e0], pts[e1]);
            bool inCirc;
            if (orient == PredicateSign.Positive)
                inCirc = InCircle.Evaluate(pts[opp], pts[e0], pts[e1], pts[other]) == PredicateSign.Positive;
            else if (orient == PredicateSign.Negative)
                inCirc = InCircle.Evaluate(pts[opp], pts[e1], pts[e0], pts[other]) == PredicateSign.Positive;
            else
                return false; // degenerate

            if (!inCirc) return false;

            // Check that the quadrilateral is convex (flip is valid)
            var o1 = Orient2D.Evaluate(pts[opp], pts[e0], pts[other]);
            var o2 = Orient2D.Evaluate(pts[opp], pts[other], pts[e1]);
            if (o1 != PredicateSign.Positive || o2 != PredicateSign.Positive)
                return false;

            // Perform the flip: replace edge (e0,e1) with (opp,other)
            tris[i] = (opp, e0, other);
            tris[j] = (opp, other, e1);
            return true;
        }

        return false;
    }

    // =========================================================================
    // Vertex insertion (from original algorithm, proven correct)
    // =========================================================================

    private static void InsertVertex(List<(int A, int B, int C)> tris, Vec2[] pts, int v)
    {
        var p = pts[v];

        // Find triangle containing p
        for (int t = 0; t < tris.Count; t++)
        {
            var (a, b, c) = tris[t];

            var s1 = Orient2D.Evaluate(pts[a], pts[b], p);
            var s2 = Orient2D.Evaluate(pts[b], pts[c], p);
            var s3 = Orient2D.Evaluate(pts[c], pts[a], p);

            bool inside = s1 != PredicateSign.Negative && s2 != PredicateSign.Negative && s3 != PredicateSign.Negative;
            if (!inside) continue;

            // On edge A-B
            if (s1 == PredicateSign.Zero)
            {
                tris.RemoveAt(t);
                SplitEdge(tris, pts, a, b, c, v);
                return;
            }
            // On edge B-C
            if (s2 == PredicateSign.Zero)
            {
                tris.RemoveAt(t);
                SplitEdge(tris, pts, b, c, a, v);
                return;
            }
            // On edge C-A
            if (s3 == PredicateSign.Zero)
            {
                tris.RemoveAt(t);
                SplitEdge(tris, pts, c, a, b, v);
                return;
            }

            // Strictly inside: split into 3
            tris.RemoveAt(t);
            tris.Add((a, b, v));
            tris.Add((b, c, v));
            tris.Add((c, a, v));
            return;
        }

        // Fallback: find closest edge
        InsertOnClosestEdge(tris, pts, v);
    }

    private static void SplitEdge(List<(int A, int B, int C)> tris, Vec2[] pts, int e0, int e1, int opp, int v)
    {
        tris.Add((e0, v, opp));
        tris.Add((v, e1, opp));

        // Find and split neighbor sharing edge e0-e1
        for (int t = 0; t < tris.Count; t++)
        {
            var (a, b, c) = tris[t];
            int other = FindOppositeVertex(a, b, c, e0, e1);
            if (other < 0) continue;

            tris.RemoveAt(t);
            tris.Add((e0, v, other));
            tris.Add((v, e1, other));
            return;
        }
    }

    private static int FindOppositeVertex(int a, int b, int c, int e0, int e1)
    {
        if ((a == e0 && b == e1) || (a == e1 && b == e0)) return c;
        if ((b == e0 && c == e1) || (b == e1 && c == e0)) return a;
        if ((c == e0 && a == e1) || (c == e1 && a == e0)) return b;
        return -1;
    }

    private static void InsertOnClosestEdge(List<(int A, int B, int C)> tris, Vec2[] pts, int v)
    {
        var p = pts[v];
        double bestDist = double.MaxValue;
        int bestTri = -1;
        int bestE0 = -1, bestE1 = -1, bestOpp = -1;

        for (int t = 0; t < tris.Count; t++)
        {
            var (a, b, c) = tris[t];
            CheckEdge(pts, a, b, c, p, t, ref bestDist, ref bestTri, ref bestE0, ref bestE1, ref bestOpp);
            CheckEdge(pts, b, c, a, p, t, ref bestDist, ref bestTri, ref bestE0, ref bestE1, ref bestOpp);
            CheckEdge(pts, c, a, b, p, t, ref bestDist, ref bestTri, ref bestE0, ref bestE1, ref bestOpp);
        }

        if (bestTri >= 0)
        {
            tris.RemoveAt(bestTri);
            SplitEdge(tris, pts, bestE0, bestE1, bestOpp, v);
        }
    }

    private static void CheckEdge(Vec2[] pts, int e0, int e1, int opp, Vec2 p, int triIdx,
        ref double bestDist, ref int bestTri, ref int bestE0, ref int bestE1, ref int bestOpp)
    {
        double d = PointEdgeDistSq(p, pts[e0], pts[e1]);
        if (d < bestDist)
        {
            bestDist = d;
            bestTri = triIdx;
            bestE0 = e0;
            bestE1 = e1;
            bestOpp = opp;
        }
    }

    private static double PointEdgeDistSq(Vec2 p, Vec2 a, Vec2 b)
    {
        var ab = new Vec2(b.X - a.X, b.Y - a.Y);
        var ap = new Vec2(p.X - a.X, p.Y - a.Y);
        double t = (ap.X * ab.X + ap.Y * ab.Y) / (ab.X * ab.X + ab.Y * ab.Y + 1e-30);
        t = System.Math.Max(0, System.Math.Min(1, t));
        double dx = p.X - (a.X + t * ab.X);
        double dy = p.Y - (a.Y + t * ab.Y);
        return dx * dx + dy * dy;
    }

    // =========================================================================
    // Constraint enforcement (from original algorithm, proven correct)
    // =========================================================================

    private static void EnforceConstraint(List<(int A, int B, int C)> tris, Vec2[] pts, int s, int e)
    {
        if (EdgeExists(tris, s, e))
            return;

        var crossingIndices = new List<int>();
        for (int t = 0; t < tris.Count; t++)
        {
            var (a, b, c) = tris[t];
            if (TriangleCrossedBySegment(pts, a, b, c, s, e))
                crossingIndices.Add(t);
        }

        if (crossingIndices.Count == 0) return;

        var cavity = new HashSet<int>();
        foreach (int t in crossingIndices)
        {
            var (a, b, c) = tris[t];
            cavity.Add(a);
            cavity.Add(b);
            cavity.Add(c);
        }

        crossingIndices.Sort();
        for (int i = crossingIndices.Count - 1; i >= 0; i--)
            tris.RemoveAt(crossingIndices[i]);

        var above = new List<int>();
        var below = new List<int>();

        foreach (int v in cavity)
        {
            if (v == s || v == e) continue;
            var sign = Orient2D.Evaluate(pts[s], pts[e], pts[v]);
            if (sign == PredicateSign.Positive)
                above.Add(v);
            else if (sign == PredicateSign.Negative)
                below.Add(v);
        }

        TriangulateCavitySide(tris, pts, s, e, above);
        TriangulateCavitySide(tris, pts, e, s, below);
    }

    private static void TriangulateCavitySide(
        List<(int A, int B, int C)> tris, Vec2[] pts,
        int s, int e, List<int> side)
    {
        if (side.Count == 0)
            return;

        var dir = new Vec2(pts[e].X - pts[s].X, pts[e].Y - pts[s].Y);
        side.Sort((a, b) =>
        {
            double ta = (pts[a].X - pts[s].X) * dir.X + (pts[a].Y - pts[s].Y) * dir.Y;
            double tb = (pts[b].X - pts[s].X) * dir.X + (pts[b].Y - pts[s].Y) * dir.Y;
            return ta.CompareTo(tb);
        });

        var poly = new List<int> { s };
        poly.AddRange(side);
        poly.Add(e);

        EarClipPolygon(tris, pts, poly);
    }

    private static void EarClipPolygon(List<(int A, int B, int C)> tris, Vec2[] pts, List<int> poly)
    {
        while (poly.Count > 3)
        {
            bool earFound = false;
            for (int i = 0; i < poly.Count; i++)
            {
                int prev = poly[(i - 1 + poly.Count) % poly.Count];
                int curr = poly[i];
                int next = poly[(i + 1) % poly.Count];

                if (IsEarInPoly(pts, poly, prev, curr, next))
                {
                    tris.Add((prev, curr, next));
                    poly.RemoveAt(i);
                    earFound = true;
                    break;
                }
            }
            if (!earFound)
            {
                for (int i = 1; i < poly.Count - 1; i++)
                    tris.Add((poly[0], poly[i], poly[i + 1]));
                return;
            }
        }

        if (poly.Count == 3)
            tris.Add((poly[0], poly[1], poly[2]));
    }

    private static bool IsEarInPoly(Vec2[] pts, List<int> poly, int prev, int curr, int next)
    {
        if (Orient2D.Evaluate(pts[prev], pts[curr], pts[next]) != PredicateSign.Positive)
            return false;

        for (int i = 0; i < poly.Count; i++)
        {
            int idx = poly[i];
            if (idx == prev || idx == curr || idx == next) continue;
            if (PointInTriangle2D(pts[idx], pts[prev], pts[curr], pts[next]))
                return false;
        }
        return true;
    }

    private static bool EdgeExists(List<(int A, int B, int C)> tris, int s, int e)
    {
        foreach (var (a, b, c) in tris)
        {
            if (HasEdge(a, b, c, s, e)) return true;
        }
        return false;
    }

    private static bool HasEdge(int a, int b, int c, int s, int e)
    {
        return (a == s && b == e) || (b == s && a == e) ||
               (b == s && c == e) || (c == s && b == e) ||
               (c == s && a == e) || (a == s && c == e);
    }

    private static bool TriangleCrossedBySegment(Vec2[] pts, int a, int b, int c, int s, int e)
    {
        if (a == s || a == e || b == s || b == e || c == s || c == e) return false;
        if (HasEdge(a, b, c, s, e)) return false;

        return SegmentsCross(pts, s, e, a, b) ||
               SegmentsCross(pts, s, e, b, c) ||
               SegmentsCross(pts, s, e, c, a);
    }

    private static bool SegmentsCross(Vec2[] pts, int a, int b, int c, int d)
    {
        var s1 = Orient2D.Evaluate(pts[a], pts[b], pts[c]);
        var s2 = Orient2D.Evaluate(pts[a], pts[b], pts[d]);
        var s3 = Orient2D.Evaluate(pts[c], pts[d], pts[a]);
        var s4 = Orient2D.Evaluate(pts[c], pts[d], pts[b]);

        if (s1 != s2 && s1 != PredicateSign.Zero && s2 != PredicateSign.Zero &&
            s3 != s4 && s3 != PredicateSign.Zero && s4 != PredicateSign.Zero)
            return true;

        return false;
    }

    // =========================================================================
    // Simple ear-clipping (no constraints)
    // =========================================================================

    private static List<(int A, int B, int C)> EarClipTriangulate(Vec2[] vertices2D)
    {
        var triangles = new List<(int, int, int)>();
        int n = vertices2D.Length;

        var indices = new List<int>();
        for (int i = 0; i < n; i++)
            indices.Add(i);

        while (indices.Count > 3)
        {
            bool earFound = false;
            for (int i = 0; i < indices.Count; i++)
            {
                int prev = indices[(i - 1 + indices.Count) % indices.Count];
                int curr = indices[i];
                int next = indices[(i + 1) % indices.Count];

                if (IsEar(vertices2D, indices, prev, curr, next))
                {
                    triangles.Add((prev, curr, next));
                    indices.RemoveAt(i);
                    earFound = true;
                    break;
                }
            }
            if (!earFound)
            {
                for (int i = 1; i < indices.Count - 1; i++)
                    triangles.Add((indices[0], indices[i], indices[i + 1]));
                break;
            }
        }

        if (indices.Count == 3)
            triangles.Add((indices[0], indices[1], indices[2]));

        return triangles;
    }

    private static bool IsEar(Vec2[] vertices, List<int> polygon, int prev, int curr, int next)
    {
        var a = vertices[prev];
        var b = vertices[curr];
        var c = vertices[next];

        if (Orient2D.Evaluate(a, b, c) != PredicateSign.Positive)
            return false;

        for (int i = 0; i < polygon.Count; i++)
        {
            int idx = polygon[i];
            if (idx == prev || idx == curr || idx == next) continue;

            if (PointInTriangle2D(vertices[idx], a, b, c))
                return false;
        }
        return true;
    }

    // =========================================================================
    // Shared utilities
    // =========================================================================

    private static bool PointInTriangle2D(Vec2 p, Vec2 a, Vec2 b, Vec2 c)
    {
        var s1 = Orient2D.Evaluate(a, b, p);
        var s2 = Orient2D.Evaluate(b, c, p);
        var s3 = Orient2D.Evaluate(c, a, p);

        bool hasNeg = s1 == PredicateSign.Negative || s2 == PredicateSign.Negative || s3 == PredicateSign.Negative;
        bool hasPos = s1 == PredicateSign.Positive || s2 == PredicateSign.Positive || s3 == PredicateSign.Positive;

        return !(hasNeg && hasPos);
    }

    private static long EdgeKey(int a, int b) =>
        a < b ? ((long)a << 32) | (uint)b : ((long)b << 32) | (uint)a;

    /// <summary>
    /// Returns the axis index (0=X, 1=Y, 2=Z) with the largest absolute normal component.
    /// </summary>
    public static int GetDominantAxis(Vec3 normal)
    {
        double ax = System.Math.Abs(normal.X);
        double ay = System.Math.Abs(normal.Y);
        double az = System.Math.Abs(normal.Z);
        if (ax >= ay && ax >= az) return 0;
        if (ay >= az) return 1;
        return 2;
    }

    /// <summary>
    /// Projects a 3D point to 2D by dropping the given axis.
    /// </summary>
    public static Vec2 ProjectTo2D(Vec3 p, int dropAxis) => dropAxis switch
    {
        0 => new Vec2(p.Y, p.Z),
        1 => new Vec2(p.X, p.Z),
        _ => new Vec2(p.X, p.Y),
    };
}
