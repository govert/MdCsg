using MdCsg.Api;
using MdCsg.Math;

namespace MdCsg.Tests.Api;

/// <summary>
/// Geometric property tests for all primitive types.
/// Verifies vertex positions, face counts, mesh validity across parameter ranges.
/// </summary>
public class PrimitivesGeometryTests
{
    // =========================================================================
    // Cube: vertex count and face count at various sizes
    // =========================================================================

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(2.0)]
    [InlineData(5.0)]
    [InlineData(10.0)]
    [InlineData(100.0)]
    [InlineData(0.01)]
    public void Cube_HasCorrectTopology(double size)
    {
        var cube = Primitives.Cube(Vec3.Zero, size);
        Assert.Equal(8, cube.Mesh.Vertices.Count);
        Assert.Equal(12, cube.Mesh.Faces.Count);
    }

    // =========================================================================
    // Cube: vertices within expected bounds
    // =========================================================================

    [Theory]
    [InlineData(0, 0, 0, 1.0)]
    [InlineData(1, 2, 3, 2.0)]
    [InlineData(-5, -5, -5, 10.0)]
    [InlineData(100, 200, 300, 0.5)]
    public void Cube_VerticesWithinBounds(double cx, double cy, double cz, double size)
    {
        var center = new Vec3(cx, cy, cz);
        var cube = Primitives.Cube(center, size);
        double half = size / 2.0;
        foreach (var v in cube.Mesh.Vertices)
        {
            Assert.True(v.Position.X >= cx - half - 1e-10 && v.Position.X <= cx + half + 1e-10);
            Assert.True(v.Position.Y >= cy - half - 1e-10 && v.Position.Y <= cy + half + 1e-10);
            Assert.True(v.Position.Z >= cz - half - 1e-10 && v.Position.Z <= cz + half + 1e-10);
        }
    }

    // =========================================================================
    // Box: various dimensions
    // =========================================================================

    [Theory]
    [InlineData(1.0, 2.0, 3.0)]
    [InlineData(0.5, 0.5, 0.5)]
    [InlineData(10.0, 1.0, 0.1)]
    [InlineData(100.0, 100.0, 100.0)]
    [InlineData(0.01, 0.01, 0.01)]
    public void Box_HasCorrectTopology(double sx, double sy, double sz)
    {
        var box = Primitives.Box(Vec3.Zero, new Vec3(sx, sy, sz));
        Assert.Equal(8, box.Mesh.Vertices.Count);
        Assert.Equal(12, box.Mesh.Faces.Count);
    }

    [Theory]
    [InlineData(0, 0, 0, 2.0, 3.0, 4.0)]
    [InlineData(1, 2, 3, 1.0, 1.0, 1.0)]
    [InlineData(-5, 0, 5, 10.0, 20.0, 30.0)]
    public void Box_VerticesWithinBounds(double cx, double cy, double cz, double sx, double sy, double sz)
    {
        var center = new Vec3(cx, cy, cz);
        var box = Primitives.Box(center, new Vec3(sx, sy, sz));
        double hx = sx / 2.0, hy = sy / 2.0, hz = sz / 2.0;
        foreach (var v in box.Mesh.Vertices)
        {
            Assert.True(v.Position.X >= cx - hx - 1e-10 && v.Position.X <= cx + hx + 1e-10);
            Assert.True(v.Position.Y >= cy - hy - 1e-10 && v.Position.Y <= cy + hy + 1e-10);
            Assert.True(v.Position.Z >= cz - hz - 1e-10 && v.Position.Z <= cz + hz + 1e-10);
        }
    }

    // =========================================================================
    // Sphere: vertex count scales with subdivision
    // =========================================================================

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Sphere_VertexCountScalesWithSubdivision(int subdiv)
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, subdiv);
        Assert.True(sphere.Mesh.Vertices.Count > 0);
        Assert.True(sphere.Mesh.Faces.Count > 0);
        if (subdiv > 0)
        {
            var lower = Primitives.Sphere(Vec3.Zero, 1.0, subdiv - 1);
            Assert.True(sphere.Mesh.Vertices.Count > lower.Mesh.Vertices.Count);
        }
    }

    // =========================================================================
    // Sphere: all vertices at expected radius
    // =========================================================================

    [Theory]
    [InlineData(1.0, 0)]
    [InlineData(1.0, 1)]
    [InlineData(1.0, 2)]
    [InlineData(2.0, 1)]
    [InlineData(5.0, 1)]
    [InlineData(0.5, 2)]
    [InlineData(10.0, 0)]
    public void Sphere_VerticesAtExpectedRadius(double radius, int subdiv)
    {
        var center = new Vec3(1, 2, 3);
        var sphere = Primitives.Sphere(center, radius, subdiv);
        foreach (var v in sphere.Mesh.Vertices)
        {
            double dist = Vec3.Distance(v.Position, center);
            Assert.True(System.Math.Abs(dist - radius) < 1e-6,
                $"Vertex at distance {dist}, expected {radius}");
        }
    }

    // =========================================================================
    // Sphere: various centers and radii
    // =========================================================================

    [Theory]
    [InlineData(0, 0, 0, 1.0)]
    [InlineData(5, 5, 5, 2.0)]
    [InlineData(-10, 0, 10, 0.5)]
    [InlineData(100, 200, 300, 10.0)]
    [InlineData(0, 0, 0, 0.01)]
    public void Sphere_HasFaces(double cx, double cy, double cz, double r)
    {
        var sphere = Primitives.Sphere(new Vec3(cx, cy, cz), r, 1);
        Assert.True(sphere.Mesh.Faces.Count > 0);
    }

    // =========================================================================
    // Cylinder: various parameters
    // =========================================================================

    [Theory]
    [InlineData(1, 0, 0, 0.5, 2.0, 8)]
    [InlineData(0, 1, 0, 0.5, 2.0, 8)]
    [InlineData(0, 0, 1, 0.5, 2.0, 8)]
    [InlineData(0, 0, 1, 1.0, 1.0, 12)]
    [InlineData(0, 0, 1, 1.0, 1.0, 16)]
    [InlineData(0, 0, 1, 1.0, 1.0, 32)]
    [InlineData(0, 0, 1, 0.1, 10.0, 8)]
    [InlineData(0, 0, 1, 5.0, 0.1, 8)]
    public void Cylinder_HasFaces(double ax, double ay, double az, double radius, double height, int segments)
    {
        var cyl = Primitives.Cylinder(Vec3.Zero, new Vec3(ax, ay, az), radius, height, segments);
        Assert.True(cyl.Mesh.Faces.Count > 0);
        Assert.True(cyl.Mesh.Vertices.Count > 0);
    }

    // =========================================================================
    // Cylinder: more segments → more vertices
    // =========================================================================

    [Theory]
    [InlineData(8, 12)]
    [InlineData(12, 16)]
    [InlineData(16, 32)]
    [InlineData(32, 64)]
    public void Cylinder_MoreSegmentsMoreVertices(int segsLow, int segsHigh)
    {
        var low = Primitives.Cylinder(Vec3.Zero, Vec3.UnitZ, 1.0, 2.0, segsLow);
        var high = Primitives.Cylinder(Vec3.Zero, Vec3.UnitZ, 1.0, 2.0, segsHigh);
        Assert.True(high.Mesh.Vertices.Count > low.Mesh.Vertices.Count);
    }

    // =========================================================================
    // Cylinder: at various centers
    // =========================================================================

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(5, 0, 0)]
    [InlineData(0, 5, 0)]
    [InlineData(0, 0, 5)]
    [InlineData(-10, 20, -30)]
    [InlineData(100, 200, 300)]
    public void Cylinder_AtVariousCenters(double cx, double cy, double cz)
    {
        var cyl = Primitives.Cylinder(new Vec3(cx, cy, cz), Vec3.UnitZ, 1.0, 2.0);
        Assert.True(cyl.Mesh.Faces.Count > 0);
    }

    // =========================================================================
    // Cone: various parameters
    // =========================================================================

    [Theory]
    [InlineData(0, 0, 1, 1.0, 2.0, 8)]
    [InlineData(0, 0, 1, 1.0, 2.0, 12)]
    [InlineData(0, 0, 1, 1.0, 2.0, 16)]
    [InlineData(0, 0, 1, 0.5, 1.0, 8)]
    [InlineData(0, 0, 1, 2.0, 0.5, 8)]
    [InlineData(1, 0, 0, 1.0, 2.0, 8)]
    [InlineData(0, 1, 0, 1.0, 2.0, 8)]
    public void Cone_HasFaces(double ax, double ay, double az, double radius, double height, int segments)
    {
        var cone = Primitives.Cone(Vec3.Zero, new Vec3(ax, ay, az), radius, height, segments);
        Assert.True(cone.Mesh.Faces.Count > 0);
        Assert.True(cone.Mesh.Vertices.Count > 0);
    }

    // =========================================================================
    // Cone: more segments → more faces
    // =========================================================================

    [Theory]
    [InlineData(8, 12)]
    [InlineData(12, 16)]
    [InlineData(16, 32)]
    public void Cone_MoreSegmentsMoreFaces(int segsLow, int segsHigh)
    {
        var low = Primitives.Cone(Vec3.Zero, Vec3.UnitZ, 1.0, 2.0, segsLow);
        var high = Primitives.Cone(Vec3.Zero, Vec3.UnitZ, 1.0, 2.0, segsHigh);
        Assert.True(high.Mesh.Faces.Count > low.Mesh.Faces.Count);
    }

    // =========================================================================
    // Torus: various parameters
    // =========================================================================

    [Theory]
    [InlineData(2.0, 0.5, 12, 8)]
    [InlineData(2.0, 0.5, 16, 12)]
    [InlineData(2.0, 0.5, 24, 16)]
    [InlineData(5.0, 1.0, 12, 8)]
    [InlineData(1.0, 0.3, 12, 8)]
    [InlineData(10.0, 2.0, 8, 6)]
    public void Torus_HasFaces(double majorR, double minorR, int majorSegs, int minorSegs)
    {
        var torus = Primitives.Torus(Vec3.Zero, Vec3.UnitZ, majorR, minorR, majorSegs, minorSegs);
        Assert.True(torus.Mesh.Faces.Count > 0);
        Assert.True(torus.Mesh.Vertices.Count > 0);
    }

    // =========================================================================
    // Torus: vertex count == majorSegs * minorSegs
    // =========================================================================

    [Theory]
    [InlineData(12, 8)]
    [InlineData(16, 12)]
    [InlineData(24, 16)]
    [InlineData(8, 6)]
    public void Torus_VertexCount(int majorSegs, int minorSegs)
    {
        var torus = Primitives.Torus(Vec3.Zero, Vec3.UnitZ, 2.0, 0.5, majorSegs, minorSegs);
        Assert.Equal(majorSegs * minorSegs, torus.Mesh.Vertices.Count);
    }

    // =========================================================================
    // Torus: face count == 2 * majorSegs * minorSegs (all quads triangulated)
    // =========================================================================

    [Theory]
    [InlineData(12, 8)]
    [InlineData(16, 12)]
    [InlineData(24, 16)]
    [InlineData(8, 6)]
    public void Torus_FaceCount(int majorSegs, int minorSegs)
    {
        var torus = Primitives.Torus(Vec3.Zero, Vec3.UnitZ, 2.0, 0.5, majorSegs, minorSegs);
        Assert.Equal(2 * majorSegs * minorSegs, torus.Mesh.Faces.Count);
    }

    // =========================================================================
    // Torus: all vertices within expected bounding range
    // =========================================================================

    [Theory]
    [InlineData(0, 0, 0, 2.0, 0.5)]
    [InlineData(5, 5, 5, 3.0, 1.0)]
    [InlineData(-10, 0, 10, 5.0, 0.5)]
    public void Torus_VerticesWithinBounds(double cx, double cy, double cz, double majorR, double minorR)
    {
        var center = new Vec3(cx, cy, cz);
        var torus = Primitives.Torus(center, Vec3.UnitZ, majorR, minorR, 12, 8);
        double maxDist = majorR + minorR + 1e-6;
        foreach (var v in torus.Mesh.Vertices)
        {
            double dx = v.Position.X - cx;
            double dy = v.Position.Y - cy;
            double dz = v.Position.Z - cz;
            double dist = System.Math.Sqrt(dx * dx + dy * dy + dz * dz);
            Assert.True(dist <= maxDist,
                $"Vertex at distance {dist}, expected <= {maxDist}");
        }
    }

    // =========================================================================
    // All primitives: mesh has consistent face-vertex connectivity
    // =========================================================================

    [Theory]
    [InlineData("cube")]
    [InlineData("sphere")]
    [InlineData("cylinder")]
    [InlineData("cone")]
    [InlineData("torus")]
    public void AllPrimitives_FacesReferenceValidVertices(string type)
    {
        var solid = type switch
        {
            "cube" => Primitives.Cube(Vec3.Zero, 2.0),
            "sphere" => Primitives.Sphere(Vec3.Zero, 1.0, 1),
            "cylinder" => Primitives.Cylinder(Vec3.Zero, Vec3.UnitZ, 0.5, 2.0),
            "cone" => Primitives.Cone(Vec3.Zero, Vec3.UnitZ, 1.0, 2.0, 12),
            "torus" => Primitives.Torus(Vec3.Zero, Vec3.UnitZ, 2.0, 0.5, 12, 8),
            _ => throw new ArgumentException()
        };

        foreach (var face in solid.Mesh.Faces)
        {
            var he = face.Edge;
            var start = he;
            int count = 0;
            do
            {
                Assert.NotNull(he.Target);
                he = he.Next;
                count++;
            } while (he != start && count < 10);
            Assert.Equal(3, count); // All faces should be triangles
        }
    }

    // =========================================================================
    // All primitives: BVH is built
    // =========================================================================

    [Theory]
    [InlineData("cube")]
    [InlineData("sphere")]
    [InlineData("cylinder")]
    [InlineData("cone")]
    [InlineData("torus")]
    public void AllPrimitives_HasBvh(string type)
    {
        var solid = type switch
        {
            "cube" => Primitives.Cube(Vec3.Zero, 2.0),
            "sphere" => Primitives.Sphere(Vec3.Zero, 1.0, 1),
            "cylinder" => Primitives.Cylinder(Vec3.Zero, Vec3.UnitZ, 0.5, 2.0),
            "cone" => Primitives.Cone(Vec3.Zero, Vec3.UnitZ, 1.0, 2.0, 12),
            "torus" => Primitives.Torus(Vec3.Zero, Vec3.UnitZ, 2.0, 0.5, 12, 8),
            _ => throw new ArgumentException()
        };
        Assert.NotNull(solid.Bvh);
    }
}
