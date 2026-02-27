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

1. Expand coplanar tie-break policy from guardrail detection into explicit reconstruction semantics.
2. Continue replacing constrained triangulation internals under `Kernel/Triangulation` (expand native coverage and retire legacy fallback cases).
3. Burn down the remaining `ChainedCsg` step fallback signatures (currently concentrated in a small partition-fallback class) to restore zero-fallback parity for that path.
4. Add replay corpus files for the remaining showcase artifact/fallback signatures and run them in CI conformance.

## 6. Risks and Mitigations

- Risk: complexity growth in exact fallback paths.
  - Mitigation: isolate exact logic behind small kernel interfaces.
- Risk: performance regression.
  - Mitigation: make fallback targeted and measurable; keep fast certified path.
- Risk: ambiguous semantics in boundary cases.
  - Mitigation: lock semantic truth table in conformance tests before implementation.
