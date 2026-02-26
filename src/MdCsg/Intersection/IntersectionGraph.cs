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

        return MergePairOutputs(results);
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
