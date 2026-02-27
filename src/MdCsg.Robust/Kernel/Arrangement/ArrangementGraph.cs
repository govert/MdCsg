namespace MdCsg.Robust.Kernel.Arrangement;

public sealed class ArrangementGraph
{
    public ArrangementGraph(
        IReadOnlyList<ArrangementVertex> vertices,
        IReadOnlyList<ArrangementEdge> edges,
        IReadOnlyDictionary<int, IReadOnlyList<int>> incidentEdgesByVertex,
        int coplanarFaceCountA,
        int coplanarFaceCountB,
        int coplanarPairNormalsAgreeCount = 0,
        int coplanarPairNormalsOpposeCount = 0)
    {
        Vertices = vertices;
        Edges = edges;
        IncidentEdgesByVertex = incidentEdgesByVertex;
        CoplanarFaceCountA = coplanarFaceCountA;
        CoplanarFaceCountB = coplanarFaceCountB;
        CoplanarPairNormalsAgreeCount = coplanarPairNormalsAgreeCount;
        CoplanarPairNormalsOpposeCount = coplanarPairNormalsOpposeCount;
    }

    public IReadOnlyList<ArrangementVertex> Vertices { get; }

    public IReadOnlyList<ArrangementEdge> Edges { get; }

    public IReadOnlyDictionary<int, IReadOnlyList<int>> IncidentEdgesByVertex { get; }

    public int CoplanarFaceCountA { get; }

    public int CoplanarFaceCountB { get; }

    public int CoplanarPairNormalsAgreeCount { get; }

    public int CoplanarPairNormalsOpposeCount { get; }

    public bool HasCoplanarPairs => CoplanarFaceCountA > 0 || CoplanarFaceCountB > 0;

    public bool HasOpposingCoplanarPairs => CoplanarPairNormalsOpposeCount > 0;
}
