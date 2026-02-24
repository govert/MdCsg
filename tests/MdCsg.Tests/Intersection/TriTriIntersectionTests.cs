using MdCsg.Intersection;
using MdCsg.Math;

namespace MdCsg.Tests.Intersection;

public class TriTriIntersectionTests
{
    [Fact]
    public void OverlappingTriangles_ReturnSegment()
    {
        // Two triangles that cross each other
        var t1 = new Triangle3(
            new Vec3(0, 0, 0),
            new Vec3(2, 0, 0),
            new Vec3(1, 2, 0));

        var t2 = new Triangle3(
            new Vec3(1, 1, -1),
            new Vec3(1, 1, 1),
            new Vec3(1, -1, 0));

        bool result = TriTriIntersection.Intersect(t1, t2, out var seg);
        Assert.True(result);
        Assert.True(seg.Length > 0);
    }

    [Fact]
    public void DisjointTriangles_ReturnFalse()
    {
        var t1 = new Triangle3(
            new Vec3(0, 0, 0),
            new Vec3(1, 0, 0),
            new Vec3(0, 1, 0));

        var t2 = new Triangle3(
            new Vec3(5, 5, 5),
            new Vec3(6, 5, 5),
            new Vec3(5, 6, 5));

        bool result = TriTriIntersection.Intersect(t1, t2, out _);
        Assert.False(result);
    }

    [Fact]
    public void CoplanarTriangles_ReturnFalse()
    {
        // Same plane, overlapping area
        var t1 = new Triangle3(
            new Vec3(0, 0, 0),
            new Vec3(1, 0, 0),
            new Vec3(0, 1, 0));

        var t2 = new Triangle3(
            new Vec3(0.5, 0, 0),
            new Vec3(1.5, 0, 0),
            new Vec3(0.5, 1, 0));

        bool result = TriTriIntersection.Intersect(t1, t2, out _);
        Assert.False(result); // coplanar is handled as degenerate
    }

    [Fact]
    public void ParallelNonCoplanar_ReturnFalse()
    {
        var t1 = new Triangle3(
            new Vec3(0, 0, 0),
            new Vec3(1, 0, 0),
            new Vec3(0, 1, 0));

        var t2 = new Triangle3(
            new Vec3(0, 0, 1),
            new Vec3(1, 0, 1),
            new Vec3(0, 1, 1));

        bool result = TriTriIntersection.Intersect(t1, t2, out _);
        Assert.False(result);
    }

    [Fact]
    public void PerpendicularCrossing_ReturnSegment()
    {
        // Triangle in XY plane
        var t1 = new Triangle3(
            new Vec3(-1, -1, 0),
            new Vec3(1, -1, 0),
            new Vec3(0, 1, 0));

        // Triangle in XZ plane crossing through
        var t2 = new Triangle3(
            new Vec3(-1, 0, -1),
            new Vec3(1, 0, -1),
            new Vec3(0, 0, 1));

        bool result = TriTriIntersection.Intersect(t1, t2, out var seg);
        Assert.True(result);
        // Intersection should be a segment along the X axis at y=0, z=0
        Assert.Equal(0, seg.Start.Z, 1e-8);
        Assert.Equal(0, seg.End.Z, 1e-8);
    }
}
