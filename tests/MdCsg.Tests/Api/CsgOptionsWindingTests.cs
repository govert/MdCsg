using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Api;

/// <summary>Phase 6: CsgOptions — winding number mode, grid size, weld tolerance, custom classification</summary>
public class CsgOptionsWindingTests
{
    [Fact]
    public void DefaultOptions_UseRayCast()
    {
        var options = new CsgOptions();
        Assert.False(options.UseWindingNumber);
    }

    [Fact]
    public void DefaultOptions_GridSize()
    {
        var options = new CsgOptions();
        Assert.Equal(1e-8, options.GridSize);
    }

    [Fact]
    public void DefaultOptions_WeldTolerance()
    {
        var options = new CsgOptions();
        Assert.Equal(1e-8, options.WeldTolerance);
    }

    [Fact]
    public void DefaultOptions_NoClassificationStrategy()
    {
        var options = new CsgOptions();
        Assert.Null(options.ClassificationStrategy);
    }

    [Fact]
    public void WindingNumber_Union_ProducesResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var options = new CsgOptions { UseWindingNumber = true };
        var result = Csg.Union(a, b, options);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void WindingNumber_Intersection_ProducesResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var options = new CsgOptions { UseWindingNumber = true };
        var result = Csg.Intersect(a, b, options);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void WindingNumber_Difference_ProducesResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var options = new CsgOptions { UseWindingNumber = true };
        var result = Csg.Difference(a, b, options);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void WindingAndRayCast_SameFaceCount_Union()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var resultRC = Csg.Union(a, b);
        var resultWN = Csg.Union(a, b, new CsgOptions { UseWindingNumber = true });
        Assert.Equal(resultRC.FaceCount, resultWN.FaceCount);
    }

    [Fact]
    public void WindingAndRayCast_SameFaceCount_Intersection()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var resultRC = Csg.Intersect(a, b);
        var resultWN = Csg.Intersect(a, b, new CsgOptions { UseWindingNumber = true });
        Assert.Equal(resultRC.FaceCount, resultWN.FaceCount);
    }

    [Fact]
    public void CustomGridSize_Accepted()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var options = new CsgOptions { GridSize = 1e-6 };
        var result = Csg.Union(a, b, options);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void CustomWeldTolerance_Accepted()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var options = new CsgOptions { WeldTolerance = 1e-6 };
        var result = Csg.Union(a, b, options);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Disjoint_WindingNumber_Union()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var options = new CsgOptions { UseWindingNumber = true };
        var result = Csg.Union(a, b, options);
        Assert.Equal(a.Mesh.Faces.Count + b.Mesh.Faces.Count, result.FaceCount);
    }

    [Fact]
    public void Disjoint_WindingNumber_Intersection_Empty()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var options = new CsgOptions { UseWindingNumber = true };
        var result = Csg.Intersect(a, b, options);
        Assert.Equal(0, result.FaceCount);
    }

    [Fact]
    public void SphereSphere_WindingNumber_Union()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0, 0), 1.0, 2).Mesh);
        var options = new CsgOptions { UseWindingNumber = true };
        var result = Csg.Union(a, b, options);
        Assert.True(result.FaceCount > 0);
    }
}
