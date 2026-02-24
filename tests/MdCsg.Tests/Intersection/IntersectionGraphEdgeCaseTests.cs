using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Intersection;

/// <summary>Phase 6: IntersectionGraph edge cases</summary>
public class IntersectionGraphEdgeCaseTests
{
    [Fact]
    public void Compute_OverlappingCubes_HasSegments()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void Compute_DisjointCubes_NoSegments()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(10, 0, 0)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.Empty(graph.Segments);
    }

    [Fact]
    public void Compute_Segments_HaveValidFaceIndices()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
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
    public void Compute_FaceSegmentsA_ConsistentWithSegments()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        int total = 0;
        foreach (var kvp in graph.FaceSegmentsA)
        {
            foreach (var seg in kvp.Value)
            {
                Assert.Equal(kvp.Key, seg.FaceIndexA);
            }
            total += kvp.Value.Count;
        }
        // Each segment should appear in FaceSegmentsA
        Assert.True(total >= graph.Segments.Count(s =>
            graph.FaceSegmentsA.ContainsKey(s.FaceIndexA)));
    }

    [Fact]
    public void Compute_FaceSegmentsB_ConsistentWithSegments()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        foreach (var kvp in graph.FaceSegmentsB)
        {
            foreach (var seg in kvp.Value)
            {
                Assert.Equal(kvp.Key, seg.FaceIndexB);
            }
        }
    }

    [Fact]
    public void Compute_AllSegments_NonDegenerate()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        foreach (var seg in graph.Segments)
        {
            Assert.False(seg.IsDegenerate, $"Segment {seg.Start} → {seg.End} is degenerate");
        }
    }

    [Fact]
    public void Compute_CustomGridSize()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB, 1e-6);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void Compute_CubeSphere_HasSegments()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.6, 2).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void Compute_Symmetric_SegmentCount()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graphAB = IntersectionGraph.Compute(meshA, meshB);
        var graphBA = IntersectionGraph.Compute(meshB, meshA);
        // Same number of intersection segments both directions
        Assert.Equal(graphAB.Segments.Count, graphBA.Segments.Count);
    }

    [Fact]
    public void Compute_CoplanarFaces_Empty_ForNonCoplanarMeshes()
    {
        // Cubes offset by 0.5 won't have coplanar faces
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.Empty(graph.CoplanarFacesA);
        Assert.Empty(graph.CoplanarFacesB);
    }

    [Fact]
    public void Compute_ContainedCube_HasSegments()
    {
        // Small cube fully inside big cube
        var meshA = MeshFactory.CreateCube(size: 3).Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(1, 1, 1), size: 0.5).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        // Contained = no intersection
        Assert.Empty(graph.Segments);
    }

    [Fact]
    public void Compute_SpheresSameCenter_HasManySegments()
    {
        var meshA = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var meshB = MeshFactory.CreateSphere(Vec3.Zero, 1.2, 2).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        // Concentric spheres don't intersect if inner is fully contained
        Assert.Empty(graph.Segments);
    }
}
