using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: Combinatorial tests — all mesh pair combinations x all CSG operations</summary>
public class MeshPairCombinationTests
{
    [Theory]
    [InlineData(0.2, 0.2, 0.2)]
    [InlineData(0.5, 0.5, 0.5)]
    [InlineData(0.8, 0.8, 0.8)]
    public void CubeCube_Union_VariousOffsets(double x, double y, double z)
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(x, y, z)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol > 0.5);
    }

    [Theory]
    [InlineData(0.2, 0.2, 0.2)]
    [InlineData(0.5, 0.5, 0.5)]
    [InlineData(0.8, 0.8, 0.8)]
    public void CubeCube_Intersection_VariousOffsets(double x, double y, double z)
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(x, y, z)).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.True(result.FaceCount > 0);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol > 0);
    }

    [Theory]
    [InlineData(0.2, 0.2, 0.2)]
    [InlineData(0.5, 0.5, 0.5)]
    [InlineData(0.8, 0.8, 0.8)]
    public void CubeCube_Difference_VariousOffsets(double x, double y, double z)
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(x, y, z)).Mesh);
        var result = Csg.Difference(a, b);
        Assert.True(result.FaceCount > 0);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol > 0);
    }

    [Fact]
    public void CubeSphere_Union_Positive()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.6, 2).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
        Assert.True(VolumeCalculator.ComputeAbsoluteVolume(result.Mesh) > 0.5);
    }

    [Fact]
    public void CubeSphere_Intersection_Positive()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.6, 2).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.True(result.FaceCount > 0);
        Assert.True(VolumeCalculator.ComputeAbsoluteVolume(result.Mesh) > 0);
    }

    [Fact]
    public void CubeSphere_Difference_Positive()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.3, 2).Mesh);
        var result = Csg.Difference(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void SphereSphere_Union_Overlapping()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1, 2).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void SphereSphere_Intersection_Overlapping()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0, 0), 1, 2).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void TetrahedronCube_Union()
    {
        var a = new Solid(MeshFactory.CreateTetrahedron(size: 0.5).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(-0.2, -0.2, -0.2)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void TetrahedronCube_Intersection()
    {
        var a = new Solid(MeshFactory.CreateTetrahedron(size: 0.5).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(-0.2, -0.2, -0.2)).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void SmallCube_InsideLarge_Difference()
    {
        var big = new Solid(MeshFactory.CreateCube(size: 2).Mesh);
        var small = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), size: 0.5).Mesh);
        var result = Csg.Difference(big, small);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        // 8 - 0.125 = 7.875
        Assert.True(vol > 7, $"Expected ~7.875, got {vol}");
    }

    [Fact]
    public void DifferentSizes_Union_Reasonable()
    {
        var big = new Solid(MeshFactory.CreateCube(size: 2).Mesh);
        var small = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), size: 0.3).Mesh);
        var result = Csg.Union(big, small);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol >= 7.9); // big cube volume
    }

    [Theory]
    [InlineData(0.3)]
    [InlineData(0.5)]
    [InlineData(0.7)]
    public void CubeCube_InclusionExclusion_Holds(double offset)
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(offset, offset, offset)).Mesh);
        double vA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double vB = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        double vU = VolumeCalculator.ComputeAbsoluteVolume(Csg.Union(a, b).Mesh);
        double vI = VolumeCalculator.ComputeAbsoluteVolume(Csg.Intersect(a, b).Mesh);
        double expected = vA + vB - vI;
        Assert.True(System.Math.Abs(vU - expected) < 0.3,
            $"IE failed at offset={offset}: U={vU}, expected={expected}");
    }
}
