using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Classification;

/// <summary>Phase 6: RayCastClassifier voting — inside/outside, consistency with winding, boundary points, multiple shapes</summary>
public class RayCastVotingRobustnessTests
{
    [Fact]
    public void CubeCenter_Inside()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        Assert.Equal(SolidClassification.Inside, RayCastClassifier.Classify(new Vec3(0.5, 0.5, 0.5), bvh));
    }

    [Fact]
    public void CubeOutside_Outside()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        Assert.Equal(SolidClassification.Outside, RayCastClassifier.Classify(new Vec3(5, 5, 5), bvh));
    }

    [Fact]
    public void CubeCornerInterior_Inside()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        Assert.Equal(SolidClassification.Inside, RayCastClassifier.Classify(new Vec3(0.01, 0.01, 0.01), bvh));
    }

    [Fact]
    public void CubeNearFace_Inside()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        // Very close to +X face but still inside
        Assert.Equal(SolidClassification.Inside, RayCastClassifier.Classify(new Vec3(0.999, 0.5, 0.5), bvh));
    }

    [Fact]
    public void SphereCenter_Inside()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        Assert.Equal(SolidClassification.Inside, RayCastClassifier.Classify(Vec3.Zero, bvh));
    }

    [Fact]
    public void SphereFarOutside_Outside()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        Assert.Equal(SolidClassification.Outside, RayCastClassifier.Classify(new Vec3(10, 0, 0), bvh));
    }

    [Fact]
    public void OffsetSphere_Inside()
    {
        var center = new Vec3(5, 5, 5);
        var mesh = MeshFactory.CreateSphere(center, 1.0, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        Assert.Equal(SolidClassification.Inside, RayCastClassifier.Classify(center, bvh));
    }

    [Fact]
    public void OffsetSphere_Outside()
    {
        var center = new Vec3(5, 5, 5);
        var mesh = MeshFactory.CreateSphere(center, 1.0, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        Assert.Equal(SolidClassification.Outside, RayCastClassifier.Classify(Vec3.Zero, bvh));
    }

    [Fact]
    public void Tetrahedron_CentroidInside()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        var bvh = BvhTree.Build(mesh);
        Assert.Equal(SolidClassification.Inside, RayCastClassifier.Classify(Vec3.Zero, bvh));
    }

    [Fact]
    public void Tetrahedron_Outside()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        var bvh = BvhTree.Build(mesh);
        Assert.Equal(SolidClassification.Outside, RayCastClassifier.Classify(new Vec3(10, 10, 10), bvh));
    }

    [Fact]
    public void ConsistentWithWinding_CubeInterior()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var points = new[]
        {
            new Vec3(0.5, 0.5, 0.5),
            new Vec3(0.1, 0.9, 0.1),
            new Vec3(0.9, 0.1, 0.9),
        };
        foreach (var p in points)
        {
            Assert.Equal(
                WindingNumberClassifier.Classify(p, bvh),
                RayCastClassifier.Classify(p, bvh));
        }
    }

    [Fact]
    public void ConsistentWithWinding_SphereInterior()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        Assert.Equal(
            WindingNumberClassifier.Classify(Vec3.Zero, bvh),
            RayCastClassifier.Classify(Vec3.Zero, bvh));
    }

    [Fact]
    public void MultipleOutsidePoints_AllOutside()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var outsidePoints = new[]
        {
            new Vec3(-1, 0.5, 0.5),
            new Vec3(2, 0.5, 0.5),
            new Vec3(0.5, -1, 0.5),
            new Vec3(0.5, 2, 0.5),
            new Vec3(0.5, 0.5, -1),
            new Vec3(0.5, 0.5, 2),
        };
        foreach (var p in outsidePoints)
        {
            Assert.Equal(SolidClassification.Outside, RayCastClassifier.Classify(p, bvh));
        }
    }

    [Fact]
    public void Deterministic_SameResult()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var p = new Vec3(0.3, 0.7, 0.4);
        var r1 = RayCastClassifier.Classify(p, bvh);
        var r2 = RayCastClassifier.Classify(p, bvh);
        Assert.Equal(r1, r2);
    }

    [Fact]
    public void LargeOffsetCube_Inside()
    {
        var mesh = MeshFactory.CreateCube(new Vec3(1000, 2000, 3000)).Mesh;
        var bvh = BvhTree.Build(mesh);
        Assert.Equal(SolidClassification.Inside,
            RayCastClassifier.Classify(new Vec3(1000.5, 2000.5, 3000.5), bvh));
    }
}
