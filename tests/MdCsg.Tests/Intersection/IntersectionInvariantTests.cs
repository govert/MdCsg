using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Intersection;

/// <summary>Phase 6: IntersectionGraph and TriTriIntersection invariants</summary>
public class IntersectionInvariantTests
{
    [Fact]
    public void IntersectionGraph_OverlappingCubes_HasSegments()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.True(graph.Segments.Count > 0);
        Assert.True(graph.FaceSegmentsA.Count > 0);
        Assert.True(graph.FaceSegmentsB.Count > 0);
    }

    [Fact]
    public void IntersectionGraph_DisjointCubes_NoSegments()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        Assert.Empty(graph.Segments);
        Assert.Empty(graph.FaceSegmentsA);
        Assert.Empty(graph.FaceSegmentsB);
    }

    [Fact]
    public void IntersectionGraph_Segments_HaveValidFaceIndices()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        foreach (var seg in graph.Segments)
        {
            Assert.True(seg.FaceIndexA >= 0 && seg.FaceIndexA < meshA.Faces.Count,
                $"FaceIndexA {seg.FaceIndexA} out of range");
            Assert.True(seg.FaceIndexB >= 0 && seg.FaceIndexB < meshB.Faces.Count,
                $"FaceIndexB {seg.FaceIndexB} out of range");
        }
    }

    [Fact]
    public void IntersectionGraph_Segments_NonDegenerate()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        foreach (var seg in graph.Segments)
        {
            Assert.False(seg.IsDegenerate, $"Degenerate segment: {seg.Start} → {seg.End}");
        }
    }

    [Fact]
    public void IntersectionGraph_Segments_NoNaN()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh;
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
    public void IntersectionGraph_FaceSegments_MatchSegments()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);

        int totalFromA = 0;
        foreach (var kvp in graph.FaceSegmentsA)
            totalFromA += kvp.Value.Count;

        int totalFromB = 0;
        foreach (var kvp in graph.FaceSegmentsB)
            totalFromB += kvp.Value.Count;

        // Each segment appears in both A and B face maps
        Assert.Equal(graph.Segments.Count, totalFromA);
        Assert.Equal(graph.Segments.Count, totalFromB);
    }

    [Fact]
    public void IntersectionGraph_CustomGridSize()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB, 1e-6);
        Assert.True(graph.Segments.Count > 0);
    }

    [Fact]
    public void TriTriIntersection_SegmentOnBothTriangles()
    {
        // Verify intersection segment lies on both triangles' planes
        var t1 = new Triangle3(
            new Vec3(-2, -2, 0), new Vec3(2, -2, 0), new Vec3(0, 2, 0));
        var t2 = new Triangle3(
            new Vec3(0, 0, -2), new Vec3(0, 0, 2), new Vec3(0, 2, 0));
        Assert.True(TriTriIntersection.Intersect(t1, t2, out var seg));
        // Start and End should be on t1's plane (Z=0) approximately
        Assert.True(System.Math.Abs(seg.Start.Z) < 0.01);
        Assert.True(System.Math.Abs(seg.End.Z) < 0.01);
        // Start and End should be on t2's plane (X=0) approximately
        Assert.True(System.Math.Abs(seg.Start.X) < 0.01);
        Assert.True(System.Math.Abs(seg.End.X) < 0.01);
    }

    [Fact]
    public void TriTriIntersection_Commutative()
    {
        var t1 = new Triangle3(
            new Vec3(-1, -1, 0), new Vec3(1, -1, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(
            new Vec3(0, 0, -1), new Vec3(0, 0, 1), new Vec3(0, 2, 0));
        bool r1 = TriTriIntersection.Intersect(t1, t2, out var seg1);
        bool r2 = TriTriIntersection.Intersect(t2, t1, out var seg2);
        Assert.Equal(r1, r2);
        if (r1)
        {
            Assert.True(System.Math.Abs(seg1.Length - seg2.Length) < 0.01,
                $"Lengths differ: {seg1.Length} vs {seg2.Length}");
        }
    }

    [Fact]
    public void IntersectionSegment_Properties()
    {
        var seg = new IntersectionSegment(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), 3, 7);
        Assert.Equal(1.0, seg.Length, 10);
        Assert.False(seg.IsDegenerate);
        Assert.Equal(3, seg.FaceIndexA);
        Assert.Equal(7, seg.FaceIndexB);
    }

    [Fact]
    public void IntersectionSegment_Degenerate_BelowEpsilon()
    {
        double eps = MathUtil.Epsilon * 0.1;
        var seg = new IntersectionSegment(
            new Vec3(0, 0, 0), new Vec3(eps, 0, 0), 0, 0);
        Assert.True(seg.IsDegenerate);
    }

    [Fact]
    public void SnapRounding_PreservesNonTinyValues()
    {
        var v = new Vec3(1.5, 2.5, 3.5);
        var snapped = SnapRounding.Snap(v, 1e-8);
        Assert.True(Vec3.Distance(v, snapped) < 1e-6);
    }

    [Fact]
    public void SnapRounding_SnapSegment()
    {
        var seg = new IntersectionSegment(
            new Vec3(1.000000001, 2.000000001, 3.000000001),
            new Vec3(4.000000001, 5.000000001, 6.000000001), 0, 0);
        var snapped = SnapRounding.SnapSegment(seg, 1e-8);
        Assert.True(Vec3.Distance(snapped.Start, new Vec3(1, 2, 3)) < 1e-6);
        Assert.True(Vec3.Distance(snapped.End, new Vec3(4, 5, 6)) < 1e-6);
    }

    [Fact]
    public void SnapRounding_MergePoints_NearbyPointsMerge()
    {
        var points = new List<Vec3>
        {
            new(0, 0, 0),
            new(1e-9, 1e-9, 1e-9),
            new(1, 1, 1),
            new(1 + 1e-9, 1 + 1e-9, 1 + 1e-9)
        };
        var (uniquePoints, mapping) = SnapRounding.MergePoints(points, 1e-8);
        Assert.Equal(2, uniquePoints.Count);
        Assert.Equal(4, mapping.Count);
    }
}
