using MdCsg.Api;
using MdCsg.Bvh;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Tests.Mesh;

public class MeshQueriesTests
{
    private const double Tol = 1e-6;

    // ── Volume ──────────────────────────────────────────────────────

    [Fact]
    public void Volume_UnitCube_ReturnsOne()
    {
        var cube = Primitives.Cube(Vec3.Zero, 1);
        double vol = System.Math.Abs(MeshQueries.Volume(cube.Mesh));
        Assert.True(System.Math.Abs(vol - 1.0) < Tol, $"Expected volume 1, got {vol}");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Volume_Box_MatchesSideProduct(int size)
    {
        var box = Primitives.Box(Vec3.Zero, new Vec3(size, size + 1, size + 2));
        double vol = System.Math.Abs(MeshQueries.Volume(box.Mesh));
        double expected = size * (size + 1.0) * (size + 2.0);
        Assert.True(System.Math.Abs(vol - expected) < Tol,
            $"Expected volume {expected}, got {vol}");
    }

    [Theory]
    [InlineData(1.0, 3)]
    [InlineData(2.0, 3)]
    [InlineData(1.0, 4)]
    public void Volume_Sphere_ApproximatesAnalytical(double radius, int subdivisions)
    {
        var sphere = Primitives.Sphere(Vec3.Zero, radius, subdivisions);
        double vol = System.Math.Abs(MeshQueries.Volume(sphere.Mesh));
        double expected = 4.0 / 3.0 * System.Math.PI * radius * radius * radius;
        double relError = System.Math.Abs(vol - expected) / expected;
        Assert.True(relError < 0.05, $"Sphere volume {vol} differs from {expected} by {relError:P}");
    }

    [Theory]
    [InlineData(1.0, 2.0, 24)]
    [InlineData(0.5, 3.0, 48)]
    public void Volume_Cylinder_ApproximatesAnalytical(double radius, double height, int segments)
    {
        var cyl = Primitives.Cylinder(Vec3.Zero, Vec3.UnitZ, radius, height, segments);
        double vol = System.Math.Abs(MeshQueries.Volume(cyl.Mesh));
        double expected = System.Math.PI * radius * radius * height;
        double relError = System.Math.Abs(vol - expected) / expected;
        Assert.True(relError < 0.05, $"Cylinder volume {vol} differs from {expected} by {relError:P}");
    }

    // ── SurfaceArea ─────────────────────────────────────────────────

    [Fact]
    public void SurfaceArea_UnitCube_ReturnsSix()
    {
        var cube = Primitives.Cube(Vec3.Zero, 1);
        double area = MeshQueries.SurfaceArea(cube.Mesh);
        Assert.True(System.Math.Abs(area - 6.0) < Tol, $"Expected area 6, got {area}");
    }

    [Theory]
    [InlineData(1.0, 3)]
    [InlineData(2.0, 3)]
    public void SurfaceArea_Sphere_ApproximatesAnalytical(double radius, int subdivisions)
    {
        var sphere = Primitives.Sphere(Vec3.Zero, radius, subdivisions);
        double area = MeshQueries.SurfaceArea(sphere.Mesh);
        double expected = 4.0 * System.Math.PI * radius * radius;
        double relError = System.Math.Abs(area - expected) / expected;
        Assert.True(relError < 0.05, $"Sphere area {area} differs from {expected} by {relError:P}");
    }

    // ── VertexNormal ────────────────────────────────────────────────

    [Fact]
    public void VertexNormal_CubeCorner_DiagonalDirection()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2);
        // Find the vertex nearest to (1,1,1)
        var target = new Vec3(1, 1, 1);
        Vertex? best = null;
        double bestDist = double.MaxValue;
        foreach (var v in cube.Mesh.Vertices)
        {
            double d = Vec3.DistanceSquared(v.Position, target);
            if (d < bestDist) { bestDist = d; best = v; }
        }
        Assert.NotNull(best);
        var normal = MeshQueries.VertexNormal(best, cube.Mesh);
        // Should point roughly in (1,1,1) direction
        Assert.True(Vec3.Dot(normal, target.Normalized) > 0.9,
            $"Normal {normal} should point toward (1,1,1)");
    }

    [Fact]
    public void VertexNormal_Sphere_PointsOutward()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1, 2);
        foreach (var v in sphere.Mesh.Vertices)
        {
            var normal = MeshQueries.VertexNormal(v, sphere.Mesh);
            var expected = v.Position.Normalized;
            double dot = Vec3.Dot(normal, expected);
            Assert.True(dot > 0.95, $"Vertex normal {normal} should point like {expected}, dot={dot}");
        }
    }

    // ── NearestPointOnMesh ──────────────────────────────────────────

    [Fact]
    public void NearestPoint_PointOnSurface_ReturnsZeroDistance()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2);
        var p = new Vec3(1, 0, 0); // on the +X face
        var (nearest, faceIdx, dist) = MeshQueries.NearestPointOnMesh(p, cube.Mesh, cube.Bvh);
        Assert.True(dist < 1e-8, $"Expected near-zero distance, got {dist}");
        Assert.True(faceIdx >= 0);
    }

    [Fact]
    public void NearestPoint_PointOutside_ReturnsCorrectDistance()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2);
        var p = new Vec3(2, 0, 0); // 1 unit from +X face
        var (nearest, faceIdx, dist) = MeshQueries.NearestPointOnMesh(p, cube.Mesh, cube.Bvh);
        Assert.True(System.Math.Abs(dist - 1.0) < 1e-8, $"Expected distance 1, got {dist}");
        Assert.True(System.Math.Abs(nearest.X - 1.0) < 1e-8);
    }

    [Fact]
    public void NearestPoint_PointInside_ReturnsDistanceToSurface()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2);
        var p = new Vec3(0.5, 0, 0); // 0.5 from +X face
        var (_, _, dist) = MeshQueries.NearestPointOnMesh(p, cube.Mesh, cube.Bvh);
        Assert.True(dist < 1.0 + 1e-8, $"Expected dist < 1, got {dist}");
        Assert.True(dist > 0);
    }

    // ── ConnectedComponents ─────────────────────────────────────────

    [Fact]
    public void ConnectedComponents_SingleSolid_OneComponent()
    {
        var cube = Primitives.Cube(Vec3.Zero, 1);
        var comps = MeshQueries.ConnectedComponents(cube.Mesh);
        Assert.Single(comps);
        Assert.Equal(cube.Mesh.Faces.Count, comps[0].Count);
    }

    [Fact]
    public void ConnectedComponents_TwoDisjointSolids_TwoComponents()
    {
        // Build two cubes as a single mesh
        var positions = new List<Vec3>();
        var triangles = new List<(int, int, int)>();

        AddCubeTriangles(positions, triangles, new Vec3(0, 0, 0), 1);
        AddCubeTriangles(positions, triangles, new Vec3(10, 0, 0), 1);

        var builder = new MeshBuilder();
        var mesh = builder.Build(positions, triangles);

        var comps = MeshQueries.ConnectedComponents(mesh);
        Assert.Equal(2, comps.Count);
    }

    // ── BoundaryLoops ───────────────────────────────────────────────

    [Fact]
    public void BoundaryLoops_ClosedMesh_NoLoops()
    {
        var cube = Primitives.Cube(Vec3.Zero, 1);
        var loops = MeshQueries.BoundaryLoops(cube.Mesh);
        Assert.Empty(loops);
    }

    [Fact]
    public void BoundaryLoops_SingleTriangle_OneLoop()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(new Vec3(0, 0, 0));
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        mesh.AddFace(v0, v1, v2);
        // Single triangle has 3 boundary edges
        var loops = MeshQueries.BoundaryLoops(mesh);
        Assert.Single(loops);
        Assert.Equal(3, loops[0].Count);
    }

    // ── Helpers ──────────────────────────────────────────────────────

    private static void AddCubeTriangles(List<Vec3> positions, List<(int, int, int)> triangles, Vec3 center, double size)
    {
        double h = size * 0.5;
        int baseIdx = positions.Count;
        positions.Add(center + new Vec3(-h, -h, -h));
        positions.Add(center + new Vec3(h, -h, -h));
        positions.Add(center + new Vec3(h, h, -h));
        positions.Add(center + new Vec3(-h, h, -h));
        positions.Add(center + new Vec3(-h, -h, h));
        positions.Add(center + new Vec3(h, -h, h));
        positions.Add(center + new Vec3(h, h, h));
        positions.Add(center + new Vec3(-h, h, h));

        int b = baseIdx;
        triangles.Add((b + 0, b + 2, b + 1)); triangles.Add((b + 0, b + 3, b + 2));
        triangles.Add((b + 4, b + 5, b + 6)); triangles.Add((b + 4, b + 6, b + 7));
        triangles.Add((b + 0, b + 1, b + 5)); triangles.Add((b + 0, b + 5, b + 4));
        triangles.Add((b + 2, b + 3, b + 7)); triangles.Add((b + 2, b + 7, b + 6));
        triangles.Add((b + 0, b + 4, b + 7)); triangles.Add((b + 0, b + 7, b + 3));
        triangles.Add((b + 1, b + 2, b + 6)); triangles.Add((b + 1, b + 6, b + 5));
    }
}
