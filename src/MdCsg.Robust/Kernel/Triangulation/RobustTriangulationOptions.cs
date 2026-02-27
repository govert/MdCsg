using MdCsg.Math;

namespace MdCsg.Robust.Kernel.Triangulation;

public sealed class RobustTriangulationOptions
{
    public static RobustTriangulationOptions Default { get; } = new();

    public bool DeterministicOrdering { get; init; } = true;

    public bool DropDegenerateTriangles { get; init; } = true;

    public double DegenerateAreaTolerance { get; init; } = MathUtil.Epsilon;
}
