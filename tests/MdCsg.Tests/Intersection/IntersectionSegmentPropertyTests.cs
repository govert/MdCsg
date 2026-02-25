using MdCsg.Intersection;
using MdCsg.Math;

namespace MdCsg.Tests.Intersection;

/// <summary>Phase 6: IntersectionSegment — Length, IsDegenerate, FaceIndexA/B, record equality</summary>
public class IntersectionSegmentPropertyTests
{
    [Fact]
    public void Length_NonZeroSegment_Positive()
    {
        var seg = new IntersectionSegment(new Vec3(0, 0, 0), new Vec3(1, 0, 0), 0, 1);
        Assert.True(seg.Length > 0);
    }

    [Fact]
    public void Length_ZeroLength_IsZero()
    {
        var seg = new IntersectionSegment(new Vec3(1, 2, 3), new Vec3(1, 2, 3), 0, 1);
        Assert.Equal(0.0, seg.Length);
    }

    [Fact]
    public void Length_DiagonalSegment_Correct()
    {
        var seg = new IntersectionSegment(new Vec3(0, 0, 0), new Vec3(3, 4, 0), 0, 1);
        Assert.True(System.Math.Abs(seg.Length - 5.0) < 1e-10);
    }

    [Fact]
    public void IsDegenerate_ZeroLength_True()
    {
        var seg = new IntersectionSegment(new Vec3(5, 5, 5), new Vec3(5, 5, 5), 0, 1);
        Assert.True(seg.IsDegenerate);
    }

    [Fact]
    public void IsDegenerate_NonZeroLength_False()
    {
        var seg = new IntersectionSegment(new Vec3(0, 0, 0), new Vec3(1, 0, 0), 0, 1);
        Assert.False(seg.IsDegenerate);
    }

    [Fact]
    public void FaceIndexA_StoredCorrectly()
    {
        var seg = new IntersectionSegment(Vec3.Zero, Vec3.UnitX, 42, 99);
        Assert.Equal(42, seg.FaceIndexA);
    }

    [Fact]
    public void FaceIndexB_StoredCorrectly()
    {
        var seg = new IntersectionSegment(Vec3.Zero, Vec3.UnitX, 42, 99);
        Assert.Equal(99, seg.FaceIndexB);
    }

    [Fact]
    public void Start_StoredCorrectly()
    {
        var start = new Vec3(1, 2, 3);
        var seg = new IntersectionSegment(start, new Vec3(4, 5, 6), 0, 1);
        Assert.Equal(1.0, seg.Start.X);
        Assert.Equal(2.0, seg.Start.Y);
        Assert.Equal(3.0, seg.Start.Z);
    }

    [Fact]
    public void End_StoredCorrectly()
    {
        var end = new Vec3(4, 5, 6);
        var seg = new IntersectionSegment(Vec3.Zero, end, 0, 1);
        Assert.Equal(4.0, seg.End.X);
        Assert.Equal(5.0, seg.End.Y);
        Assert.Equal(6.0, seg.End.Z);
    }

    [Fact]
    public void RecordEquality_SameValues()
    {
        var s1 = new IntersectionSegment(new Vec3(1, 2, 3), new Vec3(4, 5, 6), 0, 1);
        var s2 = new IntersectionSegment(new Vec3(1, 2, 3), new Vec3(4, 5, 6), 0, 1);
        Assert.Equal(s1, s2);
    }

    [Fact]
    public void RecordEquality_DifferentStart()
    {
        var s1 = new IntersectionSegment(new Vec3(0, 0, 0), new Vec3(1, 0, 0), 0, 1);
        var s2 = new IntersectionSegment(new Vec3(2, 0, 0), new Vec3(1, 0, 0), 0, 1);
        Assert.NotEqual(s1, s2);
    }

    [Fact]
    public void RecordEquality_DifferentFaceIndex()
    {
        var s1 = new IntersectionSegment(Vec3.Zero, Vec3.UnitX, 0, 1);
        var s2 = new IntersectionSegment(Vec3.Zero, Vec3.UnitX, 0, 2);
        Assert.NotEqual(s1, s2);
    }

    [Fact]
    public void IsDegenerate_VeryShort_True()
    {
        // Segment shorter than epsilon should be degenerate
        var seg = new IntersectionSegment(new Vec3(0, 0, 0), new Vec3(1e-15, 0, 0), 0, 1);
        Assert.True(seg.IsDegenerate);
    }

    [Fact]
    public void Length_3DPythagorean()
    {
        // (1, 2, 2) → length 3
        var seg = new IntersectionSegment(Vec3.Zero, new Vec3(1, 2, 2), 0, 1);
        Assert.True(System.Math.Abs(seg.Length - 3.0) < 1e-10);
    }

    [Fact]
    public void NegativeFaceIndices_Allowed()
    {
        // Default face indices are -1 before assignment
        var seg = new IntersectionSegment(Vec3.Zero, Vec3.UnitX, -1, -1);
        Assert.Equal(-1, seg.FaceIndexA);
        Assert.Equal(-1, seg.FaceIndexB);
    }
}
