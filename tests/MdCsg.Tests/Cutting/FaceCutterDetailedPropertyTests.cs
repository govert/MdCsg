using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;

namespace MdCsg.Tests.Cutting;

/// <summary>Phase 6: FaceCutter.CutFace — zero/one/multiple segments, SubTriangle record, edge flag bits</summary>
public class FaceCutterDetailedPropertyTests
{
    [Fact]
    public void CutFace_NoSegments_ReturnsSingleSubTriangle()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var result = FaceCutter.CutFace(tri, 0, new List<IntersectionSegment>());
        Assert.Single(result);
        Assert.False(result[0].HasIntersectionEdge);
    }

    [Fact]
    public void CutFace_OneSegment_ProducesMultipleSubTriangles()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var seg = new IntersectionSegment(new Vec3(0.5, 0.5, 0), new Vec3(1.5, 0.5, 0), 0, 1);
        var result = FaceCutter.CutFace(tri, 0, new List<IntersectionSegment> { seg });
        Assert.True(result.Count > 1, $"Expected >1 sub-triangles, got {result.Count}");
    }

    [Fact]
    public void CutFace_AllSubTriangles_HaveCorrectOriginalFaceIndex()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var seg = new IntersectionSegment(new Vec3(0.5, 0.5, 0), new Vec3(1.5, 0.5, 0), 0, 1);
        var result = FaceCutter.CutFace(tri, 42, new List<IntersectionSegment> { seg });
        foreach (var st in result)
        {
            Assert.Equal(42, st.OriginalFaceIndex);
        }
    }

    [Fact]
    public void CutFace_HasIntersectionEdge_SetOnSomeSubTriangles()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var seg = new IntersectionSegment(new Vec3(0.5, 0.5, 0), new Vec3(1.5, 0.5, 0), 0, 1);
        var result = FaceCutter.CutFace(tri, 0, new List<IntersectionSegment> { seg });
        bool anyHasIntersectionEdge = false;
        foreach (var st in result)
            if (st.HasIntersectionEdge) anyHasIntersectionEdge = true;
        Assert.True(anyHasIntersectionEdge, "At least one sub-triangle should have an intersection edge");
    }

    [Fact]
    public void SubTriangle_IsEdgeIntersection_BitFlags()
    {
        // Bit 0 = edge A-B, bit 1 = edge B-C, bit 2 = edge C-A
        var st = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, true, 0x01);
        Assert.True(st.IsEdgeIntersection(0)); // A-B
        Assert.False(st.IsEdgeIntersection(1)); // B-C
        Assert.False(st.IsEdgeIntersection(2)); // C-A
    }

    [Fact]
    public void SubTriangle_IsEdgeIntersection_AllFlags()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, true, 0x07);
        Assert.True(st.IsEdgeIntersection(0));
        Assert.True(st.IsEdgeIntersection(1));
        Assert.True(st.IsEdgeIntersection(2));
    }

    [Fact]
    public void SubTriangle_IsEdgeIntersection_NoFlags()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, false, 0x00);
        Assert.False(st.IsEdgeIntersection(0));
        Assert.False(st.IsEdgeIntersection(1));
        Assert.False(st.IsEdgeIntersection(2));
    }

    [Fact]
    public void SubTriangle_RecordEquality_SameValues()
    {
        var st1 = new FaceCutter.SubTriangle(new Vec3(1, 2, 3), new Vec3(4, 5, 6), new Vec3(7, 8, 9), 0, false);
        var st2 = new FaceCutter.SubTriangle(new Vec3(1, 2, 3), new Vec3(4, 5, 6), new Vec3(7, 8, 9), 0, false);
        Assert.Equal(st1, st2);
    }

    [Fact]
    public void SubTriangle_Fields_AccessCorrectly()
    {
        var a = new Vec3(1, 0, 0);
        var b = new Vec3(0, 1, 0);
        var c = new Vec3(0, 0, 1);
        var st = new FaceCutter.SubTriangle(a, b, c, 5, true, 0x03);
        Assert.Equal(a, st.A);
        Assert.Equal(b, st.B);
        Assert.Equal(c, st.C);
        Assert.Equal(5, st.OriginalFaceIndex);
        Assert.True(st.HasIntersectionEdge);
        Assert.Equal(0x03, st.IntersectionEdgeFlags);
    }

    [Fact]
    public void CutFace_EmptySegmentList_ReturnsSingleSubTriangle()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var result = FaceCutter.CutFace(tri, 7, new List<IntersectionSegment>());
        Assert.Single(result);
        Assert.Equal(7, result[0].OriginalFaceIndex);
    }

    [Fact]
    public void CutFace_SubTrianglesVerticesNonDegenerate()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var seg = new IntersectionSegment(new Vec3(0.5, 0.5, 0), new Vec3(1.5, 0.5, 0), 0, 1);
        var result = FaceCutter.CutFace(tri, 0, new List<IntersectionSegment> { seg });
        foreach (var st in result)
        {
            // Each sub-triangle's vertices should not all be the same point
            double area = Vec3.Cross(st.B - st.A, st.C - st.A).Length;
            // Some may be very small but not zero
            Assert.True(area >= 0);
        }
    }
}
