using MdCsg.Intersection;
using MdCsg.Math;

namespace MdCsg.Tests.Intersection;

/// <summary>Phase 6: IntersectionSegment property tests — Length, IsDegenerate, record semantics</summary>
public class IntersectionSegmentPropertyTests
{
    [Fact]
    public void Length_UnitSegment()
    {
        var seg = new IntersectionSegment(Vec3.Zero, new Vec3(1, 0, 0), 0, 1);
        Assert.Equal(1.0, seg.Length, 12);
    }

    [Fact]
    public void Length_DiagonalSegment()
    {
        var seg = new IntersectionSegment(Vec3.Zero, new Vec3(3, 4, 0), 0, 1);
        Assert.Equal(5.0, seg.Length, 12);
    }

    [Fact]
    public void Length_ZeroLengthSegment()
    {
        var seg = new IntersectionSegment(new Vec3(1, 2, 3), new Vec3(1, 2, 3), 0, 1);
        Assert.Equal(0.0, seg.Length, 12);
    }

    [Fact]
    public void IsDegenerate_ZeroLength_True()
    {
        var seg = new IntersectionSegment(Vec3.Zero, Vec3.Zero, 0, 1);
        Assert.True(seg.IsDegenerate);
    }

    [Fact]
    public void IsDegenerate_VeryShort_True()
    {
        var seg = new IntersectionSegment(Vec3.Zero, new Vec3(1e-16, 0, 0), 0, 1);
        Assert.True(seg.IsDegenerate);
    }

    [Fact]
    public void IsDegenerate_NormalLength_False()
    {
        var seg = new IntersectionSegment(Vec3.Zero, new Vec3(1, 0, 0), 0, 1);
        Assert.False(seg.IsDegenerate);
    }

    [Fact]
    public void IsDegenerate_SmallButAboveEpsilon_False()
    {
        var seg = new IntersectionSegment(Vec3.Zero, new Vec3(1e-6, 0, 0), 0, 1);
        Assert.False(seg.IsDegenerate);
    }

    [Fact]
    public void FaceIndices_Preserved()
    {
        var seg = new IntersectionSegment(Vec3.Zero, new Vec3(1, 0, 0), 42, 99);
        Assert.Equal(42, seg.FaceIndexA);
        Assert.Equal(99, seg.FaceIndexB);
    }

    [Fact]
    public void RecordEquality_SameValues()
    {
        var a = new IntersectionSegment(Vec3.Zero, new Vec3(1, 0, 0), 0, 1);
        var b = new IntersectionSegment(Vec3.Zero, new Vec3(1, 0, 0), 0, 1);
        Assert.Equal(a, b);
    }

    [Fact]
    public void RecordInequality_DifferentFaceIndex()
    {
        var a = new IntersectionSegment(Vec3.Zero, new Vec3(1, 0, 0), 0, 1);
        var b = new IntersectionSegment(Vec3.Zero, new Vec3(1, 0, 0), 0, 2);
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void RecordInequality_DifferentStart()
    {
        var a = new IntersectionSegment(Vec3.Zero, new Vec3(1, 0, 0), 0, 1);
        var b = new IntersectionSegment(new Vec3(0.1, 0, 0), new Vec3(1, 0, 0), 0, 1);
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Length_LargeCoordinates()
    {
        var seg = new IntersectionSegment(new Vec3(1000, 2000, 3000), new Vec3(1001, 2000, 3000), 0, 1);
        Assert.Equal(1.0, seg.Length, 10);
    }

    [Fact]
    public void IsDegenerate_IdenticalLargeCoordinates()
    {
        var p = new Vec3(1e8, 1e8, 1e8);
        var seg = new IntersectionSegment(p, p, 0, 1);
        Assert.True(seg.IsDegenerate);
    }
}
