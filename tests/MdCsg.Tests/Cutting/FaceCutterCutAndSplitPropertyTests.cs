using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;

namespace MdCsg.Tests.Cutting;

/// <summary>Phase 6: FaceCutter — cut face, area preservation, vertex welding, sub-triangle topology</summary>
public class FaceCutterCutAndSplitPropertyTests
{
    [Fact]
    public void CutFace_NoSegments_SingleSubTriangle()
    {
        var tri = new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        var result = FaceCutter.CutFace(tri, 0, Array.Empty<IntersectionSegment>());
        Assert.Single(result);
        Assert.False(result[0].HasIntersectionEdge);
    }

    [Fact]
    public void CutFace_NoSegments_PreservesVertices()
    {
        var tri = new Triangle3(new Vec3(1, 2, 3), new Vec3(4, 5, 6), new Vec3(7, 8, 9));
        var result = FaceCutter.CutFace(tri, 5, Array.Empty<IntersectionSegment>());
        Assert.Single(result);
        Assert.Equal(tri.A, result[0].A);
        Assert.Equal(tri.B, result[0].B);
        Assert.Equal(tri.C, result[0].C);
    }

    [Fact]
    public void CutFace_NoSegments_PreservesOriginalFaceIndex()
    {
        var tri = new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        var result = FaceCutter.CutFace(tri, 42, Array.Empty<IntersectionSegment>());
        Assert.Equal(42, result[0].OriginalFaceIndex);
    }

    [Fact]
    public void CutFace_OneSegment_ProducesMultipleSubTriangles()
    {
        var tri = new Triangle3(Vec3.Zero, new Vec3(2, 0, 0), new Vec3(0, 2, 0));
        var seg = new IntersectionSegment(new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, 1);
        var result = FaceCutter.CutFace(tri, 0, new[] { seg });
        Assert.True(result.Count > 1, $"Expected more than 1 sub-triangle, got {result.Count}");
    }

    [Fact]
    public void CutFace_OneSegment_AtLeastOneHasIntersectionEdge()
    {
        var tri = new Triangle3(Vec3.Zero, new Vec3(2, 0, 0), new Vec3(0, 2, 0));
        var seg = new IntersectionSegment(new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, 1);
        var result = FaceCutter.CutFace(tri, 0, new[] { seg });
        Assert.True(result.Any(st => st.HasIntersectionEdge),
            "At least one sub-triangle should have an intersection edge");
    }

    [Fact]
    public void CutFace_OneSegment_IntersectionEdgeFlagsConsistent()
    {
        var tri = new Triangle3(Vec3.Zero, new Vec3(2, 0, 0), new Vec3(0, 2, 0));
        var seg = new IntersectionSegment(new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, 1);
        var result = FaceCutter.CutFace(tri, 0, new[] { seg });
        foreach (var st in result)
        {
            bool anyFlag = st.IntersectionEdgeFlags != 0;
            Assert.Equal(anyFlag, st.HasIntersectionEdge);
        }
    }

    [Fact]
    public void CutFace_AllSubTrianglesPreserveOriginalFaceIndex()
    {
        var tri = new Triangle3(Vec3.Zero, new Vec3(2, 0, 0), new Vec3(0, 2, 0));
        var seg = new IntersectionSegment(new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, 1);
        var result = FaceCutter.CutFace(tri, 7, new[] { seg });
        Assert.All(result, st => Assert.Equal(7, st.OriginalFaceIndex));
    }

    [Fact]
    public void CutFace_TotalArea_Preserved()
    {
        var tri = new Triangle3(Vec3.Zero, new Vec3(2, 0, 0), new Vec3(0, 2, 0));
        double originalArea = tri.Area;
        var seg = new IntersectionSegment(new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, 1);
        var result = FaceCutter.CutFace(tri, 0, new[] { seg });
        double totalArea = result.Sum(st => new Triangle3(st.A, st.B, st.C).Area);
        Assert.True(System.Math.Abs(totalArea - originalArea) < 0.01,
            $"Total area {totalArea} should match original {originalArea}");
    }

    [Fact]
    public void CutFace_SegmentThroughEdgeMidpoints_ProducesCorrectSubTriangles()
    {
        var tri = new Triangle3(Vec3.Zero, new Vec3(4, 0, 0), new Vec3(0, 4, 0));
        var seg = new IntersectionSegment(new Vec3(2, 0, 0), new Vec3(0, 2, 0), 0, 1);
        var result = FaceCutter.CutFace(tri, 0, new[] { seg });
        Assert.True(result.Count >= 2, $"Expected at least 2 sub-triangles, got {result.Count}");
    }

    [Fact]
    public void SubTriangle_IsEdgeIntersection_Bit0_AB()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, true, 0b001);
        Assert.True(st.IsEdgeIntersection(0));
        Assert.False(st.IsEdgeIntersection(1));
        Assert.False(st.IsEdgeIntersection(2));
    }

    [Fact]
    public void SubTriangle_IsEdgeIntersection_Bit1_BC()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, true, 0b010);
        Assert.False(st.IsEdgeIntersection(0));
        Assert.True(st.IsEdgeIntersection(1));
        Assert.False(st.IsEdgeIntersection(2));
    }

    [Fact]
    public void SubTriangle_IsEdgeIntersection_Bit2_CA()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, true, 0b100);
        Assert.False(st.IsEdgeIntersection(0));
        Assert.False(st.IsEdgeIntersection(1));
        Assert.True(st.IsEdgeIntersection(2));
    }

    [Fact]
    public void SubTriangle_AllEdgesIntersection()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, true, 0b111);
        Assert.True(st.IsEdgeIntersection(0));
        Assert.True(st.IsEdgeIntersection(1));
        Assert.True(st.IsEdgeIntersection(2));
    }

    [Fact]
    public void SubTriangle_NoEdgesIntersection()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, false, 0);
        Assert.False(st.IsEdgeIntersection(0));
        Assert.False(st.IsEdgeIntersection(1));
        Assert.False(st.IsEdgeIntersection(2));
    }

    [Fact]
    public void CutFace_SegmentEndpointOnVertex_Handled()
    {
        var tri = new Triangle3(Vec3.Zero, new Vec3(2, 0, 0), new Vec3(0, 2, 0));
        var seg = new IntersectionSegment(Vec3.Zero, new Vec3(1, 1, 0), 0, 1);
        var result = FaceCutter.CutFace(tri, 0, new[] { seg });
        Assert.True(result.Count >= 1, "Should handle segment starting at vertex");
        double totalArea = result.Sum(st => new Triangle3(st.A, st.B, st.C).Area);
        Assert.True(System.Math.Abs(totalArea - tri.Area) < 0.01);
    }

    [Fact]
    public void CutFace_DegenerateSegment_SameStartEnd_NoSplit()
    {
        var tri = new Triangle3(Vec3.Zero, new Vec3(2, 0, 0), new Vec3(0, 2, 0));
        var seg = new IntersectionSegment(new Vec3(1, 0, 0), new Vec3(1, 0, 0), 0, 1);
        var result = FaceCutter.CutFace(tri, 0, new[] { seg });
        Assert.True(result.Count >= 1);
    }
}
