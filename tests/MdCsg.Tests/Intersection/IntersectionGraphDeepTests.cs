using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Intersection;

/// <summary>Phase 6: IntersectionGraph deep tests — segment properties, symmetry, face segment mapping</summary>
public class IntersectionGraphDeepTests
{
    [Fact]
    public void Compute_CubeCube_HasSegments()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void Compute_CubeCube_FaceSegmentsA_NotEmpty()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.True(graph.FaceSegmentsA.Count > 0);
    }

    [Fact]
    public void Compute_CubeCube_FaceSegmentsB_NotEmpty()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.True(graph.FaceSegmentsB.Count > 0);
    }

    [Fact]
    public void Compute_Disjoint_NoSegments()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(10, 0, 0)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.Empty(graph.Segments);
    }

    [Fact]
    public void Compute_Disjoint_EmptyFaceSegments()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(10, 0, 0)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.Empty(graph.FaceSegmentsA);
        Assert.Empty(graph.FaceSegmentsB);
    }

    [Fact]
    public void Compute_CubeCube_AllSegmentsNonDegenerate()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        foreach (var seg in graph.Segments)
        {
            Assert.False(seg.IsDegenerate, "Segment should not be degenerate");
        }
    }

    [Fact]
    public void Compute_CubeCube_SegmentsHaveValidCoordinates()
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
    public void Compute_SphereCube_ManySegments()
    {
        var meshA = MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.6, 2).Mesh;
        var meshB = MeshFactory.CreateCube().Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.True(graph.Segments.Count > 10);
    }

    [Fact]
    public void Compute_FaceSegmentsA_KeysAreValidFaceIndices()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        foreach (var key in graph.FaceSegmentsA.Keys)
        {
            Assert.True(key >= 0 && key < meshA.Faces.Count, $"Invalid face index: {key}");
        }
    }

    [Fact]
    public void Compute_FaceSegmentsB_KeysAreValidFaceIndices()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        foreach (var key in graph.FaceSegmentsB.Keys)
        {
            Assert.True(key >= 0 && key < meshB.Faces.Count, $"Invalid face index: {key}");
        }
    }

    [Fact]
    public void Compute_Contained_NoSegments()
    {
        var meshA = MeshFactory.CreateCube(size: 3).Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(1, 1, 1), size: 0.5).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.Empty(graph.Segments);
    }

    [Fact]
    public void Compute_CustomGridSize_StillWorks()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB, gridSize: 1e-6);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void Compute_Symmetric_SameSegmentCount()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var g1 = IntersectionGraph.Compute(meshA, meshB);
        var g2 = IntersectionGraph.Compute(meshB, meshA);
        Assert.Equal(g1.Segments.Count, g2.Segments.Count);
    }

    [Fact]
    public void IntersectionSegment_Properties_Accessible()
    {
        var seg = new IntersectionSegment(new Vec3(0, 0, 0), new Vec3(1, 0, 0), 5, 7);
        Assert.Equal(0, seg.Start.X, 10);
        Assert.Equal(1, seg.End.X, 10);
        Assert.Equal(5, seg.FaceIndexA);
        Assert.Equal(7, seg.FaceIndexB);
    }

    [Fact]
    public void IntersectionSegment_IsDegenerate_ZeroLength()
    {
        var seg = new IntersectionSegment(new Vec3(1, 1, 1), new Vec3(1, 1, 1), 0, 0);
        Assert.True(seg.IsDegenerate);
    }

    [Fact]
    public void IntersectionSegment_IsDegenerate_NonZeroLength()
    {
        var seg = new IntersectionSegment(new Vec3(0, 0, 0), new Vec3(0, 0, 0.001), 0, 0);
        Assert.False(seg.IsDegenerate);
    }
}
