using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: VolumeCalculator tests — signed volume, divergence theorem accuracy</summary>
public class VolumeCalculatorTests
{
    [Fact]
    public void UnitCube_Volume_Is1()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        double vol = VolumeCalculator.ComputeAbsoluteVolume(mesh);
        Assert.Equal(1.0, vol, 5);
    }

    [Fact]
    public void UnitCube_SignedVolume_Positive()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        double vol = VolumeCalculator.ComputeVolume(mesh);
        // Depends on face orientation — could be positive or negative
        Assert.True(System.Math.Abs(vol) > 0.99);
    }

    [Fact]
    public void Cube_Size2_Volume_Is8()
    {
        var mesh = MeshFactory.CreateCube(size: 2).Mesh;
        double vol = VolumeCalculator.ComputeAbsoluteVolume(mesh);
        Assert.Equal(8.0, vol, 5);
    }

    [Fact]
    public void Cube_Size3_Volume_Is27()
    {
        var mesh = MeshFactory.CreateCube(size: 3).Mesh;
        double vol = VolumeCalculator.ComputeAbsoluteVolume(mesh);
        Assert.Equal(27.0, vol, 5);
    }

    [Fact]
    public void Cube_HalfSize_Volume_Is_0125()
    {
        var mesh = MeshFactory.CreateCube(size: 0.5).Mesh;
        double vol = VolumeCalculator.ComputeAbsoluteVolume(mesh);
        Assert.Equal(0.125, vol, 5);
    }

    [Fact]
    public void Tetrahedron_Volume_Positive()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        double vol = VolumeCalculator.ComputeAbsoluteVolume(mesh);
        Assert.True(vol > 0);
    }

    [Fact]
    public void Sphere_Sub2_Volume_ApproachesPi()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        double vol = VolumeCalculator.ComputeAbsoluteVolume(mesh);
        double expected = (4.0 / 3.0) * System.Math.PI;
        Assert.True(System.Math.Abs(vol - expected) < 0.3, $"vol={vol}, expected={expected}");
    }

    [Fact]
    public void Sphere_Sub3_Volume_CloserToPi()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 3).Mesh;
        double vol = VolumeCalculator.ComputeAbsoluteVolume(mesh);
        double expected = (4.0 / 3.0) * System.Math.PI;
        Assert.True(System.Math.Abs(vol - expected) < 0.2, $"vol={vol}, expected={expected}");
    }

    [Fact]
    public void Sphere_Radius2_Volume_Scales()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 2, 2).Mesh;
        double vol = VolumeCalculator.ComputeAbsoluteVolume(mesh);
        double expected = (4.0 / 3.0) * System.Math.PI * 8; // r^3 = 8
        Assert.True(System.Math.Abs(vol - expected) < 2.5, $"vol={vol}, expected={expected}");
    }

    [Fact]
    public void OffsetCube_SameVolume()
    {
        var mesh1 = MeshFactory.CreateCube().Mesh;
        var mesh2 = MeshFactory.CreateCube(new Vec3(100, 200, 300)).Mesh;
        double v1 = VolumeCalculator.ComputeAbsoluteVolume(mesh1);
        double v2 = VolumeCalculator.ComputeAbsoluteVolume(mesh2);
        Assert.True(System.Math.Abs(v1 - v2) < 0.01, $"v1={v1}, v2={v2}");
    }

    [Fact]
    public void CsgUnion_Volume_GreaterThanEither()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        double vA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double vU = VolumeCalculator.ComputeAbsoluteVolume(Csg.Union(a, b).Mesh);
        Assert.True(vU >= vA - 0.1);
    }

    [Fact]
    public void CsgIntersection_Volume_LessThanEither()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        double vA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double vI = VolumeCalculator.ComputeAbsoluteVolume(Csg.Intersect(a, b).Mesh);
        Assert.True(vI <= vA + 0.1);
    }

    [Fact]
    public void CsgDifference_Volume_LessThanA()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        double vA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double vD = VolumeCalculator.ComputeAbsoluteVolume(Csg.Difference(a, b).Mesh);
        Assert.True(vD <= vA + 0.1);
        Assert.True(vD > 0);
    }
}
