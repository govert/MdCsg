using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Properties;

public class VolumeConservationTests
{
    [Fact]
    public void VolumeConservation_OverlappingCubes()
    {
        // Vol(A ∪ B) + Vol(A ∩ B) = Vol(A) + Vol(B)
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5));

        var options = new CsgOptions { UseWindingNumber = true };
        var union = Csg.Union(a, b, options);
        var intersection = Csg.Intersect(a, b, options);

        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volB = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        double volUnion = VolumeCalculator.ComputeAbsoluteVolume(union.Mesh);
        double volIntersection = VolumeCalculator.ComputeAbsoluteVolume(intersection.Mesh);

        double lhs = volUnion + volIntersection;
        double rhs = volA + volB;

        Assert.True(System.Math.Abs(lhs - rhs) < 0.15,
            $"Vol(A∪B) + Vol(A∩B) should equal Vol(A) + Vol(B). Got {lhs} vs {rhs}.");
    }

    [Fact]
    public void UnitCube_HasVolumeOne()
    {
        var cube = MeshFactory.CreateCube();
        double vol = VolumeCalculator.ComputeAbsoluteVolume(cube.Mesh);
        Assert.Equal(1.0, vol, 0.01);
    }

    [Fact]
    public void DisjointCubes_UnionVolume_EqualsSum()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(5, 0, 0));

        var union = Csg.Union(a, b);

        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volB = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        double volUnion = VolumeCalculator.ComputeAbsoluteVolume(union.Mesh);

        Assert.True(System.Math.Abs(volUnion - (volA + volB)) < 0.01,
            $"Disjoint union volume should be sum. Expected {volA + volB}, got {volUnion}.");
    }
}
