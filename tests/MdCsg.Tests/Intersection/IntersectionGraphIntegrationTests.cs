using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Intersection;

/// <summary>Phase 6: IntersectionGraph integration tests — segment properties, face mapping, various configurations</summary>
public class IntersectionGraphIntegrationTests
{
    [Fact]
    public void Compute_OverlappingCubes_HasSegments()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void Compute_DisjointCubes_NoSegments()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(10, 10, 10)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.Equal(0, graph.Segments.Count);
    }

    [Fact]
    public void Compute_FaceSegmentsA_NonEmpty()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.True(graph.FaceSegmentsA.Count > 0);
    }

    [Fact]
    public void Compute_FaceSegmentsB_NonEmpty()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.True(graph.FaceSegmentsB.Count > 0);
    }

    [Fact]
    public void Compute_SegmentFaceIndices_Valid()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        foreach (var seg in graph.Segments)
        {
            Assert.True(seg.FaceIndexA >= 0 && seg.FaceIndexA < meshA.Faces.Count);
            Assert.True(seg.FaceIndexB >= 0 && seg.FaceIndexB < meshB.Faces.Count);
        }
    }

    [Fact]
    public void Compute_SegmentStartEnd_NotNaN()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        foreach (var seg in graph.Segments)
        {
            Assert.False(double.IsNaN(seg.Start.X));
            Assert.False(double.IsNaN(seg.Start.Y));
            Assert.False(double.IsNaN(seg.Start.Z));
            Assert.False(double.IsNaN(seg.End.X));
            Assert.False(double.IsNaN(seg.End.Y));
            Assert.False(double.IsNaN(seg.End.Z));
        }
    }

    [Fact]
    public void Compute_SegmentsNonDegenerate()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        foreach (var seg in graph.Segments)
            Assert.False(seg.IsDegenerate, $"Degenerate segment found: length={seg.Length}");
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
    public void Compute_DifferentGridSizes_BothWork()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph1 = IntersectionGraph.Compute(meshA, meshB, gridSize: 1e-8);
        var graph2 = IntersectionGraph.Compute(meshA, meshB, gridSize: 1e-6);
        Assert.True(graph1.Segments.Count > 0);
        Assert.True(graph2.Segments.Count > 0);
    }

    [Fact]
    public void Compute_SmallOverlap_StillFindsSegments()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.9, 0, 0)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void Compute_LargeOverlap_MoreSegments()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var smallOverlap = IntersectionGraph.Compute(meshA, MeshFactory.CreateCube(new Vec3(0.9, 0, 0)).Mesh);
        var largeOverlap = IntersectionGraph.Compute(meshA, MeshFactory.CreateCube(new Vec3(0.3, 0, 0)).Mesh);
        // More overlap should produce more or equal number of intersection segments
        Assert.True(largeOverlap.Segments.Count >= smallOverlap.Segments.Count,
            $"Large overlap segments ({largeOverlap.Segments.Count}) should be >= small overlap ({smallOverlap.Segments.Count})");
    }

    [Fact]
    public void Compute_TwoSpheres_HasSegments()
    {
        var meshA = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var meshB = MeshFactory.CreateSphere(new Vec3(0.5, 0, 0), 1, 2).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.True(graph.Segments.Count > 0);
    }
}
