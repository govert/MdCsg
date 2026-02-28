namespace MdCsg.Robust.Kernel.Triangulation;

public sealed record RobustTriangulationResult(
    IReadOnlyList<(int A, int B, int C)> Triangles,
    int DroppedDegenerateTriangleCount,
    bool UsedLegacyKernel)
{
    public bool Succeeded { get; init; } = true;

    public RobustTriangulationFallbackReason FailureReason { get; init; } =
        RobustTriangulationFallbackReason.None;

    public RobustTriangulationFailureStage FailureStage { get; init; } =
        RobustTriangulationFailureStage.None;

    public string? FailureCode { get; init; }

    public string? FailureSignature { get; init; }

    public RobustTriangulationFallbackReason LegacyFallbackReason { get; init; } =
        RobustTriangulationFallbackReason.None;

    public string? LegacyFallbackSignature { get; init; }
}
