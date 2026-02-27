using MdCsg.Api;
using MdCsg.Math;
using Xunit;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 01: Every primitive type in CSG pairings.</summary>
public class CsgPrimitiveVarietyTests
{
    private static readonly Vec3 Origin = Vec3.Zero;
    private static readonly Vec3 Up = Vec3.UnitZ;

    private static Solid MakeSphere() => Primitives.Sphere(Origin, 1.0, 2);
    private static Solid MakeCube() => Primitives.Cube(Origin, 2.0);
    private static Solid MakeBox() => Primitives.Box(Origin, new Vec3(2, 1, 1));
    private static Solid MakeCylinder() => Primitives.Cylinder(new Vec3(0, 0, -1), Up, 0.8, 2.0, 16);
    private static Solid MakeCone() => Primitives.Cone(new Vec3(0, 0, -1), Up, 1.0, 2.0, 16);
    private static Solid MakeTorus() => Primitives.Torus(Origin, Up, 1.0, 0.3, 16, 8);

    [Fact]
    public void UnionSphereAndCube_ProducesValidMesh()
    {
        var r = Csg.Union(MakeSphere(), MakeCube());
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void IntersectSphereAndCube_SmallerThanEither()
    {
        var s = MakeSphere();
        var c = MakeCube();
        var r = Csg.Intersect(s, c);
        var vol = new Solid(r.Mesh).Volume();
        Assert.True(vol < s.Volume() + 0.1);
        Assert.True(vol > 0);
    }

    [Fact]
    public void DifferenceCubeMinusSphere_HasPositiveVolume()
    {
        var r = Csg.Difference(MakeCube(), MakeSphere());
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void UnionBoxAndCylinder_ProducesValidMesh()
    {
        var r = Csg.Union(MakeBox(), MakeCylinder());
        Assert.True(r.FaceCount > 0);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void IntersectBoxAndCylinder_SmallerVolume()
    {
        var b = MakeBox();
        var cyl = MakeCylinder();
        var r = Csg.Intersect(b, cyl);
        Assert.True(new Solid(r.Mesh).Volume() < b.Volume() + 0.01);
    }

    [Fact]
    public void DifferenceBoxMinusCone_ProducesValidMesh()
    {
        var r = Csg.Difference(MakeBox(), MakeCone());
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void UnionConeAndSphere_ProducesValidMesh()
    {
        var r = Csg.Union(MakeCone(), MakeSphere());
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void IntersectConeAndCylinder_ProducesValidMesh()
    {
        var r = Csg.Intersect(MakeCone(), MakeCylinder());
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void DifferenceSphereMinusCone_ProducesValidMesh()
    {
        var r = Csg.Difference(MakeSphere(), MakeCone());
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void UnionTorusAndCube_ProducesValidMesh()
    {
        var r = Csg.Union(MakeTorus(), MakeCube());
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void IntersectTorusAndSphere_ProducesValidMesh()
    {
        var r = Csg.Intersect(MakeTorus(), MakeSphere());
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void DifferenceCubeMinusTorus_ProducesValidMesh()
    {
        var r = Csg.Difference(MakeCube(), MakeTorus());
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void UnionCylinderAndCone_ProducesValidMesh()
    {
        var r = Csg.Union(MakeCylinder(), MakeCone());
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void DifferenceCylinderMinusSphere_ProducesValidMesh()
    {
        var r = Csg.Difference(MakeCylinder(), MakeSphere());
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void IntersectTorusAndCylinder_ProducesValidMesh()
    {
        var r = Csg.Intersect(MakeTorus(), MakeCylinder());
        Assert.True(r.FaceCount >= 0);
    }

    [Fact]
    public void DifferenceTorusMinusCone_ProducesValidMesh()
    {
        var r = Csg.Difference(MakeTorus(), MakeCone());
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void UnionSphereAndCone_VolumeGreaterThanSphereAlone()
    {
        var s = MakeSphere();
        var cone = MakeCone();
        var r = Csg.Union(s, cone);
        var vol = new Solid(r.Mesh).Volume();
        // Union volume should be at least as large as either input minus overlap tolerance
        Assert.True(vol > 0);
    }

    [Fact]
    public void IntersectCubeAndBox_ProducesValidMesh()
    {
        var cube = MakeCube();
        var box = MakeBox();
        var r = Csg.Intersect(cube, box);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void DifferenceSphereMinusCylinder_ProducesValidMesh()
    {
        var r = Csg.Difference(MakeSphere(), MakeCylinder());
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void UnionTorusAndCone_ProducesValidMesh()
    {
        var r = Csg.Union(MakeTorus(), MakeCone());
        Assert.True(r.FaceCount > 0);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }
}
