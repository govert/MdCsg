using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Api;

/// <summary>Phase 6: CsgOptions GridSize effect — different grid sizes, weld tolerances, classification strategy</summary>
public class CsgOptionsGridSizeEffectTests
{
    [Fact]
    public void DefaultOptions_ProducesResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var result = Csg.Union(a, b, new CsgOptions());
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void SmallGridSize_ProducesResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var result = Csg.Union(a, b, new CsgOptions { GridSize = 1e-12 });
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void LargeGridSize_ProducesResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var result = Csg.Union(a, b, new CsgOptions { GridSize = 1e-4 });
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void WindingNumber_ProducesResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var result = Csg.Union(a, b, new CsgOptions { UseWindingNumber = true });
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void DefaultVsSmallGrid_SimilarFaceCount()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);

        var r1 = Csg.Union(a, b, new CsgOptions { GridSize = 1e-8 });
        var r2 = Csg.Union(a, b, new CsgOptions { GridSize = 1e-12 });

        // Face counts should be similar (grid size mainly affects snap precision)
        Assert.True(System.Math.Abs(r1.FaceCount - r2.FaceCount) <= 5,
            $"Grid 1e-8: {r1.FaceCount} faces, Grid 1e-12: {r2.FaceCount} faces");
    }

    [Fact]
    public void WindingVsRayCast_SimilarVolume()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);

        var rRay = Csg.Union(a, b, new CsgOptions { UseWindingNumber = false });
        var rWind = Csg.Union(a, b, new CsgOptions { UseWindingNumber = true });

        double volRay = VolumeCalculator.ComputeAbsoluteVolume(rRay.Mesh);
        double volWind = VolumeCalculator.ComputeAbsoluteVolume(rWind.Mesh);

        Assert.True(System.Math.Abs(volRay - volWind) < 0.1,
            $"Ray: {volRay:F4}, Winding: {volWind:F4}");
    }

    [Fact]
    public void Options_WeldTolerance_Default()
    {
        var opts = new CsgOptions();
        Assert.Equal(1e-8, opts.WeldTolerance);
    }

    [Fact]
    public void Options_GridSize_Default()
    {
        var opts = new CsgOptions();
        Assert.Equal(1e-8, opts.GridSize);
    }

    [Fact]
    public void Options_UseWindingNumber_DefaultFalse()
    {
        var opts = new CsgOptions();
        Assert.False(opts.UseWindingNumber);
    }

    [Fact]
    public void Options_ClassificationStrategy_DefaultNull()
    {
        var opts = new CsgOptions();
        Assert.Null(opts.ClassificationStrategy);
    }

    [Fact]
    public void Intersection_WithOptions_Produces()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var result = Csg.Intersect(a, b, new CsgOptions());
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Difference_WithOptions_Produces()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var result = Csg.Difference(a, b, new CsgOptions());
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void DisjointCubes_AllOptions_SameResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);

        var r1 = Csg.Union(a, b);
        var r2 = Csg.Union(a, b, new CsgOptions { UseWindingNumber = true });
        var r3 = Csg.Union(a, b, new CsgOptions { GridSize = 1e-12 });

        // Disjoint cubes: result should always be 24 faces (12 + 12)
        Assert.Equal(24, r1.FaceCount);
        Assert.Equal(24, r2.FaceCount);
        Assert.Equal(24, r3.FaceCount);
    }
}
