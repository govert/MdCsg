using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG pipeline metadata — CsgResult fields, patch counts, intersection segments</summary>
public class CsgPipelineMetadataTests
{
    [Fact]
    public void Union_HasFaces()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
        Assert.True(result.VertexCount > 0);
    }

    [Fact]
    public void Union_HasPatches()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.PatchCountA > 0);
        Assert.True(result.PatchCountB > 0);
    }

    [Fact]
    public void OverlappingCubes_HasIntersectionSegments()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.IntersectionSegmentCount > 0,
            $"Overlapping cubes should have intersection segments, got {result.IntersectionSegmentCount}");
    }

    [Fact]
    public void DisjointCubes_NoIntersectionSegments()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        Assert.Equal(0, result.IntersectionSegmentCount);
    }

    [Fact]
    public void DisjointCubes_Union_FaceCount24()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        Assert.Equal(24, result.FaceCount); // 12 + 12
    }

    [Fact]
    public void DisjointCubes_Difference_FaceCount12()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var result = Csg.Difference(a, b);
        Assert.Equal(12, result.FaceCount); // only A's faces
    }

    [Fact]
    public void OverlappingCubes_Union_MoreFacesThanInput()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 12, "Union of overlapping cubes should have > 12 faces");
    }

    [Fact]
    public void Intersection_DegenerateCount_Low()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Intersect(a, b);
        // Degenerate patches should be rare for well-separated cubes
        Assert.True(result.DegenerateCount <= result.PatchCountA + result.PatchCountB);
    }

    [Fact]
    public void FaceCount_Equals_MeshFaceCount()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        Assert.Equal(result.Mesh.Faces.Count, result.FaceCount);
    }

    [Fact]
    public void VertexCount_Equals_MeshVertexCount()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        Assert.Equal(result.Mesh.Vertices.Count, result.VertexCount);
    }

    [Fact]
    public void CubeSphere_Intersection_HasPatches()
    {
        var cube = new Solid(MeshFactory.CreateCube(Vec3.Zero, 2).Mesh);
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(1, 1, 1), 0.5, 2).Mesh);
        var result = Csg.Intersect(cube, sphere);
        Assert.True(result.PatchCountA > 0);
        Assert.True(result.PatchCountB > 0);
    }

    [Fact]
    public void AllOperations_SameInputs_ProduceResults()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var union = Csg.Union(a, b);
        var intersect = Csg.Intersect(a, b);
        var diff = Csg.Difference(a, b);
        Assert.True(union.FaceCount > 0);
        Assert.True(intersect.FaceCount > 0);
        Assert.True(diff.FaceCount > 0);
    }
}
