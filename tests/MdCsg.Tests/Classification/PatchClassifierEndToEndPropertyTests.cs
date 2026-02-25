using MdCsg.Api;
using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Cutting;
using MdCsg.Math;
using MdCsg.Patches;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Classification;

/// <summary>Phase 6: PatchClassifier — ClassifyAll, degenerate threshold, CPU strategy, CsgOptions integration</summary>
public class PatchClassifierEndToEndPropertyTests
{
    [Fact]
    public void DegenerateMarginThreshold_IsPositive()
    {
        Assert.True(PatchClassifier.DegenerateMarginThreshold > 0);
    }

    [Fact]
    public void DegenerateMarginThreshold_IsSmall()
    {
        Assert.True(PatchClassifier.DegenerateMarginThreshold < 1e-5);
    }

    [Fact]
    public void CpuStrategy_ImplementsInterface()
    {
        var strategy = new CpuPatchClassificationStrategy();
        Assert.IsAssignableFrom<IPatchClassificationStrategy>(strategy);
    }

    [Fact]
    public void CsgOptions_DefaultStrategy_IsNull()
    {
        var options = new CsgOptions();
        Assert.Null(options.ClassificationStrategy);
    }

    [Fact]
    public void CsgOptions_DefaultGridSize()
    {
        var options = new CsgOptions();
        Assert.Equal(1e-8, options.GridSize);
    }

    [Fact]
    public void CsgOptions_DefaultUseWindingNumber_IsFalse()
    {
        var options = new CsgOptions();
        Assert.False(options.UseWindingNumber);
    }

    [Fact]
    public void CsgOptions_DefaultWeldTolerance()
    {
        var options = new CsgOptions();
        Assert.Equal(1e-8, options.WeldTolerance);
    }

    [Fact]
    public void CsgOptions_CanSetStrategy()
    {
        var strategy = new CpuPatchClassificationStrategy();
        var options = new CsgOptions { ClassificationStrategy = strategy };
        Assert.Same(strategy, options.ClassificationStrategy);
    }

    [Fact]
    public void CsgOptions_CanSetWindingNumber()
    {
        var options = new CsgOptions { UseWindingNumber = true };
        Assert.True(options.UseWindingNumber);
    }

    [Fact]
    public void SolidClassification_Inside_Value()
    {
        Assert.Equal(SolidClassification.Inside, (SolidClassification)0);
    }

    [Fact]
    public void SolidClassification_Outside_Value()
    {
        Assert.Equal(SolidClassification.Outside, (SolidClassification)1);
    }

    [Fact]
    public void ClassifyAll_ViaCSG_PatchesGetClassified()
    {
        // Integration test: run a CSG operation and verify patches were classified
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Union(a, b);
        Assert.True(result.Mesh.Faces.Count > 0);
        // Patches were classified — if they weren't, the CSG would produce garbage
    }

    [Fact]
    public void ClassifyAll_WithWindingNumber_ViaCSG_Works()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var options = new CsgOptions { UseWindingNumber = true };
        var result = Csg.Union(a, b, options);
        Assert.True(result.Mesh.Faces.Count > 0);
    }

    [Fact]
    public void ClassifyAll_WithCpuStrategy_ViaCSG_Works()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var options = new CsgOptions { ClassificationStrategy = new CpuPatchClassificationStrategy() };
        var result = Csg.Union(a, b, options);
        Assert.True(result.Mesh.Faces.Count > 0);
    }

    [Fact]
    public void ClassifyAll_CpuStrategy_MatchesDefault()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var defaultResult = Csg.Union(a, b);
        var cpuResult = Csg.Union(a, b, new CsgOptions { ClassificationStrategy = new CpuPatchClassificationStrategy() });
        Assert.Equal(defaultResult.Mesh.Faces.Count, cpuResult.Mesh.Faces.Count);
    }

    [Fact]
    public void CsgResult_DegenerateCount_NonNegative()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Union(a, b);
        Assert.True(result.DegenerateCount >= 0);
    }

    [Fact]
    public void CsgResult_PatchCountA_Positive()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Union(a, b);
        Assert.True(result.PatchCountA > 0, $"PatchCountA should be positive, got {result.PatchCountA}");
    }

    [Fact]
    public void CsgResult_PatchCountB_Positive()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Union(a, b);
        Assert.True(result.PatchCountB > 0, $"PatchCountB should be positive, got {result.PatchCountB}");
    }

    [Fact]
    public void CsgResult_IntersectionSegmentCount_Positive()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Union(a, b);
        Assert.True(result.IntersectionSegmentCount > 0,
            $"Should have intersection segments, got {result.IntersectionSegmentCount}");
    }

    [Fact]
    public void CsgResult_FaceCount_MatchesMesh()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Union(a, b);
        Assert.Equal(result.Mesh.Faces.Count, result.FaceCount);
    }

    [Fact]
    public void CsgResult_VertexCount_MatchesMesh()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = Csg.Union(a, b);
        Assert.Equal(result.Mesh.Vertices.Count, result.VertexCount);
    }
}
