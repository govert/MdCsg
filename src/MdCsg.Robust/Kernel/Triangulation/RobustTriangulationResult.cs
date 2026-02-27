namespace MdCsg.Robust.Kernel.Triangulation;

public sealed record RobustTriangulationResult(
    IReadOnlyList<(int A, int B, int C)> Triangles,
    int DroppedDegenerateTriangleCount,
    bool UsedLegacyKernel)
{
    public RobustTriangulationFallbackReason LegacyFallbackReason { get; init; } =
        RobustTriangulationFallbackReason.None;

    public string? LegacyFallbackSignature { get; init; }
}
