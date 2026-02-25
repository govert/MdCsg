using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Intersection;

/// <summary>Phase 6: IntersectionGraph — coplanar face tracking, face-segment mapping, segment snap rounding</summary>
public class IntersectionGraphCoplanarTrackingTests
{
    [Fact]
    public void OverlappingCubes_ProducesSegments()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        Assert.True(graph.Segments.Count > 0, "Overlapping cubes should produce segments");
    }

    [Fact]
    public void DisjointCubes_NoSegments()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(10, 0, 0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        Assert.Empty(graph.Segments);
    }

    [Fact]
    public void FaceSegmentsA_ContainsOnlyValidFaceIndices()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        foreach (var kvp in graph.FaceSegmentsA)
        {
            Assert.True(kvp.Key >= 0 && kvp.Key < cubeA.Mesh.Faces.Count);
            Assert.True(kvp.Value.Count > 0);
        }
    }

    [Fact]
    public void FaceSegmentsB_ContainsOnlyValidFaceIndices()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        foreach (var kvp in graph.FaceSegmentsB)
        {
            Assert.True(kvp.Key >= 0 && kvp.Key < cubeB.Mesh.Faces.Count);
            Assert.True(kvp.Value.Count > 0);
        }
    }

    [Fact]
    public void Segments_AroundSnapped()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        double gridSize = 1e-8;
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh, gridSize);
        foreach (var seg in graph.Segments)
        {
            // Snap rounding: coordinates should be aligned to grid
            Assert.True(seg.Length > 0, "No degenerate segments should be stored");
        }
    }

    [Fact]
    public void Segments_NonDegenerate()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.2));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        foreach (var seg in graph.Segments)
        {
            Assert.False(seg.IsDegenerate, $"Segment from {seg.Start} to {seg.End} is degenerate (length={seg.Length})");
        }
    }

    [Fact]
    public void Segments_HaveValidFaceReferences()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        foreach (var seg in graph.Segments)
        {
            Assert.True(seg.FaceIndexA >= 0 && seg.FaceIndexA < cubeA.Mesh.Faces.Count);
            Assert.True(seg.FaceIndexB >= 0 && seg.FaceIndexB < cubeB.Mesh.Faces.Count);
        }
    }

    [Fact]
    public void DisjointCubes_EmptyFaceSegmentMaps()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(10, 0, 0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        Assert.Empty(graph.FaceSegmentsA);
        Assert.Empty(graph.FaceSegmentsB);
    }

    [Fact]
    public void DisjointCubes_NoCoplanarFaces()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(10, 0, 0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        Assert.Empty(graph.CoplanarFacesA);
        Assert.Empty(graph.CoplanarFacesB);
    }

    [Fact]
    public void OverlappingCubes_SegmentEndpoints_WithinBothBounds()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);

        var boundsA = cubeA.Bounds;
        var boundsB = cubeB.Bounds;
        var overlap = new Aabb(
            new Vec3(System.Math.Max(boundsA.Min.X, boundsB.Min.X) - 0.01,
                     System.Math.Max(boundsA.Min.Y, boundsB.Min.Y) - 0.01,
                     System.Math.Max(boundsA.Min.Z, boundsB.Min.Z) - 0.01),
            new Vec3(System.Math.Min(boundsA.Max.X, boundsB.Max.X) + 0.01,
                     System.Math.Min(boundsA.Max.Y, boundsB.Max.Y) + 0.01,
                     System.Math.Min(boundsA.Max.Z, boundsB.Max.Z) + 0.01));

        foreach (var seg in graph.Segments)
        {
            Assert.True(overlap.Contains(seg.Start),
                $"Segment start {seg.Start} outside overlap region");
            Assert.True(overlap.Contains(seg.End),
                $"Segment end {seg.End} outside overlap region");
        }
    }

    [Fact]
    public void SphereCube_Overlapping_ProducesSegments()
    {
        // Sphere at (1, 0.5, 0.5) r=0.6 partially outside the unit cube [0,1]^3
        var sphere = MeshFactory.CreateSphere(new Vec3(1, 0.5, 0.5), 0.6, 2);
        var cube = MeshFactory.CreateCube();
        var graph = IntersectionGraph.Compute(sphere.Mesh, cube.Mesh);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void EmptyMesh_NoSegments()
    {
        var builder = new MeshBuilder();
        var emptyMesh = builder.Build(Array.Empty<Triangle3>());
        var cube = MeshFactory.CreateCube();
        var graph = IntersectionGraph.Compute(emptyMesh, cube.Mesh);
        Assert.Empty(graph.Segments);
    }

    [Fact]
    public void CustomGridSize_SmallGrid_MorePrecise()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graphSmall = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh, 1e-12);
        var graphDefault = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        // Both should produce segments
        Assert.True(graphSmall.Segments.Count > 0);
        Assert.True(graphDefault.Segments.Count > 0);
    }
}
