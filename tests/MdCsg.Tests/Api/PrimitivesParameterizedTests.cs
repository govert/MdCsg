using MdCsg.Api;
using MdCsg.Math;

namespace MdCsg.Tests.Api;

/// <summary>
/// Parameterized tests for Primitives: box, cube, sphere, cylinder, cone, torus across
/// many positions, sizes, and subdivision levels.
/// </summary>
public class PrimitivesParameterizedTests
{
    // =========================================================================
    // Box: various sizes and centers
    // =========================================================================

    public static IEnumerable<object[]> BoxSizes => new[]
    {
        new object[] { 0.1, 0.1, 0.1 },
        new object[] { 1.0, 1.0, 1.0 },
        new object[] { 1.0, 2.0, 3.0 },
        new object[] { 10.0, 10.0, 10.0 },
        new object[] { 0.5, 100.0, 0.5 },
        new object[] { 100.0, 0.1, 50.0 },
    };

    public static IEnumerable<object[]> Centers => new[]
    {
        new object[] { 0.0, 0.0, 0.0 },
        new object[] { 1.0, 2.0, 3.0 },
        new object[] { -5.0, -5.0, -5.0 },
        new object[] { 100.0, 0.0, -100.0 },
        new object[] { 0.001, 0.001, 0.001 },
    };

    [Theory]
    [MemberData(nameof(BoxSizes))]
    public void Box_HasCorrectFaceCount(double sx, double sy, double sz)
    {
        var solid = Primitives.Box(Vec3.Zero, new Vec3(sx, sy, sz));
        Assert.Equal(12, solid.Mesh.Faces.Count); // 6 faces * 2 tris
    }

    [Theory]
    [MemberData(nameof(BoxSizes))]
    public void Box_HasEightVertices(double sx, double sy, double sz)
    {
        var solid = Primitives.Box(Vec3.Zero, new Vec3(sx, sy, sz));
        Assert.Equal(8, solid.Mesh.Vertices.Count);
    }

    [Theory]
    [MemberData(nameof(Centers))]
    public void Box_CenteredCorrectly(double cx, double cy, double cz)
    {
        var center = new Vec3(cx, cy, cz);
        var solid = Primitives.Box(center, new Vec3(2, 2, 2));

        var sum = Vec3.Zero;
        for (int i = 0; i < solid.Mesh.Vertices.Count; i++)
            sum += solid.Mesh.Vertices[i].Position;
        var centroid = sum * (1.0 / solid.Mesh.Vertices.Count);

        Assert.True(Vec3.Distance(centroid, center) < 1e-10,
            $"Centroid {centroid} should be at {center}");
    }

    [Theory]
    [MemberData(nameof(BoxSizes))]
    public void Box_BoundsMatchSize(double sx, double sy, double sz)
    {
        var size = new Vec3(sx, sy, sz);
        var solid = Primitives.Box(Vec3.Zero, size);
        var bounds = solid.Bounds;

        Assert.True(System.Math.Abs(bounds.Max.X - bounds.Min.X - sx) < 1e-10);
        Assert.True(System.Math.Abs(bounds.Max.Y - bounds.Min.Y - sy) < 1e-10);
        Assert.True(System.Math.Abs(bounds.Max.Z - bounds.Min.Z - sz) < 1e-10);
    }

    // =========================================================================
    // Cube: various extents
    // =========================================================================

    [Theory]
    [InlineData(0.01)]
    [InlineData(0.1)]
    [InlineData(1.0)]
    [InlineData(10.0)]
    [InlineData(100.0)]
    public void Cube_HasTwelveFaces(double extent)
    {
        var solid = Primitives.Cube(Vec3.Zero, extent);
        Assert.Equal(12, solid.Mesh.Faces.Count);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(0.1)]
    [InlineData(1.0)]
    [InlineData(10.0)]
    [InlineData(100.0)]
    public void Cube_BoundsAreSymmetric(double extent)
    {
        var solid = Primitives.Cube(Vec3.Zero, extent);
        var half = extent / 2;
        Assert.True(System.Math.Abs(solid.Bounds.Min.X + half) < 1e-10);
        Assert.True(System.Math.Abs(solid.Bounds.Max.X - half) < 1e-10);
    }

    // =========================================================================
    // Sphere: subdivisions
    // =========================================================================

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 80)]
    [InlineData(2, 320)]
    [InlineData(3, 1280)]
    public void Sphere_FaceCountBySubdivision(int subdivisions, int expectedFaces)
    {
        var solid = Primitives.Sphere(Vec3.Zero, 1.0, subdivisions);
        Assert.Equal(expectedFaces, solid.Mesh.Faces.Count);
    }

    [Theory]
    [InlineData(0, 12)]
    [InlineData(1, 42)]
    [InlineData(2, 162)]
    [InlineData(3, 642)]
    public void Sphere_VertexCountBySubdivision(int subdivisions, int expectedVerts)
    {
        var solid = Primitives.Sphere(Vec3.Zero, 1.0, subdivisions);
        Assert.Equal(expectedVerts, solid.Mesh.Vertices.Count);
    }

    [Theory]
    [InlineData(0.1)]
    [InlineData(1.0)]
    [InlineData(5.0)]
    [InlineData(50.0)]
    public void Sphere_VerticesAtCorrectRadius(double radius)
    {
        var solid = Primitives.Sphere(Vec3.Zero, radius, 2);
        for (int i = 0; i < solid.Mesh.Vertices.Count; i++)
        {
            double dist = solid.Mesh.Vertices[i].Position.Length;
            Assert.True(System.Math.Abs(dist - radius) < radius * 0.01,
                $"Vertex {i}: dist={dist}, expected ~{radius}");
        }
    }

    [Theory]
    [MemberData(nameof(Centers))]
    public void Sphere_CenteredCorrectly(double cx, double cy, double cz)
    {
        var center = new Vec3(cx, cy, cz);
        var solid = Primitives.Sphere(center, 2.0, 2);

        var sum = Vec3.Zero;
        for (int i = 0; i < solid.Mesh.Vertices.Count; i++)
            sum += solid.Mesh.Vertices[i].Position;
        var centroid = sum * (1.0 / solid.Mesh.Vertices.Count);

        Assert.True(Vec3.Distance(centroid, center) < 0.1);
    }

    // =========================================================================
    // Cylinder: segments and axes
    // =========================================================================

    [Theory]
    [InlineData(6)]
    [InlineData(8)]
    [InlineData(12)]
    [InlineData(24)]
    [InlineData(32)]
    [InlineData(64)]
    public void Cylinder_FaceCountBySegments(int segments)
    {
        var solid = Primitives.Cylinder(Vec3.Zero, Vec3.UnitZ, 1.0, 5.0, segments);
        Assert.Equal(segments * 4, solid.Mesh.Faces.Count);
    }

    [Theory]
    [InlineData(1, 0, 0)]
    [InlineData(0, 1, 0)]
    [InlineData(0, 0, 1)]
    [InlineData(1, 1, 0)]
    [InlineData(1, 1, 1)]
    [InlineData(2, -1, 3)]
    public void Cylinder_VariousAxes_ValidMesh(double ax, double ay, double az)
    {
        var solid = Primitives.Cylinder(Vec3.Zero, new Vec3(ax, ay, az), 1.0, 5.0);
        Assert.True(solid.Mesh.Faces.Count > 0);
        Assert.True(solid.Mesh.Vertices.Count > 0);
    }

    [Theory]
    [InlineData(0.01, 1.0)]
    [InlineData(1.0, 1.0)]
    [InlineData(5.0, 10.0)]
    [InlineData(0.5, 100.0)]
    public void Cylinder_VariousDimensions_ValidMesh(double radius, double height)
    {
        var solid = Primitives.Cylinder(Vec3.Zero, Vec3.UnitZ, radius, height);
        Assert.True(solid.Mesh.Faces.Count > 0);
    }

    // =========================================================================
    // Cone: segments
    // =========================================================================

    [Theory]
    [InlineData(6)]
    [InlineData(12)]
    [InlineData(24)]
    [InlineData(32)]
    public void Cone_FaceCountBySegments(int segments)
    {
        var solid = Primitives.Cone(Vec3.Zero, Vec3.UnitZ, 1.0, 5.0, segments);
        Assert.Equal(segments * 2, solid.Mesh.Faces.Count); // base + side
    }

    [Theory]
    [InlineData(1, 0, 0)]
    [InlineData(0, 1, 0)]
    [InlineData(0, 0, 1)]
    [InlineData(1, 1, 1)]
    public void Cone_VariousAxes_ValidMesh(double ax, double ay, double az)
    {
        var solid = Primitives.Cone(Vec3.Zero, new Vec3(ax, ay, az), 1.0, 5.0);
        Assert.True(solid.Mesh.Faces.Count > 0);
    }

    // =========================================================================
    // Torus: segments
    // =========================================================================

    [Theory]
    [InlineData(6, 4)]
    [InlineData(12, 6)]
    [InlineData(24, 12)]
    [InlineData(32, 16)]
    public void Torus_FaceCountBySegments(int majorSegs, int minorSegs)
    {
        var solid = Primitives.Torus(Vec3.Zero, Vec3.UnitZ, 2.0, 0.5, majorSegs, minorSegs);
        Assert.Equal(majorSegs * minorSegs * 2, solid.Mesh.Faces.Count);
    }

    [Theory]
    [InlineData(1, 0, 0)]
    [InlineData(0, 1, 0)]
    [InlineData(0, 0, 1)]
    [InlineData(1, 1, 1)]
    public void Torus_VariousAxes_ValidMesh(double ax, double ay, double az)
    {
        var solid = Primitives.Torus(Vec3.Zero, new Vec3(ax, ay, az), 2.0, 0.5);
        Assert.True(solid.Mesh.Faces.Count > 0);
    }
}
