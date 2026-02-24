using MdCsg.Api;
using MdCsg.Classification;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Api;

/// <summary>Batch 26: Solid, CsgOptions, CsgResult tests (20 tests)</summary>
public class SolidCsgOptionsTests
{
    [Fact]
    public void Solid_FromTriangles_CreatesMesh()
    {
        var tris = new[]
        {
            new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY),
            new Triangle3(Vec3.UnitX, new Vec3(1, 1, 0), Vec3.UnitY),
        };
        var solid = Solid.FromTriangles(tris);
        Assert.Equal(2, solid.Mesh.Faces.Count);
    }

    [Fact]
    public void Solid_FromTriangles_BuildsBvh()
    {
        var tris = new[]
        {
            new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY),
        };
        var solid = Solid.FromTriangles(tris);
        Assert.True(solid.Bvh.NodeCount >= 1);
    }

    [Fact]
    public void Solid_FromIndexed_CreatesMesh()
    {
        var positions = new Vec3[] { Vec3.Zero, Vec3.UnitX, Vec3.UnitY, new Vec3(1, 1, 0) };
        var triangles = new[] { (0, 1, 2), (1, 3, 2) };
        var solid = Solid.FromIndexed(positions, triangles);
        Assert.Equal(2, solid.Mesh.Faces.Count);
    }

    [Fact]
    public void Solid_Bounds_ContainsMesh()
    {
        var cube = MeshFactory.CreateCube();
        var solid = new Solid(cube.Mesh);
        var bounds = solid.Bounds;
        Assert.True(bounds.Min.X <= 0);
        Assert.True(bounds.Max.X >= 1);
    }

    [Fact]
    public void Solid_FromMeshFactory_HasBvh()
    {
        var cube = MeshFactory.CreateCube();
        var solid = new Solid(cube.Mesh);
        Assert.True(solid.Bvh.NodeCount > 0);
    }

    [Fact]
    public void CsgOptions_DefaultGridSize()
    {
        var opts = new CsgOptions();
        Assert.Equal(1e-8, opts.GridSize);
    }

    [Fact]
    public void CsgOptions_DefaultWeldTolerance()
    {
        var opts = new CsgOptions();
        Assert.Equal(1e-8, opts.WeldTolerance);
    }

    [Fact]
    public void CsgOptions_DefaultUseWindingNumber_False()
    {
        var opts = new CsgOptions();
        Assert.False(opts.UseWindingNumber);
    }

    [Fact]
    public void CsgOptions_DefaultClassificationStrategy_Null()
    {
        var opts = new CsgOptions();
        Assert.Null(opts.ClassificationStrategy);
    }

    [Fact]
    public void CsgOptions_SetGridSize()
    {
        var opts = new CsgOptions { GridSize = 1e-6 };
        Assert.Equal(1e-6, opts.GridSize);
    }

    [Fact]
    public void CsgOptions_SetUseWindingNumber()
    {
        var opts = new CsgOptions { UseWindingNumber = true };
        Assert.True(opts.UseWindingNumber);
    }

    [Fact]
    public void CsgOptions_SetClassificationStrategy()
    {
        var strategy = new CpuPatchClassificationStrategy();
        var opts = new CsgOptions { ClassificationStrategy = strategy };
        Assert.Same(strategy, opts.ClassificationStrategy);
    }

    [Fact]
    public void CsgResult_FaceCount_MatchesMesh()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var solidA = new Solid(cubeA.Mesh);
        var solidB = new Solid(cubeB.Mesh);
        var result = Csg.Union(solidA, solidB);
        Assert.Equal(result.Mesh.Faces.Count, result.FaceCount);
    }

    [Fact]
    public void CsgResult_VertexCount_MatchesMesh()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var solidA = new Solid(cubeA.Mesh);
        var solidB = new Solid(cubeB.Mesh);
        var result = Csg.Union(solidA, solidB);
        Assert.Equal(result.Mesh.Vertices.Count, result.VertexCount);
    }

    [Fact]
    public void CsgResult_HasPatchCounts()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var solidA = new Solid(cubeA.Mesh);
        var solidB = new Solid(cubeB.Mesh);
        var result = Csg.Union(solidA, solidB);
        Assert.True(result.PatchCountA > 0);
        Assert.True(result.PatchCountB > 0);
    }

    [Fact]
    public void CsgResult_IntersectionSegmentCount()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var solidA = new Solid(cubeA.Mesh);
        var solidB = new Solid(cubeB.Mesh);
        var result = Csg.Union(solidA, solidB);
        Assert.True(result.IntersectionSegmentCount > 0);
    }

    [Fact]
    public void CsgResult_DisjointCubes_NoIntersections()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(5, 0, 0));
        var solidA = new Solid(cubeA.Mesh);
        var solidB = new Solid(cubeB.Mesh);
        var result = Csg.Union(solidA, solidB);
        Assert.Equal(0, result.IntersectionSegmentCount);
    }

    [Fact]
    public void Solid_FromTriangles_CustomTolerance()
    {
        var tris = new[]
        {
            new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY),
        };
        var solid = Solid.FromTriangles(tris, 1e-12);
        Assert.Equal(1, solid.Mesh.Faces.Count);
    }

    [Fact]
    public void Solid_FromIndexed_CustomTolerance()
    {
        var positions = new Vec3[] { Vec3.Zero, Vec3.UnitX, Vec3.UnitY };
        var triangles = new[] { (0, 1, 2) };
        var solid = Solid.FromIndexed(positions, triangles, 1e-12);
        Assert.Equal(1, solid.Mesh.Faces.Count);
    }

    [Fact]
    public void CsgOptions_SetWeldTolerance()
    {
        var opts = new CsgOptions { WeldTolerance = 1e-6 };
        Assert.Equal(1e-6, opts.WeldTolerance);
    }
}
