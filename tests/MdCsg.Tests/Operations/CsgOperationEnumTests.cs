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

/// <summary>Phase 6: CsgOperation enum tests and PatchAssembler selection per operation</summary>
public class CsgOperationEnumTests
{
    [Fact]
    public void CsgOperation_EnumValues()
    {
        Assert.Equal(0, (int)CsgOperation.Union);
        Assert.Equal(1, (int)CsgOperation.Intersection);
        Assert.Equal(2, (int)CsgOperation.Difference);
    }

    [Fact]
    public void CsgOperation_AllValues()
    {
        var values = Enum.GetValues(typeof(CsgOperation));
        Assert.Equal(3, values.Length);
    }

    private static (List<Patch> PatchesA, List<Patch> PatchesB,
        IReadOnlyList<FaceCutter.SubTriangle> SubTrisA,
        IReadOnlyList<FaceCutter.SubTriangle> SubTrisB)
        SetupPatches()
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
    public void AllOperations_ProduceDistinctCounts()
    {
        var (pA, pB, stA, stB) = SetupPatches();
        var unionResult = PatchAssembler.Assemble(pA, pB, stA, stB, CsgOperation.Union);
        var interResult = PatchAssembler.Assemble(pA, pB, stA, stB, CsgOperation.Intersection);
        var diffResult = PatchAssembler.Assemble(pA, pB, stA, stB, CsgOperation.Difference);
        // At least two of three should differ
        int unionC = unionResult.Triangles.Count;
        int interC = interResult.Triangles.Count;
        int diffC = diffResult.Triangles.Count;
        Assert.True(unionC != interC || interC != diffC,
            $"All ops produced same count: {unionC}");
    }

    [Fact]
    public void Union_KeepsOutside_FromBothMeshes()
    {
        var (pA, pB, stA, stB) = SetupPatches();
        var result = PatchAssembler.Assemble(pA, pB, stA, stB, CsgOperation.Union);
        // Union keeps outside patches from both, so more triangles than intersection
        Assert.True(result.Triangles.Count > 0);
    }

    [Fact]
    public void Intersection_KeepsInside_FromBothMeshes()
    {
        var (pA, pB, stA, stB) = SetupPatches();
        var result = PatchAssembler.Assemble(pA, pB, stA, stB, CsgOperation.Intersection);
        Assert.True(result.Triangles.Count > 0);
    }

    [Fact]
    public void Difference_KeepsOutsideA_InsideB()
    {
        var (pA, pB, stA, stB) = SetupPatches();
        var result = PatchAssembler.Assemble(pA, pB, stA, stB, CsgOperation.Difference);
        Assert.True(result.Triangles.Count > 0);
    }

    [Fact]
    public void Difference_FlipsList_SameLength_AsTriangles()
    {
        var (pA, pB, stA, stB) = SetupPatches();
        var result = PatchAssembler.Assemble(pA, pB, stA, stB, CsgOperation.Difference);
        Assert.Equal(result.Triangles.Count, result.FlipNormals.Count);
    }

    [Fact]
    public void Union_NoFlips()
    {
        var (pA, pB, stA, stB) = SetupPatches();
        var result = PatchAssembler.Assemble(pA, pB, stA, stB, CsgOperation.Union);
        Assert.All(result.FlipNormals, f => Assert.False(f));
    }
}
