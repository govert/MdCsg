using System.Diagnostics;
using MdCsg.Api;
using MdCsg.Mesh;

namespace MdCsg.Robust;

/// <summary>
/// Transitional engine used while the dedicated robust kernel is being built.
/// It executes operations through the current CSG implementation and applies
/// strict validation/diagnostics so conformance tests can be authored now.
/// </summary>
public sealed class LegacyBridgedRobustCsgEngine : IRobustCsgEngine
{
    public RobustCsgResult Execute(
        Solid a,
        Solid b,
        RobustCsgOperation operation,
        RobustOperationOptions? options = null)
    {
        var opts = options ?? RobustOperationOptions.Default;
        var issues = new List<RobustIssue>();
        var totalSw = Stopwatch.StartNew();

        if (opts.ValidateInput)
        {
            ValidateInput(a, "A", opts.Mode, issues);
            ValidateInput(b, "B", opts.Mode, issues);
        }

        if (opts.FailOnValidationError && issues.Any(i => i.Severity == RobustIssueSeverity.Error))
        {
            totalSw.Stop();
            return new RobustCsgResult(
                result: null,
                issues,
                new RobustDiagnostics { TotalElapsed = totalSw.Elapsed });
        }

        var opSw = Stopwatch.StartNew();
        var result = operation switch
        {
            RobustCsgOperation.Union => Csg.Union(a, b),
            RobustCsgOperation.Intersection => Csg.Intersect(a, b),
            RobustCsgOperation.Difference => Csg.Difference(a, b),
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
        };
        opSw.Stop();

        if (opts.ValidateOutput)
        {
            ValidateOutput(result.Mesh, opts.Mode, issues);
        }

        totalSw.Stop();

        var hasErrors = issues.Any(i => i.Severity == RobustIssueSeverity.Error);
        var finalResult = opts.FailOnValidationError && hasErrors ? null : result;

        return new RobustCsgResult(
            result: finalResult,
            issues,
            new RobustDiagnostics
            {
                TotalElapsed = totalSw.Elapsed,
                OperationElapsed = opSw.Elapsed,
                PredicateEscalationCount = 0,
                ClassificationFallbackCount = 0
            });
    }

    private static void ValidateInput(
        Solid solid,
        string label,
        RobustMode mode,
        List<RobustIssue> issues)
    {
        if (!HasFiniteVertices(solid.Mesh))
        {
            AddIssue(
                issues,
                mode,
                RobustIssueCode.InputMeshContainsNonFiniteCoordinate,
                $"{label}: input mesh contains non-finite coordinates.");
        }

        int boundary = MeshValidator.CountBoundaryEdges(solid.Mesh);
        if (boundary > 0)
        {
            AddIssue(
                issues,
                mode,
                RobustIssueCode.InputMeshNotClosed,
                $"{label}: input mesh is not closed (boundary edges: {boundary}).");
        }

        if (!MeshValidator.IsEdgeManifold(solid.Mesh))
        {
            AddIssue(
                issues,
                mode,
                RobustIssueCode.InputMeshNotEdgeManifold,
                $"{label}: input mesh is not edge-manifold.");
        }
    }

    private static void ValidateOutput(
        HalfEdgeMesh mesh,
        RobustMode mode,
        List<RobustIssue> issues)
    {
        int boundary = MeshValidator.CountBoundaryEdges(mesh);
        if (boundary > 0)
        {
            AddIssue(
                issues,
                mode,
                RobustIssueCode.OutputMeshNotClosed,
                $"Output mesh is not closed (boundary edges: {boundary}).");
        }

        if (!MeshValidator.IsEdgeManifold(mesh))
        {
            AddIssue(
                issues,
                mode,
                RobustIssueCode.OutputMeshNotEdgeManifold,
                "Output mesh is not edge-manifold.");
        }
    }

    private static bool HasFiniteVertices(HalfEdgeMesh mesh)
    {
        foreach (var vertex in mesh.Vertices)
        {
            var p = vertex.Position;
            if (!IsFinite(p.X) || !IsFinite(p.Y) || !IsFinite(p.Z))
                return false;
        }
        return true;
    }

    private static bool IsFinite(double value)
        => !double.IsNaN(value) && !double.IsInfinity(value);

    private static void AddIssue(
        List<RobustIssue> issues,
        RobustMode mode,
        RobustIssueCode code,
        string message)
    {
        var severity = mode == RobustMode.Strict
            ? RobustIssueSeverity.Error
            : RobustIssueSeverity.Warning;
        issues.Add(new RobustIssue(severity, code, message));
    }
}
