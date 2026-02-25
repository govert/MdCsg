using MdCsg.Classification;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Classification;

/// <summary>Phase 6: SolidClassification enum, RayCast/Winding classifiers, CpuStrategy interface</summary>
public class SolidClassificationEnumPropertyTests
{
    [Fact]
    public void SolidClassification_Inside_Value()
    {
        Assert.Equal(0, (int)SolidClassification.Inside);
    }

    [Fact]
    public void SolidClassification_Outside_Value()
    {
        Assert.Equal(1, (int)SolidClassification.Outside);
    }

    [Fact]
    public void RayCastClassifier_InsideCube_ReturnsInside()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var point = new Vec3(1, 1, 1); // center of cube
        var result = RayCastClassifier.Classify(point, cube.Bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void RayCastClassifier_OutsideCube_ReturnsOutside()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var point = new Vec3(10, 10, 10);
        var result = RayCastClassifier.Classify(point, cube.Bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void WindingNumberClassifier_InsideCube_ReturnsInside()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var point = new Vec3(1, 1, 1);
        var result = WindingNumberClassifier.Classify(point, cube.Bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void WindingNumberClassifier_OutsideCube_ReturnsOutside()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var point = new Vec3(10, 10, 10);
        var result = WindingNumberClassifier.Classify(point, cube.Bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void RayCastClassifier_InsideSphere_ReturnsInside()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 2.0, 2);
        var point = new Vec3(0.1, 0.1, 0.1);
        var result = RayCastClassifier.Classify(point, sphere.Bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void RayCastClassifier_OutsideSphere_ReturnsOutside()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var point = new Vec3(10, 0, 0);
        var result = RayCastClassifier.Classify(point, sphere.Bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void WindingNumber_InsideSphere_NearOne()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 2.0, 2);
        var point = new Vec3(0.1, 0.1, 0.1);
        double wn = WindingNumberClassifier.ComputeWindingNumber(point, sphere.Bvh);
        Assert.True(System.Math.Abs(wn - 1.0) < 0.1, $"Winding number inside sphere should be ~1.0, got {wn}");
    }

    [Fact]
    public void WindingNumber_OutsideSphere_NearZero()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var point = new Vec3(10, 0, 0);
        double wn = WindingNumberClassifier.ComputeWindingNumber(point, sphere.Bvh);
        Assert.True(System.Math.Abs(wn) < 0.1, $"Winding number outside sphere should be ~0.0, got {wn}");
    }

    [Fact]
    public void CpuPatchClassificationStrategy_ImplementsInterface()
    {
        var strategy = new CpuPatchClassificationStrategy();
        Assert.IsAssignableFrom<IPatchClassificationStrategy>(strategy);
    }

    [Fact]
    public void RayCastAndWinding_AgreeOnCubeInterior()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var point = new Vec3(1, 1, 1);
        var rc = RayCastClassifier.Classify(point, cube.Bvh);
        var wn = WindingNumberClassifier.Classify(point, cube.Bvh);
        Assert.Equal(rc, wn);
    }

    [Fact]
    public void RayCastAndWinding_AgreeOnCubeExterior()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var point = new Vec3(10, 10, 10);
        var rc = RayCastClassifier.Classify(point, cube.Bvh);
        var wn = WindingNumberClassifier.Classify(point, cube.Bvh);
        Assert.Equal(rc, wn);
    }

    [Fact]
    public void RayCastClassifier_InsideTetrahedron_ReturnsInside()
    {
        var tet = MeshFactory.CreateTetrahedron();
        var centroid = Vec3.Zero;
        foreach (var v in tet.Mesh.Vertices)
            centroid = centroid + v.Position;
        centroid = centroid / tet.Mesh.Vertices.Count;
        var result = RayCastClassifier.Classify(centroid, tet.Bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }
}
