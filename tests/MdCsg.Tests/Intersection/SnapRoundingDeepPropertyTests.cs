using MdCsg.Intersection;
using MdCsg.Math;

namespace MdCsg.Tests.Intersection;

/// <summary>Phase 6: SnapRounding — Snap, SnapSegment, MergePoints detailed edge cases</summary>
public class SnapRoundingDeepPropertyTests
{
    [Fact]
    public void Snap_Zero_StaysZero()
    {
        var result = SnapRounding.Snap(Vec3.Zero);
        Assert.Equal(0.0, result.X);
        Assert.Equal(0.0, result.Y);
        Assert.Equal(0.0, result.Z);
    }

    [Fact]
    public void Snap_ExactGridMultiple_Unchanged()
    {
        double grid = MathUtil.DefaultGridSize;
        var pt = new Vec3(grid * 3, grid * 7, grid * -5);
        var snapped = SnapRounding.Snap(pt);
        Assert.True(System.Math.Abs(snapped.X - pt.X) < 1e-20);
        Assert.True(System.Math.Abs(snapped.Y - pt.Y) < 1e-20);
        Assert.True(System.Math.Abs(snapped.Z - pt.Z) < 1e-20);
    }

    [Fact]
    public void Snap_OffGrid_SnapsToNearest()
    {
        double grid = 0.01;
        var pt = new Vec3(0.024, 0.076, 0.155);
        var snapped = SnapRounding.Snap(pt, grid);
        Assert.True(System.Math.Abs(snapped.X - 0.02) < 1e-10);
        Assert.True(System.Math.Abs(snapped.Y - 0.08) < 1e-10);
        Assert.True(System.Math.Abs(snapped.Z - 0.16) < 1e-10);
    }

    [Fact]
    public void Snap_NegativeValues_SnapsCorrectly()
    {
        double grid = 0.1;
        var pt = new Vec3(-0.34, -0.78, -0.12);
        var snapped = SnapRounding.Snap(pt, grid);
        Assert.True(System.Math.Abs(snapped.X - (-0.3)) < 1e-10);
        Assert.True(System.Math.Abs(snapped.Y - (-0.8)) < 1e-10);
        Assert.True(System.Math.Abs(snapped.Z - (-0.1)) < 1e-10);
    }

    [Fact]
    public void Snap_Idempotent()
    {
        var pt = new Vec3(3.14159, 2.71828, 1.41421);
        var s1 = SnapRounding.Snap(pt);
        var s2 = SnapRounding.Snap(s1);
        Assert.Equal(s1.X, s2.X);
        Assert.Equal(s1.Y, s2.Y);
        Assert.Equal(s1.Z, s2.Z);
    }

    [Fact]
    public void SnapSegment_BothEndpointsSnapped()
    {
        double grid = 0.1;
        var seg = new IntersectionSegment(
            new Vec3(0.04, 0.06, 0.08),
            new Vec3(1.04, 1.06, 1.08),
            0, 1);
        var snapped = SnapRounding.SnapSegment(seg, grid);
        Assert.True(System.Math.Abs(snapped.Start.X - 0.0) < 1e-10);
        Assert.True(System.Math.Abs(snapped.End.X - 1.0) < 1e-10);
    }

    [Fact]
    public void SnapSegment_PreservesFaceIndices()
    {
        var seg = new IntersectionSegment(
            new Vec3(0.04, 0.06, 0.08),
            new Vec3(1.04, 1.06, 1.08),
            42, 99);
        var snapped = SnapRounding.SnapSegment(seg);
        Assert.Equal(42, snapped.FaceIndexA);
        Assert.Equal(99, snapped.FaceIndexB);
    }

    [Fact]
    public void MergePoints_NoDuplicates_AllReturned()
    {
        var points = new List<Vec3>
        {
            new Vec3(0, 0, 0),
            new Vec3(1, 0, 0),
            new Vec3(0, 1, 0)
        };
        var merged = SnapRounding.MergePoints(points);
        Assert.Equal(3, merged.UniquePoints.Count);
    }

    [Fact]
    public void MergePoints_ExactDuplicates_Merged()
    {
        var points = new List<Vec3>
        {
            new Vec3(1, 2, 3),
            new Vec3(1, 2, 3),
            new Vec3(4, 5, 6)
        };
        var merged = SnapRounding.MergePoints(points);
        Assert.Equal(2, merged.UniquePoints.Count);
    }

    [Fact]
    public void MergePoints_NearDuplicates_Merged()
    {
        double tol = 1e-6;
        var points = new List<Vec3>
        {
            new Vec3(0, 0, 0),
            new Vec3(1e-8, 1e-8, 1e-8), // within tolerance
            new Vec3(1, 0, 0)
        };
        var merged = SnapRounding.MergePoints(points, tol);
        Assert.Equal(2, merged.UniquePoints.Count);
    }

    [Fact]
    public void MergePoints_Empty_ReturnsEmpty()
    {
        var merged = SnapRounding.MergePoints(new List<Vec3>());
        Assert.Empty(merged.UniquePoints);
    }

    [Fact]
    public void MergePoints_SinglePoint_ReturnsSingle()
    {
        var points = new List<Vec3> { new Vec3(5, 5, 5) };
        var merged = SnapRounding.MergePoints(points);
        Assert.Single(merged.UniquePoints);
    }

    [Fact]
    public void Snap_LargeValue_Works()
    {
        var pt = new Vec3(1e6, 1e6, 1e6);
        var snapped = SnapRounding.Snap(pt);
        Assert.True(System.Math.Abs(snapped.X - 1e6) < MathUtil.DefaultGridSize);
    }

    [Fact]
    public void Snap_CustomGridSize()
    {
        var pt = new Vec3(0.333, 0.666, 0.999);
        var snapped = SnapRounding.Snap(pt, 0.5);
        Assert.True(System.Math.Abs(snapped.X - 0.5) < 1e-10);
        Assert.True(System.Math.Abs(snapped.Y - 0.5) < 1e-10);
        Assert.True(System.Math.Abs(snapped.Z - 1.0) < 1e-10);
    }
}
