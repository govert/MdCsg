using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: Numerical stability edge cases — very small/large/offset meshes, near-touching</summary>
public class CsgNumericalStabilityTests
{
    [Fact]
    public void SmallCubes_Union()
    {
        var a = new Solid(MeshFactory.CreateCube(Vec3.Zero, 0.001).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.0005, 0.0005, 0.0005), 0.001).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void LargeCubes_Union()
    {
        var a = new Solid(MeshFactory.CreateCube(Vec3.Zero, 1000).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(500, 500, 500), 1000).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void FarFromOrigin_Union()
    {
        var offset = new Vec3(1e6, 1e6, 1e6);
        var a = new Solid(MeshFactory.CreateCube(offset).Mesh);
        var b = new Solid(MeshFactory.CreateCube(offset + new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void NearlyTouching_TinyOverlap_Intersect()
    {
        // Cubes with very small overlap
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.99, 0, 0)).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void NegativeCoordinates_Union()
    {
        var a = new Solid(MeshFactory.CreateCube(new Vec3(-2, -2, -2)).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(-1.5, -1.5, -1.5)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void MixedPositiveNegative_Difference()
    {
        var a = new Solid(MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5)).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0, 0, 0)).Mesh);
        var result = Csg.Difference(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Axis_Aligned_Partial_Overlap_X()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        Assert.True(Csg.Union(a, b).FaceCount > 0);
        Assert.True(Csg.Intersect(a, b).FaceCount > 0);
        Assert.True(Csg.Difference(a, b).FaceCount > 0);
    }

    [Fact]
    public void Axis_Aligned_Partial_Overlap_Y()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0, 0.5, 0)).Mesh);
        Assert.True(Csg.Union(a, b).FaceCount > 0);
        Assert.True(Csg.Intersect(a, b).FaceCount > 0);
        Assert.True(Csg.Difference(a, b).FaceCount > 0);
    }

    [Fact]
    public void Axis_Aligned_Partial_Overlap_Z()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0, 0, 0.5)).Mesh);
        Assert.True(Csg.Union(a, b).FaceCount > 0);
        Assert.True(Csg.Intersect(a, b).FaceCount > 0);
        Assert.True(Csg.Difference(a, b).FaceCount > 0);
    }

    [Fact]
    public void SmallSphere_InsideLargeCube_Difference()
    {
        var cube = new Solid(MeshFactory.CreateCube(Vec3.Zero, 2).Mesh);
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(1, 1, 1), 0.3, 2).Mesh);
        var result = Csg.Difference(cube, sphere);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void DifferentSizes_Union()
    {
        var big = new Solid(MeshFactory.CreateCube(Vec3.Zero, 5).Mesh);
        var small = new Solid(MeshFactory.CreateCube(new Vec3(2, 2, 2), 0.5).Mesh);
        var result = Csg.Union(big, small);
        Assert.True(result.FaceCount > 0);
    }
}
