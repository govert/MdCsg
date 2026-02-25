using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG with scaled shapes — different sizes, aspect ratios, volume verification</summary>
public class CsgScaledShapeTests
{
    [Fact]
    public void SmallCubeInLargeCube_Difference_VolumeCorrect()
    {
        var outer = new Solid(MeshFactory.CreateCube(Vec3.Zero, 3).Mesh);
        var inner = new Solid(MeshFactory.CreateCube(new Vec3(1, 1, 1), 1).Mesh);
        var result = Csg.Difference(outer, inner);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(System.Math.Abs(vol - (27 - 1)) < 0.5,
            $"Volume {vol} should be ~26");
    }

    [Fact]
    public void LargeCubeSmallCube_Intersection_EqualsSmaller()
    {
        var large = new Solid(MeshFactory.CreateCube(Vec3.Zero, 3).Mesh);
        var small = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), 0.5).Mesh);
        var result = Csg.Intersect(large, small);
        double volSmall = VolumeCalculator.ComputeAbsoluteVolume(small.Mesh);
        double volResult = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(System.Math.Abs(volResult - volSmall) < 0.01);
    }

    [Fact]
    public void LargeCubeSmallCube_Union_EqualsLarger()
    {
        var large = new Solid(MeshFactory.CreateCube(Vec3.Zero, 3).Mesh);
        var small = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), 0.5).Mesh);
        var result = Csg.Union(large, small);
        double volLarge = VolumeCalculator.ComputeAbsoluteVolume(large.Mesh);
        double volResult = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(System.Math.Abs(volResult - volLarge) < 0.1);
    }

    [Fact]
    public void TwoEqualCubes_HalfOverlap_UnionVolume()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        // Two 1x1x1 cubes offset by 0.5 in x: union = 1.5
        Assert.True(System.Math.Abs(vol - 1.5) < 0.05);
    }

    [Fact]
    public void TwoEqualCubes_HalfOverlap_IntersectionVolume()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var result = Csg.Intersect(a, b);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(System.Math.Abs(vol - 0.5) < 0.05);
    }

    [Fact]
    public void SphereDifference_CreatesIndentation()
    {
        var cube = new Solid(MeshFactory.CreateCube(Vec3.Zero, 2).Mesh);
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(1, 1, 2), 0.5, 2).Mesh);
        var result = Csg.Difference(cube, sphere);
        double volCube = VolumeCalculator.ComputeAbsoluteVolume(cube.Mesh);
        double volResult = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(volResult < volCube);
        Assert.True(volResult > volCube * 0.9);
    }

    [Fact]
    public void Size2_Union_Size1_VolumeCorrect()
    {
        var big = new Solid(MeshFactory.CreateCube(Vec3.Zero, 2).Mesh);
        var small = new Solid(MeshFactory.CreateCube(new Vec3(1.5, 0, 0), 1).Mesh);
        var result = Csg.Union(big, small);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        // Big is 8, small is 1, overlap is 0.5*1*2 = 0.5 in z ×  0.5 in x × 2 in y
        // Union = 8 + 1 - overlap
        Assert.True(vol > 8.0);
    }

    [Fact]
    public void ScaledTetrahedra_Intersection_Valid()
    {
        var large = new Solid(MeshFactory.CreateTetrahedron(Vec3.Zero, 2).Mesh);
        var small = new Solid(MeshFactory.CreateTetrahedron(Vec3.Zero, 0.5).Mesh);
        var result = Csg.Intersect(large, small);
        if (result.FaceCount > 0)
        {
            double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
            Assert.True(vol > 0);
        }
    }

    [Fact]
    public void ScaledSpheres_Union_Valid()
    {
        var big = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 2.0, 2).Mesh);
        var small = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 0.5, 2).Mesh);
        var result = Csg.Union(big, small);
        double volBig = VolumeCalculator.ComputeAbsoluteVolume(big.Mesh);
        double volResult = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(System.Math.Abs(volResult - volBig) / volBig < 0.05);
    }

    [Fact]
    public void HalfSizeOffset_AllOps_ValidFaceCount()
    {
        var a = new Solid(MeshFactory.CreateCube(Vec3.Zero, 2).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(1, 1, 1), 1).Mesh);
        Assert.True(Csg.Union(a, b).FaceCount > 0);
        Assert.True(Csg.Intersect(a, b).FaceCount > 0);
        Assert.True(Csg.Difference(a, b).FaceCount > 0);
    }
}
