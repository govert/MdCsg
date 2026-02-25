using MdCsg.Api;
using MdCsg.Classification;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Api;

/// <summary>Phase 6: CsgOptions edge cases — extreme values, custom strategies, property combinations</summary>
public class CsgOptionsEdgeCaseTests
{
    [Fact]
    public void DefaultOptions_GridSize_1e8()
    {
        var opts = new CsgOptions();
        Assert.Equal(1e-8, opts.GridSize);
    }

    [Fact]
    public void DefaultOptions_WeldTolerance_1e8()
    {
        var opts = new CsgOptions();
        Assert.Equal(1e-8, opts.WeldTolerance);
    }

    [Fact]
    public void DefaultOptions_UseWindingNumber_False()
    {
        var opts = new CsgOptions();
        Assert.False(opts.UseWindingNumber);
    }

    [Fact]
    public void DefaultOptions_ClassificationStrategy_Null()
    {
        var opts = new CsgOptions();
        Assert.Null(opts.ClassificationStrategy);
    }

    [Fact]
    public void CustomGridSize_UsedInCsg()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var opts = new CsgOptions { GridSize = 1e-6 };
        var result = Csg.Union(a, b, opts);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void LargeGridSize_StillProducesResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var opts = new CsgOptions { GridSize = 0.01 };
        var result = Csg.Union(a, b, opts);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void SmallGridSize_StillProducesResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var opts = new CsgOptions { GridSize = 1e-12 };
        var result = Csg.Union(a, b, opts);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void CustomWeldTolerance_StillProducesResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var opts = new CsgOptions { WeldTolerance = 1e-4 };
        var result = Csg.Union(a, b, opts);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void UseWindingNumber_True_ProducesResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var opts = new CsgOptions { UseWindingNumber = true };
        var result = Csg.Union(a, b, opts);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void ExplicitCpuStrategy_SameAsDefault()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var defaultResult = Csg.Union(a, b);
        var explicitResult = Csg.Union(a, b, new CsgOptions
        {
            ClassificationStrategy = new CpuPatchClassificationStrategy()
        });
        Assert.Equal(defaultResult.FaceCount, explicitResult.FaceCount);
    }

    [Fact]
    public void AllOptionsSet_ProducesResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var opts = new CsgOptions
        {
            GridSize = 1e-7,
            WeldTolerance = 1e-7,
            UseWindingNumber = true,
            ClassificationStrategy = new CpuPatchClassificationStrategy()
        };
        var result = Csg.Intersect(a, b, opts);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void NullOptions_UsesDefaults()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        // Passing null options should use defaults
        var result = Csg.Union(a, b, null);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Difference_WithWindingNumber_ProducesResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var opts = new CsgOptions { UseWindingNumber = true };
        var result = Csg.Difference(a, b, opts);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersect_WithWindingNumber_ProducesResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var opts = new CsgOptions { UseWindingNumber = true };
        var result = Csg.Intersect(a, b, opts);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void GridSize_Settable()
    {
        var opts = new CsgOptions { GridSize = 0.123 };
        Assert.Equal(0.123, opts.GridSize);
    }

    [Fact]
    public void WeldTolerance_Settable()
    {
        var opts = new CsgOptions { WeldTolerance = 0.456 };
        Assert.Equal(0.456, opts.WeldTolerance);
    }
}
