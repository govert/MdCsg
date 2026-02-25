using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: IntersectionGraph.Compute — segment counts, face mapping, coplanar detection, disjoint/overlapping</summary>
public class IntersectionGraphIntegrationPropertyTests
{
    [Fact]
    public void Compute_OverlappingCubes_HasSegments()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void Compute_DisjointCubes_NoSegments()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 1.0);
        var b = MeshFactory.CreateCube(new Vec3(10, 0, 0), 1.0);
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.Equal(0, graph.Segments.Count);
    }

    [Fact]
    public void Compute_OverlappingSpheres_HasSegments()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void Compute_FaceSegmentsA_MapsToValidFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        foreach (var kvp in graph.FaceSegmentsA)
        {
            Assert.True(kvp.Key >= 0 && kvp.Key < a.Mesh.Faces.Count);
            Assert.True(kvp.Value.Count > 0);
        }
    }

    [Fact]
    public void Compute_FaceSegmentsB_MapsToValidFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        foreach (var kvp in graph.FaceSegmentsB)
        {
            Assert.True(kvp.Key >= 0 && kvp.Key < b.Mesh.Faces.Count);
            Assert.True(kvp.Value.Count > 0);
        }
    }

    [Fact]
    public void Compute_AllSegments_NonDegenerate()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 2);
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        foreach (var seg in graph.Segments)
        {
            Assert.False(seg.IsDegenerate);
            Assert.True(seg.Length > 0);
        }
    }

    [Fact]
    public void Compute_DisjointSpheres_EmptyFaceMaps()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var b = MeshFactory.CreateSphere(new Vec3(10, 0, 0), 1.0, 1);
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.Equal(0, graph.FaceSegmentsA.Count);
        Assert.Equal(0, graph.FaceSegmentsB.Count);
    }

    [Fact]
    public void Compute_OverlappingCubes_SegmentFaceIndicesValid()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        foreach (var seg in graph.Segments)
        {
            Assert.True(seg.FaceIndexA >= 0 && seg.FaceIndexA < a.Mesh.Faces.Count);
            Assert.True(seg.FaceIndexB >= 0 && seg.FaceIndexB < b.Mesh.Faces.Count);
        }
    }

    [Fact]
    public void Compute_Deterministic_SameResult()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var g1 = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        var g2 = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.Equal(g1.Segments.Count, g2.Segments.Count);
    }

    [Fact]
    public void Compute_CubeSphere_HasFaceSegmentsForBoth()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 2);
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.True(graph.FaceSegmentsA.Count > 0);
        Assert.True(graph.FaceSegmentsB.Count > 0);
    }

    [Fact]
    public void Compute_HigherSubdivision_MoreSegments()
    {
        var a1 = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var b1 = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var g1 = IntersectionGraph.Compute(a1.Mesh, b1.Mesh);

        var a2 = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var b2 = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 2);
        var g2 = IntersectionGraph.Compute(a2.Mesh, b2.Mesh);

        Assert.True(g2.Segments.Count > g1.Segments.Count);
    }

    [Fact]
    public void Compute_CoplanarFaces_Empty_ForNonCoplanar()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        // Spheres are unlikely to have coplanar faces
        Assert.Equal(0, graph.CoplanarFacesA.Count);
        Assert.Equal(0, graph.CoplanarFacesB.Count);
    }

    [Fact]
    public void Compute_TetrahedronCube_HasSegments()
    {
        var a = MeshFactory.CreateTetrahedron(new Vec3(0.5, 0.5, 0.5));
        var b = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.True(graph.Segments.Count > 0);
    }
}
