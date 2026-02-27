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
