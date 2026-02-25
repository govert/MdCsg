using MdCsg.Api;
using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Cutting;

/// <summary>Phase 6: MeshCutter — Cut with various segment configs, CsgResult metadata</summary>
public class MeshCutterPropertyTests
{
    [Fact]
    public void Cut_NoSegments_PreservesFaces()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var empty = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(cube.Mesh, empty);
        Assert.Equal(cube.Mesh.Faces.Count, result.SubTriangles.Count);
    }

    [Fact]
    public void Cut_NoSegments_SubTrianglesHaveNoIntersectionEdges()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var empty = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(cube.Mesh, empty);
        foreach (var st in result.SubTriangles)
            Assert.False(st.HasIntersectionEdge);
    }

    [Fact]
    public void Cut_NoSegments_PreservesFaceIndices()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var empty = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(cube.Mesh, empty);
        for (int i = 0; i < result.SubTriangles.Count; i++)
            Assert.Equal(i, result.SubTriangles[i].OriginalFaceIndex);
    }

    [Fact]
    public void Cut_WithSegments_ProducesMoreSubTriangles()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var otherCube = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var graph = IntersectionGraph.Compute(cube.Mesh, otherCube.Mesh);
        var result = MeshCutter.Cut(cube.Mesh, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        Assert.True(result.SubTriangles.Count >= cube.Mesh.Faces.Count);
    }

    [Fact]
    public void Cut_WithSegments_SomeHaveIntersectionEdges()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var otherCube = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var graph = IntersectionGraph.Compute(cube.Mesh, otherCube.Mesh);
        var result = MeshCutter.Cut(cube.Mesh, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        Assert.True(result.SubTriangles.Any(st => st.HasIntersectionEdge));
    }

    [Fact]
    public void Cut_ResultMeshHasFaces()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var otherCube = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var graph = IntersectionGraph.Compute(cube.Mesh, otherCube.Mesh);
        var result = MeshCutter.Cut(cube.Mesh, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        Assert.True(result.Mesh.Faces.Count > 0);
    }

    [Fact]
    public void Cut_OriginalFaceIdsTransferred()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var empty = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(cube.Mesh, empty);
        for (int i = 0; i < result.Mesh.Faces.Count; i++)
        {
            Assert.True(result.Mesh.Faces[i].OriginalFaceId >= 0);
        }
    }

    // --- CsgResult tests ---

    [Fact]
    public void CsgResult_Union_HasPositiveFaceCount()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
        Assert.True(result.VertexCount > 0);
    }

    [Fact]
    public void CsgResult_Union_HasIntersectionSegments()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Union(a, b);
        Assert.True(result.IntersectionSegmentCount > 0);
    }

    [Fact]
    public void CsgResult_Disjoint_NoIntersectionSegments()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 1.0);
        var b = MeshFactory.CreateCube(new Vec3(100, 0, 0), 1.0);
        var result = Csg.Union(a, b);
        Assert.Equal(0, result.IntersectionSegmentCount);
    }

    [Fact]
    public void CsgResult_HasPatchCounts()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Union(a, b);
        Assert.True(result.PatchCountA > 0);
        Assert.True(result.PatchCountB > 0);
    }

    [Fact]
    public void CsgResult_FaceCount_MatchesMeshFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Union(a, b);
        Assert.Equal(result.Mesh.Faces.Count, result.FaceCount);
    }

    [Fact]
    public void CsgResult_VertexCount_MatchesMeshVertices()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Union(a, b);
        Assert.Equal(result.Mesh.Vertices.Count, result.VertexCount);
    }

    [Fact]
    public void CsgResult_DegenerateCount_NonNegative()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Union(a, b);
        Assert.True(result.DegenerateCount >= 0);
    }
}
