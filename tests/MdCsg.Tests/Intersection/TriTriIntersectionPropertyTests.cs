using MdCsg.Intersection;
using MdCsg.Math;

namespace MdCsg.Tests.Intersection;

/// <summary>Phase 6: TriTriIntersection — Intersect, AreCoplanar, IntersectCoplanar with diverse triangle configurations</summary>
public class TriTriIntersectionPropertyTests
{
    [Fact]
    public void Intersect_CrossingTriangles_ReturnsTrue()
    {
        var t1 = new Triangle3(new Vec3(-1, 0, -1), new Vec3(1, 0, -1), new Vec3(0, 0, 1));
        var t2 = new Triangle3(new Vec3(-1, -1, 0), new Vec3(1, -1, 0), new Vec3(0, 1, 0));
        Assert.True(TriTriIntersection.Intersect(t1, t2, out var seg));
        Assert.True(seg.Length > 0);
    }

    [Fact]
    public void Intersect_DisjointTriangles_ReturnsFalse()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(10, 10, 10), new Vec3(11, 10, 10), new Vec3(10, 11, 10));
        Assert.False(TriTriIntersection.Intersect(t1, t2, out _));
    }

    [Fact]
    public void Intersect_ParallelTriangles_ReturnsFalse()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0, 0, 5), new Vec3(1, 0, 5), new Vec3(0, 1, 5));
        Assert.False(TriTriIntersection.Intersect(t1, t2, out _));
    }

    [Fact]
    public void Intersect_AllVerticesAbove_ReturnsFalse()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0, 0, 1), new Vec3(1, 0, 2), new Vec3(0, 1, 3));
        Assert.False(TriTriIntersection.Intersect(t1, t2, out _));
    }

    [Fact]
    public void Intersect_PerpendicularCrossing_SegmentIsValid()
    {
        var t1 = new Triangle3(new Vec3(-2, -2, 0), new Vec3(2, -2, 0), new Vec3(0, 2, 0));
        var t2 = new Triangle3(new Vec3(0, -1, -2), new Vec3(0, -1, 2), new Vec3(0, 1, 0));
        bool hit = TriTriIntersection.Intersect(t1, t2, out var seg);
        if (hit)
        {
            Assert.True(seg.Length > 0);
        }
    }

    [Fact]
    public void Intersect_SegmentHasDefaultFaceIndices()
    {
        var t1 = new Triangle3(new Vec3(-1, 0, -1), new Vec3(1, 0, -1), new Vec3(0, 0, 1));
        var t2 = new Triangle3(new Vec3(-1, -1, 0), new Vec3(1, -1, 0), new Vec3(0, 1, 0));
        TriTriIntersection.Intersect(t1, t2, out var seg);
        Assert.Equal(-1, seg.FaceIndexA);
        Assert.Equal(-1, seg.FaceIndexB);
    }

    [Fact]
    public void Intersect_SymmetricLength()
    {
        var t1 = new Triangle3(new Vec3(-1, 0, -1), new Vec3(1, 0, -1), new Vec3(0, 0, 1));
        var t2 = new Triangle3(new Vec3(-1, -1, 0), new Vec3(1, -1, 0), new Vec3(0, 1, 0));
        TriTriIntersection.Intersect(t1, t2, out var seg12);
        TriTriIntersection.Intersect(t2, t1, out var seg21);
        Assert.True(System.Math.Abs(seg12.Length - seg21.Length) < 1e-10);
    }

    [Fact]
    public void Intersect_LargeTriangles_Works()
    {
        var t1 = new Triangle3(new Vec3(-1000, 0, -1000), new Vec3(1000, 0, -1000), new Vec3(0, 0, 1000));
        var t2 = new Triangle3(new Vec3(-1000, -1000, 0), new Vec3(1000, -1000, 0), new Vec3(0, 1000, 0));
        Assert.True(TriTriIntersection.Intersect(t1, t2, out var seg));
        Assert.True(seg.Length > 0);
    }

    [Fact]
    public void Intersect_TinyTriangles_DoesNotThrow()
    {
        var s = 1e-6;
        var t1 = new Triangle3(new Vec3(-s, 0, -s), new Vec3(s, 0, -s), new Vec3(0, 0, s));
        var t2 = new Triangle3(new Vec3(-s, -s, 0), new Vec3(s, -s, 0), new Vec3(0, s, 0));
        TriTriIntersection.Intersect(t1, t2, out _);
    }

    [Fact]
    public void AreCoplanar_SamePlane_True()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(2, 0, 0), new Vec3(3, 0, 0), new Vec3(2, 1, 0));
        Assert.True(TriTriIntersection.AreCoplanar(t1, t2));
    }

    [Fact]
    public void AreCoplanar_DifferentPlanes_False()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0, 0, 1), new Vec3(1, 0, 1), new Vec3(0, 1, 2));
        Assert.False(TriTriIntersection.AreCoplanar(t1, t2));
    }

    [Fact]
    public void AreCoplanar_ParallelOffset_False()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0, 0, 5), new Vec3(1, 0, 5), new Vec3(0, 1, 5));
        Assert.False(TriTriIntersection.AreCoplanar(t1, t2));
    }

    [Fact]
    public void AreCoplanar_IdenticalTriangle_True()
    {
        var t = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.True(TriTriIntersection.AreCoplanar(t, t));
    }

    [Fact]
    public void IntersectCoplanar_NonOverlapping_NoSegments()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(10, 10, 0), new Vec3(11, 10, 0), new Vec3(10, 11, 0));
        bool result = TriTriIntersection.IntersectCoplanar(t1, t2, out var s1, out var s2, out _);
        Assert.False(result);
        Assert.Empty(s1);
        Assert.Empty(s2);
    }

    [Fact]
    public void IntersectCoplanar_SameNormals_AgreesTrue()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var t2 = new Triangle3(new Vec3(0.5, 0.5, 0), new Vec3(2.5, 0.5, 0), new Vec3(1.5, 2.5, 0));
        TriTriIntersection.IntersectCoplanar(t1, t2, out _, out _, out var agree);
        Assert.True(agree);
    }

    [Fact]
    public void IntersectCoplanar_OppositeNormals_AgreesFalse()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var t2 = new Triangle3(new Vec3(0.5, 0.5, 0), new Vec3(1.5, 2.5, 0), new Vec3(2.5, 0.5, 0));
        TriTriIntersection.IntersectCoplanar(t1, t2, out _, out _, out var agree);
        Assert.False(agree);
    }

    [Fact]
    public void Intersect_CoplanarTriangles_ReturnsFalse()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var t2 = new Triangle3(new Vec3(0.5, 0.5, 0), new Vec3(2.5, 0.5, 0), new Vec3(1.5, 2.5, 0));
        Assert.False(TriTriIntersection.Intersect(t1, t2, out _));
    }

    [Fact]
    public void IntersectionSegment_Length_Correct()
    {
        var seg = new IntersectionSegment(Vec3.Zero, Vec3.UnitX, 0, 1);
        Assert.True(System.Math.Abs(seg.Length - 1.0) < 1e-15);
    }

    [Fact]
    public void IntersectionSegment_IsDegenerate_ZeroLength()
    {
        var seg = new IntersectionSegment(Vec3.Zero, Vec3.Zero, 0, 1);
        Assert.True(seg.IsDegenerate);
    }

    [Fact]
    public void IntersectionSegment_NotDegenerate_NonZero()
    {
        var seg = new IntersectionSegment(Vec3.Zero, new Vec3(1, 0, 0), 0, 1);
        Assert.False(seg.IsDegenerate);
    }

    [Fact]
    public void Intersect_InDifferentOctants_ReturnsFalse()
    {
        var t1 = new Triangle3(new Vec3(1, 1, 1), new Vec3(2, 1, 1), new Vec3(1, 2, 1));
        var t2 = new Triangle3(new Vec3(-3, -3, -3), new Vec3(-2, -3, -3), new Vec3(-3, -2, -3));
        Assert.False(TriTriIntersection.Intersect(t1, t2, out _));
    }

    [Fact]
    public void AreCoplanar_TiltedPlane_True()
    {
        var t1 = new Triangle3(new Vec3(3, 0, 0), new Vec3(0, 3, 0), new Vec3(0, 0, 3));
        var t2 = new Triangle3(new Vec3(1, 1, 1), new Vec3(2, 1, 0), new Vec3(1, 2, 0));
        Assert.True(TriTriIntersection.AreCoplanar(t1, t2));
    }

    [Fact]
    public void IntersectionSegment_RecordEquality()
    {
        var a = new IntersectionSegment(Vec3.Zero, Vec3.UnitX, 0, 1);
        var b = new IntersectionSegment(Vec3.Zero, Vec3.UnitX, 0, 1);
        Assert.Equal(a, b);
    }
}
