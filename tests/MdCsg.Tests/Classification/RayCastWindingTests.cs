using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Classification;

/// <summary>Batch 21: RayCastClassifier and WindingNumberClassifier tests (20 tests)</summary>
public class RayCastWindingTests
{
    [Fact]
    public void RayCast_InsideCube_ReturnsInside()
    {
        var cube = MeshFactory.CreateCube();
        var result = RayCastClassifier.Classify(new Vec3(0.5, 0.5, 0.5), cube.Bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void RayCast_OutsideCube_ReturnsOutside()
    {
        var cube = MeshFactory.CreateCube();
        var result = RayCastClassifier.Classify(new Vec3(5, 5, 5), cube.Bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void RayCast_FarFromCube_ReturnsOutside()
    {
        var cube = MeshFactory.CreateCube();
        var result = RayCastClassifier.Classify(new Vec3(-10, -10, -10), cube.Bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void RayCast_InsideSphere_ReturnsInside()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1, 2);
        var result = RayCastClassifier.Classify(new Vec3(0, 0, 0), sphere.Bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void RayCast_OutsideSphere_ReturnsOutside()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1, 2);
        var result = RayCastClassifier.Classify(new Vec3(3, 0, 0), sphere.Bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void RayCast_InsideTetrahedron_ReturnsInside()
    {
        var tet = MeshFactory.CreateTetrahedron(Vec3.Zero, 2);
        // Centroid of tetrahedron at origin with size 2
        var result = RayCastClassifier.Classify(new Vec3(0, 0.3, 0), tet.Bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void RayCast_OutsideTetrahedron_ReturnsOutside()
    {
        var tet = MeshFactory.CreateTetrahedron(Vec3.Zero, 1);
        var result = RayCastClassifier.Classify(new Vec3(5, 5, 5), tet.Bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void RayCast_InsideOffsetCube_ReturnsInside()
    {
        var cube = MeshFactory.CreateCube(new Vec3(10, 10, 10));
        var result = RayCastClassifier.Classify(new Vec3(10.5, 10.5, 10.5), cube.Bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void RayCast_EmptyMesh_ReturnsOutside()
    {
        var mesh = new MdCsg.Mesh.HalfEdgeMesh();
        var bvh = BvhTree.Build(mesh);
        var result = RayCastClassifier.Classify(Vec3.Zero, bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void RayCast_NearSurface_StillClassifiesCorrectly()
    {
        var cube = MeshFactory.CreateCube();
        // Just inside the cube near the +X face
        var result = RayCastClassifier.Classify(new Vec3(0.99, 0.5, 0.5), cube.Bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    // --- WindingNumber ---

    [Fact]
    public void Winding_InsideCube_ReturnsInside()
    {
        var cube = MeshFactory.CreateCube();
        var result = WindingNumberClassifier.Classify(new Vec3(0.5, 0.5, 0.5), cube.Bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void Winding_OutsideCube_ReturnsOutside()
    {
        var cube = MeshFactory.CreateCube();
        var result = WindingNumberClassifier.Classify(new Vec3(5, 5, 5), cube.Bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void Winding_InsideSphere_ReturnsInside()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1, 2);
        var result = WindingNumberClassifier.Classify(new Vec3(0, 0, 0), sphere.Bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void Winding_OutsideSphere_ReturnsOutside()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1, 2);
        var result = WindingNumberClassifier.Classify(new Vec3(3, 0, 0), sphere.Bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void Winding_InsideCube_WindingNearOne()
    {
        var cube = MeshFactory.CreateCube();
        double wn = WindingNumberClassifier.ComputeWindingNumber(new Vec3(0.5, 0.5, 0.5), cube.Bvh);
        Assert.True(wn > 0.9 && wn < 1.1, $"Expected ~1.0, got {wn}");
    }

    [Fact]
    public void Winding_OutsideCube_WindingNearZero()
    {
        var cube = MeshFactory.CreateCube();
        double wn = WindingNumberClassifier.ComputeWindingNumber(new Vec3(5, 5, 5), cube.Bvh);
        Assert.True(System.Math.Abs(wn) < 0.1, $"Expected ~0.0, got {wn}");
    }

    [Fact]
    public void Winding_EmptyMesh_ReturnsOutside()
    {
        var mesh = new MdCsg.Mesh.HalfEdgeMesh();
        var bvh = BvhTree.Build(mesh);
        var result = WindingNumberClassifier.Classify(Vec3.Zero, bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void Winding_EmptyMesh_WindingZero()
    {
        var mesh = new MdCsg.Mesh.HalfEdgeMesh();
        var bvh = BvhTree.Build(mesh);
        Assert.Equal(0, WindingNumberClassifier.ComputeWindingNumber(Vec3.Zero, bvh));
    }

    [Fact]
    public void BothClassifiers_Agree_InsideCube()
    {
        var cube = MeshFactory.CreateCube();
        var pt = new Vec3(0.5, 0.5, 0.5);
        var rc = RayCastClassifier.Classify(pt, cube.Bvh);
        var wn = WindingNumberClassifier.Classify(pt, cube.Bvh);
        Assert.Equal(rc, wn);
    }

    [Fact]
    public void BothClassifiers_Agree_OutsideCube()
    {
        var cube = MeshFactory.CreateCube();
        var pt = new Vec3(5, 5, 5);
        var rc = RayCastClassifier.Classify(pt, cube.Bvh);
        var wn = WindingNumberClassifier.Classify(pt, cube.Bvh);
        Assert.Equal(rc, wn);
    }
}
