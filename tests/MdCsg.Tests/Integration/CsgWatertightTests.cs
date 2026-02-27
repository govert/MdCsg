using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Tests.Integration;

/// <summary>
/// Tests that CSG operation results are closed manifold meshes (watertight).
/// Every half-edge must have a twin, ensuring no holes or gaps in the output.
/// </summary>
public class CsgWatertightTests
{
    private static readonly CsgOptions Opts = new() { Parallel = false };

    private static Solid Cube(Vec3 center, double size) => Primitives.Cube(center, size);
    private static Solid Sphere(Vec3 center, double radius) => Primitives.Sphere(center, radius, 2);

    private static void AssertClosedManifold(HalfEdgeMesh mesh, string context)
    {
        int boundary = MeshValidator.CountBoundaryEdges(mesh);
        Assert.True(boundary == 0,
            $"{context}: expected 0 boundary edges but found {boundary}");
        Assert.True(MeshValidator.IsEdgeManifold(mesh),
            $"{context}: mesh is not edge-manifold");
    }

    // ── Offset cubes ──────────────────────────────────────────────

    [Fact]
    public void OffsetCubes_Union_IsClosedManifold()
    {
        var a = Cube(Vec3.Zero, 2.0);
        var b = Cube(new Vec3(0.8, 0, 0), 2.0);
        var result = Csg.Union(a, b, Opts);
        AssertClosedManifold(result.Mesh, "OffsetCubes Union");
    }

    [Fact]
    public void OffsetCubes_Intersection_IsClosedManifold()
    {
        var a = Cube(Vec3.Zero, 2.0);
        var b = Cube(new Vec3(0.8, 0, 0), 2.0);
        var result = Csg.Intersect(a, b, Opts);
        AssertClosedManifold(result.Mesh, "OffsetCubes Intersection");
    }

    [Fact]
    public void OffsetCubes_Difference_IsClosedManifold()
    {
        var a = Cube(Vec3.Zero, 2.0);
        var b = Cube(new Vec3(0.8, 0, 0), 2.0);
        var result = Csg.Difference(a, b, Opts);
        AssertClosedManifold(result.Mesh, "OffsetCubes Difference");
    }

    // ── Concentric cubes ──────────────────────────────────────────

    [Fact]
    public void ConcentricCubes_Union_IsClosedManifold()
    {
        var a = Cube(Vec3.Zero, 2.0);
        var b = Cube(Vec3.Zero, 1.0);
        var result = Csg.Union(a, b, Opts);
        AssertClosedManifold(result.Mesh, "ConcentricCubes Union");
    }

    [Fact]
    public void ConcentricCubes_Intersection_IsClosedManifold()
    {
        var a = Cube(Vec3.Zero, 2.0);
        var b = Cube(Vec3.Zero, 1.0);
        var result = Csg.Intersect(a, b, Opts);
        AssertClosedManifold(result.Mesh, "ConcentricCubes Intersection");
    }

    [Fact]
    public void ConcentricCubes_Difference_IsClosedManifold()
    {
        var a = Cube(Vec3.Zero, 2.0);
        var b = Cube(Vec3.Zero, 1.0);
        var result = Csg.Difference(a, b, Opts);
        AssertClosedManifold(result.Mesh, "ConcentricCubes Difference");
    }

    // ── Sphere-cube ───────────────────────────────────────────────

    [Fact]
    public void SphereCube_Union_IsClosedManifold()
    {
        var a = Sphere(Vec3.Zero, 1.0);
        var b = Cube(new Vec3(0.5, 0, 0), 1.5);
        var result = Csg.Union(a, b, Opts);
        AssertClosedManifold(result.Mesh, "SphereCube Union");
    }

    [Fact]
    public void SphereCube_Intersection_IsClosedManifold()
    {
        var a = Sphere(Vec3.Zero, 1.0);
        var b = Cube(new Vec3(0.5, 0, 0), 1.5);
        var result = Csg.Intersect(a, b, Opts);
        AssertClosedManifold(result.Mesh, "SphereCube Intersection");
    }

    [Fact]
    public void SphereCube_Difference_IsClosedManifold()
    {
        var a = Sphere(Vec3.Zero, 1.0);
        var b = Cube(new Vec3(0.5, 0, 0), 1.5);
        var result = Csg.Difference(a, b, Opts);
        AssertClosedManifold(result.Mesh, "SphereCube Difference");
    }

    // ── Sphere-sphere ─────────────────────────────────────────────

    [Fact]
    public void SphereSphere_Union_IsClosedManifold()
    {
        var a = Sphere(Vec3.Zero, 1.0);
        var b = Sphere(new Vec3(0.8, 0, 0), 1.0);
        var result = Csg.Union(a, b, Opts);
        AssertClosedManifold(result.Mesh, "SphereSphere Union");
    }

    [Fact]
    public void SphereSphere_Intersection_IsClosedManifold()
    {
        var a = Sphere(Vec3.Zero, 1.0);
        var b = Sphere(new Vec3(0.8, 0, 0), 1.0);
        var result = Csg.Intersect(a, b, Opts);
        AssertClosedManifold(result.Mesh, "SphereSphere Intersection");
    }

    [Fact]
    public void SphereSphere_Difference_IsClosedManifold()
    {
        var a = Sphere(Vec3.Zero, 1.0);
        var b = Sphere(new Vec3(0.8, 0, 0), 1.0);
        var result = Csg.Difference(a, b, Opts);
        AssertClosedManifold(result.Mesh, "SphereSphere Difference");
    }

    // ── Chained operations ────────────────────────────────────────

    [Fact]
    public void ChainedUnionThenIntersect_IsClosedManifold()
    {
        var a = Cube(Vec3.Zero, 2.0);
        var b = Cube(new Vec3(0.8, 0, 0), 2.0);
        var c = Sphere(new Vec3(0.4, 0, 0), 1.5);

        var ab = new Solid(Csg.Union(a, b, Opts).Mesh);
        var result = Csg.Intersect(ab, c, Opts);
        AssertClosedManifold(result.Mesh, "(A∪B)∩C");
    }

    [Fact]
    public void ChainedDifferenceThenUnion_IsClosedManifold()
    {
        var a = Cube(Vec3.Zero, 2.0);
        var b = Sphere(Vec3.Zero, 0.8);
        var c = Cube(new Vec3(1.0, 0, 0), 1.0);

        var ab = new Solid(Csg.Difference(a, b, Opts).Mesh);
        var result = Csg.Union(ab, c, Opts);
        AssertClosedManifold(result.Mesh, "(A\\B)∪C");
    }

    // ── Diagonal offset ───────────────────────────────────────────

    [Fact]
    public void DiagonalOffsetCubes_Union_IsClosedManifold()
    {
        var a = Cube(Vec3.Zero, 2.0);
        var b = Cube(new Vec3(0.5, 0.5, 0.5), 2.0);
        var result = Csg.Union(a, b, Opts);
        AssertClosedManifold(result.Mesh, "DiagonalOffset Union");
    }

    [Fact]
    public void DiagonalOffsetCubes_Difference_IsClosedManifold()
    {
        var a = Cube(Vec3.Zero, 2.0);
        var b = Cube(new Vec3(0.5, 0.5, 0.5), 2.0);
        var result = Csg.Difference(a, b, Opts);
        AssertClosedManifold(result.Mesh, "DiagonalOffset Difference");
    }
}
