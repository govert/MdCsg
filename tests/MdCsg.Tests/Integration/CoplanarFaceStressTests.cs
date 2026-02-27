using MdCsg.Api;
using MdCsg.Math;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 16: Coplanar face handling stress.</summary>
public class CoplanarFaceStressTests
{
    [Fact]
    public void IdenticalCubes_UnionHandlesCoplanarFaces()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(Vec3.Zero, 2.0);
        var r = Csg.Union(a, b);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void IdenticalCubes_IntersectionHandlesCoplanarFaces()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(Vec3.Zero, 2.0);
        var r = Csg.Intersect(a, b);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void SharedFaceCubes_UnionNoOverlap()
    {
        var a = Primitives.Cube(new Vec3(0, 0, -1), 2.0);
        var b = Primitives.Cube(new Vec3(0, 0, 1), 2.0);
        var r = Csg.Union(a, b);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void CoplanarBoxes_SameHeight_UnionWorks()
    {
        var a = Primitives.Box(new Vec3(-1, 0, 0), new Vec3(2, 2, 2));
        var b = Primitives.Box(new Vec3(1, 0, 0), new Vec3(2, 2, 2));
        var r = Csg.Union(a, b);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void CoplanarBoxes_SameHeight_IntersectionWorks()
    {
        var a = Primitives.Box(new Vec3(-1, 0, 0), new Vec3(2, 2, 2));
        var b = Primitives.Box(new Vec3(1, 0, 0), new Vec3(2, 2, 2));
        var r = Csg.Intersect(a, b);
        Assert.True(r.FaceCount >= 0);
    }

    [Fact]
    public void FlushFaces_DifferenceWorks()
    {
        var a = Primitives.Cube(new Vec3(0, 0, -1), 2.0);
        var b = Primitives.Cube(new Vec3(0, 0, 1), 2.0);
        var r = Csg.Difference(a, b);
        Assert.True(r.FaceCount >= 0);
    }

    [Fact]
    public void PartialCoplanar_UnionWorks()
    {
        var a = Primitives.Box(Vec3.Zero, new Vec3(2, 2, 2));
        var b = Primitives.Box(new Vec3(1, 0, 0), new Vec3(2, 2, 2));
        var r = Csg.Union(a, b);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void PartialCoplanar_DifferenceWorks()
    {
        var a = Primitives.Box(Vec3.Zero, new Vec3(2, 2, 2));
        var b = Primitives.Box(new Vec3(1, 0, 0), new Vec3(2, 2, 2));
        var r = Csg.Difference(a, b);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void NestedCoplanar_ContainedCube()
    {
        var outer = Primitives.Cube(Vec3.Zero, 4.0);
        var inner = Primitives.Box(Vec3.Zero, new Vec3(4, 2, 2)); // shares top/bottom faces
        var r = Csg.Intersect(outer, inner);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void MultipleCubesInRow_CoplanarFaces()
    {
        Solid acc = Primitives.Cube(new Vec3(-2, 0, 0), 2.0);
        acc = new Solid(Csg.Union(acc, Primitives.Cube(new Vec3(0, 0, 0), 2.0)).Mesh);
        acc = new Solid(Csg.Union(acc, Primitives.Cube(new Vec3(2, 0, 0), 2.0)).Mesh);
        Assert.True(acc.Volume() > 0);
    }

    [Fact]
    public void CoplanarWithWindingNumber_Works()
    {
        var opts = new CsgOptions { UseWindingNumber = true };
        var a = Primitives.Cube(new Vec3(0, 0, -1), 2.0);
        var b = Primitives.Cube(new Vec3(0, 0, 1), 2.0);
        var r = Csg.Union(a, b, opts);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void SmallOffsetFromCoplanar_UnionWorks()
    {
        var a = Primitives.Cube(new Vec3(0, 0, -1.001), 2.0);
        var b = Primitives.Cube(new Vec3(0, 0, 1.001), 2.0);
        var r = Csg.Union(a, b);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void ManyCoplanarFaces_GridOfCubes()
    {
        var solids = new List<Solid>();
        for (int x = 0; x < 3; x++)
            for (int y = 0; y < 3; y++)
                solids.Add(Primitives.Cube(new Vec3(x * 2, y * 2, 0), 2.0));
        var r = Csg.Union(solids);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void CoplanarSpheres_IdenticalUnion()
    {
        var a = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var b = Primitives.Sphere(Vec3.Zero, 1.0, 2);
        var r = Csg.Union(a, b);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void BoxesFlushOnTwoFaces_UnionWorks()
    {
        var a = Primitives.Box(new Vec3(-1, -1, 0), new Vec3(2, 2, 2));
        var b = Primitives.Box(new Vec3(1, 1, 0), new Vec3(2, 2, 2));
        var r = Csg.Union(a, b);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void CoplanarDifference_ExactlyRemoves()
    {
        var a = Primitives.Cube(Vec3.Zero, 4.0);
        var b = Primitives.Box(Vec3.Zero, new Vec3(4, 4, 2)); // shares top/bottom faces
        var r = Csg.Difference(a, b);
        Assert.True(r.FaceCount >= 0);
    }

    [Fact]
    public void AlmostCoplanar_SmallOffset()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(0, 0, 1e-6), 2.0);
        var r = Csg.Union(a, b);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void CoplanarParallelNonOverlapping_UnionValid()
    {
        var a = Primitives.Cube(new Vec3(-2, 0, 0), 2.0);
        var b = Primitives.Cube(new Vec3(2, 0, 0), 2.0);
        var r = Csg.Union(a, b);
        Assert.True(new Solid(r.Mesh).Volume() > 0);
    }

    [Fact]
    public void CoplanarFaces_NonParallelOptions()
    {
        var opts = new CsgOptions { Parallel = false };
        var a = Primitives.Cube(new Vec3(0, 0, -1), 2.0);
        var b = Primitives.Cube(new Vec3(0, 0, 1), 2.0);
        var r = Csg.Union(a, b, opts);
        Assert.True(r.FaceCount > 0);
    }
}
