using System.Diagnostics;
using MdCsg.Api;
using MdCsg.Mesh;
using MdCsg.Robust.Kernel.Arrangement;
using MdCsg.Robust.Kernel.Predicates;
using MdCsg.Robust.Validation;

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
        var predicateTelemetry = new PredicateTelemetryCounter();
        ArrangementGraph? arrangement = null;
        ArrangementAnalysis arrangementAnalysis = default;
        var totalSw = Stopwatch.StartNew();

        if (opts.ValidateInput)
        {
            ValidateInput(a, "A", opts.Mode, issues, predicateTelemetry);
            ValidateInput(b, "B", opts.Mode, issues, predicateTelemetry);
        }

        if (opts.AnalyzeInputIntersection)
        {
            arrangement = ArrangementBuilder.Build(a.Mesh, b.Mesh);
            arrangementAnalysis = ArrangementAnalyzer.Analyze(arrangement);
            if (arrangement.HasCoplanarPairs)
            {
                var severity = opts.TreatCoplanarIntersectionAsError
                    ? RobustIssueSeverity.Error
                    : RobustIssueSeverity.Warning;
                AddIssue(
                    issues,
                    opts.Mode,
                    RobustIssueCode.InputIntersectionContainsCoplanarPairs,
                    $"Input intersection has coplanar face pairs (A={arrangement.CoplanarFaceCountA}, B={arrangement.CoplanarFaceCountB}).",
                    severity);
            }

            if (arrangement.HasOpposingCoplanarPairs)
            {
                var severity = opts.TreatOpposingCoplanarPairsAsError
                    ? RobustIssueSeverity.Error
                    : RobustIssueSeverity.Warning;
                AddIssue(
                    issues,
                    opts.Mode,
                    RobustIssueCode.InputIntersectionContainsOpposingCoplanarPairs,
                    $"Input intersection has opposing coplanar face pairs ({arrangement.CoplanarPairNormalsOpposeCount}).",
                    severity);
            }

            if (arrangementAnalysis.EndpointVertexCount > 0)
            {
                var severity = opts.TreatOpenArrangementAsError
                    ? RobustIssueSeverity.Error
                    : RobustIssueSeverity.Warning;
                AddIssue(
                    issues,
                    opts.Mode,
                    RobustIssueCode.InputArrangementHasOpenEndpoints,
                    $"Input arrangement has open endpoints ({arrangementAnalysis.EndpointVertexCount}).",
                    severity);
            }
        }

        if (opts.FailOnValidationError && issues.Any(i => i.Severity == RobustIssueSeverity.Error))
        {
            totalSw.Stop();
            return new RobustCsgResult(
                result: null,
                issues,
                BuildDiagnostics(totalSw.Elapsed, TimeSpan.Zero, predicateTelemetry, arrangement, arrangementAnalysis));
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
            ValidateOutput(result.Mesh, opts.Mode, issues, predicateTelemetry);
        }

        totalSw.Stop();

        var hasErrors = issues.Any(i => i.Severity == RobustIssueSeverity.Error);
        var finalResult = opts.FailOnValidationError && hasErrors ? null : result;

        return new RobustCsgResult(
            result: finalResult,
            issues,
            BuildDiagnostics(totalSw.Elapsed, opSw.Elapsed, predicateTelemetry, arrangement, arrangementAnalysis));
    }

    private static void ValidateInput(
        Solid solid,
        string label,
        RobustMode mode,
        List<RobustIssue> issues,
        PredicateTelemetryCounter predicateTelemetry)
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

        int degenerateFaces = DegenerateFaceInspector.CountDegenerateFaces(solid.Mesh, predicateTelemetry);
        if (degenerateFaces > 0)
        {
            AddIssue(
                issues,
                mode,
                RobustIssueCode.InputMeshHasDegenerateFaces,
                $"{label}: input mesh has degenerate faces ({degenerateFaces}).");
        }
    }

    private static void ValidateOutput(
        HalfEdgeMesh mesh,
        RobustMode mode,
        List<RobustIssue> issues,
        PredicateTelemetryCounter predicateTelemetry)
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

        int degenerateFaces = DegenerateFaceInspector.CountDegenerateFaces(mesh, predicateTelemetry);
        if (degenerateFaces > 0)
        {
            AddIssue(
                issues,
                mode,
                RobustIssueCode.OutputMeshHasDegenerateFaces,
                $"Output mesh has degenerate faces ({degenerateFaces}).");
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
        string message,
        RobustIssueSeverity? explicitSeverity = null)
    {
        var severity = explicitSeverity ?? (mode == RobustMode.Strict
            ? RobustIssueSeverity.Error
            : RobustIssueSeverity.Warning);
        issues.Add(new RobustIssue(severity, code, message));
    }

    private static RobustDiagnostics BuildDiagnostics(
        TimeSpan totalElapsed,
        TimeSpan operationElapsed,
        PredicateTelemetryCounter predicateTelemetry,
        ArrangementGraph? arrangement,
        ArrangementAnalysis arrangementAnalysis)
    {
        return new RobustDiagnostics
        {
            TotalElapsed = totalElapsed,
            OperationElapsed = operationElapsed,
            ArrangementVertexCount = arrangement?.Vertices.Count ?? 0,
            ArrangementEdgeCount = arrangement?.Edges.Count ?? 0,
            ArrangementCoplanarFaceCountA = arrangement?.CoplanarFaceCountA ?? 0,
            ArrangementCoplanarFaceCountB = arrangement?.CoplanarFaceCountB ?? 0,
            ArrangementCoplanarPairNormalsAgreeCount = arrangement?.CoplanarPairNormalsAgreeCount ?? 0,
            ArrangementCoplanarPairNormalsOpposeCount = arrangement?.CoplanarPairNormalsOpposeCount ?? 0,
            ArrangementEndpointVertexCount = arrangementAnalysis.EndpointVertexCount,
            ArrangementConnectedComponentCount = arrangementAnalysis.ConnectedComponentCount,
            PredicateEscalationCount = predicateTelemetry.EscalationCount,
            PredicateDoubleCount = predicateTelemetry.DoubleCount,
            PredicateExpansionCount = predicateTelemetry.ExpansionCount,
            PredicateExactCount = predicateTelemetry.ExactCount,
            ClassificationFallbackCount = 0
        };
    }
}
