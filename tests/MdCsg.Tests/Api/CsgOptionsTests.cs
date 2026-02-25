using MdCsg.Api;
using MdCsg.Classification;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Api;

/// <summary>Phase 6: CsgOptions tests — option defaults and behavior</summary>
public class CsgOptionsTests
{
    [Fact]
    public void Defaults_GridSize()
    {
        var opts = new CsgOptions();
        Assert.Equal(1e-8, opts.GridSize);
    }

    [Fact]
    public void Defaults_UseWindingNumber_False()
    {
        var opts = new CsgOptions();
        Assert.False(opts.UseWindingNumber);
    }

    [Fact]
    public void Defaults_WeldTolerance()
    {
        var opts = new CsgOptions();
        Assert.Equal(1e-8, opts.WeldTolerance);
    }

    [Fact]
    public void Defaults_ClassificationStrategy_Null()
    {
        var opts = new CsgOptions();
        Assert.Null(opts.ClassificationStrategy);
    }

    [Fact]
    public void WithWindingNumber_ProducesResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var opts = new CsgOptions { UseWindingNumber = true };
        var result = Csg.Union(a, b, opts);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void WithWindingNumber_SameFaceCountAsDefault()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var resultDefault = Csg.Union(a, b);
        var resultWinding = Csg.Union(a, b, new CsgOptions { UseWindingNumber = true });
        Assert.Equal(resultDefault.FaceCount, resultWinding.FaceCount);
    }

    [Fact]
    public void WithCpuStrategy_SameAsDefault()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var resultDefault = Csg.Union(a, b);
        var resultCpu = Csg.Union(a, b, new CsgOptions
        {
            ClassificationStrategy = new CpuPatchClassificationStrategy()
        });
        Assert.Equal(resultDefault.FaceCount, resultCpu.FaceCount);
    }

    [Fact]
    public void WithCustomGridSize_ProducesResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var opts = new CsgOptions { GridSize = 1e-6 };
        var result = Csg.Union(a, b, opts);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void WithCustomWeldTolerance_ProducesResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var opts = new CsgOptions { WeldTolerance = 1e-6 };
        var result = Csg.Union(a, b, opts);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void AllOptions_CombinedWork()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var opts = new CsgOptions
        {
            UseWindingNumber = true,
            GridSize = 1e-6,
            WeldTolerance = 1e-6,
            ClassificationStrategy = new CpuPatchClassificationStrategy()
        };
        var result = Csg.Union(a, b, opts);
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
    public void Difference_WithWindingNumber_ProducesResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var opts = new CsgOptions { UseWindingNumber = true };
        var result = Csg.Difference(a, b, opts);
        Assert.True(result.FaceCount > 0);
    }
}
