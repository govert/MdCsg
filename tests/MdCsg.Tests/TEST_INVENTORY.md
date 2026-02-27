# Test Inventory

This document tracks the current test-suite shape and latest verified totals.

## Latest Verified Run

- Date: 2026-02-27
- Command: `dotnet test MdCsg.slnx -c Release -v minimal`
- Result: pass

| Project | Target | Passed | Failed | Skipped | Total |
|---|---|---:|---:|---:|---:|
| MdCsg.Tests | net10.0 | 14444 | 0 | 8 | 14452 |
| MdCsg.Tests | net48 | 14409 | 0 | 8 | 14417 |
| MdCsg.Gpu.Tests | net10.0 | 15 | 0 | 0 | 15 |

## Coverage Areas (MdCsg.Tests)

`MdCsg.Tests` covers:

- `Api`
- `Arithmetic`
- `Bvh`
- `Classification`
- `Cutting`
- `Fitting`
- `Integration`
- `Intersection`
- `Math`
- `Mesh`
- `Operations`
- `Patches`
- `Predicates`
- `Properties`
- `Spatial`

The suite includes unit tests, property tests, integration tests, stress tests, and performance-guardrail tests.
