using MdCsg.Intersection;
using MdCsg.Math;

namespace MdCsg.Tests.Intersection;

/// <summary>Phase 6: SnapRounding precision tests — Snap, SnapSegment, grid alignment</summary>
public class SnapRoundingPrecisionTests
{
    [Fact]
    public void Snap_Zero_ReturnsZero()
    {
        var snapped = SnapRounding.Snap(Vec3.Zero);
        Assert.Equal(Vec3.Zero, snapped);
    }

    [Fact]
    public void Snap_ExactGridValue_Unchanged()
    {
        var v = new Vec3(0.5, 1.0, 1.5);
        var snapped = SnapRounding.Snap(v, 0.5);
        Assert.Equal(v, snapped);
    }

    [Fact]
    public void Snap_NearbyValue_RoundsToGrid()
    {
        var v = new Vec3(0.500000001, 0.999999999, 1.500000002);
        var snapped = SnapRounding.Snap(v, 1e-8);
        Assert.True(System.Math.Abs(snapped.X - 0.5) < 1e-7);
        Assert.True(System.Math.Abs(snapped.Y - 1.0) < 1e-7);
        Assert.True(System.Math.Abs(snapped.Z - 1.5) < 1e-7);
    }

    [Fact]
    public void Snap_Idempotent()
    {
        var v = new Vec3(0.123456789, 0.987654321, 0.555555555);
        var s1 = SnapRounding.Snap(v);
        var s2 = SnapRounding.Snap(s1);
        Assert.Equal(s1, s2);
    }

    [Fact]
    public void Snap_NegativeValues()
    {
        var v = new Vec3(-0.5, -1.0, -0.25);
        var snapped = SnapRounding.Snap(v, 0.25);
        Assert.Equal(v, snapped);
    }

    [Fact]
    public void Snap_LargeValues()
    {
        var v = new Vec3(1000.0, 2000.0, 3000.0);
        var snapped = SnapRounding.Snap(v, 1e-8);
        Assert.True(Vec3.Distance(v, snapped) < 1e-7);
    }

    [Fact]
    public void SnapSegment_PreservesFaceIndices()
    {
        var seg = new IntersectionSegment(new Vec3(0.1, 0.2, 0.3), new Vec3(0.4, 0.5, 0.6), 42, 99);
        var snapped = SnapRounding.SnapSegment(seg);
        Assert.Equal(42, snapped.FaceIndexA);
        Assert.Equal(99, snapped.FaceIndexB);
    }

    [Fact]
    public void SnapSegment_Idempotent()
    {
        var seg = new IntersectionSegment(new Vec3(0.1, 0.2, 0.3), new Vec3(0.4, 0.5, 0.6), 0, 1);
        var s1 = SnapRounding.SnapSegment(seg);
        var s2 = SnapRounding.SnapSegment(s1);
        Assert.Equal(s1.Start, s2.Start);
        Assert.Equal(s1.End, s2.End);
    }

    [Fact]
    public void SnapSegment_StartEndSnapped()
    {
        var seg = new IntersectionSegment(
            new Vec3(0.100000001, 0.200000002, 0.300000003),
            new Vec3(0.400000001, 0.500000002, 0.600000003),
            0, 1);
        var snapped = SnapRounding.SnapSegment(seg, 1e-8);
        // Snapped values should be within grid size of original
        Assert.True(Vec3.Distance(seg.Start, snapped.Start) < 1e-7);
        Assert.True(Vec3.Distance(seg.End, snapped.End) < 1e-7);
    }

    [Fact]
    public void MergePoints_SinglePoint_SingleUnique()
    {
        var points = new List<Vec3> { new(1, 2, 3) };
        var (unique, mapping) = SnapRounding.MergePoints(points);
        Assert.Single(unique);
        Assert.Single(mapping);
    }

    [Fact]
    public void MergePoints_DuplicatePoints_Merged()
    {
        var p = new Vec3(1, 2, 3);
        var points = new List<Vec3> { p, p, p };
        var (unique, mapping) = SnapRounding.MergePoints(points);
        Assert.Single(unique);
        Assert.Equal(mapping[0], mapping[1]);
        Assert.Equal(mapping[1], mapping[2]);
    }

    [Fact]
    public void MergePoints_WithinTolerance_Merged()
    {
        var points = new List<Vec3>
        {
            new(0, 0, 0), new(1e-10, 0, 0)
        };
        var (unique, mapping) = SnapRounding.MergePoints(points, tolerance: 1e-8);
        Assert.Single(unique);
    }

    [Fact]
    public void MergePoints_BeyondTolerance_NotMerged()
    {
        var points = new List<Vec3>
        {
            new(0, 0, 0), new(0.01, 0, 0)
        };
        var (unique, mapping) = SnapRounding.MergePoints(points, tolerance: 1e-8);
        Assert.Equal(2, unique.Count);
    }

    [Fact]
    public void MergePoints_Empty_Empty()
    {
        var (unique, mapping) = SnapRounding.MergePoints(new List<Vec3>());
        Assert.Empty(unique);
        Assert.Empty(mapping);
    }

    [Fact]
    public void MergePoints_ManyDuplicates_SingleUnique()
    {
        var p = new Vec3(5, 5, 5);
        var points = Enumerable.Range(0, 100).Select(_ => p).ToList();
        var (unique, mapping) = SnapRounding.MergePoints(points);
        Assert.Single(unique);
    }

    [Fact]
    public void MergePoints_MappingIndicesValid()
    {
        var points = new List<Vec3>
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 0, 0), new(2, 0, 0)
        };
        var (unique, mapping) = SnapRounding.MergePoints(points);
        foreach (var kvp in mapping)
            Assert.True(kvp.Value >= 0 && kvp.Value < unique.Count);
    }
}
