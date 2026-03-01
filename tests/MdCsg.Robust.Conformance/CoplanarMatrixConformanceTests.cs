using MdCsg.Api;
using MdCsg.Math;

namespace MdCsg.Robust.Conformance;

public class CoplanarMatrixConformanceTests
{
    private static readonly RobustOperationOptions StrictCoplanarOpts = new()
    {
        Mode = RobustMode.Strict,
        Deterministic = true,
        UseRobustTriangulationKernel = true,
        TreatCoplanarIntersectionAsError = false,
        TreatOpposingCoplanarPairsAsError = false,
        FailOnValidationError = false
    };

    [Fact]
    public void CoplanarSharedFace_EmitsOperationMappedCoplanarMatrixRows()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(2, 0, 0), 2.0);

        var union = RobustCsg.Union(a, b, StrictCoplanarOpts);
        var intersection = RobustCsg.Intersect(a, b, StrictCoplanarOpts);
        var difference = RobustCsg.Difference(a, b, StrictCoplanarOpts);

        string unionCert = GetCert(union, "coplanar-matrix:");
        string intersectionCert = GetCert(intersection, "coplanar-matrix:");
        string differenceCert = GetCert(difference, "coplanar-matrix:");

        Assert.Contains("pass=1", unionCert, StringComparison.Ordinal);
        Assert.Contains("pass=1", intersectionCert, StringComparison.Ordinal);
        Assert.Contains("pass=1", differenceCert, StringComparison.Ordinal);

        Assert.Contains("A:Union:", unionCert, StringComparison.Ordinal);
        Assert.Contains("B:Union:", unionCert, StringComparison.Ordinal);
        Assert.Contains("A:Intersection:", intersectionCert, StringComparison.Ordinal);
        Assert.Contains("B:Intersection:", intersectionCert, StringComparison.Ordinal);
        Assert.Contains("A:Difference:", differenceCert, StringComparison.Ordinal);
        Assert.Contains("B:Difference:", differenceCert, StringComparison.Ordinal);
    }

    [Fact]
    public void CoplanarMatrixCertificate_IsDeterministic_ForComplementTransformedUnion()
    {
        var baseA = Primitives.Cube(Vec3.Zero, 2.0);
        var baseB = Primitives.Cube(new Vec3(2, 0, 0), 2.0);
        var a = baseA.Complement();

        var baseline = RobustCsg.Union(a, baseB, StrictCoplanarOpts);
        string baselineCert = GetCert(baseline, "coplanar-matrix:");
        Assert.Contains("pass=1", baselineCert, StringComparison.Ordinal);

        for (int i = 0; i < 5; i++)
        {
            var next = RobustCsg.Union(a, baseB, StrictCoplanarOpts);
            Assert.Equal(baselineCert, GetCert(next, "coplanar-matrix:"));
        }
    }

    private static string GetCert(RobustCsgResult result, string prefix)
    {
        string? cert = result.Diagnostics.StageInvariantCertificates
            .LastOrDefault(c => c.StartsWith(prefix, StringComparison.Ordinal));
        Assert.False(string.IsNullOrWhiteSpace(cert));
        return cert!;
    }
}
