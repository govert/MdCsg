using MdCsg.Api;
using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Operations;
using MdCsg.Patches;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: End-to-end pipeline tests — step through each CSG stage manually</summary>
public class EndToEndPipelineTests
{
    [Fact]
    public void Pipeline_CubeCube_AllStagesSucceed()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;

        // Step 1: Intersection graph
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.True(graph.Segments.Count > 0);

        // Step 2: Cut meshes
        var cutA = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        var cutB = MeshCutter.Cut(meshB, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsB);
        Assert.True(cutA.SubTriangles.Count >= meshA.Faces.Count);
        Assert.True(cutB.SubTriangles.Count >= meshB.Faces.Count);

        // Step 3: Adjacency and patches
        var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var adjB = SubTriangleAdjacency.Build(cutB.SubTriangles);
        var patchesA = PatchExtractor.Extract(cutA.SubTriangles, adjA);
        var patchesB = PatchExtractor.Extract(cutB.SubTriangles, adjB);
        Assert.True(patchesA.Count >= 1);
        Assert.True(patchesB.Count >= 1);

        // Step 4: Classify
        var bvhA = BvhTree.Build(meshA);
        var bvhB = BvhTree.Build(meshB);
        PatchClassifier.ClassifyAll(patchesA, cutA.SubTriangles, bvhB);
        PatchClassifier.ClassifyAll(patchesB, cutB.SubTriangles, bvhA);

        // Step 5: Assemble (union)
        var assembly = PatchAssembler.Assemble(
            patchesA, patchesB,
            cutA.SubTriangles, cutB.SubTriangles,
            CsgOperation.Union);
        Assert.True(assembly.Triangles.Count > 0);

        // Step 6: Stitch
        var result = MeshStitcher.Stitch(assembly.Triangles);
        Assert.True(result.Faces.Count > 0);
    }

    [Fact]
    public void Pipeline_Disjoint_NoIntersections()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(10, 0, 0)).Mesh;

        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.Empty(graph.Segments);

        // Cut should produce no extra sub-triangles
        var cutA = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        Assert.Equal(meshA.Faces.Count, cutA.SubTriangles.Count);
    }

    [Fact]
    public void Pipeline_Contained_NoIntersections()
    {
        var meshA = MeshFactory.CreateCube(size: 3).Mesh; // big
        var meshB = MeshFactory.CreateCube(new Vec3(1, 1, 1), size: 0.5).Mesh; // small inside
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.Empty(graph.Segments);
    }

    [Fact]
    public void Pipeline_SphereCube_ManySegments()
    {
        var meshA = MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.6, 2).Mesh;
        var meshB = MeshFactory.CreateCube().Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.True(graph.Segments.Count > 10);
    }

    [Fact]
    public void Pipeline_PatchesPartitionSubTriangles()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var patchesA = PatchExtractor.Extract(cutA.SubTriangles, adjA);

        // All sub-triangles should be in exactly one patch
        var seen = new HashSet<int>();
        foreach (var patch in patchesA)
        {
            foreach (int idx in patch.SubTriangleIndices)
            {
                Assert.True(seen.Add(idx), $"Sub-triangle {idx} appears in multiple patches");
            }
        }
        Assert.Equal(cutA.SubTriangles.Count, seen.Count);
    }

    [Fact]
    public void Pipeline_AllOperations_ProduceResults()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);

        var u = Csg.Union(a, b);
        var i = Csg.Intersect(a, b);
        var d = Csg.Difference(a, b);

        Assert.True(u.FaceCount > 0);
        Assert.True(i.FaceCount > 0);
        Assert.True(d.FaceCount > 0);

        double vu = VolumeCalculator.ComputeAbsoluteVolume(u.Mesh);
        double vi = VolumeCalculator.ComputeAbsoluteVolume(i.Mesh);
        double vd = VolumeCalculator.ComputeAbsoluteVolume(d.Mesh);
        Assert.True(vu > vi);
        Assert.True(vd > 0);
    }

    [Fact]
    public void Pipeline_CutMesh_OriginalFaceIds_Contiguous()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);

        var originalFaceIds = cutA.SubTriangles.Select(st => st.OriginalFaceIndex).Distinct().ToList();
        // All original face IDs should be valid
        foreach (int id in originalFaceIds)
        {
            Assert.True(id >= 0 && id < meshA.Faces.Count);
        }
        // All original faces should be represented
        Assert.Equal(meshA.Faces.Count, originalFaceIds.Count);
    }
}
