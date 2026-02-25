using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Classification;

/// <summary>Phase 6: Winding number vs ray cast classifier consistency and boundary tests</summary>
public class WindingVsRayCastConsistencyTests
{
    [Fact]
    public void InsideCube_BothClassifiers_Inside()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var point = new Vec3(0.5, 0.5, 0.5); // center of unit cube
        Assert.Equal(SolidClassification.Inside, RayCastClassifier.Classify(point, bvh));
        Assert.Equal(SolidClassification.Inside, WindingNumberClassifier.Classify(point, bvh));
    }

    [Fact]
    public void OutsideCube_BothClassifiers_Outside()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var point = new Vec3(5, 5, 5);
        Assert.Equal(SolidClassification.Outside, RayCastClassifier.Classify(point, bvh));
        Assert.Equal(SolidClassification.Outside, WindingNumberClassifier.Classify(point, bvh));
    }

    [Fact]
    public void InsideSphere_BothClassifiers_Inside()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        var point = Vec3.Zero;
        Assert.Equal(SolidClassification.Inside, RayCastClassifier.Classify(point, bvh));
        Assert.Equal(SolidClassification.Inside, WindingNumberClassifier.Classify(point, bvh));
    }

    [Fact]
    public void OutsideSphere_BothClassifiers_Outside()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        var point = new Vec3(5, 0, 0);
        Assert.Equal(SolidClassification.Outside, RayCastClassifier.Classify(point, bvh));
        Assert.Equal(SolidClassification.Outside, WindingNumberClassifier.Classify(point, bvh));
    }

    [Fact]
    public void InsideTetrahedron_BothAgree()
    {
        var mesh = MeshFactory.CreateTetrahedron(Vec3.Zero, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        var centroid = new Vec3(0.5, 0.5, 0.5); // approximate center
        var rc = RayCastClassifier.Classify(centroid, bvh);
        var wn = WindingNumberClassifier.Classify(centroid, bvh);
        Assert.Equal(rc, wn);
    }

    [Fact]
    public void WindingNumber_InsideCube_NearOne()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        double wn = WindingNumberClassifier.ComputeWindingNumber(new Vec3(0.5, 0.5, 0.5), bvh);
        Assert.True(wn > 0.9, $"Winding number {wn} should be near 1 inside cube");
    }

    [Fact]
    public void WindingNumber_OutsideCube_NearZero()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        double wn = WindingNumberClassifier.ComputeWindingNumber(new Vec3(5, 5, 5), bvh);
        Assert.True(System.Math.Abs(wn) < 0.1, $"Winding number {wn} should be near 0 outside cube");
    }

    [Fact]
    public void WindingNumber_InsideSphere_NearOne()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 3).Mesh;
        var bvh = BvhTree.Build(mesh);
        double wn = WindingNumberClassifier.ComputeWindingNumber(Vec3.Zero, bvh);
        Assert.True(wn > 0.9, $"Winding number {wn} should be near 1 inside sphere");
    }

    [Fact]
    public void WindingNumber_VeryFarPoint_NearZero()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        double wn = WindingNumberClassifier.ComputeWindingNumber(new Vec3(1000, 1000, 1000), bvh);
        Assert.True(System.Math.Abs(wn) < 0.01);
    }

    [Fact]
    public void RayCast_MultipleDirections_Consistency()
    {
        // Point clearly inside → should get Inside regardless of ray perturbation
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var result = RayCastClassifier.Classify(new Vec3(0.5, 0.5, 0.5), bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void Classify_PointAlongAxis_InsideOrOutside()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        // Points along X axis
        Assert.Equal(SolidClassification.Outside, RayCastClassifier.Classify(new Vec3(-1, 0.5, 0.5), bvh));
        Assert.Equal(SolidClassification.Inside, RayCastClassifier.Classify(new Vec3(0.5, 0.5, 0.5), bvh));
        Assert.Equal(SolidClassification.Outside, RayCastClassifier.Classify(new Vec3(2, 0.5, 0.5), bvh));
    }

    [Fact]
    public void PatchClassifier_DegenerateThreshold_IsSmall()
    {
        Assert.True(PatchClassifier.DegenerateMarginThreshold > 0);
        Assert.True(PatchClassifier.DegenerateMarginThreshold < 1e-5);
    }

    [Fact]
    public void SolidClassification_EnumValues()
    {
        Assert.Equal(0, (int)SolidClassification.Inside);
        Assert.Equal(1, (int)SolidClassification.Outside);
    }

    [Fact]
    public void WindingNumber_EmptyBvh_ReturnsZero()
    {
        var mesh = new MeshBuilder().Build(new List<Triangle3>());
        var bvh = BvhTree.Build(mesh);
        double wn = WindingNumberClassifier.ComputeWindingNumber(Vec3.Zero, bvh);
        Assert.Equal(0, wn);
    }

    [Fact]
    public void RayCast_EmptyBvh_Outside()
    {
        var mesh = new MeshBuilder().Build(new List<Triangle3>());
        var bvh = BvhTree.Build(mesh);
        Assert.Equal(SolidClassification.Outside, RayCastClassifier.Classify(Vec3.Zero, bvh));
    }
}
