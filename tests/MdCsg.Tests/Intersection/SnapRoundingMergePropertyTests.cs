using MdCsg.Intersection;
using MdCsg.Math;

namespace MdCsg.Tests.Intersection;

/// <summary>Phase 6: SnapRounding — Snap, SnapSegment, MergePoints tolerance, grid alignment</summary>
public class SnapRoundingMergePropertyTests
{
    [Fact]
    public void Snap_ExactGridPoint_Unchanged()
    {
        var v = new Vec3(0.5, 1.0, 1.5);
        var snapped = SnapRounding.Snap(v, 0.5);
        Assert.True(System.Math.Abs(snapped.X - 0.5) < 1e-10);
        Assert.True(System.Math.Abs(snapped.Y - 1.0) < 1e-10);
        Assert.True(System.Math.Abs(snapped.Z - 1.5) < 1e-10);
    }

    [Fact]
    public void Snap_NearGridPoint_Rounds()
    {
        var v = new Vec3(0.501, 0.999, 1.501);
        var snapped = SnapRounding.Snap(v, 0.5);
        Assert.True(System.Math.Abs(snapped.X - 0.5) < 1e-10);
        Assert.True(System.Math.Abs(snapped.Y - 1.0) < 1e-10);
        Assert.True(System.Math.Abs(snapped.Z - 1.5) < 1e-10);
    }

    [Fact]
    public void Snap_Zero_IsZero()
    {
        var snapped = SnapRounding.Snap(Vec3.Zero, 0.1);
        Assert.True(System.Math.Abs(snapped.X) < 1e-10);
        Assert.True(System.Math.Abs(snapped.Y) < 1e-10);
        Assert.True(System.Math.Abs(snapped.Z) < 1e-10);
    }

    [Fact]
    public void Snap_NegativeValues_RoundsCorrectly()
    {
        var v = new Vec3(-0.3, -0.7, -1.2);
        var snapped = SnapRounding.Snap(v, 0.5);
        // -0.3 → -0.5, -0.7 → -0.5, -1.2 → -1.0
        Assert.True(System.Math.Abs(snapped.X - (-0.5)) < 1e-10);
        Assert.True(System.Math.Abs(snapped.Y - (-0.5)) < 1e-10);
        Assert.True(System.Math.Abs(snapped.Z - (-1.0)) < 1e-10);
    }

    [Fact]
    public void SnapSegment_PreservesFaceIndices()
    {
        var seg = new IntersectionSegment(new Vec3(0.1, 0.2, 0.3), new Vec3(0.4, 0.5, 0.6), 7, 13);
        var snapped = SnapRounding.SnapSegment(seg, 0.5);
        Assert.Equal(7, snapped.FaceIndexA);
        Assert.Equal(13, snapped.FaceIndexB);
    }

    [Fact]
    public void SnapSegment_EndpointsSnapped()
    {
        var seg = new IntersectionSegment(new Vec3(0.1, 0.2, 0.3), new Vec3(0.9, 1.1, 1.4), 0, 1);
        var snapped = SnapRounding.SnapSegment(seg, 0.5);
        // Start: 0.1→0.0, 0.2→0.0, 0.3→0.5
        // End: 0.9→1.0, 1.1→1.0, 1.4→1.5
        Assert.True(System.Math.Abs(snapped.Start.X - 0.0) < 1e-10);
        Assert.True(System.Math.Abs(snapped.End.X - 1.0) < 1e-10);
    }

    [Fact]
    public void MergePoints_IdenticalPoints_MergedToOne()
    {
        var points = new Vec3[] { new Vec3(1, 2, 3), new Vec3(1, 2, 3), new Vec3(1, 2, 3) };
        var (unique, mapping) = SnapRounding.MergePoints(points, 1e-6);
        Assert.Single(unique);
        Assert.Equal(0, mapping[0]);
        Assert.Equal(0, mapping[1]);
        Assert.Equal(0, mapping[2]);
    }

    [Fact]
    public void MergePoints_DistinctPoints_AllKept()
    {
        var points = new Vec3[] { Vec3.Zero, new Vec3(10, 0, 0), new Vec3(0, 10, 0) };
        var (unique, mapping) = SnapRounding.MergePoints(points, 1e-6);
        Assert.Equal(3, unique.Count);
    }

    [Fact]
    public void MergePoints_NearbyPoints_Merged()
    {
        var points = new Vec3[] { new Vec3(0, 0, 0), new Vec3(1e-9, 1e-9, 1e-9) };
        var (unique, mapping) = SnapRounding.MergePoints(points, 1e-6);
        Assert.Single(unique);
        Assert.Equal(mapping[0], mapping[1]);
    }

    [Fact]
    public void MergePoints_Empty_EmptyResult()
    {
        var (unique, mapping) = SnapRounding.MergePoints(Array.Empty<Vec3>(), 1e-6);
        Assert.Empty(unique);
        Assert.Empty(mapping);
    }

    [Fact]
    public void MergePoints_MappingCoversAllInputs()
    {
        var points = new Vec3[] { Vec3.Zero, Vec3.UnitX, Vec3.UnitY, Vec3.UnitZ };
        var (unique, mapping) = SnapRounding.MergePoints(points, 1e-6);
        Assert.Equal(4, mapping.Count);
        for (int i = 0; i < points.Length; i++)
            Assert.True(mapping.ContainsKey(i));
    }

    [Fact]
    public void MergePoints_MappingIndicesValid()
    {
        var points = new Vec3[] { Vec3.Zero, Vec3.UnitX, new Vec3(1e-10, 0, 0) };
        var (unique, mapping) = SnapRounding.MergePoints(points, 1e-6);
        foreach (var idx in mapping.Values)
        {
            Assert.True(idx >= 0 && idx < unique.Count,
                $"Mapping index {idx} should be < unique count {unique.Count}");
        }
    }

    [Fact]
    public void MergePoints_ClusterOfThree_OneMerged()
    {
        var points = new Vec3[]
        {
            new Vec3(0, 0, 0),
            new Vec3(1e-10, 0, 0),
            new Vec3(0, 1e-10, 0),
            new Vec3(10, 10, 10)
        };
        var (unique, mapping) = SnapRounding.MergePoints(points, 1e-6);
        // First three points should merge, fourth stays separate
        Assert.Equal(2, unique.Count);
        Assert.Equal(mapping[0], mapping[1]);
        Assert.Equal(mapping[0], mapping[2]);
        Assert.NotEqual(mapping[0], mapping[3]);
    }

    [Fact]
    public void Snap_Idempotent()
    {
        var v = new Vec3(1.234, 5.678, 9.012);
        double grid = 0.1;
        var once = SnapRounding.Snap(v, grid);
        var twice = SnapRounding.Snap(once, grid);
        Assert.True(System.Math.Abs(once.X - twice.X) < 1e-15);
        Assert.True(System.Math.Abs(once.Y - twice.Y) < 1e-15);
        Assert.True(System.Math.Abs(once.Z - twice.Z) < 1e-15);
    }

    [Fact]
    public void Snap_LargeValues_StillSnaps()
    {
        var v = new Vec3(1000.123, 2000.456, 3000.789);
        var snapped = SnapRounding.Snap(v, 0.5);
        Assert.True(System.Math.Abs(snapped.X - 1000.0) < 1e-10);
        Assert.True(System.Math.Abs(snapped.Y - 2000.5) < 1e-10);
        Assert.True(System.Math.Abs(snapped.Z - 3001.0) < 1e-10);
    }
}
