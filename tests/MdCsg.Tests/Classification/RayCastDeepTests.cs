using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Classification;

/// <summary>Phase 6: RayCastClassifier deep tests — direction perturbation, voting, edge cases</summary>
public class RayCastDeepTests
{
    [Fact]
    public void Classify_InsideCube_ReturnsInside()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var result = RayCastClassifier.Classify(new Vec3(0.5, 0.5, 0.5), bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void Classify_OutsideCube_ReturnsOutside()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var result = RayCastClassifier.Classify(new Vec3(5, 5, 5), bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Theory]
    [InlineData(0.5, 0.5, 0.001)]
    [InlineData(0.5, 0.5, 0.999)]
    [InlineData(0.001, 0.5, 0.5)]
    [InlineData(0.999, 0.5, 0.5)]
    [InlineData(0.5, 0.001, 0.5)]
    [InlineData(0.5, 0.999, 0.5)]
    public void Classify_NearFace_InsideCube(double x, double y, double z)
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var result = RayCastClassifier.Classify(new Vec3(x, y, z), bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Theory]
    [InlineData(-0.01, 0.5, 0.5)]
    [InlineData(1.01, 0.5, 0.5)]
    [InlineData(0.5, -0.01, 0.5)]
    [InlineData(0.5, 1.01, 0.5)]
    [InlineData(0.5, 0.5, -0.01)]
    [InlineData(0.5, 0.5, 1.01)]
    public void Classify_JustOutside_ReturnsOutside(double x, double y, double z)
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var result = RayCastClassifier.Classify(new Vec3(x, y, z), bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void Classify_NearCorner_InsideCube()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        // Near (0,0,0) corner, just inside
        var result = RayCastClassifier.Classify(new Vec3(0.001, 0.001, 0.001), bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void Classify_NearCorner_OutsideCube()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        // Near (0,0,0) corner, just outside
        var result = RayCastClassifier.Classify(new Vec3(-0.001, -0.001, -0.001), bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void Classify_InsideSphere_ReturnsInside()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        var result = RayCastClassifier.Classify(new Vec3(0, 0, 0), bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void Classify_OutsideSphere_ReturnsOutside()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        var result = RayCastClassifier.Classify(new Vec3(2, 0, 0), bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void Classify_InsideTetrahedron_ReturnsInside()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        var bvh = BvhTree.Build(mesh);
        var result = RayCastClassifier.Classify(new Vec3(0.25, 0.25, 0.25), bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void Classify_OutsideTetrahedron_ReturnsOutside()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        var bvh = BvhTree.Build(mesh);
        var result = RayCastClassifier.Classify(new Vec3(5, 5, 5), bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void Classify_OffsetCube_InsideAtCenter()
    {
        var mesh = MeshFactory.CreateCube(new Vec3(10, 10, 10)).Mesh;
        var bvh = BvhTree.Build(mesh);
        var result = RayCastClassifier.Classify(new Vec3(10.5, 10.5, 10.5), bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void Classify_OffsetCube_OriginIsOutside()
    {
        var mesh = MeshFactory.CreateCube(new Vec3(10, 10, 10)).Mesh;
        var bvh = BvhTree.Build(mesh);
        var result = RayCastClassifier.Classify(Vec3.Zero, bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void Classify_ScaledCube_InsideAtCenter()
    {
        var mesh = MeshFactory.CreateCube(size: 5).Mesh;
        var bvh = BvhTree.Build(mesh);
        var result = RayCastClassifier.Classify(new Vec3(2.5, 2.5, 2.5), bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void Classify_SmallCube_InsideAtCenter()
    {
        var mesh = MeshFactory.CreateCube(size: 0.1).Mesh;
        var bvh = BvhTree.Build(mesh);
        var result = RayCastClassifier.Classify(new Vec3(0.05, 0.05, 0.05), bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void Classify_FarFromMesh_QuicklyOutside()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var result = RayCastClassifier.Classify(new Vec3(1000, 1000, 1000), bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void Classify_NegativeCoordinates_OutsideCube()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var result = RayCastClassifier.Classify(new Vec3(-0.5, -0.5, -0.5), bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void Classify_OnAxisLine_MultipleCubePoints()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        // Walk along x-axis through center of cube
        for (double x = -0.5; x <= 1.5; x += 0.1)
        {
            var result = RayCastClassifier.Classify(new Vec3(x, 0.5, 0.5), bvh);
            bool shouldBeInside = x > 0 && x < 1;
            if (shouldBeInside)
                Assert.Equal(SolidClassification.Inside, result);
            else
                Assert.Equal(SolidClassification.Outside, result);
        }
    }
}
