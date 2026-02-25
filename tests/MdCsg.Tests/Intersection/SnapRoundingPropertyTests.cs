using MdCsg.Intersection;
using MdCsg.Math;

namespace MdCsg.Tests.Intersection;

/// <summary>Phase 6: SnapRounding — Snap, SnapSegment, MergePoints</summary>
public class SnapRoundingPropertyTests
{
    [Fact]
    public void Snap_OnGrid_Unchanged()
    {
        var v = new Vec3(0.1, 0.2, 0.3);
        var snapped = SnapRounding.Snap(v, 0.1);
        Assert.True(System.Math.Abs(snapped.X - 0.1) < 1e-10);
        Assert.True(System.Math.Abs(snapped.Y - 0.2) < 1e-10);
        Assert.True(System.Math.Abs(snapped.Z - 0.3) < 1e-10);
    }

    [Fact]
    public void Snap_OffGrid_SnapsToNearest()
    {
        var v = new Vec3(0.14, 0.26, 0.38);
        var snapped = SnapRounding.Snap(v, 0.1);
        Assert.True(System.Math.Abs(snapped.X - 0.1) < 1e-10);
        Assert.True(System.Math.Abs(snapped.Y - 0.3) < 1e-10);
        Assert.True(System.Math.Abs(snapped.Z - 0.4) < 1e-10);
    }

    [Fact]
    public void Snap_NegativeCoordinates()
    {
        var v = new Vec3(-0.14, -0.26, -0.38);
        var snapped = SnapRounding.Snap(v, 0.1);
        Assert.True(System.Math.Abs(snapped.X - (-0.1)) < 1e-10);
        Assert.True(System.Math.Abs(snapped.Y - (-0.3)) < 1e-10);
        Assert.True(System.Math.Abs(snapped.Z - (-0.4)) < 1e-10);
    }

    [Fact]
    public void Snap_Zero_StaysZero()
    {
        var snapped = SnapRounding.Snap(Vec3.Zero, 0.1);
        Assert.True(System.Math.Abs(snapped.X) < 1e-10);
        Assert.True(System.Math.Abs(snapped.Y) < 1e-10);
        Assert.True(System.Math.Abs(snapped.Z) < 1e-10);
    }

    [Fact]
    public void Snap_VerySmallGrid()
    {
        var v = new Vec3(1.23456789, 2.34567890, 3.45678901);
        var snapped = SnapRounding.Snap(v, 1e-6);
        Assert.True(Vec3.Distance(v, snapped) < 1e-6);
    }

    [Fact]
    public void Snap_LargeGrid()
    {
        var v = new Vec3(3.7, 8.2, 14.9);
        var snapped = SnapRounding.Snap(v, 5.0);
        Assert.True(System.Math.Abs(snapped.X - 5.0) < 1e-10);
        Assert.True(System.Math.Abs(snapped.Y - 10.0) < 1e-10);
        Assert.True(System.Math.Abs(snapped.Z - 15.0) < 1e-10);
    }

    [Fact]
    public void SnapSegment_PreservesFaceIndices()
    {
        var seg = new IntersectionSegment(new Vec3(0.14, 0.26, 0), new Vec3(1.14, 1.26, 0), 42, 99);
        var snapped = SnapRounding.SnapSegment(seg, 0.1);
        Assert.Equal(42, snapped.FaceIndexA);
        Assert.Equal(99, snapped.FaceIndexB);
    }

    [Fact]
    public void SnapSegment_SnapsEndpoints()
    {
        var seg = new IntersectionSegment(new Vec3(0.14, 0, 0), new Vec3(1.14, 0, 0), 0, 1);
        var snapped = SnapRounding.SnapSegment(seg, 0.1);
        Assert.True(System.Math.Abs(snapped.Start.X - 0.1) < 1e-10);
        Assert.True(System.Math.Abs(snapped.End.X - 1.1) < 1e-10);
    }

    [Fact]
    public void SnapSegment_ShortSegment_CanBecomeDegenerate()
    {
        // Both endpoints snap to 0.1
        var seg = new IntersectionSegment(new Vec3(0.11, 0, 0), new Vec3(0.12, 0, 0), 0, 1);
        var snapped = SnapRounding.SnapSegment(seg, 0.1);
        Assert.True(snapped.IsDegenerate);
    }

    [Fact]
    public void MergePoints_AllIdentical_SingleUnique()
    {
        var points = new[] { Vec3.Zero, Vec3.Zero, Vec3.Zero };
        var (unique, mapping) = SnapRounding.MergePoints(points, 1e-10);
        Assert.Single(unique);
        Assert.Equal(0, mapping[0]);
        Assert.Equal(0, mapping[1]);
        Assert.Equal(0, mapping[2]);
    }

    [Fact]
    public void MergePoints_AllDistinct_KeepsAll()
    {
        var points = new[] { new Vec3(0,0,0), new Vec3(10,0,0), new Vec3(0,10,0) };
        var (unique, _) = SnapRounding.MergePoints(points, 1e-10);
        Assert.Equal(3, unique.Count);
    }

    [Fact]
    public void MergePoints_TwoClose_Merged()
    {
        var points = new[] { new Vec3(0,0,0), new Vec3(1e-12,0,0), new Vec3(10,0,0) };
        var (unique, mapping) = SnapRounding.MergePoints(points, 1e-10);
        Assert.Equal(2, unique.Count);
        Assert.Equal(mapping[0], mapping[1]);
    }

    [Fact]
    public void MergePoints_EmptyList()
    {
        var (unique, mapping) = SnapRounding.MergePoints(Array.Empty<Vec3>(), 1e-10);
        Assert.Empty(unique);
        Assert.Empty(mapping);
    }

    [Fact]
    public void MergePoints_SinglePoint()
    {
        var (unique, mapping) = SnapRounding.MergePoints(new[] { Vec3.UnitX }, 1e-10);
        Assert.Single(unique);
        Assert.Equal(0, mapping[0]);
    }

    [Fact]
    public void MergePoints_MappingCoversAllIndices()
    {
        var points = new[] { new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,0,0), new Vec3(2,0,0) };
        var (unique, mapping) = SnapRounding.MergePoints(points, 1e-10);
        for (int i = 0; i < points.Length; i++)
            Assert.True(mapping.ContainsKey(i));
    }

    [Fact]
    public void MergePoints_MappingIndicesAreValid()
    {
        var points = new[] { new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,0,0), new Vec3(2,0,0) };
        var (unique, mapping) = SnapRounding.MergePoints(points, 1e-10);
        foreach (var kvp in mapping)
            Assert.True(kvp.Value >= 0 && kvp.Value < unique.Count);
    }

    [Fact]
    public void Snap_DefaultGridSize_Works()
    {
        var v = new Vec3(1.5, 2.5, 3.5);
        var snapped = SnapRounding.Snap(v);
        Assert.True(Vec3.Distance(v, snapped) < 1.0);
    }
}
