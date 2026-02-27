using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 18: Volume/Area via MeshQueries vs Solid API.</summary>
public class VolumeAndAreaConsistencyTests
{
    private const double Tol = 0.01;

    [Fact]
    public void CubeVolume_ApiMatchesMeshQueries()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var apiVol = cube.Volume();
        var mqVol = System.Math.Abs(MeshQueries.Volume(cube.Mesh));
        Assert.True(System.Math.Abs(apiVol - mqVol) < Tol);
    }

    [Fact]
    public void CubeArea_ApiMatchesMeshQueries()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var apiArea = cube.SurfaceArea();
        var mqArea = MeshQueries.SurfaceArea(cube.Mesh);
        Assert.True(System.Math.Abs(apiArea - mqArea) < Tol);
    }

    [Fact]
    public void SphereVolume_ApiMatchesMeshQueries()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var apiVol = sphere.Volume();
        var mqVol = System.Math.Abs(MeshQueries.Volume(sphere.Mesh));
        Assert.True(System.Math.Abs(apiVol - mqVol) < Tol);
    }

    [Fact]
    public void SphereArea_ApiMatchesMeshQueries()
    {
        var sphere = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var apiArea = sphere.SurfaceArea();
        var mqArea = MeshQueries.SurfaceArea(sphere.Mesh);
        Assert.True(System.Math.Abs(apiArea - mqArea) < Tol);
    }

    [Fact]
    public void BoxVolume_MatchesExpected()
    {
        var box = Primitives.Box(Vec3.Zero, new Vec3(3, 2, 1));
        Assert.True(System.Math.Abs(box.Volume() - 6.0) < Tol);
    }

    [Fact]
    public void BoxArea_MatchesExpected()
    {
        var box = Primitives.Box(Vec3.Zero, new Vec3(3, 2, 1));
        double expected = 2 * (3 * 2 + 3 * 1 + 2 * 1); // 22
        Assert.True(System.Math.Abs(box.SurfaceArea() - expected) < Tol);
    }

    [Fact]
    public void CubeVolume_MatchesExpected()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        Assert.True(System.Math.Abs(cube.Volume() - 8.0) < Tol);
    }

    [Fact]
    public void CubeArea_MatchesExpected()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        Assert.True(System.Math.Abs(cube.SurfaceArea() - 24.0) < Tol);
    }

    [Fact]
    public void CylinderVolume_ApproximatelyCorrect()
    {
        var cyl = Primitives.Cylinder(Vec3.Zero, Vec3.UnitZ, 1.0, 2.0, 32);
        double expected = System.Math.PI * 1.0 * 1.0 * 2.0;
        Assert.True(System.Math.Abs(cyl.Volume() - expected) < 0.3);
    }

    [Fact]
    public void ConeVolume_ApproximatelyCorrect()
    {
        var cone = Primitives.Cone(Vec3.Zero, Vec3.UnitZ, 1.0, 2.0, 32);
        double expected = System.Math.PI * 1.0 * 1.0 * 2.0 / 3.0;
        Assert.True(System.Math.Abs(cone.Volume() - expected) < 0.3);
    }

    [Fact]
    public void CsgResult_VolumeConsistent()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(Vec3.Zero, 0.8, 2);
        var r = new Solid(Csg.Difference(a, b).Mesh);
        var apiVol = r.Volume();
        var mqVol = System.Math.Abs(MeshQueries.Volume(r.Mesh));
        Assert.True(System.Math.Abs(apiVol - mqVol) < Tol);
    }

    [Fact]
    public void CsgResult_AreaConsistent()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(Vec3.Zero, 0.8, 2);
        var r = new Solid(Csg.Difference(a, b).Mesh);
        var apiArea = r.SurfaceArea();
        var mqArea = MeshQueries.SurfaceArea(r.Mesh);
        Assert.True(System.Math.Abs(apiArea - mqArea) < Tol);
    }

    [Fact]
    public void VolumeAlwaysPositive()
    {
        var shapes = new[]
        {
            Primitives.Cube(Vec3.Zero, 2.0),
            Primitives.Sphere(Vec3.Zero, 1.0, 2),
            Primitives.Box(Vec3.Zero, new Vec3(3, 2, 1)),
            Primitives.Cylinder(Vec3.Zero, Vec3.UnitZ, 1.0, 2.0, 16),
            Primitives.Cone(Vec3.Zero, Vec3.UnitZ, 1.0, 2.0, 16),
        };
        foreach (var s in shapes)
            Assert.True(s.Volume() > 0);
    }

    [Fact]
    public void AreaAlwaysPositive()
    {
        var shapes = new[]
        {
            Primitives.Cube(Vec3.Zero, 2.0),
            Primitives.Sphere(Vec3.Zero, 1.0, 2),
            Primitives.Box(Vec3.Zero, new Vec3(3, 2, 1)),
        };
        foreach (var s in shapes)
            Assert.True(s.SurfaceArea() > 0);
    }

    [Fact]
    public void SphereVolume_ConvergesWithSubdivisions()
    {
        double expected = 4.0 / 3.0 * System.Math.PI;
        var s1 = Primitives.Sphere(Vec3.Zero, 1.0, 1).Volume();
        var s2 = Primitives.Sphere(Vec3.Zero, 1.0, 2).Volume();
        var s3 = Primitives.Sphere(Vec3.Zero, 1.0, 3).Volume();
        Assert.True(System.Math.Abs(s3 - expected) < System.Math.Abs(s1 - expected));
    }

    [Fact]
    public void AreaGreaterThanVolumePowerTwoThirds()
    {
        // For a unit cube: area = 24, vol = 8, area > vol^(2/3) always for solids > 1
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        Assert.True(cube.SurfaceArea() > System.Math.Pow(cube.Volume(), 2.0 / 3.0));
    }

    [Fact]
    public void UnionVolume_GreaterThanIntersection()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(0.5, 0, 0), 2.0);
        var unionVol = new Solid(Csg.Union(a, b).Mesh).Volume();
        var interVol = new Solid(Csg.Intersect(a, b).Mesh).Volume();
        Assert.True(unionVol > interVol);
    }

    [Fact]
    public void TransformedVolume_Preserved()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var t = Transform3.FromTranslation(new Vec3(5, 5, 5));
        var moved = cube.Transform(t);
        Assert.True(System.Math.Abs(cube.Volume() - moved.Volume()) < Tol);
    }
}
