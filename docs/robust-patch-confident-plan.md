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
- Rescue-bar gate slice 3 now explicitly includes reconstruction-incidence and algebraic conformance suites alongside triangulation/smoke guardrails.

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

## 6. Risks and Mitigations

- Risk: complexity growth in exact fallback paths.
  - Mitigation: isolate exact logic behind small kernel interfaces.
- Risk: performance regression.
  - Mitigation: make fallback targeted and measurable; keep fast certified path.
- Risk: ambiguous semantics in boundary cases.
  - Mitigation: lock semantic truth table in conformance tests before implementation.
