using MdCsg.Intersection;
using MdCsg.Math;

namespace MdCsg.Tests.Intersection;

/// <summary>Phase 6: SnapRounding + IntersectionSegment — snap behavior, degenerate detection, merge properties</summary>
public class SnapRoundingPropertyTests
{
    [Fact]
    public void Snap_GridAligned_NoChange()
    {
        var p = new Vec3(1.0, 2.0, 3.0);
        var snapped = SnapRounding.Snap(p, 1e-8);
        Assert.Equal(p.X, snapped.X, 15);
        Assert.Equal(p.Y, snapped.Y, 15);
        Assert.Equal(p.Z, snapped.Z, 15);
    }

    [Fact]
    public void Snap_TinyOffset_RoundsToGrid()
    {
        var p = new Vec3(1.0 + 1e-12, 2.0 + 1e-12, 3.0 + 1e-12);
        var snapped = SnapRounding.Snap(p, 1e-8);
        // After snapping, should be close to grid
        Assert.True(System.Math.Abs(snapped.X - 1.0) < 1e-7);
        Assert.True(System.Math.Abs(snapped.Y - 2.0) < 1e-7);
        Assert.True(System.Math.Abs(snapped.Z - 3.0) < 1e-7);
    }

    [Fact]
    public void Snap_Idempotent()
    {
        var p = new Vec3(1.23456, 7.89012, 3.45678);
        var snapped1 = SnapRounding.Snap(p, 1e-8);
        var snapped2 = SnapRounding.Snap(snapped1, 1e-8);
        Assert.Equal(snapped1.X, snapped2.X, 15);
        Assert.Equal(snapped1.Y, snapped2.Y, 15);
        Assert.Equal(snapped1.Z, snapped2.Z, 15);
    }

    [Fact]
    public void SnapSegment_PreservesFaceIndices()
    {
        var seg = new IntersectionSegment(
            new Vec3(1.0 + 1e-12, 0, 0), new Vec3(2.0 + 1e-12, 0, 0), 5, 10);
        var snapped = SnapRounding.SnapSegment(seg, 1e-8);
        Assert.Equal(5, snapped.FaceIndexA);
        Assert.Equal(10, snapped.FaceIndexB);
    }

    [Fact]
    public void SnapSegment_NearDuplicate_BecomesDegenerate()
    {
        var seg = new IntersectionSegment(
            new Vec3(1.0, 2.0, 3.0),
            new Vec3(1.0 + 1e-12, 2.0 + 1e-12, 3.0 + 1e-12),
            0, 1);
        var snapped = SnapRounding.SnapSegment(seg, 1e-8);
        Assert.True(snapped.IsDegenerate);
    }

    [Fact]
    public void IntersectionSegment_Length_Correct()
    {
        var seg = new IntersectionSegment(Vec3.Zero, new Vec3(3, 4, 0), 0, 1);
        Assert.Equal(5.0, seg.Length, 10);
    }

    [Fact]
    public void IntersectionSegment_ZeroLength_IsDegenerate()
    {
        var seg = new IntersectionSegment(Vec3.Zero, Vec3.Zero, 0, 1);
        Assert.True(seg.IsDegenerate);
    }

    [Fact]
    public void IntersectionSegment_NonZeroLength_NotDegenerate()
    {
        var seg = new IntersectionSegment(Vec3.Zero, new Vec3(1, 0, 0), 0, 1);
        Assert.False(seg.IsDegenerate);
    }

    [Fact]
    public void MergePoints_IdenticalPoints_SingleUnique()
    {
        var points = new List<Vec3>
        {
            new(1, 2, 3),
            new(1, 2, 3),
            new(1, 2, 3)
        };
        var (unique, mapping) = SnapRounding.MergePoints(points, 1e-8);
        Assert.Single(unique);
        Assert.Equal(0, mapping[0]);
        Assert.Equal(0, mapping[1]);
        Assert.Equal(0, mapping[2]);
    }

    [Fact]
    public void MergePoints_DistinctPoints_AllUnique()
    {
        var points = new List<Vec3>
        {
            new(0, 0, 0),
            new(1, 0, 0),
            new(0, 1, 0)
        };
        var (unique, mapping) = SnapRounding.MergePoints(points, 1e-8);
        Assert.Equal(3, unique.Count);
    }

    [Fact]
    public void MergePoints_NearDuplicates_Merged()
    {
        var points = new List<Vec3>
        {
            new(1.0, 2.0, 3.0),
            new(1.0 + 1e-12, 2.0 + 1e-12, 3.0 + 1e-12),
            new(5.0, 6.0, 7.0)
        };
        var (unique, mapping) = SnapRounding.MergePoints(points, 1e-8);
        Assert.Equal(2, unique.Count);
        Assert.Equal(mapping[0], mapping[1]);
    }

    [Fact]
    public void MergePoints_Empty_ReturnsEmpty()
    {
        var (unique, mapping) = SnapRounding.MergePoints(new List<Vec3>(), 1e-8);
        Assert.Empty(unique);
        Assert.Empty(mapping);
    }

    [Fact]
    public void MergePoints_Mapping_CoversAllInputs()
    {
        var points = new List<Vec3>
        {
            new(0, 0, 0),
            new(1, 1, 1),
            new(0, 0, 1e-12), // near-duplicate of index 0
            new(2, 2, 2)
        };
        var (unique, mapping) = SnapRounding.MergePoints(points, 1e-8);
        Assert.Equal(4, mapping.Count);
        for (int i = 0; i < points.Count; i++)
        {
            Assert.True(mapping.ContainsKey(i));
            Assert.True(mapping[i] >= 0 && mapping[i] < unique.Count);
        }
    }

    [Fact]
    public void Snap_LargeCoordinates_StillSnaps()
    {
        var p = new Vec3(1e10 + 1e-12, 1e10 + 1e-12, 1e10 + 1e-12);
        var snapped = SnapRounding.Snap(p, 1e-8);
        // Should be close to the grid value
        Assert.True(System.Math.Abs(snapped.X - p.X) < 1e-7);
    }
}
