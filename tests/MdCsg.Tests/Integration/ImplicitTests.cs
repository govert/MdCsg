using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

public class ImplicitTests
{
    // =========================================================================
    // Sphere: Intersect
    // =========================================================================

    [Fact]
    public void Intersect_CubeWithImplicitSphere_ProducesValidMesh()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2); // [0,2]^3
        var sphere = new ImplicitSphere(new Vec3(1, 1, 1), 1.2); // centered in cube

        var result = Csg.Intersect(cube, sphere);
        Assert.True(result.FaceCount > 0, "Intersect(cube, sphere) should produce faces");
    }

    [Fact]
    public void Intersect_CubeWithLargeSphere_PreservesCubeVolume()
    {
        // A sphere large enough to fully contain the cube should preserve the full cube
        var cube = MeshFactory.CreateCube(Vec3.Zero); // [0,1]^3
        var sphere = new ImplicitSphere(new Vec3(0.5, 0.5, 0.5), 10.0);

        var result = Csg.Intersect(cube, sphere);
        Assert.True(result.FaceCount > 0);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol > 0.9 && vol < 1.1,
            $"Cube fully inside sphere should preserve volume ~1.0, got {vol}");
    }

    [Fact]
    public void Intersect_CubeWithSphereAtCorner_ProducesSmallVolume()
    {
        // Sphere centered at cube corner so edges clearly cross the sphere
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2); // [0,2]^3
        var sphere = new ImplicitSphere(Vec3.Zero, 0.8); // centered at corner (0,0,0)

        var result = Csg.Intersect(cube, sphere);
        Assert.True(result.FaceCount > 0, "Sphere at cube corner should produce faces");
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        // 1/8 of sphere volume = 1/8 * 4/3 * pi * 0.8^3 ≈ 0.268
        Assert.True(vol > 0.05 && vol < 1.0,
            $"Intersect with sphere at corner should produce small volume, got {vol}");
    }

    // =========================================================================
    // Sphere: Difference
    // =========================================================================

    [Fact]
    public void Difference_CubeWithImplicitSphere_ProducesValidMesh()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2); // [0,2]^3
        var sphere = new ImplicitSphere(new Vec3(1, 1, 1), 0.8); // sphere inside cube

        var result = Csg.Difference(cube, sphere);
        Assert.True(result.FaceCount > 0, "Difference(cube, sphere) should produce faces");
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        // Cube volume = 8, sphere volume ~ 2.14, difference ~ 5.86
        Assert.True(vol > 3.0,
            $"Difference should have substantial volume, got {vol}");
    }

    [Fact]
    public void Difference_CubeWithLargeSphere_ProducesEmptyOrSmall()
    {
        // Sphere contains the entire cube → nothing left after subtraction
        var cube = MeshFactory.CreateCube(Vec3.Zero); // [0,1]^3
        var sphere = new ImplicitSphere(new Vec3(0.5, 0.5, 0.5), 10.0);

        var result = Csg.Difference(cube, sphere);
        double vol = result.FaceCount > 0
            ? VolumeCalculator.ComputeAbsoluteVolume(result.Mesh)
            : 0;
        Assert.True(vol < 0.1,
            $"Cube minus enclosing sphere should have near-zero volume, got {vol}");
    }

    // =========================================================================
    // Cylinder: Intersect
    // =========================================================================

    [Fact]
    public void Intersect_CubeWithImplicitCylinder_ProducesValidMesh()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2); // [0,2]^3
        // Vertical cylinder through center of cube
        var cylinder = new ImplicitCylinder(
            new Vec3(1, 1, -1), // base below cube
            new Vec3(0, 0, 1),  // Z-axis
            0.8,                // radius
            4.0);               // height (extends through cube)

        var result = Csg.Intersect(cube, cylinder);
        Assert.True(result.FaceCount > 0, "Intersect(cube, cylinder) should produce faces");
    }

    [Fact]
    public void Intersect_CubeWithLargeCylinder_PreservesCubeVolume()
    {
        // A very large cylinder should contain the entire cube
        var cube = MeshFactory.CreateCube(Vec3.Zero); // [0,1]^3
        var cylinder = new ImplicitCylinder(
            new Vec3(0.5, 0.5, -10),
            new Vec3(0, 0, 1),
            100.0, // huge radius
            100.0); // huge height

        var result = Csg.Intersect(cube, cylinder);
        Assert.True(result.FaceCount > 0);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol > 0.9 && vol < 1.1,
            $"Cube fully inside cylinder should preserve volume ~1.0, got {vol}");
    }

    // =========================================================================
    // Cylinder: Difference
    // =========================================================================

    [Fact]
    public void Difference_CubeWithImplicitCylinder_ProducesValidMesh()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2); // [0,2]^3
        // Thin cylinder through center
        var cylinder = new ImplicitCylinder(
            new Vec3(1, 1, -1),
            new Vec3(0, 0, 1),
            0.3,
            4.0);

        var result = Csg.Difference(cube, cylinder);
        Assert.True(result.FaceCount > 0, "Difference(cube, cylinder) should produce faces");
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        // Cube vol = 8, cylinder vol within cube ~ pi*0.3^2*2 ~ 0.565, difference ~ 7.43
        Assert.True(vol > 5.0,
            $"Difference should have substantial volume, got {vol}");
    }

    // =========================================================================
    // Commutative: Intersect(ImplicitSolid, Solid)
    // =========================================================================

    [Fact]
    public void Intersect_ImplicitSphereWithCube_SameAsCubeWithSphere()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2);
        var sphere = new ImplicitSphere(new Vec3(1, 1, 1), 1.2);

        var result1 = Csg.Intersect(cube, sphere);
        var result2 = Csg.Intersect(sphere, cube);

        double vol1 = VolumeCalculator.ComputeAbsoluteVolume(result1.Mesh);
        double vol2 = VolumeCalculator.ComputeAbsoluteVolume(result2.Mesh);

        Assert.True(System.Math.Abs(vol1 - vol2) < 0.01,
            $"Intersect(cube, sphere)={vol1:F6} should match Intersect(sphere, cube)={vol2:F6}");
    }

    // =========================================================================
    // Unsupported operations
    // =========================================================================

    [Fact]
    public void Union_SolidWithImplicit_ThrowsNotSupported()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero);
        var sphere = new ImplicitSphere(Vec3.Zero, 1);
        Assert.Throws<NotSupportedException>(() => Csg.Union(cube, sphere));
    }

    [Fact]
    public void Union_ImplicitWithSolid_ThrowsNotSupported()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero);
        var sphere = new ImplicitSphere(Vec3.Zero, 1);
        Assert.Throws<NotSupportedException>(() => Csg.Union(sphere, cube));
    }

    [Fact]
    public void Difference_ImplicitMinusSolid_ThrowsNotSupported()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero);
        var sphere = new ImplicitSphere(Vec3.Zero, 1);
        Assert.Throws<NotSupportedException>(() => Csg.Difference(sphere, cube));
    }

    // =========================================================================
    // Chained operations: implicit sphere then HalfSpace
    // =========================================================================

    [Fact]
    public void ChainedOps_IntersectSphereThenDifferenceHalfSpace()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2); // [0,2]^3
        var sphere = new ImplicitSphere(new Vec3(1, 1, 1), 1.2);

        // First: intersect cube with sphere
        var step1 = Csg.Intersect(cube, sphere);
        Assert.True(step1.FaceCount > 0);

        // Then: difference with a half-space (slice in half)
        var step1Solid = new Solid(step1.Mesh);
        var hs = HalfSpace.FromPointAndNormal(new Vec3(1, 0, 0), new Vec3(1, 0, 0)); // X > 1
        var step2 = Csg.Difference(step1Solid, hs);
        Assert.True(step2.FaceCount > 0, "Chained sphere-then-halfspace should produce faces");
    }

    // =========================================================================
    // Sphere vs meshed sphere comparison
    // =========================================================================

    [Fact]
    public void ImplicitSphere_IntersectCube_ReasonableVolume()
    {
        // Cube [0,2]^3 with sphere at center, radius 1.2
        // The face diagonals of the cube cross the sphere boundary
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2);
        var sphere = new ImplicitSphere(new Vec3(1, 1, 1), 1.2);

        var result = Csg.Intersect(cube, sphere);
        Assert.True(result.FaceCount > 0, "Intersection should produce faces");
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);

        // Full sphere volume = 4/3 * pi * 1.2^3 ≈ 7.24
        // Sphere partially extends beyond cube at corners, so intersection < sphere
        Assert.True(vol > 1.0 && vol < 8.0,
            $"Intersection volume should be between 1 and 8, got {vol}");
    }
}
