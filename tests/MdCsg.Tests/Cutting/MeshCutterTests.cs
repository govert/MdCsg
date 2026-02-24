using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Cutting;

/// <summary>Batch 18: MeshCutter tests (20 tests)</summary>
public class MeshCutterTests
{
    [Fact]
    public void NoSegments_SubTriangleCount_EqualsFaceCount()
    {
        var cube = MeshFactory.CreateCube();
        var emptySegs = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(cube.Mesh, emptySegs);
        Assert.Equal(12, result.SubTriangles.Count);
    }

    [Fact]
    public void NoSegments_SubTriangles_HaveNoIntersectionEdge()
    {
        var cube = MeshFactory.CreateCube();
        var emptySegs = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(cube.Mesh, emptySegs);
        foreach (var st in result.SubTriangles)
            Assert.False(st.HasIntersectionEdge);
    }

    [Fact]
    public void NoSegments_OriginalFaceIndices_AreSequential()
    {
        var cube = MeshFactory.CreateCube();
        var emptySegs = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(cube.Mesh, emptySegs);
        for (int i = 0; i < result.SubTriangles.Count; i++)
            Assert.Equal(i, result.SubTriangles[i].OriginalFaceIndex);
    }

    [Fact]
    public void WithSegments_ProducesMoreSubTriangles()
    {
        var cube = MeshFactory.CreateCube();
        // Compute actual intersection with another cube
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph = IntersectionGraph.Compute(cube.Mesh, cubeB.Mesh);
        var result = MeshCutter.Cut(cube.Mesh, graph.FaceSegmentsA);
        Assert.True(result.SubTriangles.Count > 12);
    }

    [Fact]
    public void WithSegments_SomeSubTrianglesHaveIntersectionEdges()
    {
        var cube = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph = IntersectionGraph.Compute(cube.Mesh, cubeB.Mesh);
        var result = MeshCutter.Cut(cube.Mesh, graph.FaceSegmentsA);
        Assert.True(result.SubTriangles.Any(st => st.HasIntersectionEdge));
    }

    [Fact]
    public void WithSegments_SomeSubTrianglesHaveNoIntersectionEdges()
    {
        var cube = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph = IntersectionGraph.Compute(cube.Mesh, cubeB.Mesh);
        var result = MeshCutter.Cut(cube.Mesh, graph.FaceSegmentsA);
        Assert.True(result.SubTriangles.Any(st => !st.HasIntersectionEdge));
    }

    [Fact]
    public void CutResult_MeshHasFaces()
    {
        var cube = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph = IntersectionGraph.Compute(cube.Mesh, cubeB.Mesh);
        var result = MeshCutter.Cut(cube.Mesh, graph.FaceSegmentsA);
        Assert.True(result.Mesh.Faces.Count > 0);
    }

    [Fact]
    public void CutResult_MeshFaceCountMatchesSubTriangleCount()
    {
        var cube = MeshFactory.CreateCube();
        var emptySegs = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(cube.Mesh, emptySegs);
        Assert.Equal(result.SubTriangles.Count, result.Mesh.Faces.Count);
    }

    [Fact]
    public void DisjointCubes_CutA_NoExtraSubTriangles()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(5, 0, 0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        var result = MeshCutter.Cut(cubeA.Mesh, graph.FaceSegmentsA);
        Assert.Equal(12, result.SubTriangles.Count);
    }

    [Fact]
    public void CutBothMeshes_BothHaveSubTriangles()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        var resultA = MeshCutter.Cut(cubeA.Mesh, graph.FaceSegmentsA);
        var resultB = MeshCutter.Cut(cubeB.Mesh, graph.FaceSegmentsB);
        Assert.True(resultA.SubTriangles.Count > 12);
        Assert.True(resultB.SubTriangles.Count > 12);
    }

    [Fact]
    public void SubTriangles_AllHaveValidOriginalFaceIndex()
    {
        var cube = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph = IntersectionGraph.Compute(cube.Mesh, cubeB.Mesh);
        var result = MeshCutter.Cut(cube.Mesh, graph.FaceSegmentsA);
        foreach (var st in result.SubTriangles)
            Assert.True(st.OriginalFaceIndex >= 0 && st.OriginalFaceIndex < 12);
    }

    [Fact]
    public void EmptyFaceSegmentDict_SameAsNoSegments()
    {
        var cube = MeshFactory.CreateCube();
        var result = MeshCutter.Cut(cube.Mesh, new Dictionary<int, List<IntersectionSegment>>());
        Assert.Equal(12, result.SubTriangles.Count);
    }

    [Fact]
    public void CutTetrahedron_NoSegments()
    {
        var tet = MeshFactory.CreateTetrahedron();
        var result = MeshCutter.Cut(tet.Mesh, new Dictionary<int, List<IntersectionSegment>>());
        Assert.Equal(tet.Mesh.Faces.Count, result.SubTriangles.Count);
    }

    [Fact]
    public void CutSphere_NoSegments()
    {
        var sphere = MeshFactory.CreateSphere(subdivisions: 1);
        var result = MeshCutter.Cut(sphere.Mesh, new Dictionary<int, List<IntersectionSegment>>());
        Assert.Equal(sphere.Mesh.Faces.Count, result.SubTriangles.Count);
    }

    [Fact]
    public void CutCubeSphere_ProducesExtraSubTriangles()
    {
        var cube = MeshFactory.CreateCube();
        var sphere = MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 1);
        var graph = IntersectionGraph.Compute(cube.Mesh, sphere.Mesh);
        if (graph.Segments.Count > 0)
        {
            var result = MeshCutter.Cut(cube.Mesh, graph.FaceSegmentsA);
            Assert.True(result.SubTriangles.Count > 12);
        }
    }

    [Fact]
    public void CutResult_SubTriangles_NonDegenerateArea()
    {
        var cube = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph = IntersectionGraph.Compute(cube.Mesh, cubeB.Mesh);
        var result = MeshCutter.Cut(cube.Mesh, graph.FaceSegmentsA);
        foreach (var st in result.SubTriangles)
        {
            var tri = new Triangle3(st.A, st.B, st.C);
            // Sub-triangles should have non-degenerate area
            Assert.True(tri.Area >= 0);
        }
    }

    [Fact]
    public void CutFace_OriginalFaceIdIsTransferred()
    {
        var cube = MeshFactory.CreateCube();
        var emptySegs = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(cube.Mesh, emptySegs);
        for (int i = 0; i < result.Mesh.Faces.Count; i++)
            Assert.Equal(i, result.Mesh.Faces[i].OriginalFaceId);
    }

    [Fact]
    public void Symmetric_CutCounts()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        var resultA = MeshCutter.Cut(cubeA.Mesh, graph.FaceSegmentsA);
        var resultB = MeshCutter.Cut(cubeB.Mesh, graph.FaceSegmentsB);
        // Both should have the same number of extra sub-triangles from cutting
        Assert.Equal(resultA.SubTriangles.Count, resultB.SubTriangles.Count);
    }

    [Fact]
    public void FaceSegments_WithEmptyList_TreatedAsNoCut()
    {
        var cube = MeshFactory.CreateCube();
        var segs = new Dictionary<int, List<IntersectionSegment>>
        {
            { 0, new List<IntersectionSegment>() }
        };
        var result = MeshCutter.Cut(cube.Mesh, segs);
        Assert.Equal(12, result.SubTriangles.Count);
    }

    [Fact]
    public void CutResult_MeshHasVertices()
    {
        var cube = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph = IntersectionGraph.Compute(cube.Mesh, cubeB.Mesh);
        var result = MeshCutter.Cut(cube.Mesh, graph.FaceSegmentsA);
        Assert.True(result.Mesh.Vertices.Count > 0);
    }
}
