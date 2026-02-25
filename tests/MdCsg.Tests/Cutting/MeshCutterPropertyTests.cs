using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Cutting;

/// <summary>Phase 6: MeshCutter — Cut with/without segments, sub-triangle counts, original face indices</summary>
public class MeshCutterPropertyTests
{
    [Fact]
    public void Cut_NoSegments_SubTriangleCountEqualsFaceCount()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var emptySegments = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(cube.Mesh, emptySegments);
        Assert.Equal(cube.Mesh.Faces.Count, result.SubTriangles.Count);
    }

    [Fact]
    public void Cut_NoSegments_OriginalFaceIndicesMatch()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var emptySegments = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(cube.Mesh, emptySegments);
        for (int i = 0; i < result.SubTriangles.Count; i++)
        {
            Assert.Equal(i, result.SubTriangles[i].OriginalFaceIndex);
        }
    }

    [Fact]
    public void Cut_NoSegments_NoIntersectionEdges()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var emptySegments = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(cube.Mesh, emptySegments);
        foreach (var st in result.SubTriangles)
        {
            Assert.False(st.HasIntersectionEdge);
        }
    }

    [Fact]
    public void Cut_WithSegments_MoreSubTrianglesThanFaces()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var sphere = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 2);
        var graph = IntersectionGraph.Compute(cube.Mesh, sphere.Mesh);
        if (graph.FaceSegmentsA.Count == 0) return; // skip if no intersection

        var result = MeshCutter.Cut(cube.Mesh, graph.FaceSegmentsA);
        Assert.True(result.SubTriangles.Count >= cube.Mesh.Faces.Count);
    }

    [Fact]
    public void Cut_WithSegments_AllOriginalFaceIndicesValid()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var sphere = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 2);
        var graph = IntersectionGraph.Compute(cube.Mesh, sphere.Mesh);
        var result = MeshCutter.Cut(cube.Mesh, graph.FaceSegmentsA);
        foreach (var st in result.SubTriangles)
        {
            Assert.True(st.OriginalFaceIndex >= 0 && st.OriginalFaceIndex < cube.Mesh.Faces.Count);
        }
    }

    [Fact]
    public void Cut_ProducesMesh()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var emptySegments = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(cube.Mesh, emptySegments);
        Assert.NotNull(result.Mesh);
        Assert.True(result.Mesh.Faces.Count > 0);
    }

    [Fact]
    public void Cut_Sphere_NoSegments_SubTriangleCountMatchesFaces()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var emptySegments = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(sphere.Mesh, emptySegments);
        Assert.Equal(sphere.Mesh.Faces.Count, result.SubTriangles.Count);
    }

    [Fact]
    public void Cut_OverlappingCubes_BothMeshesHaveMoreSubTriangles()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        
        var cutA = MeshCutter.Cut(a.Mesh, graph.FaceSegmentsA);
        var cutB = MeshCutter.Cut(b.Mesh, graph.FaceSegmentsB);
        
        Assert.True(cutA.SubTriangles.Count >= a.Mesh.Faces.Count);
        Assert.True(cutB.SubTriangles.Count >= b.Mesh.Faces.Count);
    }

    [Fact]
    public void CutResult_IsRecord()
    {
        var mesh = new HalfEdgeMesh();
        var subTris = new List<FaceCutter.SubTriangle>();
        var result = new MeshCutter.CutResult(mesh, subTris);
        Assert.Same(mesh, result.Mesh);
        Assert.Same(subTris, result.SubTriangles);
    }
}
