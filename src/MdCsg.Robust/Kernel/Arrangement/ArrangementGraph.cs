namespace MdCsg.Robust.Kernel.Arrangement;

public sealed class ArrangementGraph
{
    public ArrangementGraph(
        IReadOnlyList<ArrangementVertex> vertices,
        IReadOnlyList<ArrangementEdge> edges,
        IReadOnlyDictionary<int, IReadOnlyList<int>> incidentEdgesByVertex,
        int coplanarFaceCountA,
        int coplanarFaceCountB)
    {
        Vertices = vertices;
        Edges = edges;
        IncidentEdgesByVertex = incidentEdgesByVertex;
        CoplanarFaceCountA = coplanarFaceCountA;
        CoplanarFaceCountB = coplanarFaceCountB;
    }

    public IReadOnlyList<ArrangementVertex> Vertices { get; }

    public IReadOnlyList<ArrangementEdge> Edges { get; }

    public IReadOnlyDictionary<int, IReadOnlyList<int>> IncidentEdgesByVertex { get; }

    public int CoplanarFaceCountA { get; }

    public int CoplanarFaceCountB { get; }

    public bool HasCoplanarPairs => CoplanarFaceCountA > 0 || CoplanarFaceCountB > 0;
}
