using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Operations;

/// <summary>
/// Stitches selected triangles into a manifold output mesh.
/// </summary>
public static class MeshStitcher
{
    /// <summary>
    /// Boundary/incidence summary for a stitched mesh.
    /// </summary>
    /// <param name="BoundaryHalfEdgeCount">Number of half-edges with no twin.</param>
    /// <param name="OpenBoundaryVertexCount">Number of boundary vertices where in-degree != out-degree.</param>
    /// <param name="UnmatchedUndirectedEdgeCount">Number of undirected edges referenced exactly once.</param>
    /// <param name="NonManifoldUndirectedEdgeCount">Number of undirected edges referenced more than twice.</param>
    public readonly record struct BoundaryIncidenceSummary(
        int BoundaryHalfEdgeCount,
        int OpenBoundaryVertexCount,
        int UnmatchedUndirectedEdgeCount,
        int NonManifoldUndirectedEdgeCount);

    /// <summary>
    /// Deterministic boundary-loop assembly summary.
    /// </summary>
    /// <param name="ClosedLoopCount">Number of closed loops detected.</param>
    /// <param name="OpenChainCount">Number of open chains encountered.</param>
    /// <param name="AmbiguousBranchVertexCount">Number of branch vertices with multiple next-edge candidates.</param>
    /// <param name="SkippedDegenerateTriangleCount">Number of loop-fill fan triangles skipped as degenerate.</param>
    public readonly record struct BoundaryLoopAssemblySummary(
        int ClosedLoopCount,
        int OpenChainCount,
        int AmbiguousBranchVertexCount,
        int SkippedDegenerateTriangleCount);

    /// <summary>
    /// Creates a HalfEdgeMesh from a list of triangles.
    /// </summary>
    public static HalfEdgeMesh Stitch(IReadOnlyList<Triangle3> triangles, double weldTolerance = 1e-8)
    {
        var builder = new MeshBuilder(weldTolerance);
        return builder.Build(triangles);
    }

    /// <summary>
    /// Repairs boundary edges in the stitched mesh by finding untwinned half-edge pairs
    /// whose endpoints match within <paramref name="tolerance"/>, merging their vertices,
    /// and linking them as twins.
    /// This handles cases where MeshBuilder's vertex welding missed close vertices
    /// (e.g., when 3D edge snapping in FaceCutter moves shared intersection points
    /// to slightly different positions in each mesh).
    /// </summary>
    /// <param name="mesh">The mesh to repair in-place.</param>
    /// <param name="tolerance">Maximum distance for matching boundary endpoints.</param>
    public static void RepairBoundary(HalfEdgeMesh mesh, double tolerance)
    {
        // Phase 1: Collect boundary half-edges
        var boundary = new List<HalfEdge>();
        foreach (var he in mesh.HalfEdges)
        {
            if (he.Twin == null)
                boundary.Add(he);
        }

        if (boundary.Count < 2) return;

        var tolSq = tolerance * tolerance;

        // Phase 2: Find pairs of boundary half-edges that should be twins
        // (reverse direction with close endpoints)
        var vertexMerge = new Dictionary<int, Vertex>();
        var paired = new HashSet<int>();

        for (int i = 0; i < boundary.Count; i++)
        {
            var h1 = boundary[i];
            if (paired.Contains(h1.Id)) continue;

            var origin1 = h1.Origin.Position;
            var target1 = h1.Target.Position;

            double bestDist = double.MaxValue;
            int bestJ = -1;

            for (int j = i + 1; j < boundary.Count; j++)
            {
                var h2 = boundary[j];
                if (paired.Contains(h2.Id)) continue;

                // Check reverse direction: h2.Origin ≈ h1.Target AND h2.Target ≈ h1.Origin
                double d1 = Vec3.DistanceSquared(h2.Origin.Position, target1);
                double d2 = Vec3.DistanceSquared(h2.Target.Position, origin1);

                if (d1 < tolSq && d2 < tolSq)
                {
                    double totalDist = d1 + d2;
                    if (totalDist < bestDist)
                    {
                        bestDist = totalDist;
                        bestJ = j;
                    }
                }
            }

            if (bestJ >= 0)
            {
                var h2 = boundary[bestJ];
                paired.Add(h1.Id);
                paired.Add(h2.Id);

                // Queue vertex merges: h2's endpoints → h1's endpoints
                var originV1 = h1.Origin;
                var targetV1 = h1.Target;
                var originV2 = h2.Origin;  // should match targetV1
                var targetV2 = h2.Target;  // should match originV1

                if (originV2.Id != targetV1.Id && !vertexMerge.ContainsKey(originV2.Id))
                    vertexMerge[originV2.Id] = targetV1;
                if (targetV2.Id != originV1.Id && !vertexMerge.ContainsKey(targetV2.Id))
                    vertexMerge[targetV2.Id] = originV1;
            }
        }

        if (vertexMerge.Count == 0)
        {
            // No merges needed, but there may be boundary half-edges with
            // matching vertex IDs that just need twin linking (e.g., from
            // non-manifold duplicate edges in MeshBuilder.LinkTwins).
            RelinkBoundaryTwinsDeterministic(mesh);
            return;
        }

        // Phase 3: Resolve transitive chains (A→B, B→C becomes A→C)
        bool changed = true;
        while (changed)
        {
            changed = false;
            var keys = new List<int>(vertexMerge.Keys);
            foreach (var key in keys)
            {
                var target = vertexMerge[key];
                if (vertexMerge.TryGetValue(target.Id, out var deeper) && deeper.Id != target.Id)
                {
                    vertexMerge[key] = deeper;
                    changed = true;
                }
            }
        }

        // Phase 4: Apply vertex merges to all half-edges
        foreach (var he in mesh.HalfEdges)
        {
            if (vertexMerge.TryGetValue(he.Target.Id, out var replacement))
                he.Target = replacement;
        }

        // Phase 5: Re-link twins for previously-boundary half-edges
        RelinkBoundaryTwinsDeterministic(mesh);
    }

    /// <summary>
    /// Closes boundary loops by triangulating each loop with a fan from its centroid.
    /// This fills gaps in the mesh (e.g., at the intersection curve where mesh A's
    /// and mesh B's sub-triangles don't share edges).
    /// </summary>
    /// <param name="mesh">The mesh to repair in-place.</param>
    public static void CloseBoundaryLoops(HalfEdgeMesh mesh)
        => CloseBoundaryLoopsDeterministic(mesh);

    /// <summary>
    /// Closes deterministically assembled boundary loops by triangulating each loop with a fan from its centroid.
    /// </summary>
    /// <param name="mesh">The mesh to repair in-place.</param>
    /// <param name="skipDegenerateFillTriangles">When true, skip zero-area fan triangles during loop fill.</param>
    /// <returns>Loop-assembly summary (including ambiguity/open-chain counts).</returns>
    public static BoundaryLoopAssemblySummary CloseBoundaryLoopsDeterministic(
        HalfEdgeMesh mesh,
        bool skipDegenerateFillTriangles = false)
    {
        var loops = CollectBoundaryLoopsDeterministic(
            mesh,
            out int openChains,
            out int ambiguousBranchVertices);
        if (loops.Count == 0)
            return new BoundaryLoopAssemblySummary(
                ClosedLoopCount: 0,
                OpenChainCount: openChains,
                AmbiguousBranchVertexCount: ambiguousBranchVertices,
                SkippedDegenerateTriangleCount: 0);

        // Phase 3: Fill each boundary loop with fan triangulation.
        int skippedDegenerateTriangles = 0;
        foreach (var loop in loops)
        {
            // Compute centroid
            var centroid = Vec3.Zero;
            foreach (var he in loop)
                centroid = centroid + he.Origin.Position;
            centroid = centroid / loop.Count;

            // Add centroid vertex
            var centroidVertex = mesh.AddVertex(centroid);

            // Create fan triangles.
            // Each boundary half-edge he goes Origin→Target along the boundary.
            // The fill triangle should create the reverse edge Target→Origin.
            // Fill triangle: (Target, Origin, Centroid) so that:
            //   edge0: Target→Origin = twin of boundary he
            //   edge1: Origin→Centroid
            //   edge2: Centroid→Target
            for (int i = 0; i < loop.Count; i++)
            {
                var he = loop[i];
                if (skipDegenerateFillTriangles
                    && IsDegenerateFillTriangle(he.Target.Position, he.Origin.Position, centroid))
                {
                    skippedDegenerateTriangles++;
                    continue;
                }

                mesh.AddFace(he.Target, he.Origin, centroidVertex);
            }
        }

        // Phase 4: deterministically relink twins for new + existing boundary half-edges.
        RelinkBoundaryTwinsDeterministic(mesh);
        return new BoundaryLoopAssemblySummary(
            ClosedLoopCount: loops.Count,
            OpenChainCount: openChains,
            AmbiguousBranchVertexCount: ambiguousBranchVertices,
            SkippedDegenerateTriangleCount: skippedDegenerateTriangles);
    }

    /// <summary>
    /// Analyzes deterministic boundary-loop assembly without mutating the mesh.
    /// </summary>
    public static BoundaryLoopAssemblySummary AnalyzeBoundaryLoopAssembly(HalfEdgeMesh mesh)
    {
        var loops = CollectBoundaryLoopsDeterministic(
            mesh,
            out int openChains,
            out int ambiguousBranchVertices);
        return new BoundaryLoopAssemblySummary(
            ClosedLoopCount: loops.Count,
            OpenChainCount: openChains,
            AmbiguousBranchVertexCount: ambiguousBranchVertices,
            SkippedDegenerateTriangleCount: 0);
    }

    private static bool IsDegenerateFillTriangle(Vec3 a, Vec3 b, Vec3 c)
    {
        double abSq = Vec3.DistanceSquared(a, b);
        double bcSq = Vec3.DistanceSquared(b, c);
        double caSq = Vec3.DistanceSquared(c, a);
        if (abSq <= 0.0 || bcSq <= 0.0 || caSq <= 0.0)
            return true;

        var cross = Vec3.Cross(b - a, c - a);
        return cross.X == 0.0 && cross.Y == 0.0 && cross.Z == 0.0;
    }

    private static List<List<HalfEdge>> CollectBoundaryLoopsDeterministic(
        HalfEdgeMesh mesh,
        out int openChains,
        out int ambiguousBranchVertices)
    {
        openChains = 0;
        ambiguousBranchVertices = 0;

        // Phase 1: collect boundary half-edges and next-edge map (legacy-compatible).
        var boundary = new List<HalfEdge>();
        var startMap = new Dictionary<int, List<HalfEdge>>();
        foreach (var he in mesh.HalfEdges)
        {
            if (he.Twin != null)
                continue;

            boundary.Add(he);
            int originId = he.Origin.Id;
            if (!startMap.TryGetValue(originId, out var list))
            {
                list = [];
                startMap[originId] = list;
            }
            list.Add(he);
        }

        if (boundary.Count < 3)
            return [];

        boundary.Sort(static (a, b) => a.Id.CompareTo(b.Id));
        foreach (var list in startMap.Values)
            list.Sort(static (a, b) => a.Id.CompareTo(b.Id));

        // Phase 2: deterministically chain boundary half-edges into loops.
        var used = new HashSet<int>();
        var loops = new List<List<HalfEdge>>();

        foreach (var start in boundary)
        {
            if (used.Contains(start.Id))
                continue;

            var loop = new List<HalfEdge>();
            var current = start;

            while (!used.Contains(current.Id))
            {
                used.Add(current.Id);
                loop.Add(current);

                int targetId = current.Target.Id;
                HalfEdge? next = null;
                if (startMap.TryGetValue(targetId, out var candidates))
                {
                    int unvisitedCount = 0;
                    for (int i = 0; i < candidates.Count; i++)
                    {
                        var candidate = candidates[i];
                        if (used.Contains(candidate.Id))
                            continue;

                        if (next is null)
                            next = candidate;
                        unvisitedCount++;
                    }

                    if (unvisitedCount > 1)
                        ambiguousBranchVertices++;

                    if (next is null)
                    {
                        // If all candidates are used, check if we're closing the loop.
                        for (int i = 0; i < candidates.Count; i++)
                        {
                            if (candidates[i] == start)
                            {
                                next = start;
                                break;
                            }
                        }
                    }
                }

                if (next is null)
                {
                    openChains++;
                    break;
                }

                if (next == start)
                    break;

                current = next;
            }

            bool isClosed = loop.Count >= 3
                && startMap.ContainsKey(loop[^1].Target.Id)
                && startMap[loop[^1].Target.Id].Contains(start);
            if (isClosed)
                loops.Add(loop);
        }

        return loops;
    }

    /// <summary>
    /// Deterministically links untwinned half-edges that have matching (reverse) vertex pairs.
    /// Returns the number of twin links created.
    /// </summary>
    public static int RelinkBoundaryTwinsDeterministic(HalfEdgeMesh mesh)
    {
        var boundary = new List<HalfEdge>();
        foreach (var he in mesh.HalfEdges)
        {
            if (he.Twin != null)
                continue;
            boundary.Add(he);
        }

        if (boundary.Count < 2)
            return 0;

        boundary.Sort(static (a, b) => a.Id.CompareTo(b.Id));

        var edgeMap = new Dictionary<(int, int), List<HalfEdge>>();
        foreach (var he in boundary)
        {
            var key = (he.Origin.Id, he.Target.Id);
            if (!edgeMap.TryGetValue(key, out var list))
            {
                list = [];
                edgeMap[key] = list;
            }
            list.Add(he);
        }

        foreach (var list in edgeMap.Values)
            list.Sort(static (a, b) => a.Id.CompareTo(b.Id));

        int linked = 0;
        foreach (var he in boundary)
        {
            if (he.Twin != null)
                continue;

            var twinKey = (he.Target.Id, he.Origin.Id);
            if (!edgeMap.TryGetValue(twinKey, out var candidates))
                continue;

            for (int i = 0; i < candidates.Count; i++)
            {
                var twin = candidates[i];
                if (twin == he || twin.Twin != null)
                    continue;

                he.Twin = twin;
                twin.Twin = he;
                linked++;
                break;
            }
        }

        return linked;
    }

    /// <summary>
    /// Computes deterministic boundary/incidence metrics for the current mesh topology.
    /// </summary>
    /// <param name="mesh">Mesh to analyze.</param>
    /// <returns>Boundary and undirected-edge incidence summary.</returns>
    public static BoundaryIncidenceSummary AnalyzeBoundaryIncidence(HalfEdgeMesh mesh)
    {
        int boundaryHalfEdges = 0;
        var outgoing = new Dictionary<int, int>();
        var incoming = new Dictionary<int, int>();
        var undirectedEdgeUse = new Dictionary<long, int>(mesh.HalfEdges.Count);

        foreach (var he in mesh.HalfEdges)
        {
            int a = he.Origin.Id;
            int b = he.Target.Id;
            int lo = a < b ? a : b;
            int hi = a < b ? b : a;
            long key = ((long)lo << 32) | (uint)hi;
            undirectedEdgeUse.TryGetValue(key, out int use);
            undirectedEdgeUse[key] = use + 1;

            if (he.Twin == null)
            {
                boundaryHalfEdges++;
                outgoing.TryGetValue(a, out int outCount);
                outgoing[a] = outCount + 1;
                incoming.TryGetValue(b, out int inCount);
                incoming[b] = inCount + 1;
            }
        }

        int openBoundaryVertices = 0;
        var boundaryVertices = new HashSet<int>(outgoing.Keys);
        foreach (int v in incoming.Keys)
            boundaryVertices.Add(v);

        foreach (int v in boundaryVertices)
        {
            outgoing.TryGetValue(v, out int outCount);
            incoming.TryGetValue(v, out int inCount);
            if (outCount != inCount)
                openBoundaryVertices++;
        }

        int unmatchedUndirected = 0;
        int nonManifoldUndirected = 0;
        foreach (int useCount in undirectedEdgeUse.Values)
        {
            if (useCount == 1)
                unmatchedUndirected++;
            else if (useCount > 2)
                nonManifoldUndirected++;
        }

        return new BoundaryIncidenceSummary(
            boundaryHalfEdges,
            openBoundaryVertices,
            unmatchedUndirected,
            nonManifoldUndirected);
    }
}
