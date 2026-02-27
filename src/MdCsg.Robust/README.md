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
  - applies deterministic post-op degenerate-face pruning and topology repair before strict output validation
- `Kernel/Predicates/CertifiedPredicates` with precision-tier telemetry
- `Kernel/Arrangement/ArrangementBuilder` native BVH+tri-tri arrangement builder
  - deterministic overlap traversal and canonical snapped-segment ordering
  - coplanar pair orientation counters (agreeing vs opposing normals)
- `Kernel/Arrangement/ArrangementAnalyzer` endpoint/component topology signals
- `Kernel/Triangulation/RobustConstrainedTriangulator` with native robust unconstrained triangulation plus staged native constrained handling (face-point-set incremental solver, non-crossing constraint partitioning, constrained-ear path), deterministic output normalization, and legacy fallback for unsupported constrained inputs
  - face-point-set activation is selectively gated to favor dense/face-cutter style inputs while preserving constrained-polygon bridge invariants
- `Diagnostics/Replay` arrangement replay capture/serialize/replay harness
- `Validation/DegenerateFaceInspector` using certified predicate checks
- `RobustOperationOptions.TreatCoplanarIntersectionAsError` guardrail toggle for strict conformance runs
- `RobustOperationOptions.TreatOpposingCoplanarPairsAsError` guardrail toggle for strict conformance runs
- `RobustOperationOptions.TreatOpenArrangementAsError` guardrail toggle for strict conformance runs
- `RobustOperationOptions.UseRobustTriangulationKernel` toggle for bridge-phase kernel routing

Current conformance snapshot:

- `tests/MdCsg.Robust.Conformance` currently runs 37/37 passing (no skipped tests).
- Stable-overlap and smoke union zero-fallback checks are active and passing.
- Showcase parity:
  - `CsgOperations` strict mode is closed + zero-fallback,
  - `ChainedCsg` step-2 strict mode is closed + zero-fallback,
  - `ChainedCsg` step-3 strict mode currently has an active repro asserting topology failure with zero triangulation fallback.
