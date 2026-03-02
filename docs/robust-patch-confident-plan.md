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
- Chained showcase step-3 remains an active fail-closed blocker, now narrowed to residual output degenerates with reconstruction/output topology preserved (`reconstruction:pass`, `output:fail` with `boundary=0`, `manifold=1`, `deg>0`).
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
- Status: Completed (selected-candidate certified/uncertified counters + deterministic classification evidence fingerprints in strict certificates).

## Stage 53 - Coplanar Semantics Completion Matrix

Deliverables:

- Enumerate and implement full operation/source coplanar truth matrix for Union/Intersect/Difference including complements.
- Add matrix-driven conformance tests with explicit expected inclusion/exclusion outcomes.
- Emit coplanar decision certificates referencing matrix row IDs.

Exit criteria:

- Coplanar behavior is fully table-driven and test-covered for strict mode.
- No ad hoc coplanar branch remains in strict reconstruction/classification.
- Status: Completed (matrix row IDs emitted via `coplanar-matrix:*` certificates + deterministic conformance for shared-face and complement-transformed cases).

## Stage 54 - Non-Manifold Input Policy and Fail-Closed Contracts

Deliverables:

- Define explicit strict-mode policy for non-manifold/degenerate inputs (reject vs sanitized-accept paths).
- Add preflight validation certificates for input policy outcomes.
- Add regression corpus entries for known problematic non-manifold inputs.

Exit criteria:

- Input policy is deterministic, documented, and enforced by conformance tests.
- Non-manifold handling no longer relies on implicit downstream behavior.
- Status: Completed (`NonManifoldInputPolicy` strict reject/sanitize preflight path in `LegacyBridgedRobustCsgEngine`; deterministic `input-policy:*` component accounting certificates; conformance coverage in `InputPolicyConformanceTests` for sanitize-success and fail-closed no-valid-component classes).

## Stage 55 - Fuzz Escalation + Delta Minimization Pipeline

Deliverables:

- Expand seeded strict fuzz to include stage-focused mutation families (coplanar tangency, near-collinear constraints, thin shells).
- Add automatic failing-case minimization pipeline and checked-in minimized repro artifacts.
- Classify unresolved fuzz failures into versioned blocker classes with pinned signatures.

Exit criteria:

- New fuzz failures land with minimized deterministic repros before triage closure.
- Gate reports distinguish new failure classes from known blockers.
- Status: Completed (`RobustFuzzEscalationTests` adds stage-focused mutation families + deterministic delta minimization reporting for unknown signatures; checked-in minimized blocker replay corpus under `ReplayCorpus/fuzz`; robustness gate slice 2 now runs smoke + escalation fuzz suites).

## Stage 56 - Robustness Gate Quality Bands and Blocker Ledger

Deliverables:

- Split gate results into `hard-fail`, `known-blocked`, and `observability` bands with explicit policy.
- Add machine-readable blocker ledger (IDs, signatures, expected certificates, owning stage).
- Enforce ledger consistency in CI (unknown blocker signature => fail).

Exit criteria:

- CI output cleanly separates regressions from known unresolved blockers.
- Blocker lifecycle is auditable and deterministic.
- Status: Completed (machine-readable ledger at `tools/ci/robust-blocker-ledger.json`; `RobustBlockerLedgerTests` enforces schema + known-signature coverage + unknown-signature hard-fail policy; gate/readiness scripts now emit explicit hard-fail/known-blocked/observability bands).

## Stage 57 - Strict Path API Finalization and Legacy Isolation

Deliverables:

- Freeze strict robust API semantics for downstream consumers (result contracts, issue codes, certificate shape).
- Isolate legacy/differential APIs behind explicit diagnostic namespaces and flags.
- Add migration doc section mapping old behavior to strict robust contracts.

Exit criteria:

- Strict path is stable and consumable without depending on legacy behavior.
- Legacy path remains available only for diagnostics/comparison workflows.
- Status: Completed (API snapshot gates in `RobustApiContractSnapshotTests`; migration contract + checklist gate in `RobustMigrationDocTests`; diagnostics-only legacy bridge isolated under `MdCsg.Robust.Diagnostics.Legacy` with explicit `AllowLegacyExecution` opt-in).

## Stage 58 - Narrow Step-3 Blocker to Degenerate-Only Fail-Closed

Deliverables:

- Harden deterministic post-op/reconstruction cleanup to avoid step-3 topology regressions while preserving strict fail-closed behavior.
- Re-pin replay manifests, showcase/readiness conformance, and blocker ledger to the narrowed blocker signature.

Exit criteria:

- Step-3 blocker reproduces deterministically with closed/manifold topology preserved and residual degenerate faces as the sole strict failure class.
- Status: Completed (`FUZZ-BLOCKER-0001` signature now pins `issues=OutputMeshHasDegenerateFaces|StageInvariantViolation;boundary=0;openLoops=0;unmatched=0` with `reconstruction:pass;` + `output:fail;` contracts).

## Stage 59 - Lock Degenerate-Repair Baseline Contracts

Deliverables:

- Add explicit blocker assertions for `deg-prune:*` stage certificates in showcase/readiness conformance.
- Pin closed/manifold output topology invariants in blocker tests while unresolved degenerate output remains fail-closed.

Exit criteria:

- Blocker contract fails only for residual degenerates, with deterministic `deg-prune:phase=pre/post` evidence and zero topology regression (`boundary=0`, `manifold=1`) in tests.
- Status: Completed (showcase/readiness blocker tests assert `deg-prune:*` tags + closed/manifold output certificate tags and remain deterministic under replay/gate suites).

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

## 9. Stage Batch (60-69) Planning

Goal:

- Eliminate the remaining chained step-3 strict blocker (`OutputMeshHasDegenerateFaces`) by replacing residual heuristic cleanup with deterministic, certificate-backed degenerate-repair contracts.

## Stage 60 - Degenerate Burn-Down Plan Freeze

Deliverables:

- Check in Stage 60-69 execution plan with deterministic contracts, dependencies, and validation map.
- Declare explicit success condition for blocker closure: strict chained step-3 succeeds with zero output degenerates and zero topology regressions.

Exit criteria:

- Stage 60-69 plan is documented and committed.
- Status: Completed (this plan section).

## Stage 61 - Degenerate Certificate Decomposition

Deliverables:

- Extend strict diagnostics with deterministic degenerate phase accounting (pre-remove, reseal-introduced, post-remove residual).
- Surface degenerate provenance certificate tags so regressions are attributable to exact repair phase.

Exit criteria:

- Conformance can assert deterministic phase-local degenerate accounting on chained step-3 repro.
- Status: Completed (`LegacyBridgedRobustCsgEngine` `deg-prune:*` certificates now emit `afterRemove`, `resealIntro`, and `netRemoved`; conformance assertions added in `RobustShowcaseParityTests` and `RobustReadinessSnapshotTests`).

## Stage 62 - Degenerate-Safe Boundary Reseal

Deliverables:

- Harden deterministic boundary loop fill/relink to reject zero-area triangles before insertion.
- Ensure reseal path cannot silently increase degenerate-face count in strict mode.

Exit criteria:

- Reseal contracts are deterministic and certificate-backed.
- Status: Completed (`deg-prune:*` now emits `resealSafe`/`resealLoopDegSkipped`; strict acceptance now rejects unsafe closed-contract reseal outcomes, and reseal loop fill can skip exact zero-area fan triangles in reseal mode).

## Stage 63 - Fixed-Point Degenerate Cleanup Pass

Deliverables:

- Introduce bounded deterministic fixed-point degenerate cleanup (prune/reseal/recheck loop with strict budget).
- Emit iteration/termination reason in `deg-prune:*` certificates.

Exit criteria:

- Cleanup converges deterministically and never bypasses topology invariants.
- Status: Completed (`PruneDegenerateOutputFaces` now runs bounded fixed-point cleanup with `MaxDegeneratePruneIterations=3`, and `deg-prune:*` certificates now emit deterministic `iters`, `applied`, and `term` tags).

## Stage 64 - Conformance and Replay Contract Expansion

Deliverables:

- Add strict conformance and replay assertions for new degenerate cleanup certificates and convergence contracts.
- Add anti-regression coverage for reseal-generated degenerate classes.

Exit criteria:

- New degenerate contracts are pinned under deterministic tests.
- Status: Completed (`ReconstructionReplayCorpusTests` now pins deterministic `deg-prune:phase=pre/post` certificate equality and schema invariants, including fixed-point `iters/applied/term` contracts and accepted-vs-output deg alignment).

## Stage 65 - Blocker Closure or Reclassification

Deliverables:

- If chained step-3 is resolved, convert former blocker tests from fail-closed expectation to strict success expectation.
- If unresolved, pin narrowed residual signature and owner stage with new certificate evidence.

Exit criteria:

- Blocker state is explicit, deterministic, and reflected in tests + ledger.
- Status: Completed (blocker remains unresolved; ledger updated with stage-65 ownership and explicit `deg-prune:phase=post;` evidence requirement while preserving pinned fail-closed signature class).

## Stage 66 - Robustness Gate Promotion (Degenerate Contracts)

Deliverables:

- Promote new degenerate conformance/replay assertions into robustness gate slices.
- Ensure gate output separates contract regressions from known-blocked classes.

Exit criteria:

- CI hard-fails on degenerate-contract drift or unknown signatures.
- Status: Completed (`run-robustness-gate.ps1` slice 1 now explicitly includes `KnownBlockerCorpus_IsExplicitlyFailClosed` alongside replay/showcase suites, and gate output emits explicit `ROBUST_GATE_DEG_PRUNE_CONTRACT=PASS` observability signal).

## Stage 67 - Readiness/Band Alignment

Deliverables:

- Align readiness report script/tests with post-stage blocker state and certificate schema.
- Keep quality-band semantics (`hard-fail` / `known-blocked` / `observability`) deterministic.

Exit criteria:

- Readiness output and tests agree on blocker/band semantics.
- Status: Completed (`run-robust-readiness-report.ps1` now emits `KNOWN_BLOCKER_DETAIL` with owning-stage mapping and explicit `READINESS_DEG_PRUNE_CONTRACT=PASS`, with known-blocked band aligned to blocker detail output).

## Stage 68 - Docs and Spec Realignment

Deliverables:

- Update `src/MdCsg.Robust/README.md` and `docs/ROBUSTNESS_SPEC.md` for degenerate-repair behavior and closure criteria.
- Remove stale statements that no longer match strict runtime behavior.

Exit criteria:

- Runtime/docs/spec/test expectations are aligned.
- Status: Completed (`src/MdCsg.Robust/README.md` and `docs/ROBUSTNESS_SPEC.md` now document fixed-point `deg-prune:*` contracts, rejected-prune fail-closed semantics, and updated gate/readiness observability signals).

## Stage 69 - Validation Sweep and Stage Sign-Off

Deliverables:

- Run targeted conformance plus robustness gate slices relevant to degenerate contracts.
- Mark stages 61-69 complete with exact evidence in this plan doc.

Exit criteria:

- Stage batch 60-69 has passing validation evidence and is fully documented.
- Status: Completed (validation sweep passed on degenerate-contract-relevant gates: robustness gate slice-1 equivalent filter passed `20` tests in `18m16s`, and `tools/ci/run-robust-readiness-report.ps1` passed `7` tests in `5m03s` with `READINESS_DEG_PRUNE_CONTRACT=PASS`).

## 9.1 Stage Dependencies and Execution Order (60-69)

Dependency chain:

1. Stage 61 -> Stage 62 -> Stage 63
2. Stage 63 -> Stage 64 -> Stage 65
3. Stage 65 -> Stage 66 -> Stage 67
4. Stage 68 runs after Stage 65 contract outcome is known.
5. Stage 69 runs after Stages 66-68.

Execution policy:

- One stage per commit.
- Do not close Stage 65 without matching blocker-ledger + conformance updates.
- Do not close Stage 66+ without targeted script/test evidence.

## 9.2 Validation Map (60-69)

- Stage 61:
  - Add deterministic assertions for decomposed `deg-prune:*` accounting tags.
- Stage 62:
  - Add tests that reseal path does not introduce new zero-area faces in strict mode.
- Stage 63:
  - Assert bounded fixed-point cleanup iteration tags and deterministic termination reason.
- Stage 64:
  - Add replay and showcase tests asserting new cleanup cert schema.
- Stage 65:
  - Update blocker ledger tests and replay manifest expectations to resolved/reclassified outcome.
- Stage 66:
  - Update `tools/ci/run-robustness-gate.ps1` slices for new degenerate contract tests.
- Stage 67:
  - Update readiness script/test expectations and quality-band mapping.
- Stage 68:
  - Update README/spec assertions and migration language.
- Stage 69:
  - Run and record targeted/full gate evidence for the 60-69 batch.

## 9. Risks and Mitigations

- Risk: complexity growth in exact fallback paths.
  - Mitigation: isolate exact logic behind small kernel interfaces.
- Risk: performance regression.
  - Mitigation: make fallback targeted and measurable; keep fast certified path.
- Risk: ambiguous semantics in boundary cases.
  - Mitigation: lock semantic truth table in conformance tests before implementation.

## 10. Stage Batch (70-78) Execution

Goal:

- Unblock `FUZZ-BLOCKER-0001` by removing root-cause residual degenerate output faces without relaxing strict fail-closed contracts.

## Stage 70 - Forensic Freeze for Residual Degenerates

Deliverables:

- Add deterministic residual-degenerate forensic certificates for blocked outputs (count/hash/sample).
- Pin blocker residual diagnostics in conformance so future changes are attributable.

Exit criteria:

- Step-3 blocker emits deterministic `deg-residual:*` evidence aligned with output degenerate count.
- Status: Completed (`deg-residual:v=1` certificate now emitted on degenerate output and asserted in showcase/readiness blocker tests).

## Stage 71 - Residual Degenerate Taxonomy

Deliverables:

- Classify residual degenerate faces into deterministic taxonomy buckets.
- Emit taxonomy counters and signatures in diagnostics certificates.

Exit criteria:

- Blocker residuals are partitioned into explicit bucket counts with deterministic fingerprints.
- Status: Completed (`deg-residual:*` now emits deterministic taxonomy counters (`taxDupVid`, `taxZeroEdge`, `taxDupPos`, `taxCollinear`) plus `taxHash`, and blocker conformance asserts bucket-sum invariants).

## Stage 72 - Source-Stage Prevention Guards

Deliverables:

- Add deterministic face-emission guards for taxonomy classes that can be prevented upstream.
- Ensure prevented classes are rejected before entering reconstruction/output mesh.

Exit criteria:

- Preventable taxonomy classes are blocked at emission sites with certificate evidence.
- Status: Completed (`MeshStitcher.CloseBoundaryLoopsDeterministic` now applies deterministic pre-emission guards (duplicate vertex id, zero-edge, duplicate-position, optional collinear), and `deg-prune:*` certificates now expose class-level reseal reject counters).

## Stage 73 - Local Reconstruction Repair Kernel

Deliverables:

- Add deterministic local repair/retriangulation for residual degenerate neighborhoods.
- Restrict repair to arrangement-authoritative boundaries to avoid semantic drift.

Exit criteria:

- Local repair removes targeted residual classes without topology regression.
- Status: Pending.

## Stage 74 - Snap/Collapse Safety Hardening

Deliverables:

- Strengthen deterministic anti-collapse snap constraints around repaired neighborhoods.
- Emit additional safety counters in reconstruction certificates.

Exit criteria:

- Snap logic cannot introduce known residual degenerate classes under strict mode.
- Status: Pending.

## Stage 75 - Strict Closure Attempt (Blocker Resolution Try)

Deliverables:

- Attempt full strict success on chained step-3 with zero output degenerates.
- Flip conformance expectation to success if resolved; otherwise preserve explicit fail-closed blocker evidence.

Exit criteria:

- Either blocker resolved with strict success contracts, or unresolved state is explicitly re-pinned with updated evidence.
- Status: Pending.

## Stage 76 - Blocker Ledger Migration

Deliverables:

- If resolved, retire `FUZZ-BLOCKER-0001`; if unresolved, split/refine blocker classes by taxonomy where justified.
- Keep ledger/signature/test policy deterministic and auditable.

Exit criteria:

- Ledger matches actual blocker classes and ownership stages.
- Status: Pending.

## Stage 77 - Gate and Readiness Promotion

Deliverables:

- Promote unblock-path assertions into robustness gate/readiness scripts and tests.
- Ensure regressions fail hard and known-blocked output remains explicit.

Exit criteria:

- CI/gate/readiness enforce the updated blocker policy deterministically.
- Status: Pending.

## Stage 78 - Validation Sweep and Batch Sign-Off

Deliverables:

- Run targeted validation sweep for the 70-78 batch.
- Update plan statuses with exact evidence and sign-off state.

Exit criteria:

- Stage batch 70-78 is fully documented with passing validation for delivered contracts.
- Status: Pending.
