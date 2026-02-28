namespace MdCsg.Robust.Kernel.Triangulation;

public enum RobustTriangulationFailureStage
{
    None = 0,
    FacePointSet = 1,
    Partition = 2,
    ConstrainedEar = 3,
    ConstraintValidation = 4
}
