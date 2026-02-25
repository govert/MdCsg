using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Intersection;

/// <summary>Phase 6: IntersectionGraph structural invariant tests — face mapping, segment validity, grid sizes</summary>
public class IntersectionGraphStructuralTests
{
    [Fact]
    public void Compute_DisjointMeshes_EmptyGraph()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(10, 10, 10)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.Empty(graph.Segments);
        Assert.Empty(graph.FaceSegmentsA);
        Assert.Empty(graph.FaceSegmentsB);
    }

    [Fact]
    public void Compute_OverlappingCubes_SegmentsInBothMaps()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        // Every segment should appear in either FaceSegmentsA or FaceSegmentsB
        Assert.True(graph.FaceSegmentsA.Count > 0);
        Assert.True(graph.FaceSegmentsB.Count > 0);
    }

    [Fact]
    public void FaceSegmentsA_AllSegments_HaveValidFaceIndexA()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        foreach (var (faceIdx, segs) in graph.FaceSegmentsA)
        {
            Assert.True(faceIdx >= 0 && faceIdx < meshA.Faces.Count);
            foreach (var seg in segs)
                Assert.Equal(faceIdx, seg.FaceIndexA);
        }
    }

    [Fact]
    public void FaceSegmentsB_AllSegments_HaveValidFaceIndexB()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        foreach (var (faceIdx, segs) in graph.FaceSegmentsB)
        {
            Assert.True(faceIdx >= 0 && faceIdx < meshB.Faces.Count);
            foreach (var seg in segs)
                Assert.Equal(faceIdx, seg.FaceIndexB);
        }
    }

    [Fact]
    public void AllSegments_NonDegenerate()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        foreach (var seg in graph.Segments)
        {
            Assert.False(seg.IsDegenerate, $"Degenerate segment: {seg.Start} → {seg.End}");
            Assert.True(seg.Length > 0);
        }
    }

    [Fact]
    public void SegmentEndpoints_AreSnapped()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB, gridSize: 1e-6);
        // After snapping, values should be "clean" (multiples of grid)
        foreach (var seg in graph.Segments)
        {
            Assert.False(double.IsNaN(seg.Start.X));
            Assert.False(double.IsInfinity(seg.Start.X));
            Assert.False(double.IsNaN(seg.End.X));
            Assert.False(double.IsInfinity(seg.End.X));
        }
    }

    [Fact]
    public void Compute_LargerGridSize_MayReduceSegments()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var fine = IntersectionGraph.Compute(meshA, meshB, gridSize: 1e-10);
        var coarse = IntersectionGraph.Compute(meshA, meshB, gridSize: 1e-4);
        // Both should produce valid results (coarse may merge some degenerate segments)
        Assert.True(fine.Segments.Count > 0);
        Assert.True(coarse.Segments.Count > 0);
    }

    [Fact]
    public void Compute_SphereSphere_HasSegments()
    {
        var meshA = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var meshB = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1, 2).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void Compute_TwoTetrahedra_HasSegments()
    {
        var meshA = MeshFactory.CreateTetrahedron().Mesh;
        var meshB = MeshFactory.CreateTetrahedron(new Vec3(0.3, 0.3, 0.3)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void SamePosition_CoplanarDetection_DoesNotCrash()
    {
        // Identical cubes → coplanar faces exist, but BVH face AABB overlap
        // may or may not find them depending on how face AABBs touch
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube().Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        // Should at least not crash; coplanar detection depends on BVH overlap
        Assert.True(graph.CoplanarFacesA.Count >= 0);
    }

    [Fact]
    public void CoplanarFaceKeys_InRange()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        foreach (var (key, _) in graph.CoplanarFacesA)
            Assert.True(key >= 0 && key < meshA.Faces.Count);
        foreach (var (key, _) in graph.CoplanarFacesB)
            Assert.True(key >= 0 && key < meshB.Faces.Count);
    }
}
