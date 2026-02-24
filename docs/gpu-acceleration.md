# GPU-Accelerated Patch Classification

## Overview

MdCsg.Gpu provides GPU-accelerated patch classification for the Patch-Confident CSG algorithm using Vulkan compute shaders via Silk.NET. It replaces Step 4 (patch classification) of the CSG pipeline — the embarrassingly parallel bottleneck — while keeping the core MdCsg library zero-dependency.

## Architecture

```
MdCsg (zero dependencies)         MdCsg.Gpu (Silk.NET Vulkan)
┌─────────────────────┐           ┌──────────────────────────┐
│ IPatchClassification │◄──────────│ GpuPatchClassifier       │
│ Strategy             │           │   confident_point.comp   │
│                      │           │   raycast_classify.comp  │
│ CpuPatchClassifi-    │           │   winding_number.comp    │
│ cationStrategy       │           │                          │
└─────────────────────┘           │ VulkanContext             │
                                  │ GpuBufferPool             │
                                  │ ComputePipeline           │
                                  │ ShaderCompiler (Shaderc)  │
                                  │ BvhFlattener              │
                                  │ GpuAccelerator            │
                                  └──────────────────────────┘
```

### Strategy Pattern

The core library defines `IPatchClassificationStrategy` with a single method:

```csharp
int ClassifyAll(
    IReadOnlyList<Patch> patches,
    IReadOnlyList<SubTriangle> subTriangles,
    BvhTree otherBvh,
    bool useWindingNumber);
```

`CsgOptions.ClassificationStrategy` accepts any implementation. When null (default), the CPU strategy is used automatically.

### GPU Pipeline

Three Vulkan compute shaders execute sequentially per mesh:

1. **confident_point.comp** — One workgroup (256 threads) per patch. Cooperative sub-triangle iteration with shared-memory parallel reduction to find the max-margin centroid.

2. **raycast_classify.comp** — One thread per patch. Shoots 3 perturbed rays (same directions as CPU), iterative BVH traversal, Moller-Trumbore intersection, majority vote.

3. **winding_number.comp** — One workgroup (256 threads) per patch. Cooperative face iteration computing Van Oosterom-Strackee solid angles, shared-memory reduction.

All shaders use float32 (Pi 4 has no float64 support). The max-margin algorithm inherently tolerates this precision loss.

## Usage

### Basic GPU acceleration

```csharp
using MdCsg.Api;
using MdCsg.Gpu;

// Create GPU classifier (returns null if no Vulkan device)
using var gpuClassifier = GpuAccelerator.TryCreate();

var result = Csg.Union(solidA, solidB, new CsgOptions
{
    ClassificationStrategy = gpuClassifier  // null falls back to CPU
});
```

### Automatic fallback

```csharp
// Always returns a working strategy (GPU or CPU)
var strategy = GpuAccelerator.CreateWithFallback();

var result = Csg.Intersect(solidA, solidB, new CsgOptions
{
    ClassificationStrategy = strategy
});
```

### Reuse across operations

```csharp
// Create once, reuse for many operations
using var classifier = GpuAccelerator.TryCreate();

for (int i = 0; i < 100; i++)
{
    var result = Csg.Difference(solidA, solidsB[i], new CsgOptions
    {
        ClassificationStrategy = classifier
    });
}
```

## Platform Support

| Platform | GPU | Status |
|----------|-----|--------|
| Windows x64 | NVIDIA/AMD/Intel | Vulkan 1.0+ required |
| Linux x64 | NVIDIA/AMD/Intel | Vulkan 1.0+ via Mesa or proprietary |
| Linux ARM64 (Pi 4+) | VideoCore VI | Vulkan 1.3 via Mesa V3DV |
| Any (no GPU) | — | Automatic CPU fallback |

## Runtime Shader Compilation

GLSL compute shaders are embedded as assembly resources and compiled to SPIR-V at runtime via Silk.NET.Shaderc. Compilation is cached — first use has a one-time cost (~100ms), subsequent uses are instant.

## Dependencies

- **Silk.NET.Vulkan** 2.x — Vulkan API bindings
- **Silk.NET.Vulkan.Extensions.KHR** 2.x — KHR extension support
- **Silk.NET.Shaderc** 2.x — Runtime GLSL→SPIR-V compilation

The core `MdCsg` library remains dependency-free.

## Running Benchmarks

```bash
dotnet run -c Release --project benchmarks/MdCsg.Gpu.Benchmarks
```

## Running Tests

```bash
# Core library tests (134 tests, no GPU needed)
dotnet test tests/MdCsg.Tests

# GPU tests (11 tests, gracefully skipped if no Vulkan)
dotnet test tests/MdCsg.Gpu.Tests
```
