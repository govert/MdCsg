using MdCsg.Bvh;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Robust.Kernel.Arrangement;

public static class ArrangementBuilder
{
    public static ArrangementGraph Build(
        HalfEdgeMesh meshA,
        HalfEdgeMesh meshB,
        double gridSize = MathUtil.DefaultGridSize,
        bool parallel = false)
    {
        // Keep deterministic behavior for conformance; parallel native arrangement
        // traversal can be added after ordering/stability guarantees are formalized.
        _ = parallel;

        var bvhA = BvhTree.Build(meshA);
        var bvhB = BvhTree.Build(meshB);
        var overlappingPairs = BvhTraversal.FindOverlappingPairs(bvhA, bvhB);

        var segments = new List<IntersectionSegment>(overlappingPairs.Count);
        var uniqueSegmentKeys = new HashSet<(long, long, long, long, long, long, int, int)>();
        var coplanarFacesA = new HashSet<int>();
        var coplanarFacesB = new HashSet<int>();

        foreach (var (faceA, faceB) in overlappingPairs)
        {
            var triA = GetTriangle(meshA, faceA);
            var triB = GetTriangle(meshB, faceB);

            if (TriTriIntersection.Intersect(triA, triB, out var regular))
            {
                TryAddSegment(regular, faceA, faceB, gridSize, segments, uniqueSegmentKeys);
                continue;
            }

            if (!TriTriIntersection.AreCoplanar(triA, triB))
                continue;

            coplanarFacesA.Add(faceA);
            coplanarFacesB.Add(faceB);

            if (!TriTriIntersection.IntersectCoplanar(
                triA,
                triB,
                out var segsForA,
                out var segsForB,
                out _))
            {
                continue;
            }

            foreach (var (start, end) in segsForA)
            {
                var seg = new IntersectionSegment(start, end, faceA, faceB);
                TryAddSegment(seg, faceA, faceB, gridSize, segments, uniqueSegmentKeys);
            }

            foreach (var (start, end) in segsForB)
            {
                var seg = new IntersectionSegment(start, end, faceA, faceB);
                TryAddSegment(seg, faceA, faceB, gridSize, segments, uniqueSegmentKeys);
            }
        }

        return BuildFromSegments(
            segments,
            gridSize,
            coplanarFacesA.Count,
            coplanarFacesB.Count);
    }

    public static ArrangementGraph Build(
        IntersectionGraph graph,
        double gridSize = MathUtil.DefaultGridSize)
    {
        return BuildFromSegments(
            graph.Segments,
            gridSize,
            graph.CoplanarFacesA.Count,
            graph.CoplanarFacesB.Count);
    }

    private static ArrangementGraph BuildFromSegments(
        IReadOnlyList<IntersectionSegment> segments,
        double gridSize,
        int coplanarFaceCountA,
        int coplanarFaceCountB)
    {
        var vertices = new List<ArrangementVertex>();
        var edges = new List<ArrangementEdge>(segments.Count);
        var vertexByKey = new Dictionary<(long X, long Y, long Z), int>();
        var incidentMutable = new Dictionary<int, List<int>>();

        foreach (var seg in segments)
        {
            int startId = GetOrAddVertex(seg.Start, gridSize, vertices, vertexByKey);
            int endId = GetOrAddVertex(seg.End, gridSize, vertices, vertexByKey);

            int edgeId = edges.Count;
            var edge = new ArrangementEdge(
                edgeId,
                startId,
                endId,
                seg.FaceIndexA,
                seg.FaceIndexB,
                IsDegenerate(startId, endId, seg));
            edges.Add(edge);

            AddIncidentEdge(incidentMutable, startId, edgeId);
            if (endId != startId)
                AddIncidentEdge(incidentMutable, endId, edgeId);
        }

        var incident = incidentMutable.ToDictionary(
            kvp => kvp.Key,
            kvp => (IReadOnlyList<int>)kvp.Value);

        return new ArrangementGraph(
            vertices,
            edges,
            incident,
            coplanarFaceCountA,
            coplanarFaceCountB);
    }

    private static int GetOrAddVertex(
        Vec3 point,
        double gridSize,
        List<ArrangementVertex> vertices,
        Dictionary<(long X, long Y, long Z), int> vertexByKey)
    {
        var key = MakeGridKey(point, gridSize);
        if (vertexByKey.TryGetValue(key, out int existing))
            return existing;

        int id = vertices.Count;
        vertices.Add(new ArrangementVertex(id, point));
        vertexByKey[key] = id;
        return id;
    }

    private static (long X, long Y, long Z) MakeGridKey(Vec3 point, double gridSize)
    {
        if (gridSize <= 0)
            return (ToLongBits(point.X), ToLongBits(point.Y), ToLongBits(point.Z));

        return (
            (long)System.Math.Round(point.X / gridSize),
            (long)System.Math.Round(point.Y / gridSize),
            (long)System.Math.Round(point.Z / gridSize));
    }

    private static long ToLongBits(double value)
        => BitConverter.DoubleToInt64Bits(value);

    private static bool IsDegenerate(int startId, int endId, IntersectionSegment seg)
        => startId == endId || seg.IsDegenerate;

    private static void TryAddSegment(
        IntersectionSegment seg,
        int faceA,
        int faceB,
        double gridSize,
        List<IntersectionSegment> segments,
        HashSet<(long, long, long, long, long, long, int, int)> uniqueSegmentKeys)
    {
        var snapped = new IntersectionSegment(
            SnapRounding.Snap(seg.Start, gridSize),
            SnapRounding.Snap(seg.End, gridSize),
            faceA,
            faceB);

        if (snapped.IsDegenerate)
            return;

        var startKey = MakeGridKey(snapped.Start, gridSize);
        var endKey = MakeGridKey(snapped.End, gridSize);

        // Canonicalize endpoint order so AB == BA.
        if (Compare(startKey, endKey) > 0)
        {
            (startKey, endKey) = (endKey, startKey);
        }

        var key = (
            startKey.X, startKey.Y, startKey.Z,
            endKey.X, endKey.Y, endKey.Z,
            faceA, faceB);

        if (!uniqueSegmentKeys.Add(key))
            return;

        segments.Add(snapped);
    }

    private static int Compare((long X, long Y, long Z) a, (long X, long Y, long Z) b)
    {
        int cmpX = a.X.CompareTo(b.X);
        if (cmpX != 0) return cmpX;
        int cmpY = a.Y.CompareTo(b.Y);
        if (cmpY != 0) return cmpY;
        return a.Z.CompareTo(b.Z);
    }

    private static Triangle3 GetTriangle(HalfEdgeMesh mesh, int faceIndex)
    {
        var face = mesh.Faces[faceIndex];
        face.GetTrianglePositions(out var a, out var b, out var c);
        return new Triangle3(a, b, c);
    }

    private static void AddIncidentEdge(Dictionary<int, List<int>> incident, int vertexId, int edgeId)
    {
        if (!incident.TryGetValue(vertexId, out var list))
        {
            list = [];
            incident[vertexId] = list;
        }

        list.Add(edgeId);
    }
}
