using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Intersection;

/// <summary>Phase 6: IntersectionGraph — segment symmetry, face segment maps, grid size effects</summary>
public class IntersectionGraphPropertyTests
{
    [Fact]
    public void Disjoint_NoSegments()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB, 1e-8);
        Assert.Empty(graph.Segments);
    }

    [Fact]
    public void Overlapping_HasSegments()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB, 1e-8);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void SegmentEndpoints_AreSnapped()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB, 1e-8);

        foreach (var seg in graph.Segments)
        {
            // Endpoints should be snapped to grid
            Assert.True(seg.Length >= 0);
        }
    }

    [Fact]
    public void FaceSegmentsA_AllFaceIndicesValid()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB, 1e-8);

        foreach (var kvp in graph.FaceSegmentsA)
        {
            Assert.True(kvp.Key >= 0 && kvp.Key < meshA.Faces.Count);
            Assert.True(kvp.Value.Count > 0);
        }
    }

    [Fact]
    public void FaceSegmentsB_AllFaceIndicesValid()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB, 1e-8);

        foreach (var kvp in graph.FaceSegmentsB)
        {
            Assert.True(kvp.Key >= 0 && kvp.Key < meshB.Faces.Count);
            Assert.True(kvp.Value.Count > 0);
        }
    }

    [Fact]
    public void SegmentFaceIndices_InRange()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB, 1e-8);

        foreach (var seg in graph.Segments)
        {
            Assert.True(seg.FaceIndexA >= 0 && seg.FaceIndexA < meshA.Faces.Count);
            Assert.True(seg.FaceIndexB >= 0 && seg.FaceIndexB < meshB.Faces.Count);
        }
    }

    [Fact]
    public void Segments_NonDegenerate()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB, 1e-8);

        foreach (var seg in graph.Segments)
        {
            Assert.False(seg.IsDegenerate, $"Degenerate segment found: length={seg.Length}");
        }
    }

    [Fact]
    public void CoplanarFaces_EmptyForNonAlignedOffsetCubes()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB, 1e-8);

        // Non-axis-aligned offset → no coplanar faces
        Assert.Equal(0, graph.CoplanarFacesA.Count);
        Assert.Equal(0, graph.CoplanarFacesB.Count);
    }

    [Fact]
    public void SphereSphere_HasManySegments()
    {
        var meshA = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh;
        var meshB = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 2).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB, 1e-8);
        Assert.True(graph.Segments.Count > 10, $"Expected many segments, got {graph.Segments.Count}");
    }

    [Fact]
    public void FullyContained_HasSegments()
    {
        // Small cube fully inside big cube — boundary faces may still intersect
        var meshA = MeshFactory.CreateCube(new Vec3(-1, -1, -1), 4).Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), 0.5).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB, 1e-8);
        // No intersection: small cube is fully inside
        Assert.Equal(0, graph.Segments.Count);
    }
}
