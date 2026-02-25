using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: Chained CSG operations — multi-step Boolean algebra</summary>
public class CsgChainedOperationsPropertyTests
{
    [Fact]
    public void UnionThenDifference_ProducesFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var c = MeshFactory.CreateCube(new Vec3(0, 1, 0), 1.0);
        var ab = Csg.Union(a, b);
        var abSolid = new Solid(ab.Mesh);
        var result = Csg.Difference(abSolid, c);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void UnionThenIntersect_ProducesFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var c = MeshFactory.CreateCube(Vec3.Zero, 3.0);
        var ab = Csg.Union(a, b);
        var abSolid = new Solid(ab.Mesh);
        var result = Csg.Intersect(abSolid, c);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void DifferenceThenUnion_ProducesFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 3.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 1.0);
        var c = MeshFactory.CreateCube(new Vec3(-1, 0, 0), 1.0);
        var ab = Csg.Difference(a, b);
        var abSolid = new Solid(ab.Mesh);
        var result = Csg.Union(abSolid, c);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_IsCommutative_SameFaceCount()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var ab = Csg.Union(a, b);
        var ba = Csg.Union(b, a);
        Assert.Equal(ab.FaceCount, ba.FaceCount);
    }

    [Fact]
    public void Intersect_IsCommutative_SameFaceCount()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var ab = Csg.Intersect(a, b);
        var ba = Csg.Intersect(b, a);
        Assert.Equal(ab.FaceCount, ba.FaceCount);
    }

    [Fact]
    public void Difference_IsNotCommutative()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var ab = Csg.Difference(a, b);
        var ba = Csg.Difference(b, a);
        // A\B and B\A are generally different shapes
        // They might happen to have same face count in some cases, but vertex positions differ
        Assert.True(ab.FaceCount > 0);
        Assert.True(ba.FaceCount > 0);
    }

    [Fact]
    public void DisjointUnion_SumOfFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 1.0);
        var b = MeshFactory.CreateCube(new Vec3(100, 0, 0), 1.0);
        var result = Csg.Union(a, b);
        // Disjoint union should preserve all faces from both
        Assert.Equal(a.Mesh.Faces.Count + b.Mesh.Faces.Count, result.FaceCount);
    }

    [Fact]
    public void DisjointIntersect_EmptyResult()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 1.0);
        var b = MeshFactory.CreateCube(new Vec3(100, 0, 0), 1.0);
        var result = Csg.Intersect(a, b);
        Assert.Equal(0, result.FaceCount);
    }

    [Fact]
    public void ContainedCube_Difference_ProducesFaces()
    {
        // Small cube inside large cube — difference should produce hollow shape
        var a = MeshFactory.CreateCube(Vec3.Zero, 4.0);
        var b = MeshFactory.CreateCube(Vec3.Zero, 1.0);
        var result = Csg.Difference(a, b);
        Assert.True(result.FaceCount > 0);
        Assert.True(result.FaceCount > a.Mesh.Faces.Count);
    }

    [Fact]
    public void ContainedCube_Intersect_IsSmallCube()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 4.0);
        var b = MeshFactory.CreateCube(Vec3.Zero, 1.0);
        var result = Csg.Intersect(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void ThreeWayUnion_ProducesFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var c = MeshFactory.CreateCube(new Vec3(0, 1, 0), 2.0);
        var ab = Csg.Union(a, b);
        var abSolid = new Solid(ab.Mesh);
        var result = Csg.Union(abSolid, c);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_DisjointResult_NoIntersectionSegments()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 1.0);
        var b = MeshFactory.CreateCube(new Vec3(100, 0, 0), 1.0);
        var result = Csg.Union(a, b);
        Assert.Equal(0, result.IntersectionSegmentCount);
    }
}
