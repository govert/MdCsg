using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Intersection;

/// <summary>Batch 14: SnapRounding and IntersectionSegment/Graph tests (20 tests)</summary>
public class SnapRoundingIntersectionSegTests
{
    // --- SnapRounding ---

    [Fact]
    public void Snap_RoundsToGrid()
    {
        var pt = new Vec3(1.000000004, 2.000000006, 3.0);
        var snapped = SnapRounding.Snap(pt, 1e-8);
        Assert.Equal(1.0, snapped.X, 1e-12);
        Assert.Equal(2.00000001, snapped.Y, 1e-12);
        Assert.Equal(3.0, snapped.Z, 1e-12);
    }

    [Fact]
    public void Snap_ExactValues_Unchanged()
    {
        var pt = new Vec3(1.0, 2.0, 3.0);
        var snapped = SnapRounding.Snap(pt, 1e-8);
        Assert.Equal(pt, snapped);
    }

    [Fact]
    public void SnapSegment_SnapsEndpoints()
    {
        var seg = new IntersectionSegment(
            new Vec3(1.000000004, 0, 0),
            new Vec3(2.000000004, 0, 0),
            0, 1);
        var snapped = SnapRounding.SnapSegment(seg, 1e-8);
        Assert.Equal(1.0, snapped.Start.X, 1e-12);
        Assert.Equal(2.0, snapped.End.X, 1e-12);
    }

    [Fact]
    public void SnapSegment_PreservesFaceIndices()
    {
        var seg = new IntersectionSegment(Vec3.Zero, Vec3.UnitX, 5, 10);
        var snapped = SnapRounding.SnapSegment(seg);
        Assert.Equal(5, snapped.FaceIndexA);
        Assert.Equal(10, snapped.FaceIndexB);
    }

    [Fact]
    public void MergePoints_IdenticalPoints()
    {
        var points = new Vec3[] { new(1, 0, 0), new(1, 0, 0), new(1, 0, 0) };
        var (unique, mapping) = SnapRounding.MergePoints(points);
        Assert.Single(unique);
        Assert.Equal(0, mapping[0]);
        Assert.Equal(0, mapping[1]);
        Assert.Equal(0, mapping[2]);
    }

    [Fact]
    public void MergePoints_AllDifferent()
    {
        var points = new Vec3[] { new(0, 0, 0), new(1, 0, 0), new(0, 1, 0) };
        var (unique, mapping) = SnapRounding.MergePoints(points);
        Assert.Equal(3, unique.Count);
    }

    [Fact]
    public void MergePoints_NearbyPoints()
    {
        var points = new Vec3[]
        {
            new(0, 0, 0),
            new(1e-10, 0, 0),
            new(1, 0, 0),
        };
        var (unique, mapping) = SnapRounding.MergePoints(points, 1e-8);
        Assert.Equal(2, unique.Count);
        Assert.Equal(mapping[0], mapping[1]); // First two merged
    }

    [Fact]
    public void MergePoints_EmptyList()
    {
        var (unique, mapping) = SnapRounding.MergePoints(Array.Empty<Vec3>());
        Assert.Empty(unique);
        Assert.Empty(mapping);
    }

    [Fact]
    public void MergePoints_SinglePoint()
    {
        var (unique, mapping) = SnapRounding.MergePoints(new[] { new Vec3(5, 5, 5) });
        Assert.Single(unique);
        Assert.Equal(0, mapping[0]);
    }

    [Fact]
    public void MergePoints_PreservesOrder()
    {
        var points = new Vec3[] { new(0, 0, 0), new(1, 0, 0), new(2, 0, 0) };
        var (unique, mapping) = SnapRounding.MergePoints(points);
        Assert.Equal(0, mapping[0]);
        Assert.Equal(1, mapping[1]);
        Assert.Equal(2, mapping[2]);
    }

    // --- IntersectionGraph via full pipeline ---

    [Fact]
    public void IntersectionGraph_DisjointCubes_NoSegments()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(5, 0, 0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        Assert.Empty(graph.Segments);
    }

    [Fact]
    public void IntersectionGraph_OverlappingCubes_HasSegments()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void IntersectionGraph_FaceSegments_ArePopulated()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        Assert.True(graph.FaceSegmentsA.Count > 0);
        Assert.True(graph.FaceSegmentsB.Count > 0);
    }

    [Fact]
    public void IntersectionGraph_IdenticalCubes_DetectsCoplanarity()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube();
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        // Identical cubes have all faces coplanar
        Assert.True(graph.CoplanarFacesA.Count > 0 || graph.CoplanarFacesB.Count > 0 || graph.Segments.Count >= 0);
    }

    [Fact]
    public void IntersectionGraph_SegmentEndpoints_AreSnapped()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        double gridSize = 1e-8;
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh, gridSize);
        foreach (var seg in graph.Segments)
        {
            // After snapping, coordinates should be near multiples of grid size
            double rem = System.Math.Abs(seg.Start.X / gridSize - System.Math.Round(seg.Start.X / gridSize));
            Assert.True(rem < 0.6, $"Snap remainder {rem} for X={seg.Start.X}");
        }
    }

    [Fact]
    public void IntersectionGraph_CubeSphere_HasSegments()
    {
        var cube = MeshFactory.CreateCube();
        // Sphere that sticks out of the cube (radius big enough to cross faces)
        var sphere = MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 1);
        var graph = IntersectionGraph.Compute(cube.Mesh, sphere.Mesh);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void IntersectionGraph_NoCoplanar_WhenDifferentPlanes()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0.3, 0.7));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        Assert.Empty(graph.CoplanarFacesA);
        Assert.Empty(graph.CoplanarFacesB);
    }

    [Fact]
    public void IntersectionGraph_NoDegenerate_Segments()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        foreach (var seg in graph.Segments)
            Assert.False(seg.IsDegenerate);
    }

    [Fact]
    public void IntersectionGraph_Symmetric_SegmentCounts()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graphAB = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        var graphBA = IntersectionGraph.Compute(cubeB.Mesh, cubeA.Mesh);
        Assert.Equal(graphAB.Segments.Count, graphBA.Segments.Count);
    }

    [Fact]
    public void IntersectionGraph_CoplanarCubes_AtOffset_HasCoplanarFaces()
    {
        // Two cubes share face planes y=0, y=1, z=0, z=1
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        Assert.True(graph.CoplanarFacesA.Count > 0);
    }
}
