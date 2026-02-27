using MdCsg.Api;
using MdCsg.Math;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 13: Disjoint solids → intersection has 0 volume.</summary>
public class EmptyIntersectionTests
{
    [Fact]
    public void DisjointCubes_IntersectionEmpty()
    {
        var a = Primitives.Cube(new Vec3(-5, 0, 0), 2.0);
        var b = Primitives.Cube(new Vec3(5, 0, 0), 2.0);
        var r = Csg.Intersect(a, b);
        Assert.True(r.FaceCount == 0 || new Solid(r.Mesh).Volume() < 0.01);
    }

    [Fact]
    public void DisjointSpheres_IntersectionEmpty()
    {
        var a = Primitives.Sphere(new Vec3(-5, 0, 0), 1.0, 2);
        var b = Primitives.Sphere(new Vec3(5, 0, 0), 1.0, 2);
        var r = Csg.Intersect(a, b);
        Assert.True(r.FaceCount == 0 || new Solid(r.Mesh).Volume() < 0.01);
    }

    [Fact]
    public void DisjointCubeAndSphere_IntersectionEmpty()
    {
        var a = Primitives.Cube(new Vec3(-5, 0, 0), 2.0);
        var b = Primitives.Sphere(new Vec3(5, 0, 0), 1.0, 2);
        var r = Csg.Intersect(a, b);
        Assert.True(r.FaceCount == 0 || new Solid(r.Mesh).Volume() < 0.01);
    }

    [Fact]
    public void DisjointCubes_DifferenceEqualsMinuend()
    {
        var a = Primitives.Cube(new Vec3(-5, 0, 0), 2.0);
        var b = Primitives.Cube(new Vec3(5, 0, 0), 2.0);
        var r = Csg.Difference(a, b);
        Assert.True(System.Math.Abs(new Solid(r.Mesh).Volume() - a.Volume()) < 0.5);
    }

    [Fact]
    public void DisjointCubes_UnionEqualsSum()
    {
        var a = Primitives.Cube(new Vec3(-5, 0, 0), 2.0);
        var b = Primitives.Cube(new Vec3(5, 0, 0), 2.0);
        var r = Csg.Union(a, b);
        var vol = new Solid(r.Mesh).Volume();
        Assert.True(System.Math.Abs(vol - a.Volume() - b.Volume()) < 0.5);
    }

    [Fact]
    public void DisjointCylinders_IntersectionEmpty()
    {
        var a = Primitives.Cylinder(new Vec3(-5, 0, 0), Vec3.UnitZ, 0.5, 2.0, 12);
        var b = Primitives.Cylinder(new Vec3(5, 0, 0), Vec3.UnitZ, 0.5, 2.0, 12);
        var r = Csg.Intersect(a, b);
        Assert.True(r.FaceCount == 0 || new Solid(r.Mesh).Volume() < 0.01);
    }

    [Fact]
    public void DisjointCones_IntersectionEmpty()
    {
        var a = Primitives.Cone(new Vec3(-5, 0, 0), Vec3.UnitZ, 1.0, 2.0, 12);
        var b = Primitives.Cone(new Vec3(5, 0, 0), Vec3.UnitZ, 1.0, 2.0, 12);
        var r = Csg.Intersect(a, b);
        Assert.True(r.FaceCount == 0 || new Solid(r.Mesh).Volume() < 0.01);
    }

    [Fact]
    public void DisjointAlongY_IntersectionEmpty()
    {
        var a = Primitives.Cube(new Vec3(0, -5, 0), 2.0);
        var b = Primitives.Cube(new Vec3(0, 5, 0), 2.0);
        var r = Csg.Intersect(a, b);
        Assert.True(r.FaceCount == 0 || new Solid(r.Mesh).Volume() < 0.01);
    }

    [Fact]
    public void DisjointAlongZ_IntersectionEmpty()
    {
        var a = Primitives.Cube(new Vec3(0, 0, -5), 2.0);
        var b = Primitives.Cube(new Vec3(0, 0, 5), 2.0);
        var r = Csg.Intersect(a, b);
        Assert.True(r.FaceCount == 0 || new Solid(r.Mesh).Volume() < 0.01);
    }

    [Fact]
    public void DisjointSpheres_UnionHasTwoComponents()
    {
        var a = Primitives.Sphere(new Vec3(-5, 0, 0), 1.0, 1);
        var b = Primitives.Sphere(new Vec3(5, 0, 0), 1.0, 1);
        var r = new Solid(Csg.Union(a, b).Mesh);
        Assert.True(r.IsInside(new Vec3(-5, 0, 0)));
        Assert.True(r.IsInside(new Vec3(5, 0, 0)));
        Assert.False(r.IsInside(Vec3.Zero));
    }

    [Fact]
    public void WidelySeparated_IntersectionEmpty()
    {
        var a = Primitives.Cube(new Vec3(-100, 0, 0), 2.0);
        var b = Primitives.Cube(new Vec3(100, 0, 0), 2.0);
        var r = Csg.Intersect(a, b);
        Assert.True(r.FaceCount == 0 || new Solid(r.Mesh).Volume() < 0.01);
    }

    [Fact]
    public void DisjointDiagonal_IntersectionEmpty()
    {
        var a = Primitives.Cube(new Vec3(-5, -5, -5), 2.0);
        var b = Primitives.Cube(new Vec3(5, 5, 5), 2.0);
        var r = Csg.Intersect(a, b);
        Assert.True(r.FaceCount == 0 || new Solid(r.Mesh).Volume() < 0.01);
    }

    [Fact]
    public void JustBarelySeparated_IntersectionEmpty()
    {
        // Cubes of extent 2 (side = 2, so half = 1). Centers at -1.1 and 1.1 → gap of 0.2
        var a = Primitives.Cube(new Vec3(-1.1, 0, 0), 2.0);
        var b = Primitives.Cube(new Vec3(1.1, 0, 0), 2.0);
        var r = Csg.Intersect(a, b);
        Assert.True(r.FaceCount == 0 || new Solid(r.Mesh).Volume() < 0.01);
    }

    [Fact]
    public void DisjointSphereAndCylinder_IntersectionEmpty()
    {
        var a = Primitives.Sphere(new Vec3(-5, 0, 0), 1.0, 2);
        var b = Primitives.Cylinder(new Vec3(5, 0, 0), Vec3.UnitZ, 0.5, 2.0, 12);
        var r = Csg.Intersect(a, b);
        Assert.True(r.FaceCount == 0 || new Solid(r.Mesh).Volume() < 0.01);
    }

    [Fact]
    public void Disjoint_DifferencePreservesMinuend()
    {
        var a = Primitives.Sphere(new Vec3(-5, 0, 0), 1.0, 2);
        var b = Primitives.Sphere(new Vec3(5, 0, 0), 1.0, 2);
        var r = Csg.Difference(a, b);
        Assert.True(System.Math.Abs(new Solid(r.Mesh).Volume() - a.Volume()) < 0.5);
    }

    [Fact]
    public void DisjointBoxes_IntersectionEmpty()
    {
        var a = Primitives.Box(new Vec3(-5, 0, 0), new Vec3(2, 3, 1));
        var b = Primitives.Box(new Vec3(5, 0, 0), new Vec3(1, 2, 3));
        var r = Csg.Intersect(a, b);
        Assert.True(r.FaceCount == 0 || new Solid(r.Mesh).Volume() < 0.01);
    }

    [Fact]
    public void DisjointTori_IntersectionEmpty()
    {
        var a = Primitives.Torus(new Vec3(-5, 0, 0), Vec3.UnitZ, 1.0, 0.3, 12, 6);
        var b = Primitives.Torus(new Vec3(5, 0, 0), Vec3.UnitZ, 1.0, 0.3, 12, 6);
        var r = Csg.Intersect(a, b);
        Assert.True(r.FaceCount == 0 || new Solid(r.Mesh).Volume() < 0.01);
    }

    [Fact]
    public void SmallSphereInsideLargeSphere_IntersectionNotEmpty()
    {
        var a = Primitives.Sphere(Vec3.Zero, 2.0, 2);
        var b = Primitives.Sphere(Vec3.Zero, 0.5, 2);
        var r = Csg.Intersect(a, b);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void ContainedCubes_IntersectionNotEmpty()
    {
        var a = Primitives.Cube(Vec3.Zero, 4.0);
        var b = Primitives.Cube(Vec3.Zero, 2.0);
        var r = Csg.Intersect(a, b);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }
}
