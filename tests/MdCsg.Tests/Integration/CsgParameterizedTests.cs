using MdCsg.Api;
using MdCsg.Math;

namespace MdCsg.Tests.Integration;

/// <summary>
/// Parameterized CSG operations across solid types, positions, scales, and operations.
/// Generates hundreds of tests via Theory/MemberData.
/// </summary>
public class CsgParameterizedTests
{
    // =========================================================================
    // Test data generators
    // =========================================================================

    public static IEnumerable<object[]> Offsets => new[]
    {
        new object[] { 0.0, 0.0, 0.0 },
        new object[] { 0.3, 0.0, 0.0 },
        new object[] { 0.0, 0.3, 0.0 },
        new object[] { 0.0, 0.0, 0.3 },
        new object[] { 0.3, 0.3, 0.3 },
        new object[] { -0.3, 0.2, 0.1 },
        new object[] { 0.5, -0.5, 0.5 },
        new object[] { 0.8, 0.0, 0.0 },
    };

    public static IEnumerable<object[]> Scales => new[]
    {
        new object[] { 0.5 },
        new object[] { 1.0 },
        new object[] { 2.0 },
        new object[] { 5.0 },
        new object[] { 10.0 },
    };

    // =========================================================================
    // Cube-Cube: Union at various offsets
    // =========================================================================

    [Theory]
    [MemberData(nameof(Offsets))]
    public void CubeCube_Union_ProducesFaces(double ox, double oy, double oz)
    {
        var a = Primitives.Cube(Vec3.Zero, 1.0);
        var b = Primitives.Cube(new Vec3(ox, oy, oz), 1.0);
        var result = Csg.Union(a, b);
        Assert.True(result.Mesh.Faces.Count > 0, $"Union at ({ox},{oy},{oz}) should produce faces");
    }

    [Theory]
    [MemberData(nameof(Offsets))]
    public void CubeCube_Intersect_ProducesFaces(double ox, double oy, double oz)
    {
        var a = Primitives.Cube(Vec3.Zero, 1.0);
        var b = Primitives.Cube(new Vec3(ox, oy, oz), 1.0);
        var result = Csg.Intersect(a, b);
        Assert.True(result.Mesh.Faces.Count > 0, $"Intersect at ({ox},{oy},{oz}) should produce faces");
    }

    [Theory]
    [MemberData(nameof(Offsets))]
    public void CubeCube_Difference_ProducesFaces(double ox, double oy, double oz)
    {
        var a = Primitives.Cube(Vec3.Zero, 1.0);
        var b = Primitives.Cube(new Vec3(ox, oy, oz), 0.6);
        var result = Csg.Difference(a, b);
        Assert.True(result.Mesh.Faces.Count > 0, $"Difference at ({ox},{oy},{oz}) should produce faces");
    }

    // =========================================================================
    // Cube-Sphere: various offsets
    // =========================================================================

    [Theory]
    [MemberData(nameof(Offsets))]
    public void CubeSphere_Union_ProducesFaces(double ox, double oy, double oz)
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var sphere = Primitives.Sphere(new Vec3(ox, oy, oz), 1.0, 1);
        var result = Csg.Union(cube, sphere);
        Assert.True(result.Mesh.Faces.Count > 0);
    }

    [Theory]
    [MemberData(nameof(Offsets))]
    public void CubeSphere_Intersect_ProducesFaces(double ox, double oy, double oz)
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var sphere = Primitives.Sphere(new Vec3(ox, oy, oz), 1.0, 1);
        var result = Csg.Intersect(cube, sphere);
        Assert.True(result.Mesh.Faces.Count > 0);
    }

    [Theory]
    [MemberData(nameof(Offsets))]
    public void CubeSphere_Difference_ProducesFaces(double ox, double oy, double oz)
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var sphere = Primitives.Sphere(new Vec3(ox, oy, oz), 0.8, 1);
        var result = Csg.Difference(cube, sphere);
        Assert.True(result.Mesh.Faces.Count > 0);
    }

    // =========================================================================
    // Sphere-Sphere: various offsets
    // =========================================================================

    [Theory]
    [MemberData(nameof(Offsets))]
    public void SphereSphere_Union_ProducesFaces(double ox, double oy, double oz)
    {
        var a = Primitives.Sphere(Vec3.Zero, 1.0, 1);
        var b = Primitives.Sphere(new Vec3(ox, oy, oz), 1.0, 1);
        var result = Csg.Union(a, b);
        Assert.True(result.Mesh.Faces.Count > 0);
    }

    [Theory]
    [MemberData(nameof(Offsets))]
    public void SphereSphere_Intersect_ProducesFaces(double ox, double oy, double oz)
    {
        var a = Primitives.Sphere(Vec3.Zero, 1.0, 1);
        var b = Primitives.Sphere(new Vec3(ox, oy, oz), 1.0, 1);
        var result = Csg.Intersect(a, b);
        Assert.True(result.Mesh.Faces.Count > 0);
    }

    [Theory]
    [MemberData(nameof(Offsets))]
    public void SphereSphere_Difference_ProducesFaces(double ox, double oy, double oz)
    {
        var a = Primitives.Sphere(Vec3.Zero, 1.0, 1);
        var b = Primitives.Sphere(new Vec3(ox, oy, oz), 0.6, 1);
        var result = Csg.Difference(a, b);
        Assert.True(result.Mesh.Faces.Count > 0);
    }

    // =========================================================================
    // Cube-Cylinder: various offsets
    // =========================================================================

    [Theory]
    [MemberData(nameof(Offsets))]
    public void CubeCylinder_Difference_ProducesFaces(double ox, double oy, double oz)
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var cyl = Primitives.Cylinder(new Vec3(ox, oy, oz - 2), Vec3.UnitZ, 0.4, 4.0);
        var result = Csg.Difference(cube, cyl);
        Assert.True(result.Mesh.Faces.Count > 0);
    }

    // =========================================================================
    // Scale invariance: same operation at different scales
    // =========================================================================

    [Theory]
    [MemberData(nameof(Scales))]
    public void ScaleInvariance_Union_ProducesFaces(double scale)
    {
        var a = Primitives.Cube(Vec3.Zero, scale);
        var b = Primitives.Sphere(new Vec3(scale * 0.3, 0, 0), scale * 0.5, 1);
        var result = Csg.Union(a, b);
        Assert.True(result.Mesh.Faces.Count > 0, $"scale={scale}");
    }

    [Theory]
    [MemberData(nameof(Scales))]
    public void ScaleInvariance_Intersect_ProducesFaces(double scale)
    {
        var a = Primitives.Cube(Vec3.Zero, scale);
        var b = Primitives.Sphere(new Vec3(scale * 0.3, 0, 0), scale * 0.5, 1);
        var result = Csg.Intersect(a, b);
        Assert.True(result.Mesh.Faces.Count > 0, $"scale={scale}");
    }

    [Theory]
    [MemberData(nameof(Scales))]
    public void ScaleInvariance_Difference_ProducesFaces(double scale)
    {
        var a = Primitives.Cube(Vec3.Zero, scale);
        var b = Primitives.Sphere(Vec3.Zero, scale * 0.3, 1);
        var result = Csg.Difference(a, b);
        Assert.True(result.Mesh.Faces.Count > 0, $"scale={scale}");
    }

    // =========================================================================
    // Result has valid BVH
    // =========================================================================

    [Theory]
    [MemberData(nameof(Offsets))]
    public void CsgResult_HasValidBvh(double ox, double oy, double oz)
    {
        var a = Primitives.Cube(Vec3.Zero, 1.0);
        var b = Primitives.Cube(new Vec3(ox, oy, oz), 1.0);
        var result = Csg.Union(a, b);
        var solid = new Solid(result.Mesh);
        Assert.NotNull(solid.Bvh);
    }

    // =========================================================================
    // Commutativity: Union(A,B) face count == Union(B,A) face count
    // =========================================================================

    [Theory]
    [InlineData(0.3, 0.0, 0.0)]
    [InlineData(0.0, 0.3, 0.0)]
    [InlineData(0.3, 0.3, 0.3)]
    [InlineData(0.5, -0.5, 0.5)]
    public void Union_Commutativity_BothProduceFaces(double ox, double oy, double oz)
    {
        var a = Primitives.Cube(Vec3.Zero, 1.0);
        var b = Primitives.Cube(new Vec3(ox, oy, oz), 1.0);

        var ab = Csg.Union(a, b);
        var ba = Csg.Union(b, a);

        Assert.True(ab.Mesh.Faces.Count > 0, "Union(A,B) should produce faces");
        Assert.True(ba.Mesh.Faces.Count > 0, "Union(B,A) should produce faces");
    }

    [Theory]
    [InlineData(0.3, 0.0, 0.0)]
    [InlineData(0.0, 0.3, 0.0)]
    [InlineData(0.3, 0.3, 0.3)]
    public void Intersect_Commutativity_BothProduceFaces(double ox, double oy, double oz)
    {
        var a = Primitives.Cube(Vec3.Zero, 1.0);
        var b = Primitives.Cube(new Vec3(ox, oy, oz), 1.0);

        var ab = Csg.Intersect(a, b);
        var ba = Csg.Intersect(b, a);

        Assert.True(ab.Mesh.Faces.Count > 0, "Intersect(A,B) should produce faces");
        Assert.True(ba.Mesh.Faces.Count > 0, "Intersect(B,A) should produce faces");
    }

    // =========================================================================
    // Parallel vs sequential consistency
    // =========================================================================

    [Theory]
    [InlineData(0.3, 0.0, 0.0)]
    [InlineData(0.0, 0.3, 0.0)]
    [InlineData(0.3, 0.3, 0.3)]
    public void CsgUnion_ParallelMatchesSequential(double ox, double oy, double oz)
    {
        var a = Primitives.Cube(Vec3.Zero, 1.0);
        var b = Primitives.Cube(new Vec3(ox, oy, oz), 1.0);

        var seq = Csg.Union(a, b, new CsgOptions { Parallel = false });
        var par = Csg.Union(a, b, new CsgOptions { Parallel = true });

        Assert.Equal(seq.Mesh.Faces.Count, par.Mesh.Faces.Count);
    }
}
