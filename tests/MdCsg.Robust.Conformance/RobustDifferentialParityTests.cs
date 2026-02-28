using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Robust.Conformance;

public class RobustDifferentialParityTests
{
    // Legacy output is a parity reference, not a geometric oracle.
    private const double MaxRelativeVolumeDeltaVsLegacy = 0.15;

    private static readonly RobustOperationOptions StrictOpts = new()
    {
        Mode = RobustMode.Strict,
        Deterministic = true,
        UseRobustTriangulationKernel = true
    };

    public static IEnumerable<object[]> StableCases()
    {
        yield return
        [
            "cube-cube-overlap",
            Primitives.Cube(Vec3.Zero, 2.0),
            Primitives.Cube(new Vec3(0.75, 0, 0), 2.0)
        ];
        yield return
        [
            "sphere-cube-offset",
            Primitives.Sphere(Vec3.Zero, 1.2, 3),
            Primitives.Cube(new Vec3(0.6, 0, 0), 1.5)
        ];
        yield return
        [
            "cube-cube-diagonal-overlap",
            Primitives.Cube(Vec3.Zero, 2.0),
            Primitives.Cube(new Vec3(0.6, 0.6, 0.2), 2.0)
        ];
    }

    [Theory]
    [MemberData(nameof(StableCases))]
    public void RobustUnion_IsTopologyAndVolumeConsistent_WithLegacyOnStableCases(
        string caseName,
        Solid a,
        Solid b)
    {
        AssertDifferential(caseName, a, b, RobustCsgOperation.Union);
    }

    [Theory]
    [MemberData(nameof(StableCases))]
    public void RobustIntersection_IsTopologyAndVolumeConsistent_WithLegacyOnStableCases(
        string caseName,
        Solid a,
        Solid b)
    {
        AssertDifferential(caseName, a, b, RobustCsgOperation.Intersection);
    }

    [Theory]
    [MemberData(nameof(StableCases))]
    public void RobustDifference_IsTopologyAndVolumeConsistent_WithLegacyOnStableCases(
        string caseName,
        Solid a,
        Solid b)
    {
        AssertDifferential(caseName, a, b, RobustCsgOperation.Difference);
    }

    private static void AssertDifferential(
        string caseName,
        Solid a,
        Solid b,
        RobustCsgOperation op)
    {
        var robust = ExecuteRobust(op, a, b);
        var legacy = ExecuteLegacy(op, a, b);

        Assert.True(robust.Succeeded, $"{caseName}:{op} robust failed.");
        Assert.NotNull(robust.Result);
        RobustDiagnosticsAssertions.AssertNoTriangulationDegradation(robust.Diagnostics);
        Assert.Contains(
            robust.Diagnostics.StageInvariantCertificates,
            static c => c.StartsWith("patch-extraction:mode=", StringComparison.Ordinal));
        Assert.Contains(
            robust.Diagnostics.StageInvariantCertificates,
            c => c.StartsWith("reconstruction-policy:", StringComparison.Ordinal)
                && c.Contains("pass=1", StringComparison.Ordinal));

        int robustBoundary = MeshValidator.CountBoundaryEdges(robust.Result!.Mesh);
        int legacyBoundary = MeshValidator.CountBoundaryEdges(legacy.Mesh);
        Assert.True(robustBoundary <= legacyBoundary, $"{caseName}:{op} robust boundary regression.");
        Assert.Equal(0, robustBoundary);

        if (legacyBoundary == 0)
        {
            double robustVolume = System.Math.Abs(new Solid(robust.Result.Mesh).Volume());
            double legacyVolume = System.Math.Abs(new Solid(legacy.Mesh).Volume());
            double scale = System.Math.Max(1e-6, System.Math.Max(robustVolume, legacyVolume));
            double relErr = System.Math.Abs(robustVolume - legacyVolume) / scale;
            Assert.InRange(relErr, 0.0, MaxRelativeVolumeDeltaVsLegacy);
        }
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
