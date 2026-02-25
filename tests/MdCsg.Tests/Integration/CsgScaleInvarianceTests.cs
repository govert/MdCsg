using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG scale invariance — operations at different scales produce proportional results</summary>
public class CsgScaleInvarianceTests
{
    private static double Volume(CsgResult r) => VolumeCalculator.ComputeAbsoluteVolume(r.Mesh);

    [Fact]
    public void Union_SmallCubes_Positive()
    {
        var a = new Solid(MeshFactory.CreateCube(Vec3.Zero, 0.1).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.05, 0, 0), 0.1).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
        Assert.True(Volume(result) > 0);
    }

    [Fact]
    public void Union_LargeCubes_Positive()
    {
        var a = new Solid(MeshFactory.CreateCube(Vec3.Zero, 100).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(50, 0, 0), 100).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
        double vol = Volume(result);
        Assert.True(vol > 1e6); // > 100^3
    }

    [Fact]
    public void Intersection_SmallCubes_Positive()
    {
        var a = new Solid(MeshFactory.CreateCube(Vec3.Zero, 0.1).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.05, 0, 0), 0.1).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.True(result.FaceCount > 0);
        Assert.True(Volume(result) > 0);
    }

    [Fact]
    public void Difference_SmallCubes_Positive()
    {
        var a = new Solid(MeshFactory.CreateCube(Vec3.Zero, 0.1).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.05, 0, 0), 0.1).Mesh);
        var result = Csg.Difference(a, b);
        Assert.True(result.FaceCount > 0);
        Assert.True(Volume(result) > 0);
    }

    [Fact]
    public void VolumeScaling_CubeDoubled()
    {
        // Cube of size 1 has volume 1, cube of size 2 has volume 8
        var small = MeshFactory.CreateCube(Vec3.Zero, 1);
        var large = MeshFactory.CreateCube(Vec3.Zero, 2);
        double volSmall = VolumeCalculator.ComputeAbsoluteVolume(small.Mesh);
        double volLarge = VolumeCalculator.ComputeAbsoluteVolume(large.Mesh);
        Assert.True(System.Math.Abs(volLarge / volSmall - 8.0) < 0.01);
    }

    [Fact]
    public void VolumeScaling_CubeHalved()
    {
        var unit = MeshFactory.CreateCube(Vec3.Zero, 1);
        var half = MeshFactory.CreateCube(Vec3.Zero, 0.5);
        double volUnit = VolumeCalculator.ComputeAbsoluteVolume(unit.Mesh);
        double volHalf = VolumeCalculator.ComputeAbsoluteVolume(half.Mesh);
        Assert.True(System.Math.Abs(volHalf / volUnit - 0.125) < 0.001);
    }

    [Fact]
    public void Union_OffsetCubes_VolumeRatio()
    {
        // Two cubes overlapping 50%: Union volume should be 1.5x of one cube
        var a = new Solid(MeshFactory.CreateCube(Vec3.Zero, 1).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0), 1).Mesh);
        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volUnion = Volume(Csg.Union(a, b));
        double ratio = volUnion / volA;
        Assert.True(ratio > 1.0 && ratio < 2.0,
            $"Expected ratio between 1 and 2, got {ratio:F4}");
    }

    [Fact]
    public void Sphere_VolumeScaling()
    {
        var s1 = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var s2 = MeshFactory.CreateSphere(Vec3.Zero, 2.0, 2);
        double vol1 = VolumeCalculator.ComputeAbsoluteVolume(s1.Mesh);
        double vol2 = VolumeCalculator.ComputeAbsoluteVolume(s2.Mesh);
        // Volume scales as r^3
        double ratio = vol2 / vol1;
        Assert.True(System.Math.Abs(ratio - 8.0) < 0.5,
            $"Expected ~8, got {ratio:F4}");
    }

    [Fact]
    public void Tetrahedron_VolumeScaling()
    {
        var t1 = MeshFactory.CreateTetrahedron(Vec3.Zero, 1);
        var t2 = MeshFactory.CreateTetrahedron(Vec3.Zero, 2);
        double vol1 = VolumeCalculator.ComputeAbsoluteVolume(t1.Mesh);
        double vol2 = VolumeCalculator.ComputeAbsoluteVolume(t2.Mesh);
        double ratio = vol2 / vol1;
        Assert.True(System.Math.Abs(ratio - 8.0) < 0.5,
            $"Expected ~8, got {ratio:F4}");
    }

    [Fact]
    public void Translation_PreservesVolume()
    {
        double vol1 = VolumeCalculator.ComputeAbsoluteVolume(MeshFactory.CreateCube(Vec3.Zero).Mesh);
        double vol2 = VolumeCalculator.ComputeAbsoluteVolume(MeshFactory.CreateCube(new Vec3(100, 200, 300)).Mesh);
        Assert.Equal(vol1, vol2, 5);
    }

    [Fact]
    public void NegativeOffset_Cube_SameVolume()
    {
        double vol1 = VolumeCalculator.ComputeAbsoluteVolume(MeshFactory.CreateCube(Vec3.Zero).Mesh);
        double vol2 = VolumeCalculator.ComputeAbsoluteVolume(MeshFactory.CreateCube(new Vec3(-5, -10, -15)).Mesh);
        Assert.Equal(vol1, vol2, 5);
    }
}
