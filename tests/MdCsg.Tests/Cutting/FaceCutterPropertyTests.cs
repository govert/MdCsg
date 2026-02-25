using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;

namespace MdCsg.Tests.Cutting;

/// <summary>Phase 6: FaceCutter — CutFace with various segment configurations, SubTriangle properties</summary>
public class FaceCutterPropertyTests
{
    [Fact]
    public void CutFace_NoSegments_ReturnsSingleSubTriangle()
    {
        var tri = new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        var result = FaceCutter.CutFace(tri, 0, Array.Empty<IntersectionSegment>());
        Assert.Single(result);
        Assert.False(result[0].HasIntersectionEdge);
        Assert.Equal(0, result[0].OriginalFaceIndex);
    }

    [Fact]
    public void CutFace_NoSegments_PreservesVertices()
    {
        var tri = new Triangle3(new Vec3(1, 2, 3), new Vec3(4, 5, 6), new Vec3(7, 8, 9));
        var result = FaceCutter.CutFace(tri, 5, Array.Empty<IntersectionSegment>());
        Assert.Equal(new Vec3(1, 2, 3), result[0].A);
        Assert.Equal(new Vec3(4, 5, 6), result[0].B);
        Assert.Equal(new Vec3(7, 8, 9), result[0].C);
    }

    [Fact]
    public void CutFace_OneSegment_ProducesMultipleSubTriangles()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        // Segment cutting across the middle
        var seg = new IntersectionSegment(new Vec3(0.5, 1, 0), new Vec3(1.5, 1, 0), 0, 1);
        var result = FaceCutter.CutFace(tri, 0, new[] { seg });
        Assert.True(result.Count > 1);
    }

    [Fact]
    public void CutFace_OneSegment_HasIntersectionEdgeFlags()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var seg = new IntersectionSegment(new Vec3(0.5, 1, 0), new Vec3(1.5, 1, 0), 0, 1);
        var result = FaceCutter.CutFace(tri, 0, new[] { seg });
        // At least one sub-triangle should have intersection edges
        Assert.True(result.Any(st => st.HasIntersectionEdge));
    }

    [Fact]
    public void CutFace_PreservesFaceIndex()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var seg = new IntersectionSegment(new Vec3(0.5, 1, 0), new Vec3(1.5, 1, 0), 0, 1);
        var result = FaceCutter.CutFace(tri, 42, new[] { seg });
        foreach (var st in result)
            Assert.Equal(42, st.OriginalFaceIndex);
    }

    [Fact]
    public void SubTriangle_IsEdgeIntersection_Bit0()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, true, 1);
        Assert.True(st.IsEdgeIntersection(0));  // A-B
        Assert.False(st.IsEdgeIntersection(1)); // B-C
        Assert.False(st.IsEdgeIntersection(2)); // C-A
    }

    [Fact]
    public void SubTriangle_IsEdgeIntersection_Bit1()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, true, 2);
        Assert.False(st.IsEdgeIntersection(0));
        Assert.True(st.IsEdgeIntersection(1));
        Assert.False(st.IsEdgeIntersection(2));
    }

    [Fact]
    public void SubTriangle_IsEdgeIntersection_Bit2()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, true, 4);
        Assert.False(st.IsEdgeIntersection(0));
        Assert.False(st.IsEdgeIntersection(1));
        Assert.True(st.IsEdgeIntersection(2));
    }

    [Fact]
    public void SubTriangle_IsEdgeIntersection_AllBits()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, true, 7);
        Assert.True(st.IsEdgeIntersection(0));
        Assert.True(st.IsEdgeIntersection(1));
        Assert.True(st.IsEdgeIntersection(2));
    }

    [Fact]
    public void SubTriangle_NoFlags_NoIntersectionEdges()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, false, 0);
        Assert.False(st.IsEdgeIntersection(0));
        Assert.False(st.IsEdgeIntersection(1));
        Assert.False(st.IsEdgeIntersection(2));
    }

    [Fact]
    public void CutFace_DegenerateSegment_IgnoredLikeNoSegments()
    {
        var tri = new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        // Degenerate segment (same start and end)
        var seg = new IntersectionSegment(new Vec3(0.5, 0.5, 0), new Vec3(0.5, 0.5, 0), 0, 1);
        var result = FaceCutter.CutFace(tri, 0, new[] { seg });
        // Should still produce valid sub-triangles
        Assert.True(result.Count >= 1);
    }

    [Fact]
    public void SubTriangle_RecordEquality()
    {
        var a = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, false, 0);
        var b = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, false, 0);
        Assert.Equal(a, b);
    }

    [Fact]
    public void CutFace_TotalAreaPreserved()
    {
        var tri = new Triangle3(new Vec3(0, 0, 0), new Vec3(4, 0, 0), new Vec3(0, 4, 0));
        var seg = new IntersectionSegment(new Vec3(1, 1, 0), new Vec3(3, 1, 0), 0, 1);
        var result = FaceCutter.CutFace(tri, 0, new[] { seg });
        double totalArea = result.Sum(st =>
        {
            var t = new Triangle3(st.A, st.B, st.C);
            return t.Area;
        });
        Assert.True(System.Math.Abs(totalArea - tri.Area) < 0.01);
    }
}
