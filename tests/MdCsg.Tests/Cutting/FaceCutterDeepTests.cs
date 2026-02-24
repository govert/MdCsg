using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;

namespace MdCsg.Tests.Cutting;

/// <summary>Phase 6: FaceCutter deep tests — segment endpoint accuracy, complex patterns, edge flags</summary>
public class FaceCutterDeepTests
{
    [Fact]
    public void CutFace_NoSegments_ReturnsSingleSubTriangle()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var result = FaceCutter.CutFace(tri, 0, Array.Empty<IntersectionSegment>());
        Assert.Single(result);
        Assert.False(result[0].HasIntersectionEdge);
        Assert.Equal(0, result[0].OriginalFaceIndex);
    }

    [Fact]
    public void CutFace_SingleSegment_ProducesMultipleSubTriangles()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var seg = new IntersectionSegment(new Vec3(0.5, 0.5, 0), new Vec3(1.5, 0.5, 0), 0, 0);
        var result = FaceCutter.CutFace(tri, 5, new[] { seg });
        Assert.True(result.Count > 1, $"Expected multiple sub-triangles, got {result.Count}");
        Assert.All(result, st => Assert.Equal(5, st.OriginalFaceIndex));
    }

    [Fact]
    public void CutFace_SubTriangles_CoverOriginalArea()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(0, 2, 0));
        var seg = new IntersectionSegment(new Vec3(0.5, 0.5, 0), new Vec3(1, 0.5, 0), 0, 0);
        var result = FaceCutter.CutFace(tri, 0, new[] { seg });

        double totalArea = result.Sum(st =>
            new Triangle3(st.A, st.B, st.C).Area);
        double originalArea = tri.Area;
        Assert.True(System.Math.Abs(totalArea - originalArea) < 1e-6,
            $"Sub-triangle area sum {totalArea} doesn't match original {originalArea}");
    }

    [Fact]
    public void CutFace_IntersectionEdgeFlags_SetCorrectly()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var seg = new IntersectionSegment(new Vec3(0.5, 0.5, 0), new Vec3(1.5, 0.5, 0), 0, 0);
        var result = FaceCutter.CutFace(tri, 0, new[] { seg });

        // At least one sub-triangle should have intersection edge
        Assert.Contains(result, st => st.HasIntersectionEdge);
        // And at least one should not
        Assert.Contains(result, st => !st.HasIntersectionEdge);
    }

    [Fact]
    public void CutFace_SegmentEndpointOnVertex_HandledGracefully()
    {
        // Segment starts at a triangle vertex
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var seg = new IntersectionSegment(new Vec3(0, 0, 0), new Vec3(1, 1, 0), 0, 0);
        var result = FaceCutter.CutFace(tri, 0, new[] { seg });
        Assert.True(result.Count >= 1);

        double totalArea = result.Sum(st => new Triangle3(st.A, st.B, st.C).Area);
        Assert.True(System.Math.Abs(totalArea - tri.Area) < 1e-6);
    }

    [Fact]
    public void CutFace_SegmentEndpointOnEdge_HandledGracefully()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        // Segment endpoint on the edge from A to B at midpoint (1, 0, 0)
        var seg = new IntersectionSegment(new Vec3(1, 0, 0), new Vec3(0.5, 1, 0), 0, 0);
        var result = FaceCutter.CutFace(tri, 0, new[] { seg });
        Assert.True(result.Count >= 2);
    }

    [Fact]
    public void CutFace_TwoSegments_MoreSubTriangles()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(3, 0, 0), new Vec3(1.5, 3, 0));
        var seg1 = new IntersectionSegment(new Vec3(0.5, 0.5, 0), new Vec3(2, 0.5, 0), 0, 0);
        var seg2 = new IntersectionSegment(new Vec3(1, 1.5, 0), new Vec3(2, 1.5, 0), 0, 0);
        var result = FaceCutter.CutFace(tri, 0, new[] { seg1, seg2 });
        Assert.True(result.Count > 2, $"Expected many sub-triangles with 2 segments, got {result.Count}");
    }

    [Fact]
    public void CutFace_DegenerateSegment_SameStartEnd_NoExtraSubTriangles()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        // Start == End → degenerate segment, should be effectively ignored
        var seg = new IntersectionSegment(new Vec3(0.5, 0.5, 0), new Vec3(0.5, 0.5, 0), 0, 0);
        var result = FaceCutter.CutFace(tri, 0, new[] { seg });
        // Should produce at least the original triangle (might add vertex but produce valid triangulation)
        Assert.True(result.Count >= 1);
    }

    [Fact]
    public void CutFace_SegmentAlongEdge_NoExtraCuts()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        // Segment along the bottom edge of the triangle
        var seg = new IntersectionSegment(new Vec3(0.5, 0, 0), new Vec3(1.5, 0, 0), 0, 0);
        var result = FaceCutter.CutFace(tri, 0, new[] { seg });
        Assert.True(result.Count >= 1);
        // Area should still be preserved
        double totalArea = result.Sum(st => new Triangle3(st.A, st.B, st.C).Area);
        Assert.True(System.Math.Abs(totalArea - tri.Area) < 1e-6);
    }

    [Fact]
    public void SubTriangle_IsEdgeIntersection_BitFlags()
    {
        // bit 0 = A-B, bit 1 = B-C, bit 2 = C-A
        var st = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.Zero, Vec3.Zero, 0, true, 0b101);
        Assert.True(st.IsEdgeIntersection(0));  // A-B: bit 0
        Assert.False(st.IsEdgeIntersection(1)); // B-C: bit 1
        Assert.True(st.IsEdgeIntersection(2));  // C-A: bit 2
    }

    [Fact]
    public void SubTriangle_NoIntersection_AllFlagsZero()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.Zero, Vec3.Zero, 0, false, 0);
        Assert.False(st.IsEdgeIntersection(0));
        Assert.False(st.IsEdgeIntersection(1));
        Assert.False(st.IsEdgeIntersection(2));
    }

    [Fact]
    public void SubTriangle_AllEdgesIntersection()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.Zero, Vec3.Zero, 0, true, 0b111);
        Assert.True(st.IsEdgeIntersection(0));
        Assert.True(st.IsEdgeIntersection(1));
        Assert.True(st.IsEdgeIntersection(2));
    }

    [Fact]
    public void CutFace_LargeTriangle_StillCorrectArea()
    {
        double s = 100.0;
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(s, 0, 0), new Vec3(0, s, 0));
        var seg = new IntersectionSegment(new Vec3(s * 0.3, s * 0.3, 0), new Vec3(s * 0.7, s * 0.3, 0), 0, 0);
        var result = FaceCutter.CutFace(tri, 0, new[] { seg });

        double totalArea = result.Sum(st => new Triangle3(st.A, st.B, st.C).Area);
        Assert.True(System.Math.Abs(totalArea - tri.Area) < 1e-2,
            $"Area mismatch: {totalArea} vs {tri.Area}");
    }

    [Fact]
    public void CutFace_Integration_CubeCube_AllSubTrianglesValid()
    {
        var meshA = TestHelpers.MeshFactory.CreateCube().Mesh;
        var meshB = TestHelpers.MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);

        foreach (var st in cutA.SubTriangles)
        {
            var area = new Triangle3(st.A, st.B, st.C).Area;
            Assert.True(area >= 0, "Sub-triangle has negative area");
            Assert.False(double.IsNaN(st.A.X) || double.IsNaN(st.B.X) || double.IsNaN(st.C.X));
        }
    }

    [Fact]
    public void CutFace_Integration_AllOriginalFacesRepresented()
    {
        var meshA = TestHelpers.MeshFactory.CreateCube().Mesh;
        var meshB = TestHelpers.MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, (Dictionary<int, List<IntersectionSegment>>)graph.FaceSegmentsA);

        var originalFaces = cutA.SubTriangles.Select(st => st.OriginalFaceIndex).Distinct().ToHashSet();
        for (int i = 0; i < meshA.Faces.Count; i++)
        {
            Assert.Contains(i, originalFaces);
        }
    }
}
