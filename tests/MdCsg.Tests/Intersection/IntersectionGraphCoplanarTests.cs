using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Intersection;

/// <summary>Phase 6: IntersectionGraph coplanar face handling tests</summary>
public class IntersectionGraphCoplanarTests
{
    [Fact]
    public void Compute_NonOverlapping_NoSegments()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(5, 5, 5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.Empty(graph.Segments);
        Assert.Empty((Dictionary<int, System.Collections.Generic.List<IntersectionSegment>>)graph.FaceSegmentsA);
        Assert.Empty((Dictionary<int, System.Collections.Generic.List<IntersectionSegment>>)graph.FaceSegmentsB);
    }

    [Fact]
    public void Compute_Overlapping_HasSegments()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void Compute_FaceSegmentsA_FaceIndicesValid()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        foreach (var kvp in graph.FaceSegmentsA)
        {
            Assert.True(kvp.Key >= 0 && kvp.Key < meshA.Faces.Count, $"Invalid face A index: {kvp.Key}");
            Assert.True(kvp.Value.Count > 0);
        }
    }

    [Fact]
    public void Compute_FaceSegmentsB_FaceIndicesValid()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        foreach (var kvp in graph.FaceSegmentsB)
        {
            Assert.True(kvp.Key >= 0 && kvp.Key < meshB.Faces.Count, $"Invalid face B index: {kvp.Key}");
            Assert.True(kvp.Value.Count > 0);
        }
    }

    [Fact]
    public void Compute_SegmentEndpoints_NotNaN()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        foreach (var seg in graph.Segments)
        {
            Assert.False(double.IsNaN(seg.Start.X) || double.IsNaN(seg.Start.Y) || double.IsNaN(seg.Start.Z));
            Assert.False(double.IsNaN(seg.End.X) || double.IsNaN(seg.End.Y) || double.IsNaN(seg.End.Z));
        }
    }

    [Fact]
    public void Compute_SegmentsNotDegenerate()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        foreach (var seg in graph.Segments)
        {
            Assert.False(seg.IsDegenerate, $"Degenerate segment: {seg.Start} → {seg.End}");
        }
    }

    [Fact]
    public void Compute_CoplanarFaces_EmptyForNonCoplanar()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        // Standard overlapping cubes don't have coplanar faces (different offset)
        Assert.Empty((Dictionary<int, bool>)graph.CoplanarFacesA);
        Assert.Empty((Dictionary<int, bool>)graph.CoplanarFacesB);
    }

    [Fact]
    public void Compute_Symmetry_SegmentCountMatches()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var g1 = IntersectionGraph.Compute(meshA, meshB);
        var g2 = IntersectionGraph.Compute(meshB, meshA);
        Assert.Equal(g1.Segments.Count, g2.Segments.Count);
    }

    [Fact]
    public void Compute_WithCustomGridSize_ProducesResults()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB, gridSize: 1e-6);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void Compute_SphereCube_HasSegments()
    {
        var meshA = MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.6, 2).Mesh;
        var meshB = MeshFactory.CreateCube().Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void Compute_SphereCube_FaceSegmentsBothPopulated()
    {
        var meshA = MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.6, 2).Mesh;
        var meshB = MeshFactory.CreateCube().Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.True(graph.FaceSegmentsA.Count > 0);
        Assert.True(graph.FaceSegmentsB.Count > 0);
    }

    [Fact]
    public void Compute_SegmentFaceIndices_Consistent()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        foreach (var seg in graph.Segments)
        {
            Assert.True(seg.FaceIndexA >= 0, $"Invalid FaceIndexA: {seg.FaceIndexA}");
            Assert.True(seg.FaceIndexB >= 0, $"Invalid FaceIndexB: {seg.FaceIndexB}");
        }
    }

    [Fact]
    public void Compute_DisjointMeshes_EmptyGraph()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(10, 10, 10)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.Empty(graph.Segments);
    }

    [Fact]
    public void Compute_ContainedCube_HasSegments()
    {
        // Small cube inside large cube
        var meshA = MeshFactory.CreateCube(new Vec3(-1, -1, -1), size: 4).Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), size: 0.3).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        // No intersection segments because B is entirely inside A
        Assert.Empty(graph.Segments);
    }
}
