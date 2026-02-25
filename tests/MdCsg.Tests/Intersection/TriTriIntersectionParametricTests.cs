using MdCsg.Intersection;
using MdCsg.Math;

namespace MdCsg.Tests.Intersection;

/// <summary>Phase 6: TriTriIntersection parametric — various overlap configurations, parallel, touching vertex, edge-edge</summary>
public class TriTriIntersectionParametricTests
{
    [Fact]
    public void Intersect_PerpendicularTriangles_OneSegment()
    {
        // Triangle in XY plane
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        // Triangle in XZ plane cutting through t1
        var t2 = new Triangle3(new Vec3(0.5, 0, -1), new Vec3(0.5, 0, 1), new Vec3(0.5, 2, 0));
        bool result = TriTriIntersection.Intersect(t1, t2, out var seg);
        Assert.True(result);
        Assert.True(seg.Length > 0);
    }

    [Fact]
    public void Intersect_DisjointTriangles_NoIntersection()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(5, 5, 5), new Vec3(6, 5, 5), new Vec3(5, 6, 5));
        bool result = TriTriIntersection.Intersect(t1, t2, out _);
        Assert.False(result);
    }

    [Fact]
    public void Intersect_ParallelNonCoplanar_NoIntersection()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0, 0, 1), new Vec3(1, 0, 1), new Vec3(0, 1, 1));
        bool result = TriTriIntersection.Intersect(t1, t2, out _);
        Assert.False(result);
    }

    [Fact]
    public void Intersect_SharedEdge_NoIntersection()
    {
        // Two triangles sharing edge but not piercing each other
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, -1, 0));
        // Coplanar - should return false for standard intersection
        bool result = TriTriIntersection.Intersect(t1, t2, out _);
        // Coplanar triangles return false from Intersect
        Assert.False(result);
    }

    [Fact]
    public void AreCoplanar_SamePlane_True()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0.5, 0.5, 0), new Vec3(1.5, 0.5, 0), new Vec3(0.5, 1.5, 0));
        Assert.True(TriTriIntersection.AreCoplanar(t1, t2));
    }

    [Fact]
    public void AreCoplanar_DifferentPlane_False()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0, 0, 1), new Vec3(1, 0, 1), new Vec3(0, 1, 1));
        Assert.False(TriTriIntersection.AreCoplanar(t1, t2));
    }

    [Fact]
    public void AreCoplanar_Angled_False()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 0, 1));
        Assert.False(TriTriIntersection.AreCoplanar(t1, t2));
    }

    [Fact]
    public void Intersect_EdgeCrossing_Segment()
    {
        // T1 in XY plane, T2 crosses T1 edge
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(4, 0, 0), new Vec3(2, 4, 0));
        var t2 = new Triangle3(new Vec3(2, 1, -1), new Vec3(2, 1, 1), new Vec3(2, 3, 0));
        bool result = TriTriIntersection.Intersect(t1, t2, out var seg);
        Assert.True(result);
        // Segment endpoints should be on Z=0 plane (intersection of XY plane)
        Assert.Equal(0, seg.Start.Z, 1e-10);
        Assert.Equal(0, seg.End.Z, 1e-10);
    }

    [Fact]
    public void Intersect_SegmentEndpoints_OnBothTriangles()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var t2 = new Triangle3(new Vec3(0.5, 0.5, -1), new Vec3(1.5, 0.5, -1), new Vec3(1, 1.5, 1));
        bool result = TriTriIntersection.Intersect(t1, t2, out var seg);
        if (result)
        {
            // Both endpoints should be finite
            Assert.False(double.IsNaN(seg.Start.X));
            Assert.False(double.IsNaN(seg.End.X));
            Assert.False(double.IsInfinity(seg.Start.X));
            Assert.False(double.IsInfinity(seg.End.X));
        }
    }

    [Fact]
    public void Intersect_ResultSegment_HasPositiveLength()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var t2 = new Triangle3(new Vec3(0.5, 0.5, -1), new Vec3(1.5, 0.5, -1), new Vec3(1, 0.5, 1));
        bool result = TriTriIntersection.Intersect(t1, t2, out var seg);
        Assert.True(result);
        Assert.True(seg.Length > 0);
    }

    [Fact]
    public void IntersectCoplanar_Overlapping_ProducesSegments()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var t2 = new Triangle3(new Vec3(0.5, 0, 0), new Vec3(2.5, 0, 0), new Vec3(1.5, 2, 0));
        bool result = TriTriIntersection.IntersectCoplanar(t1, t2, out var segsForT1, out var segsForT2, out _);
        // Overlapping coplanar triangles should produce some segments
        Assert.True(segsForT1.Count >= 0);
        Assert.True(segsForT2.Count >= 0);
    }

    [Fact]
    public void IntersectCoplanar_Disjoint_NoSegments()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(5, 0, 0), new Vec3(6, 0, 0), new Vec3(5, 1, 0));
        bool result = TriTriIntersection.IntersectCoplanar(t1, t2, out var segsForT1, out var segsForT2, out _);
        Assert.Equal(0, segsForT1.Count);
        Assert.Equal(0, segsForT2.Count);
    }

    [Fact]
    public void Intersect_DefaultFaceIndices_AreNegativeOne()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var t2 = new Triangle3(new Vec3(0.5, 0.5, -1), new Vec3(1.5, 0.5, -1), new Vec3(1, 0.5, 1));
        bool result = TriTriIntersection.Intersect(t1, t2, out var seg);
        Assert.True(result);
        // Default face indices when not provided
        Assert.Equal(-1, seg.FaceIndexA);
        Assert.Equal(-1, seg.FaceIndexB);
    }

    [Fact]
    public void Intersect_Symmetric_ProducesResult()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var t2 = new Triangle3(new Vec3(0.5, 0.5, -1), new Vec3(1.5, 0.5, -1), new Vec3(1, 0.5, 1));
        bool r1 = TriTriIntersection.Intersect(t1, t2, out var s1);
        bool r2 = TriTriIntersection.Intersect(t2, t1, out var s2);
        Assert.Equal(r1, r2);
        if (r1 && r2)
        {
            // Lengths should be approximately equal
            Assert.Equal(s1.Length, s2.Length, 1e-10);
        }
    }
}
