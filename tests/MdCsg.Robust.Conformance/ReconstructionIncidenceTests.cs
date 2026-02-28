using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Operations;

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
        string baselineCert = GetReconstructionCert(baseline);

        for (int i = 0; i < 5; i++)
        {
            var next = RobustCsg.Union(a, b, opts);
            Assert.True(next.Succeeded);
            Assert.Equal(baselineCert, GetReconstructionCert(next));
        }
    }

    private static string GetReconstructionCert(RobustCsgResult result)
    {
        string? cert = result.Diagnostics.ReconstructionInvariantCertificates
            .LastOrDefault(static c => c.StartsWith("reconstruction:", StringComparison.Ordinal));
        Assert.False(string.IsNullOrWhiteSpace(cert));
        return cert!;
    }
}
