using System.Diagnostics;
using System.Collections.Concurrent;
using System.Linq;
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
        int triangulationFallbackWorkBudgetExceededCount = 0;
        int triangulationNativeFailureCount = 0;
        int triangulationNativeFailureInvalidOrCrossingConstraintCount = 0;
        int triangulationNativeFailurePartitionFailureCount = 0;
        int triangulationNativeFailureConstrainedEarFailureCount = 0;
        int triangulationNativeFailureWorkBudgetExceededCount = 0;
        var triangulationFallbackSignatureCounts = new ConcurrentDictionary<string, int>(StringComparer.Ordinal);
        var triangulationNativeFailureSignatureCounts = new ConcurrentDictionary<string, int>(StringComparer.Ordinal);
        var triangulationNativeFailureCodeCounts = new ConcurrentDictionary<string, int>(StringComparer.Ordinal);
        int reconstructionBoundaryHalfEdgeCount = 0;
        int reconstructionOpenBoundaryLoopCount = 0;
        int reconstructionUnmatchedUndirectedEdgeCount = 0;
        int reconstructionNonManifoldUndirectedEdgeCount = 0;
        int reconstructionDroppedComponentCount = 0;
        int reconstructionArrangementSnapCount = 0;
        int reconstructionArrangementEdgeSnapCount = 0;
        int reconstructionComponentCount = 0;
        int reconstructionInvalidComponentCount = 0;
        int classificationFallbackCount = 0;
        var reconstructionCertificates = new List<string>();
        var stageCertificates = new List<string>();
        MeshInvariantSnapshot inputAInvariant = default;
        MeshInvariantSnapshot inputBInvariant = default;
        MeshInvariantSnapshot outputInvariant = default;
        var totalSw = Stopwatch.StartNew();

        if (opts.ValidateInput)
        {
            inputAInvariant = ValidateInput(a, "A", opts.Mode, issues, predicateTelemetry);
            inputBInvariant = ValidateInput(b, "B", opts.Mode, issues, predicateTelemetry);

            bool inputStagePass = inputAInvariant.IsValid && inputBInvariant.IsValid;
            stageCertificates.Add(
                $"input:{(inputStagePass ? "pass" : "fail")};"
                + $"A[finite={(inputAInvariant.HasFiniteVertices ? 1 : 0)},boundary={inputAInvariant.BoundaryEdgeCount},manifold={(inputAInvariant.IsEdgeManifold ? 1 : 0)},deg={inputAInvariant.DegenerateFaceCount}];"
                + $"B[finite={(inputBInvariant.HasFiniteVertices ? 1 : 0)},boundary={inputBInvariant.BoundaryEdgeCount},manifold={(inputBInvariant.IsEdgeManifold ? 1 : 0)},deg={inputBInvariant.DegenerateFaceCount}]");

            if (opts.Mode == RobustMode.Strict && !inputStagePass)
            {
                AddIssue(
                    issues,
                    opts.Mode,
                    RobustIssueCode.StageInvariantViolation,
                    "Input stage invariant gate failed (non-finite/open/non-manifold/degenerate input).",
                    RobustIssueSeverity.Error);
            }
        }
        else
        {
            stageCertificates.Add("input:skipped");
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

            bool arrangementTopologyClosed = arrangementAnalysis.EndpointVertexCount == 0;
            bool arrangementStagePass = arrangementTopologyClosed || !opts.TreatOpenArrangementAsError;
            stageCertificates.Add(
                $"arrangement:{(arrangementStagePass ? "pass" : "fail")};"
                + $"vertices={arrangement.Vertices.Count};edges={arrangement.Edges.Count};"
                + $"endpoints={arrangementAnalysis.EndpointVertexCount};components={arrangementAnalysis.ConnectedComponentCount};"
                + $"coplanarOppose={arrangement.CoplanarPairNormalsOpposeCount};"
                + $"gateOpenEndpoints={(opts.TreatOpenArrangementAsError ? 1 : 0)}");

            if (opts.Mode == RobustMode.Strict && !arrangementStagePass)
            {
                AddIssue(
                    issues,
                    opts.Mode,
                    RobustIssueCode.StageInvariantViolation,
                    $"Arrangement stage invariant gate failed (open endpoints: {arrangementAnalysis.EndpointVertexCount}).",
                    RobustIssueSeverity.Error);
            }
        }
        else
        {
            stageCertificates.Add("arrangement:skipped");
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
                    triangulationFallbackWorkBudgetExceededCount,
                    SummarizeFallbackSignatures(triangulationFallbackSignatureCounts),
                    triangulationNativeFailureCount,
                    triangulationNativeFailureInvalidOrCrossingConstraintCount,
                    triangulationNativeFailurePartitionFailureCount,
                    triangulationNativeFailureConstrainedEarFailureCount,
                    triangulationNativeFailureWorkBudgetExceededCount,
                    SummarizeFallbackSignatures(triangulationNativeFailureSignatureCounts),
                    SummarizeFallbackSignatures(triangulationNativeFailureCodeCounts),
                    classificationFallbackCount,
                    reconstructionBoundaryHalfEdgeCount,
                    reconstructionOpenBoundaryLoopCount,
                    reconstructionUnmatchedUndirectedEdgeCount,
                    reconstructionNonManifoldUndirectedEdgeCount,
                    reconstructionDroppedComponentCount,
                    reconstructionArrangementSnapCount,
                    reconstructionArrangementEdgeSnapCount,
                    reconstructionComponentCount,
                    reconstructionInvalidComponentCount,
                    reconstructionCertificates.ToArray(),
                    stageCertificates.ToArray()));
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
                        AllowLegacyFallback = opts.Mode != RobustMode.Strict,
                        DeterministicOrdering = opts.Deterministic,
                        DropDegenerateTriangles = true,
                        DegenerateAreaTolerance = MathUtil.Epsilon
                    });

                if (!triResult.Succeeded)
                {
                    System.Threading.Interlocked.Increment(ref triangulationNativeFailureCount);
                    switch (triResult.FailureReason)
                    {
                        case RobustTriangulationFallbackReason.InvalidOrCrossingConstraints:
                            System.Threading.Interlocked.Increment(ref triangulationNativeFailureInvalidOrCrossingConstraintCount);
                            break;
                        case RobustTriangulationFallbackReason.PartitioningFailed:
                            System.Threading.Interlocked.Increment(ref triangulationNativeFailurePartitionFailureCount);
                            break;
                        case RobustTriangulationFallbackReason.ConstrainedEarFailed:
                            System.Threading.Interlocked.Increment(ref triangulationNativeFailureConstrainedEarFailureCount);
                            break;
                        case RobustTriangulationFallbackReason.WorkBudgetExceeded:
                            System.Threading.Interlocked.Increment(ref triangulationNativeFailureWorkBudgetExceededCount);
                            break;
                    }

                    if (!string.IsNullOrWhiteSpace(triResult.FailureSignature))
                    {
                        triangulationNativeFailureSignatureCounts.AddOrUpdate(
                            triResult.FailureSignature!,
                            1,
                            static (_, current) => current + 1);
                    }

                    if (!string.IsNullOrWhiteSpace(triResult.FailureCode))
                    {
                        triangulationNativeFailureCodeCounts.AddOrUpdate(
                            triResult.FailureCode!,
                            1,
                            static (_, current) => current + 1);
                    }

                    // Fail closed in strict robust mode by withholding triangles from failed faces.
                    return Array.Empty<(int A, int B, int C)>();
                }

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
                        case RobustTriangulationFallbackReason.WorkBudgetExceeded:
                            System.Threading.Interlocked.Increment(ref triangulationFallbackWorkBudgetExceededCount);
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
            TriangulationKernel = triangulationKernel,
            PatchExtractionMode = PatchExtractionMode.Auto,
            PreferTopologyPreservingPatchExtraction = opts.Mode == RobustMode.Strict
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

        classificationFallbackCount = result.DegenerateCount;
        int classifiedPatchCount = result.PatchCountA + result.PatchCountB;
        int classifiedCertifiedCount = System.Math.Max(0, classifiedPatchCount - classificationFallbackCount);

        if (result.SelectedPatchExtractionMode.HasValue)
        {
            stageCertificates.Add(
                $"patch-extraction:mode={result.SelectedPatchExtractionMode.Value};"
                + $"boundary={result.SelectedPatchExtractionBoundaryEdgeCount.GetValueOrDefault(-1)};"
                + $"manifold={(result.SelectedPatchExtractionIsEdgeManifold == true ? 1 : 0)};"
                + $"components={result.SelectedPatchExtractionConnectedComponentCount.GetValueOrDefault(-1)};"
                + $"authority={result.SelectedPatchBoundaryAuthority?.ToString() ?? "Unknown"}");
        }
        if (result.PatchExtractionCandidateSignatures.Count > 0)
        {
            stageCertificates.Add(
                "patch-extraction-candidates:"
                + string.Join("|", result.PatchExtractionCandidateSignatures));
        }
        stageCertificates.Add(
            $"classification:pass;certified={classifiedCertifiedCount};"
            + $"fallback={classificationFallbackCount};policy=margin>errorBound");

        var policySnapshot = EvaluateReconstructionPolicySnapshot(operation, result);
        stageCertificates.Add(
            $"reconstruction-policy:op={operation};"
            + $"fromA={policySnapshot.FromA};"
            + $"fromB={policySnapshot.FromB};"
            + $"flipB={policySnapshot.FlippedFromB};"
            + $"pass={(policySnapshot.IsValid ? 1 : 0)};"
            + $"rule={policySnapshot.Rule}");
        if (opts.Mode == RobustMode.Strict && !policySnapshot.IsValid)
        {
            AddIssue(
                issues,
                opts.Mode,
                RobustIssueCode.StageInvariantViolation,
                $"Reconstruction policy invariant failed for {operation}: fromA={policySnapshot.FromA}, fromB={policySnapshot.FromB}, flipB={policySnapshot.FlippedFromB}.",
                RobustIssueSeverity.Error);
        }

        result = PruneDegenerateOutputFaces(result, csgOptions.WeldTolerance, predicateTelemetry);
        result = ReconstructOutputTopology(
            result,
            csgOptions.WeldTolerance,
            arrangement,
            out reconstructionDroppedComponentCount,
            out reconstructionArrangementSnapCount,
            out reconstructionArrangementEdgeSnapCount);
        result = PruneDegenerateOutputFaces(result, csgOptions.WeldTolerance, predicateTelemetry);
        var reconstructionInvariant = AnalyzeReconstructionTopology(result.Mesh);
        reconstructionBoundaryHalfEdgeCount = reconstructionInvariant.BoundaryHalfEdgeCount;
        reconstructionOpenBoundaryLoopCount = reconstructionInvariant.OpenBoundaryLoopCount;
        reconstructionUnmatchedUndirectedEdgeCount = reconstructionInvariant.UnmatchedUndirectedEdgeCount;
        reconstructionNonManifoldUndirectedEdgeCount = reconstructionInvariant.NonManifoldUndirectedEdgeCount;
        var componentInvariant = AnalyzeComponentTopology(result.Mesh);
        reconstructionComponentCount = componentInvariant.ComponentCount;
        reconstructionInvalidComponentCount = componentInvariant.InvalidComponentCount;

        bool triangulationAccountingPass =
            triangulationInvocationCount
            == triangulationNativeCount
                + triangulationLegacyFallbackCount
                + triangulationNativeFailureCount;
        bool triangulationNoLegacyInStrictPass =
            opts.Mode != RobustMode.Strict
            || triangulationLegacyFallbackCount == 0;
        bool triangulationNoNativeFailurePass = triangulationNativeFailureCount == 0;
        bool triangulationStagePass =
            triangulationAccountingPass
            && triangulationNoLegacyInStrictPass
            && triangulationNoNativeFailurePass;

        stageCertificates.Add(
            $"triangulation:{(triangulationStagePass ? "pass" : "fail")};"
            + $"invocations={triangulationInvocationCount};native={triangulationNativeCount};"
            + $"legacy={triangulationLegacyFallbackCount};nativeFail={triangulationNativeFailureCount};"
            + $"accounting={(triangulationAccountingPass ? 1 : 0)};strictNoLegacy={(triangulationNoLegacyInStrictPass ? 1 : 0)}");

        if (opts.Mode == RobustMode.Strict && !triangulationStagePass)
        {
            AddIssue(
                issues,
                opts.Mode,
                RobustIssueCode.StageInvariantViolation,
                "Triangulation stage invariant gate failed (native/legacy accounting or fail-closed policy violation).",
                RobustIssueSeverity.Error);
        }

        if (triangulationNativeFailureInvalidOrCrossingConstraintCount > 0)
        {
            AddIssue(
                issues,
                opts.Mode,
                RobustIssueCode.TriangulationInvalidOrCrossingConstraints,
                $"Native constrained triangulation rejected invalid/crossing constraints ({triangulationNativeFailureInvalidOrCrossingConstraintCount}).",
                RobustIssueSeverity.Error);
        }

        if (triangulationNativeFailurePartitionFailureCount > 0)
        {
            AddIssue(
                issues,
                opts.Mode,
                RobustIssueCode.TriangulationPartitioningFailed,
                $"Native constrained triangulation partitioning failed ({triangulationNativeFailurePartitionFailureCount}).",
                RobustIssueSeverity.Error);
        }

        if (triangulationNativeFailureConstrainedEarFailureCount > 0)
        {
            AddIssue(
                issues,
                opts.Mode,
                RobustIssueCode.TriangulationConstrainedEarFailed,
                $"Native constrained triangulation constrained-ear solving failed ({triangulationNativeFailureConstrainedEarFailureCount}).",
                RobustIssueSeverity.Error);
        }

        if (triangulationNativeFailureWorkBudgetExceededCount > 0)
        {
            AddIssue(
                issues,
                opts.Mode,
                RobustIssueCode.TriangulationWorkBudgetExceeded,
                $"Native constrained triangulation work budget was exceeded ({triangulationNativeFailureWorkBudgetExceededCount}).",
                RobustIssueSeverity.Error);
        }

        if (triangulationNativeFailureCount > 0)
        {
            AddIssue(
                issues,
                opts.Mode,
                RobustIssueCode.TriangulationNativeFailure,
                $"Native constrained triangulation failed on {triangulationNativeFailureCount} face(s); strict mode is fail-closed for these faces.",
                RobustIssueSeverity.Error);
        }

        if (triangulationFallbackWorkBudgetExceededCount > 0)
        {
            AddIssue(
                issues,
                opts.Mode,
                RobustIssueCode.TriangulationWorkBudgetExceeded,
                $"Triangulation work budget was exceeded ({triangulationFallbackWorkBudgetExceededCount}); fail-closed behavior is required for strict mode.",
                RobustIssueSeverity.Error);
        }

        bool reconstructionStagePass = reconstructionInvariant.IsValid;
        string reconstructionCertificate =
            $"reconstruction:{(reconstructionStagePass ? "pass" : "fail")};"
            + $"boundary={reconstructionInvariant.BoundaryHalfEdgeCount};"
            + $"openLoops={reconstructionInvariant.OpenBoundaryLoopCount};"
            + $"unmatched={reconstructionInvariant.UnmatchedUndirectedEdgeCount};"
            + $"nonManifold={reconstructionInvariant.NonManifoldUndirectedEdgeCount};"
            + $"oriented={(reconstructionInvariant.IsConsistentlyOriented ? 1 : 0)};"
            + $"dropped={reconstructionDroppedComponentCount};"
            + $"arrSnap={reconstructionArrangementSnapCount};"
            + $"arrEdgeSnap={reconstructionArrangementEdgeSnapCount};"
            + $"components={reconstructionComponentCount};"
            + $"invalidComponents={reconstructionInvalidComponentCount}";
        reconstructionCertificates.Add(reconstructionCertificate);
        stageCertificates.Add(reconstructionCertificate);

        if (opts.Mode == RobustMode.Strict && !reconstructionStagePass)
        {
            AddIssue(
                issues,
                opts.Mode,
                RobustIssueCode.StageInvariantViolation,
                "Reconstruction stage invariant gate failed (open/non-manifold/unoriented output).",
                RobustIssueSeverity.Error);
            AddIssue(
                issues,
                opts.Mode,
                RobustIssueCode.ReconstructionInvariantViolation,
                $"Reconstruction invariants failed: boundary={reconstructionInvariant.BoundaryHalfEdgeCount}, "
                + $"openLoops={reconstructionInvariant.OpenBoundaryLoopCount}, "
                + $"unmatched={reconstructionInvariant.UnmatchedUndirectedEdgeCount}, "
                + $"nonManifold={reconstructionInvariant.NonManifoldUndirectedEdgeCount}, "
                + $"oriented={(reconstructionInvariant.IsConsistentlyOriented ? 1 : 0)}.",
                RobustIssueSeverity.Error);
        }

        if (reconstructionInvariant.UnmatchedUndirectedEdgeCount > 0)
        {
            AddIssue(
                issues,
                opts.Mode,
                RobustIssueCode.ReconstructionPatchSelectionFailed,
                $"Reconstruction produced unmatched edges ({reconstructionInvariant.UnmatchedUndirectedEdgeCount}).",
                RobustIssueSeverity.Error);
        }

        if (reconstructionInvariant.BoundaryHalfEdgeCount > 0
            || reconstructionInvariant.OpenBoundaryLoopCount > 0)
        {
            AddIssue(
                issues,
                opts.Mode,
                RobustIssueCode.ReconstructionStitchingFailed,
                $"Reconstruction stitching left open boundary topology (boundary={reconstructionInvariant.BoundaryHalfEdgeCount}, openLoops={reconstructionInvariant.OpenBoundaryLoopCount}).",
                RobustIssueSeverity.Error);
        }

        if (reconstructionInvalidComponentCount > 0)
        {
            AddIssue(
                issues,
                opts.Mode,
                RobustIssueCode.ReconstructionPatchSelectionFailed,
                $"Reconstruction contains invalid components ({reconstructionInvalidComponentCount}/{reconstructionComponentCount}).",
                RobustIssueSeverity.Error);
        }

        if (opts.ValidateOutput)
        {
            outputInvariant = ValidateOutput(result.Mesh, opts.Mode, issues, predicateTelemetry);
            bool outputStagePass = outputInvariant.IsValid;
            stageCertificates.Add(
                $"output:{(outputStagePass ? "pass" : "fail")};"
                + $"finite={(outputInvariant.HasFiniteVertices ? 1 : 0)};"
                + $"boundary={outputInvariant.BoundaryEdgeCount};"
                + $"manifold={(outputInvariant.IsEdgeManifold ? 1 : 0)};"
                + $"deg={outputInvariant.DegenerateFaceCount}");

            if (opts.Mode == RobustMode.Strict && !outputStagePass)
            {
                AddIssue(
                    issues,
                    opts.Mode,
                    RobustIssueCode.StageInvariantViolation,
                    "Output stage invariant gate failed (non-finite/open/non-manifold/degenerate output).",
                    RobustIssueSeverity.Error);
            }
        }
        else
        {
            stageCertificates.Add("output:skipped");
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
                triangulationFallbackWorkBudgetExceededCount,
                SummarizeFallbackSignatures(triangulationFallbackSignatureCounts),
                triangulationNativeFailureCount,
                triangulationNativeFailureInvalidOrCrossingConstraintCount,
                triangulationNativeFailurePartitionFailureCount,
                triangulationNativeFailureConstrainedEarFailureCount,
                triangulationNativeFailureWorkBudgetExceededCount,
                SummarizeFallbackSignatures(triangulationNativeFailureSignatureCounts),
                SummarizeFallbackSignatures(triangulationNativeFailureCodeCounts),
                classificationFallbackCount,
                reconstructionBoundaryHalfEdgeCount,
                reconstructionOpenBoundaryLoopCount,
                reconstructionUnmatchedUndirectedEdgeCount,
                reconstructionNonManifoldUndirectedEdgeCount,
                reconstructionDroppedComponentCount,
                reconstructionArrangementSnapCount,
                reconstructionArrangementEdgeSnapCount,
                reconstructionComponentCount,
                reconstructionInvalidComponentCount,
                reconstructionCertificates.ToArray(),
                stageCertificates.ToArray()));
    }

    private static MeshInvariantSnapshot ValidateInput(
        Solid solid,
        string label,
        RobustMode mode,
        List<RobustIssue> issues,
        PredicateTelemetryCounter predicateTelemetry)
    {
        bool hasFinite = HasFiniteVertices(solid.Mesh);
        if (!hasFinite)
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

        bool isEdgeManifold = MeshValidator.IsEdgeManifold(solid.Mesh);
        if (!isEdgeManifold)
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

        return new MeshInvariantSnapshot(
            hasFinite,
            boundary,
            isEdgeManifold,
            degenerateFaces);
    }

    private static MeshInvariantSnapshot ValidateOutput(
        HalfEdgeMesh mesh,
        RobustMode mode,
        List<RobustIssue> issues,
        PredicateTelemetryCounter predicateTelemetry)
    {
        bool hasFinite = HasFiniteVertices(mesh);
        int boundary = MeshValidator.CountBoundaryEdges(mesh);
        if (boundary > 0)
        {
            AddIssue(
                issues,
                mode,
                RobustIssueCode.OutputMeshNotClosed,
                $"Output mesh is not closed (boundary edges: {boundary}).");
        }

        bool isEdgeManifold = MeshValidator.IsEdgeManifold(mesh);
        if (!isEdgeManifold)
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

        return new MeshInvariantSnapshot(
            hasFinite,
            boundary,
            isEdgeManifold,
            degenerateFaces);
    }

    private readonly record struct MeshInvariantSnapshot(
        bool HasFiniteVertices,
        int BoundaryEdgeCount,
        bool IsEdgeManifold,
        int DegenerateFaceCount)
    {
        public bool IsValid =>
            HasFiniteVertices
            && BoundaryEdgeCount == 0
            && IsEdgeManifold
            && DegenerateFaceCount == 0;
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
            IntersectionSegmentCount = result.IntersectionSegmentCount,
            SelectedPatchExtractionMode = result.SelectedPatchExtractionMode,
            SelectedPatchExtractionBoundaryEdgeCount = result.SelectedPatchExtractionBoundaryEdgeCount,
            SelectedPatchExtractionIsEdgeManifold = result.SelectedPatchExtractionIsEdgeManifold,
            SelectedPatchExtractionConnectedComponentCount = result.SelectedPatchExtractionConnectedComponentCount,
            SelectedPatchBoundaryAuthority = result.SelectedPatchBoundaryAuthority,
            SelectedAssemblyTrianglesFromA = result.SelectedAssemblyTrianglesFromA,
            SelectedAssemblyTrianglesFromB = result.SelectedAssemblyTrianglesFromB,
            SelectedAssemblyFlippedTrianglesFromB = result.SelectedAssemblyFlippedTrianglesFromB,
            PatchExtractionCandidateSignatures = result.PatchExtractionCandidateSignatures
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

    private static CsgResult ReconstructOutputTopology(
        CsgResult result,
        double weldTolerance,
        ArrangementGraph? arrangement,
        out int droppedComponentCount,
        out int arrangementSnapCount,
        out int arrangementEdgeSnapCount)
    {
        droppedComponentCount = 0;
        arrangementSnapCount = 0;
        arrangementEdgeSnapCount = 0;
        var mesh = result.Mesh;
        if (mesh.Faces.Count == 0)
            return result;

        var bounds = mesh.GetBounds();
        double sceneScale = System.Math.Max(1.0, bounds.Size.Length);
        double baseTolerance = System.Math.Max(
            System.Math.Max(weldTolerance * 4.0, sceneScale * 1e-8),
            1e-6);

        for (int pass = 0; pass < 4; pass++)
        {
            if (IsMeshClosedOrientedAndManifold(mesh))
                return result;

            double tol = baseTolerance * (1 << pass);
            if (arrangement != null && arrangement.Vertices.Count > 0)
            {
                arrangementSnapCount += SnapBoundaryVerticesToArrangement(mesh, arrangement, tol);
                arrangementEdgeSnapCount += SnapBoundaryVerticesToArrangementEdges(mesh, arrangement, tol);
            }
            MeshStitcher.RepairBoundary(mesh, tol);
            MeshStitcher.RelinkBoundaryTwinsDeterministic(mesh);

            var incidence = MeshStitcher.AnalyzeBoundaryIncidence(mesh);
            if (incidence.BoundaryHalfEdgeCount > 0 && incidence.OpenBoundaryVertexCount == 0)
                MeshStitcher.CloseBoundaryLoops(mesh);

            // A second relink pass after loop fill catches new near-equal endpoints.
            MeshStitcher.RepairBoundary(mesh, tol * 2.0);
            MeshStitcher.RelinkBoundaryTwinsDeterministic(mesh);

            if (TryPruneInvalidComponents(mesh, weldTolerance, out var pruned, out int dropped))
            {
                droppedComponentCount += dropped;
                mesh = pruned;
                result = WithMesh(result, mesh);
            }
        }

        return result;
    }

    private static int SnapBoundaryVerticesToArrangement(
        HalfEdgeMesh mesh,
        ArrangementGraph arrangement,
        double tolerance)
    {
        if (tolerance <= 0 || arrangement.Vertices.Count == 0)
            return 0;

        var boundaryVertices = CollectBoundaryVertices(mesh);
        if (boundaryVertices.Count == 0)
            return 0;

        var arrangementPositions = arrangement.Vertices
            .OrderBy(static v => v.Id)
            .Select(static v => v.Position)
            .ToArray();
        if (arrangementPositions.Length == 0)
            return 0;

        double tolSq = tolerance * tolerance;
        int snapped = 0;

        foreach (var vertex in boundaryVertices)
        {
            int bestIndex = -1;
            double bestDistSq = tolSq;

            for (int i = 0; i < arrangementPositions.Length; i++)
            {
                double distSq = Vec3.DistanceSquared(vertex.Position, arrangementPositions[i]);
                if (distSq > bestDistSq)
                    continue;

                if (distSq < bestDistSq || (distSq == bestDistSq && (bestIndex < 0 || i < bestIndex)))
                {
                    bestDistSq = distSq;
                    bestIndex = i;
                }
            }

            if (bestIndex < 0 || bestDistSq == 0)
                continue;

            vertex.Position = arrangementPositions[bestIndex];
            snapped++;
        }

        return snapped;
    }

    private static int SnapBoundaryVerticesToArrangementEdges(
        HalfEdgeMesh mesh,
        ArrangementGraph arrangement,
        double tolerance)
    {
        if (tolerance <= 0 || arrangement.Edges.Count == 0 || arrangement.Vertices.Count == 0)
            return 0;

        var boundaryVertices = CollectBoundaryVertices(mesh);
        if (boundaryVertices.Count == 0)
            return 0;

        var vertexPositions = arrangement.Vertices
            .OrderBy(static v => v.Id)
            .ToDictionary(static v => v.Id, static v => v.Position);
        var edgeSegments = arrangement.Edges
            .Where(static e => !e.IsDegenerate)
            .OrderBy(static e => e.Id)
            .Select(e =>
            {
                if (!vertexPositions.TryGetValue(e.StartVertexId, out var start)
                    || !vertexPositions.TryGetValue(e.EndVertexId, out var end))
                    return ((Vec3 Start, Vec3 End)?)null;
                return (start, end);
            })
            .Where(static s => s.HasValue)
            .Select(static s => s!.Value)
            .ToArray();

        if (edgeSegments.Length == 0)
            return 0;

        double tolSq = tolerance * tolerance;
        int snapped = 0;

        foreach (var vertex in boundaryVertices)
        {
            var original = vertex.Position;
            Vec3 bestPoint = original;
            double bestDistSq = tolSq;

            for (int i = 0; i < edgeSegments.Length; i++)
            {
                var segment = edgeSegments[i];
                if (!TryProjectPointToSegment(
                    original,
                    segment.Start,
                    segment.End,
                    out var projected))
                {
                    continue;
                }

                double distSq = Vec3.DistanceSquared(original, projected);
                if (distSq > bestDistSq || distSq == 0)
                    continue;

                bestDistSq = distSq;
                bestPoint = projected;
            }

            if (bestDistSq < tolSq)
            {
                vertex.Position = bestPoint;
                snapped++;
            }
        }

        return snapped;
    }

    private static List<Vertex> CollectBoundaryVertices(HalfEdgeMesh mesh)
    {
        var boundaryVertices = new Dictionary<int, Vertex>();
        foreach (var he in mesh.HalfEdges)
        {
            if (he.Twin != null)
                continue;

            boundaryVertices[he.Origin.Id] = he.Origin;
            boundaryVertices[he.Target.Id] = he.Target;
        }

        if (boundaryVertices.Count == 0)
            return [];

        return boundaryVertices.Values
            .OrderBy(static v => v.Id)
            .ToList();
    }

    private static bool TryProjectPointToSegment(
        Vec3 point,
        Vec3 segStart,
        Vec3 segEnd,
        out Vec3 projected)
    {
        var dir = segEnd - segStart;
        double lenSq = dir.LengthSquared;
        if (lenSq <= 1e-24)
        {
            projected = segStart;
            return false;
        }

        double t = Vec3.Dot(point - segStart, dir) / lenSq;
        t = System.Math.Max(0.0, System.Math.Min(1.0, t));
        projected = segStart + dir * t;
        return true;
    }

    private static bool IsMeshClosedOrientedAndManifold(HalfEdgeMesh mesh)
    {
        if (MeshValidator.CountBoundaryEdges(mesh) != 0)
            return false;
        if (!MeshValidator.IsEdgeManifold(mesh))
            return false;
        return MeshValidator.IsConsistentlyOriented(mesh);
    }

    private static bool TryPruneInvalidComponents(
        HalfEdgeMesh mesh,
        double weldTolerance,
        out HalfEdgeMesh pruned,
        out int droppedComponentCount)
    {
        pruned = mesh;
        droppedComponentCount = 0;

        var components = MeshQueries.ConnectedComponents(mesh);
        if (components.Count <= 1)
            return false;

        var keep = new bool[components.Count];
        int keepCount = 0;
        int largestFaces = 0;
        int largestIdx = -1;
        for (int ci = 0; ci < components.Count; ci++)
        {
            var comp = components[ci];
            int faceCount = comp.Count;
            var edgeStats = GetComponentEdgeStats(mesh, comp);
            bool valid = faceCount >= 4
                && edgeStats.BoundaryEdgeCount == 0
                && edgeStats.NonManifoldEdgeCount == 0;
            keep[ci] = valid;
            if (valid)
            {
                keepCount++;
                if (faceCount > largestFaces)
                {
                    largestFaces = faceCount;
                    largestIdx = ci;
                }
            }
        }

        if (keepCount == 0)
        {
            for (int ci = 0; ci < components.Count; ci++)
            {
                int faceCount = components[ci].Count;
                if (faceCount > largestFaces)
                {
                    largestFaces = faceCount;
                    largestIdx = ci;
                }
            }

            if (largestIdx >= 0)
            {
                keep[largestIdx] = true;
                keepCount = 1;
            }
        }

        if (keepCount == components.Count || keepCount == 0)
            return false;

        var positions = new List<Vec3>(mesh.Vertices.Count);
        foreach (var v in mesh.Vertices)
            positions.Add(v.Position);

        var triangles = new List<(int I0, int I1, int I2)>();
        for (int ci = 0; ci < components.Count; ci++)
        {
            if (!keep[ci])
                continue;

            foreach (int faceIdx in components[ci])
            {
                var verts = mesh.Faces[faceIdx].GetVertices();
                triangles.Add((verts[0].Id, verts[1].Id, verts[2].Id));
            }
        }

        var rebuilt = new MeshBuilder(weldTolerance).Build(positions, triangles);
        rebuilt.IsComplemented = mesh.IsComplemented;

        pruned = rebuilt;
        droppedComponentCount = components.Count - keepCount;
        return droppedComponentCount > 0;
    }

    private readonly struct ComponentEdgeStats
    {
        public int BoundaryEdgeCount { get; }
        public int NonManifoldEdgeCount { get; }

        public ComponentEdgeStats(int boundaryEdgeCount, int nonManifoldEdgeCount)
        {
            BoundaryEdgeCount = boundaryEdgeCount;
            NonManifoldEdgeCount = nonManifoldEdgeCount;
        }
    }

    private static ComponentEdgeStats GetComponentEdgeStats(HalfEdgeMesh mesh, IReadOnlyList<int> componentFaces)
    {
        var edgeUse = new Dictionary<long, int>(componentFaces.Count * 3);
        foreach (int faceIdx in componentFaces)
        {
            var verts = mesh.Faces[faceIdx].GetVertices();
            CountUndirectedEdge(edgeUse, verts[0].Id, verts[1].Id);
            CountUndirectedEdge(edgeUse, verts[1].Id, verts[2].Id);
            CountUndirectedEdge(edgeUse, verts[2].Id, verts[0].Id);
        }

        int boundary = 0;
        int nonManifold = 0;
        foreach (int count in edgeUse.Values)
        {
            if (count == 1) boundary++;
            else if (count > 2) nonManifold++;
        }

        return new ComponentEdgeStats(boundary, nonManifold);
    }

    private static void CountUndirectedEdge(Dictionary<long, int> edgeUse, int i0, int i1)
    {
        int lo = i0 < i1 ? i0 : i1;
        int hi = i0 < i1 ? i1 : i0;
        long key = ((long)lo << 32) | (uint)hi;
        edgeUse.TryGetValue(key, out int count);
        edgeUse[key] = count + 1;
    }

    private static CsgResult WithMesh(CsgResult source, HalfEdgeMesh mesh)
    {
        return new CsgResult
        {
            Mesh = mesh,
            PatchCountA = source.PatchCountA,
            PatchCountB = source.PatchCountB,
            DegenerateCount = source.DegenerateCount,
            IntersectionSegmentCount = source.IntersectionSegmentCount,
            SelectedPatchExtractionMode = source.SelectedPatchExtractionMode,
            SelectedPatchExtractionBoundaryEdgeCount = source.SelectedPatchExtractionBoundaryEdgeCount,
            SelectedPatchExtractionIsEdgeManifold = source.SelectedPatchExtractionIsEdgeManifold,
            SelectedPatchExtractionConnectedComponentCount = source.SelectedPatchExtractionConnectedComponentCount,
            SelectedPatchBoundaryAuthority = source.SelectedPatchBoundaryAuthority,
            SelectedAssemblyTrianglesFromA = source.SelectedAssemblyTrianglesFromA,
            SelectedAssemblyTrianglesFromB = source.SelectedAssemblyTrianglesFromB,
            SelectedAssemblyFlippedTrianglesFromB = source.SelectedAssemblyFlippedTrianglesFromB,
            PatchExtractionCandidateSignatures = source.PatchExtractionCandidateSignatures
        };
    }

    private static ReconstructionInvariantSnapshot AnalyzeReconstructionTopology(HalfEdgeMesh mesh)
    {
        var incidence = MeshStitcher.AnalyzeBoundaryIncidence(mesh);
        bool consistentlyOriented = MeshValidator.IsConsistentlyOriented(mesh);

        return new ReconstructionInvariantSnapshot(
            incidence.BoundaryHalfEdgeCount,
            incidence.OpenBoundaryVertexCount,
            incidence.UnmatchedUndirectedEdgeCount,
            incidence.NonManifoldUndirectedEdgeCount,
            consistentlyOriented);
    }

    private readonly record struct ReconstructionInvariantSnapshot(
        int BoundaryHalfEdgeCount,
        int OpenBoundaryLoopCount,
        int UnmatchedUndirectedEdgeCount,
        int NonManifoldUndirectedEdgeCount,
        bool IsConsistentlyOriented)
    {
        public bool IsValid =>
            BoundaryHalfEdgeCount == 0
            && OpenBoundaryLoopCount == 0
            && UnmatchedUndirectedEdgeCount == 0
            && IsConsistentlyOriented;
    }

    private static ReconstructionPolicySnapshot EvaluateReconstructionPolicySnapshot(
        RobustCsgOperation operation,
        CsgResult result)
    {
        int fromA = result.SelectedAssemblyTrianglesFromA.GetValueOrDefault(0);
        int fromB = result.SelectedAssemblyTrianglesFromB.GetValueOrDefault(0);
        int flippedFromB = result.SelectedAssemblyFlippedTrianglesFromB.GetValueOrDefault(0);

        bool valid = operation switch
        {
            RobustCsgOperation.Union => flippedFromB == 0,
            RobustCsgOperation.Intersection => flippedFromB == 0,
            RobustCsgOperation.Difference => flippedFromB >= 0 && flippedFromB <= fromB,
            _ => true
        };

        string rule = operation switch
        {
            RobustCsgOperation.Union => "union:no-flip-b",
            RobustCsgOperation.Intersection => "intersection:no-flip-b",
            RobustCsgOperation.Difference => "difference:0<=flipB<=fromB",
            _ => "unknown"
        };

        return new ReconstructionPolicySnapshot(
            FromA: fromA,
            FromB: fromB,
            FlippedFromB: flippedFromB,
            IsValid: valid,
            Rule: rule);
    }

    private readonly record struct ReconstructionPolicySnapshot(
        int FromA,
        int FromB,
        int FlippedFromB,
        bool IsValid,
        string Rule);

    private static ReconstructionComponentSnapshot AnalyzeComponentTopology(HalfEdgeMesh mesh)
    {
        var components = MeshQueries.ConnectedComponents(mesh);
        if (components.Count == 0)
            return new ReconstructionComponentSnapshot(ComponentCount: 0, InvalidComponentCount: 0);

        int invalid = 0;
        foreach (var component in components)
        {
            if (component.Count < 4)
            {
                invalid++;
                continue;
            }

            var componentMesh = BuildComponentMesh(mesh, component);
            if (MeshValidator.CountBoundaryEdges(componentMesh) > 0
                || !MeshValidator.IsEdgeManifold(componentMesh))
            {
                invalid++;
            }
        }

        return new ReconstructionComponentSnapshot(
            ComponentCount: components.Count,
            InvalidComponentCount: invalid);
    }

    private static HalfEdgeMesh BuildComponentMesh(HalfEdgeMesh mesh, IReadOnlyList<int> componentFaces)
    {
        var vertexMap = new Dictionary<int, int>(componentFaces.Count * 3);
        var positions = new List<Vec3>(componentFaces.Count * 3);
        var triangles = new List<(int I0, int I1, int I2)>(componentFaces.Count);

        foreach (int faceIdx in componentFaces)
        {
            var verts = mesh.Faces[faceIdx].GetVertices();
            int i0 = MapVertex(verts[0].Id, verts[0].Position);
            int i1 = MapVertex(verts[1].Id, verts[1].Position);
            int i2 = MapVertex(verts[2].Id, verts[2].Position);
            triangles.Add((i0, i1, i2));
        }

        var componentMesh = new MeshBuilder(0.0).Build(positions, triangles);
        componentMesh.IsComplemented = mesh.IsComplemented;
        return componentMesh;

        int MapVertex(int originalId, Vec3 position)
        {
            if (vertexMap.TryGetValue(originalId, out int mapped))
                return mapped;

            int next = positions.Count;
            vertexMap[originalId] = next;
            positions.Add(position);
            return next;
        }
    }

    private readonly record struct ReconstructionComponentSnapshot(
        int ComponentCount,
        int InvalidComponentCount);

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
        int triangulationFallbackWorkBudgetExceededCount,
        IReadOnlyList<string> triangulationFallbackSignatures,
        int triangulationNativeFailureCount,
        int triangulationNativeFailureInvalidOrCrossingConstraintCount,
        int triangulationNativeFailurePartitionFailureCount,
        int triangulationNativeFailureConstrainedEarFailureCount,
        int triangulationNativeFailureWorkBudgetExceededCount,
        IReadOnlyList<string> triangulationNativeFailureSignatures,
        IReadOnlyList<string> triangulationNativeFailureCodes,
        int classificationFallbackCount,
        int reconstructionBoundaryHalfEdgeCount,
        int reconstructionOpenBoundaryLoopCount,
        int reconstructionUnmatchedUndirectedEdgeCount,
        int reconstructionNonManifoldUndirectedEdgeCount,
        int reconstructionDroppedComponentCount,
        int reconstructionArrangementSnapCount,
        int reconstructionArrangementEdgeSnapCount,
        int reconstructionComponentCount,
        int reconstructionInvalidComponentCount,
        IReadOnlyList<string> reconstructionInvariantCertificates,
        IReadOnlyList<string> stageInvariantCertificates)
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
            TriangulationFallbackWorkBudgetExceededCount = triangulationFallbackWorkBudgetExceededCount,
            TriangulationFallbackSignatures = triangulationFallbackSignatures,
            TriangulationNativeFailureCount = triangulationNativeFailureCount,
            TriangulationNativeFailureInvalidOrCrossingConstraintCount = triangulationNativeFailureInvalidOrCrossingConstraintCount,
            TriangulationNativeFailurePartitionFailureCount = triangulationNativeFailurePartitionFailureCount,
            TriangulationNativeFailureConstrainedEarFailureCount = triangulationNativeFailureConstrainedEarFailureCount,
            TriangulationNativeFailureWorkBudgetExceededCount = triangulationNativeFailureWorkBudgetExceededCount,
            TriangulationNativeFailureSignatures = triangulationNativeFailureSignatures,
            TriangulationNativeFailureCodes = triangulationNativeFailureCodes,
            ReconstructionBoundaryHalfEdgeCount = reconstructionBoundaryHalfEdgeCount,
            ReconstructionOpenBoundaryLoopCount = reconstructionOpenBoundaryLoopCount,
            ReconstructionUnmatchedUndirectedEdgeCount = reconstructionUnmatchedUndirectedEdgeCount,
            ReconstructionNonManifoldUndirectedEdgeCount = reconstructionNonManifoldUndirectedEdgeCount,
            ReconstructionDroppedComponentCount = reconstructionDroppedComponentCount,
            ReconstructionArrangementSnapCount = reconstructionArrangementSnapCount,
            ReconstructionArrangementEdgeSnapCount = reconstructionArrangementEdgeSnapCount,
            ReconstructionComponentCount = reconstructionComponentCount,
            ReconstructionInvalidComponentCount = reconstructionInvalidComponentCount,
            ReconstructionInvariantCertificates = reconstructionInvariantCertificates,
            StageInvariantCertificates = stageInvariantCertificates,
            ClassificationFallbackCount = classificationFallbackCount
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
