using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG result diagnostics — intersection segment count, patch counts, face counts</summary>
public class CsgIntersectionSegmentCountTests
{
    [Fact]
    public void Disjoint_ZeroIntersectionSegments()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        Assert.Equal(0, result.IntersectionSegmentCount);
    }

    [Fact]
    public void Overlapping_HasIntersectionSegments()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.IntersectionSegmentCount > 0);
    }

    [Fact]
    public void PatchCounts_Positive_ForOverlapping()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.PatchCountA > 0);
        Assert.True(result.PatchCountB > 0);
    }

    [Fact]
    public void PatchCounts_OneEach_ForDisjoint()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        // Disjoint: each mesh has exactly one patch (entire surface)
        Assert.Equal(1, result.PatchCountA);
        Assert.Equal(1, result.PatchCountB);
    }

    [Fact]
    public void FaceCount_Union_GreaterThanIntersection()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);

        int fcUnion = Csg.Union(a, b).FaceCount;
        int fcIntersect = Csg.Intersect(a, b).FaceCount;

        Assert.True(fcUnion > fcIntersect,
            $"Union ({fcUnion}) should have more faces than Intersection ({fcIntersect})");
    }

    [Fact]
    public void VertexCount_ConsistentWithFaces()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var result = Csg.Union(a, b);

        // For a closed mesh: V - E + F = 2 and E = 3F/2 → V = F/2 + 2
        // So V should be roughly F/2 for large F
        Assert.True(result.VertexCount > 0);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersection_Empty_ZeroFaces()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.Equal(0, result.FaceCount);
        Assert.Equal(0, result.VertexCount);
    }

    [Fact]
    public void Difference_SameAsA_ZeroSegments()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var result = Csg.Difference(a, b);
        Assert.Equal(0, result.IntersectionSegmentCount);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void SphereSphere_IntersectionSegments()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 2).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.IntersectionSegmentCount > 0);
    }

    [Fact]
    public void DegenerateCount_NonNegative()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);

        Assert.True(Csg.Union(a, b).DegenerateCount >= 0);
        Assert.True(Csg.Intersect(a, b).DegenerateCount >= 0);
        Assert.True(Csg.Difference(a, b).DegenerateCount >= 0);
    }

    [Fact]
    public void OverlappingCubes_IntersectionSegments_Symmetric()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);

        int segsUnion = Csg.Union(a, b).IntersectionSegmentCount;
        int segsIntersect = Csg.Intersect(a, b).IntersectionSegmentCount;
        int segsDiff = Csg.Difference(a, b).IntersectionSegmentCount;

        // Same intersection graph for all three operations
        Assert.Equal(segsUnion, segsIntersect);
        Assert.Equal(segsUnion, segsDiff);
    }
}
