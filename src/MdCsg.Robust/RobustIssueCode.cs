namespace MdCsg.Robust;

public enum RobustIssueCode
{
    InputMeshContainsNonFiniteCoordinate,
    InputMeshNotClosed,
    InputMeshNotEdgeManifold,
    InputMeshHasDegenerateFaces,
    InputIntersectionContainsCoplanarPairs,
    InputArrangementHasOpenEndpoints,
    OutputMeshNotClosed,
    OutputMeshNotEdgeManifold,
    OutputMeshHasDegenerateFaces
}
