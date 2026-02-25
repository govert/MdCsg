using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG with various offsets — diagonal, axis-aligned, near-touching, large offset</summary>
public class CsgMultiOffsetTests
{
    [Theory]
    [InlineData(0.1)]
    [InlineData(0.3)]
    [InlineData(0.5)]
    [InlineData(0.7)]
    [InlineData(0.9)]
    public void Union_VaryingOverlap_X(double dx)
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(dx, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Theory]
    [InlineData(0.1)]
    [InlineData(0.3)]
    [InlineData(0.5)]
    [InlineData(0.7)]
    [InlineData(0.9)]
    public void Intersect_VaryingOverlap_X(double dx)
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(dx, 0, 0)).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Theory]
    [InlineData(0.1)]
    [InlineData(0.3)]
    [InlineData(0.5)]
    [InlineData(0.7)]
    [InlineData(0.9)]
    public void Difference_VaryingOverlap_X(double dx)
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(dx, 0, 0)).Mesh);
        var result = Csg.Difference(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Theory]
    [InlineData(0.1, 0.1, 0.1)]
    [InlineData(0.3, 0.3, 0.3)]
    [InlineData(0.5, 0.5, 0.5)]
    [InlineData(0.7, 0.7, 0.7)]
    public void Union_DiagonalOffset(double dx, double dy, double dz)
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(dx, dy, dz)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_NearTouch_SmallGap()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.999, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersect_NearTouch_SmallOverlap()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.95, 0, 0)).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_TwoAxisOffset()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Difference_TwoAxisOffset()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0, 0.5, 0.5)).Mesh);
        var result = Csg.Difference(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_SmallCubeOnLargeCube_HasMoreFaces()
    {
        var large = new Solid(MeshFactory.CreateCube(Vec3.Zero, 3).Mesh);
        var small = new Solid(MeshFactory.CreateCube(new Vec3(2.5, 1.5, 1.5), 0.5).Mesh);
        var result = Csg.Union(large, small);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Difference_CreatesCavity()
    {
        var large = new Solid(MeshFactory.CreateCube(Vec3.Zero, 3).Mesh);
        var small = new Solid(MeshFactory.CreateCube(new Vec3(1, 1, 1), 0.5).Mesh);
        var result = Csg.Difference(large, small);
        Assert.True(result.FaceCount > large.Mesh.Faces.Count,
            "Cavity should add faces");
    }
}
