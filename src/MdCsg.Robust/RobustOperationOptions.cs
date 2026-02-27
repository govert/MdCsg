namespace MdCsg.Robust;

public sealed class RobustOperationOptions
{
    public static RobustOperationOptions Default { get; } = new();

    public RobustMode Mode { get; init; } = RobustMode.Strict;

    public bool Deterministic { get; init; } = true;

    public bool ValidateInput { get; init; } = true;

    public bool ValidateOutput { get; init; } = true;

    public bool AnalyzeInputIntersection { get; init; } = true;

    public bool TreatCoplanarIntersectionAsError { get; init; } = false;

    public bool TreatOpposingCoplanarPairsAsError { get; init; } = false;

    public bool TreatOpenArrangementAsError { get; init; } = false;

    public bool UseRobustTriangulationKernel { get; init; } = true;

    public bool FailOnValidationError { get; init; } = true;
}
