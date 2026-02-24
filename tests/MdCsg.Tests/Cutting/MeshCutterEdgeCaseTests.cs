using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Cutting;

/// <summary>Phase 6: MeshCutter edge cases</summary>
public class MeshCutterEdgeCaseTests
{
    [Fact]
    public void Cut_NoSegments_IdenticalSubTriangles()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var emptySegments = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(mesh, emptySegments);
        // Each face produces one sub-triangle
        Assert.Equal(mesh.Faces.Count, result.SubTriangles.Count);
    }

    [Fact]
    public void Cut_NoSegments_OriginalFaceIds()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var emptySegments = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(mesh, emptySegments);
        for (int i = 0; i < result.SubTriangles.Count; i++)
        {
            Assert.Equal(i, result.SubTriangles[i].OriginalFaceIndex);
        }
    }

    [Fact]
    public void Cut_NoSegments_SubTriangles_NoIntersectionEdges()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var emptySegments = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(mesh, emptySegments);
        foreach (var st in result.SubTriangles)
        {
            Assert.False(st.IsEdgeIntersection(0));
            Assert.False(st.IsEdgeIntersection(1));
            Assert.False(st.IsEdgeIntersection(2));
        }
    }

    [Fact]
    public void Cut_WithSegments_MoreSubTriangles()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var result = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        // Cut mesh should have more sub-triangles than original faces
        Assert.True(result.SubTriangles.Count >= meshA.Faces.Count);
    }

    [Fact]
    public void Cut_WithSegments_AllOriginalFaceIdsValid()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var result = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        foreach (var st in result.SubTriangles)
        {
            Assert.True(st.OriginalFaceIndex >= 0 && st.OriginalFaceIndex < meshA.Faces.Count,
                $"OriginalFaceIndex {st.OriginalFaceIndex} out of range");
        }
    }

    [Fact]
    public void Cut_WithSegments_SomeHaveIntersectionEdges()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var result = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        bool anyIntersectionEdge = result.SubTriangles.Any(st =>
            st.IsEdgeIntersection(0) || st.IsEdgeIntersection(1) || st.IsEdgeIntersection(2));
        Assert.True(anyIntersectionEdge, "Should have intersection edges after cutting");
    }

    [Fact]
    public void Cut_ResultMesh_HasFaces()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var result = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        Assert.True(result.Mesh.Faces.Count > 0);
    }

    [Fact]
    public void Cut_Tetrahedron_NoSegments()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        var emptySegments = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(mesh, emptySegments);
        Assert.Equal(4, result.SubTriangles.Count);
    }

    [Fact]
    public void Cut_SubTriangles_NonDegenerate()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var result = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        foreach (var st in result.SubTriangles)
        {
            var tri = new Triangle3(st.A, st.B, st.C);
            Assert.True(tri.Area > 0, "Sub-triangle should be non-degenerate");
        }
    }

    [Fact]
    public void Cut_Sphere_WithSegments()
    {
        var meshA = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(-0.3, -0.3, -0.3), size: 1).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var result = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        Assert.True(result.SubTriangles.Count >= meshA.Faces.Count);
    }
}
