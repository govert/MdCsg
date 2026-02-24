using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG identity and volume algebraic property tests</summary>
public class CsgIdentityTests
{
    [Fact]
    public void InclusionExclusion_CubeCube()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        double vA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double vB = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        double vU = VolumeCalculator.ComputeAbsoluteVolume(Csg.Union(a, b).Mesh);
        double vI = VolumeCalculator.ComputeAbsoluteVolume(Csg.Intersect(a, b).Mesh);
        // |A ∪ B| = |A| + |B| - |A ∩ B|
        double expected = vA + vB - vI;
        Assert.True(System.Math.Abs(vU - expected) < 0.2,
            $"Inclusion-exclusion: U={vU}, expected={expected}");
    }

    [Fact]
    public void Partition_A_Into_Intersection_And_Difference()
    {
        // |A| = |A∩B| + |A\B|
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        double vA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double vI = VolumeCalculator.ComputeAbsoluteVolume(Csg.Intersect(a, b).Mesh);
        double vD = VolumeCalculator.ComputeAbsoluteVolume(Csg.Difference(a, b).Mesh);
        Assert.True(System.Math.Abs(vA - (vI + vD)) < 0.2,
            $"|A|={vA}, |A∩B|={vI}, |A\\B|={vD}, sum={vI + vD}");
    }

    [Fact]
    public void Disjoint_Union_SumOfVolumes()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        double vA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double vB = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        double vU = VolumeCalculator.ComputeAbsoluteVolume(Csg.Union(a, b).Mesh);
        Assert.True(System.Math.Abs(vU - (vA + vB)) < 0.1,
            $"Disjoint union: U={vU}, A+B={vA + vB}");
    }

    [Fact]
    public void Disjoint_Difference_VolumeOfA()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        double vA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double vD = VolumeCalculator.ComputeAbsoluteVolume(Csg.Difference(a, b).Mesh);
        Assert.True(System.Math.Abs(vD - vA) < 0.1);
    }

    [Fact]
    public void Contained_Intersection_VolumeOfSmaller()
    {
        var big = new Solid(MeshFactory.CreateCube(size: 3).Mesh);
        var small = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), size: 1).Mesh);
        double vSmall = VolumeCalculator.ComputeAbsoluteVolume(small.Mesh);
        double vI = VolumeCalculator.ComputeAbsoluteVolume(Csg.Intersect(big, small).Mesh);
        Assert.True(System.Math.Abs(vI - vSmall) < 0.2,
            $"Contained intersection: I={vI}, small={vSmall}");
    }

    [Fact]
    public void Union_Commutative_Volume()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.4, 0.4, 0.4)).Mesh);
        double v1 = VolumeCalculator.ComputeAbsoluteVolume(Csg.Union(a, b).Mesh);
        double v2 = VolumeCalculator.ComputeAbsoluteVolume(Csg.Union(b, a).Mesh);
        Assert.True(System.Math.Abs(v1 - v2) < 0.01, $"Union commutative: {v1} vs {v2}");
    }

    [Fact]
    public void Intersect_Commutative_Volume()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.4, 0.4, 0.4)).Mesh);
        double v1 = VolumeCalculator.ComputeAbsoluteVolume(Csg.Intersect(a, b).Mesh);
        double v2 = VolumeCalculator.ComputeAbsoluteVolume(Csg.Intersect(b, a).Mesh);
        Assert.True(System.Math.Abs(v1 - v2) < 0.01, $"Intersect commutative: {v1} vs {v2}");
    }

    [Fact]
    public void Union_LargerThanBoth()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        double vA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double vB = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        double vU = VolumeCalculator.ComputeAbsoluteVolume(Csg.Union(a, b).Mesh);
        Assert.True(vU >= vA - 0.1);
        Assert.True(vU >= vB - 0.1);
    }

    [Fact]
    public void Intersection_SmallerThanBoth()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        double vA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double vB = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        double vI = VolumeCalculator.ComputeAbsoluteVolume(Csg.Intersect(a, b).Mesh);
        Assert.True(vI <= vA + 0.1);
        Assert.True(vI <= vB + 0.1);
    }

    [Fact]
    public void Difference_SmallerThanA()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        double vA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double vD = VolumeCalculator.ComputeAbsoluteVolume(Csg.Difference(a, b).Mesh);
        Assert.True(vD <= vA + 0.1);
    }

    [Fact]
    public void Cube_Volume_Exact()
    {
        double vol = VolumeCalculator.ComputeAbsoluteVolume(MeshFactory.CreateCube().Mesh);
        Assert.Equal(1.0, vol, 5);
    }

    [Fact]
    public void Cube_Scaled_Volume()
    {
        double vol = VolumeCalculator.ComputeAbsoluteVolume(MeshFactory.CreateCube(size: 3).Mesh);
        Assert.Equal(27.0, vol, 5);
    }

    [Fact]
    public void Sphere_Volume_HighSub()
    {
        double vol = VolumeCalculator.ComputeAbsoluteVolume(MeshFactory.CreateSphere(Vec3.Zero, 1, 3).Mesh);
        double expected = (4.0 / 3.0) * System.Math.PI;
        Assert.True(System.Math.Abs(vol - expected) < 0.2,
            $"Sphere volume: {vol}, expected: {expected}");
    }
}
