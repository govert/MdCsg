using MdCsg.Intersection;
using MdCsg.Math;

namespace MdCsg.Cutting;

/// <summary>
/// Cuts a single triangular face along intersection segments, producing sub-triangles.
/// </summary>
public static class FaceCutter
{
    /// <summary>
    /// A sub-triangle produced by cutting a face.
    /// </summary>
    /// <param name="A">First vertex position.</param>
    /// <param name="B">Second vertex position.</param>
    /// <param name="C">Third vertex position.</param>
    /// <param name="OriginalFaceIndex">Index of the original face that was cut.</param>
    /// <param name="HasIntersectionEdge">Whether any edge of this sub-triangle is an intersection edge.</param>
    /// <param name="IntersectionEdgeFlags">Per-edge flags: bit 0 = A-B, bit 1 = B-C, bit 2 = C-A.</param>
    public readonly record struct SubTriangle(Vec3 A, Vec3 B, Vec3 C, int OriginalFaceIndex, bool HasIntersectionEdge, byte IntersectionEdgeFlags = 0)
    {
        /// <summary>Returns true if the edge from vertex at index e to vertex at (e+1)%3 is an intersection edge.</summary>
        public bool IsEdgeIntersection(int e) => (IntersectionEdgeFlags & (1 << e)) != 0;
    }

    /// <summary>
    /// Cuts a triangle along the given intersection segments.
    /// </summary>
    /// <param name="triangle">The original triangle.</param>
    /// <param name="faceIndex">The original face index.</param>
    /// <param name="segments">Intersection segments that cross this triangle.</param>
    /// <returns>Sub-triangles after cutting.</returns>
    public static List<SubTriangle> CutFace(Triangle3 triangle, int faceIndex, IReadOnlyList<IntersectionSegment> segments)
    {
        if (segments.Count == 0)
        {
            return [new SubTriangle(triangle.A, triangle.B, triangle.C, faceIndex, false)];
        }

        // Collect all unique vertices: original triangle corners + intersection endpoints
        var vertices = new List<Vec3> { triangle.A, triangle.B, triangle.C };
        var constraintPairs = new List<(int Start, int End)>();

        foreach (var seg in segments)
        {
            int startIdx = AddOrFindVertex(vertices, seg.Start);
            int endIdx = AddOrFindVertex(vertices, seg.End);
            if (startIdx != endIdx)
                constraintPairs.Add((startIdx, endIdx));
        }

        // Triangulate with constraints
        var faceNormal = triangle.Normal;
        var triIndices = ConstrainedTriangulator.Triangulate(vertices, constraintPairs, faceNormal);

        var result = new List<SubTriangle>();
        foreach (var (a, b, c) in triIndices)
        {
            // Check each edge individually: A-B (bit 0), B-C (bit 1), C-A (bit 2)
            byte edgeFlags = 0;
            foreach (var (cs, ce) in constraintPairs)
            {
                if (IsEdgePair(a, b, cs, ce)) edgeFlags |= 1;      // A-B
                if (IsEdgePair(b, c, cs, ce)) edgeFlags |= 1 << 1;  // B-C
                if (IsEdgePair(c, a, cs, ce)) edgeFlags |= 1 << 2;  // C-A
            }

            result.Add(new SubTriangle(vertices[a], vertices[b], vertices[c], faceIndex, edgeFlags != 0, edgeFlags));
        }

        return result;
    }

    private static int AddOrFindVertex(List<Vec3> vertices, Vec3 point, double tolerance = 1e-8)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            if (Vec3.DistanceSquared(vertices[i], point) < tolerance * tolerance)
                return i;
        }
        vertices.Add(point);
        return vertices.Count - 1;
    }

    private static bool IsEdgePair(int e0, int e1, int cs, int ce)
    {
        return (e0 == cs && e1 == ce) || (e0 == ce && e1 == cs);
    }
}
