using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Robust.Conformance;

public class RobustConformanceBacklogTests
{
    [Fact]
    public void CoplanarSharedFace_StrictMode_FailsFastWithCoplanarIssue()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(2, 0, 0), 2.0); // shared face at x=1
        var result = RobustCsg.Union(a, b, new RobustOperationOptions
        {
            Mode = RobustMode.Strict,
            AnalyzeInputIntersection = true,
            TreatCoplanarIntersectionAsError = true,
            FailOnValidationError = true
        });

        Assert.False(result.Succeeded);
        Assert.Null(result.Result);
        Assert.Contains(result.Issues, i => i.Code == RobustIssueCode.InputIntersectionContainsCoplanarPairs);
        Assert.True(result.Diagnostics.ArrangementCoplanarFaceCountA > 0);
        Assert.True(result.Diagnostics.ArrangementCoplanarFaceCountB > 0);
    }

    [Fact]
    public void CoplanarSharedFace_StrictMode_FailsFastWithOpposingCoplanarIssue()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(2, 0, 0), 2.0); // shared face with opposing outward normals
        var result = RobustCsg.Union(a, b, new RobustOperationOptions
        {
            Mode = RobustMode.Strict,
            AnalyzeInputIntersection = true,
            TreatCoplanarIntersectionAsError = false,
            TreatOpposingCoplanarPairsAsError = true,
            FailOnValidationError = true
        });

        Assert.False(result.Succeeded);
        Assert.Null(result.Result);
        Assert.Contains(result.Issues, i => i.Code == RobustIssueCode.InputIntersectionContainsOpposingCoplanarPairs);
        Assert.True(result.Diagnostics.ArrangementCoplanarPairNormalsOpposeCount > 0);
    }

    [Fact]
    public void ThinSlab_HalfSpaceChain_StrictMode_StaysClosed()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var top = HalfSpace.FromPointAndNormal(new Vec3(0, 0, 0.01), Vec3.UnitZ);
        var bottom = HalfSpace.FromPointAndNormal(new Vec3(0, 0, -0.01), -Vec3.UnitZ);

        var r1 = new Solid(Csg.Intersect(cube, top).Mesh);
        var r2 = Csg.Intersect(r1, bottom);
        var robust = RobustCsg.Union(new Solid(r2.Mesh), r1, new RobustOperationOptions
        {
            Mode = RobustMode.Strict,
            AnalyzeInputIntersection = true,
            TreatOpenArrangementAsError = true,
            FailOnValidationError = true
        });

        Assert.True(robust.Succeeded);
        Assert.NotNull(robust.Result);
        Assert.Equal(0, MeshValidator.CountBoundaryEdges(robust.Result!.Mesh));
        Assert.DoesNotContain(robust.Issues, i => i.Severity == RobustIssueSeverity.Error);
    }

    [Fact]
    public void KissingContact_Intersection_ProducesNoSpuriousVolume()
    {
        var a = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        var b = Primitives.Sphere(new Vec3(2.0, 0, 0), 1.0, 3); // tangent at one point
        var result = RobustCsg.Intersect(a, b, new RobustOperationOptions { Mode = RobustMode.Strict });

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Result);
        Assert.True(new Solid(result.Result!.Mesh).Volume() < 1e-6);
        Assert.Equal(0, result.Diagnostics.TriangulationLegacyFallbackCount);
        Assert.Equal(0, result.Diagnostics.TriangulationFallbackInvalidOrCrossingConstraintCount);
        Assert.Equal(0, result.Diagnostics.TriangulationFallbackPartitionFailureCount);
        Assert.Equal(0, result.Diagnostics.TriangulationFallbackConstrainedEarFailureCount);
        Assert.Equal(0, result.Diagnostics.TriangulationFallbackWorkBudgetExceededCount);
        Assert.Empty(result.Diagnostics.TriangulationFallbackSignatures);
    }

    [Fact]
    public void ThinSlab_HalfSpaceChain_RemainsClosed()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var top = HalfSpace.FromPointAndNormal(new Vec3(0, 0, 0.01), Vec3.UnitZ);
        var bottom = HalfSpace.FromPointAndNormal(new Vec3(0, 0, -0.01), -Vec3.UnitZ);

        var r1 = new Solid(Csg.Intersect(cube, top).Mesh);
        var r2 = Csg.Intersect(r1, bottom);
        var robust = RobustCsg.Union(new Solid(r2.Mesh), r1, new RobustOperationOptions { Mode = RobustMode.Strict });

        Assert.True(robust.Succeeded);
        Assert.NotNull(robust.Result);
        Assert.Equal(0, MeshValidator.CountBoundaryEdges(robust.Result!.Mesh));
        Assert.Equal(0, robust.Diagnostics.TriangulationLegacyFallbackCount);
        Assert.Equal(0, robust.Diagnostics.TriangulationFallbackInvalidOrCrossingConstraintCount);
        Assert.Equal(0, robust.Diagnostics.TriangulationFallbackPartitionFailureCount);
        Assert.Equal(0, robust.Diagnostics.TriangulationFallbackConstrainedEarFailureCount);
        Assert.Equal(0, robust.Diagnostics.TriangulationFallbackWorkBudgetExceededCount);
        Assert.Empty(robust.Diagnostics.TriangulationFallbackSignatures);
    }

    [Fact]
    public void StableOverlap_Union_UsesZeroLegacyTriangulationFallback()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(0.75, 0, 0), 2.0);

        var result = RobustCsg.Union(a, b, new RobustOperationOptions
        {
            Mode = RobustMode.Strict,
            Deterministic = true,
            UseRobustTriangulationKernel = true
        });

        Assert.True(result.Succeeded);
        Assert.True(
            result.Diagnostics.TriangulationLegacyFallbackCount == 0,
            BuildFallbackMessage(result.Diagnostics));
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
            + $"WorkBudgetExceeded={diagnostics.TriangulationFallbackWorkBudgetExceededCount}, "
            + $"Signatures={top}";
    }
}
