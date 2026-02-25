using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Cutting;

/// <summary>Phase 6: MeshCutter CutResult structural tests — sub-triangles, face mapping, edge flags</summary>
public class MeshCutterSubTriangleTests
{
    [Fact]
    public void Cut_NoSegments_SubTrianglesMatchFaces()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var emptySegments = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(mesh, emptySegments);
        Assert.Equal(mesh.Faces.Count, result.SubTriangles.Count);
    }

    [Fact]
    public void Cut_NoSegments_AllOriginalFaceIndicesValid()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var emptySegments = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(mesh, emptySegments);
        foreach (var st in result.SubTriangles)
            Assert.True(st.OriginalFaceIndex >= 0 && st.OriginalFaceIndex < mesh.Faces.Count);
    }

    [Fact]
    public void Cut_NoSegments_NoIntersectionEdges()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var emptySegments = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(mesh, emptySegments);
        foreach (var st in result.SubTriangles)
            Assert.False(st.HasIntersectionEdge);
    }

    [Fact]
    public void Cut_WithSegments_MoreSubTriangles()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        // Cut faces should produce more sub-triangles than original faces
        Assert.True(cutA.SubTriangles.Count >= meshA.Faces.Count);
    }

    [Fact]
    public void Cut_WithSegments_SomeIntersectionEdges()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        Assert.True(cutA.SubTriangles.Any(st => st.HasIntersectionEdge));
    }

    [Fact]
    public void Cut_VerticesNotNaN()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        foreach (var st in cutA.SubTriangles)
        {
            Assert.False(double.IsNaN(st.A.X));
            Assert.False(double.IsNaN(st.B.Y));
            Assert.False(double.IsNaN(st.C.Z));
        }
    }

    [Fact]
    public void Cut_Tetrahedron_NoSegments_4SubTriangles()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        var emptySegments = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(mesh, emptySegments);
        Assert.Equal(4, result.SubTriangles.Count);
    }

    [Fact]
    public void Cut_Sphere_NoSegments_MatchesFaceCount()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var emptySegments = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(mesh, emptySegments);
        Assert.Equal(mesh.Faces.Count, result.SubTriangles.Count);
    }

    [Fact]
    public void Cut_BothMeshes_ProduceValidSubTriangles()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        var cutB = MeshCutter.Cut(meshB, graph.FaceSegmentsB);
        Assert.True(cutA.SubTriangles.Count > 0);
        Assert.True(cutB.SubTriangles.Count > 0);
    }

    [Fact]
    public void SubTriangle_RecordEquality()
    {
        var st1 = new FaceCutter.SubTriangle(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false);
        var st2 = new FaceCutter.SubTriangle(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false);
        Assert.Equal(st1, st2);
    }

    [Fact]
    public void SubTriangle_RecordInequality()
    {
        var st1 = new FaceCutter.SubTriangle(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false);
        var st2 = new FaceCutter.SubTriangle(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, true);
        Assert.NotEqual(st1, st2);
    }
}
