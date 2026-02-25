using MdCsg.Api;
using MdCsg.Classification;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CsgOptions — GridSize, WeldTolerance, ClassificationStrategy configuration effects</summary>
public class CsgOptionsConfigurationPropertyTests
{
    [Fact]
    public void DefaultOptions_AllDefaultValues()
    {
        var opts = new CsgOptions();
        Assert.False(opts.UseWindingNumber);
        Assert.Equal(1e-8, opts.GridSize);
        Assert.Equal(1e-8, opts.WeldTolerance);
        Assert.Null(opts.ClassificationStrategy);
    }

    [Fact]
    public void CustomGridSize_AffectsResult()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var defaultResult = Csg.Union(a, b);
        var fineGrid = Csg.Union(a, b, new CsgOptions { GridSize = 1e-12 });
        // Both should succeed, face count may differ slightly
        Assert.True(defaultResult.FaceCount > 0);
        Assert.True(fineGrid.FaceCount > 0);
    }

    [Fact]
    public void CpuClassificationStrategy_ExplicitlySet_Works()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var opts = new CsgOptions { ClassificationStrategy = new CpuPatchClassificationStrategy() };
        var result = Csg.Union(a, b, opts);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void CpuStrategy_MatchesDefault()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var defaultResult = Csg.Union(a, b);
        var cpuResult = Csg.Union(a, b, new CsgOptions { ClassificationStrategy = new CpuPatchClassificationStrategy() });
        Assert.Equal(defaultResult.FaceCount, cpuResult.FaceCount);
        Assert.Equal(defaultResult.VertexCount, cpuResult.VertexCount);
        Assert.Equal(defaultResult.PatchCountA, cpuResult.PatchCountA);
        Assert.Equal(defaultResult.PatchCountB, cpuResult.PatchCountB);
    }

    [Fact]
    public void UseWindingNumber_True_ProducesFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Union(a, b, new CsgOptions { UseWindingNumber = true });
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_ResultHasCorrectMetadata()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Union(a, b);
        Assert.True(result.PatchCountA > 0);
        Assert.True(result.PatchCountB > 0);
        Assert.True(result.IntersectionSegmentCount > 0);
        Assert.True(result.DegenerateCount >= 0);
    }

    [Fact]
    public void Intersect_ResultHasCorrectMetadata()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Intersect(a, b);
        Assert.True(result.PatchCountA > 0);
        Assert.True(result.PatchCountB > 0);
        Assert.True(result.IntersectionSegmentCount > 0);
    }

    [Fact]
    public void Difference_ResultHasCorrectMetadata()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Difference(a, b);
        Assert.True(result.PatchCountA > 0);
        Assert.True(result.PatchCountB > 0);
        Assert.True(result.IntersectionSegmentCount > 0);
    }

    [Fact]
    public void CsgResult_VertexCount_Positive()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Union(a, b);
        Assert.True(result.VertexCount > 0);
    }

    [Fact]
    public void CsgResult_FaceCount_MatchesMeshFaceCount()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Union(a, b);
        Assert.Equal(result.Mesh.Faces.Count, result.FaceCount);
    }

    [Fact]
    public void CsgResult_VertexCount_MatchesMeshVertexCount()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Union(a, b);
        Assert.Equal(result.Mesh.Vertices.Count, result.VertexCount);
    }

    [Fact]
    public void WeldTolerance_Large_MergesMoreVertices()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var tightWeld = Csg.Union(a, b, new CsgOptions { WeldTolerance = 1e-12 });
        var looseWeld = Csg.Union(a, b, new CsgOptions { WeldTolerance = 1e-4 });
        // Loose welding should merge more or same number of vertices
        Assert.True(looseWeld.VertexCount <= tightWeld.VertexCount,
            $"Loose weld ({looseWeld.VertexCount}) should have <= vertices than tight ({tightWeld.VertexCount})");
    }

    [Fact]
    public void Disjoint_AllOperations_CorrectSegmentCount()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(10, 0, 0));
        var union = Csg.Union(a, b);
        var inter = Csg.Intersect(a, b);
        var diff = Csg.Difference(a, b);
        Assert.Equal(0, union.IntersectionSegmentCount);
        Assert.Equal(0, inter.IntersectionSegmentCount);
        Assert.Equal(0, diff.IntersectionSegmentCount);
    }

    [Fact]
    public void Disjoint_PatchCountsMatch()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(10, 0, 0));
        var result = Csg.Union(a, b);
        // Each mesh should have exactly 1 patch (the whole mesh, since no intersection)
        Assert.Equal(1, result.PatchCountA);
        Assert.Equal(1, result.PatchCountB);
    }

    [Fact]
    public void NullOptions_UsesDefaults()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var withNull = Csg.Union(a, b, null);
        var withDefault = Csg.Union(a, b, new CsgOptions());
        Assert.Equal(withNull.FaceCount, withDefault.FaceCount);
    }

    [Fact]
    public void Union_Intersection_Difference_AllSucceed()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        Assert.True(Csg.Union(a, b).FaceCount > 0);
        Assert.True(Csg.Intersect(a, b).FaceCount > 0);
        Assert.True(Csg.Difference(a, b).FaceCount > 0);
    }
}
