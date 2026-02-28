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
        Assert.Contains(step3.Issues, i => i.Code == RobustIssueCode.ReconstructionInvariantViolation);
        Assert.Contains(step3.Issues, i => i.Code == RobustIssueCode.OutputMeshNotClosed);
        Assert.Contains(step3.Issues, i => i.Code == RobustIssueCode.OutputMeshNotEdgeManifold);
        RobustDiagnosticsAssertions.AssertHasPatchExtractionCertificate(step3.Diagnostics);
        Assert.Contains(step3.Diagnostics.StageInvariantCertificates, static c => c.StartsWith("patch-extraction-candidates:", StringComparison.Ordinal));
        var candidates = GetStageCertificate(step3, "patch-extraction-candidates:");
        Assert.Contains("IntraFace:", candidates, StringComparison.Ordinal);
        Assert.Contains("Global:", candidates, StringComparison.Ordinal);
        Assert.Contains("Arrangement:", candidates, StringComparison.Ordinal);
        Assert.Contains(step3.Diagnostics.StageInvariantCertificates, c => c.StartsWith("reconstruction:fail;", StringComparison.Ordinal));
        RobustDiagnosticsAssertions.AssertNoTriangulationDegradation(step3.Diagnostics);
    }

    [Fact]
    public void PatchExtractionCandidateSignatures_AreDeterministic_OnStableOverlapCase()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(0.75, 0, 0), 2.0);
        var baseline = RobustCsg.Union(a, b, StrictRobustOpts);
        Assert.True(baseline.Succeeded);
        string baselineCandidates = GetStageCertificate(baseline, "patch-extraction-candidates:");

        for (int i = 0; i < 5; i++)
        {
            var next = RobustCsg.Union(a, b, StrictRobustOpts);
            Assert.True(next.Succeeded);
            Assert.Equal(baselineCandidates, GetStageCertificate(next, "patch-extraction-candidates:"));
        }
    }

    private static void AssertRobustClosedWithoutFallback(RobustCsgResult result)
    {
        Assert.True(result.Succeeded, BuildIssueMessage(result));
        Assert.NotNull(result.Result);
        Assert.Equal(0, MeshValidator.CountBoundaryEdges(result.Result!.Mesh));
        RobustDiagnosticsAssertions.AssertHasPatchExtractionCertificate(result.Diagnostics);
        RobustDiagnosticsAssertions.AssertNoTriangulationDegradation(result.Diagnostics);
    }

    private static void AssertRobustClosed(RobustCsgResult result)
    {
        Assert.True(result.Succeeded, BuildIssueMessage(result));
        Assert.NotNull(result.Result);
        Assert.Equal(0, MeshValidator.CountBoundaryEdges(result.Result!.Mesh));
    }

    private static string BuildIssueMessage(RobustCsgResult result)
    {
        var fallback = RobustDiagnosticsAssertions.BuildTriangulationDegradationMessage(result.Diagnostics);
        if (result.Issues.Count == 0)
            return $"No issues were reported. {fallback}";

        var issues = string.Join(" | ", result.Issues.Select(static issue =>
            $"{issue.Severity}:{issue.Code}:{issue.Message}"));
        return $"{issues} | {fallback}";
    }

    private static RobustCsgResult RunStep3()
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

        return RobustCsg.Difference(step2Solid, cylY, StrictRobustOpts);
    }

    private static string GetStageCertificate(RobustCsgResult result, string prefix)
    {
        string? cert = result.Diagnostics.StageInvariantCertificates
            .LastOrDefault(c => c.StartsWith(prefix, StringComparison.Ordinal));
        Assert.False(string.IsNullOrWhiteSpace(cert));
        return cert!;
    }
}
