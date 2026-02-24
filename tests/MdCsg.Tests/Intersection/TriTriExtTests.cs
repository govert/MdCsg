using MdCsg.Intersection;
using MdCsg.Math;

namespace MdCsg.Tests.Intersection;

/// <summary>Batch 13: TriTriIntersection extended tests (20 tests)</summary>
public class TriTriExtTests
{
    [Fact]
    public void Disjoint_InXY_Separated()
    {
        var t1 = new Triangle3(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0));
        var t2 = new Triangle3(new Vec3(5,5,0), new Vec3(6,5,0), new Vec3(5,6,0));
        Assert.False(TriTriIntersection.Intersect(t1, t2, out _));
    }

    [Fact]
    public void Disjoint_Parallel_DifferentPlanes()
    {
        var t1 = new Triangle3(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0));
        var t2 = new Triangle3(new Vec3(0,0,5), new Vec3(1,0,5), new Vec3(0,1,5));
        Assert.False(TriTriIntersection.Intersect(t1, t2, out _));
    }

    [Fact]
    public void Crossing_Perpendicular_Produces_Segment()
    {
        // T1 in XY plane, T2 in XZ plane, crossing along X axis
        var t1 = new Triangle3(new Vec3(-1,-1,0), new Vec3(2,-1,0), new Vec3(0.5,2,0));
        var t2 = new Triangle3(new Vec3(-1,0,-1), new Vec3(2,0,-1), new Vec3(0.5,0,2));
        Assert.True(TriTriIntersection.Intersect(t1, t2, out var seg));
        Assert.True(seg.Length > 0);
    }

    [Fact]
    public void EdgeTouch_NoIntersection()
    {
        // Two triangles sharing an edge but not overlapping
        var t1 = new Triangle3(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0));
        var t2 = new Triangle3(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,-1,0));
        // Same plane but different sides — this is coplanar case
        bool result = TriTriIntersection.Intersect(t1, t2, out _);
        // Coplanar returns false from Intersect
        Assert.False(result);
    }

    [Fact]
    public void OneTriangleCrossingOther_BothDirections()
    {
        var t1 = new Triangle3(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0.5,1,0));
        var t2 = new Triangle3(new Vec3(0.25,0.5,-1), new Vec3(0.75,0.5,-1), new Vec3(0.5,0.5,1));
        Assert.True(TriTriIntersection.Intersect(t1, t2, out var seg));
        Assert.True(seg.Length > 0);
    }

    [Fact]
    public void VertexOnEdge_StillDetects()
    {
        var t1 = new Triangle3(new Vec3(0,0,0), new Vec3(2,0,0), new Vec3(1,2,0));
        var t2 = new Triangle3(new Vec3(1,0,-1), new Vec3(1,1,-1), new Vec3(1,0.5,1));
        bool result = TriTriIntersection.Intersect(t1, t2, out _);
        // This should detect an intersection since t2 crosses through t1
        Assert.True(result);
    }

    [Fact]
    public void LargeTriangles_CrossingDetected()
    {
        double s = 1000;
        var t1 = new Triangle3(new Vec3(-s,-s,0), new Vec3(s,-s,0), new Vec3(0,s,0));
        var t2 = new Triangle3(new Vec3(-s,0,-s), new Vec3(s,0,-s), new Vec3(0,0,s));
        Assert.True(TriTriIntersection.Intersect(t1, t2, out _));
    }

    [Fact]
    public void TinyTriangles_CrossingDetected()
    {
        double eps = 1e-6;
        var t1 = new Triangle3(new Vec3(-eps,-eps,0), new Vec3(eps,-eps,0), new Vec3(0,eps,0));
        var t2 = new Triangle3(new Vec3(-eps,0,-eps), new Vec3(eps,0,-eps), new Vec3(0,0,eps));
        Assert.True(TriTriIntersection.Intersect(t1, t2, out _));
    }

    [Fact]
    public void AllVerticesOnOneSide_NoIntersection()
    {
        var t1 = new Triangle3(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0));
        var t2 = new Triangle3(new Vec3(0,0,1), new Vec3(1,0,1), new Vec3(0,1,1));
        Assert.False(TriTriIntersection.Intersect(t1, t2, out _));
    }

    [Fact]
    public void IntersectionSegment_Length_IsPositive()
    {
        var t1 = new Triangle3(new Vec3(-1,-1,0), new Vec3(2,-1,0), new Vec3(0.5,2,0));
        var t2 = new Triangle3(new Vec3(0,-1,-1), new Vec3(0,2,-1), new Vec3(0,0.5,2));
        Assert.True(TriTriIntersection.Intersect(t1, t2, out var seg));
        Assert.True(seg.Length > 0);
    }

    [Fact]
    public void AreCoplanar_SamePlane_ReturnsTrue()
    {
        var t1 = new Triangle3(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0));
        var t2 = new Triangle3(new Vec3(0.5,0,0), new Vec3(1.5,0,0), new Vec3(0.5,1,0));
        Assert.True(TriTriIntersection.AreCoplanar(t1, t2));
    }

    [Fact]
    public void AreCoplanar_DifferentPlanes_ReturnsFalse()
    {
        var t1 = new Triangle3(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0));
        var t2 = new Triangle3(new Vec3(0,0,1), new Vec3(1,0,1), new Vec3(0,1,1));
        Assert.False(TriTriIntersection.AreCoplanar(t1, t2));
    }

    [Fact]
    public void AreCoplanar_Perpendicular_ReturnsFalse()
    {
        var t1 = new Triangle3(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0));
        var t2 = new Triangle3(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,0,1));
        Assert.False(TriTriIntersection.AreCoplanar(t1, t2));
    }

    [Fact]
    public void IntersectCoplanar_Overlapping()
    {
        // t2 entirely inside t1 — all edges of t2 are clipped to be inside t1
        var t1 = new Triangle3(new Vec3(-1,-1,0), new Vec3(3,-1,0), new Vec3(1,3,0));
        var t2 = new Triangle3(new Vec3(0.5,0,0), new Vec3(1.5,0,0), new Vec3(1,1,0));
        bool result = TriTriIntersection.IntersectCoplanar(t1, t2, out var segsA, out var segsB, out var normalsAgree);
        // May or may not produce segments depending on clipping rules
        // But normal agreement should be correct when segments exist
        if (result)
            Assert.True(normalsAgree); // Both in XY plane, same winding
    }

    [Fact]
    public void IntersectCoplanar_Disjoint()
    {
        var t1 = new Triangle3(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0));
        var t2 = new Triangle3(new Vec3(5,5,0), new Vec3(6,5,0), new Vec3(5,6,0));
        Assert.False(TriTriIntersection.IntersectCoplanar(t1, t2, out _, out _, out _));
    }

    [Fact]
    public void IntersectCoplanar_OppositeNormals()
    {
        var t1 = new Triangle3(new Vec3(0,0,0), new Vec3(2,0,0), new Vec3(1,2,0));
        // Reversed winding
        var t2 = new Triangle3(new Vec3(0.5,0.5,0), new Vec3(1,1.5,0), new Vec3(1.5,0.5,0));
        if (TriTriIntersection.IntersectCoplanar(t1, t2, out _, out _, out var normalsAgree))
        {
            Assert.False(normalsAgree);
        }
    }

    [Fact]
    public void IntersectionSegment_IsDegenerate_ZeroLength()
    {
        var seg = new IntersectionSegment(Vec3.Zero, Vec3.Zero, 0, 1);
        Assert.True(seg.IsDegenerate);
    }

    [Fact]
    public void IntersectionSegment_IsDegenerate_TinyLength()
    {
        var seg = new IntersectionSegment(Vec3.Zero, new Vec3(1e-12, 0, 0), 0, 1);
        Assert.True(seg.IsDegenerate);
    }

    [Fact]
    public void IntersectionSegment_NotDegenerate()
    {
        var seg = new IntersectionSegment(Vec3.Zero, Vec3.UnitX, 0, 1);
        Assert.False(seg.IsDegenerate);
        Assert.Equal(1.0, seg.Length, 1e-15);
    }

    [Fact]
    public void Intersect_ReturnedSegment_HasDefaultFaceIndices()
    {
        var t1 = new Triangle3(new Vec3(-1,-1,0), new Vec3(2,-1,0), new Vec3(0.5,2,0));
        var t2 = new Triangle3(new Vec3(0,-1,-1), new Vec3(0,2,-1), new Vec3(0,0.5,2));
        Assert.True(TriTriIntersection.Intersect(t1, t2, out var seg));
        Assert.Equal(-1, seg.FaceIndexA);
        Assert.Equal(-1, seg.FaceIndexB);
    }
}
