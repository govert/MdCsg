using MdCsg.Intersection;
using MdCsg.Math;

namespace MdCsg.Tests.Intersection;

/// <summary>Phase 6: SnapRounding.MergePoints — deduplication, tolerance boundaries, mapping correctness</summary>
public class SnapRoundingMergePointTests
{
    [Fact]
    public void MergePoints_EmptyList_EmptyResult()
    {
        var (unique, mapping) = SnapRounding.MergePoints(new List<Vec3>());
        Assert.Equal(0, unique.Count);
        Assert.Equal(0, mapping.Count);
    }

    [Fact]
    public void MergePoints_SinglePoint_OneUnique()
    {
        var points = new List<Vec3> { new Vec3(1, 2, 3) };
        var (unique, mapping) = SnapRounding.MergePoints(points);
        Assert.Equal(1, unique.Count);
        Assert.Equal(0, mapping[0]);
    }

    [Fact]
    public void MergePoints_IdenticalPoints_OneUnique()
    {
        var p = new Vec3(1, 2, 3);
        var points = new List<Vec3> { p, p, p, p };
        var (unique, mapping) = SnapRounding.MergePoints(points);
        Assert.Equal(1, unique.Count);
        Assert.Equal(0, mapping[0]);
        Assert.Equal(0, mapping[1]);
        Assert.Equal(0, mapping[2]);
        Assert.Equal(0, mapping[3]);
    }

    [Fact]
    public void MergePoints_DistinctPoints_AllUnique()
    {
        var points = new List<Vec3>
        {
            new Vec3(0, 0, 0),
            new Vec3(1, 0, 0),
            new Vec3(0, 1, 0),
            new Vec3(0, 0, 1),
        };
        var (unique, mapping) = SnapRounding.MergePoints(points);
        Assert.Equal(4, unique.Count);
        // Each maps to unique index
        var indices = new HashSet<int>(mapping.Values);
        Assert.Equal(4, indices.Count);
    }

    [Fact]
    public void MergePoints_NearPoints_Merged()
    {
        var tolerance = 1e-6;
        var points = new List<Vec3>
        {
            new Vec3(0, 0, 0),
            new Vec3(1e-7, 1e-7, 1e-7), // within tolerance of first
        };
        var (unique, mapping) = SnapRounding.MergePoints(points, tolerance);
        Assert.Equal(1, unique.Count);
        Assert.Equal(mapping[0], mapping[1]);
    }

    [Fact]
    public void MergePoints_JustOutsideTolerance_NotMerged()
    {
        var tolerance = 1e-6;
        // Distance = sqrt(3) * 1e-6 ≈ 1.73e-6, squared = 3e-12 > tolerance^2 = 1e-12
        var points = new List<Vec3>
        {
            new Vec3(0, 0, 0),
            new Vec3(1e-6, 1e-6, 1e-6),
        };
        var (unique, mapping) = SnapRounding.MergePoints(points, tolerance);
        Assert.Equal(2, unique.Count);
    }

    [Fact]
    public void MergePoints_MappingCoversAllInputs()
    {
        var points = new List<Vec3>
        {
            new Vec3(0, 0, 0),
            new Vec3(1, 0, 0),
            new Vec3(0, 0, 0),
            new Vec3(2, 0, 0),
            new Vec3(1, 0, 0),
        };
        var (unique, mapping) = SnapRounding.MergePoints(points);
        // Every input index has a mapping
        for (int i = 0; i < points.Count; i++)
        {
            Assert.True(mapping.ContainsKey(i));
            Assert.True(mapping[i] >= 0 && mapping[i] < unique.Count);
        }
    }

    [Fact]
    public void MergePoints_DuplicatesMapToSame()
    {
        var points = new List<Vec3>
        {
            new Vec3(0, 0, 0),
            new Vec3(1, 0, 0),
            new Vec3(0, 0, 0), // same as [0]
            new Vec3(1, 0, 0), // same as [1]
        };
        var (unique, mapping) = SnapRounding.MergePoints(points);
        Assert.Equal(2, unique.Count);
        Assert.Equal(mapping[0], mapping[2]);
        Assert.Equal(mapping[1], mapping[3]);
    }

    [Fact]
    public void MergePoints_FirstPointIsCanonical()
    {
        var p1 = new Vec3(1, 2, 3);
        var p2 = new Vec3(1, 2, 3); // identical
        var points = new List<Vec3> { p1, p2 };
        var (unique, _) = SnapRounding.MergePoints(points);
        Assert.Equal(p1, unique[0]);
    }

    [Fact]
    public void MergePoints_ThreeCluster_OneMerged()
    {
        var tolerance = 0.1;
        var points = new List<Vec3>
        {
            new Vec3(0, 0, 0),
            new Vec3(0.01, 0, 0),   // near cluster 1
            new Vec3(0.02, 0, 0),   // near cluster 1
            new Vec3(10, 10, 10),   // far away
        };
        var (unique, mapping) = SnapRounding.MergePoints(points, tolerance);
        // First three should merge (each within tolerance of the first found)
        Assert.Equal(mapping[0], mapping[1]);
        Assert.Equal(mapping[0], mapping[2]);
        Assert.NotEqual(mapping[0], mapping[3]);
    }

    [Fact]
    public void MergePoints_NegativeCoordinates()
    {
        var points = new List<Vec3>
        {
            new Vec3(-1, -2, -3),
            new Vec3(-1, -2, -3),
        };
        var (unique, mapping) = SnapRounding.MergePoints(points);
        Assert.Equal(1, unique.Count);
        Assert.Equal(mapping[0], mapping[1]);
    }

    [Fact]
    public void MergePoints_LargeCoordinates()
    {
        var points = new List<Vec3>
        {
            new Vec3(1e8, 2e8, 3e8),
            new Vec3(1e8, 2e8, 3e8),
            new Vec3(1e8 + 1, 2e8, 3e8),
        };
        var (unique, mapping) = SnapRounding.MergePoints(points);
        Assert.Equal(2, unique.Count);
        Assert.Equal(mapping[0], mapping[1]);
        Assert.NotEqual(mapping[0], mapping[2]);
    }

    [Fact]
    public void MergePoints_DefaultTolerance()
    {
        // Default tolerance is MathUtil.DefaultGridSize = 1e-8
        var points = new List<Vec3>
        {
            new Vec3(0, 0, 0),
            new Vec3(1e-9, 0, 0), // within 1e-8
        };
        var (unique, _) = SnapRounding.MergePoints(points);
        Assert.Equal(1, unique.Count);
    }

    [Fact]
    public void MergePoints_OrderPreserved()
    {
        var points = new List<Vec3>
        {
            new Vec3(5, 0, 0),
            new Vec3(3, 0, 0),
            new Vec3(1, 0, 0),
        };
        var (unique, mapping) = SnapRounding.MergePoints(points);
        Assert.Equal(3, unique.Count);
        // First-seen order
        Assert.Equal(new Vec3(5, 0, 0), unique[0]);
        Assert.Equal(new Vec3(3, 0, 0), unique[1]);
        Assert.Equal(new Vec3(1, 0, 0), unique[2]);
        Assert.Equal(0, mapping[0]);
        Assert.Equal(1, mapping[1]);
        Assert.Equal(2, mapping[2]);
    }

    [Fact]
    public void Snap_PreservesGridAligned()
    {
        var p = new Vec3(1.0, 2.0, 3.0);
        var snapped = SnapRounding.Snap(p);
        Assert.Equal(p, snapped);
    }

    [Fact]
    public void Snap_RoundsToGrid()
    {
        var p = new Vec3(1.000000004, 2.000000006, 3.0);
        var snapped = SnapRounding.Snap(p, 1e-8);
        // 1.000000004 rounds to 1.0, 2.000000006 rounds to 2.00000001
        Assert.Equal(1.0, snapped.X, 1e-9);
        Assert.True(System.Math.Abs(snapped.Z - 3.0) < 1e-9);
    }

    [Fact]
    public void SnapSegment_BothEndpointsSnapped()
    {
        var seg = new IntersectionSegment(
            new Vec3(0.000000004, 0, 0),
            new Vec3(1.000000004, 0, 0),
            5, 10);
        var snapped = SnapRounding.SnapSegment(seg, 1e-8);
        Assert.Equal(0.0, snapped.Start.X, 1e-9);
        Assert.Equal(1.0, snapped.End.X, 1e-9);
    }
}
