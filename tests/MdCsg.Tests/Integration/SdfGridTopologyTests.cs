using MdCsg.Api;
using MdCsg.Math;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 28: SDF grid sign changes at boundaries.</summary>
public class SdfGridTopologyTests
{
    [Fact]
    public void CubeGrid_HasSignChanges()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var sdf = new SignedDistanceField(cube);
        var bounds = new Aabb(new Vec3(-2, -2, -2), new Vec3(2, 2, 2));
        var grid = sdf.SampleOnGrid(bounds, 10, 10, 10);
        bool hasSignChange = false;
        for (int x = 0; x < 9; x++)
            for (int y = 0; y < 10; y++)
                for (int z = 0; z < 10; z++)
                    if (grid[x, y, z] * grid[x + 1, y, z] < 0)
                        hasSignChange = true;
        Assert.True(hasSignChange);
    }

    [Fact]
    public void SphereGrid_HasSignChanges()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var sdf = new SignedDistanceField(sphere);
        var bounds = new Aabb(new Vec3(-2, -2, -2), new Vec3(2, 2, 2));
        var grid = sdf.SampleOnGrid(bounds, 10, 10, 10);
        bool hasSignChange = false;
        for (int x = 0; x < 9; x++)
            for (int y = 0; y < 10; y++)
                for (int z = 0; z < 10; z++)
                    if (grid[x, y, z] * grid[x + 1, y, z] < 0)
                        hasSignChange = true;
        Assert.True(hasSignChange);
    }

    [Fact]
    public void CubeGrid_InteriorIsNegative()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var sdf = new SignedDistanceField(cube);
        var bounds = new Aabb(new Vec3(-0.5, -0.5, -0.5), new Vec3(0.5, 0.5, 0.5));
        var grid = sdf.SampleOnGrid(bounds, 3, 3, 3);
        for (int x = 0; x < 3; x++)
            for (int y = 0; y < 3; y++)
                for (int z = 0; z < 3; z++)
                    Assert.True(grid[x, y, z] < 0);
    }

    [Fact]
    public void CubeGrid_ExteriorIsPositive()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var sdf = new SignedDistanceField(cube);
        var bounds = new Aabb(new Vec3(3, 3, 3), new Vec3(5, 5, 5));
        var grid = sdf.SampleOnGrid(bounds, 3, 3, 3);
        for (int x = 0; x < 3; x++)
            for (int y = 0; y < 3; y++)
                for (int z = 0; z < 3; z++)
                    Assert.True(grid[x, y, z] > 0);
    }

    [Fact]
    public void GridSignChanges_OnAllAxes()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var sdf = new SignedDistanceField(cube);
        var bounds = new Aabb(new Vec3(-2, -2, -2), new Vec3(2, 2, 2));
        var grid = sdf.SampleOnGrid(bounds, 10, 10, 10);
        bool xChange = false, yChange = false, zChange = false;
        for (int x = 0; x < 9; x++)
            for (int y = 0; y < 10; y++)
                for (int z = 0; z < 10; z++)
                {
                    if (grid[x, y, z] * grid[x + 1, y, z] < 0) xChange = true;
                }
        for (int x = 0; x < 10; x++)
            for (int y = 0; y < 9; y++)
                for (int z = 0; z < 10; z++)
                {
                    if (grid[x, y, z] * grid[x, y + 1, z] < 0) yChange = true;
                }
        for (int x = 0; x < 10; x++)
            for (int y = 0; y < 10; y++)
                for (int z = 0; z < 9; z++)
                {
                    if (grid[x, y, z] * grid[x, y, z + 1] < 0) zChange = true;
                }
        Assert.True(xChange);
        Assert.True(yChange);
        Assert.True(zChange);
    }

    [Fact]
    public void SphereGrid_InteriorAllNegative()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var sdf = new SignedDistanceField(sphere);
        var bounds = new Aabb(new Vec3(-0.3, -0.3, -0.3), new Vec3(0.3, 0.3, 0.3));
        var grid = sdf.SampleOnGrid(bounds, 3, 3, 3);
        for (int x = 0; x < 3; x++)
            for (int y = 0; y < 3; y++)
                for (int z = 0; z < 3; z++)
                    Assert.True(grid[x, y, z] < 0);
    }

    [Fact]
    public void CsgResult_GridHasSignChanges()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(Vec3.Zero, 0.8, 2);
        var r = new Solid(Csg.Difference(a, b).Mesh);
        var sdf = new SignedDistanceField(r);
        var bounds = new Aabb(new Vec3(-2, -2, -2), new Vec3(2, 2, 2));
        var grid = sdf.SampleOnGrid(bounds, 8, 8, 8);
        bool hasSignChange = false;
        for (int x = 0; x < 7; x++)
            for (int y = 0; y < 8; y++)
                for (int z = 0; z < 8; z++)
                    if (grid[x, y, z] * grid[x + 1, y, z] < 0)
                        hasSignChange = true;
        Assert.True(hasSignChange);
    }

    [Fact]
    public void GridValues_AreContinuous()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var sdf = new SignedDistanceField(cube);
        var bounds = new Aabb(new Vec3(-2, -2, -2), new Vec3(2, 2, 2));
        var grid = sdf.SampleOnGrid(bounds, 10, 10, 10);
        double maxJump = 0;
        double cellSize = 4.0 / 9;
        for (int x = 0; x < 9; x++)
            for (int y = 0; y < 10; y++)
                for (int z = 0; z < 10; z++)
                {
                    double jump = System.Math.Abs(grid[x + 1, y, z] - grid[x, y, z]);
                    if (jump > maxJump) maxJump = jump;
                }
        Assert.True(maxJump < cellSize * 3); // Lipschitz bound
    }

    [Fact]
    public void LargeGrid_AllValuesFinite()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var sdf = new SignedDistanceField(cube);
        var bounds = new Aabb(new Vec3(-3, -3, -3), new Vec3(3, 3, 3));
        var grid = sdf.SampleOnGrid(bounds, 8, 8, 8);
        for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
                for (int z = 0; z < 8; z++)
                {
                    Assert.False(double.IsNaN(grid[x, y, z]));
                    Assert.False(double.IsInfinity(grid[x, y, z]));
                }
    }

    [Fact]
    public void SdfGrid_SignChangeCorrelatesWithBoundary()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var sdf = new SignedDistanceField(cube);
        // Sample along X axis through the center
        int signChanges = 0;
        double prev = sdf.SignedDistance(new Vec3(-2, 0, 0));
        for (int i = 1; i <= 20; i++)
        {
            double x = -2 + i * 0.2;
            double val = sdf.SignedDistance(new Vec3(x, 0, 0));
            if (prev * val < 0) signChanges++;
            prev = val;
        }
        Assert.Equal(2, signChanges); // enter and exit the cube
    }

    [Fact]
    public void SdfGrid_SphereSignChangesAreTwo()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var sdf = new SignedDistanceField(sphere);
        int signChanges = 0;
        double prev = sdf.SignedDistance(new Vec3(-2, 0, 0));
        for (int i = 1; i <= 20; i++)
        {
            double x = -2 + i * 0.2;
            double val = sdf.SignedDistance(new Vec3(x, 0, 0));
            if (prev * val < 0) signChanges++;
            prev = val;
        }
        Assert.Equal(2, signChanges);
    }

    [Fact]
    public void SdfGrid_SymmetricCube()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var sdf = new SignedDistanceField(cube);
        var d1 = sdf.SignedDistance(new Vec3(1.5, 0, 0));
        var d2 = sdf.SignedDistance(new Vec3(-1.5, 0, 0));
        Assert.True(System.Math.Abs(d1 - d2) < 0.1);
    }

    [Fact]
    public void SdfGrid_CylinderHasSignChanges()
    {
        var cyl = Primitives.Cylinder(new Vec3(0, 0, -1), Vec3.UnitZ, 1.0, 2.0, 16);
        var sdf = new SignedDistanceField(cyl);
        var bounds = new Aabb(new Vec3(-2, -2, -2), new Vec3(2, 2, 2));
        var grid = sdf.SampleOnGrid(bounds, 6, 6, 6);
        bool hasChange = false;
        for (int x = 0; x < 5; x++)
            for (int y = 0; y < 6; y++)
                for (int z = 0; z < 6; z++)
                    if (grid[x, y, z] * grid[x + 1, y, z] < 0) hasChange = true;
        Assert.True(hasChange);
    }

    [Fact]
    public void SdfGrid_BoxHasSignChanges()
    {
        var box = Primitives.Box(Vec3.Zero, new Vec3(3, 2, 1));
        var sdf = new SignedDistanceField(box);
        var bounds = new Aabb(new Vec3(-3, -3, -3), new Vec3(3, 3, 3));
        var grid = sdf.SampleOnGrid(bounds, 8, 8, 8);
        bool hasChange = false;
        for (int x = 0; x < 7; x++)
            if (grid[x, 4, 4] * grid[x + 1, 4, 4] < 0) hasChange = true;
        Assert.True(hasChange);
    }

    [Fact]
    public void SdfGrid_1x1x1_Works()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var sdf = new SignedDistanceField(cube);
        var bounds = new Aabb(Vec3.Zero, Vec3.Zero);
        var grid = sdf.SampleOnGrid(bounds, 1, 1, 1);
        Assert.True(grid[0, 0, 0] < 0); // center of cube
    }

    [Fact]
    public void SdfGrid_MagnitudeIncreasesWithDistance()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var sdf = new SignedDistanceField(cube);
        double d1 = sdf.SignedDistance(new Vec3(2, 0, 0));
        double d5 = sdf.SignedDistance(new Vec3(5, 0, 0));
        Assert.True(d5 > d1);
    }
}
