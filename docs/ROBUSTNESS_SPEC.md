# ROBUSTNESS_SPEC

Status: Draft v0.1  
Audience: MdCsg maintainers and contributors  
Applies to: next-generation robust Patch-Confident CSG engine (`MdCsg.Robust`, planned)

## 1. Problem Statement

The current implementation demonstrates severe geometric artifacts on realistic interaction meshes.  
This spec defines hard correctness guarantees and conformance criteria for a robust replacement.

This is a correctness-first document. Performance targets are secondary and must never relax correctness gates.

## 2. Scope

In scope:

- Regularized boolean operations on closed triangle meshes: union, intersection, difference.
- Deterministic outputs for identical inputs/options.
- Robust behavior on coplanar, tangent, near-degenerate, and thin-feature interactions.
- Explicit handling of invalid input meshes.

Out of scope (initially):

- Open-surface booleans.
- Non-manifold output modes.
- Exact symbolic surface output (NURBS/BREP).
- Arbitrary precision everywhere in hot paths.

## 3. Semantics

Operations use regularized set semantics:

- `A union B := closure(interior(A ∪ B))`
- `A intersect B := closure(interior(A ∩ B))`
- `A \ B := closure(interior(A \ B))`

Boundary-only contacts (face-face touching, edge-edge kissing, vertex touching) must not create spurious volume.

## 4. Input Contract

Accepted input:

- Triangle meshes with finite coordinates.
- Preferably closed and orientable.

Required behavior:

- If input is valid closed solid: operation must produce valid output per Section 5.
- If input is invalid: return structured diagnostics (`IssueCode`, location/index context, severity), and either:
  - fail fast in strict mode, or
  - run repair/preprocess pipeline in permissive mode.

No silent corruption is allowed.

## 5. Output Guarantees

For successful operations in strict mode:

- No NaN/Inf coordinates.
- Topologically closed: zero boundary edges.
- Edge-manifold (exactly two incident faces per undirected edge).
- Consistent orientation (outward normals for positive solids).
- Deterministic indexing/order under stable options.
- Volume sign and magnitude consistent with operation semantics within numeric verification tolerance.

## 6. Numeric Policy

Correctness decisions (topology-changing branch points) must be certified:

- Use filtered predicates (`double` fast path + exact fallback).
- A decision is accepted only if error bounds prove sign correctness.
- If not provable, escalate (`expansion` / `rational`) until certified.

Examples of certified decisions:

- Orientation tests (`Orient2D`, `Orient3D`).
- In-circle / side-of-plane used by triangulation and classification.
- Segment-triangle and triangle-triangle ordering/tie breaking when topology depends on sign.

Heuristic epsilon-only classification is forbidden in correctness-critical paths.

## 6.1. Implementation Discipline (Non-Negotiable)

Robust geometry work must be fail-closed and certificate-driven. Experimental one-off fixes are not acceptable as a final state.

- Use exact predicates and exact constructions for every topology-changing decision/intersection.
- Enforce topology-first invariants (closedness, manifoldness, consistent orientation) at every stage boundary.
- Constrained triangulation must either:
  - succeed with all constraints satisfied, or
  - return a structured failure reason; partial/silent acceptance is forbidden.
- No silent degradation paths (including hidden fallbacks or budget exhaustion paths without explicit failure signaling).
- Maintain adversarial conformance coverage with staged invariants and deterministic replay seeds for every discovered failure class.

## 7. Patch-Confident Constraints

Patch-confident classification may be used only when:

- Patch connectivity is built from a robust arrangement graph.
- Candidate confident point has certified margin `m`.
- Classification error bound `e` is computed and `m > e` is proven.

If `m <= e`, fallback is required:

- Multi-candidate certified voting and/or
- exact classification predicate path.

Single-centroid-only classification is not sufficient without certification.

## 8. Coplanar and Degenerate Handling

The engine must have explicit rules for:

- coplanar overlapping faces,
- coincident edges,
- tangent contacts,
- near-zero-area fragments,
- repeated/duplicate triangles.

Tie-breaking must be deterministic and global (not local epsilon hacks).
Simulation-of-Simplicity style symbolic perturbation is acceptable if consistently implemented and documented.

## 9. Conformance Test Matrix

The conformance suite must include:

- Primitive-pair truth cases (cube, sphere, cylinder, half-space).
- Coplanar and kissing contact catalog.
- Thin-slab and near-tangent stress cases.
- Randomized fuzz corpus with seed persistence.
- Differential tests against at least one trusted reference kernel.
- Algebraic/property checks:
  - commutativity (`union`, `intersect`),
  - idempotency,
  - absorption,
  - De Morgan relations (within verification tolerance),
  - transform invariance.

## 10. Quality Gates

A candidate robust engine is production-ready only if:

- All conformance tests pass in CI on `net10.0` and `net48` (or documented target subset).
- Strict-mode rescue bar is green in CI:
  - zero legacy triangulation fallback on showcase-parity and replay corpus suites,
  - seeded fuzz smoke completes with zero unresolved correctness failures.
- Fuzz campaign reaches target iteration budget with zero unresolved correctness failures.
- Determinism checks pass across repeated runs.
- Showcase corpus renders without topological artifacts.

## 11. Diagnostics Requirements

Every operation must expose structured diagnostics:

- stage timings,
- stage invariant certificates (`input`, `arrangement`, `triangulation`, `output`),
- escalation counts (double -> expansion -> exact),
- fallback triggers,
- detected degeneracy counters,
- mesh validity results before and after operation.

Diagnostics are mandatory for debugging and regression triage.

## 12. Acceptance Definition

The robust engine is accepted when:

- It satisfies Sections 3-11 for the defined scope.
- Existing non-robust path can remain as legacy/fast mode, but robust mode is default for correctness-critical usage.
