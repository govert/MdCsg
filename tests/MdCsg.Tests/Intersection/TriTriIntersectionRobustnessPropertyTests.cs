using MdCsg.Intersection;
using MdCsg.Math;

namespace MdCsg.Tests.Intersection;

/// <summary>Phase 6: TriTriIntersection — Intersect, AreCoplanar, IntersectCoplanar edge cases</summary>
public class TriTriIntersectionRobustnessPropertyTests
{
    [Fact]
    public void Intersect_OverlappingTriangles_ReturnsSegment()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(4, 0, 0), new Vec3(2, 4, 0));
        var t2 = new Triangle3(new Vec3(2, 0, -2), new Vec3(2, 0, 2), new Vec3(2, 4, 0));
        bool result = TriTriIntersection.Intersect(t1, t2, out var seg);
        Assert.True(result, "Crossing triangles should intersect");
        Assert.False(seg.IsDegenerate);
    }

    [Fact]
    public void Intersect_DisjointTriangles_ReturnsFalse()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(10, 10, 10), new Vec3(11, 10, 10), new Vec3(10, 11, 10));
        bool result = TriTriIntersection.Intersect(t1, t2, out _);
        Assert.False(result);
    }

    [Fact]
    public void Intersect_SameTriangle_CoplanarReturnsFalse()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        bool result = TriTriIntersection.Intersect(t1, t1, out _);
        Assert.False(result, "Coplanar triangles return false from Intersect");
    }

    [Fact]
    public void Intersect_ParallelTriangles_ReturnsFalse()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0, 0, 5), new Vec3(1, 0, 5), new Vec3(0, 1, 5));
        bool result = TriTriIntersection.Intersect(t1, t2, out _);
        Assert.False(result);
    }

    [Fact]
    public void Intersect_AllT2AbovePlane_ReturnsFalse()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(4, 0, 0), new Vec3(0, 4, 0));
        var t2 = new Triangle3(new Vec3(0, 0, 1), new Vec3(1, 0, 1), new Vec3(0, 1, 1));
        bool result = TriTriIntersection.Intersect(t1, t2, out _);
        Assert.False(result);
    }

    [Fact]
    public void Intersect_AllT2BelowPlane_ReturnsFalse()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(4, 0, 0), new Vec3(0, 4, 0));
        var t2 = new Triangle3(new Vec3(0, 0, -3), new Vec3(1, 0, -3), new Vec3(0, 1, -3));
        bool result = TriTriIntersection.Intersect(t1, t2, out _);
        Assert.False(result);
    }

    [Fact]
    public void Intersect_Perpendicular_SegmentHasLength()
    {
        var t1 = new Triangle3(new Vec3(-2, -2, 0), new Vec3(2, -2, 0), new Vec3(0, 2, 0));
        var t2 = new Triangle3(new Vec3(-2, 0, -2), new Vec3(2, 0, -2), new Vec3(0, 0, 2));
        bool result = TriTriIntersection.Intersect(t1, t2, out var seg);
        Assert.True(result);
        Assert.True(seg.Length > 0.1, $"Segment length should be significant, got {seg.Length}");
    }

    [Fact]
    public void Intersect_TiltedTriangles_ProducesSegment()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(4, 0, 0), new Vec3(2, 4, 2));
        var t2 = new Triangle3(new Vec3(2, -1, -1), new Vec3(2, 3, -1), new Vec3(2, 1, 3));
        bool result = TriTriIntersection.Intersect(t1, t2, out var seg);
        Assert.True(result);
        Assert.False(seg.IsDegenerate);
    }

    [Fact]
    public void Intersect_Symmetric()
    {
        var t1 = new Triangle3(new Vec3(-2, -2, 0), new Vec3(2, -2, 0), new Vec3(0, 2, 0));
        var t2 = new Triangle3(new Vec3(-2, 0, -2), new Vec3(2, 0, -2), new Vec3(0, 0, 2));
        bool r1 = TriTriIntersection.Intersect(t1, t2, out var seg1);
        bool r2 = TriTriIntersection.Intersect(t2, t1, out var seg2);
        Assert.Equal(r1, r2);
        if (r1)
        {
            Assert.True(System.Math.Abs(seg1.Length - seg2.Length) < 0.01,
                $"Symmetric intersections should have same length: {seg1.Length} vs {seg2.Length}");
        }
    }

    [Fact]
    public void Intersect_OneVertexOnPlane_NoException()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(4, 0, 0), new Vec3(0, 4, 0));
        var t2 = new Triangle3(new Vec3(1, 1, 0), new Vec3(1, 1, 2), new Vec3(1, 1, -2));
        _ = TriTriIntersection.Intersect(t1, t2, out _);
        Assert.True(true);
    }

    [Fact]
    public void Intersect_TwoVerticesOnPlane_NoException()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(4, 0, 0), new Vec3(0, 4, 0));
        var t2 = new Triangle3(new Vec3(1, 1, 2), new Vec3(1, 1, 0), new Vec3(2, 1, 0));
        _ = TriTriIntersection.Intersect(t1, t2, out _);
        Assert.True(true);
    }

    [Fact]
    public void AreCoplanar_SamePlane_ReturnsTrue()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(2, 0, 0), new Vec3(3, 0, 0), new Vec3(2, 1, 0));
        Assert.True(TriTriIntersection.AreCoplanar(t1, t2));
    }

    [Fact]
    public void AreCoplanar_DifferentPlane_ReturnsFalse()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0, 0, 1), new Vec3(1, 0, 1), new Vec3(0, 1, 1));
        Assert.False(TriTriIntersection.AreCoplanar(t1, t2));
    }

    [Fact]
    public void AreCoplanar_Tilted_ReturnsFalse()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 0, 1));
        Assert.False(TriTriIntersection.AreCoplanar(t1, t2));
    }

    [Fact]
    public void AreCoplanar_SameTriangle_ReturnsTrue()
    {
        var t = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.True(TriTriIntersection.AreCoplanar(t, t));
    }

    [Fact]
    public void IntersectCoplanar_DisjointSamePlane_NoSegments()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(5, 0, 0), new Vec3(6, 0, 0), new Vec3(5, 1, 0));
        bool result = TriTriIntersection.IntersectCoplanar(t1, t2, out var s1, out var s2, out _);
        Assert.False(result);
        Assert.Empty(s1);
        Assert.Empty(s2);
    }

    [Fact]
    public void IntersectCoplanar_Overlapping_ProducesSegments()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(4, 0, 0), new Vec3(0, 4, 0));
        var t2 = new Triangle3(new Vec3(1, 1, 0), new Vec3(5, 1, 0), new Vec3(1, 5, 0));
        bool result = TriTriIntersection.IntersectCoplanar(t1, t2, out var s1, out var s2, out _);
        Assert.True(result, "Overlapping coplanar triangles should produce segments");
    }

    [Fact]
    public void IntersectCoplanar_NormalsAgree_SameWinding()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(4, 0, 0), new Vec3(0, 4, 0));
        var t2 = new Triangle3(new Vec3(1, 1, 0), new Vec3(5, 1, 0), new Vec3(1, 5, 0));
        TriTriIntersection.IntersectCoplanar(t1, t2, out _, out _, out var agree);
        Assert.True(agree, "Same-winding coplanar triangles should have normals agree");
    }

    [Fact]
    public void IntersectCoplanar_NormalsDisagree_OppositeWinding()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(4, 0, 0), new Vec3(0, 4, 0));
        var t2 = new Triangle3(new Vec3(1, 1, 0), new Vec3(1, 5, 0), new Vec3(5, 1, 0));
        TriTriIntersection.IntersectCoplanar(t1, t2, out _, out _, out var agree);
        Assert.False(agree, "Opposite-winding coplanar triangles should have normals disagree");
    }

    [Fact]
    public void Intersect_LargeTriangle_SmallTriangle_Works()
    {
        var big = new Triangle3(new Vec3(-100, -100, 0), new Vec3(100, -100, 0), new Vec3(0, 100, 0));
        var small = new Triangle3(new Vec3(-0.1, -0.1, -0.1), new Vec3(0.1, -0.1, 0.1), new Vec3(0, 0.1, 0));
        bool result = TriTriIntersection.Intersect(big, small, out var seg);
        Assert.True(result);
        Assert.True(seg.Length > 0);
    }

    [Fact]
    public void Intersect_NearMissTriangles_ReturnsFalse()
    {
        // Two triangles barely not intersecting
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0, 0, 0.01), new Vec3(1, 0, 0.01), new Vec3(0.5, 0.5, 1));
        // All of t2 above t1's plane → no intersection
        bool result = TriTriIntersection.Intersect(t1, t2, out _);
        Assert.False(result);
    }

    [Fact]
    public void Intersect_LargeCoordinates_StillWorks()
    {
        var offset = new Vec3(1e6, 1e6, 1e6);
        var t1 = new Triangle3(
            offset + new Vec3(-2, -2, 0),
            offset + new Vec3(2, -2, 0),
            offset + new Vec3(0, 2, 0));
        var t2 = new Triangle3(
            offset + new Vec3(-2, 0, -2),
            offset + new Vec3(2, 0, -2),
            offset + new Vec3(0, 0, 2));
        bool result = TriTriIntersection.Intersect(t1, t2, out var seg);
        Assert.True(result, "Should work at large coordinates");
        Assert.True(seg.Length > 0.1);
    }
}
