using MdCsg.Classification;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Classification;

/// <summary>Phase 6: WindingNumberClassifier — classification, winding number values, boundary behavior</summary>
public class WindingNumberClassifierIntegrationPropertyTests
{
    [Fact]
    public void Classify_InsideCube_ReturnsInside()
    {
        var cube = MeshFactory.CreateCube();
        var result = WindingNumberClassifier.Classify(new Vec3(0.5, 0.5, 0.5), cube.Bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void Classify_OutsideCube_ReturnsOutside()
    {
        var cube = MeshFactory.CreateCube();
        var result = WindingNumberClassifier.Classify(new Vec3(5, 5, 5), cube.Bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void Classify_InsideSphere_ReturnsInside()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 2.0, 2);
        var result = WindingNumberClassifier.Classify(Vec3.Zero, sphere.Bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void Classify_OutsideSphere_ReturnsOutside()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var result = WindingNumberClassifier.Classify(new Vec3(5, 0, 0), sphere.Bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void ComputeWindingNumber_InsideCube_NearOne()
    {
        var cube = MeshFactory.CreateCube();
        double wn = WindingNumberClassifier.ComputeWindingNumber(new Vec3(0.5, 0.5, 0.5), cube.Bvh);
        Assert.True(wn > 0.9 && wn < 1.1, $"Winding number inside cube should be ~1.0, got {wn}");
    }

    [Fact]
    public void ComputeWindingNumber_OutsideCube_NearZero()
    {
        var cube = MeshFactory.CreateCube();
        double wn = WindingNumberClassifier.ComputeWindingNumber(new Vec3(5, 5, 5), cube.Bvh);
        Assert.True(System.Math.Abs(wn) < 0.1, $"Winding number outside cube should be ~0, got {wn}");
    }

    [Fact]
    public void ComputeWindingNumber_InsideSphere_NearOne()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        double wn = WindingNumberClassifier.ComputeWindingNumber(Vec3.Zero, sphere.Bvh);
        Assert.True(wn > 0.9 && wn < 1.1, $"Winding number at sphere center should be ~1.0, got {wn}");
    }

    [Fact]
    public void ComputeWindingNumber_FarFromSphere_NearZero()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        double wn = WindingNumberClassifier.ComputeWindingNumber(new Vec3(100, 0, 0), sphere.Bvh);
        Assert.True(System.Math.Abs(wn) < 0.01, $"Winding number far from sphere should be ~0, got {wn}");
    }

    [Fact]
    public void Classify_AgreesWithRayCast_InsideCube()
    {
        var cube = MeshFactory.CreateCube();
        var point = new Vec3(0.5, 0.5, 0.5);
        var rayCast = RayCastClassifier.Classify(point, cube.Bvh);
        var winding = WindingNumberClassifier.Classify(point, cube.Bvh);
        Assert.Equal(rayCast, winding);
    }

    [Fact]
    public void Classify_AgreesWithRayCast_OutsideCube()
    {
        var cube = MeshFactory.CreateCube();
        var point = new Vec3(5, 5, 5);
        var rayCast = RayCastClassifier.Classify(point, cube.Bvh);
        var winding = WindingNumberClassifier.Classify(point, cube.Bvh);
        Assert.Equal(rayCast, winding);
    }

    [Fact]
    public void Classify_AgreesWithRayCast_InsideSphere()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 2.0, 2);
        var point = new Vec3(0.1, 0.1, 0.1);
        var rayCast = RayCastClassifier.Classify(point, sphere.Bvh);
        var winding = WindingNumberClassifier.Classify(point, sphere.Bvh);
        Assert.Equal(rayCast, winding);
    }

    [Fact]
    public void Classify_AgreesWithRayCast_OutsideSphere()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var point = new Vec3(5, 5, 5);
        var rayCast = RayCastClassifier.Classify(point, sphere.Bvh);
        var winding = WindingNumberClassifier.Classify(point, sphere.Bvh);
        Assert.Equal(rayCast, winding);
    }

    [Fact]
    public void Classify_InsideTetrahedron_ReturnsInside()
    {
        var tet = MeshFactory.CreateTetrahedron();
        var center = tet.Bounds.Min + (tet.Bounds.Max - tet.Bounds.Min) * 0.5;
        var result = WindingNumberClassifier.Classify(center, tet.Bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void Classify_FarFromTetrahedron_ReturnsOutside()
    {
        var tet = MeshFactory.CreateTetrahedron();
        var result = WindingNumberClassifier.Classify(new Vec3(100, 100, 100), tet.Bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void ComputeWindingNumber_InsideTetrahedron_NearOne()
    {
        var tet = MeshFactory.CreateTetrahedron();
        var center = tet.Bounds.Min + (tet.Bounds.Max - tet.Bounds.Min) * 0.5;
        double wn = WindingNumberClassifier.ComputeWindingNumber(center, tet.Bvh);
        Assert.True(wn > 0.9 && wn < 1.1, $"Winding number inside tet should be ~1.0, got {wn}");
    }
}
