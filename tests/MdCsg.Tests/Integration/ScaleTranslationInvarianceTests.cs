using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: Scale and translation invariance of CSG operations</summary>
public class ScaleTranslationInvarianceTests
{
    [Fact]
    public void Union_AtOffset_ProducesValidVolume()
    {
        var a = new Solid(MeshFactory.CreateCube(new Vec3(5, 5, 5)).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5.3, 5.3, 5.3)).Mesh);
        var r = Csg.Union(a, b);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(r.Mesh);
        // Two overlapping unit cubes: union volume should be between 1 and 2
        Assert.True(vol > 0.8 && vol < 2.5, $"Union volume at offset: {vol}");
    }

    [Fact]
    public void Intersection_AtOffset_ProducesValidVolume()
    {
        var a = new Solid(MeshFactory.CreateCube(new Vec3(5, 5, 5)).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5.3, 5.3, 5.3)).Mesh);
        var r = Csg.Intersect(a, b);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(r.Mesh);
        // Overlap = 0.7^3 = 0.343
        Assert.True(vol > 0.1, $"Intersection volume at offset: {vol}");
    }

    [Fact]
    public void Difference_AtOffset_ProducesValidVolume()
    {
        var a = new Solid(MeshFactory.CreateCube(new Vec3(5, 5, 5)).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5.3, 5.3, 5.3)).Mesh);
        var r = Csg.Difference(a, b);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(r.Mesh);
        // A minus overlap: 1 - 0.343 = 0.657
        Assert.True(vol > 0.1 && vol < 1.5, $"Difference volume at offset: {vol}");
    }

    [Fact]
    public void Union_TranslationInvariant_BothProduceFaces()
    {
        var a1 = new Solid(MeshFactory.CreateCube().Mesh);
        var b1 = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        int fc1 = Csg.Union(a1, b1).FaceCount;

        var a2 = new Solid(MeshFactory.CreateCube(new Vec3(100, 100, 100)).Mesh);
        var b2 = new Solid(MeshFactory.CreateCube(new Vec3(100.3, 100.3, 100.3)).Mesh);
        int fc2 = Csg.Union(a2, b2).FaceCount;

        Assert.True(fc1 > 0 && fc2 > 0, $"Both should have faces: fc1={fc1}, fc2={fc2}");
    }

    [Fact]
    public void Union_ScaleInvariant_Volume()
    {
        var a1 = new Solid(MeshFactory.CreateCube(size: 1).Mesh);
        var b1 = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3), size: 1).Mesh);
        double v1 = VolumeCalculator.ComputeAbsoluteVolume(Csg.Union(a1, b1).Mesh);

        var a2 = new Solid(MeshFactory.CreateCube(size: 2).Mesh);
        var b2 = new Solid(MeshFactory.CreateCube(new Vec3(0.6, 0.6, 0.6), size: 2).Mesh);
        double v2 = VolumeCalculator.ComputeAbsoluteVolume(Csg.Union(a2, b2).Mesh);

        // Scale 2 → volume ratio should be 8 (2^3)
        Assert.True(System.Math.Abs(v2 / v1 - 8.0) < 1.0,
            $"Scale invariance: v1={v1}, v2={v2}, ratio={v2 / v1}");
    }

    [Fact]
    public void Intersection_ScaleInvariant_Volume()
    {
        var a1 = new Solid(MeshFactory.CreateCube(size: 1).Mesh);
        var b1 = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3), size: 1).Mesh);
        double v1 = VolumeCalculator.ComputeAbsoluteVolume(Csg.Intersect(a1, b1).Mesh);

        var a2 = new Solid(MeshFactory.CreateCube(size: 2).Mesh);
        var b2 = new Solid(MeshFactory.CreateCube(new Vec3(0.6, 0.6, 0.6), size: 2).Mesh);
        double v2 = VolumeCalculator.ComputeAbsoluteVolume(Csg.Intersect(a2, b2).Mesh);

        // Scale 2 → volume ratio should be 8 (2^3)
        double ratio = v2 / v1;
        Assert.True(System.Math.Abs(ratio - 8.0) < 2.0,
            $"Scale invariance: v1={v1}, v2={v2}, ratio={ratio}");
    }

    [Fact]
    public void NegativeCoordinates_CsgWorks()
    {
        var a = new Solid(MeshFactory.CreateCube(new Vec3(-2, -2, -2)).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(-1.7, -1.7, -1.7)).Mesh);
        var r = Csg.Union(a, b);
        Assert.True(r.FaceCount > 0);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(r.Mesh);
        Assert.True(vol > 0);
    }

    [Fact]
    public void MixedPositiveNegative_CsgWorks()
    {
        var a = new Solid(MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5)).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0, 0, 0)).Mesh);
        var r = Csg.Union(a, b);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void SmallScale_CsgWorks()
    {
        var a = new Solid(MeshFactory.CreateCube(size: 0.01).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.003, 0.003, 0.003), size: 0.01).Mesh);
        var r = Csg.Union(a, b);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void LargeScale_CsgWorks()
    {
        var a = new Solid(MeshFactory.CreateCube(size: 100).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(30, 30, 30), size: 100).Mesh);
        var r = Csg.Union(a, b);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void DisjointCubes_AllOps_WorkCorrectly()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(10, 0, 0)).Mesh);

        var u = Csg.Union(a, b);
        var i = Csg.Intersect(a, b);
        var d = Csg.Difference(a, b);

        Assert.True(u.FaceCount > 0);
        Assert.True(d.FaceCount > 0);
        // Disjoint intersection should have 0 segments
        Assert.Equal(0, i.IntersectionSegmentCount);
    }

    [Fact]
    public void Difference_Volume_Positive()
    {
        var a = new Solid(MeshFactory.CreateCube(size: 2).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), size: 1).Mesh);
        var r = Csg.Difference(a, b);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(r.Mesh);
        // A=8, B=1, overlap=1 → diff should be ~7
        Assert.True(vol > 5.0 && vol < 8.5, $"Difference volume: {vol}");
    }

    [Fact]
    public void NegativeCoordinates_Intersection_HasVolume()
    {
        var a = new Solid(MeshFactory.CreateCube(new Vec3(-2, -2, -2)).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(-1.7, -1.7, -1.7)).Mesh);
        var r = Csg.Intersect(a, b);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(r.Mesh);
        // Overlap = 0.7^3 = 0.343
        Assert.True(vol > 0.1, $"Intersection should have volume, got {vol}");
    }
}
