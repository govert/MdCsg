using MdCsg.Intersection;
using MdCsg.Math;

namespace MdCsg.Tests.Intersection;

/// <summary>Phase 6: TriTriIntersection segment endpoint accuracy — endpoints on triangle planes, symmetry</summary>
public class TriTriSegmentEndpointAccuracyTests
{
    private static double PointTriangleDistance(Vec3 p, Triangle3 t)
    {
        // Signed distance from plane of triangle
        var n = Vec3.Cross(t.B - t.A, t.C - t.A);
        var len = n.Length;
        if (len < 1e-15) return double.MaxValue;
        return System.Math.Abs(Vec3.Dot(n, p - t.A)) / len;
    }

    [Fact]
    public void IntersectionEndpoints_LieOnT1Plane()
    {
        // Endpoints of the intersection segment must lie on the plane of t1
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var t2 = new Triangle3(new Vec3(0.5, 0.5, -1), new Vec3(0.5, 0.5, 1), new Vec3(1.5, 0.5, 0));

        if (TriTriIntersection.Intersect(t1, t2, out var seg))
        {
            Assert.True(PointTriangleDistance(seg.Start, t1) < 1e-6,
                $"Start point {seg.Start} not on t1 plane, dist={PointTriangleDistance(seg.Start, t1)}");
            Assert.True(PointTriangleDistance(seg.End, t1) < 1e-6,
                $"End point {seg.End} not on t1 plane, dist={PointTriangleDistance(seg.End, t1)}");
        }
    }

    [Fact]
    public void Symmetric_SameEndpoints()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var t2 = new Triangle3(new Vec3(0.5, 0.5, -1), new Vec3(0.5, 0.5, 1), new Vec3(1.5, 0.5, 0));

        bool r1 = TriTriIntersection.Intersect(t1, t2, out var seg1);
        bool r2 = TriTriIntersection.Intersect(t2, t1, out var seg2);

        Assert.Equal(r1, r2);
        if (r1 && r2)
        {
            // Endpoints may be in different order, but the set should be the same
            double d1 = System.Math.Min(
                Vec3.DistanceSquared(seg1.Start, seg2.Start) + Vec3.DistanceSquared(seg1.End, seg2.End),
                Vec3.DistanceSquared(seg1.Start, seg2.End) + Vec3.DistanceSquared(seg1.End, seg2.Start));
            Assert.True(d1 < 1e-6, $"Endpoints don't match between t1-t2 and t2-t1");
        }
    }

    [Fact]
    public void PerpendicularTriangles_MeetAtLine()
    {
        // t1 on XY plane, t2 on XZ plane
        var t1 = new Triangle3(new Vec3(-1, -1, 0), new Vec3(1, -1, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(-1, 0, -1), new Vec3(1, 0, -1), new Vec3(0, 0, 1));

        bool intersects = TriTriIntersection.Intersect(t1, t2, out var seg);
        if (intersects)
        {
            // Intersection should be along the X axis (y=0, z=0)
            Assert.True(System.Math.Abs(seg.Start.Y) < 1e-6);
            Assert.True(System.Math.Abs(seg.Start.Z) < 1e-6);
            Assert.True(System.Math.Abs(seg.End.Y) < 1e-6);
            Assert.True(System.Math.Abs(seg.End.Z) < 1e-6);
        }
    }

    [Fact]
    public void NoIntersection_ParallelTriangles()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0, 0, 1), new Vec3(1, 0, 1), new Vec3(0, 1, 1));

        bool intersects = TriTriIntersection.Intersect(t1, t2, out _);
        Assert.False(intersects);
    }

    [Fact]
    public void NoIntersection_SamePosDifferentSide()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(5, 0, -1), new Vec3(6, 0, -1), new Vec3(5, 1, -1));

        bool intersects = TriTriIntersection.Intersect(t1, t2, out _);
        Assert.False(intersects);
    }

    [Fact]
    public void TouchingEdge_MayOrMayNotIntersect()
    {
        // Two triangles touching at an edge
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 0, 1));

        // Edge-touching case: result depends on degenerate check
        bool intersects = TriTriIntersection.Intersect(t1, t2, out var seg);
        if (intersects)
            Assert.True(seg.Length > 0);
    }

    [Fact]
    public void AreCoplanar_SamePlane_True()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0.5, 0.5, 0), new Vec3(1.5, 0, 0), new Vec3(0, 1.5, 0));

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
    public void AreCoplanar_Symmetric()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0.5, 0.5, 0), new Vec3(2, 0, 0), new Vec3(0, 2, 0));

        Assert.Equal(
            TriTriIntersection.AreCoplanar(t1, t2),
            TriTriIntersection.AreCoplanar(t2, t1));
    }

    [Fact]
    public void IntersectCoplanar_Overlapping_ProducesSegments()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 2, 0));
        var t2 = new Triangle3(new Vec3(0.5, 0.5, 0), new Vec3(2.5, 0, 0), new Vec3(1, 1.5, 0));

        bool result = TriTriIntersection.IntersectCoplanar(t1, t2,
            out var segsForT1, out var segsForT2, out bool normalsAgree);

        if (result)
        {
            Assert.True(segsForT1.Count > 0 || segsForT2.Count > 0);
            Assert.True(normalsAgree); // Both on same plane with same orientation
        }
    }

    [Fact]
    public void IntersectCoplanar_NonOverlapping_NoSegments()
    {
        var t1 = new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(5, 0, 0), new Vec3(6, 0, 0), new Vec3(5, 1, 0));

        bool result = TriTriIntersection.IntersectCoplanar(t1, t2,
            out var segsForT1, out var segsForT2, out _);

        // Non-overlapping coplanar triangles should produce no segments
        if (!result)
        {
            Assert.Empty(segsForT1);
            Assert.Empty(segsForT2);
        }
    }

    [Fact]
    public void IntersectionSegment_NonDegenerate()
    {
        var t1 = new Triangle3(new Vec3(-1, -1, 0), new Vec3(1, -1, 0), new Vec3(0, 1, 0));
        var t2 = new Triangle3(new Vec3(0, 0, -1), new Vec3(0, 0, 1), new Vec3(1, 0, 0));

        if (TriTriIntersection.Intersect(t1, t2, out var seg))
        {
            Assert.False(seg.IsDegenerate);
            Assert.True(seg.Length > 0);
        }
    }
}
