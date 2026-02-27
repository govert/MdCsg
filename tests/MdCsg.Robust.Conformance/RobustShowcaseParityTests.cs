using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Robust.Conformance;

public class RobustShowcaseParityTests
{
    private static readonly RobustOperationOptions StrictRobustOpts = new()
    {
        Mode = RobustMode.Strict,
        Deterministic = true,
        UseRobustTriangulationKernel = true
    };

    [Fact]
    public void CsgOperationsSceneCases_StrictMode_StayClosed_AndUseZeroFallback()
    {
        var a = Primitives.Sphere(Vec3.Zero, 1.2, 3);
        var b = Primitives.Cube(new Vec3(0.6, 0, 0), 1.5);

        var union = RobustCsg.Union(a, b, StrictRobustOpts);
        var intersection = RobustCsg.Intersect(a, b, StrictRobustOpts);
        var difference = RobustCsg.Difference(a, b, StrictRobustOpts);

        AssertRobustClosedWithoutFallback(union);
        AssertRobustClosedWithoutFallback(intersection);
        AssertRobustClosedWithoutFallback(difference);
    }

    [Fact]
    public void ChainedCsgSceneCase_Step2_StaysClosed()
    {
        var y = new Vec3(0, 1, 0);
        var sphere = Primitives.Sphere(Vec3.Zero, 1.3, 3);
        var box = Primitives.Cube(Vec3.Zero, 1.8);
        var cylX = Primitives.Cylinder(new Vec3(-1.5, 0, 0), new Vec3(1, 0, 0), 0.5, 3.0);

        var step1 = RobustCsg.Intersect(sphere, box, StrictRobustOpts);
        AssertRobustClosed(step1);
        var step1Solid = new Solid(step1.Result!.Mesh);

        var step2 = RobustCsg.Difference(step1Solid, cylX, StrictRobustOpts);
        AssertRobustClosedWithoutFallback(step2);
    }

    [Fact]
    public void ChainedCsgSceneCase_Step3_ReproducesTopologyDefect_WithZeroFallback()
    {
        var y = new Vec3(0, 1, 0);
        var sphere = Primitives.Sphere(Vec3.Zero, 1.3, 3);
        var box = Primitives.Cube(Vec3.Zero, 1.8);
        var cylX = Primitives.Cylinder(new Vec3(-1.5, 0, 0), new Vec3(1, 0, 0), 0.5, 3.0);
        var cylY = Primitives.Cylinder(new Vec3(0, -1.5, 0), y, 0.5, 3.0);

        var step1 = RobustCsg.Intersect(sphere, box, StrictRobustOpts);
        AssertRobustClosedWithoutFallback(step1);
        var step1Solid = new Solid(step1.Result!.Mesh);

        var step2 = RobustCsg.Difference(step1Solid, cylX, StrictRobustOpts);
        AssertRobustClosedWithoutFallback(step2);
        var step2Solid = new Solid(step2.Result!.Mesh);

        var step3 = RobustCsg.Difference(step2Solid, cylY, StrictRobustOpts);
        Assert.False(step3.Succeeded);
        Assert.Contains(step3.Issues, i => i.Code == RobustIssueCode.OutputMeshNotClosed);
        Assert.Contains(step3.Issues, i => i.Code == RobustIssueCode.OutputMeshNotEdgeManifold);
        Assert.True(
            step3.Diagnostics.TriangulationLegacyFallbackCount == 0,
            BuildFallbackMessage(step3.Diagnostics));
        Assert.Equal(0, step3.Diagnostics.TriangulationFallbackInvalidOrCrossingConstraintCount);
        Assert.Equal(0, step3.Diagnostics.TriangulationFallbackPartitionFailureCount);
        Assert.Equal(0, step3.Diagnostics.TriangulationFallbackConstrainedEarFailureCount);
        Assert.Empty(step3.Diagnostics.TriangulationFallbackSignatures);
    }

    private static void AssertRobustClosedWithoutFallback(RobustCsgResult result)
    {
        Assert.True(result.Succeeded, BuildIssueMessage(result));
        Assert.NotNull(result.Result);
        Assert.Equal(0, MeshValidator.CountBoundaryEdges(result.Result!.Mesh));
        Assert.True(
            result.Diagnostics.TriangulationLegacyFallbackCount == 0,
            BuildFallbackMessage(result.Diagnostics));
        Assert.Equal(0, result.Diagnostics.TriangulationFallbackInvalidOrCrossingConstraintCount);
        Assert.Equal(0, result.Diagnostics.TriangulationFallbackPartitionFailureCount);
        Assert.Equal(0, result.Diagnostics.TriangulationFallbackConstrainedEarFailureCount);
        Assert.Empty(result.Diagnostics.TriangulationFallbackSignatures);
    }

    private static void AssertRobustClosed(RobustCsgResult result)
    {
        Assert.True(result.Succeeded, BuildIssueMessage(result));
        Assert.NotNull(result.Result);
        Assert.Equal(0, MeshValidator.CountBoundaryEdges(result.Result!.Mesh));
    }

    private static string BuildFallbackMessage(RobustDiagnostics diagnostics)
    {
        var top = diagnostics.TriangulationFallbackSignatures.Count == 0
            ? "<none>"
            : string.Join(" | ", diagnostics.TriangulationFallbackSignatures);
        return $"LegacyFallback={diagnostics.TriangulationLegacyFallbackCount}, "
            + $"InvalidOrCrossing={diagnostics.TriangulationFallbackInvalidOrCrossingConstraintCount}, "
            + $"Partition={diagnostics.TriangulationFallbackPartitionFailureCount}, "
            + $"ConstrainedEar={diagnostics.TriangulationFallbackConstrainedEarFailureCount}, "
            + $"Signatures={top}";
    }

    private static string BuildIssueMessage(RobustCsgResult result)
    {
        var fallback = BuildFallbackMessage(result.Diagnostics);
        if (result.Issues.Count == 0)
            return $"No issues were reported. {fallback}";

        var issues = string.Join(" | ", result.Issues.Select(static issue =>
            $"{issue.Severity}:{issue.Code}:{issue.Message}"));
        return $"{issues} | {fallback}";
    }
}
