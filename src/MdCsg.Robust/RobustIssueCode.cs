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
    OutputMeshNotClosed,
    OutputMeshNotEdgeManifold,
    OutputMeshHasDegenerateFaces
}
