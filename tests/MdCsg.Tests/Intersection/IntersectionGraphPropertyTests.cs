using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Intersection;

/// <summary>Phase 6: IntersectionGraph — Compute with overlapping and non-overlapping meshes</summary>
public class IntersectionGraphPropertyTests
{
    [Fact]
    public void Compute_DisjointCubes_NoSegments()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(100, 0, 0), 2.0);
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.Empty(graph.Segments);
        Assert.Empty(graph.FaceSegmentsA);
        Assert.Empty(graph.FaceSegmentsB);
    }

    [Fact]
    public void Compute_OverlappingCubes_HasSegments()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void Compute_OverlappingCubes_SegmentsHaveValidFaceIndices()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        foreach (var seg in graph.Segments)
        {
            Assert.True(seg.FaceIndexA >= 0 && seg.FaceIndexA < a.Mesh.Faces.Count);
            Assert.True(seg.FaceIndexB >= 0 && seg.FaceIndexB < b.Mesh.Faces.Count);
        }
    }

    [Fact]
    public void Compute_OverlappingCubes_FaceSegmentsAPopulated()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.True(graph.FaceSegmentsA.Count > 0);
    }

    [Fact]
    public void Compute_OverlappingCubes_FaceSegmentsBPopulated()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.True(graph.FaceSegmentsB.Count > 0);
    }

    [Fact]
    public void Compute_OverlappingCubes_NoSegmentsAreDegenerate()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        foreach (var seg in graph.Segments)
            Assert.False(seg.IsDegenerate);
    }

    [Fact]
    public void Compute_ContainedCube_HasSegments()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 4.0);
        var b = MeshFactory.CreateCube(Vec3.Zero, 1.0);
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void Compute_CubeSphere_HasSegments()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateSphere(Vec3.Zero, 1.5, 2);
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void Compute_DisjointCubes_NoCoplanarFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(100, 0, 0), 2.0);
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.Empty(graph.CoplanarFacesA);
        Assert.Empty(graph.CoplanarFacesB);
    }

    [Fact]
    public void Compute_SegmentsAreSnapped()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        double gridSize = 0.01;
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh, gridSize);
        foreach (var seg in graph.Segments)
        {
            // Snapped coordinates should be multiples of gridSize within tolerance
            Assert.True(System.Math.Abs(seg.Start.X / gridSize - System.Math.Round(seg.Start.X / gridSize)) < 1e-6);
        }
    }

    [Fact]
    public void Compute_SegmentCount_Symmetric()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        // Each segment appears once in segments list
        int fromA = graph.FaceSegmentsA.Values.Sum(l => l.Count);
        int fromB = graph.FaceSegmentsB.Values.Sum(l => l.Count);
        // Each non-coplanar segment is stored in both FaceSegmentsA and FaceSegmentsB
        Assert.Equal(fromA, fromB);
    }
}
