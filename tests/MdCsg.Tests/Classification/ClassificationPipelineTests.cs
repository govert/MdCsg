using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Patches;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Classification;

/// <summary>Phase 6: Classification pipeline tests — full classify cycle, degenerate count, strategy consistency</summary>
public class ClassificationPipelineTests
{
    private static (List<Patch> Patches, IReadOnlyList<FaceCutter.SubTriangle> SubTriangles, BvhTree Bvh)
        SetupCubeCubeA(Vec3? offsetB = null)
    {
        var off = offsetB ?? new Vec3(0.5, 0.5, 0.5);
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(off).Mesh;
        var bvhB = BvhTree.Build(meshB);
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA,
            (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patches = PatchExtractor.Extract(cutA.SubTriangles, adjA);
        return (patches, cutA.SubTriangles, bvhB);
    }

    [Fact]
    public void ClassifyAll_AllPatchesGet_IsInside()
    {
        var (patches, subTris, bvh) = SetupCubeCubeA();
        PatchClassifier.ClassifyAll(patches, subTris, bvh);
        Assert.All(patches, p => Assert.NotNull(p.IsInside));
    }

    [Fact]
    public void ClassifyAll_ConfidentPoints_NotNaN()
    {
        var (patches, subTris, bvh) = SetupCubeCubeA();
        PatchClassifier.ClassifyAll(patches, subTris, bvh);
        foreach (var p in patches)
        {
            if (p.HasConfidentPoint)
            {
                Assert.False(double.IsNaN(p.ConfidentPoint.X));
                Assert.False(double.IsNaN(p.ConfidentPoint.Y));
                Assert.False(double.IsNaN(p.ConfidentPoint.Z));
            }
        }
    }

    [Fact]
    public void ClassifyAll_DegenerateCountSmall()
    {
        var (patches, subTris, bvh) = SetupCubeCubeA();
        int degCount = PatchClassifier.ClassifyAll(patches, subTris, bvh);
        Assert.True(degCount <= patches.Count / 2);
    }

    [Fact]
    public void ClassifyAll_InsideOutside_BothPresent()
    {
        var (patches, subTris, bvh) = SetupCubeCubeA();
        PatchClassifier.ClassifyAll(patches, subTris, bvh);
        Assert.Contains(patches, p => p.IsInside == true);
        Assert.Contains(patches, p => p.IsInside == false);
    }

    [Fact]
    public void ClassifyAll_WithWindingNumber_AllClassified()
    {
        var (patches, subTris, bvh) = SetupCubeCubeA();
        PatchClassifier.ClassifyAll(patches, subTris, bvh, useWindingNumber: true);
        Assert.All(patches, p => Assert.NotNull(p.IsInside));
    }

    [Fact]
    public void ClassifyAll_DifferentOffset_StillClassifies()
    {
        var (patches, subTris, bvh) = SetupCubeCubeA(new Vec3(0.3, 0.3, 0.3));
        PatchClassifier.ClassifyAll(patches, subTris, bvh);
        Assert.All(patches, p => Assert.NotNull(p.IsInside));
    }

    [Fact]
    public void ClassifyAll_LargeOffset_HasBothSides()
    {
        var (patches, subTris, bvh) = SetupCubeCubeA(new Vec3(0.8, 0, 0));
        PatchClassifier.ClassifyAll(patches, subTris, bvh);
        Assert.Contains(patches, p => p.IsInside == true);
        Assert.Contains(patches, p => p.IsInside == false);
    }

    [Fact]
    public void ClassifyAll_SphereCube_AllClassified()
    {
        var meshA = MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.6, 2).Mesh;
        var meshB = MeshFactory.CreateCube().Mesh;
        var bvhB = BvhTree.Build(meshB);
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA,
            (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patches = PatchExtractor.Extract(cutA.SubTriangles, adjA);
        PatchClassifier.ClassifyAll(patches, cutA.SubTriangles, bvhB);
        Assert.All(patches, p => Assert.NotNull(p.IsInside));
    }

    [Fact]
    public void CpuStrategy_MatchesStaticMethod()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var bvhB = BvhTree.Build(meshB);
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA,
            (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);

        var patches1 = PatchExtractor.Extract(cutA.SubTriangles, adjA);
        int deg1 = PatchClassifier.ClassifyAll(patches1, cutA.SubTriangles, bvhB);

        var patches2 = PatchExtractor.Extract(cutA.SubTriangles, adjA);
        var strategy = new CpuPatchClassificationStrategy();
        int deg2 = strategy.ClassifyAll(patches2, cutA.SubTriangles, bvhB, false);

        Assert.Equal(deg1, deg2);
        for (int i = 0; i < patches1.Count; i++)
            Assert.Equal(patches1[i].IsInside, patches2[i].IsInside);
    }
}
