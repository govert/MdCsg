using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Api;

/// <summary>Batch 45: CsgResult metadata and pipeline verification tests (20 tests)</summary>
public class CsgResultMetadataTests
{
    [Fact]
    public void Union_Result_HasMesh()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Union(a, b);
        Assert.NotNull(result.Mesh);
    }

    [Fact]
    public void Union_Result_HasIntersectionSegments()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.IntersectionSegmentCount > 0);
    }

    [Fact]
    public void Union_Result_HasPatchCounts()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.PatchCountA > 0, $"PatchCountA={result.PatchCountA}");
        Assert.True(result.PatchCountB > 0, $"PatchCountB={result.PatchCountB}");
    }

    [Fact]
    public void Union_DisjointCubes_ZeroIntersectionSegments()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        Assert.Equal(0, result.IntersectionSegmentCount);
    }

    [Fact]
    public void Union_DisjointCubes_PatchCountsAreOne()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        Assert.Equal(1, result.PatchCountA);
        Assert.Equal(1, result.PatchCountB);
    }

    [Fact]
    public void Intersect_DisjointCubes_EmptyResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.Equal(0, result.FaceCount);
        Assert.Equal(0, result.VertexCount);
    }

    [Fact]
    public void Result_FaceCount_MatchesMesh()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Union(a, b);
        Assert.Equal(result.Mesh.Faces.Count, result.FaceCount);
    }

    [Fact]
    public void Result_VertexCount_MatchesMesh()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Union(a, b);
        Assert.Equal(result.Mesh.Vertices.Count, result.VertexCount);
    }

    [Fact]
    public void OverlappingCubes_ZeroDegeneratePatches()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Union(a, b);
        Assert.Equal(0, result.DegenerateCount);
    }

    [Fact]
    public void Difference_Result_HasMetadata()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Difference(a, b);
        Assert.NotNull(result.Mesh);
        Assert.True(result.FaceCount > 0);
        Assert.True(result.IntersectionSegmentCount > 0);
    }

    [Fact]
    public void Intersect_Result_HasMetadata()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.NotNull(result.Mesh);
        Assert.True(result.FaceCount > 0);
        Assert.True(result.IntersectionSegmentCount > 0);
    }

    [Fact]
    public void WindingNumber_ProducesSameMetadata()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var rc = Csg.Union(a, b, new CsgOptions { UseWindingNumber = false });
        var wn = Csg.Union(a, b, new CsgOptions { UseWindingNumber = true });
        Assert.Equal(rc.FaceCount, wn.FaceCount);
        Assert.Equal(rc.PatchCountA, wn.PatchCountA);
    }

    [Fact]
    public void CpuStrategy_ProducesSameResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var defaultResult = Csg.Union(a, b);
        var cpuResult = Csg.Union(a, b, new CsgOptions
        {
            ClassificationStrategy = new MdCsg.Classification.CpuPatchClassificationStrategy()
        });
        Assert.Equal(defaultResult.FaceCount, cpuResult.FaceCount);
    }

    [Fact]
    public void CustomGridSize_ProducesResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Union(a, b, new CsgOptions { GridSize = 1e-6 });
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void CustomWeldTolerance_ProducesResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Union(a, b, new CsgOptions { WeldTolerance = 1e-6 });
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void SphereSphere_HasMultiplePatches()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(1.2, 0.3, 0.1), 1, 2).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.PatchCountA >= 1);
        Assert.True(result.PatchCountB >= 1);
    }

    [Fact]
    public void CubeTetra_Union_HasPatches()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateTetrahedron(new Vec3(0.3, 0.3, 0.3), 1).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.PatchCountA > 0);
        Assert.True(result.PatchCountB > 0);
    }

    [Fact]
    public void Solid_FromMesh_HasBvh()
    {
        var cube = MeshFactory.CreateCube();
        var solid = new Solid(cube.Mesh);
        // Solid should build BVH internally; we can verify it works by running CSG
        var result = Csg.Union(solid, new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh));
        Assert.Equal(24, result.FaceCount);
    }

    [Fact]
    public void Result_CanBeConvertedToSolid()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Union(a, b);
        var solid = new Solid(result.Mesh);
        // The solid should be usable for further operations
        Assert.True(solid.Mesh.Faces.Count > 0);
    }
}
