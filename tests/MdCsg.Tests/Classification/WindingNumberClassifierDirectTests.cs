using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Classification;

/// <summary>Phase 6: WindingNumberClassifier — direct classification, agrees with RayCast</summary>
public class WindingNumberClassifierDirectTests
{
    [Fact]
    public void InsideCube_IsInside()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var result = WindingNumberClassifier.Classify(new Vec3(0.5, 0.5, 0.5), bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void OutsideCube_IsOutside()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var result = WindingNumberClassifier.Classify(new Vec3(5, 5, 5), bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void InsideSphere_IsInside()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh);
        var result = WindingNumberClassifier.Classify(Vec3.Zero, bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void OutsideSphere_IsOutside()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh);
        var result = WindingNumberClassifier.Classify(new Vec3(5, 0, 0), bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void AgreesWith_RayCast_InsideCube()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var pt = new Vec3(0.5, 0.5, 0.5);
        var rc = RayCastClassifier.Classify(pt, bvh);
        var wn = WindingNumberClassifier.Classify(pt, bvh);
        Assert.Equal(rc, wn);
    }

    [Fact]
    public void AgreesWith_RayCast_OutsideCube()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var pt = new Vec3(5, 5, 5);
        var rc = RayCastClassifier.Classify(pt, bvh);
        var wn = WindingNumberClassifier.Classify(pt, bvh);
        Assert.Equal(rc, wn);
    }

    [Fact]
    public void AgreesWith_RayCast_InsideSphere()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh);
        var pt = new Vec3(0.1, 0.1, 0.1);
        var rc = RayCastClassifier.Classify(pt, bvh);
        var wn = WindingNumberClassifier.Classify(pt, bvh);
        Assert.Equal(rc, wn);
    }

    [Fact]
    public void AgreesWith_RayCast_OutsideSphere()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh);
        var pt = new Vec3(3, 0, 0);
        var rc = RayCastClassifier.Classify(pt, bvh);
        var wn = WindingNumberClassifier.Classify(pt, bvh);
        Assert.Equal(rc, wn);
    }

    [Fact]
    public void FarAway_IsOutside()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var result = WindingNumberClassifier.Classify(new Vec3(1000, 1000, 1000), bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void MultipleInternalPoints_AllInside()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var points = new[]
        {
            new Vec3(0.2, 0.2, 0.2),
            new Vec3(0.8, 0.8, 0.8),
            new Vec3(0.5, 0.5, 0.5),
        };

        foreach (var p in points)
        {
            Assert.Equal(SolidClassification.Inside, WindingNumberClassifier.Classify(p, bvh));
        }
    }
}
