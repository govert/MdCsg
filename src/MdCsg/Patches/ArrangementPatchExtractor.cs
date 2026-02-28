using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using System.Linq;

namespace MdCsg.Patches;

/// <summary>
/// Extracts patches via global flood-fill while using arrangement segment ownership
/// as the authoritative boundary classifier for shared sub-triangle edges.
/// </summary>
public static class ArrangementPatchExtractor
{
    private readonly record struct EdgeRef(
        int TriIndex,
        int FaceIndex,
        Vec3 Start,
        Vec3 End,
        bool IsTaggedIntersectionEdge);

    /// <summary>
    /// Extracts patches from sub-triangles by stopping flood-fill on edges that are either:
    /// - explicitly tagged as intersection edges by face cutting, or
    /// - geometrically recognized as arrangement-owned intersection edges for the original face.
    /// </summary>
    public static List<Patch> Extract(
        IReadOnlyList<FaceCutter.SubTriangle> subTriangles,
        IReadOnlyDictionary<int, List<IntersectionSegment>> faceSegments,
        double tolerance = 1e-8)
    {
        int n = subTriangles.Count;
        if (n == 0)
            return [];

        var adjacency = BuildArrangementAwareAdjacency(subTriangles, faceSegments, tolerance);
        var patches = FloodFill(adjacency);
        PatchIdentityAssigner.Assign(
            patches,
            subTriangles,
            PatchBoundaryAuthority.Arrangement);
        return patches;
    }

    private static List<(int Neighbor, bool IsBoundary)>[] BuildArrangementAwareAdjacency(
        IReadOnlyList<FaceCutter.SubTriangle> subTriangles,
        IReadOnlyDictionary<int, List<IntersectionSegment>> faceSegments,
        double tolerance)
    {
        int n = subTriangles.Count;
        var adjacency = new List<(int Neighbor, bool IsBoundary)>[n];
        for (int i = 0; i < n; i++)
            adjacency[i] = [];

        var vertexMap = new Dictionary<long, List<(int Id, Vec3 Pos)>>();
        int nextVertexId = 0;
        double tol = System.Math.Max(1e-12, tolerance);
        double tolSq = tol * tol;
        double tolInv = 1.0 / tol;

        int GetCanonicalVertexId(Vec3 pos)
        {
            long gx = (long)System.Math.Round(pos.X * tolInv);
            long gy = (long)System.Math.Round(pos.Y * tolInv);
            long gz = (long)System.Math.Round(pos.Z * tolInv);

            for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            for (int dz = -1; dz <= 1; dz++)
            {
                long key = HashGrid(gx + dx, gy + dy, gz + dz);
                if (!vertexMap.TryGetValue(key, out var bucket))
                    continue;

                for (int i = 0; i < bucket.Count; i++)
                {
                    if (Vec3.DistanceSquared(bucket[i].Pos, pos) <= tolSq)
                        return bucket[i].Id;
                }
            }

            long primary = HashGrid(gx, gy, gz);
            if (!vertexMap.TryGetValue(primary, out var primaryBucket))
            {
                primaryBucket = [];
                vertexMap[primary] = primaryBucket;
            }

            int id = nextVertexId++;
            primaryBucket.Add((id, pos));
            return id;
        }

        var edgeMap = new Dictionary<(int, int), List<EdgeRef>>();

        for (int triIdx = 0; triIdx < n; triIdx++)
        {
            var tri = subTriangles[triIdx];
            var verts = new[] { tri.A, tri.B, tri.C };
            int idA = GetCanonicalVertexId(tri.A);
            int idB = GetCanonicalVertexId(tri.B);
            int idC = GetCanonicalVertexId(tri.C);
            var ids = new[] { idA, idB, idC };

            for (int e = 0; e < 3; e++)
            {
                int i0 = ids[e];
                int i1 = ids[(e + 1) % 3];
                var key = i0 < i1 ? (i0, i1) : (i1, i0);

                if (!edgeMap.TryGetValue(key, out var refs))
                {
                    refs = [];
                    edgeMap[key] = refs;
                }

                refs.Add(new EdgeRef(
                    TriIndex: triIdx,
                    FaceIndex: tri.OriginalFaceIndex,
                    Start: verts[e],
                    End: verts[(e + 1) % 3],
                    IsTaggedIntersectionEdge: tri.IsEdgeIntersection(e)));
            }
        }

        var faceBoundaryCache = new Dictionary<(int FaceIndex, int V0, int V1), bool>();

        foreach (var kvp in edgeMap)
        {
            var edgeKey = kvp.Key;
            var refs = kvp.Value;
            bool isBoundary = refs.Any(static r => r.IsTaggedIntersectionEdge);
            if (!isBoundary)
            {
                for (int i = 0; i < refs.Count; i++)
                {
                    var r = refs[i];
                    var cacheKey = (r.FaceIndex, edgeKey.Item1, edgeKey.Item2);
                    if (!faceBoundaryCache.TryGetValue(cacheKey, out bool onBoundary))
                    {
                        onBoundary = IsArrangementBoundaryEdge(
                            r.FaceIndex,
                            r.Start,
                            r.End,
                            faceSegments,
                            tol);
                        faceBoundaryCache[cacheKey] = onBoundary;
                    }

                    if (onBoundary)
                    {
                        isBoundary = true;
                        break;
                    }
                }
            }

            for (int i = 0; i < refs.Count; i++)
            {
                for (int j = i + 1; j < refs.Count; j++)
                {
                    int a = refs[i].TriIndex;
                    int b = refs[j].TriIndex;
                    adjacency[a].Add((b, isBoundary));
                    adjacency[b].Add((a, isBoundary));
                }
            }
        }

        return adjacency;
    }

    private static bool IsArrangementBoundaryEdge(
        int faceIndex,
        Vec3 edgeStart,
        Vec3 edgeEnd,
        IReadOnlyDictionary<int, List<IntersectionSegment>> faceSegments,
        double tolerance)
    {
        if (!faceSegments.TryGetValue(faceIndex, out var segments) || segments.Count == 0)
            return false;

        for (int i = 0; i < segments.Count; i++)
        {
            var seg = segments[i];
            if (seg.IsDegenerate)
                continue;

            if (IsSubSegmentOf(edgeStart, edgeEnd, seg.Start, seg.End, tolerance))
                return true;
            if (IsSubSegmentOf(edgeStart, edgeEnd, seg.End, seg.Start, tolerance))
                return true;
        }

        return false;
    }

    private static bool IsSubSegmentOf(
        Vec3 edgeStart,
        Vec3 edgeEnd,
        Vec3 segmentStart,
        Vec3 segmentEnd,
        double tolerance)
    {
        if (!TryProjectToSegment(edgeStart, segmentStart, segmentEnd, tolerance, out _))
            return false;
        if (!TryProjectToSegment(edgeEnd, segmentStart, segmentEnd, tolerance, out _))
            return false;
        return true;
    }

    private static bool TryProjectToSegment(
        Vec3 point,
        Vec3 segStart,
        Vec3 segEnd,
        double tolerance,
        out double t)
    {
        var dir = segEnd - segStart;
        double lenSq = dir.LengthSquared;
        if (lenSq <= tolerance * tolerance)
        {
            t = 0;
            return false;
        }

        t = Vec3.Dot(point - segStart, dir) / lenSq;
        double tEps = System.Math.Max(1e-6, tolerance / System.Math.Sqrt(lenSq));
        if (t < -tEps || t > 1.0 + tEps)
            return false;

        var closest = segStart + dir * t;
        return Vec3.DistanceSquared(point, closest) <= tolerance * tolerance;
    }

    private static List<Patch> FloodFill(IReadOnlyList<IReadOnlyList<(int Neighbor, bool IsBoundary)>> adjacency)
    {
        int n = adjacency.Count;
        var patchOf = new int[n];
        for (int i = 0; i < n; i++)
            patchOf[i] = -1;

        var patches = new List<Patch>();

        for (int i = 0; i < n; i++)
        {
            if (patchOf[i] >= 0)
                continue;

            var patch = new Patch(patches.Count);
            patches.Add(patch);

            var queue = new Queue<int>();
            queue.Enqueue(i);
            patchOf[i] = patch.Id;
            patch.SubTriangleIndices.Add(i);

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                foreach (var (neighbor, isBoundary) in adjacency[current])
                {
                    if (patchOf[neighbor] >= 0 || isBoundary)
                        continue;

                    patchOf[neighbor] = patch.Id;
                    patch.SubTriangleIndices.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        return patches;
    }

    private static long HashGrid(long gx, long gy, long gz)
    {
#if NET
        return HashCode.Combine(gx, gy, gz);
#else
        unchecked { return (gx * 397L ^ gy) * 397L ^ gz; }
#endif
    }
}
