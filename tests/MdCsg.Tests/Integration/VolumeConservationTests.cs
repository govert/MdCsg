using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 31: Volume conservation tests (20 tests)</summary>
public class VolumeConservationTests
{
    [Fact]
    public void Cube_Volume_IsOne()
    {
        var cube = MeshFactory.CreateCube();
        MeshAssertions.AssertVolume(cube.Mesh, 1.0, 0.01);
    }

    [Fact]
    public void Cube_Size2_Volume_Is8()
    {
        var cube = MeshFactory.CreateCube(size: 2);
        MeshAssertions.AssertVolume(cube.Mesh, 8.0, 0.01);
    }

    [Fact]
    public void Cube_Size05_Volume()
    {
        var cube = MeshFactory.CreateCube(size: 0.5);
        MeshAssertions.AssertVolume(cube.Mesh, 0.125, 0.01);
    }

    [Fact]
    public void DisjointUnion_Volume_IsSum()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(System.Math.Abs(vol - 2.0) < 0.1, $"Expected ~2.0, got {vol}");
    }

    [Fact]
    public void Union_Volume_GreaterThanEither()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Union(a, b);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol > 1.0, $"Union volume {vol} should be > 1.0");
    }

    [Fact]
    public void Union_Volume_LessThanSum()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Union(a, b);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol < 2.0, $"Union volume {vol} should be < 2.0");
    }

    [Fact]
    public void Intersection_Volume_LessThanEither()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Intersect(a, b);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol < 1.0, $"Intersection volume {vol} should be < 1.0");
        Assert.True(vol > 0, $"Intersection volume should be > 0");
    }

    [Fact]
    public void Difference_Volume_LessThanA()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Difference(a, b);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol < 1.0, $"Difference volume {vol} should be < 1.0");
        Assert.True(vol > 0, $"Difference volume should be > 0");
    }

    [Fact]
    public void InclusionExclusion_UnionPlusIntersect_EqualsSumOfParts()
    {
        // V(A∪B) + V(A∩B) = V(A) + V(B)
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var union = Csg.Union(a, b);
        var intersect = Csg.Intersect(a, b);
        double vU = VolumeCalculator.ComputeAbsoluteVolume(union.Mesh);
        double vI = VolumeCalculator.ComputeAbsoluteVolume(intersect.Mesh);
        double vA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double vB = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        Assert.True(System.Math.Abs((vU + vI) - (vA + vB)) < 0.15,
            $"Inclusion-exclusion: {vU} + {vI} != {vA} + {vB}");
    }

    [Fact]
    public void Difference_Plus_Intersection_EqualsA()
    {
        // V(A\B) + V(A∩B) = V(A)
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var diff = Csg.Difference(a, b);
        var intersect = Csg.Intersect(a, b);
        double vDiff = VolumeCalculator.ComputeAbsoluteVolume(diff.Mesh);
        double vInt = VolumeCalculator.ComputeAbsoluteVolume(intersect.Mesh);
        double vA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        Assert.True(System.Math.Abs((vDiff + vInt) - vA) < 0.15,
            $"V(A\\B) + V(A∩B) = {vDiff} + {vInt} = {vDiff + vInt}, expected {vA}");
    }

    [Fact]
    public void DisjointIntersection_Volume_IsZero()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.Equal(0, result.FaceCount);
    }

    [Fact]
    public void DisjointDifference_Volume_EqualsA()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var result = Csg.Difference(a, b);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(System.Math.Abs(vol - 1.0) < 0.01, $"Expected ~1.0, got {vol}");
    }

    [Fact]
    public void Sphere_Volume_ApproximatelyCorrect()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1, 3);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(sphere.Mesh);
        double expected = (4.0 / 3.0) * System.Math.PI; // ~4.189
        Assert.True(System.Math.Abs(vol - expected) < 0.5, $"Sphere volume {vol}, expected ~{expected}");
    }

    [Fact]
    public void ContainedCube_Intersection_EqualsInner()
    {
        var outer = new Solid(MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5), 2).Mesh);
        var inner = new Solid(MeshFactory.CreateCube(new Vec3(0.25, 0.25, 0.25), 0.5).Mesh);
        var result = Csg.Intersect(outer, inner);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(System.Math.Abs(vol - 0.125) < 0.05, $"Expected ~0.125, got {vol}");
    }

    [Fact]
    public void ContainedCube_Union_EqualsOuter()
    {
        var outer = new Solid(MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5), 2).Mesh);
        var inner = new Solid(MeshFactory.CreateCube(new Vec3(0.25, 0.25, 0.25), 0.5).Mesh);
        var result = Csg.Union(outer, inner);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(System.Math.Abs(vol - 8.0) < 0.5, $"Expected ~8.0, got {vol}");
    }

    [Fact]
    public void ContainedCube_Difference_HasHole()
    {
        var outer = new Solid(MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5), 2).Mesh);
        var inner = new Solid(MeshFactory.CreateCube(new Vec3(0.25, 0.25, 0.25), 0.5).Mesh);
        var result = Csg.Difference(outer, inner);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(System.Math.Abs(vol - (8.0 - 0.125)) < 0.5, $"Expected ~{8.0 - 0.125}, got {vol}");
    }

    [Fact]
    public void Union_Commutative_Volume()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        double vAB = VolumeCalculator.ComputeAbsoluteVolume(Csg.Union(a, b).Mesh);
        double vBA = VolumeCalculator.ComputeAbsoluteVolume(Csg.Union(b, a).Mesh);
        Assert.True(System.Math.Abs(vAB - vBA) < 0.1, $"A∪B vol={vAB}, B∪A vol={vBA}");
    }

    [Fact]
    public void Intersection_Commutative_Volume()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        double vAB = VolumeCalculator.ComputeAbsoluteVolume(Csg.Intersect(a, b).Mesh);
        double vBA = VolumeCalculator.ComputeAbsoluteVolume(Csg.Intersect(b, a).Mesh);
        Assert.True(System.Math.Abs(vAB - vBA) < 0.1, $"A∩B vol={vAB}, B∩A vol={vBA}");
    }

    [Fact]
    public void OverlapVolume_Correct()
    {
        // Two unit cubes at diagonal offset — intersection volume should be between 0 and 1
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var intersect = Csg.Intersect(a, b);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(intersect.Mesh);
        Assert.True(vol > 0.1 && vol < 1.0,
            $"Overlap volume {vol}, expected between 0.1 and 1.0");
    }
}
