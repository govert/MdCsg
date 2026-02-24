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
    public readonly record struct SubTriangle(Vec3 A, Vec3 B, Vec3 C, int OriginalFaceIndex, bool HasIntersectionEdge);

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
            // Check if any edge of this sub-triangle is a constraint edge
            bool hasIntEdge = false;
            foreach (var (cs, ce) in constraintPairs)
            {
                if (SharesEdge(a, b, c, cs, ce))
                {
                    hasIntEdge = true;
                    break;
                }
            }

            result.Add(new SubTriangle(vertices[a], vertices[b], vertices[c], faceIndex, hasIntEdge));
        }

        return result;
    }

    private static int AddOrFindVertex(List<Vec3> vertices, Vec3 point, double tolerance = 1e-10)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            if (Vec3.DistanceSquared(vertices[i], point) < tolerance * tolerance)
                return i;
        }
        vertices.Add(point);
        return vertices.Count - 1;
    }

    private static bool SharesEdge(int a, int b, int c, int cs, int ce)
    {
        return (a == cs && b == ce) || (b == cs && a == ce) ||
               (b == cs && c == ce) || (c == cs && b == ce) ||
               (a == cs && c == ce) || (c == cs && a == ce);
    }
}
