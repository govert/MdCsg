using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Api;

public class SignedDistanceFieldTests
{
    // =========================================================================
    // Unit cube SDF
    // =========================================================================

    [Fact]
    public void UnitCube_Center_IsNegative()
    {
        var cube = Primitives.Cube(new Vec3(0.5, 0.5, 0.5), 1.0);
        var sdf = new SignedDistanceField(cube);
        double d = sdf.SignedDistance(new Vec3(0.5, 0.5, 0.5));
        Assert.True(d < 0, $"Center of cube should be inside (negative SDF), got {d}");
    }

    [Fact]
    public void UnitCube_Outside_IsPositive()
    {
        var cube = Primitives.Cube(new Vec3(0.5, 0.5, 0.5), 1.0);
        var sdf = new SignedDistanceField(cube);
        double d = sdf.SignedDistance(new Vec3(2.0, 0.5, 0.5));
        Assert.True(d > 0, $"Point outside cube should be positive SDF, got {d}");
    }

    [Fact]
    public void UnitCube_NearSurface_SmallAbsoluteValue()
    {
        var cube = Primitives.Cube(new Vec3(0.5, 0.5, 0.5), 1.0);
        var sdf = new SignedDistanceField(cube);
        // Point slightly outside the +X face
        double d = sdf.SignedDistance(new Vec3(1.01, 0.5, 0.5));
        Assert.True(d > 0, $"Point just outside should be positive, got {d}");
        Assert.True(d < 0.1, $"Point just outside should have small SDF, got {d}");
    }

    // =========================================================================
    // Sphere SDF vs. analytic
    // =========================================================================

    [Fact]
    public void Sphere_Center_EqualsNegativeRadius()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        var sdf = new SignedDistanceField(sphere);
        double d = sdf.SignedDistance(Vec3.Zero);
        // Should be approximately -1.0 (distance from center to surface)
        Assert.True(d < 0, $"Center of sphere should be inside, got {d}");
        Assert.True(System.Math.Abs(d + 1.0) < 0.05,
            $"Center SDF should be ~-1.0, got {d}");
    }

    [Fact]
    public void Sphere_FarOutside_MatchesAnalytic()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        var sdf = new SignedDistanceField(sphere);
        var point = new Vec3(3.0, 0, 0);
        double d = sdf.SignedDistance(point);
        double expected = 2.0; // |p| - r = 3.0 - 1.0
        Assert.True(d > 0, $"Point outside sphere should be positive, got {d}");
        Assert.True(System.Math.Abs(d - expected) < 0.1,
            $"Sphere SDF at (3,0,0): expected ~{expected}, got {d}");
    }

    [Fact]
    public void Sphere_HalfwayInside_MatchesAnalytic()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        var sdf = new SignedDistanceField(sphere);
        var point = new Vec3(0.5, 0, 0);
        double d = sdf.SignedDistance(point);
        double expected = -0.5; // |p| - r = 0.5 - 1.0
        Assert.True(d < 0, $"Point inside sphere should be negative, got {d}");
        Assert.True(System.Math.Abs(d - expected) < 0.05,
            $"Sphere SDF at (0.5,0,0): expected ~{expected}, got {d}");
    }

    [Fact]
    public void Sphere_SignFlipsAtSurface()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        var sdf = new SignedDistanceField(sphere);

        double inside = sdf.SignedDistance(new Vec3(0.9, 0, 0));
        double outside = sdf.SignedDistance(new Vec3(1.1, 0, 0));

        Assert.True(inside < 0, $"Inside point should be negative, got {inside}");
        Assert.True(outside > 0, $"Outside point should be positive, got {outside}");
    }

    // =========================================================================
    // Batch query
    // =========================================================================

    [Fact]
    public void SignedDistances_Batch_MatchesIndividualQueries()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var sdf = new SignedDistanceField(cube);

        var points = new Vec3[]
        {
            Vec3.Zero,             // inside
            new Vec3(2, 0, 0),     // outside
            new Vec3(0.5, 0, 0),   // inside
        };

        var results = new double[points.Length];
        sdf.SignedDistances(points, results);

        for (int i = 0; i < points.Length; i++)
        {
            double expected = sdf.SignedDistance(points[i]);
            Assert.Equal(expected, results[i]);
        }
    }

    // =========================================================================
    // Grid sampling
    // =========================================================================

    [Fact]
    public void SampleOnGrid_Sphere_InsideCountApproximatesVolume()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        var sdf = new SignedDistanceField(sphere);

        int n = 20;
        var bounds = new Aabb(new Vec3(-1.5, -1.5, -1.5), new Vec3(1.5, 1.5, 1.5));
        var grid = sdf.SampleOnGrid(bounds, n, n, n);

        int insideCount = 0;
        for (int x = 0; x < n; x++)
            for (int y = 0; y < n; y++)
                for (int z = 0; z < n; z++)
                    if (grid[x, y, z] < 0) insideCount++;

        double cellVolume = (3.0 / (n - 1)) * (3.0 / (n - 1)) * (3.0 / (n - 1));
        double approxVolume = insideCount * cellVolume;
        double expected = 4.0 / 3.0 * System.Math.PI; // ~4.189

        Assert.True(System.Math.Abs(approxVolume - expected) < 1.0,
            $"Grid volume estimate: expected ~{expected:F2}, got {approxVolume:F2}");
    }

    [Fact]
    public void SampleOnGrid_Cube_AllCornersOutside()
    {
        var cube = Primitives.Cube(Vec3.Zero, 1.0);
        var sdf = new SignedDistanceField(cube);

        var bounds = new Aabb(new Vec3(-2, -2, -2), new Vec3(2, 2, 2));
        var grid = sdf.SampleOnGrid(bounds, 3, 3, 3);

        // Corner of sampling grid (far from cube) should be positive
        Assert.True(grid[0, 0, 0] > 0);
        Assert.True(grid[2, 2, 2] > 0);

        // Center of sampling grid (cube center) should be negative
        Assert.True(grid[1, 1, 1] < 0);
    }
}
