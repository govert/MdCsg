using MdCsg.Api;
using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Operations;
using MdCsg.Patches;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Operations;

/// <summary>Phase 6: PatchAssembler selection logic tests — all 3 operations, normal flipping, triangle counts</summary>
public class PatchAssemblerSelectionTests
{
    private static (List<Patch> PatchesA, List<Patch> PatchesB,
        IReadOnlyList<FaceCutter.SubTriangle> SubTrisA,
        IReadOnlyList<FaceCutter.SubTriangle> SubTrisB)
        SetupClassifiedPatches()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var bvhA = BvhTree.Build(meshA);
        var bvhB = BvhTree.Build(meshB);
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        var cutB = MeshCutter.Cut(meshB, graph.FaceSegmentsB);
        var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var adjB = SubTriangleAdjacency.Build(cutB.SubTriangles);
        var patchesA = PatchExtractor.Extract(cutA.SubTriangles, adjA);
        var patchesB = PatchExtractor.Extract(cutB.SubTriangles, adjB);
        PatchClassifier.ClassifyAll(patchesA, cutA.SubTriangles, bvhB);
        PatchClassifier.ClassifyAll(patchesB, cutB.SubTriangles, bvhA);
        return (patchesA, patchesB, cutA.SubTriangles, cutB.SubTriangles);
    }

    [Fact]
    public void Union_ProducesTriangles()
    {
        var (pA, pB, stA, stB) = SetupClassifiedPatches();
        var result = PatchAssembler.Assemble(pA, pB, stA, stB, CsgOperation.Union);
        Assert.True(result.Triangles.Count > 0);
    }

    [Fact]
    public void Intersect_ProducesTriangles()
    {
        var (pA, pB, stA, stB) = SetupClassifiedPatches();
        var result = PatchAssembler.Assemble(pA, pB, stA, stB, CsgOperation.Intersection);
        Assert.True(result.Triangles.Count > 0);
    }

    [Fact]
    public void Difference_ProducesTriangles()
    {
        var (pA, pB, stA, stB) = SetupClassifiedPatches();
        var result = PatchAssembler.Assemble(pA, pB, stA, stB, CsgOperation.Difference);
        Assert.True(result.Triangles.Count > 0);
    }

    [Fact]
    public void Union_HasMoreTrianglesThanIntersect()
    {
        var (pA, pB, stA, stB) = SetupClassifiedPatches();
        var union = PatchAssembler.Assemble(pA, pB, stA, stB, CsgOperation.Union);
        var inter = PatchAssembler.Assemble(pA, pB, stA, stB, CsgOperation.Intersection);
        Assert.True(union.Triangles.Count > inter.Triangles.Count,
            $"Union ({union.Triangles.Count}) should have more triangles than Intersection ({inter.Triangles.Count})");
    }

    [Fact]
    public void Difference_FlipNormals_HasSomeFlipped()
    {
        var (pA, pB, stA, stB) = SetupClassifiedPatches();
        var result = PatchAssembler.Assemble(pA, pB, stA, stB, CsgOperation.Difference);
        // Difference flips B's inside normals
        Assert.Contains(result.FlipNormals, f => f == true);
    }

    [Fact]
    public void Union_NoFlippedNormals()
    {
        var (pA, pB, stA, stB) = SetupClassifiedPatches();
        var result = PatchAssembler.Assemble(pA, pB, stA, stB, CsgOperation.Union);
        // Union should not flip any normals
        Assert.All(result.FlipNormals, f => Assert.False(f));
    }

    [Fact]
    public void Intersect_NoFlippedNormals()
    {
        var (pA, pB, stA, stB) = SetupClassifiedPatches();
        var result = PatchAssembler.Assemble(pA, pB, stA, stB, CsgOperation.Intersection);
        Assert.All(result.FlipNormals, f => Assert.False(f));
    }

    [Fact]
    public void AllOperations_TriangleAndFlipCounts_Match()
    {
        var (pA, pB, stA, stB) = SetupClassifiedPatches();
        foreach (var op in new[] { CsgOperation.Union, CsgOperation.Intersection, CsgOperation.Difference })
        {
            var result = PatchAssembler.Assemble(pA, pB, stA, stB, op);
            Assert.Equal(result.Triangles.Count, result.FlipNormals.Count);
        }
    }

    [Fact]
    public void AllOperations_NoNaN_InTriangles()
    {
        var (pA, pB, stA, stB) = SetupClassifiedPatches();
        foreach (var op in new[] { CsgOperation.Union, CsgOperation.Intersection, CsgOperation.Difference })
        {
            var result = PatchAssembler.Assemble(pA, pB, stA, stB, op);
            foreach (var tri in result.Triangles)
            {
                Assert.False(double.IsNaN(tri.A.X));
                Assert.False(double.IsNaN(tri.B.Y));
                Assert.False(double.IsNaN(tri.C.Z));
            }
        }
    }

    [Fact]
    public void AllOperations_ProduceNonEmptyResult()
    {
        var (pA, pB, stA, stB) = SetupClassifiedPatches();
        foreach (var op in new[] { CsgOperation.Union, CsgOperation.Intersection, CsgOperation.Difference })
        {
            var result = PatchAssembler.Assemble(pA, pB, stA, stB, op);
            Assert.True(result.Triangles.Count > 0, $"Operation {op} produced empty result");
        }
    }
}
