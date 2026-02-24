using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;

namespace MdCsg.Tests.Cutting;

/// <summary>Phase 6: FaceCutter edge cases and SubTriangle properties</summary>
public class FaceCutterEdgeCaseTests
{
    [Fact]
    public void CutFace_NoSegments_SingleSubTriangle()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var segments = new List<IntersectionSegment>();
        var result = FaceCutter.CutFace(tri, 0, segments);
        Assert.Single(result);
        Assert.False(result[0].HasIntersectionEdge);
    }

    [Fact]
    public void CutFace_NoSegments_PreservesOriginalVertices()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var result = FaceCutter.CutFace(tri, 0, new List<IntersectionSegment>());
        Assert.Equal(new Vec3(0, 0, 0), result[0].A);
        Assert.Equal(new Vec3(1, 0, 0), result[0].B);
        Assert.Equal(new Vec3(0, 1, 0), result[0].C);
    }

    [Fact]
    public void CutFace_SingleSegment_ProducesMultipleSubTriangles()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        // Segment cuts across the triangle
        var seg = new IntersectionSegment(new Vec3(0.5, 0, 0), new Vec3(0, 0.5, 0), 0, 1);
        var result = FaceCutter.CutFace(tri, 0, new List<IntersectionSegment> { seg });
        Assert.True(result.Count >= 2, $"Expected >= 2 sub-triangles, got {result.Count}");
    }

    [Fact]
    public void CutFace_SingleSegment_PreservesFaceIndex()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var seg = new IntersectionSegment(new Vec3(0.5, 0, 0), new Vec3(0, 0.5, 0), 0, 1);
        var result = FaceCutter.CutFace(tri, 42, new List<IntersectionSegment> { seg });
        foreach (var sub in result)
            Assert.Equal(42, sub.OriginalFaceIndex);
    }

    [Fact]
    public void CutFace_SingleSegment_SomeHaveIntersectionEdge()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var seg = new IntersectionSegment(new Vec3(0.5, 0, 0), new Vec3(0, 0.5, 0), 0, 1);
        var result = FaceCutter.CutFace(tri, 0, new List<IntersectionSegment> { seg });
        bool anyWithEdge = result.Any(s => s.HasIntersectionEdge);
        Assert.True(anyWithEdge, "At least one sub-triangle should have an intersection edge");
    }

    [Fact]
    public void CutFace_SegmentAtVertex_HandleGracefully()
    {
        // Segment starts at a vertex of the triangle
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var seg = new IntersectionSegment(new Vec3(0, 0, 0), new Vec3(0.5, 0.5, 0), 0, 1);
        var result = FaceCutter.CutFace(tri, 0, new List<IntersectionSegment> { seg });
        Assert.True(result.Count >= 1);
    }

    [Fact]
    public void CutFace_SegmentAlongEdge_HandleGracefully()
    {
        // Segment along edge A-B
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var seg = new IntersectionSegment(new Vec3(0.2, 0, 0), new Vec3(0.8, 0, 0), 0, 1);
        var result = FaceCutter.CutFace(tri, 0, new List<IntersectionSegment> { seg });
        Assert.True(result.Count >= 1);
    }

    [Fact]
    public void CutFace_TwoSegments_MoreSubTriangles()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(0, 2, 0));
        var seg1 = new IntersectionSegment(new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, 1);
        var seg2 = new IntersectionSegment(new Vec3(0.5, 0, 0), new Vec3(0, 0.5, 0), 0, 2);
        var result = FaceCutter.CutFace(tri, 0, new List<IntersectionSegment> { seg1, seg2 });
        Assert.True(result.Count >= 3, $"Expected >= 3, got {result.Count}");
    }

    [Fact]
    public void CutFace_AllSubTrianglesHavePositiveArea()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var seg = new IntersectionSegment(new Vec3(0.5, 0, 0), new Vec3(0, 0.5, 0), 0, 1);
        var result = FaceCutter.CutFace(tri, 0, new List<IntersectionSegment> { seg });
        foreach (var sub in result)
        {
            var subTri = new Triangle3(sub.A, sub.B, sub.C);
            Assert.True(subTri.Area > 0, "Sub-triangle should have positive area");
        }
    }

    [Fact]
    public void SubTriangle_IsEdgeIntersection_Flags()
    {
        // Edge flags: bit 0 = A-B, bit 1 = B-C, bit 2 = C-A
        var sub = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, true, 0b101);
        Assert.True(sub.IsEdgeIntersection(0));  // A-B
        Assert.False(sub.IsEdgeIntersection(1)); // B-C
        Assert.True(sub.IsEdgeIntersection(2));  // C-A
    }

    [Fact]
    public void SubTriangle_NoEdgeFlags_AllFalse()
    {
        var sub = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, false, 0);
        Assert.False(sub.IsEdgeIntersection(0));
        Assert.False(sub.IsEdgeIntersection(1));
        Assert.False(sub.IsEdgeIntersection(2));
    }

    [Fact]
    public void SubTriangle_AllEdgeFlags_AllTrue()
    {
        var sub = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, true, 0b111);
        Assert.True(sub.IsEdgeIntersection(0));
        Assert.True(sub.IsEdgeIntersection(1));
        Assert.True(sub.IsEdgeIntersection(2));
    }

    [Fact]
    public void CutFace_DuplicateSegmentEndpoints_Deduplication()
    {
        // Segment with endpoints near existing triangle vertices
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        // Start/end very close to existing vertices — should be merged
        var seg = new IntersectionSegment(
            new Vec3(1e-12, 0, 0),  // nearly vertex A
            new Vec3(0.5, 0, 0),
            0, 1);
        var result = FaceCutter.CutFace(tri, 0, new List<IntersectionSegment> { seg });
        Assert.True(result.Count >= 1);
    }

    [Fact]
    public void CutFace_DegenerateSegment_NoConstraintAdded()
    {
        // Zero-length segment (both endpoints same) — no constraint pair is added
        // but the point may be inserted as a vertex, causing re-triangulation
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var seg = new IntersectionSegment(new Vec3(0.5, 0.2, 0), new Vec3(0.5, 0.2, 0), 0, 1);
        var result = FaceCutter.CutFace(tri, 0, new List<IntersectionSegment> { seg });
        // No constraint edge, so none should have intersection edge flags
        Assert.True(result.All(s => !s.HasIntersectionEdge));
    }
}
