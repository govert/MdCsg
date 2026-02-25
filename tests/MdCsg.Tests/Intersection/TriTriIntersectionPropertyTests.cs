using MdCsg.Intersection;
using MdCsg.Math;

namespace MdCsg.Tests.Intersection;

/// <summary>Phase 6: TriTriIntersection — Intersect, AreCoplanar, IntersectCoplanar edge cases</summary>
public class TriTriIntersectionPropertyTests
{
    [Fact]
    public void Intersect_PerpendicularTriangles_ProducesSegment()
    {
        // Triangle in XY plane and triangle in XZ plane, overlapping along X axis
        var t1 = new Triangle3(new Vec3(0, -1, 0), new Vec3(2, -1, 0), new Vec3(1, 1, 0));
        var t2 = new Triangle3(new Vec3(0, 0, -1), new Vec3(2, 0, -1), new Vec3(1, 0, 1));
        bool hit = TriTriIntersection.Intersect(t1, t2, out var seg);
        Assert.True(hit);
        Assert.True(seg.Length > 0);
    }

    [Fact]
    public void Intersect_DisjointTriangles_NoIntersection()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(10, 10, 10), new Vec3(11, 10, 10), new Vec3(10, 11, 10));
        bool hit = TriTriIntersection.Intersect(t1, t2, out _);
        Assert.False(hit);
    }

    [Fact]
    public void Intersect_SamePlaneTriangles_ReturnsFalse()
    {
        // Coplanar triangles: Intersect returns false (handled by IntersectCoplanar separately)
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0.5, 0, 0), new Vec3(1.5, 0, 0), new Vec3(0.5, 1, 0));
        bool hit = TriTriIntersection.Intersect(t1, t2, out _);
        Assert.False(hit);
    }

    [Fact]
    public void Intersect_AllVerticesOneSide_NoIntersection()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0, 0, 5), new Vec3(1, 0, 5), new Vec3(0, 1, 5));
        bool hit = TriTriIntersection.Intersect(t1, t2, out _);
        Assert.False(hit);
    }

    [Fact]
    public void AreCoplanar_SamePlane_ReturnsTrue()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(2, 0, 0), new Vec3(3, 0, 0), new Vec3(2, 1, 0));
        Assert.True(TriTriIntersection.AreCoplanar(t1, t2));
    }

    [Fact]
    public void AreCoplanar_DifferentPlanes_ReturnsFalse()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0, 0, 1), new Vec3(1, 0, 1), new Vec3(0, 1, 2));
        Assert.False(TriTriIntersection.AreCoplanar(t1, t2));
    }

    [Fact]
    public void IntersectCoplanar_OverlappingTriangles_ProducesSegments()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var t2 = new Triangle3(new Vec3(0.5, 0.5, 0), new Vec3(2.5, 0.5, 0), new Vec3(1.5, 2.5, 0));
        bool result = TriTriIntersection.IntersectCoplanar(t1, t2, out var segsForT1, out var segsForT2, out bool normalsAgree);
        Assert.True(result);
        Assert.True(normalsAgree);
    }

    [Fact]
    public void IntersectCoplanar_OppositeNormals_NormalsDisagree()
    {
        // t2 has reversed winding (CW vs CCW)
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var t2 = new Triangle3(new Vec3(0.5, 0.5, 0), new Vec3(1.5, 2.5, 0), new Vec3(2.5, 0.5, 0));
        TriTriIntersection.IntersectCoplanar(t1, t2, out _, out _, out bool normalsAgree);
        Assert.False(normalsAgree);
    }

    [Fact]
    public void IntersectCoplanar_DisjointCoplanar_NoSegments()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(10, 10, 0), new Vec3(11, 10, 0), new Vec3(10, 11, 0));
        bool result = TriTriIntersection.IntersectCoplanar(t1, t2, out var segsForT1, out var segsForT2, out _);
        Assert.False(result);
        Assert.Empty(segsForT1);
        Assert.Empty(segsForT2);
    }

    [Fact]
    public void Intersect_Segment_HasNonNegativeLength()
    {
        var t1 = new Triangle3(new Vec3(0, -1, 0), new Vec3(2, -1, 0), new Vec3(1, 1, 0));
        var t2 = new Triangle3(new Vec3(0, 0, -1), new Vec3(2, 0, -1), new Vec3(1, 0, 1));
        TriTriIntersection.Intersect(t1, t2, out var seg);
        Assert.True(seg.Length >= 0);
    }

    [Fact]
    public void Intersect_IsSymmetric()
    {
        var t1 = new Triangle3(new Vec3(0, -1, 0), new Vec3(2, -1, 0), new Vec3(1, 1, 0));
        var t2 = new Triangle3(new Vec3(0, 0, -1), new Vec3(2, 0, -1), new Vec3(1, 0, 1));
        bool hit1 = TriTriIntersection.Intersect(t1, t2, out _);
        bool hit2 = TriTriIntersection.Intersect(t2, t1, out _);
        Assert.Equal(hit1, hit2);
    }

    [Fact]
    public void Intersect_ParallelNonCoplanar_NoIntersection()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0, 0, 1), new Vec3(1, 0, 1), new Vec3(0, 1, 1));
        Assert.False(TriTriIntersection.Intersect(t1, t2, out _));
    }

    [Fact]
    public void AreCoplanar_ParallelNotCoplanar_ReturnsFalse()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0, 0, 0.001), new Vec3(1, 0, 0.001), new Vec3(0, 1, 0.001));
        Assert.False(TriTriIntersection.AreCoplanar(t1, t2));
    }
}
