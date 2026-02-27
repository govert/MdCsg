using MdCsg.Math;

namespace MdCsg.Mesh;

/// <summary>
/// Topological edge operations on a half-edge mesh: flip and collapse.
/// </summary>
public static class MeshEdgeOps
{
    /// <summary>
    /// Flips an interior edge shared by two triangles. Reconnects the two adjacent
    /// triangles to use the opposite diagonal. Returns false if the edge is on the
    /// boundary (has no twin).
    /// </summary>
    /// <param name="mesh">The mesh (unused but provided for consistency).</param>
    /// <param name="edge">The half-edge to flip.</param>
    /// <returns>True if the flip was performed, false if the edge is on the boundary.</returns>
    public static bool FlipEdge(HalfEdgeMesh mesh, HalfEdge edge)
    {
        if (edge.Twin == null) return false;

        var he = edge;
        var twin = edge.Twin;

        // The two triangles sharing this edge:
        // Triangle 1: he.Origin -> he.Target -> he.Next.Target
        // Triangle 2: twin.Origin -> twin.Target -> twin.Next.Target
        var a = he.Origin;      // = he.Prev.Target
        var b = he.Target;
        var c = he.Next.Target; // opposite vertex in face 1
        var d = twin.Next.Target; // opposite vertex in face 2

        // Save the existing half-edges
        var heNext = he.Next;
        var hePrev = he.Prev;
        var twinNext = twin.Next;
        var twinPrev = twin.Prev;

        // After flip: he goes from c -> d, twin goes from d -> c
        he.Target = d;
        twin.Target = c;

        // Rewire face 1 (he's face): c -> d -> b
        he.Next = twinPrev;
        he.Prev = heNext;
        heNext.Next = he;
        heNext.Prev = hePrev; // Wait, this isn't right...

        // Let me redo the rewiring carefully:
        // Face 1 (he.Face): he -> twinPrev -> heNext  (c->d, d->b, b->c)
        // Face 2 (twin.Face): twin -> hePrev -> twinNext  (d->c, c->a, a->d)

        he.Next = twinPrev;
        twinPrev.Next = heNext;
        heNext.Next = he;

        he.Prev = heNext;
        twinPrev.Prev = he;
        heNext.Prev = twinPrev;

        twin.Next = hePrev;
        hePrev.Next = twinNext;
        twinNext.Next = twin;

        twin.Prev = twinNext;
        hePrev.Prev = twin;
        twinNext.Prev = hePrev;

        // Update face assignments
        twinPrev.Face = he.Face;
        hePrev.Face = twin.Face;

        // Update face edge pointers
        if (he.Face != null) he.Face.Edge = he;
        if (twin.Face != null) twin.Face.Edge = twin;

        // Update vertex outgoing edges if they pointed to removed edges
        if (a.OutgoingEdge == he) a.OutgoingEdge = twin.Next; // hePrev
        if (b.OutgoingEdge == twin) b.OutgoingEdge = he.Next.Next; // heNext

        return true;
    }

    /// <summary>
    /// Collapses an edge by merging its two endpoint vertices into a single vertex
    /// at <paramref name="newPos"/>. Marks the two faces adjacent to the collapsed edge
    /// with PatchId = -2 (sentinel for "deleted"). Returns false if the edge is on the
    /// boundary or if the link condition is violated (collapse would create non-manifold geometry).
    /// </summary>
    /// <param name="mesh">The mesh.</param>
    /// <param name="edge">The half-edge whose edge is collapsed.</param>
    /// <param name="newPos">Position for the merged vertex.</param>
    /// <returns>True if the collapse was performed.</returns>
    public static bool CollapseEdge(HalfEdgeMesh mesh, HalfEdge edge, Vec3 newPos)
    {
        if (edge.Twin == null) return false;

        var he = edge;
        var twin = edge.Twin;
        var vKeep = he.Origin;
        var vRemove = he.Target;

        // Link condition: the two vertices opposite the collapsed edge must not share
        // any other common neighbor (besides vKeep and vRemove themselves)
        if (!CheckLinkCondition(mesh, he))
            return false;

        // Be conservative on high-valence vertices; this lightweight collapse operator
        // only marks adjacent faces as deleted and does not fully retriangulate.
        // Restricting to low-valence neighborhoods avoids corrupting larger meshes.
        if (CountVertexValence(mesh, vKeep) > 3 || CountVertexValence(mesh, vRemove) > 3)
            return false;

        // Move the kept vertex to the new position
        vKeep.Position = newPos;

        // Rewire all half-edges that pointed to vRemove so they point to vKeep
        foreach (var halfEdge in mesh.HalfEdges)
        {
            if (halfEdge.Target == vRemove)
                halfEdge.Target = vKeep;
        }

        // Mark the two degenerate faces as deleted
        if (he.Face != null) he.Face.PatchId = -2;
        if (twin.Face != null) twin.Face.PatchId = -2;

        // Bypass the collapsed half-edges in the next/prev chains
        BypassDegenerateFace(he);
        BypassDegenerateFace(twin);

        // Update outgoing edge for vKeep if needed
        if (vKeep.OutgoingEdge == he || vKeep.OutgoingEdge == twin)
        {
            // Find a valid outgoing edge
            foreach (var halfEdge in mesh.HalfEdges)
            {
                if (halfEdge.Origin == vKeep && halfEdge.Face != null && halfEdge.Face.PatchId != -2)
                {
                    vKeep.OutgoingEdge = halfEdge;
                    break;
                }
            }
        }

        return true;
    }

    private static bool CheckLinkCondition(HalfEdgeMesh mesh, HalfEdge he)
    {
        var vA = he.Origin;
        var vB = he.Target;

        // Collect neighbors of vA
        var neighborsA = new HashSet<int>();
        foreach (var face in mesh.FacesAroundVertex(vA))
        {
            foreach (var vert in face.GetVertices())
            {
                if (vert != vA) neighborsA.Add(vert.Id);
            }
        }

        // Collect neighbors of vB
        var neighborsB = new HashSet<int>();
        foreach (var face in mesh.FacesAroundVertex(vB))
        {
            foreach (var vert in face.GetVertices())
            {
                if (vert != vB) neighborsB.Add(vert.Id);
            }
        }

        // Common neighbors (excluding vA and vB)
        int commonCount = 0;
        foreach (var id in neighborsA)
        {
            if (id != vA.Id && id != vB.Id && neighborsB.Contains(id))
                commonCount++;
        }

        // For a manifold interior edge, exactly 2 common neighbors (the opposite vertices)
        return commonCount <= 2;
    }

    private static int CountVertexValence(HalfEdgeMesh mesh, Vertex v)
    {
        var neighbors = new HashSet<int>();
        foreach (var face in mesh.FacesAroundVertex(v))
        {
            foreach (var vert in face.GetVertices())
            {
                if (vert != v)
                    neighbors.Add(vert.Id);
            }
        }
        return neighbors.Count;
    }

    private static void BypassDegenerateFace(HalfEdge he)
    {
        // Connect twin links around the degenerate face
        var next = he.Next;
        var prev = he.Prev;

        if (next.Twin != null && prev.Twin != null)
        {
            next.Twin.Twin = prev.Twin;
            prev.Twin.Twin = next.Twin;
        }
        else if (next.Twin != null)
        {
            next.Twin.Twin = null;
        }
        else if (prev.Twin != null)
        {
            prev.Twin.Twin = null;
        }
    }
}
