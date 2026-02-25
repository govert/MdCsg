using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG with winding number mode — verify UseWindingNumber option produces valid results</summary>
public class CsgWindingNumberModeTests
{
    private static CsgOptions WindingOptions => new CsgOptions { UseWindingNumber = true };

    [Fact]
    public void Union_WindingNumber_PositiveVolume()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Union(a, b, WindingOptions);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol > 0);
    }

    [Fact]
    public void Intersection_WindingNumber_PositiveVolume()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Intersect(a, b, WindingOptions);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol > 0);
    }

    [Fact]
    public void Difference_WindingNumber_PositiveVolume()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Difference(a, b, WindingOptions);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol > 0);
    }

    [Fact]
    public void WindingNumber_VsCastRay_SameUnionVolume()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        double volRay = VolumeCalculator.ComputeAbsoluteVolume(Csg.Union(a, b).Mesh);
        double volWinding = VolumeCalculator.ComputeAbsoluteVolume(Csg.Union(a, b, WindingOptions).Mesh);
        Assert.True(System.Math.Abs(volRay - volWinding) < 0.1,
            $"Ray={volRay}, Winding={volWinding}");
    }

    [Fact]
    public void WindingNumber_VsCastRay_SameIntersectionVolume()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        double volRay = VolumeCalculator.ComputeAbsoluteVolume(Csg.Intersect(a, b).Mesh);
        double volWinding = VolumeCalculator.ComputeAbsoluteVolume(Csg.Intersect(a, b, WindingOptions).Mesh);
        Assert.True(System.Math.Abs(volRay - volWinding) < 0.1,
            $"Ray={volRay}, Winding={volWinding}");
    }

    [Fact]
    public void WindingNumber_VsCastRay_SameDifferenceVolume()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        double volRay = VolumeCalculator.ComputeAbsoluteVolume(Csg.Difference(a, b).Mesh);
        double volWinding = VolumeCalculator.ComputeAbsoluteVolume(Csg.Difference(a, b, WindingOptions).Mesh);
        Assert.True(System.Math.Abs(volRay - volWinding) < 0.1,
            $"Ray={volRay}, Winding={volWinding}");
    }

    [Fact]
    public void Disjoint_WindingNumber_UnionVolumeIsSum()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double volB = VolumeCalculator.ComputeAbsoluteVolume(b.Mesh);
        double volUnion = VolumeCalculator.ComputeAbsoluteVolume(Csg.Union(a, b, WindingOptions).Mesh);
        Assert.True(System.Math.Abs(volUnion - (volA + volB)) < 0.01);
    }

    [Fact]
    public void FullyContained_WindingNumber_DifferenceCorrect()
    {
        var outer = new Solid(MeshFactory.CreateCube(Vec3.Zero, 2).Mesh);
        var inner = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), 0.5).Mesh);
        var result = Csg.Difference(outer, inner, WindingOptions);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(System.Math.Abs(vol - (8 - 0.125)) < 0.5);
    }

    [Fact]
    public void SphereSphere_WindingNumber_Union_PositiveVolume()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0, 0), 1.0, 2).Mesh);
        var result = Csg.Union(a, b, WindingOptions);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol > 0);
    }
}
