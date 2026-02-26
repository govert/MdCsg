using MdCsg.Math;

namespace MdCsg.Api;

/// <summary>
/// Factory methods for creating standard geometric primitives as <see cref="Solid"/> objects.
/// </summary>
public static class Primitives
{
    /// <summary>
    /// Creates an axis-aligned box centered at <paramref name="center"/> with the given <paramref name="size"/>.
    /// </summary>
    /// <param name="center">Center of the box.</param>
    /// <param name="size">Full extent in each axis (width, height, depth).</param>
    /// <returns>A closed manifold box.</returns>
    public static Solid Box(Vec3 center, Vec3 size)
    {
        var half = size * 0.5;
        var min = center - half;
        var max = center + half;

        var positions = new Vec3[]
        {
            new(min.X, min.Y, min.Z), // 0
            new(max.X, min.Y, min.Z), // 1
            new(max.X, max.Y, min.Z), // 2
            new(min.X, max.Y, min.Z), // 3
            new(min.X, min.Y, max.Z), // 4
            new(max.X, min.Y, max.Z), // 5
            new(max.X, max.Y, max.Z), // 6
            new(min.X, max.Y, max.Z), // 7
        };

        var triangles = new (int, int, int)[]
        {
            (0, 2, 1), (0, 3, 2), // bottom (-Z)
            (4, 5, 6), (4, 6, 7), // top (+Z)
            (0, 1, 5), (0, 5, 4), // front (-Y)
            (2, 3, 7), (2, 7, 6), // back (+Y)
            (0, 4, 7), (0, 7, 3), // left (-X)
            (1, 2, 6), (1, 6, 5), // right (+X)
        };

        return Solid.FromIndexed(positions, triangles);
    }

    /// <summary>
    /// Creates an axis-aligned cube centered at <paramref name="center"/> with uniform <paramref name="extent"/>.
    /// </summary>
    /// <param name="center">Center of the cube.</param>
    /// <param name="extent">Full side length.</param>
    /// <returns>A closed manifold cube.</returns>
    public static Solid Cube(Vec3 center, double extent)
        => Box(center, new Vec3(extent, extent, extent));

    /// <summary>
    /// Creates an icosphere centered at <paramref name="center"/> with the given <paramref name="radius"/>.
    /// </summary>
    /// <param name="center">Center of the sphere.</param>
    /// <param name="radius">Radius of the sphere.</param>
    /// <param name="subdivisions">Number of icosphere subdivision passes (0 = icosahedron, 2 = 320 faces).</param>
    /// <returns>A closed manifold sphere approximation.</returns>
    public static Solid Sphere(Vec3 center, double radius, int subdivisions = 2)
    {
        double phi = (1.0 + System.Math.Sqrt(5.0)) / 2.0;
        var verts = new List<Vec3>
        {
            new Vec3(-1, phi, 0).Normalized * radius + center,
            new Vec3(1, phi, 0).Normalized * radius + center,
            new Vec3(-1, -phi, 0).Normalized * radius + center,
            new Vec3(1, -phi, 0).Normalized * radius + center,
            new Vec3(0, -1, phi).Normalized * radius + center,
            new Vec3(0, 1, phi).Normalized * radius + center,
            new Vec3(0, -1, -phi).Normalized * radius + center,
            new Vec3(0, 1, -phi).Normalized * radius + center,
            new Vec3(phi, 0, -1).Normalized * radius + center,
            new Vec3(phi, 0, 1).Normalized * radius + center,
            new Vec3(-phi, 0, -1).Normalized * radius + center,
            new Vec3(-phi, 0, 1).Normalized * radius + center,
        };

        var faces = new List<(int, int, int)>
        {
            (0, 11, 5), (0, 5, 1), (0, 1, 7), (0, 7, 10), (0, 10, 11),
            (1, 5, 9), (5, 11, 4), (11, 10, 2), (10, 7, 6), (7, 1, 8),
            (3, 9, 4), (3, 4, 2), (3, 2, 6), (3, 6, 8), (3, 8, 9),
            (4, 9, 5), (2, 4, 11), (6, 2, 10), (8, 6, 7), (9, 8, 1),
        };

        for (int s = 0; s < subdivisions; s++)
        {
            var newFaces = new List<(int, int, int)>();
            var midpointCache = new Dictionary<(int, int), int>();

            int GetMidpoint(int a, int b)
            {
                var key = a < b ? (a, b) : (b, a);
                if (midpointCache.TryGetValue(key, out int idx))
                    return idx;
                var mid = ((verts[a] + verts[b]) * 0.5 - center).Normalized * radius + center;
                idx = verts.Count;
                verts.Add(mid);
                midpointCache[key] = idx;
                return idx;
            }

            foreach (var (a, b, c) in faces)
            {
                int ab = GetMidpoint(a, b);
                int bc = GetMidpoint(b, c);
                int ca = GetMidpoint(c, a);

                newFaces.Add((a, ab, ca));
                newFaces.Add((b, bc, ab));
                newFaces.Add((c, ca, bc));
                newFaces.Add((ab, bc, ca));
            }

            faces = newFaces;
        }

        return Solid.FromIndexed(verts, faces);
    }

    /// <summary>
    /// Creates a cylinder along the given <paramref name="axis"/> direction.
    /// </summary>
    /// <param name="baseCenter">Center of the bottom cap.</param>
    /// <param name="axis">Direction of the cylinder axis (will be normalized).</param>
    /// <param name="radius">Radius of the cylinder.</param>
    /// <param name="height">Height of the cylinder along the axis.</param>
    /// <param name="segments">Number of segments around the circumference.</param>
    /// <returns>A closed manifold cylinder.</returns>
    public static Solid Cylinder(Vec3 baseCenter, Vec3 axis, double radius, double height, int segments = 24)
    {
        var (u, v, w) = BuildBasis(axis);
        var top = baseCenter + w * height;

        var verts = new List<Vec3>();
        var faces = new List<(int, int, int)>();

        // Bottom cap center (index 0), top cap center (index 1)
        verts.Add(baseCenter);
        verts.Add(top);

        // Bottom ring: indices [2, 2+segments)
        // Top ring: indices [2+segments, 2+2*segments)
        for (int i = 0; i < segments; i++)
        {
            double angle = 2.0 * System.Math.PI * i / segments;
            double cos = System.Math.Cos(angle);
            double sin = System.Math.Sin(angle);
            var offset = u * (cos * radius) + v * (sin * radius);
            verts.Add(baseCenter + offset);
        }
        for (int i = 0; i < segments; i++)
        {
            double angle = 2.0 * System.Math.PI * i / segments;
            double cos = System.Math.Cos(angle);
            double sin = System.Math.Sin(angle);
            var offset = u * (cos * radius) + v * (sin * radius);
            verts.Add(top + offset);
        }

        int bBase = 2;
        int tBase = 2 + segments;

        for (int i = 0; i < segments; i++)
        {
            int next = (i + 1) % segments;

            // Bottom cap (normal faces down — wind CW from above, so CCW from below)
            faces.Add((0, bBase + next, bBase + i));

            // Top cap (normal faces up — wind CCW from above)
            faces.Add((1, tBase + i, tBase + next));

            // Side quads (two triangles each)
            faces.Add((bBase + i, bBase + next, tBase + next));
            faces.Add((bBase + i, tBase + next, tBase + i));
        }

        return Solid.FromIndexed(verts, faces);
    }

    /// <summary>
    /// Creates a cone along the given <paramref name="axis"/> direction.
    /// </summary>
    /// <param name="baseCenter">Center of the base cap.</param>
    /// <param name="axis">Direction from base to apex (will be normalized).</param>
    /// <param name="radius">Radius of the base.</param>
    /// <param name="height">Height of the cone along the axis.</param>
    /// <param name="segments">Number of segments around the circumference.</param>
    /// <returns>A closed manifold cone.</returns>
    public static Solid Cone(Vec3 baseCenter, Vec3 axis, double radius, double height, int segments = 24)
    {
        var (u, v, w) = BuildBasis(axis);
        var apex = baseCenter + w * height;

        var verts = new List<Vec3>();
        var faces = new List<(int, int, int)>();

        // Base center (index 0), apex (index 1)
        verts.Add(baseCenter);
        verts.Add(apex);

        // Base ring: indices [2, 2+segments)
        for (int i = 0; i < segments; i++)
        {
            double angle = 2.0 * System.Math.PI * i / segments;
            double cos = System.Math.Cos(angle);
            double sin = System.Math.Sin(angle);
            verts.Add(baseCenter + u * (cos * radius) + v * (sin * radius));
        }

        int bBase = 2;

        for (int i = 0; i < segments; i++)
        {
            int next = (i + 1) % segments;

            // Base cap
            faces.Add((0, bBase + next, bBase + i));

            // Side triangle to apex
            faces.Add((bBase + i, bBase + next, 1));
        }

        return Solid.FromIndexed(verts, faces);
    }

    /// <summary>
    /// Creates a torus centered at <paramref name="center"/> with the given radii.
    /// </summary>
    /// <param name="center">Center of the torus.</param>
    /// <param name="axis">Axis of symmetry (will be normalized).</param>
    /// <param name="majorRadius">Distance from center to tube center.</param>
    /// <param name="minorRadius">Radius of the tube.</param>
    /// <param name="majorSegments">Number of segments around the major circle.</param>
    /// <param name="minorSegments">Number of segments around the tube cross-section.</param>
    /// <returns>A closed manifold torus.</returns>
    public static Solid Torus(Vec3 center, Vec3 axis, double majorRadius, double minorRadius,
        int majorSegments = 24, int minorSegments = 12)
    {
        var (u, v, w) = BuildBasis(axis);

        var verts = new List<Vec3>();
        var faces = new List<(int, int, int)>();

        // Generate vertices
        for (int i = 0; i < majorSegments; i++)
        {
            double theta = 2.0 * System.Math.PI * i / majorSegments;
            double cosTheta = System.Math.Cos(theta);
            double sinTheta = System.Math.Sin(theta);

            // Direction from center to tube center
            var ringCenter = center + u * (cosTheta * majorRadius) + v * (sinTheta * majorRadius);
            var radialDir = u * cosTheta + v * sinTheta;

            for (int j = 0; j < minorSegments; j++)
            {
                double phi = 2.0 * System.Math.PI * j / minorSegments;
                double cosPhi = System.Math.Cos(phi);
                double sinPhi = System.Math.Sin(phi);

                var pos = ringCenter + radialDir * (cosPhi * minorRadius) + w * (sinPhi * minorRadius);
                verts.Add(pos);
            }
        }

        // Generate faces
        for (int i = 0; i < majorSegments; i++)
        {
            int nextI = (i + 1) % majorSegments;
            for (int j = 0; j < minorSegments; j++)
            {
                int nextJ = (j + 1) % minorSegments;

                int v00 = i * minorSegments + j;
                int v10 = nextI * minorSegments + j;
                int v01 = i * minorSegments + nextJ;
                int v11 = nextI * minorSegments + nextJ;

                faces.Add((v00, v10, v11));
                faces.Add((v00, v11, v01));
            }
        }

        return Solid.FromIndexed(verts, faces);
    }

    /// <summary>
    /// Extrudes a 2D profile along a direction to create a solid.
    /// The profile is assumed to be a simple (non-self-intersecting) polygon in the local XY plane,
    /// mapped to 3D via a basis derived from the direction vector.
    /// </summary>
    /// <param name="profile">The 2D profile vertices (at least 3). Should be in CCW order.</param>
    /// <param name="direction">The extrusion direction (will be normalized).</param>
    /// <param name="height">The extrusion height along the direction.</param>
    /// <returns>A closed manifold solid.</returns>
    public static Solid Extrude(Vec2[] profile, Vec3 direction, double height)
    {
        if (profile.Length < 3)
            throw new ArgumentException("At least 3 profile vertices required.", nameof(profile));

        var (u, v, w) = BuildBasis(direction);
        int n = profile.Length;

        // Map 2D profile to 3D bottom and top positions
        var verts = new List<Vec3>();

        // Bottom ring: indices [0, n)
        for (int i = 0; i < n; i++)
            verts.Add(u * profile[i].X + v * profile[i].Y);

        // Top ring: indices [n, 2n)
        var offset = w * height;
        for (int i = 0; i < n; i++)
            verts.Add(u * profile[i].X + v * profile[i].Y + offset);

        var faces = new List<(int, int, int)>();

        // Side quads (two triangles each)
        for (int i = 0; i < n; i++)
        {
            int next = (i + 1) % n;
            // Bottom edge: i → next, Top edge: n+i → n+next
            // Outward-facing: CCW from outside
            faces.Add((i, next, n + next));
            faces.Add((i, n + next, n + i));
        }

        // Triangulate caps using ear clipping
        var capTriangles = EarClipTriangulate(profile);

        // Bottom cap: faces inward (reverse winding)
        foreach (var (a, b, c) in capTriangles)
            faces.Add((c, b, a));

        // Top cap: faces outward (same winding)
        foreach (var (a, b, c) in capTriangles)
            faces.Add((n + a, n + b, n + c));

        return Solid.FromIndexed(verts, faces);
    }

    /// <summary>
    /// Creates a solid of revolution by rotating a 2D profile around an axis.
    /// The profile is in the XZ-plane (X = radial distance, Y = height along axis).
    /// Profile points with X=0 are treated as on-axis and triangulated with fan triangles.
    /// </summary>
    /// <param name="profile">The 2D profile vertices (at least 2). X = radial distance from axis, Y = position along axis.</param>
    /// <param name="axis">The axis of revolution (will be normalized). The axis runs along the Y-component direction.</param>
    /// <param name="segments">Number of segments around the circumference.</param>
    /// <returns>A closed manifold solid.</returns>
    public static Solid Revolve(Vec2[] profile, Vec3 axis, int segments = 24)
    {
        if (profile.Length < 2)
            throw new ArgumentException("At least 2 profile vertices required.", nameof(profile));
        if (segments < 3)
            throw new ArgumentException("At least 3 segments required.", nameof(segments));

        var (u, v, w) = BuildBasis(axis);
        int n = profile.Length;

        var verts = new List<Vec3>();
        var faces = new List<(int, int, int)>();

        // For each profile vertex, generate ring of positions around the axis
        // Vertex layout: ring[i][j] = vertex at profile index i, segment j
        // Index: i * segments + j (for non-axis points)
        // Axis points (X≈0) get a single shared vertex

        // Track which profile points are on-axis vs off-axis
        var vertexIndex = new int[n, segments]; // maps (profileIdx, segmentIdx) → vertex index
        var isOnAxis = new bool[n];
        var axisVertexIdx = new int[n]; // single vertex index for on-axis points

        for (int i = 0; i < n; i++)
        {
            if (System.Math.Abs(profile[i].X) < 1e-10)
            {
                // On-axis point: single vertex
                isOnAxis[i] = true;
                axisVertexIdx[i] = verts.Count;
                verts.Add(w * profile[i].Y);
                for (int j = 0; j < segments; j++)
                    vertexIndex[i, j] = axisVertexIdx[i];
            }
            else
            {
                // Off-axis point: ring of vertices
                isOnAxis[i] = false;
                for (int j = 0; j < segments; j++)
                {
                    double angle = 2.0 * System.Math.PI * j / segments;
                    double cos = System.Math.Cos(angle);
                    double sin = System.Math.Sin(angle);
                    double r = profile[i].X;
                    vertexIndex[i, j] = verts.Count;
                    verts.Add(u * (cos * r) + v * (sin * r) + w * profile[i].Y);
                }
            }
        }

        // Connect adjacent profile segments with triangle strips or fans
        for (int i = 0; i < n - 1; i++)
        {
            int iNext = i + 1;

            for (int j = 0; j < segments; j++)
            {
                int jNext = (j + 1) % segments;

                int v00 = vertexIndex[i, j];
                int v01 = vertexIndex[i, jNext];
                int v10 = vertexIndex[iNext, j];
                int v11 = vertexIndex[iNext, jNext];

                if (isOnAxis[i] && isOnAxis[iNext])
                {
                    // Both on axis: degenerate, skip
                    continue;
                }
                else if (isOnAxis[i])
                {
                    // Fan triangle from axis point to next ring
                    if (v10 != v11) // avoid degenerate
                        faces.Add((v00, v11, v10));
                }
                else if (isOnAxis[iNext])
                {
                    // Fan triangle from ring to axis point
                    if (v00 != v01) // avoid degenerate
                        faces.Add((v00, v01, v10));
                }
                else
                {
                    // Quad: two triangles
                    faces.Add((v00, v11, v10));
                    faces.Add((v00, v01, v11));
                }
            }
        }

        return Solid.FromIndexed(verts, faces);
    }

    /// <summary>
    /// Triangulates a simple polygon using the ear-clipping algorithm.
    /// </summary>
    /// <param name="polygon">The polygon vertices in CCW order.</param>
    /// <returns>A list of triangle index triples (indices into the original polygon array).</returns>
    internal static List<(int, int, int)> EarClipTriangulate(Vec2[] polygon)
    {
        int n = polygon.Length;
        var triangles = new List<(int, int, int)>();
        if (n < 3) return triangles;

        // Build a linked list of vertex indices
        var indices = new List<int>(n);
        for (int i = 0; i < n; i++)
            indices.Add(i);

        // Ensure CCW orientation
        double signedArea = 0;
        for (int i = 0; i < n; i++)
        {
            var a = polygon[i];
            var b = polygon[(i + 1) % n];
            signedArea += a.X * b.Y - b.X * a.Y;
        }
        if (signedArea < 0)
            indices.Reverse();

        int maxIter = n * n; // safety limit
        int iter = 0;
        while (indices.Count > 2 && iter < maxIter)
        {
            iter++;
            bool earFound = false;

            for (int i = 0; i < indices.Count; i++)
            {
                int prev = (i + indices.Count - 1) % indices.Count;
                int next = (i + 1) % indices.Count;

                int ai = indices[prev];
                int bi = indices[i];
                int ci = indices[next];

                var a = polygon[ai];
                var b = polygon[bi];
                var c = polygon[ci];

                // Check if this is a convex vertex (left turn for CCW)
                double cross = (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
                if (cross <= 0)
                    continue; // reflex vertex

                // Check if any other vertex is inside this triangle
                bool containsPoint = false;
                for (int j = 0; j < indices.Count; j++)
                {
                    if (j == prev || j == i || j == next) continue;
                    if (PointInTriangle(polygon[indices[j]], a, b, c))
                    {
                        containsPoint = true;
                        break;
                    }
                }

                if (!containsPoint)
                {
                    triangles.Add((ai, bi, ci));
                    indices.RemoveAt(i);
                    earFound = true;
                    break;
                }
            }

            if (!earFound)
                break; // degenerate polygon
        }

        return triangles;
    }

    private static bool PointInTriangle(Vec2 p, Vec2 a, Vec2 b, Vec2 c)
    {
        double d1 = (p.X - b.X) * (a.Y - b.Y) - (a.X - b.X) * (p.Y - b.Y);
        double d2 = (p.X - c.X) * (b.Y - c.Y) - (b.X - c.X) * (p.Y - c.Y);
        double d3 = (p.X - a.X) * (c.Y - a.Y) - (c.X - a.X) * (p.Y - a.Y);

        bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(hasNeg && hasPos);
    }

    /// <summary>
    /// Builds an orthonormal basis (u, v, w) where w is the normalized input axis.
    /// </summary>
    private static (Vec3 U, Vec3 V, Vec3 W) BuildBasis(Vec3 axis)
    {
        var w = axis.Normalized;

        // Pick a vector not parallel to w
        var helper = System.Math.Abs(w.X) < 0.9
            ? new Vec3(1, 0, 0)
            : new Vec3(0, 1, 0);

        var u = Vec3.Cross(w, helper).Normalized;
        var v = Vec3.Cross(w, u);
        return (u, v, w);
    }
}
