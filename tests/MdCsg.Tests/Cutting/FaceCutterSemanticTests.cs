using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;

namespace MdCsg.Tests.Cutting;

/// <summary>Semantic paths: FaceCutter edge cases and SubTriangle properties</summary>
public class FaceCutterSemanticTests
{
    [Fact]
    public void CutFace_NoSegments_ReturnsSingleTriangle()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var result = FaceCutter.CutFace(tri, 42, new List<IntersectionSegment>());
        Assert.Single(result);
        Assert.Equal(42, result[0].OriginalFaceIndex);
        Assert.False(result[0].HasIntersectionEdge);
    }

    [Fact]
    public void CutFace_SingleSegment_ProducesMultipleSubTriangles()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(4, 0, 0), new Vec3(0, 4, 0));
        var seg = new IntersectionSegment(new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, 0);
        var result = FaceCutter.CutFace(tri, 0, new[] { seg });
        Assert.True(result.Count >= 2, $"Expected >= 2 sub-triangles, got {result.Count}");
    }

    [Fact]
    public void CutFace_SegmentAtCorner_HandlesGracefully()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(0, 2, 0));
        // Segment from vertex A to midpoint of BC
        var seg = new IntersectionSegment(new Vec3(0, 0, 0), new Vec3(1, 1, 0), 0, 0);
        var result = FaceCutter.CutFace(tri, 0, new[] { seg });
        Assert.True(result.Count >= 2);
    }

    [Fact]
    public void CutFace_DegenerateSegment_SamePoints_Skipped()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(4, 0, 0), new Vec3(0, 4, 0));
        // Degenerate segment (same start and end within tolerance)
        var seg = new IntersectionSegment(new Vec3(1, 1, 0), new Vec3(1, 1, 0), 0, 0);
        var result = FaceCutter.CutFace(tri, 0, new[] { seg });
        // Should still produce valid output (the coincident vertices merge)
        Assert.True(result.Count >= 1);
    }

    [Fact]
    public void SubTriangle_IsEdgeIntersection_Bits()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, true, 0b101);
        Assert.True(st.IsEdgeIntersection(0));   // bit 0 set (A-B)
        Assert.False(st.IsEdgeIntersection(1));   // bit 1 not set (B-C)
        Assert.True(st.IsEdgeIntersection(2));    // bit 2 set (C-A)
    }

    [Fact]
    public void SubTriangle_NoIntersectionEdge()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 5, false, 0);
        Assert.False(st.HasIntersectionEdge);
        Assert.False(st.IsEdgeIntersection(0));
        Assert.False(st.IsEdgeIntersection(1));
        Assert.False(st.IsEdgeIntersection(2));
        Assert.Equal(5, st.OriginalFaceIndex);
    }

    [Fact]
    public void CutFace_TwoParallelSegments_ProducesStrips()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(6, 0, 0), new Vec3(0, 6, 0));
        var seg1 = new IntersectionSegment(new Vec3(2, 0, 0), new Vec3(0, 2, 0), 0, 0);
        var seg2 = new IntersectionSegment(new Vec3(4, 0, 0), new Vec3(0, 4, 0), 0, 0);
        var result = FaceCutter.CutFace(tri, 0, new[] { seg1, seg2 });
        Assert.True(result.Count >= 3, $"Expected >= 3, got {result.Count}");
    }

    [Fact]
    public void CutFace_SegmentAlongEdge_HandlesGracefully()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(4, 0, 0), new Vec3(0, 4, 0));
        // Segment along edge A-B
        var seg = new IntersectionSegment(new Vec3(1, 0, 0), new Vec3(3, 0, 0), 0, 0);
        var result = FaceCutter.CutFace(tri, 0, new[] { seg });
        Assert.True(result.Count >= 1);
    }

    [Fact]
    public void CutFace_IntersectionEdge_Flagged()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(4, 0, 0), new Vec3(0, 4, 0));
        var seg = new IntersectionSegment(new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, 0);
        var result = FaceCutter.CutFace(tri, 0, new[] { seg });

        bool anyFlagged = result.Any(st => st.HasIntersectionEdge);
        Assert.True(anyFlagged, "At least one sub-triangle should have an intersection edge");
    }
}
