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
        var graph = IntersectionGraph.Compute(meshA, meshB, gridSize, parallel);
        return Build(graph, gridSize);
    }

    public static ArrangementGraph Build(
        IntersectionGraph graph,
        double gridSize = MathUtil.DefaultGridSize)
    {
        var vertices = new List<ArrangementVertex>();
        var edges = new List<ArrangementEdge>(graph.Segments.Count);
        var vertexByKey = new Dictionary<(long X, long Y, long Z), int>();
        var incidentMutable = new Dictionary<int, List<int>>();

        foreach (var seg in graph.Segments)
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
            graph.CoplanarFacesA.Count,
            graph.CoplanarFacesB.Count);
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
