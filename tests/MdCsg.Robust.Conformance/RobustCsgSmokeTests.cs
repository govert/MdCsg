using MdCsg.Api;
using MdCsg.Math;

namespace MdCsg.Robust.Conformance;

public class RobustCsgSmokeTests
{
    [Fact]
    public void Union_Smoke_Succeeds()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(0.75, 0, 0), 2.0);

        var result = RobustCsg.Union(a, b);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Result);
        Assert.True(result.Result!.FaceCount > 0);
        Assert.True(result.Diagnostics.TotalElapsed >= TimeSpan.Zero);
        Assert.True(result.Diagnostics.OperationElapsed >= TimeSpan.Zero);
        Assert.True(result.Diagnostics.ArrangementVertexCount >= 0);
        Assert.True(result.Diagnostics.ArrangementEdgeCount >= 0);
        Assert.True(result.Diagnostics.ArrangementConnectedComponentCount >= 0);
    }

    [Fact]
    public void Union_IsDeterministicInStrictMode_ForBasicCase()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(0.5, 0.5, 0), 2.0);
        var opts = new RobustOperationOptions
        {
            Mode = RobustMode.Strict,
            Deterministic = true
        };

        var r1 = RobustCsg.Union(a, b, opts);
        var r2 = RobustCsg.Union(a, b, opts);

        Assert.True(r1.Succeeded);
        Assert.True(r2.Succeeded);
        Assert.NotNull(r1.Result);
        Assert.NotNull(r2.Result);
        Assert.Equal(r1.Result!.FaceCount, r2.Result!.FaceCount);
        Assert.Equal(r1.Result.VertexCount, r2.Result.VertexCount);
    }

    [Fact]
    public void Union_ReportsTriangulationTelemetry_WhenRobustKernelEnabled()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(0.75, 0, 0), 2.0);
        var opts = new RobustOperationOptions
        {
            Mode = RobustMode.Strict,
            Deterministic = true,
            UseRobustTriangulationKernel = true
        };

        var result = RobustCsg.Union(a, b, opts);

        Assert.True(result.Succeeded);
        Assert.True(result.Diagnostics.TriangulationInvocationCount > 0);
        RobustDiagnosticsAssertions.AssertNoTriangulationDegradation(result.Diagnostics);
        Assert.Equal(
            result.Diagnostics.TriangulationInvocationCount,
            result.Diagnostics.TriangulationNativeCount + result.Diagnostics.TriangulationLegacyFallbackCount);
        Assert.Equal(0, result.Diagnostics.TriangulationLegacyFallbackCount);
        Assert.Equal(0, result.Diagnostics.TriangulationNativeFailureCount);
        Assert.NotEmpty(result.Diagnostics.StageInvariantCertificates);
        Assert.Contains(result.Diagnostics.StageInvariantCertificates, c => c.StartsWith("input:pass;", StringComparison.Ordinal));
        RobustDiagnosticsAssertions.AssertHasPatchExtractionCertificate(result.Diagnostics);
        Assert.Contains(result.Diagnostics.StageInvariantCertificates, c => c.StartsWith("classification:pass;", StringComparison.Ordinal));
        Assert.Contains(result.Diagnostics.StageInvariantCertificates, c => c.StartsWith("triangulation:pass;", StringComparison.Ordinal));
        Assert.Contains(result.Diagnostics.StageInvariantCertificates, c => c.StartsWith("reconstruction:pass;", StringComparison.Ordinal));
        Assert.Contains(result.Diagnostics.StageInvariantCertificates, c => c.StartsWith("output:pass;", StringComparison.Ordinal));
        Assert.Equal(result.Result!.DegenerateCount, result.Diagnostics.ClassificationFallbackCount);
        Assert.NotEmpty(result.Diagnostics.ReconstructionInvariantCertificates);
        Assert.Contains(result.Diagnostics.ReconstructionInvariantCertificates, c => c.StartsWith("reconstruction:pass;", StringComparison.Ordinal));
    }

    [Fact]
    public void Union_WithRobustKernelDisabled_HasNoTriangulationTelemetry()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(0.75, 0, 0), 2.0);
        var opts = new RobustOperationOptions
        {
            Mode = RobustMode.Strict,
            Deterministic = true,
            UseRobustTriangulationKernel = false
        };

        var result = RobustCsg.Union(a, b, opts);

        Assert.True(result.Succeeded);
        Assert.Equal(0, result.Diagnostics.TriangulationInvocationCount);
        Assert.Equal(0, result.Diagnostics.TriangulationNativeCount);
        Assert.Equal(0, result.Diagnostics.TriangulationLegacyFallbackCount);
        Assert.Equal(0, result.Diagnostics.TriangulationFallbackInvalidOrCrossingConstraintCount);
        Assert.Equal(0, result.Diagnostics.TriangulationFallbackPartitionFailureCount);
        Assert.Equal(0, result.Diagnostics.TriangulationFallbackConstrainedEarFailureCount);
        Assert.Equal(0, result.Diagnostics.TriangulationFallbackWorkBudgetExceededCount);
        Assert.Empty(result.Diagnostics.TriangulationFallbackSignatures);
        Assert.Equal(0, result.Diagnostics.TriangulationNativeFailureCount);
        Assert.Equal(0, result.Diagnostics.TriangulationNativeFailureInvalidOrCrossingConstraintCount);
        Assert.Equal(0, result.Diagnostics.TriangulationNativeFailurePartitionFailureCount);
        Assert.Equal(0, result.Diagnostics.TriangulationNativeFailureConstrainedEarFailureCount);
        Assert.Equal(0, result.Diagnostics.TriangulationNativeFailureWorkBudgetExceededCount);
        Assert.Empty(result.Diagnostics.TriangulationNativeFailureSignatures);
        Assert.Empty(result.Diagnostics.TriangulationNativeFailureCodes);
    }

    [Fact]
    public void StrictMode_RejectsOpenInputMesh()
    {
        var open = Solid.FromTriangles(
        [
            new Triangle3(
                new Vec3(0, 0, 0),
                new Vec3(1, 0, 0),
                new Vec3(0, 1, 0))
        ]);
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var opts = new RobustOperationOptions
        {
            Mode = RobustMode.Strict,
            ValidateInput = true,
            FailOnValidationError = true
        };

        var result = RobustCsg.Union(open, cube, opts);

        Assert.False(result.Succeeded);
        Assert.Null(result.Result);
        Assert.Contains(result.Issues, i => i.Code == RobustIssueCode.InputMeshNotClosed);
        Assert.Contains(result.Issues, i => i.Code == RobustIssueCode.StageInvariantViolation);
    }
}
