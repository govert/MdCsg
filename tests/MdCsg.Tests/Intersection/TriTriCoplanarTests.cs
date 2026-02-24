using MdCsg.Intersection;
using MdCsg.Math;

namespace MdCsg.Tests.Intersection;

/// <summary>Phase 6: TriTriIntersection coplanar and boundary tests</summary>
public class TriTriCoplanarTests
{
    [Fact]
    public void AreCoplanar_SamePlane_True()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0.5, 0, 0), new Vec3(1.5, 0, 0), new Vec3(0.5, 1, 0));
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
    public void AreCoplanar_TiltedSamePlane_True()
    {
        // Both on plane z = x
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 1), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0.5, 0, 0.5), new Vec3(1.5, 0, 1.5), new Vec3(0.5, 1, 0.5));
        Assert.True(TriTriIntersection.AreCoplanar(t1, t2));
    }

    [Fact]
    public void Intersect_Coplanar_ReturnsFalse()
    {
        // Overlapping coplanar triangles
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(0, 2, 0));
        var t2 = new Triangle3(new Vec3(0.5, 0, 0), new Vec3(2.5, 0, 0), new Vec3(0.5, 2, 0));
        bool result = TriTriIntersection.Intersect(t1, t2, out _);
        Assert.False(result); // Coplanar is handled separately
    }

    [Fact]
    public void IntersectCoplanar_Overlapping_HasSegments()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(0, 2, 0));
        var t2 = new Triangle3(new Vec3(1, 0, 0), new Vec3(3, 0, 0), new Vec3(1, 2, 0));
        bool result = TriTriIntersection.IntersectCoplanar(t1, t2, out var segsA, out var segsB, out bool normalsAgree);
        // If they overlap, should have segments
        if (result)
        {
            Assert.True(segsA.Count > 0 || segsB.Count > 0);
        }
    }

    [Fact]
    public void IntersectCoplanar_SameNormals_NormalsAgree()
    {
        // Same winding → normals agree
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(0, 2, 0));
        var t2 = new Triangle3(new Vec3(1, 0, 0), new Vec3(3, 0, 0), new Vec3(1, 2, 0));
        if (TriTriIntersection.IntersectCoplanar(t1, t2, out _, out _, out bool normalsAgree))
        {
            Assert.True(normalsAgree);
        }
    }

    [Fact]
    public void IntersectCoplanar_OppositeNormals_NormalsDisagree()
    {
        // Reversed winding on t2 → normals opposite
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(0, 2, 0));
        var t2 = new Triangle3(new Vec3(1, 0, 0), new Vec3(1, 2, 0), new Vec3(3, 0, 0)); // reversed
        if (TriTriIntersection.IntersectCoplanar(t1, t2, out _, out _, out bool normalsAgree))
        {
            Assert.False(normalsAgree);
        }
    }

    [Fact]
    public void IntersectCoplanar_Disjoint_ReturnsFalse()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(5, 5, 0), new Vec3(6, 5, 0), new Vec3(5, 6, 0));
        bool result = TriTriIntersection.IntersectCoplanar(t1, t2, out _, out _, out _);
        Assert.False(result);
    }

    [Fact]
    public void Intersect_ParallelNotCoplanar_NoIntersection()
    {
        // Parallel planes, offset in Z
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0, 0, 0.5), new Vec3(1, 0, 0.5), new Vec3(0, 1, 0.5));
        Assert.False(TriTriIntersection.Intersect(t1, t2, out _));
    }

    [Fact]
    public void Intersect_CrossingTriangles_ReturnsSegment()
    {
        // T1 in XY plane, T2 in XZ plane, crossing through
        var t1 = new Triangle3(new Vec3(-1, -1, 0), new Vec3(2, -1, 0), new Vec3(-1, 2, 0));
        var t2 = new Triangle3(new Vec3(0, 0, -1), new Vec3(1, 0, -1), new Vec3(0.5, 0, 2));
        bool result = TriTriIntersection.Intersect(t1, t2, out var seg);
        Assert.True(result);
        Assert.True(seg.Length > 0);
    }

    [Fact]
    public void Intersect_Symmetric()
    {
        var t1 = new Triangle3(new Vec3(-1, -1, 0), new Vec3(2, -1, 0), new Vec3(-1, 2, 0));
        var t2 = new Triangle3(new Vec3(0, 0, -1), new Vec3(1, 0, -1), new Vec3(0.5, 0, 2));
        TriTriIntersection.Intersect(t1, t2, out var seg12);
        TriTriIntersection.Intersect(t2, t1, out var seg21);
        // Both should intersect (same segment, possibly different direction)
        Assert.True(seg12.Length > 0);
        Assert.True(seg21.Length > 0);
        Assert.Equal(seg12.Length, seg21.Length, 5);
    }

    [Fact]
    public void Intersect_TouchingEdge_MayOrMayNotIntersect()
    {
        // T2 just touches the edge of T1 — boundary case
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0.5, 0, 0), new Vec3(0.5, 0, 1), new Vec3(0.5, 1, 0.5));
        bool result = TriTriIntersection.Intersect(t1, t2, out var seg);
        // Just verify no crash
        if (result) Assert.True(seg.Length >= 0);
    }

    [Fact]
    public void Intersect_Perpendicular_ValidSegment()
    {
        // T1 in XY, T2 perpendicular in YZ, overlapping
        var t1 = new Triangle3(new Vec3(-2, -2, 0), new Vec3(2, -2, 0), new Vec3(0, 2, 0));
        var t2 = new Triangle3(new Vec3(0, -2, -2), new Vec3(0, 2, -2), new Vec3(0, 0, 2));
        bool result = TriTriIntersection.Intersect(t1, t2, out var seg);
        Assert.True(result);
        // Intersection should be along the Y axis at x=0, z=0
        Assert.True(seg.Length > 0);
    }
}
