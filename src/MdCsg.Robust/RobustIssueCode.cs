namespace MdCsg.Robust;

public enum RobustIssueCode
{
    InputMeshContainsNonFiniteCoordinate,
    InputMeshNotClosed,
    InputMeshNotEdgeManifold,
    InputMeshHasDegenerateFaces,
    InputIntersectionContainsCoplanarPairs,
    InputIntersectionContainsOpposingCoplanarPairs,
    InputArrangementHasOpenEndpoints,
    StageInvariantViolation,
    TriangulationNativeFailure,
    TriangulationInvalidOrCrossingConstraints,
    TriangulationPartitioningFailed,
    TriangulationConstrainedEarFailed,
    TriangulationWorkBudgetExceeded,
    ReconstructionInvariantViolation,
    ReconstructionPatchSelectionFailed,
    ReconstructionStitchingFailed,
    OutputMeshNotClosed,
    OutputMeshNotEdgeManifold,
    OutputMeshHasDegenerateFaces
}
