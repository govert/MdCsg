using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Classification;

/// <summary>Phase 6: WindingNumberClassifier — compute winding number values, threshold behavior</summary>
public class WindingNumberComputeValueTests
{
    [Fact]
    public void InsideCube_WindingNumber_Near1()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        double wn = WindingNumberClassifier.ComputeWindingNumber(new Vec3(0.5, 0.5, 0.5), bvh);
        Assert.True(System.Math.Abs(wn - 1.0) < 0.1, $"Expected ~1.0, got {wn}");
    }

    [Fact]
    public void OutsideCube_WindingNumber_Near0()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        double wn = WindingNumberClassifier.ComputeWindingNumber(new Vec3(5, 5, 5), bvh);
        Assert.True(System.Math.Abs(wn) < 0.1, $"Expected ~0.0, got {wn}");
    }

    [Fact]
    public void InsideSphere_WindingNumber_Near1()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh);
        double wn = WindingNumberClassifier.ComputeWindingNumber(Vec3.Zero, bvh);
        Assert.True(System.Math.Abs(wn - 1.0) < 0.15, $"Expected ~1.0, got {wn}");
    }

    [Fact]
    public void FarOutside_WindingNumber_NearZero()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        double wn = WindingNumberClassifier.ComputeWindingNumber(new Vec3(1000, 1000, 1000), bvh);
        Assert.True(System.Math.Abs(wn) < 0.01, $"Expected ~0.0, got {wn}");
    }

    [Fact]
    public void WindingNumber_Positive_Inside()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        double wn = WindingNumberClassifier.ComputeWindingNumber(new Vec3(0.5, 0.5, 0.5), bvh);
        Assert.True(wn > 0.5, $"Expected > 0.5 for inside point, got {wn}");
    }

    [Fact]
    public void WindingNumber_BelowThreshold_Outside()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        double wn = WindingNumberClassifier.ComputeWindingNumber(new Vec3(5, 5, 5), bvh);
        Assert.True(wn < 0.5, $"Expected < 0.5 for outside point, got {wn}");
    }

    [Fact]
    public void TranslatedCube_InsidePoint()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube(new Vec3(10, 10, 10)).Mesh);
        double wn = WindingNumberClassifier.ComputeWindingNumber(new Vec3(10.5, 10.5, 10.5), bvh);
        Assert.True(System.Math.Abs(wn - 1.0) < 0.1, $"Expected ~1.0, got {wn}");
    }

    [Fact]
    public void TranslatedCube_OutsidePoint()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube(new Vec3(10, 10, 10)).Mesh);
        double wn = WindingNumberClassifier.ComputeWindingNumber(Vec3.Zero, bvh);
        Assert.True(System.Math.Abs(wn) < 0.1, $"Expected ~0.0, got {wn}");
    }

    [Fact]
    public void MultipleInsidePoints_AllNear1()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var pts = new[]
        {
            new Vec3(0.2, 0.2, 0.2),
            new Vec3(0.8, 0.8, 0.8),
            new Vec3(0.5, 0.1, 0.9),
        };
        foreach (var pt in pts)
        {
            double wn = WindingNumberClassifier.ComputeWindingNumber(pt, bvh);
            Assert.True(System.Math.Abs(wn - 1.0) < 0.1, $"Point {pt}: expected ~1.0, got {wn}");
        }
    }

    [Fact]
    public void MultipleOutsidePoints_AllNear0()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var pts = new[]
        {
            new Vec3(-1, -1, -1),
            new Vec3(2, 2, 2),
            new Vec3(5, 0.5, 0.5),
        };
        foreach (var pt in pts)
        {
            double wn = WindingNumberClassifier.ComputeWindingNumber(pt, bvh);
            Assert.True(System.Math.Abs(wn) < 0.1, $"Point {pt}: expected ~0.0, got {wn}");
        }
    }

    [Fact]
    public void Classify_MatchesThreshold()
    {
        // Verify that Classify uses 0.5 threshold
        var bvh = BvhTree.Build(MeshFactory.CreateCube().Mesh);
        var pt = new Vec3(0.5, 0.5, 0.5);
        double wn = WindingNumberClassifier.ComputeWindingNumber(pt, bvh);
        var classification = WindingNumberClassifier.Classify(pt, bvh);

        if (wn > 0.5)
            Assert.Equal(SolidClassification.Inside, classification);
        else
            Assert.Equal(SolidClassification.Outside, classification);
    }

    [Fact]
    public void InsideTetrahedron_AboveThreshold()
    {
        var bvh = BvhTree.Build(MeshFactory.CreateTetrahedron().Mesh);
        var pt = new Vec3(0.5, 0.29, 0.2);
        double wn = WindingNumberClassifier.ComputeWindingNumber(pt, bvh);
        Assert.True(wn > 0.5, $"Expected > 0.5 for inside tetrahedron point, got {wn}");
    }
}
