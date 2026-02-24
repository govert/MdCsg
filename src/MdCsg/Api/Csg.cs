using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Operations;
using MdCsg.Patches;

namespace MdCsg.Api;

/// <summary>
/// Static entry point for CSG (Constructive Solid Geometry) boolean operations.
/// Implements the Patch-Confident CSG algorithm.
/// </summary>
public static class Csg
{
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

    /// <summary>
    /// Evaluates a CSG boolean operation between two solids.
    /// </summary>
    private static CsgResult Evaluate(Solid a, Solid b, CsgOperation operation, CsgOptions options)
    {
        // Step 1: Find all intersections between the two meshes
        var intersections = IntersectionGraph.Compute(a.Mesh, b.Mesh, options.GridSize);

        // Step 2: Cut both meshes along intersection curves
        var cutA = MeshCutter.Cut(a.Mesh, intersections.FaceSegmentsA);
        var cutB = MeshCutter.Cut(b.Mesh, intersections.FaceSegmentsB);

        // Step 3: Build adjacency and extract patches
        var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var adjB = SubTriangleAdjacency.Build(cutB.SubTriangles);

        var patchesA = PatchExtractor.Extract(cutA.SubTriangles, adjA);
        var patchesB = PatchExtractor.Extract(cutB.SubTriangles, adjB);

        // Step 4: Classify patches (the novel part — uses max-margin confident points)
        int degA = PatchClassifier.ClassifyAll(patchesA, cutA.SubTriangles, b.Bvh, options.UseWindingNumber);
        int degB = PatchClassifier.ClassifyAll(patchesB, cutB.SubTriangles, a.Bvh, options.UseWindingNumber);

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
}
