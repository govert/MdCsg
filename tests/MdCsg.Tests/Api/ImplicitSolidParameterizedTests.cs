using MdCsg.Api;
using MdCsg.Math;

namespace MdCsg.Tests.Api;

/// <summary>
/// Parameterized tests for ImplicitSphere and ImplicitCylinder operations.
/// Note: Implicit CSG requires mesh edges to cross the implicit surface.
/// </summary>
public class ImplicitSolidParameterizedTests
{
    // =========================================================================
    // ImplicitSphere: intersection with cube (sphere larger than cube)
    // =========================================================================

    [Theory]
    [InlineData(0, 0, 0, 1.5)]
    [InlineData(0.3, 0, 0, 1.5)]
    [InlineData(0, 0.3, 0, 1.5)]
    [InlineData(0, 0, 0.3, 1.5)]
    [InlineData(0, 0, 0, 2.0)]
    [InlineData(0, 0, 0, 3.0)]
    public void ImplicitSphere_IntersectCube_ProducesFaces(double sx, double sy, double sz, double radius)
    {
        // Cube size 1.5: edges at +-0.75. Sphere must be large enough to cross edges.
        var cube = Primitives.Cube(Vec3.Zero, 1.5);
        var sphere = new ImplicitSphere(new Vec3(sx, sy, sz), radius);
        var result = Csg.Intersect(cube, sphere);
        Assert.True(result.Mesh.Faces.Count > 0,
            $"Sphere at ({sx},{sy},{sz}) r={radius}: expected faces");
    }

    // =========================================================================
    // ImplicitSphere: difference from cube (sphere must cross edges)
    // =========================================================================

    [Theory]
    [InlineData(0, 0, 0, 0.5)]
    [InlineData(0.3, 0, 0, 0.5)]
    [InlineData(0, 0.3, 0, 0.5)]
    [InlineData(0, 0, 0.3, 0.5)]
    [InlineData(0, 0, 0, 0.8)]
    public void ImplicitSphere_DiffFromCube_ProducesFaces(double sx, double sy, double sz, double radius)
    {
        // Cube size 1.5: sphere at center with r=0.5 crosses interior edges
        var cube = Primitives.Cube(Vec3.Zero, 1.5);
        var sphere = new ImplicitSphere(new Vec3(sx, sy, sz), radius);
        var result = Csg.Difference(cube, sphere);
        Assert.True(result.Mesh.Faces.Count > 0);
    }

    // =========================================================================
    // ImplicitSphere: various radii (crossing cube edges)
    // =========================================================================

    [Theory]
    [InlineData(0.8)]
    [InlineData(1.0)]
    [InlineData(1.5)]
    [InlineData(2.0)]
    public void ImplicitSphere_VariousRadii_Intersect(double radius)
    {
        // Cube size 1.0, sphere radius >= 0.8 ensures edge crossings
        var cube = Primitives.Cube(Vec3.Zero, 1.0);
        var sphere = new ImplicitSphere(Vec3.Zero, radius);
        var result = Csg.Intersect(cube, sphere);
        Assert.True(result.Mesh.Faces.Count > 0, $"r={radius}");
    }

    // =========================================================================
    // ImplicitCylinder: intersection with cube (cylinder protrudes)
    // =========================================================================

    [Theory]
    [InlineData(0, 0, 1, 0.8, 4.0)]
    [InlineData(1, 0, 0, 0.8, 4.0)]
    [InlineData(0, 1, 0, 0.8, 4.0)]
    [InlineData(0, 0, 1, 1.0, 4.0)]
    [InlineData(0, 0, 1, 1.2, 4.0)]
    public void ImplicitCylinder_IntersectCube_ProducesFaces(double ax, double ay, double az, double radius, double height)
    {
        // Cube size 1.5: cylinder with r>=0.8 crosses face edges
        var cube = Primitives.Cube(Vec3.Zero, 1.5);
        var axis = new Vec3(ax, ay, az);
        var cyl = new ImplicitCylinder(Vec3.Zero - axis * (height / 2), axis, radius, height);
        var result = Csg.Intersect(cube, cyl);
        Assert.True(result.Mesh.Faces.Count > 0,
            $"Cylinder axis=({ax},{ay},{az}) r={radius} h={height}");
    }

    // =========================================================================
    // ImplicitCylinder: difference from cube
    // =========================================================================

    [Theory]
    [InlineData(0, 0, 1, 0.5, 4.0)]
    [InlineData(1, 0, 0, 0.5, 4.0)]
    [InlineData(0, 1, 0, 0.5, 4.0)]
    public void ImplicitCylinder_DiffFromCube_ProducesFaces(double ax, double ay, double az, double radius, double height)
    {
        var cube = Primitives.Cube(Vec3.Zero, 1.5);
        var axis = new Vec3(ax, ay, az);
        var cyl = new ImplicitCylinder(Vec3.Zero - axis * (height / 2), axis, radius, height);
        var result = Csg.Difference(cube, cyl);
        Assert.True(result.Mesh.Faces.Count > 0);
    }

    // =========================================================================
    // ImplicitSphere: determinism
    // =========================================================================

    [Theory]
    [InlineData(0, 0, 0, 1.5)]
    [InlineData(0.3, 0, 0, 1.5)]
    public void ImplicitSphere_Deterministic(double sx, double sy, double sz, double r)
    {
        var cube = Primitives.Cube(Vec3.Zero, 1.5);
        var sphere = new ImplicitSphere(new Vec3(sx, sy, sz), r);
        var r1 = Csg.Intersect(cube, sphere);
        var r2 = Csg.Intersect(cube, sphere);
        Assert.Equal(r1.Mesh.Faces.Count, r2.Mesh.Faces.Count);
    }

    // =========================================================================
    // ImplicitCylinder: determinism
    // =========================================================================

    [Theory]
    [InlineData(0, 0, 1, 0.8, 4.0)]
    [InlineData(1, 0, 0, 0.8, 4.0)]
    public void ImplicitCylinder_Deterministic(double ax, double ay, double az, double r, double h)
    {
        var cube = Primitives.Cube(Vec3.Zero, 1.5);
        var cyl = new ImplicitCylinder(Vec3.Zero - new Vec3(ax, ay, az) * (h / 2), new Vec3(ax, ay, az), r, h);
        var r1 = Csg.Intersect(cube, cyl);
        var r2 = Csg.Intersect(cube, cyl);
        Assert.Equal(r1.Mesh.Faces.Count, r2.Mesh.Faces.Count);
    }

    // =========================================================================
    // ImplicitSphere: intersect with sphere solid (sphere surface crosses edges)
    // =========================================================================

    [Theory]
    [InlineData(0, 0, 0, 1.2)]
    [InlineData(0.3, 0, 0, 1.2)]
    [InlineData(0, 0.3, 0, 1.2)]
    public void ImplicitSphere_IntersectWithSolidSphere_ProducesFaces(double sx, double sy, double sz, double r)
    {
        // Solid sphere r=1.0 subdiv 2. Implicit sphere larger so it crosses mesh edges.
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var implSphere = new ImplicitSphere(new Vec3(sx, sy, sz), r);
        var result = Csg.Intersect(sphere, implSphere);
        Assert.True(result.Mesh.Faces.Count > 0);
    }

    // =========================================================================
    // Implicit with Solid on left side: Intersect(Implicit, Solid)
    // =========================================================================

    [Theory]
    [InlineData(0, 0, 0, 1.5)]
    [InlineData(0.3, 0, 0, 1.5)]
    [InlineData(0, 0.3, 0, 1.5)]
    public void ImplicitSolid_IntersectImplicitWithSolid(double sx, double sy, double sz, double r)
    {
        var cube = Primitives.Cube(Vec3.Zero, 1.5);
        var sphere = new ImplicitSphere(new Vec3(sx, sy, sz), r);
        var result = Csg.Intersect(sphere, cube);
        Assert.True(result.Mesh.Faces.Count > 0);
    }
}
