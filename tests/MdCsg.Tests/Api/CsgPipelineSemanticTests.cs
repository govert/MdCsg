using MdCsg.Api;
using MdCsg.Classification;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Api;

/// <summary>Semantic paths: full CSG pipeline, options, result metadata, Solid factories</summary>
public class CsgPipelineSemanticTests
{
    [Fact]
    public void CsgResult_HasAllMetadata()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var r = Csg.Union(a, b);
        Assert.True(r.FaceCount > 0);
        Assert.True(r.VertexCount > 0);
        Assert.True(r.PatchCountA > 0);
        Assert.True(r.PatchCountB > 0);
        Assert.True(r.IntersectionSegmentCount > 0);
    }

    [Fact]
    public void CsgOptions_Defaults()
    {
        var opts = new CsgOptions();
        Assert.Equal(1e-8, opts.GridSize);
        Assert.Equal(1e-8, opts.WeldTolerance);
        Assert.False(opts.UseWindingNumber);
        Assert.Null(opts.ClassificationStrategy);
    }

    [Fact]
    public void CsgOptions_CustomValues()
    {
        var strategy = new CpuPatchClassificationStrategy();
        var opts = new CsgOptions
        {
            GridSize = 1e-6,
            WeldTolerance = 1e-6,
            UseWindingNumber = true,
            ClassificationStrategy = strategy
        };
        Assert.Equal(1e-6, opts.GridSize);
        Assert.True(opts.UseWindingNumber);
        Assert.Same(strategy, opts.ClassificationStrategy);
    }

    [Fact]
    public void Union_WithWindingNumber_SameAsFaceCount()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var rc = Csg.Union(a, b, new CsgOptions { UseWindingNumber = false });
        var wn = Csg.Union(a, b, new CsgOptions { UseWindingNumber = true });
        Assert.Equal(rc.FaceCount, wn.FaceCount);
    }

    [Fact]
    public void Intersect_WithCpuStrategy_Explicit()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var opts = new CsgOptions { ClassificationStrategy = new CpuPatchClassificationStrategy() };
        var r = Csg.Intersect(a, b, opts);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void DisjointCubes_Union_SumOfFaces()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var r = Csg.Union(a, b);
        Assert.Equal(24, r.FaceCount); // 12 + 12
        Assert.Equal(0, r.IntersectionSegmentCount);
    }

    [Fact]
    public void DisjointCubes_Intersect_Empty()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var r = Csg.Intersect(a, b);
        Assert.Equal(0, r.FaceCount);
    }

    [Fact]
    public void DisjointCubes_Difference_OnlyA()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var r = Csg.Difference(a, b);
        Assert.Equal(12, r.FaceCount); // just A
    }

    [Fact]
    public void Solid_FromTriangles_CreatesCube()
    {
        var cube = MeshFactory.CreateCube();
        var tris = new List<Triangle3>();
        foreach (var face in cube.Mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            tris.Add(new Triangle3(a, b, c));
        }
        var solid = Solid.FromTriangles(tris);
        Assert.Equal(12, solid.Mesh.Faces.Count);
        Assert.NotNull(solid.Bvh);
    }

    [Fact]
    public void Solid_FromIndexed_CreatesCube()
    {
        var positions = new Vec3[]
        {
            new(0, 0, 0), new(1, 0, 0), new(1, 1, 0), new(0, 1, 0),
            new(0, 0, 1), new(1, 0, 1), new(1, 1, 1), new(0, 1, 1)
        };
        var indices = new (int, int, int)[]
        {
            (0,2,1), (0,3,2), (4,5,6), (4,6,7),
            (0,1,5), (0,5,4), (2,3,7), (2,7,6),
            (1,2,6), (1,6,5), (0,4,7), (0,7,3)
        };
        var solid = Solid.FromIndexed(positions, indices);
        Assert.Equal(12, solid.Mesh.Faces.Count);
    }

    [Fact]
    public void Solid_Bounds()
    {
        var solid = new Solid(MeshFactory.CreateCube().Mesh);
        var bounds = solid.Bounds;
        Assert.True(bounds.Min.X >= -0.01);
        Assert.True(bounds.Max.X <= 1.01);
    }

    [Fact]
    public void Solid_Bvh_NotNull()
    {
        var solid = new Solid(MeshFactory.CreateCube().Mesh);
        Assert.True(solid.Bvh.NodeCount > 0);
    }

    [Fact]
    public void Union_CustomGridSize_Works()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var r = Csg.Union(a, b, new CsgOptions { GridSize = 1e-6 });
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void Union_CustomWeldTolerance_Works()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var r = Csg.Union(a, b, new CsgOptions { WeldTolerance = 1e-6 });
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void Difference_PatchCountB_PositiveForOverlap()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var r = Csg.Difference(a, b);
        Assert.True(r.PatchCountB > 0);
    }

    [Fact]
    public void Intersect_DegenerateCount_ZeroForDiagonal()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        Assert.Equal(0, Csg.Intersect(a, b).DegenerateCount);
    }

    [Fact]
    public void CsgResult_VertexCount_Positive()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var r = Csg.Union(a, b);
        Assert.True(r.VertexCount > 0);
    }
}
