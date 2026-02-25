using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Classification;

/// <summary>Phase 6: RayCastClassifier — majority voting, multiple shapes, consistency</summary>
public class RayCastMajorityVotingTests
{
    [Fact]
    public void CubeCenter_AllDirections_Agree()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        // The center of the cube should consistently classify as inside
        var result = RayCastClassifier.Classify(new Vec3(0.5, 0.5, 0.5), bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void CubeOutside_AllDirections_Agree()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var result = RayCastClassifier.Classify(new Vec3(5, 5, 5), bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void NearCorner_StillInside()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        // Very close to corner but still inside
        var result = RayCastClassifier.Classify(new Vec3(0.01, 0.01, 0.01), bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void NearFace_StillInside()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        // Very close to a face but still inside
        var result = RayCastClassifier.Classify(new Vec3(0.5, 0.5, 0.999), bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void JustOutside_IsOutside()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var result = RayCastClassifier.Classify(new Vec3(0.5, 0.5, 1.001), bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void SphereCenter_IsInside()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh);
        var result = RayCastClassifier.Classify(Vec3.Zero, bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void SphereOffCenter_StillInside()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh);
        var result = RayCastClassifier.Classify(new Vec3(0.3, 0.3, 0.3), bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void TranslatedCube_InsideDetected()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube(new Vec3(100, 200, 300)).Mesh);
        var result = RayCastClassifier.Classify(new Vec3(100.5, 200.5, 300.5), bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void TranslatedCube_OriginalOrigin_IsOutside()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube(new Vec3(100, 200, 300)).Mesh);
        var result = RayCastClassifier.Classify(Vec3.Zero, bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void Consistency_MultipleCallsSameResult()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var pt = new Vec3(0.5, 0.5, 0.5);
        var r1 = RayCastClassifier.Classify(pt, bvh);
        var r2 = RayCastClassifier.Classify(pt, bvh);
        var r3 = RayCastClassifier.Classify(pt, bvh);
        Assert.Equal(r1, r2);
        Assert.Equal(r2, r3);
    }

    [Fact]
    public void NegativeCoordinates_InsideCube()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube(new Vec3(-1, -1, -1)).Mesh);
        var result = RayCastClassifier.Classify(new Vec3(-0.5, -0.5, -0.5), bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void LargeCube_InsidePoint()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube(Vec3.Zero, 100).Mesh);
        var result = RayCastClassifier.Classify(new Vec3(50, 50, 50), bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }
}
