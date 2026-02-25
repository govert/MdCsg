using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;

namespace MdCsg.Tests.Cutting;

/// <summary>Phase 6: FaceCutter SubTriangle — edge flag bits, intersection edge detection</summary>
public class FaceCutterEdgeFlagBitTests
{
    [Fact]
    public void SubTriangle_DefaultFlags_AllFalse()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false);
        Assert.False(st.IsEdgeIntersection(0)); // A-B
        Assert.False(st.IsEdgeIntersection(1)); // B-C
        Assert.False(st.IsEdgeIntersection(2)); // C-A
    }

    [Fact]
    public void SubTriangle_Bit0_MarksAB()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, true, 0b001);
        Assert.True(st.IsEdgeIntersection(0));  // A-B is set
        Assert.False(st.IsEdgeIntersection(1)); // B-C not set
        Assert.False(st.IsEdgeIntersection(2)); // C-A not set
    }

    [Fact]
    public void SubTriangle_Bit1_MarksBC()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, true, 0b010);
        Assert.False(st.IsEdgeIntersection(0)); // A-B not set
        Assert.True(st.IsEdgeIntersection(1));  // B-C is set
        Assert.False(st.IsEdgeIntersection(2)); // C-A not set
    }

    [Fact]
    public void SubTriangle_Bit2_MarksCA()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, true, 0b100);
        Assert.False(st.IsEdgeIntersection(0)); // A-B not set
        Assert.False(st.IsEdgeIntersection(1)); // B-C not set
        Assert.True(st.IsEdgeIntersection(2));  // C-A is set
    }

    [Fact]
    public void SubTriangle_AllBits_AllTrue()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, true, 0b111);
        Assert.True(st.IsEdgeIntersection(0));
        Assert.True(st.IsEdgeIntersection(1));
        Assert.True(st.IsEdgeIntersection(2));
    }

    [Fact]
    public void CutFace_NoSegments_NoIntersectionEdges()
    {
        var tri = new Triangle3(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var result = FaceCutter.CutFace(tri, 0, new List<IntersectionSegment>());
        Assert.Single(result);
        Assert.False(result[0].HasIntersectionEdge);
        Assert.Equal((byte)0, result[0].IntersectionEdgeFlags);
    }

    [Fact]
    public void CutFace_WithSegment_SomeEdgesMarked()
    {
        var tri = new Triangle3(Vec3.Zero, new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        // Segment cutting through the triangle
        var seg = new IntersectionSegment(new Vec3(0.5, 0.5, 0), new Vec3(1.5, 0.5, 0), 0, 1);
        var result = FaceCutter.CutFace(tri, 0, new List<IntersectionSegment> { seg });

        Assert.True(result.Count > 1, $"Expected multiple sub-triangles, got {result.Count}");

        // At least one sub-triangle should have an intersection edge
        Assert.True(result.Any(st => st.HasIntersectionEdge),
            "Expected at least one sub-triangle with intersection edge");
    }

    [Fact]
    public void CutFace_SubTriangles_PreserveOriginalFaceIndex()
    {
        var tri = new Triangle3(Vec3.Zero, new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var seg = new IntersectionSegment(new Vec3(0.5, 0.5, 0), new Vec3(1.5, 0.5, 0), 0, 1);
        var result = FaceCutter.CutFace(tri, 42, new List<IntersectionSegment> { seg });

        foreach (var st in result)
            Assert.Equal(42, st.OriginalFaceIndex);
    }

    [Fact]
    public void CutFace_SubTriangles_HavePositiveArea()
    {
        var tri = new Triangle3(Vec3.Zero, new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var seg = new IntersectionSegment(new Vec3(0.5, 0.5, 0), new Vec3(1.5, 0.5, 0), 0, 1);
        var result = FaceCutter.CutFace(tri, 0, new List<IntersectionSegment> { seg });

        foreach (var st in result)
        {
            var area = new Triangle3(st.A, st.B, st.C).Area;
            Assert.True(area > 0, $"Sub-triangle should have positive area, got {area}");
        }
    }

    [Fact]
    public void SubTriangle_RecordEquality()
    {
        var a = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, true, 0b101);
        var b = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, true, 0b101);
        Assert.Equal(a, b);
    }

    [Fact]
    public void SubTriangle_RecordInequality_DifferentFlags()
    {
        var a = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, true, 0b001);
        var b = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, true, 0b010);
        Assert.NotEqual(a, b);
    }
}
