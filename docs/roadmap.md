# MdCsg Roadmap

This roadmap is now complemented by:

- [ROBUSTNESS_SPEC.md](ROBUSTNESS_SPEC.md) for correctness guarantees and acceptance gates.
- [robust-patch-confident-plan.md](robust-patch-confident-plan.md) for implementation sequencing.

## Guiding Principle

**Robustness is the highest priority.** A correct answer slowly beats a wrong answer quickly. Every feature should maintain or improve the library's reliability on real-world input. When there is a tension between performance and correctness, correctness wins.

## Phase 1: Robustness Foundation

These two items address the most important correctness gaps. They should be tackled first because every subsequent feature builds on the assumption that the core algorithm handles arbitrary input reliably.

### 1.1 Constrained Delaunay Triangulation (CDT)

**Problem:** The current `ConstrainedTriangulator` uses ear-clipping with incremental vertex insertion. This fails on complex intersection patterns — multiple crossing segments creating non-star-shaped sub-regions, near-degenerate slivers, and T-junctions at intersection curve endpoints.

**Impact:** Real-world meshes (CAD exports, scanned geometry) routinely produce these patterns. Failure in the triangulator means the entire CSG operation fails or produces corrupt output.

**Approach:**
- Implement a 2D Constrained Delaunay Triangulation using robust predicates (Orient2D, InCircle are already available)
- The Delaunay property maximizes minimum angles, avoiding slivers
- Constraint edges (intersection segments) are enforced by edge flipping rather than cavity re-triangulation
- Consider the Shewchuk/Si approach: incremental insertion + edge recovery
- The existing `ConstrainedTriangulator` API (`Triangulate(vertices, constraints, faceNormal)`) can remain unchanged — only the implementation changes

**Verification:**
- All existing tests must still pass (the API is unchanged)
- Add stress tests with pathological intersection patterns: many parallel cuts, near-tangent intersections, T-junctions
- Measure minimum triangle angle in output — Delaunay should consistently produce better angles than ear-clipping

### 1.2 Coplanar Face Handling

**Problem:** When two meshes share a face (or have faces in the same plane), patch classification breaks down. The confident point margin approaches zero because the point lies on (or very near) the other solid's surface. The current workaround — marking patches as "degenerate" and applying heuristic coplanar rules — is fragile.

**Impact:** This is arguably the most common beginner scenario: union of two cubes sharing a face, difference of a cube from a larger cube where faces align. It also arises in real CAD workflows where parts are designed to be flush.

**Approach candidates (to be evaluated):**
- **Symbolic perturbation (Simulation of Simplicity):** Perturb one mesh infinitesimally so no faces are exactly coplanar. The perturbation is symbolic — never actually applied — but it breaks all ties consistently. Well-studied technique (Edelsbrunner & Mücke). Would need to propagate through Orient3D predicates.
- **Exact arithmetic fallback:** For patches flagged degenerate (margin < threshold), switch to exact rational arithmetic for the classification decision. The `Rational` type already exists in the arithmetic layer.
- **Volumetric perturbation:** Actually offset one mesh by epsilon along face normals. Simpler to implement but changes the geometry slightly.

**Verification:**
- Union/Intersection/Difference of two identical cubes
- Two cubes sharing a face
- Cube with a face-aligned half-space
- Mesh with self-touching faces (e.g., two tetrahedra sharing a face)

## Phase 2: Signed Distance Fields

### 2.1 SDF Computation from Mesh

**Problem:** There is no way to query the signed distance from an arbitrary point to a mesh surface. The unsigned distance exists (`ConfidentPoint.ComputeMinDistanceToMesh`) and inside/outside classification exists (`RayCastClassifier`), but they are not combined into a coherent SDF API.

**Value:** SDF is the bridge between mesh representation and point/voxel representations. It enables:
- Voxelization (sample on grid, threshold at zero)
- Collision detection (SDF value = penetration depth)
- Morphological operations (offset = shift the zero-level-set)
- Point cloud filtering (classify points against solid regions)
- Future implicit-surface CSG operands

**Approach:**
- New class `SignedDistanceField` (or method on `Solid`)
- `double SignedDistance(Vec3 point)` = `ComputeMinDistanceToMesh(point, bvh)` × sign from `RayCastClassifier`
- Negative inside, positive outside (standard convention)
- Batch query: `void SignedDistances(ReadOnlySpan<Vec3> points, Span<double> results)` for point cloud workflows
- Grid sampling: `double[,,] SampleOnGrid(Aabb bounds, int nx, int ny, int nz)` for voxelization
- Consider BVH-accelerated batch queries (sort points spatially, traverse BVH once)

**Interactions:**
- Point cloud clipping: `IPointClassifier` already enables per-point inside/outside. SDF adds distance information.
- Half-space SDF is trivial: `plane.SignedDistanceTo(point)`. So `PlanePointClassifier.DistanceToSurface` is already the unsigned version.

## Phase 3: Parallelism

### 3.1 CPU Parallel Execution

**Problem:** The entire pipeline is single-threaded. For large meshes (100K+ faces), patch classification and intersection detection dominate runtime.

**Approach:**
- **Patch classification:** `Parallel.ForEach` over patches. Each patch's confident point selection and classification is fully independent. This is the easiest win.
- **Intersection detection:** After BVH dual-tree traversal produces overlapping face pairs, `TriTriIntersection` for each pair is independent. Partition pairs into chunks and process in parallel. Need thread-safe collection for output segments.
- **Mesh cutting:** Each face is cut independently. Can partition faces and cut in parallel, then merge sub-triangle lists.
- **BVH construction:** SAH BVH build is inherently recursive and parallelizable (each subtree is independent after the partition step).

**Considerations:**
- Avoid allocations in hot loops — use `ArrayPool<T>` for temporary buffers
- The GPU path (`MdCsg.Gpu`) already parallelizes classification. CPU parallelism covers users without Vulkan.
- SIMD (`Vector256<double>`) for Vec3 operations in BVH traversal and distance computation is orthogonal to thread parallelism and can be done independently.

## Phase 4: Library Completeness

### 4.1 Primitive Constructors

Move `MeshFactory` patterns into the main library as `MdCsg.Api.Primitives`:
- `Box(center, size)`, `Cube(center, extent)`
- `Sphere(center, radius, subdivisions)`
- `Cylinder(base, axis, radius, height)`
- `Cone(base, axis, radius, height)`
- `Torus(center, axis, majorRadius, minorRadius)`

Each returns a `Solid`. These are straightforward — the sphere icosahedron subdivision pattern already exists in test helpers.

### 4.2 Mesh I/O

Separate package `MdCsg.IO` to keep the core dependency-free:
- STL (binary + ASCII) read/write
- OBJ read/write (vertices + faces, no materials)
- PLY read (for point cloud import)
- Round-trip test: write → read → compare mesh

### 4.3 Implicit Primitives via `IPointClassifier`

The `IPointClassifier` abstraction already supports this. A `SpherePointClassifier` would:
- `Classify(point)` = `Distance(point, center) < radius ? Inside : Outside`
- `DistanceToSurface(point)` = `|Distance(point, center) - radius|`

Combined with an analytic sphere-mesh intersection (replacing `PlaneIntersector` with `SphereIntersector`), this gives exact sphere CSG without meshing the sphere first. Higher quality intersection curves, no tessellation artifacts.

The same pattern applies to cylinders, cones, and any implicit surface where:
1. Inside/outside can be computed analytically
2. Surface-mesh intersection curves can be computed per-face

This is architecturally interesting because it validates the `IPointClassifier` design introduced with HalfSpace.

## Future Considerations (Not Yet Planned)

- **Mesh simplification post-CSG** — QEM edge collapse to reduce triangle count near intersection curves
- **CSG tree / lazy evaluation** — tree of operations, optimize before evaluation, cache intermediates
- **Offset surfaces / Minkowski sums** — shell generation for 3D printing, clearance checking
- **Point cloud as implicit CSG operand** — reconstruct SDF from point cloud + normals, wrap as `IPointClassifier`
- **Convex decomposition** — useful for physics engines consuming CSG results
- **Mesh repair** — fix non-manifold edges, tiny gaps, degenerate triangles before CSG
