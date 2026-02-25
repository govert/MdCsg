using MdCsg.Classification;
using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Operations;
using MdCsg.Patches;

namespace MdCsg.Api;

/// <summary>
/// CSG pipeline for mesh-vs-halfspace operations.
/// Only the mesh contributes geometry; the half-space acts as
/// an analytic classifier and intersection primitive.
/// After patch selection, cap faces are generated on the cutting plane
/// to produce a closed manifold result.
/// </summary>
public static class CsgHalfSpace
{
    /// <summary>
    /// Evaluates a CSG operation between a mesh solid and a half-space.
    /// The mesh is always operand A; the half-space is always operand B.
    /// </summary>
    public static CsgResult Evaluate(Solid mesh, HalfSpace hs, CsgOperation operation, CsgOptions options)
    {
        var plane = hs.Plane;

        // Step 1: Find plane-mesh intersections
        var faceSegments = PlaneIntersector.Compute(mesh.Mesh, plane, options.GridSize);

        int segmentCount = 0;
        foreach (var kvp in faceSegments)
            segmentCount += kvp.Value.Count;

        // Step 2: Cut the mesh along intersection curves
        var cut = MeshCutter.Cut(mesh.Mesh, faceSegments);

        // Step 3: Build adjacency and extract patches
        var adj = SubTriangleAdjacency.Build(cut.SubTriangles);
        var patches = PatchExtractor.Extract(cut.SubTriangles, adj);

        // Step 4: Classify patches against the half-space
        var classifier = new PlanePointClassifier(plane);
        int degCount = PatchClassifier.ClassifyAll(patches, cut.SubTriangles, classifier);

        // Step 5: Assemble mesh A patches (half-space contributes no mesh patches)
        var emptyPatches = (IReadOnlyList<Patch>)Array.Empty<Patch>();
        var emptySubTris = (IReadOnlyList<FaceCutter.SubTriangle>)Array.Empty<FaceCutter.SubTriangle>();

        var assembly = PatchAssembler.Assemble(
            patches, emptyPatches,
            cut.SubTriangles, emptySubTris,
            operation);

        // Step 5b: Generate cap faces on the cutting plane to close the mesh
        var capTriangles = GenerateCapTriangles(faceSegments, plane, operation, options.GridSize);
        assembly.Triangles.AddRange(capTriangles);

        // Step 6: Stitch into output mesh
        var resultMesh = MeshStitcher.Stitch(assembly.Triangles, options.WeldTolerance);

        return new CsgResult
        {
            Mesh = resultMesh,
            PatchCountA = patches.Count,
            PatchCountB = 0,
            DegenerateCount = degCount,
            IntersectionSegmentCount = segmentCount
        };
    }

    /// <summary>
    /// Generates cap triangles on the cutting plane to seal the boundary.
    /// Chains intersection segments into closed polygons, then triangulates each.
    /// </summary>
    private static List<Triangle3> GenerateCapTriangles(
        Dictionary<int, List<IntersectionSegment>> faceSegments,
        Plane plane,
        CsgOperation operation,
        double tolerance)
    {
        var polygons = ChainSegmentsIntoPolygons(faceSegments, tolerance);
        var result = new List<Triangle3>();

        // Cap normal direction:
        //   Intersect(mesh, hs): keep the inside → cap normal faces outward = -plane.Normal
        //   Difference(mesh, hs): keep the outside → cap normal faces outward = +plane.Normal
        bool capNormalMatchesPlane = operation == CsgOperation.Difference;

        foreach (var polygon in polygons)
        {
            if (polygon.Count < 3) continue;

            // Compute centroid for fan triangulation
            Vec3 centroid = Vec3.Zero;
            foreach (var v in polygon)
                centroid = centroid + v;
            centroid = centroid / polygon.Count;

            // Determine polygon winding by checking the first fan triangle's normal
            var edge1 = polygon[0] - centroid;
            var edge2 = polygon[1] - centroid;
            var testNormal = Vec3.Cross(edge1, edge2);
            bool naturallyAlignedWithPlane = Vec3.Dot(testNormal, plane.Normal) > 0;

            // Do we need to flip the winding?
            bool needFlip = naturallyAlignedWithPlane != capNormalMatchesPlane;

            for (int i = 0; i < polygon.Count; i++)
            {
                int j = (i + 1) % polygon.Count;
                if (needFlip)
                    result.Add(new Triangle3(centroid, polygon[j], polygon[i]));
                else
                    result.Add(new Triangle3(centroid, polygon[i], polygon[j]));
            }
        }

        return result;
    }

    /// <summary>
    /// Chains intersection segments into closed polygon loops by matching endpoints.
    /// </summary>
    private static List<List<Vec3>> ChainSegmentsIntoPolygons(
        Dictionary<int, List<IntersectionSegment>> faceSegments,
        double tolerance)
    {
        var segments = new List<(Vec3 Start, Vec3 End)>();
        foreach (var kvp in faceSegments)
            foreach (var seg in kvp.Value)
                segments.Add((seg.Start, seg.End));

        if (segments.Count == 0)
            return [];

        var toleranceSq = tolerance * tolerance;
        var used = new bool[segments.Count];
        var result = new List<List<Vec3>>();

        for (int startIdx = 0; startIdx < segments.Count; startIdx++)
        {
            if (used[startIdx]) continue;

            var polygon = new List<Vec3>();
            polygon.Add(segments[startIdx].Start);
            var current = segments[startIdx].End;
            used[startIdx] = true;

            for (int iter = 0; iter < segments.Count; iter++)
            {
                // Check if we've returned to the start
                if (Vec3.DistanceSquared(current, polygon[0]) < toleranceSq)
                    break;

                polygon.Add(current);

                // Find next unused segment connecting to current
                bool found = false;
                for (int i = 0; i < segments.Count; i++)
                {
                    if (used[i]) continue;

                    if (Vec3.DistanceSquared(segments[i].Start, current) < toleranceSq)
                    {
                        current = segments[i].End;
                        used[i] = true;
                        found = true;
                        break;
                    }
                    if (Vec3.DistanceSquared(segments[i].End, current) < toleranceSq)
                    {
                        current = segments[i].Start;
                        used[i] = true;
                        found = true;
                        break;
                    }
                }

                if (!found) break;
            }

            if (polygon.Count >= 3)
                result.Add(polygon);
        }

        return result;
    }
}
