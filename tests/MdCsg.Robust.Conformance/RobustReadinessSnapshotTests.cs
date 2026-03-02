using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Robust.Conformance;

public class RobustReadinessSnapshotTests
{
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
    public void StableStrictCorpus_HasNoTriangulationDebt_AndClosedOutputs(
        string caseName,
        Solid a,
        Solid b)
    {
        var union = RobustCsg.Union(a, b, StrictOpts);
        var intersection = RobustCsg.Intersect(a, b, StrictOpts);
        var difference = RobustCsg.Difference(a, b, StrictOpts);

        AssertStrictHealthy(caseName, "union", union);
        AssertStrictHealthy(caseName, "intersection", intersection);
        AssertStrictHealthy(caseName, "difference", difference);
    }

    [Fact]
    public void KnownBlockerCorpus_IsExplicitlyFailClosed()
    {
        var y = new Vec3(0, 1, 0);
        var sphere = Primitives.Sphere(Vec3.Zero, 1.3, 3);
        var box = Primitives.Cube(Vec3.Zero, 1.8);
        var cylX = Primitives.Cylinder(new Vec3(-1.5, 0, 0), new Vec3(1, 0, 0), 0.5, 3.0);
        var cylY = Primitives.Cylinder(new Vec3(0, -1.5, 0), y, 0.5, 3.0);

        var step1 = RobustCsg.Intersect(sphere, box, StrictOpts);
        Assert.True(step1.Succeeded);
        var step2 = RobustCsg.Difference(new Solid(step1.Result!.Mesh), cylX, StrictOpts);
        Assert.True(step2.Succeeded);
        var step3 = RobustCsg.Difference(new Solid(step2.Result!.Mesh), cylY, StrictOpts);

        Assert.False(step3.Succeeded);
        Assert.Contains(step3.Issues, i => i.Code == RobustIssueCode.OutputMeshHasDegenerateFaces);
        Assert.DoesNotContain(step3.Issues, i => i.Code == RobustIssueCode.OutputMeshNotClosed);
        Assert.DoesNotContain(step3.Issues, i => i.Code == RobustIssueCode.OutputMeshNotEdgeManifold);
        string reconstructionCert = GetStageCertificate(step3, "reconstruction:");
        Assert.StartsWith("reconstruction:pass;", reconstructionCert, StringComparison.Ordinal);
        Assert.Equal(0, ParseIntTag(reconstructionCert, "boundary"));
        Assert.Equal(0, ParseIntTag(reconstructionCert, "unmatched"));
        Assert.True(ParseIntTag(reconstructionCert, "snapCollapseReject") >= 0);
        Assert.True(ParseIntTag(reconstructionCert, "snapDegReject") >= 0);
        string prePrune = GetStageCertificate(step3, "deg-prune:phase=pre;");
        string postPrune = GetStageCertificate(step3, "deg-prune:phase=post;");
        Assert.True(ParseIntTag(prePrune, "before") > 0);
        Assert.True(ParseIntTag(prePrune, "removed") > 0);
        Assert.True(ParseIntTag(prePrune, "afterRemove") >= 0);
        Assert.Equal(
            ParseIntTag(prePrune, "after") - ParseIntTag(prePrune, "afterRemove"),
            ParseIntTag(prePrune, "resealIntro"));
        Assert.Equal(1, ParseIntTag(prePrune, "resealSafe"));
        Assert.True(ParseIntTag(prePrune, "resealLoopDegSkipped") >= 0);
        int preLoopDupVid = ParseIntTag(prePrune, "resealLoopDupVidSkipped");
        int preLoopZeroEdge = ParseIntTag(prePrune, "resealLoopZeroEdgeSkipped");
        int preLoopDupPos = ParseIntTag(prePrune, "resealLoopDupPosSkipped");
        int preLoopCollinear = ParseIntTag(prePrune, "resealLoopCollinearSkipped");
        Assert.Equal(
            ParseIntTag(prePrune, "resealLoopDegSkipped"),
            preLoopDupVid + preLoopZeroEdge + preLoopDupPos + preLoopCollinear);
        Assert.Equal(
            ParseIntTag(prePrune, "before") - ParseIntTag(prePrune, "after"),
            ParseIntTag(prePrune, "netRemoved"));
        int preIters = ParseIntTag(prePrune, "iters");
        int preApplied = ParseIntTag(prePrune, "applied");
        Assert.InRange(preIters, 1, 3);
        Assert.InRange(preApplied, 0, preIters);
        Assert.Contains("term=", prePrune, StringComparison.Ordinal);
        int preClosedGuard = ParseIntTag(prePrune, "closedGuard");
        Assert.True(preClosedGuard is 0 or 1);
        int preAccepted = ParseIntTag(prePrune, "accepted");
        Assert.True(preAccepted is 0 or 1);
        if (preClosedGuard == 1 && preAccepted == 1)
        {
            Assert.Equal(1, ParseIntTag(prePrune, "resealSafe"));
            Assert.Equal(0, ParseIntTag(prePrune, "boundaryAfter"));
            Assert.Equal(0, ParseIntTag(prePrune, "unmatchedAfter"));
        }
        Assert.True(ParseIntTag(postPrune, "before") > 0);
        Assert.True(ParseIntTag(postPrune, "removed") > 0);
        Assert.True(ParseIntTag(postPrune, "afterRemove") >= 0);
        Assert.Equal(
            ParseIntTag(postPrune, "after") - ParseIntTag(postPrune, "afterRemove"),
            ParseIntTag(postPrune, "resealIntro"));
        Assert.Equal(1, ParseIntTag(postPrune, "resealSafe"));
        Assert.True(ParseIntTag(postPrune, "resealLoopDegSkipped") >= 0);
        int postLoopDupVid = ParseIntTag(postPrune, "resealLoopDupVidSkipped");
        int postLoopZeroEdge = ParseIntTag(postPrune, "resealLoopZeroEdgeSkipped");
        int postLoopDupPos = ParseIntTag(postPrune, "resealLoopDupPosSkipped");
        int postLoopCollinear = ParseIntTag(postPrune, "resealLoopCollinearSkipped");
        Assert.Equal(
            ParseIntTag(postPrune, "resealLoopDegSkipped"),
            postLoopDupVid + postLoopZeroEdge + postLoopDupPos + postLoopCollinear);
        Assert.Equal(
            ParseIntTag(postPrune, "before") - ParseIntTag(postPrune, "after"),
            ParseIntTag(postPrune, "netRemoved"));
        int postIters = ParseIntTag(postPrune, "iters");
        int postApplied = ParseIntTag(postPrune, "applied");
        Assert.InRange(postIters, 1, 3);
        Assert.InRange(postApplied, 0, postIters);
        Assert.Contains("term=", postPrune, StringComparison.Ordinal);
        int postClosedGuard = ParseIntTag(postPrune, "closedGuard");
        Assert.True(postClosedGuard is 0 or 1);
        int postAccepted = ParseIntTag(postPrune, "accepted");
        Assert.True(postAccepted is 0 or 1);
        if (postClosedGuard == 1 && postAccepted == 1)
        {
            Assert.Equal(1, ParseIntTag(postPrune, "resealSafe"));
            Assert.Equal(0, ParseIntTag(postPrune, "boundaryAfter"));
            Assert.Equal(0, ParseIntTag(postPrune, "unmatchedAfter"));
        }
        string localRepairCert = GetStageCertificate(step3, "deg-local-repair:");
        int localRepairGate = ParseIntTag(localRepairCert, "gate");
        int localRepairBefore = ParseIntTag(localRepairCert, "before");
        int localRepairAfter = ParseIntTag(localRepairCert, "after");
        int localRepairAttempted = ParseIntTag(localRepairCert, "attempted");
        int localRepairRemoved = ParseIntTag(localRepairCert, "removed");
        int localRepairSingleTry = ParseIntTag(localRepairCert, "singleTry");
        int localRepairPairTry = ParseIntTag(localRepairCert, "pairTry");
        int localRepairTripleTry = ParseIntTag(localRepairCert, "tripleTry");
        int localRepairMultiApplied = ParseIntTag(localRepairCert, "multiApplied");
        int localRepairMaxArity = ParseIntTag(localRepairCert, "maxArity");
        int localRepairIters = ParseIntTag(localRepairCert, "iters");
        int localRepairApplied = ParseIntTag(localRepairCert, "applied");
        Assert.True(localRepairGate is 0 or 1);
        Assert.True(localRepairBefore >= localRepairAfter);
        Assert.True(localRepairAttempted >= localRepairRemoved);
        Assert.True(localRepairSingleTry >= 0);
        Assert.Equal(0, localRepairPairTry);
        Assert.Equal(0, localRepairTripleTry);
        Assert.Equal(0, localRepairMultiApplied);
        Assert.InRange(localRepairMaxArity, 0, 1);
        Assert.True(localRepairRemoved >= 0);
        Assert.True(localRepairApplied >= 0);
        Assert.True(localRepairIters >= localRepairApplied);
        Assert.Contains("term=", localRepairCert, StringComparison.Ordinal);
        string outputCert = GetStageCertificate(step3, "output:");
        Assert.StartsWith("output:fail;", outputCert, StringComparison.Ordinal);
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
        int collinearOnly = ParseIntTag(residualCert, "collinearOnly");
        int colAdjPairs = ParseIntTag(residualCert, "colAdjPairs");
        int colVertSpan = ParseIntTag(residualCert, "colVertSpan");
        Assert.Equal(outputDeg, taxDupVid + taxZeroEdge + taxDupPos + taxCollinear);
        Assert.True(collinearOnly is 0 or 1);
        Assert.True(colAdjPairs >= 0);
        Assert.True(colVertSpan >= 0);
        if (taxCollinear == outputDeg && outputDeg > 0)
            Assert.Equal(1, collinearOnly);
        Assert.Contains("taxHash=", residualCert, StringComparison.Ordinal);
        Assert.Contains("colAdjHash=", residualCert, StringComparison.Ordinal);
        Assert.Contains("sample=", residualCert, StringComparison.Ordinal);
        int expectedOutputDeg = postAccepted == 1
            ? ParseIntTag(postPrune, "after")
            : ParseIntTag(postPrune, "before");
        Assert.Equal(expectedOutputDeg, outputDeg);
        RobustDiagnosticsAssertions.AssertNoTriangulationDegradation(step3.Diagnostics);
    }

    private static void AssertStrictHealthy(string caseName, string op, RobustCsgResult result)
    {
        Assert.True(result.Succeeded, $"{caseName}:{op} strict robust failed.");
        Assert.NotNull(result.Result);
        Assert.Equal(0, MeshValidator.CountBoundaryEdges(result.Result!.Mesh));
        RobustDiagnosticsAssertions.AssertNoTriangulationDegradation(result.Diagnostics);

        Assert.Contains(
            result.Diagnostics.StageInvariantCertificates,
            static c => c.StartsWith("reconstruction:pass;", StringComparison.Ordinal)
                && c.Contains("nonWorse=1", StringComparison.Ordinal));
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
