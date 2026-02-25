using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;

namespace MdCsg.Tests.Cutting;

/// <summary>Phase 6: FaceCutter SubTriangle edge flags — IsEdgeIntersection, IntersectionEdgeFlags bitfield</summary>
public class FaceCutterEdgeFlagTests
{
    [Fact]
    public void NoSegments_SingleSubTriangle_NoFlags()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var result = FaceCutter.CutFace(tri, 0, Array.Empty<IntersectionSegment>());
        Assert.Single(result);
        Assert.False(result[0].HasIntersectionEdge);
        Assert.Equal(0, result[0].IntersectionEdgeFlags);
    }

    [Fact]
    public void NoSegments_PreservesOriginalVertices()
    {
        var tri = new Triangle3(new Vec3(1, 2, 3), new Vec3(4, 5, 6), new Vec3(7, 8, 9));
        var result = FaceCutter.CutFace(tri, 5, Array.Empty<IntersectionSegment>());
        Assert.Single(result);
        Assert.Equal(tri.A, result[0].A);
        Assert.Equal(tri.B, result[0].B);
        Assert.Equal(tri.C, result[0].C);
        Assert.Equal(5, result[0].OriginalFaceIndex);
    }

    [Fact]
    public void WithSegment_ProducesMultipleSubTriangles()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var seg = new IntersectionSegment(new Vec3(0.5, 0.5, 0), new Vec3(1.5, 0.5, 0), 0, 1);
        var result = FaceCutter.CutFace(tri, 0, new[] { seg });
        Assert.True(result.Count >= 3, $"Expected >= 3 sub-triangles, got {result.Count}");
    }

    [Fact]
    public void WithSegment_SomeHaveIntersectionEdge()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var seg = new IntersectionSegment(new Vec3(0.5, 0.5, 0), new Vec3(1.5, 0.5, 0), 0, 1);
        var result = FaceCutter.CutFace(tri, 0, new[] { seg });
        Assert.True(result.Any(st => st.HasIntersectionEdge), "Should have at least one intersection edge");
    }

    [Fact]
    public void WithSegment_AllOriginalFaceIndex_SameAs_Input()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var seg = new IntersectionSegment(new Vec3(0.5, 0.5, 0), new Vec3(1.5, 0.5, 0), 7, 3);
        var result = FaceCutter.CutFace(tri, 7, new[] { seg });
        foreach (var st in result)
            Assert.Equal(7, st.OriginalFaceIndex);
    }

    [Fact]
    public void SubTriangle_IsEdgeIntersection_Bit0_AB()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, true, 0b001);
        Assert.True(st.IsEdgeIntersection(0)); // A-B
        Assert.False(st.IsEdgeIntersection(1)); // B-C
        Assert.False(st.IsEdgeIntersection(2)); // C-A
    }

    [Fact]
    public void SubTriangle_IsEdgeIntersection_Bit1_BC()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, true, 0b010);
        Assert.False(st.IsEdgeIntersection(0));
        Assert.True(st.IsEdgeIntersection(1));
        Assert.False(st.IsEdgeIntersection(2));
    }

    [Fact]
    public void SubTriangle_IsEdgeIntersection_Bit2_CA()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, true, 0b100);
        Assert.False(st.IsEdgeIntersection(0));
        Assert.False(st.IsEdgeIntersection(1));
        Assert.True(st.IsEdgeIntersection(2));
    }

    [Fact]
    public void SubTriangle_IsEdgeIntersection_AllBits()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, true, 0b111);
        Assert.True(st.IsEdgeIntersection(0));
        Assert.True(st.IsEdgeIntersection(1));
        Assert.True(st.IsEdgeIntersection(2));
    }

    [Fact]
    public void SubTriangle_IsEdgeIntersection_NoBits()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false, 0);
        Assert.False(st.IsEdgeIntersection(0));
        Assert.False(st.IsEdgeIntersection(1));
        Assert.False(st.IsEdgeIntersection(2));
    }

    [Fact]
    public void SubTriangle_HasIntersectionEdge_WhenFlagsNonzero()
    {
        var st1 = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, true, 0b010);
        Assert.True(st1.HasIntersectionEdge);
        var st2 = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false, 0);
        Assert.False(st2.HasIntersectionEdge);
    }

    [Fact]
    public void CutFace_EmptySegmentList_SingleSubTri()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var result = FaceCutter.CutFace(tri, 3, new List<IntersectionSegment>());
        Assert.Single(result);
        Assert.Equal(3, result[0].OriginalFaceIndex);
    }

    [Fact]
    public void SubTriangle_Equality_SameValues()
    {
        var a = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, true, 3);
        var b = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, true, 3);
        Assert.Equal(a, b);
    }

    [Fact]
    public void SubTriangle_Inequality_DifferentFlags()
    {
        var a = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, true, 1);
        var b = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, true, 2);
        Assert.NotEqual(a, b);
    }
}
