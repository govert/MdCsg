using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Operations;

/// <summary>
/// Stitches selected triangles into a manifold output mesh.
/// </summary>
public static class MeshStitcher
{
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
            RelinkBoundaryTwins(boundary);
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
        RelinkBoundaryTwins(boundary);
    }

    /// <summary>
    /// Closes boundary loops by triangulating each loop with a fan from its centroid.
    /// This fills gaps in the mesh (e.g., at the intersection curve where mesh A's
    /// and mesh B's sub-triangles don't share edges).
    /// </summary>
    /// <param name="mesh">The mesh to repair in-place.</param>
    public static void CloseBoundaryLoops(HalfEdgeMesh mesh)
    {
        // Phase 1: Collect boundary half-edges and build next-edge map.
        // For each boundary vertex, find the boundary half-edge starting at that vertex.
        var boundary = new List<HalfEdge>();
        var startMap = new Dictionary<int, List<HalfEdge>>();

        foreach (var he in mesh.HalfEdges)
        {
            if (he.Twin != null) continue;
            boundary.Add(he);
            int originId = he.Origin.Id;
            if (!startMap.TryGetValue(originId, out var list))
            {
                list = [];
                startMap[originId] = list;
            }
            list.Add(he);
        }

        if (boundary.Count < 3) return;

        // Phase 2: Chain boundary half-edges into loops.
        var used = new HashSet<int>();
        var loops = new List<List<HalfEdge>>();

        foreach (var start in boundary)
        {
            if (used.Contains(start.Id)) continue;

            var loop = new List<HalfEdge>();
            var current = start;

            while (!used.Contains(current.Id))
            {
                used.Add(current.Id);
                loop.Add(current);

                // Find next boundary half-edge: starts at current.Target
                int targetId = current.Target.Id;
                HalfEdge? next = null;

                if (startMap.TryGetValue(targetId, out var candidates))
                {
                    foreach (var c in candidates)
                    {
                        if (!used.Contains(c.Id))
                        {
                            next = c;
                            break;
                        }
                    }
                    // If all candidates are used, check if we're closing the loop
                    if (next == null)
                    {
                        foreach (var c in candidates)
                        {
                            if (c == start)
                            {
                                next = start; // loop closed
                                break;
                            }
                        }
                    }
                }

                if (next == null) break; // open chain
                if (next == start) break; // loop closed
                current = next;
            }

            // Only process closed loops with enough vertices for triangulation
            bool isClosed = loop.Count >= 3 &&
                            startMap.ContainsKey(loop[^1].Target.Id) &&
                            startMap[loop[^1].Target.Id].Contains(start);

            if (isClosed && loop.Count >= 3)
                loops.Add(loop);
        }

        if (loops.Count == 0) return;

        // Phase 3: Fill each boundary loop with fan triangulation.
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
                mesh.AddFace(he.Target, he.Origin, centroidVertex);
            }
        }

        // Phase 4: Link twins for new + existing boundary half-edges.
        // Build a map of all untwinned half-edges and pair them.
        var untwinned = new List<HalfEdge>();
        foreach (var he in mesh.HalfEdges)
        {
            if (he.Twin == null)
                untwinned.Add(he);
        }

        var edgeMap = new Dictionary<(int, int), List<HalfEdge>>();
        foreach (var he in untwinned)
        {
            var key = (he.Origin.Id, he.Target.Id);
            if (!edgeMap.TryGetValue(key, out var list))
            {
                list = new List<HalfEdge>(1);
                edgeMap[key] = list;
            }
            list.Add(he);
        }

        foreach (var he in untwinned)
        {
            if (he.Twin != null) continue;
            var twinKey = (he.Target.Id, he.Origin.Id);
            if (edgeMap.TryGetValue(twinKey, out var candidates))
            {
                for (int i = 0; i < candidates.Count; i++)
                {
                    var twin = candidates[i];
                    if (twin.Twin == null && twin != he)
                    {
                        he.Twin = twin;
                        twin.Twin = he;
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Links untwinned half-edges that have matching (reverse) vertex pairs.
    /// </summary>
    private static void RelinkBoundaryTwins(List<HalfEdge> boundary)
    {
        var edgeMap = new Dictionary<(int, int), HalfEdge>();
        foreach (var he in boundary)
        {
            if (he.Twin != null) continue;
            var key = (he.Origin.Id, he.Target.Id);
            if (!edgeMap.ContainsKey(key))
                edgeMap[key] = he;
        }

        foreach (var he in boundary)
        {
            if (he.Twin != null) continue;
            var twinKey = (he.Target.Id, he.Origin.Id);
            if (edgeMap.TryGetValue(twinKey, out var twin) && twin != he && twin.Twin == null)
            {
                he.Twin = twin;
                twin.Twin = he;
            }
        }
    }
}
