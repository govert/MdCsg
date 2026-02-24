using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Classification;

/// <summary>Phase 6: WindingNumberClassifier deep tests</summary>
public class WindingNumberDeepTests
{
    [Fact]
    public void WindingNumber_CubeCenter_NearOne()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        double wn = WindingNumberClassifier.ComputeWindingNumber(new Vec3(0.5, 0.5, 0.5), bvh);
        Assert.True(System.Math.Abs(wn - 1.0) < 0.1, $"Winding number at center: {wn}");
    }

    [Fact]
    public void WindingNumber_FarOutside_NearZero()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        double wn = WindingNumberClassifier.ComputeWindingNumber(new Vec3(100, 100, 100), bvh);
        Assert.True(System.Math.Abs(wn) < 0.1, $"Winding number far outside: {wn}");
    }

    [Fact]
    public void WindingNumber_JustOutside_NearZero()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        double wn = WindingNumberClassifier.ComputeWindingNumber(new Vec3(-0.01, 0.5, 0.5), bvh);
        Assert.True(wn < 0.5, $"Winding number just outside: {wn}");
    }

    [Fact]
    public void WindingNumber_JustInside_NearOne()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        double wn = WindingNumberClassifier.ComputeWindingNumber(new Vec3(0.01, 0.5, 0.5), bvh);
        Assert.True(wn > 0.5, $"Winding number just inside: {wn}");
    }

    [Fact]
    public void WindingNumber_SphereCenter_NearOne()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        double wn = WindingNumberClassifier.ComputeWindingNumber(Vec3.Zero, bvh);
        Assert.True(System.Math.Abs(wn - 1.0) < 0.1, $"Sphere center winding number: {wn}");
    }

    [Fact]
    public void WindingNumber_SphereOutside_NearZero()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        double wn = WindingNumberClassifier.ComputeWindingNumber(new Vec3(5, 0, 0), bvh);
        Assert.True(System.Math.Abs(wn) < 0.1, $"Sphere outside winding number: {wn}");
    }

    [Fact]
    public void Classify_MultipleInsidePoints()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var insidePoints = new[]
        {
            new Vec3(0.1, 0.5, 0.5),
            new Vec3(0.9, 0.5, 0.5),
            new Vec3(0.5, 0.1, 0.5),
            new Vec3(0.5, 0.9, 0.5),
            new Vec3(0.5, 0.5, 0.1),
            new Vec3(0.5, 0.5, 0.9),
        };
        foreach (var p in insidePoints)
        {
            Assert.Equal(SolidClassification.Inside, WindingNumberClassifier.Classify(p, bvh));
        }
    }

    [Fact]
    public void Classify_Agrees_WithRayCast_InsideSphere()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        var points = new[]
        {
            new Vec3(0.1, 0.1, 0.1),
            new Vec3(-0.3, 0.2, 0.1),
            new Vec3(0, 0, 0.5),
        };
        foreach (var p in points)
        {
            var wn = WindingNumberClassifier.Classify(p, bvh);
            var rc = RayCastClassifier.Classify(p, bvh);
            Assert.Equal(rc, wn);
        }
    }

    [Fact]
    public void Classify_Agrees_WithRayCast_OutsideSphere()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        var points = new[]
        {
            new Vec3(2, 0, 0),
            new Vec3(0, 2, 0),
            new Vec3(0, 0, -2),
        };
        foreach (var p in points)
        {
            var wn = WindingNumberClassifier.Classify(p, bvh);
            var rc = RayCastClassifier.Classify(p, bvh);
            Assert.Equal(rc, wn);
        }
    }

    [Fact]
    public void WindingNumber_Tetrahedron_Center_Inside()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        var bvh = BvhTree.Build(mesh);
        // Centroid of tetrahedron
        var center = new Vec3(0.25, 0.25, 0.25);
        var result = WindingNumberClassifier.Classify(center, bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void WindingNumber_Tetrahedron_Outside()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        var bvh = BvhTree.Build(mesh);
        var result = WindingNumberClassifier.Classify(new Vec3(5, 5, 5), bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }
}
