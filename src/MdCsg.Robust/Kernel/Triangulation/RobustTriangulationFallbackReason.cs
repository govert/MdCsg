namespace MdCsg.Robust.Kernel.Triangulation;

public enum RobustTriangulationFallbackReason
{
    None = 0,
    InvalidOrCrossingConstraints = 1,
    PartitioningFailed = 2,
    ConstrainedEarFailed = 3,
    WorkBudgetExceeded = 4
}
