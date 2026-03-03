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
    public void PreviouslyBlockedCorpus_NowSucceeds()
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

        Assert.True(step3.Succeeded);
        Assert.DoesNotContain(step3.Issues, i => i.Code == RobustIssueCode.OutputMeshHasDegenerateFaces);
        Assert.DoesNotContain(step3.Issues, i => i.Code == RobustIssueCode.OutputMeshNotClosed);
        Assert.DoesNotContain(step3.Issues, i => i.Code == RobustIssueCode.OutputMeshNotEdgeManifold);
        string reconstructionCert = GetStageCertificate(step3, "reconstruction:");
        Assert.StartsWith("reconstruction:pass;", reconstructionCert, StringComparison.Ordinal);
        Assert.Equal(0, ParseIntTag(reconstructionCert, "boundary"));
        Assert.Equal(0, ParseIntTag(reconstructionCert, "unmatched"));
        string collapseCert = GetStageCertificate(step3, "deg-collinear-collapse:");
        Assert.True(ParseIntTag(collapseCert, "merged") >= 0);
        Assert.Contains("term=", collapseCert, StringComparison.Ordinal);
        string outputCert = GetStageCertificate(step3, "output:");
        Assert.StartsWith("output:pass;", outputCert, StringComparison.Ordinal);
        Assert.Equal(0, ParseIntTag(outputCert, "boundary"));
        Assert.Equal(1, ParseIntTag(outputCert, "manifold"));
        Assert.Equal(0, ParseIntTag(outputCert, "deg"));
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
