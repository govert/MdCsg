# MdCsg

A Constructive Solid Geometry library in C# implementing the **Patch-Confident CSG** algorithm.

The core idea: instead of expensive exact arithmetic everywhere or fragile epsilon-based comparisons, the algorithm makes classification decisions only at points with **maximum geometric margin**, where double-precision floating-point is provably correct. This achieves O(n log n + k) complexity while maintaining numerical robustness.

## Features

- **Boolean operations**: union, intersection, difference on closed triangle meshes
- **Robust predicates**: Shewchuk-style adaptive precision with automatic escalation (double → expansion → exact rational)
- **Patch-confident classification**: max-margin point selection eliminates misclassification without exact arithmetic on the hot path
- **SAH-accelerated BVH**: surface area heuristic bounding volume hierarchy for O(n log n + k) intersection detection
- **Dual classifier**: ray-casting (fast) or generalized winding number (handles non-manifold input)
- **Zero external dependencies**: pure .NET, no native code, AOT-compatible
- **Multi-target**: .NET 10 and .NET Framework 4.8 from a single codebase
- **Optional GPU acceleration**: Vulkan compute shaders for patch classification via Silk.NET (Windows, Linux x64, Raspberry Pi 4+)

## Quick Start

```csharp
using MdCsg.Api;
using MdCsg.Math;

// Create two overlapping cubes
var cubeA = Primitives.Cube(Vec3.Zero, 1.0);
var cubeB = Primitives.Cube(new Vec3(0.5, 0.5, 0.5), 1.0);

// Boolean operations
var union        = Csg.Union(cubeA, cubeB);
var intersection = Csg.Intersect(cubeA, cubeB);
var difference   = Csg.Difference(cubeA, cubeB);

// Inspect the result
Console.WriteLine($"Faces: {union.FaceCount}, Vertices: {union.VertexCount}");
```

### Creating Solids

```csharp
// From triangle soup
var triangles = new List<Triangle3> { /* your triangles */ };
var solid = Solid.FromTriangles(triangles);

// From indexed mesh
var positions = new List<Vec3> { /* vertices */ };
var indices = new List<(int, int, int)> { /* triangle indices */ };
var solid = Solid.FromIndexed(positions, indices);
```

### Options

```csharp
var options = new CsgOptions
{
    GridSize = 1e-8,                                  // snap rounding grid resolution
    UseWindingNumber = true,                          // winding number classifier (vs ray-casting)
    WeldTolerance = 1e-8,                             // vertex welding distance
    PatchExtractionMode = PatchExtractionMode.Auto,   // Auto | IntraFace | Global | Arrangement
    PreferTopologyPreservingPatchExtraction = true,   // in Auto mode, evaluate both and pick better topology
};
var result = Csg.Union(a, b, options);
```

### GPU Acceleration (Optional)

The optional `MdCsg.Gpu` package accelerates patch classification using Vulkan compute shaders. The core library remains zero-dependency — GPU support is opt-in via a strategy interface.

```csharp
using MdCsg.Api;
using MdCsg.Gpu;

// Returns null gracefully if no Vulkan device is available
using var gpuClassifier = GpuAccelerator.TryCreate();

var result = Csg.Union(solidA, solidB, new CsgOptions
{
    ClassificationStrategy = gpuClassifier  // null falls back to CPU automatically
});

// Or use CreateWithFallback() to always get a working strategy
var strategy = GpuAccelerator.CreateWithFallback();
```

Three Vulkan compute shaders run the classification pipeline:
1. **confident_point.comp** — max-margin centroid selection (256 threads/workgroup per patch)
2. **raycast_classify.comp** — 3-ray majority vote with BVH traversal
3. **winding_number.comp** — Van Oosterom-Strackee solid angle computation

All shaders use float32 for broad GPU compatibility (including Raspberry Pi 4 VideoCore VI which lacks float64). The max-margin algorithm inherently tolerates this precision loss.

| Platform | GPU | Vulkan Version |
|----------|-----|----------------|
| Windows x64 | NVIDIA/AMD/Intel | 1.0+ |
| Linux x64 | NVIDIA/AMD/Intel | 1.0+ (Mesa or proprietary) |
| Linux ARM64 (Pi 4+) | VideoCore VI | 1.3 via Mesa V3DV |
| Any (no GPU) | --- | Automatic CPU fallback |

See [docs/gpu-acceleration.md](docs/gpu-acceleration.md) for full GPU architecture details.

## Architecture

The library is organized in layers, each building on the one below:

```
Layer 0  Math/           Vec2, Vec3, Plane, Aabb, Triangle3, Ray, Segment
Layer 1  Arithmetic/     Shewchuk expansion arithmetic, exact Rational, adaptive precision
Layer 2  Predicates/     Orient2D, Orient3D, InCircle, PlaneClassification
Layer 3  Mesh/           Half-edge DCEL mesh (Vertex, HalfEdge, Face, MeshBuilder)
Layer 4  Bvh/            SAH-based BVH with flat array layout, dual-tree traversal
Layer 5  Intersection/   Moller tri-tri intersection, snap rounding, intersection graph
Layer 6  Cutting/        Constrained triangulation, face/mesh cutting along intersections
Layer 7  Patches/        Flood-fill patch extraction bounded by intersection edges
Layer 8  Classification/ Confident point selection, ray-cast & winding number classifiers
Layer 9  Operations/     Patch assembly (select/flip by operation), mesh stitching
Layer 10 Api/            Public API: Solid, Csg, CsgOptions, CsgResult
```

### The Patch-Confident Algorithm

Standard CSG algorithms classify every point on the mesh surface as inside or outside the other solid. This requires either exact arithmetic (slow) or epsilon-based comparisons (fragile).

The patch-confident approach:

1. **Cut** both meshes along their intersection curves
2. **Extract patches** — connected groups of sub-triangles bounded by intersection edges
3. **For each patch**, find the sub-triangle centroid with the **maximum distance** to any face of the other solid (the "confident point")
4. **Classify** only at that confident point, where the geometric margin guarantees double-precision is correct
5. **Assemble** the result by selecting patches based on their classification and the operation

This gives the correctness of exact arithmetic with the speed of floating-point.

## Building

Requires .NET SDK 10.0 or later (builds both net10.0 and net48 targets).

```bash
dotnet build
dotnet test                                    # Core library tests (~14k per target)
dotnet test tests/MdCsg.Gpu.Tests              # GPU tests (15 tests, gracefully skipped if no Vulkan)
dotnet test tests/MdCsg.Robust.Conformance     # Robustness conformance scaffold
pwsh ./tools/ci/run-robustness-gate.ps1        # Robustness rescue-bar gate (same command used in CI)
dotnet run -c Release --project src/MdCsg.Showcase/MdCsg.Showcase.csproj
```

## Benchmarks

```bash
dotnet run -c Release -f net10.0 --project benchmarks/MdCsg.Benchmarks/MdCsg.Benchmarks.csproj
dotnet run -c Release -f net48 --project benchmarks/MdCsg.Benchmarks/MdCsg.Benchmarks.csproj
```

See [docs/net48-port.md](docs/net48-port.md) for the full .NET Framework 4.8 port details and performance comparison.
For robustness-first redesign work, see [docs/ROBUSTNESS_SPEC.md](docs/ROBUSTNESS_SPEC.md) and [docs/robust-patch-confident-plan.md](docs/robust-patch-confident-plan.md).

## Project Structure

```
MdCsg/
├── src/MdCsg/                    # Library (net10.0 + net48)
│   ├── Arithmetic/               # Expansion arithmetic, Rational, adaptive precision
│   ├── Math/                     # Vec2, Vec3, Plane, Aabb, Triangle3, Ray, Segment
│   ├── Predicates/               # Orient2D, Orient3D, InCircle, PlaneClassification
│   ├── Mesh/                     # Half-edge DCEL mesh, builder, validator
│   ├── Bvh/                      # BVH tree, traversal
│   ├── Intersection/             # Tri-tri intersection, snap rounding, intersection graph
│   ├── Cutting/                  # Constrained triangulator, face/mesh cutter
│   ├── Patches/                  # Patch extraction, sub-triangle adjacency
│   ├── Classification/           # Confident point, ray-cast & winding number classifiers
│   ├── Operations/               # Patch assembly, mesh stitching
│   └── Api/                      # Solid, Csg, CsgOptions, CsgResult
├── src/MdCsg.Gpu/                # GPU acceleration (net10.0 only, Silk.NET Vulkan)
│   ├── Shaders/                  # GLSL compute shaders
│   ├── Interop/                  # GPU struct layouts (GpuVec3, GpuBvhNode)
│   └── ...                       # VulkanContext, ComputePipeline, GpuPatchClassifier
├── src/MdCsg.Robust/             # Planned robustness-first CSG kernel scaffold
├── src/MdCsg.Showcase/           # Windows D3D11 showcase application
├── tests/MdCsg.Tests/            # ~14k tests (xUnit, net10.0 + net48)
├── tests/MdCsg.Gpu.Tests/        # 15 GPU tests (gracefully skipped without Vulkan)
├── tests/MdCsg.Robust.Conformance/ # Spec-first robust conformance tests
├── benchmarks/MdCsg.Benchmarks/  # BenchmarkDotNet performance tests
├── tools/                        # Diagnostic console tools (MeshDiag, DiagRun)
├── screenshots/                  # Showcase screenshots (PNG)
└── docs/                         # Documentation
```

## License

[MIT](LICENSE)
