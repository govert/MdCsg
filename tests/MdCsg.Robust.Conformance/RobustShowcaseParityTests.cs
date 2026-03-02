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
    public void ChainedCsgSceneCase_Step3_ReproducesDegenerateOutputDefect_WithZeroFallback()
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
        Assert.Contains(step3.Issues, i => i.Code == RobustIssueCode.OutputMeshHasDegenerateFaces);
        Assert.DoesNotContain(step3.Issues, i => i.Code == RobustIssueCode.OutputMeshNotClosed);
        Assert.DoesNotContain(step3.Issues, i => i.Code == RobustIssueCode.OutputMeshNotEdgeManifold);
        RobustDiagnosticsAssertions.AssertHasPatchExtractionCertificate(step3.Diagnostics);
        Assert.Contains(step3.Diagnostics.StageInvariantCertificates, static c => c.StartsWith("patch-extraction-candidates:", StringComparison.Ordinal));
        var candidates = GetStageCertificate(step3, "patch-extraction-candidates:");
        var signatures = candidates["patch-extraction-candidates:".Length..]
            .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        Assert.Equal(3, signatures.Length);
        Assert.Contains(signatures, static s => s.StartsWith("IntraFace:", StringComparison.Ordinal));
        Assert.Contains(signatures, static s => s.StartsWith("Global:", StringComparison.Ordinal));
        Assert.Contains(signatures, static s => s.StartsWith("Arrangement:", StringComparison.Ordinal));
        foreach (string signature in signatures)
        {
            Assert.Contains("authority=", signature, StringComparison.Ordinal);
            Assert.Contains("boundary=", signature, StringComparison.Ordinal);
            Assert.Contains("manifold=", signature, StringComparison.Ordinal);
            Assert.Contains("components=", signature, StringComparison.Ordinal);
            Assert.Contains("patchA=", signature, StringComparison.Ordinal);
            Assert.Contains("patchB=", signature, StringComparison.Ordinal);
            Assert.Contains("deg=", signature, StringComparison.Ordinal);
            Assert.Contains("tri=", signature, StringComparison.Ordinal);
        }
        string reconstructionCert = GetStageCertificate(step3, "reconstruction:");
        Assert.StartsWith("reconstruction:pass;", reconstructionCert, StringComparison.Ordinal);
        Assert.Equal(0, ParseIntTag(reconstructionCert, "boundary"));
        Assert.Equal(0, ParseIntTag(reconstructionCert, "openLoops"));
        Assert.Equal(0, ParseIntTag(reconstructionCert, "unmatched"));
        Assert.Contains(step3.Diagnostics.StageInvariantCertificates, c => c.StartsWith("reconstruction-pre:", StringComparison.Ordinal));
        Assert.Contains("nonWorse=", reconstructionCert, StringComparison.Ordinal);
        string degPrunePre = GetStageCertificate(step3, "deg-prune:phase=pre;");
        string degPrunePost = GetStageCertificate(step3, "deg-prune:phase=post;");
        Assert.True(ParseIntTag(degPrunePre, "before") > 0);
        Assert.True(ParseIntTag(degPrunePre, "removed") > 0);
        Assert.True(ParseIntTag(degPrunePre, "afterRemove") >= 0);
        Assert.Equal(
            ParseIntTag(degPrunePre, "after") - ParseIntTag(degPrunePre, "afterRemove"),
            ParseIntTag(degPrunePre, "resealIntro"));
        Assert.Equal(1, ParseIntTag(degPrunePre, "resealSafe"));
        Assert.True(ParseIntTag(degPrunePre, "resealLoopDegSkipped") >= 0);
        Assert.Equal(
            ParseIntTag(degPrunePre, "before") - ParseIntTag(degPrunePre, "after"),
            ParseIntTag(degPrunePre, "netRemoved"));
        int preIters = ParseIntTag(degPrunePre, "iters");
        int preApplied = ParseIntTag(degPrunePre, "applied");
        Assert.InRange(preIters, 1, 3);
        Assert.InRange(preApplied, 0, preIters);
        Assert.Contains("term=", degPrunePre, StringComparison.Ordinal);
        Assert.Equal(1, ParseIntTag(degPrunePre, "closedGuard"));
        int preAccepted = ParseIntTag(degPrunePre, "accepted");
        Assert.True(preAccepted is 0 or 1);
        if (preAccepted == 1)
        {
            Assert.Equal(1, ParseIntTag(degPrunePre, "resealSafe"));
            Assert.Equal(0, ParseIntTag(degPrunePre, "boundaryAfter"));
            Assert.Equal(0, ParseIntTag(degPrunePre, "unmatchedAfter"));
        }
        Assert.True(ParseIntTag(degPrunePost, "before") > 0);
        Assert.True(ParseIntTag(degPrunePost, "removed") > 0);
        Assert.True(ParseIntTag(degPrunePost, "afterRemove") >= 0);
        Assert.Equal(
            ParseIntTag(degPrunePost, "after") - ParseIntTag(degPrunePost, "afterRemove"),
            ParseIntTag(degPrunePost, "resealIntro"));
        Assert.Equal(1, ParseIntTag(degPrunePost, "resealSafe"));
        Assert.True(ParseIntTag(degPrunePost, "resealLoopDegSkipped") >= 0);
        Assert.Equal(
            ParseIntTag(degPrunePost, "before") - ParseIntTag(degPrunePost, "after"),
            ParseIntTag(degPrunePost, "netRemoved"));
        int postIters = ParseIntTag(degPrunePost, "iters");
        int postApplied = ParseIntTag(degPrunePost, "applied");
        Assert.InRange(postIters, 1, 3);
        Assert.InRange(postApplied, 0, postIters);
        Assert.Contains("term=", degPrunePost, StringComparison.Ordinal);
        Assert.Equal(1, ParseIntTag(degPrunePost, "closedGuard"));
        int postAccepted = ParseIntTag(degPrunePost, "accepted");
        Assert.True(postAccepted is 0 or 1);
        if (postAccepted == 1)
        {
            Assert.Equal(1, ParseIntTag(degPrunePost, "resealSafe"));
            Assert.Equal(0, ParseIntTag(degPrunePost, "boundaryAfter"));
            Assert.Equal(0, ParseIntTag(degPrunePost, "unmatchedAfter"));
        }
        string outputCert = GetStageCertificate(step3, "output:");
        Assert.Contains("output:fail;", outputCert, StringComparison.Ordinal);
        Assert.Equal(0, ParseIntTag(outputCert, "boundary"));
        Assert.Equal(1, ParseIntTag(outputCert, "manifold"));
        int outputDeg = ParseIntTag(outputCert, "deg");
        Assert.True(outputDeg > 0);
        string residualCert = GetStageCertificate(step3, "deg-residual:");
        Assert.Equal(1, ParseIntTag(residualCert, "v"));
        Assert.Equal(outputDeg, ParseIntTag(residualCert, "count"));
        Assert.Equal(outputDeg, ParseIntTag(residualCert, "expected"));
        Assert.Equal(1, ParseIntTag(residualCert, "countMatch"));
        int taxDupVid = ParseIntTag(residualCert, "taxDupVid");
        int taxZeroEdge = ParseIntTag(residualCert, "taxZeroEdge");
        int taxDupPos = ParseIntTag(residualCert, "taxDupPos");
        int taxCollinear = ParseIntTag(residualCert, "taxCollinear");
        Assert.Equal(outputDeg, taxDupVid + taxZeroEdge + taxDupPos + taxCollinear);
        Assert.Contains("taxHash=", residualCert, StringComparison.Ordinal);
        Assert.Contains("sample=", residualCert, StringComparison.Ordinal);
        int expectedOutputDeg = postAccepted == 1
            ? ParseIntTag(degPrunePost, "after")
            : ParseIntTag(degPrunePost, "before");
        Assert.Equal(expectedOutputDeg, outputDeg);
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

    private static int ParseIntTag(string cert, string tag)
    {
        string prefix = tag + "=";
        var parts = cert.Split(';', StringSplitOptions.RemoveEmptyEntries);
        string value = parts.FirstOrDefault(p => p.StartsWith(prefix, StringComparison.Ordinal)) ?? string.Empty;
        Assert.False(string.IsNullOrWhiteSpace(value));
        string text = value[prefix.Length..];
        Assert.True(int.TryParse(text, out int parsed), $"Invalid integer tag '{tag}' in certificate '{cert}'.");
        return parsed;
    }
}
