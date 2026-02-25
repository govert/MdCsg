using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Cutting;

/// <summary>Phase 6: MeshCutter integration tests — CutResult properties, face mapping, no-op cuts</summary>
public class MeshCutterIntegrationTests
{
    [Fact]
    public void Cut_NoSegments_SubTrianglesMatchFaces()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var empty = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(mesh, empty);
        Assert.Equal(mesh.Faces.Count, result.SubTriangles.Count);
    }

    [Fact]
    public void Cut_NoSegments_OriginalFaceIndicesSequential()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var empty = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(mesh, empty);
        for (int i = 0; i < result.SubTriangles.Count; i++)
            Assert.Equal(i, result.SubTriangles[i].OriginalFaceIndex);
    }

    [Fact]
    public void Cut_NoSegments_NoIntersectionEdges()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var empty = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(mesh, empty);
        Assert.All(result.SubTriangles, st => Assert.False(st.HasIntersectionEdge));
    }

    [Fact]
    public void Cut_WithIntersection_MoreSubTriangles()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA,
            (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        Assert.True(cutA.SubTriangles.Count > meshA.Faces.Count);
    }

    [Fact]
    public void Cut_AllSubTriangles_ValidOriginalFaceIndex()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA,
            (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        Assert.All(cutA.SubTriangles, st =>
            Assert.True(st.OriginalFaceIndex >= 0 && st.OriginalFaceIndex < meshA.Faces.Count));
    }

    [Fact]
    public void Cut_HasIntersectionEdges()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA,
            (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        Assert.Contains(cutA.SubTriangles, st => st.HasIntersectionEdge);
    }

    [Fact]
    public void Cut_SphereVsCube_Produces()
    {
        var meshA = MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.6, 2).Mesh;
        var meshB = MeshFactory.CreateCube().Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA,
            (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        Assert.True(cutA.SubTriangles.Count >= meshA.Faces.Count);
    }

    [Fact]
    public void Cut_Tetrahedron_NoCut()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        var empty = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(mesh, empty);
        Assert.Equal(4, result.SubTriangles.Count);
    }

    [Fact]
    public void CutResult_MeshFaceCount_MatchesSubTriangles()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var empty = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(mesh, empty);
        Assert.Equal(result.SubTriangles.Count, result.Mesh.Faces.Count);
    }
}
