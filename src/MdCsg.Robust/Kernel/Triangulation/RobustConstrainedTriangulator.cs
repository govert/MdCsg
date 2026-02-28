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
    private enum PartitionFailureKind
    {
        None = 0,
        InvalidOrCrossingConstraints = 1,
        ConstraintSplitFailure = 2,
        ConstraintEdgeMissingAfterPartition = 3
    }

    private enum FacePointSetFailureKind
    {
        None = 0,
        WorkBudgetExceeded = 1
    }

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
        var normalizedConstraints = NormalizeConstraints(vertices2D, constraints);

        bool useLegacyKernel = false;
        var fallbackReason = RobustTriangulationFallbackReason.None;
        string? fallbackSignature = null;
        List<(int A, int B, int C)> rawTriangles;
        if (normalizedConstraints.Count == 0)
        {
            rawTriangles = EarClipTriangulate(vertices2D);
        }
        else if (TryTriangulateConstrained(
            vertices2D,
            normalizedConstraints,
            out var constrainedTriangles,
            out var nativeFailureReason))
        {
            rawTriangles = constrainedTriangles;
        }
        else
        {
            rawTriangles = ConstrainedTriangulator.Triangulate(vertices3D, normalizedConstraints, faceNormal);
            useLegacyKernel = true;
            fallbackReason = nativeFailureReason;
            var signature = BuildConstraintSignature(vertices2D, normalizedConstraints);
            fallbackSignature = fallbackReason == RobustTriangulationFallbackReason.WorkBudgetExceeded
                ? $"work-budget-exceeded:{signature}"
                : signature;
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
            UsedLegacyKernel: useLegacyKernel)
        {
            LegacyFallbackReason = fallbackReason,
            LegacyFallbackSignature = fallbackSignature
        };
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
        out List<(int A, int B, int C)> triangles,
        out RobustTriangulationFallbackReason failureReason)
    {
        bool sawWorkBudgetExceeded = false;
        if (ShouldPreferFacePointSet(vertices2D, constraints))
        {
            if (TryTriangulateFacePointSet(
                vertices2D,
                constraints,
                out triangles,
                out var facePointSetFailure))
            {
                failureReason = RobustTriangulationFallbackReason.None;
                return true;
            }

            if (facePointSetFailure == FacePointSetFailureKind.WorkBudgetExceeded)
                sawWorkBudgetExceeded = true;
        }

        if (TryTriangulateConstrainedByPartition(
            vertices2D,
            constraints,
            out triangles,
            out var partitionFailure))
        {
            failureReason = RobustTriangulationFallbackReason.None;
            return true;
        }

        if (TryTriangulateConstrainedByEarConstraints(vertices2D, constraints, out triangles))
        {
            failureReason = RobustTriangulationFallbackReason.None;
            return true;
        }

        if (sawWorkBudgetExceeded)
        {
            failureReason = RobustTriangulationFallbackReason.WorkBudgetExceeded;
            return false;
        }

        failureReason = partitionFailure switch
        {
            PartitionFailureKind.InvalidOrCrossingConstraints => RobustTriangulationFallbackReason.InvalidOrCrossingConstraints,
            PartitionFailureKind.ConstraintSplitFailure => RobustTriangulationFallbackReason.PartitioningFailed,
            PartitionFailureKind.ConstraintEdgeMissingAfterPartition => RobustTriangulationFallbackReason.PartitioningFailed,
            _ => RobustTriangulationFallbackReason.ConstrainedEarFailed
        };

        return false;
    }

    private static bool ShouldPreferFacePointSet(
        Vec2[] vertices2D,
        IReadOnlyList<(int Start, int End)> constraints)
    {
        if (vertices2D.Length <= 4
            && ConstraintsContainProperCrossing(vertices2D, constraints))
            return false;

        // Heuristic gate:
        // - dense/large constrained inputs are usually face-cutter style point sets,
        // - low-complexity polygonal inputs are better handled by partition/ear paths.
        if (!IsSimpleBoundaryOrder(vertices2D))
            return true;

        if (vertices2D.Length > 10)
            return true;

        if (constraints.Count >= vertices2D.Length - 2)
            return true;

        return AllPointsInsideSeedTriangle(vertices2D);
    }

    private static bool ConstraintsContainProperCrossing(
        Vec2[] vertices2D,
        IReadOnlyList<(int Start, int End)> constraints)
    {
        int count = vertices2D.Length;
        if (constraints.Count < 2 || count < 4)
            return false;

        var unique = new List<long>(constraints.Count);
        var seen = new HashSet<long>();
        foreach (var (start, end) in constraints)
        {
            if (start < 0 || end < 0 || start >= count || end >= count || start == end)
                continue;

            long key = EdgeKey(start, end);
            if (seen.Add(key))
                unique.Add(key);
        }

        for (int i = 0; i < unique.Count; i++)
        {
            var (a0, a1) = DecodeEdgeKey(unique[i]);
            for (int j = i + 1; j < unique.Count; j++)
            {
                var (b0, b1) = DecodeEdgeKey(unique[j]);
                if (a0 == b0 || a0 == b1 || a1 == b0 || a1 == b1)
                    continue;

                if (SegmentsProperlyIntersect(vertices2D[a0], vertices2D[a1], vertices2D[b0], vertices2D[b1]))
                    return true;
            }
        }

        return false;
    }

    private static bool IsSimpleBoundaryOrder(Vec2[] vertices2D)
    {
        int count = vertices2D.Length;
        if (count < 3)
            return false;

        // Reject repeated consecutive vertices.
        for (int i = 0; i < count; i++)
        {
            int j = (i + 1) % count;
            if (vertices2D[i].Equals(vertices2D[j]))
                return false;
        }

        // Polygon edges from index order must not self-intersect at non-adjacent pairs.
        for (int i = 0; i < count; i++)
        {
            int iNext = (i + 1) % count;
            for (int j = i + 1; j < count; j++)
            {
                int jNext = (j + 1) % count;

                if (i == j || i == jNext || iNext == j || iNext == jNext)
                    continue;

                if (SegmentsProperlyIntersect(
                    vertices2D[i],
                    vertices2D[iNext],
                    vertices2D[j],
                    vertices2D[jNext]))
                    return false;
            }
        }

        // Must have non-zero signed area to be a usable boundary loop.
        double signedArea2 = 0.0;
        for (int i = 0; i < count; i++)
        {
            var a = vertices2D[i];
            var b = vertices2D[(i + 1) % count];
            signedArea2 += a.X * b.Y - b.X * a.Y;
        }

        return System.Math.Abs(signedArea2) > 1e-18;
    }

    private static bool TryTriangulateFacePointSet(
        Vec2[] vertices2D,
        IReadOnlyList<(int Start, int End)> constraints,
        out List<(int A, int B, int C)> triangles,
        out FacePointSetFailureKind failureKind)
    {
        triangles = [];
        failureKind = FacePointSetFailureKind.None;
        if (vertices2D.Length < 3)
            return false;

        if (!TrySelectSeedTriangle(vertices2D, out int s0, out int s1, out int s2))
            return true;

        bool flipped = false;
        var tris = new List<(int A, int B, int C)>();
        if (Orient2D.Evaluate(vertices2D[s0], vertices2D[s1], vertices2D[s2]) == PredicateSign.Positive)
            tris.Add((s0, s1, s2));
        else
        {
            tris.Add((s0, s2, s1));
            flipped = true;
        }

        for (int v = 0; v < vertices2D.Length; v++)
        {
            if (v == s0 || v == s1 || v == s2)
                continue;

            InsertVertexIntoTriangulation(tris, vertices2D, v);
        }

        foreach (var (start, end) in constraints)
        {
            if (start < 0 || end < 0 || start >= vertices2D.Length || end >= vertices2D.Length || start == end)
                return false;

            int constraintWorkBudget = ComputeConstraintWorkBudget(vertices2D.Length, tris.Count);
            EnforceConstraintInTriangulation(
                tris,
                vertices2D,
                start,
                end,
                recursionDepth: 0,
                ref constraintWorkBudget);

            if (constraintWorkBudget <= 0)
            {
                triangles = [];
                failureKind = FacePointSetFailureKind.WorkBudgetExceeded;
                return false;
            }
        }

        for (int i = tris.Count - 1; i >= 0; i--)
        {
            var tri = tris[i];
            var sign = Orient2D.Evaluate(vertices2D[tri.A], vertices2D[tri.B], vertices2D[tri.C]);
            if (sign == PredicateSign.Zero)
            {
                tris.RemoveAt(i);
                continue;
            }

            if (sign == PredicateSign.Negative)
                tris[i] = (tri.A, tri.C, tri.B);
        }

        if (flipped)
        {
            for (int i = 0; i < tris.Count; i++)
                tris[i] = (tris[i].A, tris[i].C, tris[i].B);
        }

        triangles = tris;
        return true;
    }

    private static bool TrySelectSeedTriangle(
        Vec2[] vertices2D,
        out int s0,
        out int s1,
        out int s2)
    {
        s0 = -1;
        s1 = -1;
        s2 = -1;

        double bestArea2 = 0.0;
        int count = vertices2D.Length;
        for (int i = 0; i < count - 2; i++)
        {
            var pi = vertices2D[i];
            for (int j = i + 1; j < count - 1; j++)
            {
                var pj = vertices2D[j];
                for (int k = j + 1; k < count; k++)
                {
                    var pk = vertices2D[k];
                    if (Orient2D.Evaluate(pi, pj, pk) == PredicateSign.Zero)
                        continue;

                    double area2 = System.Math.Abs(
                        (pj.X - pi.X) * (pk.Y - pi.Y)
                        - (pj.Y - pi.Y) * (pk.X - pi.X));
                    if (area2 <= bestArea2)
                        continue;

                    bestArea2 = area2;
                    s0 = i;
                    s1 = j;
                    s2 = k;
                }
            }
        }

        return s0 >= 0;
    }

    private static bool AllPointsInsideSeedTriangle(Vec2[] vertices2D)
    {
        if (vertices2D.Length <= 3)
            return true;

        var a = vertices2D[0];
        var b = vertices2D[1];
        var c = vertices2D[2];

        var seedOrient = Orient2D.Evaluate(a, b, c);
        if (seedOrient == PredicateSign.Zero)
            return false;

        for (int i = 3; i < vertices2D.Length; i++)
        {
            var p = vertices2D[i];
            var s1 = Orient2D.Evaluate(a, b, p);
            var s2 = Orient2D.Evaluate(b, c, p);
            var s3 = Orient2D.Evaluate(c, a, p);

            if (seedOrient == PredicateSign.Positive)
            {
                if (s1 == PredicateSign.Negative || s2 == PredicateSign.Negative || s3 == PredicateSign.Negative)
                    return false;
            }
            else
            {
                if (s1 == PredicateSign.Positive || s2 == PredicateSign.Positive || s3 == PredicateSign.Positive)
                    return false;
            }
        }

        return true;
    }

    private static bool TryTriangulateConstrainedByPartition(
        Vec2[] vertices2D,
        IReadOnlyList<(int Start, int End)> constraints,
        out List<(int A, int B, int C)> triangles,
        out PartitionFailureKind failureKind)
    {
        int count = vertices2D.Length;
        triangles = new List<(int A, int B, int C)>(System.Math.Max(0, count - 2));
        failureKind = PartitionFailureKind.None;

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
            failureKind = PartitionFailureKind.InvalidOrCrossingConstraints;
            return false;
        }

        if (requiredConstraints.Count == 0)
        {
            triangles = EarClipTriangulate(vertices2D, polygon);
            return true;
        }

        var orderedConstraints = new List<long>(requiredConstraints);
        orderedConstraints.Sort();
        var remainingConstraints = new List<long>(orderedConstraints);

        var polygons = new List<List<int>> { polygon };

        int guard = orderedConstraints.Count * orderedConstraints.Count + 16;
        while (remainingConstraints.Count > 0 && guard-- > 0)
        {
            bool madeProgress = false;
            for (int ci = 0; ci < remainingConstraints.Count; ci++)
            {
                long constraint = remainingConstraints[ci];
                var (start, end) = DecodeEdgeKey(constraint);
                bool satisfied = false;

                if (ConstraintIsBoundaryInAnyPolygon(polygons, start, end))
                {
                    satisfied = true;
                }
                else
                {
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
                            // Endpoints can appear in multiple sub-polygons after earlier
                            // splits; a failed split in one candidate does not imply global failure.
                            continue;
                        }

                        if (!alreadyBoundary)
                        {
                            polygons[i] = first;
                            polygons.Add(second);
                        }

                        satisfied = true;
                        break;
                    }
                }

                if (!satisfied)
                    continue;

                remainingConstraints.RemoveAt(ci);
                ci--;
                madeProgress = true;
            }

            if (!madeProgress)
            {
                triangles = [];
                failureKind = PartitionFailureKind.ConstraintSplitFailure;
                return false;
            }
        }

        if (remainingConstraints.Count > 0)
        {
            triangles = [];
            failureKind = PartitionFailureKind.ConstraintSplitFailure;
            return false;
        }

        triangles = new List<(int A, int B, int C)>(System.Math.Max(0, count - 2));
        foreach (var subPolygon in polygons)
            triangles.AddRange(EarClipTriangulate(vertices2D, subPolygon));

        foreach (long constraint in orderedConstraints)
        {
            if (!HasTriangleEdge(triangles, constraint))
            {
                triangles = [];
                failureKind = PartitionFailureKind.ConstraintEdgeMissingAfterPartition;
                return false;
            }
        }

        return true;
    }

    private static bool ConstraintIsBoundaryInAnyPolygon(
        IReadOnlyList<List<int>> polygons,
        int start,
        int end)
    {
        for (int i = 0; i < polygons.Count; i++)
        {
            var polygon = polygons[i];
            int startIdx = polygon.IndexOf(start);
            if (startIdx < 0)
                continue;

            int endIdx = polygon.IndexOf(end);
            if (endIdx < 0)
                continue;

            if (AreAdjacentIndices(startIdx, endIdx, polygon.Count))
                return true;
        }

        return false;
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

    private static void InsertVertexIntoTriangulation(List<(int A, int B, int C)> tris, Vec2[] pts, int vertexIndex)
    {
        var p = pts[vertexIndex];

        for (int t = 0; t < tris.Count; t++)
        {
            var (a, b, c) = tris[t];

            var s1 = Orient2D.Evaluate(pts[a], pts[b], p);
            var s2 = Orient2D.Evaluate(pts[b], pts[c], p);
            var s3 = Orient2D.Evaluate(pts[c], pts[a], p);

            bool inside = s1 != PredicateSign.Negative && s2 != PredicateSign.Negative && s3 != PredicateSign.Negative;
            if (!inside)
                continue;

            if (s1 == PredicateSign.Zero)
            {
                tris.RemoveAt(t);
                SplitTriangulationEdge(tris, a, b, c, vertexIndex);
                return;
            }

            if (s2 == PredicateSign.Zero)
            {
                tris.RemoveAt(t);
                SplitTriangulationEdge(tris, b, c, a, vertexIndex);
                return;
            }

            if (s3 == PredicateSign.Zero)
            {
                tris.RemoveAt(t);
                SplitTriangulationEdge(tris, c, a, b, vertexIndex);
                return;
            }

            tris.RemoveAt(t);
            tris.Add((a, b, vertexIndex));
            tris.Add((b, c, vertexIndex));
            tris.Add((c, a, vertexIndex));
            return;
        }

        InsertOnClosestTriangulationEdge(tris, pts, vertexIndex);
    }

    private static void SplitTriangulationEdge(
        List<(int A, int B, int C)> tris,
        int edgeStart,
        int edgeEnd,
        int opposite,
        int splitVertex)
    {
        tris.Add((edgeStart, splitVertex, opposite));
        tris.Add((splitVertex, edgeEnd, opposite));

        for (int t = 0; t < tris.Count; t++)
        {
            var (a, b, c) = tris[t];
            int other = FindTriangulationOppositeVertex(a, b, c, edgeStart, edgeEnd);
            if (other < 0)
                continue;

            tris.RemoveAt(t);
            tris.Add((edgeStart, splitVertex, other));
            tris.Add((splitVertex, edgeEnd, other));
            return;
        }
    }

    private static int FindTriangulationOppositeVertex(int a, int b, int c, int edgeStart, int edgeEnd)
    {
        if ((a == edgeStart && b == edgeEnd) || (a == edgeEnd && b == edgeStart)) return c;
        if ((b == edgeStart && c == edgeEnd) || (b == edgeEnd && c == edgeStart)) return a;
        if ((c == edgeStart && a == edgeEnd) || (c == edgeEnd && a == edgeStart)) return b;
        return -1;
    }

    private static void InsertOnClosestTriangulationEdge(
        List<(int A, int B, int C)> tris,
        Vec2[] pts,
        int vertexIndex)
    {
        var p = pts[vertexIndex];
        double bestDist = double.MaxValue;
        int bestTri = -1;
        int bestEdgeStart = -1;
        int bestEdgeEnd = -1;
        int bestOpposite = -1;

        for (int t = 0; t < tris.Count; t++)
        {
            var (a, b, c) = tris[t];
            CheckTriangulationEdge(pts, a, b, c, p, t, ref bestDist, ref bestTri, ref bestEdgeStart, ref bestEdgeEnd, ref bestOpposite);
            CheckTriangulationEdge(pts, b, c, a, p, t, ref bestDist, ref bestTri, ref bestEdgeStart, ref bestEdgeEnd, ref bestOpposite);
            CheckTriangulationEdge(pts, c, a, b, p, t, ref bestDist, ref bestTri, ref bestEdgeStart, ref bestEdgeEnd, ref bestOpposite);
        }

        if (bestTri < 0)
            return;

        tris.RemoveAt(bestTri);
        SplitTriangulationEdge(tris, bestEdgeStart, bestEdgeEnd, bestOpposite, vertexIndex);
    }

    private static void CheckTriangulationEdge(
        Vec2[] pts,
        int edgeStart,
        int edgeEnd,
        int opposite,
        Vec2 p,
        int triIndex,
        ref double bestDist,
        ref int bestTri,
        ref int bestEdgeStart,
        ref int bestEdgeEnd,
        ref int bestOpposite)
    {
        double d = PointEdgeDistanceSquared(p, pts[edgeStart], pts[edgeEnd]);
        if (d < bestDist)
        {
            bestDist = d;
            bestTri = triIndex;
            bestEdgeStart = edgeStart;
            bestEdgeEnd = edgeEnd;
            bestOpposite = opposite;
        }
    }

    private static double PointEdgeDistanceSquared(Vec2 p, Vec2 a, Vec2 b)
    {
        var ab = new Vec2(b.X - a.X, b.Y - a.Y);
        var ap = new Vec2(p.X - a.X, p.Y - a.Y);
        double t = (ap.X * ab.X + ap.Y * ab.Y) / (ab.X * ab.X + ab.Y * ab.Y + 1e-30);
        t = System.Math.Max(0, System.Math.Min(1, t));
        double dx = p.X - (a.X + t * ab.X);
        double dy = p.Y - (a.Y + t * ab.Y);
        return dx * dx + dy * dy;
    }

    private static void EnforceConstraintInTriangulation(
        List<(int A, int B, int C)> tris,
        Vec2[] pts,
        int start,
        int end,
        int recursionDepth,
        ref int workBudget)
    {
        if (workBudget <= 0)
            return;

        if (recursionDepth > 64)
        {
            workBudget = 0;
            return;
        }

        int callCost = 8 + System.Math.Max(1, tris.Count / 8);
        if (callCost > workBudget)
        {
            workBudget = 0;
            return;
        }

        workBudget -= callCost;

        if (TriangulationEdgeExists(tris, start, end))
            return;

        var crossingIndices = new List<int>();
        for (int t = 0; t < tris.Count; t++)
        {
            var (a, b, c) = tris[t];
            if (TriangulationTriangleCrossedBySegment(pts, a, b, c, start, end))
                crossingIndices.Add(t);
        }

        if (crossingIndices.Count == 0)
        {
            SplitAndEnforceConstraintAtCollinearVertices(
                tris,
                pts,
                start,
                end,
                recursionDepth,
                ref workBudget);
            return;
        }

        var cavityVertices = new HashSet<int>();
        foreach (int t in crossingIndices)
        {
            var (a, b, c) = tris[t];
            cavityVertices.Add(a);
            cavityVertices.Add(b);
            cavityVertices.Add(c);
        }

        crossingIndices.Sort();
        for (int i = crossingIndices.Count - 1; i >= 0; i--)
            tris.RemoveAt(crossingIndices[i]);

        var above = new List<int>();
        var below = new List<int>();

        foreach (int v in cavityVertices)
        {
            if (v == start || v == end)
                continue;

            var sign = Orient2D.Evaluate(pts[start], pts[end], pts[v]);
            if (sign == PredicateSign.Positive)
                above.Add(v);
            else if (sign == PredicateSign.Negative)
                below.Add(v);
        }

        TriangulateConstraintCavitySide(tris, pts, start, end, above);
        TriangulateConstraintCavitySide(tris, pts, end, start, below);
    }

    private static bool TriangulationEdgeExists(List<(int A, int B, int C)> tris, int start, int end)
    {
        foreach (var (a, b, c) in tris)
        {
            if (HasTriangulationEdge(a, b, c, start, end))
                return true;
        }

        return false;
    }

    private static bool HasTriangulationEdge(int a, int b, int c, int start, int end)
    {
        return (a == start && b == end) || (b == start && a == end)
            || (b == start && c == end) || (c == start && b == end)
            || (c == start && a == end) || (a == start && c == end);
    }

    private static bool TriangulationTriangleCrossedBySegment(
        Vec2[] pts,
        int a,
        int b,
        int c,
        int start,
        int end)
    {
        if (a == start || a == end || b == start || b == end || c == start || c == end)
            return false;

        if (HasTriangulationEdge(a, b, c, start, end))
            return false;

        return TriangulationSegmentsCross(pts, start, end, a, b)
            || TriangulationSegmentsCross(pts, start, end, b, c)
            || TriangulationSegmentsCross(pts, start, end, c, a);
    }

    private static bool TriangulationSegmentsCross(Vec2[] pts, int a, int b, int c, int d)
    {
        var s1 = Orient2D.Evaluate(pts[a], pts[b], pts[c]);
        var s2 = Orient2D.Evaluate(pts[a], pts[b], pts[d]);
        var s3 = Orient2D.Evaluate(pts[c], pts[d], pts[a]);
        var s4 = Orient2D.Evaluate(pts[c], pts[d], pts[b]);

        if (s1 != s2 && s1 != PredicateSign.Zero && s2 != PredicateSign.Zero
            && s3 != s4 && s3 != PredicateSign.Zero && s4 != PredicateSign.Zero)
            return true;

        return false;
    }

    private static void SplitAndEnforceConstraintAtCollinearVertices(
        List<(int A, int B, int C)> tris,
        Vec2[] pts,
        int start,
        int end,
        int recursionDepth,
        ref int workBudget)
    {
        if (workBudget <= 0)
            return;

        if (recursionDepth > 64)
        {
            workBudget = 0;
            return;
        }

        double dx = pts[end].X - pts[start].X;
        double dy = pts[end].Y - pts[start].Y;
        double lenSq = dx * dx + dy * dy;
        if (lenSq < 1e-30)
            return;

        var collinear = new List<(int Vertex, double T)>();
        int vertexCount = 0;
        foreach (var (a, b, c) in tris)
        {
            if (a + 1 > vertexCount) vertexCount = a + 1;
            if (b + 1 > vertexCount) vertexCount = b + 1;
            if (c + 1 > vertexCount) vertexCount = c + 1;
        }

        for (int v = 0; v < vertexCount; v++)
        {
            if (v == start || v == end)
                continue;

            if (Orient2D.Evaluate(pts[start], pts[end], pts[v]) != PredicateSign.Zero)
                continue;

            double t = (pts[v].X - pts[start].X) * dx + (pts[v].Y - pts[start].Y) * dy;
            if (t > 0 && t < lenSq)
                collinear.Add((v, t));
        }

        if (collinear.Count == 0)
            return;

        collinear.Sort((a, b) => a.T.CompareTo(b.T));

        int prev = start;
        foreach (var (v, _) in collinear)
        {
            EnforceConstraintInTriangulation(tris, pts, prev, v, recursionDepth + 1, ref workBudget);
            prev = v;
        }

        EnforceConstraintInTriangulation(tris, pts, prev, end, recursionDepth + 1, ref workBudget);
    }

    private static void TriangulateConstraintCavitySide(
        List<(int A, int B, int C)> tris,
        Vec2[] pts,
        int start,
        int end,
        List<int> side)
    {
        if (side.Count == 0)
            return;

        var dir = new Vec2(pts[end].X - pts[start].X, pts[end].Y - pts[start].Y);
        side.Sort((a, b) =>
        {
            double ta = (pts[a].X - pts[start].X) * dir.X + (pts[a].Y - pts[start].Y) * dir.Y;
            double tb = (pts[b].X - pts[start].X) * dir.X + (pts[b].Y - pts[start].Y) * dir.Y;
            return ta.CompareTo(tb);
        });

        var poly = new List<int> { start };
        poly.AddRange(side);
        poly.Add(end);

        EarClipConstraintPolygon(tris, pts, poly);
    }

    private static void EarClipConstraintPolygon(
        List<(int A, int B, int C)> tris,
        Vec2[] pts,
        List<int> poly)
    {
        while (poly.Count > 3)
        {
            bool earFound = false;
            for (int i = 0; i < poly.Count; i++)
            {
                int prev = poly[(i - 1 + poly.Count) % poly.Count];
                int curr = poly[i];
                int next = poly[(i + 1) % poly.Count];

                if (!IsEarInConstraintPolygon(pts, poly, prev, curr, next))
                    continue;

                tris.Add((prev, curr, next));
                poly.RemoveAt(i);
                earFound = true;
                break;
            }

            if (earFound)
                continue;

            for (int i = 1; i < poly.Count - 1; i++)
                tris.Add((poly[0], poly[i], poly[i + 1]));
            return;
        }

        if (poly.Count == 3)
            tris.Add((poly[0], poly[1], poly[2]));
    }

    private static bool IsEarInConstraintPolygon(Vec2[] pts, List<int> poly, int prev, int curr, int next)
    {
        if (Orient2D.Evaluate(pts[prev], pts[curr], pts[next]) != PredicateSign.Positive)
            return false;

        for (int i = 0; i < poly.Count; i++)
        {
            int idx = poly[i];
            if (idx == prev || idx == curr || idx == next)
                continue;

            if (PointInTriangleInclusive(pts[idx], pts[prev], pts[curr], pts[next]))
                return false;
        }

        return true;
    }

    private static string BuildConstraintSignature(
        Vec2[] vertices2D,
        IReadOnlyList<(int Start, int End)> constraints)
    {
        int invalid = 0;
        int degenerate = 0;
        int duplicates = 0;
        var unique = new HashSet<long>();

        int vertexCount = vertices2D.Length;
        var degree = new Dictionary<int, int>();

        foreach (var (start, end) in constraints)
        {
            if (start < 0 || end < 0 || start >= vertexCount || end >= vertexCount)
            {
                invalid++;
                continue;
            }

            if (start == end)
            {
                degenerate++;
                continue;
            }

            long key = EdgeKey(start, end);
            if (!unique.Add(key))
            {
                duplicates++;
                continue;
            }

            degree.TryGetValue(start, out int dStart);
            degree[start] = dStart + 1;
            degree.TryGetValue(end, out int dEnd);
            degree[end] = dEnd + 1;
        }

        int crossing = 0;
        var edges = new List<long>(unique);
        for (int i = 0; i < edges.Count; i++)
        {
            var (a0, a1) = DecodeEdgeKey(edges[i]);
            for (int j = i + 1; j < edges.Count; j++)
            {
                var (b0, b1) = DecodeEdgeKey(edges[j]);
                if (a0 == b0 || a0 == b1 || a1 == b0 || a1 == b1)
                    continue;

                if (SegmentsProperlyIntersect(vertices2D[a0], vertices2D[a1], vertices2D[b0], vertices2D[b1]))
                    crossing++;
            }
        }

        int maxDegree = 0;
        foreach (int d in degree.Values)
        {
            if (d > maxDegree)
                maxDegree = d;
        }

        var edgeList = new List<long>(unique);
        edgeList.Sort();
        const int edgeSampleCount = 64;
        string edgeSample = string.Join(",", edgeList.Take(edgeSampleCount).Select(static e =>
        {
            int a = (int)(e >> 32);
            int b = (int)(uint)e;
            return $"{a}-{b}";
        }));
        if (edgeList.Count > edgeSampleCount)
            edgeSample += ",...";

        ulong edgeHash = 1469598103934665603UL;
        foreach (long edge in edgeList)
        {
            uint hi = (uint)(edge >> 32);
            uint lo = (uint)edge;
            edgeHash ^= hi;
            edgeHash *= 1099511628211UL;
            edgeHash ^= lo;
            edgeHash *= 1099511628211UL;
        }

        return $"v={vertexCount};m={constraints.Count};u={unique.Count};inv={invalid};deg={degenerate};dup={duplicates};cross={crossing};maxDeg={maxDegree};edgeSample={edgeSample};edgeHash={edgeHash:x16}";
    }

    private static int ComputeConstraintWorkBudget(int vertexCount, int triangleCount)
    {
        // Segment enforcement scans/updates local cavities repeatedly.
        // Budget scales with mesh complexity so large valid inputs do not
        // trip fail-closed guards prematurely.
        long n = System.Math.Max(3, vertexCount);
        long t = System.Math.Max(1, triangleCount);
        long budget = System.Math.Max(200_000L, (n * n * 8L) + (t * 64L));
        return budget > int.MaxValue ? int.MaxValue : (int)budget;
    }

    private static IReadOnlyList<(int Start, int End)> NormalizeConstraints(
        Vec2[] vertices2D,
        IReadOnlyList<(int Start, int End)> constraints)
    {
        int count = vertices2D.Length;
        if (constraints.Count == 0 || count < 3)
            return constraints;

        var dedupedEdges = new List<(int Start, int End)>(constraints.Count);
        var edgeSet = new HashSet<long>();

        foreach (var (start, end) in constraints)
        {
            if (start < 0 || end < 0 || start >= count || end >= count || start == end)
                continue;

            long key = EdgeKey(start, end);
            if (!edgeSet.Add(key))
                continue;

            dedupedEdges.Add((start, end));
        }

        if (dedupedEdges.Count == 0)
            return Array.Empty<(int Start, int End)>();

        var normalized = new List<(int Start, int End)>(dedupedEdges.Count * 2);
        var normalizedSet = new HashSet<long>();

        foreach (var (start, end) in dedupedEdges)
        {
            var a = vertices2D[start];
            var b = vertices2D[end];
            var direction = new Vec2(b.X - a.X, b.Y - a.Y);
            double lengthSq = direction.LengthSquared;
            if (lengthSq <= 1e-30)
                continue;

            var splits = new List<(int Vertex, double T)>();
            for (int vertex = 0; vertex < count; vertex++)
            {
                if (vertex == start || vertex == end)
                    continue;

                var p = vertices2D[vertex];
                if (Orient2D.Evaluate(a, b, p) != PredicateSign.Zero)
                    continue;

                double t = ((p.X - a.X) * direction.X + (p.Y - a.Y) * direction.Y) / lengthSq;
                if (t <= 1e-12 || t >= 1.0 - 1e-12)
                    continue;

                splits.Add((vertex, t));
            }

            if (splits.Count == 0)
            {
                AddEdgeIfNew(start, end, normalizedSet, normalized);
                continue;
            }

            splits.Sort(static (x, y) => x.T.CompareTo(y.T));

            int prev = start;
            foreach (var split in splits)
            {
                AddEdgeIfNew(prev, split.Vertex, normalizedSet, normalized);
                prev = split.Vertex;
            }

            AddEdgeIfNew(prev, end, normalizedSet, normalized);
        }

        return normalized;
    }

    private static void AddEdgeIfNew(
        int start,
        int end,
        HashSet<long> edgeSet,
        List<(int Start, int End)> output)
    {
        if (start == end)
            return;

        long key = EdgeKey(start, end);
        if (!edgeSet.Add(key))
            return;

        output.Add((start, end));
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
