using System.Diagnostics;
using System.Collections.Concurrent;
using MdCsg.Api;
using MdCsg.Cutting;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Operations;
using MdCsg.Predicates;
using MdCsg.Robust.Kernel.Arrangement;
using MdCsg.Robust.Kernel.Predicates;
using MdCsg.Robust.Kernel.Triangulation;
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
        int triangulationInvocationCount = 0;
        int triangulationNativeCount = 0;
        int triangulationLegacyFallbackCount = 0;
        int triangulationDroppedDegenerateCount = 0;
        int triangulationFallbackInvalidOrCrossingConstraintCount = 0;
        int triangulationFallbackPartitionFailureCount = 0;
        int triangulationFallbackConstrainedEarFailureCount = 0;
        var triangulationFallbackSignatureCounts = new ConcurrentDictionary<string, int>(StringComparer.Ordinal);
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
                BuildDiagnostics(
                    totalSw.Elapsed,
                    TimeSpan.Zero,
                    predicateTelemetry,
                    arrangement,
                    arrangementAnalysis,
                    triangulationInvocationCount,
                    triangulationNativeCount,
                    triangulationLegacyFallbackCount,
                    triangulationDroppedDegenerateCount,
                    triangulationFallbackInvalidOrCrossingConstraintCount,
                    triangulationFallbackPartitionFailureCount,
                    triangulationFallbackConstrainedEarFailureCount,
                    SummarizeFallbackSignatures(triangulationFallbackSignatureCounts)));
        }

        var robustTriangulator = new RobustConstrainedTriangulator();
        ConstrainedTriangulationKernel? triangulationKernel = null;
        if (opts.UseRobustTriangulationKernel)
        {
            triangulationKernel = (vertices3D, constraints, faceNormal) =>
            {
                System.Threading.Interlocked.Increment(ref triangulationInvocationCount);

                var triResult = robustTriangulator.Triangulate(
                    vertices3D,
                    constraints,
                    faceNormal,
                    new RobustTriangulationOptions
                    {
                        DeterministicOrdering = opts.Deterministic,
                        DropDegenerateTriangles = true,
                        DegenerateAreaTolerance = MathUtil.Epsilon
                    });

                if (triResult.UsedLegacyKernel)
                {
                    System.Threading.Interlocked.Increment(ref triangulationLegacyFallbackCount);
                    switch (triResult.LegacyFallbackReason)
                    {
                        case RobustTriangulationFallbackReason.InvalidOrCrossingConstraints:
                            System.Threading.Interlocked.Increment(ref triangulationFallbackInvalidOrCrossingConstraintCount);
                            break;
                        case RobustTriangulationFallbackReason.PartitioningFailed:
                            System.Threading.Interlocked.Increment(ref triangulationFallbackPartitionFailureCount);
                            break;
                        case RobustTriangulationFallbackReason.ConstrainedEarFailed:
                            System.Threading.Interlocked.Increment(ref triangulationFallbackConstrainedEarFailureCount);
                            break;
                    }

                    if (!string.IsNullOrWhiteSpace(triResult.LegacyFallbackSignature))
                    {
                        triangulationFallbackSignatureCounts.AddOrUpdate(
                            triResult.LegacyFallbackSignature!,
                            1,
                            static (_, current) => current + 1);
                    }
                }
                else
                    System.Threading.Interlocked.Increment(ref triangulationNativeCount);

                if (triResult.DroppedDegenerateTriangleCount > 0)
                {
                    System.Threading.Interlocked.Add(
                        ref triangulationDroppedDegenerateCount,
                        triResult.DroppedDegenerateTriangleCount);
                }

                return triResult.Triangles;
            };
        }

        var csgOptions = new CsgOptions
        {
            Parallel = !opts.Deterministic,
            TriangulationKernel = triangulationKernel
        };

        var opSw = Stopwatch.StartNew();
        var result = operation switch
        {
            RobustCsgOperation.Union => Csg.Union(a, b, csgOptions),
            RobustCsgOperation.Intersection => Csg.Intersect(a, b, csgOptions),
            RobustCsgOperation.Difference => Csg.Difference(a, b, csgOptions),
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
        };
        opSw.Stop();

        result = PruneDegenerateOutputFaces(result, csgOptions.WeldTolerance, predicateTelemetry);
        RepairOutputTopology(result.Mesh, csgOptions.WeldTolerance);
        result = PruneDegenerateOutputFaces(result, csgOptions.WeldTolerance, predicateTelemetry);

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
            BuildDiagnostics(
                totalSw.Elapsed,
                opSw.Elapsed,
                predicateTelemetry,
                arrangement,
                arrangementAnalysis,
                triangulationInvocationCount,
                triangulationNativeCount,
                triangulationLegacyFallbackCount,
                triangulationDroppedDegenerateCount,
                triangulationFallbackInvalidOrCrossingConstraintCount,
                triangulationFallbackPartitionFailureCount,
                triangulationFallbackConstrainedEarFailureCount,
                SummarizeFallbackSignatures(triangulationFallbackSignatureCounts)));
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

    private static CsgResult PruneDegenerateOutputFaces(
        CsgResult result,
        double weldTolerance,
        PredicateTelemetryCounter predicateTelemetry)
    {
        if (result.Mesh.Faces.Count == 0)
            return result;

        var kept = new List<Triangle3>(result.Mesh.Faces.Count);
        bool removedAny = false;

        foreach (var face in result.Mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            var areaSign = EvaluateProjectedAreaSign(a, b, c);
            predicateTelemetry.Add(areaSign.Tier);
            if (areaSign.Sign == PredicateSign.Zero)
            {
                removedAny = true;
                continue;
            }

            kept.Add(new Triangle3(a, b, c));
        }

        if (!removedAny)
            return result;

        var rebuilt = new MeshBuilder(weldTolerance).Build(kept);
        rebuilt.IsComplemented = result.Mesh.IsComplemented;

        return new CsgResult
        {
            Mesh = rebuilt,
            PatchCountA = result.PatchCountA,
            PatchCountB = result.PatchCountB,
            DegenerateCount = result.DegenerateCount,
            IntersectionSegmentCount = result.IntersectionSegmentCount
        };
    }

    private static CertifiedPredicateResult EvaluateProjectedAreaSign(Vec3 a, Vec3 b, Vec3 c)
    {
        Vec3 n = Vec3.Cross(b - a, c - a);
        double ax = System.Math.Abs(n.X);
        double ay = System.Math.Abs(n.Y);
        double az = System.Math.Abs(n.Z);

        Vec2 pa;
        Vec2 pb;
        Vec2 pc;

        if (ax >= ay && ax >= az)
        {
            pa = new Vec2(a.Y, a.Z);
            pb = new Vec2(b.Y, b.Z);
            pc = new Vec2(c.Y, c.Z);
        }
        else if (ay >= az)
        {
            pa = new Vec2(a.X, a.Z);
            pb = new Vec2(b.X, b.Z);
            pc = new Vec2(c.X, c.Z);
        }
        else
        {
            pa = new Vec2(a.X, a.Y);
            pb = new Vec2(b.X, b.Y);
            pc = new Vec2(c.X, c.Y);
        }

        return CertifiedPredicates.Orient2D(pa, pb, pc);
    }

    private static void RepairOutputTopology(HalfEdgeMesh mesh, double weldTolerance)
    {
        int boundary = MeshValidator.CountBoundaryEdges(mesh);
        bool manifold = MeshValidator.IsEdgeManifold(mesh);
        if (boundary == 0 && manifold)
            return;

        double repairTolerance = System.Math.Max(weldTolerance * 4.0, 1e-6);
        MeshStitcher.RepairBoundary(mesh, repairTolerance);

        if (MeshValidator.CountBoundaryEdges(mesh) > 0)
            MeshStitcher.CloseBoundaryLoops(mesh);
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
        ArrangementAnalysis arrangementAnalysis,
        int triangulationInvocationCount,
        int triangulationNativeCount,
        int triangulationLegacyFallbackCount,
        int triangulationDroppedDegenerateCount,
        int triangulationFallbackInvalidOrCrossingConstraintCount,
        int triangulationFallbackPartitionFailureCount,
        int triangulationFallbackConstrainedEarFailureCount,
        IReadOnlyList<string> triangulationFallbackSignatures)
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
            TriangulationInvocationCount = triangulationInvocationCount,
            TriangulationNativeCount = triangulationNativeCount,
            TriangulationLegacyFallbackCount = triangulationLegacyFallbackCount,
            TriangulationDroppedDegenerateCount = triangulationDroppedDegenerateCount,
            TriangulationFallbackInvalidOrCrossingConstraintCount = triangulationFallbackInvalidOrCrossingConstraintCount,
            TriangulationFallbackPartitionFailureCount = triangulationFallbackPartitionFailureCount,
            TriangulationFallbackConstrainedEarFailureCount = triangulationFallbackConstrainedEarFailureCount,
            TriangulationFallbackSignatures = triangulationFallbackSignatures,
            ClassificationFallbackCount = 0
        };
    }

    private static IReadOnlyList<string> SummarizeFallbackSignatures(
        IReadOnlyDictionary<string, int> signatureCounts,
        int maxItems = 8)
    {
        if (signatureCounts.Count == 0)
            return Array.Empty<string>();

        return signatureCounts
            .OrderByDescending(static kvp => kvp.Value)
            .ThenBy(static kvp => kvp.Key, StringComparer.Ordinal)
            .Take(maxItems)
            .Select(static kvp => $"{kvp.Value}x {kvp.Key}")
            .ToArray();
    }
}
