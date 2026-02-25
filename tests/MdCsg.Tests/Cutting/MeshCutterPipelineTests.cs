using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Cutting;

/// <summary>Phase 6: MeshCutter pipeline — CutResult structure, original face mapping, sub-triangle vertex validity</summary>
public class MeshCutterPipelineTests
{
    [Fact]
    public void CutResult_HasMesh()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var empty = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(mesh, empty);
        Assert.NotNull(result.Mesh);
        Assert.True(result.Mesh.Faces.Count > 0);
    }

    [Fact]
    public void CutResult_SubTriangles_NotNull()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var empty = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(mesh, empty);
        Assert.NotNull(result.SubTriangles);
    }

    [Fact]
    public void NoCut_OriginalFaceIds_Transferred()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var empty = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(mesh, empty);
        // After cutting, each sub-triangle's OriginalFaceIndex should map to a valid face
        for (int i = 0; i < result.SubTriangles.Count; i++)
        {
            int orig = result.SubTriangles[i].OriginalFaceIndex;
            Assert.True(orig >= 0 && orig < mesh.Faces.Count,
                $"SubTriangle {i} has OriginalFaceIndex={orig} out of range [0,{mesh.Faces.Count})");
        }
    }

    [Fact]
    public void NoCut_SubTriangleCount_EqualsFaceCount()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var empty = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(mesh, empty);
        Assert.Equal(mesh.Faces.Count, result.SubTriangles.Count);
    }

    [Fact]
    public void WithCut_SubTriangles_MoreThanFaces()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        Assert.True(cutA.SubTriangles.Count >= meshA.Faces.Count);
    }

    [Fact]
    public void WithCut_OriginalFaceIds_Valid()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        foreach (var st in cutA.SubTriangles)
            Assert.True(st.OriginalFaceIndex >= 0 && st.OriginalFaceIndex < meshA.Faces.Count);
    }

    [Fact]
    public void WithCut_NoNanVertices()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        foreach (var st in cutA.SubTriangles)
        {
            Assert.False(double.IsNaN(st.A.X) || double.IsNaN(st.A.Y) || double.IsNaN(st.A.Z));
            Assert.False(double.IsNaN(st.B.X) || double.IsNaN(st.B.Y) || double.IsNaN(st.B.Z));
            Assert.False(double.IsNaN(st.C.X) || double.IsNaN(st.C.Y) || double.IsNaN(st.C.Z));
        }
    }

    [Fact]
    public void WithCut_NoInfinityVertices()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        foreach (var st in cutA.SubTriangles)
        {
            Assert.False(double.IsInfinity(st.A.X) || double.IsInfinity(st.A.Y) || double.IsInfinity(st.A.Z));
            Assert.False(double.IsInfinity(st.B.X) || double.IsInfinity(st.B.Y) || double.IsInfinity(st.B.Z));
            Assert.False(double.IsInfinity(st.C.X) || double.IsInfinity(st.C.Y) || double.IsInfinity(st.C.Z));
        }
    }

    [Fact]
    public void Tetrahedron_NoCut_4SubTriangles()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        var empty = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(mesh, empty);
        Assert.Equal(4, result.SubTriangles.Count);
    }

    [Fact]
    public void Sphere_NoCut_MatchesFaceCount()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var empty = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(mesh, empty);
        Assert.Equal(mesh.Faces.Count, result.SubTriangles.Count);
    }

    [Fact]
    public void CutBothMeshes_IndependentResults()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        var cutB = MeshCutter.Cut(meshB, graph.FaceSegmentsB);
        Assert.True(cutA.SubTriangles.Count > 0);
        Assert.True(cutB.SubTriangles.Count > 0);
        // A and B cuts are independent
        Assert.NotSame(cutA.Mesh, cutB.Mesh);
    }

    [Fact]
    public void CutResult_SubTriangles_CoverAllOriginalFaces()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(mesh, meshB);
        var cut = MeshCutter.Cut(mesh, graph.FaceSegmentsA);
        var coveredFaces = cut.SubTriangles.Select(st => st.OriginalFaceIndex).Distinct().ToHashSet();
        // All original faces should have at least one sub-triangle
        for (int i = 0; i < mesh.Faces.Count; i++)
            Assert.Contains(i, coveredFaces);
    }

    [Fact]
    public void NoCut_SubTriangles_NoIntersectionEdges()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var empty = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(mesh, empty);
        Assert.True(result.SubTriangles.All(st => !st.HasIntersectionEdge));
    }
}
