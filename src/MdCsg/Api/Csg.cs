using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Operations;
using MdCsg.Patches;

namespace MdCsg.Api;

/// <summary>
/// Static entry point for CSG (Constructive Solid Geometry) boolean operations.
/// Implements the Patch-Confident CSG algorithm.
/// </summary>
public static class Csg
{
    // ── Mesh × Mesh ──────────────────────────────────────────────

    /// <summary>
    /// Computes the union A ∪ B.
    /// </summary>
    public static CsgResult Union(Solid a, Solid b, CsgOptions? options = null) =>
        Evaluate(a, b, CsgOperation.Union, options ?? new CsgOptions());

    /// <summary>
    /// Computes the intersection A ∩ B.
    /// </summary>
    public static CsgResult Intersect(Solid a, Solid b, CsgOptions? options = null) =>
        Evaluate(a, b, CsgOperation.Intersection, options ?? new CsgOptions());

    /// <summary>
    /// Computes the difference A \ B.
    /// </summary>
    public static CsgResult Difference(Solid a, Solid b, CsgOptions? options = null) =>
        Evaluate(a, b, CsgOperation.Difference, options ?? new CsgOptions());

    // ── Mesh × HalfSpace ────────────────────────────────────────

    /// <summary>Union of a mesh and a half-space produces an infinite result.</summary>
    public static CsgResult Union(Solid a, HalfSpace b, CsgOptions? options = null) =>
        throw new NotSupportedException("Union of a mesh and a half-space produces an infinite result.");

    /// <summary>Clips mesh A to the interior of half-space B.</summary>
    public static CsgResult Intersect(Solid a, HalfSpace b, CsgOptions? options = null) =>
        CsgHalfSpace.Evaluate(a, b, CsgOperation.Intersection, options ?? new CsgOptions());

    /// <summary>Subtracts a half-space from mesh A, keeping the part outside the half-space.</summary>
    public static CsgResult Difference(Solid a, HalfSpace b, CsgOptions? options = null) =>
        CsgHalfSpace.Evaluate(a, b, CsgOperation.Difference, options ?? new CsgOptions());

    // ── HalfSpace × Mesh ────────────────────────────────────────

    /// <summary>Union of a half-space and a mesh produces an infinite result.</summary>
    public static CsgResult Union(HalfSpace a, Solid b, CsgOptions? options = null) =>
        throw new NotSupportedException("Union of a half-space and a mesh produces an infinite result.");

    /// <summary>Clips mesh B to the interior of half-space A (commutative with Intersect(Solid, HalfSpace)).</summary>
    public static CsgResult Intersect(HalfSpace a, Solid b, CsgOptions? options = null) =>
        CsgHalfSpace.Evaluate(b, a, CsgOperation.Intersection, options ?? new CsgOptions());

    /// <summary>Subtracting a mesh from a half-space produces an infinite result.</summary>
    public static CsgResult Difference(HalfSpace a, Solid b, CsgOptions? options = null) =>
        throw new NotSupportedException("Subtracting a mesh from a half-space produces an infinite result.");

    // ── Mesh × ImplicitSolid ─────────────────────────────────────

    /// <summary>Union of a mesh and an implicit solid requires tessellating the implicit surface.</summary>
    public static CsgResult Union(Solid a, ImplicitSolid b, CsgOptions? options = null) =>
        throw new NotSupportedException("Union with an implicit solid requires tessellating its surface. Use a meshed primitive instead.");

    /// <summary>Clips mesh A to the interior of implicit solid B.</summary>
    public static CsgResult Intersect(Solid a, ImplicitSolid b, CsgOptions? options = null) =>
        CsgImplicit.Evaluate(a, b, CsgOperation.Intersection, options ?? new CsgOptions());

    /// <summary>Subtracts an implicit solid from mesh A, keeping the part outside the implicit surface.</summary>
    public static CsgResult Difference(Solid a, ImplicitSolid b, CsgOptions? options = null) =>
        CsgImplicit.Evaluate(a, b, CsgOperation.Difference, options ?? new CsgOptions());

    // ── ImplicitSolid × Mesh ─────────────────────────────────────

    /// <summary>Union of an implicit solid and a mesh requires tessellating the implicit surface.</summary>
    public static CsgResult Union(ImplicitSolid a, Solid b, CsgOptions? options = null) =>
        throw new NotSupportedException("Union with an implicit solid requires tessellating its surface. Use a meshed primitive instead.");

    /// <summary>Clips mesh B to the interior of implicit solid A (commutative with Intersect(Solid, ImplicitSolid)).</summary>
    public static CsgResult Intersect(ImplicitSolid a, Solid b, CsgOptions? options = null) =>
        CsgImplicit.Evaluate(b, a, CsgOperation.Intersection, options ?? new CsgOptions());

    /// <summary>Subtracting a mesh from an implicit solid requires tessellating the implicit surface.</summary>
    public static CsgResult Difference(ImplicitSolid a, Solid b, CsgOptions? options = null) =>
        throw new NotSupportedException("Subtracting a mesh from an implicit solid requires tessellating its surface. Use a meshed primitive instead.");

    // ── HalfSpace × HalfSpace ───────────────────────────────────

    /// <summary>HalfSpace-HalfSpace operations are not supported.</summary>
    public static CsgResult Union(HalfSpace a, HalfSpace b, CsgOptions? options = null) =>
        throw new NotSupportedException("CSG operations between two half-spaces are not supported.");

    /// <summary>HalfSpace-HalfSpace operations are not supported.</summary>
    public static CsgResult Intersect(HalfSpace a, HalfSpace b, CsgOptions? options = null) =>
        throw new NotSupportedException("CSG operations between two half-spaces are not supported.");

    /// <summary>HalfSpace-HalfSpace operations are not supported.</summary>
    public static CsgResult Difference(HalfSpace a, HalfSpace b, CsgOptions? options = null) =>
        throw new NotSupportedException("CSG operations between two half-spaces are not supported.");

    /// <summary>
    /// Evaluates a CSG boolean operation between two solids.
    /// </summary>
    private static CsgResult Evaluate(Solid a, Solid b, CsgOperation operation, CsgOptions options)
    {
        // Step 1: Find all intersections between the two meshes
        var intersections = IntersectionGraph.Compute(a.Mesh, b.Mesh, options.GridSize, options.Parallel);

        // Step 2: Cut both meshes along intersection curves
        var cutA = MeshCutter.Cut(a.Mesh, intersections.FaceSegmentsA, options.Parallel);
        var cutB = MeshCutter.Cut(b.Mesh, intersections.FaceSegmentsB, options.Parallel);

        // Step 3: Build adjacency and extract patches
        var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var adjB = SubTriangleAdjacency.Build(cutB.SubTriangles);

        var patchesA = PatchExtractor.Extract(cutA.SubTriangles, adjA);
        var patchesB = PatchExtractor.Extract(cutB.SubTriangles, adjB);

        // Step 3b: Mark coplanar patches
        MarkCoplanarPatches(patchesA, cutA.SubTriangles, intersections.CoplanarFacesA);
        MarkCoplanarPatches(patchesB, cutB.SubTriangles, intersections.CoplanarFacesB);

        // Step 4: Classify patches (the novel part — uses max-margin confident points)
        var classifier = options.ClassificationStrategy ?? new CpuPatchClassificationStrategy(options.Parallel);
        int degA = classifier.ClassifyAll(patchesA, cutA.SubTriangles, b.Bvh, options.UseWindingNumber);
        int degB = classifier.ClassifyAll(patchesB, cutB.SubTriangles, a.Bvh, options.UseWindingNumber);

        // Step 5: Assemble result by selecting appropriate patches
        var assembly = PatchAssembler.Assemble(
            patchesA, patchesB,
            cutA.SubTriangles, cutB.SubTriangles,
            operation);

        // Step 6: Stitch into output mesh
        var resultMesh = MeshStitcher.Stitch(assembly.Triangles, options.WeldTolerance);

        return new CsgResult
        {
            Mesh = resultMesh,
            PatchCountA = patchesA.Count,
            PatchCountB = patchesB.Count,
            DegenerateCount = degA + degB,
            IntersectionSegmentCount = intersections.Segments.Count
        };
    }

    // ── Complement ──────────────────────────────────────────────────

    /// <summary>
    /// Returns the complement of a solid by reversing the winding of all faces.
    /// </summary>
    /// <param name="solid">The solid to complement.</param>
    /// <returns>A new solid with reversed face normals.</returns>
    public static Solid Complement(Solid solid)
    {
        var positions = new List<Vec3>(solid.Mesh.Vertices.Count);
        foreach (var v in solid.Mesh.Vertices)
            positions.Add(v.Position);

        var triangles = new List<(int, int, int)>(solid.Mesh.Faces.Count);
        foreach (var face in solid.Mesh.Faces)
        {
            var verts = face.GetVertices();
            // Swap second and third to reverse winding
            triangles.Add((verts[0].Id, verts[2].Id, verts[1].Id));
        }

        var builder = new MeshBuilder(1e-12);
        var mesh = builder.Build(positions, triangles);
        return new Solid(mesh);
    }

    // ── Bulk Operations ─────────────────────────────────────────────

    /// <summary>
    /// Computes the union of multiple solids via left-fold.
    /// </summary>
    /// <param name="solids">The solids to unite.</param>
    /// <param name="options">Optional CSG options.</param>
    /// <returns>The result of uniting all solids.</returns>
    public static CsgResult Union(IEnumerable<Solid> solids, CsgOptions? options = null)
    {
        return FoldSolids(solids, CsgOperation.Union, options);
    }

    /// <summary>
    /// Computes the intersection of multiple solids via left-fold.
    /// </summary>
    /// <param name="solids">The solids to intersect.</param>
    /// <param name="options">Optional CSG options.</param>
    /// <returns>The result of intersecting all solids.</returns>
    public static CsgResult Intersect(IEnumerable<Solid> solids, CsgOptions? options = null)
    {
        return FoldSolids(solids, CsgOperation.Intersection, options);
    }

    private static CsgResult FoldSolids(IEnumerable<Solid> solids, CsgOperation op, CsgOptions? options)
    {
        Solid? accumulator = null;
        CsgResult? lastResult = null;

        foreach (var solid in solids)
        {
            if (accumulator == null)
            {
                accumulator = solid;
                continue;
            }

            lastResult = op switch
            {
                CsgOperation.Union => Union(accumulator, solid, options),
                CsgOperation.Intersection => Intersect(accumulator, solid, options),
                CsgOperation.Difference => Difference(accumulator, solid, options),
                _ => throw new ArgumentOutOfRangeException()
            };
            accumulator = new Solid(lastResult.Mesh);
        }

        if (lastResult != null)
            return lastResult;

        if (accumulator != null)
        {
            return new CsgResult
            {
                Mesh = accumulator.Mesh,
                PatchCountA = 0,
                PatchCountB = 0,
                DegenerateCount = 0,
                IntersectionSegmentCount = 0
            };
        }

        throw new ArgumentException("At least one solid is required.", nameof(solids));
    }

    /// <summary>
    /// Marks patches whose sub-triangles come from coplanar face regions.
    /// A patch is coplanar if any of its original faces are in the coplanar map.
    /// </summary>
    private static void MarkCoplanarPatches(
        IReadOnlyList<Patch> patches,
        IReadOnlyList<FaceCutter.SubTriangle> subTriangles,
        IReadOnlyDictionary<int, bool> coplanarFaces)
    {
        if (coplanarFaces.Count == 0) return;

        foreach (var patch in patches)
        {
            // Check if any sub-triangle in this patch has a coplanar original face
            // and the patch has low margin (indicating it's in the overlap region)
            foreach (int triIdx in patch.SubTriangleIndices)
            {
                int origFace = subTriangles[triIdx].OriginalFaceIndex;
                if (coplanarFaces.TryGetValue(origFace, out bool normalsAgree))
                {
                    patch.CoplanarNormalsAgree = normalsAgree;
                    break;
                }
            }
        }
    }
}
