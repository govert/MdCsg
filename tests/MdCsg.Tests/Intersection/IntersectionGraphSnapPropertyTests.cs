using MdCsg.Api;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Intersection;

/// <summary>Phase 6: IntersectionGraph — Compute, snap rounding, face segment mapping, coplanar tracking</summary>
public class IntersectionGraphSnapPropertyTests
{
    [Fact]
    public void Compute_OverlappingCubes_HasSegments()
    {
        var cubeA = MeshFactory.CreateCube(Vec3.Zero);
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0.0, 0.0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        Assert.True(graph.Segments.Count > 0, "Overlapping cubes should have intersection segments");
    }

    [Fact]
    public void Compute_DisjointCubes_NoSegments()
    {
        var cubeA = MeshFactory.CreateCube(Vec3.Zero);
        var cubeB = MeshFactory.CreateCube(new Vec3(10, 0, 0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        Assert.Equal(0, graph.Segments.Count);
    }

    [Fact]
    public void Compute_FaceSegmentsA_KeysValid()
    {
        var cubeA = MeshFactory.CreateCube(Vec3.Zero);
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0.0, 0.0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        foreach (var key in graph.FaceSegmentsA.Keys)
        {
            Assert.True(key >= 0 && key < cubeA.Mesh.Faces.Count,
                $"Face index {key} should be valid for mesh A");
        }
    }

    [Fact]
    public void Compute_FaceSegmentsB_KeysValid()
    {
        var cubeA = MeshFactory.CreateCube(Vec3.Zero);
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0.0, 0.0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        foreach (var key in graph.FaceSegmentsB.Keys)
        {
            Assert.True(key >= 0 && key < cubeB.Mesh.Faces.Count,
                $"Face index {key} should be valid for mesh B");
        }
    }

    [Fact]
    public void Compute_SegmentsAreNonDegenerate()
    {
        var cubeA = MeshFactory.CreateCube(Vec3.Zero);
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0.0, 0.0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        foreach (var seg in graph.Segments)
        {
            Assert.False(seg.IsDegenerate, $"Segment {seg.Start} -> {seg.End} should not be degenerate");
        }
    }

    [Fact]
    public void Compute_SegmentFaceReferencesValid()
    {
        var cubeA = MeshFactory.CreateCube(Vec3.Zero);
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0.0, 0.0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        foreach (var seg in graph.Segments)
        {
            Assert.True(seg.FaceIndexA >= 0 && seg.FaceIndexA < cubeA.Mesh.Faces.Count);
            Assert.True(seg.FaceIndexB >= 0 && seg.FaceIndexB < cubeB.Mesh.Faces.Count);
        }
    }

    [Fact]
    public void Compute_FaceSegmentsA_ListsContainSegments()
    {
        var cubeA = MeshFactory.CreateCube(Vec3.Zero);
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0.0, 0.0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        foreach (var kvp in graph.FaceSegmentsA)
        {
            Assert.True(kvp.Value.Count > 0, $"Face {kvp.Key} segment list should not be empty");
            foreach (var seg in kvp.Value)
            {
                Assert.Equal(kvp.Key, seg.FaceIndexA);
            }
        }
    }

    [Fact]
    public void Compute_FaceSegmentsB_ListsContainSegments()
    {
        var cubeA = MeshFactory.CreateCube(Vec3.Zero);
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0.0, 0.0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        foreach (var kvp in graph.FaceSegmentsB)
        {
            Assert.True(kvp.Value.Count > 0);
            foreach (var seg in kvp.Value)
            {
                Assert.Equal(kvp.Key, seg.FaceIndexB);
            }
        }
    }

    [Fact]
    public void Compute_CubeSphere_HasSegments()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero);
        var sphere = MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 2);
        var graph = IntersectionGraph.Compute(cube.Mesh, sphere.Mesh);
        Assert.True(graph.Segments.Count > 0, "Overlapping cube-sphere should have intersection segments");
    }

    [Fact]
    public void Compute_DisjointCubes_EmptyMaps()
    {
        var cubeA = MeshFactory.CreateCube(Vec3.Zero);
        var cubeB = MeshFactory.CreateCube(new Vec3(10, 0, 0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        Assert.Empty(graph.FaceSegmentsA);
        Assert.Empty(graph.FaceSegmentsB);
    }

    [Fact]
    public void Compute_SegmentEndpoints_WithinBounds()
    {
        var cubeA = MeshFactory.CreateCube(Vec3.Zero);
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0.0, 0.0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        var boundsA = cubeA.Bounds;
        var boundsB = cubeB.Bounds;
        foreach (var seg in graph.Segments)
        {
            // Endpoints should be within the overlap region (with tolerance)
            Assert.True(seg.Start.X >= boundsA.Min.X - 0.01 && seg.Start.X <= boundsB.Max.X + 0.01);
            Assert.True(seg.End.X >= boundsA.Min.X - 0.01 && seg.End.X <= boundsB.Max.X + 0.01);
        }
    }

    [Fact]
    public void Compute_TwoSpheres_HasSegments()
    {
        var sphereA = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var sphereB = MeshFactory.CreateSphere(new Vec3(1.0, 0, 0), 1.0, 2);
        var graph = IntersectionGraph.Compute(sphereA.Mesh, sphereB.Mesh);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void SnapRounding_Snap_RoundsToGrid()
    {
        double gridSize = 0.1;
        var v = new Vec3(0.123456, 0.789012, 0.345678);
        var snapped = SnapRounding.Snap(v, gridSize);
        // Each component should be a multiple of gridSize
        Assert.True(System.Math.Abs(snapped.X / gridSize - System.Math.Round(snapped.X / gridSize)) < 1e-10);
        Assert.True(System.Math.Abs(snapped.Y / gridSize - System.Math.Round(snapped.Y / gridSize)) < 1e-10);
        Assert.True(System.Math.Abs(snapped.Z / gridSize - System.Math.Round(snapped.Z / gridSize)) < 1e-10);
    }

    [Fact]
    public void SnapRounding_Snap_PreservesExact()
    {
        double gridSize = 0.5;
        var v = new Vec3(1.0, 2.0, 3.0);
        var snapped = SnapRounding.Snap(v, gridSize);
        Assert.True(System.Math.Abs(snapped.X - 1.0) < 1e-10);
        Assert.True(System.Math.Abs(snapped.Y - 2.0) < 1e-10);
        Assert.True(System.Math.Abs(snapped.Z - 3.0) < 1e-10);
    }

    [Fact]
    public void IntersectionSegment_IsDegenerate_WhenStartEqualsEnd()
    {
        var seg = new IntersectionSegment(Vec3.Zero, Vec3.Zero, 0, 0);
        Assert.True(seg.IsDegenerate);
    }

    [Fact]
    public void IntersectionSegment_NotDegenerate_WhenDifferent()
    {
        var seg = new IntersectionSegment(Vec3.Zero, Vec3.UnitX, 0, 0);
        Assert.False(seg.IsDegenerate);
    }
}
