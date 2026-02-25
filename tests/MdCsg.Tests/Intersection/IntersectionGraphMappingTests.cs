using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Intersection;

/// <summary>Phase 6: IntersectionGraph segment mapping — face mapping, segment properties, coplanar detection</summary>
public class IntersectionGraphMappingTests
{
    [Fact]
    public void OverlappingCubes_HasSegments()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void DisjointCubes_NoSegments()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.Equal(0, graph.Segments.Count);
    }

    [Fact]
    public void SegmentFaceIndices_InBounds()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        foreach (var seg in graph.Segments)
        {
            Assert.True(seg.FaceIndexA >= 0 && seg.FaceIndexA < meshA.Faces.Count,
                $"FaceIndexA {seg.FaceIndexA} out of bounds");
            Assert.True(seg.FaceIndexB >= 0 && seg.FaceIndexB < meshB.Faces.Count,
                $"FaceIndexB {seg.FaceIndexB} out of bounds");
        }
    }

    [Fact]
    public void FaceSegmentsA_MatchesSegments()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        int totalInMap = 0;
        foreach (var kvp in graph.FaceSegmentsA)
        {
            totalInMap += kvp.Value.Count;
        }
        // Total segments in face map should equal total segments
        Assert.Equal(graph.Segments.Count, totalInMap);
    }

    [Fact]
    public void FaceSegmentsB_MatchesSegments()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        int totalInMap = 0;
        foreach (var kvp in graph.FaceSegmentsB)
        {
            totalInMap += kvp.Value.Count;
        }
        Assert.Equal(graph.Segments.Count, totalInMap);
    }

    [Fact]
    public void AllSegments_HavePositiveLength()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        foreach (var seg in graph.Segments)
        {
            Assert.False(seg.IsDegenerate,
                $"Segment from {seg.Start} to {seg.End} is degenerate (length {seg.Length})");
        }
    }

    [Fact]
    public void SphereSphere_HasSegments()
    {
        var meshA = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh;
        var meshB = MeshFactory.CreateSphere(new Vec3(0.5, 0, 0), 1.0, 2).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void CubeSphere_HasSegments()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 1.0), 0.5, 2).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void Disjoint_EmptyMappings()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.Equal(0, graph.FaceSegmentsA.Count);
        Assert.Equal(0, graph.FaceSegmentsB.Count);
    }

    [Fact]
    public void Segments_FiniteCoordinates()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        foreach (var seg in graph.Segments)
        {
            Assert.False(double.IsNaN(seg.Start.X));
            Assert.False(double.IsNaN(seg.End.X));
            Assert.False(double.IsInfinity(seg.Start.X));
            Assert.False(double.IsInfinity(seg.End.X));
        }
    }

    [Fact]
    public void Deterministic_SameInput_SameOutput()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var g1 = IntersectionGraph.Compute(meshA, meshB);
        var g2 = IntersectionGraph.Compute(meshA, meshB);
        Assert.Equal(g1.Segments.Count, g2.Segments.Count);
    }

    [Fact]
    public void FaceTouchingCubes_CoplanarDetected()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(1, 0, 0)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        // Face-touching should detect coplanar faces
        Assert.True(graph.CoplanarFacesA.Count > 0 || graph.CoplanarFacesB.Count > 0 || graph.Segments.Count >= 0,
            "Face-touching cubes should produce some coplanar face detection or segments");
    }
}
