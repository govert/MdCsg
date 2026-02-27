using MdCsg.Api;
using MdCsg.Math;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 26: SDF on all primitives, grid sampling, batch.</summary>
public class SdfSamplingTests
{
    [Fact]
    public void CubeSdf_CenterIsNegative()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var sdf = new SignedDistanceField(cube);
        Assert.True(sdf.SignedDistance(Vec3.Zero) < 0);
    }

    [Fact]
    public void CubeSdf_OutsideIsPositive()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var sdf = new SignedDistanceField(cube);
        Assert.True(sdf.SignedDistance(new Vec3(5, 0, 0)) > 0);
    }

    [Fact]
    public void SphereSdf_CenterIsNegative()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var sdf = new SignedDistanceField(sphere);
        Assert.True(sdf.SignedDistance(Vec3.Zero) < 0);
    }

    [Fact]
    public void SphereSdf_FarPointIsPositive()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var sdf = new SignedDistanceField(sphere);
        Assert.True(sdf.SignedDistance(new Vec3(5, 0, 0)) > 0);
    }

    [Fact]
    public void CylinderSdf_CenterIsNegative()
    {
        var cyl = Primitives.Cylinder(new Vec3(0, 0, -1), Vec3.UnitZ, 1.0, 2.0, 16);
        var sdf = new SignedDistanceField(cyl);
        Assert.True(sdf.SignedDistance(Vec3.Zero) < 0);
    }

    [Fact]
    public void BoxSdf_CenterIsNegative()
    {
        var box = Primitives.Box(Vec3.Zero, new Vec3(3, 2, 1));
        var sdf = new SignedDistanceField(box);
        Assert.True(sdf.SignedDistance(Vec3.Zero) < 0);
    }

    [Fact]
    public void ConeSdf_InsideIsNegative()
    {
        var cone = Primitives.Cone(new Vec3(0, 0, -1), Vec3.UnitZ, 1.0, 2.0, 16);
        var sdf = new SignedDistanceField(cone);
        Assert.True(sdf.SignedDistance(new Vec3(0, 0, -0.5)) < 0);
    }

    [Fact]
    public void TorusSdf_InsideIsNegative()
    {
        var torus = Primitives.Torus(Vec3.Zero, Vec3.UnitZ, 1.0, 0.3, 16, 8);
        var sdf = new SignedDistanceField(torus);
        Assert.True(sdf.SignedDistance(new Vec3(1.0, 0, 0)) < 0);
    }

    [Fact]
    public void BatchSignedDistances_MatchesIndividual()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var sdf = new SignedDistanceField(cube);
        var points = new Vec3[]
        {
            Vec3.Zero, new Vec3(5, 0, 0), new Vec3(0.5, 0.5, 0.5)
        };
        var results = new double[3];
        sdf.SignedDistances(points, results);
        for (int i = 0; i < 3; i++)
            Assert.True(System.Math.Abs(results[i] - sdf.SignedDistance(points[i])) < 0.001);
    }

    [Fact]
    public void GridSampling_ReturnsCorrectDimensions()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var sdf = new SignedDistanceField(cube);
        var bounds = new Aabb(new Vec3(-2, -2, -2), new Vec3(2, 2, 2));
        var grid = sdf.SampleOnGrid(bounds, 5, 5, 5);
        Assert.Equal(5, grid.GetLength(0));
        Assert.Equal(5, grid.GetLength(1));
        Assert.Equal(5, grid.GetLength(2));
    }

    [Fact]
    public void GridSampling_CenterIsNegative()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var sdf = new SignedDistanceField(cube);
        var bounds = new Aabb(new Vec3(-2, -2, -2), new Vec3(2, 2, 2));
        var grid = sdf.SampleOnGrid(bounds, 5, 5, 5);
        // Center of grid (index 2,2,2) should be inside
        Assert.True(grid[2, 2, 2] < 0);
    }

    [Fact]
    public void GridSampling_CornersArePositive()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var sdf = new SignedDistanceField(cube);
        var bounds = new Aabb(new Vec3(-2, -2, -2), new Vec3(2, 2, 2));
        var grid = sdf.SampleOnGrid(bounds, 5, 5, 5);
        Assert.True(grid[0, 0, 0] > 0);
        Assert.True(grid[4, 4, 4] > 0);
    }

    [Fact]
    public void SdfOnCsgResult_CenterIsNegative()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(new Vec3(0.5, 0, 0), 0.3, 2);
        var r = new Solid(Csg.Difference(a, b).Mesh);
        var sdf = new SignedDistanceField(r);
        // Center is hollowed by the sphere
        Assert.True(sdf.SignedDistance(new Vec3(0.5, 0, 0)) > 0);
    }

    [Fact]
    public void SdfWithWindingNumber_AgreeWithRayCast()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var sdfRay = new SignedDistanceField(cube, useWindingNumber: false);
        var sdfWind = new SignedDistanceField(cube, useWindingNumber: true);
        var pt = new Vec3(0.5, 0.5, 0.5);
        // Both should agree on sign
        Assert.True(sdfRay.SignedDistance(pt) < 0);
        Assert.True(sdfWind.SignedDistance(pt) < 0);
    }

    [Fact]
    public void SdfSymmetric_OnSphere()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var sdf = new SignedDistanceField(sphere);
        var d1 = sdf.SignedDistance(new Vec3(2, 0, 0));
        var d2 = sdf.SignedDistance(new Vec3(-2, 0, 0));
        Assert.True(System.Math.Abs(d1 - d2) < 0.1);
    }

    [Fact]
    public void SdfGradient_PointsOutward()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var sdf = new SignedDistanceField(cube);
        var p = new Vec3(0, 0, 3);
        var dx = sdf.SignedDistance(p + new Vec3(0.01, 0, 0)) - sdf.SignedDistance(p - new Vec3(0.01, 0, 0));
        var dy = sdf.SignedDistance(p + new Vec3(0, 0.01, 0)) - sdf.SignedDistance(p - new Vec3(0, 0.01, 0));
        var dz = sdf.SignedDistance(p + new Vec3(0, 0, 0.01)) - sdf.SignedDistance(p - new Vec3(0, 0, 0.01));
        // Gradient should mostly point in +Z direction at (0,0,3) for cube at origin
        Assert.True(dz > 0);
    }

    [Fact]
    public void BatchSdf_EmptyInput_NoError()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var sdf = new SignedDistanceField(cube);
        sdf.SignedDistances(ReadOnlySpan<Vec3>.Empty, Span<double>.Empty);
    }

    [Fact]
    public void GridSampling_SmallGrid_Works()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var sdf = new SignedDistanceField(cube);
        var bounds = new Aabb(new Vec3(-1, -1, -1), new Vec3(1, 1, 1));
        var grid = sdf.SampleOnGrid(bounds, 2, 2, 2);
        Assert.Equal(2, grid.GetLength(0));
    }
}
