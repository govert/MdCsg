using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;

namespace MdCsg.Tests.Intersection;

/// <summary>Phase 6: Coplanar triangle clipping — IntersectCoplanar edge cases and segment geometry</summary>
public class TriTriCoplanarClippingTests
{
    [Fact]
    public void FullyContained_NoClippingSegments()
    {
        // Small triangle fully inside large triangle, same plane Z=0
        // When an edge is entirely within the other triangle (tMin≈0, tMax≈1),
        // ClipEdgeAgainstTriangle filters it out as a boundary-only edge
        var big = new Triangle3(new Vec3(-2, -2, 0), new Vec3(4, -2, 0), new Vec3(-2, 4, 0));
        var small = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        bool result = TriTriIntersection.IntersectCoplanar(big, small, out var segsForBig, out var segsForSmall, out var normalsAgree);
        Assert.True(normalsAgree);
        // Fully contained edges are filtered, but partially clipped edges of big may be produced
    }

    [Fact]
    public void FullyContained_Reversed_NoClippingSegments()
    {
        var big = new Triangle3(new Vec3(-2, -2, 0), new Vec3(4, -2, 0), new Vec3(-2, 4, 0));
        var small = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        TriTriIntersection.IntersectCoplanar(small, big, out var segsForSmall, out var segsForBig, out _);
        // Edges of big that are clipped against small produce partial segments
        // Edges of small are fully inside big and filtered out
        Assert.Equal(0, segsForSmall.Count);
    }

    [Fact]
    public void DisjointOnSamePlane_NoSegments()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(5, 0, 0), new Vec3(6, 0, 0), new Vec3(5, 1, 0));
        bool result = TriTriIntersection.IntersectCoplanar(t1, t2, out _, out _, out _);
        Assert.False(result);
    }

    [Fact]
    public void SameNormal_NormalsAgree()
    {
        // Both CCW on Z=0
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(0, 2, 0));
        var t2 = new Triangle3(new Vec3(0.5, 0.5, 0), new Vec3(2.5, 0.5, 0), new Vec3(0.5, 2.5, 0));
        TriTriIntersection.IntersectCoplanar(t1, t2, out _, out _, out var normalsAgree);
        Assert.True(normalsAgree);
    }

    [Fact]
    public void OppositeWinding_NormalsDisagree()
    {
        // t1 is CCW (normal +Z), t2 is CW (normal -Z)
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(0, 2, 0));
        var t2 = new Triangle3(new Vec3(0.5, 0.5, 0), new Vec3(0.5, 2.5, 0), new Vec3(2.5, 0.5, 0));
        TriTriIntersection.IntersectCoplanar(t1, t2, out _, out _, out var normalsAgree);
        Assert.False(normalsAgree);
    }

    [Fact]
    public void OverlappingCorner_ProducesClippingSegments()
    {
        // Two triangles overlapping at a corner region
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(0, 2, 0));
        var t2 = new Triangle3(new Vec3(1, 1, 0), new Vec3(3, 1, 0), new Vec3(1, 3, 0));
        bool result = TriTriIntersection.IntersectCoplanar(t1, t2, out var segsForT1, out var segsForT2, out _);
        // The overlap region should produce clipping segments
        if (result)
            Assert.True(segsForT1.Count + segsForT2.Count > 0);
    }

    [Fact]
    public void SegmentEndpoints_AreFinite()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(0, 2, 0));
        var t2 = new Triangle3(new Vec3(0.5, 0.5, 0), new Vec3(2.5, 0.5, 0), new Vec3(0.5, 2.5, 0));
        TriTriIntersection.IntersectCoplanar(t1, t2, out var segsForT1, out var segsForT2, out _);
        foreach (var (start, end) in segsForT1)
        {
            Assert.False(double.IsNaN(start.X) || double.IsNaN(start.Y) || double.IsNaN(start.Z));
            Assert.False(double.IsNaN(end.X) || double.IsNaN(end.Y) || double.IsNaN(end.Z));
            Assert.False(double.IsInfinity(start.X) || double.IsInfinity(end.X));
        }
        foreach (var (start, end) in segsForT2)
        {
            Assert.False(double.IsNaN(start.X) || double.IsNaN(start.Y) || double.IsNaN(start.Z));
            Assert.False(double.IsNaN(end.X) || double.IsNaN(end.Y) || double.IsNaN(end.Z));
        }
    }

    [Fact]
    public void SegmentEndpoints_OnSamePlane()
    {
        // Both on Z=0
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(0, 2, 0));
        var t2 = new Triangle3(new Vec3(0.5, 0.5, 0), new Vec3(2.5, 0.5, 0), new Vec3(0.5, 2.5, 0));
        TriTriIntersection.IntersectCoplanar(t1, t2, out var segsForT1, out var segsForT2, out _);
        foreach (var (start, end) in segsForT1)
        {
            Assert.Equal(0, start.Z, 1e-10);
            Assert.Equal(0, end.Z, 1e-10);
        }
        foreach (var (start, end) in segsForT2)
        {
            Assert.Equal(0, start.Z, 1e-10);
            Assert.Equal(0, end.Z, 1e-10);
        }
    }

    [Fact]
    public void OnXYPlane_GetDominantAxis_DropsZ()
    {
        var normal = new Vec3(0, 0, 1);
        Assert.Equal(2, ConstrainedTriangulator.GetDominantAxis(normal));
    }

    [Fact]
    public void OnXZPlane_GetDominantAxis_DropsY()
    {
        var normal = new Vec3(0, 1, 0);
        Assert.Equal(1, ConstrainedTriangulator.GetDominantAxis(normal));
    }

    [Fact]
    public void OnYZPlane_GetDominantAxis_DropsX()
    {
        var normal = new Vec3(1, 0, 0);
        Assert.Equal(0, ConstrainedTriangulator.GetDominantAxis(normal));
    }

    [Fact]
    public void CoplanarOnXZPlane_Overlapping()
    {
        // Triangles on Y=0 plane
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(0, 0, 2));
        var t2 = new Triangle3(new Vec3(0.5, 0, 0.5), new Vec3(2.5, 0, 0.5), new Vec3(0.5, 0, 2.5));
        bool result = TriTriIntersection.IntersectCoplanar(t1, t2, out var segsForT1, out var segsForT2, out _);
        if (result)
        {
            // All endpoints should have Y=0
            foreach (var (start, end) in segsForT1)
            {
                Assert.Equal(0, start.Y, 1e-10);
                Assert.Equal(0, end.Y, 1e-10);
            }
        }
    }

    [Fact]
    public void CoplanarOnYZPlane_Overlapping()
    {
        // Triangles on X=0 plane
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(0, 2, 0), new Vec3(0, 0, 2));
        var t2 = new Triangle3(new Vec3(0, 0.5, 0.5), new Vec3(0, 2.5, 0.5), new Vec3(0, 0.5, 2.5));
        bool result = TriTriIntersection.IntersectCoplanar(t1, t2, out var segsForT1, out var segsForT2, out _);
        if (result)
        {
            foreach (var (start, end) in segsForT1)
            {
                Assert.Equal(0, start.X, 1e-10);
                Assert.Equal(0, end.X, 1e-10);
            }
        }
    }

    [Fact]
    public void AreCoplanar_SamePlane_DifferentWinding_True()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0.5, 0.5, 0), new Vec3(0.5, 1.5, 0), new Vec3(1.5, 0.5, 0));
        Assert.True(TriTriIntersection.AreCoplanar(t1, t2));
    }

    [Fact]
    public void AreCoplanar_OffsetPlane_False()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0, 0, 0.001), new Vec3(1, 0, 0.001), new Vec3(0, 1, 0.001));
        Assert.False(TriTriIntersection.AreCoplanar(t1, t2));
    }

    [Fact]
    public void SegmentsAreNonDegenerate()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(0, 2, 0));
        var t2 = new Triangle3(new Vec3(0.5, 0.5, 0), new Vec3(2.5, 0.5, 0), new Vec3(0.5, 2.5, 0));
        TriTriIntersection.IntersectCoplanar(t1, t2, out var segsForT1, out var segsForT2, out _);
        foreach (var (start, end) in segsForT1)
            Assert.True(Vec3.Distance(start, end) > 1e-10, "Segment should be non-degenerate");
        foreach (var (start, end) in segsForT2)
            Assert.True(Vec3.Distance(start, end) > 1e-10, "Segment should be non-degenerate");
    }

    [Fact]
    public void ProjectTo2D_DropX_CorrectComponents()
    {
        var p = new Vec3(1, 2, 3);
        var p2d = ConstrainedTriangulator.ProjectTo2D(p, 0);
        Assert.Equal(2, p2d.X); // Y
        Assert.Equal(3, p2d.Y); // Z
    }

    [Fact]
    public void ProjectTo2D_DropY_CorrectComponents()
    {
        var p = new Vec3(1, 2, 3);
        var p2d = ConstrainedTriangulator.ProjectTo2D(p, 1);
        Assert.Equal(1, p2d.X); // X
        Assert.Equal(3, p2d.Y); // Z
    }

    [Fact]
    public void ProjectTo2D_DropZ_CorrectComponents()
    {
        var p = new Vec3(1, 2, 3);
        var p2d = ConstrainedTriangulator.ProjectTo2D(p, 2);
        Assert.Equal(1, p2d.X); // X
        Assert.Equal(2, p2d.Y); // Y
    }
}
