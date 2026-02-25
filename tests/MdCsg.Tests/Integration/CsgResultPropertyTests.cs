using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CsgResult — IntersectionSegmentCount, DegenerateCount, patch counts for various inputs</summary>
public class CsgResultPropertyTests
{
    [Fact]
    public void DisjointCubes_NoIntersectionSegments()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(5, 0, 0));
        var result = Csg.Union(a, b);
        Assert.Equal(0, result.IntersectionSegmentCount);
    }

    [Fact]
    public void OverlappingCubes_HasIntersectionSegments()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Union(a, b);
        Assert.True(result.IntersectionSegmentCount > 0);
    }

    [Fact]
    public void DisjointCubes_Union_PatchCountA_EqualsOne()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(5, 0, 0));
        var result = Csg.Union(a, b);
        Assert.Equal(1, result.PatchCountA);
    }

    [Fact]
    public void DisjointCubes_Union_PatchCountB_EqualsOne()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(5, 0, 0));
        var result = Csg.Union(a, b);
        Assert.Equal(1, result.PatchCountB);
    }

    [Fact]
    public void OverlappingCubes_PatchCountA_AtLeastTwo()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Union(a, b);
        Assert.True(result.PatchCountA >= 2, $"PatchCountA should be >= 2, got {result.PatchCountA}");
    }

    [Fact]
    public void OverlappingCubes_PatchCountB_AtLeastTwo()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Union(a, b);
        Assert.True(result.PatchCountB >= 2, $"PatchCountB should be >= 2, got {result.PatchCountB}");
    }

    [Fact]
    public void DegenerateCount_NonNegative()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Union(a, b);
        Assert.True(result.DegenerateCount >= 0);
    }

    [Fact]
    public void DisjointCubes_DegenerateCount_Zero()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(5, 0, 0));
        var result = Csg.Union(a, b);
        Assert.Equal(0, result.DegenerateCount);
    }

    [Fact]
    public void CubeSphere_HasIntersectionSegments()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero);
        var sphere = MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 2);
        var result = Csg.Union(cube, sphere);
        Assert.True(result.IntersectionSegmentCount > 0);
    }

    [Fact]
    public void CubeSphere_PatchCounts_Positive()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero);
        var sphere = MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 2);
        var result = Csg.Union(cube, sphere);
        Assert.True(result.PatchCountA >= 1);
        Assert.True(result.PatchCountB >= 1);
    }

    [Fact]
    public void SameOperation_DifferentOperators_SameSegmentCount()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var union = Csg.Union(a, b);
        var inter = Csg.Intersect(a, b);
        var diff = Csg.Difference(a, b);
        // All operations on the same pair should find the same number of intersection segments
        Assert.Equal(union.IntersectionSegmentCount, inter.IntersectionSegmentCount);
        Assert.Equal(inter.IntersectionSegmentCount, diff.IntersectionSegmentCount);
    }
}
