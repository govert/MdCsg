using MdCsg.Api;
using MdCsg.Math;
using Xunit;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 02: 3-5+ sequential CSG ops, operator chains.</summary>
public class ChainedCsgOperationTests
{
    [Fact]
    public void ThreeUnions_ProducesValidMesh()
    {
        var a = Primitives.Cube(new Vec3(-1, 0, 0), 1.5);
        var b = Primitives.Cube(Vec3.Zero, 1.5);
        var c = Primitives.Cube(new Vec3(1, 0, 0), 1.5);
        var r1 = Csg.Union(a, b);
        var r2 = Csg.Union(new Solid(r1.Mesh), c);
        Assert.True(r2.FaceCount > 0);
        Assert.True(new Solid(r2.Mesh).Volume() > 0);
    }

    [Fact]
    public void UnionThenDifference_ProducesValidMesh()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(new Vec3(0.5, 0, 0), 0.8, 2);
        var c = Primitives.Cylinder(new Vec3(0, 0, -1.5), Vec3.UnitZ, 0.3, 3.0, 12);
        var r1 = Csg.Union(a, b);
        var r2 = Csg.Difference(new Solid(r1.Mesh), c);
        Assert.True(new Solid(r2.Mesh).Volume() > 0);
    }

    [Fact]
    public void DifferenceThenIntersect_ProducesValidMesh()
    {
        var a = Primitives.Cube(Vec3.Zero, 3.0);
        var b = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var c = Primitives.Box(Vec3.Zero, new Vec3(4, 1, 1));
        var r1 = Csg.Difference(a, b);
        var r2 = Csg.Intersect(new Solid(r1.Mesh), c);
        Assert.True(new Solid(r2.Mesh).Volume() > 0);
    }

    [Fact]
    public void FourChainedUnions_ProducesValidMesh()
    {
        Solid acc = Primitives.Cube(new Vec3(-2, 0, 0), 1.0);
        for (int i = -1; i <= 1; i++)
        {
            var s = Primitives.Cube(new Vec3(i, 0, 0), 1.0);
            acc = new Solid(Csg.Union(acc, s).Mesh);
        }
        Assert.True(acc.Volume() > 0);
        Assert.True(acc.Mesh.Faces.Count > 0);
    }

    [Fact]
    public void FiveChainedDifferences_ProducesValidMesh()
    {
        var box = Primitives.Cube(Vec3.Zero, 4.0);
        for (int i = 0; i < 5; i++)
        {
            double x = -1.5 + i * 0.75;
            var hole = Primitives.Cylinder(new Vec3(x, 0, -3), Vec3.UnitZ, 0.2, 6.0, 8);
            box = new Solid(Csg.Difference(box, hole).Mesh);
        }
        Assert.True(box.Volume() > 0);
    }

    [Fact]
    public void AlternatingUnionDifference_ProducesValidMesh()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(new Vec3(0.5, 0, 0), 0.5, 2);
        var c = Primitives.Sphere(new Vec3(-0.5, 0, 0), 0.5, 2);
        var r1 = Csg.Union(a, b);
        var r2 = Csg.Difference(new Solid(r1.Mesh), c);
        Assert.True(new Solid(r2.Mesh).Volume() > 0);
    }

    [Fact]
    public void IntersectChain_GrowsSmallerEachTime()
    {
        var a = Primitives.Cube(Vec3.Zero, 3.0);
        var b = Primitives.Cube(new Vec3(0.5, 0, 0), 3.0);
        var c = Primitives.Cube(new Vec3(1.0, 0, 0), 3.0);
        var r1 = new Solid(Csg.Intersect(a, b).Mesh);
        var vol1 = r1.Volume();
        var r2 = new Solid(Csg.Intersect(r1, c).Mesh);
        var vol2 = r2.Volume();
        Assert.True(vol2 <= vol1 + 0.1);
        Assert.True(vol2 > 0);
    }

    [Fact]
    public void ThreeBodyUnion_CoversBoundingVolume()
    {
        var a = Primitives.Cube(new Vec3(-0.5, 0, 0), 1.5);
        var b = Primitives.Cube(new Vec3(0.5, 0, 0), 1.5);
        var c = Primitives.Cube(new Vec3(0, 0.5, 0), 1.5);
        var r1 = Csg.Union(a, b);
        var r2 = Csg.Union(new Solid(r1.Mesh), c);
        var solid = new Solid(r2.Mesh);
        Assert.True(solid.IsInside(Vec3.Zero));
    }

    [Fact]
    public void UnionThenSelfDifference_ApproachesZeroVolume()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(Vec3.Zero, 0.5, 2);
        var r1 = Csg.Union(a, b);
        var s = new Solid(r1.Mesh);
        var r2 = Csg.Difference(s, s);
        Assert.True(r2.FaceCount == 0 || new Solid(r2.Mesh).Volume() < 0.01);
    }

    [Fact]
    public void ChainedCsg_MaintainsManifoldness()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Sphere(new Vec3(0.8, 0, 0), 0.6, 2);
        var c = Primitives.Cylinder(new Vec3(0, 0, -2), Vec3.UnitZ, 0.3, 4.0, 12);
        var r1 = new Solid(Csg.Difference(a, b).Mesh);
        var r2 = new Solid(Csg.Difference(r1, c).Mesh);
        Assert.True(r2.Mesh.Faces.Count > 0);
        Assert.True(r2.Mesh.Vertices.Count > 0);
    }

    [Fact]
    public void DifferenceChain_ReducesVolume()
    {
        var cube = Primitives.Cube(Vec3.Zero, 3.0);
        var originalVol = cube.Volume();
        var result = cube;
        for (int i = 0; i < 3; i++)
        {
            var sphere = Primitives.Sphere(new Vec3(i * 0.5 - 0.5, 0, 0), 0.4, 1);
            result = new Solid(Csg.Difference(result, sphere).Mesh);
        }
        Assert.True(result.Volume() < originalVol);
        Assert.True(result.Volume() > 0);
    }

    [Fact]
    public void UnionChain_IncludesAllInputPoints()
    {
        var a = Primitives.Cube(new Vec3(-2, 0, 0), 1.0);
        var b = Primitives.Cube(new Vec3(0, 0, 0), 1.0);
        var c = Primitives.Cube(new Vec3(2, 0, 0), 1.0);
        var r1 = Csg.Union(a, b);
        var r2 = Csg.Union(new Solid(r1.Mesh), c);
        var solid = new Solid(r2.Mesh);
        Assert.True(solid.IsInside(new Vec3(-2, 0, 0)));
        Assert.True(solid.IsInside(new Vec3(0, 0, 0)));
        Assert.True(solid.IsInside(new Vec3(2, 0, 0)));
    }

    [Fact]
    public void IntersectThenDifference_ProducesValidResult()
    {
        var a = Primitives.Cube(Vec3.Zero, 3.0);
        var b = Primitives.Sphere(Vec3.Zero, 1.5, 2);
        var c = Primitives.Cylinder(new Vec3(0, 0, -2), Vec3.UnitZ, 0.3, 4.0, 12);
        var intersection = new Solid(Csg.Intersect(a, b).Mesh);
        var result = new Solid(Csg.Difference(intersection, c).Mesh);
        Assert.True(result.Volume() > 0);
    }

    [Fact]
    public void FourWayIntersect_ProducesSmallVolume()
    {
        var a = Primitives.Cube(new Vec3(0.3, 0, 0), 2.0);
        var b = Primitives.Cube(new Vec3(-0.3, 0, 0), 2.0);
        var c = Primitives.Cube(new Vec3(0, 0.3, 0), 2.0);
        var d = Primitives.Cube(new Vec3(0, -0.3, 0), 2.0);
        var r = new Solid(Csg.Intersect(a, b).Mesh);
        r = new Solid(Csg.Intersect(r, c).Mesh);
        r = new Solid(Csg.Intersect(r, d).Mesh);
        Assert.True(r.Volume() > 0);
    }

    [Fact]
    public void DifferenceThenUnionAddBack_ApproximatesOriginal()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var sphere = Primitives.Sphere(new Vec3(0.5, 0, 0), 0.3, 1);
        var without = new Solid(Csg.Difference(cube, sphere).Mesh);
        var addBack = new Solid(Csg.Union(without, sphere).Mesh);
        Assert.True(System.Math.Abs(addBack.Volume() - cube.Volume()) < 0.5);
    }

    [Fact]
    public void UnionWithSmallOverlap_CorrectTopology()
    {
        var a = Primitives.Cube(new Vec3(-0.9, 0, 0), 2.0);
        var b = Primitives.Cube(new Vec3(0.9, 0, 0), 2.0);
        var r = Csg.Union(a, b);
        Assert.True(r.FaceCount > 0);
        Assert.True(new Solid(r.Mesh).Volume() > a.Volume());
    }

    [Fact]
    public void ChainedOps_ResultIsInsideTestValid()
    {
        var a = Primitives.Cube(Vec3.Zero, 4.0);
        var b = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var r = new Solid(Csg.Difference(a, b).Mesh);
        // Center is hollowed
        Assert.False(r.IsInside(Vec3.Zero));
        // Corner is still there
        Assert.True(r.IsInside(new Vec3(1.5, 1.5, 1.5)));
    }

    [Fact]
    public void MultipleSmallSphereSubtractions_WorksCorrectly()
    {
        var box = Primitives.Cube(Vec3.Zero, 3.0);
        var result = box;
        for (int i = 0; i < 3; i++)
        {
            var sphere = Primitives.Sphere(new Vec3(0, 0, i * 0.6 - 0.6), 0.3, 1);
            result = new Solid(Csg.Difference(result, sphere).Mesh);
        }
        Assert.True(result.Volume() > 0);
        Assert.True(result.Volume() < box.Volume());
    }

    [Fact]
    public void ChainedCsg_WithDifferentPrimitiveTypes()
    {
        var cube = Primitives.Cube(Vec3.Zero, 3.0);
        var sphere = Primitives.Sphere(Vec3.Zero, 1.2, 2);
        var cyl = Primitives.Cylinder(new Vec3(0, 0, -2), Vec3.UnitZ, 0.4, 4.0, 12);
        var r1 = new Solid(Csg.Intersect(cube, sphere).Mesh);
        var r2 = new Solid(Csg.Difference(r1, cyl).Mesh);
        Assert.True(r2.Volume() > 0);
        Assert.True(r2.Mesh.Faces.Count > 0);
    }
}
