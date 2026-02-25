using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Classification;

/// <summary>Phase 6: RayCastClassifier — direct classification of points as inside/outside</summary>
public class RayCastClassifierDirectTests
{
    [Fact]
    public void InsideCube_IsInside()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var result = RayCastClassifier.Classify(new Vec3(0.5, 0.5, 0.5), bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void OutsideCube_IsOutside()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var result = RayCastClassifier.Classify(new Vec3(5, 5, 5), bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void FarFromCube_IsOutside()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var result = RayCastClassifier.Classify(new Vec3(-100, -100, -100), bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void InsideSphere_IsInside()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh);
        var result = RayCastClassifier.Classify(Vec3.Zero, bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void OutsideSphere_IsOutside()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh);
        var result = RayCastClassifier.Classify(new Vec3(5, 0, 0), bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void InsideTranslatedCube_IsInside()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube(new Vec3(10, 10, 10)).Mesh);
        var result = RayCastClassifier.Classify(new Vec3(10.5, 10.5, 10.5), bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void OutsideTranslatedCube_IsOutside()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube(new Vec3(10, 10, 10)).Mesh);
        var result = RayCastClassifier.Classify(Vec3.Zero, bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void InsideTetrahedron_IsInside()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateTetrahedron().Mesh);
        // Centroid of regular tetrahedron with vertices at (0,0,0), (1,0,0), (0.5,0.87,0), (0.5,0.29,0.82)
        // The centroid should be inside
        var centroid = new Vec3(0.5, 0.29, 0.2);
        var result = RayCastClassifier.Classify(centroid, bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void MultiplePoints_Inside_Consistent()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var points = new[]
        {
            new Vec3(0.1, 0.1, 0.1),
            new Vec3(0.5, 0.5, 0.5),
            new Vec3(0.9, 0.9, 0.9),
            new Vec3(0.5, 0.1, 0.9),
        };

        foreach (var p in points)
        {
            Assert.Equal(SolidClassification.Inside, RayCastClassifier.Classify(p, bvh));
        }
    }

    [Fact]
    public void MultiplePoints_Outside_Consistent()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var points = new[]
        {
            new Vec3(-1, -1, -1),
            new Vec3(2, 2, 2),
            new Vec3(-1, 0.5, 0.5),
            new Vec3(0.5, 0.5, 2),
        };

        foreach (var p in points)
        {
            Assert.Equal(SolidClassification.Outside, RayCastClassifier.Classify(p, bvh));
        }
    }
}
