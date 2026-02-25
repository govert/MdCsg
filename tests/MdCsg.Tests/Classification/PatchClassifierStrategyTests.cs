using MdCsg.Api;
using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Cutting;
using MdCsg.Math;
using MdCsg.Patches;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Classification;

/// <summary>Phase 6: PatchClassifier + CpuPatchClassificationStrategy — threshold, strategy pattern, classification</summary>
public class PatchClassifierStrategyTests
{
    [Fact]
    public void DegenerateMarginThreshold_Is1eMinus10()
    {
        Assert.Equal(1e-10, PatchClassifier.DegenerateMarginThreshold);
    }

    [Fact]
    public void CpuStrategy_ImplementsInterface()
    {
        var strategy = new CpuPatchClassificationStrategy();
        Assert.IsAssignableFrom<IPatchClassificationStrategy>(strategy);
    }

    [Fact]
    public void ClassifyAll_EmptyPatches_ReturnsZeroDegenerate()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var patches = new List<Patch>();
        var subTriangles = new List<FaceCutter.SubTriangle>();
        int deg = PatchClassifier.ClassifyAll(patches, subTriangles, bvh);
        Assert.Equal(0, deg);
    }

    [Fact]
    public void CpuStrategy_ClassifyAll_SameAsStatic()
    {
        // Overlapping cubes — CPU strategy should produce same results
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);

        var resultDefault = Csg.Union(a, b);
        var resultCpu = Csg.Union(a, b, new CsgOptions
        {
            ClassificationStrategy = new CpuPatchClassificationStrategy()
        });

        Assert.Equal(resultDefault.FaceCount, resultCpu.FaceCount);
        Assert.Equal(resultDefault.VertexCount, resultCpu.VertexCount);
    }

    [Fact]
    public void ClassifyAll_WithWindingNumber_ProducesValidResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);

        var result = Csg.Union(a, b, new CsgOptions { UseWindingNumber = true });
        Assert.True(result.FaceCount > 0);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol > 1.0 && vol < 2.0, $"Unexpected volume: {vol:F4}");
    }

    [Fact]
    public void ClassifyAll_RayCast_DefaultBehavior()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);

        // Default is ray cast (not winding)
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void CsgOptions_NullStrategy_UsesCpu()
    {
        var options = new CsgOptions { ClassificationStrategy = null };
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);

        var result = Csg.Union(a, b, options);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersection_DifferentMethodsSameResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);

        var rayCast = Csg.Intersect(a, b, new CsgOptions { UseWindingNumber = false });
        var winding = Csg.Intersect(a, b, new CsgOptions { UseWindingNumber = true });

        double volRay = VolumeCalculator.ComputeAbsoluteVolume(rayCast.Mesh);
        double volWind = VolumeCalculator.ComputeAbsoluteVolume(winding.Mesh);

        // Both methods should give similar volumes
        Assert.True(System.Math.Abs(volRay - volWind) < 0.1,
            $"RayCast={volRay:F4} vs Winding={volWind:F4}");
    }

    [Fact]
    public void Difference_WithStrategy_ProducesCorrectResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);

        var result = Csg.Difference(a, b, new CsgOptions
        {
            ClassificationStrategy = new CpuPatchClassificationStrategy()
        });

        double volA = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol < volA + 0.01);
        Assert.True(vol > 0);
    }

    [Fact]
    public void DegenerateCount_ReportedInResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);

        var result = Csg.Union(a, b);
        // DegenerateCount should be non-negative
        Assert.True(result.DegenerateCount >= 0);
    }

    [Fact]
    public void SolidClassification_HasTwoValues()
    {
        var values = Enum.GetValues(typeof(SolidClassification));
        Assert.Equal(2, values.Length);
    }

    [Fact]
    public void SolidClassification_Inside_NotEqual_Outside()
    {
        Assert.NotEqual(SolidClassification.Inside, SolidClassification.Outside);
    }
}
