using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Classification;

/// <summary>Phase 6: Winding number solid angle and edge case tests</summary>
public class WindingNumberSolidAngleTests
{
    [Fact]
    public void ComputeWindingNumber_InsideCube_Precise()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        double wn = WindingNumberClassifier.ComputeWindingNumber(new Vec3(0.5, 0.5, 0.5), bvh);
        // For a closed cube, winding number at center should be exactly 1.0
        Assert.True(System.Math.Abs(wn - 1.0) < 1e-10, $"Expected 1.0, got {wn}");
    }

    [Fact]
    public void ComputeWindingNumber_OutsideCube_Precise()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        double wn = WindingNumberClassifier.ComputeWindingNumber(new Vec3(5, 5, 5), bvh);
        Assert.True(System.Math.Abs(wn) < 1e-10, $"Expected 0.0, got {wn}");
    }

    [Theory]
    [InlineData(0.5, 0.5, 0.01)]   // near Z=0 face
    [InlineData(0.5, 0.5, 0.99)]   // near Z=1 face
    [InlineData(0.01, 0.5, 0.5)]   // near X=0 face
    [InlineData(0.99, 0.5, 0.5)]   // near X=1 face
    [InlineData(0.5, 0.01, 0.5)]   // near Y=0 face
    [InlineData(0.5, 0.99, 0.5)]   // near Y=1 face
    public void Classify_NearFace_InsideCube(double x, double y, double z)
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var result = WindingNumberClassifier.Classify(new Vec3(x, y, z), bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Theory]
    [InlineData(0.5, 0.5, -0.01)]
    [InlineData(0.5, 0.5, 1.01)]
    [InlineData(-0.01, 0.5, 0.5)]
    [InlineData(1.01, 0.5, 0.5)]
    [InlineData(0.5, -0.01, 0.5)]
    [InlineData(0.5, 1.01, 0.5)]
    public void Classify_JustOutsideFace_OutsideCube(double x, double y, double z)
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var result = WindingNumberClassifier.Classify(new Vec3(x, y, z), bvh);
        Assert.Equal(SolidClassification.Outside, result);
    }

    [Fact]
    public void ComputeWindingNumber_InsideSphere_AccurateForSubdiv3()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 3).Mesh;
        var bvh = BvhTree.Build(mesh);
        double wn = WindingNumberClassifier.ComputeWindingNumber(Vec3.Zero, bvh);
        // Higher subdivisions should give more accurate winding number
        Assert.True(System.Math.Abs(wn - 1.0) < 0.05, $"Sphere subdiv 3 center: {wn}");
    }

    [Fact]
    public void ComputeWindingNumber_OffsetCube_CenterIsOne()
    {
        var mesh = MeshFactory.CreateCube(new Vec3(10, 20, 30)).Mesh;
        var bvh = BvhTree.Build(mesh);
        double wn = WindingNumberClassifier.ComputeWindingNumber(new Vec3(10.5, 20.5, 30.5), bvh);
        Assert.True(System.Math.Abs(wn - 1.0) < 1e-10, $"Offset cube center: {wn}");
    }

    [Fact]
    public void ComputeWindingNumber_ScaledCube_CenterIsOne()
    {
        var mesh = MeshFactory.CreateCube(size: 5).Mesh;
        var bvh = BvhTree.Build(mesh);
        double wn = WindingNumberClassifier.ComputeWindingNumber(new Vec3(2.5, 2.5, 2.5), bvh);
        Assert.True(System.Math.Abs(wn - 1.0) < 1e-10, $"Scaled cube center: {wn}");
    }

    [Fact]
    public void Classify_NearCorner_InsideCube()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        // Near the (0,0,0) corner but inside
        var result = WindingNumberClassifier.Classify(new Vec3(0.01, 0.01, 0.01), bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void Classify_NearEdge_InsideCube()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        // Near the edge X=[0,1], Y=0, Z=0 but inside
        var result = WindingNumberClassifier.Classify(new Vec3(0.5, 0.01, 0.01), bvh);
        Assert.Equal(SolidClassification.Inside, result);
    }

    [Fact]
    public void RayCast_WindingNumber_Agree_MultipleSpherePoints()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var bvh = BvhTree.Build(mesh);
        var points = new[]
        {
            new Vec3(0.3, 0, 0),
            new Vec3(0, 0.3, 0),
            new Vec3(0, 0, 0.3),
            new Vec3(0.2, 0.2, 0.2),
            new Vec3(-0.3, -0.3, -0.3),
        };
        foreach (var p in points)
        {
            var rc = RayCastClassifier.Classify(p, bvh);
            var wn = WindingNumberClassifier.Classify(p, bvh);
            Assert.Equal(rc, wn);
        }
    }

    [Fact]
    public void RayCast_WindingNumber_Agree_OutsidePoints()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bvh = BvhTree.Build(mesh);
        var points = new[]
        {
            new Vec3(-1, 0.5, 0.5),
            new Vec3(2, 0.5, 0.5),
            new Vec3(0.5, -1, 0.5),
            new Vec3(0.5, 2, 0.5),
            new Vec3(0.5, 0.5, -1),
            new Vec3(0.5, 0.5, 2),
        };
        foreach (var p in points)
        {
            var rc = RayCastClassifier.Classify(p, bvh);
            var wn = WindingNumberClassifier.Classify(p, bvh);
            Assert.Equal(rc, wn);
        }
    }

    [Fact]
    public void WindingNumber_WithWindingOption_MatchesDefault()
    {
        // Test that PatchClassifier with useWindingNumber=true gives same results
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var bvhB = BvhTree.Build(meshB);
        var graph = MdCsg.Intersection.IntersectionGraph.Compute(meshA, meshB);
        var cutA = MdCsg.Cutting.MeshCutter.Cut(meshA,
            (Dictionary<int, List<MdCsg.Intersection.IntersectionSegment>>)graph.FaceSegmentsA);
        var adjA = MdCsg.Patches.SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patchesRc = MdCsg.Patches.PatchExtractor.Extract(cutA.SubTriangles, adjA);
        PatchClassifier.ClassifyAll(patchesRc, cutA.SubTriangles, bvhB, useWindingNumber: false);

        // Re-extract and classify with winding
        var patchesWn = MdCsg.Patches.PatchExtractor.Extract(cutA.SubTriangles, adjA);
        PatchClassifier.ClassifyAll(patchesWn, cutA.SubTriangles, bvhB, useWindingNumber: true);

        Assert.Equal(patchesRc.Count, patchesWn.Count);
        for (int i = 0; i < patchesRc.Count; i++)
        {
            Assert.Equal(patchesRc[i].IsInside, patchesWn[i].IsInside);
        }
    }
}
