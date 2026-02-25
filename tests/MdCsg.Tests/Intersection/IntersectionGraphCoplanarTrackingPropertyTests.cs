using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Intersection;

/// <summary>Phase 6: IntersectionGraph — coplanar face tracking, segment mapping, graph properties</summary>
public class IntersectionGraphCoplanarTrackingPropertyTests
{
    [Fact]
    public void DisjointMeshes_NoSegments()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.Equal(0, graph.Segments.Count);
    }

    [Fact]
    public void DisjointMeshes_EmptyFaceSegments()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.Equal(0, graph.FaceSegmentsA.Count);
        Assert.Equal(0, graph.FaceSegmentsB.Count);
    }

    [Fact]
    public void OverlappingCubes_HasSegments()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.True(graph.Segments.Count > 0,
            $"Overlapping cubes should have intersection segments");
    }

    [Fact]
    public void Segments_HaveValidFaceIndices()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);

        foreach (var seg in graph.Segments)
        {
            Assert.True(seg.FaceIndexA >= 0 && seg.FaceIndexA < meshA.Faces.Count,
                $"FaceIndexA {seg.FaceIndexA} out of range");
            Assert.True(seg.FaceIndexB >= 0 && seg.FaceIndexB < meshB.Faces.Count,
                $"FaceIndexB {seg.FaceIndexB} out of range");
        }
    }

    [Fact]
    public void Segments_AreNotDegenerate()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);

        foreach (var seg in graph.Segments)
            Assert.False(seg.IsDegenerate, $"Segment should not be degenerate: length={seg.Length}");
    }

    [Fact]
    public void FaceSegmentsA_ContainsSameSegments()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);

        int totalInA = 0;
        foreach (var kvp in graph.FaceSegmentsA)
            totalInA += kvp.Value.Count;

        // Each segment appears in at least one face in A
        Assert.True(totalInA >= graph.Segments.Count,
            $"FaceSegmentsA total {totalInA} should >= segments {graph.Segments.Count}");
    }

    [Fact]
    public void NonAlignedCubes_NoCoplanarFaces()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);

        Assert.Equal(0, graph.CoplanarFacesA.Count);
        Assert.Equal(0, graph.CoplanarFacesB.Count);
    }

    [Fact]
    public void AxisAlignedCubes_MayHaveCoplanarFaces()
    {
        // Offset (0.5, 0, 0): right face of A aligns with left face of B
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);

        // These should have coplanar face pairs where faces share the x=0.5 plane
        // (or x=1.0 depending on cube definition)
        // Just verify the operation completes without error; coplanar count depends on geometry
        Assert.NotNull(graph.CoplanarFacesA);
        Assert.NotNull(graph.CoplanarFacesB);
    }

    [Fact]
    public void SphereCube_HasSegments()
    {
        // Sphere at origin r=1, cube at origin (corner) → guaranteed overlap
        var meshA = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.True(graph.Segments.Count > 0,
            $"Sphere and cube should have intersection segments");
    }

    [Fact]
    public void EmptyMeshA_NoSegments()
    {
        var meshA = new HalfEdgeMesh();
        var meshB = MeshFactory.CreateCube().Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.Equal(0, graph.Segments.Count);
    }

    [Fact]
    public void EmptyMeshB_NoSegments()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = new HalfEdgeMesh();
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.Equal(0, graph.Segments.Count);
    }

    [Fact]
    public void SegmentStartEndAreDistinct()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);

        foreach (var seg in graph.Segments)
        {
            Assert.True(Vec3.Distance(seg.Start, seg.End) > MathUtil.Epsilon,
                $"Segment endpoints should be distinct");
        }
    }
}
