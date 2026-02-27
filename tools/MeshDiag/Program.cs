using System;
using System.Collections.Generic;
using System.Linq;
using MdCsg.Api;
using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Operations;
using MdCsg.Patches;

static void TracePipeline(string label, Solid a, Solid b, CsgOperation operation)
{
    Console.WriteLine($"\n{'=',-60}");
    Console.WriteLine($"  {label}  ({operation})");
    Console.WriteLine($"{'=',-60}");

    // Step 1: Intersection graph
    var intersections = IntersectionGraph.Compute(a.Mesh, b.Mesh);
    Console.WriteLine($"  Segments: {intersections.Segments.Count}");

    // Step 2: Cut meshes
    var cutA = MeshCutter.Cut(a.Mesh, intersections.FaceSegmentsA);
    var cutB = MeshCutter.Cut(b.Mesh, intersections.FaceSegmentsB);
    Console.WriteLine($"  SubTris A: {cutA.SubTriangles.Count}  B: {cutB.SubTriangles.Count}");

    // Step 3: Adjacency + patches
    var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);
    var adjB = SubTriangleAdjacency.Build(cutB.SubTriangles);
    var patchesA = PatchExtractor.Extract(cutA.SubTriangles, adjA);
    var patchesB = PatchExtractor.Extract(cutB.SubTriangles, adjB);
    Console.WriteLine($"  Patches A: {patchesA.Count}  B: {patchesB.Count}");

    // Mark coplanar
    MarkCoplanarPatches(patchesA, cutA.SubTriangles, intersections.CoplanarFacesA);
    MarkCoplanarPatches(patchesB, cutB.SubTriangles, intersections.CoplanarFacesB);

    // Step 4: Classify
    PatchClassifier.ClassifyAll(patchesA, cutA.SubTriangles, b.Bvh);
    PatchClassifier.ClassifyAll(patchesB, cutB.SubTriangles, a.Bvh);

    int insA = patchesA.Count(p => p.IsInside == true);
    int insB = patchesB.Count(p => p.IsInside == true);
    Console.WriteLine($"  Inside A: {insA}/{patchesA.Count}  Inside B: {insB}/{patchesB.Count}");

    // Step 5: Assemble
    var assembly = PatchAssembler.Assemble(patchesA, patchesB, cutA.SubTriangles, cutB.SubTriangles, operation);
    Console.WriteLine($"  Assembled: {assembly.Triangles.Count} triangles");

    // Analyze: how many tris come from A vs B
    // Track which triangles came from A vs B by checking positions
    var aPositions = new HashSet<(double, double, double)>();
    foreach (var p in patchesA.Where(p => IsKept(p, operation, true)))
        foreach (int idx in p.SubTriangleIndices)
        {
            var st = cutA.SubTriangles[idx];
            aPositions.Add((st.A.X + st.B.X + st.C.X, st.A.Y + st.B.Y + st.C.Y, st.A.Z + st.B.Z + st.C.Z));
        }

    int fromA = 0, fromB = 0;
    foreach (var tri in assembly.Triangles)
    {
        var key = (tri.A.X + tri.B.X + tri.C.X, tri.A.Y + tri.B.Y + tri.C.Y, tri.A.Z + tri.B.Z + tri.C.Z);
        if (aPositions.Contains(key)) fromA++;
        else fromB++;
    }
    Console.WriteLine($"  From A: {fromA}  From B: {fromB}");

    // Step 6: Stitch with various tolerances
    foreach (double weldTol in new[] { 1e-10, 1e-8, 1e-6, 1e-4 })
    {
        var mesh = MeshStitcher.Stitch(assembly.Triangles, weldTol);
        int missingTwins = mesh.HalfEdges.Count(he => he.Twin == null);
        int verts = mesh.Vertices.Count;

        // Check for non-manifold edges (more than 2 faces sharing an edge)
        var edgeCounts = new Dictionary<(int, int), int>();
        foreach (var he in mesh.HalfEdges)
        {
            var key = he.Origin.Id < he.Target.Id
                ? (he.Origin.Id, he.Target.Id)
                : (he.Target.Id, he.Origin.Id);
            edgeCounts.TryGetValue(key, out int c);
            edgeCounts[key] = c + 1;
        }
        int nonManifold = edgeCounts.Count(kv => kv.Value > 2);
        int boundary = edgeCounts.Count(kv => kv.Value == 1);

        Console.WriteLine($"  Weld={weldTol:E0}: V={verts} F={mesh.Faces.Count} " +
            $"MissingTwins={missingTwins} NonManifold={nonManifold} Boundary={boundary}");
    }

    // Detailed analysis of boundary edges at default tolerance
    var resultMesh = MeshStitcher.Stitch(assembly.Triangles, 1e-8);
    AnalyzeBoundaryEdgeGaps(resultMesh);
}

static void AnalyzeBoundaryEdgeGaps(HalfEdgeMesh mesh)
{
    var boundary = new List<HalfEdge>();
    foreach (var he in mesh.HalfEdges)
        if (he.Twin == null) boundary.Add(he);

    if (boundary.Count == 0)
    {
        Console.WriteLine("  Mesh is closed (no boundary edges)!");
        return;
    }

    // Build lookup: for each edge direction (origin→target), find all half-edges
    var edgesByDirection = new Dictionary<(int, int), List<HalfEdge>>();
    foreach (var he in mesh.HalfEdges)
    {
        var key = (he.Origin.Id, he.Target.Id);
        if (!edgesByDirection.TryGetValue(key, out var list))
        {
            list = [];
            edgesByDirection[key] = list;
        }
        list.Add(he);
    }

    // Count different types of boundary edges
    int hasOpposite = 0; // twin exists but one of them already paired
    int noOpposite = 0; // no edge in opposite direction at all
    int duplicateDirection = 0; // multiple edges in same direction (non-manifold)

    foreach (var bhe in boundary)
    {
        var oppositeKey = (bhe.Target.Id, bhe.Origin.Id);
        var sameKey = (bhe.Origin.Id, bhe.Target.Id);

        bool hasOppEdge = edgesByDirection.ContainsKey(oppositeKey);
        int sameCount = edgesByDirection.TryGetValue(sameKey, out var sameList) ? sameList.Count : 0;

        if (hasOppEdge) hasOpposite++;
        else noOpposite++;
        if (sameCount > 1) duplicateDirection++;
    }

    Console.WriteLine($"  Boundary analysis ({boundary.Count} unpaired):");
    Console.WriteLine($"    Opposite exists (pairing issue): {hasOpposite}");
    Console.WriteLine($"    No opposite (true hole): {noOpposite}");
    Console.WriteLine($"    Duplicate same-direction: {duplicateDirection}");

    // For "no opposite" edges, check position gap to nearest edge in any direction
    if (noOpposite > 0)
    {
        var positions = new List<(Vec3 o, Vec3 t)>();
        foreach (var he in mesh.HalfEdges)
            positions.Add((he.Origin.Position, he.Target.Position));

        int sampleCount = System.Math.Min(noOpposite, 10);
        int shown = 0;
        foreach (var bhe in boundary)
        {
            if (shown >= sampleCount) break;
            var oppositeKey = (bhe.Target.Id, bhe.Origin.Id);
            if (edgesByDirection.ContainsKey(oppositeKey)) continue;

            var o = bhe.Origin.Position;
            var t = bhe.Target.Position;

            // Find nearest geometrically opposite edge
            double bestGap = double.MaxValue;
            foreach (var (po, pt) in positions)
            {
                double gap = System.Math.Sqrt(Vec3.DistanceSquared(pt, o)) +
                             System.Math.Sqrt(Vec3.DistanceSquared(po, t));
                if (gap < bestGap) bestGap = gap;
            }
            shown++;
            Console.WriteLine($"    Edge ({o.X:F6},{o.Y:F6},{o.Z:F6})→({t.X:F6},{t.Y:F6},{t.Z:F6}) gap={bestGap:E3}");
        }
    }
}

static bool IsKept(Patch patch, CsgOperation operation, bool isA)
{
    if (isA)
    {
        if (patch.CoplanarNormalsAgree.HasValue && !patch.HasConfidentPoint)
            return ShouldKeepCoplanarPatchFromA(patch.CoplanarNormalsAgree.Value, operation);
        return ShouldKeepPatchFromA(patch.IsInside ?? false, operation);
    }
    else
    {
        if (patch.CoplanarNormalsAgree.HasValue && !patch.HasConfidentPoint)
            return false;
        return ShouldKeepPatchFromB(patch.IsInside ?? false, operation);
    }
}

static bool ShouldKeepPatchFromA(bool isInsideB, CsgOperation operation) => operation switch
{
    CsgOperation.Union => !isInsideB,
    CsgOperation.Intersection => isInsideB,
    CsgOperation.Difference => !isInsideB,
    _ => false
};

static bool ShouldKeepPatchFromB(bool isInsideA, CsgOperation operation) => operation switch
{
    CsgOperation.Union => !isInsideA,
    CsgOperation.Intersection => isInsideA,
    CsgOperation.Difference => isInsideA,
    _ => false
};

static bool ShouldKeepCoplanarPatchFromA(bool normalsAgree, CsgOperation operation) => operation switch
{
    CsgOperation.Union => normalsAgree,
    CsgOperation.Intersection => normalsAgree,
    CsgOperation.Difference => !normalsAgree,
    _ => false
};

static void MarkCoplanarPatches(
    IReadOnlyList<Patch> patches,
    IReadOnlyList<FaceCutter.SubTriangle> subTriangles,
    IReadOnlyDictionary<int, bool> coplanarFaces)
{
    if (coplanarFaces.Count == 0) return;
    foreach (var patch in patches)
    {
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

Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
Console.WriteLine("║       CSG Stitch Diagnostic                             ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════╝");

Console.WriteLine("\n▶ Scene 2: OFFSET sphere+box");
var sOff = Primitives.Sphere(Vec3.Zero, 1.2, 3);
var bOff = Primitives.Cube(new Vec3(0.6, 0, 0), 1.5);
TracePipeline("OFFSET Sphere∩Box", sOff, bOff, CsgOperation.Intersection);

Console.WriteLine("\n▶ Scene 5: CONCENTRIC sphere+box");
var sCon = Primitives.Sphere(Vec3.Zero, 1.3, 3);
var bCon = Primitives.Cube(Vec3.Zero, 1.8);
TracePipeline("CONCENTRIC Sphere∩Box", sCon, bCon, CsgOperation.Intersection);

Console.WriteLine("\n▶ Scene 7: CONCENTRIC box+sphere");
var bCon7 = Primitives.Cube(Vec3.Zero, 2.0);
var sCon7 = Primitives.Sphere(Vec3.Zero, 1.4, 3);
TracePipeline("CONCENTRIC Box∩Sphere", bCon7, sCon7, CsgOperation.Intersection);

Console.WriteLine("\nDone.");
