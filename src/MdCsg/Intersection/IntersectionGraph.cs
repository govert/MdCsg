using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Intersection;

/// <summary>
/// Computes all intersection segments between two meshes using BVH-accelerated
/// dual-tree traversal.
/// </summary>
public class IntersectionGraph
{
    /// <summary>All intersection segments between the two meshes.</summary>
    public IReadOnlyList<IntersectionSegment> Segments { get; }

    /// <summary>Maps a face index in mesh A to its intersection segments.</summary>
    public IReadOnlyDictionary<int, List<IntersectionSegment>> FaceSegmentsA { get; }

    /// <summary>Maps a face index in mesh B to its intersection segments.</summary>
    public IReadOnlyDictionary<int, List<IntersectionSegment>> FaceSegmentsB { get; }

    /// <summary>
    /// Coplanar face pairs: maps face index (from either mesh) to normal agreement info.
    /// True = normals agree in direction.
    /// </summary>
    public IReadOnlyDictionary<int, bool> CoplanarFacesA { get; }

    /// <summary>Coplanar face pairs for mesh B.</summary>
    public IReadOnlyDictionary<int, bool> CoplanarFacesB { get; }

    private IntersectionGraph(
        List<IntersectionSegment> segments,
        Dictionary<int, List<IntersectionSegment>> faceSegmentsA,
        Dictionary<int, List<IntersectionSegment>> faceSegmentsB,
        Dictionary<int, bool> coplanarFacesA,
        Dictionary<int, bool> coplanarFacesB)
    {
        Segments = segments;
        FaceSegmentsA = faceSegmentsA;
        FaceSegmentsB = faceSegmentsB;
        CoplanarFacesA = coplanarFacesA;
        CoplanarFacesB = coplanarFacesB;
    }

    /// <summary>
    /// Computes the intersection graph between two meshes.
    /// </summary>
    /// <param name="meshA">First mesh.</param>
    /// <param name="meshB">Second mesh.</param>
    /// <param name="gridSize">Snap rounding grid size.</param>
    /// <param name="parallel">If true, process triangle-triangle tests in parallel.</param>
    public static IntersectionGraph Compute(
        HalfEdgeMesh meshA,
        HalfEdgeMesh meshB,
        double gridSize = MathUtil.DefaultGridSize,
        bool parallel = false)
    {
        var bvhA = BvhTree.Build(meshA);
        var bvhB = BvhTree.Build(meshB);

        var overlappingPairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);

        if (parallel && overlappingPairs.Count > 1)
            return ComputeParallel(meshA, meshB, overlappingPairs, gridSize);

        return ComputeSequential(meshA, meshB, overlappingPairs, gridSize);
    }

    /// <summary>
    /// Runs the edge-based dedup pass on both meshes' face segments.
    /// </summary>
    private static void DeduplicateAll(
        HalfEdgeMesh meshA, HalfEdgeMesh meshB,
        List<IntersectionSegment> segments,
        Dictionary<int, List<IntersectionSegment>> faceSegmentsA,
        Dictionary<int, List<IntersectionSegment>> faceSegmentsB,
        double gridSize)
    {
        DeduplicateEdgePoints(meshA, faceSegmentsA, segments, gridSize);
        DeduplicateEdgePoints(meshB, faceSegmentsB, segments, gridSize);
    }

    private static IntersectionGraph ComputeSequential(
        HalfEdgeMesh meshA,
        HalfEdgeMesh meshB,
        List<(int FaceA, int FaceB)> overlappingPairs,
        double gridSize)
    {
        var segments = new List<IntersectionSegment>();
        var faceSegmentsA = new Dictionary<int, List<IntersectionSegment>>();
        var faceSegmentsB = new Dictionary<int, List<IntersectionSegment>>();
        var coplanarFacesA = new Dictionary<int, bool>();
        var coplanarFacesB = new Dictionary<int, bool>();

        foreach (var (faceA, faceB) in overlappingPairs)
        {
            var triA = GetTriangle(meshA, faceA);
            var triB = GetTriangle(meshB, faceB);

            ProcessPairInto(triA, triB, faceA, faceB, gridSize,
                segments, faceSegmentsA, faceSegmentsB, coplanarFacesA, coplanarFacesB);
        }

        DeduplicateAll(meshA, meshB, segments, faceSegmentsA, faceSegmentsB, gridSize);

        return new IntersectionGraph(segments, faceSegmentsA, faceSegmentsB, coplanarFacesA, coplanarFacesB);
    }

    private static IntersectionGraph ComputeParallel(
        HalfEdgeMesh meshA,
        HalfEdgeMesh meshB,
        List<(int FaceA, int FaceB)> overlappingPairs,
        double gridSize)
    {
        int count = overlappingPairs.Count;
        var results = new PairOutput[count];

        System.Threading.Tasks.Parallel.For(0, count, i =>
        {
            var (faceA, faceB) = overlappingPairs[i];
            var triA = GetTriangle(meshA, faceA);
            var triB = GetTriangle(meshB, faceB);
            results[i] = ProcessPair(triA, triB, faceA, faceB, gridSize);
        });

        var graph = MergePairOutputs(results);

        // Dedup after merging parallel results
        DeduplicateAll(meshA, meshB,
            (List<IntersectionSegment>)graph.Segments,
            (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA,
            (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsB,
            gridSize);

        return graph;
    }

    /// <summary>
    /// Processes a single triangle pair into the shared result collections (sequential path).
    /// </summary>
    private static void ProcessPairInto(
        Triangle3 triA, Triangle3 triB, int faceA, int faceB, double gridSize,
        List<IntersectionSegment> segments,
        Dictionary<int, List<IntersectionSegment>> faceSegmentsA,
        Dictionary<int, List<IntersectionSegment>> faceSegmentsB,
        Dictionary<int, bool> coplanarFacesA,
        Dictionary<int, bool> coplanarFacesB)
    {
        if (TriTriIntersection.Intersect(triA, triB, out var seg))
        {
            seg = new IntersectionSegment(
                SnapRounding.Snap(seg.Start, gridSize),
                SnapRounding.Snap(seg.End, gridSize),
                faceA,
                faceB);

            if (!seg.IsDegenerate)
            {
                segments.Add(seg);
                AddToDict(faceSegmentsA, faceA, seg);
                AddToDict(faceSegmentsB, faceB, seg);
            }
        }
        else if (TriTriIntersection.AreCoplanar(triA, triB))
        {
            TriTriIntersection.IntersectCoplanar(triA, triB, out var segsForA, out var segsForB, out bool normalsAgree);

            // Always mark coplanar faces, even if no clipping segments were produced
            // (e.g., identical triangles share all edges, so no interior cuts exist)
            coplanarFacesA[faceA] = normalsAgree;
            coplanarFacesB[faceB] = normalsAgree;

            // Add clipping segments for face A (edges of triB clipped to triA)
            foreach (var (start, end) in segsForA)
            {
                var snapped = new IntersectionSegment(
                    SnapRounding.Snap(start, gridSize),
                    SnapRounding.Snap(end, gridSize),
                    faceA, faceB);
                if (!snapped.IsDegenerate)
                {
                    segments.Add(snapped);
                    AddToDict(faceSegmentsA, faceA, snapped);
                }
            }

            // Add clipping segments for face B (edges of triA clipped to triB)
            foreach (var (start, end) in segsForB)
            {
                var snapped = new IntersectionSegment(
                    SnapRounding.Snap(start, gridSize),
                    SnapRounding.Snap(end, gridSize),
                    faceA, faceB);
                if (!snapped.IsDegenerate)
                {
                    segments.Add(snapped);
                    AddToDict(faceSegmentsB, faceB, snapped);
                }
            }
        }
    }

    /// <summary>
    /// Processes a single triangle pair, returning the result (parallel path).
    /// </summary>
    private static PairOutput ProcessPair(
        Triangle3 triA, Triangle3 triB, int faceA, int faceB, double gridSize)
    {
        var output = new PairOutput { FaceA = faceA, FaceB = faceB };

        if (TriTriIntersection.Intersect(triA, triB, out var seg))
        {
            seg = new IntersectionSegment(
                SnapRounding.Snap(seg.Start, gridSize),
                SnapRounding.Snap(seg.End, gridSize),
                faceA, faceB);

            if (!seg.IsDegenerate)
                output.RegularSegment = seg;
        }
        else if (TriTriIntersection.AreCoplanar(triA, triB))
        {
            output.IsCoplanar = true;
            TriTriIntersection.IntersectCoplanar(triA, triB, out var segsForA, out var segsForB, out bool normalsAgree);
            output.NormalsAgree = normalsAgree;

            var snappedA = new List<IntersectionSegment>();
            foreach (var (start, end) in segsForA)
            {
                var snapped = new IntersectionSegment(
                    SnapRounding.Snap(start, gridSize),
                    SnapRounding.Snap(end, gridSize),
                    faceA, faceB);
                if (!snapped.IsDegenerate)
                    snappedA.Add(snapped);
            }
            if (snappedA.Count > 0)
                output.CoplanarSegsA = snappedA;

            var snappedB = new List<IntersectionSegment>();
            foreach (var (start, end) in segsForB)
            {
                var snapped = new IntersectionSegment(
                    SnapRounding.Snap(start, gridSize),
                    SnapRounding.Snap(end, gridSize),
                    faceA, faceB);
                if (!snapped.IsDegenerate)
                    snappedB.Add(snapped);
            }
            if (snappedB.Count > 0)
                output.CoplanarSegsB = snappedB;
        }

        return output;
    }

    /// <summary>
    /// Merges parallel pair outputs into a single IntersectionGraph.
    /// </summary>
    private static IntersectionGraph MergePairOutputs(PairOutput[] outputs)
    {
        var segments = new List<IntersectionSegment>();
        var faceSegmentsA = new Dictionary<int, List<IntersectionSegment>>();
        var faceSegmentsB = new Dictionary<int, List<IntersectionSegment>>();
        var coplanarFacesA = new Dictionary<int, bool>();
        var coplanarFacesB = new Dictionary<int, bool>();

        foreach (var output in outputs)
        {
            if (output.RegularSegment.HasValue)
            {
                var seg = output.RegularSegment.Value;
                segments.Add(seg);
                AddToDict(faceSegmentsA, output.FaceA, seg);
                AddToDict(faceSegmentsB, output.FaceB, seg);
            }

            if (output.IsCoplanar)
            {
                coplanarFacesA[output.FaceA] = output.NormalsAgree;
                coplanarFacesB[output.FaceB] = output.NormalsAgree;

                if (output.CoplanarSegsA != null)
                {
                    foreach (var seg in output.CoplanarSegsA)
                    {
                        segments.Add(seg);
                        AddToDict(faceSegmentsA, output.FaceA, seg);
                    }
                }

                if (output.CoplanarSegsB != null)
                {
                    foreach (var seg in output.CoplanarSegsB)
                    {
                        segments.Add(seg);
                        AddToDict(faceSegmentsB, output.FaceB, seg);
                    }
                }
            }
        }

        return new IntersectionGraph(segments, faceSegmentsA, faceSegmentsB, coplanarFacesA, coplanarFacesB);
    }

    /// <summary>
    /// Per-pair output for the parallel path.
    /// </summary>
    private struct PairOutput
    {
        /// <summary>Face index in mesh A.</summary>
        public int FaceA;

        /// <summary>Face index in mesh B.</summary>
        public int FaceB;

        /// <summary>Non-coplanar intersection segment (null if none or coplanar).</summary>
        public IntersectionSegment? RegularSegment;

        /// <summary>Whether this pair is coplanar.</summary>
        public bool IsCoplanar;

        /// <summary>Whether normals agree (only valid if IsCoplanar).</summary>
        public bool NormalsAgree;

        /// <summary>Coplanar segments for face A (already snapped, non-degenerate).</summary>
        public List<IntersectionSegment>? CoplanarSegsA;

        /// <summary>Coplanar segments for face B (already snapped, non-degenerate).</summary>
        public List<IntersectionSegment>? CoplanarSegsB;
    }

    /// <summary>
    /// Deduplicates intersection points that lie on the same original mesh edge.
    /// When two adjacent faces share an edge and both have an intersection point
    /// on it, the raw computed positions may differ slightly. This pass ensures
    /// they get identical Vec3 values by canonicalizing per-edge split points.
    /// </summary>
    private static void DeduplicateEdgePoints(
        HalfEdgeMesh mesh,
        Dictionary<int, List<IntersectionSegment>> faceSegments,
        List<IntersectionSegment> allSegments,
        double gridSize)
    {
        if (faceSegments.Count == 0) return;

        var tolSq = gridSize * gridSize;

        // Phase 1: Collect all intersection points on each original edge.
        // EdgeKey = packed (lo, hi) vertex IDs.
        var edgeCanonical = new Dictionary<long, List<Vec3>>();

        foreach (var kvp in faceSegments)
        {
            int faceIdx = kvp.Key;
            var segs = kvp.Value;
            if (faceIdx >= mesh.Faces.Count) continue;

            var face = mesh.Faces[faceIdx];
            var he0 = face.Edge;
            var he1 = he0.Next;
            var he2 = he1.Next;

            var faceEdges = new (Vec3 P0, Vec3 P1, long Key)[]
            {
                (he0.Origin.Position, he0.Target.Position, PackEdgeKey(he0.Origin.Id, he0.Target.Id)),
                (he1.Origin.Position, he1.Target.Position, PackEdgeKey(he1.Origin.Id, he1.Target.Id)),
                (he2.Origin.Position, he2.Target.Position, PackEdgeKey(he2.Origin.Id, he2.Target.Id))
            };

            foreach (var seg in segs)
            {
                RegisterPointOnEdge(seg.Start, faceEdges, edgeCanonical, gridSize, tolSq);
                RegisterPointOnEdge(seg.End, faceEdges, edgeCanonical, gridSize, tolSq);
            }
        }

        if (edgeCanonical.Count == 0) return;

        // Phase 2: Build a replacement map from any point to its canonical version.
        // For each edge, all points within gridSize of each other share the first one.
        var pointReplacements = new Dictionary<(long, long, long), Vec3>();

        foreach (var ecKvp in edgeCanonical)
        {
            var points = ecKvp.Value;
            if (points.Count <= 1) continue;

            // For each point, find its canonical (first point within gridSize)
            for (int i = 1; i < points.Count; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    if (Vec3.DistanceSquared(points[i], points[j]) < tolSq)
                    {
                        var key = MakeBitKey(points[i]);
                        if (!pointReplacements.ContainsKey(key))
                            pointReplacements[key] = points[j];
                        break;
                    }
                }
            }
        }

        if (pointReplacements.Count == 0) return;

        // Phase 3: Rewrite segments in the face-segments dictionary and allSegments list.
        RewriteSegments(faceSegments, allSegments, pointReplacements);
    }

    private static void RegisterPointOnEdge(
        Vec3 point,
        (Vec3 P0, Vec3 P1, long Key)[] faceEdges,
        Dictionary<long, List<Vec3>> edgeCanonical,
        double gridSize,
        double tolSq)
    {
        foreach (var (p0, p1, key) in faceEdges)
        {
            // Skip if point is at an original vertex
            if (Vec3.DistanceSquared(point, p0) < tolSq) return;
            if (Vec3.DistanceSquared(point, p1) < tolSq) return;

            var edge = p1 - p0;
            double edgeLenSq = edge.LengthSquared;
            if (edgeLenSq < tolSq) continue;

            double t = Vec3.Dot(point - p0, edge) / edgeLenSq;
            if (t <= gridSize || t >= 1.0 - gridSize) continue;

            var projected = p0 + edge * t;
            if (Vec3.DistanceSquared(point, projected) < tolSq)
            {
                if (!edgeCanonical.TryGetValue(key, out var list))
                {
                    list = [];
                    edgeCanonical[key] = list;
                }

                // Only add if not already present (bitwise)
                bool found = false;
                for (int i = 0; i < list.Count; i++)
                {
                    if (Vec3.DistanceSquared(list[i], point) < double.Epsilon)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found) list.Add(point);
                return;
            }
        }
    }

    private static void RewriteSegments(
        Dictionary<int, List<IntersectionSegment>> faceSegments,
        List<IntersectionSegment> allSegments,
        Dictionary<(long, long, long), Vec3> replacements)
    {
        // Rewrite allSegments
        for (int i = 0; i < allSegments.Count; i++)
        {
            var seg = allSegments[i];
            var start = TryReplace(seg.Start, replacements);
            var end = TryReplace(seg.End, replacements);
            if (start != seg.Start || end != seg.End)
                allSegments[i] = new IntersectionSegment(start, end, seg.FaceIndexA, seg.FaceIndexB);
        }

        // Rewrite faceSegments
        foreach (var fsKvp in faceSegments)
        {
            var segments = fsKvp.Value;
            for (int i = 0; i < segments.Count; i++)
            {
                var seg = segments[i];
                var start = TryReplace(seg.Start, replacements);
                var end = TryReplace(seg.End, replacements);
                if (start != seg.Start || end != seg.End)
                    segments[i] = new IntersectionSegment(start, end, seg.FaceIndexA, seg.FaceIndexB);
            }
        }
    }

    private static Vec3 TryReplace(Vec3 point, Dictionary<(long, long, long), Vec3> replacements)
    {
        var key = MakeBitKey(point);
        return replacements.TryGetValue(key, out var canonical) ? canonical : point;
    }

    private static (long, long, long) MakeBitKey(Vec3 v)
    {
        return (
            BitConverter.DoubleToInt64Bits(v.X),
            BitConverter.DoubleToInt64Bits(v.Y),
            BitConverter.DoubleToInt64Bits(v.Z));
    }

    private static long PackEdgeKey(int id0, int id1)
    {
        int lo = id0 < id1 ? id0 : id1;
        int hi = id0 < id1 ? id1 : id0;
        return ((long)lo << 32) | (uint)hi;
    }

    private static void AddToDict(Dictionary<int, List<IntersectionSegment>> dict, int faceIdx, IntersectionSegment seg)
    {
        if (!dict.TryGetValue(faceIdx, out var list))
        {
            list = [];
            dict[faceIdx] = list;
        }
        list.Add(seg);
    }

    private static Triangle3 GetTriangle(HalfEdgeMesh mesh, int faceIndex)
    {
        var face = mesh.Faces[faceIndex];
        face.GetTrianglePositions(out var a, out var b, out var c);
        return new Triangle3(a, b, c);
    }
}
