# Test Inventory — 1000-Test Marathon

## Classification Legend
- **U** = Unit test (single function/method)
- **I** = Integration test (multi-component pipeline)
- **P** = Property test (mathematical invariant)
- **E** = Edge case / degenerate input
- **R** = Robustness / precision stress test

## Batch Status Tracker

| Batch | Category | Tests | Status |
|-------|----------|-------|--------|
| 1 | Math: Vec3 extended | 20 | pending |
| 2 | Math: Vec2, Aabb, Segment, Ray | 20 | pending |
| 3 | Math: Triangle3, Plane, MathUtil | 20 | pending |
| 4 | Arithmetic: ErrorBound, Expansion ext | 20 | pending |
| 5 | Arithmetic: Rational, Adaptive ext | 20 | pending |
| 6 | Predicates: Orient2D extended | 20 | pending |
| 7 | Predicates: Orient3D extended | 20 | pending |
| 8 | Predicates: InCircle, PlaneClass ext | 20 | pending |
| 9 | Mesh: HalfEdgeMesh, Face, HalfEdge ext | 20 | pending |
| 10 | Mesh: MeshBuilder, MeshValidator ext | 20 | pending |
| 11 | BVH: BvhTree build and query | 20 | pending |
| 12 | BVH: Ray casting and traversal | 20 | pending |
| 13 | Intersection: TriTri extended | 20 | pending |
| 14 | Intersection: Coplanar, SnapRounding | 20 | pending |
| 15 | Intersection: IntersectionGraph, Segment | 20 | pending |
| 16 | Cutting: FaceCutter extended | 20 | pending |
| 17 | Cutting: ConstrainedTriangulator | 20 | pending |
| 18 | Cutting: MeshCutter | 20 | pending |
| 19 | Patches: SubTriangleAdjacency | 20 | pending |
| 20 | Patches: PatchExtractor extended | 20 | pending |
| 21 | Classification: RayCast extended | 20 | pending |
| 22 | Classification: WindingNumber extended | 20 | pending |
| 23 | Classification: ConfidentPoint ext | 20 | pending |
| 24 | Classification: PatchClassifier | 20 | pending |
| 25 | Operations: PatchAssembler | 20 | pending |
| 26 | Operations: MeshStitcher | 20 | pending |
| 27 | API: Solid, CsgOptions, CsgResult | 20 | pending |
| 28 | API: Csg.Union comprehensive | 20 | pending |
| 29 | API: Csg.Intersect comprehensive | 20 | pending |
| 30 | API: Csg.Difference comprehensive | 20 | pending |
| 31 | Integration: Multi-op chains | 20 | pending |
| 32 | Integration: Cube-cube variations | 20 | pending |
| 33 | Integration: Cube-sphere variations | 20 | pending |
| 34 | Integration: Sphere-sphere | 20 | pending |
| 35 | Integration: Tetrahedron ops | 20 | pending |
| 36 | Properties: Volume conservation | 20 | pending |
| 37 | Properties: Set algebra identities | 20 | pending |
| 38 | Properties: Commutativity/Assoc | 20 | pending |
| 39 | Properties: De Morgan laws | 20 | pending |
| 40 | Properties: Idempotency/Identity | 20 | pending |
| 41 | Edge cases: Touching faces | 20 | pending |
| 42 | Edge cases: Contained solids | 20 | pending |
| 43 | Edge cases: Degenerate inputs | 20 | pending |
| 44 | Edge cases: Coplanar faces | 20 | pending |
| 45 | Edge cases: Near-miss geometry | 20 | pending |
| 46 | Robustness: Precision stress | 20 | pending |
| 47 | Robustness: Asymmetric offsets | 20 | pending |
| 48 | Robustness: Scaled geometry | 20 | pending |
| 49 | Robustness: Rotated geometry | 20 | pending |
| 50 | Robustness: Random offsets | 20 | pending |

**Total: 1000 tests across 50 batches**
