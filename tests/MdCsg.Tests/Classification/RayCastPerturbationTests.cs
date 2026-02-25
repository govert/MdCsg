using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Classification;

/// <summary>Phase 6: RayCast classifier — perturbed directions, majority voting, edge/vertex adjacency</summary>
public class RayCastPerturbationTests
{
    [Fact]
    public void CubeCenter_IsInside()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        Assert.Equal(SolidClassification.Inside, RayCastClassifier.Classify(new Vec3(0.5, 0.5, 0.5), bvh));
    }

    [Fact]
    public void FarOutside_IsOutside()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        Assert.Equal(SolidClassification.Outside, RayCastClassifier.Classify(new Vec3(100, 100, 100), bvh));
    }

    [Theory]
    [InlineData(0.5, 0.5, 0.001)]
    [InlineData(0.5, 0.5, 0.999)]
    [InlineData(0.001, 0.5, 0.5)]
    [InlineData(0.999, 0.5, 0.5)]
    [InlineData(0.5, 0.001, 0.5)]
    [InlineData(0.5, 0.999, 0.5)]
    public void NearFace_Inside(double x, double y, double z)
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        Assert.Equal(SolidClassification.Inside, RayCastClassifier.Classify(new Vec3(x, y, z), bvh));
    }

    [Theory]
    [InlineData(0.5, 0.5, -0.01)]
    [InlineData(0.5, 0.5, 1.01)]
    [InlineData(-0.01, 0.5, 0.5)]
    [InlineData(1.01, 0.5, 0.5)]
    [InlineData(0.5, -0.01, 0.5)]
    [InlineData(0.5, 1.01, 0.5)]
    public void NearFace_Outside(double x, double y, double z)
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        Assert.Equal(SolidClassification.Outside, RayCastClassifier.Classify(new Vec3(x, y, z), bvh));
    }

    [Fact]
    public void NearCubeEdge_Inside()
    {
        // Point near the edge where X=0 and Y=0 faces meet
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var result = RayCastClassifier.Classify(new Vec3(0.01, 0.01, 0.5), bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void NearCubeCorner_Inside()
    {
        // Point near the (0,0,0) corner but inside
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var result = RayCastClassifier.Classify(new Vec3(0.01, 0.01, 0.01), bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void NearCubeCorner_Outside()
    {
        // Point near the (0,0,0) corner but outside
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var result = RayCastClassifier.Classify(new Vec3(-0.01, -0.01, -0.01), bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void Sphere_CenterInside()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        Assert.Equal(SolidClassification.Inside, RayCastClassifier.Classify(Vec3.Zero, bvh));
    }

    [Fact]
    public void Sphere_FarOutside()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        Assert.Equal(SolidClassification.Outside, RayCastClassifier.Classify(new Vec3(10, 0, 0), bvh));
    }

    [Fact]
    public void Tetrahedron_CenterInside()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        var bvh = BvhTree.Build(mesh);
        // The tetrahedron should contain some interior point
        // Use centroid of all vertices
        Vec3 center = Vec3.Zero;
        foreach (var v in mesh.Vertices)
            center = center + v.Position;
        center = center / mesh.Vertices.Count;
        Assert.Equal(SolidClassification.Inside, RayCastClassifier.Classify(center, bvh));
    }

    [Fact]
    public void Tetrahedron_FarOutside()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        var bvh = BvhTree.Build(mesh);
        Assert.Equal(SolidClassification.Outside, RayCastClassifier.Classify(new Vec3(100, 100, 100), bvh));
    }

    [Fact]
    public void OffsetCube_CenterInside()
    {
        var mesh = MeshFactory.CreateCube(new Vec3(10, 20, 30)).Mesh;
        var bvh = BvhTree.Build(mesh);
        Assert.Equal(SolidClassification.Inside, RayCastClassifier.Classify(new Vec3(10.5, 20.5, 30.5), bvh));
    }

    [Fact]
    public void ScaledCube_CenterInside()
    {
        var mesh = MeshFactory.CreateCube(Vec3.Zero, 5).Mesh;
        var bvh = BvhTree.Build(mesh);
        Assert.Equal(SolidClassification.Inside, RayCastClassifier.Classify(new Vec3(2.5, 2.5, 2.5), bvh));
    }

    [Fact]
    public void ScaledCube_OutsideCorner()
    {
        var mesh = MeshFactory.CreateCube(Vec3.Zero, 5).Mesh;
        var bvh = BvhTree.Build(mesh);
        Assert.Equal(SolidClassification.Outside, RayCastClassifier.Classify(new Vec3(6, 6, 6), bvh));
    }

    [Fact]
    public void MultiplePoints_AlongAxis_Consistent()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        // Test a line of points along X at Y=Z=0.5
        for (double x = -0.5; x <= 1.5; x += 0.1)
        {
            var result = RayCastClassifier.Classify(new Vec3(x, 0.5, 0.5), bvh);
            if (x > 0.001 && x < 0.999)
                Assert.Equal(SolidClassification.Inside, result);
            else if (x < -0.001 || x > 1.001)
                Assert.Equal(SolidClassification.Outside, result);
        }
    }

    [Fact]
    public void AgreesWith_WindingNumber_InsideCube()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var point = new Vec3(0.5, 0.5, 0.5);
        Assert.Equal(
            RayCastClassifier.Classify(point, bvh),
            WindingNumberClassifier.Classify(point, bvh));
    }

    [Fact]
    public void AgreesWith_WindingNumber_OutsideCube()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var point = new Vec3(5, 5, 5);
        Assert.Equal(
            RayCastClassifier.Classify(point, bvh),
            WindingNumberClassifier.Classify(point, bvh));
    }
}
