using MdCsg.Classification;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Classification;

/// <summary>Phase 6: RayCastClassifier — robustness, inside/outside consistency, various shapes</summary>
public class RayCastClassifierRobustnessPropertyTests
{
    [Fact]
    public void Classify_InsideCube_ReturnsInside()
    {
        var cube = MeshFactory.CreateCube();
        var result = RayCastClassifier.Classify(new Vec3(0.5, 0.5, 0.5), cube.Bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void Classify_OutsideCube_ReturnsOutside()
    {
        var cube = MeshFactory.CreateCube();
        var result = RayCastClassifier.Classify(new Vec3(5, 5, 5), cube.Bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void Classify_InsideSphere_ReturnsInside()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 2.0, 2);
        var result = RayCastClassifier.Classify(Vec3.Zero, sphere.Bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void Classify_OutsideSphere_ReturnsOutside()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var result = RayCastClassifier.Classify(new Vec3(5, 0, 0), sphere.Bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void Classify_FarAwayFromCube_ReturnsOutside()
    {
        var cube = MeshFactory.CreateCube();
        var result = RayCastClassifier.Classify(new Vec3(1000, 1000, 1000), cube.Bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void Classify_InsideOffsetCube_ReturnsInside()
    {
        var cube = MeshFactory.CreateCube(new Vec3(10, 20, 30));
        var result = RayCastClassifier.Classify(new Vec3(10.5, 20.5, 30.5), cube.Bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void Classify_OutsideOffsetCube_ReturnsOutside()
    {
        var cube = MeshFactory.CreateCube(new Vec3(10, 20, 30));
        var result = RayCastClassifier.Classify(new Vec3(0, 0, 0), cube.Bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void Classify_InsideTetrahedron_ReturnsInside()
    {
        var tet = MeshFactory.CreateTetrahedron();
        // Centroid of tetrahedron should be inside
        var centroid = tet.Bounds.Min + (tet.Bounds.Max - tet.Bounds.Min) * 0.5;
        var result = RayCastClassifier.Classify(centroid, tet.Bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void Classify_FarFromTetrahedron_ReturnsOutside()
    {
        var tet = MeshFactory.CreateTetrahedron();
        var result = RayCastClassifier.Classify(new Vec3(100, 100, 100), tet.Bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void Classify_InsideLargeCube_ReturnsInside()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 10.0);
        var result = RayCastClassifier.Classify(new Vec3(5, 5, 5), cube.Bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void Classify_JustOutsideCubeX_ReturnsOutside()
    {
        var cube = MeshFactory.CreateCube();
        var result = RayCastClassifier.Classify(new Vec3(1.5, 0.5, 0.5), cube.Bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void Classify_JustOutsideCubeY_ReturnsOutside()
    {
        var cube = MeshFactory.CreateCube();
        var result = RayCastClassifier.Classify(new Vec3(0.5, 1.5, 0.5), cube.Bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void Classify_JustOutsideCubeZ_ReturnsOutside()
    {
        var cube = MeshFactory.CreateCube();
        var result = RayCastClassifier.Classify(new Vec3(0.5, 0.5, 1.5), cube.Bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void Classify_NegativeSide_ReturnsOutside()
    {
        var cube = MeshFactory.CreateCube();
        var result = RayCastClassifier.Classify(new Vec3(-1, 0.5, 0.5), cube.Bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void Classify_InsideSphere_NearCenter_ReturnsInside()
    {
        var sphere = MeshFactory.CreateSphere(new Vec3(5, 5, 5), 2.0, 2);
        var result = RayCastClassifier.Classify(new Vec3(5.1, 5.1, 5.1), sphere.Bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }
}
