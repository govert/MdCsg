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
- `Kernel/Predicates/CertifiedPredicates` with precision-tier telemetry
- `Kernel/Arrangement/ArrangementBuilder` native BVH+tri-tri arrangement builder
  - deterministic overlap traversal and canonical snapped-segment ordering
- `Kernel/Arrangement/ArrangementAnalyzer` endpoint/component topology signals
- `Kernel/Triangulation/RobustConstrainedTriangulator` bridge with deterministic output normalization
- `Validation/DegenerateFaceInspector` using certified predicate checks
- `RobustOperationOptions.TreatCoplanarIntersectionAsError` guardrail toggle for strict conformance runs
- `RobustOperationOptions.TreatOpenArrangementAsError` guardrail toggle for strict conformance runs
