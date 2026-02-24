using MdCsg.Api;
using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Classification;

/// <summary>Phase 6: Classification edge cases — ConfidentPoint, RayCast, WindingNumber</summary>
public class ClassificationEdgeCaseTests
{
    [Fact]
    public void ConfidentPoint_PointTriangleDistSq_InsideTriangle()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(0.25, 0.25, 0);
        double distSq = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.True(distSq < 1e-20, $"Point on triangle should have ~0 distance, got {distSq}");
    }

    [Fact]
    public void ConfidentPoint_PointTriangleDistSq_AboveTriangle()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(0.25, 0.25, 3);
        double distSq = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.Equal(9.0, distSq, 5);
    }

    [Fact]
    public void ConfidentPoint_PointTriangleDistSq_NearVertex()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(-1, 0, 0);
        double distSq = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.Equal(1.0, distSq, 5);
    }

    [Fact]
    public void ConfidentPoint_PointTriangleDistSq_NearEdge()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(0.5, -1, 0);
        double distSq = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.Equal(1.0, distSq, 5);
    }

    [Fact]
    public void RayCastClassifier_InsideCube()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var result = RayCastClassifier.Classify(new Vec3(0.3, 0.4, 0.5), bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void RayCastClassifier_OutsideCube()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var result = RayCastClassifier.Classify(new Vec3(5, 5, 5), bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void RayCastClassifier_InsideSphere()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        var result = RayCastClassifier.Classify(new Vec3(0.1, 0.1, 0.1), bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void RayCastClassifier_OutsideSphere()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        var result = RayCastClassifier.Classify(new Vec3(5, 0, 0), bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void WindingNumberClassifier_InsideCube()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var result = WindingNumberClassifier.Classify(new Vec3(0.3, 0.4, 0.5), bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void WindingNumberClassifier_OutsideCube()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var result = WindingNumberClassifier.Classify(new Vec3(5, 5, 5), bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void WindingNumberClassifier_InsideSphere()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        var result = WindingNumberClassifier.Classify(new Vec3(0.1, 0.1, 0.1), bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void WindingNumberClassifier_OutsideSphere()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        var result = WindingNumberClassifier.Classify(new Vec3(5, 0, 0), bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void RayCast_And_WindingNumber_Agree_CubeInside()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var point = new Vec3(0.3, 0.4, 0.5);
        var rc = RayCastClassifier.Classify(point, bvh);
        var wn = WindingNumberClassifier.Classify(point, bvh);
        Assert.Equal(rc, wn);
    }

    [Fact]
    public void RayCast_And_WindingNumber_Agree_CubeOutside()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var point = new Vec3(5, 5, 5);
        var rc = RayCastClassifier.Classify(point, bvh);
        var wn = WindingNumberClassifier.Classify(point, bvh);
        Assert.Equal(rc, wn);
    }

    [Fact]
    public void CsgOptions_WindingNumber_DoesNotCrash()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var options = new CsgOptions { UseWindingNumber = true };
        var r = Csg.Union(a, b, options);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void CsgOptions_CustomGridSize()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var options = new CsgOptions { GridSize = 1e-6 };
        var r = Csg.Union(a, b, options);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void CsgOptions_CustomWeldTolerance()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var options = new CsgOptions { WeldTolerance = 1e-6 };
        var r = Csg.Union(a, b, options);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void RayCast_FarFromMesh_Outside()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var result = RayCastClassifier.Classify(new Vec3(1e6, 0, 0), bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void WindingNumber_FarFromMesh_Outside()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var result = WindingNumberClassifier.Classify(new Vec3(1e6, 0, 0), bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void SolidClassification_Values()
    {
        Assert.NotEqual(SolidClassification.Inside, SolidClassification.Outside);
    }
}
