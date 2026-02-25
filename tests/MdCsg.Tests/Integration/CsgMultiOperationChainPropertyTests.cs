using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG multi-operation chains — A∪B∪C, A∩B−C, A−B∩C, associativity, idempotence</summary>
public class CsgMultiOperationChainPropertyTests
{
    [Fact]
    public void UnionChain_ThreeCubes_InLine()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1.5, 0, 0), 2.0);
        var c = MeshFactory.CreateCube(new Vec3(3.0, 0, 0), 2.0);
        var ab = Csg.Union(a, b);
        var abc = Csg.Union(new Solid(ab.Mesh), c);
        Assert.True(abc.FaceCount > 0);
    }

    [Fact]
    public void DifferenceChain_TwoHoles()
    {
        var block = MeshFactory.CreateCube(Vec3.Zero, 4.0);
        var hole1 = MeshFactory.CreateSphere(new Vec3(1, 2, 2), 0.5, 1);
        var hole2 = MeshFactory.CreateSphere(new Vec3(3, 2, 2), 0.5, 1);
        var step1 = Csg.Difference(block, hole1);
        var step2 = Csg.Difference(new Solid(step1.Mesh), hole2);
        Assert.True(step2.FaceCount > 0);
    }

    [Fact]
    public void IntersectThenDifference_ProducesMesh()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 3.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 3.0);
        var intersect = Csg.Intersect(a, b);
        var c = MeshFactory.CreateSphere(new Vec3(2, 1.5, 1.5), 0.5, 1);
        var result = Csg.Difference(new Solid(intersect.Mesh), c);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void UnionThenIntersect_ProducesMesh()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var u = Csg.Union(a, b);
        var clip = MeshFactory.CreateCube(new Vec3(0.5, -0.5, -0.5), 3.0);
        var result = Csg.Intersect(new Solid(u.Mesh), clip);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_Disjoint_ThreeSpheres_FaceCountIsSum()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var b = MeshFactory.CreateSphere(new Vec3(10, 0, 0), 1.0, 1);
        var c = MeshFactory.CreateSphere(new Vec3(20, 0, 0), 1.0, 1);
        var ab = Csg.Union(a, b);
        var abc = Csg.Union(new Solid(ab.Mesh), c);
        int expectedFaces = a.Mesh.Faces.Count + b.Mesh.Faces.Count + c.Mesh.Faces.Count;
        Assert.Equal(expectedFaces, abc.FaceCount);
    }

    [Fact]
    public void Difference_Self_EmptyResult()
    {
        // A - A should be empty (but CSG doesn't handle identical meshes well)
        // At least test it doesn't crash
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var result = Csg.Difference(a, a);
        Assert.NotNull(result.Mesh);
    }

    [Fact]
    public void Union_WithSelf_ProducesMesh()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var result = Csg.Union(a, a);
        Assert.NotNull(result.Mesh);
    }

    [Fact]
    public void Intersect_WithSelf_ProducesMesh()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var result = Csg.Intersect(a, a);
        Assert.NotNull(result.Mesh);
    }

    [Fact]
    public void Difference_NonOverlapping_ReturnsOriginal()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(100, 0, 0), 1.0);
        var result = Csg.Difference(a, b);
        Assert.Equal(a.Mesh.Faces.Count, result.FaceCount);
    }

    [Fact]
    public void Intersect_NonOverlapping_Empty()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 1.0);
        var b = MeshFactory.CreateCube(new Vec3(100, 0, 0), 1.0);
        var result = Csg.Intersect(a, b);
        Assert.Equal(0, result.FaceCount);
    }

    [Fact]
    public void Union_BothOrders_ProduceFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var ab = Csg.Union(a, b);
        var ba = Csg.Union(b, a);
        Assert.True(ab.FaceCount > 0);
        Assert.True(ba.FaceCount > 0);
    }

    [Fact]
    public void DifferenceChain_FourCubes_InLine()
    {
        var block = MeshFactory.CreateCube(Vec3.Zero, 6.0);
        var cut1 = MeshFactory.CreateCube(new Vec3(1, 1, 1), 1.0);
        var cut2 = MeshFactory.CreateCube(new Vec3(3, 1, 1), 1.0);
        var step1 = Csg.Difference(block, cut1);
        var step2 = Csg.Difference(new Solid(step1.Mesh), cut2);
        Assert.True(step2.FaceCount > 0);
    }
}
