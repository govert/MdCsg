using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Classification;

/// <summary>Code coverage: RayCastClassifier, WindingNumberClassifier, PatchClassifier edge cases</summary>
public class ClassifierCoverageTests
{
    [Fact]
    public void RayCastClassifier_InsideCube_ReturnsInside()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var result = RayCastClassifier.Classify(new Vec3(0.5, 0.5, 0.5), bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void RayCastClassifier_OutsideCube_ReturnsOutside()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var result = RayCastClassifier.Classify(new Vec3(5, 5, 5), bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void RayCastClassifier_NearSurface_Classifies()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        // Just inside the cube
        var result = RayCastClassifier.Classify(new Vec3(0.01, 0.5, 0.5), bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void RayCastClassifier_FarFromOrigin()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var result = RayCastClassifier.Classify(new Vec3(100, 100, 100), bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void WindingNumberClassifier_InsideCube_ReturnsInside()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var result = WindingNumberClassifier.Classify(new Vec3(0.5, 0.5, 0.5), bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void WindingNumberClassifier_OutsideCube_ReturnsOutside()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var result = WindingNumberClassifier.Classify(new Vec3(5, 5, 5), bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void WindingNumberClassifier_EmptyBvh_Returns0()
    {
        var bvh = BvhTree.Build(new HalfEdgeMesh());
        double wn = WindingNumberClassifier.ComputeWindingNumber(Vec3.Zero, bvh);
        Assert.Equal(0, wn);
    }

    [Fact]
    public void WindingNumber_InsideCube_Near1()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        double wn = WindingNumberClassifier.ComputeWindingNumber(new Vec3(0.5, 0.5, 0.5), bvh);
        Assert.True(System.Math.Abs(wn - 1.0) < 0.1, $"Expected ~1.0, got {wn}");
    }

    [Fact]
    public void WindingNumber_OutsideCube_Near0()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        double wn = WindingNumberClassifier.ComputeWindingNumber(new Vec3(5, 5, 5), bvh);
        Assert.True(System.Math.Abs(wn) < 0.1, $"Expected ~0.0, got {wn}");
    }

    [Fact]
    public void WindingNumber_InsideSphere_Near1()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        double wn = WindingNumberClassifier.ComputeWindingNumber(Vec3.Zero, bvh);
        Assert.True(System.Math.Abs(wn - 1.0) < 0.2, $"Expected ~1.0, got {wn}");
    }

    [Fact]
    public void RayCast_And_WindingNumber_Agree_InsideCube()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var point = new Vec3(0.3, 0.4, 0.5);
        var rc = RayCastClassifier.Classify(point, bvh);
        var wn = WindingNumberClassifier.Classify(point, bvh);
        Assert.Equal(rc, wn);
    }

    [Fact]
    public void RayCast_And_WindingNumber_Agree_OutsideCube()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var point = new Vec3(5, 5, 5);
        var rc = RayCastClassifier.Classify(point, bvh);
        var wn = WindingNumberClassifier.Classify(point, bvh);
        Assert.Equal(rc, wn);
    }

    [Fact]
    public void SolidClassification_EnumValues()
    {
        Assert.NotEqual(SolidClassification.Inside, SolidClassification.Outside);
    }

    [Fact]
    public void PatchClassifier_DegenerateMarginThreshold_IsSmall()
    {
        Assert.True(PatchClassifier.DegenerateMarginThreshold > 0);
        Assert.True(PatchClassifier.DegenerateMarginThreshold < 1e-5);
    }

    [Fact]
    public void CpuPatchClassificationStrategy_ImplementsInterface()
    {
        IPatchClassificationStrategy strategy = new CpuPatchClassificationStrategy();
        Assert.NotNull(strategy);
    }

    [Fact]
    public void RayCastClassifier_NegativeCoords()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube(new Vec3(-2, -2, -2)).Mesh);
        var result = RayCastClassifier.Classify(new Vec3(-1.5, -1.5, -1.5), bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void WindingNumberClassifier_NegativeCoords()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube(new Vec3(-2, -2, -2)).Mesh);
        var result = WindingNumberClassifier.Classify(new Vec3(-1.5, -1.5, -1.5), bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void RayCastClassifier_InsideSphere()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateSphere(Vec3.Zero, 2, 2).Mesh);
        var result = RayCastClassifier.Classify(Vec3.Zero, bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void RayCastClassifier_OutsideSphere()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var result = RayCastClassifier.Classify(new Vec3(10, 0, 0), bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }
}
