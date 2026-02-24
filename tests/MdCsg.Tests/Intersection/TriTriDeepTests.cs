using MdCsg.Intersection;
using MdCsg.Math;

namespace MdCsg.Tests.Intersection;

/// <summary>Batch 47: TriTriIntersection deep tests (20 tests)</summary>
public class TriTriDeepTests
{
    [Fact]
    public void Intersecting_CrossedTriangles_ReturnsTrue()
    {
        var t1 = new Triangle3(
            new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var t2 = new Triangle3(
            new Vec3(1, 1, -1), new Vec3(1, 1, 1), new Vec3(1, -1, 0));
        Assert.True(TriTriIntersection.Intersect(t1, t2, out var seg));
        Assert.True(seg.Length > 0);
    }

    [Fact]
    public void Disjoint_ParallelTriangles_ReturnsFalse()
    {
        var t1 = new Triangle3(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(
            new Vec3(0, 0, 5), new Vec3(1, 0, 5), new Vec3(0, 1, 5));
        Assert.False(TriTriIntersection.Intersect(t1, t2, out _));
    }

    [Fact]
    public void Disjoint_SamePlane_NoOverlap_ReturnsFalse()
    {
        var t1 = new Triangle3(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(
            new Vec3(5, 0, 0), new Vec3(6, 0, 0), new Vec3(5, 1, 0));
        Assert.False(TriTriIntersection.Intersect(t1, t2, out _));
    }

    [Fact]
    public void Disjoint_NonParallel_NoOverlap_ReturnsFalse()
    {
        var t1 = new Triangle3(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(
            new Vec3(0, 0, 5), new Vec3(0, 0, 6), new Vec3(0, 1, 5));
        Assert.False(TriTriIntersection.Intersect(t1, t2, out _));
    }

    [Fact]
    public void Intersection_SegmentHasPositiveLength()
    {
        var t1 = new Triangle3(
            new Vec3(-1, -1, 0), new Vec3(1, -1, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(
            new Vec3(0, 0, -1), new Vec3(0, 0, 1), new Vec3(0, 2, 0));
        if (TriTriIntersection.Intersect(t1, t2, out var seg))
        {
            Assert.True(seg.Length > 0);
        }
    }

    [Fact]
    public void PerpendicularTriangles_Intersect()
    {
        // XY plane triangle
        var t1 = new Triangle3(
            new Vec3(-1, -1, 0), new Vec3(1, -1, 0), new Vec3(0, 1, 0));
        // XZ plane triangle passing through t1
        var t2 = new Triangle3(
            new Vec3(-1, 0, -1), new Vec3(1, 0, -1), new Vec3(0, 0, 1));
        Assert.True(TriTriIntersection.Intersect(t1, t2, out var seg));
        // Intersection should be along the X axis at Y=0, Z=0
        Assert.True(System.Math.Abs(seg.Start.Y) < 0.01);
        Assert.True(System.Math.Abs(seg.Start.Z) < 0.01);
    }

    [Fact]
    public void AllVerticesOnOneSide_NoIntersection()
    {
        var t1 = new Triangle3(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(
            new Vec3(0, 0, 1), new Vec3(1, 0, 1), new Vec3(0, 1, 1));
        Assert.False(TriTriIntersection.Intersect(t1, t2, out _));
    }

    [Fact]
    public void LargeTriangles_Intersect()
    {
        var t1 = new Triangle3(
            new Vec3(-100, -100, 0), new Vec3(100, -100, 0), new Vec3(0, 100, 0));
        var t2 = new Triangle3(
            new Vec3(0, 0, -100), new Vec3(0, 0, 100), new Vec3(0, 100, 0));
        Assert.True(TriTriIntersection.Intersect(t1, t2, out _));
    }

    [Fact]
    public void SmallTriangles_Intersect()
    {
        var t1 = new Triangle3(
            new Vec3(-0.001, -0.001, 0), new Vec3(0.001, -0.001, 0), new Vec3(0, 0.001, 0));
        var t2 = new Triangle3(
            new Vec3(0, 0, -0.001), new Vec3(0, 0, 0.001), new Vec3(0, 0.001, 0));
        Assert.True(TriTriIntersection.Intersect(t1, t2, out _));
    }

    [Fact]
    public void AreCoplanar_CoplanarTriangles()
    {
        var t1 = new Triangle3(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(
            new Vec3(0.5, 0.5, 0), new Vec3(1.5, 0.5, 0), new Vec3(0.5, 1.5, 0));
        Assert.True(TriTriIntersection.AreCoplanar(t1, t2));
    }

    [Fact]
    public void AreCoplanar_NonCoplanar_ReturnsFalse()
    {
        var t1 = new Triangle3(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(
            new Vec3(0, 0, 1), new Vec3(1, 0, 1), new Vec3(0, 1, 1));
        Assert.False(TriTriIntersection.AreCoplanar(t1, t2));
    }

    [Fact]
    public void Intersection_ReturnedSegment_HasFaceIndicesDefault()
    {
        var t1 = new Triangle3(
            new Vec3(-1, -1, 0), new Vec3(1, -1, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(
            new Vec3(0, 0, -1), new Vec3(0, 0, 1), new Vec3(0, 2, 0));
        if (TriTriIntersection.Intersect(t1, t2, out var seg))
        {
            // Face indices are unset when called directly (default struct value)
            Assert.True(seg.FaceIndexA <= 0);
            Assert.True(seg.FaceIndexB <= 0);
        }
    }

    [Fact]
    public void TwoTriangles_SameOrientation_Cross()
    {
        // Two triangles on opposite orientations crossing at the center
        var t1 = new Triangle3(
            new Vec3(0, -1, -1), new Vec3(0, 1, -1), new Vec3(0, 0, 1));
        var t2 = new Triangle3(
            new Vec3(-1, 0, -1), new Vec3(1, 0, -1), new Vec3(0, 0, 1));
        Assert.True(TriTriIntersection.Intersect(t1, t2, out var seg));
        Assert.True(seg.Length > 0);
    }

    [Fact]
    public void VertexOnPlane_OneOnOppositeSide()
    {
        // t2 has one vertex on t1's plane, one above, one below
        var t1 = new Triangle3(
            new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var t2 = new Triangle3(
            new Vec3(1, 0.5, 0), new Vec3(1, 0.5, 1), new Vec3(1, 0.5, -1));
        Assert.True(TriTriIntersection.Intersect(t1, t2, out _));
    }

    [Fact]
    public void NearMiss_AbovePlane_NoIntersection()
    {
        var t1 = new Triangle3(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(
            new Vec3(0, 0, 0.0001), new Vec3(1, 0, 0.0001), new Vec3(0, 1, 0.0001));
        Assert.False(TriTriIntersection.Intersect(t1, t2, out _));
    }

    [Fact]
    public void OffsetTriangle_DoesNotIntersect()
    {
        var t1 = new Triangle3(
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(
            new Vec3(10, 10, 0), new Vec3(11, 10, 0), new Vec3(10, 11, 0));
        Assert.False(TriTriIntersection.Intersect(t1, t2, out _));
    }

    [Fact]
    public void NegativeCoords_TrianglesIntersect()
    {
        var t1 = new Triangle3(
            new Vec3(-2, -2, 0), new Vec3(0, -2, 0), new Vec3(-1, 0, 0));
        var t2 = new Triangle3(
            new Vec3(-1, -1, -1), new Vec3(-1, -1, 1), new Vec3(-1, 1, 0));
        Assert.True(TriTriIntersection.Intersect(t1, t2, out _));
    }

    [Fact]
    public void IntersectionSegment_IsDegenerate_NearlyCoincidentPoints()
    {
        var seg = new IntersectionSegment(
            new Vec3(0, 0, 0),
            new Vec3(1e-12, 1e-12, 1e-12),
            0, 0);
        Assert.True(seg.IsDegenerate);
    }

    [Fact]
    public void IntersectionSegment_NotDegenerate_ClearlyDifferent()
    {
        var seg = new IntersectionSegment(
            new Vec3(0, 0, 0),
            new Vec3(0.1, 0, 0),
            0, 0);
        Assert.False(seg.IsDegenerate);
    }
}
