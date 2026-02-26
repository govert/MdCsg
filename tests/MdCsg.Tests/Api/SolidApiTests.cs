using MdCsg.Api;
using MdCsg.Math;

namespace MdCsg.Tests.Api;

public class SolidApiTests
{
    private const double Tol = 1e-6;

    // ── Volume ──────────────────────────────────────────────────────

    [Fact]
    public void Volume_UnitCube_ReturnsOne()
    {
        var cube = Primitives.Cube(Vec3.Zero, 1);
        Assert.True(System.Math.Abs(cube.Volume() - 1.0) < Tol);
    }

    [Theory]
    [InlineData(1.0, 2.0, 3.0)]
    [InlineData(2.0, 2.0, 2.0)]
    [InlineData(0.5, 1.5, 2.5)]
    public void Volume_Box_MatchesSideProduct(double sx, double sy, double sz)
    {
        var box = Primitives.Box(Vec3.Zero, new Vec3(sx, sy, sz));
        double expected = sx * sy * sz;
        Assert.True(System.Math.Abs(box.Volume() - expected) < Tol,
            $"Expected volume {expected}, got {box.Volume()}");
    }

    [Theory]
    [InlineData(1.0)]
    [InlineData(2.0)]
    [InlineData(0.5)]
    public void Volume_Sphere_ApproximatesAnalytical(double radius)
    {
        var sphere = Primitives.Sphere(Vec3.Zero, radius, 3);
        double expected = 4.0 / 3.0 * System.Math.PI * radius * radius * radius;
        double relError = System.Math.Abs(sphere.Volume() - expected) / expected;
        Assert.True(relError < 0.02, $"Sphere volume relative error: {relError:P}");
    }

    [Theory]
    [InlineData(1.0, 2.0)]
    [InlineData(0.5, 3.0)]
    public void Volume_Cylinder_ApproximatesAnalytical(double radius, double height)
    {
        var cyl = Primitives.Cylinder(Vec3.Zero, Vec3.UnitZ, radius, height, 48);
        double expected = System.Math.PI * radius * radius * height;
        double relError = System.Math.Abs(cyl.Volume() - expected) / expected;
        Assert.True(relError < 0.01, $"Cylinder volume relative error: {relError:P}");
    }

    // ── SurfaceArea ─────────────────────────────────────────────────

    [Fact]
    public void SurfaceArea_UnitCube_ReturnsSix()
    {
        var cube = Primitives.Cube(Vec3.Zero, 1);
        Assert.True(System.Math.Abs(cube.SurfaceArea() - 6.0) < Tol);
    }

    [Theory]
    [InlineData(1.0)]
    [InlineData(2.0)]
    public void SurfaceArea_Sphere_ApproximatesAnalytical(double radius)
    {
        var sphere = Primitives.Sphere(Vec3.Zero, radius, 3);
        double expected = 4.0 * System.Math.PI * radius * radius;
        double relError = System.Math.Abs(sphere.SurfaceArea() - expected) / expected;
        Assert.True(relError < 0.02, $"Sphere area relative error: {relError:P}");
    }

    // ── IsInside ────────────────────────────────────────────────────

    [Fact]
    public void IsInside_CubeCenter_True()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2);
        Assert.True(cube.IsInside(Vec3.Zero));
    }

    [Fact]
    public void IsInside_FarAway_False()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2);
        Assert.False(cube.IsInside(new Vec3(100, 0, 0)));
    }

    [Theory]
    [InlineData(0.1, 0.1, 0.1, true)]
    [InlineData(0.9, 0.9, 0.9, true)]
    [InlineData(2, 0, 0, false)]
    [InlineData(0, 2, 0, false)]
    [InlineData(0, 0, 2, false)]
    public void IsInside_CubeVariousPoints(double x, double y, double z, bool expected)
    {
        var cube = Primitives.Cube(Vec3.Zero, 2);
        Assert.Equal(expected, cube.IsInside(new Vec3(x, y, z)));
    }

    [Fact]
    public void IsInside_SphereCenter_True()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1, 2);
        Assert.True(sphere.IsInside(Vec3.Zero));
    }

    [Fact]
    public void IsInside_SphereOutside_False()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1, 2);
        Assert.False(sphere.IsInside(new Vec3(2, 0, 0)));
    }

    // ── Transform ───────────────────────────────────────────────────

    [Fact]
    public void Transform_Translation_PreservesVolume()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2);
        var moved = cube.Transform(Transform3.FromTranslation(new Vec3(10, 0, 0)));
        Assert.True(System.Math.Abs(cube.Volume() - moved.Volume()) < Tol);
    }

    [Fact]
    public void Transform_Rotation_PreservesVolume()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2);
        var rotated = cube.Transform(Transform3.FromRotationZ(1.0));
        Assert.True(System.Math.Abs(cube.Volume() - rotated.Volume()) < Tol);
    }

    [Fact]
    public void Transform_Translation_ShiftsBounds()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2);
        var offset = new Vec3(10, 0, 0);
        var moved = cube.Transform(Transform3.FromTranslation(offset));
        Assert.True(System.Math.Abs(moved.Bounds.Center.X - 10) < Tol);
    }

    // ── Complement ──────────────────────────────────────────────────

    [Fact]
    public void Complement_PreservesVolume()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2);
        var comp = cube.Complement();
        Assert.True(System.Math.Abs(cube.Volume() - comp.Volume()) < Tol);
    }

    [Fact]
    public void Complement_ReversesNormals()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2);
        var comp = cube.Complement();

        // The signed volume should flip sign
        double origSigned = MdCsg.Mesh.MeshQueries.Volume(cube.Mesh);
        double compSigned = MdCsg.Mesh.MeshQueries.Volume(comp.Mesh);
        Assert.True(System.Math.Abs(origSigned + compSigned) < Tol,
            $"Signed volumes should be opposite: {origSigned} vs {compSigned}");
    }

    [Fact]
    public void Complement_DoubleComplement_PreservesSignedVolume()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2);
        var doubleComp = cube.Complement().Complement();
        double orig = MdCsg.Mesh.MeshQueries.Volume(cube.Mesh);
        double dc = MdCsg.Mesh.MeshQueries.Volume(doubleComp.Mesh);
        Assert.True(System.Math.Abs(orig - dc) < Tol);
    }
}
