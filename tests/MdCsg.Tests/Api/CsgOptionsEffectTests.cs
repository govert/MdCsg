using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Api;

/// <summary>Phase 6: CsgOptions — effect of GridSize, UseWindingNumber, WeldTolerance on results</summary>
public class CsgOptionsEffectTests
{
    private static double Vol(CsgResult r) => VolumeCalculator.ComputeAbsoluteVolume(r.Mesh);

    [Fact]
    public void DefaultOptions_ProducesValidResult()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2));
        var result = Csg.Union(a, b, new CsgOptions());
        Assert.True(result.FaceCount > 0);
        Assert.True(Vol(result) > 0);
    }

    [Fact]
    public void UseWindingNumber_ProducesResult()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2));
        var options = new CsgOptions { UseWindingNumber = true };
        var result = Csg.Union(a, b, options);
        Assert.True(result.FaceCount > 0);
        Assert.True(Vol(result) > 0);
    }

    [Fact]
    public void WindingNumber_VsRayCast_SimilarVolume()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2));

        var rayCastResult = Csg.Union(a, b, new CsgOptions { UseWindingNumber = false });
        // Use fresh objects for winding number to avoid state issues
        var a2 = MeshFactory.CreateCube();
        var b2 = MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2));
        var windingResult = Csg.Union(a2, b2, new CsgOptions { UseWindingNumber = true });

        double volRay = Vol(rayCastResult);
        double volWind = Vol(windingResult);

        Assert.True(System.Math.Abs(volRay - volWind) < 0.3,
            $"Ray cast ({volRay:F4}) and winding number ({volWind:F4}) should give similar volume");
    }

    [Fact]
    public void GridSize_Default_Is1eN8()
    {
        var options = new CsgOptions();
        Assert.Equal(1e-8, options.GridSize);
    }

    [Fact]
    public void WeldTolerance_Default_Is1eN8()
    {
        var options = new CsgOptions();
        Assert.Equal(1e-8, options.WeldTolerance);
    }

    [Fact]
    public void UseWindingNumber_Default_IsFalse()
    {
        var options = new CsgOptions();
        Assert.False(options.UseWindingNumber);
    }

    [Fact]
    public void ClassificationStrategy_Default_IsNull()
    {
        var options = new CsgOptions();
        Assert.Null(options.ClassificationStrategy);
    }

    [Fact]
    public void Intersection_WithWindingNumber()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var b = MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5));
        var options = new CsgOptions { UseWindingNumber = true };
        var result = Csg.Intersect(a, b, options);
        Assert.True(result.FaceCount > 0);
        Assert.True(Vol(result) > 0);
    }

    [Fact]
    public void Difference_WithWindingNumber()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2));
        var options = new CsgOptions { UseWindingNumber = true };
        var result = Csg.Difference(a, b, options);
        Assert.True(result.FaceCount > 0);
        Assert.True(Vol(result) > 0);
    }

    [Fact]
    public void Result_IntersectionSegmentCount_Positive_ForOverlapping()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2));
        var result = Csg.Union(a, b);
        Assert.True(result.IntersectionSegmentCount > 0,
            $"Overlapping cubes should have intersection segments, got {result.IntersectionSegmentCount}");
    }

    [Fact]
    public void Result_IntersectionSegmentCount_Zero_ForDisjoint()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(5, 0, 0));
        var result = Csg.Union(a, b);
        Assert.Equal(0, result.IntersectionSegmentCount);
    }

    [Fact]
    public void Result_PatchCounts_Positive()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2));
        var result = Csg.Union(a, b);
        Assert.True(result.PatchCountA > 0, $"PatchCountA should be positive");
        Assert.True(result.PatchCountB > 0, $"PatchCountB should be positive");
    }
}
