using MdCsg.Api;
using MdCsg.Math;

namespace MdCsg.Tests.TestHelpers;

/// <summary>
/// Factory methods for creating test meshes.
/// </summary>
public static class MeshFactory
{
    /// <summary>
    /// Creates a unit cube [0,1]^3 optionally offset.
    /// </summary>
    public static Solid CreateCube(Vec3 offset = default, double size = 1.0)
    {
        var positions = new Vec3[]
        {
            new Vec3(0, 0, 0) * size + offset,
            new Vec3(1, 0, 0) * size + offset,
            new Vec3(1, 1, 0) * size + offset,
            new Vec3(0, 1, 0) * size + offset,
            new Vec3(0, 0, 1) * size + offset,
            new Vec3(1, 0, 1) * size + offset,
            new Vec3(1, 1, 1) * size + offset,
            new Vec3(0, 1, 1) * size + offset,
        };

        var triangles = new (int, int, int)[]
        {
            (0, 2, 1), (0, 3, 2),       // bottom
            (4, 5, 6), (4, 6, 7),       // top
            (0, 1, 5), (0, 5, 4),       // front
            (2, 3, 7), (2, 7, 6),       // back
            (0, 4, 7), (0, 7, 3),       // left
            (1, 2, 6), (1, 6, 5),       // right
        };

        return Solid.FromIndexed(positions, triangles);
    }

    /// <summary>
    /// Creates a regular tetrahedron centered at the origin.
    /// </summary>
    public static Solid CreateTetrahedron(Vec3 offset = default, double size = 1.0)
    {
        double s = size;
        var positions = new Vec3[]
        {
            new Vec3(1, 1, 1) * s + offset,
            new Vec3(1, -1, -1) * s + offset,
            new Vec3(-1, 1, -1) * s + offset,
            new Vec3(-1, -1, 1) * s + offset,
        };

        var triangles = new (int, int, int)[]
        {
            (0, 1, 2),
            (0, 3, 1),
            (0, 2, 3),
            (1, 3, 2),
        };

        return Solid.FromIndexed(positions, triangles);
    }

    /// <summary>
    /// Creates an approximated sphere (icosphere) by subdividing an icosahedron.
    /// </summary>
    public static Solid CreateSphere(Vec3 center = default, double radius = 1.0, int subdivisions = 2)
    {
        // Start with icosahedron
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

        // Subdivide
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
}
