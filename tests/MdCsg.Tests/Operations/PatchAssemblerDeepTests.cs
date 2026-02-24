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

/// <summary>Phase 6: PatchAssembler deep tests — assembly rules, flip normals, empty results</summary>
public class PatchAssemblerDeepTests
{
    [Fact]
    public void Assemble_Union_HasTriangles()
    {
        var (patchesA, patchesB, cutA, cutB) = SetupClassified();
        var assembly = PatchAssembler.Assemble(patchesA, patchesB, cutA.SubTriangles, cutB.SubTriangles, CsgOperation.Union);
        Assert.True(assembly.Triangles.Count > 0);
    }

    [Fact]
    public void Assemble_Intersection_HasTriangles()
    {
        var (patchesA, patchesB, cutA, cutB) = SetupClassified();
        var assembly = PatchAssembler.Assemble(patchesA, patchesB, cutA.SubTriangles, cutB.SubTriangles, CsgOperation.Intersection);
        Assert.True(assembly.Triangles.Count > 0);
    }

    [Fact]
    public void Assemble_Difference_HasTriangles()
    {
        var (patchesA, patchesB, cutA, cutB) = SetupClassified();
        var assembly = PatchAssembler.Assemble(patchesA, patchesB, cutA.SubTriangles, cutB.SubTriangles, CsgOperation.Difference);
        Assert.True(assembly.Triangles.Count > 0);
    }

    [Fact]
    public void Assemble_Union_TrianglesHavePositiveArea()
    {
        var (patchesA, patchesB, cutA, cutB) = SetupClassified();
        var assembly = PatchAssembler.Assemble(patchesA, patchesB, cutA.SubTriangles, cutB.SubTriangles, CsgOperation.Union);
        foreach (var tri in assembly.Triangles)
        {
            Assert.True(tri.Area > 0, "Assembled triangle should have positive area");
        }
    }

    [Fact]
    public void Assemble_Union_MoreTrianglesThanIntersection()
    {
        var (patchesA, patchesB, cutA, cutB) = SetupClassified();
        var unionAsm = PatchAssembler.Assemble(patchesA, patchesB, cutA.SubTriangles, cutB.SubTriangles, CsgOperation.Union);
        var intAsm = PatchAssembler.Assemble(patchesA, patchesB, cutA.SubTriangles, cutB.SubTriangles, CsgOperation.Intersection);
        Assert.True(unionAsm.Triangles.Count >= intAsm.Triangles.Count);
    }

    [Fact]
    public void Assemble_Disjoint_Union_HasAllTriangles()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh;
        var bvhA = BvhTree.Build(meshA);
        var bvhB = BvhTree.Build(meshB);
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        var cutB = MeshCutter.Cut(meshB, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsB);
        var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var adjB = SubTriangleAdjacency.Build(cutB.SubTriangles);
        var patchesA = PatchExtractor.Extract(cutA.SubTriangles, adjA);
        var patchesB = PatchExtractor.Extract(cutB.SubTriangles, adjB);
        PatchClassifier.ClassifyAll(patchesA, cutA.SubTriangles, bvhB);
        PatchClassifier.ClassifyAll(patchesB, cutB.SubTriangles, bvhA);

        var assembly = PatchAssembler.Assemble(patchesA, patchesB, cutA.SubTriangles, cutB.SubTriangles, CsgOperation.Union);
        // Disjoint: all faces from both meshes
        Assert.Equal(meshA.Faces.Count + meshB.Faces.Count, assembly.Triangles.Count);
    }

    [Fact]
    public void Assemble_Disjoint_Intersection_Empty()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh;
        var bvhA = BvhTree.Build(meshA);
        var bvhB = BvhTree.Build(meshB);
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        var cutB = MeshCutter.Cut(meshB, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsB);
        var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var adjB = SubTriangleAdjacency.Build(cutB.SubTriangles);
        var patchesA = PatchExtractor.Extract(cutA.SubTriangles, adjA);
        var patchesB = PatchExtractor.Extract(cutB.SubTriangles, adjB);
        PatchClassifier.ClassifyAll(patchesA, cutA.SubTriangles, bvhB);
        PatchClassifier.ClassifyAll(patchesB, cutB.SubTriangles, bvhA);

        var assembly = PatchAssembler.Assemble(patchesA, patchesB, cutA.SubTriangles, cutB.SubTriangles, CsgOperation.Intersection);
        Assert.Empty(assembly.Triangles);
    }

    [Fact]
    public void Assemble_Disjoint_Difference_HasOnlyA()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh;
        var bvhA = BvhTree.Build(meshA);
        var bvhB = BvhTree.Build(meshB);
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        var cutB = MeshCutter.Cut(meshB, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsB);
        var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var adjB = SubTriangleAdjacency.Build(cutB.SubTriangles);
        var patchesA = PatchExtractor.Extract(cutA.SubTriangles, adjA);
        var patchesB = PatchExtractor.Extract(cutB.SubTriangles, adjB);
        PatchClassifier.ClassifyAll(patchesA, cutA.SubTriangles, bvhB);
        PatchClassifier.ClassifyAll(patchesB, cutB.SubTriangles, bvhA);

        var assembly = PatchAssembler.Assemble(patchesA, patchesB, cutA.SubTriangles, cutB.SubTriangles, CsgOperation.Difference);
        Assert.Equal(meshA.Faces.Count, assembly.Triangles.Count);
    }

    [Fact]
    public void Assemble_AllTrianglesValid_NaN_Check()
    {
        var (patchesA, patchesB, cutA, cutB) = SetupClassified();
        foreach (var op in new[] { CsgOperation.Union, CsgOperation.Intersection, CsgOperation.Difference })
        {
            var assembly = PatchAssembler.Assemble(patchesA, patchesB, cutA.SubTriangles, cutB.SubTriangles, op);
            foreach (var tri in assembly.Triangles)
            {
                Assert.False(double.IsNaN(tri.A.X), $"{op}: NaN in triangle");
                Assert.False(double.IsNaN(tri.B.X), $"{op}: NaN in triangle");
                Assert.False(double.IsNaN(tri.C.X), $"{op}: NaN in triangle");
            }
        }
    }

    private (List<Patch> patchesA, List<Patch> patchesB, MeshCutter.CutResult cutA, MeshCutter.CutResult cutB) SetupClassified()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var bvhA = BvhTree.Build(meshA);
        var bvhB = BvhTree.Build(meshB);
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        var cutB = MeshCutter.Cut(meshB, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsB);
        var adjA = SubTriangleAdjacency.Build(cutA.SubTriangles);
        var adjB = SubTriangleAdjacency.Build(cutB.SubTriangles);
        var patchesA = PatchExtractor.Extract(cutA.SubTriangles, adjA);
        var patchesB = PatchExtractor.Extract(cutB.SubTriangles, adjB);
        PatchClassifier.ClassifyAll(patchesA, cutA.SubTriangles, bvhB);
        PatchClassifier.ClassifyAll(patchesB, cutB.SubTriangles, bvhA);
        return (patchesA, patchesB, cutA, cutB);
    }
}
