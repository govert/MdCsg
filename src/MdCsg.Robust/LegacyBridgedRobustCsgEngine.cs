using System.Diagnostics;
using System.Collections.Concurrent;
using System.Linq;
using MdCsg.Api;
using MdCsg.Arithmetic;
using MdCsg.Cutting;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Operations;
using MdCsg.Patches;
using MdCsg.Predicates;
using MdCsg.Robust.Kernel.Arrangement;
using MdCsg.Robust.Kernel.Predicates;
using MdCsg.Robust.Kernel.Triangulation;
using MdCsg.Robust.Validation;
using System.Globalization;

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
        var inputA = a;
        var inputB = b;
        var inputPolicyOutcomeA = InputPolicyOutcome.Original(a.Mesh.Faces.Count);
        var inputPolicyOutcomeB = InputPolicyOutcome.Original(b.Mesh.Faces.Count);
        if (opts.NonManifoldInputPolicy == NonManifoldInputPolicy.SanitizeAndContinue)
        {
            inputA = SanitizeInputSolid(a, out inputPolicyOutcomeA);
            inputB = SanitizeInputSolid(b, out inputPolicyOutcomeB);
        }
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
        int reconstructionLoopAssemblyOpenChainCount = 0;
        int reconstructionLoopAssemblyAmbiguousBranchCount = 0;
        int reconstructionLoopAssemblySkippedDegenerateCount = 0;
        int reconstructionSnapCollapseRejectCount = 0;
        int reconstructionSnapDegenerateRejectCount = 0;
        double reconstructionMaxSnapDistance = 0.0;
        bool reconstructionIncidencePreserved = true;
        int classificationFallbackCount = 0;
        var reconstructionCertificates = new List<string>();
        var stageCertificates = new List<string>();
        MeshInvariantSnapshot inputAInvariant = default;
        MeshInvariantSnapshot inputBInvariant = default;
        MeshInvariantSnapshot outputInvariant = default;
        var totalSw = Stopwatch.StartNew();
        stageCertificates.Add(
            $"input-policy:policy={opts.NonManifoldInputPolicy};"
            + $"A={inputPolicyOutcomeA.Action};B={inputPolicyOutcomeB.Action};"
            + $"Araw={inputPolicyOutcomeA.RawComponentCount};Avalid={inputPolicyOutcomeA.ValidComponentCount};"
            + $"Akept={inputPolicyOutcomeA.KeptFaceCount};Atotal={inputPolicyOutcomeA.TotalFaceCount};"
            + $"Braw={inputPolicyOutcomeB.RawComponentCount};Bvalid={inputPolicyOutcomeB.ValidComponentCount};"
            + $"Bkept={inputPolicyOutcomeB.KeptFaceCount};Btotal={inputPolicyOutcomeB.TotalFaceCount}");

        if (opts.ValidateInput)
        {
            inputAInvariant = ValidateInput(inputA, "A", opts.Mode, issues, predicateTelemetry);
            inputBInvariant = ValidateInput(inputB, "B", opts.Mode, issues, predicateTelemetry);

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
            arrangement = ArrangementBuilder.Build(inputA.Mesh, inputB.Mesh);
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
            PreferTopologyPreservingPatchExtraction = opts.Mode == RobustMode.Strict,
            ForceArrangementPatchExtraction = opts.Mode == RobustMode.Strict
        };

        var opSw = Stopwatch.StartNew();
        var result = operation switch
        {
            RobustCsgOperation.Union => Csg.Union(inputA, inputB, csgOptions),
            RobustCsgOperation.Intersection => Csg.Intersect(inputA, inputB, csgOptions),
            RobustCsgOperation.Difference => Csg.Difference(inputA, inputB, csgOptions),
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
        };
        opSw.Stop();
        result = PruneDuplicateOutputFaces(result, csgOptions.WeldTolerance, out int duplicateFaceDropCount);
        stageCertificates.Add($"post-op-dedup:removedFaces={duplicateFaceDropCount}");

        classificationFallbackCount = result.DegenerateCount;
        int classifiedCertifiedCount = result.SelectedCertifiedPatchCount
            ?? System.Math.Max(0, (result.PatchCountA + result.PatchCountB) - classificationFallbackCount);
        int classifiedUncertifiedCount = result.SelectedUncertifiedPatchCount ?? classificationFallbackCount;
        string classificationFingerprint =
            string.IsNullOrWhiteSpace(result.SelectedClassificationEvidenceFingerprint)
                ? "<none>"
                : result.SelectedClassificationEvidenceFingerprint!;

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
        string coplanarRows = result.SelectedCoplanarDecisionRows.Count == 0
            ? "<none>"
            : string.Join(",", result.SelectedCoplanarDecisionRows);
        stageCertificates.Add($"coplanar-matrix:rows={coplanarRows};pass=1");
        var authoritySnapshot = EvaluateReconstructionAuthoritySnapshot(result);
        stageCertificates.Add(
            "reconstruction-authority:"
            + $"mode={authoritySnapshot.Mode};"
            + $"authority={authoritySnapshot.Authority};"
            + $"boundary={authoritySnapshot.Boundary};"
            + $"manifold={authoritySnapshot.Manifold};"
            + $"components={authoritySnapshot.Components};"
            + $"pass={(authoritySnapshot.IsValid ? 1 : 0)}");
        if (opts.Mode == RobustMode.Strict && !authoritySnapshot.IsValid)
        {
            AddIssue(
                issues,
                opts.Mode,
                RobustIssueCode.StageInvariantViolation,
                "Reconstruction authority contract is missing or invalid.",
                RobustIssueSeverity.Error);
        }
        stageCertificates.Add(
            $"classification:pass;certified={classifiedCertifiedCount};"
            + $"fallback={classificationFallbackCount};"
            + $"uncertified={classifiedUncertifiedCount};"
            + $"fingerprint={classificationFingerprint};"
            + "policy=margin>errorBound");

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

        var preReconstructionInvariant = AnalyzeReconstructionTopology(result.Mesh);
        var preComponentInvariant = AnalyzeComponentTopology(result.Mesh);
        stageCertificates.Add(
            "reconstruction-pre:"
            + $"boundary={preReconstructionInvariant.BoundaryHalfEdgeCount};"
            + $"openLoops={preReconstructionInvariant.OpenBoundaryLoopCount};"
            + $"unmatched={preReconstructionInvariant.UnmatchedUndirectedEdgeCount};"
            + $"nonManifold={preReconstructionInvariant.NonManifoldUndirectedEdgeCount};"
            + $"oriented={(preReconstructionInvariant.IsConsistentlyOriented ? 1 : 0)};"
            + $"components={preComponentInvariant.ComponentCount};"
            + $"invalidComponents={preComponentInvariant.InvalidComponentCount}");

        result = PruneDegenerateOutputFaces(
            result,
            csgOptions.WeldTolerance,
            predicateTelemetry,
            out int prePruneDegBefore,
            out int prePruneRemoved,
            out int prePruneAfterRemove,
            out int prePruneResealIntroduced,
            out bool prePruneResealDegSafe,
            out int prePruneResealLoopDegSkipped,
            out int prePruneResealLoopDupVidSkipped,
            out int prePruneResealLoopZeroEdgeSkipped,
            out int prePruneResealLoopDupPosSkipped,
            out int prePruneResealLoopCollinearSkipped,
            out int prePruneDegAfter,
            out bool prePruneAccepted,
            out int prePruneIterations,
            out int prePruneAppliedIterations,
            out string prePruneTermination,
            out bool prePruneClosedGuard,
            out int prePruneBoundaryBefore,
            out int prePruneBoundaryAfter,
            out int prePruneUnmatchedBefore,
            out int prePruneUnmatchedAfter);
        stageCertificates.Add(
            $"deg-prune:phase=pre;"
            + $"before={prePruneDegBefore};"
            + $"removed={prePruneRemoved};"
            + $"afterRemove={prePruneAfterRemove};"
            + $"resealIntro={prePruneResealIntroduced};"
            + $"resealSafe={(prePruneResealDegSafe ? 1 : 0)};"
            + $"resealLoopDegSkipped={prePruneResealLoopDegSkipped};"
            + $"resealLoopDupVidSkipped={prePruneResealLoopDupVidSkipped};"
            + $"resealLoopZeroEdgeSkipped={prePruneResealLoopZeroEdgeSkipped};"
            + $"resealLoopDupPosSkipped={prePruneResealLoopDupPosSkipped};"
            + $"resealLoopCollinearSkipped={prePruneResealLoopCollinearSkipped};"
            + $"after={prePruneDegAfter};"
            + $"netRemoved={System.Math.Max(0, prePruneDegBefore - prePruneDegAfter)};"
            + $"accepted={(prePruneAccepted ? 1 : 0)};"
            + $"iters={prePruneIterations};"
            + $"applied={prePruneAppliedIterations};"
            + $"term={prePruneTermination};"
            + $"closedGuard={(prePruneClosedGuard ? 1 : 0)};"
            + $"boundaryBefore={prePruneBoundaryBefore};"
            + $"boundaryAfter={prePruneBoundaryAfter};"
            + $"unmatchedBefore={prePruneUnmatchedBefore};"
            + $"unmatchedAfter={prePruneUnmatchedAfter}");
        result = ReconstructOutputTopology(
            result,
            csgOptions.WeldTolerance,
            arrangement,
            result.AuthoritativeBoundary,
            out reconstructionDroppedComponentCount,
            out reconstructionArrangementSnapCount,
            out reconstructionArrangementEdgeSnapCount,
            out reconstructionLoopAssemblyOpenChainCount,
            out reconstructionLoopAssemblyAmbiguousBranchCount,
            out reconstructionLoopAssemblySkippedDegenerateCount,
            out reconstructionSnapCollapseRejectCount,
            out reconstructionSnapDegenerateRejectCount,
            out reconstructionMaxSnapDistance,
            out reconstructionIncidencePreserved);
        int localRepairBudget = opts.AttemptResidualDegenerateClosure
            ? LocalDegenerateRepairIterationsClosureAttempt
            : LocalDegenerateRepairIterationsDefault;
        result = TryLocalDegenerateNeighborhoodRepair(
            result,
            csgOptions.WeldTolerance,
            result.AuthoritativeBoundary,
            localRepairBudget,
            opts.AttemptResidualDegenerateClosure,
            out int localRepairAuthorityGate,
            out int localRepairDegBefore,
            out int localRepairDegAfter,
            out int localRepairAttemptedFaces,
            out int localRepairRemovedFaces,
            out int localRepairSingleTry,
            out int localRepairPairTry,
            out int localRepairTripleTry,
            out int localRepairMultiApplied,
            out int localRepairMaxArity,
            out int localRepairCollinearGuard,
            out int localRepairCollinearReject,
            out int localRepairCollinearExactCheck,
            out int localRepairCollinearExactConfirm,
            out int localRepairRetriTry,
            out int localRepairRetriApplied,
            out int localRepairIterations,
            out int localRepairAppliedIterations,
            out string localRepairTermination);
        stageCertificates.Add(
            "deg-local-repair:scope=post-reconstruct;"
            + $"gate={localRepairAuthorityGate};"
            + $"before={localRepairDegBefore};"
            + $"after={localRepairDegAfter};"
            + $"attempted={localRepairAttemptedFaces};"
            + $"removed={localRepairRemovedFaces};"
            + $"singleTry={localRepairSingleTry};"
            + $"pairTry={localRepairPairTry};"
            + $"tripleTry={localRepairTripleTry};"
            + $"multiApplied={localRepairMultiApplied};"
            + $"maxArity={localRepairMaxArity};"
            + $"colGuard={localRepairCollinearGuard};"
            + $"colReject={localRepairCollinearReject};"
            + $"colExactCheck={localRepairCollinearExactCheck};"
            + $"colExactConfirm={localRepairCollinearExactConfirm};"
            + $"retriTry={localRepairRetriTry};"
            + $"retriApplied={localRepairRetriApplied};"
            + $"iters={localRepairIterations};"
            + $"applied={localRepairAppliedIterations};"
            + $"closureAttempt={(opts.AttemptResidualDegenerateClosure ? 1 : 0)};"
            + $"budget={localRepairBudget};"
            + $"term={localRepairTermination}");
        result = PruneDegenerateOutputFaces(
            result,
            csgOptions.WeldTolerance,
            predicateTelemetry,
            out int postPruneDegBefore,
            out int postPruneRemoved,
            out int postPruneAfterRemove,
            out int postPruneResealIntroduced,
            out bool postPruneResealDegSafe,
            out int postPruneResealLoopDegSkipped,
            out int postPruneResealLoopDupVidSkipped,
            out int postPruneResealLoopZeroEdgeSkipped,
            out int postPruneResealLoopDupPosSkipped,
            out int postPruneResealLoopCollinearSkipped,
            out int postPruneDegAfter,
            out bool postPruneAccepted,
            out int postPruneIterations,
            out int postPruneAppliedIterations,
            out string postPruneTermination,
            out bool postPruneClosedGuard,
            out int postPruneBoundaryBefore,
            out int postPruneBoundaryAfter,
            out int postPruneUnmatchedBefore,
            out int postPruneUnmatchedAfter);
        stageCertificates.Add(
            $"deg-prune:phase=post;"
            + $"before={postPruneDegBefore};"
            + $"removed={postPruneRemoved};"
            + $"afterRemove={postPruneAfterRemove};"
            + $"resealIntro={postPruneResealIntroduced};"
            + $"resealSafe={(postPruneResealDegSafe ? 1 : 0)};"
            + $"resealLoopDegSkipped={postPruneResealLoopDegSkipped};"
            + $"resealLoopDupVidSkipped={postPruneResealLoopDupVidSkipped};"
            + $"resealLoopZeroEdgeSkipped={postPruneResealLoopZeroEdgeSkipped};"
            + $"resealLoopDupPosSkipped={postPruneResealLoopDupPosSkipped};"
            + $"resealLoopCollinearSkipped={postPruneResealLoopCollinearSkipped};"
            + $"after={postPruneDegAfter};"
            + $"netRemoved={System.Math.Max(0, postPruneDegBefore - postPruneDegAfter)};"
            + $"accepted={(postPruneAccepted ? 1 : 0)};"
            + $"iters={postPruneIterations};"
            + $"applied={postPruneAppliedIterations};"
            + $"term={postPruneTermination};"
            + $"closedGuard={(postPruneClosedGuard ? 1 : 0)};"
            + $"boundaryBefore={postPruneBoundaryBefore};"
            + $"boundaryAfter={postPruneBoundaryAfter};"
            + $"unmatchedBefore={postPruneUnmatchedBefore};"
            + $"unmatchedAfter={postPruneUnmatchedAfter}");
        var reconstructionInvariant = AnalyzeReconstructionTopology(result.Mesh);
        reconstructionBoundaryHalfEdgeCount = reconstructionInvariant.BoundaryHalfEdgeCount;
        reconstructionOpenBoundaryLoopCount = reconstructionInvariant.OpenBoundaryLoopCount;
        reconstructionUnmatchedUndirectedEdgeCount = reconstructionInvariant.UnmatchedUndirectedEdgeCount;
        reconstructionNonManifoldUndirectedEdgeCount = reconstructionInvariant.NonManifoldUndirectedEdgeCount;
        var componentInvariant = AnalyzeComponentTopology(result.Mesh);
        reconstructionComponentCount = componentInvariant.ComponentCount;
        reconstructionInvalidComponentCount = componentInvariant.InvalidComponentCount;
        bool reconstructionNonWorseningPass =
            reconstructionInvariant.BoundaryHalfEdgeCount <= preReconstructionInvariant.BoundaryHalfEdgeCount
            && reconstructionInvariant.OpenBoundaryLoopCount <= preReconstructionInvariant.OpenBoundaryLoopCount
            && reconstructionInvariant.UnmatchedUndirectedEdgeCount <= preReconstructionInvariant.UnmatchedUndirectedEdgeCount
            && reconstructionInvariant.NonManifoldUndirectedEdgeCount <= preReconstructionInvariant.NonManifoldUndirectedEdgeCount
            && reconstructionInvalidComponentCount <= preComponentInvariant.InvalidComponentCount;

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
            + $"loopOpenChains={reconstructionLoopAssemblyOpenChainCount};"
            + $"loopAmbiguous={reconstructionLoopAssemblyAmbiguousBranchCount};"
            + $"loopDegSkipped={reconstructionLoopAssemblySkippedDegenerateCount};"
            + $"snapCollapseReject={reconstructionSnapCollapseRejectCount};"
            + $"snapDegReject={reconstructionSnapDegenerateRejectCount};"
            + $"snapMax={reconstructionMaxSnapDistance.ToString("G17", CultureInfo.InvariantCulture)};"
            + $"incidencePreserved={(reconstructionIncidencePreserved ? 1 : 0)};"
            + $"components={reconstructionComponentCount};"
            + $"invalidComponents={reconstructionInvalidComponentCount};"
            + $"nonWorse={(reconstructionNonWorseningPass ? 1 : 0)}";
        reconstructionCertificates.Add(reconstructionCertificate);
        stageCertificates.Add(reconstructionCertificate);

        if (opts.Mode == RobustMode.Strict && !reconstructionNonWorseningPass)
        {
            AddIssue(
                issues,
                opts.Mode,
                RobustIssueCode.StageInvariantViolation,
                "Reconstruction contract violated: post-reconstruction topology worsened relative to pre-reconstruction state.",
                RobustIssueSeverity.Error);
        }

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

        if (reconstructionLoopAssemblyAmbiguousBranchCount > 0 && !reconstructionStagePass)
        {
            AddIssue(
                issues,
                opts.Mode,
                RobustIssueCode.ReconstructionStitchingFailed,
                $"Deterministic loop assembly reported ambiguous boundary branches ({reconstructionLoopAssemblyAmbiguousBranchCount}).",
                RobustIssueSeverity.Error);
            AddIssue(
                issues,
                opts.Mode,
                RobustIssueCode.StageInvariantViolation,
                "Reconstruction loop assembly ambiguity requires fail-closed behavior.",
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
            if (outputInvariant.DegenerateFaceCount > 0)
            {
                stageCertificates.Add(
                    BuildResidualDegenerateCertificate(
                        result.Mesh,
                        outputInvariant.DegenerateFaceCount));
            }

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

    private static string BuildResidualDegenerateCertificate(HalfEdgeMesh mesh, int expectedCount)
    {
        int degenerateCount = 0;
        ulong faceHash = 1469598103934665603UL;
        ulong origHash = 1469598103934665603UL;
        ulong patchHash = 1469598103934665603UL;
        ulong vertexHash = 1469598103934665603UL;
        ulong taxonomyHash = 1469598103934665603UL;
        int taxonomyDupVid = 0;
        int taxonomyZeroEdge = 0;
        int taxonomyDupPos = 0;
        int taxonomyCollinear = 0;
        var collinearFaces = new List<(int FaceId, int V0, int V1, int V2)>(16);
        var samples = new List<string>(6);

        foreach (var face in mesh.Faces)
        {
            var verts = face.GetVertices();
            Vec3 a = verts[0].Position;
            Vec3 b = verts[1].Position;
            Vec3 c = verts[2].Position;
            var areaSign = EvaluateProjectedAreaSign(a, b, c);
            if (areaSign.Sign != PredicateSign.Zero)
                continue;

            degenerateCount++;
            MixHash(ref faceHash, unchecked((uint)face.Id));
            MixHash(ref origHash, unchecked((uint)face.OriginalFaceId));
            MixHash(ref patchHash, unchecked((uint)face.PatchId));
            MixHash(ref vertexHash, unchecked((uint)verts[0].Id));
            MixHash(ref vertexHash, unchecked((uint)verts[1].Id));
            MixHash(ref vertexHash, unchecked((uint)verts[2].Id));
            var classification = ClassifyResidualDegenerateFace(verts[0], verts[1], verts[2]);
            switch (classification)
            {
                case ResidualDegenerateClass.DuplicateVertexId:
                    taxonomyDupVid++;
                    break;
                case ResidualDegenerateClass.ZeroLengthEdge:
                    taxonomyZeroEdge++;
                    break;
                case ResidualDegenerateClass.DuplicatePosition:
                    taxonomyDupPos++;
                    break;
                default:
                    taxonomyCollinear++;
                    collinearFaces.Add((face.Id, verts[0].Id, verts[1].Id, verts[2].Id));
                    break;
            }
            MixHash(ref taxonomyHash, (uint)classification);

            if (samples.Count < 6)
            {
                samples.Add(
                    $"f{face.Id}:o{face.OriginalFaceId}:p{face.PatchId}:v{verts[0].Id}-{verts[1].Id}-{verts[2].Id}:t{(int)classification}");
            }
        }

        int countMatch = degenerateCount == expectedCount ? 1 : 0;
        int collinearOnly = degenerateCount > 0 && taxonomyCollinear == degenerateCount ? 1 : 0;
        int colAdjPairs = 0;
        ulong colAdjHash = 1469598103934665603UL;
        var colVertices = new HashSet<int>();
        foreach (var collinear in collinearFaces)
        {
            colVertices.Add(collinear.V0);
            colVertices.Add(collinear.V1);
            colVertices.Add(collinear.V2);
        }

        for (int i = 0; i < collinearFaces.Count - 1; i++)
        {
            var aCol = collinearFaces[i];
            for (int j = i + 1; j < collinearFaces.Count; j++)
            {
                var bCol = collinearFaces[j];
                bool shared =
                    aCol.V0 == bCol.V0 || aCol.V0 == bCol.V1 || aCol.V0 == bCol.V2
                    || aCol.V1 == bCol.V0 || aCol.V1 == bCol.V1 || aCol.V1 == bCol.V2
                    || aCol.V2 == bCol.V0 || aCol.V2 == bCol.V1 || aCol.V2 == bCol.V2;
                if (!shared)
                    continue;

                colAdjPairs++;
                uint minFace = unchecked((uint)System.Math.Min(aCol.FaceId, bCol.FaceId));
                uint maxFace = unchecked((uint)System.Math.Max(aCol.FaceId, bCol.FaceId));
                MixHash(ref colAdjHash, minFace);
                MixHash(ref colAdjHash, maxFace);
            }
        }

        int colVertSpan = colVertices.Count;
        string sample = samples.Count == 0
            ? "<none>"
            : string.Join("|", samples);
        return "deg-residual:scope=output;"
            + "v=1;"
            + $"count={degenerateCount};"
            + $"expected={expectedCount};"
            + $"countMatch={countMatch};"
            + $"faceHash={faceHash:x16};"
            + $"origHash={origHash:x16};"
            + $"patchHash={patchHash:x16};"
            + $"vertHash={vertexHash:x16};"
            + $"taxDupVid={taxonomyDupVid};"
            + $"taxZeroEdge={taxonomyZeroEdge};"
            + $"taxDupPos={taxonomyDupPos};"
            + $"taxCollinear={taxonomyCollinear};"
            + $"collinearOnly={collinearOnly};"
            + $"colAdjPairs={colAdjPairs};"
            + $"colVertSpan={colVertSpan};"
            + $"colAdjHash={colAdjHash:x16};"
            + $"taxHash={taxonomyHash:x16};"
            + $"sample={sample}";
    }

    private enum ResidualDegenerateClass : uint
    {
        DuplicateVertexId = 1,
        ZeroLengthEdge = 2,
        DuplicatePosition = 3,
        Collinear = 4
    }

    private static ResidualDegenerateClass ClassifyResidualDegenerateFace(Vertex v0, Vertex v1, Vertex v2)
    {
        if (v0.Id == v1.Id || v1.Id == v2.Id || v2.Id == v0.Id)
            return ResidualDegenerateClass.DuplicateVertexId;

        double e01 = Vec3.DistanceSquared(v0.Position, v1.Position);
        double e12 = Vec3.DistanceSquared(v1.Position, v2.Position);
        double e20 = Vec3.DistanceSquared(v2.Position, v0.Position);
        if (e01 == 0.0 || e12 == 0.0 || e20 == 0.0)
            return ResidualDegenerateClass.ZeroLengthEdge;

        if (SamePosition(v0.Position, v1.Position)
            || SamePosition(v1.Position, v2.Position)
            || SamePosition(v2.Position, v0.Position))
        {
            return ResidualDegenerateClass.DuplicatePosition;
        }

        return ResidualDegenerateClass.Collinear;
    }

    private static bool SamePosition(Vec3 a, Vec3 b)
        => a.X == b.X && a.Y == b.Y && a.Z == b.Z;

    private static void MixHash(ref ulong hash, uint value)
    {
        hash ^= value;
        hash *= 1099511628211UL;
    }

    private static Solid SanitizeInputSolid(Solid input, out InputPolicyOutcome outcome)
    {
        var components = RawConnectedComponents(input.Mesh);
        if (components.Count == 0)
        {
            outcome = InputPolicyOutcome.Original(input.Mesh.Faces.Count);
            return input;
        }

        int validComponentCount = 0;
        int bestComponentIndex = -1;
        int bestFaceCount = -1;
        double bestAbsVolume = -1.0;
        int bestFaceOrder = int.MaxValue;

        for (int i = 0; i < components.Count; i++)
        {
            var component = components[i];
            if (component.Count == 0)
                continue;

            var componentMesh = BuildComponentMesh(input.Mesh, component);
            if (!IsSanitizeCandidateValid(componentMesh))
                continue;

            validComponentCount++;

            int faceCount = component.Count;
            double absVolume = System.Math.Abs(MeshQueries.Volume(componentMesh));
            int faceOrder = component.Min();
            bool better = faceCount > bestFaceCount
                || (faceCount == bestFaceCount && absVolume > bestAbsVolume + 1e-12)
                || (faceCount == bestFaceCount
                    && System.Math.Abs(absVolume - bestAbsVolume) <= 1e-12
                    && faceOrder < bestFaceOrder);

            if (better)
            {
                bestComponentIndex = i;
                bestFaceCount = faceCount;
                bestAbsVolume = absVolume;
                bestFaceOrder = faceOrder;
            }
        }

        if (bestComponentIndex < 0)
        {
            outcome = new InputPolicyOutcome(
                Action: "original",
                RawComponentCount: components.Count,
                ValidComponentCount: 0,
                KeptFaceCount: input.Mesh.Faces.Count,
                TotalFaceCount: input.Mesh.Faces.Count);
            return input;
        }

        var bestComponent = components[bestComponentIndex];
        bool sanitizeApplied = components.Count > 1 || bestComponent.Count != input.Mesh.Faces.Count;
        if (!sanitizeApplied)
        {
            outcome = new InputPolicyOutcome(
                Action: "original",
                RawComponentCount: components.Count,
                ValidComponentCount: validComponentCount,
                KeptFaceCount: input.Mesh.Faces.Count,
                TotalFaceCount: input.Mesh.Faces.Count);
            return input;
        }

        var sanitizedMesh = BuildComponentMesh(input.Mesh, bestComponent);
        sanitizedMesh.IsComplemented = input.Mesh.IsComplemented;
        outcome = new InputPolicyOutcome(
            Action: "sanitized",
            RawComponentCount: components.Count,
            ValidComponentCount: validComponentCount,
            KeptFaceCount: sanitizedMesh.Faces.Count,
            TotalFaceCount: input.Mesh.Faces.Count);
        return new Solid(sanitizedMesh);
    }

    private static bool IsSanitizeCandidateValid(HalfEdgeMesh mesh)
    {
        if (!HasFiniteVertices(mesh))
            return false;
        if (MeshValidator.CountBoundaryEdges(mesh) != 0)
            return false;
        if (!MeshValidator.IsEdgeManifold(mesh))
            return false;

        var telemetry = new PredicateTelemetryCounter();
        return DegenerateFaceInspector.CountDegenerateFaces(mesh, telemetry) == 0;
    }

    private static List<List<int>> RawConnectedComponents(HalfEdgeMesh mesh)
    {
        var visited = new bool[mesh.Faces.Count];
        var components = new List<List<int>>();
        var queue = new Queue<int>();

        for (int i = 0; i < mesh.Faces.Count; i++)
        {
            if (visited[i])
                continue;

            visited[i] = true;
            queue.Enqueue(i);
            var component = new List<int>();

            while (queue.Count > 0)
            {
                int faceIdx = queue.Dequeue();
                component.Add(faceIdx);

                var start = mesh.Faces[faceIdx].Edge;
                var edge = start;
                do
                {
                    if (edge.Twin?.Face is not null)
                    {
                        int neighbor = edge.Twin.Face.Id;
                        if (!visited[neighbor])
                        {
                            visited[neighbor] = true;
                            queue.Enqueue(neighbor);
                        }
                    }

                    edge = edge.Next;
                } while (edge != start);
            }

            components.Add(component);
        }

        return components;
    }

    private readonly record struct InputPolicyOutcome(
        string Action,
        int RawComponentCount,
        int ValidComponentCount,
        int KeptFaceCount,
        int TotalFaceCount)
    {
        public static InputPolicyOutcome Original(int totalFaceCount)
            => new(
                Action: "original",
                RawComponentCount: 1,
                ValidComponentCount: 1,
                KeptFaceCount: totalFaceCount,
                TotalFaceCount: totalFaceCount);
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

    private const int MaxDegeneratePruneIterations = 3;

    private readonly record struct DegeneratePrunePassResult(
        CsgResult Result,
        int BeforeDegenerate,
        int RemovedDegenerateFaces,
        int AfterRemovalDegenerate,
        int ResealIntroducedDegenerate,
        bool ResealDegenerateSafe,
        int ResealSkippedDegenerateTriangles,
        int ResealSkippedDuplicateVertexIdTriangles,
        int ResealSkippedZeroEdgeTriangles,
        int ResealSkippedDuplicatePositionTriangles,
        int ResealSkippedCollinearTriangles,
        int AfterDegenerate,
        bool Accepted,
        bool PreserveClosedContract,
        int BoundaryBefore,
        int BoundaryAfter,
        int UnmatchedBefore,
        int UnmatchedAfter);

    private static CsgResult PruneDegenerateOutputFaces(
        CsgResult result,
        double weldTolerance,
        PredicateTelemetryCounter predicateTelemetry,
        out int beforeDegenerate,
        out int removedDegenerateFaces,
        out int afterRemovalDegenerate,
        out int resealIntroducedDegenerate,
        out bool resealDegenerateSafe,
        out int resealSkippedDegenerateTriangles,
        out int resealSkippedDuplicateVertexIdTriangles,
        out int resealSkippedZeroEdgeTriangles,
        out int resealSkippedDuplicatePositionTriangles,
        out int resealSkippedCollinearTriangles,
        out int afterDegenerate,
        out bool accepted,
        out int iterations,
        out int appliedIterations,
        out string terminationReason,
        out bool preserveClosedContract,
        out int boundaryBefore,
        out int boundaryAfter,
        out int unmatchedBefore,
        out int unmatchedAfter)
    {
        beforeDegenerate = 0;
        removedDegenerateFaces = 0;
        afterRemovalDegenerate = 0;
        resealIntroducedDegenerate = 0;
        resealDegenerateSafe = true;
        resealSkippedDegenerateTriangles = 0;
        resealSkippedDuplicateVertexIdTriangles = 0;
        resealSkippedZeroEdgeTriangles = 0;
        resealSkippedDuplicatePositionTriangles = 0;
        resealSkippedCollinearTriangles = 0;
        afterDegenerate = 0;
        accepted = false;
        iterations = 0;
        appliedIterations = 0;
        terminationReason = "none";
        preserveClosedContract = false;
        boundaryBefore = 0;
        boundaryAfter = 0;
        unmatchedBefore = 0;
        unmatchedAfter = 0;

        CsgResult current = result;
        int lastAfter = 0;
        int lastAfterRemoval = 0;
        bool sawPass = false;

        for (int i = 0; i < MaxDegeneratePruneIterations; i++)
        {
            iterations++;
            var pass = PruneDegenerateOutputFacesSinglePass(current, weldTolerance, predicateTelemetry);
            if (!sawPass)
            {
                beforeDegenerate = pass.BeforeDegenerate;
                boundaryBefore = pass.BoundaryBefore;
                unmatchedBefore = pass.UnmatchedBefore;
                preserveClosedContract = pass.PreserveClosedContract;
                sawPass = true;
            }

            removedDegenerateFaces += pass.RemovedDegenerateFaces;
            resealIntroducedDegenerate += pass.ResealIntroducedDegenerate;
            resealSkippedDegenerateTriangles += pass.ResealSkippedDegenerateTriangles;
            resealSkippedDuplicateVertexIdTriangles += pass.ResealSkippedDuplicateVertexIdTriangles;
            resealSkippedZeroEdgeTriangles += pass.ResealSkippedZeroEdgeTriangles;
            resealSkippedDuplicatePositionTriangles += pass.ResealSkippedDuplicatePositionTriangles;
            resealSkippedCollinearTriangles += pass.ResealSkippedCollinearTriangles;
            resealDegenerateSafe &= pass.ResealDegenerateSafe;
            lastAfterRemoval = pass.AfterRemovalDegenerate;
            boundaryAfter = pass.BoundaryAfter;
            unmatchedAfter = pass.UnmatchedAfter;

            if (!pass.Accepted)
            {
                terminationReason = i == 0 ? "rejected-initial" : "rejected";
                break;
            }

            current = pass.Result;
            appliedIterations++;
            accepted = true;
            lastAfter = pass.AfterDegenerate;

            if (pass.AfterDegenerate == 0)
            {
                terminationReason = "cleared";
                break;
            }

            if (pass.AfterDegenerate >= pass.BeforeDegenerate)
            {
                terminationReason = "stalled";
                break;
            }

            if (appliedIterations >= MaxDegeneratePruneIterations)
            {
                terminationReason = "budget";
                break;
            }

            terminationReason = "continue";
        }

        if (!sawPass)
        {
            terminationReason = "empty";
            return current;
        }

        if (beforeDegenerate == 0)
            terminationReason = "already-clean";
        else if (terminationReason == "continue")
            terminationReason = "budget";

        if (accepted)
        {
            afterRemovalDegenerate = lastAfterRemoval;
            afterDegenerate = lastAfter;
            return current;
        }

        // Rejected/no-op passes keep the original mesh, so output-facing metrics
        // must remain aligned with the unchanged result.
        afterRemovalDegenerate = beforeDegenerate;
        afterDegenerate = beforeDegenerate;
        boundaryAfter = boundaryBefore;
        unmatchedAfter = unmatchedBefore;
        return result;
    }

    private static DegeneratePrunePassResult PruneDegenerateOutputFacesSinglePass(
        CsgResult result,
        double weldTolerance,
        PredicateTelemetryCounter predicateTelemetry)
    {
        int beforeDegenerate = 0;
        int removedDegenerateFaces = 0;
        int afterRemovalDegenerate = 0;
        int resealIntroducedDegenerate = 0;
        bool resealDegenerateSafe = true;
        int resealSkippedDegenerateTriangles = 0;
        int resealSkippedDuplicateVertexIdTriangles = 0;
        int resealSkippedZeroEdgeTriangles = 0;
        int resealSkippedDuplicatePositionTriangles = 0;
        int resealSkippedCollinearTriangles = 0;
        int afterDegenerate = 0;
        bool accepted = false;
        bool preserveClosedContract = false;
        int boundaryBefore = 0;
        int boundaryAfter = 0;
        int unmatchedBefore = 0;
        int unmatchedAfter = 0;

        if (result.Mesh.Faces.Count == 0)
        {
            return new DegeneratePrunePassResult(
                result,
                beforeDegenerate,
                removedDegenerateFaces,
                afterRemovalDegenerate,
                resealIntroducedDegenerate,
                resealDegenerateSafe,
                resealSkippedDegenerateTriangles,
                resealSkippedDuplicateVertexIdTriangles,
                resealSkippedZeroEdgeTriangles,
                resealSkippedDuplicatePositionTriangles,
                resealSkippedCollinearTriangles,
                afterDegenerate,
                accepted,
                preserveClosedContract,
                boundaryBefore,
                boundaryAfter,
                unmatchedBefore,
                unmatchedAfter);
        }

        beforeDegenerate = DegenerateFaceInspector.CountDegenerateFaces(result.Mesh, predicateTelemetry);
        afterRemovalDegenerate = beforeDegenerate;
        afterDegenerate = beforeDegenerate;
        if (beforeDegenerate == 0)
        {
            return new DegeneratePrunePassResult(
                result,
                beforeDegenerate,
                removedDegenerateFaces,
                afterRemovalDegenerate,
                resealIntroducedDegenerate,
                resealDegenerateSafe,
                resealSkippedDegenerateTriangles,
                resealSkippedDuplicateVertexIdTriangles,
                resealSkippedZeroEdgeTriangles,
                resealSkippedDuplicatePositionTriangles,
                resealSkippedCollinearTriangles,
                afterDegenerate,
                accepted,
                preserveClosedContract,
                boundaryBefore,
                boundaryAfter,
                unmatchedBefore,
                unmatchedAfter);
        }

        var beforeIncidence = MeshStitcher.AnalyzeBoundaryIncidence(result.Mesh);
        var beforeComponents = AnalyzeComponentTopology(result.Mesh);
        boundaryBefore = beforeIncidence.BoundaryHalfEdgeCount;
        unmatchedBefore = beforeIncidence.UnmatchedUndirectedEdgeCount;
        var positions = new List<Vec3>(result.Mesh.Vertices.Count);
        foreach (var vertex in result.Mesh.Vertices)
            positions.Add(vertex.Position);
        var kept = new List<(int I0, int I1, int I2)>(result.Mesh.Faces.Count);
        bool removedAny = false;

        foreach (var face in result.Mesh.Faces)
        {
            var verts = face.GetVertices();
            Vec3 a = verts[0].Position;
            Vec3 b = verts[1].Position;
            Vec3 c = verts[2].Position;
            var areaSign = EvaluateProjectedAreaSign(a, b, c);
            predicateTelemetry.Add(areaSign.Tier);
            if (areaSign.Sign == PredicateSign.Zero)
            {
                removedAny = true;
                removedDegenerateFaces++;
                continue;
            }

            kept.Add((verts[0].Id, verts[1].Id, verts[2].Id));
        }

        if (!removedAny)
        {
            return new DegeneratePrunePassResult(
                result,
                beforeDegenerate,
                removedDegenerateFaces,
                afterRemovalDegenerate,
                resealIntroducedDegenerate,
                resealDegenerateSafe,
                resealSkippedDegenerateTriangles,
                resealSkippedDuplicateVertexIdTriangles,
                resealSkippedZeroEdgeTriangles,
                resealSkippedDuplicatePositionTriangles,
                resealSkippedCollinearTriangles,
                afterDegenerate,
                accepted,
                preserveClosedContract,
                boundaryBefore,
                boundaryAfter,
                unmatchedBefore,
                unmatchedAfter);
        }

        var rebuilt = new MeshBuilder().Build(positions, kept);
        rebuilt.IsComplemented = result.Mesh.IsComplemented;
        afterRemovalDegenerate = DegenerateFaceInspector.CountDegenerateFaces(rebuilt, predicateTelemetry);
        afterDegenerate = afterRemovalDegenerate;
        preserveClosedContract =
            beforeIncidence.BoundaryHalfEdgeCount == 0
            && beforeIncidence.UnmatchedUndirectedEdgeCount == 0;
        var afterIncidence = MeshStitcher.AnalyzeBoundaryIncidence(rebuilt);
        if (preserveClosedContract
            && (afterIncidence.BoundaryHalfEdgeCount > 0
                || afterIncidence.UnmatchedUndirectedEdgeCount > 0))
        {
            var resealStats = AttemptDeterministicBoundaryReseal(rebuilt, weldTolerance);
            resealSkippedDegenerateTriangles = resealStats.LoopDegenerateSkipped;
            resealSkippedDuplicateVertexIdTriangles = resealStats.LoopDegenerateDuplicateVertexIdSkipped;
            resealSkippedZeroEdgeTriangles = resealStats.LoopDegenerateZeroEdgeSkipped;
            resealSkippedDuplicatePositionTriangles = resealStats.LoopDegenerateDuplicatePositionSkipped;
            resealSkippedCollinearTriangles = resealStats.LoopDegenerateCollinearSkipped;
            afterIncidence = MeshStitcher.AnalyzeBoundaryIncidence(rebuilt);
            afterDegenerate = DegenerateFaceInspector.CountDegenerateFaces(rebuilt, predicateTelemetry);
            resealIntroducedDegenerate = System.Math.Max(0, afterDegenerate - afterRemovalDegenerate);
            resealDegenerateSafe = afterDegenerate <= afterRemovalDegenerate;
        }
        var afterComponents = AnalyzeComponentTopology(rebuilt);
        boundaryAfter = afterIncidence.BoundaryHalfEdgeCount;
        unmatchedAfter = afterIncidence.UnmatchedUndirectedEdgeCount;

        accepted = afterDegenerate <= beforeDegenerate;
        if (preserveClosedContract)
        {
            accepted = accepted
                && resealDegenerateSafe
                && afterIncidence.BoundaryHalfEdgeCount <= beforeIncidence.BoundaryHalfEdgeCount
                && afterIncidence.UnmatchedUndirectedEdgeCount <= beforeIncidence.UnmatchedUndirectedEdgeCount;
        }
        else
        {
            accepted = accepted
                && afterComponents.InvalidComponentCount <= beforeComponents.InvalidComponentCount;
        }

        var output = accepted ? WithMesh(result, rebuilt) : result;
        return new DegeneratePrunePassResult(
            output,
            beforeDegenerate,
            removedDegenerateFaces,
            afterRemovalDegenerate,
            resealIntroducedDegenerate,
            resealDegenerateSafe,
            resealSkippedDegenerateTriangles,
            resealSkippedDuplicateVertexIdTriangles,
            resealSkippedZeroEdgeTriangles,
            resealSkippedDuplicatePositionTriangles,
            resealSkippedCollinearTriangles,
            afterDegenerate,
            accepted,
            preserveClosedContract,
            boundaryBefore,
            boundaryAfter,
            unmatchedBefore,
            unmatchedAfter);
    }

    private const int LocalDegenerateRepairIterationsDefault = 2;
    private const int LocalDegenerateRepairIterationsClosureAttempt = 24;
    private const int LocalRepairPairCandidateBudget = 128;
    private const int LocalRepairTripleCandidateBudget = 192;
    private const int LocalRepairRetriangulationBudget = 96;
    private readonly record struct DegenerateFaceDescriptor(int FaceId, int V0, int V1, int V2);
    private readonly record struct DegenerateClassCounts(
        int Total,
        int DuplicateVertexId,
        int ZeroLengthEdge,
        int DuplicatePosition,
        int Collinear);

    private static CsgResult TryLocalDegenerateNeighborhoodRepair(
        CsgResult result,
        double weldTolerance,
        ReconstructionBoundaryContract? authoritativeBoundary,
        int iterationBudget,
        bool allowPairCandidates,
        out int authorityGate,
        out int beforeDegenerate,
        out int afterDegenerate,
        out int attemptedFaces,
        out int removedFaces,
        out int singleCandidateAttempts,
        out int pairCandidateAttempts,
        out int tripleCandidateAttempts,
        out int multiFaceApplied,
        out int maxRemovedArity,
        out int collinearGuard,
        out int collinearReject,
        out int collinearExactCheck,
        out int collinearExactConfirm,
        out int retriangulationAttempts,
        out int retriangulationApplied,
        out int iterations,
        out int appliedIterations,
        out string terminationReason)
    {
        authorityGate = authoritativeBoundary?.Authority == PatchBoundaryAuthority.Arrangement ? 1 : 0;
        beforeDegenerate = CountDegenerateFacesNoTelemetry(result.Mesh);
        afterDegenerate = beforeDegenerate;
        attemptedFaces = 0;
        removedFaces = 0;
        singleCandidateAttempts = 0;
        pairCandidateAttempts = 0;
        tripleCandidateAttempts = 0;
        multiFaceApplied = 0;
        maxRemovedArity = 0;
        collinearGuard = 0;
        collinearReject = 0;
        collinearExactCheck = 0;
        collinearExactConfirm = 0;
        retriangulationAttempts = 0;
        retriangulationApplied = 0;
        iterations = 0;
        appliedIterations = 0;
        terminationReason = authorityGate == 1 ? "none" : "authority-gated";

        if (authorityGate == 0 || beforeDegenerate == 0)
        {
            if (authorityGate == 1)
                terminationReason = "already-clean";
            return result;
        }

        var current = result;
        int maxIterations = System.Math.Max(1, iterationBudget);
        for (int iter = 0; iter < maxIterations; iter++)
        {
            iterations++;
            if (!TryRemoveOneDegenerateFace(
                current.Mesh,
                weldTolerance,
                allowPairCandidates,
                out var repairedMesh,
                out int attemptedThisIteration,
                out int removedFaceArity,
                out int singleTryThisIteration,
                out int pairTryThisIteration,
                out int tripleTryThisIteration,
                out int collinearGuardThisIteration,
                out int collinearRejectThisIteration,
                out int collinearExactCheckThisIteration,
                out int collinearExactConfirmThisIteration,
                out int retriTryThisIteration,
                out int retriAppliedThisIteration,
                out int afterThisIteration))
            {
                attemptedFaces += attemptedThisIteration;
                singleCandidateAttempts += singleTryThisIteration;
                pairCandidateAttempts += pairTryThisIteration;
                tripleCandidateAttempts += tripleTryThisIteration;
                collinearGuard = System.Math.Max(collinearGuard, collinearGuardThisIteration);
                collinearReject += collinearRejectThisIteration;
                collinearExactCheck += collinearExactCheckThisIteration;
                collinearExactConfirm += collinearExactConfirmThisIteration;
                retriangulationAttempts += retriTryThisIteration;
                retriangulationApplied += retriAppliedThisIteration;
                terminationReason = "stalled";
                break;
            }

            attemptedFaces += attemptedThisIteration;
            singleCandidateAttempts += singleTryThisIteration;
            pairCandidateAttempts += pairTryThisIteration;
            tripleCandidateAttempts += tripleTryThisIteration;
            collinearGuard = System.Math.Max(collinearGuard, collinearGuardThisIteration);
            collinearReject += collinearRejectThisIteration;
            collinearExactCheck += collinearExactCheckThisIteration;
            collinearExactConfirm += collinearExactConfirmThisIteration;
            retriangulationAttempts += retriTryThisIteration;
            retriangulationApplied += retriAppliedThisIteration;
            current = WithMesh(current, repairedMesh);
            removedFaces++;
            if (removedFaceArity > maxRemovedArity)
                maxRemovedArity = removedFaceArity;
            if (removedFaceArity > 1)
                multiFaceApplied++;
            appliedIterations++;
            afterDegenerate = afterThisIteration;

            if (afterDegenerate == 0)
            {
                terminationReason = "cleared";
                return current;
            }

            if (appliedIterations >= maxIterations)
            {
                terminationReason = "budget";
                return current;
            }
        }

        if (terminationReason == "none")
            terminationReason = "stalled";

        return current;
    }

    private static bool TryRemoveOneDegenerateFace(
        HalfEdgeMesh mesh,
        double weldTolerance,
        bool allowPairCandidates,
        out HalfEdgeMesh repairedMesh,
        out int attemptedFaces,
        out int removedFaceArity,
        out int singleCandidateAttempts,
        out int pairCandidateAttempts,
        out int tripleCandidateAttempts,
        out int collinearGuard,
        out int collinearReject,
        out int collinearExactCheck,
        out int collinearExactConfirm,
        out int retriangulationAttempts,
        out int retriangulationApplied,
        out int afterDegenerate)
    {
        repairedMesh = mesh;
        attemptedFaces = 0;
        removedFaceArity = 0;
        singleCandidateAttempts = 0;
        pairCandidateAttempts = 0;
        tripleCandidateAttempts = 0;
        collinearGuard = 0;
        int collinearRejectCount = 0;
        int collinearExactCheckCount = 0;
        int collinearExactConfirmCount = 0;
        retriangulationAttempts = 0;
        retriangulationApplied = 0;
        var beforeClasses = CountDegenerateClassesNoTelemetry(mesh, requireExactCollinear: false, out _, out _);
        int beforeDegenerate = beforeClasses.Total;
        afterDegenerate = beforeDegenerate;
        if (beforeDegenerate == 0)
        {
            collinearReject = 0;
            collinearExactCheck = 0;
            collinearExactConfirm = 0;
            return false;
        }
        bool enforceCollinearProgress =
            allowPairCandidates
            && beforeClasses.Collinear == beforeClasses.Total
            && beforeClasses.Total > 0;
        if (enforceCollinearProgress)
        {
            collinearGuard = 1;
            beforeClasses = CountDegenerateClassesNoTelemetry(
                mesh,
                requireExactCollinear: true,
                out int beforeExactChecks,
                out int beforeExactConfirmed);
            collinearExactCheckCount += beforeExactChecks;
            collinearExactConfirmCount += beforeExactConfirmed;
            beforeDegenerate = beforeClasses.Total;
            afterDegenerate = beforeDegenerate;
        }

        var baselineIncidence = MeshStitcher.AnalyzeBoundaryIncidence(mesh);
        var baselineComponents = AnalyzeComponentTopology(mesh);
        bool preserveClosedContract =
            baselineIncidence.BoundaryHalfEdgeCount == 0
            && baselineIncidence.UnmatchedUndirectedEdgeCount == 0;

        var positions = new List<Vec3>(mesh.Vertices.Count);
        foreach (var v in mesh.Vertices)
            positions.Add(v.Position);

        var degenerateFaces = mesh.Faces
            .Where(IsDegenerateFaceNoTelemetry)
            .Select(static face =>
            {
                var verts = face.GetVertices();
                return new DegenerateFaceDescriptor(face.Id, verts[0].Id, verts[1].Id, verts[2].Id);
            })
            .OrderBy(static item => item.FaceId)
            .ToArray();
        if (degenerateFaces.Length == 0)
        {
            collinearReject = 0;
            collinearExactCheck = 0;
            collinearExactConfirm = 0;
            return false;
        }

        bool TryEvaluateTriangles(
            List<(int I0, int I1, int I2)> triangles,
            out HalfEdgeMesh candidateMesh,
            out int candidateDegenerate)
        {
            if (triangles.Count == 0)
            {
                candidateMesh = mesh;
                candidateDegenerate = beforeDegenerate;
                return false;
            }

            candidateMesh = new MeshBuilder(weldTolerance).Build(positions, triangles);
            candidateMesh.IsComplemented = mesh.IsComplemented;
            var candidateClasses = CountDegenerateClassesNoTelemetry(
                candidateMesh,
                requireExactCollinear: enforceCollinearProgress,
                out int candidateExactChecks,
                out int candidateExactConfirmed);
            collinearExactCheckCount += candidateExactChecks;
            collinearExactConfirmCount += candidateExactConfirmed;
            candidateDegenerate = candidateClasses.Total;
            if (candidateDegenerate >= beforeDegenerate)
                return false;
            if (enforceCollinearProgress && candidateClasses.Collinear >= beforeClasses.Collinear)
            {
                collinearRejectCount++;
                return false;
            }

            var candidateIncidence = MeshStitcher.AnalyzeBoundaryIncidence(candidateMesh);
            var candidateComponents = AnalyzeComponentTopology(candidateMesh);
            bool accepted = candidateIncidence.NonManifoldUndirectedEdgeCount <= baselineIncidence.NonManifoldUndirectedEdgeCount
                && candidateComponents.InvalidComponentCount <= baselineComponents.InvalidComponentCount;
            if (preserveClosedContract)
            {
                accepted = accepted
                    && candidateIncidence.BoundaryHalfEdgeCount <= baselineIncidence.BoundaryHalfEdgeCount
                    && candidateIncidence.UnmatchedUndirectedEdgeCount <= baselineIncidence.UnmatchedUndirectedEdgeCount;
            }

            return accepted;
        }

        bool TryCandidate(int[] removeFaceIds, out HalfEdgeMesh candidateMesh, out int candidateDegenerate)
        {
            var triangles = new List<(int I0, int I1, int I2)>(mesh.Faces.Count - removeFaceIds.Length);
            foreach (var face in mesh.Faces)
            {
                bool remove = false;
                for (int i = 0; i < removeFaceIds.Length; i++)
                {
                    if (face.Id == removeFaceIds[i])
                    {
                        remove = true;
                        break;
                    }
                }

                if (remove)
                    continue;

                var verts = face.GetVertices();
                triangles.Add((verts[0].Id, verts[1].Id, verts[2].Id));
            }

            return TryEvaluateTriangles(triangles, out candidateMesh, out candidateDegenerate);
        }

        bool TryRetriangulatedCandidate(
            DegenerateFaceDescriptor faceA,
            DegenerateFaceDescriptor faceB,
            out HalfEdgeMesh candidateMesh,
            out int candidateDegenerate)
        {
            if (!TryExtractSharedEdge(faceA, faceB, out int shared0, out int shared1, out int otherA, out int otherB))
            {
                candidateMesh = mesh;
                candidateDegenerate = beforeDegenerate;
                return false;
            }

            var removeFaceIds = new[] { faceA.FaceId, faceB.FaceId };
            var baseTriangles = new List<(int I0, int I1, int I2)>(mesh.Faces.Count);
            foreach (var face in mesh.Faces)
            {
                if (face.Id == removeFaceIds[0] || face.Id == removeFaceIds[1])
                    continue;

                var verts = face.GetVertices();
                baseTriangles.Add((verts[0].Id, verts[1].Id, verts[2].Id));
            }

            var optionA = new List<(int I0, int I1, int I2)>(baseTriangles.Count + 2);
            optionA.AddRange(baseTriangles);
            optionA.Add((shared0, otherA, otherB));
            optionA.Add((shared1, otherB, otherA));
            if (TryEvaluateTriangles(optionA, out candidateMesh, out candidateDegenerate))
                return true;

            var optionB = new List<(int I0, int I1, int I2)>(baseTriangles.Count + 2);
            optionB.AddRange(baseTriangles);
            optionB.Add((shared1, otherA, otherB));
            optionB.Add((shared0, otherB, otherA));
            if (TryEvaluateTriangles(optionB, out candidateMesh, out candidateDegenerate))
                return true;

            candidateMesh = mesh;
            candidateDegenerate = beforeDegenerate;
            return false;
        }

        foreach (var candidateFace in degenerateFaces)
        {
            attemptedFaces++;
            singleCandidateAttempts++;
            if (!TryCandidate(new[] { candidateFace.FaceId }, out var candidateMesh, out int candidateDegenerate))
                continue;

            repairedMesh = candidateMesh;
            removedFaceArity = 1;
            afterDegenerate = candidateDegenerate;
            collinearReject = collinearRejectCount;
            collinearExactCheck = collinearExactCheckCount;
            collinearExactConfirm = collinearExactConfirmCount;
            return true;
        }

        if (allowPairCandidates && degenerateFaces.Length >= 2)
        {
            int retriBudget = LocalRepairRetriangulationBudget;
            for (int i = 0; i < degenerateFaces.Length - 1; i++)
            {
                for (int j = i + 1; j < degenerateFaces.Length; j++)
                {
                    if (!ShareAnyVertex(degenerateFaces[i], degenerateFaces[j]))
                        continue;

                    attemptedFaces++;
                    retriangulationAttempts++;
                    if (retriangulationAttempts > retriBudget)
                        break;

                    if (!TryRetriangulatedCandidate(
                        degenerateFaces[i],
                        degenerateFaces[j],
                        out var candidateMesh,
                        out int candidateDegenerate))
                    {
                        continue;
                    }

                    repairedMesh = candidateMesh;
                    removedFaceArity = 2;
                    afterDegenerate = candidateDegenerate;
                    retriangulationApplied++;
                    collinearReject = collinearRejectCount;
                    collinearExactCheck = collinearExactCheckCount;
                    collinearExactConfirm = collinearExactConfirmCount;
                    return true;
                }

                if (retriangulationAttempts > retriBudget)
                    break;
            }
        }

        if (!allowPairCandidates || degenerateFaces.Length < 2)
        {
            collinearReject = collinearRejectCount;
            collinearExactCheck = collinearExactCheckCount;
            collinearExactConfirm = collinearExactConfirmCount;
            return false;
        }

        int pairBudget = LocalRepairPairCandidateBudget;
        bool pairBudgetReached = false;
        for (int i = 0; i < degenerateFaces.Length - 1; i++)
        {
            for (int j = i + 1; j < degenerateFaces.Length; j++)
            {
                if (!ShareAnyVertex(degenerateFaces[i], degenerateFaces[j]))
                    continue;

                attemptedFaces++;
                pairCandidateAttempts++;
                if (pairCandidateAttempts > pairBudget)
                {
                    pairBudgetReached = true;
                    break;
                }
                if (!TryCandidate(
                    new[] { degenerateFaces[i].FaceId, degenerateFaces[j].FaceId },
                    out var candidateMesh,
                    out int candidateDegenerate))
                {
                    continue;
                }

                repairedMesh = candidateMesh;
                removedFaceArity = 2;
                afterDegenerate = candidateDegenerate;
                collinearReject = collinearRejectCount;
                collinearExactCheck = collinearExactCheckCount;
                collinearExactConfirm = collinearExactConfirmCount;
                return true;
            }

            if (pairBudgetReached)
                break;
        }

        if (degenerateFaces.Length < 3)
        {
            collinearReject = collinearRejectCount;
            collinearExactCheck = collinearExactCheckCount;
            collinearExactConfirm = collinearExactConfirmCount;
            return false;
        }

        int tripleBudget = LocalRepairTripleCandidateBudget;
        for (int i = 0; i < degenerateFaces.Length - 2; i++)
        {
            for (int j = i + 1; j < degenerateFaces.Length - 1; j++)
            {
                for (int k = j + 1; k < degenerateFaces.Length; k++)
                {
                    if (!AreVertexConnectedTriplet(degenerateFaces[i], degenerateFaces[j], degenerateFaces[k]))
                        continue;

                    attemptedFaces++;
                    tripleCandidateAttempts++;
                    if (tripleCandidateAttempts > tripleBudget)
                    {
                        collinearReject = collinearRejectCount;
                        collinearExactCheck = collinearExactCheckCount;
                        collinearExactConfirm = collinearExactConfirmCount;
                        return false;
                    }
                    if (!TryCandidate(
                        new[] { degenerateFaces[i].FaceId, degenerateFaces[j].FaceId, degenerateFaces[k].FaceId },
                        out var candidateMesh,
                        out int candidateDegenerate))
                    {
                        continue;
                    }

                    repairedMesh = candidateMesh;
                    removedFaceArity = 3;
                    afterDegenerate = candidateDegenerate;
                    collinearReject = collinearRejectCount;
                    collinearExactCheck = collinearExactCheckCount;
                    collinearExactConfirm = collinearExactConfirmCount;
                    return true;
                }
            }
        }

        collinearReject = collinearRejectCount;
        collinearExactCheck = collinearExactCheckCount;
        collinearExactConfirm = collinearExactConfirmCount;
        return false;
    }

    private static bool ShareAnyVertex(DegenerateFaceDescriptor a, DegenerateFaceDescriptor b)
        => a.V0 == b.V0
            || a.V0 == b.V1
            || a.V0 == b.V2
            || a.V1 == b.V0
            || a.V1 == b.V1
            || a.V1 == b.V2
            || a.V2 == b.V0
            || a.V2 == b.V1
            || a.V2 == b.V2;

    private static bool AreVertexConnectedTriplet(
        DegenerateFaceDescriptor a,
        DegenerateFaceDescriptor b,
        DegenerateFaceDescriptor c)
    {
        bool ab = ShareAnyVertex(a, b);
        bool ac = ShareAnyVertex(a, c);
        bool bc = ShareAnyVertex(b, c);
        int pairCount = (ab ? 1 : 0) + (ac ? 1 : 0) + (bc ? 1 : 0);
        if (pairCount < 2)
            return false;

        bool nodeA = ab || ac;
        bool nodeB = ab || bc;
        bool nodeC = ac || bc;
        return nodeA && nodeB && nodeC;
    }

    private static bool TryExtractSharedEdge(
        DegenerateFaceDescriptor a,
        DegenerateFaceDescriptor b,
        out int shared0,
        out int shared1,
        out int otherA,
        out int otherB)
    {
        shared0 = -1;
        shared1 = -1;
        otherA = -1;
        otherB = -1;

        Span<int> av = stackalloc int[3] { a.V0, a.V1, a.V2 };
        Span<int> bv = stackalloc int[3] { b.V0, b.V1, b.V2 };
        int sharedCount = 0;
        foreach (int va in av)
        {
            bool has = false;
            foreach (int vb in bv)
            {
                if (va == vb)
                {
                    has = true;
                    break;
                }
            }

            if (has)
            {
                if (sharedCount == 0)
                    shared0 = va;
                else if (sharedCount == 1)
                    shared1 = va;
                sharedCount++;
            }
            else
            {
                otherA = va;
            }
        }

        foreach (int vb in bv)
        {
            if (vb != shared0 && vb != shared1)
            {
                otherB = vb;
                break;
            }
        }

        return sharedCount == 2
            && otherA >= 0
            && otherB >= 0
            && otherA != otherB;
    }

    private static int CountDegenerateFacesNoTelemetry(HalfEdgeMesh mesh)
    {
        return CountDegenerateClassesNoTelemetry(mesh, requireExactCollinear: false, out _, out _).Total;
    }

    private static DegenerateClassCounts CountDegenerateClassesNoTelemetry(
        HalfEdgeMesh mesh,
        bool requireExactCollinear,
        out int exactCheckCount,
        out int exactConfirmedCount)
    {
        exactCheckCount = 0;
        exactConfirmedCount = 0;
        int total = 0;
        int duplicateVertexId = 0;
        int zeroLengthEdge = 0;
        int duplicatePosition = 0;
        int collinear = 0;
        foreach (var face in mesh.Faces)
        {
            var verts = face.GetVertices();
            var areaSign = EvaluateProjectedAreaSign(
                verts[0].Position,
                verts[1].Position,
                verts[2].Position);
            if (areaSign.Sign != PredicateSign.Zero)
                continue;
            if (requireExactCollinear)
            {
                exactCheckCount++;
                if (!IsDegenerateTriangleExact3D(verts[0].Position, verts[1].Position, verts[2].Position))
                    continue;
                exactConfirmedCount++;
            }

            total++;
            switch (ClassifyResidualDegenerateFace(verts[0], verts[1], verts[2]))
            {
                case ResidualDegenerateClass.DuplicateVertexId:
                    duplicateVertexId++;
                    break;
                case ResidualDegenerateClass.ZeroLengthEdge:
                    zeroLengthEdge++;
                    break;
                case ResidualDegenerateClass.DuplicatePosition:
                    duplicatePosition++;
                    break;
                default:
                    collinear++;
                    break;
            }
        }

        return new DegenerateClassCounts(
            total,
            duplicateVertexId,
            zeroLengthEdge,
            duplicatePosition,
            collinear);
    }

    private static bool IsDegenerateTriangleExact3D(Vec3 a, Vec3 b, Vec3 c)
    {
        var bax = Rational.FromDouble(b.X - a.X);
        var bay = Rational.FromDouble(b.Y - a.Y);
        var baz = Rational.FromDouble(b.Z - a.Z);
        var cax = Rational.FromDouble(c.X - a.X);
        var cay = Rational.FromDouble(c.Y - a.Y);
        var caz = Rational.FromDouble(c.Z - a.Z);

        var nx = bay * caz - baz * cay;
        var ny = baz * cax - bax * caz;
        var nz = bax * cay - bay * cax;
        return nx.Sign == 0 && ny.Sign == 0 && nz.Sign == 0;
    }

    private static bool IsDegenerateFaceNoTelemetry(Face face)
    {
        var verts = face.GetVertices();
        var areaSign = EvaluateProjectedAreaSign(
            verts[0].Position,
            verts[1].Position,
            verts[2].Position);
        return areaSign.Sign == PredicateSign.Zero;
    }

    private readonly record struct BoundaryResealStats(
        int LoopDegenerateSkipped,
        int LoopDegenerateDuplicateVertexIdSkipped,
        int LoopDegenerateZeroEdgeSkipped,
        int LoopDegenerateDuplicatePositionSkipped,
        int LoopDegenerateCollinearSkipped);

    private static BoundaryResealStats AttemptDeterministicBoundaryReseal(HalfEdgeMesh mesh, double weldTolerance)
    {
        int loopDegenerateSkipped = 0;
        int loopDegenerateDuplicateVertexIdSkipped = 0;
        int loopDegenerateZeroEdgeSkipped = 0;
        int loopDegenerateDuplicatePositionSkipped = 0;
        int loopDegenerateCollinearSkipped = 0;
        var bounds = mesh.GetBounds();
        double sceneScale = System.Math.Max(1.0, bounds.Size.Length);
        double tol = System.Math.Max(
            System.Math.Max(weldTolerance * 4.0, sceneScale * 1e-8),
            1e-6);

        MeshStitcher.RepairBoundary(mesh, tol);
        MeshStitcher.RelinkBoundaryTwinsDeterministic(mesh);
        var incidence = MeshStitcher.AnalyzeBoundaryIncidence(mesh);
        if (incidence.BoundaryHalfEdgeCount > 0 && incidence.OpenBoundaryVertexCount == 0)
        {
            var summary = MeshStitcher.CloseBoundaryLoopsDeterministic(mesh, skipDegenerateFillTriangles: true);
            loopDegenerateSkipped += summary.SkippedDegenerateTriangleCount;
            loopDegenerateDuplicateVertexIdSkipped += summary.SkippedDuplicateVertexIdCount;
            loopDegenerateZeroEdgeSkipped += summary.SkippedZeroEdgeCount;
            loopDegenerateDuplicatePositionSkipped += summary.SkippedDuplicatePositionCount;
            loopDegenerateCollinearSkipped += summary.SkippedCollinearCount;
            MeshStitcher.RepairBoundary(mesh, tol * 2.0);
            MeshStitcher.RelinkBoundaryTwinsDeterministic(mesh);
        }

        return new BoundaryResealStats(
            loopDegenerateSkipped,
            loopDegenerateDuplicateVertexIdSkipped,
            loopDegenerateZeroEdgeSkipped,
            loopDegenerateDuplicatePositionSkipped,
            loopDegenerateCollinearSkipped);
    }

    private static CsgResult PruneDuplicateOutputFaces(
        CsgResult result,
        double weldTolerance,
        out int removedFaceCount)
    {
        removedFaceCount = 0;
        var mesh = result.Mesh;
        if (mesh.Faces.Count < 2)
            return result;

        var removeFaces = new HashSet<int>();
        var idSeen = new HashSet<(int A, int B, int C)>();
        double quantizeTol = System.Math.Max(weldTolerance, 1e-9);
        var geomGroups = new Dictionary<QuantizedTriangleKey, List<(int FaceIndex, int OrientationSign)>>();

        for (int i = 0; i < mesh.Faces.Count; i++)
        {
            var verts = mesh.Faces[i].GetVertices();
            int i0 = verts[0].Id;
            int i1 = verts[1].Id;
            int i2 = verts[2].Id;

            var idKey = SortTriangleIds(i0, i1, i2);
            if (!idSeen.Add(idKey))
                removeFaces.Add(i);

            var q0 = QuantizePoint(verts[0].Position, quantizeTol);
            var q1 = QuantizePoint(verts[1].Position, quantizeTol);
            var q2 = QuantizePoint(verts[2].Position, quantizeTol);
            if (q0.Equals(q1) || q1.Equals(q2) || q2.Equals(q0))
            {
                removeFaces.Add(i);
                continue;
            }

            var (k0, k1, k2) = SortQuantizedPoints(q0, q1, q2);
            int p0 = FindSortedPointIndex(q0, k0, k1);
            int p1 = FindSortedPointIndex(q1, k0, k1);
            int p2 = FindSortedPointIndex(q2, k0, k1);
            int inv = 0;
            if (p0 > p1) inv++;
            if (p0 > p2) inv++;
            if (p1 > p2) inv++;
            int orientationSign = (inv & 1) == 0 ? 1 : -1;

            var geomKey = new QuantizedTriangleKey(k0, k1, k2);
            if (!geomGroups.TryGetValue(geomKey, out var faces))
            {
                faces = [];
                geomGroups[geomKey] = faces;
            }

            faces.Add((i, orientationSign));
        }

        foreach (var kvp in geomGroups)
        {
            var grouped = kvp.Value
                .OrderBy(static entry => entry.FaceIndex)
                .ToList();
            if (grouped.Count <= 1)
                continue;

            var pos = grouped.Where(static entry => entry.OrientationSign > 0)
                .Select(static entry => entry.FaceIndex)
                .ToList();
            var neg = grouped.Where(static entry => entry.OrientationSign < 0)
                .Select(static entry => entry.FaceIndex)
                .ToList();

            int cancelPairs = System.Math.Min(pos.Count, neg.Count);
            for (int i = 0; i < cancelPairs; i++)
            {
                removeFaces.Add(pos[i]);
                removeFaces.Add(neg[i]);
            }

            var remaining = pos.Skip(cancelPairs)
                .Concat(neg.Skip(cancelPairs))
                .OrderBy(static idx => idx)
                .ToList();
            for (int i = 1; i < remaining.Count; i++)
                removeFaces.Add(remaining[i]);
        }

        removedFaceCount = removeFaces.Count;
        if (removedFaceCount == 0)
            return result;

        var positions = new List<Vec3>(mesh.Vertices.Count);
        foreach (var v in mesh.Vertices)
            positions.Add(v.Position);

        var kept = new List<(int I0, int I1, int I2)>(mesh.Faces.Count - removedFaceCount);
        for (int i = 0; i < mesh.Faces.Count; i++)
        {
            if (removeFaces.Contains(i))
                continue;
            var verts = mesh.Faces[i].GetVertices();
            kept.Add((verts[0].Id, verts[1].Id, verts[2].Id));
        }

        var rebuilt = new MeshBuilder(weldTolerance).Build(positions, kept);
        rebuilt.IsComplemented = mesh.IsComplemented;
        var before = MeshStitcher.AnalyzeBoundaryIncidence(mesh);
        var after = MeshStitcher.AnalyzeBoundaryIncidence(rebuilt);
        var beforeComponents = AnalyzeComponentTopology(mesh);
        var afterComponents = AnalyzeComponentTopology(rebuilt);
        bool accepted =
            after.BoundaryHalfEdgeCount <= before.BoundaryHalfEdgeCount
            && after.UnmatchedUndirectedEdgeCount <= before.UnmatchedUndirectedEdgeCount
            && after.NonManifoldUndirectedEdgeCount < before.NonManifoldUndirectedEdgeCount
            && afterComponents.InvalidComponentCount <= beforeComponents.InvalidComponentCount;

        if (!accepted)
        {
            removedFaceCount = 0;
            return result;
        }

        return WithMesh(result, rebuilt);
    }

    private readonly record struct QuantizedPoint(long X, long Y, long Z);

    private readonly record struct QuantizedTriangleKey(
        QuantizedPoint A,
        QuantizedPoint B,
        QuantizedPoint C);

    private static (int A, int B, int C) SortTriangleIds(int i0, int i1, int i2)
    {
        int a = i0;
        int b = i1;
        int c = i2;
        if (a > b)
            (a, b) = (b, a);
        if (b > c)
            (b, c) = (c, b);
        if (a > b)
            (a, b) = (b, a);
        return (a, b, c);
    }

    private static QuantizedPoint QuantizePoint(Vec3 p, double tolerance)
    {
        double inv = 1.0 / tolerance;
        return new QuantizedPoint(
            QuantizeCoord(p.X, inv),
            QuantizeCoord(p.Y, inv),
            QuantizeCoord(p.Z, inv));

        static long QuantizeCoord(double value, double invTol)
        {
            return (long)System.Math.Round(value * invTol, MidpointRounding.AwayFromZero);
        }
    }

    private static (QuantizedPoint A, QuantizedPoint B, QuantizedPoint C) SortQuantizedPoints(
        QuantizedPoint p0,
        QuantizedPoint p1,
        QuantizedPoint p2)
    {
        var points = new[] { p0, p1, p2 };
        Array.Sort(points, static (left, right) =>
        {
            int cmp = left.X.CompareTo(right.X);
            if (cmp != 0)
                return cmp;
            cmp = left.Y.CompareTo(right.Y);
            if (cmp != 0)
                return cmp;
            return left.Z.CompareTo(right.Z);
        });

        return (points[0], points[1], points[2]);
    }

    private static int FindSortedPointIndex(
        QuantizedPoint point,
        QuantizedPoint k0,
        QuantizedPoint k1)
    {
        if (point.Equals(k0))
            return 0;
        if (point.Equals(k1))
            return 1;
        return 2;
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
        ReconstructionBoundaryContract? authoritativeBoundary,
        out int droppedComponentCount,
        out int arrangementSnapCount,
        out int arrangementEdgeSnapCount,
        out int loopAssemblyOpenChainCount,
        out int loopAssemblyAmbiguousBranchCount,
        out int loopAssemblySkippedDegenerateCount,
        out int snapCollapseRejectCount,
        out int snapDegenerateRejectCount,
        out double maxSnapDistance,
        out bool incidencePreserved)
    {
        droppedComponentCount = 0;
        arrangementSnapCount = 0;
        arrangementEdgeSnapCount = 0;
        loopAssemblyOpenChainCount = 0;
        loopAssemblyAmbiguousBranchCount = 0;
        loopAssemblySkippedDegenerateCount = 0;
        snapCollapseRejectCount = 0;
        snapDegenerateRejectCount = 0;
        maxSnapDistance = 0.0;
        incidencePreserved = true;
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

            var passIncidence = MeshStitcher.AnalyzeBoundaryIncidence(mesh);
            if (passIncidence.BoundaryHalfEdgeCount == 0
                && passIncidence.UnmatchedUndirectedEdgeCount == 0)
            {
                if (TryReduceNonManifoldEdgeIncidence(
                    mesh,
                    weldTolerance,
                    out var reducedMesh,
                    out int removedFaces))
                {
                    mesh = reducedMesh;
                    result = WithMesh(result, mesh);
                }

                if (TryPruneInvalidComponents(mesh, weldTolerance, out var closedPassPruned, out int closedPassDropped))
                {
                    droppedComponentCount += closedPassDropped;
                    mesh = closedPassPruned;
                    result = WithMesh(result, mesh);
                }

                continue;
            }

            double tol = baseTolerance * (1 << pass);
            // Authority contract is emitted for reconstruction semantics, but while
            // strict-step3 remains unresolved we keep arrangement-guided snapping
            // enabled for all extraction authorities to preserve pinned blocker shape.
            if (arrangement != null && arrangement.Vertices.Count > 0)
            {
                var preIncidence = MeshStitcher.AnalyzeBoundaryIncidence(mesh);
                var vertexSnap = SnapBoundaryVerticesToArrangement(mesh, arrangement, tol);
                var edgeSnap = SnapBoundaryVerticesToArrangementEdges(mesh, arrangement, tol);
                arrangementSnapCount += vertexSnap.SnappedCount;
                arrangementEdgeSnapCount += edgeSnap.SnappedCount;
                snapCollapseRejectCount += vertexSnap.CollapseRejectCount + edgeSnap.CollapseRejectCount;
                snapDegenerateRejectCount += vertexSnap.IncidentDegenerateRejectCount + edgeSnap.IncidentDegenerateRejectCount;
                maxSnapDistance = System.Math.Max(
                    maxSnapDistance,
                    System.Math.Max(vertexSnap.MaxSnapDistance, edgeSnap.MaxSnapDistance));
                var postSnapIncidence = MeshStitcher.AnalyzeBoundaryIncidence(mesh);
                incidencePreserved &=
                    postSnapIncidence.UnmatchedUndirectedEdgeCount <= preIncidence.UnmatchedUndirectedEdgeCount
                    && postSnapIncidence.NonManifoldUndirectedEdgeCount <= preIncidence.NonManifoldUndirectedEdgeCount;
            }
            MeshStitcher.RepairBoundary(mesh, tol);
            MeshStitcher.RelinkBoundaryTwinsDeterministic(mesh);

            var incidence = MeshStitcher.AnalyzeBoundaryIncidence(mesh);
            if (incidence.BoundaryHalfEdgeCount > 0 && incidence.OpenBoundaryVertexCount == 0)
            {
                var loopAssembly = MeshStitcher.CloseBoundaryLoopsDeterministic(mesh);
                loopAssemblyOpenChainCount += loopAssembly.OpenChainCount;
                loopAssemblyAmbiguousBranchCount += loopAssembly.AmbiguousBranchVertexCount;
                loopAssemblySkippedDegenerateCount += loopAssembly.SkippedDegenerateTriangleCount;
            }

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

    private static bool TryReduceNonManifoldEdgeIncidence(
        HalfEdgeMesh mesh,
        double weldTolerance,
        out HalfEdgeMesh reduced,
        out int removedFaceCount)
    {
        reduced = mesh;
        removedFaceCount = 0;
        if (mesh.Faces.Count < 4)
            return false;

        var before = MeshStitcher.AnalyzeBoundaryIncidence(mesh);
        if (before.NonManifoldUndirectedEdgeCount == 0)
            return false;

        var faceAreas = new double[mesh.Faces.Count];
        var edgeUse = new Dictionary<long, List<EdgeFaceIncidence>>(mesh.Faces.Count * 3);
        for (int fi = 0; fi < mesh.Faces.Count; fi++)
        {
            var verts = mesh.Faces[fi].GetVertices();
            faceAreas[fi] = TriangleAreaSquared(verts[0].Position, verts[1].Position, verts[2].Position);
            AddIncidence(verts[0].Id, verts[1].Id, fi);
            AddIncidence(verts[1].Id, verts[2].Id, fi);
            AddIncidence(verts[2].Id, verts[0].Id, fi);
        }

        var removeFaces = new HashSet<int>();
        foreach (var kvp in edgeUse)
        {
            var incidences = kvp.Value;
            if (incidences.Count <= 2)
                continue;

            var pos = incidences
                .Where(static x => x.DirectionSign > 0)
                .OrderByDescending(x => faceAreas[x.FaceIndex])
                .ThenBy(static x => x.FaceIndex)
                .ToList();
            var neg = incidences
                .Where(static x => x.DirectionSign < 0)
                .OrderByDescending(x => faceAreas[x.FaceIndex])
                .ThenBy(static x => x.FaceIndex)
                .ToList();

            var keep = new HashSet<int>();
            if (pos.Count > 0 && neg.Count > 0)
            {
                keep.Add(pos[0].FaceIndex);
                keep.Add(neg[0].FaceIndex);
            }
            else
            {
                foreach (var incidence in incidences
                    .OrderByDescending(x => faceAreas[x.FaceIndex])
                    .ThenBy(static x => x.FaceIndex)
                    .Take(2))
                {
                    keep.Add(incidence.FaceIndex);
                }
            }

            foreach (var incidence in incidences)
            {
                if (!keep.Contains(incidence.FaceIndex))
                    removeFaces.Add(incidence.FaceIndex);
            }
        }

        if (removeFaces.Count == 0)
            return false;

        var positions = new List<Vec3>(mesh.Vertices.Count);
        foreach (var v in mesh.Vertices)
            positions.Add(v.Position);

        var triangles = new List<(int I0, int I1, int I2)>(mesh.Faces.Count - removeFaces.Count);
        for (int fi = 0; fi < mesh.Faces.Count; fi++)
        {
            if (removeFaces.Contains(fi))
                continue;

            var verts = mesh.Faces[fi].GetVertices();
            triangles.Add((verts[0].Id, verts[1].Id, verts[2].Id));
        }

        if (triangles.Count == 0)
            return false;

        var rebuilt = new MeshBuilder(weldTolerance).Build(positions, triangles);
        rebuilt.IsComplemented = mesh.IsComplemented;
        var after = MeshStitcher.AnalyzeBoundaryIncidence(rebuilt);
        bool removedTooManyFaces = removeFaces.Count > mesh.Faces.Count / 2;
        bool boundaryExplosion = after.BoundaryHalfEdgeCount > (before.NonManifoldUndirectedEdgeCount * 2);
        bool unmatchedExplosion = after.UnmatchedUndirectedEdgeCount > before.NonManifoldUndirectedEdgeCount;
        if (after.NonManifoldUndirectedEdgeCount >= before.NonManifoldUndirectedEdgeCount
            || removedTooManyFaces
            || boundaryExplosion
            || unmatchedExplosion)
        {
            return false;
        }

        reduced = rebuilt;
        removedFaceCount = removeFaces.Count;
        return true;

        void AddIncidence(int a, int b, int faceIndex)
        {
            int lo = a < b ? a : b;
            int hi = a < b ? b : a;
            long key = ((long)lo << 32) | (uint)hi;
            int dir = a < b ? 1 : -1;
            if (!edgeUse.TryGetValue(key, out var list))
            {
                list = [];
                edgeUse[key] = list;
            }

            list.Add(new EdgeFaceIncidence(faceIndex, dir));
        }
    }

    private readonly record struct EdgeFaceIncidence(int FaceIndex, int DirectionSign);

    private static double TriangleAreaSquared(Vec3 a, Vec3 b, Vec3 c)
    {
        var cross = Vec3.Cross(b - a, c - a);
        return cross.LengthSquared;
    }

    private static SnapPassStats SnapBoundaryVerticesToArrangement(
        HalfEdgeMesh mesh,
        ArrangementGraph arrangement,
        double tolerance)
    {
        if (tolerance <= 0 || arrangement.Vertices.Count == 0)
            return SnapPassStats.Empty;

        var boundaryVertices = CollectBoundaryVertices(mesh);
        if (boundaryVertices.Count == 0)
            return SnapPassStats.Empty;

        var arrangementPositions = arrangement.Vertices
            .OrderBy(static v => v.Id)
            .Select(static v => v.Position)
            .ToArray();
        if (arrangementPositions.Length == 0)
            return SnapPassStats.Empty;

        double tolSq = tolerance * tolerance;
        double collapseTolSq = tolSq * 0.04;
        int snapped = 0;
        int collapseReject = 0;
        int incidentDegenerateReject = 0;
        double maxSnapDistance = 0.0;

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

            var candidate = arrangementPositions[bestIndex];
            if (WouldCollapseBoundaryVertex(vertex, candidate, boundaryVertices, collapseTolSq))
            {
                collapseReject++;
                continue;
            }
            if (WouldCreateDegenerateIncidentFace(mesh, vertex, candidate))
            {
                incidentDegenerateReject++;
                continue;
            }

            vertex.Position = candidate;
            snapped++;
            maxSnapDistance = System.Math.Max(maxSnapDistance, System.Math.Sqrt(bestDistSq));
        }

        return new SnapPassStats(
            snapped,
            maxSnapDistance,
            collapseReject,
            incidentDegenerateReject);
    }

    private static SnapPassStats SnapBoundaryVerticesToArrangementEdges(
        HalfEdgeMesh mesh,
        ArrangementGraph arrangement,
        double tolerance)
    {
        if (tolerance <= 0 || arrangement.Edges.Count == 0 || arrangement.Vertices.Count == 0)
            return SnapPassStats.Empty;

        var boundaryVertices = CollectBoundaryVertices(mesh);
        if (boundaryVertices.Count == 0)
            return SnapPassStats.Empty;

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
            return SnapPassStats.Empty;

        double tolSq = tolerance * tolerance;
        double collapseTolSq = tolSq * 0.04;
        int snapped = 0;
        int collapseReject = 0;
        int incidentDegenerateReject = 0;
        double maxSnapDistance = 0.0;

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
                if (WouldCollapseBoundaryVertex(vertex, bestPoint, boundaryVertices, collapseTolSq))
                {
                    collapseReject++;
                    continue;
                }
                if (WouldCreateDegenerateIncidentFace(mesh, vertex, bestPoint))
                {
                    incidentDegenerateReject++;
                    continue;
                }
                vertex.Position = bestPoint;
                snapped++;
                maxSnapDistance = System.Math.Max(maxSnapDistance, System.Math.Sqrt(bestDistSq));
            }
        }

        return new SnapPassStats(
            snapped,
            maxSnapDistance,
            collapseReject,
            incidentDegenerateReject);
    }

    private static bool WouldCollapseBoundaryVertex(
        Vertex movingVertex,
        Vec3 candidatePosition,
        IReadOnlyList<Vertex> boundaryVertices,
        double collapseTolSq)
    {
        if (collapseTolSq <= 0)
            return false;

        for (int i = 0; i < boundaryVertices.Count; i++)
        {
            var other = boundaryVertices[i];
            if (other.Id == movingVertex.Id)
                continue;
            if (Vec3.DistanceSquared(other.Position, candidatePosition) <= collapseTolSq)
                return true;
        }

        return false;
    }

    private static bool WouldCreateDegenerateIncidentFace(
        HalfEdgeMesh mesh,
        Vertex movingVertex,
        Vec3 candidatePosition)
    {
        foreach (var face in mesh.Faces)
        {
            var verts = face.GetVertices();
            bool touches = false;
            Vec3 a = verts[0].Position;
            Vec3 b = verts[1].Position;
            Vec3 c = verts[2].Position;
            if (verts[0].Id == movingVertex.Id)
            {
                a = candidatePosition;
                touches = true;
            }
            if (verts[1].Id == movingVertex.Id)
            {
                b = candidatePosition;
                touches = true;
            }
            if (verts[2].Id == movingVertex.Id)
            {
                c = candidatePosition;
                touches = true;
            }

            if (!touches)
                continue;

            var areaSign = EvaluateProjectedAreaSign(a, b, c);
            if (areaSign.Sign == PredicateSign.Zero)
                return true;
        }

        return false;
    }

    private readonly record struct SnapPassStats(
        int SnappedCount,
        double MaxSnapDistance,
        int CollapseRejectCount,
        int IncidentDegenerateRejectCount)
    {
        public static SnapPassStats Empty => new(0, 0.0, 0, 0);
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
            PatchExtractionCandidateSignatures = source.PatchExtractionCandidateSignatures,
            SelectedCoplanarDecisionRows = source.SelectedCoplanarDecisionRows,
            SelectedCertifiedPatchCount = source.SelectedCertifiedPatchCount,
            SelectedUncertifiedPatchCount = source.SelectedUncertifiedPatchCount,
            SelectedClassificationEvidenceFingerprint = source.SelectedClassificationEvidenceFingerprint,
            AuthoritativeBoundary = source.AuthoritativeBoundary
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

    private static ReconstructionAuthoritySnapshot EvaluateReconstructionAuthoritySnapshot(CsgResult result)
    {
        var boundary = result.AuthoritativeBoundary;
        if (boundary is null)
        {
            return new ReconstructionAuthoritySnapshot(
                Mode: "Unknown",
                Authority: "Unknown",
                Boundary: -1,
                Manifold: -1,
                Components: -1,
                IsValid: false);
        }

        bool valid =
            boundary.BoundaryEdgeCount >= 0
            && boundary.ConnectedComponentCount >= 0;
        return new ReconstructionAuthoritySnapshot(
            Mode: boundary.ExtractionMode.ToString(),
            Authority: boundary.Authority.ToString(),
            Boundary: boundary.BoundaryEdgeCount,
            Manifold: boundary.IsEdgeManifold ? 1 : 0,
            Components: boundary.ConnectedComponentCount,
            IsValid: valid);
    }

    private readonly record struct ReconstructionAuthoritySnapshot(
        string Mode,
        string Authority,
        int Boundary,
        int Manifold,
        int Components,
        bool IsValid);

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
