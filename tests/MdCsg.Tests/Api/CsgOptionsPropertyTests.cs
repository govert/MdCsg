using MdCsg.Api;
using MdCsg.Classification;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Api;

/// <summary>Phase 6: CsgOptions — GridSize, UseWindingNumber, WeldTolerance, ClassificationStrategy effects</summary>
public class CsgOptionsPropertyTests
{
    [Fact]
    public void DefaultOptions_HasCorrectDefaults()
    {
        var options = new CsgOptions();
        Assert.Equal(1e-8, options.GridSize);
        Assert.False(options.UseWindingNumber);
        Assert.Equal(1e-8, options.WeldTolerance);
        Assert.Null(options.ClassificationStrategy);
    }

    [Fact]
    public void UseWindingNumber_ProducesFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var options = new CsgOptions { UseWindingNumber = true };
        var result = Csg.Union(a, b, options);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void CustomGridSize_ProducesFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var options = new CsgOptions { GridSize = 1e-6 };
        var result = Csg.Union(a, b, options);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void CustomWeldTolerance_ProducesFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var options = new CsgOptions { WeldTolerance = 1e-6 };
        var result = Csg.Union(a, b, options);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void ExplicitCpuStrategy_ProducesFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var options = new CsgOptions { ClassificationStrategy = new CpuPatchClassificationStrategy() };
        var result = Csg.Union(a, b, options);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersect_WithOptions_ProducesFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var options = new CsgOptions { GridSize = 1e-6, WeldTolerance = 1e-6 };
        var result = Csg.Intersect(a, b, options);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Difference_WithOptions_ProducesFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var options = new CsgOptions { GridSize = 1e-6, WeldTolerance = 1e-6 };
        var result = Csg.Difference(a, b, options);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void NullOptions_UsesDefaults()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Union(a, b, null);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void CsgOperation_Enum_HasThreeValues()
    {
        var values = Enum.GetValues(typeof(MdCsg.Operations.CsgOperation));
        Assert.Equal(3, values.Length);
    }
}
