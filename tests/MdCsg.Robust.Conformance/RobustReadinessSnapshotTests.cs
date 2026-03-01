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
        Assert.Contains(step3.Diagnostics.StageInvariantCertificates, c => c.StartsWith("reconstruction:pass;", StringComparison.Ordinal));
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
}
