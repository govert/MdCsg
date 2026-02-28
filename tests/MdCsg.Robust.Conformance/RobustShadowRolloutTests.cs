using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Robust.Conformance;

public class RobustShadowRolloutTests
{
    private const double MaxRelativeVolumeDeltaVsLegacy = 0.15;

    private static readonly RobustOperationOptions StrictOpts = new()
    {
        Mode = RobustMode.Strict,
        Deterministic = true,
        UseRobustTriangulationKernel = true
    };

    [Fact]
    public void StableCorpus_HasNoUnclassifiedShadowDivergences()
    {
        var cases = new (string Name, Solid A, Solid B)[]
        {
            ("cube-cube-overlap",
                Primitives.Cube(Vec3.Zero, 2.0),
                Primitives.Cube(new Vec3(0.75, 0, 0), 2.0)),
            ("sphere-cube-offset",
                Primitives.Sphere(Vec3.Zero, 1.2, 3),
                Primitives.Cube(new Vec3(0.6, 0, 0), 1.5)),
            ("cube-cube-diagonal-overlap",
                Primitives.Cube(Vec3.Zero, 2.0),
                Primitives.Cube(new Vec3(0.6, 0.6, 0.2), 2.0))
        };

        var operations = new[]
        {
            RobustCsgOperation.Union,
            RobustCsgOperation.Intersection,
            RobustCsgOperation.Difference
        };

        var unclassified = new List<string>();
        foreach (var @case in cases)
        {
            foreach (var op in operations)
            {
                var robust = ExecuteRobust(op, @case.A, @case.B);
                var legacy = ExecuteLegacy(op, @case.A, @case.B);
                string? divergence = ClassifyDivergence(@case.Name, op, robust, legacy);
                if (divergence is not null)
                    unclassified.Add(divergence);
            }
        }

        Assert.True(
            unclassified.Count == 0,
            "Unclassified robust-vs-legacy divergences detected:" + Environment.NewLine
            + string.Join(Environment.NewLine, unclassified));
    }

    private static string? ClassifyDivergence(
        string caseName,
        RobustCsgOperation op,
        RobustCsgResult robust,
        CsgResult legacy)
    {
        if (!robust.Succeeded || robust.Result is null)
            return $"{caseName}:{op}:robust-failed";

        RobustDiagnosticsAssertions.AssertNoTriangulationDegradation(robust.Diagnostics);

        int robustBoundary = MeshValidator.CountBoundaryEdges(robust.Result.Mesh);
        int legacyBoundary = MeshValidator.CountBoundaryEdges(legacy.Mesh);
        if (robustBoundary > legacyBoundary)
            return $"{caseName}:{op}:boundary-regression:{robustBoundary}>{legacyBoundary}";
        if (robustBoundary != 0)
            return $"{caseName}:{op}:robust-open:{robustBoundary}";

        if (legacyBoundary == 0)
        {
            double robustVolume = System.Math.Abs(new Solid(robust.Result.Mesh).Volume());
            double legacyVolume = System.Math.Abs(new Solid(legacy.Mesh).Volume());
            double scale = System.Math.Max(1e-6, System.Math.Max(robustVolume, legacyVolume));
            double relErr = System.Math.Abs(robustVolume - legacyVolume) / scale;
            if (relErr > MaxRelativeVolumeDeltaVsLegacy)
                return $"{caseName}:{op}:volume-delta:{relErr:R}";
        }

        return null;
    }

    private static RobustCsgResult ExecuteRobust(RobustCsgOperation op, Solid a, Solid b)
        => op switch
        {
            RobustCsgOperation.Union => RobustCsg.Union(a, b, StrictOpts),
            RobustCsgOperation.Intersection => RobustCsg.Intersect(a, b, StrictOpts),
            RobustCsgOperation.Difference => RobustCsg.Difference(a, b, StrictOpts),
            _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
        };

    private static CsgResult ExecuteLegacy(RobustCsgOperation op, Solid a, Solid b)
        => op switch
        {
            RobustCsgOperation.Union => Csg.Union(a, b, new CsgOptions { Parallel = false }),
            RobustCsgOperation.Intersection => Csg.Intersect(a, b, new CsgOptions { Parallel = false }),
            RobustCsgOperation.Difference => Csg.Difference(a, b, new CsgOptions { Parallel = false }),
            _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
        };
}
