using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Tests.Classification;

public class ClassificationTests
{
    private static (HalfEdgeMesh Mesh, BvhTree Bvh) BuildCube(Vec3 offset = default)
    {
        var positions = new Vec3[]
        {
            new Vec3(0, 0, 0) + offset, new Vec3(1, 0, 0) + offset,
            new Vec3(1, 1, 0) + offset, new Vec3(0, 1, 0) + offset,
            new Vec3(0, 0, 1) + offset, new Vec3(1, 0, 1) + offset,
            new Vec3(1, 1, 1) + offset, new Vec3(0, 1, 1) + offset,
        };

        var triangles = new (int, int, int)[]
        {
            (0, 2, 1), (0, 3, 2),
            (4, 5, 6), (4, 6, 7),
            (0, 1, 5), (0, 5, 4),
            (2, 3, 7), (2, 7, 6),
            (0, 4, 7), (0, 7, 3),
            (1, 2, 6), (1, 6, 5),
        };

        var mesh = new MeshBuilder().Build(positions, triangles);
        var bvh = BvhTree.Build(mesh);
        return (mesh, bvh);
    }

    [Fact]
    public void RayCast_PointInsideCube_IsInside()
    {
        var (_, bvh) = BuildCube();
        var result = RayCastClassifier.Classify(new Vec3(0.5, 0.5, 0.5), bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void RayCast_PointOutsideCube_IsOutside()
    {
        var (_, bvh) = BuildCube();
        var result = RayCastClassifier.Classify(new Vec3(5, 5, 5), bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void WindingNumber_PointInsideCube_IsInside()
    {
        var (_, bvh) = BuildCube();
        var result = WindingNumberClassifier.Classify(new Vec3(0.5, 0.5, 0.5), bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void WindingNumber_PointOutsideCube_IsOutside()
    {
        var (_, bvh) = BuildCube();
        var result = WindingNumberClassifier.Classify(new Vec3(5, 5, 5), bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void WindingNumber_InsideCube_ValueNearOne()
    {
        var (_, bvh) = BuildCube();
        double wn = WindingNumberClassifier.ComputeWindingNumber(new Vec3(0.5, 0.5, 0.5), bvh);
        Assert.Equal(1.0, wn, 0.01);
    }

    [Fact]
    public void WindingNumber_OutsideCube_ValueNearZero()
    {
        var (_, bvh) = BuildCube();
        double wn = WindingNumberClassifier.ComputeWindingNumber(new Vec3(5, 5, 5), bvh);
        Assert.Equal(0.0, wn, 0.01);
    }

    [Fact]
    public void RayCast_PointNearEdge_StillClassifiesCorrectly()
    {
        var (_, bvh) = BuildCube();
        // Point near an edge but clearly inside
        var result = RayCastClassifier.Classify(new Vec3(0.01, 0.01, 0.5), bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void PointTriangleDistance_PointAboveTriangle()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(0.25, 0.25, 1); // directly above

        double distSq = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.Equal(1.0, distSq, 1e-10);
    }

    [Fact]
    public void PointTriangleDistance_PointOnTriangle()
    {
        var a = new Vec3(0, 0, 0);
        var b = new Vec3(1, 0, 0);
        var c = new Vec3(0, 1, 0);
        var p = new Vec3(0.25, 0.25, 0); // on the triangle

        double distSq = ConfidentPoint.PointTriangleDistanceSq(p, a, b, c);
        Assert.Equal(0.0, distSq, 1e-10);
    }
}
