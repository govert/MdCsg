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
  - reconstruction pass also performs arrangement-edge-guided boundary projection snapping (`arrEdgeSnap`) before repair passes
  - reconstruction certificates now include component validity accounting (`components`, `invalidComponents`) and strict mode emits explicit errors when invalid components remain
  - strict stage telemetry now includes `reconstruction-policy:*` certificates with operation truth-table checks (`fromA`, `fromB`, `flipB`, `pass`)
  - strict stage telemetry now includes `reconstruction-authority:*` certificates from normalized authoritative-boundary contracts
  - reconstruction certificates now include deterministic loop-assembly telemetry (`loopOpenChains`, `loopAmbiguous`) with strict fail-closed surfacing when ambiguity coincides with invalid reconstruction output
  - arrangement snap passes now enforce deterministic anti-collapse checks and emit `snapMax` / `incidencePreserved` reconstruction contract tags
  - reconstruction metrics are computed via deterministic boundary-incidence accounting in `MeshStitcher.AnalyzeBoundaryIncidence`
  - strict bridge path uses a deterministic constructive reconstruction pass (relink, balanced-loop fill, invalid-component pruning)
  - strict bridge path now evaluates intra-face, global, and arrangement-driven patch extraction candidates and deterministically selects the best stitched topology
  - strict bridge path now forces arrangement candidate selection (`ForceArrangementPatchExtraction`) while retaining full candidate telemetry for diagnostics
  - extracted patches now carry deterministic provenance (`StableId`, source-face lineage, boundary authority) with arrangement mode explicitly marked arrangement-authoritative
  - emits stage invariant certificates (`input`, `arrangement`, `patch-extraction`, `triangulation`, `reconstruction`, `output`) and strict-mode `StageInvariantViolation` issues when gates fail
  - strict classification certificates now include deterministic evidence fingerprints with selected certified/uncertified patch counters
  - coplanar decisions are now surfaced as matrix row counters in strict diagnostics (`coplanar-matrix:rows=...`)
  - strict input preflight now enforces explicit `NonManifoldInputPolicy` (`Reject` vs `SanitizeAndContinue`) and emits deterministic `input-policy:*` component accounting tags (`raw`, `valid`, `kept`, `total`)
  - applies deterministic post-op degenerate-face pruning and topology repair before strict output validation
  - degenerate pruning is now bounded fixed-point (`iters`, `applied`, `term`) with decomposed reseal accounting (`afterRemove`, `resealIntro`, `resealSafe`, `resealLoopDegSkipped`) in `deg-prune:*` certificates
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
- `Diagnostics/Legacy` explicit diagnostics-only legacy comparison API (`LegacyComparison`) guarded by `LegacyDiagnosticsOptions.AllowLegacyExecution`
- Checked-in replay corpus fixtures and manifest assertions under `tests/MdCsg.Robust.Conformance/ReplayCorpus/arrangement`
- Checked-in triangulation replay corpus fixtures and manifest assertions under `tests/MdCsg.Robust.Conformance/ReplayCorpus/triangulation`
  - includes dense non-crossing constrained chord cases that assert native success and required-edge preservation
  - bridge conformance also pins a formerly failing `partition/split-failure` signature to native-success behavior
- Checked-in reconstruction replay corpus fixtures and manifest assertions under `tests/MdCsg.Robust.Conformance/ReplayCorpus/reconstruction`
  - includes deterministic chained step-3 blocker replay with pinned `reconstruction-pre`/`reconstruction` certificate expectations
- `Validation/DegenerateFaceInspector` using certified predicate checks
- `RobustOperationOptions.TreatCoplanarIntersectionAsError` guardrail toggle for strict conformance runs
- `RobustOperationOptions.TreatOpposingCoplanarPairsAsError` guardrail toggle for strict conformance runs
- `RobustOperationOptions.TreatOpenArrangementAsError` guardrail toggle for strict conformance runs
- `RobustOperationOptions.UseRobustTriangulationKernel` toggle for bridge-phase kernel routing

Current conformance snapshot:

- `tests/MdCsg.Robust.Conformance` currently runs green in the robustness gate slices (no known skipped gate cases).
- CI rescue bar entrypoint is `tools/ci/run-robustness-gate.ps1` (wired to `.github/workflows/robustness-gate.yml`).
  - gate runner now retries a slice once when test-host crash/abort markers are detected, and fails hard if the retry is not clean
  - gate slices: showcase/backlog/replay (including reconstruction replay), strict fuzz smoke, triangulation+smoke+reconstruction+algebraic+differential+dependency+shadow+readiness+budget guardrails, showcase runtime strict/failover contract (with hang-timeout protection)
  - gate output now emits quality bands: `ROBUST_GATE_BAND_HARD_FAIL`, `ROBUST_GATE_BAND_KNOWN_BLOCKED`, `ROBUST_GATE_BAND_OBSERVABILITY`
  - gate slice 1 explicitly includes the known-blocker deg-prune contract test (`KnownBlockerCorpus_IsExplicitlyFailClosed`) and emits `ROBUST_GATE_DEG_PRUNE_CONTRACT`
- strict readiness snapshot entrypoint is `tools/ci/run-robust-readiness-report.ps1` (reports blocker status + stable-corpus health).
  - readiness output now emits aligned quality bands and derives known blockers from `tools/ci/robust-blocker-ledger.json`
  - readiness now emits `KNOWN_BLOCKER_DETAIL` (`id@stage`) and `READINESS_DEG_PRUNE_CONTRACT`
- deterministic performance budget entrypoint is `tools/ci/run-robust-performance-budget.ps1`.
- Stable-overlap and smoke union zero-fallback checks are active and passing.
- Seeded strict fuzz smoke tests are active and passing with deterministic seeds.
- Stage-focused fuzz escalation (`RobustFuzzEscalationTests`) is active with mutation families (`coplanar-tangency`, `near-collinear-axes`, `thin-shell`), deterministic blocker signatures, and automatic minimized repro emission for unknown failure classes.
- Checked-in minimized fuzz blocker corpus (`ReplayCorpus/fuzz`) pins known blocker signatures (`FUZZ-BLOCKER-*`) and is exercised in gate slice 2.
- Machine-readable blocker ledger conformance (`RobustBlockerLedgerTests`) guards schema, blocker signature coverage, and unknown-signature hard-fail policy.
- Strict API snapshot and migration checklist gates are active (`RobustApiContractSnapshotTests`, `RobustMigrationDocTests`).
- Stable-case differential parity tests are active (`RobustDifferentialParityTests`) and assert robust boundary dominance against legacy plus bounded relative-volume drift for legacy-closed outputs.
- Kernel dependency accretion is guarded by `RobustKernelDependencyGuardTests` to prevent new legacy couplings in `src/MdCsg.Robust/Kernel`.
- Shadow rollout divergence classification is guarded by `RobustShadowRolloutTests`.
- Robust showcase now defaults to robust-strict execution with explicit `--legacy-csg` opt-out; legacy failover is available only with explicit `--allow-legacy-failover`.
- Showcase fallback contract is guarded by `tests/MdCsg.Showcase.ContractTests` (strict throw, explicit failover, and forced-legacy bypass coverage).
- Showcase parity:
  - `CsgOperations` strict mode is closed + zero-fallback,
  - `ChainedCsg` step-2 strict mode is closed + zero-fallback,
  - `ChainedCsg` step-3 strict mode currently has an active fail-closed repro with closed/manifold topology preserved and residual output degenerates (`OutputMeshHasDegenerateFaces`) pinned as the remaining blocker class (`FUZZ-BLOCKER-0001`, owning stage 65),
  - step-3 emits deterministic `patch-extraction-candidates:*` and fixed-point `deg-prune:*` certificates; unsafe prune attempts may be deterministically rejected (`accepted=0`) while preserving fail-closed output guarantees.
