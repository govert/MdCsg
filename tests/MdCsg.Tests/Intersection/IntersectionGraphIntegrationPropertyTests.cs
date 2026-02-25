using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Intersection;

/// <summary>Phase 6: IntersectionGraph.Compute — integration tests for segment computation between meshes</summary>
public class IntersectionGraphIntegrationPropertyTests
{
    [Fact]
    public void DisjointCubes_NoSegments()
    {
        var cubeA = MeshFactory.CreateCube(Vec3.Zero);
        var cubeB = MeshFactory.CreateCube(new Vec3(10, 0, 0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        Assert.Empty(graph.Segments);
    }

    [Fact]
    public void OverlappingCubes_HasSegments()
    {
        var cubeA = MeshFactory.CreateCube(Vec3.Zero);
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        Assert.True(graph.Segments.Count > 0, "Overlapping cubes should produce intersection segments");
    }

    [Fact]
    public void OverlappingCubes_AllSegmentsNonDegenerate()
    {
        var cubeA = MeshFactory.CreateCube(Vec3.Zero);
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        foreach (var seg in graph.Segments)
        {
            Assert.False(seg.IsDegenerate, $"Segment from ({seg.Start}) to ({seg.End}) should not be degenerate");
        }
    }

    [Fact]
    public void OverlappingCubes_FaceSegmentsA_NonEmpty()
    {
        var cubeA = MeshFactory.CreateCube(Vec3.Zero);
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        Assert.True(graph.FaceSegmentsA.Count > 0, "FaceSegmentsA should have entries");
    }

    [Fact]
    public void OverlappingCubes_FaceSegmentsB_NonEmpty()
    {
        var cubeA = MeshFactory.CreateCube(Vec3.Zero);
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        Assert.True(graph.FaceSegmentsB.Count > 0, "FaceSegmentsB should have entries");
    }

    [Fact]
    public void CubeSphere_HasSegments()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero);
        var sphere = MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 2);
        var graph = IntersectionGraph.Compute(cube.Mesh, sphere.Mesh);
        Assert.True(graph.Segments.Count > 0, "Cube-sphere intersection should produce segments");
    }

    [Fact]
    public void CubeSphere_SegmentsHaveValidFaceIndices()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero);
        var sphere = MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 2);
        var graph = IntersectionGraph.Compute(cube.Mesh, sphere.Mesh);
        foreach (var seg in graph.Segments)
        {
            Assert.True(seg.FaceIndexA >= 0 && seg.FaceIndexA < cube.Mesh.Faces.Count,
                $"FaceIndexA {seg.FaceIndexA} out of range");
            Assert.True(seg.FaceIndexB >= 0 && seg.FaceIndexB < sphere.Mesh.Faces.Count,
                $"FaceIndexB {seg.FaceIndexB} out of range");
        }
    }

    [Fact]
    public void CubeSphere_SegmentLength_AllPositive()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero);
        var sphere = MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 2);
        var graph = IntersectionGraph.Compute(cube.Mesh, sphere.Mesh);
        foreach (var seg in graph.Segments)
        {
            Assert.True(seg.Length > 0, $"Segment should have positive length, got {seg.Length}");
        }
    }

    [Fact]
    public void DisjointCubes_NoCoplanarFaces()
    {
        var cubeA = MeshFactory.CreateCube(Vec3.Zero);
        var cubeB = MeshFactory.CreateCube(new Vec3(10, 0, 0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        Assert.Empty(graph.CoplanarFacesA);
        Assert.Empty(graph.CoplanarFacesB);
    }

    [Fact]
    public void TwoSpheres_Overlapping_HasSegments()
    {
        var sphereA = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var sphereB = MeshFactory.CreateSphere(new Vec3(1.0, 0, 0), 1.0, 2);
        var graph = IntersectionGraph.Compute(sphereA.Mesh, sphereB.Mesh);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void TwoSpheres_Disjoint_NoSegments()
    {
        var sphereA = MeshFactory.CreateSphere(Vec3.Zero, 0.5, 2);
        var sphereB = MeshFactory.CreateSphere(new Vec3(5.0, 0, 0), 0.5, 2);
        var graph = IntersectionGraph.Compute(sphereA.Mesh, sphereB.Mesh);
        Assert.Empty(graph.Segments);
    }

    [Fact]
    public void Segments_FaceMapping_Consistent()
    {
        var cubeA = MeshFactory.CreateCube(Vec3.Zero);
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        // Every segment should appear in FaceSegmentsA or FaceSegmentsB
        foreach (var seg in graph.Segments)
        {
            bool inA = graph.FaceSegmentsA.ContainsKey(seg.FaceIndexA) &&
                       graph.FaceSegmentsA[seg.FaceIndexA].Contains(seg);
            bool inB = graph.FaceSegmentsB.ContainsKey(seg.FaceIndexB) &&
                       graph.FaceSegmentsB[seg.FaceIndexB].Contains(seg);
            Assert.True(inA || inB, "Segment should appear in face mapping");
        }
    }
}
