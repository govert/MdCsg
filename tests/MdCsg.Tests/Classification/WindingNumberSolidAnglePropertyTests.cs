using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Classification;

/// <summary>Phase 6: WindingNumberClassifier — winding number values, inside/outside, empty mesh, sphere/cube</summary>
public class WindingNumberSolidAnglePropertyTests
{
    [Fact]
    public void PointInsideCube_WindingNumberNearOne()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        double wn = WindingNumberClassifier.ComputeWindingNumber(new Vec3(0.5, 0.5, 0.5), bvh);
        Assert.True(System.Math.Abs(wn - 1.0) < 0.1, $"Expected winding number ~1 for inside point, got {wn}");
    }

    [Fact]
    public void PointOutsideCube_WindingNumberNearZero()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        double wn = WindingNumberClassifier.ComputeWindingNumber(new Vec3(5, 5, 5), bvh);
        Assert.True(System.Math.Abs(wn) < 0.1, $"Expected winding number ~0 for outside point, got {wn}");
    }

    [Fact]
    public void Classify_InsideCube_Inside()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        Assert.Equal(SolidClassification.Inside, WindingNumberClassifier.Classify(new Vec3(0.5, 0.5, 0.5), bvh));
    }

    [Fact]
    public void Classify_OutsideCube_Outside()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        Assert.Equal(SolidClassification.Outside, WindingNumberClassifier.Classify(new Vec3(5, 5, 5), bvh));
    }

    [Fact]
    public void Classify_InsideSphere_Inside()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        Assert.Equal(SolidClassification.Inside, WindingNumberClassifier.Classify(Vec3.Zero, bvh));
    }

    [Fact]
    public void Classify_OutsideSphere_Outside()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        Assert.Equal(SolidClassification.Outside, WindingNumberClassifier.Classify(new Vec3(5, 0, 0), bvh));
    }

    [Fact]
    public void EmptyMesh_WindingNumberZero()
    {
        var mesh = new MdCsg.Mesh.MeshBuilder().Build(new List<Triangle3>());
        var bvh = BvhTree.Build(mesh);
        double wn = WindingNumberClassifier.ComputeWindingNumber(Vec3.Zero, bvh);
        Assert.Equal(0, wn);
    }

    [Fact]
    public void EmptyMesh_ClassifiesOutside()
    {
        var mesh = new MdCsg.Mesh.MeshBuilder().Build(new List<Triangle3>());
        var bvh = BvhTree.Build(mesh);
        Assert.Equal(SolidClassification.Outside, WindingNumberClassifier.Classify(Vec3.Zero, bvh));
    }

    [Fact]
    public void WindingNumber_CubeCenter_IsExactlyOne()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        double wn = WindingNumberClassifier.ComputeWindingNumber(new Vec3(0.5, 0.5, 0.5), bvh);
        Assert.Equal(1.0, wn, 1e-6);
    }

    [Fact]
    public void WindingNumber_FarAway_IsNearZero()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        double wn = WindingNumberClassifier.ComputeWindingNumber(new Vec3(100, 100, 100), bvh);
        Assert.True(System.Math.Abs(wn) < 0.01);
    }

    [Fact]
    public void WindingNumber_MultipleCubeInteriorPoints_AllNearOne()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var interiorPoints = new[]
        {
            new Vec3(0.1, 0.1, 0.1),
            new Vec3(0.9, 0.9, 0.9),
            new Vec3(0.5, 0.1, 0.5),
            new Vec3(0.1, 0.5, 0.9),
        };
        foreach (var p in interiorPoints)
        {
            double wn = WindingNumberClassifier.ComputeWindingNumber(p, bvh);
            Assert.True(System.Math.Abs(wn - 1.0) < 0.1, $"Expected ~1 for {p}, got {wn}");
        }
    }

    [Fact]
    public void WindingNumber_MultipleExteriorPoints_AllNearZero()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var exteriorPoints = new[]
        {
            new Vec3(-1, 0.5, 0.5),
            new Vec3(2, 0.5, 0.5),
            new Vec3(0.5, -1, 0.5),
            new Vec3(0.5, 0.5, 2),
        };
        foreach (var p in exteriorPoints)
        {
            double wn = WindingNumberClassifier.ComputeWindingNumber(p, bvh);
            Assert.True(System.Math.Abs(wn) < 0.1, $"Expected ~0 for {p}, got {wn}");
        }
    }

    [Fact]
    public void RayCastAndWinding_AgreeOnCube()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var testPoints = new[]
        {
            new Vec3(0.5, 0.5, 0.5),  // inside
            new Vec3(5, 5, 5),          // outside
            new Vec3(-1, 0.5, 0.5),    // outside
            new Vec3(0.25, 0.25, 0.25), // inside
        };
        foreach (var p in testPoints)
        {
            var winding = WindingNumberClassifier.Classify(p, bvh);
            var raycast = RayCastClassifier.Classify(p, bvh);
            Assert.Equal(raycast, winding);
        }
    }

    [Fact]
    public void WindingNumber_SphereCenter_NearOne()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 3).Mesh;
        var bvh = BvhTree.Build(mesh);
        double wn = WindingNumberClassifier.ComputeWindingNumber(Vec3.Zero, bvh);
        Assert.True(System.Math.Abs(wn - 1.0) < 0.05, $"Expected ~1 for sphere center, got {wn}");
    }

    [Fact]
    public void WindingNumber_OffsetSphere_InsideOutsideCorrect()
    {
        var center = new Vec3(2, 3, 4);
        var mesh = MeshFactory.CreateSphere(center, 1.0, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        Assert.Equal(SolidClassification.Inside, WindingNumberClassifier.Classify(center, bvh));
        Assert.Equal(SolidClassification.Outside, WindingNumberClassifier.Classify(center + new Vec3(2, 0, 0), bvh));
    }

    [Fact]
    public void WindingNumber_CsgResult_InsideClassified()
    {
        var a = new MdCsg.Api.Solid(MeshFactory.CreateCube().Mesh);
        var b = new MdCsg.Api.Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = MdCsg.Api.Csg.Union(a, b);
        var bvh = BvhTree.Build(result.Mesh);
        // A point inside the union
        Assert.Equal(SolidClassification.Inside, WindingNumberClassifier.Classify(new Vec3(0.25, 0.25, 0.25), bvh));
    }
}
