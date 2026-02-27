namespace MdCsg.Robust;

public sealed class RobustDiagnostics
{
    public TimeSpan TotalElapsed { get; init; }

    public TimeSpan OperationElapsed { get; init; }

    public int ArrangementVertexCount { get; init; }

    public int ArrangementEdgeCount { get; init; }

    public int ArrangementCoplanarFaceCountA { get; init; }

    public int ArrangementCoplanarFaceCountB { get; init; }

    public int ArrangementEndpointVertexCount { get; init; }

    public int ArrangementConnectedComponentCount { get; init; }

    public int PredicateEscalationCount { get; init; }

    public int PredicateDoubleCount { get; init; }

    public int PredicateExpansionCount { get; init; }

    public int PredicateExactCount { get; init; }

    public int ClassificationFallbackCount { get; init; }
}
