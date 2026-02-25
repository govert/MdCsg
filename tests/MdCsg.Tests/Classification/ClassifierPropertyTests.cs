using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Classification;

/// <summary>Phase 6: RayCastClassifier and WindingNumberClassifier — inside/outside classification</summary>
public class ClassifierPropertyTests
{
    // --- RayCastClassifier ---

    [Fact]
    public void RayCast_InsideCube_Inside()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        Assert.Equal(SolidClassification.Inside, RayCastClassifier.Classify(Vec3.Zero, cube.Bvh));
    }

    [Fact]
    public void RayCast_OutsideCube_Outside()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        Assert.Equal(SolidClassification.Outside, RayCastClassifier.Classify(new Vec3(5, 0, 0), cube.Bvh));
    }

    [Fact]
    public void RayCast_FarAway_Outside()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        Assert.Equal(SolidClassification.Outside, RayCastClassifier.Classify(new Vec3(1000, 1000, 1000), cube.Bvh));
    }

    [Fact]
    public void RayCast_InsideSphere_Inside()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 2.0, 3);
        Assert.Equal(SolidClassification.Inside, RayCastClassifier.Classify(Vec3.Zero, sphere.Bvh));
    }

    [Fact]
    public void RayCast_OutsideSphere_Outside()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 3);
        Assert.Equal(SolidClassification.Outside, RayCastClassifier.Classify(new Vec3(5, 0, 0), sphere.Bvh));
    }

    [Fact]
    public void RayCast_InsideLargeCube_Inside()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 100.0);
        Assert.Equal(SolidClassification.Inside, RayCastClassifier.Classify(new Vec3(10, 10, 10), cube.Bvh));
    }

    [Fact]
    public void RayCast_JustOutsideCube_Outside()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        Assert.Equal(SolidClassification.Outside, RayCastClassifier.Classify(new Vec3(2.1, 0, 0), cube.Bvh));
    }

    [Fact]
    public void RayCast_OffAxisPoint_Inside()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 4.0);
        Assert.Equal(SolidClassification.Inside, RayCastClassifier.Classify(new Vec3(0.5, 0.5, 0.5), cube.Bvh));
    }

    // --- WindingNumberClassifier ---

    [Fact]
    public void Winding_InsideSphere_ClassifiesInside()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 2.0, 3);
        Assert.Equal(SolidClassification.Inside, WindingNumberClassifier.Classify(Vec3.Zero, sphere.Bvh));
    }

    [Fact]
    public void Winding_OutsideCube_Outside()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        Assert.Equal(SolidClassification.Outside, WindingNumberClassifier.Classify(new Vec3(5, 0, 0), cube.Bvh));
    }

    [Fact]
    public void Winding_FarAway_Outside()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        Assert.Equal(SolidClassification.Outside, WindingNumberClassifier.Classify(new Vec3(1000, 1000, 1000), cube.Bvh));
    }

    [Fact]
    public void Winding_InsideSphereHighRes_Inside()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 2.0, 3);
        Assert.Equal(SolidClassification.Inside, WindingNumberClassifier.Classify(Vec3.Zero, sphere.Bvh));
    }

    [Fact]
    public void Winding_OutsideSphere_Outside()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 3);
        Assert.Equal(SolidClassification.Outside, WindingNumberClassifier.Classify(new Vec3(5, 0, 0), sphere.Bvh));
    }

    [Fact]
    public void Winding_WindingNumber_InsideSphere_NearOneA()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 2.0, 3);
        double wn = WindingNumberClassifier.ComputeWindingNumber(Vec3.Zero, sphere.Bvh);
        Assert.True(System.Math.Abs(wn - 1.0) < 0.15);
    }

    [Fact]
    public void Winding_WindingNumber_OutsideCube_NearZero()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        double wn = WindingNumberClassifier.ComputeWindingNumber(new Vec3(5, 0, 0), cube.Bvh);
        Assert.True(System.Math.Abs(wn) < 0.1);
    }

    [Fact]
    public void Winding_WindingNumber_InsideSphere_NearOneB()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 2.0, 3);
        double wn = WindingNumberClassifier.ComputeWindingNumber(Vec3.Zero, sphere.Bvh);
        Assert.True(System.Math.Abs(wn - 1.0) < 0.15);
    }

    // --- Classifiers agree ---

    [Fact]
    public void BothClassifiers_AgreeOnInsideSphereSmall()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 2.0, 3);
        var rc = RayCastClassifier.Classify(Vec3.Zero, sphere.Bvh);
        var wn = WindingNumberClassifier.Classify(Vec3.Zero, sphere.Bvh);
        Assert.Equal(rc, wn);
    }

    [Fact]
    public void BothClassifiers_AgreeOnOutsideCube()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var rc = RayCastClassifier.Classify(new Vec3(5, 0, 0), cube.Bvh);
        var wn = WindingNumberClassifier.Classify(new Vec3(5, 0, 0), cube.Bvh);
        Assert.Equal(rc, wn);
    }

    [Fact]
    public void BothClassifiers_AgreeOnInsideSphereLarge()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 2.0, 3);
        var rc = RayCastClassifier.Classify(Vec3.Zero, sphere.Bvh);
        var wn = WindingNumberClassifier.Classify(Vec3.Zero, sphere.Bvh);
        Assert.Equal(rc, wn);
    }

    // --- SolidClassification enum ---

    [Fact]
    public void SolidClassification_HasCorrectValues()
    {
        Assert.NotEqual(SolidClassification.Inside, SolidClassification.Outside);
    }

    // --- Edge cases ---

    [Fact]
    public void Winding_EmptyBvh_Outside()
    {
        var empty = MdCsg.Api.Solid.FromTriangles(Array.Empty<Triangle3>());
        double wn = WindingNumberClassifier.ComputeWindingNumber(Vec3.Zero, empty.Bvh);
        Assert.Equal(0.0, wn);
    }
}
