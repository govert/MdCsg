# Robust Patch-Confident CSG Plan

Status: Draft v0.2

This plan operationalizes [ROBUSTNESS_SPEC.md](ROBUSTNESS_SPEC.md) into deliverable milestones.

## Initial Scaffold (Completed)

- Added `src/MdCsg.Robust` project with:
  - robust API surface (`RobustCsg`, result/options/diagnostics contracts),
  - `IRobustCsgEngine`,
  - transitional `LegacyBridgedRobustCsgEngine` with strict validation diagnostics,
  - certified predicate layer (`Kernel/Predicates`) and predicate-tier telemetry counters,
  - native arrangement scaffold (`Kernel/Arrangement`) using BVH overlap traversal + tri-tri processing, with coplanar-pair diagnostics and endpoint/component analysis.
  - deterministic arrangement ordering and normalization (sorted overlap traversal, canonicalized snapped segments, stable incident map ordering).
- Added `tests/MdCsg.Robust.Conformance` project with:
  - passing smoke tests for robust API wiring,
  - active strict conformance guardrails for coplanar shared-face, thin-slab chains, tangent/kissing contact, and stable-overlap zero-fallback behavior,
  - showcase-parity conformance coverage for `CsgOperations` and `ChainedCsg` scene pipelines.
- Added both projects to `MdCsg.slnx`.

## 1. Strategy

Build a new engine path in parallel (`MdCsg.Robust`) instead of incrementally patching the legacy pipeline.

Principles:

- Correctness before throughput.
- Certified decisions for all topology-changing branches.
- Deterministic behavior by construction.
- Legacy engine retained for comparison/benchmarking only.

Non-negotiable execution rules:

- Fail-closed behavior: uncertain/over-budget stages must emit structured failure, never silent partial success.
- Exact predicates plus exact constructions on topology-changing operations.
- Stage-by-stage topology invariants (closed/manifold/oriented) are mandatory gates.
- Constrained triangulation must prove all constraints are satisfied or return structured failure.
- Every new failure class must be captured by deterministic replay/conformance tests before closure.

Rescue bar CI gate:

- `tools/ci/run-robustness-gate.ps1` is the canonical robustness gate entrypoint.
- CI must fail if strict-mode conformance regresses on:
  - zero legacy triangulation fallback across showcase/replay suites,
  - unresolved correctness failures in seeded fuzz smoke.
- This gate is wired in `.github/workflows/robustness-gate.yml`.

## 2. Proposed Architecture

Planned package: `src/MdCsg.Robust/`

Planned modules:

- `Kernel/Predicates`: filtered predicates + exact fallback wrappers.
- `Kernel/Arrangement`: robust intersection arrangement graph construction.
- `Kernel/Triangulation`: constrained triangulation with robust constraints.
- `Kernel/Patches`: patch extraction from arrangement with stable IDs.
- `Kernel/Classification`: certified patch-confident classification + fallback path.
- `Kernel/Reconstruction`: deterministic patch selection and mesh stitching.
- `Validation`: mesh/topology invariants and semantic validators.
- `Diagnostics`: stage telemetry and escalation/fallback counters.

## 3. Milestones

## M0 - Spec and Harness

Deliverables:

- Finalize robustness semantics and input/output contracts.
- Create conformance test project (`tests/MdCsg.Robust.Conformance`).
- Build canonical corpus + fuzz scaffolding + differential adapters.

Exit criteria:

- Failing tests that encode target behavior are in place.

## M1 - Certified Predicate Layer

Deliverables:

- Unified predicate API that returns:
  - sign,
  - confidence/certification metadata,
  - escalation statistics.
- Remove direct epsilon-based sign checks in robust path.

Exit criteria:

- Predicate stress suite green with deterministic outcomes.

## M2 - Robust Arrangement Core (In Progress)

Deliverables:

- Robust tri-tri intersection and segment stitching into arrangement graph.
- Deterministic tie-breaking for overlapping/coplanar interactions.

Exit criteria:

- Arrangement invariants pass on adversarial coplanar/tangent corpus.

Current status:

- Native arrangement construction now avoids legacy `IntersectionGraph.Compute` in the mesh-mesh path.
- Deterministic ordering and dedup normalization are enforced before graph construction.
- Coplanar orientation telemetry is tracked (agreeing vs opposing coplanar face-pair normals).
- Conformance tests include repeated-run determinism and stable `parallel` flag behavior checks.
- M3 internals replacement started in `Kernel/Triangulation/RobustConstrainedTriangulator`:
  - unconstrained polygons now use a native robust ear-clipping path based on certified `Orient2D` predicates,
  - constrained input now tries (in order) native face-point-set incremental solving, native non-crossing constraint partitioning, and native constrained-ear handling before falling back to legacy triangulation for unsupported/inconsistent constrained cases.
- Legacy-bridge execution now routes face-cut triangulation through the robust kernel hook (`CsgOptions.TriangulationKernel`) with diagnostics for native vs fallback usage.
- Stable overlap zero-fallback target is now active and passing in conformance; remaining fallback burn-down is tracked via reason buckets and signature samples.
- Fallback diagnostics now include reason buckets and compact signature samples to drive targeted burn-down of remaining legacy fallback cases.
- Legacy-bridge output now applies deterministic post-op degenerate-face pruning and topology repair before strict output validation.
- Native constrained triangulation now prefers a selective face-point-set path for dense/face-cutter style inputs while routing low-complexity polygonal constraints through partition/ear paths to preserve constrained-edge invariants.
- Deterministic replay harness is available for arrangement-stage cases (`Diagnostics/Replay` with capture/serialize/replay + conformance tests).
- Checked-in arrangement replay corpus now lives under `tests/MdCsg.Robust.Conformance/ReplayCorpus/arrangement` with manifest-backed signature assertions.
- Strict robust execution now fail-closes native constrained triangulation failures without implicit legacy triangulation fallback.
- Stage invariant certificates (`input`, `arrangement`, `triangulation`, `output`) are now emitted in diagnostics, with explicit `StageInvariantViolation` issues in strict mode when gates fail.
- Seeded strict fuzz smoke conformance (`RobustFuzzSmokeTests`) now runs in the rescue bar gate and treats only structured fail-closed outcomes as acceptable unresolved-case handling.
- Native triangulation fail-closed telemetry now includes stage-local failure codes (for example `constraint-validation/invalid-or-crossing`, `partition/split-failure`, `face-point-set/work-budget-exceeded`) in diagnostics.
- Native triangulation now supports deterministic constrained-work budget overrides for reproducible fail-closed diagnostics (for example forcing `face-point-set/work-budget-exceeded` paths in conformance).
- Checked-in triangulation replay corpus now lives under `tests/MdCsg.Robust.Conformance/ReplayCorpus/triangulation` with manifest-backed deterministic outcome/failure-code assertions.
- Strict showcase/backlog/smoke conformance assertions now reject both legacy fallback and native triangulation failures, including signature/code regressions.
- Rescue bar script now runs explicit gate slices with hang protection: showcase/backlog/replay, strict fuzz smoke, and triangulation bridge/smoke guardrails.
- Reconstruction-stage telemetry now reports boundary/open-loop/unmatched/non-manifold invariants with dedicated reconstruction certificates and strict-mode reconstruction issue codes.
- Reconstruction invariant analysis is now centralized in deterministic boundary-incidence accounting (`MeshStitcher.AnalyzeBoundaryIncidence`) and covered by deterministic conformance tests.
- Coplanar reconstruction selection is now routed through an explicit per-operation/source truth table in `PatchAssembler` (documented in `ROBUSTNESS_SPEC.md`).
- Robust bridge reconstruction now runs a deterministic constructive topology pass (relink, loop closure on balanced boundaries, invalid-component pruning) with explicit dropped-component telemetry.
- Chained showcase step-3 remains an active reconstruction blocker, but is now pinned as a strict fail-closed repro requiring `reconstruction:fail` certification with zero triangulation fallback.
- Upstream CSG now supports deterministic patch extraction policy control (`Auto`, `IntraFace`, `Global`) and strict-mode topology-preserving arbitration in `Auto` mode.
- CSG operation results now emit selected patch-extraction telemetry (mode, stitched boundary count, manifold flag, connected components), and strict robust diagnostics mirror this as `patch-extraction:*` stage certificates.
- Strict robust bridge now includes arrangement-owned boundary classification as a third patch-extraction candidate (alongside intra-face/global) in deterministic topology-preserving selection.
- Patch classification now records explicit certification telemetry (`classification:pass;certified=...;fallback=...`) and surfaces exact-fallback counts in robust diagnostics.
- Reconstruction now includes arrangement-guided boundary vertex snapping before repair/loop-closure passes, with `arrSnap` telemetry in reconstruction certificates and diagnostics.
- Added strict algebraic/chain conformance coverage for commutativity/idempotency/mixed-chain closure and certificate emission (`classification:*`, `patch-extraction:*`).
- CI rescue gate now includes crash-aware rerun logic for intermittent test-host aborts and conformance determinism checks for patch/classification certificates.
- Patch-extraction diagnostics now emit deterministic candidate signatures (`patch-extraction-candidates:*`) so chained step-3 unresolved topology signatures are explicitly tracked across runs.
- Patch extraction now assigns deterministic stable patch identity/provenance (`StableId`, source-face lineage, and boundary authority), with arrangement mode explicitly marked as arrangement-authoritative.
- Native constrained triangulation now includes a second-chance face-point-set rescue pass (after partition/ear failure) and expanded dense non-crossing chord replay/bridge coverage.
- Reconstruction certificates/diagnostics now include deterministic component-level validity accounting (`components`, `invalidComponents`) with strict error surfacing for invalid reconstructed components.
- Rescue-bar gate slice 3 now explicitly includes reconstruction-incidence, algebraic, and differential parity conformance suites alongside triangulation/smoke guardrails.
- Added explicit conformance coverage for the formerly failing `partition/split-failure` triangulation signature class, now expected to succeed natively with preserved constraint edges.
- Reconstruction now applies arrangement-edge-guided boundary snapping (`arrEdgeSnap`) in addition to arrangement-vertex snapping before boundary repair.
- Strict robust diagnostics now emit reconstruction policy certificates (`reconstruction-policy:*`) with operation-aware truth-table checks on source/flip accounting.
- Added stable-case differential parity conformance (`RobustDifferentialParityTests`) comparing strict robust vs legacy outputs with robust boundary dominance checks and bounded relative volume drift on legacy-closed outputs.
- Chained step-3 blocker conformance now pins candidate signature structure and `reconstruction-pre`/`reconstruction` contract tags in showcase parity tests.
- Patch confident-point candidate evaluation is now deterministically bounded for large patches, reducing strict-step runtime blowups while preserving fail-closed classification.
- Reconstruction stage now emits explicit `reconstruction-pre:*` certificates and enforces a strict non-worsening contract (`nonWorse`) across boundary/unmatched/non-manifold/component-invalid metrics.
- Added strict readiness snapshot coverage (`RobustReadinessSnapshotTests`) and CI report entrypoint (`tools/ci/run-robust-readiness-report.ps1`).
- Added shadow rollout divergence harness (`RobustShadowRolloutTests`) that fails on unclassified robust-vs-legacy divergences in stable corpus.
- Added deterministic performance budget conformance (`RobustPerformanceBudgetTests`) and CI entrypoint (`tools/ci/run-robust-performance-budget.ps1`).
- Added robust-kernel dependency guard coverage (`RobustKernelDependencyGuardTests`) to freeze current allowed legacy couplings in `src/MdCsg.Robust/Kernel`.
- Showcase runtime now defaults to robust-strict CSG with explicit `--legacy-csg` opt-out and explicit `--allow-legacy-failover` fallback opt-in.
- Rescue-bar gate slice 3 now includes reconstruction/algebraic/differential plus dependency/shadow/readiness/performance guardrail suites.
- Rescue-bar gate now also includes reconstruction replay corpus checks (slice 1) and dedicated showcase runtime strict/failover contract tests (slice 4).

## M3 - Robust Triangulation and Patch Extraction

Deliverables:

- Replace ear-clipping behavior with robust constrained triangulation.
- Patch extraction based on arrangement boundaries only.

Exit criteria:

- No holes/non-manifold artifacts in intermediate cut meshes for corpus.

## M4 - Certified Patch-Confident Classification

Deliverables:

- Confident-point candidate generation.
- Certified margin test (`m > e`) and fallback on uncertified patches.
- Deterministic coplanar policy implementation.

Exit criteria:

- Classification disagreements against reference reduced to zero on conformance corpus.

## M5 - Deterministic Reconstruction

Deliverables:

- Patch selection truth table by operation.
- Robust stitching/welding with invariant checks.
- Deterministic output ordering.

Exit criteria:

- Output guarantees (closed, manifold, oriented) pass for corpus.

## M6 - Integration and Rollout

Deliverables:

- New API surface (opt-in robust path first).
- Benchmark + telemetry comparison to legacy engine.
- Migration docs and fallback guidance.

Exit criteria:

- Robust engine passes conformance gates and is ready to become default mode.

## 4. Workstreams

Parallel streams:

- Geometry kernel (predicates, arrangement, triangulation).
- Conformance/data (corpus, fuzzing, differential testing).
- Diagnostics/tooling (mesh inspectors, replay, trace capture).
- API/integration (surface area and migration strategy).

## 5. Immediate Next Tasks

1. Use patch-extraction telemetry on the pinned `ChainedCsg` step-3 repro to quantify which topology signatures remain unresolved after arbitration.
2. Continue replacing constrained triangulation internals under `Kernel/Triangulation` (expand native coverage and retire legacy fallback cases).
3. Move patch extraction in the robust path from legacy mesh-adjacency flood fill toward arrangement-boundary-native extraction with stable patch IDs.
4. Add replay corpus files for remaining step-3 artifact signatures and enforce deterministic conformance expectations in CI.

## 6. Stage Batch (38-42) Execution

## Stage 38 - Robust Kernel API Freeze

Deliverables:

- Freeze internal robust kernel contracts across arrangement, patch extraction, triangulation, classification, and reconstruction.
- Remove transitional coupling points where robust stages leak legacy bridge types.
- Add compile-time ownership boundaries (interfaces/types) that prevent new legacy dependencies from entering strict robust stages.

Exit criteria:

- Robust kernel modules compile and run conformance suites without introducing new legacy references.
- Interface changes require explicit plan/doc updates before merge.
- Status: Completed (`RobustKernelDependencyGuardTests`).

## Stage 39 - Default-Mode Shadow Rollout

Deliverables:

- Add shadow execution mode in conformance/showcase harnesses that runs robust strict and legacy in parallel for the same inputs.
- Produce deterministic differential reports (topology invariants, boundary edges, component counts, and volume deltas).
- Fail CI on unclassified divergences; allow only explicitly approved divergence classes tracked in docs/tests.

Exit criteria:

- Shadow mode runs on gate corpus with deterministic outputs.
- Every divergence is either resolved or mapped to a checked-in, versioned exception class.
- Status: Completed (`RobustShadowRolloutTests`).

## Stage 40 - Performance Floor and Deterministic Budgets

Deliverables:

- Add repeatable performance baselines for robust stages (arrangement, triangulation, reconstruction).
- Define deterministic stage budgets and budget-exceeded fail-closed behavior.
- Add CI trend checks to catch major regressions while preserving correctness-first policy.

Exit criteria:

- Robust gate includes budget assertions with deterministic tolerances.
- Budget overruns surface structured diagnostics and never bypass invariants.
- Status: Completed (`RobustPerformanceBudgetTests`, `tools/ci/run-robust-performance-budget.ps1`).

## Stage 41 - Strict-by-Default Switch Preparation

Deliverables:

- Flip API/harness defaults to robust strict mode with explicit opt-out path for legacy comparison.
- Update docs/showcase/scripts so default user flows reflect robust-first execution.
- Add migration notes for downstream callers that currently rely on legacy-specific behavior.

Exit criteria:

- Default invocation paths use robust strict mode end-to-end.
- Legacy path remains available only behind explicit configuration.
- Status: Completed for showcase/runtime harness (`ShowcaseCsg`, `--legacy-csg` opt-out).

## Stage 42 - Legacy Critical Path Decommission

Deliverables:

- Remove legacy engine from production boolean-operation execution path.
- Retain a reduced legacy reference harness only for differential/regression analysis.
- Finalize rescue acceptance checklist and sign-off artifacts (correctness, determinism, diagnostics, CI gating).

Exit criteria:

- Production path has no runtime dependency on legacy boolean assembly.
- Differential harness remains green and usable as long-term regression protection.
- Status: In progress/partial. Legacy remains as reference/failover where blockers persist; CI readiness and performance/dependency/shadow gates are now active.

## 7. Stage Batch (43-47) Execution

## Stage 43 - Step-3 Blocker Signature Freeze

Deliverables:

- Extend strict showcase parity tests to pin the currently unresolved chained step-3 reconstruction signature.
- Assert reconstruction certificate tags (`boundary`, `openLoops`, `unmatched`) so blocker drift is explicit in CI.

Exit criteria:

- Step-3 remains a deterministic, fail-closed blocker with stable signature expectations.
- Status: Completed (`RobustShowcaseParityTests` certificate/signature pinning).

## Stage 44 - Explicit Legacy Failover Contract

Deliverables:

- Remove automatic legacy fallback from showcase robust mode.
- Add explicit failover flag so fallback is only allowed by operator intent.
- Align docs/runtime logging with strict-default behavior.

Exit criteria:

- Showcase default robust path throws on strict robust failure unless explicit failover is configured.
- Legacy full-mode (`--legacy-csg`) remains available for comparison.
- Status: Completed (`ShowcaseCsg`/`Program` explicit failover contract + conformance/build validation).

## Stage 45 - Showcase Strict-Failure Contract Tests

Deliverables:

- Add deterministic conformance coverage for strict showcase failure behavior and failover opt-in behavior.
- Pin expected failure codes/certificates for blocked showcase steps under strict mode.

Exit criteria:

- CI guards against regression to implicit/automatic showcase fallback.
- Status: Completed (`MdCsg.Showcase.ContractTests` strict/no-failover + explicit failover coverage).

## Stage 46 - Replay Coverage for Step-3 Reconstruction Artifacts

Deliverables:

- Add replay corpus artifacts for the chained step-3 reconstruction blocker signatures.
- Assert deterministic reconstruction-pre/post certificate invariants across replay runs.

Exit criteria:

- Step-3 blocker behavior is reproducible via replay corpus without relying solely on scene harness execution.
- Status: Completed (`ReconstructionReplayCorpusTests` + reconstruction replay manifest/case).

## Stage 47 - Rescue Gate Promotion for Showcase Contract

Deliverables:

- Promote strict showcase failure-contract tests and step-3 replay checks into robustness gate slices.
- Ensure gate failures clearly separate unresolved blocker state vs regression.

Exit criteria:

- Robustness gate fails deterministically on showcase contract regressions with actionable diagnostics.
- Status: Completed (`run-robustness-gate.ps1` slice promotion for reconstruction replay + showcase runtime contract tests).

## 8. Stage Batch (48-57) Execution

## Stage 48 - Reconstruction Boundary Authority Refactor

Deliverables:

- Move boundary ownership used by reconstruction from bridge-era adjacency signals to arrangement-authoritative edge/vertex ownership records.
- Make reconstruction consume a single normalized boundary contract (`authoritativeBoundary`) independent of extraction mode internals.
- Emit explicit reconstruction authority certificates (`reconstruction-authority:*`) in diagnostics.

Exit criteria:

- Reconstruction no longer depends on legacy mesh-adjacency authority in strict mode.
- Diagnostics show deterministic authority certificates for all strict operations.
- Status: Completed (`AuthoritativeBoundary` contract + `reconstruction-authority:*` certificates with deterministic conformance assertions).

## Stage 49 - Deterministic Loop Assembly Kernel

Deliverables:

- Introduce a dedicated deterministic loop assembler for reconstruction boundaries with stable tie-breaks on branch points.
- Replace opportunistic loop stitching with graph-driven Euler-walk assembly and explicit ambiguity failure codes.
- Add conformance checks for loop ordering determinism across repeated runs.

Exit criteria:

- Reconstruction loop assembly is deterministic across seeds/runs/platforms for corpus cases.
- Ambiguous loop assembly paths fail closed with structured diagnostics.
- Status: Completed (`MeshStitcher` deterministic loop-assembly analysis + reconstruction loop telemetry/fail-closed signaling).

## Stage 50 - Certified Weld/Snap Policy Hardening

Deliverables:

- Replace heuristic reconstruction weld/snap acceptance with certified geometric checks (distance + orientation + incidence compatibility).
- Add explicit max-snap and incidence-preservation contracts to reconstruction certificates.
- Add replay-backed tests for over-snap/under-snap regression classes.

Exit criteria:

- Snap/weld decisions are certificate-backed and deterministic.
- No silent topology drift from aggressive snapping in strict mode.
- Status: Completed (deterministic anti-collapse snap checks + `snapMax`/`incidencePreserved` reconstruction certificates).

## Stage 51 - Arrangement-Native Patch Extraction Completion

Deliverables:

- Promote arrangement-native patch extraction from candidate path to primary strict path.
- Restrict bridge-era patch extraction to differential/reference-only execution.
- Add strict conformance that fails if arrangement mode is not selected where required.

Exit criteria:

- Strict robust patch extraction runs arrangement-native for gated corpus operations.
- Bridge-only extraction remains available only outside strict production path.
- Status: Completed (`ForceArrangementPatchExtraction` strict-path selection + conformance assertions on arrangement mode).

## Stage 52 - Certified Patch Classification Closure

Deliverables:

- Remove residual uncertified classification branches in strict mode.
- Require certified margin evidence for each patch keep/drop decision; emit per-patch certification summaries.
- Add conformance for deterministic classification evidence fingerprints.

Exit criteria:

- Strict mode classification contains no silent uncertified accept/reject paths.
- Classification certificates are stable across repeated runs on corpus.
- Status: Pending.

## Stage 53 - Coplanar Semantics Completion Matrix

Deliverables:

- Enumerate and implement full operation/source coplanar truth matrix for Union/Intersect/Difference including complements.
- Add matrix-driven conformance tests with explicit expected inclusion/exclusion outcomes.
- Emit coplanar decision certificates referencing matrix row IDs.

Exit criteria:

- Coplanar behavior is fully table-driven and test-covered for strict mode.
- No ad hoc coplanar branch remains in strict reconstruction/classification.
- Status: Pending.

## Stage 54 - Non-Manifold Input Policy and Fail-Closed Contracts

Deliverables:

- Define explicit strict-mode policy for non-manifold/degenerate inputs (reject vs sanitized-accept paths).
- Add preflight validation certificates for input policy outcomes.
- Add regression corpus entries for known problematic non-manifold inputs.

Exit criteria:

- Input policy is deterministic, documented, and enforced by conformance tests.
- Non-manifold handling no longer relies on implicit downstream behavior.
- Status: Pending.

## Stage 55 - Fuzz Escalation + Delta Minimization Pipeline

Deliverables:

- Expand seeded strict fuzz to include stage-focused mutation families (coplanar tangency, near-collinear constraints, thin shells).
- Add automatic failing-case minimization pipeline and checked-in minimized repro artifacts.
- Classify unresolved fuzz failures into versioned blocker classes with pinned signatures.

Exit criteria:

- New fuzz failures land with minimized deterministic repros before triage closure.
- Gate reports distinguish new failure classes from known blockers.
- Status: Pending.

## Stage 56 - Robustness Gate Quality Bands and Blocker Ledger

Deliverables:

- Split gate results into `hard-fail`, `known-blocked`, and `observability` bands with explicit policy.
- Add machine-readable blocker ledger (IDs, signatures, expected certificates, owning stage).
- Enforce ledger consistency in CI (unknown blocker signature => fail).

Exit criteria:

- CI output cleanly separates regressions from known unresolved blockers.
- Blocker lifecycle is auditable and deterministic.
- Status: Pending.

## Stage 57 - Strict Path API Finalization and Legacy Isolation

Deliverables:

- Freeze strict robust API semantics for downstream consumers (result contracts, issue codes, certificate shape).
- Isolate legacy/differential APIs behind explicit diagnostic namespaces and flags.
- Add migration doc section mapping old behavior to strict robust contracts.

Exit criteria:

- Strict path is stable and consumable without depending on legacy behavior.
- Legacy path remains available only for diagnostics/comparison workflows.
- Status: Pending.

## 8.1 Stage Dependencies and Execution Order

Dependency chain:

1. Stage 48 -> Stage 49 -> Stage 50
2. Stage 48 -> Stage 51 -> Stage 52 -> Stage 53
3. Stage 54 runs in parallel with Stages 50-53, but must complete before Stage 57 sign-off
4. Stage 55 starts after Stage 52 (classification evidence stable) and feeds Stage 56 blocker ledger inputs
5. Stage 56 must complete before Stage 57 API freeze/sign-off

Execution policy:

- Ship one stage per commit.
- No stage marked complete without explicit conformance and gate evidence in commit message/body or linked CI run.
- If a stage reveals a new unresolved class, add blocker signature pinning before any behavior change is merged.

## 8.2 Validation Map (48-57)

- Stage 48:
  - Add `reconstruction-authority:*` certificate assertions in robust conformance.
  - Extend replay checks to require deterministic authority fingerprints.
- Stage 49:
  - Add deterministic loop-order fingerprint tests under reconstruction conformance.
  - Add fail-closed tests for ambiguous branch-point loop assemblies.
- Stage 50:
  - Add replay cases for snap/weld overreach and underreach classes.
  - Assert incidence-preservation tags in `reconstruction:` certificates.
- Stage 51:
  - Add strict test asserting arrangement-native extraction is selected on gated corpus.
  - Add dependency guard to block strict-path reintroduction of legacy extraction.
- Stage 52:
  - Add per-patch classification evidence fingerprint assertions.
  - Assert zero uncertified keep/drop decisions in strict diagnostics.
- Stage 53:
  - Add matrix-driven coplanar conformance corpus (operation x source x complement state).
  - Assert emitted matrix row ID certificates for each coplanar decision.
- Stage 54:
  - Add preflight input-policy tests for representative non-manifold classes.
  - Assert deterministic reject/sanitize contract certificates.
- Stage 55:
  - Add seeded fuzz family expansion and deterministic minimization output checks.
  - Require minimized repro artifact path + signature in failure reports.
- Stage 56:
  - Add blocker ledger schema validation test.
  - Add gate test that unknown blocker signatures fail the run.
- Stage 57:
  - Add API contract snapshot tests (issue codes/certificate prefixes/result shape).
  - Add migration-doc consistency checklist gate.

## 8.3 Completion Evidence Requirements

For each stage (48-57), completion requires:

1. One implementation commit with scope limited to that stage.
2. One validation run showing relevant targeted tests pass.
3. One plan-doc status update from `Pending` to `Completed` with exact artifacts/tests named.
4. No relaxation of strict fail-closed policy or deterministic certificate guarantees.

## 9. Risks and Mitigations

- Risk: complexity growth in exact fallback paths.
  - Mitigation: isolate exact logic behind small kernel interfaces.
- Risk: performance regression.
  - Mitigation: make fallback targeted and measurable; keep fast certified path.
- Risk: ambiguous semantics in boundary cases.
  - Mitigation: lock semantic truth table in conformance tests before implementation.
