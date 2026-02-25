using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG idempotency and algebraic properties — A∪A≈A, A∩A≈A, inclusion-exclusion bounds</summary>
public class CsgIdempotencyPropertyTests
{
    [Fact]
    public void Union_ContainedSmall_VolumeEqualsLarge()
    {
        // A ∪ (small inside A) should have same volume as A
        var a = new Solid(MeshFactory.CreateCube(Vec3.Zero, 2).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), 0.5).Mesh);
        var result = Csg.Union(a, b);
        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volResult = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(System.Math.Abs(volResult - volA) / volA < 0.02,
            $"Union volume {volResult} should be ~{volA}");
    }

    [Fact]
    public void Intersection_ContainedSmall_VolumeEqualsSmall()
    {
        // A ∩ (small inside A) should have same volume as small
        var a = new Solid(MeshFactory.CreateCube(Vec3.Zero, 2).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), 0.5).Mesh);
        var result = Csg.Intersect(a, b);
        double volB = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        double volResult = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(System.Math.Abs(volResult - volB) / volB < 0.02,
            $"Intersection volume {volResult} should be ~{volB}");
    }

    [Fact]
    public void Difference_ContainedSmall_VolumeEquals_Large_Minus_Small()
    {
        var a = new Solid(MeshFactory.CreateCube(Vec3.Zero, 2).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), 0.5).Mesh);
        var result = Csg.Difference(a, b);
        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volB = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        double volResult = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(System.Math.Abs(volResult - (volA - volB)) < 0.5,
            $"Difference volume {volResult} should be ~{volA - volB}");
    }

    [Fact]
    public void InclusionExclusion_UnionVolume()
    {
        // |A ∪ B| = |A| + |B| - |A ∩ B|
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);

        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volB = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        double volUnion = VolumeCalculator.ComputeAbsoluteVolume(Csg.Union(a, b).Mesh);
        double volIntersect = VolumeCalculator.ComputeAbsoluteVolume(Csg.Intersect(a, b).Mesh);

        double expected = volA + volB - volIntersect;
        Assert.True(System.Math.Abs(volUnion - expected) < 0.1,
            $"|A∪B|={volUnion}, |A|+|B|-|A∩B|={expected}");
    }

    [Fact]
    public void Union_Volume_GreaterOrEqualEach()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volB = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        double volUnion = VolumeCalculator.ComputeAbsoluteVolume(Csg.Union(a, b).Mesh);
        Assert.True(volUnion >= volA * 0.99);
        Assert.True(volUnion >= volB * 0.99);
    }

    [Fact]
    public void Intersection_Volume_LessOrEqualEach()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volB = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        double volIntersect = VolumeCalculator.ComputeAbsoluteVolume(Csg.Intersect(a, b).Mesh);
        Assert.True(volIntersect <= volA * 1.01);
        Assert.True(volIntersect <= volB * 1.01);
    }

    [Fact]
    public void Difference_Volume_LessOrEqualA()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volDiff = VolumeCalculator.ComputeAbsoluteVolume(Csg.Difference(a, b).Mesh);
        Assert.True(volDiff <= volA * 1.01);
    }

    [Fact]
    public void DifferenceEquivalence_A_Minus_B_Eq_A_MinusIntersection()
    {
        // |A \ B| = |A| - |A ∩ B|
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);

        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volDiff = VolumeCalculator.ComputeAbsoluteVolume(Csg.Difference(a, b).Mesh);
        double volIntersect = VolumeCalculator.ComputeAbsoluteVolume(Csg.Intersect(a, b).Mesh);

        Assert.True(System.Math.Abs(volDiff - (volA - volIntersect)) < 0.1,
            $"|A\\B|={volDiff}, |A|-|A∩B|={volA - volIntersect}");
    }

    [Fact]
    public void DisjointUnion_VolumeIsSum()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volB = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        double volUnion = VolumeCalculator.ComputeAbsoluteVolume(Csg.Union(a, b).Mesh);
        Assert.True(System.Math.Abs(volUnion - (volA + volB)) < 0.01);
    }

    [Fact]
    public void DisjointIntersection_ZeroOrEmptyVolume()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var result = Csg.Intersect(a, b);
        if (result.FaceCount > 0)
        {
            double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
            Assert.True(vol < 0.001);
        }
    }

    [Fact]
    public void DisjointDifference_VolumeEqualsA()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volDiff = VolumeCalculator.ComputeAbsoluteVolume(Csg.Difference(a, b).Mesh);
        Assert.True(System.Math.Abs(volDiff - volA) < 0.01);
    }

    [Fact]
    public void FullyContained_Union_EqualsOuter()
    {
        var outer = new Solid(MeshFactory.CreateCube(Vec3.Zero, 2).Mesh);
        var inner = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), 0.5).Mesh);
        double volOuter = VolumeCalculator.ComputeAbsoluteVolume(outer.Mesh);
        double volUnion = VolumeCalculator.ComputeAbsoluteVolume(Csg.Union(outer, inner).Mesh);
        Assert.True(System.Math.Abs(volUnion - volOuter) < 0.1);
    }

    [Fact]
    public void FullyContained_Intersection_EqualsInner()
    {
        var outer = new Solid(MeshFactory.CreateCube(Vec3.Zero, 2).Mesh);
        var inner = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), 0.5).Mesh);
        double volInner = VolumeCalculator.ComputeAbsoluteVolume(inner.Mesh);
        double volIntersect = VolumeCalculator.ComputeAbsoluteVolume(Csg.Intersect(outer, inner).Mesh);
        Assert.True(System.Math.Abs(volIntersect - volInner) < 0.1);
    }
}
