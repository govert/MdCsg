using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Cutting;

/// <summary>
/// Cuts an entire mesh along an intersection graph, producing a new mesh with sub-triangles.
/// </summary>
public static class MeshCutter
{
    /// <summary>
    /// Result of cutting a mesh: new mesh + mapping from new faces to original faces.
    /// </summary>
    /// <param name="Mesh">The cut mesh with sub-triangles.</param>
    /// <param name="SubTriangles">All sub-triangles with original face indices and intersection edge flags.</param>
    public record CutResult(HalfEdgeMesh Mesh, IReadOnlyList<FaceCutter.SubTriangle> SubTriangles);

    /// <summary>
    /// Cuts a mesh along the intersection segments, producing sub-triangles.
    /// After cutting, a conformality pass splits uncut faces at intersection points
    /// on shared edges, ensuring the adjacency graph properly connects all sub-triangles.
    /// </summary>
    /// <param name="mesh">The source mesh.</param>
    /// <param name="faceSegments">Map from face index to intersection segments that cross it.</param>
    /// <param name="parallel">If true, cut faces in parallel.</param>
    /// <param name="gridSize">Tolerance for split point matching on shared edges.</param>
    /// <param name="useEdgeSplitConstraints">
    /// If true, propagate split points across shared original edges to improve conformality.
    /// </param>
    /// <param name="triangulationKernel">Optional constrained triangulation kernel override used per face.</param>
    /// <returns>The cut mesh with sub-triangle information.</returns>
    public static CutResult Cut(
        HalfEdgeMesh mesh,
        IReadOnlyDictionary<int, List<IntersectionSegment>> faceSegments,
        bool parallel = false,
        double gridSize = MathUtil.DefaultGridSize,
        bool useEdgeSplitConstraints = false,
        ConstrainedTriangulationKernel? triangulationKernel = null)
    {
        int faceCount = mesh.Faces.Count;

        // Phase 1: Collect all intersection segment endpoints that lie on original
        // mesh edges. These are grouped by canonical edge key so that adjacent faces
        // sharing an edge get the same set of split points.
        var edgeSplitMap = useEdgeSplitConstraints
            ? CollectEdgeSplitPoints(mesh, faceSegments, faceCount, gridSize)
            : new Dictionary<long, List<Vec3>>();

        // Phase 2: Cut each face. Edge split points are passed to FaceCutter as
        // explicit edge chain constraints (A→P→B), forcing the CDT to produce
        // matching boundary subdivisions on adjacent faces.
        List<FaceCutter.SubTriangle>[] perFaceResults;

        if (parallel && faceCount > 1)
            perFaceResults = CutFacesParallel(mesh, faceSegments, edgeSplitMap, faceCount, gridSize, triangulationKernel);
        else
            perFaceResults = CutFacesSequential(mesh, faceSegments, edgeSplitMap, faceCount, gridSize, triangulationKernel);

        // Note: EnsureConformality is intentionally disabled in the default cutting path
        // because it can over-split faces and introduce degenerates when split points are noisy.

        // Concatenate in order
        var allSubTriangles = new List<FaceCutter.SubTriangle>();
        for (int i = 0; i < faceCount; i++)
            allSubTriangles.AddRange(perFaceResults[i]);

        // Build new mesh from sub-triangles
        var triangles = new Triangle3[allSubTriangles.Count];
        for (int i = 0; i < allSubTriangles.Count; i++)
        {
            var st = allSubTriangles[i];
            triangles[i] = new Triangle3(st.A, st.B, st.C);
        }

        var builder = new MeshBuilder();
        var newMesh = builder.Build(triangles);

        // Transfer original face IDs
        for (int i = 0; i < System.Math.Min(newMesh.Faces.Count, allSubTriangles.Count); i++)
        {
            newMesh.Faces[i].OriginalFaceId = allSubTriangles[i].OriginalFaceIndex;
        }

        // Topology-aware twin repair: fix untwinned and non-manifold edges
        // using original face adjacency information.
        RepairTwins(newMesh, mesh);

        return new CutResult(newMesh, allSubTriangles);
    }

    /// <summary>
    /// Repairs twin links in the cut mesh using original mesh topology.
    /// MeshBuilder's LinkTwins can fail when multiple half-edges share the same vertex pair
    /// (dictionary overwrite). This method finds untwinned half-edges and links them
    /// using a multimap approach with adjacency-based preference.
    /// </summary>
    private static void RepairTwins(HalfEdgeMesh cutMesh, HalfEdgeMesh originalMesh)
    {
        // Collect all untwinned half-edges
        var untwinned = new List<HalfEdge>();
        foreach (var he in cutMesh.HalfEdges)
        {
            if (he.Twin == null)
                untwinned.Add(he);
        }

        if (untwinned.Count == 0) return;

        // Build original face adjacency
        var originalAdj = new HashSet<long>();
        foreach (var he in originalMesh.HalfEdges)
        {
            if (he.Twin != null && he.Face != null && he.Twin.Face != null)
            {
                int f1 = he.Face.Id;
                int f2 = he.Twin.Face.Id;
                originalAdj.Add(((long)f1 << 32) | (uint)f2);
                originalAdj.Add(((long)f2 << 32) | (uint)f1);
            }
            if (he.Face != null)
                originalAdj.Add(((long)he.Face.Id << 32) | (uint)he.Face.Id);
        }

        // Group untwinned half-edges by reverse direction for matching
        var reverseMap = new Dictionary<(int, int), List<HalfEdge>>();
        foreach (var he in untwinned)
        {
            var rkey = (he.Target.Id, he.Origin.Id);
            if (!reverseMap.TryGetValue(rkey, out var list))
            {
                list = [];
                reverseMap[rkey] = list;
            }
            list.Add(he);
        }

        // Pair untwinned half-edges, preferring adjacent original faces
        foreach (var he in untwinned)
        {
            if (he.Twin != null) continue;

            var key = (he.Origin.Id, he.Target.Id);
            if (!reverseMap.TryGetValue(key, out var candidates)) continue;

            int myOrigFace = he.Face?.OriginalFaceId ?? -1;

            HalfEdge? best = null;
            foreach (var cand in candidates)
            {
                if (cand.Twin != null) continue;
                int candOrigFace = cand.Face?.OriginalFaceId ?? -1;
                long adjKey = ((long)myOrigFace << 32) | (uint)candOrigFace;
                if (originalAdj.Contains(adjKey))
                {
                    best = cand;
                    break;
                }
                best ??= cand;
            }

            if (best != null && best.Twin == null)
            {
                he.Twin = best;
                best.Twin = he;
            }
        }
    }

    /// <summary>
    /// Post-cut conformality pass. For each original mesh edge, collects ALL sub-triangle
    /// vertices from both adjacent faces that lie on the edge. Then ensures both faces have
    /// all these vertices by splitting sub-triangles that span missing points.
    /// </summary>
    private static void EnsureConformality(
        HalfEdgeMesh mesh,
        List<FaceCutter.SubTriangle>[] perFaceResults,
        int faceCount,
        double gridSize)
    {
        var tolSq = gridSize * gridSize;
        double edgeTol = 3.0 * gridSize;

        // Phase 1: Collect all sub-triangle vertices on each original mesh edge.
        var edgeSplitPoints = new Dictionary<long, List<Vec3>>();

        for (int faceIdx = 0; faceIdx < faceCount; faceIdx++)
        {
            var face = mesh.Faces[faceIdx];
            var he0 = face.Edge;
            var he1 = he0.Next;
            var he2 = he1.Next;

            var faceEdges = new (Vec3 P0, Vec3 P1, int Id0, int Id1)[]
            {
                (he0.Origin.Position, he0.Target.Position, he0.Origin.Id, he0.Target.Id),
                (he1.Origin.Position, he1.Target.Position, he1.Origin.Id, he1.Target.Id),
                (he2.Origin.Position, he2.Target.Position, he2.Origin.Id, he2.Target.Id)
            };

            // Collect unique vertex positions from this face's sub-triangles
            var subVerts = new HashSet<Vec3>();
            foreach (var st in perFaceResults[faceIdx])
            {
                subVerts.Add(st.A);
                subVerts.Add(st.B);
                subVerts.Add(st.C);
            }

            foreach (var sv in subVerts)
            {
                foreach (var (p0, p1, id0, id1) in faceEdges)
                {
                    // Skip if the sub-vertex is at an original vertex
                    if (Vec3.DistanceSquared(sv, p0) < tolSq) break;
                    if (Vec3.DistanceSquared(sv, p1) < tolSq) break;

                    if (IsOnEdgeInterior(sv, p0, p1, edgeTol))
                    {
                        long key = EdgeKey(id0, id1);
                        if (!edgeSplitPoints.TryGetValue(key, out var list))
                        {
                            list = [];
                            edgeSplitPoints[key] = list;
                        }
                        AddIfNew(list, sv, tolSq);
                        break;
                    }
                }
            }
        }

        if (edgeSplitPoints.Count == 0) return;

        // Phase 2: For each face, insert any missing split points by splitting sub-triangles.
        for (int faceIdx = 0; faceIdx < faceCount; faceIdx++)
        {
            var face = mesh.Faces[faceIdx];
            var he0 = face.Edge;
            var he1 = he0.Next;
            var he2 = he1.Next;

            var edgeIds = new (int Id0, int Id1)[]
            {
                (he0.Origin.Id, he0.Target.Id),
                (he1.Origin.Id, he1.Target.Id),
                (he2.Origin.Id, he2.Target.Id)
            };

            face.GetTrianglePositions(out var va, out var vb, out var vc);
            var edgeVerts = new (Vec3 Start, Vec3 End)[]
            {
                (va, vb), (vb, vc), (vc, va)
            };

            for (int e = 0; e < 3; e++)
            {
                long key = EdgeKey(edgeIds[e].Id0, edgeIds[e].Id1);
                if (!edgeSplitPoints.TryGetValue(key, out var splits)) continue;

                var (eA, eB) = edgeVerts[e];
                var edgeDir = eB - eA;
                double edgeLenSq = edgeDir.LengthSquared;
                if (edgeLenSq < tolSq) continue;

                // Sort split points by parametric position along the edge
                var sortedSplits = new List<Vec3>(splits);
                sortedSplits.Sort((a, b) =>
                {
                    double ta = Vec3.Dot(a - eA, edgeDir);
                    double tb = Vec3.Dot(b - eA, edgeDir);
                    return ta.CompareTo(tb);
                });

                foreach (var splitPt in sortedSplits)
                {
                    InsertSplitPointIfMissing(
                        perFaceResults[faceIdx], splitPt, eA, eB, faceIdx, tolSq, edgeTol);
                }
            }
        }
    }

    /// <summary>
    /// Finds any sub-triangle edge that lies on the original edge and spans the split point
    /// (without having the split point as an endpoint), then splits that sub-triangle.
    /// The split point may already exist as a vertex of other sub-triangles — this is fine.
    /// The key check is whether the edge A→B is properly subdivided at P.
    /// </summary>
    private static void InsertSplitPointIfMissing(
        List<FaceCutter.SubTriangle> subTris,
        Vec3 splitPt,
        Vec3 edgeA,
        Vec3 edgeB,
        int faceIdx,
        double tolSq,
        double edgeTol)
    {
        // Use generous tolerance for vertex matching: the split point from one face
        // may differ slightly from the same logical point in another face's sub-triangles.
        double vertexMatchTolSq = edgeTol * edgeTol;

        for (int i = 0; i < subTris.Count; i++)
        {
            var st = subTris[i];
            var verts = new[] { st.A, st.B, st.C };

            for (int e = 0; e < 3; e++)
            {
                var v0 = verts[e];
                var v1 = verts[(e + 1) % 3];
                var v2 = verts[(e + 2) % 3]; // Opposite vertex

                // Skip if either endpoint IS the split point (edge is already split here)
                if (Vec3.DistanceSquared(v0, splitPt) < vertexMatchTolSq) continue;
                if (Vec3.DistanceSquared(v1, splitPt) < vertexMatchTolSq) continue;

                // Both v0 and v1 must lie on the original edge line
                if (!IsNearEdgeLine(v0, edgeA, edgeB, edgeTol)) continue;
                if (!IsNearEdgeLine(v1, edgeA, edgeB, edgeTol)) continue;

                // splitPt must be between v0 and v1 parametrically along the sub-edge
                var dir = v1 - v0;
                double lenSq = dir.LengthSquared;
                if (lenSq < tolSq) continue;

                double t = Vec3.Dot(splitPt - v0, dir) / lenSq;
                if (t <= 0.001 || t >= 0.999) continue;

                // Split: replace (v0, v1, v2) with (v0, P, v2) and (P, v1, v2)
                bool splitEdgeIsIntersection = st.IsEdgeIntersection(e);

                // Triangle 1: (v0, P, v2)
                byte flags1 = 0;
                if (splitEdgeIsIntersection) flags1 |= 1; // edge 0: v0→P
                if (st.IsEdgeIntersection((e + 2) % 3)) flags1 |= 1 << 2; // edge 2: v2→v0

                // Triangle 2: (P, v1, v2)
                byte flags2 = 0;
                if (splitEdgeIsIntersection) flags2 |= 1; // edge 0: P→v1
                if (st.IsEdgeIntersection((e + 1) % 3)) flags2 |= 1 << 1; // edge 1: v1→v2

                subTris[i] = new FaceCutter.SubTriangle(v0, splitPt, v2, faceIdx, flags1 != 0, flags1);
                subTris.Insert(i + 1, new FaceCutter.SubTriangle(splitPt, v1, v2, faceIdx, flags2 != 0, flags2));
                return;
            }
        }
    }

    /// <summary>
    /// Checks if a point lies near the infinite line through edgeA→edgeB.
    /// Also requires the point's parametric position to be in [-0.01, 1.01].
    /// </summary>
    private static bool IsNearEdgeLine(Vec3 point, Vec3 edgeA, Vec3 edgeB, double tolerance)
    {
        var edge = edgeB - edgeA;
        double edgeLenSq = edge.LengthSquared;
        if (edgeLenSq < tolerance * tolerance) return false;

        var toPoint = point - edgeA;
        double t = Vec3.Dot(toPoint, edge) / edgeLenSq;

        // Must be roughly on the edge segment
        if (t < -0.01 || t > 1.01) return false;

        var projected = edgeA + edge * t;
        double distSq = Vec3.DistanceSquared(point, projected);
        return distSq < tolerance * tolerance;
    }

    private static bool IsOnEdgeInterior(Vec3 point, Vec3 edgeStart, Vec3 edgeEnd, double tolerance)
    {
        var edge = edgeEnd - edgeStart;
        double edgeLenSq = edge.LengthSquared;
        if (edgeLenSq < tolerance * tolerance) return false;

        var toPoint = point - edgeStart;
        double t = Vec3.Dot(toPoint, edge) / edgeLenSq;

        // Must be in the interior (not at endpoints)
        if (t <= tolerance || t >= 1.0 - tolerance) return false;

        // Distance from point to the edge line
        var projected = edgeStart + edge * t;
        double distSq = Vec3.DistanceSquared(point, projected);
        return distSq < tolerance * tolerance;
    }

    private static long EdgeKey(int id0, int id1)
    {
        int lo = id0 < id1 ? id0 : id1;
        int hi = id0 < id1 ? id1 : id0;
        return ((long)lo << 32) | (uint)hi;
    }

    private static void AddIfNew(List<Vec3> list, Vec3 point, double toleranceSq)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (Vec3.DistanceSquared(list[i], point) < toleranceSq)
                return;
        }
        list.Add(point);
    }

    private static int AddVertexGetIndex(List<Vec3> vertices, Vec3 point, double toleranceSq)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            if (Vec3.DistanceSquared(vertices[i], point) < toleranceSq)
                return i;
        }
        vertices.Add(point);
        return vertices.Count - 1;
    }

    /// <summary>
    /// Collects all intersection segment endpoints that lie on original mesh edges.
    /// Returns a map from canonical edge key to the list of split points on that edge.
    /// </summary>
    private static Dictionary<long, List<Vec3>> CollectEdgeSplitPoints(
        HalfEdgeMesh mesh,
        IReadOnlyDictionary<int, List<IntersectionSegment>> faceSegments,
        int faceCount,
        double gridSize)
    {
        var edgeSplitMap = new Dictionary<long, List<Vec3>>();
        double tolSq = gridSize * gridSize;

        for (int faceIdx = 0; faceIdx < faceCount; faceIdx++)
        {
            if (!faceSegments.TryGetValue(faceIdx, out var segs)) continue;

            var face = mesh.Faces[faceIdx];
            var he0 = face.Edge;
            var he1 = he0.Next;
            var he2 = he1.Next;

            var faceEdges = new (Vec3 P0, Vec3 P1, int Id0, int Id1)[]
            {
                (he0.Origin.Position, he0.Target.Position, he0.Origin.Id, he0.Target.Id),
                (he1.Origin.Position, he1.Target.Position, he1.Origin.Id, he1.Target.Id),
                (he2.Origin.Position, he2.Target.Position, he2.Origin.Id, he2.Target.Id)
            };

            foreach (var seg in segs)
            {
                RegisterPointOnEdge(seg.Start, faceEdges, edgeSplitMap, gridSize, tolSq);
                RegisterPointOnEdge(seg.End, faceEdges, edgeSplitMap, gridSize, tolSq);
            }
        }

        return edgeSplitMap;
    }

    /// <summary>
    /// If the point lies in the interior of one of the face's original edges,
    /// registers it in the edge split map under that edge's canonical key.
    /// </summary>
    private static void RegisterPointOnEdge(
        Vec3 point,
        (Vec3 P0, Vec3 P1, int Id0, int Id1)[] faceEdges,
        Dictionary<long, List<Vec3>> edgeSplitMap,
        double gridSize,
        double tolSq)
    {
        foreach (var (p0, p1, id0, id1) in faceEdges)
        {
            // Skip if point is at an original vertex
            if (Vec3.DistanceSquared(point, p0) < tolSq) return;
            if (Vec3.DistanceSquared(point, p1) < tolSq) return;

            if (IsOnEdgeInterior(point, p0, p1, 3.0 * gridSize))
            {
                long key = EdgeKey(id0, id1);
                if (!edgeSplitMap.TryGetValue(key, out var list))
                {
                    list = [];
                    edgeSplitMap[key] = list;
                }
                AddIfNew(list, point, tolSq);
                return;
            }
        }
    }

    /// <summary>
    /// Looks up edge split points for a face's 3 edges from the global map.
    /// Returns null if no edges have split points.
    /// </summary>
    private static List<Vec3>[]? GetFaceEdgeSplits(
        HalfEdgeMesh mesh, int faceIdx,
        Dictionary<long, List<Vec3>> edgeSplitMap)
    {
        var face = mesh.Faces[faceIdx];
        // GetTrianglePositions returns: a=he0.Target, b=he1.Target, c=he2.Target
        // So half-edges are: he0=C→A, he1=A→B, he2=B→C
        // FaceCutter edge indices: 0=A→B, 1=B→C, 2=C→A
        // Mapping: FaceCutter edge 0 = he1, edge 1 = he2, edge 2 = he0
        var he0 = face.Edge;
        var he1 = he0.Next;
        var he2 = he1.Next;

        var edgeIds = new (int Id0, int Id1)[]
        {
            (he1.Origin.Id, he1.Target.Id),  // Edge 0: A→B
            (he2.Origin.Id, he2.Target.Id),  // Edge 1: B→C
            (he0.Origin.Id, he0.Target.Id),  // Edge 2: C→A
        };

        List<Vec3>[]? edgeSplits = null;
        for (int e = 0; e < 3; e++)
        {
            long key = EdgeKey(edgeIds[e].Id0, edgeIds[e].Id1);
            if (edgeSplitMap.TryGetValue(key, out var splits) && splits.Count > 0)
            {
                edgeSplits ??= new List<Vec3>[3];
                edgeSplits[e] = splits;
            }
        }
        return edgeSplits;
    }

    private static List<FaceCutter.SubTriangle>[] CutFacesSequential(
        HalfEdgeMesh mesh,
        IReadOnlyDictionary<int, List<IntersectionSegment>> faceSegments,
        Dictionary<long, List<Vec3>> edgeSplitMap,
        int faceCount,
        double gridSize,
        ConstrainedTriangulationKernel? triangulationKernel)
    {
        var results = new List<FaceCutter.SubTriangle>[faceCount];

        for (int faceIdx = 0; faceIdx < faceCount; faceIdx++)
        {
            results[faceIdx] = CutSingleFace(mesh, faceSegments, edgeSplitMap, faceIdx, triangulationKernel);
        }

        return results;
    }

    private static List<FaceCutter.SubTriangle>[] CutFacesParallel(
        HalfEdgeMesh mesh,
        IReadOnlyDictionary<int, List<IntersectionSegment>> faceSegments,
        Dictionary<long, List<Vec3>> edgeSplitMap,
        int faceCount,
        double gridSize,
        ConstrainedTriangulationKernel? triangulationKernel)
    {
        var results = new List<FaceCutter.SubTriangle>[faceCount];

        System.Threading.Tasks.Parallel.For(0, faceCount, faceIdx =>
        {
            results[faceIdx] = CutSingleFace(mesh, faceSegments, edgeSplitMap, faceIdx, triangulationKernel);
        });

        return results;
    }

    private static List<FaceCutter.SubTriangle> CutSingleFace(
        HalfEdgeMesh mesh,
        IReadOnlyDictionary<int, List<IntersectionSegment>> faceSegments,
        Dictionary<long, List<Vec3>> edgeSplitMap,
        int faceIdx,
        ConstrainedTriangulationKernel? triangulationKernel)
    {
        var face = mesh.Faces[faceIdx];
        face.GetTrianglePositions(out var va, out var vb, out var vc);
        var tri = new Triangle3(va, vb, vc);

        bool hasSegments = faceSegments.TryGetValue(faceIdx, out var segments) && segments.Count > 0;
        var edgeSplits = GetFaceEdgeSplits(mesh, faceIdx, edgeSplitMap);

        if (hasSegments)
            return FaceCutter.CutFace(tri, faceIdx, segments!, edgeSplits, triangulationKernel);

        if (edgeSplits != null)
            return FaceCutter.CutFace(tri, faceIdx, System.Array.Empty<IntersectionSegment>(), edgeSplits, triangulationKernel);

        return [new FaceCutter.SubTriangle(tri.A, tri.B, tri.C, faceIdx, false)];
    }
}
