# Strict Robust Migration Guide

This guide defines the Stage 57 migration contract from legacy CSG behavior to strict robust contracts.

## Scope

- Primary target API: `MdCsg.Robust.RobustCsg` (`Union`, `Intersect`, `Difference`).
- Contract surface: `RobustCsgResult`, `RobustDiagnostics`, `RobustIssueCode`, stage certificates.
- Legacy execution remains available only for diagnostics/comparison workflows.

## Behavior Mapping

| Legacy expectation | Strict robust contract |
|---|---|
| Returns a mesh even when topology is open/non-manifold in difficult cases | Fails closed with error issues and stage certificates that explain the violating stage |
| Implicit fallback behavior is opaque | `RobustDiagnostics` explicitly reports triangulation/reconstruction counters and stage certificates |
| Non-manifold/open input behavior is implicit | Explicit `NonManifoldInputPolicy` (`Reject` or `SanitizeAndContinue`) with `input-policy:*` certificate accounting |
| Coplanar branch decisions can be hard to audit | Coplanar matrix row counters are emitted via `coplanar-matrix:*` certificates |
| Patch extraction path selection is implicit | Strict path forces arrangement extraction and emits deterministic candidate/selection certificates |

## Strict Contract Checklist

- Consume success via `RobustCsgResult.Succeeded`.
- Treat `RobustIssueSeverity.Error` as hard-fail for production.
- Require deterministic stage certificates:
  - `input-policy:`
  - `input:`
  - `arrangement:`
  - `patch-extraction:`
  - `triangulation:`
  - `reconstruction-pre:`
  - `reconstruction:`
  - `output:`
- Keep strict options deterministic (`Deterministic=true`) and robust-kernel enabled.

## Legacy Isolation Policy

- Diagnostics-only namespace: `MdCsg.Robust.Diagnostics.Legacy`.
- Entry point: `LegacyComparison` with explicit opt-in flag:
  - `LegacyDiagnosticsOptions.AllowLegacyExecution = true`
- Default behavior throws when legacy execution is not explicitly enabled.
- Do not use diagnostics namespace as a production fallback path.

## Validation and Gates

- API contract snapshot gate: `RobustApiContractSnapshotTests`.
- Migration doc checklist gate: `RobustMigrationDocTests`.
- Blocker-band policy gate: `RobustBlockerLedgerTests`.
