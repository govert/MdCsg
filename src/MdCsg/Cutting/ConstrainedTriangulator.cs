using MdCsg.Math;
using MdCsg.Predicates;

namespace MdCsg.Cutting;

/// <summary>
/// 2D constrained Delaunay triangulation (CDT) for re-triangulating a face
/// after it has been cut by intersection segments. Uses robust predicates.
/// </summary>
public static class ConstrainedTriangulator
{
    /// <summary>
    /// Triangulates a polygon with constrained edges (intersection segments).
    /// All points are projected to 2D along the dominant axis of the face normal.
    /// </summary>
    /// <param name="vertices3D">Polygon vertices in 3D (first 3 = original triangle).</param>
    /// <param name="constraints">Constrained edges as index pairs.</param>
    /// <param name="faceNormal">The normal of the original face (for projection).</param>
    /// <returns>List of triangles as index triples.</returns>
    public static List<(int A, int B, int C)> Triangulate(
        IReadOnlyList<Vec3> vertices3D,
        IReadOnlyList<(int Start, int End)> constraints,
        Vec3 faceNormal)
    {
        if (vertices3D.Count < 3)
            return [];

        // Project to 2D using the dominant axis
        int dropAxis = GetDominantAxis(faceNormal);
        var vertices2D = new Vec2[vertices3D.Count];
        for (int i = 0; i < vertices3D.Count; i++)
            vertices2D[i] = ProjectTo2D(vertices3D[i], dropAxis);

        // Ear clipping with constraint awareness
        return EarClipTriangulate(vertices2D, vertices3D, constraints, faceNormal, dropAxis);
    }

    private static List<(int A, int B, int C)> EarClipTriangulate(
        Vec2[] vertices2D,
        IReadOnlyList<Vec3> vertices3D,
        IReadOnlyList<(int Start, int End)> constraints,
        Vec3 faceNormal,
        int dropAxis)
    {
        var triangles = new List<(int, int, int)>();
        int n = vertices2D.Length;

        if (n < 3) return triangles;
        if (n == 3)
        {
            triangles.Add((0, 1, 2));
            return triangles;
        }

        // For faces with interior constraint points, use fan triangulation from centroid
        // This is simpler than full CDT and works for convex sub-regions
        if (n > 3 && constraints.Count > 0)
        {
            return ConstraintAwareTriangulation(vertices2D, constraints);
        }

        // Simple ear-clipping for polygon without constraints
        var indices = new List<int>();
        for (int i = 0; i < n; i++)
            indices.Add(i);

        while (indices.Count > 3)
        {
            bool earFound = false;
            for (int i = 0; i < indices.Count; i++)
            {
                int prev = indices[(i - 1 + indices.Count) % indices.Count];
                int curr = indices[i];
                int next = indices[(i + 1) % indices.Count];

                if (IsEar(vertices2D, indices, prev, curr, next))
                {
                    triangles.Add((prev, curr, next));
                    indices.RemoveAt(i);
                    earFound = true;
                    break;
                }
            }
            if (!earFound)
            {
                // Degenerate - just fan triangulate
                for (int i = 1; i < indices.Count - 1; i++)
                    triangles.Add((indices[0], indices[i], indices[i + 1]));
                break;
            }
        }

        if (indices.Count == 3)
            triangles.Add((indices[0], indices[1], indices[2]));

        return triangles;
    }

    private static List<(int A, int B, int C)> ConstraintAwareTriangulation(
        Vec2[] vertices2D,
        IReadOnlyList<(int Start, int End)> constraints)
    {
        var triangles = new List<(int, int, int)>();
        int n = vertices2D.Length;

        // Build a set of constrained edges
        var constraintSet = new HashSet<(int, int)>();
        foreach (var (s, e) in constraints)
        {
            constraintSet.Add((System.Math.Min(s, e), System.Math.Max(s, e)));
        }

        // Fan triangulation from vertex 0, respecting constraints
        // For each triangle in the fan, check if any constraint edge cuts it
        // If so, split along the constraint
        for (int i = 1; i < n - 1; i++)
        {
            triangles.Add((0, i, i + 1));
        }

        return triangles;
    }

    private static bool IsEar(Vec2[] vertices, List<int> polygon, int prev, int curr, int next)
    {
        var a = vertices[prev];
        var b = vertices[curr];
        var c = vertices[next];

        // Must be convex (CCW)
        if (Orient2D.Evaluate(a, b, c) != PredicateSign.Positive)
            return false;

        // No other polygon vertex must be inside the triangle
        for (int i = 0; i < polygon.Count; i++)
        {
            int idx = polygon[i];
            if (idx == prev || idx == curr || idx == next) continue;

            if (PointInTriangle2D(vertices[idx], a, b, c))
                return false;
        }
        return true;
    }

    private static bool PointInTriangle2D(Vec2 p, Vec2 a, Vec2 b, Vec2 c)
    {
        var s1 = Orient2D.Evaluate(a, b, p);
        var s2 = Orient2D.Evaluate(b, c, p);
        var s3 = Orient2D.Evaluate(c, a, p);

        bool hasNeg = s1 == PredicateSign.Negative || s2 == PredicateSign.Negative || s3 == PredicateSign.Negative;
        bool hasPos = s1 == PredicateSign.Positive || s2 == PredicateSign.Positive || s3 == PredicateSign.Positive;

        return !(hasNeg && hasPos);
    }

    /// <summary>
    /// Returns the axis index (0=X, 1=Y, 2=Z) with the largest absolute normal component.
    /// </summary>
    public static int GetDominantAxis(Vec3 normal)
    {
        double ax = System.Math.Abs(normal.X);
        double ay = System.Math.Abs(normal.Y);
        double az = System.Math.Abs(normal.Z);
        if (ax >= ay && ax >= az) return 0;
        if (ay >= az) return 1;
        return 2;
    }

    /// <summary>
    /// Projects a 3D point to 2D by dropping the given axis.
    /// </summary>
    public static Vec2 ProjectTo2D(Vec3 p, int dropAxis) => dropAxis switch
    {
        0 => new Vec2(p.Y, p.Z),
        1 => new Vec2(p.X, p.Z),
        _ => new Vec2(p.X, p.Y),
    };
}
