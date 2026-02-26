namespace MdCsg.Math;

/// <summary>
/// A 2D polygon defined by an ordered sequence of vertices.
/// </summary>
public readonly struct Polygon2
{
    /// <summary>The vertices of the polygon in order.</summary>
    public Vec2[] Vertices { get; }

    /// <summary>
    /// Creates a polygon from an array of vertices.
    /// </summary>
    /// <param name="vertices">The ordered vertices. The array is stored directly (not copied).</param>
    public Polygon2(Vec2[] vertices)
    {
        Vertices = vertices;
    }

    /// <summary>
    /// Returns the signed area of the polygon using the shoelace formula.
    /// Positive for counter-clockwise winding, negative for clockwise.
    /// </summary>
    public double SignedArea
    {
        get
        {
            double sum = 0;
            int n = Vertices.Length;
            for (int i = 0; i < n; i++)
            {
                var a = Vertices[i];
                var b = Vertices[(i + 1) % n];
                sum += a.X * b.Y - b.X * a.Y;
            }
            return sum * 0.5;
        }
    }

    /// <summary>Returns the unsigned area of the polygon.</summary>
    public double Area => System.Math.Abs(SignedArea);

    /// <summary>Returns true if the polygon has clockwise winding (negative signed area).</summary>
    public bool IsClockwise => SignedArea < 0;

    /// <summary>
    /// Returns a new polygon with the vertex order reversed.
    /// </summary>
    /// <returns>A polygon with reversed winding.</returns>
    public Polygon2 Reversed()
    {
        var rev = new Vec2[Vertices.Length];
        for (int i = 0; i < Vertices.Length; i++)
            rev[i] = Vertices[Vertices.Length - 1 - i];
        return new Polygon2(rev);
    }

    /// <summary>
    /// Tests whether a point lies inside the polygon using the ray casting (crossing number) algorithm.
    /// </summary>
    /// <param name="point">The query point.</param>
    /// <returns>True if the point is inside the polygon.</returns>
    public bool Contains(Vec2 point)
    {
        int n = Vertices.Length;
        bool inside = false;
        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            var vi = Vertices[i];
            var vj = Vertices[j];
            if (((vi.Y > point.Y) != (vj.Y > point.Y)) &&
                (point.X < (vj.X - vi.X) * (point.Y - vi.Y) / (vj.Y - vi.Y) + vi.X))
            {
                inside = !inside;
            }
        }
        return inside;
    }

    /// <summary>
    /// Offsets the polygon by moving each vertex along the bisector of its adjacent edge normals.
    /// Uses miter-join. Positive distance expands a CCW polygon outward.
    /// </summary>
    /// <param name="distance">The offset distance (positive = outward for CCW polygons).</param>
    /// <returns>The offset polygon.</returns>
    public Polygon2 Offset(double distance)
    {
        int n = Vertices.Length;
        if (n < 3) return this;

        var result = new Vec2[n];
        for (int i = 0; i < n; i++)
        {
            int prev = (i + n - 1) % n;
            int next = (i + 1) % n;

            var e0 = Vertices[i] - Vertices[prev];
            var e1 = Vertices[next] - Vertices[i];

            // Outward normals (perpendicular, pointing right of edge direction for CCW polygon)
            var n0 = new Vec2(e0.Y, -e0.X);
            var n1 = new Vec2(e1.Y, -e1.X);

            double len0 = n0.Length;
            double len1 = n1.Length;
            if (len0 > 1e-30) n0 = n0 / len0;
            if (len1 > 1e-30) n1 = n1 / len1;

            var bisector = n0 + n1;
            double bisLen = bisector.Length;
            if (bisLen < 1e-30)
            {
                // Edges are parallel — use either normal
                result[i] = Vertices[i] + n0 * distance;
            }
            else
            {
                bisector = bisector / bisLen;
                // Miter: project distance along bisector
                double dot = Vec2.Dot(bisector, n0);
                if (System.Math.Abs(dot) < 1e-12) dot = 1e-12;
                double miterDist = distance / dot;
                result[i] = Vertices[i] + bisector * miterDist;
            }
        }
        return new Polygon2(result);
    }
}
