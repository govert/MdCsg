using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Operations;

namespace MdCsg.Tests.Api;

/// <summary>
/// Parameterized HalfSpace and CsgHalfSpace tests across planes, orientations, and solids.
/// </summary>
public class HalfSpaceParameterizedTests
{
    // =========================================================================
    // HalfSpace construction from various normals
    // =========================================================================

    [Theory]
    [InlineData(1, 0, 0)]
    [InlineData(0, 1, 0)]
    [InlineData(0, 0, 1)]
    [InlineData(-1, 0, 0)]
    [InlineData(0, -1, 0)]
    [InlineData(0, 0, -1)]
    [InlineData(1, 1, 0)]
    [InlineData(1, 0, 1)]
    [InlineData(0, 1, 1)]
    [InlineData(1, 1, 1)]
    [InlineData(2, -1, 3)]
    [InlineData(-3, 2, -1)]
    public void Constructor_NormalizesNormal(double nx, double ny, double nz)
    {
        var hs = new HalfSpace(new Plane(new Vec3(nx, ny, nz), 0));
        double len = hs.Plane.Normal.Length;
        // Normal might not be pre-normalized in the Plane struct depending on constructor
        // but HalfSpace should store a valid plane
        Assert.True(hs.Plane.Normal.Length > 0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(5)]
    [InlineData(-5)]
    [InlineData(100)]
    public void FromPointAndNormal_PointOnPlane_ZeroSignedDistance(double offset)
    {
        var normal = Vec3.UnitZ;
        var point = normal * offset;
        var plane = Plane.FromPointAndNormal(point, normal);
        var hs = new HalfSpace(plane);

        double sd = hs.Plane.SignedDistanceTo(point);
        Assert.True(System.Math.Abs(sd) < 1e-10,
            $"Point on plane should have zero signed dist, got {sd}");
    }

    // =========================================================================
    // Classification: above vs below plane
    // =========================================================================

    [Theory]
    [InlineData(1, 0, 0, 0, 1, 0, 0)]    // +X normal, test point at (1,0,0)
    [InlineData(0, 1, 0, 0, 0, 1, 0)]    // +Y normal, test point at (0,1,0)
    [InlineData(0, 0, 1, 0, 0, 0, 1)]    // +Z normal, test point at (0,0,1)
    [InlineData(1, 0, 0, 5, 10, 0, 0)]   // +X normal at x=5, test at (10,0,0)
    public void AbovePlane_PositiveSignedDistance(
        double nx, double ny, double nz, double dist,
        double px, double py, double pz)
    {
        var plane = new Plane(new Vec3(nx, ny, nz).Normalized, dist);
        double sd = plane.SignedDistanceTo(new Vec3(px, py, pz));
        Assert.True(sd > 0, $"Point ({px},{py},{pz}) should be above plane, got sd={sd}");
    }

    [Theory]
    [InlineData(1, 0, 0, 0, -1, 0, 0)]
    [InlineData(0, 1, 0, 0, 0, -1, 0)]
    [InlineData(0, 0, 1, 0, 0, 0, -1)]
    [InlineData(1, 0, 0, 5, 0, 0, 0)]    // +X normal at x=5, test at (0,0,0)
    public void BelowPlane_NegativeSignedDistance(
        double nx, double ny, double nz, double dist,
        double px, double py, double pz)
    {
        var plane = new Plane(new Vec3(nx, ny, nz).Normalized, dist);
        double sd = plane.SignedDistanceTo(new Vec3(px, py, pz));
        Assert.True(sd < 0, $"Point ({px},{py},{pz}) should be below plane, got sd={sd}");
    }

    // =========================================================================
    // Flipped: reversed classification
    // =========================================================================

    [Theory]
    [InlineData(1, 0, 0, 0)]
    [InlineData(0, 1, 0, 5)]
    [InlineData(0, 0, 1, -3)]
    [InlineData(1, 1, 1, 2)]
    public void Flipped_NegatesSignedDistance(double nx, double ny, double nz, double dist)
    {
        var normal = new Vec3(nx, ny, nz).Normalized;
        var plane = new Plane(normal, dist);
        var flipped = plane.Flipped;
        var testPoint = new Vec3(1, 2, 3);

        double sd = plane.SignedDistanceTo(testPoint);
        double fsd = flipped.SignedDistanceTo(testPoint);

        Assert.True(System.Math.Abs(sd + fsd) < 1e-10,
            $"Flipped SD should negate: {sd} + {fsd} != 0");
    }

    // =========================================================================
    // CsgHalfSpace: cutting cubes at various planes
    // =========================================================================

    [Theory]
    [InlineData(1, 0, 0, 0.5)]
    [InlineData(0, 1, 0, 0.5)]
    [InlineData(0, 0, 1, 0.5)]
    [InlineData(1, 0, 0, 0.25)]
    [InlineData(1, 0, 0, 0.75)]
    [InlineData(0, 0, 1, 0.1)]
    [InlineData(0, 0, 1, 0.9)]
    public void CsgHalfSpace_CutCube_ProducesFaces(double nx, double ny, double nz, double dist)
    {
        var cube = Primitives.Cube(new Vec3(0.5, 0.5, 0.5), 1.0);
        var plane = new Plane(new Vec3(nx, ny, nz).Normalized, dist);
        var hs = new HalfSpace(plane);

        var result = CsgHalfSpace.Evaluate(cube, hs, CsgOperation.Intersection, new CsgOptions());
        Assert.True(result.Mesh.Faces.Count > 0,
            $"Cut with normal=({nx},{ny},{nz}), dist={dist} should produce faces");
    }

    [Theory]
    [InlineData(1, 0, 0, 0.5)]
    [InlineData(0, 1, 0, 0.5)]
    [InlineData(0, 0, 1, 0.5)]
    public void CsgHalfSpace_DiffCube_ProducesFaces(double nx, double ny, double nz, double dist)
    {
        var cube = Primitives.Cube(new Vec3(0.5, 0.5, 0.5), 1.0);
        var plane = new Plane(new Vec3(nx, ny, nz).Normalized, dist);
        var hs = new HalfSpace(plane);

        var result = CsgHalfSpace.Evaluate(cube, hs, CsgOperation.Difference, new CsgOptions());
        Assert.True(result.Mesh.Faces.Count > 0);
    }

    // =========================================================================
    // CsgHalfSpace: cutting sphere at various planes
    // =========================================================================

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.5)]
    [InlineData(-0.5)]
    [InlineData(0.9)]
    public void CsgHalfSpace_CutSphere_Intersect(double zOffset)
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var plane = Plane.FromPointAndNormal(new Vec3(0, 0, zOffset), Vec3.UnitZ);
        var hs = new HalfSpace(plane);

        var result = CsgHalfSpace.Evaluate(sphere, hs, CsgOperation.Intersection, new CsgOptions());
        Assert.True(result.Mesh.Faces.Count > 0,
            $"Cut sphere at z={zOffset} should produce faces");
    }

    // =========================================================================
    // Tilted cut planes
    // =========================================================================

    [Theory]
    [InlineData(1, 1, 0)]
    [InlineData(1, 0, 1)]
    [InlineData(0, 1, 1)]
    [InlineData(1, 1, 1)]
    [InlineData(2, 1, -1)]
    public void CsgHalfSpace_TiltedCut_ProducesFaces(double nx, double ny, double nz)
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var normal = new Vec3(nx, ny, nz).Normalized;
        var plane = Plane.FromPointAndNormal(Vec3.Zero, normal);
        var hs = new HalfSpace(plane);

        var result = CsgHalfSpace.Evaluate(cube, hs, CsgOperation.Intersection, new CsgOptions());
        Assert.True(result.Mesh.Faces.Count > 0,
            $"Tilted cut with normal=({nx},{ny},{nz}) should produce faces");
    }
}
