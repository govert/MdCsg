using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Cutting;

/// <summary>Phase 6: MeshCutter area preservation — total area before/after cutting, sub-triangle area validity, original face coverage</summary>
public class MeshCutterAreaPreservationTests
{
    private static double TriangleArea(Vec3 a, Vec3 b, Vec3 c) =>
        Vec3.Cross(b - a, c - a).Length / 2.0;

    [Fact]
    public void NoCut_AreaPreserved()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        double originalArea = 0;
        foreach (var face in mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            originalArea += TriangleArea(a, b, c);
        }

        var empty = new Dictionary<int, List<IntersectionSegment>>();
        var cut = MeshCutter.Cut(mesh, empty);
        double cutArea = 0;
        foreach (var sub in cut.SubTriangles)
            cutArea += TriangleArea(sub.A, sub.B, sub.C);

        Assert.Equal(originalArea, cutArea, 1e-10);
    }

    [Fact]
    public void NoCut_SubTriangleCount_EqualsFaceCount()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var empty = new Dictionary<int, List<IntersectionSegment>>();
        var cut = MeshCutter.Cut(mesh, empty);
        Assert.Equal(mesh.Faces.Count, cut.SubTriangles.Count);
    }

    [Fact]
    public void NoCut_AllOriginalFaceIndices_Present()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var empty = new Dictionary<int, List<IntersectionSegment>>();
        var cut = MeshCutter.Cut(mesh, empty);
        var covered = new HashSet<int>();
        foreach (var sub in cut.SubTriangles)
            covered.Add(sub.OriginalFaceIndex);
        Assert.Equal(mesh.Faces.Count, covered.Count);
    }

    [Fact]
    public void WithCut_AreaApproximatelyPreserved()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);

        double originalArea = 0;
        foreach (var face in meshA.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            originalArea += TriangleArea(a, b, c);
        }

        var cut = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        double cutArea = 0;
        foreach (var sub in cut.SubTriangles)
            cutArea += TriangleArea(sub.A, sub.B, sub.C);

        // Area should be approximately preserved (small numeric error from triangulation)
        Assert.True(System.Math.Abs(cutArea - originalArea) / originalArea < 0.01,
            $"Area difference too large: original={originalArea}, cut={cutArea}");
    }

    [Fact]
    public void WithCut_MoreSubTriangles_ThanOriginal()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cut = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        Assert.True(cut.SubTriangles.Count > meshA.Faces.Count);
    }

    [Fact]
    public void WithCut_AllSubTriangles_NonDegenerateArea()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cut = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        foreach (var sub in cut.SubTriangles)
        {
            double area = TriangleArea(sub.A, sub.B, sub.C);
            Assert.True(area > 1e-20, $"Zero-area sub-triangle found");
        }
    }

    [Fact]
    public void UncutFaces_PassThroughAsIs()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cut = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        // Some faces are not cut (no intersection segments)
        var cutFaces = new HashSet<int>(graph.FaceSegmentsA.Keys);
        foreach (var sub in cut.SubTriangles)
        {
            if (!cutFaces.Contains(sub.OriginalFaceIndex))
            {
                // Uncut face should be a single sub-triangle matching original
                Assert.False(sub.HasIntersectionEdge,
                    $"Uncut face {sub.OriginalFaceIndex} should not have intersection edges");
            }
        }
    }

    [Fact]
    public void CutFaces_HaveIntersectionEdges()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cut = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        var cutFaces = new HashSet<int>(graph.FaceSegmentsA.Keys);
        bool anyIntersectionEdge = false;
        foreach (var sub in cut.SubTriangles)
        {
            if (cutFaces.Contains(sub.OriginalFaceIndex) && sub.HasIntersectionEdge)
            {
                anyIntersectionEdge = true;
                break;
            }
        }
        Assert.True(anyIntersectionEdge, "Cut faces should have some intersection edges");
    }

    [Fact]
    public void Sphere_NoCut_AreaPreserved()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh;
        double originalArea = 0;
        foreach (var face in mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            originalArea += TriangleArea(a, b, c);
        }
        var cut = MeshCutter.Cut(mesh, new Dictionary<int, List<IntersectionSegment>>());
        double cutArea = 0;
        foreach (var sub in cut.SubTriangles)
            cutArea += TriangleArea(sub.A, sub.B, sub.C);
        Assert.Equal(originalArea, cutArea, 1e-10);
    }

    [Fact]
    public void Tetra_NoCut_FourSubTriangles()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        var cut = MeshCutter.Cut(mesh, new Dictionary<int, List<IntersectionSegment>>());
        Assert.Equal(4, cut.SubTriangles.Count);
    }

    [Fact]
    public void BothMeshes_Cut_Independently()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        var cutB = MeshCutter.Cut(meshB, graph.FaceSegmentsB);
        Assert.True(cutA.SubTriangles.Count > 0);
        Assert.True(cutB.SubTriangles.Count > 0);
        // Both should have more sub-triangles than original faces
        Assert.True(cutA.SubTriangles.Count > meshA.Faces.Count);
        Assert.True(cutB.SubTriangles.Count > meshB.Faces.Count);
    }
}
