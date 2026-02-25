using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG edge cases — empty inputs, very small solids, large offset, multi-operation chains</summary>
public class CsgEdgeCaseExtendedPropertyTests
{
    [Fact]
    public void Union_EmptyWithCube_PreservesCube()
    {
        var empty = Solid.FromTriangles(Array.Empty<Triangle3>());
        var cube = MeshFactory.CreateCube();
        var result = Csg.Union(cube, empty);
        Assert.Equal(cube.Mesh.Faces.Count, result.FaceCount);
    }

    [Fact]
    public void Union_CubeWithEmpty_PreservesCube()
    {
        var empty = Solid.FromTriangles(Array.Empty<Triangle3>());
        var cube = MeshFactory.CreateCube();
        var result = Csg.Union(empty, cube);
        Assert.Equal(cube.Mesh.Faces.Count, result.FaceCount);
    }

    [Fact]
    public void Intersect_EmptyWithCube_Empty()
    {
        var empty = Solid.FromTriangles(Array.Empty<Triangle3>());
        var cube = MeshFactory.CreateCube();
        var result = Csg.Intersect(cube, empty);
        Assert.Equal(0, result.FaceCount);
    }

    [Fact]
    public void Difference_CubeMinusEmpty_PreservesCube()
    {
        var empty = Solid.FromTriangles(Array.Empty<Triangle3>());
        var cube = MeshFactory.CreateCube();
        var result = Csg.Difference(cube, empty);
        Assert.Equal(cube.Mesh.Faces.Count, result.FaceCount);
    }

    [Fact]
    public void Difference_EmptyMinusCube_Empty()
    {
        var empty = Solid.FromTriangles(Array.Empty<Triangle3>());
        var cube = MeshFactory.CreateCube();
        var result = Csg.Difference(empty, cube);
        Assert.Equal(0, result.FaceCount);
    }

    [Fact]
    public void Union_TwoEmpty_Empty()
    {
        var a = Solid.FromTriangles(Array.Empty<Triangle3>());
        var b = Solid.FromTriangles(Array.Empty<Triangle3>());
        var result = Csg.Union(a, b);
        Assert.Equal(0, result.FaceCount);
    }

    [Fact]
    public void Union_LargeOffset_DisjointFaceSum()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 1.0);
        var b = MeshFactory.CreateCube(new Vec3(1000, 1000, 1000), 1.0);
        var result = Csg.Union(a, b);
        Assert.Equal(a.Mesh.Faces.Count + b.Mesh.Faces.Count, result.FaceCount);
    }

    [Fact]
    public void Chain_UnionThenDifference_ProducesFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var c = MeshFactory.CreateCube(new Vec3(0, 1, 0), 1.5);
        var unionResult = Csg.Union(a, b);
        var unionSolid = Solid.FromTriangles(
            unionResult.Mesh.Faces.Select(f =>
            {
                f.GetTrianglePositions(out var p1, out var p2, out var p3);
                return new Triangle3(p1, p2, p3);
            }).ToArray());
        var result = Csg.Difference(unionSolid, c);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Chain_IntersectThenUnion_ProducesFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var c = MeshFactory.CreateCube(new Vec3(0, 0, 1), 1.0);
        var intersectResult = Csg.Intersect(a, b);
        var intersectSolid = Solid.FromTriangles(
            intersectResult.Mesh.Faces.Select(f =>
            {
                f.GetTrianglePositions(out var p1, out var p2, out var p3);
                return new Triangle3(p1, p2, p3);
            }).ToArray());
        var result = Csg.Union(intersectSolid, c);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_SmallSolids_ProducesFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 0.01);
        var b = MeshFactory.CreateCube(new Vec3(0.005, 0, 0), 0.01);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersect_SmallSolids_ProducesFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 0.01);
        var b = MeshFactory.CreateCube(new Vec3(0.005, 0, 0), 0.01);
        var result = Csg.Intersect(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_CubeWithCube_SamePosition_DifferentSize()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 3.0);
        var b = MeshFactory.CreateCube(Vec3.Zero, 1.0); // b contained in a
        var result = Csg.Union(a, b);
        // Union of contained should be approximately a
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersect_CubeContainsCube_ApproximatesSmaller()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 4.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 1, 1), 1.0);
        var result = Csg.Intersect(a, b);
        // Intersection should approximate the smaller cube
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Difference_LargeMinusSmallContained_ProducesFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 4.0);
        var b = MeshFactory.CreateSphere(new Vec3(2, 2, 2), 0.5, 1);
        var result = Csg.Difference(a, b);
        Assert.True(result.FaceCount > a.Mesh.Faces.Count,
            "Difference with contained sphere should produce more faces than original cube");
    }
}
