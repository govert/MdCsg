using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Robust.Conformance;

public class RobustFuzzSmokeTests
{
    private static readonly RobustOperationOptions StrictRobustOptions = new()
    {
        Mode = RobustMode.Strict,
        Deterministic = true,
        TreatOpenArrangementAsError = true,
        UseRobustTriangulationKernel = true
    };

    [Fact]
    public void SeededBooleanFuzzSmoke_StrictMode_HasNoUnresolvedFailures()
    {
        var rng = new Random(20260228);
        var failures = new List<string>();

        const int caseCount = 64;
        for (int i = 0; i < caseCount; i++)
        {
            var a = CreateRandomSolid(rng);
            var b = CreateRandomSolid(rng);
            var op = (RobustCsgOperation)rng.Next(0, 3);

            var result = Execute(op, a, b);
            if (!TryValidateResult(result, out string? failure))
            {
                failures.Add($"case={i};op={op};{failure}");
                if (failures.Count >= 12)
                    break;
            }
        }

        Assert.True(
            failures.Count == 0,
            "Unresolved strict fuzz failures:\n" + string.Join('\n', failures));
    }

    [Fact]
    public void SeededNearTangentFuzzSmoke_StrictMode_HasNoUnresolvedFailures()
    {
        var rng = new Random(20260301);
        var failures = new List<string>();

        const int caseCount = 32;
        for (int i = 0; i < caseCount; i++)
        {
            double rA = 0.9 + 0.4 * rng.NextDouble();
            double rB = 0.9 + 0.4 * rng.NextDouble();
            double centerDistance = rA + rB + NextRange(rng, -0.02, 0.02);

            var a = Primitives.Sphere(Vec3.Zero, rA, 3);
            var b = Primitives.Sphere(new Vec3(centerDistance, NextRange(rng, -0.02, 0.02), 0), rB, 3);
            var op = (RobustCsgOperation)rng.Next(0, 3);

            var result = Execute(op, a, b);
            if (!TryValidateResult(result, out string? failure))
            {
                failures.Add($"near-tangent-case={i};op={op};{failure}");
                if (failures.Count >= 12)
                    break;
            }
        }

        Assert.True(
            failures.Count == 0,
            "Unresolved strict near-tangent failures:\n" + string.Join('\n', failures));
    }

    private static RobustCsgResult Execute(RobustCsgOperation operation, Solid a, Solid b)
        => operation switch
        {
            RobustCsgOperation.Union => RobustCsg.Union(a, b, StrictRobustOptions),
            RobustCsgOperation.Intersection => RobustCsg.Intersect(a, b, StrictRobustOptions),
            RobustCsgOperation.Difference => RobustCsg.Difference(a, b, StrictRobustOptions),
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
        };

    private static bool TryValidateResult(RobustCsgResult result, out string? failure)
    {
        if (!result.Succeeded || result.Result is null)
        {
            bool hasOutputTopologyErrors = result.Issues.Any(static i =>
                i.Code == RobustIssueCode.OutputMeshNotClosed
                || i.Code == RobustIssueCode.OutputMeshNotEdgeManifold
                || i.Code == RobustIssueCode.OutputMeshHasDegenerateFaces);
            if (!hasOutputTopologyErrors)
            {
                bool hasFailClosedCause = result.Issues.Any(static i =>
                    i.Code == RobustIssueCode.InputArrangementHasOpenEndpoints
                    || i.Code == RobustIssueCode.InputIntersectionContainsCoplanarPairs
                    || i.Code == RobustIssueCode.InputIntersectionContainsOpposingCoplanarPairs
                    || i.Code == RobustIssueCode.TriangulationNativeFailure
                    || i.Code == RobustIssueCode.TriangulationInvalidOrCrossingConstraints
                    || i.Code == RobustIssueCode.TriangulationPartitioningFailed
                    || i.Code == RobustIssueCode.TriangulationConstrainedEarFailed
                    || i.Code == RobustIssueCode.TriangulationWorkBudgetExceeded
                    || i.Code == RobustIssueCode.ReconstructionInvariantViolation
                    || i.Code == RobustIssueCode.ReconstructionPatchSelectionFailed
                    || i.Code == RobustIssueCode.ReconstructionStitchingFailed
                    || i.Code == RobustIssueCode.StageInvariantViolation);
                if (hasFailClosedCause)
                {
                    failure = null;
                    return true;
                }
            }

            failure = "operation failed: " + string.Join(
                " | ",
                result.Issues.Select(static i => $"{i.Severity}:{i.Code}"));
            return false;
        }

        if (result.Issues.Any(static i => i.Severity == RobustIssueSeverity.Error))
        {
            failure = "error issues reported: " + string.Join(
                " | ",
                result.Issues.Select(static i => $"{i.Severity}:{i.Code}"));
            return false;
        }

        if (result.Diagnostics.TriangulationLegacyFallbackCount > 0
            || result.Diagnostics.TriangulationNativeFailureCount > 0)
        {
            failure = $"triangulation degraded: legacy={result.Diagnostics.TriangulationLegacyFallbackCount};"
                + $"nativeFail={result.Diagnostics.TriangulationNativeFailureCount}";
            return false;
        }

        int boundary = MeshValidator.CountBoundaryEdges(result.Result.Mesh);
        if (boundary > 0)
        {
            failure = $"output boundary edges: {boundary}";
            return false;
        }

        if (!MeshValidator.IsEdgeManifold(result.Result.Mesh))
        {
            failure = "output not edge-manifold";
            return false;
        }

        if (!result.Diagnostics.StageInvariantCertificates.Any(static c => c.StartsWith("triangulation:pass;", StringComparison.Ordinal))
            || !result.Diagnostics.StageInvariantCertificates.Any(static c => c.StartsWith("output:pass;", StringComparison.Ordinal)))
        {
            failure = "stage certificates missing pass markers";
            return false;
        }

        failure = null;
        return true;
    }

    private static Solid CreateRandomSolid(Random rng)
    {
        int kind = rng.Next(0, 3);
        return kind switch
        {
            0 => Primitives.Cube(
                new Vec3(NextRange(rng, -0.9, 0.9), NextRange(rng, -0.9, 0.9), NextRange(rng, -0.9, 0.9)),
                NextRange(rng, 1.1, 2.0)),
            1 => Primitives.Sphere(
                new Vec3(NextRange(rng, -0.8, 0.8), NextRange(rng, -0.8, 0.8), NextRange(rng, -0.8, 0.8)),
                NextRange(rng, 0.7, 1.4),
                3),
            _ => CreateRandomCylinder(rng)
        };
    }

    private static Solid CreateRandomCylinder(Random rng)
    {
        var center = new Vec3(NextRange(rng, -0.7, 0.7), NextRange(rng, -0.7, 0.7), NextRange(rng, -0.7, 0.7));
        Vec3 axis = rng.Next(0, 3) switch
        {
            0 => Vec3.UnitX,
            1 => Vec3.UnitY,
            _ => Vec3.UnitZ
        };

        double length = NextRange(rng, 1.2, 2.6);
        var start = center - axis * (0.5 * length);
        return Primitives.Cylinder(start, axis, NextRange(rng, 0.35, 0.75), length, 3);
    }

    private static double NextRange(Random rng, double min, double max)
        => min + (max - min) * rng.NextDouble();
}
