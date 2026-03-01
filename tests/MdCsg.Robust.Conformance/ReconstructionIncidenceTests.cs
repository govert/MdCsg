using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Operations;
using System.Linq;

namespace MdCsg.Robust.Conformance;

public class ReconstructionIncidenceTests
{
    [Fact]
    public void BoundaryIncidence_OnClosedCube_IsZero()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var summary = MeshStitcher.AnalyzeBoundaryIncidence(cube.Mesh);

        Assert.Equal(0, summary.BoundaryHalfEdgeCount);
        Assert.Equal(0, summary.OpenBoundaryVertexCount);
        Assert.Equal(0, summary.UnmatchedUndirectedEdgeCount);
        Assert.Equal(0, summary.NonManifoldUndirectedEdgeCount);
    }

    [Fact]
    public void StrictUnion_ReconstructionCertificate_IsDeterministicAcrossRuns()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(0.75, 0, 0), 2.0);
        var opts = new RobustOperationOptions
        {
            Mode = RobustMode.Strict,
            Deterministic = true,
            UseRobustTriangulationKernel = true
        };

        var baseline = RobustCsg.Union(a, b, opts);
        Assert.True(baseline.Succeeded);
        Assert.True(baseline.Diagnostics.ReconstructionArrangementSnapCount >= 0);
        Assert.True(baseline.Diagnostics.ReconstructionArrangementEdgeSnapCount >= 0);
        Assert.True(baseline.Diagnostics.ReconstructionComponentCount > 0);
        Assert.Equal(0, baseline.Diagnostics.ReconstructionInvalidComponentCount);
        string baselineCert = GetReconstructionCert(baseline);
        string baselinePreCert = GetStageCert(baseline, "reconstruction-pre:");
        string baselineAuthorityCert = GetStageCert(baseline, "reconstruction-authority:");
        Assert.Contains("arrSnap=", baselineCert, StringComparison.Ordinal);
        Assert.Contains("arrEdgeSnap=", baselineCert, StringComparison.Ordinal);
        Assert.Contains("components=", baselineCert, StringComparison.Ordinal);
        Assert.Contains("invalidComponents=", baselineCert, StringComparison.Ordinal);
        Assert.Contains("nonWorse=1", baselineCert, StringComparison.Ordinal);
        Assert.Contains("invalidComponents=", baselinePreCert, StringComparison.Ordinal);
        Assert.Contains("pass=1", baselineAuthorityCert, StringComparison.Ordinal);
        Assert.Contains("authority=", baselineAuthorityCert, StringComparison.Ordinal);
        Assert.Contains("mode=", baselineAuthorityCert, StringComparison.Ordinal);
        Assert.Contains("boundary=", baselineAuthorityCert, StringComparison.Ordinal);

        for (int i = 0; i < 5; i++)
        {
            var next = RobustCsg.Union(a, b, opts);
            Assert.True(next.Succeeded);
            Assert.Equal(
                baseline.Diagnostics.ReconstructionArrangementEdgeSnapCount,
                next.Diagnostics.ReconstructionArrangementEdgeSnapCount);
            Assert.Equal(baseline.Diagnostics.ReconstructionComponentCount, next.Diagnostics.ReconstructionComponentCount);
            Assert.Equal(baseline.Diagnostics.ReconstructionInvalidComponentCount, next.Diagnostics.ReconstructionInvalidComponentCount);
            Assert.Equal(baselinePreCert, GetStageCert(next, "reconstruction-pre:"));
            Assert.Equal(baselineAuthorityCert, GetStageCert(next, "reconstruction-authority:"));
            Assert.Equal(baselineCert, GetReconstructionCert(next));
        }
    }

    [Fact]
    public void StrictUnion_PatchAndClassificationCertificates_AreDeterministicAcrossRuns()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(0.75, 0, 0), 2.0);
        var opts = new RobustOperationOptions
        {
            Mode = RobustMode.Strict,
            Deterministic = true,
            UseRobustTriangulationKernel = true
        };

        var baseline = RobustCsg.Union(a, b, opts);
        Assert.True(baseline.Succeeded);
        string baselinePatch = GetStageCert(baseline, "patch-extraction:");
        string baselineClassification = GetStageCert(baseline, "classification:");

        for (int i = 0; i < 5; i++)
        {
            var next = RobustCsg.Union(a, b, opts);
            Assert.True(next.Succeeded);
            Assert.Equal(baselinePatch, GetStageCert(next, "patch-extraction:"));
            Assert.Equal(baselineClassification, GetStageCert(next, "classification:"));
        }
    }

    [Fact]
    public void CloseBoundaryLoopsDeterministic_ClosesSimpleLoop()
    {
        var mesh = BuildOpenQuadMesh();
        var before = MeshStitcher.AnalyzeBoundaryIncidence(mesh);
        Assert.True(before.BoundaryHalfEdgeCount > 0);

        var summary = MeshStitcher.CloseBoundaryLoopsDeterministic(mesh);
        Assert.True(summary.ClosedLoopCount > 0);
        Assert.Equal(0, summary.OpenChainCount);

        var after = MeshStitcher.AnalyzeBoundaryIncidence(mesh);
        Assert.Equal(0, after.BoundaryHalfEdgeCount);
        Assert.Equal(0, after.OpenBoundaryVertexCount);
    }

    [Fact]
    public void AnalyzeBoundaryLoopAssembly_IsDeterministic_OnMultiLoopBoundaryMesh()
    {
        var mesh = BuildAmbiguousBoundaryMesh();
        var first = MeshStitcher.AnalyzeBoundaryLoopAssembly(mesh);
        var second = MeshStitcher.AnalyzeBoundaryLoopAssembly(mesh);
        Assert.Equal(first, second);
        Assert.True(first.ClosedLoopCount > 0);
    }

    private static string GetReconstructionCert(RobustCsgResult result)
    {
        string? cert = result.Diagnostics.ReconstructionInvariantCertificates
            .LastOrDefault(static c => c.StartsWith("reconstruction:", StringComparison.Ordinal));
        Assert.False(string.IsNullOrWhiteSpace(cert));
        return cert!;
    }

    private static string GetStageCert(RobustCsgResult result, string prefix)
    {
        string? cert = result.Diagnostics.StageInvariantCertificates
            .LastOrDefault(c => c.StartsWith(prefix, StringComparison.Ordinal));
        Assert.False(string.IsNullOrWhiteSpace(cert));
        return cert!;
    }

    private static HalfEdgeMesh BuildOpenQuadMesh()
    {
        var triangles = new List<Triangle3>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(1, 1, 0)),
            new(new Vec3(0, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0))
        };
        return new MeshBuilder(0.0).Build(triangles);
    }

    private static HalfEdgeMesh BuildAmbiguousBoundaryMesh()
    {
        var triangles = new List<Triangle3>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(0, 0, 0), new Vec3(-1, 0, 0), new Vec3(0, -1, 0))
        };
        return new MeshBuilder(0.0).Build(triangles);
    }
}
