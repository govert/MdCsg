using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;

namespace MdCsg.Tests.Cutting;

/// <summary>Phase 6: FaceCutter — SubTriangle record, CutFace with/without segments, edge flag properties</summary>
public class FaceCutterSubTrianglePropertyTests
{
    [Fact]
    public void SubTriangle_NoIntersection_NoFlags()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, false);
        Assert.False(st.HasIntersectionEdge);
        Assert.Equal(0, st.IntersectionEdgeFlags);
    }

    [Fact]
    public void SubTriangle_WithFlags_HasIntersectionEdge()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, true, 0b001);
        Assert.True(st.HasIntersectionEdge);
        Assert.True(st.IsEdgeIntersection(0)); // A-B
        Assert.False(st.IsEdgeIntersection(1)); // B-C
        Assert.False(st.IsEdgeIntersection(2)); // C-A
    }

    [Fact]
    public void SubTriangle_AllFlags()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, true, 0b111);
        Assert.True(st.IsEdgeIntersection(0));
        Assert.True(st.IsEdgeIntersection(1));
        Assert.True(st.IsEdgeIntersection(2));
    }

    [Fact]
    public void SubTriangle_OriginalFaceIndex_Preserved()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 42, false);
        Assert.Equal(42, st.OriginalFaceIndex);
    }

    [Fact]
    public void SubTriangle_Vertices_Preserved()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var c = new Vec3(7, 8, 9);
        var st = new FaceCutter.SubTriangle(a, b, c, 0, false);
        Assert.Equal(a, st.A);
        Assert.Equal(b, st.B);
        Assert.Equal(c, st.C);
    }

    [Fact]
    public void CutFace_NoSegments_SingleSubTriangle()
    {
        var tri = new Triangle3(Vec3.Zero, new Vec3(2, 0, 0), new Vec3(0, 2, 0));
        var result = FaceCutter.CutFace(tri, 0, Array.Empty<IntersectionSegment>());
        Assert.Single(result);
        Assert.False(result[0].HasIntersectionEdge);
    }

    [Fact]
    public void CutFace_NoSegments_PreservesVertices()
    {
        var a = Vec3.Zero;
        var b = new Vec3(2, 0, 0);
        var c = new Vec3(0, 2, 0);
        var tri = new Triangle3(a, b, c);
        var result = FaceCutter.CutFace(tri, 5, Array.Empty<IntersectionSegment>());
        Assert.Single(result);
        Assert.Equal(5, result[0].OriginalFaceIndex);
    }

    [Fact]
    public void CutFace_OneSegment_ProducesMultipleSubTriangles()
    {
        var tri = new Triangle3(Vec3.Zero, new Vec3(4, 0, 0), new Vec3(0, 4, 0));
        var seg = new IntersectionSegment(
            new Vec3(2, 0, 0), new Vec3(0, 2, 0), 0, 1);
        var result = FaceCutter.CutFace(tri, 0, new[] { seg });
        Assert.True(result.Count >= 2, $"Cutting should produce at least 2 sub-triangles, got {result.Count}");
    }

    [Fact]
    public void CutFace_OneSegment_SomeHaveIntersectionEdge()
    {
        var tri = new Triangle3(Vec3.Zero, new Vec3(4, 0, 0), new Vec3(0, 4, 0));
        var seg = new IntersectionSegment(
            new Vec3(2, 0, 0), new Vec3(0, 2, 0), 0, 1);
        var result = FaceCutter.CutFace(tri, 0, new[] { seg });
        bool anyIntersection = result.Any(st => st.HasIntersectionEdge);
        Assert.True(anyIntersection, "Some sub-triangles should have intersection edges");
    }

    [Fact]
    public void CutFace_AllSubTriangles_SameOriginalFace()
    {
        var tri = new Triangle3(Vec3.Zero, new Vec3(4, 0, 0), new Vec3(0, 4, 0));
        var seg = new IntersectionSegment(
            new Vec3(2, 0, 0), new Vec3(0, 2, 0), 0, 1);
        var result = FaceCutter.CutFace(tri, 7, new[] { seg });
        foreach (var st in result)
            Assert.Equal(7, st.OriginalFaceIndex);
    }

    [Fact]
    public void CutFace_SegmentEndpointOnVertex_HandlesGracefully()
    {
        var tri = new Triangle3(Vec3.Zero, new Vec3(4, 0, 0), new Vec3(0, 4, 0));
        // Segment from vertex A to midpoint of BC
        var seg = new IntersectionSegment(
            Vec3.Zero, new Vec3(2, 2, 0), 0, 1);
        var result = FaceCutter.CutFace(tri, 0, new[] { seg });
        Assert.True(result.Count >= 2);
    }

    [Fact]
    public void CutFace_EmptySegmentList_SingleSubTriangle()
    {
        var tri = new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        var result = FaceCutter.CutFace(tri, 0, new List<IntersectionSegment>());
        Assert.Single(result);
    }
}
