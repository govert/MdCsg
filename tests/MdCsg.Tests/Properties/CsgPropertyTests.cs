using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Properties;

public class CsgPropertyTests
{
    private static readonly CsgOptions Options = new() { UseWindingNumber = true };

    [Fact]
    public void Union_NearlyContained_ApproximatesOuter()
    {
        // Outer cube fully contains a smaller inner cube
        // Union should be approximately the outer cube
        var outer = MeshFactory.CreateCube(new Vec3(-0.1, -0.1, -0.1), 1.2);
        var inner = MeshFactory.CreateCube(new Vec3(0.1, 0.1, 0.1), 0.8);

        var result = Csg.Union(outer, inner, Options);

        double outerVol = VolumeCalculator.ComputeAbsoluteVolume(outer.Mesh);
        double resultVol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);

        Assert.True(System.Math.Abs(resultVol - outerVol) < 0.2,
            $"Union with contained solid should ≈ outer volume. Expected ~{outerVol}, got {resultVol}.");
    }

    [Fact]
    public void Intersection_ContainedCube_ApproximatesInner()
    {
        // Intersection of a big and small cube should ≈ smaller cube
        var outer = MeshFactory.CreateCube(new Vec3(-0.1, -0.1, -0.1), 1.2);
        var inner = MeshFactory.CreateCube(new Vec3(0.1, 0.1, 0.1), 0.8);

        var result = Csg.Intersect(outer, inner, Options);

        double innerVol = VolumeCalculator.ComputeAbsoluteVolume(inner.Mesh);
        double resultVol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);

        Assert.True(System.Math.Abs(resultVol - innerVol) < 0.2,
            $"Intersection should ≈ inner volume. Expected ~{innerVol}, got {resultVol}.");
    }

    [Fact]
    public void Difference_DisjointCubes_IsOriginal()
    {
        // A \ B = A when B is disjoint
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(5, 5, 5));

        var result = Csg.Difference(a, b, Options);

        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double resultVol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);

        Assert.True(System.Math.Abs(resultVol - volA) < 0.01,
            $"A\\B with disjoint B should = A. Expected {volA}, got {resultVol}.");
    }

    [Fact]
    public void Union_IsCommutative_Approximately()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5));

        var ab = Csg.Union(a, b, Options);
        var ba = Csg.Union(b, a, Options);

        double volAB = VolumeCalculator.ComputeAbsoluteVolume(ab.Mesh);
        double volBA = VolumeCalculator.ComputeAbsoluteVolume(ba.Mesh);

        Assert.True(System.Math.Abs(volAB - volBA) < 0.1,
            $"A∪B and B∪A should have same volume. Got {volAB} and {volBA}.");
    }

    [Fact]
    public void Intersection_IsCommutative_Approximately()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5));

        var ab = Csg.Intersect(a, b, Options);
        var ba = Csg.Intersect(b, a, Options);

        double volAB = VolumeCalculator.ComputeAbsoluteVolume(ab.Mesh);
        double volBA = VolumeCalculator.ComputeAbsoluteVolume(ba.Mesh);

        Assert.True(System.Math.Abs(volAB - volBA) < 0.1,
            $"A∩B and B∩A should have same volume. Got {volAB} and {volBA}.");
    }
}
