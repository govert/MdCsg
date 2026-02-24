using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: Axis-aligned CSG tests — cubes aligned on different axes</summary>
public class AxisAlignedCsgTests
{
    [Theory]
    [InlineData(0.5, 0, 0)]
    [InlineData(0, 0.5, 0)]
    [InlineData(0, 0, 0.5)]
    public void Union_SingleAxisOffset_Positive(double x, double y, double z)
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(x, y, z)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol > 1.0, $"Union volume too small: {vol}");
        Assert.True(vol < 2.0, $"Union volume too large: {vol}");
    }

    [Theory]
    [InlineData(0.5, 0, 0)]
    [InlineData(0, 0.5, 0)]
    [InlineData(0, 0, 0.5)]
    public void Intersection_SingleAxisOffset_Positive(double x, double y, double z)
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(x, y, z)).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.True(result.FaceCount > 0);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol > 0 && vol < 1.0, $"Intersection volume: {vol}");
    }

    [Theory]
    [InlineData(0.5, 0, 0)]
    [InlineData(0, 0.5, 0)]
    [InlineData(0, 0, 0.5)]
    public void Difference_SingleAxisOffset_Positive(double x, double y, double z)
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(x, y, z)).Mesh);
        var result = Csg.Difference(a, b);
        Assert.True(result.FaceCount > 0);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol > 0 && vol < 1.0, $"Difference volume: {vol}");
    }

    [Theory]
    [InlineData(0.5, 0.5, 0)]
    [InlineData(0.5, 0, 0.5)]
    [InlineData(0, 0.5, 0.5)]
    public void Union_TwoAxisOffset_Positive(double x, double y, double z)
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(x, y, z)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol > 1.0);
    }

    [Fact]
    public void Union_DiagonalOffset_Positive()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Union(a, b);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol > 1.0);
    }

    [Fact]
    public void Union_NegativeOffset_Works()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_LargeAndSmall_VolumeCorrect()
    {
        var big = new Solid(MeshFactory.CreateCube(size: 3).Mesh);
        var small = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), size: 0.5).Mesh);
        double vBig = VolumeCalculator.ComputeAbsoluteVolume(big.Mesh);
        var result = Csg.Union(big, small);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        // Small is entirely inside big, so union volume = big
        Assert.True(System.Math.Abs(vol - vBig) < 0.5, $"vol={vol}, vBig={vBig}");
    }

    [Fact]
    public void Intersection_ContainedCube_VolumeEqualsSmall()
    {
        var big = new Solid(MeshFactory.CreateCube(new Vec3(-1, -1, -1), size: 4).Mesh);
        var small = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3), size: 0.5).Mesh);
        double vSmall = VolumeCalculator.ComputeAbsoluteVolume(small.Mesh);
        var result = Csg.Intersect(big, small);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(System.Math.Abs(vol - vSmall) < 0.1);
    }
}
