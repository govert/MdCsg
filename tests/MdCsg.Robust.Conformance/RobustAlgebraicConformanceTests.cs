using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;
using System.Linq;

namespace MdCsg.Robust.Conformance;

public class RobustAlgebraicConformanceTests
{
    private static readonly RobustOperationOptions StrictOpts = new()
    {
        Mode = RobustMode.Strict,
        Deterministic = true,
        UseRobustTriangulationKernel = true
    };

    [Fact]
    public void Union_IsCommutativeByVolume_AndClosed()
    {
        var a = Primitives.Sphere(Vec3.Zero, 1.2, 3);
        var b = Primitives.Cube(new Vec3(0.6, 0, 0), 1.5);

        var ab = RobustCsg.Union(a, b, StrictOpts);
        var ba = RobustCsg.Union(b, a, StrictOpts);

        AssertRobustClosedWithoutFallback(ab);
        AssertRobustClosedWithoutFallback(ba);

        double volAb = new Solid(ab.Result!.Mesh).Volume();
        double volBa = new Solid(ba.Result!.Mesh).Volume();
        Assert.InRange(System.Math.Abs(volAb - volBa), 0.0, 1e-3);
    }

    [Fact]
    public void Union_Idempotent_PreservesVolumeAndClosure()
    {
        var a = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        var result = RobustCsg.Union(a, a, StrictOpts);

        AssertRobustClosedWithoutFallback(result);

        double expected = a.Volume();
        double actual = new Solid(result.Result!.Mesh).Volume();
        Assert.InRange(System.Math.Abs(actual - expected), 0.0, 1e-3);
    }

    [Fact]
    public void MixedChain_ShowcaseStep2Style_StaysClosed()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.3, 3);
        var box = Primitives.Cube(Vec3.Zero, 1.8);
        var cut = Primitives.Cylinder(new Vec3(-1.5, 0, 0), Vec3.UnitX, 0.5, 3.0);

        var step1 = RobustCsg.Intersect(sphere, box, StrictOpts);
        AssertRobustClosedWithoutFallback(step1);
        var step1Solid = new Solid(step1.Result!.Mesh);

        var step2 = RobustCsg.Difference(step1Solid, cut, StrictOpts);
        AssertRobustClosedWithoutFallback(step2);
    }

    [Fact]
    public void StrictBooleanOps_EmitPatchAndClassificationCertificates()
    {
        var a = Primitives.Sphere(Vec3.Zero, 1.2, 3);
        var b = Primitives.Cube(new Vec3(0.6, 0, 0), 1.5);

        var union = RobustCsg.Union(a, b, StrictOpts);
        var intersection = RobustCsg.Intersect(a, b, StrictOpts);
        var difference = RobustCsg.Difference(a, b, StrictOpts);

        AssertRobustClosedWithoutFallback(union);
        AssertRobustClosedWithoutFallback(intersection);
        AssertRobustClosedWithoutFallback(difference);

        string unionCert = GetCert(union, "classification:");
        string intersectionCert = GetCert(intersection, "classification:");
        string differenceCert = GetCert(difference, "classification:");

        Assert.Contains("uncertified=", unionCert, StringComparison.Ordinal);
        Assert.Contains("uncertified=", intersectionCert, StringComparison.Ordinal);
        Assert.Contains("uncertified=", differenceCert, StringComparison.Ordinal);
        Assert.Contains("fingerprint=", unionCert, StringComparison.Ordinal);
        Assert.Contains("fingerprint=", intersectionCert, StringComparison.Ordinal);
        Assert.Contains("fingerprint=", differenceCert, StringComparison.Ordinal);
    }

    [Fact]
    public void StrictBooleanOps_EmitReconstructionPolicyCertificates()
    {
        var a = Primitives.Sphere(Vec3.Zero, 1.2, 3);
        var b = Primitives.Cube(new Vec3(0.6, 0, 0), 1.5);

        var union = RobustCsg.Union(a, b, StrictOpts);
        var intersection = RobustCsg.Intersect(a, b, StrictOpts);
        var difference = RobustCsg.Difference(a, b, StrictOpts);

        AssertRobustClosedWithoutFallback(union);
        AssertRobustClosedWithoutFallback(intersection);
        AssertRobustClosedWithoutFallback(difference);

        Assert.Contains("pass=1", GetPolicyCert(union), StringComparison.Ordinal);
        Assert.Contains("pass=1", GetPolicyCert(intersection), StringComparison.Ordinal);

        string diffCert = GetPolicyCert(difference);
        Assert.Contains("pass=1", diffCert, StringComparison.Ordinal);
        int fromB = ParseIntTag(diffCert, "fromB");
        int flipB = ParseIntTag(diffCert, "flipB");
        Assert.InRange(flipB, 0, fromB);
    }

    [Fact]
    public void StrictBooleanOps_SelectArrangementPatchExtractionMode()
    {
        var a = Primitives.Sphere(Vec3.Zero, 1.2, 3);
        var b = Primitives.Cube(new Vec3(0.6, 0, 0), 1.5);

        var union = RobustCsg.Union(a, b, StrictOpts);
        var intersection = RobustCsg.Intersect(a, b, StrictOpts);
        var difference = RobustCsg.Difference(a, b, StrictOpts);

        AssertRobustClosedWithoutFallback(union);
        AssertRobustClosedWithoutFallback(intersection);
        AssertRobustClosedWithoutFallback(difference);

        RobustDiagnosticsAssertions.AssertPatchExtractionMode(union.Diagnostics, PatchExtractionMode.Arrangement);
        RobustDiagnosticsAssertions.AssertPatchExtractionMode(intersection.Diagnostics, PatchExtractionMode.Arrangement);
        RobustDiagnosticsAssertions.AssertPatchExtractionMode(difference.Diagnostics, PatchExtractionMode.Arrangement);
    }

    [Fact]
    public void StrictUnion_ClassificationEvidenceFingerprint_IsDeterministic()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(0.75, 0, 0), 2.0);

        var baseline = RobustCsg.Union(a, b, StrictOpts);
        AssertRobustClosedWithoutFallback(baseline);
        string baselineCert = GetCert(baseline, "classification:");

        for (int i = 0; i < 5; i++)
        {
            var next = RobustCsg.Union(a, b, StrictOpts);
            AssertRobustClosedWithoutFallback(next);
            Assert.Equal(baselineCert, GetCert(next, "classification:"));
        }
    }

    private static void AssertRobustClosedWithoutFallback(RobustCsgResult result)
    {
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Result);
        Assert.Equal(0, MeshValidator.CountBoundaryEdges(result.Result!.Mesh));
        RobustDiagnosticsAssertions.AssertHasPatchExtractionCertificate(result.Diagnostics);
        RobustDiagnosticsAssertions.AssertNoTriangulationDegradation(result.Diagnostics);
    }

    private static string GetPolicyCert(RobustCsgResult result)
    {
        string? cert = result.Diagnostics.StageInvariantCertificates
            .LastOrDefault(static c => c.StartsWith("reconstruction-policy:", StringComparison.Ordinal));
        Assert.False(string.IsNullOrWhiteSpace(cert));
        return cert!;
    }

    private static string GetCert(RobustCsgResult result, string prefix)
    {
        string? cert = result.Diagnostics.StageInvariantCertificates
            .LastOrDefault(c => c.StartsWith(prefix, StringComparison.Ordinal));
        Assert.False(string.IsNullOrWhiteSpace(cert));
        return cert!;
    }

    private static int ParseIntTag(string cert, string tag)
    {
        var parts = cert.Split(';', StringSplitOptions.RemoveEmptyEntries);
        string prefix = tag + "=";
        string value = parts.FirstOrDefault(p => p.StartsWith(prefix, StringComparison.Ordinal)) ?? string.Empty;
        Assert.False(string.IsNullOrWhiteSpace(value));
        string text = value[prefix.Length..];
        Assert.True(int.TryParse(text, out int parsed), $"Invalid integer tag '{tag}' in certificate '{cert}'.");
        return parsed;
    }
}
