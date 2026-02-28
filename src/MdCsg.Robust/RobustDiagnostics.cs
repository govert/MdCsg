namespace MdCsg.Robust;

public sealed class RobustDiagnostics
{
    public TimeSpan TotalElapsed { get; init; }

    public TimeSpan OperationElapsed { get; init; }

    public int ArrangementVertexCount { get; init; }

    public int ArrangementEdgeCount { get; init; }

    public int ArrangementCoplanarFaceCountA { get; init; }

    public int ArrangementCoplanarFaceCountB { get; init; }

    public int ArrangementCoplanarPairNormalsAgreeCount { get; init; }

    public int ArrangementCoplanarPairNormalsOpposeCount { get; init; }

    public int ArrangementEndpointVertexCount { get; init; }

    public int ArrangementConnectedComponentCount { get; init; }

    public int PredicateEscalationCount { get; init; }

    public int PredicateDoubleCount { get; init; }

    public int PredicateExpansionCount { get; init; }

    public int PredicateExactCount { get; init; }

    public int TriangulationInvocationCount { get; init; }

    public int TriangulationNativeCount { get; init; }

    public int TriangulationLegacyFallbackCount { get; init; }

    public int TriangulationDroppedDegenerateCount { get; init; }

    public int TriangulationFallbackInvalidOrCrossingConstraintCount { get; init; }

    public int TriangulationFallbackPartitionFailureCount { get; init; }

    public int TriangulationFallbackConstrainedEarFailureCount { get; init; }

    public int TriangulationFallbackWorkBudgetExceededCount { get; init; }

    public IReadOnlyList<string> TriangulationFallbackSignatures { get; init; } = Array.Empty<string>();

    public int TriangulationNativeFailureCount { get; init; }

    public int TriangulationNativeFailureInvalidOrCrossingConstraintCount { get; init; }

    public int TriangulationNativeFailurePartitionFailureCount { get; init; }

    public int TriangulationNativeFailureConstrainedEarFailureCount { get; init; }

    public int TriangulationNativeFailureWorkBudgetExceededCount { get; init; }

    public IReadOnlyList<string> TriangulationNativeFailureSignatures { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> TriangulationNativeFailureCodes { get; init; } = Array.Empty<string>();

    public int ReconstructionBoundaryHalfEdgeCount { get; init; }

    public int ReconstructionOpenBoundaryLoopCount { get; init; }

    public int ReconstructionUnmatchedUndirectedEdgeCount { get; init; }

    public int ReconstructionNonManifoldUndirectedEdgeCount { get; init; }

    public int ReconstructionDroppedComponentCount { get; init; }

    public int ReconstructionArrangementSnapCount { get; init; }

    public int ReconstructionArrangementEdgeSnapCount { get; init; }

    public int ReconstructionComponentCount { get; init; }

    public int ReconstructionInvalidComponentCount { get; init; }

    public IReadOnlyList<string> ReconstructionInvariantCertificates { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> StageInvariantCertificates { get; init; } = Array.Empty<string>();

    public int ClassificationFallbackCount { get; init; }
}
