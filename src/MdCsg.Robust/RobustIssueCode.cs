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
    TriangulationNativeFailure,
    TriangulationInvalidOrCrossingConstraints,
    TriangulationPartitioningFailed,
    TriangulationConstrainedEarFailed,
    TriangulationWorkBudgetExceeded,
    OutputMeshNotClosed,
    OutputMeshNotEdgeManifold,
    OutputMeshHasDegenerateFaces
}
