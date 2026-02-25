using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;

namespace MdCsg.Tests.Cutting;

/// <summary>Phase 6: FaceCutter — cutting with segments, edge flags, area preservation, no segments</summary>
public class FaceCutterIntegrationTests
{
    private static readonly Triangle3 UnitTri = new(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));

    [Fact]
    public void NoSegments_ReturnsSingleSubTriangle()
    {
        var subs = FaceCutter.CutFace(UnitTri, 0, new List<IntersectionSegment>());
        Assert.Single(subs);
        Assert.Equal(Vec3.Zero, subs[0].A);
        Assert.Equal(new Vec3(1, 0, 0), subs[0].B);
        Assert.Equal(new Vec3(0, 1, 0), subs[0].C);
    }

    [Fact]
    public void NoSegments_HasNoIntersectionEdge()
    {
        var subs = FaceCutter.CutFace(UnitTri, 0, new List<IntersectionSegment>());
        Assert.False(subs[0].HasIntersectionEdge);
        Assert.Equal(0, subs[0].IntersectionEdgeFlags);
    }

    [Fact]
    public void NoSegments_OriginalFaceIndex_Preserved()
    {
        var subs = FaceCutter.CutFace(UnitTri, 42, new List<IntersectionSegment>());
        Assert.Equal(42, subs[0].OriginalFaceIndex);
    }

    [Fact]
    public void SingleSegment_ProducesMultipleSubTriangles()
    {
        // Segment cutting across the triangle
        var seg = new IntersectionSegment(new Vec3(0.5, 0, 0), new Vec3(0, 0.5, 0), 0, 1);
        var subs = FaceCutter.CutFace(UnitTri, 0, new[] { seg });
        Assert.True(subs.Count >= 2, $"Expected >= 2 sub-triangles, got {subs.Count}");
    }

    [Fact]
    public void SingleSegment_SomeEdgesFlagged()
    {
        var seg = new IntersectionSegment(new Vec3(0.5, 0, 0), new Vec3(0, 0.5, 0), 0, 1);
        var subs = FaceCutter.CutFace(UnitTri, 0, new[] { seg });
        bool anyIntersection = subs.Any(s => s.HasIntersectionEdge);
        Assert.True(anyIntersection);
    }

    [Fact]
    public void SingleSegment_AreaPreserved()
    {
        var seg = new IntersectionSegment(new Vec3(0.5, 0, 0), new Vec3(0, 0.5, 0), 0, 1);
        var subs = FaceCutter.CutFace(UnitTri, 0, new[] { seg });

        double totalArea = subs.Sum(s =>
            new Triangle3(s.A, s.B, s.C).Area);
        Assert.True(System.Math.Abs(totalArea - UnitTri.Area) < 1e-6,
            $"Total area {totalArea:F6} vs original {UnitTri.Area:F6}");
    }

    [Fact]
    public void SingleSegment_AllOriginalFaceIndex()
    {
        var seg = new IntersectionSegment(new Vec3(0.5, 0, 0), new Vec3(0, 0.5, 0), 0, 1);
        var subs = FaceCutter.CutFace(UnitTri, 7, new[] { seg });
        Assert.All(subs, s => Assert.Equal(7, s.OriginalFaceIndex));
    }

    [Fact]
    public void IsEdgeIntersection_BitFlags()
    {
        // Test the bit flag method directly
        var st = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.Zero, Vec3.Zero, 0, true, 0b101);
        Assert.True(st.IsEdgeIntersection(0));  // bit 0 set
        Assert.False(st.IsEdgeIntersection(1)); // bit 1 not set
        Assert.True(st.IsEdgeIntersection(2));  // bit 2 set
    }

    [Fact]
    public void SubTriangle_NoFlags_AllEdgesNotIntersection()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.Zero, Vec3.Zero, 0, false, 0);
        Assert.False(st.IsEdgeIntersection(0));
        Assert.False(st.IsEdgeIntersection(1));
        Assert.False(st.IsEdgeIntersection(2));
    }

    [Fact]
    public void SubTriangle_AllFlags_AllEdgesIntersection()
    {
        var st = new FaceCutter.SubTriangle(Vec3.Zero, Vec3.Zero, Vec3.Zero, 0, true, 0b111);
        Assert.True(st.IsEdgeIntersection(0));
        Assert.True(st.IsEdgeIntersection(1));
        Assert.True(st.IsEdgeIntersection(2));
    }

    [Fact]
    public void TwoSegments_ProducesMoreSubTriangles()
    {
        var seg1 = new IntersectionSegment(new Vec3(0.5, 0, 0), new Vec3(0, 0.5, 0), 0, 1);
        var seg2 = new IntersectionSegment(new Vec3(0.25, 0, 0), new Vec3(0, 0.25, 0), 0, 2);
        var subs = FaceCutter.CutFace(UnitTri, 0, new[] { seg1, seg2 });
        Assert.True(subs.Count >= 3, $"Expected >= 3 sub-triangles, got {subs.Count}");
    }

    [Fact]
    public void SubTriangle_RecordEquality()
    {
        var s1 = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false);
        var s2 = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false);
        Assert.Equal(s1, s2);
    }

    [Fact]
    public void SubTriangle_RecordInequality_DifferentFace()
    {
        var s1 = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false);
        var s2 = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 1, false);
        Assert.NotEqual(s1, s2);
    }
}
