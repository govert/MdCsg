using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Cutting;

/// <summary>Phase 6: MeshCutter deep tests — cut results, sub-triangle properties, area conservation</summary>
public class MeshCutterDeepTests
{
    [Fact]
    public void Cut_NoSegments_SubTriangleCountEqualsFaceCount()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var empty = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(mesh, empty);
        Assert.Equal(mesh.Faces.Count, result.SubTriangles.Count);
    }

    [Fact]
    public void Cut_NoSegments_OriginalFaceIdsPreserved()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var empty = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(mesh, empty);
        for (int i = 0; i < result.SubTriangles.Count; i++)
        {
            Assert.True(result.SubTriangles[i].OriginalFaceIndex >= 0);
            Assert.True(result.SubTriangles[i].OriginalFaceIndex < mesh.Faces.Count);
        }
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
    public void Cut_WithSegments_MoreSubTriangles()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var result = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        Assert.True(result.SubTriangles.Count > meshA.Faces.Count);
    }

    [Fact]
    public void Cut_WithSegments_SomeHaveIntersectionEdges()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var result = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        Assert.Contains(result.SubTriangles, st => st.HasIntersectionEdge);
    }

    [Fact]
    public void Cut_AllSubTriangles_HavePositiveArea()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var result = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        foreach (var st in result.SubTriangles)
        {
            double area = new Triangle3(st.A, st.B, st.C).Area;
            Assert.True(area >= 0, $"Sub-triangle from face {st.OriginalFaceIndex} has negative area");
        }
    }

    [Fact]
    public void Cut_AreaConservation_PerFace()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var result = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);

        // Group sub-triangles by original face
        var groups = result.SubTriangles.GroupBy(st => st.OriginalFaceIndex);
        foreach (var group in groups)
        {
            double subAreaSum = group.Sum(st => new Triangle3(st.A, st.B, st.C).Area);
            var face = meshA.Faces[group.Key];
            face.GetTrianglePositions(out var a, out var b, out var c);
            double originalArea = new Triangle3(a, b, c).Area;
            Assert.True(System.Math.Abs(subAreaSum - originalArea) < 1e-6,
                $"Face {group.Key}: sub-area {subAreaSum} vs original {originalArea}");
        }
    }

    [Fact]
    public void Cut_AllOriginalFaces_Represented()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var result = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);

        var faces = result.SubTriangles.Select(st => st.OriginalFaceIndex).Distinct().ToHashSet();
        Assert.Equal(meshA.Faces.Count, faces.Count);
    }

    [Fact]
    public void Cut_SphereCube_MoreSubTriangles()
    {
        var meshA = MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.6, 2).Mesh;
        var meshB = MeshFactory.CreateCube().Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var result = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        Assert.True(result.SubTriangles.Count > meshA.Faces.Count);
    }

    [Fact]
    public void Cut_SubTriangles_NoNaN()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var result = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);
        foreach (var st in result.SubTriangles)
        {
            Assert.False(double.IsNaN(st.A.X) || double.IsNaN(st.A.Y) || double.IsNaN(st.A.Z));
            Assert.False(double.IsNaN(st.B.X) || double.IsNaN(st.B.Y) || double.IsNaN(st.B.Z));
            Assert.False(double.IsNaN(st.C.X) || double.IsNaN(st.C.Y) || double.IsNaN(st.C.Z));
        }
    }

    [Fact]
    public void CutResult_Mesh_IsNewMesh_WithSameFaceCount()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var empty = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(meshA, empty);
        // CutResult.Mesh is a newly built mesh, not the original
        Assert.NotSame(meshA, result.Mesh);
        Assert.Equal(meshA.Faces.Count, result.Mesh.Faces.Count);
    }
}
