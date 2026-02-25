using MdCsg.Api;
using MdCsg.Classification;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Api;

/// <summary>Phase 6: CsgOptions comprehensive property and integration tests</summary>
public class CsgOptionsPropertyTests
{
    [Fact]
    public void Default_GridSize()
    {
        var opts = new CsgOptions();
        Assert.Equal(1e-8, opts.GridSize);
    }

    [Fact]
    public void Default_UseWindingNumber_False()
    {
        var opts = new CsgOptions();
        Assert.False(opts.UseWindingNumber);
    }

    [Fact]
    public void Default_WeldTolerance()
    {
        var opts = new CsgOptions();
        Assert.Equal(1e-8, opts.WeldTolerance);
    }

    [Fact]
    public void Default_ClassificationStrategy_Null()
    {
        var opts = new CsgOptions();
        Assert.Null(opts.ClassificationStrategy);
    }

    [Fact]
    public void CustomGridSize_Applied()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Union(a, b, new CsgOptions { GridSize = 1e-6 });
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void UseWindingNumber_Applied()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Union(a, b, new CsgOptions { UseWindingNumber = true });
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void CpuStrategy_Explicit_Works()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var opts = new CsgOptions { ClassificationStrategy = new CpuPatchClassificationStrategy() };
        var result = Csg.Union(a, b, opts);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void CustomWeldTolerance_Applied()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Union(a, b, new CsgOptions { WeldTolerance = 1e-6 });
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void AllOptionsSet_Works()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var opts = new CsgOptions
        {
            GridSize = 1e-7,
            UseWindingNumber = true,
            WeldTolerance = 1e-7,
            ClassificationStrategy = new CpuPatchClassificationStrategy()
        };
        var result = Csg.Union(a, b, opts);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void IPatchClassificationStrategy_InterfaceContract()
    {
        var strategy = new CpuPatchClassificationStrategy();
        Assert.IsAssignableFrom<IPatchClassificationStrategy>(strategy);
    }
}
