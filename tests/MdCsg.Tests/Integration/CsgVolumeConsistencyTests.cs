using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG volume consistency — union/intersection/difference volume relationships</summary>
public class CsgVolumeConsistencyTests
{
    [Fact]
    public void Union_Volume_GreaterThanEitherInput()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Union(a, b);
        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volB = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        double volR = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(volR >= volA * 0.99, $"Union volume {volR} should be >= A volume {volA}");
        Assert.True(volR >= volB * 0.99, $"Union volume {volR} should be >= B volume {volB}");
    }

    [Fact]
    public void Intersection_Volume_LessThanEitherInput()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Intersect(a, b);
        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volB = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        double volR = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(volR <= volA * 1.01, $"Intersection volume {volR} should be <= A volume {volA}");
        Assert.True(volR <= volB * 1.01, $"Intersection volume {volR} should be <= B volume {volB}");
    }

    [Fact]
    public void InclusionExclusion_VolumeIdentity()
    {
        // V(A ∪ B) = V(A) + V(B) - V(A ∩ B)
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volB = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        var union = Csg.Union(a, b);
        var inter = Csg.Intersect(a, b);
        double volUnion = VolumeCalculator.ComputeAbsoluteVolume(union.Mesh);
        double volInter = VolumeCalculator.ComputeAbsoluteVolume(inter.Mesh);
        double expected = volA + volB - volInter;
        Assert.True(System.Math.Abs(volUnion - expected) < 0.05,
            $"V(A∪B)={volUnion} should ≈ V(A)+V(B)-V(A∩B)={expected}");
    }

    [Fact]
    public void Difference_Volume_LessThanMinuend()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Difference(a, b);
        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volR = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(volR <= volA * 1.01, $"Difference volume {volR} should be <= A volume {volA}");
    }

    [Fact]
    public void Difference_Volume_Plus_Intersection_Equals_A()
    {
        // V(A - B) + V(A ∩ B) ≈ V(A)
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var diff = Csg.Difference(a, b);
        var inter = Csg.Intersect(a, b);
        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volDiff = VolumeCalculator.ComputeAbsoluteVolume(diff.Mesh);
        double volInter = VolumeCalculator.ComputeAbsoluteVolume(inter.Mesh);
        Assert.True(System.Math.Abs((volDiff + volInter) - volA) < 0.05,
            $"V(A-B)={volDiff} + V(A∩B)={volInter} should ≈ V(A)={volA}");
    }

    [Fact]
    public void DisjointUnion_Volume_EqualsSum()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volB = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        double volR = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.Equal(volA + volB, volR, 1e-8);
    }

    [Fact]
    public void DisjointIntersection_Volume_IsZero()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var result = Csg.Intersect(a, b);
        double volR = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.Equal(0.0, volR, 1e-10);
    }

    [Fact]
    public void FullyContained_Intersection_Volume_EqualsInner()
    {
        var outer = new Solid(MeshFactory.CreateCube(Vec3.Zero, 2).Mesh);
        var inner = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), 0.5).Mesh);
        var result = Csg.Intersect(outer, inner);
        double volInner = VolumeCalculator.ComputeAbsoluteVolume(inner.Mesh);
        double volResult = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(System.Math.Abs(volResult - volInner) < 0.01,
            $"Intersection volume {volResult} should ≈ inner volume {volInner}");
    }

    [Fact]
    public void FullyContained_Union_Volume_EqualsOuter()
    {
        var outer = new Solid(MeshFactory.CreateCube(Vec3.Zero, 2).Mesh);
        var inner = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), 0.5).Mesh);
        var result = Csg.Union(outer, inner);
        double volOuter = VolumeCalculator.ComputeAbsoluteVolume(outer.Mesh);
        double volResult = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(System.Math.Abs(volResult - volOuter) < 0.01,
            $"Union volume {volResult} should ≈ outer volume {volOuter}");
    }

    [Fact]
    public void FullyContained_Difference_Volume_HasCavity()
    {
        var outer = new Solid(MeshFactory.CreateCube(Vec3.Zero, 2).Mesh);
        var inner = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), 0.5).Mesh);
        var result = Csg.Difference(outer, inner);
        double volOuter = VolumeCalculator.ComputeAbsoluteVolume(outer.Mesh);
        double volInner = VolumeCalculator.ComputeAbsoluteVolume(inner.Mesh);
        double volResult = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(System.Math.Abs(volResult - (volOuter - volInner)) < 0.05,
            $"Difference volume {volResult} should ≈ {volOuter} - {volInner} = {volOuter - volInner}");
    }

    [Fact]
    public void CubeSphere_Union_Volume_GreaterThanEither()
    {
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 1.0), 0.5, 2).Mesh);
        var result = Csg.Union(cube, sphere);
        double volCube = VolumeCalculator.ComputeAbsoluteVolume(cube.Mesh);
        double volResult = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(volResult > volCube * 0.99);
    }

    [Fact]
    public void Intersection_Overlapping_Volume_Positive()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Intersect(a, b);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        // Overlap region is 0.5^3 = 0.125
        Assert.True(System.Math.Abs(vol - 0.125) < 0.02,
            $"Intersection volume {vol} should ≈ 0.125");
    }
}
