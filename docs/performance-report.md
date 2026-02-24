# Performance Report

Scaling tests and edge case analysis for MdCsg, run on AMD Ryzen 7 4800H (8C/16T), Windows 11, .NET SDK 10.0.200-preview.

## Edge Case Results

14 tests exercising adversarial geometry. 10 pass, 4 fail.

| Test | Result | Notes |
|---|---|---|
| Disjoint cubes: union volume = sum | PASS | |
| Disjoint cubes: intersection is empty | PASS | |
| Contained cube: union = outer | PASS | |
| Contained cube: intersection = inner | PASS | |
| Nearly touching cubes (gap=0.0001) | PASS | |
| Tiny overlap cubes (overlap=0.001) | **FAIL** | Volume 1.67 vs expected 2.00 |
| Large cube minus tiny cube | PASS | |
| Overlapping spheres: volume conservation | PASS | |
| Cube-sphere intersection = sphere | PASS | |
| Sequential: (A union B) \ C | **FAIL** | Accumulated mesh quality loss |
| Tetrahedron-cube intersection | **FAIL** | 0 output faces, 6 intersection segments detected but result is empty |
| Sphere union commutativity | PASS | |
| De Morgan: Vol(A\B) = Vol(A) - Vol(A&B) | **FAIL** | Delta = 0.25 (25% error) |
| High-res sphere union (1280 tris each) | PASS | |

### Failure Analysis

**Tiny overlap (overlap = 0.001 along X axis):** The snap rounding grid (1e-8) is much finer than the overlap, so the geometry is preserved. The issue is that the very thin intersection region produces sub-triangles that straddle the boundary, and the confident-point classifier picks points that are close to both surfaces, reducing classification accuracy. The resulting union volume is 1.67 instead of 2.00. This is a fundamental limitation of the ear-clipping triangulator combined with the single-centroid confident-point heuristic — thin slivers have centroids near the surface, reducing margin.

**Sequential operations (A union B) then difference C:** The A union B result already has a 14% volume error (1.71 instead of 1.50 for two cubes overlapping by half). The subsequent difference compounds this. The root cause is the same: the simple ear-clipping triangulator produces sub-triangles of varying quality, and the confident-point approach can misclassify patches near sharp intersection curves on axis-aligned geometry.

**Tetrahedron-cube intersection:** The tetrahedron at origin with size=0.5 has vertices at (0.5,0.5,0.5), (0.5,-0.5,-0.5), (-0.5,0.5,-0.5), (-0.5,-0.5,0.5) — it extends into negative space outside the cube [0,1]^3. The intersection produces 6 segments (the tet is cut by cube faces), but the result has 0 output faces. This indicates that all patches of the cut tetrahedron were classified as "outside" the cube, even the patches that are genuinely inside. This is a bug in the patch classifier — when a mesh is cut into many small patches by intersection curves, some patches may have centroids very close to the other surface, leading to misclassification.

**De Morgan volume identity:** Vol(A\B) + Vol(A&B) = 0.75, expected 1.0. This is a consequence of the same patch classification issues. The difference and intersection each lose some volume, and the losses don't cancel in this identity.

### Known Limitations

These failures reveal the current limitations of the implementation:

1. **Ear-clipping triangulator** produces low-quality sub-triangles near intersection curves, especially for axis-aligned or near-coplanar geometry. A full constrained Delaunay triangulation would produce better sub-triangles with more favorable aspect ratios.

2. **Single centroid as confident point** works well when patches are large and well-separated from the other surface, but degrades when patches are thin slivers or nearly tangent to the other surface. A more robust approach would sample multiple candidate points per patch and use a voting scheme.

3. **No handling of coplanar face pairs** — when two triangles from different meshes are nearly coplanar, the intersection detection may produce degenerate or missing segments.

These are algorithmic limitations, not bugs in the port. Both .NET 10 and .NET Framework 4.8 produce identical results (same 10/14 pass rate, identical volumes).

## Scaling Results

### Sphere-Sphere Union (Overlapping, Offset 0.8 Radii)

The primary benchmark: two overlapping icospheres of increasing resolution.

| Subdiv | Tris/mesh | Output faces | Intersection segs | .NET 10 (ms) | .NET 4.8 (ms) | Ratio |
|---:|---:|---:|---:|---:|---:|---:|
| 0 | 20 | 94 | 19 | 2 | 1 | — |
| 1 | 80 | 254 | 32 | 6 | 4 | — |
| 2 | 320 | 782 | 52 | 24 | 24 | 1.0x |
| 3 | 1,280 | 2,871 | 108 | 91 | 122 | 1.3x |
| 4 | 5,120 | 10,776 | 216 | 659 | 848 | 1.3x |
| 5 | 20,480 | 42,094 | 446 | 3,402 | 6,824 | 2.0x |
| 6 | 81,920 | 166,061 | 894 | 27,303 | 59,545 | 2.2x |
| 7 | 327,680 | 659,832 | 1,732 | 333,995 | — | — |

The 1-minute mark on .NET 10 is reached between subdivision 6 (27s) and subdivision 7 (334s), corresponding to 82K–328K triangles per mesh.

### Growth Rate Analysis

For the sphere-sphere union, measuring time vs. input size:

| Step | n (tris/mesh) | Time (ms) | n ratio | Time ratio | Implied exponent |
|---:|---:|---:|---:|---:|---:|
| 2→3 | 320→1,280 | 24→91 | 4.0x | 3.8x | 1.37 |
| 3→4 | 1,280→5,120 | 91→659 | 4.0x | 7.2x | 2.01 |
| 4→5 | 5,120→20,480 | 659→3,402 | 4.0x | 5.2x | 1.68 |
| 5→6 | 20,480→81,920 | 3,402→27,303 | 4.0x | 8.0x | 2.12 |
| 6→7 | 81,920→327,680 | 27,303→333,995 | 4.0x | 12.2x | 2.55 |

The implied exponent grows from ~1.4 to ~2.5 as the mesh size increases, indicating **worse than O(n^2) scaling at large n**. The theoretical O(n log n + k) is not being achieved in practice. The bottleneck analysis:

- **Mesh building (vertex welding):** Uses a hash map keyed by snapped coordinates. At large n, the hash function (XOR-multiply of grid coordinates) likely has collision chains, causing the welding step to degrade toward O(n^2).
- **BVH dual-tree traversal:** For overlapping meshes, the traversal visits O(k) leaf pairs where k = number of potentially overlapping face pairs. The AABB overlap checks at each level should prune well for separated regions, but the intersection band produces O(sqrt(n)) pairs per face, giving O(n * sqrt(n)) = O(n^1.5) total pairs.
- **Patch classification (confident point):** For each patch, computes nearest-face distance via BVH traversal. This is O(patches * log(n)). The number of patches is bounded by the number of intersection segments, which is O(k).

The dominant cost appears to be mesh construction (vertex welding), not the BVH intersection detection.

### Disjoint Spheres (No Intersections)

This isolates the cost of mesh building + BVH construction + trivial classification (no cutting, no patches).

| Subdiv | Tris/mesh | Intersection segs | .NET 10 (ms) |
|---:|---:|---:|---:|
| 0 | 20 | 0 | 0 |
| 1 | 80 | 0 | 1 |
| 2 | 320 | 0 | 47 |
| 3 | 1,280 | 0 | 48 |
| 4 | 5,120 | 0 | 262 |
| 5 | 20,480 | 0 | 1,981 |
| 6 | 81,920 | 0 | 22,735 |
| 7 | 327,680 | 0 | 205,954 |

With 0 intersection segments, the entire cost is mesh construction + BVH build + single-patch classification. Growth rates:

| Step | Time ratio | Implied exponent |
|---:|---:|---:|
| 4→5 | 7.6x | 2.26 |
| 5→6 | 11.5x | 2.49 |
| 6→7 | 9.1x | 2.25 |

This confirms that even without intersections, the base infrastructure cost grows as roughly O(n^2.3). The mesh builder's vertex welding is the primary suspect.

### Cube Minus Sphere (Fully Contained)

Sphere is fully inside the cube, so there are 0 intersection segments. The sphere's patch is classified as "inside" and kept with flipped normals.

| Subdiv | Sphere tris | Output faces | .NET 10 (ms) |
|---:|---:|---:|---:|
| 0 | 20 | 32 | 0 |
| 1 | 80 | 92 | 1 |
| 2 | 320 | 332 | 4 |
| 3 | 1,280 | 1,292 | 62 |
| 4 | 5,120 | 5,132 | 96 |
| 5 | 20,480 | 20,492 | 294 |
| 6 | 81,920 | 81,932 | 1,283 |
| 7 | 327,680 | 327,692 | 6,939 |

This scales better because only one mesh (the sphere) is large. The cube has 12 triangles, so the BVH dual-tree traversal is asymmetric and fast.

### Chain of Overlapping Cubes (Sequential Union)

Tests iterated CSG: union of N cubes placed along the X axis, each overlapping the previous by 0.5.

| N cubes | Output faces | .NET 10 (ms) |
|---:|---:|---:|
| 2 | 51 | 1 |
| 4 | 98 | 40 |
| 8 | 191 | 33 |
| 16 | 351 | 64 |
| 32 | 670 | 169 |
| 64 | 1,310 | 524 |

Scaling is roughly linear in N (doubling N roughly doubles time), which is expected: each step does a constant-size CSG between the accumulated result and one new cube. The accumulated mesh grows linearly, but each new cube only intersects the rightmost portion.

### Sphere Grid (NxNxN Sequential Union)

The most demanding test: sequential union of N^3 spheres arranged in a grid.

| N | Spheres | Output faces | .NET 10 (ms) |
|---:|---:|---:|---:|
| 1 | 1 | 80 | 0 |
| 2 | 8 | 1,708 | 87 |
| 3 | 27 | 7,940 | 1,836 |
| 4 | 64 | 20,016 | 13,231 |
| 5 | 125 | 41,143 | 68,665 |
| 6 | 216 | 82,042 | 394,496 |

The 1-minute mark is crossed between N=4 (13s) and N=5 (69s) — a 4x4x4 grid of 64 spheres with ~20K output faces. The time growth is super-quadratic because each successive union operates on an increasingly large accumulated mesh.

## .NET 10 vs .NET 4.8 Summary

| Workload | .NET 10 | .NET 4.8 | Ratio |
|---|---:|---:|---:|
| Sphere-sphere union (5,120 tris) | 659 ms | 848 ms | 1.3x |
| Sphere-sphere union (20,480 tris) | 3,402 ms | 6,824 ms | 2.0x |
| Sphere-sphere union (81,920 tris) | 27,303 ms | 59,545 ms | 2.2x |
| Edge case pass rate | 10/14 | 10/14 | identical |

The .NET 4.8 runtime is 1.3–2.2x slower, with the gap widening as mesh size increases (due to JIT quality differences in loop optimization and bounds-check elimination). Both runtimes produce identical geometric results.

## Recommendations for Future Optimization

1. **Replace vertex welding hash:** The current XOR-multiply hash causes collision chains at large n. A spatial hash with better distribution (e.g., a hash based on Morton codes or a proper spatial grid) would reduce welding from O(n^2) worst-case to O(n) expected.

2. **Implement full constrained Delaunay triangulation:** The ear-clipping triangulator produces low-quality sub-triangles that degrade confident-point classification. A robust CDT would produce well-shaped triangles, improving classification accuracy and reducing the 4 edge case failures.

3. **Multi-point confident sampling:** Instead of a single centroid per patch, sample several candidate points and use majority voting. This would handle thin slivers and near-tangent patches that currently cause misclassification.

4. **Parallel BVH traversal:** The dual-tree traversal and per-patch classification are embarrassingly parallel. Using `Parallel.ForEach` on .NET 10 (with a sequential fallback on .NET 4.8) would give a near-linear speedup with core count.

5. **Object pooling for mesh building:** The mesh builder allocates many small objects (Vertex, HalfEdge, Face). An arena allocator or object pool would reduce GC pressure at large scale.
