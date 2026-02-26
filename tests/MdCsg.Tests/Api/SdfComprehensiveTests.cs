using MdCsg.Api;
using MdCsg.Math;

namespace MdCsg.Tests.Api;

/// <summary>
/// Comprehensive SDF tests: sign correctness, distance accuracy, various geometries.
/// </summary>
public class SdfComprehensiveTests
{
    // =========================================================================
    // Cube SDF: grid of points along each axis
    // =========================================================================

    public static IEnumerable<object[]> CubeAxisPositions()
    {
        // Points along X axis from -2 to 3 (cube at [0,1])
        for (int i = -20; i <= 30; i++)
        {
            double x = i * 0.1;
            yield return new object[] { x, 0.5, 0.5, x > 0 && x < 1 };
        }
    }

    [Theory]
    [MemberData(nameof(CubeAxisPositions))]
    public void CubeSdf_AlongXAxis_SignCorrect(double x, double y, double z, bool shouldBeInside)
    {
        var sdf = new SignedDistanceField(Primitives.Cube(new Vec3(0.5, 0.5, 0.5), 1.0));
        double d = sdf.SignedDistance(new Vec3(x, y, z));
        if (shouldBeInside)
            Assert.True(d < 0, $"({x},{y},{z}) should be inside, got {d}");
        else if (x < -0.05 || x > 1.05)
            Assert.True(d > 0, $"({x},{y},{z}) should be outside, got {d}");
    }

    // =========================================================================
    // Sphere SDF: radial sign check
    // =========================================================================

    public static IEnumerable<object[]> SphereRadialPositions()
    {
        for (int i = 0; i <= 30; i++)
        {
            double r = i * 0.1;
            bool inside = r < 1.0;
            yield return new object[] { r, inside };
        }
    }

    [Theory]
    [MemberData(nameof(SphereRadialPositions))]
    public void SphereSdf_RadialSignCorrect(double r, bool shouldBeInside)
    {
        var sdf = new SignedDistanceField(Primitives.Sphere(Vec3.Zero, 1.0, 2));
        double d = sdf.SignedDistance(new Vec3(r, 0, 0));
        if (shouldBeInside && r < 0.9)
            Assert.True(d < 0, $"r={r} should be inside, got {d}");
        else if (!shouldBeInside && r > 1.1)
            Assert.True(d > 0, $"r={r} should be outside, got {d}");
    }

    // =========================================================================
    // SDF: various cube centers
    // =========================================================================

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(5, 5, 5)]
    [InlineData(-10, 0, 10)]
    [InlineData(100, 200, 300)]
    public void CubeSdf_CenterIsInside(double cx, double cy, double cz)
    {
        var center = new Vec3(cx, cy, cz);
        var sdf = new SignedDistanceField(Primitives.Cube(center, 2.0));
        double d = sdf.SignedDistance(center);
        Assert.True(d < 0, $"Center ({cx},{cy},{cz}) should be inside, got {d}");
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(5, 5, 5)]
    [InlineData(-10, 0, 10)]
    [InlineData(100, 200, 300)]
    public void CubeSdf_FarPointIsOutside(double cx, double cy, double cz)
    {
        var center = new Vec3(cx, cy, cz);
        var sdf = new SignedDistanceField(Primitives.Cube(center, 2.0));
        double d = sdf.SignedDistance(center + new Vec3(100, 0, 0));
        Assert.True(d > 0, $"Far point should be outside, got {d}");
    }

    // =========================================================================
    // SDF: cylinder
    // =========================================================================

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(0, 0, 0.5)]
    [InlineData(0, 0, -0.5)]
    [InlineData(0.2, 0, 0)]
    [InlineData(0, 0.2, 0)]
    public void CylinderSdf_InsidePoints(double x, double y, double z)
    {
        var cyl = Primitives.Cylinder(new Vec3(0, 0, -1), Vec3.UnitZ, 0.5, 2.0);
        var sdf = new SignedDistanceField(cyl);
        double d = sdf.SignedDistance(new Vec3(x, y, z));
        Assert.True(d < 0, $"({x},{y},{z}) should be inside cylinder, got {d}");
    }

    [Theory]
    [InlineData(2, 0, 0)]
    [InlineData(0, 2, 0)]
    [InlineData(0, 0, 5)]
    [InlineData(0, 0, -5)]
    [InlineData(5, 5, 5)]
    public void CylinderSdf_OutsidePoints(double x, double y, double z)
    {
        var cyl = Primitives.Cylinder(new Vec3(0, 0, -1), Vec3.UnitZ, 0.5, 2.0);
        var sdf = new SignedDistanceField(cyl);
        double d = sdf.SignedDistance(new Vec3(x, y, z));
        Assert.True(d > 0, $"({x},{y},{z}) should be outside cylinder, got {d}");
    }

    // =========================================================================
    // SDF: torus
    // =========================================================================

    [Theory]
    [InlineData(2.0, 0, 0)]
    [InlineData(0, 2.0, 0)]
    [InlineData(-2.0, 0, 0)]
    [InlineData(0, -2.0, 0)]
    public void TorusSdf_OnRingCenterIsInside(double x, double y, double z)
    {
        var torus = Primitives.Torus(Vec3.Zero, Vec3.UnitZ, 2.0, 0.5, 24, 12);
        var sdf = new SignedDistanceField(torus);
        double d = sdf.SignedDistance(new Vec3(x, y, z));
        Assert.True(d < 0, $"({x},{y},{z}) should be inside torus, got {d}");
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(0, 0, 5)]
    [InlineData(5, 5, 5)]
    [InlineData(10, 0, 0)]
    public void TorusSdf_OutsidePoints(double x, double y, double z)
    {
        var torus = Primitives.Torus(Vec3.Zero, Vec3.UnitZ, 2.0, 0.5, 24, 12);
        var sdf = new SignedDistanceField(torus);
        double d = sdf.SignedDistance(new Vec3(x, y, z));
        Assert.True(d > 0, $"({x},{y},{z}) should be outside torus, got {d}");
    }

    // =========================================================================
    // SDF: CSG result
    // =========================================================================

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(0.2, 0, 0)]
    [InlineData(0, 0.2, 0)]
    [InlineData(0, 0, 0.2)]
    public void CsgResultSdf_InsidePointsNegative(double x, double y, double z)
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(0.5, 0, 0), 2.0);
        var result = Csg.Union(a, b);
        var solid = new Solid(result.Mesh);
        var sdf = new SignedDistanceField(solid);
        double d = sdf.SignedDistance(new Vec3(x, y, z));
        Assert.True(d < 0, $"({x},{y},{z}) should be inside union, got {d}");
    }

    // =========================================================================
    // SDF: distance monotonicity (farther = larger SDF)
    // =========================================================================

    [Theory]
    [InlineData(1.5, 2.0)]
    [InlineData(2.0, 3.0)]
    [InlineData(3.0, 5.0)]
    [InlineData(5.0, 10.0)]
    public void CubeSdf_DistanceMonotonicity(double near, double far)
    {
        var sdf = new SignedDistanceField(Primitives.Cube(new Vec3(0.5, 0.5, 0.5), 1.0));
        double dNear = sdf.SignedDistance(new Vec3(near, 0.5, 0.5));
        double dFar = sdf.SignedDistance(new Vec3(far, 0.5, 0.5));
        Assert.True(dFar > dNear, $"near={dNear}, far={dFar}");
    }
}
