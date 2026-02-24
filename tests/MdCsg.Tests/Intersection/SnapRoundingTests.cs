using MdCsg.Intersection;
using MdCsg.Math;

namespace MdCsg.Tests.Intersection;

/// <summary>Batch 37: SnapRounding and IntersectionSegment tests (20 tests)</summary>
public class SnapRoundingTests
{
    [Fact]
    public void Snap_ExactGridPoint_Unchanged()
    {
        var p = new Vec3(0.1, 0.2, 0.3);
        var snapped = SnapRounding.Snap(p, 0.1);
        Assert.True(Vec3.Distance(p, snapped) < 1e-12);
    }

    [Fact]
    public void Snap_NearGridPoint_Snaps()
    {
        var p = new Vec3(0.100000001, 0.199999999, 0.300000002);
        var snapped = SnapRounding.Snap(p, 0.1);
        Assert.True(System.Math.Abs(snapped.X - 0.1) < 1e-12);
        Assert.True(System.Math.Abs(snapped.Y - 0.2) < 1e-12);
        Assert.True(System.Math.Abs(snapped.Z - 0.3) < 1e-12);
    }

    [Fact]
    public void Snap_DefaultGridSize_Works()
    {
        var p = new Vec3(1.0, 2.0, 3.0);
        var snapped = SnapRounding.Snap(p);
        Assert.True(Vec3.Distance(p, snapped) < 1e-7);
    }

    [Fact]
    public void Snap_Zero_StaysZero()
    {
        var snapped = SnapRounding.Snap(Vec3.Zero, 0.01);
        Assert.Equal(0.0, snapped.X);
        Assert.Equal(0.0, snapped.Y);
        Assert.Equal(0.0, snapped.Z);
    }

    [Fact]
    public void Snap_NegativeValues_SnapsCorrectly()
    {
        var p = new Vec3(-0.31, -0.59, -0.91);
        var snapped = SnapRounding.Snap(p, 0.1);
        Assert.True(System.Math.Abs(snapped.X - (-0.3)) < 1e-10);
        Assert.True(System.Math.Abs(snapped.Y - (-0.6)) < 1e-10);
        Assert.True(System.Math.Abs(snapped.Z - (-0.9)) < 1e-10);
    }

    [Fact]
    public void SnapSegment_BothEndpointsSnapped()
    {
        var seg = new IntersectionSegment(
            new Vec3(0.100000001, 0.2, 0.3),
            new Vec3(0.4, 0.500000001, 0.6),
            0, 1);
        var snapped = SnapRounding.SnapSegment(seg, 0.1);
        Assert.True(System.Math.Abs(snapped.Start.X - 0.1) < 1e-12);
        Assert.True(System.Math.Abs(snapped.End.Y - 0.5) < 1e-12);
    }

    [Fact]
    public void SnapSegment_PreservesFaceIndices()
    {
        var seg = new IntersectionSegment(Vec3.Zero, Vec3.UnitX, 42, 99);
        var snapped = SnapRounding.SnapSegment(seg, 0.1);
        Assert.Equal(42, snapped.FaceIndexA);
        Assert.Equal(99, snapped.FaceIndexB);
    }

    [Fact]
    public void MergePoints_DistinctPoints_AllUnique()
    {
        var points = new List<Vec3>
        {
            new Vec3(0, 0, 0),
            new Vec3(1, 0, 0),
            new Vec3(0, 1, 0),
            new Vec3(0, 0, 1)
        };
        var (unique, mapping) = SnapRounding.MergePoints(points);
        Assert.Equal(4, unique.Count);
        Assert.Equal(0, mapping[0]);
        Assert.Equal(1, mapping[1]);
        Assert.Equal(2, mapping[2]);
        Assert.Equal(3, mapping[3]);
    }

    [Fact]
    public void MergePoints_DuplicatePoints_Merged()
    {
        var points = new List<Vec3>
        {
            new Vec3(0, 0, 0),
            new Vec3(1, 0, 0),
            new Vec3(0.000000001, 0.000000001, 0), // near-duplicate of [0]
        };
        var (unique, mapping) = SnapRounding.MergePoints(points);
        Assert.Equal(2, unique.Count);
        Assert.Equal(mapping[0], mapping[2]); // both map to same unique point
    }

    [Fact]
    public void MergePoints_AllSame_SingleUnique()
    {
        var p = new Vec3(1, 2, 3);
        var points = new List<Vec3> { p, p, p, p };
        var (unique, mapping) = SnapRounding.MergePoints(points);
        Assert.Single(unique);
        Assert.True(mapping.Values.All(v => v == 0));
    }

    [Fact]
    public void MergePoints_EmptyList_EmptyResult()
    {
        var (unique, mapping) = SnapRounding.MergePoints(new List<Vec3>());
        Assert.Empty(unique);
        Assert.Empty(mapping);
    }

    [Fact]
    public void MergePoints_CustomTolerance_AffectsMerging()
    {
        var points = new List<Vec3>
        {
            new Vec3(0, 0, 0),
            new Vec3(0.05, 0, 0),
        };
        // With tight tolerance, they are distinct
        var (unique1, _) = SnapRounding.MergePoints(points, 0.01);
        Assert.Equal(2, unique1.Count);

        // With loose tolerance, they merge
        var (unique2, _) = SnapRounding.MergePoints(points, 0.1);
        Assert.Single(unique2);
    }

    [Fact]
    public void IntersectionSegment_Length_Correct()
    {
        var seg = new IntersectionSegment(Vec3.Zero, Vec3.UnitX, 0, 1);
        Assert.True(System.Math.Abs(seg.Length - 1.0) < 1e-10);
    }

    [Fact]
    public void IntersectionSegment_IsDegenerate_ForZeroLength()
    {
        var seg = new IntersectionSegment(Vec3.Zero, Vec3.Zero, 0, 1);
        Assert.True(seg.IsDegenerate);
    }

    [Fact]
    public void IntersectionSegment_NotDegenerate_ForNonZeroLength()
    {
        var seg = new IntersectionSegment(Vec3.Zero, Vec3.UnitX, 0, 1);
        Assert.False(seg.IsDegenerate);
    }

    [Fact]
    public void IntersectionSegment_Length_DiagonalSegment()
    {
        var seg = new IntersectionSegment(
            new Vec3(1, 2, 3),
            new Vec3(4, 6, 3),
            0, 1);
        double expected = System.Math.Sqrt(9 + 16); // 5
        Assert.True(System.Math.Abs(seg.Length - expected) < 1e-10);
    }

    [Fact]
    public void MergePoints_Mapping_CoversAllInput()
    {
        var points = new List<Vec3>
        {
            new Vec3(0, 0, 0),
            new Vec3(1, 0, 0),
            new Vec3(0, 0, 0.0000000001), // near-dup of [0]
            new Vec3(2, 0, 0),
            new Vec3(1.0000000001, 0, 0), // near-dup of [1]
        };
        var (unique, mapping) = SnapRounding.MergePoints(points);
        Assert.Equal(5, mapping.Count); // every input has a mapping
        Assert.Equal(3, unique.Count);
    }

    [Fact]
    public void Snap_LargeValues_WorksCorrectly()
    {
        var p = new Vec3(1000.0000005, 2000.0000005, 3000.0000005);
        var snapped = SnapRounding.Snap(p, 0.000001);
        Assert.True(System.Math.Abs(snapped.X - 1000.000001) < 1e-10 ||
                    System.Math.Abs(snapped.X - 1000.000000) < 1e-10);
    }

    [Fact]
    public void SnapSegment_DefaultGridSize()
    {
        var seg = new IntersectionSegment(
            new Vec3(0.1, 0.2, 0.3),
            new Vec3(0.4, 0.5, 0.6),
            5, 10);
        var snapped = SnapRounding.SnapSegment(seg);
        Assert.Equal(5, snapped.FaceIndexA);
        Assert.Equal(10, snapped.FaceIndexB);
        Assert.True(snapped.Length > 0);
    }

    [Fact]
    public void IntersectionSegment_FaceIndices_Stored()
    {
        var seg = new IntersectionSegment(Vec3.Zero, Vec3.UnitY, 123, 456);
        Assert.Equal(123, seg.FaceIndexA);
        Assert.Equal(456, seg.FaceIndexB);
    }
}
