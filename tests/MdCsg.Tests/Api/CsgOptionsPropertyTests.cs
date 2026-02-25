using MdCsg.Api;
using MdCsg.Classification;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Api;

/// <summary>Phase 6: CsgOptions — default values, UseWindingNumber, GridSize, WeldTolerance, ClassificationStrategy</summary>
public class CsgOptionsPropertyTests
{
    [Fact]
    public void Default_ClassificationStrategy_IsNull()
    {
        var options = new CsgOptions();
        Assert.Null(options.ClassificationStrategy);
    }

    [Fact]
    public void Default_GridSize_IsSmallPositive()
    {
        var options = new CsgOptions();
        Assert.True(options.GridSize > 0);
        Assert.True(options.GridSize < 1e-3);
    }

    [Fact]
    public void Default_UseWindingNumber_False()
    {
        var options = new CsgOptions();
        Assert.False(options.UseWindingNumber);
    }

    [Fact]
    public void Default_WeldTolerance_IsSmallPositive()
    {
        var options = new CsgOptions();
        Assert.True(options.WeldTolerance > 0);
        Assert.True(options.WeldTolerance < 1e-3);
    }

    [Fact]
    public void UseWindingNumber_CanBeSet()
    {
        var options = new CsgOptions { UseWindingNumber = true };
        Assert.True(options.UseWindingNumber);
    }

    [Fact]
    public void GridSize_CanBeChanged()
    {
        var options = new CsgOptions { GridSize = 1e-6 };
        Assert.Equal(1e-6, options.GridSize);
    }

    [Fact]
    public void WeldTolerance_CanBeChanged()
    {
        var options = new CsgOptions { WeldTolerance = 1e-12 };
        Assert.Equal(1e-12, options.WeldTolerance);
    }

    [Fact]
    public void ClassificationStrategy_CanBeSet()
    {
        var strategy = new CpuPatchClassificationStrategy();
        var options = new CsgOptions { ClassificationStrategy = strategy };
        Assert.Same(strategy, options.ClassificationStrategy);
    }

    [Fact]
    public void Union_WithWindingNumber_ProducesFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Union(a, b, new CsgOptions { UseWindingNumber = true });
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersect_WithWindingNumber_ProducesFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Intersect(a, b, new CsgOptions { UseWindingNumber = true });
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Difference_WithWindingNumber_ProducesFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Difference(a, b, new CsgOptions { UseWindingNumber = true });
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_WithCustomGridSize_ProducesFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Union(a, b, new CsgOptions { GridSize = 1e-6 });
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_WithCpuStrategy_ProducesFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Union(a, b, new CsgOptions { ClassificationStrategy = new CpuPatchClassificationStrategy() });
        Assert.True(result.FaceCount > 0);
    }
}
