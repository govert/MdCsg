using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Classification;

/// <summary>Phase 6: SolidClassification enum, RayCastClassifier, WindingNumberClassifier — direct classification tests</summary>
public class SolidClassificationEnumPropertyTests
{
    [Fact]
    public void Enum_HasInsideAndOutside()
    {
        var values = Enum.GetValues(typeof(SolidClassification));
        Assert.Equal(2, values.Length);
        Assert.Contains(SolidClassification.Inside, (SolidClassification[])values);
        Assert.Contains(SolidClassification.Outside, (SolidClassification[])values);
    }

    [Fact]
    public void Enum_InsideNotEqualOutside()
    {
        Assert.NotEqual(SolidClassification.Inside, SolidClassification.Outside);
    }

    [Fact]
    public void RayCast_PointInsideCube_IsInside()
    {
        var cube = MeshFactory.CreateCube();
        var result = RayCastClassifier.Classify(new Vec3(0.5, 0.5, 0.5), cube.Bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void RayCast_PointOutsideCube_IsOutside()
    {
        var cube = MeshFactory.CreateCube();
        var result = RayCastClassifier.Classify(new Vec3(5, 5, 5), cube.Bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void RayCast_PointFarNegative_IsOutside()
    {
        var cube = MeshFactory.CreateCube();
        var result = RayCastClassifier.Classify(new Vec3(-10, -10, -10), cube.Bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void WindingNumber_PointInsideCube_IsInside()
    {
        var cube = MeshFactory.CreateCube();
        var result = WindingNumberClassifier.Classify(new Vec3(0.5, 0.5, 0.5), cube.Bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void WindingNumber_PointOutsideCube_IsOutside()
    {
        var cube = MeshFactory.CreateCube();
        var result = WindingNumberClassifier.Classify(new Vec3(5, 5, 5), cube.Bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void WindingNumber_ValueInsideCube_NearOne()
    {
        var cube = MeshFactory.CreateCube();
        double wn = WindingNumberClassifier.ComputeWindingNumber(new Vec3(0.5, 0.5, 0.5), cube.Bvh);
        Assert.True(wn > 0.9 && wn < 1.1, $"Winding number inside cube should be ~1, got {wn}");
    }

    [Fact]
    public void WindingNumber_ValueOutsideCube_NearZero()
    {
        var cube = MeshFactory.CreateCube();
        double wn = WindingNumberClassifier.ComputeWindingNumber(new Vec3(5, 5, 5), cube.Bvh);
        Assert.True(System.Math.Abs(wn) < 0.1, $"Winding number outside cube should be ~0, got {wn}");
    }

    [Fact]
    public void RayCast_InsideSphere_IsInside()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var result = RayCastClassifier.Classify(Vec3.Zero, sphere.Bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void RayCast_OutsideSphere_IsOutside()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var result = RayCastClassifier.Classify(new Vec3(3, 0, 0), sphere.Bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void WindingNumber_InsideSphere_IsInside()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var result = WindingNumberClassifier.Classify(Vec3.Zero, sphere.Bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void RayCastAndWinding_AgreeOnCubeInterior()
    {
        var cube = MeshFactory.CreateCube();
        var point = new Vec3(0.3, 0.7, 0.2);
        var rc = RayCastClassifier.Classify(point, cube.Bvh);
        var wn = WindingNumberClassifier.Classify(point, cube.Bvh);
        Assert.Equal(rc, wn);
    }

    [Fact]
    public void RayCastAndWinding_AgreeOnCubeExterior()
    {
        var cube = MeshFactory.CreateCube();
        var point = new Vec3(2, 3, 4);
        var rc = RayCastClassifier.Classify(point, cube.Bvh);
        var wn = WindingNumberClassifier.Classify(point, cube.Bvh);
        Assert.Equal(rc, wn);
    }

    [Fact]
    public void WindingNumber_EmptyMesh_IsOutside()
    {
        var mesh = new MdCsg.Mesh.HalfEdgeMesh();
        var bvh = BvhTree.Build(mesh);
        var result = WindingNumberClassifier.Classify(Vec3.Zero, bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void WindingNumber_EmptyMesh_ValueIsZero()
    {
        var mesh = new MdCsg.Mesh.HalfEdgeMesh();
        var bvh = BvhTree.Build(mesh);
        double wn = WindingNumberClassifier.ComputeWindingNumber(Vec3.Zero, bvh);
        Assert.Equal(0.0, wn, 15);
    }

    [Fact]
    public void RayCast_InsideTetrahedron_IsInside()
    {
        var tet = MeshFactory.CreateTetrahedron();
        var result = RayCastClassifier.Classify(Vec3.Zero, tet.Bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void WindingNumber_InsideTetrahedron_IsInside()
    {
        var tet = MeshFactory.CreateTetrahedron();
        var result = WindingNumberClassifier.Classify(Vec3.Zero, tet.Bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void RayCast_InsideOffsetCube_IsInside()
    {
        var cube = MeshFactory.CreateCube(new Vec3(10, 20, 30));
        var result = RayCastClassifier.Classify(new Vec3(10.5, 20.5, 30.5), cube.Bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void WindingNumber_InsideOffsetCube_IsInside()
    {
        var cube = MeshFactory.CreateCube(new Vec3(10, 20, 30));
        var result = WindingNumberClassifier.Classify(new Vec3(10.5, 20.5, 30.5), cube.Bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }
}
