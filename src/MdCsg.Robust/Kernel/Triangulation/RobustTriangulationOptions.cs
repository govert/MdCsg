using MdCsg.Math;

namespace MdCsg.Robust.Kernel.Triangulation;

public sealed class RobustTriangulationOptions
{
    public static RobustTriangulationOptions Default { get; } = new();

    // Transitional compatibility switch: strict robust execution should keep this false.
    public bool AllowLegacyFallback { get; init; } = false;

    public bool DeterministicOrdering { get; init; } = true;

    public bool DropDegenerateTriangles { get; init; } = true;

    public double DegenerateAreaTolerance { get; init; } = MathUtil.Epsilon;
}
