using MdCsg.Intersection;
using MdCsg.Math;

namespace MdCsg.Tests.Intersection;

/// <summary>Phase 6: IntersectionSegment edge cases — degenerate, large coords, snapping, record semantics</summary>
public class IntersectionSegmentEdgeCaseTests
{
    [Fact]
    public void IsDegenerate_ExactlyAtThreshold()
    {
        // MathUtil.Epsilon = 1e-10
        var seg = new IntersectionSegment(Vec3.Zero, new Vec3(1e-10, 0, 0), 0, 1);
        // Length = 1e-10, which is NOT < Epsilon (it equals it), so NOT degenerate
        Assert.False(seg.IsDegenerate);
    }

    [Fact]
    public void IsDegenerate_JustBelowThreshold()
    {
        var seg = new IntersectionSegment(Vec3.Zero, new Vec3(0.9e-10, 0, 0), 0, 1);
        Assert.True(seg.IsDegenerate);
    }

    [Fact]
    public void Length_Symmetric()
    {
        var start = new Vec3(1, 2, 3);
        var end = new Vec3(4, 5, 6);
        var seg1 = new IntersectionSegment(start, end, 0, 1);
        var seg2 = new IntersectionSegment(end, start, 0, 1);
        Assert.Equal(seg1.Length, seg2.Length, 1e-14);
    }

    [Fact]
    public void RecordHash_SameValues_SameHash()
    {
        var a = new IntersectionSegment(new Vec3(1, 2, 3), new Vec3(4, 5, 6), 7, 8);
        var b = new IntersectionSegment(new Vec3(1, 2, 3), new Vec3(4, 5, 6), 7, 8);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void RecordToString_NotNull()
    {
        var seg = new IntersectionSegment(Vec3.Zero, new Vec3(1, 0, 0), 0, 1);
        Assert.NotNull(seg.ToString());
        Assert.True(seg.ToString().Length > 0);
    }

    [Fact]
    public void FaceIndicesCanBeZero()
    {
        var seg = new IntersectionSegment(Vec3.Zero, new Vec3(1, 0, 0), 0, 0);
        Assert.Equal(0, seg.FaceIndexA);
        Assert.Equal(0, seg.FaceIndexB);
    }

    [Fact]
    public void FaceIndicesCanBeLarge()
    {
        var seg = new IntersectionSegment(Vec3.Zero, new Vec3(1, 0, 0), 999999, 888888);
        Assert.Equal(999999, seg.FaceIndexA);
        Assert.Equal(888888, seg.FaceIndexB);
    }

    [Fact]
    public void NegativeCoordinates_Length_Positive()
    {
        var seg = new IntersectionSegment(new Vec3(-5, -5, -5), new Vec3(-4, -5, -5), 0, 1);
        Assert.Equal(1.0, seg.Length, 1e-14);
    }

    [Fact]
    public void LargeCoordinates_NoOverflow()
    {
        var seg = new IntersectionSegment(new Vec3(1e8, 2e8, 3e8), new Vec3(1e8 + 1, 2e8, 3e8), 0, 1);
        Assert.Equal(1.0, seg.Length, 1e-6);
        Assert.False(seg.IsDegenerate);
    }

    [Fact]
    public void With_Start_CreatesNewSegment()
    {
        var seg = new IntersectionSegment(Vec3.Zero, new Vec3(1, 0, 0), 0, 1);
        var seg2 = seg with { Start = new Vec3(0.5, 0, 0) };
        Assert.Equal(new Vec3(0.5, 0, 0), seg2.Start);
        Assert.Equal(seg.End, seg2.End);
        Assert.Equal(seg.FaceIndexA, seg2.FaceIndexA);
    }

    [Fact]
    public void With_FaceIndexA_CreatesNewSegment()
    {
        var seg = new IntersectionSegment(Vec3.Zero, new Vec3(1, 0, 0), 0, 1);
        var seg2 = seg with { FaceIndexA = 42 };
        Assert.Equal(42, seg2.FaceIndexA);
        Assert.Equal(seg.Start, seg2.Start);
    }

    [Fact]
    public void SnapSegment_PreservesIndices()
    {
        var seg = new IntersectionSegment(new Vec3(0.123, 0.456, 0.789), new Vec3(0.321, 0.654, 0.987), 42, 99);
        var snapped = SnapRounding.SnapSegment(seg);
        Assert.Equal(42, snapped.FaceIndexA);
        Assert.Equal(99, snapped.FaceIndexB);
    }

    [Fact]
    public void SnapSegment_IsIdempotent()
    {
        var seg = new IntersectionSegment(new Vec3(0.1, 0.2, 0.3), new Vec3(0.4, 0.5, 0.6), 0, 1);
        var s1 = SnapRounding.SnapSegment(seg);
        var s2 = SnapRounding.SnapSegment(s1);
        Assert.Equal(s1.Start, s2.Start);
        Assert.Equal(s1.End, s2.End);
    }
}
