using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG boundary and edge case tests — touching, contained, small/large, various offsets</summary>
public class CsgBoundaryTests
{
    [Fact]
    public void Union_TouchingFaces_Produces()
    {
        // Two cubes touching at x=1 face
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(1, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_SmallCubeInsideLarge_Produces()
    {
        var large = new Solid(MeshFactory.CreateCube(size: 2).Mesh);
        var small = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), size: 0.3).Mesh);
        var result = Csg.Union(large, small);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersect_SmallInsideLarge_Produces()
    {
        var large = new Solid(MeshFactory.CreateCube(size: 2).Mesh);
        var small = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), size: 0.3).Mesh);
        var result = Csg.Intersect(large, small);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Difference_SmallFromLarge_Produces()
    {
        var large = new Solid(MeshFactory.CreateCube(size: 2).Mesh);
        var small = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), size: 0.3).Mesh);
        var result = Csg.Difference(large, small);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_MultipleOffsets_AllProduce()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        foreach (double offset in new[] { 0.1, 0.3, 0.5, 0.7, 0.9 })
        {
            var b = new Solid(MeshFactory.CreateCube(new Vec3(offset, 0, 0)).Mesh);
            var result = Csg.Union(a, b);
            Assert.True(result.FaceCount > 0, $"Union failed at offset {offset}");
        }
    }

    [Fact]
    public void Intersect_MultipleOffsets_AllProduce()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        foreach (double offset in new[] { 0.1, 0.3, 0.5, 0.7, 0.9 })
        {
            var b = new Solid(MeshFactory.CreateCube(new Vec3(offset, 0, 0)).Mesh);
            var result = Csg.Intersect(a, b);
            Assert.True(result.FaceCount > 0, $"Intersect failed at offset {offset}");
        }
    }

    [Fact]
    public void Difference_MultipleOffsets_AllProduce()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        foreach (double offset in new[] { 0.1, 0.3, 0.5, 0.7, 0.9 })
        {
            var b = new Solid(MeshFactory.CreateCube(new Vec3(offset, 0, 0)).Mesh);
            var result = Csg.Difference(a, b);
            Assert.True(result.FaceCount > 0, $"Difference failed at offset {offset}");
        }
    }

    [Fact]
    public void Union_DiagonalOffset_Produces()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersect_DiagonalOffset_Produces()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_TwoSpheres_Produces()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1, 2).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Difference_CubeMinusCube_SmallOffset_Produces()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.1, 0.1, 0.1)).Mesh);
        var result = Csg.Difference(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_TetrahedronSphere_Produces()
    {
        var a = new Solid(MeshFactory.CreateTetrahedron(Vec3.Zero, 0.5).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 0.4, 2).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }
}
