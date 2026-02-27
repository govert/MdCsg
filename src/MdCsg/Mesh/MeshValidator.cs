namespace MdCsg.Mesh;

/// <summary>
/// Validates topological properties of a half-edge mesh.
/// </summary>
public static class MeshValidator
{
    /// <summary>
    /// Checks whether every half-edge has a twin (the mesh is "closed" at the edge level).
    /// </summary>
    public static bool AllEdgesHaveTwins(HalfEdgeMesh mesh)
    {
        foreach (var he in mesh.HalfEdges)
        {
            if (he.Twin == null) return false;
        }
        return true;
    }

    /// <summary>
    /// Checks whether the mesh is manifold at edges: every edge has exactly one twin.
    /// </summary>
    public static bool IsEdgeManifold(HalfEdgeMesh mesh)
    {
        foreach (var he in mesh.HalfEdges)
        {
            if (he.Twin == null) return false;
            if (he.Twin.Twin != he) return false;
        }
        return true;
    }

    /// <summary>
    /// Checks whether all faces are consistently oriented (CCW winding) by verifying
    /// that each half-edge and its twin point in opposite directions.
    /// </summary>
    public static bool IsConsistentlyOriented(HalfEdgeMesh mesh)
    {
        foreach (var he in mesh.HalfEdges)
        {
            if (he.Twin == null) continue;
            // In consistent orientation, he goes from A to B and twin goes from B to A
            if (he.Origin.Id != he.Twin.Target.Id) return false;
            if (he.Target.Id != he.Twin.Origin.Id) return false;
        }
        return true;
    }

    /// <summary>
    /// Computes the Euler characteristic: V - E + F.
    /// For a closed orientable surface of genus g, this equals 2 - 2g.
    /// For a sphere (genus 0), this equals 2.
    /// </summary>
    public static int EulerCharacteristic(HalfEdgeMesh mesh)
    {
        int v = mesh.Vertices.Count;
        int e = mesh.HalfEdges.Count / 2; // each edge is represented by two half-edges
        int f = mesh.Faces.Count;
        return v - e + f;
    }

    /// <summary>
    /// Checks whether next/prev links form valid cycles for every face.
    /// </summary>
    public static bool HasValidFaceCycles(HalfEdgeMesh mesh)
    {
        foreach (var face in mesh.Faces)
        {
            var start = face.Edge;
            if (start == null) return false;

            var current = start;
            int count = 0;
            do
            {
                if (current.Face != face) return false;
                if (current.Next.Prev != current) return false;
                current = current.Next;
                count++;
                if (count > mesh.HalfEdges.Count) return false; // cycle detection
            } while (current != start);
        }
        return true;
    }

    /// <summary>
    /// Counts the number of boundary edges (half-edges without twins).
    /// A closed manifold mesh should have 0 boundary edges.
    /// </summary>
    public static int CountBoundaryEdges(HalfEdgeMesh mesh)
    {
        int count = 0;
        foreach (var he in mesh.HalfEdges)
        {
            if (he.Twin == null) count++;
        }
        return count;
    }

    /// <summary>
    /// Runs all validation checks and returns a summary.
    /// </summary>
    public static MeshValidationResult Validate(HalfEdgeMesh mesh)
    {
        return new MeshValidationResult
        {
            AllEdgesHaveTwins = AllEdgesHaveTwins(mesh),
            IsEdgeManifold = IsEdgeManifold(mesh),
            IsConsistentlyOriented = IsConsistentlyOriented(mesh),
            HasValidFaceCycles = HasValidFaceCycles(mesh),
            EulerCharacteristic = EulerCharacteristic(mesh),
            VertexCount = mesh.Vertices.Count,
            EdgeCount = mesh.HalfEdges.Count / 2,
            FaceCount = mesh.Faces.Count
        };
    }
}

/// <summary>
/// Result of mesh validation.
/// </summary>
public record MeshValidationResult
{
    /// <summary>Whether all edges have twins.</summary>
    public required bool AllEdgesHaveTwins { get; init; }

    /// <summary>Whether the mesh is edge-manifold.</summary>
    public required bool IsEdgeManifold { get; init; }

    /// <summary>Whether faces are consistently oriented.</summary>
    public required bool IsConsistentlyOriented { get; init; }

    /// <summary>Whether face cycles are valid.</summary>
    public required bool HasValidFaceCycles { get; init; }

    /// <summary>The Euler characteristic V-E+F.</summary>
    public required int EulerCharacteristic { get; init; }

    /// <summary>Number of vertices.</summary>
    public required int VertexCount { get; init; }

    /// <summary>Number of edges.</summary>
    public required int EdgeCount { get; init; }

    /// <summary>Number of faces.</summary>
    public required int FaceCount { get; init; }

    /// <summary>Whether the mesh passes all checks for a closed manifold.</summary>
    public bool IsClosedManifold => AllEdgesHaveTwins && IsEdgeManifold && IsConsistentlyOriented && HasValidFaceCycles;
}
