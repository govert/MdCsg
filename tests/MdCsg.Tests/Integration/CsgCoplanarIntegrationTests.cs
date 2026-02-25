using MdCsg.Api;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: Coplanar face handling through the full CSG pipeline — IntersectionGraph tracking and operations</summary>
public class CsgCoplanarIntegrationTests
{
    [Fact]
    public void IntersectionGraph_DisjointCubes_NoCoplanarFaces()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(100, 0, 0)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.Equal(0, graph.CoplanarFacesA.Count);
        Assert.Equal(0, graph.CoplanarFacesB.Count);
    }

    [Fact]
    public void IntersectionGraph_OverlappingCubes_NoCoplanar()
    {
        // Offset cubes that intersect but share no coplanar faces
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        // No coplanar faces expected since faces are on different planes
        Assert.Equal(0, graph.CoplanarFacesA.Count);
        Assert.Equal(0, graph.CoplanarFacesB.Count);
    }

    [Fact]
    public void IntersectionGraph_HasSegments_WhenOverlapping()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.True(graph.Segments.Count > 0, "Overlapping cubes should produce intersection segments");
    }

    [Fact]
    public void IntersectionGraph_DisjointCubes_NoSegments()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(100, 0, 0)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.Equal(0, graph.Segments.Count);
    }

    [Fact]
    public void IntersectionGraph_FaceSegmentsA_KeysAreValidFaceIndices()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        foreach (var kvp in graph.FaceSegmentsA)
        {
            Assert.True(kvp.Key >= 0 && kvp.Key < meshA.Faces.Count,
                $"Face index {kvp.Key} out of range for mesh A");
        }
    }

    [Fact]
    public void IntersectionGraph_FaceSegmentsB_KeysAreValidFaceIndices()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        foreach (var kvp in graph.FaceSegmentsB)
        {
            Assert.True(kvp.Key >= 0 && kvp.Key < meshB.Faces.Count,
                $"Face index {kvp.Key} out of range for mesh B");
        }
    }

    [Fact]
    public void IntersectionGraph_Segments_HaveValidFaceIndices()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        foreach (var seg in graph.Segments)
        {
            Assert.True(seg.FaceIndexA >= 0 && seg.FaceIndexA < meshA.Faces.Count);
            Assert.True(seg.FaceIndexB >= 0 && seg.FaceIndexB < meshB.Faces.Count);
        }
    }

    [Fact]
    public void IntersectionGraph_Segments_AreNotDegenerate()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        foreach (var seg in graph.Segments)
            Assert.False(seg.IsDegenerate, $"Segment {seg.Start} → {seg.End} is degenerate");
    }

    [Fact]
    public void IntersectionGraph_Segments_Symmetric()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graphAB = IntersectionGraph.Compute(meshA, meshB);
        var graphBA = IntersectionGraph.Compute(meshB, meshA);
        // Same number of segments regardless of order
        Assert.Equal(graphAB.Segments.Count, graphBA.Segments.Count);
    }

    [Fact]
    public void IntersectionGraph_CustomGridSize_StillProduces()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB, gridSize: 1e-6);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void Csg_Union_WithCoplanarFaces_Succeeds()
    {
        // Two cubes sharing an exact face should work via coplanar path
        var a = new Solid(MeshFactory.CreateCube(Vec3.Zero, 1).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(1, 0, 0), 1).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Csg_Difference_WithCoplanarFaces_Succeeds()
    {
        var a = new Solid(MeshFactory.CreateCube(Vec3.Zero, 1).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(1, 0, 0), 1).Mesh);
        var result = Csg.Difference(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Csg_Intersect_NearlyCoplanar_Succeeds()
    {
        // Two cubes offset by a tiny amount, so faces are nearly coplanar
        var a = new Solid(MeshFactory.CreateCube(Vec3.Zero, 1).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.999, 0, 0), 1).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void IntersectionGraph_SphereCube_HasSegments()
    {
        var meshA = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var meshB = MeshFactory.CreateCube(Vec3.Zero, 1).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void IntersectionGraph_SphereSphere_HasSegments()
    {
        var meshA = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var meshB = MeshFactory.CreateSphere(new Vec3(0.8, 0, 0), 1, 2).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void IntersectionGraph_Segments_EndpointsAreSnapped()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        double gridSize = 1e-8;
        var graph = IntersectionGraph.Compute(meshA, meshB, gridSize);
        foreach (var seg in graph.Segments)
        {
            // Snap-rounded endpoints should be representable on the grid
            var reSnappedStart = SnapRounding.Snap(seg.Start, gridSize);
            var reSnappedEnd = SnapRounding.Snap(seg.End, gridSize);
            Assert.Equal(seg.Start.X, reSnappedStart.X, 1e-12);
            Assert.Equal(seg.Start.Y, reSnappedStart.Y, 1e-12);
            Assert.Equal(seg.Start.Z, reSnappedStart.Z, 1e-12);
            Assert.Equal(seg.End.X, reSnappedEnd.X, 1e-12);
            Assert.Equal(seg.End.Y, reSnappedEnd.Y, 1e-12);
            Assert.Equal(seg.End.Z, reSnappedEnd.Z, 1e-12);
        }
    }
}
