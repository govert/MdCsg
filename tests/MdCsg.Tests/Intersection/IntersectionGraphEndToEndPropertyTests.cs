using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Intersection;

/// <summary>Phase 6: IntersectionGraph — end-to-end computation, segment symmetry, face segment mapping, coplanar tracking</summary>
public class IntersectionGraphEndToEndPropertyTests
{
    [Fact]
    public void OverlappingCubes_ProducesSegments()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.True(graph.Segments.Count > 0, "Overlapping cubes should produce intersection segments");
    }

    [Fact]
    public void DisjointCubes_NoSegments()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(100, 0, 0));
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.Equal(0, graph.Segments.Count);
    }

    [Fact]
    public void OverlappingCubes_FaceSegmentsA_NonEmpty()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.True(graph.FaceSegmentsA.Count > 0, "Should have face segments for mesh A");
    }

    [Fact]
    public void OverlappingCubes_FaceSegmentsB_NonEmpty()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.True(graph.FaceSegmentsB.Count > 0, "Should have face segments for mesh B");
    }

    [Fact]
    public void DisjointCubes_EmptyFaceSegments()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(100, 0, 0));
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.Equal(0, graph.FaceSegmentsA.Count);
        Assert.Equal(0, graph.FaceSegmentsB.Count);
    }

    [Fact]
    public void SegmentFaceIndices_ValidRange()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        foreach (var seg in graph.Segments)
        {
            Assert.True(seg.FaceIndexA >= 0 && seg.FaceIndexA < a.Mesh.Faces.Count,
                $"FaceIndexA {seg.FaceIndexA} out of range");
            Assert.True(seg.FaceIndexB >= 0 && seg.FaceIndexB < b.Mesh.Faces.Count,
                $"FaceIndexB {seg.FaceIndexB} out of range");
        }
    }

    [Fact]
    public void SegmentEndpoints_NonDegenerate()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        foreach (var seg in graph.Segments)
        {
            double len = Vec3.Distance(seg.Start, seg.End);
            Assert.True(len > 1e-12, $"Segment should be non-degenerate, length={len}");
        }
    }

    [Fact]
    public void Deterministic_SameSegmentCount()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var g1 = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        var g2 = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.Equal(g1.Segments.Count, g2.Segments.Count);
    }

    [Fact]
    public void FaceSegmentsA_KeysInRange()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        foreach (var kvp in graph.FaceSegmentsA)
        {
            Assert.True(kvp.Key >= 0 && kvp.Key < a.Mesh.Faces.Count,
                $"FaceSegmentsA key {kvp.Key} out of range");
            Assert.True(kvp.Value.Count > 0, "Face segment list should not be empty");
        }
    }

    [Fact]
    public void FaceSegmentsB_KeysInRange()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        foreach (var kvp in graph.FaceSegmentsB)
        {
            Assert.True(kvp.Key >= 0 && kvp.Key < b.Mesh.Faces.Count,
                $"FaceSegmentsB key {kvp.Key} out of range");
            Assert.True(kvp.Value.Count > 0, "Face segment list should not be empty");
        }
    }

    [Fact]
    public void CubeSphere_ProducesSegments()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var sphere = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var graph = IntersectionGraph.Compute(cube.Mesh, sphere.Mesh);
        Assert.True(graph.Segments.Count > 0, "Cube-sphere overlap should produce segments");
    }

    [Fact]
    public void CoplanarFaces_TrackedForSamePlane()
    {
        // Two cubes at the same position will have coplanar faces
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        // Identical cubes → all faces coplanar
        Assert.True(graph.CoplanarFacesA.Count > 0 || graph.Segments.Count > 0,
            "Identical cubes should have coplanar faces or intersection segments");
    }

    [Fact]
    public void CustomGridSize_Works()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh, 1e-12);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void EmptyMeshA_NoSegments()
    {
        var a = MdCsg.Api.Solid.FromTriangles(Array.Empty<Triangle3>());
        var b = MeshFactory.CreateCube();
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.Equal(0, graph.Segments.Count);
    }

    [Fact]
    public void EmptyMeshB_NoSegments()
    {
        var a = MeshFactory.CreateCube();
        var b = MdCsg.Api.Solid.FromTriangles(Array.Empty<Triangle3>());
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.Equal(0, graph.Segments.Count);
    }

    [Fact]
    public void SegmentsInFaceSegments_ConsistentWithGlobalList()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph = IntersectionGraph.Compute(a.Mesh, b.Mesh);

        // Count total segments in FaceSegmentsA and FaceSegmentsB
        int totalInA = 0;
        foreach (var kvp in graph.FaceSegmentsA)
            totalInA += kvp.Value.Count;

        int totalInB = 0;
        foreach (var kvp in graph.FaceSegmentsB)
            totalInB += kvp.Value.Count;

        // Non-coplanar segments appear in both A and B mappings;
        // coplanar segments only in one. So totalInA + totalInB >= Segments.Count
        Assert.True(totalInA + totalInB >= graph.Segments.Count,
            $"Combined face segments ({totalInA}+{totalInB}) should be >= global segments ({graph.Segments.Count})");
        Assert.True(totalInA > 0, "FaceSegmentsA should have entries for overlapping cubes");
        Assert.True(totalInB > 0, "FaceSegmentsB should have entries for overlapping cubes");
    }
}
