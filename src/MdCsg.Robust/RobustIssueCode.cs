namespace MdCsg.Robust;

public enum RobustIssueCode
{
    InputMeshContainsNonFiniteCoordinate,
    InputMeshNotClosed,
    InputMeshNotEdgeManifold,
    InputMeshHasDegenerateFaces,
    OutputMeshNotClosed,
    OutputMeshNotEdgeManifold,
    OutputMeshHasDegenerateFaces
}
