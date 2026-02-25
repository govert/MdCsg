using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Api;

/// <summary>Phase 6: CsgResult metadata — FaceCount, VertexCount, PatchCounts, DegenerateCount, IntersectionSegmentCount</summary>
public class CsgResultMetadataDeepPropertyTests
{
    [Fact]
    public void Union_OverlappingSpheres_FaceCountGreaterThanZero()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
        Assert.True(result.VertexCount > 0);
    }

    [Fact]
    public void Intersect_OverlappingSpheres_VertexCountPositive()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var result = Csg.Intersect(a, b);
        Assert.True(result.VertexCount > 0);
    }

    [Fact]
    public void Union_Disjoint_SegmentCountZero()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 1.0);
        var b = MeshFactory.CreateCube(new Vec3(10, 0, 0), 1.0);
        var result = Csg.Union(a, b);
        Assert.Equal(0, result.IntersectionSegmentCount);
    }

    [Fact]
    public void Intersect_Disjoint_FaceCountZero()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 1.0);
        var b = MeshFactory.CreateCube(new Vec3(10, 0, 0), 1.0);
        var result = Csg.Intersect(a, b);
        Assert.Equal(0, result.FaceCount);
    }

    [Fact]
    public void Union_Overlapping_PatchCountsPositive()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Union(a, b);
        Assert.True(result.PatchCountA > 0);
        Assert.True(result.PatchCountB > 0);
    }

    [Fact]
    public void Union_Overlapping_SegmentCountPositive()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Union(a, b);
        Assert.True(result.IntersectionSegmentCount > 0);
    }

    [Fact]
    public void FaceCount_MatchesMeshFaceCount()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var result = Csg.Union(a, b);
        Assert.Equal(result.Mesh.Faces.Count, result.FaceCount);
    }

    [Fact]
    public void VertexCount_MatchesMeshVertexCount()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var result = Csg.Union(a, b);
        Assert.Equal(result.Mesh.Vertices.Count, result.VertexCount);
    }

    [Fact]
    public void DegenerateCount_NonNegative()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var result = Csg.Union(a, b);
        Assert.True(result.DegenerateCount >= 0);
    }

    [Fact]
    public void Union_EmptyBoth_AllZero()
    {
        var a = Solid.FromTriangles(Array.Empty<Triangle3>());
        var b = Solid.FromTriangles(Array.Empty<Triangle3>());
        var result = Csg.Union(a, b);
        Assert.Equal(0, result.FaceCount);
        Assert.Equal(0, result.VertexCount);
        Assert.Equal(0, result.IntersectionSegmentCount);
        Assert.Equal(0, result.PatchCountA);
        Assert.Equal(0, result.PatchCountB);
    }

    [Fact]
    public void Difference_Overlapping_HasMesh()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateSphere(new Vec3(1, 1, 1), 0.5, 1);
        var result = Csg.Difference(a, b);
        Assert.NotNull(result.Mesh);
        Assert.True(result.FaceCount > 0);
    }
}
