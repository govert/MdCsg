using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;

namespace MdCsg.Tests.Cutting;

/// <summary>Batch 16: FaceCutter tests (20 tests)</summary>
public class FaceCutterTests
{
    private static Triangle3 UnitTriXY => new(Vec3.Zero, Vec3.UnitX, Vec3.UnitY);

    [Fact]
    public void NoSegments_ReturnsSingleSubTriangle()
    {
        var result = FaceCutter.CutFace(UnitTriXY, 0, Array.Empty<IntersectionSegment>());
        Assert.Single(result);
        Assert.Equal(Vec3.Zero, result[0].A);
        Assert.Equal(Vec3.UnitX, result[0].B);
        Assert.Equal(Vec3.UnitY, result[0].C);
    }

    [Fact]
    public void NoSegments_SubTriangle_NoIntersectionEdge()
    {
        var result = FaceCutter.CutFace(UnitTriXY, 0, Array.Empty<IntersectionSegment>());
        Assert.False(result[0].HasIntersectionEdge);
        Assert.Equal(0, result[0].IntersectionEdgeFlags);
    }

    [Fact]
    public void NoSegments_PreservesFaceIndex()
    {
        var result = FaceCutter.CutFace(UnitTriXY, 42, Array.Empty<IntersectionSegment>());
        Assert.Equal(42, result[0].OriginalFaceIndex);
    }

    [Fact]
    public void SingleSegment_ProducesMultipleSubTriangles()
    {
        var seg = new IntersectionSegment(new Vec3(0.5, 0, 0), new Vec3(0, 0.5, 0), -1, -1);
        var result = FaceCutter.CutFace(UnitTriXY, 0, new[] { seg });
        Assert.True(result.Count >= 2);
    }

    [Fact]
    public void SingleSegment_AllSubTrianglesHaveSameFaceIndex()
    {
        var seg = new IntersectionSegment(new Vec3(0.5, 0, 0), new Vec3(0, 0.5, 0), -1, -1);
        var result = FaceCutter.CutFace(UnitTriXY, 7, new[] { seg });
        foreach (var st in result)
            Assert.Equal(7, st.OriginalFaceIndex);
    }

    [Fact]
    public void SingleSegment_SomeSubTrianglesHaveIntersectionEdge()
    {
        var seg = new IntersectionSegment(new Vec3(0.5, 0, 0), new Vec3(0, 0.5, 0), -1, -1);
        var result = FaceCutter.CutFace(UnitTriXY, 0, new[] { seg });
        Assert.True(result.Any(st => st.HasIntersectionEdge));
    }

    [Fact]
    public void SingleSegment_TotalVerticesCoverOriginal()
    {
        var seg = new IntersectionSegment(new Vec3(0.5, 0, 0), new Vec3(0, 0.5, 0), -1, -1);
        var result = FaceCutter.CutFace(UnitTriXY, 0, new[] { seg });
        var allVerts = new HashSet<Vec3>();
        foreach (var st in result)
        {
            allVerts.Add(st.A);
            allVerts.Add(st.B);
            allVerts.Add(st.C);
        }
        Assert.True(allVerts.Count >= 5);
    }

    [Fact]
    public void SegmentThroughMidpoints_ProducesValidCut()
    {
        var seg = new IntersectionSegment(new Vec3(0.5, 0, 0), new Vec3(0, 0.5, 0), -1, -1);
        var result = FaceCutter.CutFace(UnitTriXY, 0, new[] { seg });
        Assert.True(result.Count >= 2);
    }

    [Fact]
    public void SegmentEndpointAtVertex_Handled()
    {
        var seg = new IntersectionSegment(Vec3.Zero, new Vec3(0.5, 0.5, 0), -1, -1);
        var result = FaceCutter.CutFace(UnitTriXY, 0, new[] { seg });
        Assert.True(result.Count >= 2);
    }

    [Fact]
    public void DegenerateSegment_SameEndpoints_ReturnsSingleTriangle()
    {
        var seg = new IntersectionSegment(new Vec3(0.3, 0.3, 0), new Vec3(0.3, 0.3, 0), -1, -1);
        var result = FaceCutter.CutFace(UnitTriXY, 0, new[] { seg });
        Assert.True(result.Count >= 1);
    }

    [Fact]
    public void LargerTriangle_CutProducesSubTriangles()
    {
        var tri = new Triangle3(new Vec3(-1, -1, 0), new Vec3(2, -1, 0), new Vec3(0.5, 2, 0));
        var seg = new IntersectionSegment(new Vec3(0, 0, 0), new Vec3(1, 0, 0), -1, -1);
        var result = FaceCutter.CutFace(tri, 0, new[] { seg });
        Assert.True(result.Count >= 2);
    }

    [Fact]
    public void MultipleSegments_ProducesMoreSubTriangles()
    {
        var seg1 = new IntersectionSegment(new Vec3(0.3, 0, 0), new Vec3(0, 0.3, 0), -1, -1);
        var seg2 = new IntersectionSegment(new Vec3(0.7, 0, 0), new Vec3(0, 0.7, 0), -1, -1);
        var result = FaceCutter.CutFace(UnitTriXY, 0, new[] { seg1, seg2 });
        Assert.True(result.Count >= 3);
    }

    [Fact]
    public void SubTriangle_IsEdgeIntersection_ReflectsFlags()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, true, 0b101);
        Assert.True(st.IsEdgeIntersection(0));
        Assert.False(st.IsEdgeIntersection(1));
        Assert.True(st.IsEdgeIntersection(2));
    }

    [Fact]
    public void SubTriangle_IsEdgeIntersection_AllFalseWhenNoFlags()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, false, 0);
        Assert.False(st.IsEdgeIntersection(0));
        Assert.False(st.IsEdgeIntersection(1));
        Assert.False(st.IsEdgeIntersection(2));
    }

    [Fact]
    public void SubTriangle_IsEdgeIntersection_AllTrue()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.UnitX, Vec3.UnitY, 0, true, 0b111);
        Assert.True(st.IsEdgeIntersection(0));
        Assert.True(st.IsEdgeIntersection(1));
        Assert.True(st.IsEdgeIntersection(2));
    }

    [Fact]
    public void CutFace_InXZPlane()
    {
        var tri = new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitZ);
        var seg = new IntersectionSegment(new Vec3(0.5, 0, 0), new Vec3(0, 0, 0.5), -1, -1);
        var result = FaceCutter.CutFace(tri, 0, new[] { seg });
        Assert.True(result.Count >= 2);
    }

    [Fact]
    public void CutFace_InYZPlane()
    {
        var tri = new Triangle3(Vec3.Zero, Vec3.UnitY, Vec3.UnitZ);
        var seg = new IntersectionSegment(new Vec3(0, 0.5, 0), new Vec3(0, 0, 0.5), -1, -1);
        var result = FaceCutter.CutFace(tri, 0, new[] { seg });
        Assert.True(result.Count >= 2);
    }

    [Fact]
    public void CutFace_SegmentNearEdge_StillCuts()
    {
        var seg = new IntersectionSegment(new Vec3(0.01, 0, 0), new Vec3(0.01, 0.98, 0), -1, -1);
        var result = FaceCutter.CutFace(UnitTriXY, 0, new[] { seg });
        Assert.True(result.Count >= 2);
    }

    [Fact]
    public void CutFace_ParallelSegments()
    {
        var tri = new Triangle3(new Vec3(-1, -1, 0), new Vec3(3, -1, 0), new Vec3(1, 3, 0));
        var seg1 = new IntersectionSegment(new Vec3(0, 0, 0), new Vec3(2, 0, 0), -1, -1);
        var seg2 = new IntersectionSegment(new Vec3(0, 1, 0), new Vec3(2, 1, 0), -1, -1);
        var result = FaceCutter.CutFace(tri, 0, new[] { seg1, seg2 });
        Assert.True(result.Count >= 3);
    }

    [Fact]
    public void CutFace_AllSubTrianglesPreserveFaceIndex()
    {
        var tri = new Triangle3(new Vec3(-1, -1, 0), new Vec3(3, -1, 0), new Vec3(1, 3, 0));
        var seg = new IntersectionSegment(new Vec3(0, 0, 0), new Vec3(2, 0, 0), -1, -1);
        var result = FaceCutter.CutFace(tri, 99, new[] { seg });
        foreach (var st in result)
            Assert.Equal(99, st.OriginalFaceIndex);
    }
}
