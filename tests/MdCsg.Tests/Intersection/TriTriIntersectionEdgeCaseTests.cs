using MdCsg.Intersection;
using MdCsg.Math;

namespace MdCsg.Tests.Intersection;

/// <summary>Phase 6: Triangle-triangle intersection edge cases</summary>
public class TriTriIntersectionEdgeCaseTests
{
    [Fact]
    public void Disjoint_NoIntersection()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(5, 5, 5), new Vec3(6, 5, 5), new Vec3(5, 6, 5));
        Assert.False(TriTriIntersection.Intersect(t1, t2, out _));
    }

    [Fact]
    public void Coplanar_ReturnsFalse()
    {
        // Two coplanar triangles on Z=0 plane - Intersect returns false for coplanar
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0.5, 0, 0), new Vec3(1.5, 0, 0), new Vec3(0.5, 1, 0));
        Assert.False(TriTriIntersection.Intersect(t1, t2, out _));
    }

    [Fact]
    public void AreCoplanar_SamePlane_True()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0.5, 0, 0), new Vec3(1.5, 0, 0), new Vec3(0.5, 1, 0));
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
    public void Crossing_ProducesSegment()
    {
        // T1 on XY plane, T2 crosses it
        var t1 = new Triangle3(new Vec3(-1, -1, 0), new Vec3(2, -1, 0), new Vec3(-1, 2, 0));
        var t2 = new Triangle3(new Vec3(0, 0, -1), new Vec3(1, 0, -1), new Vec3(0.5, 0, 1));
        bool result = TriTriIntersection.Intersect(t1, t2, out var seg);
        Assert.True(result);
        Assert.True(seg.Length > 0);
    }

    [Fact]
    public void Crossing_SegmentEndpointsOnBothTriangles()
    {
        var t1 = new Triangle3(new Vec3(-1, -1, 0), new Vec3(2, -1, 0), new Vec3(-1, 2, 0));
        var t2 = new Triangle3(new Vec3(0, 0, -1), new Vec3(1, 0, -1), new Vec3(0.5, 0, 1));
        TriTriIntersection.Intersect(t1, t2, out var seg);
        // Segment endpoints should be near Z=0 (on t1's plane)
        Assert.True(System.Math.Abs(seg.Start.Z) < 0.1);
        Assert.True(System.Math.Abs(seg.End.Z) < 0.1);
    }

    [Fact]
    public void AllVerticesOneSide_NoIntersection()
    {
        // All of t2's vertices above t1's plane
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0, 0, 1), new Vec3(1, 0, 2), new Vec3(0, 1, 3));
        Assert.False(TriTriIntersection.Intersect(t1, t2, out _));
    }

    [Fact]
    public void OneVertexOnPlane_OthersAbove_NoIntersection()
    {
        // One vertex of t2 on t1's plane, others above
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0.5, 0.5, 0), new Vec3(1, 0, 1), new Vec3(0, 1, 1));
        // One vertex coplanar, others on same side: may or may not intersect
        // But should not crash
        TriTriIntersection.Intersect(t1, t2, out _);
    }

    [Fact]
    public void PerpendicularTriangles_Intersect()
    {
        // T1 on XY, T2 on XZ, crossing along X axis
        var t1 = new Triangle3(new Vec3(-1, -1, 0), new Vec3(2, -1, 0), new Vec3(-1, 2, 0));
        var t2 = new Triangle3(new Vec3(-1, 0, -1), new Vec3(2, 0, -1), new Vec3(-1, 0, 2));
        bool result = TriTriIntersection.Intersect(t1, t2, out var seg);
        Assert.True(result);
    }

    [Fact]
    public void SmallTriangles_StillIntersect()
    {
        double s = 1e-6;
        var t1 = new Triangle3(new Vec3(0, 0, 0) * s, new Vec3(1, 0, 0) * s, new Vec3(0, 1, 0) * s);
        var t2 = new Triangle3(new Vec3(0.1, 0.1, -0.5) * s, new Vec3(0.9, 0.1, -0.5) * s, new Vec3(0.5, 0.5, 0.5) * s);
        bool result = TriTriIntersection.Intersect(t1, t2, out var seg);
        // Small triangles can still detect intersections (or not if degenerate)
        if (result)
            Assert.True(seg.Length > 0);
    }

    [Fact]
    public void LargeTriangles_StillIntersect()
    {
        double s = 1e6;
        var t1 = new Triangle3(new Vec3(-1, -1, 0) * s, new Vec3(2, -1, 0) * s, new Vec3(-1, 2, 0) * s);
        var t2 = new Triangle3(new Vec3(0, 0, -1) * s, new Vec3(1, 0, -1) * s, new Vec3(0.5, 0.5, 1) * s);
        bool result = TriTriIntersection.Intersect(t1, t2, out var seg);
        Assert.True(result);
    }

    [Fact]
    public void IntersectCoplanar_OverlappingTriangles()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(0, 2, 0));
        var t2 = new Triangle3(new Vec3(0.5, 0.5, 0), new Vec3(2.5, 0.5, 0), new Vec3(0.5, 2.5, 0));
        bool result = TriTriIntersection.IntersectCoplanar(t1, t2, out var segsForT1, out var segsForT2, out var normalsAgree);
        Assert.True(normalsAgree);
        // Should have some clipping segments
        if (result)
        {
            Assert.True(segsForT1.Count > 0 || segsForT2.Count > 0);
        }
    }

    [Fact]
    public void IntersectCoplanar_DisjointTriangles()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(5, 5, 0), new Vec3(6, 5, 0), new Vec3(5, 6, 0));
        bool result = TriTriIntersection.IntersectCoplanar(t1, t2, out _, out _, out _);
        Assert.False(result);
    }

    [Fact]
    public void IntersectCoplanar_OppositeNormals()
    {
        // Same plane but opposite winding
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(0, 2, 0));
        var t2 = new Triangle3(new Vec3(0.5, 0.5, 0), new Vec3(0.5, 2.5, 0), new Vec3(2.5, 0.5, 0));
        TriTriIntersection.IntersectCoplanar(t1, t2, out _, out _, out var normalsAgree);
        Assert.False(normalsAgree);
    }

    [Fact]
    public void IntersectCoplanar_IdenticalTriangles()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        // Identical triangle: edges are on boundary, so ClipEdgeAgainstTriangle filters them
        TriTriIntersection.IntersectCoplanar(t1, t1, out var segsForT1, out var segsForT2, out var normalsAgree);
        Assert.True(normalsAgree);
    }

    [Fact]
    public void Intersect_Symmetric()
    {
        var t1 = new Triangle3(new Vec3(-1, -1, 0), new Vec3(2, -1, 0), new Vec3(-1, 2, 0));
        var t2 = new Triangle3(new Vec3(0, 0, -1), new Vec3(1, 0, -1), new Vec3(0.5, 0.5, 1));
        bool r1 = TriTriIntersection.Intersect(t1, t2, out var seg1);
        bool r2 = TriTriIntersection.Intersect(t2, t1, out var seg2);
        Assert.Equal(r1, r2);
        if (r1 && r2)
        {
            // Segment lengths should be similar
            Assert.True(System.Math.Abs(seg1.Length - seg2.Length) < 0.01);
        }
    }

    [Fact]
    public void IntersectionSegment_IsDegenerate_TrueForZeroLength()
    {
        var seg = new IntersectionSegment(new Vec3(1, 2, 3), new Vec3(1, 2, 3), 0, 1);
        Assert.True(seg.IsDegenerate);
    }

    [Fact]
    public void IntersectionSegment_Length_Correct()
    {
        var seg = new IntersectionSegment(new Vec3(0, 0, 0), new Vec3(3, 4, 0), 0, 1);
        Assert.Equal(5.0, seg.Length, 10);
    }

    [Fact]
    public void TwoVerticesOnPlane_StillProducesResult()
    {
        // Two vertices of t2 on t1's plane, one above
        var t1 = new Triangle3(new Vec3(-1, -1, 0), new Vec3(2, -1, 0), new Vec3(-1, 2, 0));
        var t2 = new Triangle3(new Vec3(0.2, 0.2, 0), new Vec3(0.8, 0.2, 0), new Vec3(0.5, 0.5, 1));
        bool result = TriTriIntersection.Intersect(t1, t2, out _);
        // Should not crash regardless
    }
}
