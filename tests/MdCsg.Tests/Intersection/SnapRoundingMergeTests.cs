using MdCsg.Intersection;
using MdCsg.Math;

namespace MdCsg.Tests.Intersection;

/// <summary>Phase 6: SnapRounding point merge deep tests</summary>
public class SnapRoundingMergeTests
{
    [Fact]
    public void MergePoints_NoMerge_AllUnique()
    {
        var points = new List<Vec3>
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(0, 0, 1)
        };
        var (unique, mapping) = SnapRounding.MergePoints(points);
        Assert.Equal(4, unique.Count);
        for (int i = 0; i < 4; i++)
            Assert.Equal(i, mapping[i]);
    }

    [Fact]
    public void MergePoints_Duplicates_Merged()
    {
        var points = new List<Vec3>
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 0, 0)
        };
        var (unique, mapping) = SnapRounding.MergePoints(points);
        Assert.Equal(2, unique.Count);
        Assert.Equal(mapping[0], mapping[2]);
    }

    [Fact]
    public void MergePoints_NearPoints_MergedWithinTolerance()
    {
        var points = new List<Vec3>
        {
            new(0, 0, 0), new(1e-9, 0, 0)
        };
        var (unique, mapping) = SnapRounding.MergePoints(points, tolerance: 1e-8);
        Assert.Single(unique);
    }

    [Fact]
    public void MergePoints_FarPoints_NotMerged()
    {
        var points = new List<Vec3>
        {
            new(0, 0, 0), new(0.01, 0, 0)
        };
        var (unique, mapping) = SnapRounding.MergePoints(points, tolerance: 1e-8);
        Assert.Equal(2, unique.Count);
    }

    [Fact]
    public void MergePoints_AllSame_SingleUnique()
    {
        var p = new Vec3(1, 2, 3);
        var points = new List<Vec3> { p, p, p, p, p };
        var (unique, mapping) = SnapRounding.MergePoints(points);
        Assert.Single(unique);
    }

    [Fact]
    public void MergePoints_MappingCoversAllInput()
    {
        var points = new List<Vec3>
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 0, 0), new(2, 0, 0), new(1, 0, 0)
        };
        var (unique, mapping) = SnapRounding.MergePoints(points);
        Assert.Equal(5, mapping.Count);
        foreach (var kvp in mapping)
            Assert.True(kvp.Value >= 0 && kvp.Value < unique.Count);
    }

    [Fact]
    public void MergePoints_Empty_EmptyResult()
    {
        var (unique, mapping) = SnapRounding.MergePoints(new List<Vec3>());
        Assert.Empty(unique);
        Assert.Empty(mapping);
    }

    [Fact]
    public void MergePoints_TransitiveNotRequired()
    {
        // A near B, B near C, but A not near C — merging is pairwise not transitive
        var points = new List<Vec3>
        {
            new(0, 0, 0),
            new(5e-9, 0, 0),
            new(1.5e-8, 0, 0)
        };
        var (unique, mapping) = SnapRounding.MergePoints(points, tolerance: 1e-8);
        // 0 and 1 merge, 2 may or may not merge with 1 depending on first-match
        Assert.True(unique.Count >= 2 && unique.Count <= 3);
    }

    [Fact]
    public void MergePoints_LargeOffset_StillWorks()
    {
        var points = new List<Vec3>
        {
            new(100, 200, 300), new(100, 200, 300), new(101, 200, 300)
        };
        var (unique, mapping) = SnapRounding.MergePoints(points);
        Assert.Equal(2, unique.Count);
    }

    [Fact]
    public void SnapSegment_PreservesIndices()
    {
        var seg = new IntersectionSegment(new Vec3(0.1, 0.2, 0.3), new Vec3(0.4, 0.5, 0.6), 42, 99);
        var snapped = SnapRounding.SnapSegment(seg, 1e-8);
        Assert.Equal(42, snapped.FaceIndexA);
        Assert.Equal(99, snapped.FaceIndexB);
    }

    [Fact]
    public void SnapSegment_Idempotent()
    {
        var seg = new IntersectionSegment(new Vec3(0.1, 0.2, 0.3), new Vec3(0.4, 0.5, 0.6), 0, 1);
        var s1 = SnapRounding.SnapSegment(seg, 1e-8);
        var s2 = SnapRounding.SnapSegment(s1, 1e-8);
        Assert.Equal(s1.Start, s2.Start);
        Assert.Equal(s1.End, s2.End);
    }
}
