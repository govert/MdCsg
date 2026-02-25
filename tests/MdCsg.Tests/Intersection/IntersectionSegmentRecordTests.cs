using MdCsg.Intersection;
using MdCsg.Math;

namespace MdCsg.Tests.Intersection;

/// <summary>Phase 6: IntersectionSegment record struct — Length, IsDegenerate, equality, face indices</summary>
public class IntersectionSegmentRecordTests
{
    [Fact]
    public void Length_ZeroDistance_IsZero()
    {
        var seg = new IntersectionSegment(Vec3.Zero, Vec3.Zero, 0, 0);
        Assert.Equal(0, seg.Length);
    }

    [Fact]
    public void Length_UnitDistance()
    {
        var seg = new IntersectionSegment(Vec3.Zero, new Vec3(1, 0, 0), 0, 1);
        Assert.Equal(1.0, seg.Length, 1e-10);
    }

    [Fact]
    public void Length_DiagonalDistance()
    {
        var seg = new IntersectionSegment(Vec3.Zero, new Vec3(1, 1, 1), 0, 1);
        Assert.Equal(System.Math.Sqrt(3), seg.Length, 1e-10);
    }

    [Fact]
    public void IsDegenerate_ZeroLength_True()
    {
        var seg = new IntersectionSegment(Vec3.Zero, Vec3.Zero, 0, 0);
        Assert.True(seg.IsDegenerate);
    }

    [Fact]
    public void IsDegenerate_TinyLength_True()
    {
        var seg = new IntersectionSegment(Vec3.Zero, new Vec3(1e-12, 0, 0), 0, 0);
        Assert.True(seg.IsDegenerate);
    }

    [Fact]
    public void IsDegenerate_NormalLength_False()
    {
        var seg = new IntersectionSegment(Vec3.Zero, new Vec3(1, 0, 0), 0, 1);
        Assert.False(seg.IsDegenerate);
    }

    [Fact]
    public void FaceIndexA_Stored()
    {
        var seg = new IntersectionSegment(Vec3.Zero, new Vec3(1, 0, 0), 42, 7);
        Assert.Equal(42, seg.FaceIndexA);
    }

    [Fact]
    public void FaceIndexB_Stored()
    {
        var seg = new IntersectionSegment(Vec3.Zero, new Vec3(1, 0, 0), 42, 7);
        Assert.Equal(7, seg.FaceIndexB);
    }

    [Fact]
    public void Start_Stored()
    {
        var start = new Vec3(1, 2, 3);
        var seg = new IntersectionSegment(start, new Vec3(4, 5, 6), 0, 0);
        Assert.Equal(start, seg.Start);
    }

    [Fact]
    public void End_Stored()
    {
        var end = new Vec3(4, 5, 6);
        var seg = new IntersectionSegment(new Vec3(1, 2, 3), end, 0, 0);
        Assert.Equal(end, seg.End);
    }

    [Fact]
    public void RecordEquality_SameValues()
    {
        var s1 = new IntersectionSegment(Vec3.Zero, new Vec3(1, 0, 0), 0, 1);
        var s2 = new IntersectionSegment(Vec3.Zero, new Vec3(1, 0, 0), 0, 1);
        Assert.Equal(s1, s2);
    }

    [Fact]
    public void RecordInequality_DifferentFaceIndex()
    {
        var s1 = new IntersectionSegment(Vec3.Zero, new Vec3(1, 0, 0), 0, 1);
        var s2 = new IntersectionSegment(Vec3.Zero, new Vec3(1, 0, 0), 0, 2);
        Assert.NotEqual(s1, s2);
    }

    [Fact]
    public void RecordInequality_DifferentEndpoint()
    {
        var s1 = new IntersectionSegment(Vec3.Zero, new Vec3(1, 0, 0), 0, 1);
        var s2 = new IntersectionSegment(Vec3.Zero, new Vec3(2, 0, 0), 0, 1);
        Assert.NotEqual(s1, s2);
    }

    [Fact]
    public void HashCode_EqualSegments_SameHash()
    {
        var s1 = new IntersectionSegment(Vec3.Zero, new Vec3(1, 0, 0), 0, 1);
        var s2 = new IntersectionSegment(Vec3.Zero, new Vec3(1, 0, 0), 0, 1);
        Assert.Equal(s1.GetHashCode(), s2.GetHashCode());
    }

    [Fact]
    public void IsDegenerate_BoundaryEpsilon()
    {
        // Exactly at epsilon boundary
        var seg = new IntersectionSegment(Vec3.Zero, new Vec3(MathUtil.Epsilon * 0.5, 0, 0), 0, 0);
        Assert.True(seg.IsDegenerate);
    }

    [Fact]
    public void IsDegenerate_JustAboveEpsilon()
    {
        var seg = new IntersectionSegment(Vec3.Zero, new Vec3(MathUtil.Epsilon * 2, 0, 0), 0, 0);
        Assert.False(seg.IsDegenerate);
    }
}
