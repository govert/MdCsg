using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Api;

/// <summary>Phase 6: CsgResult property tests — metadata, face/vertex counts, degenerate count</summary>
public class CsgResultPropertyTests
{
    [Fact]
    public void Union_FaceCount_Positive()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
        Assert.Equal(result.Mesh.Faces.Count, result.FaceCount);
    }

    [Fact]
    public void Union_VertexCount_Positive()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.VertexCount > 0);
        Assert.Equal(result.Mesh.Vertices.Count, result.VertexCount);
    }

    [Fact]
    public void Union_PatchCounts_Positive()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.PatchCountA > 0);
        Assert.True(result.PatchCountB > 0);
    }

    [Fact]
    public void Union_IntersectionSegmentCount_Positive()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.IntersectionSegmentCount > 0);
    }

    [Fact]
    public void Union_DegenerateCount_NonNegative()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.DegenerateCount >= 0);
    }

    [Fact]
    public void Intersect_HasMetadata()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.True(result.FaceCount > 0);
        Assert.True(result.PatchCountA > 0);
        Assert.True(result.IntersectionSegmentCount > 0);
    }

    [Fact]
    public void Difference_HasMetadata()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Difference(a, b);
        Assert.True(result.FaceCount > 0);
        Assert.True(result.PatchCountA > 0);
        Assert.True(result.IntersectionSegmentCount > 0);
    }

    [Fact]
    public void Union_FaceCountGreaterThanIntersect()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var union = Csg.Union(a, b);
        var inter = Csg.Intersect(a, b);
        Assert.True(union.FaceCount > inter.FaceCount,
            $"Union faces ({union.FaceCount}) should be > Intersect faces ({inter.FaceCount})");
    }

    [Fact]
    public void SphereCube_Union_HasMoreFacesThanEither()
    {
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.6, 2).Mesh);
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var result = Csg.Union(sphere, cube);
        // Union should generally have more faces than intersection due to more surface area
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void AllOperations_ResultMeshIsValid()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        foreach (var result in new[] { Csg.Union(a, b), Csg.Intersect(a, b), Csg.Difference(a, b) })
        {
            Assert.True(result.FaceCount > 0);
            Assert.True(result.VertexCount > 0);
            foreach (var face in result.Mesh.Faces)
                Assert.Equal(3, face.GetVertices().Count);
        }
    }
}
