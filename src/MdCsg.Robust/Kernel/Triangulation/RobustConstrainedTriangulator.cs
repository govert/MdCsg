using MdCsg.Cutting;
using MdCsg.Math;
using MdCsg.Predicates;

namespace MdCsg.Robust.Kernel.Triangulation;

/// <summary>
/// Transitional robust triangulator:
/// - unconstrained polygons use a native robust ear-clipping path,
/// - constrained polygons first try a native constrained-ear path and fall back
///   to the legacy constrained triangulator when unsupported.
///
/// Output is normalized for deterministic/validated downstream consumption.
/// </summary>
public sealed class RobustConstrainedTriangulator : IRobustConstrainedTriangulator
{
    public RobustTriangulationResult Triangulate(
        IReadOnlyList<Vec3> vertices3D,
        IReadOnlyList<(int Start, int End)> constraints,
        Vec3 faceNormal,
        RobustTriangulationOptions? options = null)
    {
        var opts = options ?? RobustTriangulationOptions.Default;
        if (vertices3D.Count < 3)
            return new RobustTriangulationResult([], 0, UsedLegacyKernel: false);

        var vertices2D = ProjectToFacePlane(vertices3D, faceNormal);

        bool useLegacyKernel = false;
        List<(int A, int B, int C)> rawTriangles;
        if (constraints.Count == 0)
        {
            rawTriangles = EarClipTriangulate(vertices2D);
        }
        else if (TryTriangulateConstrained(vertices2D, constraints, out var constrainedTriangles))
        {
            rawTriangles = constrainedTriangles;
        }
        else
        {
            rawTriangles = ConstrainedTriangulator.Triangulate(vertices3D, constraints, faceNormal);
            useLegacyKernel = true;
        }

        var normalizedTriangles = new List<(int A, int B, int C)>(rawTriangles.Count);
        int droppedDegenerate = 0;
        double tol = System.Math.Max(0, opts.DegenerateAreaTolerance);
        double tolSq = tol * tol;
        bool hasNormal = faceNormal.LengthSquared > 0;

        foreach (var tri in rawTriangles)
        {
            if (!HasValidIndices(tri, vertices3D.Count) || HasRepeatedVertex(tri))
            {
                droppedDegenerate++;
                continue;
            }

            var a = vertices3D[tri.A];
            var b = vertices3D[tri.B];
            var c = vertices3D[tri.C];
            var cross = Vec3.Cross(b - a, c - a);
            var triAreaSq = cross.LengthSquared;

            if (triAreaSq <= tolSq)
            {
                if (opts.DropDegenerateTriangles)
                {
                    droppedDegenerate++;
                    continue;
                }

                normalizedTriangles.Add(tri);
                continue;
            }

            var oriented = tri;
            if (hasNormal && Vec3.Dot(cross, faceNormal) < 0)
                oriented = (tri.A, tri.C, tri.B);

            normalizedTriangles.Add(oriented);
        }

        if (opts.DeterministicOrdering)
        {
            normalizedTriangles.Sort(static (x, y) =>
                CompareTriangle(CanonicalizeForSort(x), CanonicalizeForSort(y)));
        }

        return new RobustTriangulationResult(
            normalizedTriangles,
            droppedDegenerate,
            UsedLegacyKernel: useLegacyKernel);
    }

    private static bool HasValidIndices((int A, int B, int C) tri, int vertexCount)
        => tri.A >= 0 && tri.B >= 0 && tri.C >= 0
            && tri.A < vertexCount && tri.B < vertexCount && tri.C < vertexCount;

    private static bool HasRepeatedVertex((int A, int B, int C) tri)
        => tri.A == tri.B || tri.B == tri.C || tri.C == tri.A;

    private static (int A, int B, int C) CanonicalizeForSort((int A, int B, int C) tri)
    {
        if (tri.A <= tri.B && tri.A <= tri.C)
            return tri;
        if (tri.B <= tri.A && tri.B <= tri.C)
            return (tri.B, tri.C, tri.A);
        return (tri.C, tri.A, tri.B);
    }

    private static int CompareTriangle((int A, int B, int C) a, (int A, int B, int C) b)
    {
        int cmpA = a.A.CompareTo(b.A);
        if (cmpA != 0) return cmpA;
        int cmpB = a.B.CompareTo(b.B);
        if (cmpB != 0) return cmpB;
        return a.C.CompareTo(b.C);
    }

    private static bool TryTriangulateConstrained(
        Vec2[] vertices2D,
        IReadOnlyList<(int Start, int End)> constraints,
        out List<(int A, int B, int C)> triangles)
    {
        if (TryTriangulateConstrainedByPartition(vertices2D, constraints, out triangles))
            return true;

        return TryTriangulateConstrainedByEarConstraints(vertices2D, constraints, out triangles);
    }

    private static bool TryTriangulateConstrainedByPartition(
        Vec2[] vertices2D,
        IReadOnlyList<(int Start, int End)> constraints,
        out List<(int A, int B, int C)> triangles)
    {
        int count = vertices2D.Length;
        triangles = new List<(int A, int B, int C)>(System.Math.Max(0, count - 2));

        var polygon = new List<int>(count);
        for (int i = 0; i < count; i++)
            polygon.Add(i);

        EnsureCounterClockwise(vertices2D, polygon);

        if (!TryBuildRequiredConstraintState(
            vertices2D,
            polygon,
            constraints,
            out var requiredConstraints,
            out _))
        {
            triangles = [];
            return false;
        }

        if (requiredConstraints.Count == 0)
        {
            triangles = EarClipTriangulate(vertices2D, polygon);
            return true;
        }

        var orderedConstraints = new List<long>(requiredConstraints);
        orderedConstraints.Sort();

        var polygons = new List<List<int>> { polygon };

        foreach (long constraint in orderedConstraints)
        {
            var (start, end) = DecodeEdgeKey(constraint);
            bool satisfied = false;

            for (int i = 0; i < polygons.Count; i++)
            {
                var candidate = polygons[i];
                if (!candidate.Contains(start) || !candidate.Contains(end))
                    continue;

                if (!TryApplyConstraintSplit(
                    candidate,
                    start,
                    end,
                    vertices2D,
                    out var first,
                    out var second,
                    out bool alreadyBoundary))
                {
                    triangles = [];
                    return false;
                }

                if (!alreadyBoundary)
                {
                    polygons[i] = first;
                    polygons.Add(second);
                }

                satisfied = true;
                break;
            }

            if (!satisfied)
            {
                triangles = [];
                return false;
            }
        }

        triangles = new List<(int A, int B, int C)>(System.Math.Max(0, count - 2));
        foreach (var subPolygon in polygons)
            triangles.AddRange(EarClipTriangulate(vertices2D, subPolygon));

        foreach (long constraint in orderedConstraints)
        {
            if (!HasTriangleEdge(triangles, constraint))
            {
                triangles = [];
                return false;
            }
        }

        return true;
    }

    private static bool TryTriangulateConstrainedByEarConstraints(
        Vec2[] vertices2D,
        IReadOnlyList<(int Start, int End)> constraints,
        out List<(int A, int B, int C)> triangles)
    {
        int count = vertices2D.Length;
        triangles = new List<(int A, int B, int C)>(System.Math.Max(0, count - 2));

        var polygon = new List<int>(count);
        for (int i = 0; i < count; i++)
            polygon.Add(i);

        EnsureCounterClockwise(vertices2D, polygon);

        if (!TryBuildRequiredConstraintState(
            vertices2D,
            polygon,
            constraints,
            out var requiredConstraints,
            out var requiredByVertex))
        {
            triangles = [];
            return false;
        }

        int guard = count * count + constraints.Count * count + 16;
        while (polygon.Count > 3 && guard-- > 0)
        {
            bool earFound = false;
            for (int i = 0; i < polygon.Count; i++)
            {
                int prev = polygon[(i - 1 + polygon.Count) % polygon.Count];
                int curr = polygon[i];
                int next = polygon[(i + 1) % polygon.Count];

                if (HasBlockingConstraint(curr, prev, next, requiredByVertex))
                    continue;

                if (!IsEar(vertices2D, polygon, prev, curr, next))
                    continue;

                if (!DiagonalRespectsRequiredConstraints(prev, next, vertices2D, requiredConstraints))
                    continue;

                triangles.Add((prev, curr, next));
                SatisfyConstraintEdge(prev, curr, requiredConstraints, requiredByVertex);
                SatisfyConstraintEdge(curr, next, requiredConstraints, requiredByVertex);
                SatisfyConstraintEdge(next, prev, requiredConstraints, requiredByVertex);

                polygon.RemoveAt(i);
                earFound = true;
                break;
            }

            if (earFound)
                continue;

            triangles = [];
            return false;
        }

        if (polygon.Count != 3)
        {
            triangles = [];
            return false;
        }

        triangles.Add((polygon[0], polygon[1], polygon[2]));
        SatisfyConstraintEdge(polygon[0], polygon[1], requiredConstraints, requiredByVertex);
        SatisfyConstraintEdge(polygon[1], polygon[2], requiredConstraints, requiredByVertex);
        SatisfyConstraintEdge(polygon[2], polygon[0], requiredConstraints, requiredByVertex);

        if (requiredConstraints.Count == 0)
            return true;

        triangles = [];
        return false;
    }

    private static List<(int A, int B, int C)> EarClipTriangulate(Vec2[] vertices2D)
    {
        var polygon = new List<int>(vertices2D.Length);
        for (int i = 0; i < vertices2D.Length; i++)
            polygon.Add(i);

        return EarClipTriangulate(vertices2D, polygon);
    }

    private static List<(int A, int B, int C)> EarClipTriangulate(
        Vec2[] vertices2D,
        List<int> polygonSource)
    {
        var polygon = new List<int>(polygonSource);
        int count = polygon.Count;
        var triangles = new List<(int A, int B, int C)>(System.Math.Max(0, count - 2));

        if (count < 3)
            return triangles;

        if (count == 3)
        {
            triangles.Add((polygon[0], polygon[1], polygon[2]));
            return triangles;
        }

        for (int i = 0; i < count; i++)
        {
            if (polygon[i] < 0 || polygon[i] >= vertices2D.Length)
                return [];
        }

        EnsureCounterClockwise(vertices2D, polygon);

        int guard = count * count + 8;
        while (polygon.Count > 3 && guard-- > 0)
        {
            bool earFound = false;
            for (int i = 0; i < polygon.Count; i++)
            {
                int prev = polygon[(i - 1 + polygon.Count) % polygon.Count];
                int curr = polygon[i];
                int next = polygon[(i + 1) % polygon.Count];

                if (!IsEar(vertices2D, polygon, prev, curr, next))
                    continue;

                triangles.Add((prev, curr, next));
                polygon.RemoveAt(i);
                earFound = true;
                break;
            }

            if (earFound)
                continue;

            // Deterministic fallback for degenerate/non-simple loops:
            // fan triangulation over current order and let normalization drop invalids.
            for (int i = 1; i < polygon.Count - 1; i++)
                triangles.Add((polygon[0], polygon[i], polygon[i + 1]));
            return triangles;
        }

        if (polygon.Count == 3)
            triangles.Add((polygon[0], polygon[1], polygon[2]));

        return triangles;
    }

    private static bool TryBuildRequiredConstraintState(
        Vec2[] vertices2D,
        List<int> polygon,
        IReadOnlyList<(int Start, int End)> constraints,
        out HashSet<long> requiredConstraints,
        out Dictionary<int, HashSet<int>> requiredByVertex)
    {
        requiredConstraints = [];
        requiredByVertex = [];
        int count = vertices2D.Length;
        var boundaryEdges = BuildBoundaryEdges(polygon);

        foreach (var (start, end) in constraints)
        {
            if (start < 0 || end < 0 || start >= count || end >= count || start == end)
                return false;

            long key = EdgeKey(start, end);
            if (boundaryEdges.Contains(key))
                continue;

            if (!requiredConstraints.Add(key))
                continue;

            AddRequiredNeighbor(requiredByVertex, start, end);
            AddRequiredNeighbor(requiredByVertex, end, start);
        }

        if (requiredConstraints.Count <= 1)
            return true;

        var requiredEdgeArray = new List<long>(requiredConstraints);
        for (int i = 0; i < requiredEdgeArray.Count; i++)
        {
            var (a0, a1) = DecodeEdgeKey(requiredEdgeArray[i]);
            for (int j = i + 1; j < requiredEdgeArray.Count; j++)
            {
                var (b0, b1) = DecodeEdgeKey(requiredEdgeArray[j]);
                if (a0 == b0 || a0 == b1 || a1 == b0 || a1 == b1)
                    continue;

                if (SegmentsProperlyIntersect(vertices2D[a0], vertices2D[a1], vertices2D[b0], vertices2D[b1]))
                    return false;
            }
        }

        return true;
    }

    private static HashSet<long> BuildBoundaryEdges(List<int> polygon)
    {
        var boundaryEdges = new HashSet<long>(polygon.Count);
        for (int i = 0; i < polygon.Count; i++)
        {
            int a = polygon[i];
            int b = polygon[(i + 1) % polygon.Count];
            boundaryEdges.Add(EdgeKey(a, b));
        }

        return boundaryEdges;
    }

    private static bool TryApplyConstraintSplit(
        List<int> polygon,
        int start,
        int end,
        Vec2[] vertices2D,
        out List<int> first,
        out List<int> second,
        out bool alreadyBoundary)
    {
        first = [];
        second = [];
        alreadyBoundary = false;

        int startIdx = polygon.IndexOf(start);
        int endIdx = polygon.IndexOf(end);
        if (startIdx < 0 || endIdx < 0)
            return false;

        if (AreAdjacentIndices(startIdx, endIdx, polygon.Count))
        {
            alreadyBoundary = true;
            return true;
        }

        if (!DiagonalRespectsPolygonEdges(start, end, polygon, vertices2D))
            return false;

        first = BuildPathBetween(polygon, startIdx, endIdx);
        second = BuildPathBetween(polygon, endIdx, startIdx);

        if (first.Count < 3 || second.Count < 3)
            return false;

        if (!HasUniqueVertices(first) || !HasUniqueVertices(second))
            return false;

        return true;
    }

    private static bool AreAdjacentIndices(int a, int b, int count)
    {
        if (count < 2)
            return false;

        if (a == b)
            return false;

        return (a + 1) % count == b || (b + 1) % count == a;
    }

    private static List<int> BuildPathBetween(List<int> polygon, int fromIndex, int toIndex)
    {
        var path = new List<int>();
        int idx = fromIndex;
        int guard = polygon.Count + 1;

        while (guard-- > 0)
        {
            path.Add(polygon[idx]);
            if (idx == toIndex)
                break;

            idx = (idx + 1) % polygon.Count;
        }

        return path;
    }

    private static bool HasUniqueVertices(List<int> polygon)
    {
        var seen = new HashSet<int>();
        foreach (int v in polygon)
        {
            if (!seen.Add(v))
                return false;
        }

        return true;
    }

    private static bool DiagonalRespectsPolygonEdges(
        int start,
        int end,
        List<int> polygon,
        Vec2[] vertices2D)
    {
        var a = vertices2D[start];
        var b = vertices2D[end];

        for (int i = 0; i < polygon.Count; i++)
        {
            int edgeStart = polygon[i];
            int edgeEnd = polygon[(i + 1) % polygon.Count];

            if (edgeStart == start || edgeStart == end || edgeEnd == start || edgeEnd == end)
                continue;

            if (SegmentsProperlyIntersect(a, b, vertices2D[edgeStart], vertices2D[edgeEnd]))
                return false;
        }

        return true;
    }

    private static bool HasTriangleEdge(
        IReadOnlyList<(int A, int B, int C)> triangles,
        long edgeKey)
    {
        var (start, end) = DecodeEdgeKey(edgeKey);
        for (int i = 0; i < triangles.Count; i++)
        {
            if (HasEdge(triangles[i], start, end))
                return true;
        }

        return false;
    }

    private static bool HasEdge((int A, int B, int C) tri, int start, int end)
    {
        return (tri.A == start && tri.B == end) || (tri.B == start && tri.A == end)
            || (tri.B == start && tri.C == end) || (tri.C == start && tri.B == end)
            || (tri.C == start && tri.A == end) || (tri.A == start && tri.C == end);
    }

    private static bool HasBlockingConstraint(
        int vertex,
        int prev,
        int next,
        IReadOnlyDictionary<int, HashSet<int>> requiredByVertex)
    {
        if (!requiredByVertex.TryGetValue(vertex, out var requiredNeighbors))
            return false;

        foreach (int neighbor in requiredNeighbors)
        {
            if (neighbor != prev && neighbor != next)
                return true;
        }

        return false;
    }

    private static bool DiagonalRespectsRequiredConstraints(
        int a,
        int b,
        Vec2[] vertices2D,
        HashSet<long> requiredConstraints)
    {
        if (requiredConstraints.Count == 0)
            return true;

        if (requiredConstraints.Contains(EdgeKey(a, b)))
            return true;

        foreach (long key in requiredConstraints)
        {
            var (c, d) = DecodeEdgeKey(key);
            if (a == c || a == d || b == c || b == d)
                continue;

            if (SegmentsProperlyIntersect(vertices2D[a], vertices2D[b], vertices2D[c], vertices2D[d]))
                return false;
        }

        return true;
    }

    private static bool SegmentsProperlyIntersect(Vec2 a, Vec2 b, Vec2 c, Vec2 d)
    {
        var s1 = Orient2D.Evaluate(a, b, c);
        var s2 = Orient2D.Evaluate(a, b, d);
        var s3 = Orient2D.Evaluate(c, d, a);
        var s4 = Orient2D.Evaluate(c, d, b);

        if (s1 == PredicateSign.Zero || s2 == PredicateSign.Zero || s3 == PredicateSign.Zero || s4 == PredicateSign.Zero)
            return false;

        return s1 != s2 && s3 != s4;
    }

    private static void SatisfyConstraintEdge(
        int a,
        int b,
        HashSet<long> requiredConstraints,
        Dictionary<int, HashSet<int>> requiredByVertex)
    {
        long key = EdgeKey(a, b);
        if (!requiredConstraints.Remove(key))
            return;

        RemoveRequiredNeighbor(requiredByVertex, a, b);
        RemoveRequiredNeighbor(requiredByVertex, b, a);
    }

    private static void AddRequiredNeighbor(
        Dictionary<int, HashSet<int>> requiredByVertex,
        int vertex,
        int neighbor)
    {
        if (!requiredByVertex.TryGetValue(vertex, out var neighbors))
        {
            neighbors = [];
            requiredByVertex[vertex] = neighbors;
        }

        neighbors.Add(neighbor);
    }

    private static void RemoveRequiredNeighbor(
        Dictionary<int, HashSet<int>> requiredByVertex,
        int vertex,
        int neighbor)
    {
        if (!requiredByVertex.TryGetValue(vertex, out var neighbors))
            return;

        neighbors.Remove(neighbor);
        if (neighbors.Count == 0)
            requiredByVertex.Remove(vertex);
    }

    private static bool IsEar(Vec2[] vertices, List<int> polygon, int prev, int curr, int next)
    {
        if (Orient2D.Evaluate(vertices[prev], vertices[curr], vertices[next]) != PredicateSign.Positive)
            return false;

        for (int i = 0; i < polygon.Count; i++)
        {
            int idx = polygon[i];
            if (idx == prev || idx == curr || idx == next)
                continue;

            if (PointInTriangleInclusive(vertices[idx], vertices[prev], vertices[curr], vertices[next]))
                return false;
        }

        return true;
    }

    private static bool PointInTriangleInclusive(Vec2 p, Vec2 a, Vec2 b, Vec2 c)
    {
        var s1 = Orient2D.Evaluate(a, b, p);
        var s2 = Orient2D.Evaluate(b, c, p);
        var s3 = Orient2D.Evaluate(c, a, p);

        bool hasNegative = s1 == PredicateSign.Negative || s2 == PredicateSign.Negative || s3 == PredicateSign.Negative;
        if (hasNegative)
            return false;

        bool hasPositive = s1 == PredicateSign.Positive || s2 == PredicateSign.Positive || s3 == PredicateSign.Positive;
        return hasPositive || (s1 == PredicateSign.Zero && s2 == PredicateSign.Zero && s3 == PredicateSign.Zero);
    }

    private static void EnsureCounterClockwise(Vec2[] vertices2D, List<int> polygon)
    {
        double signedArea2 = 0;
        for (int i = 0; i < polygon.Count; i++)
        {
            var a = vertices2D[polygon[i]];
            var b = vertices2D[polygon[(i + 1) % polygon.Count]];
            signedArea2 += a.X * b.Y - b.X * a.Y;
        }

        if (signedArea2 < 0)
        {
            polygon.Reverse();
            return;
        }

        if (signedArea2 > 0)
            return;

        // Degenerate area estimate; ask robust predicate for first non-collinear turn.
        for (int i = 0; i < polygon.Count; i++)
        {
            int prev = polygon[(i - 1 + polygon.Count) % polygon.Count];
            int curr = polygon[i];
            int next = polygon[(i + 1) % polygon.Count];
            var sign = Orient2D.Evaluate(vertices2D[prev], vertices2D[curr], vertices2D[next]);
            if (sign == PredicateSign.Negative)
            {
                polygon.Reverse();
                return;
            }

            if (sign == PredicateSign.Positive)
                return;
        }
    }

    private static Vec2[] ProjectToFacePlane(IReadOnlyList<Vec3> vertices3D, Vec3 faceNormal)
    {
        var origin = vertices3D[0];
        var edgeAB = vertices3D[1] - origin;
        var edgeAC = vertices3D[2] - origin;

        Vec3 normal = faceNormal;
        if (normal.LengthSquared <= 1e-30)
            normal = Vec3.Cross(edgeAB, edgeAC);

        var u = NormalizeOrFallback(edgeAB, edgeAC, new Vec3(1, 0, 0));
        var v = Vec3.Cross(normal, u);

        if (v.LengthSquared <= 1e-30)
        {
            int dropAxis = GetDominantAxis(normal);
            var dropped = new Vec2[vertices3D.Count];
            for (int i = 0; i < vertices3D.Count; i++)
                dropped[i] = ProjectTo2D(vertices3D[i], dropAxis);
            return dropped;
        }

        v = v.Normalized;

        var result = new Vec2[vertices3D.Count];
        for (int i = 0; i < vertices3D.Count; i++)
        {
            var d = vertices3D[i] - origin;
            result[i] = new Vec2(Vec3.Dot(d, u), Vec3.Dot(d, v));
        }

        return result;
    }

    private static Vec3 NormalizeOrFallback(Vec3 primary, Vec3 secondary, Vec3 fallback)
    {
        if (primary.LengthSquared > 1e-30)
            return primary.Normalized;

        if (secondary.LengthSquared > 1e-30)
            return secondary.Normalized;

        return fallback;
    }

    private static int GetDominantAxis(Vec3 normal)
    {
        double ax = System.Math.Abs(normal.X);
        double ay = System.Math.Abs(normal.Y);
        double az = System.Math.Abs(normal.Z);
        if (ax >= ay && ax >= az) return 0;
        if (ay >= az) return 1;
        return 2;
    }

    private static Vec2 ProjectTo2D(Vec3 p, int dropAxis) => dropAxis switch
    {
        0 => new Vec2(p.Y, p.Z),
        1 => new Vec2(p.X, p.Z),
        _ => new Vec2(p.X, p.Y),
    };

    private static long EdgeKey(int a, int b)
        => a < b ? ((long)a << 32) | (uint)b : ((long)b << 32) | (uint)a;

    private static (int A, int B) DecodeEdgeKey(long key)
        => ((int)(key >> 32), (int)(uint)key);
}
