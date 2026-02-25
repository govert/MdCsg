using MdCsg.Intersection;
using MdCsg.Math;

namespace MdCsg.Tests.Intersection;

/// <summary>Phase 6: SnapRounding roundtrip and idempotency — snap, merge, segment endpoint stability</summary>
public class SnapRoundingRoundtripTests
{
    [Fact]
    public void Snap_Idempotent()
    {
        var p = new Vec3(1.23456789, 2.34567891, 3.45678912);
        var snapped = SnapRounding.Snap(p);
        var snappedAgain = SnapRounding.Snap(snapped);
        Assert.Equal(snapped, snappedAgain);
    }

    [Fact]
    public void Snap_ZeroStaysZero()
    {
        var p = Vec3.Zero;
        var snapped = SnapRounding.Snap(p);
        Assert.Equal(0, snapped.X, 1e-12);
        Assert.Equal(0, snapped.Y, 1e-12);
        Assert.Equal(0, snapped.Z, 1e-12);
    }

    [Fact]
    public void Snap_ExactGridPoint_Unchanged()
    {
        double grid = 1e-8;
        var p = new Vec3(3 * grid, 5 * grid, 7 * grid);
        var snapped = SnapRounding.Snap(p, grid);
        Assert.Equal(p.X, snapped.X, 1e-14);
        Assert.Equal(p.Y, snapped.Y, 1e-14);
        Assert.Equal(p.Z, snapped.Z, 1e-14);
    }

    [Fact]
    public void Snap_NegativeValues()
    {
        var p = new Vec3(-1.23456789, -2.34567891, -3.45678912);
        var snapped = SnapRounding.Snap(p);
        var snappedAgain = SnapRounding.Snap(snapped);
        Assert.Equal(snapped, snappedAgain);
    }

    [Fact]
    public void Snap_LargeValues()
    {
        var p = new Vec3(1e6 + 0.5e-8, 2e6, 3e6);
        var snapped = SnapRounding.Snap(p);
        Assert.False(double.IsNaN(snapped.X));
        Assert.False(double.IsInfinity(snapped.X));
    }

    [Fact]
    public void Snap_CustomGridSize()
    {
        var p = new Vec3(0.123, 0.456, 0.789);
        var snapped = SnapRounding.Snap(p, 0.1);
        // Should snap to nearest 0.1
        Assert.Equal(0.1, snapped.X, 1e-10);
        Assert.Equal(0.5, snapped.Y, 1e-10);
        Assert.Equal(0.8, snapped.Z, 1e-10);
    }

    [Fact]
    public void SnapSegment_PreservesFaceIndices()
    {
        var seg = new IntersectionSegment(
            new Vec3(0.1234, 0.5678, 0.9012), new Vec3(0.3456, 0.7890, 0.1234), 42, 99);
        var snapped = SnapRounding.SnapSegment(seg);
        Assert.Equal(42, snapped.FaceIndexA);
        Assert.Equal(99, snapped.FaceIndexB);
    }

    [Fact]
    public void SnapSegment_EndpointsAreSnapped()
    {
        var seg = new IntersectionSegment(
            new Vec3(0.123, 0.456, 0.789), new Vec3(0.321, 0.654, 0.987), 0, 1);
        var snapped = SnapRounding.SnapSegment(seg);
        var reSnappedStart = SnapRounding.Snap(snapped.Start);
        var reSnappedEnd = SnapRounding.Snap(snapped.End);
        Assert.Equal(snapped.Start, reSnappedStart);
        Assert.Equal(snapped.End, reSnappedEnd);
    }

    [Fact]
    public void MergePoints_AllUnique_NoMerging()
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
    public void MergePoints_AllSame_MergesToOne()
    {
        var points = new List<Vec3>
        {
            new(1, 2, 3), new(1, 2, 3), new(1, 2, 3)
        };
        var (unique, mapping) = SnapRounding.MergePoints(points);
        Assert.Equal(1, unique.Count);
        Assert.Equal(mapping[0], mapping[1]);
        Assert.Equal(mapping[0], mapping[2]);
    }

    [Fact]
    public void MergePoints_NearbyWithinTolerance_Merged()
    {
        double tol = 0.01;
        var points = new List<Vec3>
        {
            new(0, 0, 0), new(0.005, 0, 0), new(1, 0, 0)
        };
        var (unique, mapping) = SnapRounding.MergePoints(points, tol);
        Assert.True(unique.Count <= 2, "Within-tolerance points should merge");
        Assert.Equal(mapping[0], mapping[1]);
    }

    [Fact]
    public void MergePoints_BeyondTolerance_NotMerged()
    {
        double tol = 0.001;
        var points = new List<Vec3>
        {
            new(0, 0, 0), new(0.01, 0, 0)
        };
        var (unique, mapping) = SnapRounding.MergePoints(points, tol);
        Assert.Equal(2, unique.Count);
        Assert.NotEqual(mapping[0], mapping[1]);
    }

    [Fact]
    public void MergePoints_Empty_EmptyResult()
    {
        var (unique, mapping) = SnapRounding.MergePoints(new List<Vec3>());
        Assert.Equal(0, unique.Count);
        Assert.Equal(0, mapping.Count);
    }

    [Fact]
    public void MergePoints_MappingCoversAllInputs()
    {
        var points = new List<Vec3>
        {
            new(0, 0, 0), new(1, 1, 1), new(0, 0, 0), new(2, 2, 2), new(1, 1, 1)
        };
        var (unique, mapping) = SnapRounding.MergePoints(points);
        Assert.Equal(5, mapping.Count);
        foreach (var kvp in mapping)
        {
            Assert.True(kvp.Value >= 0 && kvp.Value < unique.Count);
        }
    }

    [Fact]
    public void SnapSegment_Idempotent()
    {
        var seg = new IntersectionSegment(
            new Vec3(0.123, 0.456, 0.789), new Vec3(0.321, 0.654, 0.987), 0, 1);
        var snapped = SnapRounding.SnapSegment(seg);
        var snappedAgain = SnapRounding.SnapSegment(snapped);
        Assert.Equal(snapped.Start, snappedAgain.Start);
        Assert.Equal(snapped.End, snappedAgain.End);
    }

    [Fact]
    public void SnapSegment_CustomGridSize()
    {
        var seg = new IntersectionSegment(
            new Vec3(0.123, 0.456, 0.789), new Vec3(0.321, 0.654, 0.987), 0, 1);
        var snapped = SnapRounding.SnapSegment(seg, 0.01);
        // Re-snapping should be idempotent
        var reSnapped = SnapRounding.SnapSegment(snapped, 0.01);
        Assert.Equal(snapped.Start.X, reSnapped.Start.X, 1e-14);
        Assert.Equal(snapped.Start.Y, reSnapped.Start.Y, 1e-14);
        Assert.Equal(snapped.End.X, reSnapped.End.X, 1e-14);
    }
}
