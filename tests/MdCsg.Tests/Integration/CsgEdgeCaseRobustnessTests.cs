using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG edge case robustness — touching edges, shared vertices, thin slices</summary>
public class CsgEdgeCaseRobustnessTests
{
    [Fact]
    public void SharedVertex_Union_NoCrash()
    {
        // Two cubes touching at a single corner
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(1, 1, 1)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void SharedEdge_Union_NoCrash()
    {
        // Two cubes sharing an edge
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(1, 1, 0)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void ThinSlice_Difference_NoCrash()
    {
        // Subtract a thin slice from a cube
        var cube = new Solid(MeshFactory.CreateCube(Vec3.Zero, 2).Mesh);
        var slice = new Solid(MeshFactory.CreateCube(new Vec3(0.9, -0.1, -0.1), 0.2).Mesh);
        var result = Csg.Difference(cube, slice);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void SmallCube_LargeOffset_Union_Disjoint()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(100, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(System.Math.Abs(vol - 2.0) < 0.01);
    }

    [Fact]
    public void BarelyCrossing_Intersection_NoCrash()
    {
        // Cubes overlapping by just 0.01 in one axis
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.99, 0, 0)).Mesh);
        var result = Csg.Intersect(a, b);
        if (result.FaceCount > 0)
        {
            double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
            Assert.True(vol >= 0);
        }
    }

    [Fact]
    public void LargeOverlap_Difference_LargeResult()
    {
        // Large overlap: B covers most of A, difference should be small
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.1, 0.1, 0.1)).Mesh);
        var result = Csg.Difference(a, b);
        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volDiff = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(volDiff < volA);
    }

    [Fact]
    public void TetrahedronTetrahedron_AllOps_NoCrash()
    {
        var a = new Solid(MeshFactory.CreateTetrahedron().Mesh);
        var b = new Solid(MeshFactory.CreateTetrahedron(new Vec3(0.3, 0.3, 0.3)).Mesh);
        Assert.True(Csg.Union(a, b).FaceCount > 0);
        var intersect = Csg.Intersect(a, b);
        if (intersect.FaceCount > 0)
            Assert.True(VolumeCalculator.ComputeAbsoluteVolume(intersect.Mesh) >= 0);
        Assert.True(Csg.Difference(a, b).FaceCount > 0);
    }

    [Fact]
    public void SphereCube_AllOps_NoCrash()
    {
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 2).Mesh);
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        Assert.True(Csg.Union(sphere, cube).FaceCount > 0);
        Assert.True(Csg.Intersect(sphere, cube).FaceCount > 0);
        Assert.True(Csg.Difference(sphere, cube).FaceCount > 0);
        Assert.True(Csg.Difference(cube, sphere).FaceCount > 0);
    }

    [Fact]
    public void MultipleSmallSubtractions_NoCrash()
    {
        var cube = new Solid(MeshFactory.CreateCube(Vec3.Zero, 2).Mesh);
        var small1 = new Solid(MeshFactory.CreateCube(new Vec3(0.2, 0.2, 0.2), 0.3).Mesh);
        var result1 = Csg.Difference(cube, small1);
        Assert.True(result1.FaceCount > 0);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result1.Mesh);
        Assert.True(vol > 7); // 8 - 0.027 ≈ 7.97
    }

    [Fact]
    public void OffAxisCubes_Union_NoCrash()
    {
        // Cubes offset diagonally
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.7, 0.1)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol > 1.0);
    }
}
