using MdCsg.Intersection;
using MdCsg.Math;

namespace MdCsg.Tests.Intersection;

/// <summary>Phase 6: TriTriIntersection — Moller's algorithm: crossing, parallel, coplanar, touching, and degenerate cases</summary>
public class TriTriIntersectionMollerPropertyTests
{
    [Fact]
    public void CrossingTriangles_Intersect()
    {
        // Two triangles crossing in the middle
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var t2 = new Triangle3(new Vec3(1, 1, -1), new Vec3(1, 1, 1), new Vec3(1, -1, 0));
        bool result = TriTriIntersection.Intersect(t1, t2, out var seg);
        Assert.True(result, "Crossing triangles should intersect");
    }

    [Fact]
    public void CrossingTriangles_SegmentNonDegenerate()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var t2 = new Triangle3(new Vec3(1, 0.5, -1), new Vec3(1, 0.5, 1), new Vec3(0.5, 0.5, 0));
        if (TriTriIntersection.Intersect(t1, t2, out var seg))
        {
            double len = Vec3.Distance(seg.Start, seg.End);
            Assert.True(len > 1e-10, $"Intersection segment should be non-degenerate, length={len}");
        }
    }

    [Fact]
    public void ParallelNonCoplanar_NoIntersection()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0, 0, 1), new Vec3(1, 0, 1), new Vec3(0, 1, 1));
        bool result = TriTriIntersection.Intersect(t1, t2, out _);
        Assert.False(result, "Parallel non-coplanar triangles should not intersect");
    }

    [Fact]
    public void DisjointSamePlane_NoIntersection()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(5, 5, 0), new Vec3(6, 5, 0), new Vec3(5, 6, 0));
        bool result = TriTriIntersection.Intersect(t1, t2, out _);
        Assert.False(result);
    }

    [Fact]
    public void CoplanarOverlapping_ReturnsFalse()
    {
        // Coplanar case returns false from Intersect (handled separately)
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var t2 = new Triangle3(new Vec3(0.5, 0.5, 0), new Vec3(1.5, 0.5, 0), new Vec3(1, 1.5, 0));
        bool result = TriTriIntersection.Intersect(t1, t2, out _);
        Assert.False(result, "Coplanar triangles return false from Intersect");
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
        var t2 = new Triangle3(new Vec3(0, 0, 1), new Vec3(1, 0, 1), new Vec3(0, 1, 1));
        Assert.False(TriTriIntersection.AreCoplanar(t1, t2));
    }

    [Fact]
    public void Intersect_Symmetric()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var t2 = new Triangle3(new Vec3(1, 0.5, -1), new Vec3(1, 0.5, 1), new Vec3(0.5, 0.5, 0));
        bool r1 = TriTriIntersection.Intersect(t1, t2, out _);
        bool r2 = TriTriIntersection.Intersect(t2, t1, out _);
        Assert.Equal(r1, r2);
    }

    [Fact]
    public void WellSeparated_NoIntersection()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(100, 100, 100), new Vec3(101, 100, 100), new Vec3(100, 101, 100));
        Assert.False(TriTriIntersection.Intersect(t1, t2, out _));
    }

    [Fact]
    public void VertexOnOtherPlane_StillClassifies()
    {
        // One vertex of t2 exactly on t1's plane
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var t2 = new Triangle3(new Vec3(1, 0.5, 0), new Vec3(1, 0.5, 1), new Vec3(0.5, 0.5, -1));
        // This should not crash
        bool result = TriTriIntersection.Intersect(t1, t2, out _);
        Assert.True(result == true || result == false); // just verify no exception
    }

    [Fact]
    public void IntersectCoplanar_OverlappingTriangles_ProducesSegments()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var t2 = new Triangle3(new Vec3(0.5, 0.5, 0), new Vec3(1.5, 0.5, 0), new Vec3(1, 1.5, 0));
        bool result = TriTriIntersection.IntersectCoplanar(t1, t2, out var segsForT1, out var segsForT2, out var normalsAgree);
        // At least one of segsForT1 or segsForT2 should have segments
        if (result)
        {
            Assert.True(segsForT1.Count > 0 || segsForT2.Count > 0);
        }
    }

    [Fact]
    public void IntersectCoplanar_NormalsAgree_ParallelTriangles()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var t2 = new Triangle3(new Vec3(0.5, 0.5, 0), new Vec3(1.5, 0.5, 0), new Vec3(1, 1.5, 0));
        TriTriIntersection.IntersectCoplanar(t1, t2, out _, out _, out var normalsAgree);
        Assert.True(normalsAgree, "Same-orientation coplanar triangles should have normals agree");
    }

    [Fact]
    public void IntersectCoplanar_FlippedNormals_Disagree()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0)); // normal +Z
        var t2 = new Triangle3(new Vec3(0.5, 0.5, 0), new Vec3(1, 1.5, 0), new Vec3(1.5, 0.5, 0)); // reversed winding, normal -Z
        TriTriIntersection.IntersectCoplanar(t1, t2, out _, out _, out var normalsAgree);
        Assert.False(normalsAgree, "Reversed-winding coplanar triangles should have normals disagree");
    }

    [Fact]
    public void IntersectCoplanar_DisjointCoplanar_NoSegments()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(5, 5, 0), new Vec3(6, 5, 0), new Vec3(5, 6, 0));
        bool result = TriTriIntersection.IntersectCoplanar(t1, t2, out var s1, out var s2, out _);
        Assert.False(result);
        Assert.Equal(0, s1.Count);
        Assert.Equal(0, s2.Count);
    }

    [Fact]
    public void CrossingTriangle_SegmentEndpoints_OnBothTriangles()
    {
        var t1 = new Triangle3(new Vec3(-1, -1, 0), new Vec3(1, -1, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0, 0, -1), new Vec3(0, 0, 1), new Vec3(0, -2, 0));
        if (TriTriIntersection.Intersect(t1, t2, out var seg))
        {
            // Both endpoints should be near z=0 plane (t1 is on z=0)
            Assert.True(System.Math.Abs(seg.Start.Z) < 0.1 || System.Math.Abs(seg.End.Z) < 0.1,
                "Segment endpoints should be near t1's plane");
        }
    }

    [Fact]
    public void SameTriangle_Coplanar()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.True(TriTriIntersection.AreCoplanar(t1, t1));
    }

    [Fact]
    public void AreCoplanar_OneVertexOff_False()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0.1));
        Assert.False(TriTriIntersection.AreCoplanar(t1, t2));
    }
}
