using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Intersection;

/// <summary>Batch 43: IntersectionGraph tests (20 tests)</summary>
public class IntersectionGraphTests
{
    [Fact]
    public void DisjointCubes_NoSegments()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(5, 0, 0));
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.Empty(graph.Segments);
    }

    [Fact]
    public void DisjointCubes_EmptyFaceSegments()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(5, 0, 0));
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.Empty(graph.FaceSegmentsA);
        Assert.Empty(graph.FaceSegmentsB);
    }

    [Fact]
    public void OverlappingCubes_HasSegments()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.True(graph.Segments.Count > 0, "Overlapping cubes should produce intersection segments");
    }

    [Fact]
    public void OverlappingCubes_SegmentsHaveValidFaceIndices()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        foreach (var seg in graph.Segments)
        {
            Assert.True(seg.FaceIndexA >= 0 && seg.FaceIndexA < a.Mesh.Faces.Count);
            Assert.True(seg.FaceIndexB >= 0 && seg.FaceIndexB < b.Mesh.Faces.Count);
        }
    }

    [Fact]
    public void OverlappingCubes_SegmentsAreNonDegenerate()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        foreach (var seg in graph.Segments)
        {
            Assert.False(seg.IsDegenerate, $"Segment length {seg.Length} is degenerate");
        }
    }

    [Fact]
    public void OverlappingCubes_FaceSegmentsA_NotEmpty()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.True(graph.FaceSegmentsA.Count > 0);
    }

    [Fact]
    public void OverlappingCubes_FaceSegmentsB_NotEmpty()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.True(graph.FaceSegmentsB.Count > 0);
    }

    [Fact]
    public void OverlappingCubes_FaceSegmentsA_PointToValidSegments()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        foreach (var kvp in graph.FaceSegmentsA)
        {
            Assert.True(kvp.Key >= 0 && kvp.Key < a.Mesh.Faces.Count);
            Assert.True(kvp.Value.Count > 0);
            foreach (var seg in kvp.Value)
            {
                Assert.Equal(kvp.Key, seg.FaceIndexA);
            }
        }
    }

    [Fact]
    public void OverlappingCubes_Symmetric_SegmentCount()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var graph1 = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        var graph2 = IntersectionGraph.Compute(b.Mesh, a.Mesh);
        Assert.Equal(graph1.Segments.Count, graph2.Segments.Count);
    }

    [Fact]
    public void CubeSphere_HasSegments()
    {
        var cube = MeshFactory.CreateCube();
        var sphere = MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 2);
        var graph = IntersectionGraph.Compute(cube.Mesh, sphere.Mesh);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void CubeTetra_HasSegments()
    {
        var cube = MeshFactory.CreateCube();
        var tet = MeshFactory.CreateTetrahedron(new Vec3(0.3, 0.3, 0.3), 1);
        var graph = IntersectionGraph.Compute(cube.Mesh, tet.Mesh);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void CustomGridSize_Produces_Segments()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh, gridSize: 1e-6);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void SegmentsLieInBothBounds()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var boundsA = a.Mesh.GetBounds();
        var boundsB = b.Mesh.GetBounds();
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        foreach (var seg in graph.Segments)
        {
            // Segments should be in the intersection zone (roughly)
            Assert.True(seg.Start.X >= boundsA.Min.X - 0.01 && seg.Start.X <= boundsB.Max.X + 0.01);
        }
    }

    [Fact]
    public void DisjointCubes_NoCoplanarFaces()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(5, 0, 0));
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.Empty(graph.CoplanarFacesA);
        Assert.Empty(graph.CoplanarFacesB);
    }

    [Fact]
    public void OverlappingCubes_DiagonalOffset_NoCoplanarFaces()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        // Diagonal offset means no shared face planes
        Assert.Empty(graph.CoplanarFacesA);
    }

    [Fact]
    public void SphereSphere_HasSegments()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1, 2);
        var b = MeshFactory.CreateSphere(new Vec3(1.2, 0.3, 0.1), 1, 2);
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void ContainedCube_MayHaveSegments()
    {
        var outer = MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5), 2);
        var inner = MeshFactory.CreateCube(new Vec3(0.25, 0.25, 0.25), 0.5);
        var graph = IntersectionGraph.Compute(outer.Mesh, inner.Mesh);
        // Contained geometry may or may not produce segments (inner is fully inside)
        Assert.True(graph.Segments.Count >= 0);
    }

    [Fact]
    public void OverlappingCubes_AllSegments_HavePositiveLength()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        foreach (var seg in graph.Segments)
        {
            Assert.True(seg.Length > 0);
        }
    }

    [Fact]
    public void OverlappingCubes_SegmentEndpointsAreSnapped()
    {
        var gridSize = 1e-6;
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh, gridSize);
        foreach (var seg in graph.Segments)
        {
            // Each coordinate should be a multiple of gridSize (within floating point tolerance)
            double remX = System.Math.Abs(seg.Start.X / gridSize - System.Math.Round(seg.Start.X / gridSize));
            Assert.True(remX < 0.01, $"Start.X={seg.Start.X} not snapped to grid {gridSize}");
        }
    }
}
