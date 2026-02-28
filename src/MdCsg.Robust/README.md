# MdCsg.Robust (Planned)

This directory is reserved for the robustness-first Patch-Confident CSG engine.

Authoritative design docs:

- `docs/ROBUSTNESS_SPEC.md`
- `docs/robust-patch-confident-plan.md`

Planned top-level structure:

- `Kernel/Predicates`
- `Kernel/Arrangement`
- `Kernel/Triangulation`
- `Kernel/Patches`
- `Kernel/Classification`
- `Kernel/Reconstruction`
- `Validation`
- `Diagnostics`

Initial scaffold now exists:

- `MdCsg.Robust.csproj`
- `IRobustCsgEngine`
- `RobustCsg` API wrapper
- `LegacyBridgedRobustCsgEngine` (transitional bridge + validation diagnostics)
  - can route face-cut triangulation through `Kernel/Triangulation/RobustConstrainedTriangulator` via `CsgOptions.TriangulationKernel`
  - emits triangulation telemetry (invocation/native/fallback/dropped-degenerate counts, fallback reason buckets, fallback signature samples)
  - emits classification certification telemetry (certified vs exact-fallback patch counts) via `classification:*` stage certificates and diagnostics fallback counters
  - emits reconstruction telemetry (boundary/open-loop/unmatched/non-manifold counts + reconstruction certificates)
  - reconstruction pass now performs arrangement-guided boundary vertex snapping before boundary repair and records snap counts in diagnostics/certificates
  - reconstruction certificates now include component validity accounting (`components`, `invalidComponents`) and strict mode emits explicit errors when invalid components remain
  - reconstruction metrics are computed via deterministic boundary-incidence accounting in `MeshStitcher.AnalyzeBoundaryIncidence`
  - strict bridge path uses a deterministic constructive reconstruction pass (relink, balanced-loop fill, invalid-component pruning)
  - strict bridge path now evaluates intra-face, global, and arrangement-driven patch extraction candidates and deterministically selects the best stitched topology
  - extracted patches now carry deterministic provenance (`StableId`, source-face lineage, boundary authority) with arrangement mode explicitly marked arrangement-authoritative
  - emits stage invariant certificates (`input`, `arrangement`, `patch-extraction`, `triangulation`, `reconstruction`, `output`) and strict-mode `StageInvariantViolation` issues when gates fail
  - applies deterministic post-op degenerate-face pruning and topology repair before strict output validation
- `Kernel/Predicates/CertifiedPredicates` with precision-tier telemetry
- `Kernel/Arrangement/ArrangementBuilder` native BVH+tri-tri arrangement builder
  - deterministic overlap traversal and canonical snapped-segment ordering
  - coplanar pair orientation counters (agreeing vs opposing normals)
- reconstruction coplanar patch selection follows an explicit source/operation truth table (see `docs/ROBUSTNESS_SPEC.md`)
- `Kernel/Arrangement/ArrangementAnalyzer` endpoint/component topology signals
- `Kernel/Triangulation/RobustConstrainedTriangulator` with native robust unconstrained triangulation plus staged native constrained handling (face-point-set incremental solver, non-crossing constraint partitioning, constrained-ear path), deterministic output normalization, and legacy fallback for unsupported constrained inputs
  - face-point-set activation is selectively gated to favor dense/face-cutter style inputs while preserving constrained-polygon bridge invariants
  - when partition/ear paths cannot satisfy required constraints, constrained triangulation now retries a native face-point-set rescue pass before reporting failure
  - strict robust execution now disables legacy fallback and reports structured native failure reasons (fail closed)
  - constrained work-budget override (`RobustTriangulationOptions.ConstraintWorkBudgetOverride`) is available for deterministic fail-closed repro/diagnostics coverage
- `Diagnostics/Replay` arrangement replay capture/serialize/replay harness
- Checked-in replay corpus fixtures and manifest assertions under `tests/MdCsg.Robust.Conformance/ReplayCorpus/arrangement`
- Checked-in triangulation replay corpus fixtures and manifest assertions under `tests/MdCsg.Robust.Conformance/ReplayCorpus/triangulation`
  - includes dense non-crossing constrained chord cases that assert native success and required-edge preservation
- `Validation/DegenerateFaceInspector` using certified predicate checks
- `RobustOperationOptions.TreatCoplanarIntersectionAsError` guardrail toggle for strict conformance runs
- `RobustOperationOptions.TreatOpposingCoplanarPairsAsError` guardrail toggle for strict conformance runs
- `RobustOperationOptions.TreatOpenArrangementAsError` guardrail toggle for strict conformance runs
- `RobustOperationOptions.UseRobustTriangulationKernel` toggle for bridge-phase kernel routing

Current conformance snapshot:

- `tests/MdCsg.Robust.Conformance` currently runs green in the robustness gate slices (no known skipped gate cases).
- CI rescue bar entrypoint is `tools/ci/run-robustness-gate.ps1` (wired to `.github/workflows/robustness-gate.yml`).
  - gate runner now retries a slice once when test-host crash/abort markers are detected, and fails hard if the retry is not clean
  - gate slices: showcase/backlog/replay, strict fuzz smoke, triangulation+smoke+reconstruction+algebraic guardrails (with hang-timeout protection)
- Stable-overlap and smoke union zero-fallback checks are active and passing.
- Seeded strict fuzz smoke tests are active and passing with deterministic seeds.
- Showcase parity:
  - `CsgOperations` strict mode is closed + zero-fallback,
  - `ChainedCsg` step-2 strict mode is closed + zero-fallback,
  - `ChainedCsg` step-3 strict mode currently has an active fail-closed repro asserting reconstruction/output invariant failure with zero triangulation fallback,
  - step-3 now emits deterministic `patch-extraction-candidates:*` certificates (IntraFace/Global/Arrangement signatures) so unresolved topology signatures are tracked across runs.
