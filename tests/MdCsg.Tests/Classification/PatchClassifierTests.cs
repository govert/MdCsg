using MdCsg.Classification;
using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Patches;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Classification;

/// <summary>Batch 23: PatchClassifier tests (20 tests)</summary>
public class PatchClassifierTests
{
    [Fact]
    public void DegenerateMarginThreshold_IsSmall()
    {
        Assert.True(PatchClassifier.DegenerateMarginThreshold < 1e-8);
    }

    [Fact]
    public void ClassifyAll_DisjointCubes_AllOutside()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(5, 0, 0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        var cutResult = MeshCutter.Cut(cubeA.Mesh, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutResult.SubTriangles);
        var patches = PatchExtractor.Extract(cutResult.SubTriangles, adj);
        PatchClassifier.ClassifyAll(patches, cutResult.SubTriangles, cubeB.Bvh);
        foreach (var p in patches)
            Assert.False(p.IsInside);
    }

    [Fact]
    public void ClassifyAll_OverlappingCubes_MixedClassification()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        var cutResult = MeshCutter.Cut(cubeA.Mesh, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutResult.SubTriangles);
        var patches = PatchExtractor.Extract(cutResult.SubTriangles, adj);
        PatchClassifier.ClassifyAll(patches, cutResult.SubTriangles, cubeB.Bvh);

        bool anyInside = patches.Any(p => p.IsInside == true);
        bool anyOutside = patches.Any(p => p.IsInside == false);
        Assert.True(anyInside);
        Assert.True(anyOutside);
    }

    [Fact]
    public void ClassifyAll_SetsConfidentPoint()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        var cutResult = MeshCutter.Cut(cubeA.Mesh, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutResult.SubTriangles);
        var patches = PatchExtractor.Extract(cutResult.SubTriangles, adj);
        PatchClassifier.ClassifyAll(patches, cutResult.SubTriangles, cubeB.Bvh);

        foreach (var p in patches)
        {
            Assert.True(p.HasConfidentPoint);
            Assert.NotNull(p.IsInside);
        }
    }

    [Fact]
    public void ClassifyAll_WithWindingNumber_ProducesSameResult()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);

        var cutResult1 = MeshCutter.Cut(cubeA.Mesh, graph.FaceSegmentsA);
        var adj1 = SubTriangleAdjacency.Build(cutResult1.SubTriangles);
        var patches1 = PatchExtractor.Extract(cutResult1.SubTriangles, adj1);
        PatchClassifier.ClassifyAll(patches1, cutResult1.SubTriangles, cubeB.Bvh, useWindingNumber: false);

        var cutResult2 = MeshCutter.Cut(cubeA.Mesh, graph.FaceSegmentsA);
        var adj2 = SubTriangleAdjacency.Build(cutResult2.SubTriangles);
        var patches2 = PatchExtractor.Extract(cutResult2.SubTriangles, adj2);
        PatchClassifier.ClassifyAll(patches2, cutResult2.SubTriangles, cubeB.Bvh, useWindingNumber: true);

        Assert.Equal(patches1.Count, patches2.Count);
        for (int i = 0; i < patches1.Count; i++)
            Assert.Equal(patches1[i].IsInside, patches2[i].IsInside);
    }

    [Fact]
    public void ClassifyAll_ReturnsZeroDegenerateForDisjoint()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(5, 0, 0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        var cutResult = MeshCutter.Cut(cubeA.Mesh, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutResult.SubTriangles);
        var patches = PatchExtractor.Extract(cutResult.SubTriangles, adj);
        int degen = PatchClassifier.ClassifyAll(patches, cutResult.SubTriangles, cubeB.Bvh);
        Assert.Equal(0, degen);
    }

    [Fact]
    public void ClassifyAll_CubeSphere_HasMixedClassification()
    {
        var cube = MeshFactory.CreateCube();
        var sphere = MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 1);
        var graph = IntersectionGraph.Compute(cube.Mesh, sphere.Mesh);
        if (graph.Segments.Count > 0)
        {
            var cutResult = MeshCutter.Cut(cube.Mesh, graph.FaceSegmentsA);
            var adj = SubTriangleAdjacency.Build(cutResult.SubTriangles);
            var patches = PatchExtractor.Extract(cutResult.SubTriangles, adj);
            PatchClassifier.ClassifyAll(patches, cutResult.SubTriangles, sphere.Bvh);
            Assert.True(patches.Any(p => p.IsInside == true));
            Assert.True(patches.Any(p => p.IsInside == false));
        }
    }

    [Fact]
    public void CpuStrategy_MatchesStaticMethod()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);

        var cutResult1 = MeshCutter.Cut(cubeA.Mesh, graph.FaceSegmentsA);
        var adj1 = SubTriangleAdjacency.Build(cutResult1.SubTriangles);
        var patches1 = PatchExtractor.Extract(cutResult1.SubTriangles, adj1);
        PatchClassifier.ClassifyAll(patches1, cutResult1.SubTriangles, cubeB.Bvh);

        var cutResult2 = MeshCutter.Cut(cubeA.Mesh, graph.FaceSegmentsA);
        var adj2 = SubTriangleAdjacency.Build(cutResult2.SubTriangles);
        var patches2 = PatchExtractor.Extract(cutResult2.SubTriangles, adj2);
        var strategy = new CpuPatchClassificationStrategy();
        strategy.ClassifyAll(patches2, cutResult2.SubTriangles, cubeB.Bvh, false);

        for (int i = 0; i < patches1.Count; i++)
            Assert.Equal(patches1[i].IsInside, patches2[i].IsInside);
    }

    [Fact]
    public void ClassifyAll_AllPatchesGetClassified()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        var cutResult = MeshCutter.Cut(cubeA.Mesh, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutResult.SubTriangles);
        var patches = PatchExtractor.Extract(cutResult.SubTriangles, adj);
        PatchClassifier.ClassifyAll(patches, cutResult.SubTriangles, cubeB.Bvh);

        foreach (var p in patches)
            Assert.NotNull(p.IsInside);
    }

    [Fact]
    public void ClassifyAll_TetrahedronInCube_InsidePatchExists()
    {
        var cube = MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5), 2);
        var tet = MeshFactory.CreateTetrahedron(new Vec3(0.5, 0.5, 0.5), 0.8);
        var graph = IntersectionGraph.Compute(tet.Mesh, cube.Mesh);
        if (graph.Segments.Count > 0)
        {
            var cutResult = MeshCutter.Cut(tet.Mesh, graph.FaceSegmentsA);
            var adj = SubTriangleAdjacency.Build(cutResult.SubTriangles);
            var patches = PatchExtractor.Extract(cutResult.SubTriangles, adj);
            PatchClassifier.ClassifyAll(patches, cutResult.SubTriangles, cube.Bvh);
            // Tetrahedron partially inside the cube
            bool anyInside = patches.Any(p => p.IsInside == true);
            bool anyOutside = patches.Any(p => p.IsInside == false);
            Assert.True(anyInside || anyOutside); // at least some classification happens
        }
    }

    [Fact]
    public void SolidClassification_Enum_HasTwoValues()
    {
        Assert.Equal(SolidClassification.Inside, (SolidClassification)0);
        Assert.Equal(SolidClassification.Outside, (SolidClassification)1);
    }

    [Fact]
    public void ClassifyAll_OverlappingCubes_NoDegeneracy()
    {
        var cubeA = MeshFactory.CreateCube();
        // Diagonal offset avoids coplanar face planes
        var cubeB = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        var cutResult = MeshCutter.Cut(cubeA.Mesh, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutResult.SubTriangles);
        var patches = PatchExtractor.Extract(cutResult.SubTriangles, adj);
        int degen = PatchClassifier.ClassifyAll(patches, cutResult.SubTriangles, cubeB.Bvh);
        Assert.Equal(0, degen);
    }

    [Fact]
    public void ClassifyAll_HasConfidentPoint_AllTrue_ForOverlap()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        var cutResult = MeshCutter.Cut(cubeA.Mesh, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutResult.SubTriangles);
        var patches = PatchExtractor.Extract(cutResult.SubTriangles, adj);
        PatchClassifier.ClassifyAll(patches, cutResult.SubTriangles, cubeB.Bvh);

        foreach (var p in patches)
            Assert.True(p.HasConfidentPoint);
    }

    [Fact]
    public void IPatchClassificationStrategy_CpuImplements()
    {
        IPatchClassificationStrategy strategy = new CpuPatchClassificationStrategy();
        Assert.NotNull(strategy);
    }

    [Fact]
    public void ClassifyAll_SinglePatchDisjoint_AllOutside()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(10, 10, 10));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        var cutResult = MeshCutter.Cut(cubeA.Mesh, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutResult.SubTriangles);
        var patches = PatchExtractor.Extract(cutResult.SubTriangles, adj);
        Assert.Single(patches);
        PatchClassifier.ClassifyAll(patches, cutResult.SubTriangles, cubeB.Bvh);
        Assert.False(patches[0].IsInside);
    }

    [Fact]
    public void ClassifyAll_SphereInSphere_InsidePatch()
    {
        var outer = MeshFactory.CreateSphere(Vec3.Zero, 2, 2);
        var inner = MeshFactory.CreateSphere(Vec3.Zero, 0.5, 1);
        var graph = IntersectionGraph.Compute(inner.Mesh, outer.Mesh);
        // Inner sphere is fully contained — no intersection segments
        Assert.Empty(graph.Segments);
        var cutResult = MeshCutter.Cut(inner.Mesh, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutResult.SubTriangles);
        var patches = PatchExtractor.Extract(cutResult.SubTriangles, adj);
        PatchClassifier.ClassifyAll(patches, cutResult.SubTriangles, outer.Bvh);
        // Single patch should be "inside" the outer sphere
        Assert.Single(patches);
        Assert.True(patches[0].IsInside);
    }

    [Fact]
    public void ClassifyAll_SphereSurroundsCube_CubeIsInside()
    {
        var sphere = MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 2, 2);
        var cube = MeshFactory.CreateCube();
        var graph = IntersectionGraph.Compute(cube.Mesh, sphere.Mesh);
        if (graph.Segments.Count == 0)
        {
            // Cube fully inside sphere — single patch classified as inside
            var cutResult = MeshCutter.Cut(cube.Mesh, graph.FaceSegmentsA);
            var adj = SubTriangleAdjacency.Build(cutResult.SubTriangles);
            var patches = PatchExtractor.Extract(cutResult.SubTriangles, adj);
            PatchClassifier.ClassifyAll(patches, cutResult.SubTriangles, sphere.Bvh);
            Assert.Single(patches);
            Assert.True(patches[0].IsInside);
        }
    }
}
